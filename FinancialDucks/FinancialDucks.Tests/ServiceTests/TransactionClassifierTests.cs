using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.ServiceTests
{
    public class TransactionClassifierTests : TestBase
    {
        [Theory]
        [InlineData("Krusty Burger #Shelbyville", -12.99D, "2022/3/1", "Krusty Burger", "Fast-Food,Restaurants,Food,Debits")]
        [InlineData("Krusty Burger #Shelbyville", -12.99D, "2022/3/1", "$20 or Less", "Debits")]     
        [InlineData("Krusty Burger #Shelbyville", -50.00D, "2022/3/1", "$50-$100", "Debits")]
        [InlineData("Krusty Burger #Shelbyville", -100.00D, "2022/3/1", "$50-$100", "Debits")]
        [InlineData("Krusty Burger #Shelbyville", -50.00D, "2022/3/1", "2022 Debits", "Debits")]
        [InlineData("Krusty Burger #Shelbyville", -50.00D, "2022/1/1", "2022 Debits", "Debits")]
        [InlineData("Krusty Burger #Shelbyville", -50.00D, "2022/12/31", "2022 Debits", "Debits")]
        public async Task CanClassifyTransaction(string description, decimal amount, string dateStr, string expectedClassification, string expectedAncestors)
        {
            var serviceProvider = CreateServiceProvider();
            var classifier = serviceProvider.GetService<ITransactionClassifier>();

            var transaction = new ImportedTransaction(null, amount, DateTime.Parse(dateStr), description, 0);

            var result = (await classifier!.Classify(transaction)).FirstOrDefault(p => p.Name == expectedClassification);
               
            Assert.NotNull(result);
            Assert.Equal(expectedClassification, result.Name);

            foreach(var expectedAncestor in expectedAncestors.Split(','))
                Assert.Contains(result.GetAncestors(), a => a.Name == expectedAncestor);
        }

        [Theory]
        [InlineData("Krusty Burger #Shelbyville", -20.01D, "2022/3/1", "$20 or Less")]
        [InlineData("Krusty Burger #Shelbyville", -49.99D, "2022/3/1", "$50-$100")]
        [InlineData("Krusty Burger #Shelbyville", -100.01D, "2022/3/1", "$50-$100")]
        [InlineData("Krusty Burger #Shelbyville", -50.00D, "2021/12/31", "2022 Debits")]
        [InlineData("Krusty Burger #Shelbyville", -50.00D, "2023/1/1", "2022 Debits")]
        public async Task TestNegativeClassification(string description, decimal amount, string dateStr, string shouldNotHaveClassification)
        {
            var serviceProvider = CreateServiceProvider();
            var classifier = serviceProvider.GetService<ITransactionClassifier>();

            var transaction = new ImportedTransaction(null, amount, DateTime.Parse(dateStr), description, 0);

            var result = (await classifier!.Classify(transaction));

            Assert.All(result, r => Assert.NotEqual(shouldNotHaveClassification, r.Name));
        }
    }
}
