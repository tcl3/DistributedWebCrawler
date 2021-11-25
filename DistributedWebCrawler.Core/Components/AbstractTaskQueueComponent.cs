using DistributedWebCrawler.Core.Compontents;
using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Components
{
    public abstract class AbstractTaskQueueComponent<TRequest> : AbstractTaskQueueComponent<TRequest, bool>
        where TRequest : RequestBase
    {
        protected AbstractTaskQueueComponent(IConsumer<TRequest, bool> consumer, ILogger logger, string name, TaskQueueSettings settings) 
            : base(consumer, logger, name, settings)
        {
        }
    }

    public abstract class AbstractTaskQueueComponent<TRequest, TResult> : AbstractComponent 
        where TRequest : RequestBase
    {
        private readonly IConsumer<TRequest, TResult> _consumer;
        private readonly ILogger _logger;
        private readonly TaskQueueSettings _taskQueueSettings;
        private readonly SemaphoreSlim _itemSemaphore;

        private volatile bool _isPaused;
        
        private readonly SemaphoreSlim _pauseSemaphore;

        protected AbstractTaskQueueComponent(IConsumer<TRequest, TResult> consumer, ILogger logger,
            string name, TaskQueueSettings taskQueueSettings) : base(logger, name)
        {
            _consumer = consumer;
            _logger = logger;
            _taskQueueSettings = taskQueueSettings;
            _itemSemaphore = new SemaphoreSlim(taskQueueSettings.MaxConcurrentItems, taskQueueSettings.MaxConcurrentItems);
            _pauseSemaphore = new SemaphoreSlim(0);
        }

        protected override CrawlerComponentStatus GetStatus()
        {
            // TODO: Implement the correct status here to allow us to exit when done
            return CrawlerComponentStatus.Busy;
        }

        protected override async Task ComponentStartAsync(CrawlerStartState startState)
        {
            if (startState == CrawlerStartState.Paused)
            {
                await PauseAsync().ConfigureAwait(false);
            }

            await Task.Run(QueueLoop).ConfigureAwait(false);            
        }
        
        private async Task QueueLoop()
        {
            while (Status != CrawlerComponentStatus.Completed)
            {                
                await _itemSemaphore.WaitAsync().ConfigureAwait(false);

                if (_isPaused)
                {
                    await _pauseSemaphore.WaitAsync().ConfigureAwait(false);
                }

                var currentItem = await _consumer.DequeueAsync().ConfigureAwait(false);
                
                var cancellationTokenSource = new CancellationTokenSource();

                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(_taskQueueSettings.QueueItemTimeoutSeconds));

                var task = ProcessItemAsync(currentItem, cancellationTokenSource.Token);

                _ = task.ContinueWith(r =>
                {
                    _itemSemaphore.Release();
                }, TaskScheduler.Current);

                _ = task.ContinueWith(r =>
                {
                    // TODO: indicate whether task errored or not
                    _consumer.NotifyCompletedAsync(currentItem, task.Status, task.Result);
                }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);

                _ = task.ContinueWith(r =>
                {
                    _logger.LogInformation($"Task cancelled while processing queued item");
                }, CancellationToken.None, TaskContinuationOptions.OnlyOnCanceled, TaskScheduler.Current);

                _ = task.ContinueWith(r =>
                {
                    var aggregateException = task.Exception;

                    if (aggregateException == null || aggregateException.InnerExceptions.Count == 0)
                    {
                        _logger.LogError($"Uncaught exception in {Name}.");
                        return;
                    }

                    foreach (var exception in aggregateException.InnerExceptions)
                    {
                        _logger.LogError(exception, $"Uncaught exception in {Name}");
                    }

                }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
            }
        }

        public override Task PauseAsync()
        {
            base.PauseAsync();
            _logger.LogInformation("Pausing...");
            _isPaused = true;
            return Task.CompletedTask; 
        }

        public override Task ResumeAsync()
        {
            base.ResumeAsync();
            _logger.LogInformation("Resuming...");
            _isPaused = false;
            _pauseSemaphore.Release();

            return Task.CompletedTask;
        }

        protected abstract Task<TResult> ProcessItemAsync(TRequest item, CancellationToken cancellationToken);
    }
}