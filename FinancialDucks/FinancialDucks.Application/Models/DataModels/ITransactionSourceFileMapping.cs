namespace FinancialDucks.Application.Models
{
    public interface ITransactionSourceFileMapping : IWithId
    {
        public string FilePattern { get; set; }

        public int SourceId { get; }
    }

    public interface ITransactionSourceFileMappingDetail: ITransactionSourceFileMapping
    {
        public ITransactionSource Source { get; }
    }

}
