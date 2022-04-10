﻿using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using FinancialDucks.Client.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components.Categories
{
    public partial class CategoryTreeNode : INotificationHandler<CategoryChangeNotification>, IDisposable
    {
        public string NewCategoryText { get; set; }
        public int? NumTransactions { get; private set; }

        public decimal? DollarTotal { get; private set; }

        public int Id => Category==null ? 0 : Category.Id;

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

        protected override async Task OnInitializedAsync()
        {
            if (Category == null)
                return;

            CategoryChangeDispatcher.Register(this);
            var stats = await Mediator.Send(new CategoryStatsFeature.Query(Category,
                new DateTime(2022, 1, 1),
                new DateTime(2022, 12, 1)));
            NumTransactions = stats.TransactionCount;
            DollarTotal = stats.Total;
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
            await CategorySelected.InvokeAsync(Category);
        }

        public async Task Handle(CategoryChangeNotification notification, CancellationToken cancellationToken)
        {
            var changedNode = Category.Root().GetDescendant(notification.Category.Id);

            if (changedNode.HasLinearRelationTo(Category))
            {
                var stats = await Mediator.Send(new CategoryStatsFeature.Query(Category,
                    DateTime.Now.AddYears(-1),
                    DateTime.Now));
                NumTransactions = stats.TransactionCount;
                DollarTotal = stats.Total;
                StateHasChanged();
            }
        }
    }
}