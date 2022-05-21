using FinancialDucks.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Runtime.Versioning;
using Xunit;

namespace FinancialDucks.Tests.ServiceTests
{
    [SupportedOSPlatform("windows")]
    public class ExcelToCSVConverterTests : TestBase
    {
        [Theory]
        [InlineData($"\\IRA\\2022_05_14.xls")]
        public void CanConvertExcelToCSV(string filePath)
        {
            var serviceProvider = CreateServiceProvider()!;

            var file = GetTestFile(serviceProvider, filePath);
            var converter = serviceProvider.GetRequiredService<IExcelToCSVConverter>();

            var result = converter.ConvertExcelToCSV(file);
            Assert.Equal(".csv", result.Extension);
            var lines = File.ReadAllLines(result.FullName);

            Assert.True(lines.Length > 1);
        }
    }
}
