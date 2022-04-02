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

        public static string AutoCapitalize(this string str)
        {
            str = $" {str.CleanExtraSpaces()} "
                     .ToLower();

            str = Regex.Replace(str, @"(\s)(\w)", m => $" {m.Groups[2].Value.ToUpper()}");
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
    }
}
