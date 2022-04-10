using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client.Pages
{
    public partial class Statistics
    {
        [Inject]
        public IMediator Mediator { get; set; }

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

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        //protected override Task OnAfterRenderAsync(bool firstRender)
        //{
        //    if (_rangeStart.HasChanges || _rangeEnd.HasChanges)
        //    {

        //    }
        //}

        public void ChangeCategory(ICategoryDetail newCategory)
        {
            Category = newCategory;
        }

        public void ChangeCategory(ICategory newCategory)
        {
            Category = Category.Root().GetDescendant(newCategory.Id);
        }

    }
}
