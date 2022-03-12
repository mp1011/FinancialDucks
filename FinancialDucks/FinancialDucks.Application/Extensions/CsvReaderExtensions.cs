using CsvHelper;

namespace FinancialDucks.Application.Extensions
{
    public static class CsvReaderExtensions
    {
        public static string? TryRead(this CsvReader csv, params string[] possibleColumns)
        {
            foreach (var possibleColumn in possibleColumns)
            {
                var col = Array.IndexOf(csv.HeaderRecord, possibleColumn);
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
          
            decimal value;
            if (decimal.TryParse(field, out value))
                return value;
       
            return 0;
        }
    }
}
