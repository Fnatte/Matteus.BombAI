using System;

namespace Matteus.BombAI
{
	public static class RandomHelper
	{
		private static readonly Random instance = new Random();
		public static Random Instance { get { return instance; } }
	}
}

