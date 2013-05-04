using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Matteus.BombAI.Host
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Game game = new Game(1, 10, 5, 5);

			List<Client> clients = new List<Client>();
			List<Round> rounds = new List<Round>();

			clients.Add(Client.Start(
				"/usr/bin/mono",
				"/home/matteus/Development/Matteus.AIContest/Matteus.AIContest.Skuni/bin/Debug/Matteus.AIContest.Skuni.exe"
			));

			clients.ForEach(x => game.PrepareClient(x));
			clients.ForEach(x => game.OutputHeader(x));

			rounds.Add(Round.CreateUsingClients(game, clients, false));

			clients.ForEach(x => game.OutputRound(x, rounds.Last()));

			for (int i = 0; i < 10; i++)
			{
				rounds.Add(Round.CreateUsingClients(game, clients));
				clients.ForEach(x => game.OutputRound(x, rounds.Last())); 
			}

			Console.ReadLine();
		}
	}
}
