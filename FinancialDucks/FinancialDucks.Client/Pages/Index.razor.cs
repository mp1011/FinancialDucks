using FinancialDucks.Application.Features;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Pages
{
    public partial class Index
    {
        [Inject]
        public IMediator Mediator { get; set; }

        public DateTime? LastTransactionDate { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var syncStatus = await Mediator.Send(new SyncFeature.Query());
            LastTransactionDate = syncStatus.Max(p => p.LastTransactionDate);
        }
    }
}
