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

namespace idTech4
{
	/// <summary>
	/// Console Variables (CVars) are used to hold scalar or string variables
	/// that can be changed or displayed at the console as well as accessed
	/// directly in code.
	/// </summary>
	/// <remarks>
	/// CVars are mostly used to hold settings that can be changed from the
	/// console or saved to and loaded from configuration files. CVars are also
	/// occasionally used to communicate information between different modules
	/// of the program.
	/// <p/>
	/// CVars are restricted from having the same names as console commands to
	/// keep the console interface from being ambiguous.
	/// <p/>
	/// CVars can be accessed from the console in three ways:
	/// cvarName			prints the current value
	/// cvarName X			sets the value to X if the variable exists
	/// set cvarName X		as above, but creates the CVar if not present
	/// <p/>
	/// CVars may be declared in classes and in functions.
	/// However declarations in classes and functions should always be static to
	/// save space and time. Making CVars static does not change their
	/// functionality due to their global nature.
	/// <p/>
	/// CVars should be contructed only through one of the constructors with name,
	/// value, flags and description. The name, value and description parameters
	/// to the constructor have to be static strings, do not use va() or the like
	/// functions returning a string.
	/// <p/>
	/// CVars may be declared multiple times using the same name string. However,
	/// they will all reference the same value and changing the value of one CVar
	/// changes the value of all CVars with the same name.
	/// <p/>
	/// CVars should always be declared with the correct type flag: CVAR_BOOL,
	/// CVAR_INTEGER or CVAR_FLOAT. If no such flag is specified the CVar
	/// defaults to type string. If the CVAR_BOOL flag is used there is no need
	/// to specify an argument auto-completion function because the CVar gets
	/// one assigned automatically.
	/// <p/>
	/// CVars are automatically range checked based on their type and any min/max
	/// or valid string set specified in the constructor.
	/// <p/>
	/// CVars are always considered cheats except when CVAR_NOCHEAT, CVAR_INIT,
	/// CVAR_ROM, CVAR_ARCHIVE, CVAR_USERINFO, CVAR_SERVERINFO, CVAR_NETWORKSYNC
	/// is set.
	/// </remarks>
	public sealed class idCvarSystem
	{
		#region Properties
		/// <summary>
		/// Has the cvar system been initialized yet?
		/// </summary>
		public bool IsInitialized
		{
			get
			{
				return _initialized;
			}
		}

		/// <summary>
		/// Gets/sets the modified flags that tell what kind of cvars have changed.
		/// </summary>
		public CvarFlags ModifiedFlags
		{
			get
			{
				return _modifiedFlags;
			}
			set
			{
				_modifiedFlags = value;
			}
		}
		#endregion

		#region Members
		private bool _initialized;
		private CvarFlags _modifiedFlags;
		private Dictionary<string, idInternalCvar> _cvarList = new Dictionary<string, idInternalCvar>(StringComparer.CurrentCultureIgnoreCase);

		internal static List<idCvar> StaticList = new List<idCvar>();
		#endregion

		#region Constructor
		public idCvarSystem()
		{

		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			if(this.IsInitialized == true)
			{
				throw new InvalidOperationException("cvar system already initialized");
			}

			idE.CmdSystem.AddCommand("toggle", "toggles a cvar", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Toggle));
			idE.CmdSystem.AddCommand("set", "sets a cvar", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Set));
			idE.CmdSystem.AddCommand("sets", "sets a cvar and flags it as server info", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_SetS));
			idE.CmdSystem.AddCommand("setu", "sets a cvar and flags it as user info", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_SetU));
			idE.CmdSystem.AddCommand("sett", "sets a cvar and flags it as tool", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_SetT));
			idE.CmdSystem.AddCommand("seta", "sets a cvar and flags it as archive", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_SetA));
			idE.CmdSystem.AddCommand("reset", "resets a cvar", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Reset));
			idE.CmdSystem.AddCommand("listCvars", "list cvars", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_List));
			idE.CmdSystem.AddCommand("cvar_reset", "restart the cvar system", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Restart));

			RegisterStatics();

			_initialized = true;
		}

		public string GetString(string name)
		{
			idInternalCvar intern = FindInternal(name);

			if(intern != null)
			{
				return intern.ToString();
			}

			return string.Empty;
		}

		public bool GetBool(string name)
		{
			idInternalCvar intern = FindInternal(name);

			if(intern != null)
			{
				return intern.ToBool();
			}

			return false;
		}

		public int GetInt(string name)
		{
			idInternalCvar intern = FindInternal(name);

			if(intern != null)
			{
				return intern.ToInt();
			}

			return 0;
		}

		public bool IsModified(string name)
		{
			idInternalCvar intern = FindInternal(name);

			if(intern != null)
			{
				return intern.IsModified;
			}

			return false;
		}

		public void ClearModified(string name)
		{
			idInternalCvar intern = FindInternal(name);

			if(intern != null)
			{
				intern.IsModified = false;
			}
		}

		public void Register(idCvar var)
		{
			var.Internal = var;

			idInternalCvar intern = FindInternal(var.Name);

			if(intern != null)
			{
				intern.Update(var);
			}
			else
			{
				intern = new idInternalCvar(var);

				_cvarList.Add(intern.Name, intern);
			}

			var.Internal = intern;
		}

		/// <summary>
		/// Called by the command system when a command is unrecognized.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public bool Command(idCmdArgs args)
		{
			idInternalCvar intern = FindInternal(args.Get(0));

			if(intern == null)
			{
				return false;
			}

			if(args.Length == 1)
			{
				// print the variable
				idConsole.WriteLine("\"{0}\" is: \"{1}\" {2} default: \"{3}\"", intern.Name, intern.ToString(), idColorString.White, intern.ResetString);

				if(intern.Description.Length > 0)
				{
					idConsole.WriteLine("{0}{1}", idColorString.White, intern.Description);
				}
			}
			else
			{
				// set the value
				intern.Set(args.ToString(), false, false);
			}

			return true;
		}

		public void SetString(string name, string value)
		{
			SetString(name, value, 0);
		}

		public void SetString(string name, string value, CvarFlags flags)
		{
			SetInternal(name, value, flags);
		}

		public void SetBool(string name, bool value)
		{
			SetBool(name, value, 0);
		}

		public void SetBool(string name, bool value, CvarFlags flags)
		{
			SetInternal(name, value.ToString(), flags);
		}

		public void SetInteger(string name, int value)
		{
			SetInteger(name, value, 0);
		}

		public void SetInteger(string name, int value, CvarFlags flags)
		{
			SetInternal(name, value.ToString(), flags);
		}

		public void SetFloat(string name, float value)
		{
			SetFloat(name, value, 0);
		}

		public void SetFloat(string name, float value, CvarFlags flags)
		{
			SetInternal(name, value.ToString(), flags);
		}
		#endregion

		#region Private
		private idInternalCvar FindInternal(string name)
		{
			if(_cvarList.ContainsKey(name) == true)
			{
				return _cvarList[name];
			}

			return null;
		}

		private void SetInternal(string name, string value, CvarFlags flags)
		{
			idInternalCvar intern = FindInternal(name);

			if(intern != null)
			{
				intern.SetStringInternal(value);
				intern.Flags |= flags & ~CvarFlags.Static;
				intern.UpdateCheat();
			}
			else
			{
				intern = new idInternalCvar(name, value, flags);

				_cvarList.Add(intern.Name, intern);
			}
		}

		private void RegisterStatics()
		{
			foreach(idCvar var in StaticList)
			{
				Register(var);
			}

			StaticList.Clear();
		}

		private void ListByFlags(idCmdArgs args, CvarFlags flags)
		{
			int argNum = 1;
			string match;
			ShowMode show = ShowMode.Value;
			List<idCvar> list = new List<idCvar>();

			if((StringComparer.InvariantCultureIgnoreCase.Compare(args.Get(argNum), "-") == 0) || (StringComparer.InvariantCultureIgnoreCase.Compare(args.Get(argNum), "/") == 0))
			{
				if((StringComparer.InvariantCultureIgnoreCase.Compare(args.Get(argNum + 1), "help") == 0) || (StringComparer.InvariantCultureIgnoreCase.Compare(args.Get(argNum + 1), "?") == 0))
				{
					argNum = 3;
					show = ShowMode.Description;
				}
				else if((StringComparer.InvariantCultureIgnoreCase.Compare(args.Get(argNum + 1), "type") == 0) || (StringComparer.InvariantCultureIgnoreCase.Compare(args.Get(argNum + 1), "range") == 0))
				{
					argNum = 3;
					show = ShowMode.Type;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(args.Get(argNum + 1), "flags") == 0)
				{
					argNum = 3;
					show = ShowMode.Flags;
				}
			}

			if(args.Length > argNum)
			{
				match = args.Get(argNum, -1);
				match = match.Replace(" ", "");
			}
			else
			{
				match = "";
			}

			foreach(KeyValuePair<string, idInternalCvar> kvp in _cvarList)
			{
				idInternalCvar cvar = kvp.Value;

				if((cvar.Flags & flags) == 0)
				{
					continue;
				}

				if((match.Length > 0) && (cvar.Name.ToLower().Contains(match.ToLower()) == false))
				{
					continue;
				}

				list.Add(cvar);
			}

			list.OrderBy(a => a.Name);

			switch(show)
			{
				case ShowMode.Value:
					foreach(idCvar cvar in list)
					{
						idConsole.WriteLine("{0}{1}\"{2}\"", cvar.Name.PadRight(32), idColorString.White, cvar.ToString());
					}
					break;

				case ShowMode.Description:
					foreach(idCvar cvar in list)
					{
						idConsole.WriteLine("{0}{1}{2}", cvar.Name.PadRight(32), idColorString.White, idHelper.WrapText(cvar.Description, 77 - 33, 33));
					}
					break;

				case ShowMode.Type:
					foreach(idCvar cvar in list)
					{
						if((cvar.Flags & CvarFlags.Bool) != 0)
						{
							idConsole.WriteLine("{0}{1}bool", cvar.Name.PadRight(32), idColorString.Cyan);
						}
						else if((cvar.Flags & CvarFlags.Integer) != 0)
						{
							if(cvar.MinValue < cvar.MaxValue)
							{
								idConsole.WriteLine("{0}{1}int {2}[{3}, {4}]", cvar.Name.PadRight(32), idColorString.Green, idColorString.White, cvar.MinValue, cvar.MaxValue);
							}
							else
							{
								idConsole.WriteLine("{0}{1}int", cvar.Name.PadRight(32), idColorString.Green);
							}
						}
						else if((cvar.Flags & CvarFlags.Float) != 0)
						{
							if(cvar.MinValue < cvar.MaxValue)
							{
								idConsole.WriteLine("{0}{1}float {2}[{3}, {4}]", cvar.Name.PadRight(32), idColorString.Red, idColorString.White, cvar.MinValue, cvar.MaxValue);
							}
							else
							{
								idConsole.WriteLine("{0}{1}float", cvar.Name.PadRight(32), idColorString.Red);
							}
						}
						else if(cvar.ValueStrings != null)
						{
							idConsole.Write("{0}{1}string {2}[", cvar.Name.PadRight(32), idColorString.White, idColorString.White);

							for(int j = 0; j < cvar.ValueStrings.Length; j++)
							{
								if(j > 0)
								{
									idConsole.Write("{0}, {1}", idColorString.White, cvar.ValueStrings[j]);
								}
								else
								{
									idConsole.Write("{0}{1}", idColorString.White, cvar.ValueStrings[j]);
								}
							}

							idConsole.WriteLine("{0}]", idColorString.White);
						}
						else
						{
							idConsole.WriteLine("{0}{1}string", cvar.Name.PadRight(32), idColorString.White);
						}
					}
					break;

				case ShowMode.Flags:
					foreach(idCvar cvar in list)
					{
						idConsole.Write(cvar.Name.PadRight(32));

						string str = string.Empty;

						if((cvar.Flags & CvarFlags.Bool) != 0)
						{
							str += string.Format("{0}B ", idColorString.Cyan);
						}
						else if((cvar.Flags & CvarFlags.Integer) != 0)
						{
							str += string.Format("{0}U ", idColorString.Green);
						}
						else if((cvar.Flags & CvarFlags.Float) != 0)
						{
							str += string.Format("{0}F ", idColorString.Red);
						}
						else
						{
							str += string.Format("{0}S ", idColorString.White);
						}

						if((cvar.Flags & CvarFlags.System) != 0)
						{
							str += string.Format("{0}SYS  ", idColorString.White);
						}
						else if((cvar.Flags & CvarFlags.Renderer) != 0)
						{
							str += string.Format("{0}RNDR ", idColorString.White);
						}
						else if((cvar.Flags & CvarFlags.Sound) != 0)
						{
							str += string.Format("{0}SND  ", idColorString.White);
						}
						else if((cvar.Flags & CvarFlags.Gui) != 0)
						{
							str += string.Format("{0}GUI  ", idColorString.White);
						}
						else if((cvar.Flags & CvarFlags.Game) != 0)
						{
							str += string.Format("{0}GAME ", idColorString.White);
						}
						else if((cvar.Flags & CvarFlags.Tool) != 0)
						{
							str += string.Format("{0}TOOL ", idColorString.White);
						}
						else
						{
							str += string.Format("{0}     ", idColorString.White);
						}

						str += ((cvar.Flags & CvarFlags.UserInfo) != 0) ? "UI " : "   ";
						str += ((cvar.Flags & CvarFlags.ServerInfo) != 0) ? "SI " : "   ";
						str += ((cvar.Flags & CvarFlags.Static) != 0) ? "ST " : "   ";
						str += ((cvar.Flags & CvarFlags.Cheat) != 0) ? "CH " : "   ";
						str += ((cvar.Flags & CvarFlags.Init) != 0) ? "IN " : "   ";
						str += ((cvar.Flags & CvarFlags.ReadOnly) != 0) ? "RO " : "   ";
						str += ((cvar.Flags & CvarFlags.Archive) != 0) ? "AR " : "   ";
						str += ((cvar.Flags & CvarFlags.Modified) != 0) ? "MO " : "   ";

						idConsole.WriteLine(str);						
					}
					break;
			}

			idConsole.WriteLine("\n{0} cvars listed\n", list.Count);
			idConsole.WriteLine("listCvar [search string]          = list cvar values");
			idConsole.WriteLine("listCvar -help [search string]    = list cvar descriptions");
			idConsole.WriteLine("listCvar -type [search string]    = list cvar types");
			idConsole.WriteLine("listCvar -flags [search string]   = list cvar flags");
		}
		#endregion

		#region Command handlers
		private void Cmd_Toggle(object sender, CommandEventArgs e)
		{
			if(e.Args.Length < 2)
			{
				idConsole.WriteLine("usage:");
				idConsole.WriteLine("    toggle <variable> - toggles between 0 and 1");
				idConsole.WriteLine("    toggle <variable> <value> - toggles between 0 and <value>");
				idConsole.WriteLine("    toggle <variable [string 1] [string 2]...[string n] - cycles through all strings");
			}
			else
			{
				idInternalCvar cvar = idE.CvarSystem.FindInternal(e.Args.Get(1));

				if(cvar == null)
				{
					idConsole.WriteLine("toggle: cvar \"{0}\" not found", e.Args.Get(1));
				}
				else if(e.Args.Length > 3)
				{
					// cycle through multiple values
					string text = cvar.ToString();
					int i = 0;

					for(i = 2; i < e.Args.Length; i++)
					{
						if(StringComparer.CurrentCultureIgnoreCase.Compare(text, e.Args.Get(i)) == 0)
						{
							i++;
							break;
						}
					}

					if(i >= e.Args.Length)
					{
						i = 2;
					}

					idConsole.WriteLine("set {0} = {1}", e.Args.Get(1), e.Args.Get(i));
					cvar.Set(e.Args.Get(i), false, false);
				}
				else
				{
					// toggle between 0 and 1
					float current = cvar.ToFloat();
					float set = 0;

					if(e.Args.Length == 3)
					{
						float.TryParse(e.Args.Get(2), out set);
					}
					else
					{
						set = 1.0f;
					}

					if(current == 0.0f)
					{
						current = set;
					}
					else
					{
						current = 0.0f;
					}

					idConsole.WriteLine("set {0} = {1}", e.Args.Get(1), current);
					cvar.Set(current.ToString(), false, false);
				}
			}
		}

		private void Cmd_Set(object sender, CommandEventArgs e)
		{
			idE.CvarSystem.SetString(e.Args.Get(1), e.Args.Get(2, e.Args.Length - 1));
		}

		private void Cmd_SetS(object sender, CommandEventArgs e)
		{
			Cmd_Set(sender, e);

			idInternalCvar cvar = FindInternal(e.Args.Get(1));

			if(cvar != null)
			{
				cvar.Flags |= CvarFlags.ServerInfo | CvarFlags.Archive;
			}
		}

		private void Cmd_SetU(object sender, CommandEventArgs e)
		{
			Cmd_Set(sender, e);

			idInternalCvar cvar = FindInternal(e.Args.Get(1));

			if(cvar != null)
			{
				cvar.Flags |= CvarFlags.UserInfo | CvarFlags.Archive;
			}
		}

		private void Cmd_SetT(object sender, CommandEventArgs e)
		{
			Cmd_Set(sender, e);

			idInternalCvar cvar = FindInternal(e.Args.Get(1));

			if(cvar != null)
			{
				cvar.Flags |= CvarFlags.Tool;
			}
		}

		private void Cmd_SetA(object sender, CommandEventArgs e)
		{
			Cmd_Set(sender, e);

			idInternalCvar cvar = FindInternal(e.Args.Get(1));

			if(cvar != null)
			{
				cvar.Flags |= CvarFlags.Archive;
			}
		}

		private void Cmd_Reset(object sender, CommandEventArgs e)
		{
			if(e.Args.Length != 2)
			{
				idConsole.WriteLine("usage: reset <variable>");
			}
			else
			{
				idInternalCvar cvar = FindInternal(e.Args.Get(1));

				if(cvar != null)
				{
					cvar.Reset();
				}
			}
		}

		private void Cmd_List(object sender, CommandEventArgs e)
		{
			ListByFlags(e.Args, CvarFlags.All);
		}

		private void Cmd_Restart(object sender, CommandEventArgs e)
		{
			List<string> toRemove = new List<string>();

			foreach(KeyValuePair<string, idInternalCvar> kvp in _cvarList)
			{
				idInternalCvar cvar = kvp.Value;

				// don't mess with rom values
				if((cvar.Flags & (CvarFlags.ReadOnly | CvarFlags.Init)) != 0)
				{
					continue;
				}

				// throw out any variables the user created
				if((cvar.Flags & CvarFlags.Static) == 0)
				{
					toRemove.Add(cvar.Name);
				}
				else
				{
					cvar.Reset();
				}
			}

			foreach(string name in toRemove)
			{
				_cvarList.Remove(name);
			}
		}
		#endregion
		#endregion

		#region Show flags
		private enum ShowMode
		{
			Value,
			Description,
			Type,
			Flags
		}
		#endregion
	}

	[Flags]
	public enum CvarFlags
	{
		/// <summary>All flags.</summary>
		All = -1,
		/// <summary>Variable is a boolean.</summary>
		Bool = 1 << 0,
		/// <summary>Variable is an integer.</summary>
		Integer = 1 << 1,
		/// <summary>Variable is a float.</summary>
		Float = 1 << 2,
		/// <summary>System variable.</summary>
		System = 1 << 3,
		/// <summary>Renderer variable.</summary>
		Renderer = 1 << 4,
		/// <summary>Sound variable.</summary>
		Sound = 1 << 5,
		/// <summary>GUI variable.</summary>
		Gui = 1 << 6,
		/// <summary>Game variable.</summary>
		Game = 1 << 7,
		/// <summary>Tool variable.</summary>
		Tool = 1 << 8,
		/// <summary>Sent to servers, available to menu.</summary>
		UserInfo = 1 << 9,
		/// <summary>Sent from servers, available to menu.</summary>
		ServerInfo = 1 << 10,
		/// <summary>Cvar is synced from the server to clients.</summary>
		NetworkSync = 1 << 11,
		/// <summary>Statically declared, not user created.</summary>
		Static = 1 << 12,
		/// <summary>Variable is considered a cheat.</summary>
		Cheat = 1 << 13,
		/// <summary>Variable is not considered a cheat.</summary>
		NoCheat = 1 << 14,
		/// <summary>Can only be set from the command-line.</summary>
		Init = 1 << 15,
		/// <summary>Display only, cannot be set by user at all.</summary>
		ReadOnly = 1 << 16,
		/// <summary>Set to cause it to be saved to a config file.</summary>
		Archive = 1 << 17,
		/// <summary>Set when the variable is modified.</summary>
		Modified = 1 << 18
	}
}