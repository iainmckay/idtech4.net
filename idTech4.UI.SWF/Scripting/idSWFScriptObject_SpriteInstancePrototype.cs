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

namespace idTech4.UI.SWF.Scripting
{
	public class idSWFScriptObject_SpriteInstancePrototype : idSWFScriptObject
	{
		#region Constructor
		public idSWFScriptObject_SpriteInstancePrototype()
			: base()
		{
			Set("duplicateMovieClip",	new idSWFScriptFunction_Sprite(ScriptFunction_duplicateMovieClip));
			Set("gotoAndPlay",			new idSWFScriptFunction_Sprite(ScriptFunction_gotoAndPlay));
			Set("gotoAndStop",			new idSWFScriptFunction_Sprite(ScriptFunction_gotoAndStop));
			Set("swapDepths",			new idSWFScriptFunction_Sprite(ScriptFunction_swapDepths));
			Set("nextFrame",			new idSWFScriptFunction_Sprite(ScriptFunction_nextFrame));
			Set("prevFrame",			new idSWFScriptFunction_Sprite(ScriptFunction_prevFrame));
			Set("play",					new idSWFScriptFunction_Sprite(ScriptFunction_play));
			Set("stop",					new idSWFScriptFunction_Sprite(ScriptFunction_stop));

			idLog.Warning("TODO: SpriteInstancePrototype");
			/*SWF_SPRITE_NATIVE_VAR_SET(_x);
			SWF_SPRITE_NATIVE_VAR_SET(_y);
			SWF_SPRITE_NATIVE_VAR_SET(_xscale);
			SWF_SPRITE_NATIVE_VAR_SET(_yscale);
			SWF_SPRITE_NATIVE_VAR_SET(_alpha);
			SWF_SPRITE_NATIVE_VAR_SET(_brightness);
			SWF_SPRITE_NATIVE_VAR_SET(_visible);
			SWF_SPRITE_NATIVE_VAR_SET(_width);
			SWF_SPRITE_NATIVE_VAR_SET(_height);
			SWF_SPRITE_NATIVE_VAR_SET(_rotation);
			SWF_SPRITE_NATIVE_VAR_SET(_name);
			SWF_SPRITE_NATIVE_VAR_SET(_currentframe);
			SWF_SPRITE_NATIVE_VAR_SET(_totalframes);
			SWF_SPRITE_NATIVE_VAR_SET(_target);
			SWF_SPRITE_NATIVE_VAR_SET(_framesloaded);
			SWF_SPRITE_NATIVE_VAR_SET(_droptarget);
			SWF_SPRITE_NATIVE_VAR_SET(_url);
			SWF_SPRITE_NATIVE_VAR_SET(_highquality);
			SWF_SPRITE_NATIVE_VAR_SET(_focusrect);
			SWF_SPRITE_NATIVE_VAR_SET(_soundbuftime);
			SWF_SPRITE_NATIVE_VAR_SET(_quality);
			SWF_SPRITE_NATIVE_VAR_SET(_mousex);
			SWF_SPRITE_NATIVE_VAR_SET(_mousey);

			SWF_SPRITE_NATIVE_VAR_SET(_stereoDepth);
			SWF_SPRITE_NATIVE_VAR_SET(_itemindex);
			SWF_SPRITE_NATIVE_VAR_SET(material);
			SWF_SPRITE_NATIVE_VAR_SET(materialWidth);
			SWF_SPRITE_NATIVE_VAR_SET(materialHeight);
			SWF_SPRITE_NATIVE_VAR_SET(xOffset);

			SWF_SPRITE_NATIVE_VAR_SET(onEnterFrame);*/
			//SWF_SPRITE_NATIVE_VAR_SET( onLoad );
		}
		#endregion

		#region Script Functions
		private idSWFScriptVariable ScriptFunction_duplicateMovieClip(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_gotoAndPlay(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_gotoAndStop(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_swapDepths(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_nextFrame(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_prevFrame(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_play(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_stop(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}