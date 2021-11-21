using System;

namespace DistributedWebCrawler.Core
{
    internal class DomainPattern
    {
        private readonly string _domainPattern;

        public DomainPattern(string domainPattern)
        {
            if (string.IsNullOrWhiteSpace(domainPattern))
            {
                throw new ArgumentNullException(nameof(domainPattern));
            }
            _domainPattern = domainPattern.ToLowerInvariant();
        }

        public bool Match(string stringToMatch)
        {
            if (string.IsNullOrEmpty(stringToMatch))
            {
                throw new ArgumentException(nameof(stringToMatch));
            }

            stringToMatch = stringToMatch.ToLowerInvariant();

            // Handles the case where "*.example.com" should match "example.com"
            if (_domainPattern.StartsWith("*.") && MatchInternal(stringToMatch, patternIndex: 2))
            {
                return true;
            }

            return MatchInternal(stringToMatch);
        }

        private bool MatchInternal(string stringToMatch, int patternIndex = 0, int stringToMatchIndex = 0)
        {
            while (stringToMatchIndex < stringToMatch.Length && patternIndex < _domainPattern.Length)
            {
                var patternChar = _domainPattern[patternIndex];
                var stringToMatchChar = stringToMatch[stringToMatchIndex];
                if (patternChar == stringToMatchChar)
                {
                    stringToMatchIndex++;
                    patternIndex++;
                }
                else
                {
                    if (patternChar == '*')
                    {
                        return MatchInternal(stringToMatch, patternIndex, stringToMatchIndex + 1)
                            || MatchInternal(stringToMatch, patternIndex + 1, stringToMatchIndex);

                    }
                    else
                    {
                        return false;
                    }
                }
            }

            while (patternIndex < _domainPattern.Length && _domainPattern[patternIndex] == '*')
            {
                patternIndex++;
            }

            return stringToMatchIndex == stringToMatch.Length && patternIndex == _domainPattern.Length;
        }
    }
}
