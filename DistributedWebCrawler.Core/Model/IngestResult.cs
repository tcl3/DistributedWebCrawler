using System;

namespace DistributedWebCrawler.Core.Model
{
    public class IngestResult : RequestBase
    {
        public IngestResult() : base()
        {

        }

        public string Path { get; init; } = string.Empty;
        public Guid? ContentId { get; init; } 
        public string MediaType { get; set; } = string.Empty;
    }
}
