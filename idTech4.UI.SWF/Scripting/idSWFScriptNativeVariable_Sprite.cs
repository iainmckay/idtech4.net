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
	public class idSWFScriptNativeVariable_Sprite : idSWFScriptNativeVariable
	{
		#region Members
		private NestedScriptSpriteGetVariable _getCallback;
		private NestedScriptSpriteSetVariable _setCallback;
		#endregion

		#region Constructor
		public idSWFScriptNativeVariable_Sprite(NestedScriptSpriteGetVariable getCallback, NestedScriptSpriteSetVariable setCallback)
			: base()
		{
			_getCallback = getCallback;
			_setCallback = setCallback;
		}

		public idSWFScriptNativeVariable_Sprite(NestedScriptSpriteGetVariable getCallback)
			: base()
		{
			_getCallback = getCallback;
		}
		#endregion

		#region idSWFScriptNativeVariable implementation
		public override idSWFScriptVariable Get(idSWFScriptObject scriptObj)
		{
			if(_getCallback != null)
			{
				idSWFSpriteInstance spriteInstance = (scriptObj != null) ? scriptObj.Sprite : null;

				if(spriteInstance != null)
				{
					return _getCallback.Invoke(scriptObj, spriteInstance);
				}
			}

			return new idSWFScriptVariable();
		}

		public override void Set(idSWFScriptObject scriptObj, idSWFScriptVariable value)
		{
			if(_setCallback != null)
			{
				idSWFSpriteInstance spriteInstance = (scriptObj != null) ? scriptObj.Sprite : null;

				if(spriteInstance != null)
				{
					_setCallback.Invoke(scriptObj, spriteInstance, value);
				}
			}
		}
		#endregion
	}

	public delegate idSWFScriptVariable NestedScriptSpriteGetVariable(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance);
	public delegate void NestedScriptSpriteSetVariable(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value);
}