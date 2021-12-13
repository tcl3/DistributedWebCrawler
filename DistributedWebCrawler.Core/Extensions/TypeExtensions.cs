using System;
using System.Collections.Generic;
using System.Linq;

namespace DistributedWebCrawler.Core.Extensions
{
    public static class TypeExtensions
	{
		public static IEnumerable<Type> GetBaseTypes(this Type type)
		{
			if (type?.BaseType == null) return Enumerable.Empty<Type>();

			var types = new List<Type>();
			var currentType = type.BaseType;
			do
			{
				types.Add(currentType);
				currentType = currentType.BaseType;
			} while (currentType != null);

			return types;
		}
	}
}
