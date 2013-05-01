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

using idTech4.Services;

namespace idTech4.UI.SWF.Scripting
{
	/// <summary>
	/// An object in an action script is a collection of variables. Functions are also variables.
	/// </summary>
	public class idSWFScriptObject
	{
		#region Properties
		public idSWFScriptObject Prototype
		{
			get
			{
				return _prototype;
			}
		}

		public idSWFSpriteInstance Sprite
		{
			get
			{
				if(_objectType == ObjectType.Sprite)
				{
					return _spriteInstance;
				}

				return null;
			}
		}

		public idSWFTextInstance Text
		{
			get
			{
				if(_objectType == ObjectType.Text)
				{
					return _textInstance;
				}

				return null;
			}
		}
		#endregion

		#region Members
		private ObjectType _objectType;
		private idSWFScriptObject _prototype;

		private idSWFSpriteInstance _spriteInstance;	// only valid if _objectType = Sprite
		private	idSWFTextInstance _textInstance;		// only valid if _objectType = Text

		private List<idSWFNamedVariable> _variables                     = new List<idSWFNamedVariable>();
		private Dictionary<string, idSWFNamedVariable> _variablesByName = new Dictionary<string,idSWFNamedVariable>();

		private ushort _flags;
		#endregion

		#region Constructor
		public idSWFScriptObject()
		{
			_objectType = ObjectType.Object;

			Clear();
		}

		public idSWFScriptObject(idSWFSpriteInstance spriteInstance, idSWFScriptObject prototype)
		{
			_objectType     = ObjectType.Sprite;
			_spriteInstance = spriteInstance;
			_prototype      = prototype;

			Clear();
		}

		public idSWFScriptObject(idSWFTextInstance textInstance, idSWFScriptObject prototype)
		{
			_objectType   = ObjectType.Text;
			_textInstance = textInstance;
			_prototype    = prototype;

			Clear();
		}

		// TODO: cleanup
		/*idSWFScriptObject::~idSWFScriptObject() {
			if ( prototype != NULL ) {
				prototype->Release();
			}
		}*/
		#endregion

		#region Set/Get
		public idSWFScriptVariable Get(string name)
		{
			idSWFNamedVariable variable = GetVariable(name, false);

			if(variable == null)
			{
				return new idSWFScriptVariable();
			}
			else
			{
				if(variable.Native != null)
				{
					return variable.Native.Get(this);
				}
				else
				{
					return variable.Value;
				}
			}
		}

		public idSWFScriptVariable Get(int index)
		{
			idSWFNamedVariable variable = GetVariable(index, false);

			if(variable == null)
			{
				return new idSWFScriptVariable();
			}
			else
			{
				if(variable.Native != null)
				{
					return variable.Native.Get(this);
				}
				else
				{
					return variable.Value;
				}
			}
		}

		public idSWFScriptVariable GetDefaultValue(bool stringHint)
		{
			string[] methods = { "toString", "valueOf" };

			if(stringHint == false)
			{
				Array.Reverse(methods);
			}

			for(int i = 0; i < 2; i++)
			{
				idSWFScriptVariable method = Get(methods[i]);

				if(method.IsFunction == true)
				{
					idSWFScriptVariable value = method.Function.Invoke(this, new idSWFParameterList());

					if((value.IsObject == false) && (value.IsFunction == false))
					{
						return value;
					}
				}
			}

			switch(_objectType)
			{
				case ObjectType.Object:
					return new idSWFScriptVariable("[object]");

				case ObjectType.Array:
					return new idSWFScriptVariable("[array]");

				case ObjectType.Sprite:
					if(_spriteInstance != null)
					{
						if(_spriteInstance.Parent == null)
						{
							return new idSWFScriptVariable("[_root]");
						}
						else
						{
							return new idSWFScriptVariable(string.Format("[{0}]", _spriteInstance.Name));
						}
					}
					else
					{
						return new idSWFScriptVariable("[NULL]");
					}
					break;

				case ObjectType.Text:
					return new idSWFScriptVariable("[edittext]");
			}

			return new idSWFScriptVariable("[unknown]");
		}

		public idSWFScriptObject GetNestedObject(string arg1, string arg2 = null, string arg3 = null, string arg4 = null, string arg5 = null, string arg6 = null)
		{
			idSWFScriptVariable var = GetNestedVariable(arg1, arg2, arg3, arg4, arg5, arg6);

			if(var.IsObject == null)
			{
				return null;
			}

			return var.Object;
		}

		public idSWFSpriteInstance GetNestedSprite(string arg1, string arg2 = null, string arg3 = null, string arg4 = null, string arg5 = null, string arg6 = null)
		{
			return GetNestedVariable(arg1, arg2, arg3, arg4, arg5, arg6).ToSprite();
		}

		public idSWFTextInstance GetNestedText(string arg1, string arg2 = null, string arg3 = null, string arg4 = null, string arg5 = null, string arg6 = null)
		{
			return GetNestedVariable(arg1, arg2, arg3, arg4, arg5, arg6).ToText();
		}

		public idSWFScriptVariable GetNestedVariable(string arg1, string arg2 = null, string arg3 = null, string arg4 = null, string arg5 = null, string arg6 = null)
		{
			List<string> list = new List<string>(new string[] {arg1, arg2, arg3, arg4, arg5, arg6});
			list.RemoveAll(x => x == null);

			idSWFScriptObject baseObject = this;
			idSWFScriptVariable retVal   = new idSWFScriptVariable();

			for(int i = 0; i < list.Count; ++i)
			{
				idSWFScriptVariable var = baseObject.Get(list[i]);

				// when at the end of object path just use the latest value as result
				if(i == (list.Count - 1))
				{
					retVal = var;
					break;
				}

				// encountered variable in path that wasn't an object
				if(var.IsObject == false)
				{
					retVal = new idSWFScriptVariable();
					break;
				}

				baseObject = var.Object;
			}

			return retVal;
		}

		public idSWFScriptObject GetObject(int index)
		{
			idSWFScriptVariable var = Get(index);

			if(var.IsObject == true)
			{
				return var.Object;
			}

			return null;
		}

		public idSWFScriptObject GetObject(string name)
		{
			idSWFScriptVariable var = Get(name);

			if(var.IsObject == true)
			{
				return var.Object;
			}

			return null;
		}

		public idSWFSpriteInstance GetSprite(int index)
		{
			return Get(index).ToSprite();
		}

		public idSWFSpriteInstance GetSprite(string name)
		{
			return Get(name).ToSprite();
		}

		public idSWFTextInstance GetText(string name)
		{
			idSWFScriptVariable var = Get(name);

			if(var.IsObject == true)
			{
				return var.Object.Text;
			}

			return null;
		}

		public void Set(int index, idSWFScriptVariable value)
		{
			if(index < 0)
			{
				if(idEngine.Instance.GetService<ICVarSystem>().GetBool("swf_debug") == true)
				{
					idLog.Warning("SWF: Trying to set a negative array index.");
				}

				return;
			}

			if(_objectType == ObjectType.Array)
			{
				idSWFNamedVariable lengthVar = GetVariable("length", true);

				if(lengthVar.Value.ToInt32() <= index)
				{
					lengthVar.Value = new idSWFScriptVariable(index + 1);
				}
			}
			
			idSWFNamedVariable variable = GetVariable(index, true);

			if(variable.Native != null)
			{
				variable.Native.Set(this, value);
			}
			else if((variable.Flags & (idSWFNamedVariable.Flag_ReadOnly)) == 0)
			{
				variable.Value = value;
			}
		}

		public void Set(string name, string value)
		{
			Set(name, new idSWFScriptVariable(value));
		}

		public void Set(string name, idSWFScriptFunction value)
		{
			Set(name, new idSWFScriptVariable(value));
		}

		public void Set(string name, idSWFScriptObject value)
		{
			Set(name, new idSWFScriptVariable(value));
		}

		public void Set(string name, idSWFScriptVariable value)
		{
			SetInternal(name, value);
		}

		public void SetNull(string name)
		{
			SetInternal(name, null);
		}

		private void SetInternal(string name, idSWFScriptVariable value)
		{
			if(_objectType == ObjectType.Array)
			{
				idLog.Warning("TODO: scriptObject setArray");
	
				/*if ( idStr::Cmp( name, "length" ) == 0 ) {
					int newLength = value.ToInteger();
					for ( int i = 0; i < variables.Num(); i++ ) {
						if ( variables[i].index >= newLength ) {
							variables.RemoveIndexFast( i );
							i--;
						}
					}
					// rebuild the hash table
					for ( int i = 0; i < VARIABLE_HASH_BUCKETS; i++ ) {
						variablesHash[i] = -1;
					}
					for ( int i = 0; i < variables.Num(); i++ ) {
						int hash = idStr::Hash( variables[i].name.c_str() ) & ( VARIABLE_HASH_BUCKETS - 1 );
						variables[i].hashNext = variablesHash[hash];
						variablesHash[hash] = i;
					}
				} else {
					int iName = atoi( name );
					if ( iName > 0 || ( iName == 0 && idStr::Cmp( name, "0" ) == 0 ) ) {
						swfNamedVar_t * lengthVar = GetVariable( "length", true );
						if ( lengthVar->value.ToInteger() <= iName ) {
							lengthVar->value = idSWFScriptVar( iName + 1 );
						}
					}
				}*/
			}

			idSWFNamedVariable variable = GetVariable(name, true);

			if(variable.Native != null)
			{
				variable.Native.Set(this, value);
			}
			else if((variable.Flags & idSWFNamedVariable.Flag_ReadOnly) == 0)
			{
				variable.Value = value;
			}
		}
		
		public void SetNative(string name, idSWFScriptNativeVariable native)
		{
			idSWFNamedVariable var = GetVariable(name, true);
			var.Flags |= 1 << 1;
			var.Native = native;

			if(native.IsReadOnly == true)
			{
				var.Flags |= 1 << 1;
			}
		}

		private idSWFNamedVariable GetVariable(int index, bool create)
		{
			for(int i = 0; i < _variables.Count; i++)
			{
				if(_variables[i].Index == index)
				{
					return _variables[i];
				}
			}

			if(create == true)
			{
				idSWFNamedVariable variable = new idSWFNamedVariable();
				variable.Flags = idSWFNamedVariable.Flag_None;
				variable.Index = index;
				variable.Name = index.ToString();

				_variables.Add(variable);
				_variablesByName.Add(variable.Name, variable);
		
				return variable;
			}
	
			return null;
		}

		private idSWFNamedVariable GetVariable(string name, bool create)
		{
			if(_variablesByName.ContainsKey(name) == true)
			{
				return _variablesByName[name];
			}

			if(_prototype != null)
			{
				idSWFNamedVariable variable = _prototype.GetVariable(name, false);

				if((variable != null) && ((variable.Native != null) || (create == false)))
				{
					// If the variable is native, we want to pull it from the prototype even if we're going to set it
					return variable;
				}
			}

			if(create == true)
			{
				idSWFNamedVariable variable = new idSWFNamedVariable();
				variable.Flags = idSWFNamedVariable.Flag_None;
				
				int.TryParse(name, out variable.Index);

				if((variable.Index == 0) && (name == "0"))
				{
					variable.Index = -1;
				}

				variable.Name = name;
				
				_variables.Add(variable);
				_variablesByName.Add(name, variable);

				return variable;
			}
		
			return null;
		}

		public bool HasProperty(string name)
		{
			return (GetVariable(name, false) != null);
		}
		#endregion

		#region State
		public void MakeArray()
		{
			_objectType = ObjectType.Array;

			idSWFNamedVariable variable = GetVariable("length", true);
			variable.Value              = new idSWFScriptVariable(0);
			variable.Flags              = idSWFNamedVariable.Flag_DontEnum;
		}

		public void Clear()
		{
			_variables.Clear();
			_variablesByName.Clear();
		}
		#endregion
	}

	public enum ObjectType
	{
		Object,
		Array,
		Sprite,
		Text
	}

	public class idSWFNamedVariable
	{
		#region Constants
		internal const int Flag_None     = 0;
		internal const int Flag_ReadOnly = (int) (1ul << 1);
		internal const int Flag_DontEnum = (int) (1ul << 2);
		#endregion

		#region Members
		public int	Index;
		public string Name;
		public idSWFScriptVariable Value;
		public idSWFScriptNativeVariable Native;
		public int Flags;
		#endregion

		#region Constructor
		public idSWFNamedVariable()
		{

		}
		#endregion
	}
}