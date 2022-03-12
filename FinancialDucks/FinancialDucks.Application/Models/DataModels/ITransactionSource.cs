namespace FinancialDucks.Application.Models
{
    public interface ITransactionSource : IWithId
    {
        string Name { get; set; }
        int TypeId { get; set; }
    }

    public interface ITransactionSourceDetail : ITransactionSource
    {
        ICollection<ITransactionSourceFileMappingDetail> SourceFileMappings { get; }
    }
}
