using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FinancialDucks.Tests.FeatureTests
{
    public class AccountBalanceFeatureTests :TestBase
    {
        [Theory]
        [InlineData("Bank A", "2022/1/1", "2022/6/1", 80000, 82500, TimeInterval.Monthly)]
        [InlineData("Bank A", "2021/1/1", "2021/6/1", 81600, 80000, TimeInterval.Monthly)]
        [InlineData("Bank A", "2024/1/1", "2024/6/1", 400000, 400000, TimeInterval.Monthly)]
        [InlineData("Bank A", "2022/1/1", "2022/3/1", 80000, 80400, TimeInterval.Daily)]
        [InlineData("Bank A,Bank B", "2022/1/1", "2024/1/1", 170000, 862600, TimeInterval.Quarterly)]
        public async Task CanCalculateAccountBalanceAtDate(string accountsCSV, string fromDate, string toDate, 
            decimal expectedFirstTotal, decimal expectedLastTotal, TimeInterval interval)
        {
            var serviceProvider = CreateServiceProvider();
            var mockDataHelper = serviceProvider.GetRequiredService<MockDataHelper>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var accountNames = accountsCSV.Split(',');
            var selectedSources = mockDataHelper
                .GetMockTransactionSources()
                .Where(t => accountNames.Contains(t.Name))
                .ToArray();

            foreach (var source in selectedSources)
            {
                mockDataHelper.MockSourceSnapshots.Add(
                    mockDataHelper.CreateMockSourceSnapshot(source, new DateTime(2022, 1, 1), source.Id * 10000));

                mockDataHelper.MockSourceSnapshots.Add(
                   mockDataHelper.CreateMockSourceSnapshot(source, new DateTime(2023, 1, 1), source.Id * 50000));
            }

            Assert.NotEmpty(selectedSources);

            //add transactions from 2021/1/1 to 2024
            foreach (var source in selectedSources)
                mockDataHelper.AddDebitAndCreditTransactions(source);

            var result = await mediator.Send(new AccountBalanceFeature.Query(
                    selectedSources,
                    DateTime.Parse(fromDate),
                    DateTime.Parse(toDate),
                    interval));

            Assert.Equal(expectedFirstTotal, result.First().Amount);
            Assert.Equal(expectedLastTotal, result.Last().Amount);
        }
    }
}
