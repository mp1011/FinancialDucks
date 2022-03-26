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
    public class MockDataHelper
    {

        public ITransactionSourceDetail[] GetMockTransactionSources()
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

        public ITransactionSourceDetail CreateMockTransactionSource(int id, string name, params string[] fileMappings)
        {
            var mock = new Mock<ITransactionSourceDetail>();
            mock.Setup(x => x.Id).Returns(id);
            mock.Setup(x => x.Name).Returns(name);
            mock.Setup(x => x.SourceFileMappings)
                .Returns(CreateMockTransactionSourceMapping(mock.Object, fileMappings).ToList());

            return mock.Object;
        }

        private IEnumerable<ITransactionSourceFileMappingDetail> CreateMockTransactionSourceMapping(ITransactionSourceDetail source, 
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

        public ICategoryDetail GetMockCategoryTree()
        {
            int id = 1;
            var root = new TestCategory(id++, "Master", null);

            var debits = root.AddChild(id++, "Debits");
            
            debits
                 .AddChild(id++, "Food")
                 .AddChild(id++, "Restaurants")
                 .AddChild(id++, "Fast-Food")
                 .AddChildReturnThis(id++, "Krusty Burger")
                 .AddChildReturnThis(id++, "McDonalds");



            debits
                .AddChild(id++, "Entertainment")
                .AddChild(id++, "Streaming Services")
                .AddChild(id++, "Netflix");

            debits
                .AddChild(id++, "$20 or Less");

            debits
                .AddChild(id++, "$50-$100");

            debits
                .AddChild(id++, "2022 Debits");

            root.AddChild(id++, "Transfers");
            root.AddChild(id++, "Credits");

            return root;
        }

        public IEnumerable<ICategoryRuleDetail> GetMockCategoryRules()
        {
            var categories = GetMockCategoryTree();
            int id = 1;

            yield return new CategoryRule(id++, Priority: 0, categories.GetDescendant("Krusty Burger")!,
                SubstringMatch: "Krusty Burger");

            yield return new CategoryRule(id++, Priority: 0, categories.GetDescendant("McDonalds")!,
              SubstringMatch: "McDonalds");


            yield return new CategoryRule(id++, Priority: 0, categories.GetDescendant("$20 or Less")!,
               AmountMax: 0M,
               AmountMin: -20M);

            yield return new CategoryRule(id++, Priority: 0, categories.GetDescendant("$50-$100")!,
                AmountMax: -50M,
                AmountMin: -100M);

            yield return new CategoryRule(id++, Priority: 0, categories.GetDescendant("2022 Debits")!,
               DateMin: new DateTime(2022, 1, 1),
               DateMax: new DateTime(2022, 12, 31));

            yield return new CategoryRule(id++, Priority:0, Category:categories.GetDescendant("Transfers")!,
                SubstringMatch: "Transfer");
        }

        public List<ITransactionDetail> MockTransations { get; } = new List<ITransactionDetail>();
        public void AddKrustyBurgerTransactions()
        {
            DateTime date = new DateTime(2022, 1, 1);

            while(date.Month < 3)
            {
                MockTransations.Add(GetMockTransaction(date, -9.99M, "Krusty Burger"));
                date = date.AddDays(5);
            }
        }

        public void AddMcDonaldsTransations()
        {
            DateTime  date = new DateTime(2022, 1, 15);

            while (date.Month < 3)
            {
                MockTransations.Add(GetMockTransaction(date, -7.99M, "McDonalds"));
                date = date.AddDays(8);
            }
        }

        public void AddTransferTransactions()
        {
            DateTime date = new DateTime(2022, 1, 1);

            while (date.Month < 3)
            {
                MockTransations.Add(GetMockTransaction(date, -99.99M, "Transfer From Account"));
                MockTransations.Add(GetMockTransaction(date, 99.99M, "Transfer To Account"));

                date = date.AddDays(5);
            }
        }


        public void AddPaycheckTransactions()
        {
            DateTime date = new DateTime(2022, 1, 1);

            while (date.Month < 3)
            {
                MockTransations.Add(GetMockTransaction(date, 500.00M, "Paycheck"));

                date = date.AddDays(5);
            }
        }


        public void AddUnclassifiedTransactions()
        {
            DateTime date = new DateTime(2022, 1, 1);

            while (date.Month < 3)
            {
                MockTransations.Add(GetMockTransaction(date, -7.99M, "Unknown Transaction"));
                date = date.AddDays(5);
            }
        }

        private ITransactionDetail GetMockTransaction(DateTime date, decimal amount, string description)
        {
            var mock = new Mock<ITransactionDetail>();
            mock.Setup(x => x.Amount).Returns(amount);
            mock.Setup(x => x.Description).Returns(description);
            mock.Setup(x=>x.Date).Returns(date);
            return mock.Object;
        }
    }
}
