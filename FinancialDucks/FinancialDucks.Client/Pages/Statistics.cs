using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Pages
{
    public partial class Statistics
    {
        [Inject]
        public IMediator Mediator { get; set; }

        [Inject]
        public ICategoryTreeProvider CategoryTreeProvider { get; set; }

        public ICategoryDetail Category { get; set; }


        private ChangeTracked<DateTime> _rangeStart = new ChangeTracked<DateTime>(DateTime.Now.AddYears(-1));
        private ChangeTracked<DateTime> _rangeEnd = new ChangeTracked<DateTime>(DateTime.Now);

        public DateTime RangeStart
        {
            get => _rangeStart.Value;
            set => _rangeStart.Value = value;
        }

        public DateTime RangeEnd
        {
            get => _rangeEnd.Value;
            set => _rangeEnd.Value = value;
        }

        public void ChangeCategory(ICategoryDetail newCategory)
        {
            Category = newCategory;
        }

        public void ChangeCategory(ICategory newCategory)
        {
            Category = Category.Root().GetDescendant(newCategory.Id);
        }

        protected override async Task OnInitializedAsync()
        {
            var categoryTree = await CategoryTreeProvider.GetCategoryTree();
            Category = categoryTree.GetDescendant(SpecialCategory.Debits.ToString());
        }
    }
}
