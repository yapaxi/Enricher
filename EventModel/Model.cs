using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventModel
{
    public abstract class Producer
    {

    }


    public class R1 : Producer
    {

    }

    public class RMA : Producer
    {

    }

    public class Super : Producer
    {

    }

    public class Mega : Producer
    {

    }

    public abstract class SourceEvent<TOutput>
        where TOutput : Producer
    {

    }

    public abstract class SourceEvent<TInput, TOutput> : SourceEvent<TOutput>
        where TInput : Producer
        where TOutput : Producer
    {

    }

    internal interface IProducableFork<TInput>
        where TInput : Producer
    {

    }

    internal interface IConsumableFork<TInput>
        where TInput : Producer
    {

    }

    public abstract class EnrichedEvent<TSourceEvent, TInput, TOutput> : SourceEvent<TInput, TOutput>, IProducableFork<TInput>, IConsumableFork<TOutput>
        where TSourceEvent : SourceEvent<TInput>
        where TInput : Producer
        where TOutput : Producer
    {

    }

    public class OrderEvent : SourceEvent<R1, R1>
    {

    }

    public class RMAOrderEvent : EnrichedEvent<OrderEvent, R1, RMA>
    {

    }

    public class SuperOrderEvent : EnrichedEvent<OrderEvent, R1, Super>
    {

    }

    public class SuperRMAOrderEvent : EnrichedEvent<RMAOrderEvent, RMA, Super>
    {

    }

    public class SuperMegaRMAOrderEvent : EnrichedEvent<SuperRMAOrderEvent, Super, Mega>
    {

    }

    public abstract class EventEnricher<TEvent, TInput>
        where TEvent : SourceEvent<TInput>
        where TInput : Producer
    {

    }

    public abstract class SuperEventEnricher<TEvent> : EventEnricher<TEvent, Super>
        where TEvent : SourceEvent<Super>
    {

    }

    public class SuperRMAOrderEventEnricher : SuperEventEnricher<SuperRMAOrderEvent>
    {

    }
}
