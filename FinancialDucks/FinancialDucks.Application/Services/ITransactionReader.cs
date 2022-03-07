using FinancialDucks.Application.Models;

namespace FinancialDucks.Application.Services
{
    public interface ITransactionReader
    {
        Task<ITransaction[]> ParseTransactions(FileInfo file);
    }
}
