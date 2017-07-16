using EventModel.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    public interface IForkHandlerMiddleware
    {
        Task ExecuteForkHandler(object input, Type forkType);
    }
}
