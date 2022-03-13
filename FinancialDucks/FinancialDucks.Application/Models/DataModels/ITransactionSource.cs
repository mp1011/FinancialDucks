namespace FinancialDucks.Application.Models
{
    public interface ITransactionSource : IWithId, IWithName
    {
        int TypeId { get; set; }
    }

    public interface ITransactionSourceDetail : ITransactionSource
    {
        IEnumerable<ITransactionSourceFileMappingDetail> SourceFileMappings { get; }
    }
}
