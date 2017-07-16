using EventModel.Blocks;
using Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    public class ForkListener<TProducer> : IDisposable
        where TProducer : Producer
    {
        private readonly InputOutputRoute[] _routes;
        private readonly IForkHandlerMiddleware _middleware;
        private readonly IForkSubscriber _subscriber;
        private readonly List<IDisposable> _subscriptions;

        public ForkListener(RouteCollection<TProducer> routes, IForkSubscriber subscriber, IForkHandlerMiddleware middleware)
        {
            _routes = routes.OfType<InputOutputRoute>().Where(e => e.ToProducer == typeof(TProducer)).ToArray();
            _middleware = middleware;
            _subscriber = subscriber;
            _subscriptions = new List<IDisposable>();
        }

        public ForkListener(RouteCollection routes, IForkSubscriber subscriber, IForkHandlerMiddleware middleware)
            : this(routes.Filter<TProducer>(), subscriber, middleware)
        {
        }

        public void Start()
        {
            foreach (var route in _routes)
            {
                var sub = _subscriber.Subscribe(route, (m, e) => _middleware.ExecuteForkHandler(e, route.ForkType));
                _subscriptions.Add(sub);
            }
        }

        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
        }
    }
}
