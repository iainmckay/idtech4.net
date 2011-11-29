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

	/// <summary>
	/// Command arguments.
	/// </summary>
	public sealed class idCmdArgs
	{
		#region Properties
		public int Length
		{
			get
			{
				return _args.Length;
			}
		}
		#endregion

		#region Members
		private string[] _args = new string[] { };
		#endregion

		#region Constructor
		public idCmdArgs()
		{

		}

		public idCmdArgs(string text, bool keepAsStrings)
		{
			TokenizeString(text, keepAsStrings);
		}
		#endregion

		#region Methods
		/// <summary>
		/// Gets the argument at the specified index.
		/// </summary>
		/// <param name="idx"></param>
		/// <returns>Argument value or an empty string if outside the range of arguments.</returns>
		public string Get(int idx)
		{
			if((idx >= 0) && (idx < _args.Length))
			{
				return _args[idx];
			}

			return string.Empty;
		}

		/// <summary>
		/// Gets the specified range as a single string.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public string Get(int start, int end)
		{
			return Get(start, end, false);
		}

		/// <summary>
		/// Gets the specified range as a single string.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public string Get(int start, int end, bool escapeArgs)
		{
			if(end < 0)
			{
				end = _args.Length - 1;
			}
			else if(end >= _args.Length)
			{
				end = _args.Length - 1;
			}

			StringBuilder b = new StringBuilder();

			if(escapeArgs == true)
			{
				b.Append('"');
			}

			for(int i = start; i <= end; i++)
			{
				if(i > start)
				{
					if(escapeArgs == true)
					{
						b.Append("\" \"");
					}
					else
					{
						b.Append(" ");
					}
				}

				if((escapeArgs == true) && (_args[i].IndexOf('\\') != -1))
				{
					for(int j = 0; j < _args[i].Length; j++)
					{
						if(_args[i][j] == '\\')
						{
							b.Append("\\\\");
						}
						else
						{
							b.Append(_args[i].Substring(i));
						}
					}
				}
				else
				{
					b.Append(_args[i]);
				}
			}

			if(escapeArgs == true)
			{
				b.Append('"');
			}

			return b.ToString();
		}

		public void Clear()
		{
			_args = new string[] { };
		}

		/// <summary>
		/// Takes a string and breaks it up into arg tokens.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="keepAsStrings">true to only seperate tokens from whitespace and comments, ignoring punctuation.</param>
		public void TokenizeString(string text, bool keepAsStrings)
		{
			// clear previous args.
			_args = new string[] { };

			if(text.Length == 0)
			{
				return;
			}
						
			idLexer lexer = new idLexer();
			lexer.LoadMemory(text, "idCmdSystem.TokenizeString");
			lexer.Options = LexerOptions.NoErrors | LexerOptions.NoWarnings | LexerOptions.NoStringConcatination | LexerOptions.AllowPathNames | LexerOptions.NoStringEscapeCharacters | LexerOptions.AllowIPAddresses | ((keepAsStrings == true) ? LexerOptions.OnlyStrings : 0);

			idToken token = null, number = null;
			List<string> newArgs = new List<string>();
			int len = 0, totalLength = 0;

			while(true)
			{
				if(newArgs.Count == idE.MaxCommandArgs)
				{
					break; // this is usually something malicious.
				}
				
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}
				
				if((keepAsStrings == false) && (token.Value == "-"))
				{
					// check for negative numbers.
					if((number = lexer.CheckTokenType(TokenType.Number, 0)) != null)
					{
						token.Value = "-" + number;
					}
				}

				// check for cvar expansion
				if(token.Value == "$")
				{
					if((token = lexer.ReadToken()) == null)
					{
						break;
					}

					if(idE.CvarSystem.IsInitialized == true)
					{
						token.Value = idE.CvarSystem.GetString(token.ToString());
					}
					else
					{
						token.Value = "<unknown>";
					}
				}

				len = token.Value.Length;
				totalLength += len + 1;

				// regular token
				newArgs.Add(token.Value);
			}

			_args = newArgs.ToArray();
		}

		public override string ToString()
		{
			return Get(1, -1, false);
		}
		#endregion
	}

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
		private bool _wait;
		private string _cmdBuffer = string.Empty;

		private Dictionary<string, CommandDefinition> _commands = new Dictionary<string, CommandDefinition>(StringComparer.CurrentCultureIgnoreCase);

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

			// TODO
			/*AddCommand("exec", Exec_f, "executes a config file", CommandFlags.System, ArgCompletion_ConfigName);
			AddCommand("vstr", Vstr_f, "inserts the current value of a cvar as command text", CommandFlags.System);
			AddCommand("echo", Echo_f, "prints text", CommandFlags.System);
			AddCommand("parse", Parse_f, "prints tokenized string", CommandFlags.System);
			AddCommand("wait", Wait_f, "delays remaining buffered commands one or more frames", CommandFlags.System);*/

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
		/// Adds command text to the command buffer, does not add a final \n.
		/// </summary>
		/// <param name="text"></param>
		public void BufferCommandText(string text)
		{
			BufferCommandText(Execute.Append, text);
		}

		/// <summary>
		/// Adds command text to the command buffer, does not add a final \n.
		/// </summary>
		/// <param name="exec"></param>
		/// <param name="text"></param>
		public void BufferCommandText(Execute exec, string text)
		{
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
				if(_wait == true)
				{
					// skip out while text still remains in buffer, leaving it for next frame.
					_wait = false;
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
				

				string cmd = _cmdBuffer.Substring(0, i + 1);
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

				// TODO
				/*
					if ( ( cmd->flags & (CMD_FL_CHEAT|CMD_FL_TOOL) ) && session && session->IsMultiplayer() && !cvarSystem->GetCVarBool( "net_allowCheats" ) ) {
						common->Printf( "Command '%s' not valid in multiplayer mode.\n", cmd->name );
						return;
					}
				 */
					
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
			_cmdBuffer += text;
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
}