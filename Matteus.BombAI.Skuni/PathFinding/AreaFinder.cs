using System;
using System.Linq;
using System.Collections.Generic;

namespace Matteus.BombAI.Skuni.PathFinding
{
	public sealed class AreaFinder
	{
		private readonly Board board;

		public AreaFinder(Board board)
		{
			this.board = board;
		}

		public IList<Tile> FindTiles(Tile startTile, int maxSteps, Func<Tile, bool> tileCondition)
		{
			List<Tile> visited = new List<Tile>();
			List<Tile> tiles = new List<Tile>();
			Queue<Tile> queue = new Queue<Tile>();
			queue.Enqueue(startTile);

			Tile currentTile;
			while(queue.Count > 0)
			{
				currentTile = queue.Dequeue();
				visited.Add(currentTile);

				// Is this a vaild tile?
				if(tileCondition(currentTile))
				{
					// Apperently so, add it.
					tiles.Add(currentTile);

					// Add it's unvisited neighbours to the queue.
					foreach(var tile in board.NeighboursTo(currentTile).Where(x => !visited.Contains(x)))
					{
						if(Distance(startTile, tile) <= maxSteps)
							queue.Enqueue(tile);
					}
				}
			}

			return tiles;
		}

		private int Distance(Tile tile1, Tile tile2)
		{
			return Math.Abs(tile1.Position.X - tile2.Position.X) + Math.Abs(tile1.Position.Y - tile2.Position.Y);
		}
	}
}

