using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    [SupportedOSPlatform("windows")]
    public class BudgetFeatureTests :TestBase
    {

        [Theory]
        [InlineData(100.0, 0.60f)]
        [InlineData(15.0, 1.0f)]

        public async Task CanTrackBudget(decimal budgetAmount, decimal expectedPercentMet)
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();

            mockDataHelper.AddFastFoodTransactionsSinceOneYear();

            var mediator = serviceProvider.GetService<IMediator>()!;

            var category = mockDataHelper.GetMockCategoryTree().GetDescendant("Fast-Food");


            await mediator.Send(new EditBudgetLineCommand(category, budgetAmount));

            var budget = await mediator.Send(new GetBudgetLinesQuery(System.DateTime.Now));

            Assert.Equal((double)expectedPercentMet, budget[0].PercentMet, 2);
        }
    }
}
