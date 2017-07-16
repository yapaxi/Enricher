using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    public class EventMetadata
    {
        public string TypeFQN { get; }
        public string FromProducer { get; }
        public string Version { get; }

        public EventMetadata(string typeFQN, string fromProducer, string version)
        {
            this.TypeFQN = typeFQN;
            this.FromProducer = fromProducer;
            this.Version = version;
        }
    }
}
