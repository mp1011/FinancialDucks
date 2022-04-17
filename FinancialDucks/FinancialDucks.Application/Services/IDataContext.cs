using FinancialDucks.Application.Models;
using System.Diagnostics;

namespace FinancialDucks.Application.Services
{
    public interface IDataContext : IDisposable
    {
        IQueryable<ITransactionSource> TransactionSources { get; }
        IQueryable<ITransactionSourceDetail> TransactionSourcesDetail { get; }
        IQueryable<ITransactionDetail> TransactionsDetail { get; }
        IQueryable<ITransaction> Transactions { get; }
        IQueryable<ISourceSnapshot> SourceSnapshots { get; }
        IQueryable<ICategoryRuleDetail> CategoryRulesDetail { get; }
        IQueryable<ITransactionWithCategory> TransactionsWithCategories { get; }
        Task<ITransaction[]> UploadTransactions(ITransaction[] transactions);
        Task<ICategory> AddSubcategory(ICategory parent, string name);
        Task<ICategoryRuleDetail> AddCategoryRule(ICategory category, ICategoryRule rule);
        Task<ICategory> Update(ICategory category);
        Task<ICategory> Delete(ICategory category);
        Task<ICategoryRuleDetail> Delete(ICategoryRule rule);
        Task<T[]> ToArrayAsync<T>(IQueryable<T> query);
        IQueryable<T> WatchCommandText<T>(IQueryable<T> query, Action<string> watcher);
    }

    public static class IDataContextExtensions
    {
        public static IQueryable<T> WatchCommandText<T>(this IQueryable<T> query, IDataContext context, Action<string> watcher)
        {
            return context.WatchCommandText(query, watcher);
        }

        public static async Task<T[]> ToArrayAsync<T>(this IQueryable<T> query, IDataContext context)
        {
            return await context.ToArrayAsync(query);
        }

        public static IQueryable<T> DebugWatchCommandText<T>(this IQueryable<T> query, IDataContext context)
        {
            return context.WatchCommandText(query, s=> Debug.WriteLine(s));
        }
    }
}
