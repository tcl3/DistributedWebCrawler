using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    public class ComponentNameProviderTests : ComponentNameProviderTests<TestComponent, TestSuccess, ErrorCode<TestFailure>>
    {

    }
    
    public abstract class ComponentNameProviderTests<TComponent, TSuccess, TFailure>
    {
        private static readonly IEnumerable<ComponentDescriptor> TestComponentDescriptors;

        private const string TestComponentName = "Test";
        private const string TestDefaultValue = "TestDefaultValue";

        static ComponentNameProviderTests()
        {
            TestComponentDescriptors = new ComponentDescriptor[]
            {
                new(typeof(TComponent), typeof(TSuccess), typeof(TFailure), TestComponentName),
            };
        }

        [Fact]
        public void GetComponentNameFromComponentTypeTest()
        {
            var sut = new ComponentNameProvider(TestComponentDescriptors);
            var result = sut.GetComponentName<TestComponent>();
            Assert.Equal(TestComponentName, result);
        }

        [Fact]
        public void GetComponentNameFromComponentTypeFailTest()
        {
            var sut = new ComponentNameProvider(TestComponentDescriptors);
            Assert.Throws<KeyNotFoundException>(() => sut.GetComponentName<object>());
        }

        [Fact]
        public void GetComponentNameFromTypeArgumentsTest()
        {
            var sut = new ComponentNameProvider(TestComponentDescriptors);
            var result = sut.GetComponentName(typeof(TSuccess), typeof(TFailure));
            Assert.Equal(TestComponentName, result);
        }

        [Fact]
        public void GetComponentNameFromTypeArgumentsFailTest()
        {
            var sut = new ComponentNameProvider(TestComponentDescriptors);
            Assert.Throws<KeyNotFoundException>(() => sut.GetComponentName(typeof(object), typeof(TFailure)));
            Assert.Throws<KeyNotFoundException>(() => sut.GetComponentName(typeof(TSuccess), typeof(object)));
        }

        [Fact]
        public void GetComponentNameFromTypeArgumentsOrDefaultTest()
        {
            var sut = new ComponentNameProvider(TestComponentDescriptors);

            var resultsList = new List<string>
            {
                sut.GetComponentNameOrDefault<object, TFailure>(() => TestDefaultValue),
                sut.GetComponentNameOrDefault<TSuccess, object>(() => TestDefaultValue),
                sut.GetComponentNameOrDefault<object, TFailure>(TestDefaultValue),
                sut.GetComponentNameOrDefault<TSuccess, object>(TestDefaultValue),
            };

            foreach (var result in resultsList)
            {
                Assert.Equal(TestDefaultValue, result);
            }

            var componentNameResult1 = sut.GetComponentNameOrDefault<TSuccess, TFailure>(() => TestDefaultValue);
            Assert.Equal(TestComponentName, componentNameResult1);

            var componentNameResult2 = sut.GetComponentNameOrDefault<TSuccess, TFailure>(TestDefaultValue);
            Assert.Equal(TestComponentName, componentNameResult2);
        }

        [Fact]
        public void GetComponentNameOrDefaultTest()
        {
            var sut = new ComponentNameProvider(TestComponentDescriptors);
            
            var defaultValueFromFactoryReult = sut.GetComponentNameOrDefault<object>(() => TestDefaultValue);
            Assert.Equal(TestDefaultValue, defaultValueFromFactoryReult);

            var defaultValueFromValueReult = sut.GetComponentNameOrDefault<object>(TestDefaultValue);
            Assert.Equal(TestDefaultValue, defaultValueFromValueReult);

            var componentNameResult = sut.GetComponentNameOrDefault<TestComponent>(() => TestDefaultValue);
            Assert.Equal(TestComponentName, componentNameResult);
        }

        [Fact]
        public void GetFromComponentTypeTest()
        {
            var sut = new ComponentNameProvider(TestComponentDescriptors);
            var result = sut.GetFromComponentType<TestComponent>();
            Assert.Equal(TestComponentDescriptors.First(), result);
        }

        [Fact]
        public void GetFromComponentTypeFailTest()
        {
            var sut = new ComponentNameProvider(TestComponentDescriptors);
            Assert.Throws<KeyNotFoundException>(() => sut.GetFromComponentType<object>());
        }
    }
}
