using EventModel.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler
{
    public abstract class ForkHandlerBase<TInputProducer, TInput, TFork> : IForkHandler<TFork>
        where TInputProducer : Producer
        where TInput : IEvent<TInputProducer>
        where TFork : IEventFork<TInput, TInputProducer>
    {
        public abstract Task<TFork> Handle(TInput input);

        async Task<(object Instance, Type InputType, Type OutputType)> IForkHandler.Handle(object input)
        {
            var result = await Handle((TInput)input);
            return (result, typeof(TInput), typeof(TFork));
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
