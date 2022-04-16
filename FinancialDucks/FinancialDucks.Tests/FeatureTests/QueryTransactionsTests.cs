using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class QueryTransactionsTests : TestBase
    {
        [Theory]
        [InlineData("Citibank Card")]
        [InlineData("Capital One Card")]
        [InlineData("Citibank Card,Capital One Card")]
        public async Task CanQueryTransactionsBySource(string sourcesCSV)
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();

            var tree = await serviceProvider.GetRequiredService<ICategoryTreeProvider>().GetCategoryTree();
          
            List<ITransactionSource> selectedSources = new List<ITransactionSource>();
            var sourceNames = sourcesCSV.Split(',');
            int expectedCount = 0;

            foreach(var src in mockDataHelper.GetMockTransactionSources())
            {
                int count = src.Id + 10;
                if(sourceNames.Contains(src.Name))
                {
                    expectedCount += count;
                    selectedSources.Add(src);
                }
                mockDataHelper.AddTransactionsWithSource(src, src.Id+10);
            }

            Assert.NotEmpty(selectedSources);
            Assert.NotEqual(0, expectedCount);

            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var result = await mediator.Send(new TransactionsFeature.QueryTransactions(
                new TransactionsFeature.TransactionsFilter(
                   RangeStart: new DateTime(2022, 1, 1),
                   RangeEnd: new DateTime(2024, 1, 1),
                   Category: tree,
                   TextFilter: null,
                   Sources: selectedSources.ToArray(),
                   SortColumn: TransactionSortColumn.Amount,
                   SortDirection: SortDirection.Ascending), 0,1000));

            Assert.Equal(expectedCount, result.Count());
        }


        [Theory]
        [InlineData("All")]
        [InlineData("Krusty Burger")]
        [InlineData("Fast-Food")]
        [InlineData("Paychecks")]
        [InlineData("Debits")]
        public async Task CanQueryTransactionsByCategory(string categoryName)
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();

            var krustyBurgerTransactions = mockDataHelper.AddKrustyBurgerTransactions();
            var mcDonaldsTransactions = mockDataHelper.AddMcDonaldsTransations();
            var unclassifiedTransactions = mockDataHelper.AddUnclassifiedTransactions();
            var paycheckTransactions = mockDataHelper.AddPaycheckTransactions();
            var transferTransactions = mockDataHelper.AddTransferTransactions();

            var tree = await serviceProvider.GetRequiredService<ICategoryTreeProvider>().GetCategoryTree();
            var category = tree.GetDescendant(categoryName);
          
            var mediator = serviceProvider.GetService<IMediator>();
            var results = await mediator!.Send(new TransactionsFeature.QueryTransactions(
                new TransactionsFeature.TransactionsFilter(
                    RangeStart: new DateTime(2022,1,1),
                    RangeEnd: new DateTime(2022,4,1),
                    Category: category,
                    TextFilter:null,
                    Sources: new ITransactionSource[] { },
                    SortColumn: TransactionSortColumn.Date,
                    SortDirection: SortDirection.Ascending),
                0,
                100))!;

            IEnumerable<ITransactionDetail> expectedTransactions = new ITransactionDetail[] { };
            switch(categoryName)
            {
                case "All":
                    expectedTransactions = krustyBurgerTransactions
                        .Union(mcDonaldsTransactions)
                        .Union(unclassifiedTransactions)
                        .Union(paycheckTransactions);
                    break;
                case "Krusty Burger":
                    expectedTransactions = krustyBurgerTransactions;
                    break;
                case "Fast-Food":
                    expectedTransactions = krustyBurgerTransactions
                        .Union(mcDonaldsTransactions);
                    break;
                case "Paychecks":
                    expectedTransactions = paycheckTransactions;
                    break;
                case "Debits":
                    expectedTransactions = krustyBurgerTransactions
                       .Union(mcDonaldsTransactions)
                       .Union(unclassifiedTransactions);
                    break;
            }

            var expectedTotal = expectedTransactions.Sum(p => p.Amount);
            Assert.Equal(expectedTotal, results.Sum(p => p.Amount));
        }

        [Theory]
        [InlineData("2022/1/1","2022/2/1",0,10,7)]
        [InlineData("2022/1/1", "2022/2/1", 0, 5, 5)]
        [InlineData("2022/1/1", "2022/2/1", 1, 5, 2)]
        [InlineData("2022/1/1", "2022/2/1", 2, 5, 0)]
        [InlineData("2021/1/1", "2021/2/1", 2, 5, 0)]
        [InlineData("2023/1/1", "2023/2/1", 2, 5, 0)]
        [InlineData("2022/1/1", "2022/2/25", 0, 12, 12)]
        [InlineData("2022/1/2", "2022/2/25", 0, 12, 11)]
        [InlineData("2022/1/2", "2022/2/24", 0, 12, 10)]
        public async Task CanQueryTransactions(string dateStartStr, string dateEndStr, int page, int resultsPerPage, int expectedResults)
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();

            mockDataHelper.AddKrustyBurgerTransactions();
            var dateStart = DateTime.Parse(dateStartStr);
            var dateEnd = DateTime.Parse(dateEndStr);

            var mediator = serviceProvider.GetService<IMediator>();
            var results = await mediator!.Send(new TransactionsFeature.QueryTransactions(
                new TransactionsFeature.TransactionsFilter(
                    dateStart,
                    dateEnd,
                    Category:null,
                    TextFilter:null,
                    Sources: new ITransactionSource[] { },
                    SortColumn: TransactionSortColumn.Date,
                    SortDirection:SortDirection.Ascending),
                page,
                resultsPerPage))!;

            if (expectedResults > 0)
            {
                Assert.Equal(expectedResults, results.Length);
                Assert.All(results, r => Assert.InRange(r.Date, dateStart, dateEnd));
                Assert.InRange(results.Length,0, resultsPerPage);
                Assert.True(results.First().Date < results.Last().Date);    
            }
            else
                Assert.Empty(results);


        }

        [Theory]
        [InlineData(TransactionSortColumn.Amount, SortDirection.Ascending)]
        [InlineData(TransactionSortColumn.Amount, SortDirection.Descending)]
        [InlineData(TransactionSortColumn.Date, SortDirection.Ascending)]
        [InlineData(TransactionSortColumn.Date, SortDirection.Descending)]

        public async Task TransactionsCanBeSorted(TransactionSortColumn column, SortDirection sortDirection)
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();

            mockDataHelper.AddKrustyBurgerTransactions();
            mockDataHelper.AddMcDonaldsTransations();

            var dateStart = new DateTime(2022, 1, 1);
            var dateEnd = new DateTime(2022, 4, 1);

            var mediator = serviceProvider.GetService<IMediator>();
            var results = await mediator!.Send(new TransactionsFeature.QueryTransactions(
                new TransactionsFeature.TransactionsFilter(
                    dateStart,
                    dateEnd,
                    null,
                    null,
                    new ITransactionSource[] { },
                    column,
                    sortDirection),
                0,
                100))!;

            if(column == TransactionSortColumn.Date && sortDirection == SortDirection.Ascending)
                Assert.True(results[0].Date < results.Last().Date);
            else if (column == TransactionSortColumn.Date && sortDirection == SortDirection.Descending)
                Assert.True(results[0].Date > results.Last().Date);
            else if (column == TransactionSortColumn.Amount && sortDirection == SortDirection.Ascending)
                Assert.True(results[0].Amount < results.Last().Amount);
            else if (column == TransactionSortColumn.Amount && sortDirection == SortDirection.Descending)
                Assert.True(results[0].Amount > results.Last().Amount);
        }

        [Fact]
        public async Task MultipleRulesDoesNotGiveDuplicateQueryResults()
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();

            mockDataHelper.AddKrustyBurgerTransactions();
            var dateStart = new DateTime(2022, 1, 1);
            var dateEnd = new DateTime(2022, 4, 1);

            var tree = await serviceProvider.GetRequiredService<ICategoryTreeProvider>().GetCategoryTree();

            var category = tree.GetDescendant("Krusty Burger");
            var mediator = serviceProvider.GetService<IMediator>();
            var results = await mediator!.Send(new TransactionsFeature.QueryTransactions(
                new TransactionsFeature.TransactionsFilter(
                    dateStart,
                    dateEnd,
                    category,
                    null,
                    new ITransactionSource[] { },
                    TransactionSortColumn.Date,
                    SortDirection.Ascending),
                1,
                100))!;

            mockDataHelper.MockCategoryRules.Add(
                new CategoryRule(999, Category: category, SubstringMatch: "Krusty", Priority:0));

            var results2 = await mediator!.Send(new TransactionsFeature.QueryTransactions(
                new TransactionsFeature.TransactionsFilter(
                    dateStart,
                    dateEnd,
                    category,
                    null,
                    new ITransactionSource[] { },
                    TransactionSortColumn.Date,
                    SortDirection.Ascending),
                1,
                100))!;

            Assert.Equal(results.Sum(p => p.Amount), results2.Sum(p => p.Amount));

        }
    }
}
