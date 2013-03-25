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
using System.Diagnostics;

namespace idTech4.UI.SWF.Scripting
{
	/// <summary>
	/// A script function that's implemented in action script.
	/// </summary>
	public class idSWFScriptFunction_Script : idSWFScriptFunction
	{
		#region Properties
		public byte[] Data
		{
			set
			{
				_bitStream.Data = value;
			}
		}
		#endregion

		#region Members
		private List<idSWFScriptVariable> _registers;
		private List<idSWFScriptObject> _scope = new List<idSWFScriptObject>();
		private idSWFSpriteInstance _defaultSprite;		// some actions have an implicit sprite they work off of (e.g. Action_GotoFrame outside of object scope)

		private idSWFBitStream _bitStream = new idSWFBitStream();

		private ushort _flags;
		#endregion

		#region Constructor
		public idSWFScriptFunction_Script(List<idSWFScriptObject> scope, idSWFSpriteInstance defaultSprite)
		{
			_registers     = new List<idSWFScriptVariable>(4);
			_defaultSprite = defaultSprite;

			SetScope(scope);			
		}
		#endregion

		#region Misc.
		private void SetScope(List<idSWFScriptObject> newScope) 
		{
			Debug.Assert(_scope.Count == 0);

			_scope.Clear();
			_scope.AddRange(newScope);
		}
		#endregion

		#region idSWFScriptFunction implementation
		public override idSWFScriptVariable Invoke(idSWFScriptObject scriptObj, idSWFParameterList parms)
		{
			if(_bitStream.HasData == false)
			{
				throw new InvalidOperationException("No data loaded.");
			}

			_bitStream.Rewind();

			// We assume scope[0] is the global scope
			Debug.Assert(_scope.Count > 0);

			if(scriptObj == null)
			{
				scriptObj = _scope[0];
			}

			idSWFScriptObject locals = new idSWFScriptObject();
			idSWFStack stack = new idSWFStack(parms.Count + 1);

			for(int i = 0; i < parms.Count; i++)
			{
				stack[parms.Count - i - 1] = parms[i];

				// unfortunately at this point we don't have the function name anymore, so our warning messages aren't very detailed
				if(i < _parameters.Length)
				{
					if((_parameters[i].Register > 0) && (_parameters[i].Register < _registers.Count))
					{
						_registers[_parameters[i].Register] = parms[i];
					}
				
					locals.Set(_parameters[i].Name, parms[i]);
				}
			}

			// set any additional parameters to undefined
			for(int i = parms.Count; i < _parameters.Length; i++)
			{
				if((_parameters[i].Register > 0) && (_parameters[i].Register < _registers.Count))
				{
					_registers[_parameters[i].Register].SetUndefined();
				}

				locals.Set(_parameters[i].Name, new idSWFScriptVariable());
			}

			stack.A().SetInteger(parms.Count);

			int preloadReg = 1;

			if((_flags & (1ul << 0)) != 0)
			{
				// load "this" into a register
				_registers[preloadReg].Set(scriptObj);
				preloadReg++;
			}

			if((_flags & (1ul << 1)) != 0)
			{
				// create "this"
				locals.Set("this", new idSWFScriptVariable(scriptObj));
			}

			if((_flags & (1ul << 2)) != 0)
			{
				idSWFScriptObject arguments = new idSWFScriptObject();

				// load "arguments" into a register
				arguments->MakeArray();

				int elementCount = parms.Count;

				for(int i = 0; i < elementCount; i++)
				{
					arguments.Set(i, parms[i]);
				}

				_registers[preloadReg].Set(arguments);
				preloadReg++;				
			}

			if((_flags & (1ul << 3)) != 0)
			{
				idSWFScriptObject arguments = new idSWFScriptObject();

				// create "arguments"
				arguments->MakeArray();

				int elementCount = parms.Count;

				for(int i = 0; i < elementCount; i++)
				{
					arguments.Set(i, parms[i]);
				}

				locals.Set("arguments", new idSWFScriptVariable(arguments));
			}

			if((_flags & (1ul << 4)) != 0)
			{
				// load "super" into a register
				_registers[preloadReg].Set(scriptObj.Prototype);
				preloadReg++;
			}

			if((_flags & (1ul << 5)) != 0)
			{
				// create "super"
				locals.Set("super", new idSWFScriptVariable(scriptObj.Prototype));
			}

			if((_flags & (1ul << 6)) != 0)
			{
				// preload _root
				_registers[preloadReg] = _scope[0].Get("_root");
				preloadReg++;
			}

			if((_flags & (1ul << 7)) != 0)
			{
				// preload _parent
				if((scriptObj.Sprite != null) && (scriptObj.Sprite.Parent != null))
				{
					_registers[preloadReg].Set(scriptObj.Sprite.Parent.ScriptObject);
				}
				else
				{
					_registers[preloadReg].SetNull();
				}
				
				preloadReg++;
			}

			if((_flags & (1ul << 8)) != 0)
			{
				// load "_global" into a register
				_registers[preloadReg].Set(_scope[0]);
				preloadReg++;
			}

			int scopeSize = _scope.Count;
			_scope.Add(locals);

			idSWFScriptVariable retVal = Run(scriptObj, stack, _bitStream);

			Debug.Assert(_scope.Count == (scopeSize + 1));

			_scope.RemoveRange(scopeSize, _scope.Count - scopeSize);

			return retVal;
		}
		#endregion
	}
}