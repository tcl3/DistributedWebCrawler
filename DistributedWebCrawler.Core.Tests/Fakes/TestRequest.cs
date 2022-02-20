using DistributedWebCrawler.Core.Models;
using System;

namespace DistributedWebCrawler.Core.Tests.Fakes
{
    public class TestRequest : RequestBase
    {
        public TestRequest(Uri uri) : base(uri)
        {
        }
    }
}
