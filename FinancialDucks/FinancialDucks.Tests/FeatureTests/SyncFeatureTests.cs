using FinancialDucks.Application.Features;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    [SupportedOSPlatform("windows")]
    public class SyncFeatureTests : TestBase
    {
        [Fact]
        public async Task CanGetSyncStatus()
        {
            var services = CreateServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var testHelper = services.GetRequiredService<MockDataHelper>();

            var sources = testHelper.GetMockTransactionSources();
            foreach (var source in sources)
                CopyTestFiles(source.Name);

            testHelper.AddTransactionsWithSource(sources[0], 1, new DateTime(2021,1,1));
            testHelper.AddTransactionsWithSource(sources[1], 1, new DateTime(2021,2,1));

            var result = await mediator.Send(new SyncFeature.Query());

            Assert.Equal(sources.Length, result.Length);

            Assert.Equal(sources[0].Name, result[0].Account.Name);
            Assert.Equal(sources[1].Name, result[1].Account.Name);
            Assert.Equal(new DateTime(2021, 1, 1), result[0].LastTransactionDate);
            Assert.Equal(new DateTime(2021, 2, 1), result[1].LastTransactionDate);
            Assert.NotEmpty(result[0].DownloadedTransactions);
            Assert.Empty(result[4].DownloadedTransactions);
        }
    }
}
