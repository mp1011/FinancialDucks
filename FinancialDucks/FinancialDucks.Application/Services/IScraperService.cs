using FinancialDucks.Application.Features;
using FinancialDucks.Application.Models;
using FinancialDucks.Application.Models.AppModels;
using MediatR;

namespace FinancialDucks.Application.Services
{
    public interface IScraperService
    {
        Task<List<FileInfo>> Execute(ITransactionSource source, IEnumerable<IScraperCommandDetail> commands, bool showBrowser, CancellationToken cancellationToken);
    }

    public class ScraperService : IScraperService
    {
        private readonly IMediator _mediator;
        private readonly IBrowserAutomationService _browserAutomationService;
        private readonly ISettingsService _settingsService;
        private readonly ISecureStringSaver _secureStringSaver;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(initialCount:1, maxCount:1);

        public ScraperService(IMediator mediator, IBrowserAutomationService browserAutomationService, ISettingsService settingsService, ISecureStringSaver secureStringSaver)
        {
            _browserAutomationService = browserAutomationService;
            _settingsService = settingsService;
            _secureStringSaver = secureStringSaver;
            _mediator = mediator;
        }

        public async Task<List<FileInfo>> Execute(ITransactionSource source, IEnumerable<IScraperCommandDetail> commands, bool showBrowser, CancellationToken cancellationToken)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                using var browser = await _browserAutomationService.CreateBrowser(source.Url,showBrowser);
                return await Execute(browser, source, commands, cancellationToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task<List<FileInfo>> Execute(IBrowserAutomation browser, ITransactionSource source, IEnumerable<IScraperCommandDetail> commands, CancellationToken cancellationToken)
        {
            var files = new List<FileInfo>();

            foreach(var command in commands
                .Where(p=>p.SourceId == source.Id)
                .OrderBy(p=>p.Sequence))
            {
                try
                {
                    await _mediator.Publish(new WebTransactionExtractorFeature.Notification(command, "Begin", DateTime.Now, Array.Empty<ScrapedElement>()));

                    if (command.TypeId == ScraperCommandType.ClickAndDownload)
                        files.Add(await ExecuteFileDownloadCommand(browser, command, cancellationToken));
                    else
                        await Execute(browser, command, cancellationToken);

                    await _mediator.Publish(new WebTransactionExtractorFeature.Notification(command, "Success", DateTime.Now, Array.Empty<ScrapedElement>()));

                }
                catch (Exception e)
                {
                    await _mediator.Publish(new WebTransactionExtractorFeature.Notification(command, $"Failure: {e.Message}", DateTime.Now, await browser.GetScrapedElements(command.Selector, command.SearchInnerText)));
                    break;
                }
            }

            return files; 
        }
        private async Task<FileInfo> ExecuteFileDownloadCommand(IBrowserAutomation browser, IScraperCommandDetail command, CancellationToken cancellationToken)
        {
            var requestTime = DateTime.Now;
            var downloadsFolder = _settingsService.DownloadsFolder;
            if (downloadsFolder.GetFiles().FirstOrDefault(p => p.LastWriteTime > requestTime) != null)
                throw new Exception("Downloads folder contains files with a future last modified date");

            var currentFiles = downloadsFolder.GetFiles();
            await browser.DoClick(command.Selector, command.SearchInnerText, cancellationToken);

            //todo, expect file with known pattern
            DateTime start=  DateTime.Now;
            while ((DateTime.Now - start).TotalSeconds <= 30)
            {
                var newFile = downloadsFolder.GetFiles()
                                             .FirstOrDefault(p => p.LastWriteTime > requestTime 
                                                               && !p.Name.Contains("unconfirmed", StringComparison.OrdinalIgnoreCase));
                if (newFile != null)
                    return newFile;
                else
                    await Task.Delay(500);
            }

            throw new Exception("Unable to download file");
        }

        private string GetUsername(ITransactionSource source)
        {
            return _secureStringSaver.GetUsername(source);
        }
        private string GetPassword(ITransactionSource source)
        {
            return _secureStringSaver.GetPassword(source);
        }

        private async Task Execute(IBrowserAutomation browser, IScraperCommandDetail command, CancellationToken cancellationToken)
        {
            var currentUrl = browser.Url;

            switch(command.TypeId)
            {
                case ScraperCommandType.Click:
                    await browser.DoClick(command.Selector, command.SearchInnerText, cancellationToken);
                    break;
                case ScraperCommandType.FillUsername:
                    await browser.FillText(command.Selector, GetUsername(command.Source), cancellationToken);
                    break;
                case ScraperCommandType.FillPassword:
                    await browser.FillText(command.Selector, GetPassword(command.Source), cancellationToken);
                    break;
                case ScraperCommandType.FillCurrentDate:
                    await browser.FillDate(command.Selector, DateTime.Now, cancellationToken);
                    break;
                case ScraperCommandType.FillPastDate:
                    await browser.FillDate(command.Selector, DateTime.Now.AddMonths(-3), cancellationToken);
                    break;
                case ScraperCommandType.GoBack:
                    await browser.GoBack();
                    break;
                case ScraperCommandType.Wait:
                    await browser.WaitFor(command.Selector, cancellationToken);
                    break;
            }

            if (command.WaitForNavigate)
                await browser.WaitForNavigate(currentUrl);
        }
    }
}
