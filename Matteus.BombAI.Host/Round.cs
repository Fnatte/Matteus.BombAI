using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Matteus.BombAI.Host
{
	public class Round
	{
		private readonly List<Player> playersAlive = new List<Player>();
		public ReadOnlyCollection<Player> PlayersAlive { get { return playersAlive.AsReadOnly(); } }

		private readonly List<Action> actions = new List<Action>();
		public ReadOnlyCollection<Action> Actions {get { return actions.AsReadOnly(); } }

		private Round()
		{

		}

		public static Round CreateUsingClients(Game game, IEnumerable<Client> clients, bool processCommands = true)
		{
			Round round = new Round();


			foreach (var client in clients)
			{
				Command cmd = processCommands ? client.ReadCommand() : Command.Pass;
				round.actions.Add(new Action(client.Player, cmd));

				if(cmd == Command.MoveLeft) client.Player.Position += new Position(1, 0);
				else if(cmd == Command.MoveRight) client.Player.Position += new Position(-1, 0);
				else if(cmd == Command.MoveUp) client.Player.Position += new Position(0, -1);
				else if(cmd == Command.MoveDown) client.Player.Position += new Position(0, 1);
				else if(cmd is BombCommand && client.Player.Bombs.Count <= 5)
				{
					Bomb bomb = new Bomb(client.Player, ((BombCommand)cmd).Ticks);
					game.Board[bomb.Position].Bombs.Add(bomb);
				}
			}

			foreach(Player player in game.Players)
			{
				if(!player.Dead)
					round.playersAlive.Add(player);
			}

			return round;
		}
	}
}

