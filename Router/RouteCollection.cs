using EventModel.Blocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    public class RouteCollection : IEnumerable<OutputRoute>
    {
        private readonly IDictionary<Type, OutputRoute> _routes;

        public RouteCollection(IDictionary<Type, OutputRoute> routes)
        {
            _routes = routes;
        }

        public bool TryGetValue(Type key, out OutputRoute value) => _routes.TryGetValue(key, out value);

        public RouteCollection<TProducer> Filter<TProducer>()
            where TProducer : Producer
        {
            return new RouteCollection<TProducer>(_routes.Where(e => e.Value.FromProducer == typeof(TProducer) || e.Value.ToProducer == typeof(TProducer)).ToDictionary(e => e.Key, e => e.Value));
        }

        public IEnumerator<OutputRoute> GetEnumerator() => _routes.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public class RouteCollection<TProducer> : RouteCollection
        where TProducer : Producer
    {
        internal RouteCollection(IDictionary<Type, OutputRoute> routes) : base(routes)
        {

        }
    }
}
