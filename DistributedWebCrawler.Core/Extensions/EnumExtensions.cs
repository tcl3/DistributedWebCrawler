using DistributedWebCrawler.Core.Models;
using System;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class EnumExtensions
    {
        public static ErrorCode<TError> AsErrorCode<TError>(this TError error) 
            where TError : Enum
        {
            return ErrorCode<TError>.Instance(error);
        }
    }
}
