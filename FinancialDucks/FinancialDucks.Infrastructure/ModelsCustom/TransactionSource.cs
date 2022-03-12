using FinancialDucks.Application.Models;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class TransactionSource : ITransactionSourceDetail
    {
        IEnumerable<ITransactionSourceFileMappingDetail> ITransactionSourceDetail.SourceFileMappings =>
            TransactionSourceFileMappings.Cast<ITransactionSourceFileMappingDetail>();



    }
}
