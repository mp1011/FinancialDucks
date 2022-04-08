using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Application.Services
{
    public interface ITransactionsQueryBuilder
    {
        IQueryable<ITransactionWithCategory> GetTransactionCategoriesQuery(IDataContext dataContext, ICategoryDetail categoryTree, TransactionsFeature.TransactionsFilter filter);
        IQueryable<ITransactionDetail> GetTransactionsQuery(IDataContext dataContext, ICategoryDetail categoryTree, TransactionsFeature.TransactionsFilter filter);
    }

    public class TransactionsQueryBuilder : ITransactionsQueryBuilder
    {
        private IQueryable<ITransactionWithCategory> GetTransferTransactions(IDataContext dataContext, ICategoryDetail root)
        {
            var transferCategoryIds = root
                                        .GetDescendant(SpecialCategory.Transfers.ToString())!
                                        .GetThisAndAllDescendants()
                                        .Select(p => p.Id)
                                        .ToArray();

            return dataContext.TransactionsWithCategories
                .Where(c => c.CategoryId != null && transferCategoryIds.Contains(c.CategoryId.Value));
        }

        private IQueryable<ITransactionWithCategory> GetNonTransferTransactions(IDataContext dataContext, ICategoryDetail root)
        {
            var transactionQueryIds = GetTransferTransactions(dataContext, root)
                .Select(p => p.Id)
                .Distinct();

            return dataContext.TransactionsWithCategories
                    .Where(p=> !transactionQueryIds.Contains(p.Id));
        }

        public IQueryable<ITransactionWithCategory> GetTransactionCategoriesQuery(IDataContext dataContext,
                ICategoryDetail categoryTree,
                TransactionsFeature.TransactionsFilter filter)
        {
            int[] categoryIds = new int[0];
            if (filter.Category != null)
            {
                categoryIds = filter.Category
                    .GetThisAndAllDescendants()
                    .Select(c => c.Id)
                    .ToArray();
            }

            var unclassifiedCategory = categoryTree.GetDescendant(SpecialCategory.Unclassified.ToString())!;
            IQueryable<ITransactionWithCategory> query;

            if (filter.Category != null && filter.Category.Name == SpecialCategory.Debits.ToString())
                query = GetNonTransferTransactions(dataContext, categoryTree).Where(p => p.Amount < 0);
            else if (filter.Category != null && filter.Category.Name == SpecialCategory.Credits.ToString())
                query = GetNonTransferTransactions(dataContext, categoryTree).Where(p => p.Amount > 0);
            else if (filter.Category != null && filter.Category.Name == SpecialCategory.Unclassified.ToString())
                query = GetNonTransferTransactions(dataContext, categoryTree).Where(p => p.CategoryId == null);
            else if (categoryIds.Length > 0)
                query = dataContext.TransactionsWithCategories
                    .Where(p => p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value));
            else
                query = GetNonTransferTransactions(dataContext, categoryTree);

            string? textFilter = filter.TextFilter;
            if (string.IsNullOrEmpty(textFilter))
                textFilter = null;

            return query.Where(p=>p.Date >= filter.RangeStart 
                                && p.Date <= filter.RangeEnd
                                && (textFilter == null || p.Description.Contains(textFilter)));
        }
    
        public IQueryable<ITransactionDetail> ApplySorting(IQueryable<ITransactionDetail> query, 
            TransactionsFeature.TransactionsFilter filter)
        {
            if (filter.SortColumn == TransactionSortColumn.Amount && filter.SortDirection == SortDirection.Ascending)
                return query.OrderBy(p => p.Amount);
            if (filter.SortColumn == TransactionSortColumn.Amount && filter.SortDirection == SortDirection.Descending)
                return query.OrderByDescending(p => p.Amount);
            if (filter.SortColumn == TransactionSortColumn.Date && filter.SortDirection == SortDirection.Ascending)
                return query.OrderBy(p => p.Date);
            if (filter.SortColumn == TransactionSortColumn.Date && filter.SortDirection == SortDirection.Descending)
                return query.OrderByDescending(p => p.Date);

            return query;
        }

        public IQueryable<ITransactionDetail> GetTransactionsQuery(IDataContext dataContext,
                ICategoryDetail categoryTree,
                TransactionsFeature.TransactionsFilter filter)
        {
            var query = (from tc in GetTransactionCategoriesQuery(dataContext, categoryTree, filter)
                    join t in dataContext.TransactionsDetail on tc.Id equals t.Id
                    select t).Distinct();

            return ApplySorting(query, filter);
        }
    }
}
