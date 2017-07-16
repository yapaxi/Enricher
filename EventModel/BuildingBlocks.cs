using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventModel.Blocks
{
    public abstract class Producer
    {

    }

    public interface IEvent<TOutput>
        where TOutput : Producer
    {

    }

    public class Event<TInput, TOutput> : IEvent<TOutput>
        where TInput : Producer
        where TOutput : Producer
    {

    }

    public interface IExportedFork<TSource>
        where TSource : Producer
    {

    }

    public interface IImportedFork<TDestination>
        where TDestination : Producer
    {
        
    }

    public interface IEventFork<TSourceEvent, TSource> : IExportedFork<TSource>
        where TSourceEvent : IEvent<TSource>
        where TSource : Producer
    {

    }

    public abstract class ForkedEvent<TSourceEvent, TSource, TDestination> : 
        Event<TSource, TDestination>,
        IEventFork<TSourceEvent, TSource>,
        IImportedFork<TDestination>
        where TSourceEvent : IEvent<TSource>
        where TSource : Producer
        where TDestination : Producer
    {

    }

    public abstract class EventEnricher<TEvent, TInput>
        where TEvent : IEvent<TInput>
        where TInput : Producer
    {

    }

    public abstract class SuperEventEnricher<TEvent> : EventEnricher<TEvent, Super>
        where TEvent : IEvent<Super>
    {

    }

    public class RouteNameAttribute : Attribute
    {
        public string Name { get; }

        public RouteNameAttribute(string name) => Name = name;
    }
}
