using EasyNetQ;
using EasyNetQ.Topology;
using EventModel;
using RabbitMQ.Client;
using Router.RabbitMQ;
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
            var models = ModelBuilder.Create();
            PrintModels(models);

            using (var bus = RabbitHutch.CreateBus("host=192.168.100.6;timeout=120;virtualHost=routing-model;username=test;password=test"))
            {
                var router = new RabbitMQRouter(bus, models);
                router.BuildRoutes();

                var model = models.First();

                foreach (var fork in model.DirectForks)
                {
                    RecursiveConsume(bus.Advanced, fork);
                }

                bus.Advanced.Publish(new Exchange(model.OutputFullName), "", false, new MessageProperties(), new byte[] { 0x01, 0x02, 0x03 });

                Console.ReadKey(true);
            }
        }

        private static void RecursiveConsume(IAdvancedBus bus, ForkedEventObjectModel model)
        {
            bus.Consume(new Queue(model.InputFullName, false), (a, b, c) => {
                Console.WriteLine($"Consumed {a.Length} bytes from {model.InputFullName}");
                bus.Publish(new Exchange(model.OutputFullName), "", false, new MessageProperties(), a);
            });

            foreach (var fork in model.DirectForks)
            {
                RecursiveConsume(bus, fork);
            }
        }

        private static void PrintModels(IReadOnlyCollection<SourceEventObjectModel> models)
        {
            models.ToList().ForEach(Print);

            void Print(EventObjectModel model)
            {
                Console.WriteLine(model.OutputFullName);

                if (model is ForkedEventObjectModel x)
                {
                    Console.WriteLine($"---from {x.InputFullName})");
                }

                foreach (var innerModel in model.DirectForks)
                {
                    Print(innerModel);
                }
            }
        }
    }
}
