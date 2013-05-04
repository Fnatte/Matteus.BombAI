using System;

namespace Matteus.BombAI
{
	public class Action
	{
		private readonly Player player;
		private readonly Command command;

		public Player Player { get { return player; } }
		public Command Command { get { return command; } }

		public Action(Player player, Command command)
		{
			this.player = player;
			this.command = command;
		}
	}
}

