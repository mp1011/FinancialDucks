using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialDucks.Client.Components
{
    public partial class CategoryChart
    {
        [Inject]
        public IMediator Mediator { get; set; }

        [Parameter]
        public ICategoryDetail Category { get; set; }

        public string PieChartGradientConicGradientCSS { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (Category == null || Category.Name == SpecialCategory.All.ToString())
                return;

            var stats = await Mediator.Send(new CategoryStatsFeature.QueryWithChildren(Category));
            PieChartGradientConicGradientCSS = String.Join(",", stats.Children.Select((c, i) => GetPieChartSegmentCSS(stats, c, i)).ToArray());
        }

        private string GetPieChartSegmentCSS(CategoryStatsWithChildren totalStats, ChildCategoryStats stats, int index)
        {
            var color = index * (0xFFFFFF / totalStats.Children.Count());
            var hexColor = color.ToString("X6");

            double startPercent = 0;
            if (index > 0)
                startPercent = totalStats.Children.Take(index).Sum(c => c.Percent);

            double endPercent = startPercent + stats.Percent;

            return $"#{hexColor} {startPercent.ToString("P2")} {endPercent.ToString("P2")}";
        }
    }
}
