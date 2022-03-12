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


    }
}
