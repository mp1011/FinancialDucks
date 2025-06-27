using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client2.Pages
{
    public partial class CashFlow : ComponentBase
    {
        [Inject]
        protected IMediator Mediator { get; set; } = default!;

        public TransactionsFilter CurrentFilter { get; set; }

        public TimeInterval TimeInterval { get; set; }


        protected CashFlowReportItem[]? ReportItems;

    
        private async Task LoadReport()
        {
            if(CurrentFilter == null)
               return;

            ReportItems = await Mediator.Send(new CashFlowReportFeature.Query(CurrentFilter, TimeInterval));
            StateHasChanged();
        }
    }
}   