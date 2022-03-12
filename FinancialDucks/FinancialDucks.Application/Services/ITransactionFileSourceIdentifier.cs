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
            var name = fileInfo.Name.ToLower();

            var source= _dataContext.TransactionSourcesDetail
                .FirstOrDefault(p => p.SourceFileMappings.Any(s => name.Contains(s.FilePattern)));

            if (source == null)
                throw new Exception($"Unable to find source for file {fileInfo.FullName}");

            return source;
        }
    }
}
