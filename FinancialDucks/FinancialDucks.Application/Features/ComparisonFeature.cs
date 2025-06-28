using FinancialDucks.Application.Models.AppModels;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public record ComparisonFeatureQuery(TransactionsFilter Filter, DateTime CompareDateStart) : IRequest<TransactionComparison[]>
    {
        public class Handler : IRequestHandler<ComparisonFeatureQuery, TransactionComparison[]>
        {
            private readonly IMediator _mediator;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public async Task<TransactionComparison[]> Handle(ComparisonFeatureQuery request, CancellationToken cancellationToken)
            {
                var baseValues = await _mediator.Send(new CategoryStatsFeature.QueryWithChildren(request.Filter));
                var compareValues = await _mediator.Send(new CategoryStatsFeature.QueryWithChildren(CreateComparisonFilter(request)));

                var baseValuesByCategory = baseValues.Children
                    .ToDictionary(k => k.Category.Id, v => v.Total);

                var compareValuesByCategory = compareValues.Children
                    .ToDictionary(k => k.Category.Id, v => v.Total);

                var categories = baseValues.Children.Select(p => p.Category)
                    .Union(compareValues.Children.Select(p => p.Category))
                    .GroupBy(p=>p.Id)
                    .Select(p=>p.First())
                    .ToArray();

                var beforeValues = (request.Filter.RangeStart < request.CompareDateStart) ? baseValuesByCategory : compareValuesByCategory;
                var afterValues = (request.Filter.RangeStart > request.CompareDateStart) ? baseValuesByCategory : compareValuesByCategory;

                var result = new List<TransactionComparison>
                {
                    new TransactionComparison(
                    Category: baseValues.Main.Category,
                    BaseValue: baseValues.Main.Total,
                    CompareValue: compareValues.Main.Total)
                };

                result.AddRange(categories.Select(c =>
                    new TransactionComparison(
                        Category: c,
                        BaseValue: beforeValues.GetValueOrDefault(c.Id),
                        CompareValue: afterValues.GetValueOrDefault(c.Id))
                    ));

                return result.ToArray();


            }


            private TransactionsFilter CreateComparisonFilter(ComparisonFeatureQuery request)
            {
                var range = request.Filter.RangeEnd - request.Filter.RangeStart;

                return new TransactionsFilter(
                    RangeStart: request.CompareDateStart,
                    RangeEnd: request.CompareDateStart + range,
                    Category: request.Filter.Category,
                    TextFilter: request.Filter.TextFilter);
            }
        }
    }
}
