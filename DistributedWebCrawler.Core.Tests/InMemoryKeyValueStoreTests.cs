using DistributedWebCrawler.Core.Tests.Collections;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DistributedWebCrawler.Core.Tests
{
    [Collection(nameof(SystemClockDependentCollection))]
    public class InMemoryKeyValueStoreTests
    {
        private const string TestKey = "key";
        private const string TestStringValue = "value";
        private static readonly Guid TestObjectValue = Guid.NewGuid();

        [Fact]
        public async Task PutStringWithGet()
        {
            var sut = new InMemoryKeyValueStore();
            await sut.PutAsync(TestKey, TestStringValue);
            var result = await sut.GetAsync(TestKey);
            Assert.Equal(TestStringValue, result);
        }

        [Fact]
        public async Task PutObjectWithGet()
        {
            var sut = new InMemoryKeyValueStore();
            await sut.PutAsync(TestKey, TestObjectValue);
            var result = await sut.GetAsync<Guid>(TestKey);
            Assert.Equal(TestObjectValue, result);
        }

        [Fact]
        public async Task GetStringWithoutPut()
        {
            var sut = new InMemoryKeyValueStore();
            var result = await sut.GetAsync("nonExistantKey");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetObjectWithoutPut()
        {
            var sut = new InMemoryKeyValueStore();
            var result = await sut.GetAsync<Guid>("nonExistantKey");

            Assert.Equal(default(Guid), result);
        }

        [Fact]
        public async Task GetStringAfterExpired()
        {
            var fixedTime = DateTimeOffset.Now;
            var sut = new InMemoryKeyValueStore();
            try
            {
                SystemClock.DateTimeOffsetNow = () => fixedTime;
            
                await sut.PutAsync(TestKey, TestStringValue, TimeSpan.FromMilliseconds(1));

                var result = await sut.GetAsync(TestKey);
                Assert.Equal(TestStringValue, result);

                SystemClock.DateTimeOffsetNow = () => fixedTime.AddMilliseconds(2);
                var expiredResult = await sut.GetAsync(TestKey);
                
                Assert.Null(expiredResult);
            }
            finally
            {
                SystemClock.Reset();
            }
        }

        [Fact]
        public async Task GetObjectAfterExpired()
        {
            var fixedTime = DateTimeOffset.Now;
            var sut = new InMemoryKeyValueStore();
            try
            {
                SystemClock.DateTimeOffsetNow = () => fixedTime;
                
                await sut.PutAsync(TestKey, TestObjectValue, TimeSpan.FromMilliseconds(1));

                var result = await sut.GetAsync<Guid>(TestKey);
                Assert.Equal(TestObjectValue, result);

                SystemClock.DateTimeOffsetNow = () => fixedTime.AddMilliseconds(2);

                var expiredResult = await sut.GetAsync<Guid>(TestKey);

                Assert.Equal(default(Guid), expiredResult);
            }
            finally
            {
                SystemClock.Reset();
            }
            
        }

        [Fact]
        public async Task GetObjectTypeMismatch()
        {
            var sut = new InMemoryKeyValueStore();
            await sut.PutAsync(TestKey, TestObjectValue);
            
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.GetAsync(TestKey));
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.GetAsync<int>(TestKey));
        }

        [Fact]
        public async Task RemoveAfterPut()
        {
            var sut = new InMemoryKeyValueStore();
            await sut.PutAsync(TestKey, TestStringValue);

            await sut.RemoveAsync(TestKey);

            var removedResult = await sut.GetAsync(TestKey);

            Assert.Null(removedResult);
        }

        [Fact]
        public async Task RemoveNonExistantKey()
        {
            var sut = new InMemoryKeyValueStore();
            await sut.RemoveAsync(TestKey);
        }
    }
}
