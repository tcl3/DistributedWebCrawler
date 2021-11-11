using System;
using System.Runtime.Serialization;

namespace DistributedWebCrawler.Core.Exceptions
{
    [Serializable]
    public class IngesterException : Exception
    {
        public IngesterException()
        {
        }

        public IngesterException(string? message) : base(message)
        {
        }

        public IngesterException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected IngesterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}