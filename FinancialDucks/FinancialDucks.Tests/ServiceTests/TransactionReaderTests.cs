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
        [InlineData($"\\Jan2021\\citi 6204.csv", 862.59, "1/10/2021", "PAYMENT THANK YOU",3 )]
        [InlineData($"\\Jan2021\\cap.csv", 436.67, "1/10/2021", "Merchandise: GIUNTA'S MEAT FARMS",4)]
        [InlineData($"\\Jan2021\\chk_6903_current_view.csv", 1708.40, "1/8/2021", "Check # 468",1)]
        [InlineData($"\\Jan2021\\citi 9536.csv", 102.32, "1/10/2021", "PAYMENT THANK YOU",3)]
        [InlineData($"\\Jan2021\\sav_5539_current_view.csv", -11405.16, "1/5/2021", "Interest Payment",2)]
        [InlineData($"\\Jan2021\\tfcu.csv", -2364.59, "1/4/2021", "Electronic Withdrawal: Nationstar dba - Mr Cooper",5)]
        public async Task CanParseFile(string source, decimal expectedTotal, string expectedDate, string expectedDescription, int expectedSourceId)
        {
            var serviceProvider = CreateServiceProvider();
            var sourceDataFolder = serviceProvider.GetService<ISettingsService>()!.SourcePath;

            var importService = serviceProvider.GetService<ITransactionReader>();
            var results = await importService!.ParseTransactions(new FileInfo($"{sourceDataFolder.FullName}{source}"));

            Assert.NotNull(results);
            var total = results.Sum(p => p.Amount);
            Assert.Equal(expectedTotal, total);

            Assert.Equal(expectedDate, results[0].Date.ToString("M/d/yyyy"));
            Assert.Equal(expectedDescription, results[0].Description);

            Assert.Equal(expectedSourceId, results[0].SourceId);
        }
    }
}