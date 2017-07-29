using EventModel.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventModel
{
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


    [RouteName("order")]
    public class OrderEvent : Event<R1, R1>, OrderEvent.IModel
    {
        public interface IModel
        {
            int Id { get; set; }
            string OrderRequestId { get; set; }
        }

        public int Id { get; set; }
        public string OrderRequestId { get; set; }
    }

    [RouteName("self-order")]
    public class SelfOrderEvent : ForkedEvent<OrderEvent, R1, R1>, OrderEvent.IModel
    {
        public int Id { get; set; }
        public string OrderRequestId { get; set; }
    }


    [RouteName("rma-order")]
    public class RMAOrderEvent : ForkedEvent<OrderEvent, R1, RMA>
    {

    }

    [RouteName("super-order")]
    public class SuperOrderEvent : ForkedEvent<OrderEvent, R1, Super>, OrderEvent.IModel
    {
        public int Id { get; set; }
        public string OrderRequestId { get; set; }
    }

    [RouteName("super-rma-order")]
    public class SuperRMAOrderEvent : ForkedEvent<RMAOrderEvent, RMA, Super>, OrderEvent.IModel
    {
        public int Id { get; set; }
        public string OrderRequestId { get; set; }
        public int ClaimsCount { get; set; }
    }

    [RouteName("super-mega-rma-order")]
    public class SuperMegaRMAOrderEvent : ForkedEvent<SuperRMAOrderEvent, Super, Mega>, OrderEvent.IModel
    {
        public int Id { get; set; }
        public string OrderRequestId { get; set; }
        public int ClaimsCount { get; set; }
    }

    [RouteName("self-super-mega-rma-order")]
    public class SelfSuperMegaRMAOrderEvent : ForkedEvent<SuperMegaRMAOrderEvent, Mega, Mega>, OrderEvent.IModel
    {
        public int Id { get; set; }
        public string OrderRequestId { get; set; }
        public int ClaimsCount { get; set; }
    }

    [RouteName("super-self-order")]
    public class SuperSelfOrderEvent : ForkedEvent<SelfOrderEvent, R1, Super>, OrderEvent.IModel
    {
        public int Id { get; set; }
        public string OrderRequestId { get; set; }
    }
}
