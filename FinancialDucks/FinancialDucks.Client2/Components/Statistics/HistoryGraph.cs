using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Client2.Models;
using MediatR;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace FinancialDucks.Client2.Components.Statistics
{
    public partial class HistoryGraph
    {       
        [Inject]
        public IMediator Mediator { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public TransactionsFilter Filter { get; set; }

        [Parameter]
        public TimeInterval TimeInterval { get; set; } = TimeInterval.Monthly;

        [Parameter]
        public bool DistributeOverGaps { get; set; }

        public LabelledData<CategoryTimeSlice>[] TimeSlices { get; set; }

        public string Format(object value) => ((double)value).ToString("C");

        protected override async Task OnParametersSetAsync()
        { 
            if(Filter == null) return;

            var timeSlices = await Mediator.Send(
                new HistoryGraphFeature.Query(Filter, TimeInterval, DistributeOverGaps));

            TimeSlices = timeSlices
                .Select((x, i) => new LabelledData<CategoryTimeSlice>(
                    label: x.SliceStart.GetLabel(x.TimeInterval),
                    value: x.Amount.Abs(),
                    data: x))
                .ToArray();
        }

        public async Task UpdateTimeInterval(TimeInterval newInterval)
        {
            TimeInterval = newInterval;
            await OnParametersSetAsync();
        }

        public void SeriesClicked(SeriesClickEventArgs arg)
        {
            var timeSlice = arg.Data as LabelledData<CategoryTimeSlice>;
            if(timeSlice == null) return;

            var uri = $@"/transactions?dateFrom={timeSlice.Data.SliceStart.ToShortDateString()}&dateTo={timeSlice.Data.SliceEnd.ToShortDateString()}&category={Filter.Category.Name}";
            NavigationManager.NavigateTo(uri, forceLoad: true);
        }
    }
}
