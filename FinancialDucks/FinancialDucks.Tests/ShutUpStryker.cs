using FinancialDucks.Application.Models;
using Xunit;

namespace FinancialDucks.Tests
{
    public class ShutUpStryker
    {
        [Fact]
        public void StrykerPacifier()
        {
            var categoryTree = new MockDataHelper().GetMockCategoryTree();
            Assert.Null(categoryTree.GetDescendant(-999));
        }
    }
}
