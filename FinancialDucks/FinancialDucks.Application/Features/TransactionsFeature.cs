using FinancialDucks.Application.Extensions;
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
            : IRequest<TransactionWithCategories[]> { }

        public record QuerySummary(TransactionsFilter Filter, int ResultsPerPage)
           : IRequest<TransactionsSummary>
        { }


        public class QueryTransactionsHandler : IRequestHandler<QueryTransactions, TransactionWithCategories[]>
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

            public async Task<TransactionWithCategories[]> Handle(QueryTransactions request, CancellationToken cancellationToken)
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

                var transactions = await query.ToArrayAsync(dataContext);

                var transactionIds = transactions.Select(p=>p.Id).ToArray();

                var transactionsWithCategories = dataContext
                    .TransactionsWithCategories
                    .Where(p => transactionIds.Contains(p.Id))
                    .ToArray();


                var result = transactions.Select(t =>
                    new TransactionWithCategories(
                        Transaction: t,
                        Categories: GetCategoriesForTransaction(transactionsWithCategories, categoryTree, t)))
                    .ToArray();


                return result;
            }

            private IEnumerable<ICategory> GetCategoriesForTransaction(ITransactionWithCategory[] transactionsWithCategories, ICategoryDetail categoryTree, ITransaction transaction)
            {
                var categories = transactionsWithCategories
                                       .Where(p => p.Id == transaction.Id)
                                       .Select(c => categoryTree.GetDescendant(c.CategoryId.GetValueOrDefault()))
                                       .Where(p=> p != null)
                                       .Distinct()
                                       .ToArray();

                return categories
                    .Where(c => !c.GetDescendants().Intersect(categories).Any())
                    .ToArray();
            }
        }

        public class QueryTransactionPagesHandler : IRequestHandler<QuerySummary, TransactionsSummary>
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

            public async Task<TransactionsSummary> Handle(QuerySummary request, CancellationToken cancellationToken)
            {
                if (request.ResultsPerPage <= 0)
                    return new TransactionsSummary(0,0,0);

                var categoryTree = await _categoryTreeProvider.GetCategoryTree();
                using var dataContext = _dataContextProvider.CreateDataContext();

                var query = _transactionQueryBuilder.GetTransactionsQuery(dataContext, categoryTree, request.Filter);

                var totalCount = query.Count();

                var count = (int)Math.Ceiling((decimal)totalCount / request.ResultsPerPage);
                var credits = query.Where(p => p.Amount > 0).Sum(p => p.Amount);
                var debits = query.Where(p => p.Amount < 0).Sum(p => p.Amount).Abs();

                return new TransactionsSummary(count, credits, debits);
            }
        }

    }
}
