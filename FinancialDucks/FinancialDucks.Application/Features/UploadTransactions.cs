// Stryker disable all
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class UploadTransactions
    {
        public record Command(ITransaction[] Transactions) : IRequest<ITransaction[]> { }


        public class Handler : IRequestHandler<Command, ITransaction[]>
        {
            private readonly IDataContextProvider _dataContextProvider;

            public Handler(IDataContextProvider dataContextProvider)
            {
                _dataContextProvider = dataContextProvider;
            }

            public async Task<ITransaction[]> Handle(Command request, CancellationToken cancellationToken)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();
                return await dataContext.UploadTransactions(request.Transactions);
            }
        }

    }
}
