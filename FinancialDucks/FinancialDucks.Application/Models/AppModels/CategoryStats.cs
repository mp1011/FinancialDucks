namespace FinancialDucks.Application.Models.AppModels
{
    public record CategoryStats(ICategory Category, int TransactionCount, decimal Total, DescriptionWithCount[] Descriptions);

    public record DescriptionWithCount(string Description, int Count);

    public record ChildCategoryStats(ICategory Category, int TransactionCount, decimal Total, double Percent);
    public record CategoryStatsWithChildren(CategoryStats Main, ChildCategoryStats[] Children);
}
