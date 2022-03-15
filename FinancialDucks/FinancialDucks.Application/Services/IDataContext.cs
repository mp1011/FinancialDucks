using FinancialDucks.Application.Models;

namespace FinancialDucks.Application.Services
{
    public interface IDataContext
    {
        IQueryable<ITransactionSource> TransactionSources { get; }
        IQueryable<ITransactionSourceDetail> TransactionSourcesDetail { get; }
        IQueryable<ITransactionDetail> TransactionsDetail { get; }
        IQueryable<ICategoryRuleDetail> CategoryRulesDetail { get; }
        Task<ITransaction[]> UploadTransactions(ITransaction[] transactions);
        Task<ICategory> AddSubcategory(ICategory parent, string name);
    }
}
