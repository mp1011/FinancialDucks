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
    }
}
