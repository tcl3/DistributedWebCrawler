using System;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DistributedWebCrawler.Core.Compontents
{
    public abstract class AbstractComponent : ICrawlerComponent
    {
        private readonly ILogger _logger;

        protected bool IsStarted { get; private set; }

        private readonly TaskCompletionSource _taskCompletionSource;
        
        protected AbstractComponent(ILogger logger, string name)
        {
            _logger = logger;
            Name = name;
            _taskCompletionSource = new TaskCompletionSource();
        }

        public CrawlerComponentStatus Status
        {
            get
            {
                if (!IsStarted)
                {
                    return CrawlerComponentStatus.NotStarted;
                }

                return GetStatus();
            }
        }

        protected abstract CrawlerComponentStatus GetStatus();

        public string Name { get; }

        public Task StartAsync(CrawlerStartState startState = CrawlerStartState.Running)
        {
            if (IsStarted)
            {
                throw new InvalidOperationException($"Cannot start {Name} when already started");
            }

            IsStarted = true;

            _logger.LogInformation($"{Name} component started");

            _ = ComponentStartAsync(startState)
                .ContinueWith(t => 
                {
                    if (t.IsCanceled) _taskCompletionSource.SetCanceled();
                    else if (t.Exception != null) _taskCompletionSource.SetException(t.Exception);
                    else _taskCompletionSource.SetResult();
                }, TaskScheduler.Current);

            return Task.CompletedTask;
        }

        protected abstract Task ComponentStartAsync(CrawlerStartState startState);

        public virtual Task PauseAsync()
        {
            if (!IsStarted)
            {
                throw new InvalidOperationException($"Cannot call {nameof(PauseAsync)} before {nameof(StartAsync)}");
            }
            return Task.CompletedTask;
        }

        public virtual Task ResumeAsync()
        {
            if (!IsStarted)
            {
                throw new InvalidOperationException($"Cannot call {nameof(ResumeAsync)} before {nameof(StartAsync)}");
            }
            return Task.CompletedTask;
        }

        public async Task WaitUntilCompletedAsync()
        {
            await _taskCompletionSource.Task.ConfigureAwait(false);
        }
    }
}
