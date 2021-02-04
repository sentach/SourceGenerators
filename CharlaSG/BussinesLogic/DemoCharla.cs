using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace CharlaSG.BussinesLogic
{
    public record DemoCharla :IRequest<string>
    {
        public string Entrada { get; set; }

        public class DemoCharlaHandle : IRequestHandler<DemoCharla, string>
        {
            public Task<string> Handle(DemoCharla request, CancellationToken cancellationToken)
            {
                return Task.FromResult(request.Entrada);
            }
        }
    }
}
