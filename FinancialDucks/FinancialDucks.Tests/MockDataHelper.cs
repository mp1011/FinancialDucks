﻿using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Tests.TestModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinancialDucks.Tests
{
    public class MockDataHelper
    {
        public MockDataHelper()
        {
            MockCategoryRules.AddRange(GetMockCategoryRules());
        }

        public ITransactionSourceDetail[] GetMockTransactionSources()
        {
            return new ITransactionSourceDetail[]
            {
                CreateMockTransactionSource(1,"Citibank Checking","chk"),
                CreateMockTransactionSource(2, "Citibank Savings", "sav"),
                CreateMockTransactionSource(3, "Citibank Card", "citi 9536", "citi 6204","citi_9536","citi_6204","citi1","citi2"),
                CreateMockTransactionSource(4, "Capital One Card","cap"),
                CreateMockTransactionSource(5, "TFCU","tfcu"),
                CreateMockTransactionSource(6, "HSA","hsa"),
                CreateMockTransactionSource(7, "IRA","ira"),
                CreateMockTransactionSource(8, "Bank A","banka"),
                CreateMockTransactionSource(9, "Bank B","bankb")
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

        public ISourceSnapshot CreateMockSourceSnapshot(ITransactionSource source, DateTime date, decimal amount)
        {
            var mock = new Mock<ISourceSnapshot>();
            mock.Setup(x=>x.SourceId).Returns(source.Id);
            mock.Setup(x => x.Date).Returns(date);
            mock.Setup(x => x.Amount).Returns(amount);

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

        private int _nextId;

        private int NextId()
        {
            _nextId++;
            return _nextId;
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
 
            var credits = root.AddChild(id++, "Credits");
            credits
                .AddChild(id++, "Paychecks");

            return root;
        }

        public List<ICategoryRuleDetail> MockCategoryRules { get; } = new List<ICategoryRuleDetail>();

        public List<IScraperCommandDetail> MockScraperCommands { get; } = new List<IScraperCommandDetail>();

        private IEnumerable<ICategoryRuleDetail> GetMockCategoryRules()
        {
            var categories = GetMockCategoryTree();
            int id = 1;

            yield return new CategoryRule(id++, Priority: 0, categories.GetDescendant("Krusty Burger")!,
                SubstringMatch: "Krusty Burger");

            yield return new CategoryRule(id++, Priority: 0, categories.GetDescendant("McDonalds")!,
              SubstringMatch: "McDonalds");

            yield return new CategoryRule(id++, Priority: 0, categories.GetDescendant("Fast-Food")!,
                SubstringMatch: "Fast-Food");

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

            yield return new CategoryRule(id++, Priority: 0, Category: categories.GetDescendant("Paychecks")!,
               SubstringMatch: "Paycheck");

        }

        public List<ITransactionDetail> MockTransations { get; } = new List<ITransactionDetail>();

        public List<ISourceSnapshot> MockSourceSnapshots { get; } = new List<ISourceSnapshot>();

        public IEnumerable<ITransactionDetail> AddDebitAndCreditTransactions(ITransactionSourceDetail source)
        {
            DateTime lastPaymentDate = new DateTime(2022, 1, 1);

            List<ITransactionDetail> transactionDetails = new List<ITransactionDetail>();
            DateTime date = new DateTime(2021, 1, 1);
            while(date.Year < 2024)
            {
                transactionDetails.Add(AddMockTransaction(date, -100M, "Purchase", source));
                if(date >= lastPaymentDate.AddMonths(1))
                {
                    lastPaymentDate = date;
                    transactionDetails.Add(AddMockTransaction(date, 1000M, "Paycheck", source));

                }
                date = date.AddDays(10);
            }

            return transactionDetails;
        }

        public IEnumerable<ITransactionDetail> AddTransactionsWithSource(ITransactionSourceDetail source, int count, DateTime? date=null)
        {
            List<ITransactionDetail> transactionDetails = new List<ITransactionDetail>();
            date = date ?? new DateTime(2022, 1, 1);

            int index = 0;
            while(index++ < count)
            {
                transactionDetails.Add(AddMockTransaction(date.Value, -9.99M, "Krusty Burger", source));
                date = date.Value.AddDays(5);
            }

            return transactionDetails;
        }

        public IEnumerable<ITransactionDetail> AddKrustyBurgerTransactions()
        {
            List<ITransactionDetail> transactionDetails = new List<ITransactionDetail>();
            DateTime date = new DateTime(2022, 1, 1);

            while(date.Month < 3)
            {
                transactionDetails.Add(AddMockTransaction(date, -9.99M, "Krusty Burger"));
                date = date.AddDays(5);
            }

            return transactionDetails;
        }

        public IEnumerable<ITransactionDetail> AddMcDonaldsTransations()
        {
            List<ITransactionDetail> transactionDetails = new List<ITransactionDetail>();
            DateTime  date = new DateTime(2022, 1, 15);

            while (date.Month < 3)
            {
                transactionDetails.Add(AddMockTransaction(date, -7.99M, "McDonalds"));
                date = date.AddDays(8);
            }

            return transactionDetails;
        }

        public IEnumerable<ITransactionDetail> AddTransactionsForLongTerm()
        {
            List<ITransactionDetail> transactionDetails = new List<ITransactionDetail>();
            DateTime date = new DateTime(2022, 1, 1);

            var rng = new Random(3333);
            decimal price = -9.99M;



            while (date.Year < 2032)
            {
                transactionDetails.Add(AddMockTransaction(date, price, "Krusty Burger"));
                date = date.AddHours(rng.Next(5,99));

                transactionDetails.Add(AddMockTransaction(date, price, "Krusty Burger"));
                date = date.AddHours(rng.Next(5, 99));

                transactionDetails.Add(AddMockTransaction(date, price, "McDonalds"));
                date = date.AddHours(rng.Next(5, 99));

                price -= (decimal)(rng.NextDouble() * 1.99);
                price = Math.Round(price, 2);
            }

            return transactionDetails;
        }

        public IEnumerable<ITransactionDetail> AddTransferTransactions()
        {
            List<ITransactionDetail> transactionDetails = new List<ITransactionDetail>();
            DateTime date = new DateTime(2022, 1, 1);

            while (date.Month < 3)
            {
                transactionDetails.Add(AddMockTransaction(date, -999.99M, "Transfer From Account"));
                transactionDetails.Add(AddMockTransaction(date, 999.99M, "Transfer To Account"));

                date = date.AddDays(5);
            }

            return transactionDetails;
        }

        public IEnumerable<ITransactionDetail> AddPaycheckTransactions()
        {
            List<ITransactionDetail> transactionDetails = new List<ITransactionDetail>();
            DateTime date = new DateTime(2022, 1, 1);

            while (date.Month < 3)
            {
                transactionDetails.Add(AddMockTransaction(date, 500.00M, "Paycheck"));
                date = date.AddDays(7);
            }

            return transactionDetails;
        }


        public IEnumerable<ITransactionDetail> AddUnclassifiedTransactions()
        {
            List<ITransactionDetail> transactionDetails = new List<ITransactionDetail>();
            DateTime date = new DateTime(2022, 1, 1);

            while (date.Month < 3)
            {
                transactionDetails.Add(AddMockTransaction(date, -3.99M, "Unknown Transaction"));
                date = date.AddDays(5);
            }

            return transactionDetails;
        }

        public IEnumerable<ITransactionDetail> AddUnclassifiedFastFoodTransactions()
        {
            List<ITransactionDetail> transactionDetails = new List<ITransactionDetail>();
            DateTime date = new DateTime(2022, 1, 1);

            while (date.Month < 3)
            {
                transactionDetails.Add(AddMockTransaction(date, -5.49M, "Fast-Food"));
                date = date.AddDays(8);
            }

            return transactionDetails;
        }

        private ITransactionDetail AddMockTransaction(DateTime date, decimal amount, string description, 
            ITransactionSourceDetail? source=null)
        {
            var t = new TestTransaction
            {
                Id=NextId(),
                Amount=amount,
                Description=description,
                Date=date,
                SourceId = (source?.Id).GetValueOrDefault()
            };
            MockTransations.Add(t);
            return t;
        }

        public void AddMockScraperCommand(ITransactionSourceDetail source, int sequence, ScraperCommandType scraperCommandType, 
            string selector="", bool waitForNavigate=false)
        {
            var mock = new Mock<IScraperCommandDetail>();
            mock.Setup(x => x.SearchInnerText).Returns(false);
            mock.Setup(x => x.Sequence).Returns(sequence);
            mock.Setup(x=>x.Source).Returns(source);
            mock.Setup(x => x.SourceId).Returns(source.Id);
            mock.Setup(x => x.Selector).Returns(selector);
            mock.Setup(x => x.WaitForNavigate).Returns(waitForNavigate);
            mock.Setup(x => x.TypeId).Returns(scraperCommandType);
            MockScraperCommands.Add(mock.Object);
        }
    }
}
