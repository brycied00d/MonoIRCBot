/**********************************************
* C# IRC Bot                                  *
* Author: Jason Barbier						  *
***********************************************            
* Creative Commons 07/2012 By Attribution     *
***********************************************/
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;


namespace MonoBot
{
	struct Config
	{
		public string server;
		public int port;
		public string nick;
		public string name;
		public string admin;
		public string[] channels;
		public bool nickserv;
		public string nickservUserName;
		public string password;
		public bool debug;
	}
	class IRCBot
	{
		Config config;
		TcpClient sock;
		Stream stm;
		StreamReader Reader;
		StreamWriter Writer;
		public IRCBot (Config config)
		{
			this.config = config;

			try {
				/* Basic Bot Setup */
				sock = new TcpClient (config.server, config.port);
				stm = sock.GetStream ();
				Writer = new StreamWriter (stm);
				Reader = new StreamReader (stm);
				SendData("USER", config.nick + "  8 * :" + config.name);
				SendData("NICK " + config.nick);
				if (config.nickserv == true){
					ChanMessage ("nickserv", "id ", config.nickservUserName + config.password);
				}
				bool debug = config.debug;
				/* Kick off the Worker Process */
				IRCWork();
			} 
			catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		/* Message sending functions*/
		/*Raw Data*/
		public void SendData (string Command,string Message)
		{
			Writer.WriteLine(Command + " " + Message);
			Writer.Flush();
			Console.WriteLine(Command + " " + Message);
		}
		/* Channel and Private messages */
		public void ChanMessage (string target, string message)
		{
			SendData ("PRIVMSG", target + " :" + message);
		}

		/* Worker function */
		public void IRCWork()
		{
			try {
				/* Join all the chans. */
				foreach (string channel in config.channels){
					SendData ("JOIN", channel);
				}
				bool exit = false;
				/* Worker Loop */
				while (exit == false) {
					string data = Reader.ReadLine().ToString();
					if (debug == true){
					Console.WriteLine(data);
					}
					char [] delim = new char[] { ' ' };
					string[] splt = data.Split(delim,5);
					string command;
					/* Respond to server PINGs to stay online */
					if (splt[0].Contains("PING"))
					{
						Console.WriteLine("Keep Alive");
						SendData ("PONG",":"+splt[1]);
						SendData("PING", splt[1]);
					}
					/* Admin only commands */
					if (splt[0].StartsWith(config.admin))
					{
						if (splt.Length > 3){
							command = splt[3];
							switch (command)
							{
								case ":!join":
									SendData ("PRIVMSG", config.admin + " :Joining"+splt[4]);
									SendData ("JOIN", splt[4]);
									break;
								case ":!part":
									Console.WriteLine(string.Format("{0} {1}",splt[2],splt[3]));
									SendData ("PART ", splt[4] + "Gone");
									SendData ("PRIVMSG", config.admin + " :Left "+ splt[4]);
									break;
								case ":!help":
									SendData ("PRIVMSG", config.admin + " :There is no help sucka!");
									break;
								case ":!quit":
									SendData ("PRIVMSG", config.admin + " :GoodBye");
									SendData ("QUIT","Bot leaving");
									exit = true;
									sock.Close ();
									break;
							}
						}
					}
					/* General Commands */
					if (splt.Length > 3){
						command = splt[3];
						switch (command)
						{
							case ":.bender":
								string[] bender = {"Sounds like fun on a bun!","Bite my shiny metal ass","Kill all humans"};
								Random Random = new Random();
								int rndBender = Random.Next(0,(bender.Length));
								ChanMessage (splt[2], bender[rndBender]);
								break;
						}
					}
				}
				sock.Close ();
			} 
			catch (Exception e) {
				Console.WriteLine("Error..... " + e.Message);
			}
		}
	}

	class MainClass
	{
		public static void Main (string[] args)
		{
			Config conf = new Config();
			conf.name = "KusuBot";
			conf.nick = "KusuBot";
			conf.server = "chat.freenode.net";
			conf.port = 6667;
			conf.admin = ":kusuriya!";
			conf.debug = false;
			string[] channels = {"#kusu"};
			conf.channels = channels;
			new IRCBot(conf);
		}
	}
}