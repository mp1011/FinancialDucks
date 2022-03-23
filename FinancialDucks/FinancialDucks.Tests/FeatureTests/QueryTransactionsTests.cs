using FinancialDucks.Application.Features;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class QueryTransactionsTests : TestBase
    {
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
            var results = await mediator!.Send(new QueryTransactions(
                dateStart,
                dateEnd,
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
    }
}
