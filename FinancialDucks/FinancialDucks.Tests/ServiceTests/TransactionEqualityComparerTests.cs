using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using System.Collections.Generic;
using Xunit;

namespace FinancialDucks.Tests.ServiceTests
{
    public class TransactionEqualityComparerTests
    {
        public static IEnumerable<object[]> TestTransactionEquality_TestCases()
        {
            ImportedTransaction nullTransaction = null;

            yield return new object[] {
                new ImportedTransaction(
                    SourceFile:null,
                    Amount: 55.0M,
                    Date: new System.DateTime(2022,1,1),
                    Description:"Test1",
                    SourceId:1,
                    Id:1),
                 new ImportedTransaction(
                    SourceFile:null,
                    Amount: 55.0M,
                    Date: new System.DateTime(2022,1,1),
                    Description:"Test1",
                    SourceId:1,
                    Id:2),
                 true};

            yield return new object[] {
                new ImportedTransaction(
                    SourceFile:null,
                    Amount: 56.0M,
                    Date: new System.DateTime(2022,1,1),
                    Description:"Test1",
                    SourceId:1,
                    Id:1),
                 new ImportedTransaction(
                    SourceFile:null,
                    Amount: 55.0M,
                    Date: new System.DateTime(2022,1,1),
                    Description:"Test1",
                    SourceId:1,
                    Id:1),
                 false};

            yield return new object[] {
                new ImportedTransaction(
                    SourceFile:null,
                    Amount: 55.0M,
                    Date: new System.DateTime(2022,1,1),
                    Description:"Test1",
                    SourceId:1,
                    Id:1),
                 new ImportedTransaction(
                    SourceFile:null,
                    Amount: 55.0M,
                    Date: new System.DateTime(2022,1,1),
                    Description:"Test2",
                    SourceId:1,
                    Id:1),
                 false};

            yield return new object[] {
                new ImportedTransaction(
                    SourceFile:null,
                    Amount: 55.0M,
                    Date: new System.DateTime(2022,1,1),
                    Description:"Test1",
                    SourceId:1,
                    Id:1),
                 new ImportedTransaction(
                    SourceFile:null,
                    Amount: 55.0M,
                    Date: new System.DateTime(2022,1,2),
                    Description:"Test2",
                    SourceId:1,
                    Id:1),
                 false};

            yield return new object[] {
                 nullTransaction,
                 new ImportedTransaction(
                    SourceFile:null,
                    Amount: 55.0M,
                    Date: new System.DateTime(2022,1,2),
                    Description:"Test2",
                    SourceId:1,
                    Id:1),
                 false};

            yield return new object[] {
                 null,
                 null,
                 true};
        }

        [Theory]
        [MemberData(nameof(TestTransactionEquality_TestCases))]
        public void TestTransactionEquality(ITransaction t1, ITransaction t2, bool shouldBeEqual)
        {
            var comparer = new TransactionEqualityComparer();
            var areEqual = comparer.Equals(t1, t2);
            if (t1 != null)
                Assert.Equal(t1.Id, comparer.GetHashCode(t1));
            Assert.Equal(shouldBeEqual, areEqual);
        }
    }
}
