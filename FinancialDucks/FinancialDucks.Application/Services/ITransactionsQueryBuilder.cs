using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Application.Services
{
    public interface ITransactionsQueryBuilder
    {
        IQueryable<ITransactionWithCategory> GetTransactionCategoriesQuery(IDataContext dataContext, ICategoryDetail categoryTree, TransactionsFilter filter);
        IQueryable<ITransactionDetail> GetTransactionsQuery(
            IDataContext dataContext, 
            ICategoryDetail categoryTree, 
            TransactionsFilter filter,
            TransactionSortColumn sortColumn = TransactionSortColumn.Date,
            SortDirection sortDirection =    SortDirection.Ascending);
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
                TransactionsFilter filter)
        {
            int[] categoryIds = new int[0];
            bool isCredit=false, isDebit = false;

            if (filter.Category != null)
            {
                categoryIds = filter.Category
                    .GetThisAndAllDescendants()
                    .Select(c => c.Id)
                    .ToArray();

                isCredit = filter.Category.IsDescendantOf(SpecialCategory.Credits.ToString());
                isDebit = filter.Category.IsDescendantOf(SpecialCategory.Debits.ToString());
            }


            var unclassifiedCategory = categoryTree.GetDescendant(SpecialCategory.Unclassified.ToString())!;
            IQueryable<ITransactionWithCategory> query;

            if (filter.Category != null && filter.Category.Name == SpecialCategory.Debits.ToString())
                query = GetNonTransferTransactions(dataContext, categoryTree).Where(p => p.Amount < 0);
            else if (filter.Category != null && filter.Category.Name == SpecialCategory.Credits.ToString())
                query = GetNonTransferTransactions(dataContext, categoryTree).Where(p => p.Amount > 0);
            else if (filter.Category != null && filter.Category.Name == SpecialCategory.Unclassified.ToString())
                query = GetNonTransferTransactions(dataContext, categoryTree).Where(p => p.CategoryId == unclassifiedCategory.Id);
            else if (categoryIds.Length > 0)
            {
                if (filter.IncludeTransfers)
                {
                    query = dataContext.TransactionsWithCategories
                        .Where(p => p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value));
                }
                else
                {
                    query = GetNonTransferTransactions(dataContext, categoryTree)
                       .Where(p => p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value));
                }
            }
            else
                query = GetNonTransferTransactions(dataContext, categoryTree);

            string? textFilter = filter.TextFilter;
            if (string.IsNullOrEmpty(textFilter))
                textFilter = null;

            if (isCredit)
                query = query.Where(p => p.Amount > 0);

            else if(isDebit)
                query = query.Where(p => p.Amount <= 0);

            int[] sourceIds = null;
            if (filter.Sources.NotNullOrEmpty())
                sourceIds = filter.Sources.Select(p => p.Id).ToArray();

            return query.Where(p=>p.Date >= filter.RangeStart.StartOfDay()
                                && p.Date <= filter.RangeEnd.EndOfDay()
                                && (sourceIds == null || sourceIds.Contains(p.SourceId))
                                && (textFilter == null || p.Description.Contains(textFilter)));
        }
    
        private IQueryable<ITransactionDetail> ApplySorting(IQueryable<ITransactionDetail> query, 
            TransactionSortColumn sortColumn, SortDirection sortDirection)
        {
            if (sortColumn == TransactionSortColumn.Amount && sortDirection == SortDirection.Ascending)
                return query.OrderBy(p => p.Amount);
            if (sortColumn == TransactionSortColumn.Amount && sortDirection == SortDirection.Descending)
                return query.OrderByDescending(p => p.Amount);
            if (sortColumn == TransactionSortColumn.Date && sortDirection == SortDirection.Ascending)
                return query.OrderBy(p => p.Date);
            if (sortColumn == TransactionSortColumn.Date && sortDirection == SortDirection.Descending)
                return query.OrderByDescending(p => p.Date);

            return query;
        }

        public IQueryable<ITransactionDetail> GetTransactionsQuery(IDataContext dataContext,
                ICategoryDetail categoryTree,
                TransactionsFilter filter,
                TransactionSortColumn sortColumn, 
                SortDirection sortDirection)
        {
            var query = (from tc in GetTransactionCategoriesQuery(dataContext, categoryTree, filter)
                    join t in dataContext.TransactionsDetail on tc.Id equals t.Id
                    select t).Distinct();

            return ApplySorting(query, sortColumn,sortDirection);
        }
    }
}
