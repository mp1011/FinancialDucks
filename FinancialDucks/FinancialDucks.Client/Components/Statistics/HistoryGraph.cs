using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components.Statistics
{
    public partial class HistoryGraph
    {       
        [Inject]
        public IMediator Mediator { get; set; }

        [Parameter]
        public TransactionsFilter Filter { get; set; }

        [Parameter]
        public TimeInterval TimeInterval { get; set; } = TimeInterval.Monthly;

        public CategoryTimeSlice[] TimeSlices { get; set; }

        public string Format(object value) => ((double)value).ToString("C");

        protected override async Task OnParametersSetAsync()
        { 
            if(Filter == null) return;

            TimeSlices = await Mediator.Send(
                new HistoryGraphFeature.Query(Filter, TimeInterval));
        }

        public async Task UpdateTimeInterval(TimeInterval newInterval)
        {
            TimeInterval = newInterval;
            await OnParametersSetAsync();
        }
    }
}
