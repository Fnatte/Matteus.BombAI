using System;

namespace Matteus.BombAI
{
	public struct Position
	{
		private readonly int x;
		private readonly int y;

		public int X { get { return x; } }
		public int Y { get { return y; } }

		public readonly static Position Zero = new Position(0, 0);

		public Position(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public static int Distance(Position p1, Position p2)
		{
			return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
		}

		public static Position operator +(Position p1, Position p2)
		{
			return new Position(p1.X + p2.X, p1.Y + p2.Y);
		}

		public static Position operator -(Position p1, Position p2)
		{
			return new Position(p1.X - p2.X, p1.Y - p2.Y);
		}

		/*
		 * Shit has gotta be comparable
		 */

		public override bool Equals(object obj)
		{
			return obj is Position && this == (Position)obj;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public static bool operator ==(Position p1, Position p2)
		{
			return p1.X == p2.X && p1.Y == p2.Y;
		}

		public static bool operator !=(Position p1, Position p2)
		{
			return !(p1 == p2);
		}
	}
}

