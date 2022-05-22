using FinancialDucks.Application.Features;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    [SupportedOSPlatform("windows")]
    public class ImportFeatureTests : TestBase 
    {

        [Fact]
        public async Task CanImportAllTransactionsInFolder()
        {
            var serviceProvider = CreateServiceProvider();
            var mediator = serviceProvider.GetService<IMediator>();
            
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();
            foreach (var src in mockDataHelper.GetMockTransactionSources())
            {
                CopyTestFiles(src.Name);
            }

            var result = await mediator!.Send(new ReadLocalTransactions.Request());
            Assert.NotEqual(0, result.Sum(p => p.Amount));
        }
    }
}
