using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components
{
    public partial class CategoryTreeNode
    {
        public string NewCategoryText { get; set; }
        public int? NumTransactions { get; private set; }

        public decimal? DollarTotal { get; private set; }

        [Parameter]
        public ICategoryDetail Category { get; set; }

        [Parameter]
        public EventCallback<CategoriesFeature.AddCategoryCommand> NewButtonClick { get; set; }

        [Parameter]
        public EventCallback<ICategoryDetail> CategorySelected { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (Category == null)
                return;

            var stats = await Mediator.Send(new CategoryStatsFeature.Query(Category));
            NumTransactions = stats.TransactionCount;
            DollarTotal = stats.Total;
            StateHasChanged();
        }
    }
}
