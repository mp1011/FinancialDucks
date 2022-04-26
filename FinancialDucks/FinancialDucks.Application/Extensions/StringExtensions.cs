using System.Globalization;
using System.Text.RegularExpressions;

namespace FinancialDucks.Application.Extensions
{
    public static class StringExtensions
    {
        public static string CleanNumbersAndSpecialCharacters(this string str)
        {
            str = Regex.Replace(str, @"\s+", " ");
            str = Regex.Replace(str, "[0-9]", "");
            str = str.Replace(":", "");
            str = str.Replace("#", "");

            return str;
        }

        public static string RemoveNonAlphanumeric(this string str)
        {
            return Regex.Replace(str, @"\W", "");            
        }

        public static string AutoCapitalize(this string str)
        {
            str = $" {str.CleanExtraSpaces()} "
                     .ToLower();

            str = Regex.Replace(str, @"(\s)(\w)", m => $" {m.Groups[2].Value.ToUpper()}");
            return str.Trim();
        }

        public static string AddSpacesAtCapitals(this string str)
        {
            str = Regex.Replace(str, @"([a-z])([A-Z])", m => $"{m.Groups[1].Value} {m.Groups[2].Value}");
            return str.Trim();
        }

        public static string CleanExtraSpaces(this string str)
        {
            return Regex.Replace(str, @"\s+", " ").Trim();
        }

        public static bool IsNonEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static decimal ParseCurrency(this string str)
        {
            decimal d;
            if (decimal.TryParse(str, out d))
                return d;

            if (decimal.TryParse(str, NumberStyles.Currency, null,  out d))
                return d;

            return 0;
        }
    }
}
