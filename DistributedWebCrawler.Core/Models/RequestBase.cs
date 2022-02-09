﻿using System;

namespace DistributedWebCrawler.Core.Models
{
    public abstract class RequestBase
    {
        public RequestBase()
        {
            Id = Guid.NewGuid();
            CreatedAt = SystemClock.DateTimeOffsetNow();
        }

        public Guid Id { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
    }
}