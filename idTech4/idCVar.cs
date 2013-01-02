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
using idTech4.Services;

namespace idTech4
{
	public sealed class idCVar
	{
		#region Properties
		/// <summary>
		/// Gets the name of this cvar.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		/// <summary>
		/// Gets the description of this cvar.
		/// </summary>
		public string Description
		{
			get
			{
				return _description;
			}
		}

		/// <summary>
		/// Gets the flags for this cvar.
		/// </summary>
		public CVarFlags Flags
		{
			get
			{
				return _flags;
			}
		}

		/// <summary>
		/// Gets the minimum value for this cvar.
		/// </summary>
		public float MinValue
		{
			get
			{
				return _valueMin;
			}
		}

		/// <summary>
		/// Gets the max value.
		/// </summary>
		public float MaxValue
		{
			get
			{
				return _valueMax;
			}
		}

		/// <summary>
		/// Gets the value strings used for auto completion.
		/// </summary>
		public string[] ValueStrings
		{
			get
			{
				return _valueStrings;
			}
		}

		/// <summary>
		/// Gets the completion handler.
		/// </summary>
		public ArgCompletion ValueCompletion
		{
			get
			{
				return _valueCompletion;
			}
		}

		/// <summary>
		/// Gets or sets if this cvar has been modified.
		/// </summary>
		public bool IsModified
		{
			get
			{
				return ((_flags & CVarFlags.Modified) == CVarFlags.Modified);
			}
			set
			{
				if(value == true)
				{
					_flags |= CVarFlags.Modified;
				}
				else
				{
					_flags &= ~CVarFlags.Modified;
				}
			}
		}

		/// <summary>
		/// Default value to reset to.
		/// </summary>
		public string ResetString
		{
			get
			{
				return _resetString;
			}
		}
		#endregion

		#region Members
		private string _name;
		private string _description;

		private string _value;
		private string _valueString;
		private string _resetString;

		private CVarFlags _flags;
		private float _valueMin;
		private float _valueMax;
		private string[] _valueStrings;
		private ArgCompletion _valueCompletion;

		private int _intValue;
		private float _floatValue;

		private ICVarSystem _cvarSystem;
		#endregion

		#region Constructor
		internal idCVar(ICVarSystem cvarSystem)
		{
			_cvarSystem = cvarSystem;
		}

		internal idCVar(ICVarSystem cvarSystem, string name, string value, string description, CVarFlags flags) 
			: this(cvarSystem)
		{
			Init(name, value, description, 0, 0, null, flags, null);
		}

		internal idCVar(ICVarSystem cvarSystem, string name, string value, string description, ArgCompletion valueCompletion, CVarFlags flags)
			: this(cvarSystem)
		{
			Init(name, value, description, 1, -1, null, flags, valueCompletion);
		}

		internal idCVar(ICVarSystem cvarSystem, string name, string value, float valueMin, float valueMax, string description, CVarFlags flags)
			: this(cvarSystem, name, value, valueMin, valueMax, description, null, flags)
		{

		}

		internal idCVar(ICVarSystem cvarSystem, string name, string value, float valueMin, float valueMax, string description, ArgCompletion valueCompletion, CVarFlags flags)
			: this(cvarSystem)
		{
			Init(name, value, description, valueMin, valueMax, null, flags, valueCompletion);
		}

		internal idCVar(ICVarSystem cvarSystem, string name, string value, string description, string[] valueStrings, CVarFlags flags)
			: this(cvarSystem, name, value, valueStrings, description, null, flags)
		{

		}

		internal idCVar(ICVarSystem cvarSystem, string name, string value, string[] valueStrings, string description, ArgCompletion valueCompletion, CVarFlags flags)
			: this(cvarSystem)
		{
			Init(name, value, description, 1, -1, valueStrings, flags, valueCompletion);
		}
		#endregion

		#region Methods
		#region Initialization
		private void Init(string name, string value, string description, float valueMin, float valueMax, string[] valueStrings, CVarFlags flags, ArgCompletion valueCompletion)
		{
			_name = name;
			_value = value;
			_valueString = value;
			_resetString = value;

			_flags = flags;
			_description = description;
			_valueMin = valueMin;
			_valueMax = valueMax;
			_valueStrings = valueStrings;
			_valueCompletion = valueCompletion;

			_intValue = 0;
			_floatValue = 0.0f;

			UpdateValue();
			UpdateCheat();
		}
		#endregion

		#region Modification
		public void Set(string value)
		{
			Set(value, true, false);
		}

		public void Set(bool value)
		{
			Set(value.ToString(), true, false);
		}

		public void Set(int value)
		{
			Set(value.ToString(), true, false);
		}

		public void Set(float value)
		{
			Set(value.ToString(), true, false);
		}

		// let the internal commands call this
		internal void Set(string value, bool force, bool fromServer)
		{
			idLog.Warning("TODO: network cvar");

			/*if ( session && session->IsMultiplayer() && !fromServer ) {
#ifndef ID_TYPEINFO
				if ( ( flags & CVAR_NETWORKSYNC ) && idAsyncNetwork::client.IsActive() ) {
					common->Printf( "%s is a synced over the network and cannot be changed on a multiplayer client.\n", nameString.c_str() );
#if ID_ALLOW_CHEATS
					common->Printf( "ID_ALLOW_CHEATS override!\n" );
#else				
					return;
#endif
				}
#endif
				if ( ( flags & CVAR_CHEAT ) && !cvarSystem->GetCVarBool( "net_allowCheats" ) ) {
					common->Printf( "%s cannot be changed in multiplayer.\n", nameString.c_str() );
#if ID_ALLOW_CHEATS
					common->Printf( "ID_ALLOW_CHEATS override!\n" );
#else				
					return;
#endif
				}	
			}*/

			if(value == string.Empty)
			{
				value = _resetString;
			}

			if(force == false)
			{
				if((_flags & CVarFlags.ReadOnly) == CVarFlags.ReadOnly)
				{
					idLog.WriteLine("{0} is read only.", _name);
					return;
				}

				if((_flags & CVarFlags.Init) == CVarFlags.Init)
				{
					idLog.WriteLine("{0} is write protected.", _name);
					return;
				}
			}

			if(_valueString.ToLower() == value.ToLower())
			{
				return;
			}

			_valueString = value;
			_value = _valueString;

			UpdateValue();
			
			this.IsModified = true;
			_cvarSystem.SetModifiedFlags(_flags);
		}
		#endregion

		#region Updating
		private void UpdateValue()
		{
			bool clamped = false;

			if((_flags & CVarFlags.Bool) == CVarFlags.Bool)
			{
				bool tmpValue;
				int tmpValue2;

				if(bool.TryParse(_value, out tmpValue) == false)
				{
					// try to parse as an int to handle 1/0
					if(int.TryParse(_value, out tmpValue2) == true)
					{
						tmpValue = (tmpValue2 == 0) ? false : true;
					}
				}

				_intValue = (tmpValue == true) ? 1 : 0;
				_floatValue = _intValue;

				if((_intValue == 0) || (_intValue == 1))
				{
					_valueString = tmpValue.ToString();
					_value = _valueString;
				}
			}
			else if((_flags & CVarFlags.Integer) == CVarFlags.Integer)
			{
				int.TryParse(_value, out _intValue);

				if(_valueMin < _valueMax)
				{
					if(_intValue < _valueMin)
					{
						_intValue = (int) _valueMin;
						clamped = true;
					}
					else if(_intValue > _valueMax)
					{
						_intValue = (int) _valueMax;
						clamped = true;
					}
				}

				int tmpValue;

				if((clamped == true) || (int.TryParse(_value, out tmpValue) == false) || (_value.IndexOf('.') > 0))
				{
					_valueString = _intValue.ToString();
					_value = _valueString;
				}

				_floatValue = _intValue;
			}
			else if((_flags & CVarFlags.Float) == CVarFlags.Float)
			{
				float.TryParse(_value, out _floatValue);

				if(_valueMin < _valueMax)
				{
					if(_floatValue < _valueMin)
					{
						_floatValue = _valueMin;
						clamped = true;
					}
					else if(_floatValue > _valueMax)
					{
						_floatValue = _valueMax;
						clamped = true;
					}
				}

				float tmpValue;

				if((clamped == true) || (float.TryParse(_value, out tmpValue) == false))
				{
					_valueString = _floatValue.ToString();
					_value = _valueString;
				}

				_intValue = (int) _floatValue;
			}
			else
			{
				if(_valueStrings != null)
				{
					_intValue = 0;
					int count = _valueStrings.Length;

					for(int i = 0; i < count; i++)
					{
						if(_valueString.ToLower() == _valueStrings[i].ToLower())
						{
							_intValue = i;
							break;
						}
					}

					_valueString = _valueStrings[_intValue];
					_value = _valueString;
					_floatValue = _intValue;
				}
				else if(_valueString.Length < 32)
				{
					float.TryParse(_value, out _floatValue);
					_intValue = (int) _floatValue;
				}
				else
				{
					_floatValue = 0;
					_intValue = 0;
				}
			}
		}

		private void UpdateCheat()
		{
			// all variables are considered cheats except for a few types
			if((_flags & (CVarFlags.NoCheat | CVarFlags.Init | CVarFlags.ReadOnly | CVarFlags.Archive | CVarFlags.ServerInfo | CVarFlags.NetworkSync)) != 0)
			{
				_flags &= ~CVarFlags.Cheat;
			}
			else
			{
				_flags |= CVarFlags.Cheat;
			}
		}

		public void Reset()
		{
			_valueString = _resetString;
			_value = _valueString;

			UpdateValue();
		}
		#endregion

		#region Value Retrieval
		public int ToInt()
		{
			return _intValue;
		}

		public long ToInt64()
		{
			return (long) _intValue;
		}

		public float ToFloat()
		{
			return _floatValue;
		}

		public bool ToBool()
		{
			return (_intValue != 0);
		}

		public override string ToString()
		{
			return _value;
		}
		#endregion
		#endregion
	}
}