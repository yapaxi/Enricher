using EventModel.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventModel
{
    public class ModelBuilder
    {
        public static IReadOnlyCollection<SourceEventObjectModel> Create()
        {
            var allTypes = typeof(Producer).Assembly.GetTypes();

            var events = (
                from q in allTypes
                let category = GetEventCategory(q)
                where category == EventCategory.ForkedEvent || category == EventCategory.SourceEvent
                select new { Type = q, EventCategory = category }
             ).ToArray();


            var sourceEventModels = events.Where(e => e.EventCategory == EventCategory.SourceEvent)
                                           .Select(e => new SourceEventObjectModel(e.Type))
                                           .ToArray();

            var forksBySorceType = events.Where(e => e.EventCategory == EventCategory.ForkedEvent)
                                         .GroupBy(e => e.Type.BaseType.GetGenericArguments()[0])
                                         .ToDictionary(e => e.Key, e => e.Select(q => q.Type).ToArray());
            
            foreach (var sourceEventModel in sourceEventModels)
            {
                FindForksToModel(sourceEventModel, forksBySorceType);
            }

            return sourceEventModels;
        }

        private static void FindForksToModel(EventObjectModel eventObjectModel, IReadOnlyDictionary<Type, Type[]> forksBySorceType)
        {
            if (forksBySorceType.TryGetValue(eventObjectModel.EventType, out Type[] forks))
            {
                foreach (var fork in forks)
                {
                    var model = new ForkedEventObjectModel(eventObjectModel, fork);
                    eventObjectModel.AddFork(model);
                    FindForksToModel(model, forksBySorceType);
                }
            }
        }

        private static EventCategory GetEventCategory(Type type)
        {
            if (type.BaseType == typeof(object) || type.BaseType == typeof(ValueType) || type.BaseType == null)
            {
                return EventCategory.None;
            }

            var currentTypeGeneric = type.IsGenericType ? type.GetGenericTypeDefinition() : null;

            if (type.BaseType?.IsGenericType == true 
                && currentTypeGeneric != typeof(ForkedEvent<,,>)
                && currentTypeGeneric != typeof(Event<,>))
            {
                var baseTypeGenericDefinition = type.BaseType.GetGenericTypeDefinition();
                if (baseTypeGenericDefinition == typeof(Event<,>))
                {
                    return EventCategory.SourceEvent;
                }
                else if (baseTypeGenericDefinition == typeof(ForkedEvent<,,>))
                {
                    return EventCategory.ForkedEvent;
                }
            }

            return GetEventCategory(type.BaseType);
        }
    }



    public abstract class EventObjectModel
    {
        private readonly HashSet<ForkedEventObjectModel> _directForks;

        public Type EventType { get; }
        public Type ProducerType { get; protected set; }
        public abstract EventCategory Category { get; }
        public bool IsLocal { get; protected set; }
        public IReadOnlyCollection<ForkedEventObjectModel> DirectForks => _directForks;
        public abstract string OutputFullName { get; }

        public EventObjectModel(Type eventType)
        {
            _directForks = new HashSet<ForkedEventObjectModel>();
            EventType = eventType;
        }

        internal void AddFork(ForkedEventObjectModel fork)
        {
            _directForks.Add(fork);
        }
    }

    public class SourceEventObjectModel : EventObjectModel
    {
        public override EventCategory Category => EventCategory.SourceEvent;
        public override string OutputFullName => EventType.Name;

        public SourceEventObjectModel(Type eventType) 
            : base(eventType)
        {
            ProducerType = eventType.BaseType.GetGenericArguments()[0];
            IsLocal = true;
        }
    }

    public class ForkedEventObjectModel : EventObjectModel
    {
        public EventObjectModel SourceEvent { get; }
        public override EventCategory Category => EventCategory.ForkedEvent;
        public string InputFullName => $"{GetOutputFullName(SourceEvent)}:{EventType.Name}:input";
        public override string OutputFullName => $"{GetOutputFullName(SourceEvent)}:{EventType.Name}:output";

        public ForkedEventObjectModel(EventObjectModel sourceEvent, Type eventType) 
            : base(eventType)
        {
            var genericArgs = eventType.BaseType.GetGenericArguments();
            ProducerType = genericArgs[1];
            IsLocal = genericArgs[1] == genericArgs[2];
            SourceEvent = sourceEvent;
        }

        private static string GetOutputFullName(EventObjectModel model)
        {
            switch (model)
            {
                case SourceEventObjectModel x1: return x1.OutputFullName;
                case ForkedEventObjectModel x2: return x2.OutputFullName;
                default:
                    throw new InvalidOperationException($"Unexpected type of model: {model.GetType().Name}");
            }
        }
    }


    public enum EventCategory
    {
        None = 0,
        SourceEvent = 1,
        ForkedEvent = 2
    }

    public class EventModel
    {

    }
}
