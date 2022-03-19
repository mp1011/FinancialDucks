using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class AddCategoryRule
    {
        public record Command(ICategory Category, ICategoryRule rule) : IRequest<ICategoryRule> { }

        public class Handler : IRequestHandler<Command, ICategoryRule>
        {
            private readonly IDataContextProvider _dataContextProvider;

            public Handler(IDataContextProvider dataContext)
            {
                _dataContextProvider = dataContext;
            }

            public async Task<ICategoryRule> Handle(Command request, CancellationToken cancellationToken)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();
                var result = await dataContext.AddCategoryRule(request.Category, request.rule);
                return result;
            }
        }
    }
}
