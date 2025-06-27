using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models.AppModels;
using Microsoft.AspNetCore.Components;

namespace FinancialDucks.Client2.Components
{
    public partial class CashFlowReportTable
    {
        [Parameter]
        public CashFlowReportItem[]? Items { get; set; }

        public decimal RunningNet(CashFlowReportItem item)
        {
            var prior = Items.NullToEmpty().TakeWhile(p => p != item);
            var priorNet = prior.Sum(p => p.Net);
            return priorNet + item.Net;
        }
    }
}
