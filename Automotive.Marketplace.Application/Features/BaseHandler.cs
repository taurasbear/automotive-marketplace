namespace Automotive.Marketplace.Application.Features
{
    using AutoMapper;
    using Automotive.Marketplace.Application.Interfaces.Data;
    using MediatR;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class BaseHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        protected IMapper Mapper { get; set; }

        protected IUnitOfWork UnitOfWork { get; set; }

        protected BaseHandler(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this.Mapper = mapper;
            this.UnitOfWork = unitOfWork;
        }

        public abstract Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
