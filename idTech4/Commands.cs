/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System.IO;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Services;

namespace idTech4
{
	public class Commands
	{
		#region Command System
		[Command("listCmds", "lists commands", CommandFlags.System)]
		private static void Cmd_ListAllCommands(Game game, CommandEventArgs e)
		{
			game.Services.GetService<ICommandSystemService>().ListByFlags(e.Args, CommandFlags.All);
		}

		[Command("listSystemCmds", "lists system commands", CommandFlags.System)]
		private static void Cmd_ListSystemCommands(Game game, CommandEventArgs e)
		{
			game.Services.GetService<ICommandSystemService>().ListByFlags(e.Args, CommandFlags.System);
		}

		[Command("listRendererCmds", "lists renderer commands", CommandFlags.System)]
		private static void Cmd_ListRendererCommands(Game game, CommandEventArgs e)
		{
			game.Services.GetService<ICommandSystemService>().ListByFlags(e.Args, CommandFlags.Renderer);
		}

		[Command("listSoundCmds", "lists sound commands", CommandFlags.System)]
		private static void Cmd_ListSoundCommands(Game game, CommandEventArgs e)
		{
			game.Services.GetService<ICommandSystemService>().ListByFlags(e.Args, CommandFlags.Sound);
		}

		[Command("listGameCmds", "lists game commands", CommandFlags.System)]
		private static void Cmd_ListGameCommands(Game game, CommandEventArgs e)
		{
			ListByFlags(e.Args, CommandFlags.Game);
		}

		[Command("listToolCmds", "lists tool commands", CommandFlags.System)]
		private static void Cmd_ListToolCommands(Game game, CommandEventArgs e)
		{
			game.Services.GetService<ICommandSystemService>().ListByFlags(e.Args, CommandFlags.Tool);
		}

		[Command("exec", "executes a config file", CommandFlags.System/* TODO: , new EventHandler<CommandCompletionEventArgs>(ArgCompletion_ConfigName)*/)]
		private static void Cmd_Exec(Game game, CommandEventArgs e)
		{
			if(e.Args.Length != 2)
			{
				idLog.WriteLine("exec <filename> : execute a script file");
			}
			else
			{
				string fileName = e.Args.Get(1);

				if(Path.HasExtension(fileName) == false)
				{
					fileName += ".cfg";
				}

				byte[] data = idE.FileSystem.ReadFile(fileName);

				if(data == null)
				{
					idLog.WriteLine("couldn't exec {0}", e.Args.Get(1));
				}
				else
				{
					string content = UTF8Encoding.UTF8.GetString(data);

					idLog.WriteLine("execing {0}", e.Args.Get(1));
					game.Services.GetService<ICommandSystemService>().BufferCommandText(Execute.Insert, content);
				}
			}
		}

		/// <summary>
		/// Inserts the current value of a cvar as command text.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Command("vstr", "inserts the current value of a cvar as command text", CommandFlags.System)]
		private static void Cmd_VStr(Game game, CommandEventArgs e)
		{
			if(e.Args.Length != 2)
			{
				idLog.WriteLine("vstr <variablename> : execute a variable command");
			}
			else
			{
				game.Services.GetService<ICommandSystemService>().BufferCommandText(Execute.Append, idE.CvarSystem.GetString(e.Args.Get(1)));
			}
		}

		/// <summary>
		/// Just prints the rest of the line to the console.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Command("echo", "prints text", CommandFlags.System)]
		private static void Cmd_Echo(Game game, CommandEventArgs e)
		{
			int count = e.Args.Length;

			for(int i = 1; i < count; i++)
			{
				idLog.Write("{0} ", e.Args.Get(i));
			}

			idLog.WriteLine("");
		}

		/// <summary>
		/// Causes execution of the remainder of the command buffer to be delayed until next frame.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Command("wait", "delays remaining buffered commands one or more frames", CommandFlags.System)]
		private static void Cmd_Wait(Game game, CommandEventArgs e)
		{
			ICommandSystemService cmdSystem = game.Services.GetService<ICommandSystemService>();

			if(e.Args.Length == 2)
			{
				int wait;
				int.TryParse(e.Args.Get(1), out wait);

				cmdSystem.Wait = wait;
			}
			else
			{
				cmdSystem.Wait = 1;
			}
		}

		/// <summary>
		/// This just prints out how the rest of the line was parsed, as a debugging tool.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Command("parse", "prints tokenized string", CommandFlags.System)]
		private static void Cmd_Parse(object sender, CommandEventArgs e)
		{
			int count = e.Args.Length;

			for(int i = 0; i < count; i++)
			{
				idLog.WriteLine("{0}: {1}", i, e.Args.Get(i));
			}
		}
		#endregion
	}
}