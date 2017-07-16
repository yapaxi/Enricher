using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    public class InputOutputRoute : OutputRoute
    {
        public string Input { get; }

        public Type ForkType { get; }

        public InputOutputRoute(Type forkType, string input, string output, Type fromProducer, Type toProducer)
            : base(output, fromProducer, toProducer)
        {
            ForkType = forkType;
            Input = input;
        }
    }
}
