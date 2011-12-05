/*
===========================================================================

Doom 3 GPL Source Code
Copyright (C) 1999-2011 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 GPL Source Code (?Doom 3 Source Code?).  

Doom 3 Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Text;

namespace idTech4
{
	/// <summary>
	/// Console command execution and command text buffering.
	/// </summary>
	/// <remarks>
	/// Any number of commands can be added in a frame from several different
	/// sources. Most commands come from either key bindings or console line input,
	/// but entire text files can be exec'ed.
	/// <p/>
	/// Command execution takes a string, breaks it into tokens,
	/// then searches for a command or variable that matches the first token.
	/// </remarks>
	public sealed class idCmdSystem
	{
		#region Properties
		/// <summary>
		/// Has the command system been initialized yet?
		/// </summary>
		public bool IsInitialized
		{
			get
			{
				return _initialized;
			}
		}
		#endregion

		#region Members
		private bool _initialized;
		private int _wait;
		private StringBuilder _cmdBuffer = new StringBuilder();

		private Dictionary<string, CommandDefinition> _commands = new Dictionary<string, CommandDefinition>(StringComparer.OrdinalIgnoreCase);

		// piggybacks on the text buffer, avoids tokenize again and screwing it up.
		private List<idCmdArgs> _tokenizedCommands = new List<idCmdArgs>();
		#endregion

		#region Constructor
		public idCmdSystem()
		{

		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			if(this.IsInitialized == true)
			{
				throw new InvalidOperationException("Command system has already been initialized.");
			}

			AddCommand("listCmds", "lists commands", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_ListAllCommands));
			AddCommand("listSystemCmds", "lists system commands", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_ListSystemCommands));
			AddCommand("listRendererCmds", "lists renderer commands", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_ListRendererCommands));
			AddCommand("listSoundCmds", "lists sound commands", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_ListSoundCommands));
			AddCommand("listGameCmds", "lists game commands", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_ListGameCommands));
			AddCommand("listToolCmds", "lists tool commands", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_ListToolCommands));
			// TODO: AddCommand("exec", "executes a config file", CommandFlags.System, new EventHandler<CommandCompletionEventArgs>(ArgCompletion_ConfigName));
			AddCommand("vstr", "inserts the current value of a cvar as command text", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_VStr));
			AddCommand("echo", "prints text", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Echo));
			AddCommand("parse", "prints tokenized string", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Parse));
			AddCommand("wait", "delays remaining buffered commands one or more frames", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Wait));

			_initialized = true;
		}

		/// <summary>
		/// Registers a command and the delegate to call for it.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <param name="description"></param>
		public void AddCommand(string name, string description, CommandFlags flags, EventHandler<CommandEventArgs> handler)
		{
			AddCommand(name, description, flags, handler, null);
		}

		/// <summary>
		/// Registers a command and the delegate to call for it.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <param name="description"></param>
		/// <param name="argCompletion"></param>
		public void AddCommand(string name, string description, CommandFlags flags, EventHandler<CommandEventArgs> handler, EventHandler<CommandEventArgs> completionHandler)
		{
			if(_commands.ContainsKey(name) == true)
			{
				idConsole.WriteLine("idCmdSystem.AddCommand: {0} already defined", name);
			}
			else
			{
				CommandDefinition cmd = new CommandDefinition();
				cmd.Name = name;
				cmd.Description = description;

				cmd.Handler = handler;
				cmd.CompletionHandler = completionHandler;
				cmd.Flags = flags;

				_commands.Add(name, cmd);
			}
		}

		/// <summary>
		/// Adds command text to the command buffer.
		/// </summary>
		/// <param name="text"></param>
		public void BufferCommandText(string text)
		{
			BufferCommandText(Execute.Append, text);
		}

		/// <summary>
		/// Adds command text to the command buffer.
		/// </summary>
		/// <param name="exec"></param>
		/// <param name="text"></param>
		public void BufferCommandText(Execute exec, string text)
		{
			if(text.EndsWith("\n") == false)
			{
				text += '\n';
			}

			switch(exec)
			{
				case Execute.Now:
					ExecuteCommandText(text);
					break;

				case Execute.Insert:
					InsertCommandText(text);
					break;

				case Execute.Append:
					AppendCommandText(text);
					break;
			}
		}

		public void ExecuteCommandBuffer()
		{
			idCmdArgs args = null;

			while(_cmdBuffer.Length > 0)
			{
				if(_wait > 0)
				{
					// skip out while text still remains in buffer, leaving it for next frame.
					_wait--;
					break;
				}

				int quotes = 0, i;

				for(i = 0; i < _cmdBuffer.Length; i++)
				{
					if(_cmdBuffer[i] == '"')
					{
						quotes++;
					}

					if(((quotes & 1) == 0) && (_cmdBuffer[i] == ';'))
					{
						break; // don't break if inside a quoted string.
					}

					if((_cmdBuffer[i] == '\n') || (_cmdBuffer[i] == '\r'))
					{
						break;
					}
				}


				string cmd = _cmdBuffer.ToString().Substring(0, i + 1);
				_cmdBuffer = _cmdBuffer.Remove(0, i + 1);

				if(cmd == "_execTokenized")
				{
					args = _tokenizedCommands[0];
					_tokenizedCommands.RemoveAt(0);
				}
				else
				{
					args = new idCmdArgs(cmd, false);
				}

				// execute the command line that we have already tokenized.
				ExecuteTokenizedString(args);
			}
		}
		#endregion

		#region Private
		private void ExecuteCommandText(string text)
		{
			ExecuteTokenizedString(new idCmdArgs(text, false));
		}

		private void ExecuteTokenizedString(idCmdArgs args)
		{
			// execute the command line.
			if(args.Length == 0)
			{
				return; // no tokens.
			}

			// check registered command functions.
			if(_commands.ContainsKey(args.Get(0)) == true)
			{
				CommandDefinition cmd = _commands[args.Get(0)];

				if(((cmd.Flags & (CommandFlags.Cheat | CommandFlags.Tool)) == (CommandFlags.Cheat | CommandFlags.Tool)) && (idE.Session.IsMultiplayer == true) && (idE.CvarSystem.GetBool("net_allowCheats") == false))
				{
					idConsole.WriteLine("Command '{0}' not valid in multiplayer mode.", cmd.Name);
					return;
				}

				// perform the action.
				if(cmd.Handler != null)
				{
					cmd.Handler(this, new CommandEventArgs(args));
				}

				return;
			}

			// check cvars.
			// TODO
			if(idE.CvarSystem.Command(args) == true)
			{
				return;
			}

			idConsole.WriteLine("Unknown command '{0}'", args.Get(0));
		}

		private void InsertCommandText(string text)
		{
			_cmdBuffer = _cmdBuffer.Insert(0, text + '\n');
		}

		private void AppendCommandText(string text)
		{
			_cmdBuffer.Append(text);
		}

		private void ListByFlags(idCmdArgs args, CommandFlags flags)
		{
			string match = string.Empty;
			List<CommandDefinition> cmdList = new List<CommandDefinition>();

			if(args.Length > 1)
			{
				match = args.Get(1, -1).Replace(" ", "");
			}

			foreach(KeyValuePair<string, CommandDefinition> kvp in _commands)
			{
				if((kvp.Value.Flags & flags) == 0)
				{
					continue;
				}

				if((match.Length > 0) && (kvp.Value.Name.StartsWith(match) == false))
				{
					continue;
				}

				cmdList.Add(kvp.Value);
			}

			cmdList.Sort((a, b) => a.Description.CompareTo(b.Description));

			foreach(CommandDefinition cmd in cmdList)
			{
				idConsole.WriteLine(" {0} {1}", cmd.Name.PadRight(21, ' '), cmd.Description);
			}

			idConsole.WriteLine("{0} commands", cmdList.Count);
		}
		#endregion

		#region Command handlers
		private void Cmd_ListAllCommands(object sender, CommandEventArgs e)
		{
			ListByFlags(e.Args, CommandFlags.All);
		}

		private void Cmd_ListSystemCommands(object sender, CommandEventArgs e)
		{
			ListByFlags(e.Args, CommandFlags.System);
		}

		private void Cmd_ListRendererCommands(object sender, CommandEventArgs e)
		{
			ListByFlags(e.Args, CommandFlags.Renderer);
		}

		private void Cmd_ListSoundCommands(object sender, CommandEventArgs e)
		{
			ListByFlags(e.Args, CommandFlags.Sound);
		}

		private void Cmd_ListGameCommands(object sender, CommandEventArgs e)
		{
			ListByFlags(e.Args, CommandFlags.Game);
		}

		private void Cmd_ListToolCommands(object sender, CommandEventArgs e)
		{
			ListByFlags(e.Args, CommandFlags.Tool);
		}

		/// <summary>
		/// Inserts the current value of a cvar as command text.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cmd_VStr(object sender, CommandEventArgs e)
		{
			if(e.Args.Length != 2)
			{
				idConsole.WriteLine("vstr <variablename> : execute a variable command");
			}
			else
			{
				idE.CmdSystem.BufferCommandText(Execute.Append, idE.CvarSystem.GetString(e.Args.Get(1)));
			}
		}

		/// <summary>
		/// Just prints the rest of the line to the console.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cmd_Echo(object sender, CommandEventArgs e)
		{
			for(int i = 1; i < e.Args.Length; i++)
			{
				idConsole.Write("{0} ", e.Args.Get(i));
			}

			idConsole.WriteLine("");
		}

		/// <summary>
		/// Causes execution of the remainder of the command buffer to be delayed until next frame.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cmd_Wait(object sender, CommandEventArgs e)
		{
			if(e.Args.Length == 2)
			{
				int.TryParse(e.Args.Get(1), out _wait);
			}
			else
			{
				_wait = 1;
			}
		}

		/// <summary>
		/// This just prints out how the rest of the line was parsed, as a debugging tool.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cmd_Parse(object sender, CommandEventArgs e)
		{
			for(int i = 0; i < e.Args.Length; i++)
			{
				idConsole.WriteLine("{0}: {1}", i, e.Args.Get(i));
			}
		}
		#endregion

		#region Argument completion
		public static void ArgCompletion_ConfigName(object sender, CommandCompletionEventArgs e)
		{
			// TODO
			idConsole.WriteLine("ArgCompletion_ConfigName: TODO!");
		}
		#endregion
		#endregion

		#region CommandDefinition
		private class CommandDefinition
		{
			public string Name;
			public string Description;

			public EventHandler<CommandEventArgs> Handler;
			public EventHandler<CommandEventArgs> CompletionHandler;
			public CommandFlags Flags;
		}
		#endregion
	}

	/// <summary>
	/// Command flags.
	/// </summary>
	public enum CommandFlags
	{
		All = -1,
		/// <summary>Command is considered a cheat.</summary>
		Cheat = 1 << 0,
		/// <summary>System command.</summary>
		System = 1 << 1,
		/// <summary>Renderer command.</summary>
		Renderer = 1 << 2,
		/// <summary>Sound command.</summary>
		Sound = 1 << 3,
		/// <summary>Game command.</summary>
		Game = 1 << 4,
		/// <summary>Tool command.</summary>
		Tool = 1 << 5
	}

	/// <summary>
	/// Command buffer stuffing.
	/// </summary>
	public enum Execute
	{
		/// <summary>Don't return until completed.</summary>
		Now,
		/// <summary>Insert at current position, but don't run yet.</summary>
		Insert,
		/// <summary>Add to the end of the command buffer (normal case).</summary>
		Append
	}

	/// <summary>
	/// Command arguments.
	/// </summary>
	public sealed class CommandEventArgs : EventArgs
	{
		#region Properties
		public idCmdArgs Args
		{
			get
			{
				return _args;
			}
		}
		#endregion

		#region Members
		private idCmdArgs _args;
		#endregion

		#region Constructor
		public CommandEventArgs(idCmdArgs args)
			: base()
		{
			_args = args;
		}
		#endregion
	}

	public delegate void CommandCompletionHandler(string str);

	public class CommandCompletionEventArgs : EventArgs
	{
		#region Properties
		public CommandCompletionHandler Handler
		{
			get
			{
				return _handler;
			}
		}
		#endregion

		#region Members
		private CommandCompletionHandler _handler;
		#endregion

		#region Constructor
		public CommandCompletionEventArgs(CommandCompletionHandler handler)
			: base()
		{
			_handler = handler;
		}
		#endregion
	}
}