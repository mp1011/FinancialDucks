namespace FinancialDucks.Application.Models.AppModels
{
    public record CategoryStats(int TransactionCount, decimal Total, DescriptionWithCount[] Descriptions);

    public record DescriptionWithCount(string Description, int Count);
}
