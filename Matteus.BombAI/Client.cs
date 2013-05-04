using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Matteus.BombAI
{
	public class Client
	{
		/*
		private readonly List<List<Action>> rounds = new List<List<Action>>();
		private readonly List<Bomb> bombs = new List<Bomb>();
		private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();

		private Board board;
		private GameOptions gameOptions;
		*/

		private readonly Game game;

		public event EventHandler<BotCommandEventArgs> BotCommandRecivied;

		public bool IgnoreSelfUpdate
		{
			get;
			set;
		}

		public Client(Game game)
		{
			this.game = game;
		}

		public void Initialize()
		{
			int playerId, numberOfPlayers, maxTurns, boardWidth, boardHeight;
			if (!int.TryParse(Console.ReadLine(), out playerId) ||
				!int.TryParse(Console.ReadLine(), out numberOfPlayers) ||
				!int.TryParse(Console.ReadLine(), out maxTurns) ||
				!int.TryParse(Console.ReadLine(), out boardWidth) ||
				!int.TryParse(Console.ReadLine(), out boardHeight))
			{
				throw new Exception("Dude, you gotta follow the goddamn protocol. Behave!");
			}

			game.GameOptions = new GameOptions(playerId, numberOfPlayers, maxTurns, boardWidth, boardHeight);
			game.Board = new Board(boardWidth, boardHeight);
			game.Players.Add(playerId, new Player(playerId));

		}

		public virtual bool Read()
		{
			if(!ReadBoard()) return false;
			ReadPlayers();
			ReadBombs();
			ReadActions();

			return true;
		}

		protected virtual bool ReadBoard()
		{
			for (int y = 0; y < game.GameOptions.BoardHeight; y++)
			{
				String row = Console.ReadLine();
				if(String.IsNullOrEmpty(row)) return false;

				for (int x = 0; x < game.GameOptions.BoardWidth; x++)
				{
					game.Board[x, y].Type = ProtocolHelper.CharToTile(row[x]);
				}
			}

			return true;
		}

		protected virtual void ReadPlayers()
		{
			int playersToRead;
			if(!int.TryParse(Console.ReadLine().Trim(), out playersToRead))
			{
				throw new Exception("Failed to read players.");
			}

			for (int i = 0; i < playersToRead; i++)
			{
				String[] playerValues = Console.ReadLine().Trim().Split(' ');
				if(playerValues.Length == 3)
				{
					int id, x, y;
					if(!int.TryParse(playerValues[0], out id) ||
					   !int.TryParse(playerValues[1], out x) ||
					   !int.TryParse(playerValues[2], out y))
					{
						throw new Exception("Failed to read player values");
					}

					Player player;
					Position newPos = new Position(x, y);

					// Get the player with the provided id,
					// if there's no such player, create it.
					if(!game.Players.ContainsKey(id))
					{
						player = new Player(id);
						game.Players.Add(id, player);

						// Ignore the update if the property is set and this is the own player.
						if(IgnoreSelfUpdate && id == game.GameOptions.PlayerId) continue;
					}
					else
					{
						player = game.Players[id];
					}


					// Change position if it's a new one.
					// Also tell the tile we aren't there anymore.
					// Then add the player to its new tile.
					if(player.Position != newPos)
					{
						game.Board[player.Position].Players.Remove(player);
						player.Position = newPos;
						game.Board[player.Position].Players.Add(player);
					}
				}
			}
		}

		/// TODO: Don't create new bombs every round. Better just update the previous ones.
		protected virtual void ReadBombs()
		{
			int bombsToRead;
			if(!int.TryParse(Console.ReadLine().Trim(), out bombsToRead))
			{
				throw new Exception("Failed to read bombs.");
			}

			// Remove previous bombs.
			foreach(Tile tile in game.Board.Tiles()) tile.ThreatheningBombs.Clear();
			foreach(Player player in game.Players.Values) player.Bombs.Clear();
			game.Bombs.ForEach(x => game.Board[x.Position].Bombs.Remove(x));
			game.Bombs.Clear();

			for (int i = 0; i < bombsToRead; i++)
			{
				String[] bombValues = Console.ReadLine().Split(' ');
				if(bombValues.Length == 4)
				{
					int playerId, x, y, ticks;
					if(!int.TryParse(bombValues[0], out playerId) ||
					   !int.TryParse(bombValues[1], out x) ||
					   !int.TryParse(bombValues[2], out y) ||
					   !int.TryParse(bombValues[3], out ticks))
					{
						throw new Exception("Failed to read bomb values");
					}

					Bomb bomb = new Bomb(game.Players[playerId], new Position(x, y), ticks);
					game.Bombs.Add(bomb);
					game.Board[x, y].Bombs.Add(bomb);
					game.Players[playerId].Bombs.Add(bomb);
				}
			}

			// Add bomb threats
			foreach(Bomb bomb in game.Bombs)
				bomb.AddTheatToBoard(game.Board);
		}

		protected virtual void ReadActions()
		{
			List<Action> actions = new List<Action>();

			for (int i = 0; i < game.GameOptions.NumberOfPlayers; i++)
			{
				string[] actionValues = Console.ReadLine().Trim().Split(' ');
				if(actionValues.Length == 2)
				{
					int playerId;
					if(!int.TryParse(actionValues[0], out playerId))
						throw new Exception("Failed to read action: " + actionValues[0]);

					string cmdName = actionValues[1];

					if(cmdName == ProtocolConstants.DEAD)
						game.Players[playerId].Dead = true;
					else
					{
						Command command;
						if(!Command.TryParse(cmdName, out command))
						{
							int bombTicks;
							if(!int.TryParse(cmdName, out bombTicks))
							{
								throw new Exception("Failed to read command of action: " + cmdName);
							}

							command = new BombCommand(bombTicks);
						}

						actions.Add(new Action(game.Players[playerId], command));
					}
				}
			}

			game.Rounds.Add(actions);
		}

		protected virtual void OnBotCommandRecivied(Command command)
		{
			EventHandler<BotCommandEventArgs> tmp = BotCommandRecivied;
			if(tmp != null)
			{
				tmp(this, new BotCommandEventArgs(command));
			}
		}

		public void Run(IBot bot)
		{
			while(Read())
			{
				Command command = bot.GetCommand();
				Console.WriteLine(command);
				OnBotCommandRecivied(command);
			}
		}
	}
}

