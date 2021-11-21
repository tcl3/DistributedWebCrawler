namespace DistributedWebCrawler.Core.Model
{
    public class IngestResult : RequestBase
    {
        public IngestResult() : base()
        {

        }

        public string Path { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
    }
}
