using Autofac;
using EasyNetQ;
using EasyNetQ.Topology;
using EventModel;
using EventModel.Blocks;
using Handler;
using Handler.DI;
using NLog;
using RabbitMQ.Client;
using Router;
using Router.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enricher
{
    class Program
    {
        public class RMAOrderEventHandler : ForkHandlerBase<R1, OrderEvent, RMAOrderEvent>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            public async override Task<RMAOrderEvent> Handle(OrderEvent input)
            {
                Logger.Info("Got order event");
                return new RMAOrderEvent();
            }
        }

        public class SelfOrderEventHandler : ForkHandlerBase<R1, OrderEvent, SelfOrderEvent>
        {
            private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

            public async override Task<SelfOrderEvent> Handle(OrderEvent input)
            {
                Logger.Info("Got order event");
                return new SelfOrderEvent();
            }
        }

        static void Main(string[] args)
        {
            var models = ModelBuilder.Create();
            
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new HandlerModule());
            containerBuilder
                .Register(e => RabbitHutch.CreateBus("host=192.168.100.6;timeout=120;virtualHost=routing-model;username=test;password=test"))
                .As<IBus>()
                .SingleInstance();
            containerBuilder.RegisterType<RabbitMQForkSubscriber>()
                .As<IForkSubscriber>()
                .SingleInstance();
            containerBuilder.RegisterType<RabbitMQRouter>().As<IRouter>().SingleInstance();
            containerBuilder.RegisterInstance(models);
            containerBuilder.RegisterType<RabbitMQPublisher>().As<IDataPublisher>().InstancePerMatchingLifetimeScope("level2");

            RegisterProducer<R1>(containerBuilder);
            RegisterProducer<RMA>(containerBuilder);

            containerBuilder
                .RegisterType<RMAOrderEventHandler>()
                .As<IForkHandler<RMAOrderEvent>>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterType<SelfOrderEventHandler>()
                .As<IForkHandler<SelfOrderEvent>>()
                .InstancePerLifetimeScope();

            using (var container = containerBuilder.Build())
            {
                var t = new Thread(() => RunProducer<RMA>(container));
                t.Start();

                RunProducer<R1>(
                    container, 
                    publisherAction: e =>
                    {
                        while (true)
                        {
                            e.Publish(new OrderEvent()).Wait();
                            Console.WriteLine("Published");
                            Console.ReadKey(true);
                        }
                    });
            }
        }

        private static void RegisterProducer<TProducer>(ContainerBuilder containerBuilder)
            where TProducer : Producer
        {
            containerBuilder.RegisterType<SourceEventPublisher<TProducer>>().InstancePerMatchingLifetimeScope("level2");
            containerBuilder.RegisterType<ForkListener<TProducer>>().InstancePerMatchingLifetimeScope("level2");
        }

        private static void RunProducer<TProducer>(ILifetimeScope outerScope, Action<SourceEventPublisher<TProducer>> publisherAction = null)
            where TProducer : Producer
        {
            var router = outerScope.Resolve<IRouter>();
            var routes = router.BuildRoutes();
            var localRoutes = routes.Filter<TProducer>();

            using (var scope = outerScope.BeginLifetimeScope("level2", (e) => e.RegisterInstance(localRoutes)))
            {
                var listener = scope.Resolve<ForkListener<TProducer>>();
                listener.Start();

                var publisher = scope.Resolve<SourceEventPublisher<TProducer>>();

                Console.WriteLine($"Started {typeof(TProducer).Name}");
                publisherAction?.Invoke(publisher);
                Thread.CurrentThread.Join();
            }
            Console.WriteLine($"Stoped {typeof(TProducer).Name}");
        }
    }
}
