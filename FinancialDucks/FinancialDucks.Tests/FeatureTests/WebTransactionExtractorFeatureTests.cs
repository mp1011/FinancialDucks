using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class WebTransactionExtractorFeatureTests : TestBase
    {
        [Fact]
        public async Task CanExtractTransactionsFilesFromWeb()
        {
            var serviceProvider = CreateServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();
            var source = mockDataHelper.GetMockTransactionSources().First();
            mockDataHelper.AddMockScraperCommand(source, 1, ScraperCommandType.FillUsername);
            mockDataHelper.AddMockScraperCommand(source, 2, ScraperCommandType.FillPassword);
            mockDataHelper.AddMockScraperCommand(source, 3, ScraperCommandType.Click, waitForNavigate:true);
            mockDataHelper.AddMockScraperCommand(source, 4, ScraperCommandType.ClickAndDownload, selector:"download");

            var result = await mediator.Send(new WebTransactionExtractorFeature.Query());
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
    }
}
