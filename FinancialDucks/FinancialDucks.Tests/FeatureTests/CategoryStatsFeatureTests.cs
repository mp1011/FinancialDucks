﻿using FinancialDucks.Application.Features;
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
                new CategoryStatsFeature.Query(categoryTree.GetDescendant("Debits")!));
            var creditsStats = await mediator.Send(
                new CategoryStatsFeature.Query(categoryTree.GetDescendant("Credits")!));


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
