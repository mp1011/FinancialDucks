using FinancialDucks.Application.Extensions;
using FinancialDucks.Application.Models.AppModels;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace FinancialDucks.Application.Services
{
    public interface IBrowserAutomation : IDisposable
    {
        string Url { get; }

        Task<ScrapedElement[]> GetScrapedElements(string selector, string text);
        Task WaitForNavigate(string originalUrl);
        Task DoClick(string selector, string searchText, CancellationToken cancellationToken);
        Task FillText(string selector, string text, CancellationToken cancellationToken);
        Task FillDate(string selector, DateTime date, CancellationToken cancellationToken);
        Task SelectDropdownText(string selector, string text, CancellationToken cancellationToken);
        Task GoBack();
        Task WaitFor(string selector, CancellationToken cancellationToken);
    }

    public interface IBrowserAutomationService
    {
        Task<IBrowserAutomation> CreateBrowser(string url, bool showBrowser);
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

        public async Task DoClick(string selector, string searchText, CancellationToken cancellationToken)
        {
            ElementHandle element;

            if (searchText.IsNullOrEmpty())
                element = await _page.WaitForSelectorAsyncEx(selector, cancellationToken, timeout:true)
                                     .HandleError(e=>OnSelectorFail(e,selector));
            else
                element = await WaitForInnerHTMLAsync(selector, searchText, cancellationToken, TimeSpan.FromSeconds(30));

            await element.ClickAsync()
                            .HandleError(e => OnSelectorFail(e, selector));
        }


        private async Task OnSelectorFail(Exception e, string selector)
        {
            try
            {
                var content = await _page.GetContentAsync();

                //var iFrames = await _page.QuerySelectorAllAsync("iframe");
                //var iFrameContent = await Task.WhenAll(iFrames.Select(async i =>
                //{
                //    var content = await i.ContentFrameAsync();
                //    if(content == null)
                //        return null;

                //    return await content.GetContentAsync();
                //}));

                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            catch
            {

            }

            e.Rethrow();            
        }



        private async Task<ElementHandle> WaitForInnerHTMLAsync(string selector, string match, CancellationToken cancellationToken, TimeSpan timeout)
        {
            var start = DateTime.Now;
            while ((DateTime.Now-start) < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var elements = await _page.QuerySelectorAllAsyncEx(selector);
                    foreach (var element in elements.PageElements)
                    {
                        bool isVisible = await element.CheckVisible();
                        if (!isVisible)
                            continue;

                        var html = await element.GetInnerHTML(_page);
                        if (html != null && html.Trim().WildcardMatch(match))
                            return element;
                    }

                    foreach(var frame in elements.FrameGroups)
                    {
                        foreach(var element in frame.ElementHandles)
                        {
                            bool isVisible = await element.CheckVisible();                         
                            var html = await element.GetInnerHTML(frame.Frame);

                            if (!isVisible)
                                continue;

                            if (html != null && html.Trim().WildcardMatch(match))
                                return element;
                        }
                    }
                }
                catch
                {
                }
            }

            throw new Exception($"Unable to find element {selector} with text {match} ");
        }

        public async Task FillText(string selector, string text, CancellationToken cancellationToken)
        {
            var input = await _page.WaitForSelectorAsyncEx(selector, cancellationToken, timeout:true)
                                   .HandleError(e => OnSelectorFail(e, selector));

            var inputs = await _page.QuerySelectorAllAsyncEx(selector);
            if(inputs.TotalCount > 1)
            {
                var ex = new Exception($"More than one element matches the selector {selector}");
                await OnSelectorFail(ex, selector);
                throw ex;
            }

            await input.FocusAsync();
            await _page.Keyboard.DownAsync(Key.Shift);
            await _page.Keyboard.PressAsync(Key.Home);
            await _page.Keyboard.UpAsync(Key.Shift);
            await _page.Keyboard.PressAsync(Key.Delete);
            await _page.Keyboard.TypeAsync(text);
        }

        public async Task FillDate(string selector, DateTime date, CancellationToken cancellationToken)
        {
            var dateStr = date.ToString("MM/dd/yyyy");
            await FillText(selector, dateStr,cancellationToken);
            await _page.Keyboard.PressAsync(Key.Tab.ToString());
        }


        public async Task WaitForNavigate(string originalUrl)
        {
            if (_page.Url != originalUrl)
                return;

            await _page.WaitForNavigationAsync();
        }

        public async Task<ScrapedElement[]> GetScrapedElements(string selector, string searchText)
        {
          
            var elements = await _page.QuerySelectorAllAsync(selector);
            List<ScrapedElement> scrapedElements = new List<ScrapedElement>();

            foreach(var element in elements)
            {
                var innerHTML = await _page.EvaluateFunctionAsync<string>("e=>e.innerHTML", element);
                var outerHTML= await _page.EvaluateFunctionAsync<string>("e=>e.outerHTML", element);   
                
                if(searchText.IsNullOrEmpty() || innerHTML.WildcardMatch(searchText))
                    scrapedElements.Add(new ScrapedElement(outerHTML, innerHTML));
            }

            return scrapedElements.ToArray();

        }


        public async Task GoBack()
        {
            await _page.GoBackAsync();
        }

        public async Task WaitFor(string selector, CancellationToken cancellationToken)
        {
            while(true)
            {
                try
                {
                    var result = await _page.WaitForSelectorAsyncEx(selector, timeout: false, cancellationToken: cancellationToken);
                    if (result != null)
                        return;
                }
                catch
                {

                }
            }
        }

        public async Task SelectDropdownText(string selector, string text, CancellationToken cancellationToken)
        {
            var start = DateTime.Now;

            Exception? lastError = null;

            while ((DateTime.Now - start).TotalSeconds <= 60)
            {
                //standard select
                try
                {
                    await _page.SelectAsync(selector, text);
                    return;
                }
                catch { }

                //select within frame
                try
                {

                    var iFrames = await _page.QuerySelectorAllAsync("iframe");
                    var iFrameContent = await Task.WhenAll(iFrames.Select(i => i.ContentFrameAsync()));
                    foreach (var content in iFrameContent.NullToEmpty())
                    {
                        if (content == null) continue;
                        if (content.Url.IsNullOrEmpty())
                            continue;

                        try
                        {
                            await content.SelectAsync(selector, text);
                            return;
                        }
                        catch { }
                    }
                }
                catch
                {

                }


                //non-select dropdowns

                try
                {
                    ElementHandle? element = await _page.WaitForSelectorAsyncEx(selector, cancellationToken, timeout: true)
                                                        .HandleError(e => Task.FromResult(null as ElementHandle));

                    if (element != null)
                    {
                        await element.ClickAsync();

                        var itemElement = await WaitForInnerHTMLAsync("span,option", text, cancellationToken, TimeSpan.FromSeconds(5))
                                                .HandleError(e => Task.FromResult(null as ElementHandle));

                        if (itemElement != null)
                        {
                            await itemElement.ClickAsync();
                            return;
                        }
                    }
                }
                catch(Exception e)
                {
                    lastError = e;
                }
            }

            throw lastError ?? new Exception($"Unable to select dropdown text {text}");


        }
    }

    public class PuppeteerService : IBrowserAutomationService
    {
        private readonly ISettingsService _settingsService;

        public PuppeteerService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<IBrowserAutomation> CreateBrowser(string url, bool showBrowser)
        {
            var fetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = _settingsService.DownloadsFolder.FullName
            });

            var revisionInfo = await fetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = revisionInfo.ExecutablePath,
                Devtools = false,
                Headless = !showBrowser
            });


            var page = await browser.NewPageAsync();
            await page.GoToAsync(url);

            return new PuppeteerBrowser(browser, page);
        }

       
    }
}

              

