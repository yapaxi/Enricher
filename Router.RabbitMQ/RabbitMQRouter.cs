using EasyNetQ;
using EasyNetQ.Topology;
using EventModel;
using EventModel.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Router.RabbitMQ
{
    public class RabbitMQRouter
    {
        private readonly Assembly _eventModelAssembly;
        private readonly IBus _enrichmentBus;

        public RabbitMQRouter(IBus enrichmentBus,  Assembly eventModelAssembly)
        {
            _eventModelAssembly = eventModelAssembly;
            _enrichmentBus = enrichmentBus;
        }

        //public void BuildRoutes()
        //{
        //    var producers = _eventModelAssembly.ExportedTypes.Where(e => e.BaseType == typeof(Producer)).ToArray();

        //    var sources = new Dictionary<ProducedEvent, IExchange>();

        //    foreach (var producer in producers)
        //    {
        //        var model = ModelBuilder.Create(producer);

        //        foreach (var producedEvent in model.ProducedEvents)
        //        {
        //            var sourceExchange = BuildProducedEventSource(producedEvent);
        //            sources.Add(producedEvent, sourceExchange);
        //        }

        //        foreach (var fork in model.ProducedEventForks)
        //        {
        //            var sourceExchange = GetByKey(fork.Event, sources, () => new InvalidOperationException($"[Internal Error] Failed to found exchange for event \"{fork.Event.FullName}\""));
        //            BuildSourceFork(sourceExchange, fork);
        //        }
        //    }
        //}

        //private TResult GetByKey<TKey, TResult>(TKey key, IDictionary<TKey, TResult> dict, Func<Exception> onNotFound)
        //{
        //    TResult r;
        //    if (!dict.TryGetValue(key, out r))
        //    {
        //        throw onNotFound();
        //    }
        //    return r;
        //}

        //private IExchange BuildProducedEventSource(ProducedEvent @event)
        //{
        //    var exchange = _enrichmentBus.Advanced.ExchangeDeclare(@event.FullName, "fanout", durable: true);
        //    var queue = _enrichmentBus.Advanced.QueueDeclare(@event.FullName, durable: true);
        //    _enrichmentBus.Advanced.Bind(exchange, queue, "");
        //    return exchange;
        //}

        //private IQueue BuildSourceFork(IExchange sourceExchange, ProducedEventFork fork)
        //{
        //    var queue = _enrichmentBus.Advanced.QueueDeclare(fork.FullName, durable: true);
        //    _enrichmentBus.Advanced.Bind(sourceExchange, queue, "");
        //    return queue;
        //}
    }
}
