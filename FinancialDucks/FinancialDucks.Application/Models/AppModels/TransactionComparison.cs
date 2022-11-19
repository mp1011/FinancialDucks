namespace FinancialDucks.Application.Models.AppModels
{
    public record TransactionComparison(ICategoryDetail Category, decimal BaseValue, decimal CompareValue);
}
