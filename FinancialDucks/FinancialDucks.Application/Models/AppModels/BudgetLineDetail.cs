namespace FinancialDucks.Application.Models.AppModels
{
    public record BudgetLineDetail(
        ICategory Category,
        DateTime PeriodStart,
        decimal YearTotal,
        decimal YearAvg,
        decimal YearStd,
        decimal Budget,
        float PercentMet,
        decimal ActualSpent);
}
