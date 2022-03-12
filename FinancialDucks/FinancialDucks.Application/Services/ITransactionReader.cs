using CsvHelper;
using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using System.Globalization;

namespace FinancialDucks.Application.Services
{
    public interface ITransactionReader
    {
        Task<ITransaction[]> ParseTransactions(FileInfo file);
    }

    public class TransactionReader : ITransactionReader
    {
        private readonly ITransactionFileSourceIdentifier _transactionFileSourceIdentifier;

        public TransactionReader(ITransactionFileSourceIdentifier transactionFileSourceIdentifier)
        {
            _transactionFileSourceIdentifier = transactionFileSourceIdentifier;
        }

        public async Task<ITransaction[]> ParseTransactions(FileInfo file)
        {

            using var reader = new StreamReader(file.OpenRead());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();

            List<ITransaction> transactions = new List<ITransaction>();

            while(await csv.ReadAsync())
            {
                if (csv.HeaderRecord.Length <= 1)
                    csv.ReadHeader();
                else
                {
                    transactions.Add(new ImportedTransaction(
                        SourceFile: file,
                        Amount: ReadAmount(csv),
                        Date: ReadDate(csv),
                        Description: ReadDescription(csv),
                        SourceId: _transactionFileSourceIdentifier.DetectTransactionSource(file).Id));
                }
            }

            return transactions
                    .ToArray();
        }

        private decimal ReadAmount(CsvReader csv)
        {
            decimal credit = csv.TryReadDecimal("Credit","Amount Credit");
            decimal debit = csv.TryReadDecimal("Debit","Amount Debit");

            return Math.Abs(credit) - Math.Abs(debit);
        }

        private DateTime ReadDate(CsvReader csv)
        {
            var date = csv.TryReadDate("Date", "Transaction Date");
            return date.Value;
        }

        private string ReadDescription(CsvReader csv)
        {
            var description = csv.TryRead("Description")?.Trim();
            var category = csv.TryRead("Category")?.Trim();
            var memo = csv.TryRead("Memo")?.Trim();

            if (category != null && description != null)
                return $"{category}: {description}";
            else if (description != null && memo != null)
                return $"{description}: {memo}";
            else
                return description;
        }

        
    }
}
