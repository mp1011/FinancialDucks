using FinancialDucks.Application.Features;
using FinancialDucks.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.ServiceTests
{
    public class CategoryTreeProviderTests : TestBase
    {
        [Fact]
        public async Task CategoryTreeProviderCachesResultsUnlessChanged()
        {
            var serviceProvider = CreateServiceProvider();

            var cachedTreeProvider = (serviceProvider.GetRequiredService<ICategoryTreeProvider>() as CachedCategoryTreeProvider)!;             
            var tree = await cachedTreeProvider.GetCategoryTree();
            var cacheDate = cachedTreeProvider.GetLastUpdatedDate();

            await cachedTreeProvider.GetCategoryTree();
            Assert.Equal(cacheDate, cachedTreeProvider.GetLastUpdatedDate());

            var mediator = serviceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new CategoriesFeature.AddCategoryCommand(tree, "TESTNEWCATEGORY"));

            await cachedTreeProvider.GetCategoryTree();
            Assert.True(cachedTreeProvider.GetLastUpdatedDate() > cacheDate);
        }
    }
}
