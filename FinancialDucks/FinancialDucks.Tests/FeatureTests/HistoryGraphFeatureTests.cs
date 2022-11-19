using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class HistoryGraphFeatureTests :TestBase
    {
        [Theory]
        [InlineData("Krusty Burger", TimeInterval.Daily, "2022/1/1","2022/6/1")]
        [InlineData("Krusty Burger", TimeInterval.Weekly, "2022/1/2", "2022/6/1")]
        [InlineData("Krusty Burger", TimeInterval.Monthly, "2022/1/1", "2022/6/1")]
        [InlineData("Krusty Burger", TimeInterval.Quarterly, "2022/1/1", "2022/6/1")]
        [InlineData("Krusty Burger", TimeInterval.Annual, "2022/1/1", "2028/6/1")]
        [InlineData("Fast-Food", TimeInterval.Daily, "2022/1/1", "2022/6/1")]
        [InlineData("Fast-Food", TimeInterval.Weekly, "2022/1/2", "2022/6/1")]
        [InlineData("Fast-Food", TimeInterval.Monthly, "2022/1/1", "2022/6/1")]
        [InlineData("Fast-Food", TimeInterval.Quarterly, "2022/1/1", "2022/6/1")]
        [InlineData("Fast-Food", TimeInterval.Annual, "2022/1/1", "2028/6/1")]
        public async Task CanGetTransactionsOverTime(string categoryName, TimeInterval interval, string dateFrom, string dateTo)
        {
            var serviceProvider = CreateServiceProvider();

            var dataHelper = serviceProvider.GetRequiredService<MockDataHelper>();
            dataHelper.AddTransactionsForLongTerm();

            var categoryTree = await serviceProvider.GetRequiredService<ICategoryTreeProvider>().GetCategoryTree();

            var category = categoryTree.GetDescendant(categoryName)!;
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var result = await mediator.Send(new HistoryGraphFeature.Query(
                new TransactionsFilter(DateTime.Parse(dateFrom), DateTime.Parse(dateTo), category),
                interval,true));

            var firstResult = result.First();
            var lastResult = result.TakeWhile(x => x.Amount != 0).Last();

            Assert.True(result.TakeWhile(p=>p.Amount != 0).Count() >= result.Length-5);
            Assert.Equal(DateTime.Parse(dateFrom), firstResult.SliceStart);
            Assert.Equal(DateTime.Parse(dateFrom).Add(interval).AddDays(-1).EndOfDay(), firstResult.SliceEnd);
        }
    }
}
