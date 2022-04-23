using PuppeteerSharp;

namespace FinancialDucks.Application.Extensions
{
    public static class PuppeteerExtensions
    {
        public static async Task WaitForURLChange(this Page page, string oldUrl)
        {
            if (page.Url != oldUrl)
                return;

            await page.WaitForNavigationAsync();
        }

        public static async Task<FileInfo>? ClickAndWaitForFileDownloadAsync(this ElementHandle elementHandle, DirectoryInfo downloadsFolder)
        {
            var time = DateTime.Now;
            if (downloadsFolder.GetFiles().FirstOrDefault(p => p.LastWriteTime > time) != null)
                throw new Exception("Downloads folder contains files with a future last modified date");

            var currentFiles = downloadsFolder.GetFiles();
            await elementHandle.ClickAsync();
            
            //todo, timeout
            while(true)
            {
                var newFile = downloadsFolder.GetFiles().FirstOrDefault(p => p.LastWriteTime > time);
                if (newFile != null)
                    return newFile;
                else
                    await Task.Delay(500);
            }
        }


        public static async Task WaitAndClickAsync(this Page page, string selector)
        {
            var element = await page.WaitForSelectorAsync(selector);
            await element.ClickAsync();
        }

        public static async Task ClickAndWaitForNavigateAsync(this Page page, string buttonSelector)
        {
            var url = page.Url;
            var btn = await page.WaitForSelectorAsync(buttonSelector);
            await btn.ClickAsync();
            await page.WaitForURLChange(url);
        }

        public static async Task ClickAndWaitForNavigateAsync(this Page page, ElementHandle element)
        {
            var url = page.Url;
            await element.ClickAsync();
            await page.WaitForURLChange(url);
        }


        public static async Task FillInputAsync(this Page page, string selector, string text)
        {
            var input = await page.WaitForSelectorAsync(selector);
            await input.FocusAsync();
            await page.Keyboard.TypeAsync(text);
        }

        public static async Task<ElementHandle> WaitForInnerHTMLAsync(this Page page, string selector, Predicate<string> predicate)
        {
            //todo, timeout
            while (true)
            {
                try
                {
                    var elements = await page.QuerySelectorAllAsync(selector);
                    foreach (var element in elements)
                    {
                        var html = await page.EvaluateFunctionAsync<string>("e=>e.innerHTML", element);
                        if (predicate(html))
                            return element;
                    }
                }
                catch
                {
                }
            }
        }
    }
}
