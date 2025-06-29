using FinancialDucks.Application.Models.AppModels;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client2.Components
{
    public partial class Pagination
    {        
        [Parameter] public int Page { get; set; }
        [Parameter] public int PageSize { get; set; }
        [Parameter] public int TotalPages { get; set; }
        [Parameter] public EventCallback<int> OnPageChanged { get; set; }

        public int VisibleNavigationPageRange { get; } = 3;

        public int[] VisibleNavigationPages
        {
            get
            {
                if (TotalPages == 0)
                    return Array.Empty<int>();

                var start = Math.Max(1, Page - VisibleNavigationPageRange);
                var end = Math.Min(TotalPages, start + (VisibleNavigationPageRange * 2));

                return Enumerable.Range(start, (end - start) + 1).ToArray();
            }
        }
    }
}
