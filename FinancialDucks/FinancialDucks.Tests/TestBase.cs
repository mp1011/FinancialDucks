using FinancialDucks.Application.Services;
using FinancialDucks.Tests.ServiceTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using MediatR;
using System;
using System.IO;
using System.Linq;
using FinancialDucks.Application.Features;
using System.Collections.Generic;
using FinancialDucks.Application.Models;
using System.Threading.Tasks;
using FinancialDucks.Tests.TestModels;
using FinancialDucks.Tests.CustomMocks;

namespace FinancialDucks.Tests
{
    public class TestBase
    {
        protected readonly IServiceProvider _serviceProvider;

        public TestBase()
        {
            var builder = Host.CreateDefaultBuilder()
               .ConfigureServices(sc =>
               {
                   sc.AddMediatR(typeof(ReadLocalTransactions));
                   sc.AddSingleton<IEqualityComparer<ITransaction>, TransactionEqualityComparer>();
                   sc.AddSingleton<ITransactionReader, TransactionReader>();
                   sc.AddSingleton<ISettingsService, SettingsService>();
                   sc.AddSingleton<ITransactionFileSourceIdentifier, TransactionFileSourceIdentifier>();
                   sc.AddSingleton<ITransactionClassifier, TransactionClassifier>();

                   sc.AddSingleton(_ =>
                   {
                       var mock = new Mock<ICategoryTreeProvider>();
                       mock.Setup(x => x.GetCategoryTree())
                            .Returns(() => Task.FromResult(MockDataHelper.GetMockCategoryTree()));
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

            _serviceProvider = builder.Build().Services;
        }
    }
}
