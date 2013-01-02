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
using System;
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
		private static void Cmd_ListAllCommands(idEngine engine, params string[] args)
		{
			engine.GetService<ICommandSystem>().ListByFlags(args, CommandFlags.All);
		}

		[Command("listSystemCmds", "lists system commands", CommandFlags.System)]
		private static void Cmd_ListSystemCommands(idEngine engine, params string[] args)
		{
			engine.GetService<ICommandSystem>().ListByFlags(args, CommandFlags.System);
		}

		[Command("listRendererCmds", "lists renderer commands", CommandFlags.System)]
		private static void Cmd_ListRendererCommands(idEngine engine, params string[] args)
		{
			engine.GetService<ICommandSystem>().ListByFlags(args, CommandFlags.Renderer);
		}

		[Command("listSoundCmds", "lists sound commands", CommandFlags.System)]
		private static void Cmd_ListSoundCommands(idEngine engine, params string[] args)
		{
			engine.GetService<ICommandSystem>().ListByFlags(args, CommandFlags.Sound);
		}

		[Command("listGameCmds", "lists game commands", CommandFlags.System)]
		private static void Cmd_ListGameCommands(idEngine engine, params string[] args)
		{
			engine.GetService<ICommandSystem>().ListByFlags(args, CommandFlags.Game);
		}

		[Command("listToolCmds", "lists tool commands", CommandFlags.System)]
		private static void Cmd_ListToolCommands(idEngine engine, params string[] args)
		{
			engine.GetService<ICommandSystem>().ListByFlags(args, CommandFlags.Tool);
		}

		[Command("exec", "executes a config file", CommandFlags.System/* TODO: , new EventHandler<CommandCompletionEventArgs>(ArgCompletion_ConfigName)*/)]
		private static void Cmd_Exec(idEngine engine, string fileName)
		{
			if(Path.HasExtension(fileName) == false)
			{
				fileName += ".cfg";
			}

			ICommandSystem cmdSystem = engine.GetService<ICommandSystem>();
			IFileSystem fileSystem = engine.GetService<IFileSystem>();

			byte[] data = fileSystem.ReadFile(fileName);

			if(data == null)
			{
				idLog.WriteLine("couldn't exec {0}", fileName);
			}
			else
			{
				string content = UTF8Encoding.UTF8.GetString(data);

				idLog.WriteLine("execing {0}", fileName);
				cmdSystem.BufferCommandText(content, Execute.Insert);
			}
		}

		/// <summary>
		/// Inserts the current value of a cvar as command text.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Command("vstr", "inserts the current value of a cvar as command text", CommandFlags.System)]
		private static void Cmd_VStr(idEngine engine, string variable)
		{
			ICommandSystem cmdSystem = engine.GetService<ICommandSystem>();
			ICVarSystem cvarSystem = engine.GetService<ICVarSystem>();

			cmdSystem.BufferCommandText(cvarSystem.GetString(variable));
		}

		/// <summary>
		/// Just prints the rest of the line to the console.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Command("echo", "prints text", CommandFlags.System)]
		private static void Cmd_Echo(idEngine engine, params string[] args)
		{
			int count = args.Length;

			for(int i = 1; i < count; i++)
			{
				idLog.Write("{0} ", args[i]);
			}

			idLog.WriteLine("");
		}

		/// <summary>
		/// Causes execution of the remainder of the command buffer to be delayed until next frame.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Command("wait", "delays remaining buffered commands one or more frames", CommandFlags.System)]
		private static void Cmd_Wait(idEngine engine, int wait = 1)
		{
			engine.GetService<ICommandSystem>().Wait = wait;
		}

		/// <summary>
		/// This just prints out how the rest of the line was parsed, as a debugging tool.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		[Command("parse", "prints tokenized string", CommandFlags.System)]
		private static void Cmd_Parse(idEngine engine, params string[] args)
		{
			int count = args.Length;

			for(int i = 0; i < count; i++)
			{
				idLog.WriteLine("{0}: {1}", i, args[i]);
			}
		}
		#endregion

		#region Console
		[Command("clear", "clears the console", CommandFlags.System)]
		private void Console_Clear(idEngine engine)
		{
			idLog.WriteLine("TODO: console_clear");
		}

		[Command("conDump", "dumps the console text to a file", CommandFlags.System)]
		private void Console_Dump(idEngine engine, string fileName)
		{
			idLog.WriteLine("TODO: console_dump");
		}
		#endregion

		#region CVar System
		[Command("toggle", "toggles a cvar between 0 and 1", CommandFlags.System)]
		private static void CVar_Toggle(idEngine engine, string variable)
		{
			idCVar cvar = engine.GetService<ICVarSystem>().Find(variable);

			if(cvar == null)
			{
				idLog.WriteLine("toggle: cvar \"{0}\" not found", variable);
			}
			else
			{			
				// toggle between 0 and 1
				float current = cvar.ToFloat();
				float set = (current == 1.0f) ? 0.0f : 1.0f;
			
				idLog.WriteLine("set {0} = {1}", variable, set);
				cvar.Set(set.ToString(), false, false);
			}
		}

		[Command("toggle", "toggles a cvar between 0 and <value>", CommandFlags.System)]
		private static void CVar_Toggle(idEngine engine, string variable, string value)
		{
			idCVar cvar = engine.GetService<ICVarSystem>().Find(variable);

			if(cvar == null)
			{
				idLog.WriteLine("toggle: cvar \"{0}\" not found", variable);
			}
			else
			{
				// toggle between 0 and value
				float current = cvar.ToFloat();
				float set = 0;
				float.TryParse(value, out set);
				
				if(set == current)
				{
					set = 0.0f;
				}

				idLog.WriteLine("set {0} = {1}", variable, set);
				cvar.Set(set.ToString(), false, false);
			}
		}

		[Command("toggle", "toggles a cvar between each given value", CommandFlags.System)]
		private static void CVar_Toggle(idEngine engine, string variable, params string[] args)
		{
			idCVar cvar = engine.GetService<ICVarSystem>().Find(variable);

			if(cvar == null)
			{
				idLog.WriteLine("toggle: cvar \"{0}\" not found", variable);
			}
			else
			{
				// cycle through multiple values
				string text = cvar.ToString();
				string value = null;
				int i = 0;

				for(i = 0; i < args.Length; i++)
				{
					if(StringComparer.OrdinalIgnoreCase.Compare(text, args[i]) == 0)
					{
						value = args[i];
						break;
					}
				}

				if(value == null)
				{
					value = args[0];
				}

				idLog.WriteLine("set {0} = {1}", variable, value);
				cvar.Set(value, false, false);
			}				
		}

		[Command("set", "sets a cvar", CommandFlags.System)]
		[Command("sets", "sets a cvar", CommandFlags.System)]
		[Command("setu", "sets a cvar", CommandFlags.System)]
		[Command("sett", "sets a cvar", CommandFlags.System)]
		[Command("seta", "sets a cvar", CommandFlags.System)]
		private static void CVar_Set(idEngine engine, string variable, params string[] args)
		{
			engine.GetService<ICVarSystem>().Set(variable, string.Join(" ", args));
		}

		[Command("reset", "resets a cvar", CommandFlags.System)]
		private static void CVar_Reset(idEngine engine, string variable)
		{
			idCVar cvar = engine.GetService<ICVarSystem>().Find(variable);

			if(cvar != null)
			{
				cvar.Reset();
			}
		}

		[Command("listCvars", "list cvars", CommandFlags.System)]
		private static void CVar_List(idEngine engine, params string[] args)
		{
			engine.Services.GetService<ICVarSystem>().ListByFlags(args, CVarFlags.All);
		}

		[Command("cvar_reset", "restart the cvar system", CommandFlags.System)]
		private static void CVar_Restart(idEngine engine)
		{
			engine.GetService<ICVarSystem>().Restart();			
		}

		[Command("cvarAdd", "adds a value to a numeric cvar", CommandFlags.System)]
		private static void CVar_Add(idEngine engine, string variable, float value)
		{
			idCVar var = engine.GetService<ICVarSystem>().Find(variable);

			if(var == null)
			{
				idLog.WriteLine("toggle: cvar \"{0}\" not found", variable);
			}
			else
			{
				var.Set(var.ToFloat() + value);

				idLog.WriteLine("{0} = {1}", var.Name, var.ToFloat());
			}
		}
		#endregion

		#region File System
		[Command("dir",  "lists a folder", CommandFlags.System/* TODO: idCmdSystem::ArgCompletion_FileName*/)]
		private static void FS_Dir(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_Dir");
		}

		[Command("dirtree", "lists a folder with subfolders", CommandFlags.System)]
		private static void FS_DirTree(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_DirTree");
		}

		[Command("path", "lists search paths", CommandFlags.System)]
		private static void FS_Path(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_Path");
		}

		[Command("touchFile", "touches a file", CommandFlags.System)]
		private static void FS_TouchFile(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_TouchFile");
		}

		[Command("touchFileList", "touches a list of files", CommandFlags.System)]
		private static void FS_TouchFileList(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_TouchFileList");
		}

		[Command("buildGame", "builds game pak files", CommandFlags.System)]
		private static void FS_BuildGame(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_BuildGame");
		}

		[Command("writeResourceFile", "writes a .pk4 file from a supplied manifest", CommandFlags.System)]
		private static void FS_WriteResourceFile(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_WriteResourceFile");
		}

		[Command("extractResourceFile", "extracts from the supplied resource file to the supplied path", CommandFlags.System)]
		private static void FS_ExtractResourceFile(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_ExtractResourceFile");
		}

		[Command("updateResourceFile", "updates or appends the supplied files in the supplied resource file", CommandFlags.System)]
		private static void FS_UpdateResourceFile(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_UpdateResourceFile");
		}

		[Command("generateResourceCRCs", "Generates CRC checksums for all the resource files.", CommandFlags.System)]
		private static void FS_GenerateResourceCRCs(idEngine engine)
		{
			idLog.WriteLine("TODO: FS_GenerateResourceCRCs");
		}
		#endregion

		#region Input System
		[Command("bind", "binds a command to a key", CommandFlags.System /* TODO: idKeyInput::ArgCompletion_KeyName*/)]
		private void Input_Bind(idEngine engine, string key, string command = null)
		{
			idLog.WriteLine("TODO: bind");
		}

		[Command("bindunbindtwo", "binds a key but unbinds it first if there are more than two binds", CommandFlags.System)]
		private void Input_BindUnbindTwo(idEngine engine, string key, string command = null)
		{
			idLog.WriteLine("TODO: bindunbindtwo");
		}

		[Command("unbind", "unbinds any command from a key", CommandFlags.System /* TODO: idKeyInput::ArgCompletion_KeyName*/)]
		private void Input_Unbind(idEngine engine, string key)
		{
			idLog.WriteLine("TODO: unbind");
		}

		[Command("unbindall", "unbinds any commands from all keys", CommandFlags.System)]
		private void Input_UnbindAll(idEngine engine)
		{
			idLog.WriteLine("TODO: unbindall");
		}

		[Command("listBinds", "lists key bindings", CommandFlags.System)]
		private void Input_ListBinds(idEngine engine)
		{
			idLog.WriteLine("TODO: listbinds");
		}
		#endregion
	}
}