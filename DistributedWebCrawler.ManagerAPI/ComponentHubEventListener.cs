using DistributedWebCrawler.Core.Components;
using DistributedWebCrawler.Core.Interfaces;
using DistributedWebCrawler.Core.Model;
using DistributedWebCrawler.Core.Queue;
using DistributedWebCrawler.ManagerAPI.Hubs;
using DistributedWebCrawler.ManagerAPI.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DistributedWebCrawler.ManagerAPI
{
    public class ComponentHubEventListener
    {
        private readonly HashSet<IEventReceiver> _receivers;
        private readonly ConcurrentDictionary<string, Lazy<ComponentStatsBuilder>> _builderLookup;
        private readonly IHubContext<CrawlerHub, IComponentEventsHub> _hubContext;
        
        private const int HubUpdateIntervalMillis = 1000;

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

        private void OnTimerIntervalElapsed(object? source, ElapsedEventArgs e)
        {
            var builders = _builderLookup.Values.ToList();

            foreach (var lazyBuilder in builders)
            {
                var builder = lazyBuilder.Value;
                SendUpdateToHub(builder);
                builder.Reset();
            }
        }

        private void SendUpdateToHub(ComponentStatsBuilder builder)
        {
            var componentName = builder.ComponentName;

            if (builder.TryBuildCompletedItemStats(out var completedItemStats))
            {
                _hubContext.Clients.All.OnCompleted(componentName, completedItemStats);
            }

            if (builder.TryBuildFailedItemStats(out var failedItemStats))
            {
                _hubContext.Clients.All.OnFailed(componentName, failedItemStats);
            }

            if (builder.TryBuildComponentStatusStats(out var componentStatusStats))
            {
                _hubContext.Clients.All.OnComponentUpdate(componentName, componentStatusStats);
            }
        }

        public void Register(IEventReceiver eventReceiver)
        {
            _receivers.Add(eventReceiver);

            eventReceiver.OnCompletedAsync += OnCompletedAsync;
            eventReceiver.OnFailedAsync += OnFailedAsync;
            eventReceiver.OnComponentUpdateAsync += OnComponentUpdateAsync;
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

        private Task OnComponentUpdateAsync(object? sender, ComponentEventArgs<ComponentStatus> e)
        {
            var builder = GetBuilder(e.ComponentName);
            builder.AddComponentStatusUpdate(e.Result);
            return Task.CompletedTask;
        }

        private Task OnFailedAsync(object? sender, ItemFailedEventArgs e)
        {
            var builder = GetBuilder(e.ComponentName);
            builder.AddFailedItem(e);
            return Task.CompletedTask;
        }

        private Task OnCompletedAsync(object? sender, ItemCompletedEventArgs e)
        {
            var builder = GetBuilder(e.ComponentName);
            builder.AddCompletedItem(e);
            return Task.CompletedTask;
        }
        
        private ComponentStatsBuilder GetBuilder(string componentName)
        {
            var lazyValue = _builderLookup.GetOrAdd(componentName, name => new(() => new ComponentStatsBuilder(name)));
            return lazyValue.Value;
        }

        private class ComponentStatsBuilder
        {
            private int _maxTasks;
            private long _totalTaskCount;
            private long _totalQueueCount;

            private long _totalBytes;
            private long _totalBytesSinceLastUpdate;
            private bool _hasContentLength;

            private long _componentUpdateCount;
            private long _completedItemCount;
            private long _failedItemCount;

            private long _totalComponentUpdateCount;
            private long _totalCompletedItemCount;
            private long _totalFailedItemCount;

            private object _resetLock = new();

            private readonly ConcurrentBag<object> _completedItems;
            private readonly ConcurrentBag<IErrorCode> _failedItems;

            private ConcurrentDictionary<string, int> _errorCounts;

            private const int RecentItemCount = 10;

            public string ComponentName { get; }

            public ComponentStatsBuilder(string componentName)
            {                
                _errorCounts = new();
                _completedItems = new();
                _failedItems = new();

                ComponentName = componentName;
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

                    _totalBytesSinceLastUpdate = 0;

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
                    Interlocked.Add(ref _totalBytes, ingestResult.ContentLength);
                    Interlocked.Add(ref _totalBytesSinceLastUpdate, ingestResult.ContentLength);
                }

                // TODO: common ContentLength interface
                else if (args.Result is RobotsDownloaderSuccess robotsResult)
                {
                    _hasContentLength = true;
                    Interlocked.Add(ref _totalBytes, robotsResult.ContentLength);
                    Interlocked.Add(ref _totalBytesSinceLastUpdate, robotsResult.ContentLength);
                }
                _completedItems.Add(args.Result);
            }

            public void AddFailedItem(ItemFailedEventArgs args)
            {
                Interlocked.Increment(ref _failedItemCount);
                Interlocked.Increment(ref _totalFailedItemCount);
                _errorCounts.AddOrUpdate(args.Result.Error.ToString(), 0, (key, oldValue) => oldValue + 1);
                _failedItems.Add(args.Result);
            }

            public void AddComponentStatusUpdate(ComponentStatus componentStatus)
            {
                Interlocked.Increment(ref _componentUpdateCount);
                Interlocked.Increment(ref _totalComponentUpdateCount);

                _maxTasks = componentStatus.MaxConcurrentTasks;
                Interlocked.Add(ref _totalTaskCount, componentStatus.TasksInUse);
                Interlocked.Add(ref _totalQueueCount, componentStatus.QueueCount);
            }

            public bool TryBuildCompletedItemStats([NotNullWhen(returnValue: true)] out CompletedItemStats? completedItemStats)
            {
                if (_completedItemCount <= 0)
                {
                    completedItemStats = null;
                    return false;
                }

                completedItemStats = new CompletedItemStats(_completedItemCount, _totalCompletedItemCount)
                {
                    RecentItems = _completedItems.TakeLast(RecentItemCount).ToList(),
                    TotalBytes = _hasContentLength ? _totalBytes : null,
                    TotalBytesSinceLastUpdate = _hasContentLength ? _totalBytesSinceLastUpdate : null
                };

                return true;
            }

            public bool TryBuildFailedItemStats([NotNullWhen(returnValue: true)] out FailedItemStats? failedItemStats)
            {
                if (_failedItemCount <= 0)
                {
                    failedItemStats = null;
                    return false;
                }

                failedItemStats = new FailedItemStats(_failedItemCount, _totalFailedItemCount)
                {
                    ErrorCounts = new Dictionary<string, int>(_errorCounts),
                    RecentItems = _failedItems.TakeLast(RecentItemCount).ToList()
                };

                return true;
            }

            public bool TryBuildComponentStatusStats([NotNullWhen(returnValue: true)] out ComponentStatusStats? componentStatusStats)
            {
                if (_componentUpdateCount <= 0)
                {
                    componentStatusStats = null;
                    return false;
                }

                componentStatusStats = new ComponentStatusStats(_componentUpdateCount, _totalComponentUpdateCount)
                {
                    AverageTasksInUse = (int)Math.Round(_totalTaskCount / (double)_componentUpdateCount),
                    AverageQueueCount = (int)Math.Round(_totalQueueCount / (double)_componentUpdateCount),
                    MaxTasks = _maxTasks
                };

                return true;
            }
        }
    }
}