using System;

namespace DistributedWebCrawler.Core.Interfaces
{
    public interface IErrorCode
    {
        Enum Error { get; }
    }

    public interface IErrorCode<TError> : IErrorCode where TError : Enum
    {
        new TError Error { get; }
    }
}
