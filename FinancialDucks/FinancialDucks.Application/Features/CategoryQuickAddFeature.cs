using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using MediatR;

namespace FinancialDucks.Application.Features
{
    public class CategoryQuickAddFeature
    {
        public record Command(string TextMatch, ICategoryDetail Category, string NewCategoryText) : IRequest<ICategoryDetail> { }

        public class Handler : IRequestHandler<Command, ICategoryDetail>
        {
            private readonly IDataContextProvider _dataContextProvider;
            private readonly ICategoryTreeProvider _categoryTreeProvider;
            private readonly IMediator _mediator;

            public Handler(IDataContextProvider dataContextProvider, ICategoryTreeProvider categoryTreeProvider, IMediator mediator)
            {
                _dataContextProvider = dataContextProvider;
                _categoryTreeProvider = categoryTreeProvider;
                _mediator = mediator;
            }

            public async Task<ICategoryDetail> Handle(Command request, CancellationToken cancellationToken)
            {
                var category = await CreateCategory(request);
                var tree = await _categoryTreeProvider.GetCategoryTree();
                return tree.GetDescendant(category.Id)!;
            }

            private async Task<ICategory> CreateCategory(Command request)
            {
                using var dataContext = _dataContextProvider.CreateDataContext();
                ICategory category;

                if (request.NewCategoryText.IsNonEmpty())
                    category = await dataContext.AddSubcategory(request.Category, request.NewCategoryText);
                else
                    category = request.Category;

                await dataContext.AddCategoryRule(category, new CategoryRule(0, 0,
                    Category: category,
                    SubstringMatch: request.TextMatch));

                await _mediator.Publish(new CategoryChangeNotification(category));

                return category;
            }
        }
    }
}
