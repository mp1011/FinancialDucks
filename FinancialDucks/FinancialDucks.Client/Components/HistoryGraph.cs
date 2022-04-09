using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components
{
    public partial class HistoryGraph
    {       
        [Inject]
        public IMediator Mediator { get; set; }

        [Parameter]
        public ICategory Category { get; set; }

        public TimeInterval TimeInterval { get; set; } = TimeInterval.Monthly;

        public DateTime RangeStart { get; set; } = DateTime.Now.AddYears(-3);

        public DateTime RangeEnd { get; set; } = DateTime.Now;

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
