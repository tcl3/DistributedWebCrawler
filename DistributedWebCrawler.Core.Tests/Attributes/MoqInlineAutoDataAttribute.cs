using AutoFixture;
using AutoFixture.Xunit2;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class MoqInlineAutoDataAttribute : InlineAutoDataAttribute
    {
        public MoqInlineAutoDataAttribute(
            ICustomization[] additionalCustomizations,
            bool configureMembers = false,
            params object?[] values)
            : base(new MoqAutoDataAttribute(additionalCustomizations, configureMembers), values)
        {

        }

        public MoqInlineAutoDataAttribute(
            ICustomization additionalCustomization,
            bool configureMembers = false,
            params object?[] values)
            : base(new MoqAutoDataAttribute(additionalCustomization, configureMembers), values)
        {

        }

        public MoqInlineAutoDataAttribute(bool configureMembers = false, params object?[] values)
            : base(new MoqAutoDataAttribute(configureMembers), values)
        {

        }
    }
}
