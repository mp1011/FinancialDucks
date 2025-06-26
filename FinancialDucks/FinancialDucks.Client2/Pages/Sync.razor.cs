using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using FinancialDucks.Client2.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client2.Pages
{
    public partial class Sync 
        : INotificationHandler<WebTransactionExtractorFeature.Notification>, IDisposable
    {
        [Inject]
        public IMediator Mediator { get; set; }

        [Inject]
        public NotificationDispatcher<WebTransactionExtractorFeature.Notification> Dispatcher { get; set; }

        public SyncStatusViewModel[] Status { get; set; }

        protected override void OnInitialized()
        {
            Dispatcher.Register(this, NotificationPriority.Low);
        }

        protected override async Task OnInitializedAsync()
        {
            var result = await Mediator.Send(new SyncFeature.Query());
            Status = result.Select(s => new SyncStatusViewModel(s)).ToArray();
        }

        public async Task ImportTransactions()
        {
            foreach(var status in Status)
            {
                status.ImportMessage = "Importing transactions";
                StateHasChanged();

                try
                {
                    var transactions = await Mediator.Send(new UploadTransactions.Command(status.DownloadedTransactions));
                    status.ImportMessage = $"Imported {transactions.Length} Transactions";

                    await Mediator.Send(new DeleteDownloadedFilesCommand(status.Source));
                    StateHasChanged();
                }
                catch(Exception e)
                {
                    status.ImportMessage = $"Failed to import transactions: {e.Message}";
                    StateHasChanged();
                }
            }
        }

        public async Task FetchTransactions()
        {
            var selectedSources = Status
                .Where(p => p.DoFetch)
                .Select(p => p.Source)
                .ToArray();

            await Mediator.Send(new WebTransactionExtractorFeature.Query(selectedSources));
        }

        async Task INotificationHandler<WebTransactionExtractorFeature.Notification>.Handle(WebTransactionExtractorFeature.Notification notification, CancellationToken cancellationToken)
        {
            var status = Status.FirstOrDefault(p => p.AccountId == notification.Command.SourceId);
            if (status == null)
                return;


            if (status.FetchStatus == FetchStatus.Done || status.FetchStatus == FetchStatus.Failed)
                await OnInitializedAsync();

            status.FetchStatus = notification.FetchStatus;
            if(status.FetchStatus == FetchStatus.Failed)
                status.FetchMessage = $"Step {notification.Command.Sequence} - {notification.Message}";
            else
                status.FetchMessage = $"Step {notification.Command.Sequence}";

            StateHasChanged();
            return;
        }

        public void Dispose()
        {
            Dispatcher.Unregister(this);
        }
    }
}
