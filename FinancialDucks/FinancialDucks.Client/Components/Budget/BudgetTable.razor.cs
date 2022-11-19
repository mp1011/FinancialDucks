using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components.Budget
{
    public partial class BudgetTable
    {
        [Inject]
        public IMediator Mediator { get; set; }

        public DateTime Month { get; set; }

        public BudgetLineDetail[] BudgetLines { get; set; } = Array.Empty<BudgetLineDetail>();

        protected override void OnInitialized()
        {
            if (Month.Year == 1)
                Month = DateTime.Now;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            BudgetLines = await Mediator.Send(new GetBudgetLinesQuery(Month));
        }
    }
}
