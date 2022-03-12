using FinancialDucks.Application.Models;

namespace FinancialDucks.Application.Services
{
    public interface IDataContext
    {
        IQueryable<ITransactionSource> TransactionSources { get; }
        IQueryable<ITransactionSourceDetail> TransactionSourcesDetail { get; }
        IQueryable<ITransactionDetail> TransactionsDetail { get; }

        Task<ITransaction[]> UploadTransactions(ITransaction[] transactions);
    }
}
