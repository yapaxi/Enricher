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

    
    public class OrderEvent : Event<R1, R1>
    {

    }

    public class SelfOrderEvent : ForkedEvent<OrderEvent, R1, R1>
    {

    }
    

    public class RMAOrderEvent : ForkedEvent<OrderEvent, R1, RMA>
    {

    }

    public class SuperOrderEvent : ForkedEvent<OrderEvent, R1, Super>
    {

    }

    public class SuperRMAOrderEvent : ForkedEvent<RMAOrderEvent, RMA, Super>
    {

    }

    public class SuperMegaRMAOrderEvent : ForkedEvent<SuperRMAOrderEvent, Super, Mega>
    {

    }

    public class SuperRMAOrderEventEnricher : SuperEventEnricher<SuperRMAOrderEvent>
    {

    }

    public class SuperSelfOrderEvent : ForkedEvent<SelfOrderEvent, R1, Super>
    {

    }
}
