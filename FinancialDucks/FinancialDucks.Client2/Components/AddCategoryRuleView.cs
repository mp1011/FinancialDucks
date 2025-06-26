using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client2.Components
{
    public partial class AddCategoryRuleView : ICategoryRule
    {
        [Inject]
        public IMediator Mediator { get; set; }

        [Parameter]
        public ICategory Category { get; set; }

        [Parameter]
        public EventCallback<ICategoryRule> OnCategoryRuleCreated { get; set; }

        public string DescriptionContains { get; set; }

        public int CategoryId => Category.Id;

        public string SubstringMatch { get; set; }

        public decimal? AmountMin { get; set; }

        public decimal? AmountMax { get; set; }

        public DateTime? DateMin { get; set; }

        public DateTime? DateMax { get; set; }

        public short Priority { get; set; }

        public int Id =>0;

        protected override void OnInitialized()
        {
            DescriptionContains = null;
            AmountMin = null;   
            AmountMax = null;
            DateMin = null;
            DateMax = null;
            Priority = 0;
        }

        public async void CreateButton_Click()
        {
            var newRule = await Mediator.Send(new CategoryRulesFeature.AddCommand(Category, this));
            SubstringMatch = "";
            await OnCategoryRuleCreated.InvokeAsync(newRule);
            await Mediator.Publish(new CategoryChangeNotification(Category));            
        }
    }
}
