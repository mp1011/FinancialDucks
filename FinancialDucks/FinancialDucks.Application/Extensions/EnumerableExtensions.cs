namespace FinancialDucks.Application.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool NotNullOrEmpty<T>(this IEnumerable<T> e)
        {
            return e != null && e.Any();
        }
    }
}
