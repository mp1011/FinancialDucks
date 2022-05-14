using PuppeteerSharp;

namespace FinancialDucks.Application.Models
{
    public record PageElementsWithFrames(ElementHandle[] PageElements, FrameGroup[] FrameGroups);

    public record FrameGroup(Frame Frame, ElementHandle[] ElementHandles);
}
