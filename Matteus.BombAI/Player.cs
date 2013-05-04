using System;
using System.Collections.Generic;

namespace Matteus.BombAI
{
	public class Player
	{
		private readonly int id;
		public int Id
		{
			get { return id; }
		}

		public Position Position {
			get;
			set;
		}

		public bool Dead {
			get;
			set;
		}

		private readonly List<Bomb> bombs = new List<Bomb>();
		public List<Bomb> Bombs { get { return bombs; } }

		public Player(int id)
		{
			this.id = id;
		}
	}
}

