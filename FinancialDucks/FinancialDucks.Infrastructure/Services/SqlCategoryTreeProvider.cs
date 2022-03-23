using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using FinancialDucks.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using AppCategory = FinancialDucks.Application.Models.AppModels.Category;

namespace FinancialDucks.Infrastructure.Services
{
    public class SqlCategoryTreeProvider : ICategoryTreeProvider
    {
        private readonly DataContextProvider _dbContextProvider;

        public SqlCategoryTreeProvider(DataContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public async Task<ICategoryDetail> GetCategoryTree()
        {
            using var dbContext = _dbContextProvider.CreateDataContext();
            var rootHierarchy = HierarchyId.GetRoot();
            var root = dbContext.Categories.Single(p=>p.HierarchyId == rootHierarchy);
            return await BuildCategoryTree(dbContext, new AppCategory(root.Id, root.Name, null), rootHierarchy);       
        }

        private async Task<AppCategory> BuildCategoryTree(FinancialDucksContext dbContext, AppCategory category, HierarchyId hierarchyId)
        {
            var children = await dbContext.Categories
                   .AsNoTracking()
                   .Include(p=>p.CategoryRules)
                   .Where(p => p.HierarchyId.GetAncestor(1) == hierarchyId)
                   .ToArrayAsync();

            foreach(var child in children)
            {
                var childCategory = new AppCategory(child.Id, child.Name, category);
                childCategory.Rules.AddRange(child.CategoryRules);
                category.Children.Add(childCategory);
                await BuildCategoryTree(dbContext, childCategory, child.HierarchyId);
            }

            return category;
        }

    }
}
