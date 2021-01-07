using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace CharlaSG.BussinesLogic
{
    [ControllerName("Orders")]
    public record GetAllOrders : IGet<List<Order>>
    {
        public class GetAllOrdersHandle : IRequestHandler<GetAllOrders, List<Order>>
        {
            public Task<List<Order>> Handle(GetAllOrders request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new List<Order>
                {
                    new Order{Id=1,Customer="Pepe"},
                    new Order{Id=2,Customer="Juan"}
                });
            }
        }
    }
}
