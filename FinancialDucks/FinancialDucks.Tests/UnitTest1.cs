using FinancialDucks.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests
{
    public class TransactionReaderTests
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionReaderTests()
        {
            var host = Host.CreateDefaultBuilder()
               .ConfigureServices(sc =>
               {

               })
               .Build();

            _serviceProvider = host.Services;
        }

        [Theory]
        [InlineData($"\\Jan2021\\citi 6204.csv", -869.59)]
        public async Task CanParseFile(string source, decimal expectedTotal)
        {
            var sourceDataFolder = _serviceProvider.GetService<ISettingsService>()!.SourcePath;

            var importService = _serviceProvider.GetService<ITransactionReader>();
            var results = await importService!.ParseTransactions(new FileInfo($"{sourceDataFolder.FullName}{source}"));

            Assert.NotNull(results);
            var total = results.Sum(p => p.Amount);
            Assert.Equal(expectedTotal, total);
        }
    }
}