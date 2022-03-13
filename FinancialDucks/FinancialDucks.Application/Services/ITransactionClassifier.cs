using FinancialDucks.Application.Models;

namespace FinancialDucks.Application.Services
{
    public interface ITransactionClassifier
    {
        Task<ICategoryDetail[]> Classify(ITransaction transaction);
    }

    public class TransactionClassifier : ITransactionClassifier
    {
        private readonly IDataContext _dataContext;
        private readonly ICategoryTreeProvider _categoryTreeProvider;

        public TransactionClassifier(IDataContext dataContext, ICategoryTreeProvider categoryTreeProvider)
        {
            _dataContext = dataContext;
            _categoryTreeProvider = categoryTreeProvider;
        }

        public async Task<ICategoryDetail[]> Classify(ITransaction transaction)
        {
            var rules = _dataContext.CategoryRulesDetail
                .Where(rule =>
                        (rule.SubstringMatch == null || transaction.Description.Contains(rule.SubstringMatch, StringComparison.OrdinalIgnoreCase))
                        && (rule.AmountMin == null || transaction.Amount >= rule.AmountMin)
                        && (rule.AmountMax == null || transaction.Amount <= rule.AmountMax)
                        && (rule.DateMin == null || transaction.Date >= rule.DateMin)
                        && (rule.DateMax == null || transaction.Date <= rule.DateMax))
                .ToArray();

            var results = rules.Select(p => p.Category)
               .Distinct()
               .ToArray();

            var categoryTree = await _categoryTreeProvider.GetCategoryTree();

            return results
                .Select(r => categoryTree.GetDescendant(r.Name))
                .ToArray();
        }
    }
}
