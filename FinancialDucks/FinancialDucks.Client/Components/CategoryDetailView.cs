using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Components
{
    public partial class CategoryDetailView : ICategory
    {
        [Parameter]
        public ICategoryDetail Category { get; set; }

        [Parameter]
        public DescriptionWithCount[] DescriptionCounts { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        public bool ShowUncategorizedOnly { get; set; }
        public bool AddingRule { get; set; }

        private string _newCategoryName;

        int IWithId.Id => Category.Id;

        public bool Starred { get; set; }

        public string Name
        {
            get => _newCategoryName ?? Category.Name;
            set
            {
                if (value == Category.Name)
                    _newCategoryName = null;
                else
                    _newCategoryName = value;
            }
        }

        public bool IsChangingName => _newCategoryName != null;

        protected override void OnInitialized()
        {
            AddingRule = false;
        }

        public void AddRule()
        {
            AddingRule = true;
        }

        public void OnCategoryRuleCreated(ICategoryRule newRule)
        {

            AddingRule = false;
        }

        public void DeleteButton_Click()
        {

        }

        public async void UpdateCategoryName()
        {
            await Mediator.Send(new CategoriesFeature.UpdateCategoryCommand(this));
            _newCategoryName = null;
            StateHasChanged();
        }

        public void CancelCategoryName()
        {
            _newCategoryName = null;
        }

        public async void OnPromptDeleteDialog(bool confirm)
        {
            if(confirm)
                await Mediator.Send(new CategoriesFeature.DeleteCommand(Category));
        }
    }
}
