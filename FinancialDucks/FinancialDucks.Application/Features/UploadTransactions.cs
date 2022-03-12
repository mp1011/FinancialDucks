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
            private readonly IDataContext _dataContext;

            public Handler(IDataContext dataContext)
            {
                _dataContext = dataContext;
            }

            public async Task<ITransaction[]> Handle(Command request, CancellationToken cancellationToken)
            {
                return await _dataContext.UploadTransactions(request.Transactions);
            }
        }

    }
}
