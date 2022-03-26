using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class CategoryRulesFeature
    {
        public record AddCommand(ICategory Category, ICategoryRule rule) : IRequest<ICategoryRule> { }
        public record DeleteCommand(ICategoryRule rule) : IRequest<ICategoryRule> { }

        public class AddHandler : IRequestHandler<AddCommand, ICategoryRule>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly IMediator _mediator;

            public AddHandler(IDataContextProvider dataContextProvider, IMediator mediator)
            {
                _dataContextProvider = dataContextProvider;
                _mediator = mediator;
            }

            public async Task<ICategoryRule> Handle(AddCommand request, CancellationToken cancellationToken)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();
                var result = await dataContext.AddCategoryRule(request.Category, request.rule);
                await _mediator.Publish(new CategoryChangeNotification(result.Category));
                return result;
            }
        }

        public class DeleteHandler : IRequestHandler<DeleteCommand, ICategoryRule>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly IMediator _mediator;

            public DeleteHandler(IDataContextProvider dataContextProvider, IMediator mediator)
            {
                _dataContextProvider = dataContextProvider;
                _mediator = mediator;
            }

            public async Task<ICategoryRule> Handle(DeleteCommand request, CancellationToken cancellationToken)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();
                var result = await dataContext.Delete(request.rule);

                await _mediator.Publish(new CategoryChangeNotification(result.Category));
                return result;
            }
        }

    }
}
