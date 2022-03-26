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

        public bool AddingRule { get; set; }

        private string _newCategoryName;

        int IWithId.Id => Category.Id;

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

        public async void UpdateCategoryName()
        {
            await Mediator.Send(new CategoriesFeature.UpdateCategoryCommand(this));
        }

        public void CancelCategoryName()
        {
            _newCategoryName = null;
        }
    }
}
