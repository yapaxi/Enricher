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
        private readonly IBus _enrichmentBus;
        private IReadOnlyCollection<SourceEventObjectModel> _models;

        public RabbitMQRouter(IBus enrichmentBus, IReadOnlyCollection<SourceEventObjectModel> models)
        {
            _enrichmentBus = enrichmentBus;
            _models = models;
        }

        public void BuildRoutes()
        {
            foreach (var model in _models)
            {
                BuildRoutesForSource(model);
            }
        }

        private void BuildRoutesForSource(SourceEventObjectModel model)
        {
            var exchange = BuildRabbiqMQRoutesForSourceEvent(model);
            foreach (var fork in model.DirectForks)
            {
                BuildRoutesForFork(exchange, fork);
            }
        }

        private void BuildRoutesForFork(IExchange exchange, ForkedEventObjectModel model)
        {
            var forkOutputExchange = BuildRabbitMQRoutesForForkedEvent(exchange, model);
            foreach (var fork in model.DirectForks)
            {
                BuildRoutesForFork(forkOutputExchange, fork);
            }
        }

        private IExchange BuildRabbiqMQRoutesForSourceEvent(SourceEventObjectModel model)
        {
            var exchange = _enrichmentBus.Advanced.ExchangeDeclare(model.OutputFullName, "fanout", durable: true);
            var queue = _enrichmentBus.Advanced.QueueDeclare(model.OutputFullName, durable: true);
            _enrichmentBus.Advanced.Bind(exchange, queue, "");
            return exchange;
        }

        private IExchange BuildRabbitMQRoutesForForkedEvent(IExchange sourceExchange, ForkedEventObjectModel fork)
        {
            var inputQueue = _enrichmentBus.Advanced.QueueDeclare(fork.InputFullName, durable: true);
            _enrichmentBus.Advanced.Bind(sourceExchange, inputQueue, "");

            var outputExchange = _enrichmentBus.Advanced.ExchangeDeclare(fork.OutputFullName, "fanout", durable: true);
            var outputQueue = _enrichmentBus.Advanced.QueueDeclare(fork.OutputFullName, durable: true);
            return outputExchange;
        }
    }
}
