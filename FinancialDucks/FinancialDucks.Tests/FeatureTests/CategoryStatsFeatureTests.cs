using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            var result = await mediator.Send(new CategoryStatsFeature.Query(category, 
                new DateTime(2022, 1, 1) ,
                new DateTime(2022, 12, 1)));

            Assert.Equal(expectedSum, result.Total);
        }

        [Fact]
        public async Task CanGetPercentageStats()
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();

            mockDataHelper.AddMcDonaldsTransations();
            mockDataHelper.AddKrustyBurgerTransactions();
            mockDataHelper.AddUnclassifiedFastFoodTransactions();

            var fastFoodCategory = mockDataHelper.GetMockCategoryTree()
                                        .GetDescendant("Fast-Food")!;

            var mediator = serviceProvider.GetService<IMediator>()!;

            var result = await mediator.Send(new CategoryStatsFeature.QueryWithChildren(fastFoodCategory,
                new DateTime(2022, 1, 1),
                new DateTime(2022, 12, 1)));

            Assert.Equal(3, result.Children.Length);

            Assert.Equal(0.57, result.Children[0].Percent,2);
            Assert.Equal(0.23, result.Children[1].Percent,2);
            Assert.Equal(0.21, result.Children[2].Percent, 2);
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

            var result = await mediator.Send(new CategoryStatsFeature.Query(category,
                new DateTime(2022, 1, 1),
                new DateTime(2022, 12, 1)));
            Assert.Equal(fullTotal, result.Total);

        }

        [Fact]
        public async Task TransfersDoNotCountAsCreditOrDebit()
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();
            var mediator = serviceProvider.GetService<IMediator>()!;

            mockDataHelper.AddMcDonaldsTransations();
            mockDataHelper.AddKrustyBurgerTransactions();
            mockDataHelper.AddTransferTransactions();
            mockDataHelper.AddPaycheckTransactions();

            var allTransactions = mockDataHelper.MockTransations;

            var categoryTree = mockDataHelper.GetMockCategoryTree();


            var debitsStats = await mediator.Send(
                new CategoryStatsFeature.Query(categoryTree.GetDescendant("Debits")!, new DateTime(2022,1,1), new DateTime(2022, 12, 1)));
            var creditsStats = await mediator.Send(
                new CategoryStatsFeature.Query(categoryTree.GetDescendant("Credits")!, new DateTime(2022, 1, 1), new DateTime(2022, 12, 1)));


            var expectedDebits = allTransactions
                    .Where(p => p.Amount < 0 && !p.Description.Contains("Transfer"))
                    .Sum(p => p.Amount);

            var expectedCredits = allTransactions
                   .Where(p => p.Amount > 0 && !p.Description.Contains("Transfer"))
                   .Sum(p => p.Amount);

            Assert.Equal(expectedDebits, debitsStats.Total);
            Assert.Equal(expectedCredits, creditsStats.Total);
        }
    }
}
