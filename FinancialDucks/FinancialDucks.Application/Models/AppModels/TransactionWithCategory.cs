namespace FinancialDucks.Application.Models.AppModels
{
    public record TransactionWithCategory(int Id, int SourceId, string Description, DateTime Date, decimal Amount, int? CategoryId, string Category) : ITransactionWithCategory
    {
    }
}
