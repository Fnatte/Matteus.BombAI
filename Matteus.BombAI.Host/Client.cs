using System;
using System.Diagnostics;
using System.IO;

namespace Matteus.BombAI.Host
{
	public class Client
	{
		private Process process;

		private TextWriter In {
			get
			{
				if(process.HasExited) throw new Exception("Client process has exited");
				return process.StandardInput;
			}
		}
		private TextReader Out {
			get
			{
				if(process.HasExited) throw new Exception("Client process has exited");
				return process.StandardOutput;
			}
		}

		public Player Player {
			get;
			set;
		}

		private Client()
		{

		}

		public static Client Start(string filename, string args)
		{
			Client client = new Client();

			ProcessStartInfo info = new ProcessStartInfo();
			info.Arguments = args;
			info.FileName = filename;
			info.RedirectStandardOutput = true;
			info.RedirectStandardInput = true;
			info.UseShellExecute = false;
			
			client.process = Process.Start(info);
			return client;
		}

		public Command ReadCommand()
		{
			string cmdName = ReadLine();
			 
			Command command;
			if(!Command.TryParse(cmdName, out command))
			{
				Console.WriteLine("Failed to parse command from client: " + Player.Id);
			}

			return command;
		}

		public void WriteLine(object obj)
		{
			WriteLine(obj.ToString());
		}

		public void WriteLine(string str, params object[] args)
		{
			string formatted = String.Format(str, args);
			Console.WriteLine(formatted);
			In.WriteLine(formatted);
		}

		public string ReadLine()
		{
			string str = Out.ReadLine();
			Console.WriteLine(" > {0}", str);
			return str;
		}
	}
}

