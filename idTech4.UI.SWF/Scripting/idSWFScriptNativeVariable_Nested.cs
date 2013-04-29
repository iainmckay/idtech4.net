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
using System.Diagnostics;

namespace idTech4.UI.SWF.Scripting
{
	public class idSWFScriptNativeVariable_Nested<T> : idSWFScriptNativeVariable
	{
		#region Members
		private NestedScriptGetVariable<T> _getCallback;
		private NestedScriptSetVariable<T> _setCallback;
		private T _context;
		#endregion

		#region Constructor
		public idSWFScriptNativeVariable_Nested(NestedScriptGetVariable<T> getCallback, NestedScriptSetVariable<T> setCallback, T context)
			: base()
		{
			_getCallback = getCallback;
			_setCallback = setCallback;
			_context     = context;
		}
		#endregion

		#region idSWFScriptNativeVariable implementation
		public override idSWFScriptVariable Get(idSWFScriptObject scriptObj)
		{
			if(_getCallback != null)
			{
				return _getCallback.Invoke(scriptObj, _context);
			}

			return null;
		}

		public override void Set(idSWFScriptObject scriptObj, idSWFScriptVariable value)
		{
			if(_setCallback != null)
			{
				_setCallback.Invoke(scriptObj, _context, value);
			}
		}
		#endregion
	}

	public class idSWFScriptNativeVariable_NestedReadonly<T> : idSWFScriptNativeVariable_Nested<T>
	{
		#region Constructor
		public idSWFScriptNativeVariable_NestedReadonly(NestedScriptGetVariable<T> getCallback, NestedScriptSetVariable<T> setCallback, T context) 
			: base(getCallback, setCallback, context)
		{
			
		}

		public idSWFScriptNativeVariable_NestedReadonly(NestedScriptGetVariable<T> getCallback, T context)
			: base(getCallback, null, context)
		{

		}
		#endregion

		#region idSWFScriptNativeVariable_Nested implementation
		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public override void Set(idSWFScriptObject scriptObj, idSWFScriptVariable value)
		{
			Debug.Assert(false);
		}
		#endregion
	}

	public delegate idSWFScriptVariable NestedScriptGetVariable<T>(idSWFScriptObject scriptObj, T context);
	public delegate void NestedScriptSetVariable<T>(idSWFScriptObject scriptObj, T context, idSWFScriptVariable value);
}