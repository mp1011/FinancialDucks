using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components.Statistics
{
    public partial class AccountTimeline
    {
        [Parameter]
        public TimeInterval TimeInterval { get; set; }

        [Parameter]
        public TransactionsFilter Filter { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        public SourceSnapshot[] Snapshots { get; private set; }

        public string Format(object value) => ((double)value).ToString("C");

        protected override async Task OnParametersSetAsync()
        {
            if (Filter == null || !Filter.IsValid(requireCategory:false))
                return;

            Snapshots = await Mediator.Send(
                new AccountBalanceFeature.Query(
                    Sources: Filter.Sources,
                    DateFrom: Filter.RangeStart,
                    DateTo: Filter.RangeEnd,
                    Interval: TimeInterval));
        }
    }
}
