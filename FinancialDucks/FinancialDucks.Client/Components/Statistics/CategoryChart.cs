using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using FinancialDucks.Application.Extensions;
using Point = System.Drawing.Point;
using FinancialDucks.Client.Models;

namespace FinancialDucks.Client.Components.Statistics
{
    public partial class CategoryChart
    {
        [Inject]
        public IMediator Mediator { get; set; }

        [Parameter]
        public TransactionsFilter Filter { get; set; }


        [Parameter]
        public EventCallback<ICategory> CategoryClicked { get; set; }


        private CategoryStatsWithChildren _stats;

        public Selection<PieChartSection>[] Sections { get; set; }

        public ChildCategoryStats MouseoverStats { get; set; }

        public Point MouseoverPosition { get; set; }

        public string PieChartGradientConicGradientCSS { get; set; }

        public readonly int[] CSSColors = new int[] { 0xfd7f6f, 0x7eb0d5, 0xb2e061, 0xbd7ebe, 0xffb55a, 0xffee65, 0xbeb9db, 0xfdcce5, 0x8bd3c7 };

        public void ToggleSelection(Selection<PieChartSection> section)
        {
            section.Selected = !section.Selected;
            AdjustChartBasedOnSelection();
        }

        private void AdjustChartBasedOnSelection()
        {
            var adjustedTotal = Sections
                .Where(p => p.Selected)
                .Sum(p => p.Data.Stats.Total);

            var adjustedStats = new CategoryStatsWithChildren(_stats.Main,
                Sections.Where(p => p.Selected)
                        .Select(p => p.Data.Stats.AdjustPercent((double)adjustedTotal))
                        .ToArray());

            var adjustedSections = adjustedStats.Children
                .Select((childStats, index) => CreateChartSection(childStats, adjustedStats, index))
                .ToArray();

            var newGradientCSS = String.Join(",", adjustedSections.Select(s => s.ConicGradientCSS).ToArray());
            if (newGradientCSS == PieChartGradientConicGradientCSS)
                return;

            PieChartGradientConicGradientCSS = newGradientCSS;

            foreach (int index in Enumerable.Range(0, Sections.Length))
            {
                if (!Sections[index].Selected)
                    continue;

                var changedSection = adjustedSections
                    .Single(p => p.Stats.Category.Id == Sections[index].Data.Stats.Category.Id);

                Sections[index] = new Selection<PieChartSection>(changedSection, true);
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Filter == null || !Filter.IsValid || Filter.Category.Name == SpecialCategory.All.ToString())
                return;

            _stats = await Mediator.Send(new CategoryStatsFeature.QueryWithChildren(Filter));
            Sections = _stats.Children
                .Select((childStats,index) => CreateChartSection(childStats, _stats, index))
                .Where(p=>p.Stats.Total != 0)
                .Select(p=> new Selection<PieChartSection>(p, ShouldBeSelectedAtStart(p.Stats)))
                .ToArray();

            foreach(var excess in Sections
                                .Where(p=>p.Selected)
                                .Skip(CSSColors.Length))
            {
                excess.Selected = false;
            }

            AdjustChartBasedOnSelection();
        }

        private bool ShouldBeSelectedAtStart(ChildCategoryStats stats)
            => stats.Percent >= 0.01;
        

        private PieChartSection CreateChartSection(ChildCategoryStats childStats, CategoryStatsWithChildren parentStats, int index)
        {
            int color;
            if(parentStats.Children.Length <= CSSColors.Length)
                color=CSSColors[index];
            else            
                color = index * (0xFFFFFF / parentStats.Children.Count());
        
            double startPercent = 0;
            if (index > 0)
                startPercent = parentStats.Children.Take(index).Sum(c => c.Percent);

            double endPercent = startPercent + childStats.Percent;

            return new PieChartSection(childStats, startPercent, endPercent, color);
        }

        public void MouseOut(MouseEventArgs args)
        {
            var mousePoint = new Point((int)args.OffsetX, (int)args.OffsetY);
            var centerPoint = new Point(100, 100);

            if (centerPoint.DistanceTo(mousePoint) > 100)
            {
                MouseoverStats = null;
                return;
            }
        }

        public void MouseMove(MouseEventArgs args)
        {
            if (Sections == null)
                return;

            var mousePoint = new Point((int)args.OffsetX, (int)args.OffsetY);
            var centerPoint = new Point(100, 100);

            if (mousePoint.DistanceTo(MouseoverPosition) < 5)
                return;

            var angle = centerPoint.GetAngleTo(mousePoint);

            var adjustedAngle = (360 - (angle+180)).ModPositive(360);

            var percent = adjustedAngle / 360.0;
            var match = Sections
                .FirstOrDefault(p=> p.Selected 
                    && percent >= p.Data.ChartPercentFrom 
                    && percent <= p.Data.ChartPercentTo);

            if (match != null)
            {
                MouseoverStats = match.Data.Stats;
                MouseoverPosition = mousePoint;
            }
            else
            {
                MouseoverStats = null;
            }
        }

        public async Task ChartClicked(MouseEventArgs args)
        {
            if(args.Button == (int)MouseButton.Right)
            {
                await CategoryClicked.InvokeAsync(_stats.Main.Category.Parent);
                return;
            }

            MouseMove(args);
            if (MouseoverStats != null)
                await CategoryClicked.InvokeAsync(MouseoverStats.Category);
        }
    }
}
