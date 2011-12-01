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
	public class idCvar
	{
		#region Properties
		/// <summary>
		/// Gets the name of this cvar.
		/// </summary>
		public string Name
		{
			get
			{
				return _internal._name;
			}
		}

		/// <summary>
		/// Gets the description of this cvar.
		/// </summary>
		public string Description
		{
			get
			{
				return _internal._description;
			}
		}

		/// <summary>
		/// Gets the flags for this cvar.
		/// </summary>
		public CvarFlags Flags
		{
			get
			{
				return _internal._flags;
			}
			internal set
			{
				_internal._flags = value;
			}
		}

		/// <summary>
		/// Gets the minimum value for this cvar.
		/// </summary>
		public float MinValue
		{
			get
			{
				return _internal._valueMin;
			}
		}

		/// <summary>
		/// Gets the max value.
		/// </summary>
		public float MaxValue
		{
			get
			{
				return _internal._valueMax;
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
		public EventHandler<EventArgs> ValueCompletionHandler
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
				return ((_flags & CvarFlags.Modified) != 0);
			}
			set
			{
				if(value == true)
				{
					_flags |= CvarFlags.Modified;
				}
				else
				{
					_flags &= ~CvarFlags.Modified;
				}
			}
		}

		internal idCvar Internal
		{
			get
			{
				return _internal;
			}
			set
			{
				_internal = value;
			}
		}
		#endregion

		#region Members
		protected string _name; // name.
		protected string _description; // description.
		protected string _value; // value.
		protected CvarFlags _flags; // flags.
		protected float _valueMin; // minimum value.
		protected float _valueMax; // maximum value.
		protected string[] _valueStrings; // valid value strings.
		protected EventHandler<EventArgs> _valueCompletion; // auto-completion handler.

		protected int _intValue;
		protected float _floatValue;

		protected idCvar _internal;
		#endregion

		#region Constructor
		internal idCvar()
		{

		}

		public idCvar(string name, string value, string description, CvarFlags flags)
			: this(name, value, description, flags, null)
		{

		}

		public idCvar(string name, string value, string description, CvarFlags flags, EventHandler<EventArgs> valueCompletion)
		{
			Init(name, value, description, 1, -1, null, flags, valueCompletion);
		}

		public idCvar(string name, string value, string description, float valueMin, float valueMax, CvarFlags flags)
			: this(name, value, description, valueMin, valueMax, flags, null)
		{

		}

		public idCvar(string name, string value, string description, float valueMin, float valueMax, CvarFlags flags, EventHandler<EventArgs> valueCompletion)
		{
			Init(name, value, description, valueMin, valueMax, null, flags, valueCompletion);
		}

		public idCvar(string name, string value, string description, string[] valueStrings, CvarFlags flags)
			: this(name, value, description, valueStrings, flags, null)
		{

		}

		public idCvar(string name, string value, string description, string[] valueStrings, CvarFlags flags, EventHandler<EventArgs> valueCompletion)
		{
			Init(name, value, description, 1, -1, valueStrings, flags, valueCompletion);
		}
		#endregion

		#region Methods
		#region Public
		public void SetString(string value)
		{
			_internal.SetStringInternal(value);
		}

		public void SetBool(bool value)
		{
			_internal.SetBoolInternal(value);
		}

		public void SetInteger(int value)
		{
			_internal.SetIntegerInternal(value);
		}

		public void SetFloat(float value)
		{
			_internal.SetFloatInternal(value);
		}

		public int ToInt()
		{
			return _internal._intValue;
		}

		public float ToFloat()
		{
			return _internal._floatValue;
		}

		public bool ToBool()
		{
			return (_internal._intValue != 0);
		}

		public override string ToString()
		{
			return _internal._value;
		}
		#endregion

		#region Private
		private void Init(string name, string value, string description, float valueMin, float valueMax, string[] valueStrings, CvarFlags flags, EventHandler<EventArgs> valueCompletion)
		{
			_name = name;
			_value = value;
			_flags = flags | CvarFlags.Static;
			_description = description;
			_valueMin = valueMin;
			_valueMax = valueMax;
			_valueStrings = valueStrings;
			_valueCompletion = valueCompletion;

			_intValue = 0;
			_floatValue = 0.0f;

			_internal = this;

			if(idCvarSystem.StaticList != null)
			{
				idCvarSystem.StaticList.Add(this);
			}
			else
			{
				idE.CvarSystem.Register(this);
			}
		}
		#endregion

		#region Protected
		internal virtual void SetStringInternal(string value)
		{

		}

		internal virtual void SetBoolInternal(bool value)
		{

		}

		internal virtual void SetIntegerInternal(int value)
		{

		}

		internal virtual void SetFloatInternal(float value)
		{

		}
		#endregion
		#endregion
	}

	internal class idInternalCvar : idCvar
	{
		#region Properties
		public string ResetString
		{
			get
			{
				return _resetString;
			}
		}
		#endregion

		#region Members
		private string _nameString;			// name.
		private string _resetString;		// resetting will change to this value.
		private string _valueString;		// value.
		private string _descriptionString;	// description.
		#endregion

		#region Constructor
		public idInternalCvar(string name, string value, CvarFlags flags)
		{
			_name = name;
			_nameString = name;

			_value = value;
			_valueString = value;
			_resetString = value;

			_description = string.Empty;
			_descriptionString = string.Empty;

			_flags = (flags & ~CvarFlags.Static) | CvarFlags.Modified;

			_valueMin = 1;
			_valueMax = -1;
			_valueStrings = null;

			UpdateValue();
			UpdateCheat();

			_internal = this;
		}

		public idInternalCvar(idCvar var)
		{
			_name = var.Name;
			_nameString = var.Name;
			_value = var.ToString();
			_valueString = var.ToString();
			_resetString = var.ToString();
			_description = var.Description;
			_descriptionString = var.Description;
			_flags = var.Flags | CvarFlags.Modified;
			_valueMin = var.MinValue;
			_valueMax = var.MaxValue;
			_valueStrings = var.ValueStrings;
			_valueCompletion = var.ValueCompletionHandler;

			UpdateValue();
			UpdateCheat();

			_internal = this;
		}
		#endregion

		#region Methods
		#region Public
		public void Update(idCvar var)
		{
			// if this is a statically declared variable
			if((var.Flags & CvarFlags.Static) != 0)
			{
				if((_flags & CvarFlags.Static) != 0)
				{
					// the code has more than one static declaration of the same variable, make sure they have the same properties
					if(_resetString.ToLower() == var.ToString().ToLower())
					{
						idConsole.Warning("cvar '{0}' declared multiple times with different initial value", _nameString);
					}

					if((_flags & (CvarFlags.Bool | CvarFlags.Integer | CvarFlags.Float)) != (var.Flags & (CvarFlags.Bool | CvarFlags.Integer | CvarFlags.Float)))
					{
						idConsole.Warning("cvar '{0}' declared multiple times with different type", _nameString);
					}

					if((_valueMin != var.MinValue) || (_valueMax != var.MaxValue))
					{
						idConsole.Warning("cvar '{0}' declared multiple times with different minimum/maximum", _nameString);
					}
				}

				// the code is now specifying a variable that the user already set a value for, take the new value as the reset value
				_resetString = var.ToString();
				_descriptionString = var.Description;
				_description = var.Description;
				_valueMin = var.MinValue;
				_valueMax = var.MaxValue;
				_valueStrings = var.ValueStrings;
				_valueCompletion = var.ValueCompletionHandler;

				UpdateValue();

				idE.CvarSystem.ModifiedFlags = var.Flags;
			}

			_flags |= var.Flags;

			UpdateCheat();

			// only allow one non-empty reset string without a warning
			if(_resetString == string.Empty)
			{
				_resetString = var.ToString();
			}
			else if(var.ToString().ToLower() == _resetString.ToLower())
			{
				idConsole.Warning("cvar \"{0}\" given initial values: \"{1}\" and \"{2}\"", _nameString, _resetString, var.ToString());
			}
		}

		public void Reset()
		{
			_valueString = _resetString;
			_value = _valueString;

			UpdateValue();
		}
		#endregion

		#region Private
		private void UpdateValue()
		{
			bool clamped = false;

			if((_flags & CvarFlags.Bool) != 0)
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
			else if((_flags & CvarFlags.Integer) != 0)
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
			else if((_flags & CvarFlags.Float) != 0)
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

					for(int i = 0; i < _valueString.Length; i++)
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

		internal void UpdateCheat()
		{
			// all variables are considered cheats except for a few types
			if((_flags & (CvarFlags.NoCheat | CvarFlags.Init | CvarFlags.ReadOnly | CvarFlags.Archive | CvarFlags.UserInfo | CvarFlags.ServerInfo | CvarFlags.NetworkSync)) != 0)
			{
				_flags &= ~CvarFlags.Cheat;
			}
			else
			{
				_flags |= CvarFlags.Cheat;
			}
		}

		internal void Set(string value, bool force, bool fromServer)
		{
			// TODO
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
				if((_flags & CvarFlags.ReadOnly) != 0)
				{
					idConsole.WriteLine("{0} is read only.", _nameString);
					return;
				}

				if((_flags & CvarFlags.Init) != 0)
				{
					idConsole.WriteLine("{0} is write protected.", _nameString);
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

			idE.CvarSystem.ModifiedFlags = _flags;
		}
		#endregion

		#region Protected
		internal override void SetStringInternal(string value)
		{
			Set(value, true, false);
		}

		internal override void SetBoolInternal(bool value)
		{
			Set(value.ToString(), true, false);
		}

		internal override void SetIntegerInternal(int value)
		{
			Set(value.ToString(), true, false);
		}

		internal override void SetFloatInternal(float value)
		{
			Set(value.ToString(), true, false);
		}
		#endregion
		#endregion
	}
}