using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CharlaSG.BussinesLogic
{
    public record CreateOrder : IRequest<bool>
    {
        public int Id { get; set; }
        public string Customer { get; set; }

        public class HandleCreateOrder : IRequestHandler<CreateOrder, bool>
        {
            public Task<bool> Handle(CreateOrder request, CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }
        }
    }
}
