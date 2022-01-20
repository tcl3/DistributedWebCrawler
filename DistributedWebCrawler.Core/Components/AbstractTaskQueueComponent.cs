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
    public abstract class AbstractTaskQueueComponent<TRequest, TSuccess, TFailure> : ICrawlerComponent
        where TRequest : RequestBase
        where TFailure : IErrorCode
    {
        private readonly IConsumer<TRequest> _consumer;
        private readonly IEventDispatcher<TSuccess, TFailure> _eventDispatcher;
        private readonly IKeyValueStore _outstandingItemsStore;
        private readonly ILogger _logger;
        private readonly TaskQueueSettings _taskQueueSettings;
        private readonly SemaphoreSlim _itemSemaphore;

        private volatile bool _isPaused;

        private readonly SemaphoreSlim _pauseSemaphore;

        protected bool IsStarted { get; private set; }

        private readonly Lazy<string> _name;
        protected string Name => _name.Value;

        private readonly TaskCompletionSource _taskCompletionSource;

        protected AbstractTaskQueueComponent(IConsumer<TRequest> consumer,
            IEventDispatcher<TSuccess, TFailure> eventReceiver,
            IKeyValueStore keyValueStore,
            ILogger logger,
            IComponentNameProvider componentNameProvider,
            TaskQueueSettings taskQueueSettings)
        {
            _consumer = consumer;
            _eventDispatcher = eventReceiver;
            _outstandingItemsStore = keyValueStore.WithKeyPrefix("TaskQueueOutstandingItems");
            _logger = logger;
            _taskQueueSettings = taskQueueSettings;
            _itemSemaphore = new SemaphoreSlim(taskQueueSettings.MaxConcurrentItems, taskQueueSettings.MaxConcurrentItems);
            _pauseSemaphore = new SemaphoreSlim(0);
            _taskCompletionSource = new();

            _name = new Lazy<string>(() =>
            {
                var componentType = GetType();
                return componentNameProvider.GetComponentName(componentType);
            });
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

        public Task StartAsync(CrawlerRunningState startState = CrawlerRunningState.Running, CancellationToken cancellationToken = default)
        {
            if (IsStarted)
            {
                throw new InvalidOperationException($"Cannot start {Name} when already started");
            }

            IsStarted = true;

            _logger.LogInformation($"{Name} component started");

            _ = ComponentStartAndHandleExceptionAsync(startState, cancellationToken);

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

        private async Task ComponentStartAndHandleExceptionAsync(CrawlerRunningState startState, CancellationToken cancellationToken)
        {
            try
            {
                await ComponentStartAsync(startState, cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                {
                    _taskCompletionSource.SetCanceled(cancellationToken);
                }
                else
                {
                    _taskCompletionSource.SetResult();
                }

            }
            catch (OperationCanceledException ex)
            {
                _taskCompletionSource.SetCanceled(ex.CancellationToken);
            }
            catch (Exception ex)
            {
                _taskCompletionSource.SetException(ex);
            }
        }

        protected virtual async Task ComponentStartAsync(CrawlerRunningState startState, CancellationToken cancellationToken)
        {
            if (startState == CrawlerRunningState.Paused)
            {
                await PauseAsync().ConfigureAwait(false);
            }

            await Task.Run(() => ItemProcessingLoop(cancellationToken), cancellationToken).ConfigureAwait(false);
        }

        private async Task ItemProcessingLoop(CancellationToken cancellationToken)
        {
            while (Status != CrawlerComponentStatus.Completed && !cancellationToken.IsCancellationRequested)
            {
                await _itemSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (_isPaused)
                {
                    await _pauseSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var currentItem = await _consumer.DequeueAsync().ConfigureAwait(false);

                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_taskQueueSettings.QueueItemTimeoutSeconds));
                var processItemCancellationToken = cts.Token;

                _ = ProcessItemAndReleaseSemaphore(currentItem, processItemCancellationToken);
            }
        }

        private async Task ProcessItemAndReleaseSemaphore(TRequest item, CancellationToken cancellationToken)
        {
            QueuedItemResult? queuedItem = null;
            try
            {
                queuedItem = await ProcessItemAsync(item, cancellationToken).ConfigureAwait(false);

                if (queuedItem != null)
                {
                    await NotifyAsync(item, queuedItem).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) 
            {
                _logger.LogInformation($"Task cancelled while processing queued item");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Uncaught exception in {Name}");
            }
            finally
            {
                _itemSemaphore.Release();
            }
        }

        private async Task NotifyAsync(TRequest currentItem, QueuedItemResult queuedItem)
        {
            switch (queuedItem.Status)
            {
                case QueuedItemStatus.Success when queuedItem is QueuedItemResult<TSuccess> successResult:
                    await _eventDispatcher.NotifyCompletedAsync(currentItem, successResult.Result).ConfigureAwait(false);
                    break;
                case QueuedItemStatus.Failed when queuedItem is QueuedItemResult<TFailure> failedResult:
                    await _eventDispatcher.NotifyFailedAsync(currentItem, failedResult.Result).ConfigureAwait(false);
                    break;
                case QueuedItemStatus.Waiting:
                    await _outstandingItemsStore.PutAsync(currentItem.Id.ToString("N"), currentItem).ConfigureAwait(false);
                    break;
            }

            var componentStatus = GetComponentStatus();
            await _eventDispatcher.NotifyComponentStatusUpdateAsync(componentStatus).ConfigureAwait(false);
        }

        public async Task RequeueAsync<TInnerRequest>(Guid requestId, IProducer<TInnerRequest> producer, CancellationToken cancellationToken = default)
            where TInnerRequest : RequestBase
        {
            if (!IsStarted)
            {
                throw new InvalidOperationException($"Cannot call {nameof(RequeueAsync)} before {nameof(StartAsync)}");
            }

            var requestKey = requestId.ToString("N");
            var request = await _outstandingItemsStore.GetAsync<TInnerRequest>(requestKey).ConfigureAwait(false);
            if (request == null)
            {
                throw new KeyNotFoundException($"Request of type {typeof(TInnerRequest).Name} and ID: {requestId} not found in KeyValueStore");
            }

            producer.Enqueue(request);
            await _outstandingItemsStore.RemoveAsync(requestKey).ConfigureAwait(false);
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

        private ComponentStatus GetComponentStatus()
        {
            return new ComponentStatus
            {
                QueueCount = _consumer.Count,
                TasksInUse = _taskQueueSettings.MaxConcurrentItems - _itemSemaphore.CurrentCount,
                MaxConcurrentTasks = _taskQueueSettings.MaxConcurrentItems
            };
        }

        protected static QueuedItemResult<TSuccess> Success(RequestBase request, TSuccess result)
        {
            return new QueuedItemResult<TSuccess>(request.Id, QueuedItemStatus.Success, result);
        }

        protected static QueuedItemResult<TFailure> Failed(RequestBase request, TFailure result)
        {
            return new QueuedItemResult<TFailure>(request.Id, QueuedItemStatus.Failed, result);
        }

        protected static QueuedItemResult Waiting(RequestBase request)
        {
            return new QueuedItemResult(request.Id, QueuedItemStatus.Waiting);
        }

        protected abstract Task<QueuedItemResult> ProcessItemAsync(TRequest item, CancellationToken cancellationToken);
    }
}