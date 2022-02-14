using DistributedWebCrawler.Core.Models;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.ManagerAPI.Hubs;
using DistributedWebCrawler.ManagerAPI.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Timers;
using DistributedWebCrawler.Core.Enums;

using Timer = System.Timers.Timer;

namespace DistributedWebCrawler.ManagerAPI
{
    public class ComponentHubEventListener
    {
        private readonly HashSet<IEventReceiver> _receivers;
        private readonly ConcurrentDictionary<Guid, Lazy<ComponentStatsBuilder>> _builderLookup;
        private readonly IHubContext<CrawlerHub, IComponentEventsHub> _hubContext;
        
        private const int HubUpdateIntervalMillis = 1000;

        private volatile bool _currentlyIdle = true;

        public ComponentHubEventListener(IHubContext<CrawlerHub, IComponentEventsHub> hubContext)
        {
            _hubContext = hubContext;
            _receivers = new();
            _builderLookup = new();

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
                SendUpdateToHub(_hubContext.Clients.Client(connectionId), lazyBuilder.Value, forceUpdate: true);
            }
        }

        public void UpdateComponentStats(string connectionId)
        {
            UpdateComponentStats(_hubContext.Clients.Client(connectionId), forceUpdate: true);
        }

        private void UpdateComponentStats(IComponentEventsHub hub, bool forceUpdate)
        {
            var builders = _builderLookup.Values.ToList();
            var anyComponentsRunning = false;
            foreach (var lazyBuilder in builders)
            {
                var builder = lazyBuilder.Value;

                anyComponentsRunning = anyComponentsRunning || builder.CurrentStatus == CrawlerComponentStatus.Running;

                SendUpdateToHub(hub, builder, forceUpdate);

                if (!forceUpdate)
                {
                    builder.Reset();
                }
            }

            if ((forceUpdate && !builders.Any()) 
                || (!anyComponentsRunning && !_currentlyIdle && builders.Any()))
            {
                hub.OnAllComponentsIdle();
            }

            _currentlyIdle = !anyComponentsRunning;
        }

        private void OnTimerIntervalElapsed(object? source, ElapsedEventArgs e)
        {
            UpdateComponentStats(_hubContext.Clients.All, forceUpdate: false);
        }

        private void SendUpdateToHub(IComponentEventsHub hub, ComponentStatsBuilder builder, bool forceUpdate = false)
        {
            var componentName = builder.ComponentInfo.ComponentName;

            if (forceUpdate || builder.CompletedItemStatsHasUpdate())
            {
                var completedItemStats = builder.BuildCompletedItemStats();
                hub.OnCompleted(componentName, completedItemStats);
            }

            if (forceUpdate || builder.FailedItemStatsHasUpdate())
            {
                var failedItemStats = builder.BuildFailedItemStats();
                hub.OnFailed(componentName, failedItemStats);
            }

            if (forceUpdate || builder.ComponentStatsHasUpdate())
            {
                var componentStatusStats = builder.BuildComponentStatusStats();
                hub.OnComponentUpdate(componentName, componentStatusStats);
            }
        }

        private Task OnComponentUpdateAsync(object? sender, ComponentEventArgs<ComponentStatus> e)
        {
            var builder = GetBuilder(e.ComponentInfo);
            builder.AddComponentStatusUpdate(e.Result);
            return Task.CompletedTask;
        }

        private Task OnFailedAsync(object? sender, ItemFailedEventArgs e)
        {
            var builder = GetBuilder(e.ComponentInfo);
            builder.AddFailedItem(e);
            return Task.CompletedTask;
        }

        private Task OnCompletedAsync(object? sender, ItemCompletedEventArgs e)
        {
            var builder = GetBuilder(e.ComponentInfo);
            builder.AddCompletedItem(e);
            return Task.CompletedTask;
        }

        private ComponentStatsBuilder GetBuilder(ComponentInfo componentInfo)
        {
            var lazyValue = _builderLookup.GetOrAdd(componentInfo.ComponentId, name => new(() => new ComponentStatsBuilder(componentInfo)));
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

            public CrawlerComponentStatus CurrentStatus { get; private set; }

            private object _resetLock = new();

            private readonly ConcurrentBag<object> _completedItems;
            private readonly ConcurrentBag<IErrorCode> _failedItems;

            private ConcurrentDictionary<string, int> _errorCounts;

            private readonly ConcurrentDictionary<Guid, NodeStatusBuilder> _nodeStatusBuilderLookup;

            private const int RecentItemCount = 10;

            public ComponentInfo ComponentInfo { get; }

            public ComponentStatsBuilder(ComponentInfo componentInfo)
            {                
                _errorCounts = new();
                _completedItems = new();
                _failedItems = new();
                _nodeStatusBuilderLookup = new();

                ComponentInfo = componentInfo;
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

                    CurrentStatus = CrawlerComponentStatus.Unknown;

                    _completedItems.Clear();
                    _failedItems.Clear();
                    _errorCounts.Clear();
                    
                    foreach (var builder in _nodeStatusBuilderLookup.Values)
                    {
                        builder.Reset();
                    }
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

                var builder = _nodeStatusBuilderLookup.GetOrAdd(componentStatus.NodeStatus.NodeId, key => new NodeStatusBuilder());
                builder.UpdateNodeStatus(componentStatus.NodeStatus);

                CurrentStatus = componentStatus.CurrentStatus;

                Interlocked.Increment(ref _componentUpdateCount);
            }

            public bool CompletedItemStatsHasUpdate()
            {
                return _completedItemCount > 0;
            }

            public CompletedItemStats BuildCompletedItemStats()
            { 
                return new CompletedItemStats(_completedItemCount, _totalCompletedItemCount)
                {
                    RecentItems = _completedItems.TakeLast(RecentItemCount).ToList(),
                    TotalBytesIngested = _hasContentLength ? _totalBytesIngested : null,
                    TotalBytesIngestedSinceLastUpdate = _hasContentLength ? _totalBytesIngestedSinceLastUpdate : null
                };
            }

            public FailedItemStats BuildFailedItemStats()
            {
                return new FailedItemStats(_failedItemCount, _totalFailedItemCount)
                {
                    ErrorCounts = new Dictionary<string, int>(_errorCounts),
                    RecentItems = _failedItems.TakeLast(RecentItemCount).ToList()
                };
            }

            public bool FailedItemStatsHasUpdate()
            {
                return _failedItemCount > 0;
            }

            public ComponentStatusStats BuildComponentStatusStats()
            {
                var averageTasksInUse = _componentUpdateCount == 0
                    ? 0
                    : (int)Math.Round(_totalTaskCount / (double)_componentUpdateCount);
                
                var averageQueueCount = _componentUpdateCount == 0
                    ? 0
                    : (int)Math.Round(_totalQueueCount / (double)_componentUpdateCount);

                var nodeStatusLookup = _nodeStatusBuilderLookup
                    .ToArray()
                    .ToDictionary(x => x.Key, x => x.Value.Build());

                return new ComponentStatusStats(_componentUpdateCount, _totalComponentUpdateCount)
                {
                    AverageTasksInUse = averageTasksInUse,
                    AverageQueueCount = averageQueueCount,
                    MaxTasks = _maxTasks,
                    CurrentStatus = this.CurrentStatus,
                    NodeStatus = nodeStatusLookup
                };
            }

            public bool ComponentStatsHasUpdate()
            {
                return _componentUpdateCount > 0;
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
                _totalBytesDownloaded = 0;
                _totalBytesUploaded = 0;
            }
        }
    }
}