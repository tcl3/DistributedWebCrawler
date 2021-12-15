using DistributedWebCrawler.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DistributedWebCrawler.Core.Model
{
    public struct ErrorCode<TError> : IErrorCode<TError>
        where TError : Enum
    {
        private static readonly ConcurrentDictionary<TError, ErrorCode<TError>> _cache = new();

        private ErrorCode(TError code)
        {
            Error = code;
        }

        public TError Error { get; init; }

        Enum IErrorCode.Error => Error;

        public override bool Equals(object? obj)
        {
            return obj is ErrorCode<TError> code &&
                   EqualityComparer<TError>.Default.Equals(Error, code.Error);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Error);
        }

        public static bool operator ==(ErrorCode<TError> left, ErrorCode<TError> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ErrorCode<TError> left, ErrorCode<TError> right)
        {
            return !(left == right);
        }

        public static ErrorCode<TError> Instance(TError error)
        {
            return _cache.GetOrAdd(error, err => new ErrorCode<TError>(err));
        }
    }
}
