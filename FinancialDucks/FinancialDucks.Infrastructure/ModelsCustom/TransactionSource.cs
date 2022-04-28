using FinancialDucks.Application.Models;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionSource : ITransactionSourceDetail
    {
        IEnumerable<ITransactionSourceFileMappingDetail> ITransactionSourceDetail.SourceFileMappings =>
            TransactionSourceFileMappings.Cast<ITransactionSourceFileMappingDetail>();

        ITransactionSourceType ITransactionSourceDetail.SourceType => Type;

        IEnumerable<ISourceSnapshot> ITransactionSourceDetail.SourceSnapshots =>
            SourceSnapshots.Cast<ISourceSnapshot>();
    }
}
