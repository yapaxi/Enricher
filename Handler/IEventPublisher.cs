using Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    public interface IDataPublisher
    {
        Task Publish(OutputRoute route, EventMetadata metadata, object body);
    }
}
