using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public record GetBudgetLinesQuery(DateTime Month) : IRequest<BudgetLineDetail[]>
    {
        public class Handler : IRequestHandler<GetBudgetLinesQuery, BudgetLineDetail[]>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly ITransactionsQueryBuilder _transactionsQueryBuilder;
            private readonly ICategoryTreeProvider _categoryTreeProvider;

            public Handler(IDataContextProvider dataContextProvider, ITransactionsQueryBuilder transactionsQueryBuilder, ICategoryTreeProvider categoryTreeProvider)
            {
                _dataContextProvider = dataContextProvider;
                _transactionsQueryBuilder = transactionsQueryBuilder;
                _categoryTreeProvider = categoryTreeProvider;
            }

            public async Task<BudgetLineDetail[]> Handle(GetBudgetLinesQuery request, CancellationToken cancellationToken)
            {
                List<BudgetLineDetail> detail = new();

                using var dataContext = _dataContextProvider.CreateDataContext();

                var budgetLines = await dataContext.BudgetLines.ToArrayAsync(dataContext);

                foreach(var line in budgetLines)
                {
                    detail.Add(await GetDetail(line, dataContext, request.Month));
                }

                return detail.ToArray();
            }

            private async Task<BudgetLineDetail> GetDetail(IBudgetLine budgetLine, IDataContext dataContext, DateTime Month)
            {
                var categoryTree = await _categoryTreeProvider.GetCategoryTree();
                var filterCategory = categoryTree.GetDescendant(budgetLine.Category.Id);

                var transactions = await _transactionsQueryBuilder
                    .GetTransactionsQuery(
                        dataContext,
                        categoryTree,
                        filter: new TransactionsFilter(DateTime.Now.AddYears(-1), DateTime.Now, filterCategory!, false))
                    .ToArrayAsync(dataContext);


                var start = DateTime.Now.AddYears(-1).GetClosestInterval(TimeInterval.Monthly);              
                var timeSlices = start.SliceTime(TimeInterval.Monthly, DateTime.Now);

                var sliceAmounts = timeSlices.Select(t =>
                {
                    var sliceTransactions = transactions
                        .Where(p => p.Date >= t.SliceStart && p.Date < t.SliceEnd)
                        .ToArray();

                    return new
                    {
                        Year = t.SliceStart.Year,
                        Month = t.SliceStart.Month,
                        Amount = -sliceTransactions.Sum(p => p.Amount)
                    };
                });

                float percent = 1.0f;

                var budgetAmount = sliceAmounts.Single(p => p.Year == Month.Year && p.Month == Month.Month);

                if (budgetLine.Budget > 0 && budgetAmount.Amount <= budgetLine.Budget)
                    percent = (float)(budgetAmount.Amount / budgetLine.Budget);

                if (percent < 0)
                    percent = 0;

                var budgetSlice = timeSlices.Single(p => p.SliceStart.Year == Month.Year && p.SliceStart.Month == Month.Month);

                return new BudgetLineDetail(budgetLine.Category,
                    PeriodStart: budgetSlice.SliceStart,
                    YearTotal: sliceAmounts.Sum(p=>p.Amount),
                    YearAvg: sliceAmounts.Average(p => p.Amount),
                    YearStd: sliceAmounts.Select(p=>p.Amount).StandardDeviation(),
                    Budget: budgetLine.Budget,
                    PercentMet: percent,
                    ActualSpent: budgetAmount.Amount);
            }
        }
    }

    public record EditBudgetLineCommand(ICategory Category, decimal Amount) : IRequest<IBudgetLine>
    {
        public class Handler : IRequestHandler<EditBudgetLineCommand, IBudgetLine>
        {
            private readonly IDataContext _dataContext;

            public Handler(IDataContext dataContext)
            {
                _dataContext = dataContext;
            }

            public async Task<IBudgetLine> Handle(EditBudgetLineCommand request, CancellationToken cancellationToken)
            {
                return await _dataContext.Update(new BudgetLineEdit { Category = request.Category, Budget = request.Amount });
            }
        }
    }
}
