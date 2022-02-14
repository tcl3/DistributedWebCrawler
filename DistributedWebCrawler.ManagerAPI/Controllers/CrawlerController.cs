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
        public async Task StartComponent(ComponentFilter componentFilter)
        {
            await _crawlerManager.ResumeAsync(componentFilter);
        }

        [HttpPost]
        public async Task Start()
        {
            await _crawlerManager.ResumeAsync();
        }

        [HttpPost]
        public async Task StopComponent(ComponentFilter componentFilter)
        {
            await _crawlerManager.PauseAsync(componentFilter);
        }

        [HttpPost]
        public async Task Stop()
        {
            await _crawlerManager.PauseAsync();
        }


    }
}