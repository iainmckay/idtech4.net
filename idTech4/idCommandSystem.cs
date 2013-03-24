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
using System.Linq;
using System.Reflection;
using System.Text;

using idTech4.Services;

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
	public sealed class idCommandSystem : ICommandSystem
	{
		#region Members
		private bool _initialized;

		private int _wait;
		private StringBuilder _cmdBuffer = new StringBuilder();
		private Dictionary<string, List<CommandSignature>> _commands = new Dictionary<string, List<CommandSignature>>(StringComparer.OrdinalIgnoreCase);

		// piggybacks on the text buffer, avoids tokenize again and screwing it up.
		private List<CommandArguments> _tokenizedCommands = new List<CommandArguments>();

		// for scanning
		private Dictionary<Type, Type> _acceptedParameterTypes = new Dictionary<Type, Type>();
		#endregion

		#region Constructor
		public idCommandSystem()
		{
			
		}
		#endregion

		#region Methods
		#region Private
		private void ExecuteCommandText(string text)
		{
			ExecuteTokenizedString(new CommandArguments(text, false));
		}

		private void ExecuteTokenizedString(CommandArguments args)
		{
			if(args.Length == 0)
			{
				return; // no tokens
			}

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			List<CommandSignature> signatures;
			bool executed = false;

			// check registered command functions
			if(_commands.TryGetValue(args.Get(0), out signatures) == true)
			{
				// find a signature that matches the given parameters

				// 1. find a signature that has the same number of parameters
				int paramCount = args.Length - 1;

				foreach(CommandSignature sig in signatures)
				{
					if((paramCount < sig.MinParameterCount) || (paramCount > sig.MaxParameterCount))
					{
						continue;
					}

					// found a signature that will accept our number of parameters and perhaps some optional ones too.
					// see if we can convert our values to what the signature expects
					object[] convertedParameters = new object[paramCount + 1];					
					int i;

					// first parameter is always idEngine
					convertedParameters[0] = idEngine.Instance;

					for(i = 1; i < convertedParameters.Length; i++)
					{
						CommandSignatureParameter param = sig.Parameters[i - 1];

						if((convertedParameters[i] = param.Convert(args.Get(i), i, args)) == null)
						{
							// bad value
							break;
						}
					}

					// did we convert all the parameters to something usable?
					if(i != convertedParameters.Length)
					{
						continue;
					}

					// this will produce confusing behaviour if we've bound multiple signatures to the same command
					// but with completely different flags as we may still find another usable signature.
					if(((sig.Flags & (CommandFlags.Cheat | CommandFlags.Tool)) != 0)
						&& (idEngine.Instance.IsMultiplayer == true) && (cvarSystem.GetBool("net_allowCheats") == false))
					{
						idLog.WriteLine("command '{0}' not valid in multiplayer mode.", sig.Name);
						continue;
					}

					// invoke the command
					sig.Method.Invoke(null, convertedParameters);
					executed = true;

					break;
				}
			}

			if(executed == false)
			{
				// print out accepted signatures
				if(signatures != null)
				{
					foreach(CommandSignature sig in signatures)
					{
						idLog.Write("{0} ", sig.Name);

						foreach(CommandSignatureParameter param in sig.Parameters)
						{
							if(param.IsOptional == true)
							{
								idLog.Write("[{0} = {1}] ", param.Name, param.DefaultValue);
							}
							else
							{
								idLog.Write("<{0}> ", param.Name);
							}
						}

						if(string.IsNullOrEmpty(sig.Description) == false)
						{
							idLog.Write(": {0}", sig.Description);
						}

						idLog.WriteLine(string.Empty);
					}
				}
				else if(cvarSystem.Command(args) == false)
				{
					idLog.WriteLine("unknown command '{0}'", args.Get(0));
				}
			}
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

		#region Scanning
		private void Scan(Assembly assembly)
		{
			idLog.DeveloperWriteLine("loaded '{0}', scanning for commands...", assembly.ManifestModule.Name);

			foreach(Type type in assembly.GetTypes())
			{
				// don't care unless it's a class
				if(type.IsClass == false)
				{
					continue;
				}

				// handlers must be static
				foreach(MethodInfo methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
				{
					// a handler can have multiple command definitions (allow for aliases and such)
					object[] a = methodInfo.GetCustomAttributes(typeof(CommandAttribute), false);

					if(a.Length > 0)
					{
						foreach(CommandAttribute attribute in (CommandAttribute[]) a)
						{
							AddCommand(attribute, methodInfo);
						}
					}
				}
			}
		}

		/// <summary>
		/// Registers a command and the delegate to call for it.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="handler"></param>
		/// <param name="flags"></param>
		/// <param name="description"></param>
		/// <param name="argCompletion"></param>
		private void AddCommand(CommandAttribute attribute, MethodInfo method)
		{
			// we allow multiple signatures for the same command
			List<CommandSignatureParameter> parameters = new List<CommandSignatureParameter>();
			ParameterInfo[] parameterInfoList = method.GetParameters();

			// first parameter must be idEngine
			if((parameterInfoList.Length == 0) || (parameterInfoList[0].ParameterType != typeof(idEngine)))
			{
				idLog.DeveloperWriteLine("...found bad definition {0}, first parameter must be idEngine");
				return;
			}

			for(int i = 1; i < parameterInfoList.Length; i++)
			{
				ParameterInfo parameterInfo = parameterInfoList[i];

				if(IsAcceptableParameter(parameterInfo) == false)
				{
					idLog.DeveloperWriteLine("...found bad definition {0}, we only accept value types for parameters");
					return;
				}

				// build a converter for this
				parameters.Add((CommandSignatureParameter) Activator.CreateInstance(_acceptedParameterTypes[parameterInfo.ParameterType], parameterInfo.ParameterType, parameterInfo.Name, parameterInfo.IsOptional, parameterInfo.DefaultValue));
			}

			idLog.DeveloperWriteLine("...found {0}", attribute.Name);

			CommandSignature cmd = new CommandSignature(attribute.Name, attribute.Description, attribute.Flags, method, parameters.ToArray());

			if(_commands.ContainsKey(cmd.Name) == false)
			{
				_commands.Add(cmd.Name, new List<CommandSignature>());
			}

			_commands[cmd.Name].Add(cmd);
		}	

		private bool IsAcceptableParameter(ParameterInfo parameter)
		{
			if((_acceptedParameterTypes.ContainsKey(parameter.ParameterType) == false)
				|| (parameter.IsOut == true))
			{
				return false;
			}
			
			return true;
		}

		private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			Scan(args.LoadedAssembly);
		}
		#endregion
		#endregion

		#region ICommandSystemService implementation
		#region Properties
		public int Wait
		{
			get
			{
				return _wait;
			}
			set
			{
				_wait = value;
			}
		}
		#endregion

		#region Methods
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

		public void ListByFlags(string[] args, CommandFlags flags)
		{
			string match = string.Join("", args).Replace(" ", "");
			List<CommandSignature> cmdList = new List<CommandSignature>();
			
			foreach(KeyValuePair<string, List<CommandSignature>> kvp in _commands)
			{
				if((kvp.Value[0].Flags & flags) == 0)
				{
					continue;
				}

				if((match.Length > 0) && (kvp.Value[0].Name.StartsWith(match) == false))
				{
					continue;
				}

				cmdList.Add(kvp.Value[0]);
			}

			cmdList.Sort((a, b) => a.Description.CompareTo(b.Description));

			foreach(CommandSignature cmd in cmdList)
			{
				idLog.WriteLine(" {0} {1}", cmd.Name.PadRight(21, ' '), cmd.Description);
			}

			idLog.WriteLine("{0} commands", cmdList.Count);
		}

		public void Scan()
		{
			Scan(this.GetType().Assembly);
		}
		#endregion

		#region Initialization
		#region Properties
		public bool IsInitialized
		{
			get
			{
				return _initialized;
			}
		}
		#endregion

		#region Methods
		public void Initialize()
		{
			if(this.IsInitialized == true)
			{
				throw new Exception("idCommandSystem has already been initialized.");
			}

			_acceptedParameterTypes.Add(typeof(Int16),		typeof(Int16CommandParameter));
			_acceptedParameterTypes.Add(typeof(Int32),		typeof(Int32CommandParameter));
			_acceptedParameterTypes.Add(typeof(Int64),		typeof(Int64CommandParameter));
 			_acceptedParameterTypes.Add(typeof(UInt16),		typeof(UInt16CommandParameter));
			_acceptedParameterTypes.Add(typeof(UInt32),		typeof(UInt32CommandParameter));
			_acceptedParameterTypes.Add(typeof(UInt64),		typeof(UInt64CommandParameter));
			_acceptedParameterTypes.Add(typeof(Single),		typeof(SingleCommandParameter));
			_acceptedParameterTypes.Add(typeof(Double),		typeof(DoubleCommandParameter));

			_acceptedParameterTypes.Add(typeof(String),		typeof(StringCommandParameter));
			_acceptedParameterTypes.Add(typeof(String[]),	typeof(StringListCommandParameter));
			_acceptedParameterTypes.Add(typeof(Boolean),	typeof(BoolCommandParameter));
			_acceptedParameterTypes.Add(typeof(Char),		typeof(CharCommandParameter));
			_acceptedParameterTypes.Add(typeof(Decimal),	typeof(DecimalCommandParameter));
			_acceptedParameterTypes.Add(typeof(Byte),		typeof(ByteCommandParameter));
			_acceptedParameterTypes.Add(typeof(SByte),		typeof(SByteCommandParameter));
						
			_acceptedParameterTypes.Add(typeof(Enum),		typeof(EnumCommandParameter));
		
			AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(OnAssemblyLoad);

			_initialized = true;
		}
		#endregion
		#endregion
		#endregion
		#endregion

		#region CommandDefinition
		private class CommandSignature
		{
			#region Properties
			public string Name
			{
				get
				{
					return _name;
				}
			}

			public string Description
			{
				get
				{
					return _description;
				}
			}

			public CommandFlags Flags
			{
				get
				{
					return _flags;
				}
			}

			public MethodInfo Method
			{
				get
				{
					return _method;
				}
			}

			public CommandSignatureParameter[] Parameters
			{
				get
				{
					return _parameters;
				}
			}

			public int MinParameterCount
			{
				get
				{
					return _minParameterCount;
				}
			}

			public int MaxParameterCount
			{
				get
				{
					return _maxParameterCount;
				}
			}
			#endregion

			#region Members
			private string _name;
			private string _description;
			private CommandFlags _flags;

			private MethodInfo _method;
			private CommandSignatureParameter[] _parameters;

			private int _minParameterCount;
			private int _maxParameterCount;
			#endregion

			#region Constructor
			public CommandSignature(string name, string description, CommandFlags flags, MethodInfo method, CommandSignatureParameter[] parameters)
			{
				_name = name;
				_description = description;
				_flags = flags;
				_method = method;
				_parameters = parameters;

				// figure out what the minimum number of required parameters are
				_maxParameterCount = _parameters.Length;
				_minParameterCount = _maxParameterCount;

				for(int i = 0; i < _parameters.Length; i++)
				{
					if(_parameters[i].IsOptional == true)
					{
						_minParameterCount = i;
						break;
					}
				}
			}
			#endregion
		}

		private abstract class CommandSignatureParameter
		{
			public Type ParameterType;
			public string Name;
			public bool IsOptional;
			public object DefaultValue;

			public CommandSignatureParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
			{
				this.ParameterType = parameterType;
				this.Name = name;
				this.IsOptional = isOptional;
				this.DefaultValue = defaultValue;
			}

			public abstract object Convert(string value, int index, CommandArguments args);
		}

		private sealed class Int16CommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public Int16CommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				short val;

				if(short.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class Int32CommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public Int32CommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				int val;

				if(int.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class Int64CommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public Int64CommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				long val;

				if(long.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class UInt16CommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public UInt16CommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				ushort val;

				if(ushort.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class UInt32CommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public UInt32CommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				uint val;

				if(uint.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class UInt64CommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public UInt64CommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				ulong val;

				if(ulong.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class ByteCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public ByteCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				byte val;

				if(byte.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class SByteCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public SByteCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				sbyte val;

				if(sbyte.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class DecimalCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public DecimalCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				decimal val;

				if(decimal.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class DoubleCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public DoubleCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				double val;

				if(double.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class SingleCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public SingleCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				float val;

				if(float.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class StringCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public StringCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				return value;
			}
			#endregion
		}

		private sealed class StringListCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public StringListCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				return args.ToArray(index, args.Length);
			}
			#endregion
		}

		private sealed class CharCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public CharCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				char val;

				if(char.TryParse(value, out val) == true)
				{
					return val;
				}

				return null;
			}
			#endregion
		}

		private sealed class BoolCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public BoolCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{

			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				bool val;

				if(bool.TryParse(value, out val) == true)
				{
					return val;
				}

				// try some other acceptable values
				switch(value.ToLower())
				{
					case "yes":
					case "1":
						return true;

					case "no":
					case "0":
						return false;
				}

				return null;
			}
			#endregion
		}

		private sealed class EnumCommandParameter : CommandSignatureParameter
		{
			#region Constructor
			public EnumCommandParameter(Type parameterType, string name, bool isOptional, object defaultValue = null)
				: base(parameterType, name, isOptional, defaultValue = null)
			{
				
			}
			#endregion

			#region CommandSignatureParameter implementation
			public override object Convert(string value, int index, CommandArguments args)
			{
				try
				{
					return Enum.Parse(this.ParameterType, value, true);
				}
				catch(OverflowException)
				{
					return false;
				}
				catch(ArgumentException)
				{
					return false;
				}
			}
			#endregion
		}
		#endregion
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	public sealed class CommandAttribute : Attribute
	{
		#region Properties
		public string Name
		{
			get
			{
				return _command;
			}
		}

		public string Description
		{
			get
			{
				return _description;
			}
		}

		public CommandFlags Flags
		{
			get
			{
				return _flags;
			}
		}
		#endregion

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