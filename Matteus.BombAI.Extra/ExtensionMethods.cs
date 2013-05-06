using System;
using System.Linq;

namespace Matteus.BombAI.Extra
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Finds every bomb chain reaction and sets all bombs in each reaction to its lowest tick count.
		/// So if a bomb has 5 ticks left, but will be hit by another bomb in 3 ticks; this method will set
		/// that bomb to 3 ticks. It's brilliant.
		/// </summary>
		public static void CalculateBombChainReactions(this Game game)
		{
			// TODO: This could be the stupidest way to do this. But dude, it works.


			// So as long as we change ticks of any bomb, we continue.
			// We gotta do this so that we get second reactions and beyond.
			while(true)
			{
				bool changedTicksOfSomeBomb = false;
				foreach(Bomb bomb in game.Bombs)
				{
					// Get the bombs this bomb will hit (including itself)
					var tile = game.Board[bomb.Position];
					var bombsItWillHit =
						game.Board.DetonationTilesTo(bomb.Position, tile.Bombs.Count)
						.SelectMany(x => x.Bombs);

					// If the bomb is hitting any other bomb than itself
					if(bombsItWillHit.Skip(1).Any())
					{
						// If the bombs don't all have the same ticks
						int firstTicks = bombsItWillHit.First().Ticks;
						if(bombsItWillHit.Skip(1).Any(x => x.Ticks != firstTicks))
						{
							// Then set all the bombs tick count to the lowest one.
							changedTicksOfSomeBomb = true;
							int minTicks = bombsItWillHit.Min(x => x.Ticks);
							foreach(Bomb b in bombsItWillHit) b.Ticks = minTicks;
						}
					}
				}

				if(!changedTicksOfSomeBomb)
					break;
			}
		}
	}
}

