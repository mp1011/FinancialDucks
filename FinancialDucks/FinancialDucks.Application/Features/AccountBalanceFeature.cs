using FinancialDucks.Application.Extensions;
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
                    var snapshot = ComputeSnapshot(date, transactions, snapshots, request.Interval);
                    if (snapshot != null)
                        yield return snapshot;

                    date = date.Add(request.Interval);
                }
            }
            private SourceSnapshot ComputeSnapshot(DateTime date, ITransaction[] transactions, ISourceSnapshot[] snapshots, TimeInterval timeInterval)
            {
                decimal sum = 0;

                foreach(var sourceGroup in transactions.GroupBy(p=>p.SourceId))
                {
                    var sourceSnapshot = ComputeSnapshotForSingleSource(date,
                        sourceGroup.ToArray(),
                        snapshots.Where(p => p.SourceId == sourceGroup.Key).ToArray(),
                        timeInterval);

                    sum += sourceSnapshot.Amount;
                }

                return new SourceSnapshot(date, date.GetLabel(timeInterval), sum);
            }
            
            private SourceSnapshot ComputeSnapshotForSingleSource(DateTime date, ITransaction[] transactions, ISourceSnapshot[] snapshots, TimeInterval timeInterval)
            {
                var thisSnapshot = snapshots.FirstOrDefault(s => s.Date == date);
                if (thisSnapshot != null)
                    return new SourceSnapshot(thisSnapshot.Date, thisSnapshot.Date.GetLabel(timeInterval), thisSnapshot.Amount);

                var anchorSnapshot = new SourceSnapshot(snapshots.LastOrDefault(s => s.Date < date)
                    ?? snapshots.FirstOrDefault(s => s.Date > date), timeInterval);


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
                    return new SourceSnapshot(date, date.GetLabel(timeInterval), anchorSnapshot.Amount + filteredSum);
                else
                    return new SourceSnapshot(date, date.GetLabel(timeInterval), anchorSnapshot.Amount - filteredSum);
            }

            private async Task<(ITransaction[], ISourceSnapshot[])> GetTransactionsAndSnapshots(Query request)
            {

                using var context = _dataContextProvider.CreateDataContext();

                int[] sources;
                if (request.Sources != null)
                {
                    sources = request
                       .Sources
                       .Select(p => p.Id)
                       .ToArray();
                }
                else
                {
                    sources = await context
                        .TransactionSources
                        .Select(p => p.Id)
                        .ToArrayAsync(context);
                }


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
