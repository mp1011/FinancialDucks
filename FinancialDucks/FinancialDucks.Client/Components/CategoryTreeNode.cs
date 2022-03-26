using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using FinancialDucks.Client.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components
{
    public partial class CategoryTreeNode : INotificationHandler<CategoryChangeNotification>, IDisposable
    {
        public string NewCategoryText { get; set; }
        public int? NumTransactions { get; private set; }

        public decimal? DollarTotal { get; private set; }

        public int Id => Category==null ? 0 : Category.Id;

        public DescriptionWithCount[] CategoryDescriptions { get; private set; }

        [Parameter]
        public ICategoryDetail Category { get; set; }

        [Parameter]
        public EventCallback<CategoriesFeature.AddCategoryCommand> NewButtonClick { get; set; }

        [Parameter]
        public EventCallback<CategorySelectedEventArgs> CategorySelected { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        [Inject]
        public NotificationDispatcher<CategoryChangeNotification> CategoryChangeDispatcher { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (Category == null)
                return;

            CategoryChangeDispatcher.Register(this);
            var stats = await Mediator.Send(new CategoryStatsFeature.Query(Category));
            NumTransactions = stats.TransactionCount;
            DollarTotal = stats.Total;
            CategoryDescriptions = stats.Descriptions;
            StateHasChanged();
        }

        public void Dispose()
        {
            CategoryChangeDispatcher.Unregister(this);
        }

        public async Task NewButton_Clicked()
        {
            await NewButtonClick.InvokeAsync(new CategoriesFeature.AddCategoryCommand(Category, NewCategoryText));
            NewCategoryText = "";
        }

        public async Task Category_Clicked()
        {
            await CategorySelected.InvokeAsync(new CategorySelectedEventArgs(Category, CategoryDescriptions));
        }

        public async Task Handle(CategoryChangeNotification notification, CancellationToken cancellationToken)
        {
            var changedNode = Category.Root().GetDescendant(notification.Category.Id);

            if (changedNode.HasLinearRelationTo(Category))
            {
                var stats = await Mediator.Send(new CategoryStatsFeature.Query(Category));
                NumTransactions = stats.TransactionCount;
                DollarTotal = stats.Total;
                CategoryDescriptions = stats.Descriptions;
                StateHasChanged();
            }
        }
    }
}
