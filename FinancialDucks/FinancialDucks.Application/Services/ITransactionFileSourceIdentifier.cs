using FinancialDucks.Application.Models;

namespace FinancialDucks.Application.Services
{
    public interface ITransactionFileSourceIdentifier
    {
        ITransactionSource DetectTransactionSource(FileInfo fileInfo);
    }

    public class TransactionFileSourceIdentifier : ITransactionFileSourceIdentifier
    {
        private readonly IDataContext _dataContext;

        public TransactionFileSourceIdentifier(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public ITransactionSource DetectTransactionSource(FileInfo fileInfo)
        {
            string fileName = fileInfo.Name.ToLower();

            foreach(var source in _dataContext.TransactionSourcesDetail)
            {
                if (source.SourceFileMappings.Any(p => fileName.Contains(p.FilePattern, StringComparison.OrdinalIgnoreCase)))
                    return source;
            }
           
            
            throw new Exception($"Unable to find source for file {fileInfo.FullName}");
        }
    }
}
