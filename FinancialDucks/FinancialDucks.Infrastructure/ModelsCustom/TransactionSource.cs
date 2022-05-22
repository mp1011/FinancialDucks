using FinancialDucks.Application.Models;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionSource : ITransactionSourceDetail
    {
        ITransactionSourceType ITransactionSourceDetail.SourceType => Type;

        IEnumerable<ISourceSnapshot> ITransactionSourceDetail.SourceSnapshots =>
            SourceSnapshots.Cast<ISourceSnapshot>();
    }
}
