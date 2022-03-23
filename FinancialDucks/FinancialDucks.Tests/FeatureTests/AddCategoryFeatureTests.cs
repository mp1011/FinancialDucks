using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;
using AppCategory = FinancialDucks.Application.Models.AppModels.Category;

namespace FinancialDucks.Tests.FeatureTests
{
    public class AddCategoryFeatureTests : TestBase
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
    }
}
