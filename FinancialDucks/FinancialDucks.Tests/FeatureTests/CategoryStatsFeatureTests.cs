using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class CategoryStatsFeatureTests : TestBase
    {
        [Theory]
        [InlineData("Krusty Burger", -119.88)]
        [InlineData("McDonalds", -47.94)]
        [InlineData("Fast-Food", -167.82)]
        public async Task CanGetStatsForCategory(string categoryName, decimal expectedSum)
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();

            mockDataHelper.AddMcDonaldsTransations();
            mockDataHelper.AddKrustyBurgerTransactions();

            var mediator = serviceProvider.GetService<IMediator>()!;

            var category = mockDataHelper.GetMockCategoryTree()
                                         .GetDescendant(categoryName)!;

            var result = await mediator.Send(new CategoryStatsFeature.Query(category));
            Assert.Equal(expectedSum, result.Total);
        }

        [Fact]
        public async Task DebitsCategoryContainsUncategorized()
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();

            mockDataHelper.AddMcDonaldsTransations();
            mockDataHelper.AddKrustyBurgerTransactions();
            mockDataHelper.AddUnclassifiedTransactions();

            var fullTotal = mockDataHelper.MockTransations.Sum(p => p.Amount);

            var mediator = serviceProvider.GetService<IMediator>()!;

            var category = mockDataHelper.GetMockCategoryTree()
                                         .GetDescendant("Debits");

            var result = await mediator.Send(new CategoryStatsFeature.Query(category));
            Assert.Equal(fullTotal, result.Total);

        }
    }
}
