using System;

namespace Matteus.BombAI
{
	public static class ProtocolHelper
	{
		public static TileType CharToTile(char c)
		{
			switch(c)
			{
				case '#': return TileType.Wall;
				case '+': return TileType.ForceField;
				case '.': return TileType.Floor;
				default: throw new Exception("Unrecognized tile type: " + c);
			}
		}

		public static char TileToChar(TileType tileType)
		{
			switch(tileType)
			{
				case TileType.Wall: return '#';
				case TileType.ForceField: return '+';
				case TileType.Floor: return '.' ;
				default: throw new Exception("Unrecognized tile type: " + tileType);
			}
		}
	}
}

