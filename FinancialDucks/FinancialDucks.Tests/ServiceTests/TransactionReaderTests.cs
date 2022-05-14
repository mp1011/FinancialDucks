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
        [InlineData($"\\Misc\\Jan2021\\citi 6204.csv", 862.59, "1/10/2021", "PAYMENT THANK YOU",3 )]
        [InlineData($"\\Misc\\Jan2021\\cap.csv", 436.67, "1/10/2021", "Merchandise: GIUNTA'S MEAT FARMS",4)]
        [InlineData($"\\Citibank Checking\\Jan2021\\chk_6903_current_view.csv", 1708.40, "1/8/2021", "Check # 468",1)]
        [InlineData($"\\Misc\\Jan2021\\citi 9536.csv", 102.32, "1/10/2021", "PAYMENT THANK YOU",3)]
        [InlineData($"\\Citibank Savings\\Jan2021\\sav_5539_current_view.csv", -11405.16, "1/5/2021", "Interest Payment",2)]
        [InlineData($"\\Misc\\Jan2021\\tfcu.csv", -2364.59, "1/4/2021", "Electronic Withdrawal: Nationstar dba - Mr Cooper",5)]
        [InlineData($"\\Misc\\April2022\\hsa_optum.csv", 811.35, "3/31/2022", "Interest Payment", 6)]
        [InlineData($"\\Misc\\April2022\\hsabank.csv", 95.25, "4/7/2022", "Payroll Deduction", 6)]
        [InlineData($"\\Misc\\April2022\\ira_roth.csv", 47365.19, "4/1/2022", "Vanguard Value Index Fund (Automatic Purchase)", 7)]
        [InlineData($"\\Misc\\April2022\\ira_trad.csv", 92166.14, "3/31/2022", "VANGUARD HIGH-YIELD CORPORATE ADM (Accrual Dividend Reinvest)", 7)]
        [InlineData($"\\Citibank Savings\\Aug2021\\downloaded.csv",0,"X","X",0)]
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

            Assert.Empty(results.Where(p => p.Amount == 0));
        }
    }
}