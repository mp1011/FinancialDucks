namespace FinancialDucks.Application.Models
{
    public interface ITransactionSource : IWithId, IWithName
    {
        int TypeId { get; set; }
        string Url { get; }
    }

    public interface ITransactionSourceDetail : ITransactionSource
    {
        IEnumerable<ITransactionSourceFileMappingDetail> SourceFileMappings { get; }
        IEnumerable<IScraperCommandDetail> ScraperCommands { get; }
        ITransactionSourceType SourceType { get; }
    }
}
