using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class TransactionsFeature
    {
        public record QueryTransactions(
            TransactionsFilter Filter, 
            TransactionSortColumn SortColumn,      
            SortDirection SortDirection,
            int Page, 
            int ResultsPerPage)
            : IRequest<ITransactionDetail[]> { }

        public record QueryTotalPages(TransactionsFilter Filter, int ResultsPerPage)
           : IRequest<int>
        { }


        public class QueryTransactionsHandler : IRequestHandler<QueryTransactions, ITransactionDetail[]>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly ITransactionsQueryBuilder _transactionQueryBuilder;
            private readonly ICategoryTreeProvider _categoryTreeProvider;

            public QueryTransactionsHandler(IDataContextProvider dataContextProvider, ITransactionsQueryBuilder transactionQueryBuilder, ICategoryTreeProvider categoryTreeProvider)
            {
                _dataContextProvider = dataContextProvider;
                _transactionQueryBuilder = transactionQueryBuilder;
                _categoryTreeProvider = categoryTreeProvider;
            }

            public async Task<ITransactionDetail[]> Handle(QueryTransactions request, CancellationToken cancellationToken)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();

                int start = request.Page * request.ResultsPerPage;

                var categoryTree = await _categoryTreeProvider.GetCategoryTree();
                var query = _transactionQueryBuilder.GetTransactionsQuery(dataContext, categoryTree, request.Filter,
                    request.SortColumn, request.SortDirection);

                query = query
                    .Skip(start)
                    .Take(request.ResultsPerPage)
                    .DebugWatchCommandText(dataContext);

                return await query.ToArrayAsync(dataContext);
            }
        }

        public class QueryTransactionPagesHandler : IRequestHandler<QueryTotalPages, int>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly ITransactionsQueryBuilder _transactionQueryBuilder;
            private readonly ICategoryTreeProvider _categoryTreeProvider;

            public QueryTransactionPagesHandler(IDataContextProvider dataContextProvider, ITransactionsQueryBuilder transactionQueryBuilder, ICategoryTreeProvider categoryTreeProvider)
            {
                _dataContextProvider = dataContextProvider;
                _transactionQueryBuilder = transactionQueryBuilder;
                _categoryTreeProvider = categoryTreeProvider;
            }

            public async Task<int> Handle(QueryTotalPages request, CancellationToken cancellationToken)
            {
                if (request.ResultsPerPage <= 0)
                    return 0;

                var categoryTree = await _categoryTreeProvider.GetCategoryTree();
                using var dataContext = _dataContextProvider.CreateDataContext();

                var query = _transactionQueryBuilder.GetTransactionsQuery(dataContext, categoryTree, request.Filter);

                var totalCount = query.Count();

                var count = (int)Math.Ceiling((decimal)totalCount / request.ResultsPerPage);
                return count;
            }
        }

    }
}
