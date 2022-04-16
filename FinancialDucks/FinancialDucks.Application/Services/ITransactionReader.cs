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
                    var date = ReadDate(csv);
                    if (date.HasValue)
                    {
                        transactions.Add(new ImportedTransaction(
                            SourceFile: file,
                            Amount: ReadAmount(csv),
                            Date: date.Value,
                            Description: ReadDescription(csv).CleanExtraSpaces(),
                            SourceId: _transactionFileSourceIdentifier.DetectTransactionSource(file).Id));
                    }
                }
            }

            return transactions
                    .Where(t=>t.Amount != 0)
                    .ToArray();
        }

        private decimal ReadAmount(CsvReader csv)
        {
            decimal amount = csv.TryReadDecimal("Trade Amount","Amount");
            if (amount != 0)
                return amount;

            decimal credit = csv.TryReadDecimal("Credit","Amount Credit");
            decimal debit = csv.TryReadDecimal("Debit","Amount Debit");

            return Math.Abs(credit) - Math.Abs(debit);
        }

        private DateTime? ReadDate(CsvReader csv)
        {
            var date = csv.TryReadDate("Date", "Transaction Date", "Requested Date");
            return date;
        }

        private string ReadDescription(CsvReader csv)
        {
            var description = csv.TryRead("Description")?.Trim();
            var category = csv.TryRead("Category")?.Trim();
            var memo = csv.TryRead("Memo")?.Trim();

            //Transaction Type,Shares/Unit
            var transactionType = csv.TryRead("Transaction Type");
            var fundName = csv.TryRead("Fund Name");
            if (fundName != null && transactionType != null)
                return $"{fundName} ({transactionType})";

            if (category != null && description != null)
                return $"{category}: {description}";
            else if (description != null && memo != null)
                return $"{description}: {memo}";
            else
                return description ?? "";
        }

        
    }
}
