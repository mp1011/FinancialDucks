using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class CategoryStatsFeature
    {
        public record Query(ICategory Category) : IRequest<CategoryStats> { }

        public class Handler : IRequestHandler<Query, CategoryStats>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly ICategoryTreeProvider _categoryTreeProvider;

            public Handler(IDataContextProvider dataContextProvider, ICategoryTreeProvider categoryTreeProvider)
            {
                _dataContextProvider = dataContextProvider;
                _categoryTreeProvider = categoryTreeProvider;
            }

            public async Task<CategoryStats> Handle(Query request, CancellationToken cancellationToken)
            {
                var categories = await _categoryTreeProvider.GetCategoryTree();

                var requestCategory = categories.GetDescendant(request.Category.Id);
                int[] requestCategoryIds = requestCategory
                                        .GetThisAndAllDescendants()
                                        .Select(x => x.Id)
                                        .ToArray();

                using var ctx = _dataContextProvider.CreateDataContext();

                var query = from t in ctx.TransactionsDetail
                        from cr in ctx.CategoryRulesDetail
                        where requestCategoryIds.Contains(cr.CategoryId)
                            && cr.SubstringMatch != null
                            && t.Description.Contains(cr.SubstringMatch)
                        select new { Transaction = t, Category = cr.Category };

                var result = await query
                     .DebugWatchCommandText(ctx)
                     .ToArrayAsync(ctx);

                return new CategoryStats(result.Count(), result.Sum(p => p.Transaction.Amount));
            }
        }
    }
}
