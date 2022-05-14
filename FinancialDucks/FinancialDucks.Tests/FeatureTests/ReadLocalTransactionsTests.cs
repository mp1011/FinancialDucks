using FinancialDucks.Application.Features;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    [SupportedOSPlatform("windows")]
    public class ReadLocalTransactionsTests : TestBase
    {
        [Theory]
        [InlineData("Citibank Checking",367)]
        [InlineData("Citibank Savings", 45)]
        [InlineData("Bank A", 0)]
        public async Task CanReadLocalTransactions(string sourceName, int expectedTransactions)
        {
            var services = CreateServiceProvider();
            var mediator = services.GetRequiredService<IMediator>();
            var testHelper = services.GetRequiredService<MockDataHelper>();

            var source = testHelper.GetMockTransactionSources().Single(p => p.Name == sourceName);
            var result = await mediator.Send(new ReadLocalTransactions.Request(source));
            Assert.Equal(expectedTransactions, result.Length);
        }

    }
}
