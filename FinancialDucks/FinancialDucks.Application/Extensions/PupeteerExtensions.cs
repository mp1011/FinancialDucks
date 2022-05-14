using FinancialDucks.Application.Models;
using PuppeteerSharp;

namespace FinancialDucks.Application.Extensions
{
    public static class PupeteerExtensions
    {



        public static async Task<string> GetInnerHTML(this ElementHandle? element, Page page)
        {
            if (element == null)
                return string.Empty;

            try
            {
                return await page.EvaluateFunctionAsync<string>("e=>e.innerHTML", element);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task<string> GetInnerHTML(this ElementHandle? element, Frame frame)
        {
            if (element == null)
                return string.Empty;

            try
            {
                return await frame.EvaluateFunctionAsync<string>("e=>e.innerHTML", element);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static async Task<bool> CheckVisible(this ElementHandle element)
        {
            var boundingBox = await element.BoundingBoxAsync();
            return boundingBox != null && boundingBox.Height > 0;
        }


        public static async Task<PageElementsWithFrames> QuerySelectorAllAsyncEx(this Page page, string selector)
        {
            var elements = await page.QuerySelectorAllAsync(selector);

            List<FrameGroup> frames = new List<FrameGroup>();

            try
            {

                var iFrames = await page.QuerySelectorAllAsync("iframe");
                var iFrameContent = await Task.WhenAll(iFrames.Select(i => i.ContentFrameAsync()));
                foreach (var content in iFrameContent.NullToEmpty())
                {
                    if (content == null) continue;

                    var frameResult = await content.QuerySelectorAllAsync(selector);
                    if (frameResult != null)
                    {
                        frames.Add(new FrameGroup(content, frameResult));
                    }
                }
            }
            catch
            {

            }

            return new PageElementsWithFrames(elements, frames.ToArray());
        }

        public static async Task<ElementHandle> WaitForSelectorAsyncEx(this Page page, string selector,CancellationToken cancellationToken, bool timeout)
        {
            Exception lastException=null;
            DateTime start = DateTime.Now;
            while((DateTime.Now-start).TotalSeconds <= 30 || !timeout)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException($"Task cancelled whil waiting for selector {selector}");

                try
                {
                    var result = await page.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = 5000 });
                    if (result != null)
                        return result;
                }
                catch (Exception e)
                {
                    lastException = e;
                }

                try
                {

                    var iFrames = await page.QuerySelectorAllAsync("iframe");
                    var iFrameContent = await Task.WhenAll(iFrames.Select(i => i.ContentFrameAsync()));
                    foreach (var content in iFrameContent.NullToEmpty())
                    {
                        if (content == null) continue;

                        var result = await content.QuerySelectorAsync(selector);
                        if (result != null)
                            return result;
                    }
                }
                catch
                {

                }
            }

            throw lastException ?? new Exception($"Unable to find element {selector}");
        }
    }
}
