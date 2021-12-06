using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Components
{
    public abstract class AbstractTaskQueueComponent<TRequest> : AbstractTaskQueueComponent<TRequest, bool>
        where TRequest : RequestBase
    {
        protected AbstractTaskQueueComponent(IConsumer<TRequest> consumer,
            IEventDispatcher<TRequest, bool> eventDispatcher,
            IKeyValueStore keyValueStore,
            ILogger logger,
            string name, TaskQueueSettings settings)
            : base(consumer, eventDispatcher, keyValueStore, logger, name, settings)
        {
        }
    }

    public abstract class AbstractTaskQueueComponent<TRequest, TResult> : ICrawlerComponent
        where TRequest : RequestBase
    {
        private readonly IConsumer<TRequest> _consumer;
        private readonly IEventDispatcher<TRequest, TResult> _eventDispatcher;
        private readonly IKeyValueStore _outstandingItemsStore;
        private readonly ILogger _logger;
        private readonly TaskQueueSettings _taskQueueSettings;
        private readonly SemaphoreSlim _itemSemaphore;

        private volatile bool _isPaused;
        
        private readonly SemaphoreSlim _pauseSemaphore;

        protected bool IsStarted { get; private set; }

        private readonly TaskCompletionSource _taskCompletionSource;


        protected AbstractTaskQueueComponent(IConsumer<TRequest> consumer,
            IEventDispatcher<TRequest, TResult> eventReceiver,
            IKeyValueStore keyValueStore,
            ILogger logger,
            string name, TaskQueueSettings taskQueueSettings)
        {
            _consumer = consumer;
            _eventDispatcher = eventReceiver;
            _outstandingItemsStore = keyValueStore.WithKeyPrefix("TaskQueueOutstandingItems");
            _logger = logger;
            Name = name;
            _taskQueueSettings = taskQueueSettings;
            _itemSemaphore = new SemaphoreSlim(taskQueueSettings.MaxConcurrentItems, taskQueueSettings.MaxConcurrentItems);
            _pauseSemaphore = new SemaphoreSlim(0);
            _taskCompletionSource = new();
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

        public async Task WaitUntilCompletedAsync()
        {
            await _taskCompletionSource.Task.ConfigureAwait(false);
        }

        protected virtual CrawlerComponentStatus GetStatus()
        {
            // TODO: Implement the correct status here to allow us to exit when done
            return CrawlerComponentStatus.Busy;
        }

        protected virtual async Task ComponentStartAsync(CrawlerStartState startState)
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
                var cancellationToken = cancellationTokenSource.Token;

                var task = ProcessItemAsync(currentItem, cancellationToken);

                _ = task.ContinueWith(r =>
                {
                    _itemSemaphore.Release();
                }, TaskScheduler.Current);

                _ = task.ContinueWith(async t =>
                {
                    var queuedItemResult = t.Result;

                    if (queuedItemResult.Status == QueuedItemStatus.Completed)
                    {
                        await _eventDispatcher.NotifyCompletedAsync(currentItem, task.Status, queuedItemResult.Result).ConfigureAwait(false);
                    }
                    else if (queuedItemResult.Status == QueuedItemStatus.Waiting)
                    {
                        await _outstandingItemsStore.PutAsync(currentItem.Id.ToString("N"), currentItem, cancellationToken).ConfigureAwait(false);
                    }
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

        public async Task RequeueAsync<TInnerRequest>(Guid requestId, IProducer<TInnerRequest> producer, CancellationToken cancellationToken)
            where TInnerRequest : RequestBase
        {
            var requestKey = requestId.ToString("N");
            var request = await _outstandingItemsStore.GetAsync<TInnerRequest>(requestKey, cancellationToken).ConfigureAwait(false);
            if (request == null)
            {
                throw new KeyNotFoundException($"Request of type {typeof(TInnerRequest).Name} and ID: {requestId} not found in KeyValueStore");
            }

            producer.Enqueue(request);
            await _outstandingItemsStore.RemoveAsync(requestKey, cancellationToken).ConfigureAwait(false);
        }

        public Task PauseAsync()
        {
            if (!IsStarted)
            {
                throw new InvalidOperationException($"Cannot call {nameof(PauseAsync)} before {nameof(StartAsync)}");
            }

            _logger.LogInformation("Pausing...");
            _isPaused = true;
            return Task.CompletedTask; 
        }

        public Task ResumeAsync()
        {
            if (!IsStarted)
            {
                throw new InvalidOperationException($"Cannot call {nameof(ResumeAsync)} before {nameof(StartAsync)}");
            }

            _logger.LogInformation("Resuming...");
            _isPaused = false;
            _pauseSemaphore.Release();

            return Task.CompletedTask;
        }

        protected abstract Task<QueuedItemResult<TResult>> ProcessItemAsync(TRequest item, CancellationToken cancellationToken);
    }
}