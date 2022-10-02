# Distributed Web Crawler

This is a web crawler implementation in C#. The main design goal for this project is to maximize throughput by making it highly horizontally scalable.

This is a work in progress. Below is a list of features currently being implemented.

**Current progress**

The project currently only contains a console application `DistributedWebCrawler.Console`, which is used for testing. It can be configured by modifying `appsettings.json`.

**Features**
- [ ] Crawl multiple domains in parallel
  - [x] Download content from multiple URIs in parallel
  - [ ] Parse links from multiple URIs in parallel
- [x] Rate limiting, to prevent the same domain getting hammered with requests
- [x] Should respect robots.txt
- [x] Pause/resume crawling
- [ ] View/download crawl results
- [ ] Limit the pages that get crawled based on:
  - [x] Crawl depth
  - [ ] A list of included/excluded domains

**Config settings**

This is the default `appsettings.json` with added comments.

```javascript
{
  // The seeder will add a list of domains to crawl when the crawler first starts
  "SeederSettings": {
    // Can be set to either ReadFromConfig or ReadFromFile
    "Source": "ReadFromConfig"
    // These domains will be crawled if Source is set to ReadFromConfig
    "UrisToCrawl": [ "http://google.com/", "http://youtube.com/" ],
    // A list of domains from the file at this location will be used if Source is set to ReadFromFile
    "FilePath": "./path/to/file",
  },
  "IngesterSettings": {
    // Don't download links larger than this number of bytes
    "MaxContentLengthBytes": 1048576,
    // The maximum number of domains that can be downloaded from in parallel
    "MaxDomainsToCrawl": 50,
    // The number of HTTP 3xx redirects that will be followed before failing
    "MaxRedirects": 20
  },

  "SchedulerSettings": {
    // The maximum depth of links to follow from the initially requested page
    "MaxCrawlDepth": 5,
    // The maximum number of concurrent requests that can be made for robots.txt files
    "MaxConcurrentRobotsRequests": 50,
    // Will respect Allow, Disallow and CrawlDelay directives in robots.txt if set to true
    "RespectsRobotsTxt": true,
    // The number of milliseconds to wait betwen crawling pages from the same domain
    // this will be overridden by a robots.txt crawl delay directive if it exists
    "SameDomainCrawlDelayMillis": 5000
  },

  "ParserSettings": {
    // The maximum number of threads to use to parse links from HTML documents
    // NB: This currently cannot use more than one thread
    "MaxConcurrentThreads": 1
  },

  "RobotsTxtSettings": {
    // The number of seconds to cache robots.txt (on failure to download robots.txt)
    // NB: 86400 seconds = 24 hours
    "CacheIntervalSeconds": 86400
  },

  // The settings used by the HttpClient used to download content
  "CrawlerClientSettings": {
    // The User-Agent header will be set to this
    "UserAgentString": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.54 Safari/537.36",
    // The Accept-Language header will be set to this
    "AcceptLanguage": "en-US;q=0.9,en;q=0.8",
    // The HttpClient will close the connection after this long
    "TimeoutSeconds": 30
  }
}
```

Docker command to run manager only (from this directory) something like: `docker build -t test123/test -f .\DistributedWebCrawler.ManagerAPI\Dockerfile . && docker run -dt -p 49250:80 --name DistributedWebCrawler.ManagerAPI1 test123/test`
Otherwise `docker-compose up` to run everything