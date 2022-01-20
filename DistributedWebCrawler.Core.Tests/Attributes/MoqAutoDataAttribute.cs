using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using System;

namespace DistributedWebCrawler.Core.Tests.Attributes
{
    internal class MoqAutoDataAttribute : AutoDataAttribute
    {
        public MoqAutoDataAttribute(ICustomization[] additionalCustomizations, bool configureMembers = false) 
            : base(GetFixtureFactory(additionalCustomizations, configureMembers))
        {
        }

        public MoqAutoDataAttribute(ICustomization additionalCustomization, bool configureMembers = false) 
            : this(new[] { additionalCustomization }, configureMembers)
        {

        }

        public MoqAutoDataAttribute(bool configureMembers = false) 
            : this(Array.Empty<ICustomization>(), configureMembers)
        {

        }

        private static Func<IFixture> GetFixtureFactory(ICustomization[] additionalCustomizations, bool configureMembers)
        {
            return () =>
            {
                var fixture = new Fixture();
                
                foreach (var customization in additionalCustomizations)
                {
                    fixture.Customize(customization);
                }

                fixture.Customize(new AutoMoqCustomization() 
                { 
                    ConfigureMembers = configureMembers
                });

                return fixture;
            };
        }
    }
}
