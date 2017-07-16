using EventModel.Blocks;
using Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    public class SourceEventPublisher<TProducer>
        where TProducer : Producer
    {
        private readonly RouteCollection<TProducer> _routeCollection;
        private readonly IDataPublisher _publisher;

        public SourceEventPublisher(RouteCollection<TProducer> routeCollection, IDataPublisher publisher)
        {
            _routeCollection = routeCollection;
            _publisher = publisher;
        }

        public SourceEventPublisher(RouteCollection routeCollection, IDataPublisher publisher)
        {
            _routeCollection = routeCollection.Filter<TProducer>();
            _publisher = publisher;
        }

        public async Task Publish<TEvent>(TEvent @event)
            where TEvent : IEvent<TProducer>
        {
            var type = typeof(TEvent);
            if (!_routeCollection.TryGetValue(type, out OutputRoute route))
            {
                throw new Exception($"Route for type \"{type.FullName}\" has not been found");
            }

            await _publisher.Publish(
                route,
                new EventMetadata(type.AssemblyQualifiedName, typeof(TProducer).Name, "0.0.1"),
                @event);
        }
    }
}
