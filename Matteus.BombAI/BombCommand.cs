using System;

namespace Matteus.BombAI
{
	public class BombCommand : Command
	{
		private readonly int ticks;
		public int Ticks { get { return ticks; } }

		public BombCommand(int ticks) : base("bomb")
		{
			this.ticks = ticks;
		}

		public override string ToString()
		{
			return ticks.ToString();
		}
	}
}

