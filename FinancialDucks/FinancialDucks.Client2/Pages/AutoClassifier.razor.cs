using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Components;
using static FinancialDucks.Application.Features.AutoClassifierFeature;

namespace FinancialDucks.Client2.Pages
{
    public partial class AutoClassifier : INotificationHandler<AutoClassifierFeature.AutoClassifyNotification>, IDisposable
    {
        [Inject]
        public IMediator Mediator { get; set; }

        [Inject]
        public NotificationDispatcher<AutoClassifierFeature.AutoClassifyNotification> Dispatcher { get; set; }

        protected List<AutoClassificationResult> Results = new();

        protected override async Task OnInitializedAsync()
        {
            Dispatcher.Register(this, NotificationPriority.Low);
            await Mediator.Send(new AutoClassifyTransactionsQuery());
        }

        public Task Handle(AutoClassifierFeature.AutoClassifyNotification notification, CancellationToken cancellationToken)
        {
            Results.Add(notification.Result);
            StateHasChanged();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispatcher.Unregister(this);
        }
    }
}
