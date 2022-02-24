namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class RobotsAutodataAttribute : MoqInlineAutoDataAttribute
    {
        public RobotsAutodataAttribute(string? robotsContent, bool expectedResult, bool shouldInvokeCallback)
            : base(values: new object?[] { robotsContent, expectedResult, shouldInvokeCallback })
        {

        }
    }
}
