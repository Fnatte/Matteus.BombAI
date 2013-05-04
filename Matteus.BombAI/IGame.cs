using System;
using System.Collections.Generic;

namespace Matteus.BombAI
{
	public interface IGame
	{
		GameOptions GameOptions { get; }
		List<List<Action>> Rounds { get; }
		List<Bomb> Bombs { get; }
		Dictionary<int, Player> Players { get; }
		Board Board { get; }
	}
}

