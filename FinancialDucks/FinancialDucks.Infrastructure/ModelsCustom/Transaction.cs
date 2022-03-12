using FinancialDucks.Application.Models;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class Transaction : ITransactionDetail
    {
        ITransactionSource ITransactionDetail.Source => Source;
    }
}
