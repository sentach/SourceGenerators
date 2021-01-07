using MediatR;

namespace CharlaSG.BussinesLogic
{
    public interface IPost<T> : IRequest<T>
    {
    }
}
