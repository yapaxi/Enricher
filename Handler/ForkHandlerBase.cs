using EventModel.Blocks;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    public abstract class ForkHandlerBase<TInputProducer, TInput, TOutputProducer, TFork> : IForkHandler<TFork>
        where TInputProducer : Producer
        where TOutputProducer : Producer
        where TInput : IEvent<TInputProducer>
        where TFork : ForkedEvent<TInput, TInputProducer, TOutputProducer>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly PropertyInfo[] _properties;
        private readonly EventPublisher<TOutputProducer> _publisher;

        public ForkHandlerBase(EventPublisher<TOutputProducer> publisher)
        {
            _properties = (
                from q in typeof(TFork).GetInterfaces()
                where q.FullName.EndsWith("+IModel") && q.DeclaringType != typeof(TFork)
                from p in q.GetProperties()
                select p
            ).ToArray();

            _publisher = publisher;
        }

        private async Task HandleInternal(TInput input)
        {
            var fork = await Handle(input);

            foreach (var p in _properties)
            {
               p.SetValue(fork, p.GetValue(input));
            }

            await _publisher.Publish(fork);
        }

        protected abstract Task<TFork> Handle(TInput input);

        async Task IForkHandler.Handle(object input)
        {
            await HandleInternal((TInput)input);
        }
    }

    public sealed class PassthroughHandler<TInputProducer, TInput, TOutputProducer, TFork> : ForkHandlerBase<TInputProducer, TInput, TOutputProducer, TFork>
        where TInputProducer : Producer
        where TOutputProducer : Producer
        where TInput : IEvent<TInputProducer>
        where TFork : ForkedEvent<TInput, TInputProducer, TOutputProducer>, new()
    {
        public PassthroughHandler(EventPublisher<TOutputProducer> publisher)
            : base(publisher)
        {

        }

        protected override Task<TFork> Handle(TInput input)
        {
            Logger.Info($"Passing through: {typeof(TInput).Name} -> {typeof(TFork).Name} ({JsonConvert.SerializeObject(input)})");
            return Task.FromResult(new TFork());
        }
    }

    public interface IForkHandler<TFork> : IForkHandler
    {
    }

    public interface IForkHandler
    {
        Task Handle(object input);
    }
}
