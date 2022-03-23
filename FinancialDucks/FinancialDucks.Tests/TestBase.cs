using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Services;
using FinancialDucks.Tests.CustomMocks;
using FinancialDucks.Tests.ServiceTests;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FinancialDucks.Tests
{
    public class TestBase 
    {
        private bool disposedValue;

        protected IServiceProvider CreateServiceProvider()
        {
            var builder = Host.CreateDefaultBuilder()
               .ConfigureServices(sc =>
               {
                   sc.AddSingleton<MockDataHelper>();
                   sc.AddMediatR(typeof(ReadLocalTransactions));
                   sc.AddSingleton<IEqualityComparer<ITransaction>, TransactionEqualityComparer>();
                   sc.AddSingleton<ITransactionReader, TransactionReader>();
                   sc.AddSingleton<ISettingsService, SettingsService>();
                   sc.AddSingleton<ITransactionFileSourceIdentifier, TransactionFileSourceIdentifier>();
                   sc.AddSingleton<ITransactionClassifier, TransactionClassifier>();

                   sc.AddSingleton(sp =>
                   {
                       var mock = new Mock<ICategoryTreeProvider>();
                       mock.Setup(x => x.GetCategoryTree())
                            .Returns(() => Task.FromResult(sp.GetRequiredService<MockDataHelper>().GetMockCategoryTree()));
                       return mock.Object;
                   });

                   sc.AddSingleton(s =>
                   {
                       var mock = new Mock<IDataContextProvider>();
                       mock.Setup(r => r.CreateDataContext())
                            .Returns(() => s.GetService<IDataContext>()!);
                       return mock.Object;
                   });

                   sc.AddSingleton<IDataContext, MockDataContext>();
                  
               })
               .ConfigureAppConfiguration(cb =>
               {
                   var path = new FileInfo(typeof(TransactionReaderTests).Assembly.Location).Directory;
                   cb.AddJsonFile($@"{path!.FullName}\appsettings.json");
               });

            return builder.Build().Services;
        }
    }
}
