using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    [SupportedOSPlatform("windows")]
    public class ScraperCommandFeatureTests : TestBase
    {

        [Fact]
        public async Task CanSaveCommandInMiddleOfExistingSequence()
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var source = mockDataHelper.GetMockTransactionSources().First();
            mockDataHelper.AddMockScraperCommand(source, 1, ScraperCommandType.FillUsername);
            mockDataHelper.AddMockScraperCommand(source, 2, ScraperCommandType.FillPassword);
            mockDataHelper.AddMockScraperCommand(source, 3, ScraperCommandType.Click, waitForNavigate: true);
            mockDataHelper.AddMockScraperCommand(source, 4, ScraperCommandType.ClickAndDownload, selector: "download");

            await mediator.Send(new ScraperCommandsFeature.SaveCommand(mockDataHelper.CreateMockScraperCommand(source, 2, ScraperCommandType.FillCurrentDate, "", false)));

            var commands = await mediator.Send(new ScraperCommandsFeature.Query(source));
            Assert.Equal(ScraperCommandType.FillCurrentDate, commands[1].TypeId);
            Assert.Equal(2, commands[1].Sequence);

            Assert.Equal(ScraperCommandType.FillPassword, commands[2].TypeId);
            Assert.Equal(3, commands[2].Sequence);
        }
    }
}
