using System;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface ISerializer
    {
        byte[] Serialize<TData>(TData data);
        TResult? Deserialize<TResult>(ReadOnlySpan<byte> bytes);
    }
}
