using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Tests.TestModels;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FinancialDucks.Tests
{
    public static class MockDataHelper
    {
        public static ITransactionSourceDetail[] GetMockTransactionSources()
        {
            return new ITransactionSourceDetail[]
            {
                CreateMockTransactionSource(1,"Citibank Checking","chk"),
                CreateMockTransactionSource(2, "Citibank Savings", "sav"),
                CreateMockTransactionSource(3, "Citibank Card", "citi 9536", "citi 6204","citi_9536","citi_6204","citi1","citi2"),
                CreateMockTransactionSource(4, "Capital One Card","cap"),
                CreateMockTransactionSource(5, "TFCU","tfcu")
            };
        }

        public static ITransactionSourceDetail CreateMockTransactionSource(int id, string name, params string[] fileMappings)
        {
            var mock = new Mock<ITransactionSourceDetail>();
            mock.Setup(x => x.Id).Returns(id);
            mock.Setup(x => x.Name).Returns(name);
            mock.Setup(x => x.SourceFileMappings)
                .Returns(CreateMockTransactionSourceMapping(mock.Object, fileMappings).ToList());

            return mock.Object;
        }

        private static IEnumerable<ITransactionSourceFileMappingDetail> CreateMockTransactionSourceMapping(ITransactionSourceDetail source, 
            string[] fileMappings)
        {
            foreach(var mapping in fileMappings)
            {
                var mock = new Mock<ITransactionSourceFileMappingDetail>();
                mock.Setup(x => x.FilePattern).Returns(mapping);
                mock.Setup(x=>x.SourceId).Returns(source.Id);
                mock.Setup(x => x.Source).Returns(source);
                yield return mock.Object;
            }

        }

        public static ICategoryDetail GetMockCategoryTree()
        {
            var root = new TestCategory(1, "Master", null);

            var debits = root.AddChild(2, "Debits");

            debits
                 .AddChild(3, "Food")
                 .AddChild(4, "Restaurants")
                 .AddChild(5, "Fast-Food")
                 .AddChild(6, "Krusty Burger");

            debits
                .AddChild(7, "Entertainment")
                .AddChild(8, "Streaming Services")
                .AddChild(9, "Netflix");

            debits
                .AddChild(10, "$20 or Less");

            debits
               .AddChild(10, "$50-$100");

            debits
                .AddChild(10, "2022 Debits");

            return root;
        }

        public static IEnumerable<ICategoryRuleDetail> GetMockCategoryRules()
        {
            var categories = GetMockCategoryTree();

            yield return new CategoryRule(1, Priority: 0, categories.GetDescendant("Krusty Burger")!,
                SubstringMatch: "Krusty Burger");

            yield return new CategoryRule(2, Priority: 0, categories.GetDescendant("$20 or Less")!,
               AmountMax: 0M,
               AmountMin: -20M);

            yield return new CategoryRule(2, Priority: 0, categories.GetDescendant("$50-$100")!,
                AmountMax: -50M,
                AmountMin: -100M);

            yield return new CategoryRule(2, Priority: 0, categories.GetDescendant("2022 Debits")!,
               DateMin: new DateTime(2022, 1, 1),
               DateMax: new DateTime(2022, 12, 31));
        }

        public static IEnumerable<ITransactionDetail> GetMockTransactions()
        {
            DateTime date = new DateTime(2022, 1, 1);

            while(date.Month < 3)
            {
                yield return GetMockTransaction(date, 9.99M, "Krusty Burger");
                date = date.AddDays(5);
            }
        }

        private static ITransactionDetail GetMockTransaction(DateTime date, decimal amount, string description)
        {
            var mock = new Mock<ITransactionDetail>();
            mock.Setup(x => x.Amount).Returns(amount);
            mock.Setup(x => x.Description).Returns(description);
            mock.Setup(x=>x.Date).Returns(date);
            return mock.Object;
        }
    }
}
