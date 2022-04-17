using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class AccountBalanceFeature
    {
        public record Query(ITransactionSource[] Sources, DateTime DateFrom, DateTime DateTo, TimeInterval Interval)
            :IRequest<SourceSnapshot[]>
        {

        }

        public class Handler : IRequestHandler<Query, SourceSnapshot[]>
        {
            private readonly IDataContextProvider _dataContextProvider;

            public Handler(IDataContextProvider dataContextProvider)
            {
                _dataContextProvider = dataContextProvider;
            }

            public async Task<SourceSnapshot[]> Handle(Query request, CancellationToken cancellationToken)
            {
                var transactionsAndSnaphots = await GetTransactionsAndSnapshots(request);
                var transactions = transactionsAndSnaphots.Item1;
                var snapshots = transactionsAndSnaphots.Item2;

                return ComputeSnapshots(transactions, snapshots, request)
                    .ToArray();
            }

            private IEnumerable<SourceSnapshot> ComputeSnapshots(ITransaction[] transactions, ISourceSnapshot[] snapshots, Query request)
            {
                DateTime date = request.DateFrom;

                while(date <= request.DateTo)
                {
                    var snapshot = ComputeSnapshot(date, transactions, snapshots);
                    if (snapshot != null)
                        yield return snapshot;

                    date = date.Add(request.Interval);
                }
            }

            private SourceSnapshot? ComputeSnapshot(DateTime date, ITransaction[] transactions, ISourceSnapshot[] snapshots)
            {
                var thisSnapshot = snapshots.FirstOrDefault(s => s.Date == date);
                if (thisSnapshot != null)
                    return new SourceSnapshot(thisSnapshot.Date, thisSnapshot.Amount);

                var anchorSnapshot = snapshots.LastOrDefault(s => s.Date < date)
                    ?? snapshots.FirstOrDefault(s => s.Date > date);
            
                if (anchorSnapshot == null)
                    return null;

                DateTime filterFrom, filterTo;
                if(anchorSnapshot.Date < date)
                {
                    filterFrom = anchorSnapshot.Date;
                    filterTo = date;
                }
                else
                {
                    filterFrom = date;
                    filterTo = anchorSnapshot.Date;
                }

                var filteredSum = transactions
                    .Where(p => p.Date >= filterFrom && p.Date <= filterTo)
                    .Sum(p => p.Amount);

                if (anchorSnapshot.Date < date)
                    return new SourceSnapshot(date, anchorSnapshot.Amount + filteredSum);
                else
                    return new SourceSnapshot(date, anchorSnapshot.Amount - filteredSum);
            }

            private async Task<(ITransaction[], ISourceSnapshot[])> GetTransactionsAndSnapshots(Query request)
            {
                int[] sources = request
                    .Sources
                    .Select(p => p.Id)
                    .ToArray();

                using var context = _dataContextProvider.CreateDataContext();

                var transactions = await context.Transactions
                    .Where(t => sources.Contains(t.SourceId)
                        && t.Date >= request.DateFrom
                        && t.Date <= request.DateTo)
                    .ToArrayAsync(context);

                var snapshots = await context.SourceSnapshots
                    .Where(t => sources.Contains(t.SourceId))
                    .OrderBy(t=>t.Date)
                    .ToArrayAsync(context);

                return (transactions, snapshots);
            }
        }
    }
}
