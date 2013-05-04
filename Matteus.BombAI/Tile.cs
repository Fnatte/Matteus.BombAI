using System;
using System.Collections.Generic;

namespace Matteus.BombAI
{
	public class Tile
	{
		public TileType Type {
			get;
			set;
		}

		public Position Position {
			get;
			private set;
		}

		private readonly List<Player> players = new List<Player>();
		private readonly List<Bomb> bombs = new List<Bomb>();
		private readonly List<Bomb> threatheningBombs = new List<Bomb>();

		public List<Player> Players { get { return players; } }
		public List<Bomb> Bombs { get { return bombs; } }
		public List<Bomb> ThreatheningBombs { get { return threatheningBombs; } }

		public bool Walkable
		{
			get { return Type == TileType.Floor && Bombs.Count == 0; }
		}

		public Tile(TileType type, Position position)
		{
			this.Type = type;
			this.Position = position;
		}
	}
}

