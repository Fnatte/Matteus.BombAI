using System;
using System.IO;
using System.Text;

namespace Matteus.BombAI.ConsoleDumpPlayer
{
	class MainClass
	{
		private static TextReader stdin;

		public static void Main(string[] args)
		{
			if(!File.Exists("dump"))
			{
				Console.WriteLine("Could not find file \"dump\".");
				return;
			}

			stdin = Console.In;

			StreamReader sr = new StreamReader("dump");
			Console.SetIn(sr);

			Game game = new Game();
			Client client = new Client(game);
			client.Initialize();

			while(client.Read())
			{
				// Draw round to console
				Console.Clear();

				// Board
				Console.WriteLine(game.Board);
				int boardEndX = Console.CursorLeft;
				int	boardEndY = Console.CursorTop;

				// Players
				foreach(var player in game.Players.Values)
				{
					if(player.Id == -1) continue;

					Console.SetCursorPosition(player.Position.X, player.Position.Y);
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Write(player.Dead ? "D" : player.Id.ToString());
					Console.ResetColor();
				}

				// Bombs
				foreach(var bomb in game.Bombs)
				{
					Console.SetCursorPosition(bomb.Position.X, bomb.Position.Y);
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write('X');
					Console.ResetColor();
				}

				Console.SetCursorPosition(boardEndX, boardEndY);

				// Info
				Console.WriteLine();
				Console.WriteLine("Round: {0}/{1}", game.Rounds.Count, game.GameOptions.MaxTurns);
				Console.WriteLine("Players:");
				foreach(Player player in game.Players.Values)
				{
					if(player.Id == -1) continue;
					Console.WriteLine(
						"   Id:{0}, X:{1}, Y:{2}, Bombs:{3}",
					    player.Id, player.Position.X, player.Position.Y, player.Bombs.Count
					);
				}
					

				// Read input from console
				Console.SetIn(stdin);
				Console.ReadLine();
				Console.SetIn(sr);
			}
		}
	}
}
