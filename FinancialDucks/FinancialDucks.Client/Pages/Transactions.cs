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

        public string SelectedText { get; set; }
        public ITransactionDetail SelectedTransaction { get; set; }

        public TransactionsFilter CurrentFilter { get; private set; }



        public async Task OnTransactionMouseUp(TransactionMouseUpEventArgs args)
        {
            if (args.MouseArgs.Button != (int)MouseButton.Right)
                return;

            SelectedText = await JSRuntime.InvokeAsync<string>("getSelectedText");
            SelectedText = SelectedText.Trim();
            SelectedTransaction = args.Transaction;
            await JSRuntime.ShowModal(nameof(CategoryQuickAdd));            
        }

        public async Task ShowTransactionImportDialog()
        {
            ImportRequestTime = DateTime.Now;
            await JSRuntime.ShowModal(nameof(TransactionImportDialog));
        }
    }
}
