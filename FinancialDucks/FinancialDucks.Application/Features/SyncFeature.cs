using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class SyncFeature
    {
        public record Query() : IRequest<SyncStatus[]> { }

        public class Handler : IRequestHandler<Query, SyncStatus[]>
        {
            private IDataContextProvider _dataContextProvider;
            private IMediator _mediator;

            public Handler(IDataContextProvider dataContextProvider, IMediator mediator)
            {
                _dataContextProvider = dataContextProvider;
                _mediator = mediator;
            }

            public async Task<SyncStatus[]> Handle(Query request, CancellationToken cancellationToken)
            {
                using var context = _dataContextProvider.CreateDataContext();
                var accounts = await context.TransactionSourcesDetail.ToArrayAsync(context);

                return await Task.WhenAll(accounts
                                    .Select(a => GetStatus(a, context)));
            }

            private async Task<SyncStatus> GetStatus(ITransactionSourceDetail account, IDataContext context)
            {
                var lastTransaction = context.Transactions
                    .Where(p => p.SourceId == account.Id)
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefault();

                var downloadedTransactions = await _mediator.Send(new ReadLocalTransactions.Request(account));

                return new SyncStatus(account, downloadedTransactions.NullToEmpty().ToArray(), lastTransaction?.Date);
            }
        }

    }
}
