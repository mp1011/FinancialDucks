using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client2.Components
{
    public partial class AutoClassifierTable : ComponentBase
    {
        [Parameter]
        public IEnumerable<AutoClassificationResult> Results { get; set; }

        [Parameter]
        public Action<ICategory>? OnCategoryClicked { get; set; }
    }
}