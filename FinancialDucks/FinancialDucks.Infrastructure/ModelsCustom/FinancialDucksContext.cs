#nullable disable
using FinancialDucks.Application.Extensions;
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
                .Include(x => x.TransactionSourceFileMappings)
                .Include(x => x.Type)
                .Include(x => x.ScraperCommands)
                .Include(x => x.SourceSnapshots);

        IQueryable<ITransactionSource> IDataContext.TransactionSources => TransactionSources.AsNoTracking();

        IQueryable<ITransactionDetail> IDataContext.TransactionsDetail =>
            Transactions.AsNoTracking()
                .Include(x => x.Source);

        IQueryable<ITransaction> IDataContext.Transactions =>
            Transactions.AsNoTracking();

        IQueryable<ICategoryRuleDetail> IDataContext.CategoryRulesDetail =>
            CategoryRules.AsNoTracking()
                .Include(x => x.Category);

        IQueryable<ITransactionWithCategory> IDataContext.TransactionsWithCategories => 
            TransactionCategories.AsNoTracking();

        IQueryable<ISourceSnapshot> IDataContext.SourceSnapshots =>
            SourceSnapshots.AsNoTracking();

        IQueryable<IScraperCommandDetail> IDataContext.ScraperCommandsDetail =>
            ScraperCommands
                .Include(s=>s.Source)
                .AsNoTracking();

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
                recordToInsert.Description = recordToInsert.Description.CleanExtraSpaces();
                Transactions.Add(recordToInsert);
                insertedRows.Add(recordToInsert);
            }

            await SaveChangesAsync();

            return insertedRows.ToArray();
        }

        public async Task<ICategory> AddSubcategory(ICategory parent, string name)
        {

            var existing = await Categories
                .FirstOrDefaultAsync(c => c.Name == name);

            if (existing != null)
                return existing;

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

            var autoRule = new CategoryRule
            {
                CategoryId = newCategory.Id,
                SubstringMatch = newCategory.Name
            };
            CategoryRules.Add(autoRule);
            await SaveChangesAsync();


            await transaction.CommitAsync();

            return newCategory;
        }

        public async Task<ICategoryRuleDetail> AddCategoryRule(ICategory category, ICategoryRule rule)
        {
            using var transaction = await Database.BeginTransactionAsync();

            var newRule = _objectMapper.CopyIntoNew<ICategoryRule, CategoryRule>(rule);
            CategoryRules.Add(newRule);
            await SaveChangesAsync();
            await transaction.CommitAsync();

            return CategoryRules
                .Include(p => p.Category)
                .First(p => p.Id == newRule.Id);
        }

        async Task<T[]> IDataContext.ToArrayAsync<T>(IQueryable<T> query)
        {
            return await query.ToArrayAsync();
        }

        IQueryable<T> IDataContext.WatchCommandText<T>(IQueryable<T> query, Action<string> watcher)
        {
            var txt = query.ToQueryString();
            watcher?.Invoke(txt);
            return query;
        }

        public async Task<ICategory> Update(ICategory category)
        {
            using var transaction = await Database.BeginTransactionAsync();

            var dbRecord = Categories.FirstOrDefault(c => c.Id == category.Id);
            if (dbRecord == null)
                throw new Exception($"Category {category.Id} not found");

            _objectMapper.CopyAllProperties(category, dbRecord);
            Entry(dbRecord).State = EntityState.Modified;
            await SaveChangesAsync();
            await transaction.CommitAsync();

            return dbRecord;
        }

        public async Task<ICategory> Delete(ICategory category)
        {
            using var transaction = await Database.BeginTransactionAsync();

            var dbRecord = Categories.FirstOrDefault(c => c.Id == category.Id);
            if (dbRecord == null)
                return category;

            var categoryAnddescendants = Categories
                .Include(p=>p.CategoryRules)
                .Where(p => p.HierarchyId.IsDescendantOf(dbRecord.HierarchyId))
                .ToArray();

            var descendantRules = categoryAnddescendants
                .SelectMany(p => p.CategoryRules);

            CategoryRules.RemoveRange(descendantRules);
            Categories.RemoveRange(categoryAnddescendants);


            await SaveChangesAsync();
            await transaction.CommitAsync();

            return dbRecord;
        }

        public async Task<ICategoryRuleDetail?> Delete(ICategoryRule category)
        {
            using var transaction = await Database.BeginTransactionAsync();

            var dbRecord = CategoryRules
                            .Include(p=>p.Category)
                            .FirstOrDefault(c => c.Id == category.Id);

            if (dbRecord == null)
                return null;

            CategoryRules.Remove(dbRecord);
           
            await SaveChangesAsync();
            await transaction.CommitAsync(); 

            return dbRecord;
        }

        async Task<IScraperCommandDetail> IDataContext.Update(IScraperCommandDetail command)
        {
            using var transaction = await Database.BeginTransactionAsync();
            
            var dbRecord = await ScraperCommands.FirstOrDefaultAsync(c => c.Id == command.Id);
            if (dbRecord == null)
            {
                dbRecord = _objectMapper.CopyIntoNew<IScraperCommandDetail, ScraperCommand>(command,
                    new Dictionary<Type, Type> { { typeof(ITransactionSource), typeof(TransactionSource) } });

                Entry(dbRecord.Source).State = EntityState.Unchanged;
                ScraperCommands.Add(dbRecord);
            }
            else
            {
                Entry(dbRecord).State = EntityState.Modified;               
                _objectMapper.CopyAllProperties(command, dbRecord);
                if(dbRecord.Source != null)
                    Entry(dbRecord.Source).State = EntityState.Unchanged;
            }

            await SaveChangesAsync();
            await transaction.CommitAsync();

            return dbRecord;
        }

        async Task<IScraperCommandDetail> IDataContext.Delete(IScraperCommandDetail command)
        {
            using var transaction = await Database.BeginTransactionAsync();

            var dbRecord = ScraperCommands
                            .FirstOrDefault(c => c.Id == command.Id);

            if (dbRecord == null)
                return null;

            ScraperCommands.Remove(dbRecord);

            await SaveChangesAsync();
            await transaction.CommitAsync();

            return dbRecord;
        }

        public async Task<ITransactionSourceDetail> Update(ITransactionSource source)
        {
            using var transaction = await Database.BeginTransactionAsync();
            var dbRecord = TransactionSources
                .FirstOrDefault(p => p.Id == source.Id);

            Entry(dbRecord).State = EntityState.Modified;
            _objectMapper.CopyAllProperties(source, dbRecord);

            await SaveChangesAsync();
            await transaction.CommitAsync();

            return dbRecord;
        }
    }
}
