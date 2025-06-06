namespace Automotive.Marketplace.Application.Features;

using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public abstract class BaseHandler<TRequest, TResponse>(IMapper mapper, IUnitOfWork unitOfWork) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected IMapper Mapper { get; set; } = mapper;

    protected IUnitOfWork UnitOfWork { get; set; } = unitOfWork;

    public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
