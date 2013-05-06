using System;
using System.IO;

namespace Matteus.BombAI.Afmen
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			if (args.Length > 0 && File.Exists(args[0]))
			{
				StreamReader sr = new StreamReader(args[0]);
				Console.SetIn(sr);
			}

			Game game = new Game();
			Client client = new Client(game);
			Bot bot = new Bot(game);

			client.Initialize();
			bot.Initialize();

			client.Run(bot);
		}
	}
}
