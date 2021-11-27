namespace DistributedWebCrawler.Core.Robots
{
    public class RobotsCacheSettings
    {
        public string KeyPrefix { get; init; } = "RobotsTxt";
        public string? UserAgent { get; init; }

    }
}
