using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using FinancialDucks.Application.Extensions;

namespace FinancialDucks.Application.Features
{
    public class CashFlowReportFeature
    {
        public record Query(TransactionsFilter Filter, TimeInterval Interval)
            : IRequest<CashFlowReportItem[]> { }

        public class Handler : IRequestHandler<Query, CashFlowReportItem[]>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly ICategoryTreeProvider _categoryTreeProvider;
            private readonly ITransactionsQueryBuilder _transactionsQueryBuilder;

            public Handler(
                IDataContextProvider dataContextProvider,
                ICategoryTreeProvider categoryTreeProvider,
                ITransactionsQueryBuilder transactionsQueryBuilder)
            {
                _dataContextProvider = dataContextProvider;
                _categoryTreeProvider = categoryTreeProvider;
                _transactionsQueryBuilder = transactionsQueryBuilder;
            }

            public async Task<CashFlowReportItem[]> Handle(Query request, CancellationToken cancellationToken)
            {
                var results = new List<CashFlowReportItem>();
                var filter = request.Filter;
                var interval = request.Interval;

                var categoryTree = await _categoryTreeProvider.GetCategoryTree();
                var rangeStart = filter.RangeStart.Date;
                var rangeEnd = filter.RangeEnd.Date;

                using var context = _dataContextProvider.CreateDataContext();

                DateTime periodStart = rangeStart;
                while (periodStart <= rangeEnd)
                {
                    DateTime periodEnd = periodStart.Add(interval).AddDays(-1).EndOfDay();
                    if (periodEnd > rangeEnd)
                        periodEnd = rangeEnd.EndOfDay();

                    var periodFilter = new TransactionsFilter(
                        periodStart,
                        periodEnd,
                        filter.Category,
                        filter.Sources,
                        filter.IncludeTransfers,
                        filter.TextFilter
                    );

                    var query = _transactionsQueryBuilder.GetTransactionsQuery(
                        context,
                        categoryTree,
                        periodFilter
                    );

                    var transactions = await context.ToArrayAsync(query);

                    var debits = transactions.Where(t => t.Amount < 0).ToArray();
                    var credits = transactions.Where(t => t.Amount > 0).ToArray();

                    results.Add(new CashFlowReportItem(periodStart, periodEnd, debits, credits));

                    periodStart = periodEnd.AddDays(1).StartOfDay();
                }

                return results.ToArray();
            }
        }
    }
}