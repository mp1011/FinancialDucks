namespace FinancialDucks.Application.Models
{
    public record ImportedTransaction(FileInfo SourceFile, decimal Amount, DateTime Date, string Description,
        int SourceId, int Id=0) : ITransaction
    {
    }
}
