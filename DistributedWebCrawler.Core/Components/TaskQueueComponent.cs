using DistributedWebCrawler.Core.Configuration;
using DistributedWebCrawler.Core.Enums;
using DistributedWebCrawler.Core.Extensions;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Components
{
    public class TaskQueueComponent<TRequest, TSuccess, TFailure, TSettings> : ICrawlerComponent
        where TRequest : RequestBase
        where TFailure : IErrorCode
        where TSettings : TaskQueueSettings
    {
        private readonly IRequestProcessor<TRequest> _requestProcessor;
        private readonly IConsumer<TRequest> _consumer;
        private readonly IEventDispatcher<TSuccess, TFailure> _eventDispatcher;
        private readonly IKeyValueStore _outstandingItemsStore;
        private readonly ILogger _logger;
        private readonly INodeStatusProvider _nodeStatusProvider;
        private readonly TaskQueueSettings _taskQueueSettings;
        private readonly SemaphoreSlim _itemSemaphore;

        private volatile bool _isPaused;

        private readonly SemaphoreSlim _pauseSemaphore;

        protected bool IsStarted { get; private set; }

        private readonly Lazy<ComponentInfo> _componentInfo;

        public ComponentInfo ComponentInfo => _componentInfo.Value;

        private readonly TaskCompletionSource _taskCompletionSource;

        public TaskQueueComponent(
            IRequestProcessor<TRequest> requestProcessor,
            IConsumer<TRequest> consumer,
            IEventDispatcher<TSuccess, TFailure> eventReceiver,
            IKeyValueStore keyValueStore,
            ILogger<TaskQueueComponent<TRequest, TSuccess, TFailure, TSettings>> logger,
            IComponentNameProvider componentNameProvider,
            INodeStatusProvider nodeStatusProvider,
            TSettings settings)
        {
            _requestProcessor = requestProcessor;
            _consumer = consumer;
            _eventDispatcher = eventReceiver;
            _outstandingItemsStore = keyValueStore.WithKeyPrefix("TaskQueueOutstandingItems");
            _logger = logger;
            _nodeStatusProvider = nodeStatusProvider;
            _taskQueueSettings = settings;
            _itemSemaphore = new SemaphoreSlim(settings.MaxConcurrentItems, settings.MaxConcurrentItems);
            _pauseSemaphore = new SemaphoreSlim(0);
            _taskCompletionSource = new();

            _componentInfo = new Lazy<ComponentInfo>(() =>
            {
                var componentType = requestProcessor.GetType();
                var componentName = componentNameProvider.GetComponentName(componentType);
                var componentId = Guid.NewGuid();
                var nodeId = _nodeStatusProvider.CurrentNodeStatus.NodeId;
                return new ComponentInfo(componentName, componentId, nodeId);
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

                return _isPaused 
                    ? CrawlerComponentStatus.Paused 
                    : CrawlerComponentStatus.Running;
            }
        }

        public Task StartAsync(CrawlerRunningState startState = CrawlerRunningState.Running, CancellationToken cancellationToken = default)
        {
            if (IsStarted)
            {
                throw new InvalidOperationException($"Cannot start {ComponentInfo.ComponentName} when already started");
            }

            IsStarted = true;

            _logger.LogInformation($"{ComponentInfo.ComponentName} component started");

            _ = ComponentStartAndHandleExceptionAsync(startState, cancellationToken);

            return Task.CompletedTask;
        }

        public async Task WaitUntilCompletedAsync()
        {
            await _taskCompletionSource.Task.ConfigureAwait(false);
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
                try
                {
                    await _itemSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                    if (_isPaused && !cancellationToken.IsCancellationRequested)
                    {
                        await _pauseSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    }

                    var currentItem = await _consumer.DequeueAsync().ConfigureAwait(false);

                    var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(_taskQueueSettings.QueueItemTimeoutSeconds));
                    var processItemCancellationToken = cts.Token;

                    _ = ProcessItemAndReleaseSemaphore(currentItem, processItemCancellationToken);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, $"Task cancelled while processing queued item");
                }
            }
        }

        private async Task ProcessItemAndReleaseSemaphore(TRequest item, CancellationToken cancellationToken)
        {
            try
            {
                var queuedItem = await _requestProcessor.ProcessItemAsync(item, cancellationToken).ConfigureAwait(false);

                if (queuedItem != null)
                {
                    await NotifyAsync(item, queuedItem).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Uncaught exception in {ComponentInfo.ComponentName} (ComponentId: {ComponentInfo.ComponentId})");
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
                    await _eventDispatcher.NotifyCompletedAsync(currentItem, ComponentInfo, successResult.Result).ConfigureAwait(false);
                    break;
                case QueuedItemStatus.Failed when queuedItem is QueuedItemResult<TFailure> failedResult:
                    await _eventDispatcher.NotifyFailedAsync(currentItem, ComponentInfo, failedResult.Result).ConfigureAwait(false);
                    break;
                case QueuedItemStatus.Waiting:
                    await _outstandingItemsStore.PutAsync(currentItem.Id.ToString("N"), currentItem).ConfigureAwait(false);
                    break;
            }

            var componentStatus = GetComponentStatus();
            await _eventDispatcher.NotifyComponentStatusUpdateAsync(ComponentInfo, componentStatus).ConfigureAwait(false);
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
            return new ComponentStatus(nodeStatus: _nodeStatusProvider.CurrentNodeStatus)
            {
                QueueCount = _consumer.Count,
                TasksInUse = _taskQueueSettings.MaxConcurrentItems - _itemSemaphore.CurrentCount,
                MaxConcurrentTasks = _taskQueueSettings.MaxConcurrentItems,
            };
        }
    }
}