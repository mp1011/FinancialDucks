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
            return dataContext.TransactionsWithCategories
                    .Except(GetTransferTransactions(dataContext, root));
        }

        private IQueryable<ITransactionDetail> GetUncategorizedTransactions(IDataContext dataContext, ICategoryDetail root)
        {
            var queryCategorized = from t in dataContext.TransactionsDetail
                                   from cr in dataContext.CategoryRulesDetail
                                   where cr.SubstringMatch != null
                                       && t.Description.Contains(cr.SubstringMatch)
                                   select t;

            return dataContext.TransactionsDetail
                            .Except(queryCategorized);
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
            else if(categoryIds.Length > 0)
                query = dataContext.TransactionsWithCategories
                    .Where(p => p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value));
            else
                query = GetNonTransferTransactions(dataContext, categoryTree);

            return query.Where(p=>p.Date >= filter.RangeStart && p.Date <= filter.RangeEnd);
        }
    
    
        public IQueryable<ITransactionDetail> GetTransactionsQuery(IDataContext dataContext,
                ICategoryDetail categoryTree,
                TransactionsFeature.TransactionsFilter filter)
        {
            return (from tc in GetTransactionCategoriesQuery(dataContext, categoryTree, filter)
                    join t in dataContext.TransactionsDetail on tc.Id equals t.Id
                    select t).Distinct();
        }
    }
}
