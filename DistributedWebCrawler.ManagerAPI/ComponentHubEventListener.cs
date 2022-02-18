using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.ManagerAPI.Hubs;
using DistributedWebCrawler.ManagerAPI.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Timers;
using DistributedWebCrawler.Core.Enums;
using System.Diagnostics.CodeAnalysis;

using Timer = System.Timers.Timer;

namespace DistributedWebCrawler.ManagerAPI
{
    public class ComponentHubEventListener
    {
        private readonly HashSet<IEventReceiver> _receivers;
        private readonly ConcurrentDictionary<Guid, Lazy<ComponentStatsBuilder>> _builderLookup;
        private readonly ConcurrentDictionary<Guid, Lazy<NodeStatusBuilder>> _nodeStatusBuilderLookup;
        private readonly IHubContext<CrawlerHub, IComponentEventsHub> _hubContext;
        
        private const int HubUpdateIntervalMillis = 1000;

        private volatile bool _allComponentsPaused = true;

        public ComponentHubEventListener(IHubContext<CrawlerHub, IComponentEventsHub> hubContext)
        {
            _hubContext = hubContext;
            _receivers = new();
            _builderLookup = new();
            _nodeStatusBuilderLookup = new();

            var timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimerIntervalElapsed);
            timer.Interval = HubUpdateIntervalMillis;
            timer.Enabled = true;
            timer.Start();
        }

        public void Register(IEventReceiver eventReceiver)
        {
            if (_receivers.Add(eventReceiver))
            {
                eventReceiver.OnCompletedAsync += OnCompletedAsync;
                eventReceiver.OnFailedAsync += OnFailedAsync;
                eventReceiver.OnComponentUpdateAsync += OnComponentUpdateAsync;
            }
        }

        public void Unregister(IEventReceiver eventReceiver)
        {
            if (_receivers.Remove(eventReceiver))
            {
                eventReceiver.OnCompletedAsync -= OnCompletedAsync;
                eventReceiver.OnFailedAsync -= OnFailedAsync;
                eventReceiver.OnComponentUpdateAsync -= OnComponentUpdateAsync;
            }
        }

        public void UpdateComponentStats(string connectionId, Guid componentId)
        {
            if (_builderLookup.TryGetValue(componentId, out var lazyBuilder))
            {
                var builders = new[] { lazyBuilder.Value };
                SendForcedUpdateToHub(_hubContext.Clients.Client(connectionId), builders);
            }
        }

        public void UpdateComponentStats(string connectionId)
        {
            UpdateComponentStats(_hubContext.Clients.Client(connectionId), forceUpdate: true);
        }

        private void UpdateComponentStats(IComponentEventsHub hub, bool forceUpdate)
        {
            var builders = _builderLookup.Values.Select(x => x.Value).ToList();
            if (forceUpdate)
            {
                SendForcedUpdateToHub(hub, builders);
            }
            else
            {
                SendUpdateToHub(hub, builders);
            }            
        }

        private void OnTimerIntervalElapsed(object? source, ElapsedEventArgs e)
        {
            UpdateComponentStats(_hubContext.Clients.All, forceUpdate: false);
        }

        private void SendForcedUpdateToHub(IComponentEventsHub hub, IEnumerable<ComponentStatsBuilder> builders)
        {
            var componentStatsList = new List<ComponentStats>();
            foreach (var builder in builders)
            {
                var componentStats = builder.BuildComponentStats(forceUpdate: true);
                componentStatsList.Add(componentStats);
            }

            var nodeStatus = new Dictionary<Guid, NodeStatusStats>();
            foreach (var (nodeId, lazyBuilder) in _nodeStatusBuilderLookup)
            {
                var builder = lazyBuilder.Value;
                nodeStatus.Add(nodeId, builder.Build());
            }

            var componentStatsCollection = new ComponentStatsCollection
            {
                ComponentStats = componentStatsList,
                NodeStatus = nodeStatus
            };

            hub.OnComponentUpdate(componentStatsCollection);
        }

        private void SendUpdateToHub(IComponentEventsHub hub, IEnumerable<ComponentStatsBuilder> builders)
        {
            var componentStatsList = new List<ComponentStats>();
            var anyComponentsRunning = false;
            var allComponentsPaused = true;
            foreach (var builder in builders)
            {
                anyComponentsRunning = anyComponentsRunning || builder.CurrentStatus == CrawlerComponentStatus.Running;

                var componentHasUpdate = builder.ComponentStatsHasUpdate();

                allComponentsPaused = allComponentsPaused
                    && (builder.CurrentStatus == CrawlerComponentStatus.Paused
                        || !componentHasUpdate);

                if (componentHasUpdate)
                {
                    var componentStats = builder.BuildComponentStats(forceUpdate: false);
                    componentStatsList.Add(componentStats);
                }

                builder.Reset();
            }

            if (allComponentsPaused && !_allComponentsPaused && builders.Any())
            {
                hub.OnAllComponentsPaused();
            }
            
            _allComponentsPaused = allComponentsPaused && builders.Any();
            if (!componentStatsList.Any())
            {
                return;
            }

            var nodeStatus = new Dictionary<Guid, NodeStatusStats>();
            foreach (var (nodeId, lazyBuilder) in _nodeStatusBuilderLookup)
            {
                var builder = lazyBuilder.Value;
                
                if (builder.HasUpdate())
                {
                    nodeStatus.Add(nodeId, builder.Build());
                }

                builder.Reset();
            }

            var componentStatsCollection = new ComponentStatsCollection
            {
                ComponentStats = componentStatsList,
                NodeStatus = nodeStatus
            };

            hub.OnComponentUpdate(componentStatsCollection);
        }

        private Task OnComponentUpdateAsync(object? sender, ComponentEventArgs<ComponentStatus> e)
        {
            var componentStatsBuilder = GetComponentStatsBuilder(e.ComponentInfo);
            componentStatsBuilder.AddComponentStatusUpdate(e.Result);
            
            var nodeStatusBuilder = GetNodeStatusBuilder(e.Result.NodeStatus.NodeId);
            nodeStatusBuilder.UpdateNodeStatus(e.Result.NodeStatus);
            
            return Task.CompletedTask;
        }

        private Task OnFailedAsync(object? sender, ItemFailedEventArgs e)
        {
            var builder = GetComponentStatsBuilder(e.ComponentInfo);
            builder.AddFailedItem(e);
            return Task.CompletedTask;
        }

        private Task OnCompletedAsync(object? sender, ItemCompletedEventArgs e)
        {
            var builder = GetComponentStatsBuilder(e.ComponentInfo);
            builder.AddCompletedItem(e);
            return Task.CompletedTask;
        }

        private ComponentStatsBuilder GetComponentStatsBuilder(ComponentInfo componentInfo)
        {
            var lazyValue = _builderLookup.GetOrAdd(componentInfo.ComponentId, key => new(() => new ComponentStatsBuilder(componentInfo)));
            return lazyValue.Value;
        }

        private NodeStatusBuilder GetNodeStatusBuilder(Guid nodeId)
        {
            var lazyValue = _nodeStatusBuilderLookup.GetOrAdd(nodeId, key => new Lazy<NodeStatusBuilder>(() => new()));
            return lazyValue.Value;
        }

        private class ComponentStatsBuilder
        {
            private int _maxTasks;
            private long _totalTaskCount;
            private long _totalQueueCount;

            private long _totalBytesIngested;
            private long _totalBytesIngestedSinceLastUpdate;
            private bool _hasContentLength;

            private long _componentUpdateCount;
            private long _completedItemCount;
            private long _failedItemCount;

            private long _totalComponentUpdateCount;
            private long _totalCompletedItemCount;
            private long _totalFailedItemCount;

            private bool _isIdle;

            private int _lastAverageQueueCount;

            public CrawlerComponentStatus CurrentStatus { get; private set; }

            private object _resetLock = new();

            private readonly ConcurrentBag<object> _completedItems;
            private readonly ConcurrentBag<IErrorCode> _failedItems;

            private ConcurrentDictionary<string, int> _errorCounts;

            private const int RecentItemCount = 10;

            private readonly ComponentInfo _componentInfo;

            public ComponentStatsBuilder(ComponentInfo componentInfo)
            {                
                _errorCounts = new();
                _completedItems = new();
                _failedItems = new();

                _componentInfo = componentInfo;
            }

            public void Reset()
            {
                lock (_resetLock)
                {
                    _totalQueueCount = 0;
                    _totalTaskCount = 0;

                    _componentUpdateCount = 0;
                    _completedItemCount = 0;
                    _failedItemCount = 0;

                    _totalBytesIngestedSinceLastUpdate = 0;

                    _completedItems.Clear();
                    _failedItems.Clear();
                    _errorCounts.Clear();
                }
            }

            public void AddCompletedItem(ItemCompletedEventArgs args)
            {
                Interlocked.Increment(ref _completedItemCount);
                Interlocked.Increment(ref _totalCompletedItemCount);

                if (args.Result is IngestSuccess ingestResult)
                {
                    _hasContentLength = true;
                    Interlocked.Add(ref _totalBytesIngested, ingestResult.ContentLength);
                    Interlocked.Add(ref _totalBytesIngestedSinceLastUpdate, ingestResult.ContentLength);
                }

                // TODO: common ContentLength interface
                else if (args.Result is RobotsDownloaderSuccess robotsResult)
                {
                    _hasContentLength = true;
                    Interlocked.Add(ref _totalBytesIngested, robotsResult.ContentLength);
                    Interlocked.Add(ref _totalBytesIngestedSinceLastUpdate, robotsResult.ContentLength);
                }

                _completedItems.Add(args.Result);
            }

            public void AddFailedItem(ItemFailedEventArgs args)
            {
                Interlocked.Increment(ref _totalFailedItemCount);
                _errorCounts.AddOrUpdate(args.Result.Error.ToString(), 0, (key, oldValue) => oldValue + 1);
                _failedItems.Add(args.Result);

                Interlocked.Increment(ref _failedItemCount);
            }

            public void AddComponentStatusUpdate(ComponentStatus componentStatus)
            {
                Interlocked.Increment(ref _totalComponentUpdateCount);

                _maxTasks = componentStatus.MaxConcurrentTasks;
                Interlocked.Add(ref _totalTaskCount, componentStatus.TasksInUse);
                Interlocked.Add(ref _totalQueueCount, componentStatus.QueueCount);

                CurrentStatus = componentStatus.CurrentStatus;

                Interlocked.Increment(ref _componentUpdateCount);
            }

            public ComponentStats BuildComponentStats(bool forceUpdate)
            {
                var completedItemStats = forceUpdate || _completedItemCount > 0
                    ? BuildCompletedItemStats()
                    : null;

                var failedItemStats = forceUpdate || _failedItemCount > 0
                    ? BuildFailedItemStats()
                    : null;

                return new ComponentStats(_componentInfo)
                {
                    Completed = completedItemStats,
                    Failed = failedItemStats,
                    ComponentStatus = BuildComponentStatusStats(),
                };
            }

            public bool ComponentStatsHasUpdate()
            {
                var hasUpdate = _componentUpdateCount > 0 
                    || _completedItemCount > 0
                    || _failedItemCount > 0;

                var result = hasUpdate || !_isIdle;

                _isIdle = !hasUpdate;

                return result;
            }

            private CompletedItemStats BuildCompletedItemStats()
            {
                return new CompletedItemStats(_completedItemCount, _totalCompletedItemCount)
                {
                    RecentItems = _completedItems.TakeLast(RecentItemCount).ToList(),
                    TotalBytesIngested = _hasContentLength ? _totalBytesIngested : null,
                    TotalBytesIngestedSinceLastUpdate = _hasContentLength ? _totalBytesIngestedSinceLastUpdate : null
                };
            }

            private FailedItemStats BuildFailedItemStats()
            {
                return new FailedItemStats(_failedItemCount, _totalFailedItemCount)
                {
                    ErrorCounts = new Dictionary<string, int>(_errorCounts),
                    RecentItems = _failedItems.TakeLast(RecentItemCount).ToList()
                };
            }

            private ComponentStatusStats BuildComponentStatusStats()
            {
                var averageTasksInUse = _componentUpdateCount == 0
                    ? 0
                    : (int)Math.Round(_totalTaskCount / (double)_componentUpdateCount);
                
                var averageQueueCount = _componentUpdateCount == 0
                    ? CurrentStatus == CrawlerComponentStatus.Running || CurrentStatus == CrawlerComponentStatus.Unknown
                        ? 0
                        : _lastAverageQueueCount
                    : (int)Math.Round(_totalQueueCount / (double)_componentUpdateCount);

                _lastAverageQueueCount = averageQueueCount;

                return new ComponentStatusStats(_componentUpdateCount, _totalComponentUpdateCount)
                {
                    AverageTasksInUse = averageTasksInUse,
                    AverageQueueCount = averageQueueCount,
                    MaxTasks = _maxTasks,
                    CurrentStatus = this.CurrentStatus,
                };
            }
        }

        private class NodeStatusBuilder
        {
            private long _totalBytesDownloadedSinceLastUpdate;
            private long _totalBytesUploadedSinceLastUpdate;
            private long _totalBytesDownloaded;
            private long _totalBytesUploaded;

            private long _totalBytesUploadedSinceLastReset;
            private long _totalBytesDownloadedSinceLastReset;

            public void UpdateNodeStatus(NodeStatus nodeStatus)
            {
                if (nodeStatus.TotalBytesDownloaded > _totalBytesDownloaded)
                {
                    _totalBytesDownloaded = nodeStatus.TotalBytesDownloaded;
                    _totalBytesDownloadedSinceLastUpdate = nodeStatus.TotalBytesDownloaded - _totalBytesDownloadedSinceLastReset;
                }

                if (nodeStatus.TotalBytesUploaded > _totalBytesUploaded)
                {
                    _totalBytesUploaded = nodeStatus.TotalBytesUploaded;
                    _totalBytesUploadedSinceLastUpdate = nodeStatus.TotalBytesUploaded - _totalBytesUploadedSinceLastReset;
                }                
            }

            public bool HasUpdate()
            {
                return _totalBytesDownloaded > 0 || _totalBytesUploaded > 0;
            }

            public NodeStatusStats Build()
            {
                return new NodeStatusStats
                {
                    TotalBytesDownloaded = _totalBytesDownloaded,
                    TotalBytesUploaded = _totalBytesUploaded,
                    TotalBytesDownloadedSinceLastUpdate = _totalBytesDownloadedSinceLastUpdate,
                    TotalBytesUploadedSinceLastUpdate = _totalBytesUploadedSinceLastUpdate,
                };
            }

            public void Reset()
            {
                _totalBytesUploadedSinceLastReset = _totalBytesUploaded;
                _totalBytesDownloadedSinceLastReset = _totalBytesDownloaded;
                _totalBytesDownloadedSinceLastUpdate = 0;
                _totalBytesUploadedSinceLastUpdate = 0;
            }
        }
    }
}