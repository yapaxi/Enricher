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
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

            var producers = typeof(Producer).Assembly
                .ExportedTypes
                .Where(e => e.BaseType == typeof(Producer))
                .ToList();

            models.ToList().ForEach(e => BuildPassthroughModel(containerBuilder, e));
            producers.ForEach(e => RegisterProducer(e, containerBuilder));
            
            using (var container = containerBuilder.Build())
            {
                var router = container.Resolve<IRouter>();
                var routes = router.BuildRoutes();

                var threads = producers.Select(e =>
                {
                    var t = new Thread(() => RunForkListener(e, routes, container));
                    t.Start();
                    return t;
                }).ToArray();

                using (var producerScope = container.BeginLifetimeScope("level2", e => e.RegisterInstance(routes.Filter<R1>())))
                {
                    var r1publisher = producerScope.Resolve<EventPublisher<R1>>();

                    while (true)
                    {
                        r1publisher.Publish(new OrderEvent()
                        {
                            Id = Math.Abs(Guid.NewGuid().GetHashCode()) % 1000000,
                            OrderRequestId = Guid.NewGuid().ToString()
                        }).Wait();
                        Console.WriteLine("Published");
                        Console.ReadKey(true);
                    }
                }
            }
        }

        private static void RegisterProducer(Type producer, ContainerBuilder containerBuilder)
        {
            typeof(Program).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                            .Where(e => e.Name == nameof(RegisterProducer) && e.IsGenericMethod)
                            .Single()
                            .MakeGenericMethod(producer).Invoke(null, new object[] { containerBuilder });
        }

        private static void RunForkListener(Type producer, RouteCollection routes, ILifetimeScope outerScope)
        {
            typeof(Program).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                           .Where(e => e.Name == nameof(RunForkListener) && e.IsGenericMethod)
                           .Single()
                           .MakeGenericMethod(producer)
                           .Invoke(null, new object[] { outerScope, routes });
        }

        private static void RegisterProducer<TProducer>(ContainerBuilder containerBuilder)
            where TProducer : Producer
        {
            containerBuilder.RegisterType<EventPublisher<TProducer>>().InstancePerMatchingLifetimeScope("level2");
            containerBuilder.RegisterType<ForkListener<TProducer>>().InstancePerMatchingLifetimeScope("level2");
        }

        private static void RunForkListener<TProducer>(ILifetimeScope outerScope, RouteCollection routes)
            where TProducer : Producer
        {
            Logger.Info($"Running fork listener for {typeof(TProducer).Name}");

            var localRoutes = routes.Filter<TProducer>();

            using (var scope = outerScope.BeginLifetimeScope("level2", (e) => e.RegisterInstance(localRoutes)))
            {
                var listener = scope.Resolve<ForkListener<TProducer>>();
                listener.Start();
                Thread.CurrentThread.Join();
            }
        }
        
        private static void BuildPassthroughModel(ContainerBuilder containerBuilder, EventObjectModel e)
        {
            if (e is ForkedEventObjectModel f)
            {
                Logger.Info($"Building passthrough for {f.EventType.Name}");

                containerBuilder
                    .RegisterType(typeof(PassthroughHandler<,,,>).MakeGenericType(f.FromProducerType, f.SourceEvent.EventType, f.ToProducerType, f.EventType))
                    .As(typeof(IForkHandler<>).MakeGenericType(f.EventType))
                    .InstancePerLifetimeScope();
            }

            foreach (var fork in e.DirectForks)
            {
                BuildPassthroughModel(containerBuilder, fork);
            }
        }
    }
}
