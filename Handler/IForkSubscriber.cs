using EventModel.Blocks;
using Router;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    public interface IForkSubscriber
    {
        IDisposable Subscribe(InputOutputRoute route, Func<EventMetadata, object, Task> callback);
    }
}
