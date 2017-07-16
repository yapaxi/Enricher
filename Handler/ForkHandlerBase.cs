using EventModel.Blocks;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    public abstract class ForkHandlerBase<TInputProducer, TInput, TOutputProducer, TFork> : IForkHandler<TFork>
        where TInputProducer : Producer
        where TOutputProducer : Producer
        where TInput : IEvent<TInputProducer>
        where TFork : IEventFork<TInput, TInputProducer>
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public abstract Task<TFork> Handle(TInput input);

        async Task<(object Instance, Type InputType, Type OutputType)> IForkHandler.Handle(object input)
        {
            var result = await Handle((TInput)input);
            return (result, typeof(TInput), typeof(TFork));
        }
    }

    public sealed class PassthroughHandler<TInputProducer, TInput, TOutputProducer, TFork> : ForkHandlerBase<TInputProducer, TInput, TOutputProducer, TFork>
        where TInputProducer : Producer
        where TOutputProducer : Producer
        where TInput : IEvent<TInputProducer>
        where TFork : ForkedEvent<TInput, TInputProducer, TOutputProducer>, new()
    {
        private readonly EventPublisher<TOutputProducer> _publisher;

        public PassthroughHandler(EventPublisher<TOutputProducer> publisher)
        {
            _publisher = publisher;
        }

        public async override Task<TFork> Handle(TInput input)
        {
            Logger.Info($"Passing through: {typeof(TInput).Name} -> {typeof(TFork).Name}");
            var fork = new TFork();
            await _publisher.Publish(fork);
            return fork;
        }
    }

    public interface IForkHandler<TFork> : IForkHandler
    {
    }

    public interface IForkHandler
    {
        Task<(object Instance, Type InputType, Type OutputType)> Handle(object input);
    }
}
