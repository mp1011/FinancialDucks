using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using AppCategory = FinancialDucks.Application.Models.AppModels.Category;

namespace FinancialDucks.Tests.FeatureTests
{
    public class CategoryFeatureTests : TestBase
    {
        [Theory]
        [InlineData("Food","Big Kahuna Burger")]
        public async Task CanAddSubcategory(string parent, string newChild)
        {
            var serviceProvider = CreateServiceProvider();
            var root = serviceProvider.GetRequiredService<MockDataHelper>().GetMockCategoryTree();
            var parentNode = root.GetDescendant(parent)!;

            var result = await serviceProvider.GetService<IMediator>()!
                .Send(new CategoriesFeature.AddCategoryCommand(parentNode, newChild));

            parentNode.AddSubcategory(result);

            parentNode = root.GetDescendant(parent)!;
            Assert.Contains(parentNode.Children, c=>c.Name.Equals(newChild));
        }

        [Fact]
        public async Task CanMoveCategory()
        {
            var serviceProvider = CreateServiceProvider();
            var root = serviceProvider.GetRequiredService<MockDataHelper>().GetMockCategoryTree();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var testDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();
             
            var restaurantCategory = root.GetDescendant("Restaurants")!;
            var krustyBurger = root.GetDescendant("Krusty Burger")!;
            var fastFoodCategory = root.GetDescendant("Fast-Food")!;
            Assert.Contains(fastFoodCategory.Children, f => f.Name == "Krusty Burger");

            var newCategory = await mediator.Send(new CategoriesFeature.AddCategoryCommand(restaurantCategory, "Burger Places"));
            var movedCategory = await mediator.Send(new CategoriesFeature.MoveCommand(krustyBurger, newCategory));

            fastFoodCategory = root.GetDescendant("Fast-Food")!;
            Assert.DoesNotContain(fastFoodCategory.Children, f => f.Name == "Krusty Burger");
            
            var burgerPlacesCategory = root.GetDescendant("Burger Places")!;
            Assert.Contains(burgerPlacesCategory.Children, f => f.Name == "Krusty Burger");
        }
    }
}
