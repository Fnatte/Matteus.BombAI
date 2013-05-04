using System;
using System.Linq;
using System.Collections.Generic;
using Matteus.BombAI.Skuni.PathFinding;
using System.Diagnostics;

namespace Matteus.BombAI.Skuni
{
	// TODO: Calculate chain reactions.
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

			// Update our current tile.
			currentTile = game.Board[me.Position];

			// Update our neighbours tiles.
			neighbours = game.Board.NeighboursTo(me.Position);

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
			if(neighbours.All(x => x.ThreatheningBombs.Any(y => y.Ticks == 1)))
			{
				// I don't usually pass.
				// But when I do,
				// it's because shit's about to go down around me.
				return Command.Pass;
			}

			// Right now, let's keep it down to one bomb at a time.
			if(me.Bombs.Count > 0) return Move();

			// If we place a bomb here. Can it kill somebody?
			var tiles = game.Board.DetonationTilesTo(me, 1);
			if(tiles.Any(x => x.Players.Any(y => !y.Dead && y != me)))
			{
				// Let's do that if it's safe.
				if(ItIsSafeToPlaceABombHere())
					return new BombCommand(5);
			}

			// Let's seek up those motherfuckers.
			var closestRobot = GetAllPathsToPlayers()
				.Where(x => x.Value != null && !x.Key.Dead)
				.OrderBy(x => x.Value.Count)
				.Select(x => x.Key)
				.FirstOrDefault();
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
			if (me.Bombs.Count == 0 && ItIsSafeToPlaceABombHere() && WillHitAForceField())
			{
				// So we could find a safe spot.
				// Then let's place that bomb right here!
				return new BombCommand(5);
			}
			else
			{
				// TODO: Find a spot that will hit a force field and move towards it.

				// We could not find a safe spot.
				// Better not place a bomb here then!
				return Move();
			}
		}

		/// <summary>
		/// Determines if a bomb placed on the current tile will
		/// hit any force fields.
		/// </summary>
		/// <returns>
		/// True if a force field will be hit, false otherwise.
		/// </returns>
		private bool WillHitAForceField()
		{
			var tiles = game.Board.DetonationTilesTo(me, 1);
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

			// Simulate a bomb placement
			Bomb bomb = new Bomb(me, 5);
			currentTile.Bombs.Add(bomb);
			bomb.AddTheatToBoard(game.Board);

			// Now find a safe spot!
			var tiles = areafinder.FindTiles(
				currentTile, 4,
				x => x == currentTile || x.Walkable
			).Where(x => x.ThreatheningBombs.Count == 0 && x != currentTile).ToList();

			// Remove the bomb we used for our simulation
			currentTile.Bombs.Remove(bomb);
			foreach(Tile t in game.Board.Tiles()) t.ThreatheningBombs.Remove(bomb);

			return tiles.Any();
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
			if(neighbours.All(x => x.Players.Any() || !x.Walkable))
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
			return SafeNeighbour() ?? StaySafe() ?? MinorPanick() ?? Panick() ?? Command.Pass;
		}

		/// <summary>
		/// Returns a command that that moves to a safe neighbour.
		/// If no safe neighbour exists, it returns null.
		/// </summary>
		/// <returns>
		/// Movement command or null if no safe neighbour exists.
		/// </returns>
		private Command SafeNeighbour()
		{
			var tile = neighbours.RandomOrDefault(x => x.ThreatheningBombs.Count == 0 && x.Walkable);
			if(tile != null)
			{
				// Just because there ain't no bomb gonna explode on that tile now,
				// someone might still trap us there. Better make sure that's not the case.
				if(CheckTrapRisk(tile))
				{
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
				}

				return MoveTowardsTile(tile);
			}

			return null;
		}

		/// <summary>
		/// Returns pass command if the current tile is safe.
		/// </summary>
		/// <returns>
		/// Returns pass command if the current tile is safe, null otherwise.
		/// </returns>
		private Command StaySafe()
		{
			if(game.Board[me.Position].ThreatheningBombs.Count == 0)
				return Command.Pass;

			return null;
		}

		/// <summary>
		/// Finds the closest safe tile and returns a command to move
		/// towards that tile.
		/// </summary>
		/// <returns>
		/// A movement command if a safe tile was found, null otherwise.
		/// </returns>
		private Command MinorPanick()
		{
			// So all neighbour tiles are in threat.
			// Find the closest safe tile!
			Tile closestSafeTile = areafinder.FindTiles(
				currentTile, 6,
				x => x == currentTile || x.Walkable
			).FirstOrDefault(x => x.ThreatheningBombs.Count == 0 && x != currentTile);

			if(closestSafeTile != null)
			{
				// Find the path to that tile.
				var path = pathfinder.FindPath(currentTile, closestSafeTile);

				// Did we find the path?
				if(path != null && path.Count > 1)
				{
					// Get our next tile to follow the path.
					var nextTile = path[1];
					if(nextTile != null)
					{
						// TODO: Think a step ahead here. See game 9327 round 162
						// Just make sure we don't go there and explode directly!
						if(nextTile.ThreatheningBombs.Min(x => x.Ticks) == 1)
						{
							// Woaah, it's a trap. Don't go there.
							// Can we wait here one round instead?
							if(currentTile.ThreatheningBombs.Count == 0 ||currentTile.ThreatheningBombs.Min(x => x.Ticks) > 1)
							{
								// Yes, better wait here until the bomb about
								// to explode on our path is gone.
								return Command.Pass;
							}
						}

						// Okay, the path seems cool.
						return MoveTowardsTile(nextTile);
					}
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


		/// <summary>
		/// Gets the path to player.
		/// </summary>
		/// <returns>
		/// The path to player.
		/// </returns>
		/// <param name='player'>
		/// Player.
		/// </param>
		private IList<Tile> GetPathToPlayer(Player player, Func<Tile, bool> predicate = null)
		{
			// Get tiles
			Tile fromTile = game.Board[me.Position];
			Tile toTile = game.Board[player.Position];

			// Now, find path from "fromTile" to "toTile".
			var tilePath = pathfinder.FindPath(fromTile, toTile);

			return tilePath;
		}


		/// <summary>
		/// Gets all paths to players.
		/// </summary>
		/// <returns>
		/// All paths to players.
		/// </returns>
		private Dictionary<Player, IList<Tile>> GetAllPathsToPlayers(Func<Tile, bool> predicate = null)
		{
			Dictionary<Player, IList<Tile>> pathsToPlayers = new Dictionary<Player, IList<Tile>>();

			foreach(Player player in game.Players.Values)
			{
				if(player == me) continue;

				pathsToPlayers.Add(player, GetPathToPlayer(player, predicate));
			}

			return pathsToPlayers;
		}
	}
}

