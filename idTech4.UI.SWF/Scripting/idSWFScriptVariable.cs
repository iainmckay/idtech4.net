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
using System.Diagnostics;

using idTech4.Services;

namespace idTech4.UI.SWF.Scripting
{
	/// <summary>
	/// A variable in an action script.
	/// </summary>
	/// <remarks>
	/// These can be on the stack, in a script object, passed around as parameters, etc
	/// they can contain raw data (int, float), strings, functions, or objects
	/// </remarks>
	public class idSWFScriptVariable : ICloneable
	{
		#region Properties
		public idSWFScriptFunction Function
		{
			get
			{
				Debug.Assert(_type == ScriptVariableType.Function);

				return _valueFunction;
			}
		}

		public idSWFScriptObject Object
		{
			get
			{
				Debug.Assert(_type == ScriptVariableType.Object);

				return _valueObject;
			}
		}

		public bool IsString
		{
			get
			{
				return ((_type == ScriptVariableType.String) || (_type == ScriptVariableType.StringID));
			}
		}

		public bool IsNull
		{
			get
			{
				return (_type == ScriptVariableType.Null);
			}
		}

		public bool IsUndefined
		{
			get
			{
				return (_type == ScriptVariableType.Undefined);
			}
		}

		public bool IsValid
		{
			get
			{
				return ((_type != ScriptVariableType.Undefined) && (_type != ScriptVariableType.Null));
			}
		}

		public bool IsFunction
		{
			get
			{
				return (_type == ScriptVariableType.Function);
			}
		}

		public bool IsObject
		{
			get
			{
				return (_type == ScriptVariableType.Object);
			}
		}

		public bool IsNumeric
		{
			get
			{
				return ((_type == ScriptVariableType.Float) || (_type == ScriptVariableType.Integer) || (_type == ScriptVariableType.Bool));
			}
		}
		#endregion

		#region Members
		private ScriptVariableType _type;

		private float _valueFloat;
		private int	_valueInt;
		private bool _valueBool;
		private string _valueString;
		private idSWFScriptObject _valueObject;
		private idSWFScriptFunction _valueFunction;
		#endregion

		#region Constructors
		public idSWFScriptVariable()
		{
			_type = ScriptVariableType.Undefined;
		}

		public idSWFScriptVariable(idSWFScriptVariable other)
		{
			_type          = other._type;

			_valueFloat    = other._valueFloat;
			_valueInt      = other._valueInt;
			_valueBool     = other._valueBool;
			_valueObject   = other._valueObject;
			_valueString   = other._valueString;
			_valueFunction = other._valueFunction;
		}

		public idSWFScriptVariable(idSWFScriptObject value)
		{
			Set(value);
		}

		public idSWFScriptVariable(string value)
		{
			Set(value);
		}

		public idSWFScriptVariable(float value)
		{
			Set(value);
		}

		public idSWFScriptVariable(bool value)
		{
			Set(value);
		}

		public idSWFScriptVariable(int value)
		{
			Set(value);
		}

		public idSWFScriptVariable(idSWFScriptFunction value)
		{
			Set(value);
		}
		#endregion
		
		#region Misc.
		public string TypeOf()
		{
			switch(_type)
			{
				case ScriptVariableType.StringID:
					return "stringid";

				case ScriptVariableType.String:
					return "string";

				case ScriptVariableType.Float:
					return "number";

				case ScriptVariableType.Bool:
					return "boolean";

				case ScriptVariableType.Integer:
					return "number";

				case ScriptVariableType.Object:
					if(_valueObject.Sprite != null)
					{
						return "movieclip";
					}
					else if(_valueObject.Text != null)
					{
						return "text";
					}
					else
					{
						return "object";
					}
					break;

				case ScriptVariableType.Function:
					return "function";

				case ScriptVariableType.Null:
					return "null";

				case ScriptVariableType.Undefined:
					return "undefined";
			}

			return string.Empty;
		}					
		#endregion

		#region Set
		public void Set(string value)
		{
			Clear();

			_type        = ScriptVariableType.String;
			_valueString = value;
		}

		public void Set(float value)
		{
			Clear();

			_type       = ScriptVariableType.Float;
			_valueFloat = value;
		}

		public void Set(int value)
		{
			Clear();

			_type     = ScriptVariableType.Integer;
			_valueInt = value;
		}

		public void Set(bool value)
		{
			Clear();

			_type      = ScriptVariableType.Bool;
			_valueBool = value;
		}

		public void Set(idSWFScriptObject value)
		{
			Clear();

			if(value == null)
			{
				_type = ScriptVariableType.Null;
			}
			else
			{
				_type        = ScriptVariableType.Object;
				_valueObject = value;
			}
		}

		public void Set(idSWFScriptFunction value)
		{
			Clear();

			if(value == null)
			{
				_type = ScriptVariableType.Null;
			}
			else
			{
				_type = ScriptVariableType.Function;
				_valueFunction = value;
			}
		}
		
		public void SetNull()
		{
			Clear();

			_type = ScriptVariableType.Null;
		}

		public void SetUndefined()
		{
			Clear();

			_type = ScriptVariableType.Undefined;
		}

		private void Clear()
		{
			_valueFunction = null;
			_valueObject   = null;
			_valueString   = null;
		}
		#endregion

		#region To*
		public bool ToBool()
		{
			switch(_type)
			{
				case ScriptVariableType.String:
					return ((_valueString.Equals("true", StringComparison.OrdinalIgnoreCase) == true) || (_valueString == "1"));

				case ScriptVariableType.Float:
					return (_valueFloat != 0.0f);

				case ScriptVariableType.Bool:
					return _valueBool;

				case ScriptVariableType.Integer:
					return (_valueInt != 0);

				case ScriptVariableType.Object:
					return _valueObject.GetDefaultValue(false).ToBool();

				default:
					return false;
			}
		}

		public float ToFloat()
		{
			switch(_type)
			{
				case ScriptVariableType.String:
					float tmp;
					float.TryParse(_valueString, out tmp);

					return tmp;

				case ScriptVariableType.Float:
					return _valueFloat;

				case ScriptVariableType.Bool:
					return (_valueBool ? 1 : 0);

				case ScriptVariableType.Integer:
					return _valueInt;

				case ScriptVariableType.Object:
					return _valueObject.GetDefaultValue(false).ToFloat();

				case ScriptVariableType.Function:
				case ScriptVariableType.Null:
				case ScriptVariableType.Undefined:
					return 0;
			}

			return 0;
		}

		public int ToInt32()
		{
			switch(_type)
			{
				case ScriptVariableType.String:
					int tmp;
					int.TryParse(_valueString, out tmp);

					return tmp;

				case ScriptVariableType.Float:
					return (int) _valueFloat;

				case ScriptVariableType.Bool:
					return (_valueBool ? 1 : 0);

				case ScriptVariableType.Integer:
					return _valueInt;

				case ScriptVariableType.Object:
					return _valueObject.GetDefaultValue(false).ToInt32();

				case ScriptVariableType.Function:
				case ScriptVariableType.Null:
				case ScriptVariableType.Undefined:
					return 0;
			}

			return 0;
		}

		public idSWFSpriteInstance ToSprite()
		{
			if((this.IsObject == true) && (_valueObject != null))
			{
				return _valueObject.Sprite;
			}

			return null;
		}

		public override string ToString()
		{

			switch(_type)
			{
				case ScriptVariableType.StringID:
					throw new NotImplementedException();

				case ScriptVariableType.String:
					return _valueString;

				case ScriptVariableType.Float:
					return _valueFloat.ToString("G");

				case ScriptVariableType.Bool:
					return (_valueBool ? "true" : "false");

				case ScriptVariableType.Integer:
					return _valueInt.ToString();

				case ScriptVariableType.Null:
					return "[null]";

				case ScriptVariableType.Undefined:
					return "[undefined]";

				case ScriptVariableType.Object:
					return _valueObject.GetDefaultValue(true).ToString();

				case ScriptVariableType.Function:
					return "[function]";
			}

			return string.Empty;
		}
		#endregion

		#region ICloneable implementation
		public object Clone()
		{
			idSWFScriptVariable var = new idSWFScriptVariable();
			var.Clear();

			var._type          = _type;
			var._valueBool     = _valueBool;
			var._valueFloat    = _valueFloat;
			var._valueFunction = _valueFunction;
			var._valueInt      = _valueInt;
			var._valueObject   = _valueObject;
			var._valueString   = _valueString;

			return var;
		}
		#endregion
	}

	public enum ScriptVariableType
	{
		StringID,
		String,
		Float,
		Null,
		Undefined,
		Bool,
		Integer,
		Function,
		Object
	}
}