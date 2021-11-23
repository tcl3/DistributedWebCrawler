using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace DistributedWebCrawler.Core
{
    internal class MediaTypePattern
    {
        private readonly string _mediaType;

        private MediaTypePattern(string mediaType)
        {
            _mediaType = mediaType;
        }

        public bool Match(MediaTypePattern other)
        {
            var otherMediaType = other._mediaType;

            var thisIndex = 0;
            var otherIndex = 0;

            while (thisIndex < _mediaType.Length && otherIndex < otherMediaType.Length)
            {
                var thisChar = _mediaType[thisIndex];
                var otherChar = otherMediaType[otherIndex];

                if (thisChar == otherChar)
                {
                    thisIndex++;
                    otherIndex++;
                }
                else if (thisChar == '*')
                {
                    thisIndex++;

                    while ((thisIndex >= _mediaType.Length || otherChar != _mediaType[thisIndex]) && otherIndex < otherMediaType.Length - 1)
                    {
                        otherChar = otherMediaType[++otherIndex];
                    }
                }
                else if (otherChar == '*')
                {
                    otherIndex++;
                    while ((otherIndex >= otherMediaType.Length || thisChar != otherMediaType[otherIndex]) && thisIndex < _mediaType.Length - 1)
                    {
                        thisChar = _mediaType[++thisIndex];
                    }
                }
                else
                {
                    return false;
                }
            }

            return (thisIndex >= _mediaType.Length || otherMediaType.EndsWith("*"))
                && (otherIndex >= otherMediaType.Length || _mediaType.EndsWith("*"));
        }

        public static bool TryCreate(string mediaType, [NotNullWhen(returnValue: true)] out MediaTypePattern? result)
        {
            mediaType = mediaType.ToLowerInvariant();

            if (MediaTypeHeaderValue.TryParse(mediaType, out _))
            {
                result = new MediaTypePattern(mediaType);
                return true;
            }

            result = null;
            return false;
        }
    }
}
