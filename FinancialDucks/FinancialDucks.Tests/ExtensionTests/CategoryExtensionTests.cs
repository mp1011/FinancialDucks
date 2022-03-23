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
    }
}
