using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models.AppModels;
using System;
using Xunit;

namespace FinancialDucks.Tests.ExtensionTests
{
    public class DateExtensionTests
    {
        [Theory]
        [InlineData("2/1/2022", TimeInterval.Daily, "2/1/2022")]
        [InlineData("2/1/2022", TimeInterval.Quarterly, "1/1/2022")]
        [InlineData("11/15/2022", TimeInterval.Quarterly, "10/1/2022")]
        [InlineData("4/13/2022", TimeInterval.Weekly, "4/10/2022")]
        [InlineData("4/13/2022", TimeInterval.Monthly, "4/1/2022")]
        public void CanGetClosestInterval(string dateStr, TimeInterval interval, string expectedDateStr)
        {
            var date = DateTime.Parse(dateStr);
            var expectedDate = DateTime.Parse(expectedDateStr);

            var result = date.GetClosestInterval(interval);
            Assert.Equal(expectedDate, result);
        }
    }
}
