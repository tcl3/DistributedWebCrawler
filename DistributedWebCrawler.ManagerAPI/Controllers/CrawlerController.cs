using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DistributedWebCrawler.ManagerAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CrawlerController : ControllerBase
    {
        private readonly ICrawlerManager _crawlerManager;
        private readonly ILogger<CrawlerController> _logger;

        public CrawlerController(ICrawlerManager crawlerManager, ILogger<CrawlerController> logger)
        {
            _crawlerManager = crawlerManager;
            _logger = logger;
        }
        
        [HttpPost]
        public async Task Start(ComponentFilter? componentFilter = null)
        {
            componentFilter ??= ComponentFilter.MatchAll;
            await _crawlerManager.ResumeAsync(componentFilter);
        }

        [HttpPost]
        public async Task Stop(ComponentFilter? componentFilter = null)
        {
            componentFilter ??= ComponentFilter.MatchAll;
            await _crawlerManager.PauseAsync(componentFilter);
        }

        
    }
}