using FinancialDucks.Application.Models;

namespace FinancialDucks.Application.Services
{
    public interface ITransactionClassifier
    {
        Task<ICategoryDetail[]> Classify(ITransaction transaction);
    }

    public class TransactionClassifier : ITransactionClassifier
    {
        private readonly IDataContextProvider _dataContextProvider;
        private readonly ICategoryTreeProvider _categoryTreeProvider;

        public TransactionClassifier(IDataContextProvider dataContextProvider, ICategoryTreeProvider categoryTreeProvider)
        {
            _dataContextProvider = dataContextProvider;
            _categoryTreeProvider = categoryTreeProvider;
        }

        public async Task<ICategoryDetail[]> Classify(ITransaction transaction)
        {
            using var dataContext = _dataContextProvider.CreateDataContext();
            var rules = dataContext.CategoryRulesDetail
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
                .Where(p=>p!= null)
                .ToArray()!;
        }
    }
}
