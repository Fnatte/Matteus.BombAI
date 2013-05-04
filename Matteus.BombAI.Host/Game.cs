using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Matteus.BombAI.Host
{
	public class Game
	{
		private readonly List<Player> players = new List<Player>();
		private readonly int numberOfPlayers, maxTurns, boardWidth, boardHeight;
		private readonly List<Bomb> bombs = new List<Bomb>();
		private Board board;

		public Board Board { get { return board; } }
		public ReadOnlyCollection<Bomb> Bombs { get { return bombs.AsReadOnly(); } }
		public ReadOnlyCollection<Player> Players { get { return players.AsReadOnly(); } }
		public int NumberOfPlayers { get { return numberOfPlayers; } }
		public int MaxTurns { get { return maxTurns; } }
		public int BoardWidth { get { return boardWidth; } }
		public int BoardHeight { get { return boardHeight; } }

		public Game(int numberOfPlayers, int maxTurns, int boardWidth, int boardHeight)
		{
			this.numberOfPlayers = numberOfPlayers;
			this.maxTurns = maxTurns;
			this.boardWidth = boardWidth;
			this.boardHeight = boardHeight;

			board = new Board(boardWidth, boardHeight);
		}

		public void PrepareClient(Client client)
		{
			if(client.Player == null)
			{
				Player player = new Player(players.Count);
				player.Position = new Position(2, 2);
				client.Player = player;
				players.Add(player);
			}
		}

		public void OutputHeader(Client client)
		{
			client.WriteLine(client.Player.Id);
			client.WriteLine(numberOfPlayers);
			client.WriteLine(maxTurns);
			client.WriteLine(boardWidth);
			client.WriteLine(boardHeight);
		}

		public void OutputRound(Client client, Round round)
		{
			client.WriteLine(Board);

			// Players
			client.WriteLine(round.PlayersAlive.Count);
			foreach (var player in round.PlayersAlive)
			{
				client.WriteLine("{0} {1} {2}", player.Id, player.Position.X, player.Position.Y);
			}

			// Bombs
			client.WriteLine(0);

			// Actions
			foreach (var action in round.Actions)
			{
				client.WriteLine(
					"{0} {1}",
					action.Player.Id,
					action.Command is BombCommand ?
						((BombCommand)action.Command).Ticks.ToString() :
						action.Command.Name
				);
			}
		}
	}
}

