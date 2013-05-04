using System;
using System.Text;
using System.Collections.Generic;

namespace Matteus.BombAI
{
	public class Board
	{
		private readonly List<Tile> allTiles = new List<Tile>();
		private readonly Tile[][] tiles;
		private readonly int height;
		private readonly int width;

		public int Height { get { return height; } }
		public int Width { get { return width; } }

		public Tile this[int x, int y] {
			get {
				return tiles[y][x];
			}
		}

		public Tile this[Position pos] {
			get {
				return tiles[pos.Y][pos.X];
			}
		}

		public Board(int width, int height)
		{
			this.height = height;
			this.width = width;
			tiles = new Tile[height][];

			for (int y = 0; y < height; y++)
			{
				tiles[y] = new Tile[width];
				for (int x = 0; x < width; x++)
				{
					Tile tile = new Tile(
						TileType.Floor,
						new Position(x, y)
					);

					tiles[y][x] = tile;
					allTiles.Add(tile);
				}
			}
		}

		public bool IsVaildPosition(int x, int y)
		{
			return x >= 0 && y >= 0 && x < width && y < height;
		}

		public IEnumerable<Tile> Tiles()
		{
			return allTiles;
		}

		public IEnumerable<Tile> NeighboursTo(Player player)
		{
			return NeighboursTo(player.Position);
		}

		public IEnumerable<Tile> NeighboursTo(Tile tile)
		{
			return NeighboursTo(tile.Position.X, tile.Position.Y);
		}

		public IEnumerable<Tile> NeighboursTo(Position position)
		{
			return NeighboursTo(position.X, position.Y);
		}

		public IEnumerable<Tile> NeighboursTo(int x, int y)
		{
			List<Tile> tiles = new List<Tile>();

			if(x > 0)
				tiles.Add(this[x - 1, y]);
			if(y > 0)
				tiles.Add(this[x, y - 1]);

			if(x < width)
				tiles.Add(this[x + 1, y]);
			if(y < height)
				tiles.Add(this[x, y + 1]);

			return tiles;
		}

		public IEnumerable<Tile> DetonationTilesTo(Player player, int bombCount = 1)
		{
			return DetonationTilesTo(player.Position.X, player.Position.Y, bombCount);
		}

		public IEnumerable<Tile> DetonationTilesTo(int centerX, int centerY, int bombCount = 1)
		{
			List<Tile> tiles = new List<Tile>();
			int range = 2 + bombCount;

			if(!IsVaildPosition(centerX, centerY)) return tiles;

			// Add center of explosion.
			tiles.Add(this[centerX, centerY]);


			// Go upwards
			for (int y = centerY-1; y > centerY-range-1; y--)
			{
				if(IsVaildPosition(centerX, y))
				{
					Tile tile = this[centerX, y];
					tiles.Add(tile);
					if(tile.Type != TileType.Floor)
						break;
				}
			}

			// Go downwards
			for (int y = centerY+1; y < centerY+range+1; y++)
			{
				if(IsVaildPosition(centerX, y))
				{
					Tile tile = this[centerX, y];
					tiles.Add(tile);
					if(tile.Type != TileType.Floor)
						break;
				}
			}

			// Go left
			for (int x = centerX-1; x > centerX-range-1; x--)
			{
				if(IsVaildPosition(x, centerY))
				{
					Tile tile = this[x, centerY];
					tiles.Add(tile);
					if(tile.Type != TileType.Floor)
						break;
				}
			}

			// Go right
			for (int x = centerX+1; x < centerX+range+1; x++)
			{
				if(IsVaildPosition(x, centerY))
				{
					Tile tile = this[x, centerY];
					tiles.Add(tile);
					if(tile.Type != TileType.Floor)
						break;
				}
			}

			return tiles;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					sb.Append(ProtocolHelper.TileToChar(this[x,y].Type));
				}

				if(y+1!=height)
					sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}

