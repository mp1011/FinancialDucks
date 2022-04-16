using CsvHelper;

namespace FinancialDucks.Application.Extensions
{
    public static class CsvReaderExtensions
    {
        public static string? TryRead(this CsvReader csv, params string[] possibleColumns)
        {
            var headers = csv.HeaderRecord
                .Select(p => p.Trim())
                .ToArray();

            foreach (var possibleColumn in possibleColumns)
            {
                var col = Array.IndexOf(headers, possibleColumn);
                if (col == -1)
                    continue;

                return csv[col];
            }

            return null;
        }

        public static DateTime? TryReadDate(this CsvReader csv, params string[] possibleColumns)
        {
            var field = csv.TryRead(possibleColumns);
            if (field == null)
                return null;

            DateTime value;
            if (DateTime.TryParse(field, out value))
                return value;

            return null;
        }

        public static decimal TryReadDecimal(this CsvReader csv, params string[] possibleColumns)
        {
            var field = csv.TryRead(possibleColumns);
            if (field == null)
                return 0;

            return field.ParseCurrency();
        }
    }
}
