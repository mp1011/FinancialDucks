using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Components;
using System.Text;

namespace FinancialDucks.Client2.Components.Categories
{
    public partial class CategoryTreeNode 
    {
        public string NewCategoryText { get; set; }
        public int? NumTransactions { get; private set; }

        public decimal? DollarTotal { get; private set; }

        public int Id => Category==null ? 0 : Category.Id;

        private ChangeTracked<int> _categoryId = new ChangeTracked<int>();

        [Parameter]
        public ICategoryDetail Category { get; set; }

        [Parameter]
        public EventCallback<CategoriesFeature.AddCategoryCommand> NewButtonClick { get; set; }

        [Parameter]
        public EventCallback<ICategoryDetail> CategorySelected { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        [Inject]
        public NotificationDispatcher<CategoryChangeNotification> CategoryChangeDispatcher { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (Category == null)
                return;

            _categoryId.Value = Category.Id;

            if (!_categoryId.HasChanges)
                return;

            _categoryId.AcceptChanges();
            var stats = await Mediator.Send(new CategoryStatsFeature.Query(Category));
            NumTransactions = stats.TransactionCount;
            DollarTotal = stats.Total;
            StateHasChanged();
        }

        public async Task NewButton_Clicked()
        {
            await NewButtonClick.InvokeAsync(new CategoriesFeature.AddCategoryCommand(Category, NewCategoryText));
            NewCategoryText = "";
        }

        public async Task Category_Clicked()
        {
            await CategorySelected.InvokeAsync(Category);
        }
    }
}
