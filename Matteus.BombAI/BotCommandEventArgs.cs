using System;

namespace Matteus.BombAI
{
	[Serializable]
	public sealed class BotCommandEventArgs : EventArgs
	{
		private readonly Command command;
		public Command Command
		{
			get { return command; }
		}

		public BotCommandEventArgs(Command command)
		{
			this.command = command;
		}
	}
}

