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
        public ICategory Category { get; set; }
     
        [Parameter]
        public DateTime RangeStart { get; set; } = DateTime.Now.AddYears(-3);

        [Parameter]
        public DateTime RangeEnd { get; set; } = DateTime.Now;

        public TimeInterval TimeInterval { get; set; } = TimeInterval.Monthly;

        public CategoryTimeSlice[] TimeSlices { get; set; }

        public string Format(object value) => ((double)value).ToString("C");

        protected override async Task OnParametersSetAsync()
        { 
            if(Category == null) return;

            TimeSlices = await Mediator.Send(
                new HistoryGraphFeature.Query(Category, TimeInterval, RangeStart, RangeEnd));
        }

        public async Task UpdateTimeInterval(TimeInterval newInterval)
        {
            TimeInterval = newInterval;
            await OnParametersSetAsync();
        }
    }
}
