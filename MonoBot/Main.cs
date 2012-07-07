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
		public string password;
	}
	class IRCBOT
	{
		Config config;
		TcpClient sock;
		Stream stm;
		StreamReader Reader;
		StreamWriter Writer;
		public IRCBOT (Config config)
		{
			this.config = config;

			try {
				/* Basic Bot Setup */
				sock = new TcpClient (config.server, config.port);
				stm = sock.GetStream ();
				Writer = new StreamWriter (stm);
				Reader = new StreamReader (stm);
				Writer.WriteLine ("USER " + config.nick + "  8 * :" + config.name);
				Writer.Flush();
				Writer.WriteLine("NICK " + config.nick);
				Writer.Flush();
				/* Kick off the Worker Process */
				IRCWork();
			} 
			catch (Exception e) {
				Console.WriteLine(e);
			}
		}

		public void IRCWork()
		{
			try {
				/* Join all the chans. */
				foreach (string channel in config.channels){
					Writer.WriteLine ("JOIN "+ channel);
					Writer.Flush();
				}
				bool exit = false;
				/* Worker Loop */
				while (exit == false) {
					string data = Reader.ReadLine().ToString();
					Console.WriteLine(data);
					char [] delim = new char[] { ' ' };
					string[] splt = data.Split(delim,5);
					string command;
					/* Respond to server PINGs to stay online */
					if (splt[0].Contains("PING"))
					{
						Console.WriteLine("PONG!");
						Writer.WriteLine("PONG " + ":"+splt[1]);
						Writer.Flush();
						Writer.WriteLine("PING " + splt[1]);
						Writer.Flush();
					}
					/* Admin only commands */
					if (splt[0].StartsWith(config.admin))
					{
						if (splt.Length > 3){
							command = splt[3];
							switch (command)
							{
								case ":!join":
									Writer.WriteLine("PRIVMSG "+ config.admin + " :Joining"+splt[4]);
									Writer.Flush ();
									Writer.WriteLine("JOIN "+ splt[4]);
									Writer.Flush ();
									break;
								case ":!part":
									Console.WriteLine(string.Format("{0} {1}",splt[2],splt[3]));
									Writer.WriteLine("PART "+ splt[4] + "Gone");
									Writer.Flush ();
									Writer.WriteLine("PRIVMSG "+ config.admin + " :Left "+ splt[4]);
									Writer.Flush ();
									break;
								case ":!help":
									Writer.WriteLine("PRIVMSG "+ config.admin + " :There is no help sucka!");
								Writer.Flush();
									break;
								case ":!quit":
									Writer.WriteLine("QUIT Bot leaving");
									Writer.Flush ();
									Writer.WriteLine("PRIVMSG "+ config.admin + " :GoodBye");
									Writer.Flush ();
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
								Writer.WriteLine("PRIVMSG "+ splt[2] + " :" + bender[rndBender]);
								Writer.Flush ();
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
			string[] channels = {"#kusu"};
			conf.channels = channels;
			new IRCBOT(conf);
		}
	}
}