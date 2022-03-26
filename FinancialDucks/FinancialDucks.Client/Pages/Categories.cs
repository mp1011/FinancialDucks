﻿using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using FinancialDucks.Client.Models;
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

        public DescriptionWithCount[] SelectedCategoryDescriptions { get; private set; }

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
            
            if(SelectedCategory != null)
                await JSRuntime.InvokeVoidAsync("buildTree", null);
        }

        public async void NewCategoryButton_Click(CategoriesFeature.AddCategoryCommand addCategoryCommand)
        {
            var newCategory = await Mediator.Send(addCategoryCommand);
            Root.GetDescendant(addCategoryCommand.Parent.Name)
                .AddSubcategory(newCategory);

            StateHasChanged();
        }

        public void OnCategorySelected(CategorySelectedEventArgs args)
        {
            SelectedCategory = args.Category;
            SelectedCategoryDescriptions = args.Descriptions;
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