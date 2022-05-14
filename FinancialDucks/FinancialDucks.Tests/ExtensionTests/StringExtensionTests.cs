using FinancialDucks.Application.Extensions;
using Xunit;

namespace FinancialDucks.Tests.ExtensionTests
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData("   this   is  a     test","this is a test")]
        [InlineData("   this\t\tis \n  a \r    test   ", "this is a test")]

        public void TestTrimSpaces(string text, string expected)
        {
            Assert.Equal(expected, text.CleanExtraSpaces());
        }

        [Theory]
        [InlineData("HELLOWORLD","Helloworld")]
        [InlineData(" HELLO   WORLD", "Hello World")]
        public void TestAutoCapitalize(string text, string expected)
        {
            Assert.Equal(expected, text.AutoCapitalize());
        }

        [Theory]
        [InlineData("HelloWorld", "Hello World")]
        public void TestAddSpacesAtCapitals(string text, string expected)
        {
            Assert.Equal(expected, text.AddSpacesAtCapitals());
        }



        [Theory]
        [InlineData("12.45", 12.45)]
        [InlineData("(12.45)", -12.45)]
        [InlineData("$12.45", 12.45)]
        [InlineData("($214.24)", -214.24)]

        public void TestParseCurrency(string str, decimal expected)
        {
            Assert.Equal(expected, str.ParseCurrency());
        }

        [Theory]
        [InlineData("Floop the Pig","Floop*",true)]
        [InlineData("Floop the Pig", "Flop*", false)]
        [InlineData("Floop the Pig", "*the*", true)]
        [InlineData("Floop the Pig", "Flo*the*ig", true)]
        [InlineData("Floop the Pig", "Flop*the*ig", false)]
        [InlineData("Floop the Pig", "Floop", false)]
        [InlineData("Floop the Pig", "Pig", false)]
        [InlineData("Floop the Pig", "Floop the PIG", true)]
        public void TestWildcardMatch(string str, string pattern, bool isMatch)
        {
            Assert.Equal(isMatch, str.WildcardMatch(pattern));
        }
    }
}
