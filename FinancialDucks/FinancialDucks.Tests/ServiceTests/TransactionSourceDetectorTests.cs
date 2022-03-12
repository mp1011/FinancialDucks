using FinancialDucks.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using Xunit;

namespace FinancialDucks.Tests.ServiceTests
{
    public class TransactionSourceDetectorTests :TestBase
    {
        [Theory]
        [InlineData($"\\Jan2021\\citi 6204.csv", "Citibank Card")]
        [InlineData($"\\Jan2021\\cap.csv", "Capital One Card")]
        [InlineData($"\\Jan2021\\unknown.csv", null)]
        public void CanDetectTransactionFileSource(string fileName, string expectedSource)
        {
            var sourceDetector = _serviceProvider.GetService<ITransactionFileSourceIdentifier>();

            try
            {
                var sourceDataFolder = _serviceProvider.GetService<ISettingsService>()!.SourcePath;
                var file =  new FileInfo($"{sourceDataFolder.FullName}{fileName}");
                var result = sourceDetector!.DetectTransactionSource(file);
                Assert.Equal(expectedSource, result.Name);
            }
            catch(Exception e)
            {
                Assert.True(expectedSource == null);
                Assert.Contains("Unable to find source for file", e.Message);
            }
        }
    }
}
