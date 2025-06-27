using FinancialDucks.Application.Extensions;

namespace FinancialDucks.Application.Models.AppModels
{
    public record CashFlowReportItem(DateTime PeriodStart, DateTime PeriodEnd, ITransaction[] Debits, ITransaction[] Credits)
    {
        public decimal CreditAmount => Credits.Sum(x => x.Amount);

        public decimal DebitAmount => Debits.Sum(x => x.Amount.Abs());

        public decimal Net => CreditAmount - DebitAmount;
    }
}
