using FinancialDucks.Application.Extensions;
using MediatR;
using PuppeteerSharp;

namespace FinancialDucks.Application.Features
{
    public class WebTransactionExtractorFeature
    {
        public record Query : IRequest<FileInfo?>
        {

        }

        public class Handler : IRequestHandler<Query, FileInfo?>
        {
            public async Task<FileInfo?> Handle(Query request, CancellationToken cancellationToken)
            {
                var fetcher = new BrowserFetcher(new BrowserFetcherOptions
                {
                    Path = @"E:\Documents\monies\chr"
                });
                
                var revisionInfo = await fetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    ExecutablePath = revisionInfo.ExecutablePath,
                    Devtools = true,
                    Headless = false
                }); 

                try
                {
                    var page = await browser.NewPageAsync();
                    var response = await page.GoToAsync("xyz");

                    await page.FillInputAsync("input#username", "xyz");
                    await page.FillInputAsync("input#password", "xyz");
                    await page.ClickAndWaitForNavigateAsync("button#signInBtn");

                    var link = await page.WaitForInnerHTMLAsync("a", h => h.Contains("xyz"));
                    await page.ClickAndWaitForNavigateAsync(link);

                    await page.WaitAndClickAsync("#downloadTrans");

                    var downloadButton = await page.WaitForInnerHTMLAsync("button", h => h.Equals("Download"));
                    if (downloadButton != null)
                    {
                        var file = await downloadButton.ClickAndWaitForFileDownloadAsync(new DirectoryInfo("xyz"));
                        if (file != null)
                            return file;
                    }
                }
                catch(Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("!");
                }

                return null;
            }
        }
    }
}
