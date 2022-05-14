using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
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

        [Inject]
        public ICategoryTreeProvider CategoryTreeProvider { get; set; }


        public DateTime? ImportRequestTime { get; set; } = null;

        public string SelectedText { get; set; }
        public ITransactionDetail SelectedTransaction { get; set; }

        public TransactionsFilter CurrentFilter { get; private set; }

        [Parameter]
        [SupplyParameterFromQuery(Name = "dateFrom")]
        public DateTime? QueryDateFrom { get; set; }

        [Parameter]
        [SupplyParameterFromQuery(Name = "dateTo")]
        public DateTime? QueryDateTo{ get; set; }

        [Parameter]
        [SupplyParameterFromQuery(Name = "category")]
        public string? QueryCategoryName { get; set; }


        protected override async Task OnParametersSetAsync()
        {
            if (!QueryDateFrom.HasValue || !QueryDateTo.HasValue || QueryCategoryName == null)
                return;

            var root = await CategoryTreeProvider.GetCategoryTree();
            var category = root.GetDescendant(QueryCategoryName);
            CurrentFilter = new TransactionsFilter(QueryDateFrom.Value, QueryDateTo.Value, category);
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
    }
}
