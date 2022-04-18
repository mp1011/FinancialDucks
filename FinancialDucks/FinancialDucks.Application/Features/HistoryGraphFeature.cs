using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class HistoryGraphFeature
    {
        public record Query(TransactionsFilter Filter, TimeInterval TimeInterval)
            : IRequest<CategoryTimeSlice[]>
        { }

        public class Handler : IRequestHandler<Query, CategoryTimeSlice[]>
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

            public async Task<CategoryTimeSlice[]> Handle(Query request, CancellationToken cancellationToken)
            {
                if (!request.Filter.IsValid(requireCategory:true))
                    return new CategoryTimeSlice[] { };

                var snapshots = await GetSnapshots(request);

                var totalSum = snapshots.Sum(p => p.Amount);
                var timeSlices = request.Filter.RangeStart.GetClosestInterval(request.TimeInterval)
                    .SliceTime(request.TimeInterval, request.Filter.RangeEnd)
                    .ToArray();

                var directDistribution = timeSlices
                    .Select(t =>
                    {
                        var amount = snapshots
                            .Where(r => r.Date >= t.SliceStart && r.Date < t.SliceEnd)
                            .Sum(p => p.Amount)
                            .Round();

                        return new CategoryTimeSlice(amount, request.TimeInterval, t.SliceStart, t.SliceEnd);
                    })
                    .ToArray();

                var adjustedDistribution = DistributeOverGaps(directDistribution);
                return adjustedDistribution;
            }

            private CategoryTimeSlice[] DistributeOverGaps(CategoryTimeSlice[] slices)
            {
                List<CategoryTimeSlice> distributedSlices = new List<CategoryTimeSlice>();

                decimal amountToDistribute = 0;
                decimal distributionPerSlice = 0;
                int distributionsRemaining = 0;

                foreach (var index in Enumerable.Range(0, slices.Length))
                {
                    var slice = slices[index];

                    if(distributionsRemaining > 0)
                    {
                        distributedSlices.Add(Distribute(slices, index, distributionPerSlice, ref distributionsRemaining, ref amountToDistribute));
                        continue;
                    }

                    if (slice.Amount != 0)
                    {
                        distributedSlices.Add(slice);
                        continue;
                    }

                    var emptySequence = slices
                        .Skip(index)
                        .TakeWhile(r => r.Amount == 0)
                        .ToArray();

                    if (!emptySequence.Any())
                    {
                        distributedSlices.Add(slice);
                        continue;
                    }

                    var nextNonEmpty = slices
                        .Skip(index + emptySequence.Length)
                        .FirstOrDefault();

                    if(nextNonEmpty == null)
                    {
                        distributedSlices.Add(slice);
                        continue;
                    }

                    amountToDistribute = nextNonEmpty.Amount;
                    distributionPerSlice = (amountToDistribute / (emptySequence.Length + 1)).RoundDown();
                    distributionsRemaining = emptySequence.Length + 1;

                    distributedSlices.Add(Distribute(slices, index, distributionPerSlice, ref distributionsRemaining, ref amountToDistribute));
                }

                return distributedSlices.ToArray();
            }

            private CategoryTimeSlice Distribute(CategoryTimeSlice[] slices, 
                int index,
                decimal distributionPerSlice,
                ref int distributionsRemaining, 
                ref decimal amountToDistribute)
            {
                CategoryTimeSlice result;

                if (distributionsRemaining == 1)
                {
                    result = slices[index].ChangeAmount(amountToDistribute);
                    amountToDistribute = 0;
                }
                else
                {
                    result = slices[index].ChangeAmount(distributionPerSlice);
                    amountToDistribute -= distributionPerSlice;
                }

                distributionsRemaining--;
                return result;
            }

            private async Task<CategorySnapshot[]> GetSnapshots(Query request)
            {
                if (!request.Filter.IsValid(requireCategory: true))
                    return Array.Empty<CategorySnapshot>();

                using var dataContext = _dataContextProvider.CreateDataContext();

                var categories = await _categoryTreeProvider.GetCategoryTree();
                var category = categories.GetDescendant(request.Filter.Category.Id);

                var query = _transactionsQueryBuilder.GetTransactionsQuery(dataContext, categories, request.Filter);

                var resultsByDate = query
                    .Select(x => new { Date = x.Date, Amount = x.Amount })
                    .ToArray();

                return resultsByDate
                    .Select(p => new CategorySnapshot(p.Amount, p.Date))
                    .ToArray();
            }
        }
    }
}
