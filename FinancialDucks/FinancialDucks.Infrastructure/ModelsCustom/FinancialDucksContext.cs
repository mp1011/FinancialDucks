#nullable disable
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class FinancialDucksContext : IDataContext
    {
        private readonly ISettingsService _settingsService;
        private readonly IEqualityComparer<ITransaction> _transactionEqualityComparer;
        private readonly IObjectMapper _objectMapper;

        public FinancialDucksContext(ISettingsService settingsService, IEqualityComparer<ITransaction> transactionEqualityComparer,
            IObjectMapper objectMapper)
        {
            _settingsService = settingsService;
            _transactionEqualityComparer = transactionEqualityComparer;
            _objectMapper = objectMapper;
        }

        public IQueryable<ITransactionSourceDetail> TransactionSourcesDetail =>
            TransactionSources.AsNoTracking()
                .Include(x => x.TransactionSourceFileMappings);

        IQueryable<ITransactionSource> IDataContext.TransactionSources => TransactionSources.AsNoTracking();

        IQueryable<ITransactionDetail> IDataContext.TransactionsDetail =>
            Transactions.AsNoTracking()
                .Include(x => x.Source);

        IQueryable<ICategoryRuleDetail> IDataContext.CategoryRulesDetail =>
            CategoryRules.AsNoTracking()
                .Include(x => x.Category);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_settingsService.ConnectionString, x => x.UseHierarchyId());
            }
        }


        public async Task<ITransaction[]> UploadTransactions(ITransaction[] transactions)
        {
            var existing = Transactions.ToArray();

            var newRecords = transactions
                .Where(t=> !existing.Any(e=> _transactionEqualityComparer.Equals(e,t)))
                .ToArray();

            List<Transaction> insertedRows = new List<Transaction>();   

            foreach(var newRecord in newRecords)
            {
                var recordToInsert = _objectMapper.CopyIntoNew<ITransaction,Transaction>(newRecord);
                Transactions.Add(recordToInsert);
                insertedRows.Add(recordToInsert);
            }

            await SaveChangesAsync();

            return insertedRows.ToArray();
        }

        public async Task<ICategory> AddSubcategory(ICategory parent, string name)
        {
            using var transaction = Database.BeginTransaction();

            var parentCategory = Categories
                .AsNoTracking()
                .First(p => p.Id == parent.Id);

            var latestChild = Categories
                .AsNoTracking()
                .Where(p => p.HierarchyId.GetAncestor(1) == parentCategory.HierarchyId)
                .Select(p => p.HierarchyId)
                .OrderByDescending(p => p)
                .FirstOrDefault();

            var newCategory = new Category
            {
                Name = name,
                HierarchyId = parentCategory.HierarchyId.GetDescendant(latestChild, null)
            };

            Categories.Add(newCategory);
            await SaveChangesAsync();
            await transaction.CommitAsync();

            return newCategory;
        }

    }
}
