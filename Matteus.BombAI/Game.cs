using System;
using System.Collections.Generic;

namespace Matteus.BombAI
{
	public class Game : IGame
	{
		private Board board;
		private GameOptions gameOptions;
		private readonly List<List<Action>> rounds = new List<List<Action>>();
		private readonly List<Bomb> bombs = new List<Bomb>();
		private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();

		public List<List<Action>> Rounds { get { return rounds; } }
		public List<Bomb> Bombs { get { return bombs; } }
		public Dictionary<int, Player> Players { get { return players; } }
		public Board Board { get { return board; } set { board = value; } }
		public GameOptions GameOptions { get { return gameOptions; } set { gameOptions = value; } }

		public Game()
		{
		}
	}
}

