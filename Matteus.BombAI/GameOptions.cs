using System;

namespace Matteus.BombAI
{
	public struct GameOptions
	{
		private readonly int playerId, numberOfPlayers, maxTurns, boardWidth, boardHeight;

		public int PlayerId { get { return playerId; } }
		public int NumberOfPlayers { get { return numberOfPlayers; } }
		public int MaxTurns { get { return maxTurns; } }
		public int BoardWidth { get { return boardWidth; } }
		public int BoardHeight { get { return boardHeight; } }

		public GameOptions(int playerId, int numberOfPlayers, int maxTurns, int boardWidth, int boardHeight)
		{
			this.playerId = playerId;
			this.numberOfPlayers = numberOfPlayers;
			this.maxTurns = maxTurns;
			this.boardWidth = boardWidth;
			this.boardHeight = boardHeight;
		}
	}
}

