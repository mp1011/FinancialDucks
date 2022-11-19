using FinancialDucks.Application.Models.AppModels;
using FinancialDucks.Application.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FinancialDucks.Tests.CustomMocks
{
    public class MockBrowserAutomation : IBrowserAutomation
    {
        private readonly ISettingsService _settingsService;

        public MockBrowserAutomation(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public string Url { get; set; } = "";

        public void Dispose()
        {
        }
        public Task SelectDropdownText(string selector, string text, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task DoClick(string selector, string searchInnerText, CancellationToken cancellationToken)
        {
            Url += "x";

            if (selector == "download")
            {
                var downloadsFolder = _settingsService.DownloadsFolder;
                File.WriteAllText(downloadsFolder.FullName + "\\test.txt", DateTime.Now.ToLongDateString());
            }

            return Task.CompletedTask;
        }

        public Task FillDate(string selector, DateTime date, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task FillText(string selector, string text, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<ScrapedElement[]> GetScrapedElements(string selector, string searchInnerText)
        {
            throw new NotImplementedException();
        }

        public Task GoBack()
        {
            throw new NotImplementedException();
        }

        public Task WaitFor(string selector, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task WaitForNavigate(string originalUrl)
        {
            return Task.CompletedTask;
        }

        public Task NavigateTo(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task KeyPress(string keys, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
