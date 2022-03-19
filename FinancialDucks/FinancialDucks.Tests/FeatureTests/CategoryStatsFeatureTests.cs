using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class CategoryStatsFeatureTests : TestBase
    {
        [Theory]
        [InlineData("Krusty Burger", 119.88)]
        [InlineData("McDonalds", 47.94)]
        [InlineData("Fast-Food", 333.33)]
        public async Task CanGetStatsForCategory(string categoryName, decimal expectedSum)
        {
            var mediator = _serviceProvider.GetService<IMediator>()!;

            var category = MockDataHelper.GetMockCategoryTree()
                                         .GetDescendant(categoryName)!;

            var result = await mediator.Send(new CategoryStatsFeature.Query(category));
            Assert.Equal(expectedSum, result.Total);
        }
    }
}
