namespace FinancialDucks.Application.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> NullToEmpty<T>(this IEnumerable<T> list)
        {
            return list ?? new T[] { };
        }

        public static bool NotNullOrEmpty<T>(this IEnumerable<T> e)
        {
            return e != null && e.Any();
        }

        public static int MaxOrDefault(this IEnumerable<int> list, int defaultReturn=0)
        {
            if (list == null || !list.Any())
                return defaultReturn;
            else
                return list.Max();
        }

        public static decimal StandardDeviation(this IEnumerable<decimal> values)
        {
            decimal standardDeviation = 0;
            decimal[] enumerable = values as decimal[] ?? values.ToArray();
            int count = enumerable.Count();
            if (count > 1)
            {
                decimal avg = enumerable.Average();
                decimal sum = enumerable.Sum(d => (d - avg) * (d - avg));
                standardDeviation = (decimal)Math.Sqrt((double)sum / count);
            }
            return standardDeviation;
        }
    }
}
