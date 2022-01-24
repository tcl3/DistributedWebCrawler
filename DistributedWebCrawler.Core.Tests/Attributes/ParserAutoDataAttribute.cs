using DistributedWebCrawler.Core.Tests.Customizations;
using System;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    internal class ParserAutoDataAttribute : MoqAutoDataAttribute
    {
        public ParserAutoDataAttribute(string[]? hyperlinks = null) 
            : base(new ParserRequestProcessorCustomization(hyperlinks), configureMembers: true)
        {
        }
    }
}
