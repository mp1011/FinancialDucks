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

            public Handler(IDataContextProvider dataContextProvider)
            {
                _dataContextProvider = dataContextProvider;
            }

            public async Task<CategoryStats> Handle(Query request, CancellationToken cancellationToken)
            {
                using var ctx = _dataContextProvider.CreateDataContext();

                var query = from t in ctx.TransactionsDetail
                        from cr in ctx.CategoryRulesDetail
                        where cr.CategoryId == request.Category.Id
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
