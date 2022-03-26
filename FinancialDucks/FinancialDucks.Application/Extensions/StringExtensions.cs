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
    }
}
