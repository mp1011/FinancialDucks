using PuppeteerSharp;

namespace FinancialDucks.Application.Models
{
    public record PageElementsWithFrames(ElementHandle[] PageElements, FrameGroup[] FrameGroups)
    {
        public int TotalCount => PageElements.Length + FrameGroups.Sum(p => p.ElementHandles.Length);
    }

    public record FrameGroup(Frame Frame, ElementHandle[] ElementHandles);

}
