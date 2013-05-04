using System;
using System.Collections.Generic;

namespace Matteus.BombAI
{
	public class Command
	{
		private static readonly Dictionary<string, Command> instances = new Dictionary<string, Command>();

		private readonly string name;

		public string Name
		{
			get { return name; }
		}

		protected Command(string name)
		{
			this.name = name;

			if(this.GetType() == typeof(Command))
				instances.Add(name, this);
		}

		public static readonly Command MoveLeft = new Command("left");
		public static readonly Command MoveRight = new Command("right");
		public static readonly Command MoveUp = new Command("up");
		public static readonly Command MoveDown = new Command("down");
		public static readonly Command Pass = new Command("pass");

		public override string ToString()
		{
			return name;
		}

		public static explicit operator Command(string name)
		{
		    Command result;
		    if (instances.TryGetValue(name, out result))
		        return result;
		    else
		        throw new InvalidCastException();
		}

		public static bool TryParse(string name, out Command command)
		{
			if(instances.ContainsKey(name))
			{
				command = instances[name];
				return true;
			}

			command = null;
			return false;
		}
	}
}

