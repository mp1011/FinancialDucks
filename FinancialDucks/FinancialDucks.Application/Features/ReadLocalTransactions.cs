using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class ReadLocalTransactions
    {
        public record Request(ITransactionSource? Account=null) : IRequest<ITransaction[]> { }

        public class Handler : IRequestHandler<Request, ITransaction[]>
        {
            private readonly ISettingsService _settingsService;
            private readonly ITransactionReader _transactionReader;
            private readonly IEqualityComparer<ITransaction> _transactionEqualityComparer;

            public Handler(ISettingsService settingsService, ITransactionReader transactionReader, IEqualityComparer<ITransaction> transactionEqualityComparer)
            {
                _settingsService = settingsService;
                _transactionReader = transactionReader;
                _transactionEqualityComparer = transactionEqualityComparer;
            }

            private DirectoryInfo GetSourceFolder(Request request)
            {
                var folder = _settingsService.SourcePath;
                if (request.Account != null)
                    folder = new DirectoryInfo($"{folder.FullName}\\{request.Account.Name}");

                if (!folder.Exists)
                    folder.Create();

                return folder;
            }

            public async Task<ITransaction[]> Handle(Request request, CancellationToken cancellationToken)
            {
                var parsed = await Task.WhenAll(GetSourceFolder(request)
                    .GetFiles("*.csv",SearchOption.AllDirectories)
                    .Select(f => _transactionReader.ParseTransactions(f)));

                return parsed.SelectMany(p => p)
                                .Distinct(_transactionEqualityComparer)
                                .ToArray();

            }
        }
    }
}
