using System;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class StringExtensions
    {
        public static string GetCommonPrefix(this string first, string second)
        {
            first = first ?? throw new ArgumentNullException(nameof(first));
            second = second ?? throw new ArgumentNullException(nameof(second));

            if (first == string.Empty || second == string.Empty)
            {
                return string.Empty;
            }

            if (first[0] != second[0])
            {
                return string.Empty;
            }

            if (first.Length > second.Length)
            {
                var tmp = first;
                first = second;
                second = tmp;
            }

            for (var i = 1; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    return first[..i];
                }
            }

            return first;
        }
    }
}
