﻿using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FinancialDucks.Client.Pages
{
    public partial class Categories : INotificationHandler<CategoryChangeNotification>, IDisposable
    {
        public ICategoryDetail Root { get; private set; }

        [Inject]
        public IMediator Mediator { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public NotificationDispatcher<CategoryChangeNotification> CategoryChangeDispatcher { get; set; }

        public ICategoryDetail SelectedCategory { get; private set; }

        protected override void OnInitialized()
        {
            CategoryChangeDispatcher.Register(this);
        }

        public void Dispose()
        {
            CategoryChangeDispatcher.Unregister(this);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                Root = await Mediator.Send(new CategoriesFeature.CategoryTreeRequest());
                SelectedCategory = Root.Children.First();
                StateHasChanged();
                return;
            }
        }

        public async void NewCategoryButton_Click(CategoriesFeature.AddCategoryCommand addCategoryCommand)
        {
            var newCategory = await Mediator.Send(addCategoryCommand);
            Root.GetDescendant(addCategoryCommand.Parent.Name)
                .AddSubcategory(newCategory);

            StateHasChanged();
        }

        public void OnCategorySelected(ICategoryDetail category)
        {
            SelectedCategory = category;
            StateHasChanged();
        }

        public async Task Handle(CategoryChangeNotification notification, CancellationToken cancellationToken)
        {
            Root = await Mediator.Send(new CategoriesFeature.CategoryTreeRequest());
            SelectedCategory = Root.GetDescendant(SelectedCategory.Id);
            StateHasChanged();
        }
    }
}
