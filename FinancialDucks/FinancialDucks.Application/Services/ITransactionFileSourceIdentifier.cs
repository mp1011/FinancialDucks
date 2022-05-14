using FinancialDucks.Application.Models;

namespace FinancialDucks.Application.Services
{
    public interface ITransactionFileSourceIdentifier
    {
        ITransactionSource DetectTransactionSource(FileInfo fileInfo);
    }

    public class TransactionFileSourceIdentifier : ITransactionFileSourceIdentifier
    {
        private readonly IDataContextProvider _dataContextProvider;

        public TransactionFileSourceIdentifier(IDataContextProvider dataContext)
        {
            _dataContextProvider = dataContext;
        }

        public ITransactionSource DetectTransactionSource(FileInfo fileInfo)
        {
            using var dataContext = _dataContextProvider.CreateDataContext();
            string fileName = fileInfo.Name.ToLower();
         
            foreach(var source in dataContext.TransactionSourcesDetail)
            {
                if (source.SourceFileMappings.Any(p => fileName.Contains(p.FilePattern, StringComparison.OrdinalIgnoreCase)))
                    return source;

                if((fileInfo?.Directory?.FullName??"").Contains(source.Name, StringComparison.OrdinalIgnoreCase))
                    return source;
            }
           
            
            throw new Exception($"Unable to find source for file {fileInfo.FullName}");
        }
    }
}
