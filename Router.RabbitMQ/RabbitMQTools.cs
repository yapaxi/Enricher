using Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Router.RabbitMQ
{
    public static class RabbitMQTools
    {
        public static IDictionary<string, object> ToHeaders(this EventMetadata metadata)
        {
            return new Dictionary<string, object>()
            {
                ["fqn"] = StringTools.GetBytes(metadata.TypeFQN),
                ["fromProducer"] = StringTools.GetBytes(metadata.FromProducer),
                ["version"] = StringTools.GetBytes(metadata.Version)
            };
        }

        public static EventMetadata FromHeaders(IDictionary<string, object> headers)
        {
            return new EventMetadata(
                StringTools.GetString((byte[])headers["fqn"]), 
                StringTools.GetString((byte[])headers["fromProducer"]),
                StringTools.GetString((byte[])headers["version"])
            );
        }
    }
}
