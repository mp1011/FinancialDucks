namespace FinancialDucks.Application.Models
{
    public interface ITransactionSource : IWithId, IWithName
    {
        int TypeId { get; set; }
        string Url { get; }
    }

    public interface ITransactionSourceDetail : ITransactionSource
    {
        IEnumerable<ISourceSnapshot> SourceSnapshots { get; }
        ITransactionSourceType SourceType { get; }
    }
}
