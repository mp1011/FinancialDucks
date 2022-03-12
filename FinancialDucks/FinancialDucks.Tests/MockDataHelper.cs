using FinancialDucks.Application.Models;
using Moq;
using System.Collections.Generic;
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
    }
}
