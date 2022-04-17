using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components
{
    public partial class TransactionsFilterToolbar
    {
        [Parameter]
        public TransactionSortColumn SortColumn { get; set; }

        [Parameter]
        public SortDirection SortDirection { get; set; }

        [Parameter]
        public EventCallback<TransactionsFilter> OnFilterChanged { get; set; }

        public TransactionsFilter CurrentFilter { get; private set; }

        public DateTime RangeStart { get; set; } = DateTime.Now.AddMonths(-6);

        public DateTime RangeEnd { get; set; } = DateTime.Now;

        public ITransactionSource[] Sources { get; set; }

        public string TextFilter { get; set; }

        public ICategoryDetail? CategoryFilter { get; set; }

        public async Task OnCategorySelected(ICategoryDetail category)
        {
            CategoryFilter = category;
            await UpdateCurrentFilter();
        }

        private async Task UpdateCurrentFilter(bool forceUpdate = false)
        {
            if (forceUpdate
                || CurrentFilter == null
                || CurrentFilter.RangeStart != RangeStart
                || CurrentFilter.RangeEnd != RangeEnd
                || CurrentFilter.TextFilter != TextFilter
                || CurrentFilter.Category != CategoryFilter)
            {
                CurrentFilter = new TransactionsFilter(
                    RangeStart, RangeEnd, CategoryFilter, Sources,TextFilter);
                StateHasChanged();

                await OnFilterChanged.InvokeAsync(CurrentFilter);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await UpdateCurrentFilter();
        }

        public async Task OnSourcesChanged(ITransactionSourceDetail[] selectedSources)
        {
            Sources = selectedSources;
            await UpdateCurrentFilter(forceUpdate: true);
        }
    }
}
