using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client2.Components.Categories
{
    public partial class CategoryDetailView : ICategory
    {
        [Parameter]
        public ICategoryDetail Category { get; set; }

        [Inject]
        public IMediator Mediator { get; set; }

        private ChangeTracked<bool> _starred = new ChangeTracked<bool>();

        public ICategoryDetail Parent { get; set; }

        public bool Starred
        {
            get => _starred;
            set => _starred.Value = value;
        }

        public bool ShowUncategorizedOnly { get; set; }
        public bool AddingRule { get; set; }


        int IWithId.Id => Category.Id;

        private ChangeTracked<string> _name = new ChangeTracked<string>();

        public string Name
        {
            get => _name;
            set => _name.Value = value;
        }

        public bool HasChanges => _name.HasChanges || _starred.HasChanges;

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

        public async void UpdateCategory()
        {
            await Mediator.Send(new CategoriesFeature.UpdateCategoryCommand(this));
            _name.AcceptChanges();
            _starred.AcceptChanges();
            StateHasChanged();
        }

        public void CancelCategoryName()
        {
            _name.RejectChanges();
        }

        public async void OnPromptDeleteDialog(bool confirm)
        {
            if(confirm)
                await Mediator.Send(new CategoriesFeature.DeleteCommand(Category));
        }

        public void ChangeCategory(ICategory newCategory)
        {
            var newCategoryDetail = Category.Root().GetDescendant(newCategory.Id);
            if (newCategoryDetail != null && newCategoryDetail.Children.Any())
            {
                Category = newCategoryDetail;
                _starred.SetAndAccept(Category.Starred);
                _name.SetAndAccept(Category.Name);
            }
        }

        protected override void OnParametersSet()
        {
            if(Category != null)
            {
                _starred.SetAndAccept(Category.Starred);
                _name.SetAndAccept(Category.Name);

                Parent = Category.Parent;
            }                
        }

        public async Task ChangeParent()
        {
            await Mediator.Send(new CategoriesFeature.MoveCommand(Category, Parent));
        }
    }

}
