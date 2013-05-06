using System;
using System.Linq;
using Matteus.BombAI.Extra;
using Matteus.BombAI.Extra.PathFinding;
using System.Collections.Generic;

namespace Matteus.BombAI.Afmen
{
	/// <summary>
	/// This bot just tries to survive.
	/// </summary>
	public class Bot : IBot
	{
		private readonly Game game;

		private Player me;
		private Tile meTile;
		private AreaFinder areafinder;
		private Pathfinder pathfinder;


		public Bot(Game game)
		{
			this.game = game;
		}

		public void Initialize()
		{
			me = game.Players[game.GameOptions.PlayerId];
			areafinder = new AreaFinder(game.Board);
			pathfinder = new Pathfinder(game.Board);
		}

		public Command GetCommand()
		{
			// Variable updates
			meTile = game.Board[me.Position];

			// Pretend every other player places a bomb.
			foreach(var player in game.Players.Values.Where(x => !x.Dead && x != me))
			{
				Bomb bomb = new Bomb(player, 6);
				game.Bombs.Add(bomb);
				game.Board[bomb.Position].Bombs.Add(bomb);
				bomb.AddTheatToBoard(game.Board);
			}

			// Now calculate bomb chain reactions
			game.CalculateBombChainReactions();

			// If this tile is safe, stay.
			if(meTile.ThreatheningBombs.Count == 0)
				return Command.Pass;

			// Find safe tiles and order them by distance
			var safeTiles = areafinder
				.FindTiles(meTile, int.MaxValue, x => x == meTile || x.Walkable)
				.Where(x => x.ThreatheningBombs.Count == 0)
				.OrderBy(x => Position.Distance(x.Position, meTile.Position));

			// See if the path towards that tile is safe
			foreach(var safeTile in safeTiles)
			{
				var path = pathfinder.FindPath(meTile, safeTile);
				if(path != null && path.Count > 1 && IsPathSafe(path))
				{
					return MoveTowardsTile(path[1]);
				}
			}

			return Command.Pass;
		}

		private Command MoveTowardsTile(Tile tile)
		{
			int x = meTile.Position.X;
			int y = meTile.Position.Y;
			int x2 = tile.Position.X;
			int y2 = tile.Position.Y;
			if(x2 > x) return Command.MoveRight;
			if(x2 < x) return Command.MoveLeft;
			if(y2 > y) return Command.MoveDown;
			if(y2 < y) return Command.MoveUp;

			// We are already on that tile, so pass.
			return Command.Pass;
		}

		private bool IsPathSafe(IList<Tile> path)
		{
			int step = 1;
			foreach(Tile tile in path.Skip(1))
			{
				if(tile.ThreatheningBombs.Any(x => x.Ticks - step == 0))
					return false;
			}

			return true;
		}

	}
}

