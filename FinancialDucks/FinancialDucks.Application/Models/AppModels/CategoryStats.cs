namespace FinancialDucks.Application.Models.AppModels
{
    public record CategoryStats(ICategoryDetail Category, int TransactionCount, decimal Total);

    public record ChildCategoryStats(ICategory Category, int TransactionCount, decimal Total, double Percent)
    {
        public ChildCategoryStats AdjustPercent(double newTotal)
        {
            return new ChildCategoryStats(Category, TransactionCount, Total, (double)Total / newTotal);
        }
    }
    public record CategoryStatsWithChildren(CategoryStats Main, ChildCategoryStats[] Children);
}
