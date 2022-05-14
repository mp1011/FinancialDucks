using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class WebTransactionExtractorFeatureTests : TestBase, INotificationHandler<WebTransactionExtractorFeature.Notification>
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

            var result = await mediator.Send(new WebTransactionExtractorFeature.Query(new Application.Models.ITransactionSource[] { source }));
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Contains(source.Name, result[0].FullName);
        }

        [Fact]
        public async Task TransactionWebExtractRaisesNotifications()
        {
            var serviceProvider = CreateServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var dispatcher = serviceProvider.GetRequiredService<NotificationDispatcher<WebTransactionExtractorFeature.Notification>>();
            dispatcher.Register(this, NotificationPriority.High);
            try
            {

                var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();
                var source = mockDataHelper.GetMockTransactionSources().First();
                mockDataHelper.AddMockScraperCommand(source, 1, ScraperCommandType.FillUsername);
                mockDataHelper.AddMockScraperCommand(source, 2, ScraperCommandType.FillPassword);
                mockDataHelper.AddMockScraperCommand(source, 3, ScraperCommandType.Click, waitForNavigate: true);

                _notifications.Clear();
                var result = await mediator.Send(new WebTransactionExtractorFeature.Query( new ITransactionSource[] {source}));

                Assert.NotEmpty(_notifications);

            }
            finally
            {
                dispatcher.Unregister(this);
            }
        }

        private List<WebTransactionExtractorFeature.Notification> _notifications = new List<WebTransactionExtractorFeature.Notification>();
        Task INotificationHandler<WebTransactionExtractorFeature.Notification>.Handle(WebTransactionExtractorFeature.Notification notification, CancellationToken cancellationToken)
        {
            _notifications.Add(notification);
            return Task.CompletedTask;
        }
    }
}
