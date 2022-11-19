using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Pages
{
    public partial class Compare
    {
        private DateTime _lastCompareDate = DateTime.MinValue;
        private TransactionsFilter _lastFilter = null;

        [Inject]
        public IMediator Mediator { get; set; }

        public bool Loading { get; set; }

        public DateTime CompareDateStart { get; set; } = DateTime.Now.AddMonths(-18);

        public TransactionsFilter CurrentFilter { get; set; }

        public ICategory Category => CurrentFilter?.Category;

        public TransactionComparison[] Comparisons { get; set; } = new TransactionComparison[] { };

        public DateTime BeforeDate
        {
            get
            {
                if (CurrentFilter != null && CurrentFilter.RangeStart < CompareDateStart)
                    return CurrentFilter.RangeStart;
                else
                    return CompareDateStart;
            }
        }

        public DateTime AfterDate
        {
            get
            {
                if (CurrentFilter != null && CurrentFilter.RangeStart > CompareDateStart)
                    return CurrentFilter.RangeStart;
                else
                    return CompareDateStart;
            }
        }

        public void OnCategoryChanged(ICategoryDetail category)
        {
            CurrentFilter = CurrentFilter.ChangeCategory(category);
        }
         
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (CurrentFilter == null || CurrentFilter.Category==null)
                return;

            if(CurrentFilter.Equals(_lastFilter) && CompareDateStart == _lastCompareDate)
                return;

            _lastFilter = CurrentFilter.Copy();
            _lastCompareDate = CompareDateStart;

            Loading = true;
            Comparisons = await Mediator.Send(new ComparisonFeatureQuery(CurrentFilter, CompareDateStart));
            Loading = false;
            StateHasChanged();
        }

    }
}
