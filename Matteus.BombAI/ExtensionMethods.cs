using System;
using System.Linq;
using System.Collections.Generic;

namespace Matteus.BombAI
{
	public static class ExtensionMethods
	{
		public static T RandomOrDefault<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
		{
			List<T> list = new List<T>(enumerable.Where(predicate));
			if(list.Count == 0) return default(T);
			return list[RandomHelper.Instance.Next(0, list.Count)];
		}
	}
}

