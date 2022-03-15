using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using FinancialDucks.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using AppCategory = FinancialDucks.Application.Models.AppModels.Category;

namespace FinancialDucks.Infrastructure.Services
{
    public class SqlCategoryTreeProvider : ICategoryTreeProvider
    {
        private readonly FinancialDucksContext _dbContext;

        public SqlCategoryTreeProvider(FinancialDucksContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICategoryDetail> GetCategoryTree()
        {
            var rootHierarchy = HierarchyId.GetRoot();
            var root = _dbContext.Categories.Single(p=>p.HierarchyId == rootHierarchy);
            return await BuildCategoryTree(new AppCategory(root.Id, root.Name, null), rootHierarchy);       
        }

        private async Task<AppCategory> BuildCategoryTree(AppCategory category, HierarchyId hierarchyId)
        {
            var children = await _dbContext.Categories
                   .AsNoTracking()
                   .Include(p=>p.CategoryRules)
                   .Where(p => p.HierarchyId.GetAncestor(1) == hierarchyId)
                   .ToArrayAsync();

            foreach(var child in children)
            {
                var childCategory = new AppCategory(child.Id, child.Name, category);
                childCategory.Rules.AddRange(child.CategoryRules);
                category.Children.Add(childCategory);
                await BuildCategoryTree(childCategory, child.HierarchyId);
            }

            return category;
        }

    }
}
