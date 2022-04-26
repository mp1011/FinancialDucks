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
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace FinancialDucks.Tests
{
    [SupportedOSPlatform("windows")]
    public class TestBase 
    {
        protected IServiceProvider CreateServiceProvider()
        {
            var builder = Host.CreateDefaultBuilder()
               .ConfigureServices(sc =>
               {
                   sc.AddSingleton<MockDataHelper>();
                   sc.AddMediatR(typeof(ReadLocalTransactions));
                   sc.AddSingleton<IEqualityComparer<ITransaction>, TransactionEqualityComparer>();
                   sc.AddSingleton<ITransactionReader, TransactionReader>();

                   sc.AddSingleton<ISettingsService>(_ =>
                   {
                       var mock = new Mock<ISettingsService>();
                       mock.Setup(x => x.SourcePath)
                           .Returns(() => GetTestFolder("TestData"));
                       mock.Setup(x => x.DownloadsFolder)
                           .Returns(() => GetTestFolder("TestData\\Downloads", clearFiles: true));

                       return mock.Object;
                   });

                   sc.AddSingleton<ITransactionFileSourceIdentifier, TransactionFileSourceIdentifier>();
                   sc.AddSingleton<ITransactionClassifier, TransactionClassifier>();
                   sc.AddSingleton<ITransactionsQueryBuilder, TransactionsQueryBuilder>();
                   sc.AddSingleton<NotificationDispatcher<CategoryChangeNotification>>();

                   sc.AddSingleton<ICategoryTreeProvider>(sp =>
                   {
                       var dispatcher = sp.GetRequiredService<NotificationDispatcher<CategoryChangeNotification>>();
                       var mock = new Mock<ICategoryTreeProvider>();
                       mock.Setup(x => x.GetCategoryTree())
                            .Returns(() => Task.FromResult(sp.GetRequiredService<MockDataHelper>().GetMockCategoryTree()));
                       var mockProvider = mock.Object;
                       return new CachedCategoryTreeProvider(mockProvider, dispatcher);
                   });

                   sc.AddSingleton(s =>
                   {
                       var mock = new Mock<IDataContextProvider>();
                       mock.Setup(r => r.CreateDataContext())
                            .Returns(() => s.GetService<IDataContext>()!);
                       return mock.Object;
                   });

                   sc.AddSingleton<IDataContext, MockDataContext>();
                   sc.AddSingleton<IScraperService, ScraperService>();
                   sc.AddSingleton<ISecureStringSaver, SecureStringSaver>();

                   sc.AddSingleton(s =>
                   {
                       var settingsService = s.GetRequiredService<ISettingsService>();
                       var mock = new Mock<IBrowserAutomationService>();                      
                       mock.Setup(x => x.CreateBrowser(It.IsAny<string>()))
                        .Returns<string>((s) => 
                            Task.FromResult(new MockBrowserAutomation(settingsService) { Url = s } as IBrowserAutomation));

                       return mock.Object;
                   });

               });

            return builder.Build().Services;
        }
    
    
        private DirectoryInfo GetTestFolder(string relativePath, bool clearFiles=false)
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location!)!);
            
            while(directory != null && directory.Name != "FinancialDucks.Tests")
                directory = directory.Parent;

            if (directory == null)
                throw new Exception("Unable to find app folder");

            directory = new DirectoryInfo($"{directory}\\{relativePath}");
            if(!directory.Exists)
                throw new Exception("Unable to find folder: " + relativePath);

            if(clearFiles)
            {
                var files = directory.GetFiles();
                foreach (var f in files)
                    f.Delete();
            }

            return directory;
        }
    }
}
