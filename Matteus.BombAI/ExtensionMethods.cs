using System;
using System.Linq;
using System.Collections.Generic;

namespace Matteus.BombAI
{
	public static class ExtensionMethods
	{
		public static T RandomOrDefault<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate = null)
		{
			var e = predicate != null ? enumerable.Where(predicate) : enumerable;
			List<T> list = new List<T>(e);
			if(list.Count == 0) return default(T);
			return list[RandomHelper.Instance.Next(0, list.Count)];
		}
	}
}

