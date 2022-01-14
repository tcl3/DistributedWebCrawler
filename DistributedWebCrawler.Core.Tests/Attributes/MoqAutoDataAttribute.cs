using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using System;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class MoqAutoDataAttribute : AutoDataAttribute
    {
        public MoqAutoDataAttribute(ICustomization[] additionalCustomizations) : base(GetFixtureFactory(additionalCustomizations))
        {

        }

        public MoqAutoDataAttribute(ICustomization additionalCustomization) : this(new[] { additionalCustomization })
        {

        }

        public MoqAutoDataAttribute() : this(Array.Empty<ICustomization>())
        {

        }

        private static Func<IFixture> GetFixtureFactory(ICustomization[] additionalCustomizations)
        {
            return () =>
            {
                var fixture = new Fixture();
                
                foreach (var customization in additionalCustomizations)
                {
                    fixture.Customize(customization);
                }

                fixture.Customize(new AutoMoqCustomization());

                return fixture;
            };
        }
    }
}
