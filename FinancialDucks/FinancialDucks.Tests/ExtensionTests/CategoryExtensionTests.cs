using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.ExtensionTests
{
    public class CategoryExtensionTests : TestBase
    {
        [Theory]
        [InlineData("Krusty Burger","Fast-Food",false)]
        [InlineData("Krusty Burger", "Entertainment", true)]
        public async Task CanGetAncestor(string categoryName, string ancestorName, bool expectEmpty)
        {
            var serviceProvider = CreateServiceProvider();
            var tree = await serviceProvider.GetService<ICategoryTreeProvider>()!.GetCategoryTree();

            var category = tree.GetDescendant(categoryName)!;
            var ancestor = category.GetAncestors()
                .FirstOrDefault(p => p.Name == ancestorName);

            if (expectEmpty)
                Assert.Null(ancestor);
            else
                Assert.NotNull(ancestor);
        }

        [Theory]
        [InlineData("Fast-Food", "Krusty Burger", false)]
        [InlineData("Restaurants", "Krusty Burger", false)]
        [InlineData("Restaurants", "NotARestaurant", true)]
        public async Task CanGetDescendant(string categoryName, string descendantName, bool expectEmpty)
        {
            var serviceProvider = CreateServiceProvider();
            var tree = await serviceProvider.GetService<ICategoryTreeProvider>()!.GetCategoryTree();

            var category = tree.GetDescendant(categoryName)!;

            var descendant = category.GetDescendant(descendantName);
           
            if (expectEmpty)
                Assert.Null(descendant);
            else
                Assert.NotNull(descendant);
        }

        [Theory]
        [InlineData("Fast-Food", "Krusty Burger", true)]
        [InlineData("Krusty Burger", "Fast-Food", true)]
        [InlineData("Krusty Burger", "McDonalds", false)]
        [InlineData("Krusty Burger", "Food", true)]
        [InlineData("Food","Krusty Burger", true)]
        [InlineData("Krusty Burger", "Entertainment", false)]
        public async Task TestHasLinearRelation(string name1, string name2, bool expectRelated)
        {
            var serviceProvider = CreateServiceProvider();

            var tree = await serviceProvider.GetService<ICategoryTreeProvider>()!.GetCategoryTree();

            var category1 = tree.GetDescendant(name1)!;
            var category2 = tree.GetDescendant(name2)!;

            Assert.Equal(expectRelated, category1.HasLinearRelationTo(category2));
        }
    }
}
