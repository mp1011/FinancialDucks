using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class TransactionSourcesFeature
    {
        public record Query() :IRequest<ITransactionSourceDetail[]>
        { }

        public record UpdateCommand(ITransactionSource Source) : IRequest<ITransactionSourceDetail> { }

        public class Handler : IRequestHandler<Query, ITransactionSourceDetail[]>,
            IRequestHandler<UpdateCommand, ITransactionSourceDetail>
        {
            private readonly IDataContextProvider _dataContextProvider;

            public Handler(IDataContextProvider dataContextProvider)
            {
                _dataContextProvider = dataContextProvider;
            }

            public async Task<ITransactionSourceDetail[]> Handle(Query request, CancellationToken cancellationToken)
            {
                using var context = _dataContextProvider.CreateDataContext();
                return await context.TransactionSourcesDetail.ToArrayAsync(context);
            }

            public async Task<ITransactionSourceDetail> Handle(UpdateCommand request, CancellationToken cancellationToken)
            {
                using var context = _dataContextProvider.CreateDataContext();
                return await context.Update(request.Source);
            }
        }
    }
}
