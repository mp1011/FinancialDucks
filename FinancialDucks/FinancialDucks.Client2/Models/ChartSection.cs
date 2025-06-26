using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Client2.Models
{
    public record PieChartSection(ChildCategoryStats Stats, double ChartPercentFrom, double ChartPercentTo, int cssColor)
    {
        public string CSSColor => $"#{cssColor.ToString("X6")}";
        public string ConicGradientCSS=> $"#{cssColor.ToString("X6")} {ChartPercentFrom.ToString("P2")} {ChartPercentTo.ToString("P2")}";
    }
}
