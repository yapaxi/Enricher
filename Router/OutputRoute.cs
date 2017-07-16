using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    public class OutputRoute
    {
        public string Output { get; }

        public Type FromProducer { get; }
        public Type ToProducer { get; }

        public OutputRoute(string output, Type fromProducer, Type toProducer)
        {
            Output = output;
            FromProducer = fromProducer;
            ToProducer = toProducer;
        }
    }
}
