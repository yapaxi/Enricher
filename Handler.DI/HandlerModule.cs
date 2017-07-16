using Autofac;
using EventModel.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler.DI
{
    public class HandlerModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<AutofacForkResolver>()
                .AsSelf()
                .As<IForkHandlerMiddleware>()
                .InstancePerMatchingLifetimeScope("level2");
            
            base.Load(builder);
        }
    }

    public class AutofacForkResolver : IForkHandlerMiddleware
    {
        private readonly ILifetimeScope _scope;

        public AutofacForkResolver(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public async Task ExecuteForkHandler(object input, Type forkType)
        {
            using (var scope = _scope.BeginLifetimeScope())
            {
                var type = typeof(IForkHandler<>).MakeGenericType(forkType);
                var handler = (IForkHandler)scope.Resolve(type);
                await handler.Handle(input);
            }
        }
    }
}
