using System;
using System.Linq;
using System.Collections.Generic;
using Matteus.BombAI.Skuni.PathFinding;
using System.Diagnostics;

namespace Matteus.BombAI.Skuni
{
	// TODO: Advanced avoiding (see game 13599)
	// TODO: Multiple bombs and trap tactics!

	/// <summary>
	/// Skuni, probably the finest robot in the world.
	/// </summary>
	public class Bot : IBot
	{
		private int round;
		private IGame game;
		private Player me;

		private Tile currentTile;
		private IEnumerable<Tile> neighbours;

		// Tools
		private Pathfinder pathfinder;
		private AreaFinder areafinder;

		/// <summary>
		/// Initializes a new instance of the <see cref="Matteus.BombAI.Skuni.Bot"/> class.
		/// </summary>
		/// <param name='game'>
		/// Got game?
		/// </param>
		public Bot(IGame game)
		{
			this.game = game;
		}
	
		/// <summary>
		/// Initialize this bot.
		/// </summary>
		public void Initialize()
		{
			me = game.Players[game.GameOptions.PlayerId];
			pathfinder = new Pathfinder(game.Board);
			areafinder = new AreaFinder(game.Board);
			round = 0;
		}

		/// <summary>
		/// Gets the command for this round.
		/// </summary>
		/// <returns>
		/// The command.
		/// </returns>
		public Command GetCommand()
		{
			round++;

			if(round == 176)
			{
				int a = 0;
			}

			// Update our current tile.
			currentTile = game.Board[me.Position];

			// Update our neighbours tiles.
			neighbours = game.Board.NeighboursTo(me.Position);

			// Skuni is intelligent as fuck.
			CalculateBombChainReactions();

			// Let's ensure that safety, babe.
			if(currentTile.ThreatheningBombs.Any(x => x.Ticks <= 2) || currentTile.Bombs.Any(x => x.Ticks <= 3))
			{
				// It looks like we will die if we stay here.
				// Better move then? Yep.
				return Move();
			}

			// Are we (still) isolated?
			if(Isolated())
			{
				// Dude, we're pretty much alone in a boring room..
				// Let's get out of here.
				return IsolationCommand();
			}

			// Not isolated huh?
			return TryToKillThemRobots();
		}

		/// <summary>
		/// Figures out the next command to kill or set up for a kill.
		/// </summary>
		/// <returns>
		/// Command to kill all enemies!
		/// </returns>
		private Command TryToKillThemRobots()
		{
			// If all neighbour tiles are about to blow,
			// then just take a break here this round.
			if(neighbours.All(x => !x.Walkable || x.ThreatheningBombs.Any(y => y.Ticks == 1)))
			{
				// I don't usually pass.
				// But when I do,
				// it's because shit's about to go down around me.
				return Command.Pass;
			}

			// Right now, let's keep it down to one bomb at a time.
			if(me.Bombs.Count > 0) return Move();

			// Before we do anything more, let's make sure we don't get trapped.
			if(CheckTrapRisk(currentTile))
			{
				return Move();
			}

			// If we place a bomb here. Can it kill somebody?
			var tiles = game.Board.DetonationTilesTo(me, 1);
			if(tiles.Any(x => x.Players.Any(y => !y.Dead && y != me)))
			{
				// Let's do that if it's safe.
				if(ItIsSafeToPlaceABombHere())
					return new BombCommand(5);
			}

			// Let's seek up those motherfuckers.
			var closestRobot = GetClosestRobot();
			if(closestRobot != null && GetPathToPlayer(closestRobot).Count > 1)
			{
				// Get the next tile in the path to that robot.
				Tile tile = GetPathToPlayer(closestRobot)[1];
				if(tile != null)
				{
					// Is that tile safe?
					if(!tile.ThreatheningBombs.Any())
					{
						// Take a step towards him.
						return MoveTowardsTile(tile);
					}
					else
					{
						// Wait here, it's safe then.
						Command cmd = StaySafe();
						if(cmd != null) return cmd;
					}
				}
			}

			// Dunno what to do, really. Dance.
			return Move();
		}

		/// <summary>
		/// Figure out a suitable command when isolated.
		/// </summary>
		/// <returns>
		/// Returns a command.
		/// </returns>
		private Command IsolationCommand()
		{
			if (me.Bombs.Count == 0)
			{
				if(ItIsSafeToPlaceABombHere() && WillHitAForceField())
				{
					return new BombCommand(5);
				}

				// Move towards a tile to break us free from isolation.

				// Find the closest robot (ignoring bombs and force fields).
				var closestRobot = game.Players.Values
					.Where(x => x != me && !x.Dead)
					.OrderBy(x => Position.Distance(currentTile.Position, x.Position))
					.FirstOrDefault();

				// Find the path to that robot (ignoring bombs and force fields).
				var path = pathfinder.FindPath(
					currentTile,
					game.Board[closestRobot.Position],
					x => x.Type == TileType.Floor || x.Type == TileType.ForceField);

				// Follow this path and we will run into a force field we want to get trough.
				if(path != null && path.Count > 0)
				{
					// Just make sure it's safe to go there!
					Tile tile = path[1];
					if(tile.ThreatheningBombs.Count == 0 || tile.ThreatheningBombs.All(x => x.Ticks > 1))
						return MoveTowardsTile(path[1]);
				}
			}

			// We already put out a bomb, or we could not find path, or the next tile of that path was not safe.
			return Move();
		}

		/// <summary>
		/// Determines if a bomb placed on the specified tile will
		/// hit any force fields. If no tile is passed, the current tile will be used.
		/// </summary>
		/// <returns>
		/// True if a force field will be hit, false otherwise.
		/// </returns>
		private bool WillHitAForceField(Tile tile = null)
		{
			if(tile == null) tile = currentTile;
			var tiles = game.Board.DetonationTilesTo(tile.Position, 1);
			return tiles.Any(x => x.Type == TileType.ForceField);
		}

		/// <summary>
		/// Determines if it's considered safe to place a bomb
		/// at the current tile.
		/// </summary>
		/// <returns>
		/// Whether it is safe to place a bomb here.
		/// </returns>
		private bool ItIsSafeToPlaceABombHere()
		{
			// If we place a bomb here, can we find a safe spot then?
			bool safety = false;

			// We gotta calculate bomb chain reactions, so save every previous tick.
			Dictionary<Bomb, int> actuallBombTicks = new Dictionary<Bomb, int>();
			foreach(Bomb b in game.Bombs) actuallBombTicks.Add(b, b.Ticks);

			// Simulate a bomb placement of all players
			List<Bomb> simulatingBombs = new List<Bomb>();
			foreach(Player player in game.Players.Values.Where(x => !x.Dead))
			{
				Bomb bomb = new Bomb(player, 5);
				game.Bombs.Add(bomb);
				currentTile.Bombs.Add(bomb);
				bomb.AddTheatToBoard(game.Board);
				simulatingBombs.Add(bomb);
			}

			// Now we can perform the calculation
			CalculateBombChainReactions();

			// Now find a safe spot!
			var tiles = areafinder.FindTiles(
				currentTile, 4,
				x => x == currentTile || x.Walkable
			).Where(x => x.ThreatheningBombs.Count == 0 && x != currentTile).ToList();

			// Is there a safe spot?
			if(tiles.Any())
			{
				// Is the path to any of that tiles safe? (test only four)
				foreach(var tile in tiles.Take(4))
				{
					IList<Tile> path = pathfinder.FindPath(currentTile, tile);
					if(IsPathSafe(path, -1))
					{
						safety = true;
						break;
					}
				}
			}

			// Reverse the changes done by our simulation
			foreach(Bomb bomb in simulatingBombs)
			{
				currentTile.Bombs.Remove(bomb);
				game.Bombs.Remove(bomb);
			}

			foreach(Tile t in game.Board.Tiles()) t.ThreatheningBombs.RemoveAll(x => simulatingBombs.Contains(x));
			foreach(var kvp in actuallBombTicks) kvp.Key.Ticks = kvp.Value;

			// No safe spot, it's not safe to place a bomb here.
			return safety;
		}

		/// <summary>
		/// If we move to the specified tile.
		/// Is there a risk of us getting trapped?
		/// </summary>
		/// <returns>
		/// True if there's a risk of getting trapped.
		/// False otherwise
		/// </returns>
		/// <param name='tile'>
		/// The tile to check the trap risk against.
		/// </param>
		private bool CheckTrapRisk(Tile tile)
		{
			// Let's pretend we stand at "tile".
			// If there are players on the neighbours of this tile,
			// they might place a bomb there.

			// Is every neighbour a player or a blocked tile?
			// if(neighbours.All(x => x.Players.Any() || !x.Walkable))
			if(game.Board.NeighboursTo(tile).All(x => x.Players.Any(p => p != me && !p.Dead) || !x.Walkable))
			{
				// There's definitely a risk of us getting caught if we move there.   
				return true;
			}

			// TODO: Add other trap risk cases

			return false;
		}


		/// <summary>
		/// Returns a command that with an attempt to move away from the current tile.
		/// But in a way to won't kill the robot.
		/// </summary>
		private Command Move()
		{
			return SafeNeighbour() ?? StaySafe() ?? GotoSafe() ?? MinorPanick() ?? Panick() ?? Command.Pass;
		}

		/// <summary>
		/// Returns a command that that moves to a safe neighbour (zero bomb threats).
		/// If no safe neighbour exists, it returns null.
		/// </summary>
		/// <returns>
		/// Movement command or null if no safe neighbour exists.
		/// </returns>
		private Command SafeNeighbour()
		{
			var tile = neighbours.RandomOrDefault(x => x.ThreatheningBombs.Count == 0 && x.Walkable);
			return MoveTowardsTileIfNoTrapRisk(tile);
		}

		/// <summary>
		/// Returns a command that moves to the safest neighbour.
		/// That means, the tile might still have a bomb threat.
		/// But it's considered the best option looking a single step ahead.
		/// </summary>
		/// <returns>
		/// Movement command.
		/// </returns>
		private Command SafestNeighbour()
		{
			return MoveTowardsTileIfNoTrapRisk(
				neighbours
					.Where(x => x.Walkable)
					.OrderByDescending(x => x.ThreatheningBombs.Count == 0 ? int.MaxValue : x.ThreatheningBombs.Min(y => y.Ticks))
					.FirstOrDefault()
			);
		}

		/// <summary>
		/// Returns pass command if the current tile is safe and without trap risk.
		/// </summary>
		/// <returns>
		/// Returns pass command if the current tile is safe, null otherwise.
		/// </returns>
		private Command StaySafe()
		{
			if(currentTile.ThreatheningBombs.Count == 0 && !CheckTrapRisk(currentTile))
			{
				return Command.Pass;
			}

			return null;
		}

		/// <summary>
		/// Finds the closest safe tile and returns a command to move
		/// towards that tile.
		/// </summary>
		/// <returns>
		/// A movement command if a safe tile was found, null otherwise.
		/// </returns>
		private Command GotoSafe()
		{
			// So all neighbour tiles are in threat.

			// Find us safe tiles (don't try more than 6) and order them by distance!
			var safeTiles = areafinder.FindTiles(
				currentTile, 6,
				x => x == currentTile || x.Walkable
			)
			.Where(x => x.ThreatheningBombs.Count == 0 && x != currentTile)
			.OrderBy(x => Position.Distance(currentTile.Position, x.Position))
			.Take(6);

			foreach(Tile safeTile in safeTiles)
			{
				// Find the path to that tile.
				var path = pathfinder.FindPath(currentTile, safeTile);

				// Did we find the path?
				if(path != null && path.Count > 1 && IsPathSafe(path))
				{
					// Okay, the path seems cool.
					return MoveTowardsTile(path[1]);
				}
			}

			return null;
		}

		private bool IsPathSafe(IList<Tile> path, int ticksOffset = 0)
		{
			for (int i = 1; i < path.Count; i++)
			{
				Tile tile = path[i];

				if(tile.ThreatheningBombs.Any(x => x.Ticks - i + ticksOffset == 0))
				{
					return false;
				}

				// TODO: Can we add a trap risk check to all ticks?
				if(i + ticksOffset == 1 && CheckTrapRisk(tile))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Should be called when the robot cannot flee to a safe tile, because of bombs exploding.
		/// This method main objective is to survive the bombs it's isolated by.
		/// </summary>
		/// <returns>
		/// A command that tries to keep the robot alive.
		/// </returns>
		private Command MinorPanick()
		{
			// Find nearby tiles we can walk to and order them by descending threat ticks.
			var tiles = areafinder
				.FindTiles(currentTile, 3, x => x.Walkable || x == currentTile)
				.Where(x => x.ThreatheningBombs.Count > 0)
				.OrderByDescending(x => x.ThreatheningBombs.Min(y => y.Ticks));


			foreach(var tile in tiles.Take(6))
			{
				var path = pathfinder.FindPath(currentTile, tile);
				if(path != null && path.Count > 1 && IsPathSafe(path))
				{
					return MoveTowardsTile(path[1]);
				}
			}

			return null;
		}

		/// <summary>
		/// Returns a movement command in a randomized direction.
		/// Returns null if all directions are blocked.
		/// </summary>
		private Command Panick()
		{
			var tile = neighbours.RandomOrDefault(x => x.Walkable);
			if(tile != null) return MoveTowardsTile(tile);

			return null;
		}

		private Command MoveTowardsTileIfNoTrapRisk(Tile tile)
		{
			if(tile == null) return null;

			// Just because there ain't no bomb gonna explode on that tile now,
			// someone might still trap us there. Better make sure that's not the case.
			if(CheckTrapRisk(tile))
			{
				/*
				// Hmmm.. that's dangerous.
				// We could place a bomb and hope to trap some other player there.
				// But hey, safety first!
				Command command = StaySafe();
				if(command == null)
				{
					// We will die if we stay here.
					// Best option is to move on, and pray we don't get trapped.
					if(currentTile.ThreatheningBombs.Min(x => x.Ticks) == 1)
						return MoveTowardsTile(tile);
				}
				else
				{
					// We can stay here and chill one round,
					// to see how this shit turns out.
					return command;
				}
				*/
				return null;
			}

			return MoveTowardsTile(tile);
		}

		/// <summary>
		/// Moves the towards the specified tile.
		/// This method should only be used if the specified tile
		/// is a neighbour tile and is movable.
		/// </summary>
		/// <returns>
		/// The movment command, or pass if the player is already on the specified tile.
		/// </returns>
		/// <param name='tile'>
		/// Tile to move to. Should be a neighbour tile. 
		/// </param>
		private Command MoveTowardsTile(Tile tile)
		{
			if(tile.Position.X < me.Position.X) return Command.MoveLeft;
			if(tile.Position.X > me.Position.X) return Command.MoveRight;
			if(tile.Position.Y < me.Position.Y) return Command.MoveUp;
			if(tile.Position.Y > me.Position.Y) return Command.MoveDown;

			return Command.Pass;
		}

		/// <summary>
		/// Determines whether the robot is isolated.
		/// I.e the robot can't move up to another robot.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this robot isolated ; otherwise, <c>false</c>.
		/// </returns>
		private bool Isolated()
		{
			/// TODO: STILL not very efficient.

			// Loop through each player (except own robot)
			// and see if we can find a path to them.

			/*
			foreach (var player in game.Players.Values)
			{
				if(player == me || player.Dead) continue;

				// Get path
				var tilePath = GetPathToPlayer(player);

				// If we found a path, we are not isolated.
				if(tilePath != null) return false;
			}
			*/

			// Find all tiles we can walk on, or with bombs. Then see if there's any other player than me on them.
			if(areafinder.FindTiles(currentTile, int.MaxValue, x => x.Walkable || x.Bombs.Count > 0)
			   .Any(x => x.Players.Any(p => p != me && !p.Dead)))
			{
				return false;
			}

			// We could not find a path to any other player,
			// we are indeed isolated.
			return true;
		}

		private Player GetClosestRobot()
		{
			return GetAllPathsToPlayers()
				.Where(x => x.Value != null && !x.Key.Dead)
				.OrderBy(x => x.Value.Count)
				.Select(x => x.Key)
				.FirstOrDefault();
		}

		/// <summary>
		/// Gets the path to player.
		/// </summary>
		/// <returns>
		/// The path to player.
		/// </returns>
		/// <param name='player'>
		/// Player.
		/// </param>
		private IList<Tile> GetPathToPlayer(Player player)
		{
			// Get tiles
			Tile fromTile = game.Board[me.Position];
			Tile toTile = game.Board[player.Position];

			// Now, find path from "fromTile" to "toTile".
			var tilePath = pathfinder.FindPath(fromTile, toTile, x => x.Walkable || x.Players.Contains(player));

			return tilePath;
		}


		/// <summary>
		/// Gets all paths to players.
		/// </summary>
		/// <returns>
		/// All paths to players.
		/// </returns>
		private Dictionary<Player, IList<Tile>> GetAllPathsToPlayers()
		{
			Dictionary<Player, IList<Tile>> pathsToPlayers = new Dictionary<Player, IList<Tile>>();

			foreach(Player player in game.Players.Values)
			{
				if(player == me || player.Dead) continue;

				pathsToPlayers.Add(player, GetPathToPlayer(player));
			}

			return pathsToPlayers;
		}

		/// <summary>
		/// Finds every bomb chain reaction and sets all bombs in each reaction to its lowest tick count.
		/// So if a bomb has 5 ticks left, but will be hit by another bomb in 3 ticks; this method will set
		/// that bomb to 3 ticks. It's brilliant.
		/// </summary>
		private void CalculateBombChainReactions()
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

