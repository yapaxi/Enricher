using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventModel
{
    public class ModelBuilder
    {
        private static Dictionary<Type, ModelBuilder> _instances = new Dictionary<Type, ModelBuilder>();

        public static ModelBuilder Create<TProducer>()
            where TProducer : Producer
        {
            var t = typeof(TProducer);
            if (!_instances.TryGetValue(t, out ModelBuilder builder))
            {
                builder = new ModelBuilder(t);
                _instances[t] = builder;
            }

            return builder;
        }

        internal static ModelBuilder CreateInternal(Type producer)
        {
            return (ModelBuilder)typeof(ModelBuilder).GetMethod(nameof(Create)).MakeGenericMethod(producer).Invoke(null, null);
        }

        public ProducedEvent[] ProducedEvents { get; }
        public ProducedEventFork[] ProducedEventForks { get; }
        public ConsumedEventFork[] ConsumedEventForks { get; }


        private ModelBuilder(Type producer)
        {
            var types = GetType().Assembly.GetTypes();

            ProducedEvents = (
                from q in types
                where typeof(SourceEvent<,>).MakeGenericType(producer, producer).IsAssignableFrom(q.BaseType)
                select new ProducedEvent(producer, q)
            ).ToArray();

            var forksFromOwnedTypes = (
                from q in ProducedEvents
                from z in types
                where typeof(IProducableFork<>).MakeGenericType(producer).IsAssignableFrom(z)
                let sourceType = z.BaseType.GetGenericArguments()[0]
                where sourceType == q.EventType
                select new ProducedEventFork(
                       @event: q,
                       forkType: z,
                       forkTo: z.BaseType.GetGenericArguments()[2]
                )
            ).ToArray();

            var forksFromOtherForks = (
                from z in types
                where typeof(IProducableFork<>).MakeGenericType(producer).IsAssignableFrom(z)
                let sourceType = z.BaseType.GetGenericArguments()[0]
                where sourceType.BaseType.GetGenericTypeDefinition() == typeof(EnrichedEvent<,,>)
                let originalProducer = sourceType.BaseType.GetGenericArguments()[1]
                let model = CreateInternal(originalProducer)
                let originalFork = model.ProducedEventForks.FirstOrDefault(q => q.ForkType == sourceType)
                let originalEvent = model.ProducedEvents.FirstOrDefault(q => q.EventType == sourceType)
                select originalFork != null 
                       ? new ProducedEventFork(originalFork, z, z.BaseType.GetGenericArguments()[2])
                       : (originalEvent != null 
                          ? new ProducedEventFork(originalEvent, z, z.BaseType.GetGenericArguments()[2]) 
                          : throw new InvalidOperationException())
            ).ToArray();

            ProducedEventForks = forksFromOwnedTypes.Concat(forksFromOtherForks).ToArray();

            ConsumedEventForks = (
                from z in types
                where typeof(IConsumableFork<>).MakeGenericType(producer).IsAssignableFrom(z)
                let sourceProducer = z.BaseType.GetGenericArguments()[1]
                let sourceModel = CreateInternal(sourceProducer)
                select new ConsumedEventFork(
                    sourceModel.ProducedEventForks.Single(e => e.ForkType == z)
                )
            ).ToArray();
        }
    }
   
    public class ProducedEvent 
    {
        public Type Producer { get; }
        public Type EventType { get; }

        public ProducedEvent(Type producer, Type eventType)
        {
            Producer = producer;
            EventType = eventType;
            FullName = $"{producer.Name}-{eventType.Name}";
        }

        public string FullName { get; }
    }
    
    public interface IConsumedFork
    {
        ProducedEvent Event { get; }
        string FullName { get; }
    }

    public class ProducedEventFork  : IConsumedFork
    {
        public ProducedEvent Event { get; }
        public ProducedEventFork Fork { get; }
        public Type ForkType { get; }
        public Type ForkTo { get; }

        public ProducedEventFork(ProducedEvent @event, Type forkType, Type forkTo)
        {
            Event = @event;
            ForkTo = forkTo;
            ForkType = forkType;
            FullName = $"{@event.FullName}-{forkTo.Name}";
        }

        public ProducedEventFork(ProducedEventFork fork, Type forkType, Type forkTo)
        {
            Fork = fork;
            Event = fork.Event;
            ForkTo = forkTo;
            ForkType = forkType;
            FullName = $"{fork.FullName}-{forkTo.Name}";
        }

        public string FullName { get; }
    }

    public class ConsumedEventFork
    {
        public IConsumedFork Fork { get; }

        public ConsumedEventFork(IConsumedFork fork)
        {
            Fork = fork;
        }
    }
}
