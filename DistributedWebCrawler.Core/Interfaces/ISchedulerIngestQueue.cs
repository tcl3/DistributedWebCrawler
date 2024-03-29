﻿using DistributedWebCrawler.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ISchedulerIngestQueue : IDisposable
    {
        Task AddFromSchedulerAsync(SchedulerRequest schedulerRequest, IEnumerable<Uri> urisToVisit,
            CancellationToken cancellationToken = default);
    }
}
