using FinancialDucks.Application.Extensions;
using PuppeteerSharp;

namespace FinancialDucks.Application.Services
{
    public interface IBrowserAutomation : IDisposable
    {
        string Url { get; }

        Task WaitForNavigate(string originalUrl);
        Task DoClick(string selector, bool searchInnerText);
        Task FillText(string selector, string text);
    }

    public interface IBrowserAutomationService
    {
        Task<IBrowserAutomation> CreateBrowser(string url);
    }

    public class PuppeteerBrowser : IBrowserAutomation
    {
        private readonly Browser _browser;
        private readonly Page _page;

        public PuppeteerBrowser(Browser browser, Page page)
        {
            _browser = browser;
            _page = page;
        }

        public string Url => _page.Url;

        public void Dispose()
        {
            _page.Dispose();
            _browser.Dispose();
        }

        public async Task DoClick(string selector, bool searchInnerText)
        {
            ElementHandle element;

            var parts = selector.Split('|');
            if (!searchInnerText || parts.Length != 2)            
                element = await _page.WaitForSelectorAsync(selector);
            else
                element = await WaitForInnerHTMLAsync(parts[0], parts[1]);
        
            await element.ClickAsync();
        }

        private async Task<ElementHandle> WaitForInnerHTMLAsync(string selector, string match)
        {
            //todo, timeout
            while (true)
            {
                try
                {
                    var elements = await _page.QuerySelectorAllAsync(selector);
                    foreach (var element in elements)
                    {
                        var html = await _page.EvaluateFunctionAsync<string>("e=>e.innerHTML", element);
                        if (html != null &&  html.Trim().Equals(match, StringComparison.CurrentCultureIgnoreCase))
                            return element;
                    }
                }
                catch
                {
                }
            }
        }

        public async Task FillText(string selector, string text)
        {
            var input = await _page.WaitForSelectorAsync(selector);
            await input.FocusAsync();
            await _page.Keyboard.TypeAsync(text);
        }

        public async Task WaitForNavigate(string originalUrl)
        {
            if (_page.Url != originalUrl)
                return;

            await _page.WaitForNavigationAsync();
        }
    }

    public class PuppeteerService : IBrowserAutomationService
    {
        private readonly ISettingsService _settingsService;

        public PuppeteerService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<IBrowserAutomation> CreateBrowser(string url)
        {
            var fetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = _settingsService.DownloadsFolder.FullName
            });

            var revisionInfo = await fetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            //todo, change to headless
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = revisionInfo.ExecutablePath,
                Devtools = true,
                Headless = false
            });


            var page = await browser.NewPageAsync();
            await page.GoToAsync(url);

            return new PuppeteerBrowser(browser, page);
        }

       
    }
}

              

