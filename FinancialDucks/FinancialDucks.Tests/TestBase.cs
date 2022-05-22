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
    public class TestBase : IDisposable
    {
        private Guid _testId;
        private DirectoryInfo? _testFolder;

        public TestBase()
        {
            _testId = Guid.NewGuid();
        }

        

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
                           .Returns(() => CreateTestDataFolder());
                       mock.Setup(x => x.DownloadsFolder)
                           .Returns(() => CreateDownloadsFolder());

                       return mock.Object;
                   });

                   sc.AddSingleton<ITransactionFileSourceIdentifier, TransactionFileSourceIdentifier>();
                   sc.AddSingleton<ITransactionClassifier, TransactionClassifier>();
                   sc.AddSingleton<ITransactionsQueryBuilder, TransactionsQueryBuilder>();
                   sc.AddSingleton<NotificationDispatcher<CategoryChangeNotification>>();
                   sc.AddSingleton<NotificationDispatcher<WebTransactionExtractorFeature.Notification>>();

                   sc.AddSingleton<IExcelToCSVConverter, Word97ExcelConverter>();

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
                       mock.Setup(x => x.CreateBrowser(It.IsAny<string>(),It.IsAny<bool>()))
                        .Returns<string,bool>((s,b) => 
                            Task.FromResult(new MockBrowserAutomation(settingsService) { Url = s } as IBrowserAutomation));

                       return mock.Object;
                   });

               });

            return builder.Build().Services;
        }
    
    
        private DirectoryInfo CreateTestDataFolder()
        {
            if (_testFolder != null)
                return _testFolder;

            var srcData = GetSourceTestDataFolder();
            _testFolder = new DirectoryInfo($"{srcData.FullName}_{_testId}");
            if (!_testFolder.Exists)
                _testFolder.Create();

            return _testFolder;
        }


        private DirectoryInfo CreateDownloadsFolder()
        {
            var testDataFolder = CreateTestDataFolder();
            var downloads = new DirectoryInfo($"{testDataFolder.FullName}\\Downloads");
            downloads.Create();
            return downloads;
        }

        private DirectoryInfo GetSourceTestDataFolder()
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location!)!);

            while (directory != null && directory.Name != "FinancialDucks.Tests")
                directory = directory.Parent;

            if (directory == null)
                throw new Exception("Unable to find app folder");

            directory = new DirectoryInfo($"{directory}\\TestData");
            return directory;
        }

        protected void CopyTestFiles(string relativePath)
        {
            var src = GetSourceTestDataFolder();
            src = new DirectoryInfo($"{src.FullName}\\{relativePath}");

            foreach(var file in src.GetFiles("*.*", SearchOption.AllDirectories))
            {
                var filePath = file.FullName.Substring(file.FullName.IndexOf("TestData") + 8);
                GetTestFile(filePath);
            }
        }

        protected FileInfo GetTestFile(string relativePath)
        {
            var sourceDataFolder = GetSourceTestDataFolder();
            if(_testFolder == null)
                _testFolder = new DirectoryInfo($"{sourceDataFolder.FullName}_{_testId}");

            var srcFile = new FileInfo($"{sourceDataFolder.FullName}{relativePath}");
            var testFile = new FileInfo(srcFile.FullName.Replace("\\TestData\\", $"\\TestData_{_testId}\\"));

            if (!testFile.Directory!.Exists)
                testFile.Directory.Create();

            srcFile.CopyTo(testFile.FullName);
            return testFile;
        }

        public void Dispose()
        {
            try
            {
                if (_testFolder != null && _testFolder.Exists)
                    _testFolder.Delete(recursive: true);
            }
            catch
            {

            }
        }
    }
}
