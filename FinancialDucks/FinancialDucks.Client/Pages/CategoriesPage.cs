using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Components;
using static FinancialDucks.Application.Features.CategoriesFeature;

namespace FinancialDucks.Client.Pages
{
    public class CategoriesPage : ComponentBase
    {
        public ICategoryDetail Root { get; private set; }

        [Inject]
        public IMediator Mediator { get; set; }

        public ICategoryDetail SelectedCategory { get; private set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if(firstRender)
            {
                Root = await Mediator.Send(new CategoriesFeature.CategoryTreeRequest());
                SelectedCategory = Root.Children.First();

                StateHasChanged();
            }
        }

        public async void NewCategoryButton_Click(AddCategoryCommand addCategoryCommand)
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
    }
}
