using System;
using System.Linq;

namespace Matteus.BombAI
{
	public class Bomb
	{
		private readonly Position position;
		private readonly Player player;

		public Position Position {
			get { return position; }
		}

		public Player Player {
			get { return player; }
		}

		public int Ticks {
			get;
			set;
		}

		public Bomb(Player player, Position position, int ticks)
		{
			this.position = position;
			this.player = player;
			this.Ticks = ticks;
		}

		public Bomb(Player player, int ticks) : this(player, player.Position, ticks)
		{

		}

		public void AddTheatToBoard(Board board)
		{
			// Get number of bombs at this tile.
			int bombCount = board[Position].Bombs.Count;

			// Add threat to nearby tiles.
			var detonationTiles = board.DetonationTilesTo(Position.X, Position.Y, bombCount);
			foreach (var tile in detonationTiles)
			{
				tile.ThreatheningBombs.Add(this);
			}
		}
	}
}

