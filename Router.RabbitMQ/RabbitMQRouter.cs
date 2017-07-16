using EasyNetQ;
using EasyNetQ.Topology;
using EventModel;
using EventModel.Blocks;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Router.RabbitMQ
{
    public class RabbitMQRouter : IRouter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IBus _enrichmentBus;
        private IReadOnlyCollection<SourceEventObjectModel> _models;

        public RabbitMQRouter(IBus enrichmentBus, IReadOnlyCollection<SourceEventObjectModel> models)
        {
            _enrichmentBus = enrichmentBus;
            _models = models;
        }

        public RouteCollection BuildRoutes()
        {
            var routes = new Dictionary<Type, OutputRoute>();
            foreach (var model in _models)
            {
                BuildRoutesForSource(model, routes);
            }
            return new RouteCollection(routes);
        }

        private void BuildRoutesForSource(SourceEventObjectModel model, Dictionary<Type, OutputRoute> routes)
        {
            var exchange = BuildRabbiqMQRoutesForSourceEvent(model);
            routes.Add(model.EventType, new OutputRoute(model.OutputFullName, model.FromProducerType, model.FromProducerType));
            foreach (var fork in model.DirectForks)
            {
                BuildRoutesForFork(exchange, fork, routes);
            }
        }

        private void BuildRoutesForFork(IExchange exchange, ForkedEventObjectModel model, Dictionary<Type, OutputRoute> routes)
        {
            var forkOutputExchange = BuildRabbitMQRoutesForForkedEvent(exchange, model);
            routes.Add(model.EventType, new InputOutputRoute(model.EventType, model.InputFullName, model.OutputFullName, model.FromProducerType, model.ToProducerType));
            foreach (var fork in model.DirectForks)
            {
                BuildRoutesForFork(forkOutputExchange, fork, routes);
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
            Logger.Info($"Routing: \"{sourceExchange.Name}\" to \"{fork.InputFullName}\"");
            var inputQueue = _enrichmentBus.Advanced.QueueDeclare(fork.InputFullName, durable: true);
            _enrichmentBus.Advanced.Bind(sourceExchange, inputQueue, "");

            var outputExchange = _enrichmentBus.Advanced.ExchangeDeclare(fork.OutputFullName, "fanout", durable: true);
            var outputQueue = _enrichmentBus.Advanced.QueueDeclare(fork.OutputFullName, durable: true);
            _enrichmentBus.Advanced.Bind(outputExchange, outputQueue, "");
            return outputExchange;
        }
    }
}
