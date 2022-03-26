using FinancialDucks.Application.Models;
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

        public IQueryable<ICategoryRuleDetail> CategoryRulesDetail => _mockDataHelper.GetMockCategoryRules().AsQueryable();

        public Task<ICategoryRule> AddCategoryRule(ICategory category, ICategoryRule rule)
        {
            throw new NotImplementedException();
        }

        public Task<ICategory> AddSubcategory(ICategory category, string name)
        {
            return Task.FromResult(new TestCategory(0, name, category as TestCategory) as ICategory);
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
