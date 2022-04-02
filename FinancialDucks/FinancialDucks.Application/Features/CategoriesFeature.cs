using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class CategoriesFeature
    {
        public record CategoryTreeRequest() : IRequest<ICategoryDetail> { }
        public record AddCategoryCommand(ICategory Parent, string Text) : IRequest<ICategory> { }
        public record UpdateCategoryCommand(ICategory Category) : IRequest<ICategory> { }
        public record DeleteCommand(ICategory Category) : IRequest<ICategory> { }

        public class CategoryTreeRequestHandler : IRequestHandler<CategoryTreeRequest, ICategoryDetail>
        {
            private readonly ICategoryTreeProvider _categoryTreeProvider;

            public CategoryTreeRequestHandler(ICategoryTreeProvider categoryTreeProvider)
            {
                _categoryTreeProvider = categoryTreeProvider;
            }

            public async Task<ICategoryDetail> Handle(CategoryTreeRequest request, CancellationToken cancellationToken)
            {
                return await _categoryTreeProvider.GetCategoryTree();
            }
        }
        public class AddCategoryHandler : IRequestHandler<AddCategoryCommand, ICategory>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly IMediator _mediator;

            public AddCategoryHandler(IDataContextProvider dataContextProvider, IMediator mediator)
            {
                _dataContextProvider = dataContextProvider;
                _mediator = mediator;
            }

            public async Task<ICategory> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();
                var result= await dataContext.AddSubcategory(request.Parent, request.Text);
                await _mediator.Publish(new CategoryChangeNotification(result));
                return result;
            }
        }

        public class UpdateHandler : IRequestHandler<UpdateCategoryCommand,ICategory>
        {
            private readonly IMediator _mediator;
            private readonly IDataContextProvider _dataContextProvider;

            public UpdateHandler(IMediator mediator, IDataContextProvider dataContextProvider)
            {
                _mediator = mediator;
                _dataContextProvider = dataContextProvider;
            }

            public async Task<ICategory> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
            {
                using var dataContext= _dataContextProvider.CreateDataContext();
                var result = await dataContext.Update(request.Category);
                await _mediator.Publish(new CategoryChangeNotification(result));
                return result;
            }
        }

        public class DeleteHandler : IRequestHandler<DeleteCommand, ICategory>
        {
            private readonly IMediator _mediator;
            private readonly IDataContextProvider _dataContextProvider;

            public DeleteHandler(IMediator mediator, IDataContextProvider dataContextProvider)
            {
                _mediator = mediator;
                _dataContextProvider = dataContextProvider;
            }

            public async Task<ICategory> Handle(DeleteCommand request, CancellationToken cancellationToken)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();
                var result = await dataContext.Delete(request.Category);
                await _mediator.Publish(new CategoryChangeNotification(result));
                return result;
            }
        }
    }
}
