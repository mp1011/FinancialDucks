using FinancialDucks.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.ServiceTests
{
    public class TransactionReaderTests : TestBase
    {
        [Theory]
        [InlineData($"\\Bank A\\a1.csv", -2221.97, "7/14/2021", "Test 1",1 )]
        [InlineData($"\\Bank B\\b1.csv", -5205.56, "2/28/2022", "Test 1",2)]
        [InlineData($"\\Credit Card\\c1.csv", 299.73, "7/14/2021", "TESTC: Test 1",3)]
        [InlineData($"\\IRA\\transactions.xls", 543.49, "5/2/2022", "TEST (Automatic Purchase)", 4)]
        public async Task CanParseFile(string source, decimal expectedTotal, string expectedDate, string expectedDescription, int expectedSourceId)
        {
            var serviceProvider = CreateServiceProvider();

            var importService = serviceProvider.GetService<ITransactionReader>();
           
            var results = await importService!.ParseTransactions(GetTestFile(source));

            Assert.NotNull(results);
            var total = results.Sum(p => p.Amount);
            Assert.Equal(expectedTotal, total);

            Assert.Equal(expectedDate, results[0].Date.ToString("M/d/yyyy"));
            Assert.Equal(expectedDescription, results[0].Description);

            Assert.Equal(expectedSourceId, results[0].SourceId);

            Assert.Empty(results.Where(p => p.Amount == 0));
        }
    }
}