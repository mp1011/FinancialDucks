using FinancialDucks.Application.Models;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionSourceFileMapping : ITransactionSourceFileMappingDetail
    {
        ITransactionSource ITransactionSourceFileMappingDetail.Source => Source;
    }
}
