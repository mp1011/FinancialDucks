using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class CategoriesFeature
    {
        public record CategoryTreeRequest() : IRequest<ICategoryDetail> { }
        public record AddCategoryCommand(ICategory Parent, string Text) : IRequest<ICategory> { }
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
            private readonly IDataContext _dataContext;

            public AddCategoryHandler(IDataContext dataContext)
            {
                _dataContext = dataContext;
            }

            public async Task<ICategory> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
            {
                return await _dataContext.AddSubcategory(request.Parent, request.Text);
            }
        }
    }
}
