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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using idTech4.Services;
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
	public sealed class idCommandSystem : ICommandSystemService
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
		private List<CommandArguments> _tokenizedCommands = new List<CommandArguments>();
		#endregion

		#region Constructor
		public idCommandSystem()
		{

		}
		#endregion

		#region Methods
		#region Public
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
				idLog.WriteLine("idCmdSystem.AddCommand: {0} already defined", name);
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
		#endregion

		#region Private
		private void ExecuteCommandText(string text)
		{
			ExecuteTokenizedString(new CommandArguments(text, false));
		}

		private void ExecuteTokenizedString(CommandArguments args)
		{
			// execute the command line.
			if(args.Length == 0)
			{
				return; // no tokens.
			}

			CommandDefinition cmd;

			// check registered command functions.
			if(_commands.TryGetValue(args.Get(0), out cmd) == true)
			{
				if(((cmd.Flags & (CommandFlags.Cheat | CommandFlags.Tool)) != 0)
					&& (idE.Session.IsMultiplayer == true) && (idE.CvarSystem.GetBool("net_allowCheats") == false))
				{
					idLog.WriteLine("Command '{0}' not valid in multiplayer mode.", cmd.Name);
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
			if(idE.CvarSystem.Command(args) == true)
			{
				return;
			}

			idLog.WriteLine("Unknown command '{0}'", args.Get(0));
		}

		private void InsertCommandText(string text)
		{
			_cmdBuffer = _cmdBuffer.Insert(0, text + "\n");
		}

		private void AppendCommandText(string text)
		{
			_cmdBuffer.Append(text);
		}		
		#endregion
		
		#region Argument completion
		public static void ArgCompletion_ConfigName(object sender, CommandCompletionEventArgs e)
		{
			idLog.WriteLine("ArgCompletion_ConfigName: TODO!");
		}
		#endregion
		#endregion

		#region ICommandSystemService implementation
		#region Command execution/queueing
		public void BufferCommandArgs(CommandArguments args, Execute exec = Execute.Append)
		{
			switch(exec)
			{
				case Execute.Now:
					ExecuteTokenizedString(args);
					break;

				case Execute.Append:
					AppendCommandText("_execTokenized\n");
					_tokenizedCommands.Add(args);
					break;
			}
		}

		/// <summary>
		/// Adds command text to the command buffer.
		/// </summary>
		/// <param name="text">Command text to add to the buffer.</param>
		/// <param name="exec">Execution mode.</param>
		public void BufferCommandText(string text, Execute exec = Execute.Append)
		{
			if(text.EndsWith("\n") == false)
			{
				text += "\n";
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

		/// <summary>
		/// Execute all of the commands in the queue.
		/// </summary>
		public void ExecuteCommandBuffer()
		{
			CommandArguments args = null;

			while(_cmdBuffer.Length > 0)
			{
				if(_wait > 0)
				{
					// skip out while text still remains in buffer, leaving it for next frame.
					_wait--;
					break;
				}

				int quotes = 0, i;
				int len = _cmdBuffer.Length;

				for(i = 0; i < len; i++)
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

				string cmd = _cmdBuffer.ToString().Substring(0, i);
				_cmdBuffer = _cmdBuffer.Remove(0, i + 1);

				if(cmd == "_execTokenized")
				{
					args = _tokenizedCommands[0];
					_tokenizedCommands.RemoveAt(0);
				}
				else
				{
					args = new CommandArguments(cmd, false);
				}

				// execute the command line that we have already tokenized.
				ExecuteTokenizedString(args);
			}
		}
		#endregion

		#region Command completion
		public string[] CommandCompletion(Predicate<string> filter)
		{
			return Array.FindAll(_commands.Keys.ToArray(), filter);
		}

		public void ListByFlags(CommandArguments args, CommandFlags flags)
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
				idLog.WriteLine(" {0} {1}", cmd.Name.PadRight(21, ' '), cmd.Description);
			}

			idLog.WriteLine("{0} commands", cmdList.Count);
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

	[AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	public sealed class CommandAttribute : Attribute
	{
		#region Members
		private string _command;
		private string _description;
		private CommandFlags _flags;
		#endregion

		#region Constructor
		public CommandAttribute(string command, string description, CommandFlags flags)
		{
			_command = command;
			_description = description;
			_flags = flags;
		}
		#endregion
	}

	/// <summary>
	/// Command arguments.
	/// </summary>
	public sealed class CommandEventArgs : EventArgs
	{
		#region Properties
		public CommandArguments Args
		{
			get
			{
				return _args;
			}
		}
		#endregion

		#region Members
		private CommandArguments _args;
		#endregion

		#region Constructor
		public CommandEventArgs(CommandArguments args)
			: base()
		{
			_args = args;
		}
		#endregion
	}

	public delegate void CommandCompletionHandler(string str);
	
	public sealed class CommandCompletionEventArgs : EventArgs
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

	public abstract class ArgCompletion
	{
		#region Constructor
		public ArgCompletion()
		{

		}
		#endregion

		#region Methods
		public abstract string[] Complete(CommandArguments args);
		#endregion
	}

	public sealed class ArgCompletion_Integer : ArgCompletion
	{
		#region Members
		private int _min;
		private int _max;
		#endregion

		#region Constructor
		public ArgCompletion_Integer(int min, int max)
			: base()
		{
			_min = min;
			_max = max;
		}
		#endregion

		#region ArgCompletion implementation
		public override string[] Complete(CommandArguments args)
		{
			List<string> values = new List<string>();

			for(int i = _min; i < _max; i++)
			{
				values.Add(i.ToString());
			}

			return values.ToArray();
		}
		#endregion
	}

	public sealed class ArgCompletion_String : ArgCompletion
	{
		#region Members
		private string[] _values;
		#endregion

		#region Constructor
		public ArgCompletion_String(string[] values)
			: base()
		{
			_values = values;
		}
		#endregion

		#region ArgCompletion implementation
		public override string[] Complete(CommandArguments args)
		{
			return _values;
		}
		#endregion
	}
}