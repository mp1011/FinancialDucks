using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Client.Components;
using FinancialDucks.Client.Helpers;
using FinancialDucks.Client.Models;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FinancialDucks.Client.Pages
{
    public partial class Transactions 
    {

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        public DateTime? ImportRequestTime { get; set; } = null;

        public TransactionsFeature.TransactionsFilter CurrentFilter { get; private set; }
       
        public DateTime RangeStart { get; set; } = DateTime.Now.AddMonths(-6);

        public DateTime RangeEnd { get; set; } = DateTime.Now;

        public ITransactionSource[] Sources { get; set; }

        public string TextFilter { get; set; }

        public ICategoryDetail? CategoryFilter { get; set; }

        public TransactionSortColumn SortColumn { get; set; }

        public SortDirection SortDirection { get; set; }    

        public string SelectedText { get; set; }
        public ITransactionDetail SelectedTransaction { get; set; }


       

        public void OnCategorySelected(ICategoryDetail category)
        {
            CategoryFilter = category;
            UpdateCurrentFilter();
        }

        private void UpdateCurrentFilter(bool forceUpdate=false)
        {
            if (forceUpdate
                || CurrentFilter == null
                || CurrentFilter.RangeStart != RangeStart
                || CurrentFilter.RangeEnd != RangeEnd
                || CurrentFilter.TextFilter != TextFilter
                || CurrentFilter.Category != CategoryFilter
                || CurrentFilter.SortDirection != SortDirection
                || CurrentFilter.SortColumn != SortColumn)
            {
                CurrentFilter = new TransactionsFeature.TransactionsFilter(
                    RangeStart, RangeEnd, CategoryFilter, TextFilter, Sources, SortColumn,SortDirection);
                StateHasChanged();
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            UpdateCurrentFilter();
        }

        public async Task OnSourcesChanged(ITransactionSourceDetail[] selectedSources)
        {
            Sources = selectedSources;
            UpdateCurrentFilter(forceUpdate: true);
        }

        public async Task OnTransactionMouseUp(TransactionMouseUpEventArgs args)
        {
            if (args.MouseArgs.Button != (int)MouseButton.Right)
                return;

            SelectedText = await JSRuntime.InvokeAsync<string>("getSelectedText");
            SelectedText = SelectedText.Trim();
            SelectedTransaction = args.Transaction;
            await JSRuntime.ShowModal(nameof(CategoryQuickAdd));            
        }

        public void AfterCategoryQuickAdd(ICategoryDetail category)
        {
            UpdateCurrentFilter(forceUpdate:true);
        }

        public async Task ShowTransactionImportDialog()
        {
            ImportRequestTime = DateTime.Now;
            await JSRuntime.ShowModal(nameof(TransactionImportDialog));
        }
    }
}
