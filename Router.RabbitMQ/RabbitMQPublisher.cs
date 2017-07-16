using EasyNetQ;
using EasyNetQ.Topology;
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
    public class RabbitMQPublisher : IDataPublisher
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IBus _bus;

        public RabbitMQPublisher(IBus bus)
        {
            _bus = bus;
        }

        public async Task Publish(OutputRoute route, EventMetadata metadata, object body)
        {
            Logger.Info($"Publishing to exchange {route.Output}");
            var str = JsonConvert.SerializeObject(body);
            var bytes = StringTools.GetBytes(str);
            await _bus.Advanced.PublishAsync(new Exchange(route.Output), "", false, new MessageProperties()
            {
                Headers = metadata.ToHeaders()
            }, bytes);
            Logger.Info($"Published to exchange {route.Output}");
        }
    }
}
