using EventModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enricher
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder1 = ModelBuilder.Create<R1>();
            var builder2 = ModelBuilder.Create<RMA>();
            var builder3 = ModelBuilder.Create<Super>();
            var builder4 = ModelBuilder.Create<Mega>();
        }
    }
}
