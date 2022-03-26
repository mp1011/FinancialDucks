using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class TransactionsFeature
    {
        public record QueryTransactions(DateTime RangeStart, DateTime RangeEnd, int Page, int ResultsPerPage)
            : IRequest<ITransactionDetail[]> { }

        public record QueryTotalPages(DateTime RangeStart, DateTime RangeEnd, int ResultsPerPage)
           : IRequest<int>
        { }


        public class QueryTransactionsHandler : IRequestHandler<QueryTransactions, ITransactionDetail[]>
        {
            private readonly IDataContextProvider _dataContextProvider;

            public QueryTransactionsHandler(IDataContextProvider dataContextProvider)
            {
                _dataContextProvider = dataContextProvider;
            }

            public Task<ITransactionDetail[]> Handle(QueryTransactions request, CancellationToken cancellationToken)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();

                int start = request.Page * request.ResultsPerPage;

                var result = dataContext.TransactionsDetail
                    .Where(p => p.Date >= request.RangeStart
                            && p.Date <= request.RangeEnd)
                    .OrderBy(p => p.Date)
                    .Skip(start)
                    .Take(request.ResultsPerPage)
                    .ToArray();

                return Task.FromResult(result);
            }
        }

        public class QueryTransactionPagesHandler : IRequestHandler<QueryTotalPages, int>
        {
            private readonly IDataContextProvider _dataContextProvider;

            public QueryTransactionPagesHandler(IDataContextProvider dataContextProvider)
            {
                _dataContextProvider = dataContextProvider;
            }

            public Task<int> Handle(QueryTotalPages request, CancellationToken cancellationToken)
            {
                if (request.ResultsPerPage <= 0)
                    return Task.FromResult(0);

                using var dataContext = _dataContextProvider.CreateDataContext();

                var totalCount = dataContext.TransactionsDetail
                    .Where(p => p.Date >= request.RangeStart
                            && p.Date <= request.RangeEnd)
                    .Count();

                var count = (int)Math.Ceiling((decimal)totalCount / request.ResultsPerPage);
                return Task.FromResult(count);
            }
        }

    }
}
