using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using System.Linq.Expressions;

namespace FinancialDucks.Application.Features
{
    public class CategoryStatsFeature
    {
        public record Query(ICategory Category) : IRequest<CategoryStats> { }
        public record QueryWithChildren(ICategory Category) : IRequest<CategoryStatsWithChildren> { }

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

            private async Task<CategoryStats> GetStats(ICategoryDetail category)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();

                var query = _transactionsQueryBuilder.GetTransactionsQuery(dataContext, category.Root(),
                    new TransactionsFeature.TransactionsFilter(
                        RangeStart: new DateTime(1900, 1, 1),
                        RangeEnd: DateTime.Now,
                        Category: category));

                var result = await query
                     .DebugWatchCommandText(dataContext)
                     .ToArrayAsync(dataContext);

                return new CategoryStats(category, result.Count(), result.Sum(p => p.Amount));
            }

            public async Task<CategoryStats> Handle(Query request, CancellationToken cancellationToken)
            {
                var categories = await _categoryTreeProvider.GetCategoryTree();
                var category = categories.GetDescendant(request.Category.Id);
                return await GetStats(category);                
            }

            public async Task<CategoryStatsWithChildren> Handle(QueryWithChildren request, CancellationToken cancellationToken)
            {
                var categories = await _categoryTreeProvider.GetCategoryTree();

                var tasks = categories.GetDescendant(request.Category.Id)!
                    .GetThisAndChildren()
                    .Select(c=> GetStats(c))
                    .ToArray();

                var result = await Task.WhenAll(tasks);

                var parentStats = result.Single(p => p.Category.Id == request.Category.Id);
                var childStats = result
                    .Where(p => p.Category.Id != request.Category.Id)
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
