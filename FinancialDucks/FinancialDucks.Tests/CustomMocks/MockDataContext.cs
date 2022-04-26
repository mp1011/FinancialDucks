using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using FinancialDucks.Tests.TestModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialDucks.Tests.CustomMocks
{
    class MockDataContext : IDataContext
    {
        private readonly MockDataHelper _mockDataHelper;

        public MockDataContext(MockDataHelper mockDataHelper)
        {
            _mockDataHelper = mockDataHelper;
        }

        public IQueryable<ITransactionSource> TransactionSources => throw new NotImplementedException();

        public IQueryable<ITransactionSourceDetail> TransactionSourcesDetail => _mockDataHelper.GetMockTransactionSources().AsQueryable();

        public IQueryable<ITransactionDetail> TransactionsDetail => _mockDataHelper.MockTransations.AsQueryable();

        public IQueryable<ITransaction> Transactions => _mockDataHelper.MockTransations.AsQueryable();

        public IQueryable<ISourceSnapshot> SourceSnapshots => _mockDataHelper.MockSourceSnapshots.AsQueryable();

        public IQueryable<ICategoryRuleDetail> CategoryRulesDetail => _mockDataHelper.MockCategoryRules.AsQueryable();

        public IQueryable<IScraperCommandDetail> ScraperCommandsDetail => _mockDataHelper.MockScraperCommands.AsQueryable();

        public IQueryable<ITransactionWithCategory> TransactionsWithCategories
        {
            get
            {
                return _mockDataHelper.MockTransations
                    .SelectMany(t =>
                    {
                        var matchingRules = _mockDataHelper.MockCategoryRules
                            .Where(r => r.SubstringMatch != null && t.Description.Contains(r.SubstringMatch))
                            .ToArray();

                        if (matchingRules.Any())
                            return matchingRules.Select(mr => new TransactionWithCategory(t.Id, t.SourceId, t.Description, t.Date, t.Amount, mr.CategoryId, mr.Category.Name));
                        else
                            return new TransactionWithCategory[] { new TransactionWithCategory(t.Id, t.SourceId, t.Description, t.Date, t.Amount, null, null) };
                    }).AsQueryable();                       
            }
        }

       
        public Task<ICategoryRuleDetail> AddCategoryRule(ICategory category, ICategoryRule rule)
        {
            throw new NotImplementedException();
        }

        public Task<ICategory> AddSubcategory(ICategory category, string name)
        {
            return Task.FromResult(new TestCategory(0, name, category as TestCategory) as ICategory);
        }

        public Task<ICategory> Delete(ICategory category)
        {
            throw new NotImplementedException();
        }

        public Task<ICategoryRuleDetail> Delete(ICategoryRule rule)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public Task<T[]> ToArrayAsync<T>(IQueryable<T> query)
        {
            return Task.FromResult(query.ToArray());
        }

        public Task<ICategory> Update(ICategory category)
        {
            throw new NotImplementedException();
        }

        public Task<ITransaction[]> UploadTransactions(ITransaction[] transactions)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> WatchCommandText<T>(IQueryable<T> query, Action<string> watcher)
        {
            return query;
        }
    }
}
