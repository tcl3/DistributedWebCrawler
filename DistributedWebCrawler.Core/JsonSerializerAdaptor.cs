using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Text.Json;

namespace DistributedWebCrawler.Core
{
    public class JsonSerializerAdaptor : ISerializer
    {
        private readonly JsonSerializerOptions? _options;

        public JsonSerializerAdaptor(JsonSerializerOptions? options = null) 
        {
            _options = options;
        }

        public TResult? Deserialize<TResult>(ReadOnlySpan<byte> bytes)
        {
            return JsonSerializer.Deserialize<TResult>(bytes, _options);
        }

        public byte[] Serialize<TData>(TData data)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data, _options);
        }
    }
}
