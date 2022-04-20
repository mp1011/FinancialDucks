using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class CategoryStatsFeature
    {
        public record Query(TransactionsFilter Filter) 
            : IRequest<CategoryStats> 
        {

            public Query(ICategoryDetail Category)
                : this(Category, new DateTime(1900, 1, 1), DateTime.Now)
            {
            }

            public Query(ICategoryDetail Category, DateTime RangeStart, DateTime RangeEnd) 
                : this(new TransactionsFilter(RangeStart,RangeEnd,Category))
            {

            }
        }

        public record QueryWithChildren(TransactionsFilter Filter) : 
            IRequest<CategoryStatsWithChildren> 
        {
            public QueryWithChildren(ICategoryDetail Category, DateTime RangeStart, DateTime RangeEnd)
                   : this(new TransactionsFilter(RangeStart, RangeEnd, Category))
            {

            }
        }

        public class Handler 
            :   IRequestHandler<Query, CategoryStats>,
                IRequestHandler<QueryWithChildren, CategoryStatsWithChildren>

        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly ICategoryTreeProvider _categoryTreeProvider;
            private readonly ITransactionsQueryBuilder _transactionsQueryBuilder;

            public Handler(IDataContextProvider dataContextProvider, ICategoryTreeProvider categoryTreeProvider, ITransactionsQueryBuilder transactionsQueryBuilder)
            {
                _dataContextProvider = dataContextProvider;
                _categoryTreeProvider = categoryTreeProvider;
                _transactionsQueryBuilder = transactionsQueryBuilder;
            }

            private async Task<CategoryStats> GetStats(TransactionsFilter filter)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();

                var query = _transactionsQueryBuilder.GetTransactionsQuery(dataContext, filter.Category.Root(), filter);

                var result = await query
                     .DebugWatchCommandText(dataContext)
                     .ToArrayAsync(dataContext);

                return new CategoryStats(filter.Category, result.Count(), result.Sum(p => p.Amount));
            }

            public async Task<CategoryStats> Handle(Query request, CancellationToken cancellationToken)
            {
                return await GetStats(request.Filter);
            }

            public async Task<CategoryStatsWithChildren> Handle(QueryWithChildren request, CancellationToken cancellationToken)
            {
                var categories = await _categoryTreeProvider.GetCategoryTree();

                var tasks = categories.GetDescendant(request.Filter.Category.Id)!
                    .GetThisAndChildren()
                    .Select(c=> GetStats(request.Filter.ChangeCategory(c)))
                    .ToArray();

                var result = await Task.WhenAll(tasks);

                var parentStats = result.Single(p => p.Category.Id == request.Filter.Category.Id);
                var childStats = result
                    .Where(p => p.Category.Id != request.Filter.Category.Id)
                    .OrderByDescending(p => Math.Abs(p.Total))
                    .ToList();

                decimal miscAmount = parentStats.Total - childStats.Sum(p => p.Total);

                if (miscAmount != 0)
                    childStats.Add(new CategoryStats(new Category(0,"Other",false,null), 0, miscAmount));

                return new CategoryStatsWithChildren(
                    parentStats,
                    childStats.Select(c => new ChildCategoryStats(
                        Category: c.Category,
                        TransactionCount: c.TransactionCount,
                        Total: c.Total,
                        Percent: (double)c.Total / (double)parentStats.Total)).ToArray());                
            }
        }
    }
}
