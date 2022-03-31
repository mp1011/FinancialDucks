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

        public class Handler : IRequestHandler<Query, CategoryStats>
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

            public async Task<CategoryStats> Handle(Query request, CancellationToken cancellationToken)
            {
                var categories = await _categoryTreeProvider.GetCategoryTree();
                using var dataContext = _dataContextProvider.CreateDataContext();

                var category = categories.GetDescendant(request.Category.Id);

                var query = _transactionsQueryBuilder.GetTransactionCategoriesQuery(dataContext, categories,
                    new TransactionsFeature.TransactionsFilter(
                        RangeStart: new DateTime(1900, 1, 1),
                        RangeEnd: DateTime.Now,
                        Category: category));

                var result = await query
                     .DebugWatchCommandText(dataContext)
                     .ToArrayAsync(dataContext);

                var descriptionCounts = result
                    .Where(p=>p.CategoryId == null)
                    .Select(p => p.Description.CleanNumbersAndSpecialCharacters())
                    .GroupBy(g => g)
                    .Select(g => new DescriptionWithCount(g.Key, g.Count()))
                    .OrderByDescending(p => p.Count)
                    .ToArray();

                return new CategoryStats(result.Count(), result.Sum(p => p.Amount), descriptionCounts);
            }
        
        }
    }
}
