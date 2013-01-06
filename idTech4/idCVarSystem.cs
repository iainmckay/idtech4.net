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

using idTech4.Services;

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
	public sealed class idCVarSystem : ICVarSystem
	{		
		#region Members
		private CVarFlags _modifiedFlags;
		private Dictionary<string, idCVar> _cvarList = new Dictionary<string, idCVar>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Constructor
		public idCVarSystem()
		{

		}
		#endregion

		#region Methods
		#region Collections
		public void ToDictionary(idDict dict, CVarFlags flags)
		{
			dict.Clear();

			foreach(KeyValuePair<string, idCVar> cvar in _cvarList)
			{
				if((cvar.Value.Flags & flags) != 0)
				{
					dict.Set(cvar.Value.Name, cvar.ToString());
				}
			}
		}
		#endregion
				
		#region Registration
		private idCVar Register(idCVar var)
		{
			idCVar existing = Find(var.Name, true);

			if(existing != null)
			{
				idLog.WriteLine("a cvar already exists with the name '{0}'", var.Name);
			}
			else
			{
				_cvarList.Add(var.Name, var);
			}

			return var;
		}
		#endregion
		#endregion

		#region ICVarSystemService implementation
		#region Methods
		#region Command Completion
		public string[] ArgumentCompletion(string name, string argText)
		{
			CommandArguments args = new CommandArguments(argText, true);
			idCVar var = Find(name);
			List<string> matches = new List<string>();

			if((var != null) && (var.ValueCompletion != null))
			{
				matches.AddRange(var.ValueCompletion.Complete(args));
			}

			return matches.ToArray();
		}

		public string[] CommandCompletion(Predicate<string> filter)
		{
			return Array.FindAll(_cvarList.Keys.ToArray(), filter);
		}		
		#endregion

		#region Find
		public idCVar Find(string name, bool ignoreMissing = false)
		{
			idCVar var;

			if(_cvarList.TryGetValue(name, out var) == true)
			{
				return var;
			}

			if(ignoreMissing == false)
			{
				idLog.Warning("tried to find unknown cvar '{0}'", name);
			}

			return null;
		}
		#endregion

		#region Misc
		/// <summary>
		/// Called by the command system when a command is unrecognized.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public bool Command(CommandArguments args)
		{
			idCVar var = Find(args.Get(0));

			if(var == null)
			{
				return false;
			}

			if(args.Length == 1)
			{
				// print the variable
				idLog.WriteLine("\"{0}\" is: \"{1}\" {2} default: \"{3}\"", var.Name, var.ToString(), idColorString.White, var.ResetString);

				if(var.Description.Length > 0)
				{
					idLog.WriteLine("{0}{1}", idColorString.White, var.Description);
				}
			}
			else
			{
				// set the value
				var.Set(args.ToString(), false, false);
			}

			return true;
		}

		public void ListByFlags(string[] args, CVarFlags flags)
		{
			int argNum = 0;
			string match;
			ShowMode show = ShowMode.Value;
			List<idCVar> list = new List<idCVar>();

			if((StringComparer.OrdinalIgnoreCase.Compare(args[argNum], "-") == 0)
				|| (StringComparer.OrdinalIgnoreCase.Compare(args[argNum], "/") == 0))
			{
				if((StringComparer.OrdinalIgnoreCase.Compare(args[argNum], "help") == 0)
					|| (StringComparer.OrdinalIgnoreCase.Compare(args[argNum], "?") == 0))
				{
					argNum = 3;
					show = ShowMode.Description;
				}
				else if((StringComparer.OrdinalIgnoreCase.Compare(args[argNum + 1], "type") == 0)
					|| (StringComparer.OrdinalIgnoreCase.Compare(args[argNum + 1], "range") == 0))
				{
					argNum = 3;
					show = ShowMode.Type;
				}
				else if(StringComparer.OrdinalIgnoreCase.Compare(args[argNum + 1], "flags") == 0)
				{
					argNum = 3;
					show = ShowMode.Flags;
				}
			}

			if(args.Length > argNum)
			{
				match = string.Join("", args, argNum, args.Length - 1);
				match = match.Replace(" ", string.Empty);
			}
			else
			{
				match = string.Empty;
			}

			foreach(KeyValuePair<string, idCVar> kvp in _cvarList)
			{
				idCVar cvar = kvp.Value;

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
					foreach(idCVar cvar in list)
					{
						idLog.WriteLine("{0}{1}\"{2}\"", cvar.Name.PadRight(32), idColorString.White, cvar.ToString());
					}
					break;

				case ShowMode.Description:
					foreach(idCVar cvar in list)
					{
						idLog.WriteLine("{0}{1}{2}", cvar.Name.PadRight(32), idColorString.White, cvar.Description);
					}
					break;

				case ShowMode.Type:
					foreach(idCVar cvar in list)
					{
						if((cvar.Flags & CVarFlags.Bool) == CVarFlags.Bool)
						{
							idLog.WriteLine("{0}{1}bool", cvar.Name.PadRight(32), idColorString.Cyan);
						}
						else if((cvar.Flags & CVarFlags.Integer) == CVarFlags.Integer)
						{
							if(cvar.MinValue < cvar.MaxValue)
							{
								idLog.WriteLine("{0}{1}int {2}[{3}, {4}]", cvar.Name.PadRight(32), idColorString.Green, idColorString.White, cvar.MinValue, cvar.MaxValue);
							}
							else
							{
								idLog.WriteLine("{0}{1}int", cvar.Name.PadRight(32), idColorString.Green);
							}
						}
						else if((cvar.Flags & CVarFlags.Float) == CVarFlags.Float)
						{
							if(cvar.MinValue < cvar.MaxValue)
							{
								idLog.WriteLine("{0}{1}float {2}[{3}, {4}]", cvar.Name.PadRight(32), idColorString.Red, idColorString.White, cvar.MinValue, cvar.MaxValue);
							}
							else
							{
								idLog.WriteLine("{0}{1}float", cvar.Name.PadRight(32), idColorString.Red);
							}
						}
						else if(cvar.ValueStrings != null)
						{
							idLog.Write("{0}{1}string {2}[", cvar.Name.PadRight(32), idColorString.White, idColorString.White);

							int count = cvar.ValueStrings.Length;

							for(int j = 0; j < count; j++)
							{
								if(j > 0)
								{
									idLog.Write("{0}, {1}", idColorString.White, cvar.ValueStrings[j]);
								}
								else
								{
									idLog.Write("{0}{1}", idColorString.White, cvar.ValueStrings[j]);
								}
							}

							idLog.WriteLine("{0}]", idColorString.White);
						}
						else
						{
							idLog.WriteLine("{0}{1}string", cvar.Name.PadRight(32), idColorString.White);
						}
					}
					break;

				case ShowMode.Flags:
					foreach(idCVar cvar in list)
					{
						idLog.Write(cvar.Name.PadRight(32));

						string str = string.Empty;

						if((cvar.Flags & CVarFlags.Bool) == CVarFlags.Bool)
						{
							str += string.Format("{0}B ", idColorString.Cyan);
						}
						else if((cvar.Flags & CVarFlags.Integer) == CVarFlags.Integer)
						{
							str += string.Format("{0}U ", idColorString.Green);
						}
						else if((cvar.Flags & CVarFlags.Float) == CVarFlags.Float)
						{
							str += string.Format("{0}F ", idColorString.Red);
						}
						else
						{
							str += string.Format("{0}S ", idColorString.White);
						}

						if((cvar.Flags & CVarFlags.System) == CVarFlags.System)
						{
							str += string.Format("{0}SYS  ", idColorString.White);
						}
						else if((cvar.Flags & CVarFlags.Renderer) == CVarFlags.Renderer)
						{
							str += string.Format("{0}RNDR ", idColorString.White);
						}
						else if((cvar.Flags & CVarFlags.Sound) == CVarFlags.Sound)
						{
							str += string.Format("{0}SND  ", idColorString.White);
						}
						else if((cvar.Flags & CVarFlags.Gui) == CVarFlags.Gui)
						{
							str += string.Format("{0}GUI  ", idColorString.White);
						}
						else if((cvar.Flags & CVarFlags.Game) == CVarFlags.Game)
						{
							str += string.Format("{0}GAME ", idColorString.White);
						}
						else if((cvar.Flags & CVarFlags.Tool) == CVarFlags.Tool)
						{
							str += string.Format("{0}TOOL ", idColorString.White);
						}
						else
						{
							str += string.Format("{0}     ", idColorString.White);
						}

						str += ((cvar.Flags & CVarFlags.ServerInfo) == CVarFlags.ServerInfo)	? "SI " : "   ";
						str += ((cvar.Flags & CVarFlags.Static) == CVarFlags.Static)			? "ST " : "   ";
						str += ((cvar.Flags & CVarFlags.Cheat) == CVarFlags.Cheat)				? "CH " : "   ";
						str += ((cvar.Flags & CVarFlags.Init) == CVarFlags.Init)				? "IN " : "   ";
						str += ((cvar.Flags & CVarFlags.ReadOnly) == CVarFlags.ReadOnly)		? "RO " : "   ";
						str += ((cvar.Flags & CVarFlags.Archive) == CVarFlags.Archive)			? "AR " : "   ";
						str += ((cvar.Flags & CVarFlags.Modified) == CVarFlags.Modified)		? "MO " : "   ";

						idLog.WriteLine(str);
					}
					break;
			}

			idLog.WriteLine("\n{0} cvars listed\n", list.Count);
			idLog.WriteLine("listCvar [search string]          = list cvar values");
			idLog.WriteLine("listCvar -help [search string]    = list cvar descriptions");
			idLog.WriteLine("listCvar -type [search string]    = list cvar types");
			idLog.WriteLine("listCvar -flags [search string]   = list cvar flags");
		}

		public void Restart()
		{
			List<string> toRemove = new List<string>();

			foreach(KeyValuePair<string, idCVar> kvp in _cvarList)
			{
				idCVar cvar = kvp.Value;

				// don't mess with rom values
				if((cvar.Flags & (CVarFlags.ReadOnly | CVarFlags.Init)) != 0)
				{
					continue;
				}

				// throw out any variables the user created
				if((cvar.Flags & CVarFlags.Static) == 0)
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

		#region Modification
		public void ClearModified(string name)
		{
			idCVar var = Find(name);

			if(var != null)
			{
				var.IsModified = false;
			}
		}

		public void ClearModifiedFlags(CVarFlags flags)
		{
			_modifiedFlags &= ~flags;
		}

		public bool IsModified(string name)
		{
			idCVar var = Find(name);

			if(var != null)
			{
				return var.IsModified;
			}

			return false;
		}	

		public void Set(string name, string value)
		{
			SetInternal(name, value);
		}

		public void Set(string name, bool value)
		{
			SetInternal(name, ((value == true) ? 1 : 0).ToString());
		}

		public void Set(string name, int value)
		{
			SetInternal(name, value.ToString());
		}

		public void Set(string name, float value)
		{
			SetInternal(name, value.ToString());
		}

		private void SetInternal(string name, string value)
		{
			idCVar var = Find(name);

			if(var != null)
			{
				var.Set(value);
			}
			else
			{
				idLog.Warning("tried to set unknown cvar '{0}', creating...", name);

				// create the cvar if it doesn't exist
				Register(new idCVar(this, name, value, "", 0));
			}
		}

		public void SetModified(string name)
		{
			idCVar var = Find(name);

			if(var != null)
			{
				var.IsModified = true;
			}
		}

		/// <summary>
		/// Sets the modified flags that tell what kind of cvars have changed.
		/// </summary>
		public void SetModifiedFlags(CVarFlags flags)
		{
			_modifiedFlags |= flags;
		}
		#endregion

		#region Registration
		public idCVar Register(string name, string value, string description, CVarFlags flags)
		{
			return Register(new idCVar(this, name, value, description, flags | CVarFlags.Static));
		}

		public idCVar Register(string name, string value, string description, CVarFlags flags, ArgCompletion valueCompletion)
		{
			return Register(new idCVar(this, name, value, description, valueCompletion, flags | CVarFlags.Static));
		}

		public idCVar Register(string name, string value, float valueMin, float valueMax, string description, CVarFlags flags)
		{
			return Register(new idCVar(this, name, value, valueMin, valueMax, description, flags | CVarFlags.Static));
		}

		public idCVar Register(string name, string value, float valueMin, float valueMax, string description, CVarFlags flags, ArgCompletion valueCompletion)
		{
			return Register(new idCVar(this, name, value, valueMin, valueMax, description, valueCompletion, flags | CVarFlags.Static));
		}

		public idCVar Register(string name, string value, string description, string[] valueStrings, CVarFlags flags)
		{
			return Register(new idCVar(this, name, value, description, valueStrings, flags | CVarFlags.Static));
		}

		public idCVar Register(string name, string value, string[] valueStrings, string description, CVarFlags flags, ArgCompletion valueCompletion)
		{
			return Register(new idCVar(this, name, value, valueStrings, description, valueCompletion, flags | CVarFlags.Static));
		}
		#endregion

		#region Value Retrieval
		public string GetString(string name)
		{
			idCVar var = Find(name);

			if(var != null)
			{
				return var.ToString();
			}

			return string.Empty;
		}

		public bool GetBool(string name)
		{
			idCVar var = Find(name);

			if(var != null)
			{
				return var.ToBool();
			}

			return false;
		}

		public int GetInt(string name)
		{
			idCVar var = Find(name);

			if(var != null)
			{
				return var.ToInt();
			}

			return 0;
		}

		public long GetInt64(string name)
		{
			idCVar var = Find(name);

			if(var != null)
			{
				return var.ToInt64();
			}

			return 0;
		}

		public float GetFloat(string name)
		{
			idCVar var = Find(name);

			if(var != null)
			{
				return var.ToFloat();
			}

			return 0;
		}
		#endregion
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
	public enum CVarFlags
	{
		/// <summary>Variable is a boolean.</summary>
		Bool			= 1 << 0,
		/// <summary>Variable is an integer.</summary>
		Integer			= 1 << 1,
		/// <summary>Variable is a float.</summary>
		Float			= 1 << 2,
		/// <summary>System variable.</summary>
		System			= 1 << 3,
		/// <summary>Renderer variable.</summary>
		Renderer		= 1 << 4,
		/// <summary>Sound variable.</summary>
		Sound			= 1 << 5,
		/// <summary>GUI variable.</summary>
		Gui				= 1 << 6,
		/// <summary>Game variable.</summary>
		Game			= 1 << 7,
		/// <summary>Tool variable.</summary>
		Tool			= 1 << 8,
		/// <summary>Sent from servers, available to menu.</summary>
		ServerInfo		= 1 << 10,
		/// <summary>Cvar is synced from the server to clients.</summary>
		NetworkSync		= 1 << 11,
		/// <summary>Statically declared, not user created.</summary>
		Static			= 1 << 12,
		/// <summary>Variable is considered a cheat.</summary>
		Cheat			= 1 << 13,
		/// <summary>Variable is not considered a cheat.</summary>
		NoCheat			= 1 << 14,
		/// <summary>Can only be set from the command-line.</summary>
		Init			= 1 << 15,
		/// <summary>Display only, cannot be set by user at all.</summary>
		ReadOnly		= 1 << 16,
		/// <summary>Set to cause it to be saved to a config file.</summary>
		Archive			= 1 << 17,
		/// <summary>Set when the variable is modified.</summary>
		Modified		= 1 << 18,
		/// <summary>All flags.</summary>
		All				= -1
	}
}