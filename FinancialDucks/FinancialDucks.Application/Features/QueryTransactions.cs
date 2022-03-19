using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public record QueryTransactions(DateTime RangeStart, DateTime RangeEnd, int Page, int ResultsPerPage) 
        : IRequest<ITransactionDetail[]> 
    {
        public class Handler : IRequestHandler<QueryTransactions, ITransactionDetail[]>
        {
            private readonly IDataContextProvider _dataContextProvider;

            public Handler(IDataContextProvider dataContextProvider)
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
    }
}
