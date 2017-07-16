using EasyNetQ;
using EasyNetQ.Topology;
using EventModel.Blocks;
using Handler;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Router.RabbitMQ
{
    public class RabbitMQForkSubscriber : IForkSubscriber
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IBus _bus;

        public RabbitMQForkSubscriber(IBus bus)
        {
            _bus = bus;
        }

        public IDisposable Subscribe(InputOutputRoute route, Func<EventMetadata, object, Task> callback)
        {
            Logger.Info($"Subscribing on queue {route.Input}");
            return _bus.Advanced.Consume(new Queue(route.Input, false), async (a, b, c) =>
            {
                try
                {
                    var metadata = RabbitMQTools.FromHeaders(b.Headers);
                    Logger.Info($"Consumed from queue {route.Input}, version {metadata.Version}");
                    var json = StringTools.GetString(a);
                    var type = Type.GetType(metadata.TypeFQN);
                    var instance = JsonConvert.DeserializeObject(json, type);
                    await callback(metadata, instance);
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to process event from {route.Input}: {e.Message}");
                    throw;
                }
            });
        }
    }
}
