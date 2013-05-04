using System;
using System.IO;
using System.Diagnostics;

namespace Matteus.BombAI.Skuni
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			bool ignoreSelfUpdate = false;

			if(args.Length > 0 && File.Exists(args[0]))
			{
				StreamReader sr = new StreamReader(args[0]);
				Console.SetIn(sr);
				ignoreSelfUpdate = false;
			}

			TextWriterTraceListener consoleWriter = new TextWriterTraceListener(Console.Out);
			Debug.Listeners.Add(consoleWriter);

			Game game = new Game();
			Bot bot = new Bot(game);
			Client client = new Client(game);

			if(ignoreSelfUpdate)
			{
				client.IgnoreSelfUpdate = ignoreSelfUpdate;
				client.BotCommandRecivied += (sender, e) => {
					Player player = game.Players[game.GameOptions.PlayerId];

					if(e.Command == Command.MoveLeft) player.Position += new Position(1, 0);
					else if(e.Command == Command.MoveRight) player.Position += new Position(-1, 0);
					else if(e.Command == Command.MoveDown) player.Position += new Position(0, 1);
					else if(e.Command == Command.MoveUp) player.Position += new Position(0, -1);

					// Validate position
					if(player.Position.X < 0)
						player.Position = new Position(0, player.Position.Y);
					else if(player.Position.X >= game.GameOptions.BoardWidth)
						player.Position = new Position(game.GameOptions.BoardWidth - 1, player.Position.Y);
					else if(player.Position.Y < 0)
						player.Position = new Position(player.Position.X, 0);
					else if(player.Position.Y >= game.GameOptions.BoardHeight)
						player.Position = new Position(player.Position.X, game.GameOptions.BoardWidth - 1);
				};
			}

			client.Initialize();
			bot.Initialize();

			client.Run(bot);
		}
	}
}
