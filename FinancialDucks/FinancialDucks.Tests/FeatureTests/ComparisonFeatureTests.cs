using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class ComparisonFeatureTests : TestBase
    {
        [Fact]
        public async Task CanCompareTransactions()
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var transactions = mockDataHelper.AddTransactionsForLongTerm()
                    .OrderBy(p=>p.Date)
                    .ToArray();

            var dateStart = transactions[0].Date;
            var dateEnd = transactions.Last().Date;

            var category = mockDataHelper.GetMockCategoryTree().GetDescendant(SpecialCategory.Debits.ToString());
               
            var result = await mediator.Send(new ComparisonFeatureQuery(
                new TransactionsFilter(
                    RangeStart: dateStart.AddYears(1),
                    RangeEnd: dateStart.AddYears(1).AddMonths(2),
                    Category: category,
                    TextFilter: null),
                CompareDateStart: dateStart));

            Assert.NotEmpty(result);
        }
    }
}
