using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;

namespace FinancialDucks.Application.Services
{
    public interface IScraperService
    {
        Task<FileInfo?> Execute(ITransactionSource source, IEnumerable<IScraperCommandDetail> commands);
    }

    public class ScraperService : IScraperService
    {
        private readonly IBrowserAutomationService _browserAutomationService;
        private readonly ISettingsService _settingsService;
        private readonly ISecureStringSaver _secureStringSaver;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(initialCount:1, maxCount:1);

        public ScraperService(IBrowserAutomationService browserAutomationService, ISettingsService settingsService, ISecureStringSaver secureStringSaver)
        {
            _browserAutomationService = browserAutomationService;
            _settingsService = settingsService;
            _secureStringSaver = secureStringSaver;
        }

        public async Task<FileInfo?> Execute(ITransactionSource source, IEnumerable<IScraperCommandDetail> commands)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                using var browser = await _browserAutomationService.CreateBrowser(source.Url);
                return await Execute(browser, source, commands);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task<FileInfo?> Execute(IBrowserAutomation browser, ITransactionSource source, IEnumerable<IScraperCommandDetail> commands)
        {
            foreach(var command in commands
                .Where(p=>p.SourceId == source.Id)
                .OrderBy(p=>p.Sequence))
            {
                if (command.TypeId == ScraperCommandType.ClickAndDownload)
                    return await ExecuteFileDownloadCommand(browser, command);
                else 
                    await Execute(browser, command);
            }

            return null;
        }

        private async Task<FileInfo> ExecuteFileDownloadCommand(IBrowserAutomation browser, IScraperCommandDetail command)
        {
            var requestTime = DateTime.Now;
            var downloadsFolder = _settingsService.DownloadsFolder;
            if (downloadsFolder.GetFiles().FirstOrDefault(p => p.LastWriteTime > requestTime) != null)
                throw new Exception("Downloads folder contains files with a future last modified date");

            var currentFiles = downloadsFolder.GetFiles();
            await browser.DoClick(command.Selector, command.SearchInnerText);

            //todo, timeout
            //todo, expect file with known pattern
            while (true)
            {
                var newFile = downloadsFolder.GetFiles().FirstOrDefault(p => p.LastWriteTime > requestTime);
                if (newFile != null)
                    return newFile;
                else
                    await Task.Delay(500);
            }
        }

        private string GetUsername(ITransactionSource source)
        {
            return _secureStringSaver.GetUsername(source);
        }
        private string GetPassword(ITransactionSource source)
        {
            return _secureStringSaver.GetPassword(source);
        }

        private async Task Execute(IBrowserAutomation browser, IScraperCommandDetail command)
        {
            var currentUrl = browser.Url;

            switch(command.TypeId)
            {
                case ScraperCommandType.Click:
                    await browser.DoClick(command.Selector, command.SearchInnerText);
                    break;
                case ScraperCommandType.FillUsername:
                    await browser.FillText(command.Selector, GetUsername(command.Source));
                    break;
                case ScraperCommandType.FillPassword:
                    await browser.FillText(command.Selector, GetPassword(command.Source));
                    break;
            }

            if (command.WaitForNavigate)
                await browser.WaitForNavigate(currentUrl);
        }
    }
}
