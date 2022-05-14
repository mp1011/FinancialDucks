namespace FinancialDucks.Application.Models.AppModels
{
    public enum FetchStatus
    {
        NotStarted,
        Processing,
        Failed,
        Done
    }

    public record SyncStatus(ITransactionSourceDetail Account, ITransaction[] DownloadedTransactions, DateTime? LastTransactionDate);
}
