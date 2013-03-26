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
	/// <summary>
	/// This is the prototype object that all the text instance script objects reference.
	/// </summary>
	public class idSWFScriptObject_TextInstancePrototype : idSWFScriptObject
	{
		#region Constructor
		public idSWFScriptObject_TextInstancePrototype()
			: base()
		{
			Set("onKey",		new idSWFScriptFunction_Text(ScriptFunction_onKey));
			Set("onChar",		new idSWFScriptFunction_Text(ScriptFunction_onChar));
			Set("generateRnd",	new idSWFScriptFunction_Text(ScriptFunction_generateRnd));
			Set("calcNumLines", new idSWFScriptFunction_Text(ScriptFunction_calcNumLines));

			idLog.Warning("TODO: TextInstancePrototype");

			/*SWF_NATIVE_VAR_DECLARE( text );
			SWF_NATIVE_VAR_DECLARE( autoSize );
			SWF_NATIVE_VAR_DECLARE( dropShadow );
			SWF_NATIVE_VAR_DECLARE( _stroke );
			SWF_NATIVE_VAR_DECLARE( _strokeStrength );
			SWF_NATIVE_VAR_DECLARE( _strokeWeight );
			SWF_NATIVE_VAR_DECLARE( variable );
			SWF_NATIVE_VAR_DECLARE( _alpha );
			SWF_NATIVE_VAR_DECLARE( textColor );
			SWF_NATIVE_VAR_DECLARE( _visible );
			SWF_NATIVE_VAR_DECLARE( scroll );
			SWF_NATIVE_VAR_DECLARE( maxscroll );
			SWF_NATIVE_VAR_DECLARE( selectionStart );
			SWF_NATIVE_VAR_DECLARE( selectionEnd );
			SWF_NATIVE_VAR_DECLARE( isTooltip );
			SWF_NATIVE_VAR_DECLARE( mode );
			SWF_NATIVE_VAR_DECLARE( delay );
			SWF_NATIVE_VAR_DECLARE( renderSound );
			SWF_NATIVE_VAR_DECLARE( updateScroll );
			SWF_NATIVE_VAR_DECLARE( subtitle );
			SWF_NATIVE_VAR_DECLARE( subtitleAlign );
			SWF_NATIVE_VAR_DECLARE( subtitleSourceID );
			SWF_NATIVE_VAR_DECLARE( subtitleSpeaker );
	
			SWF_NATIVE_VAR_DECLARE_READONLY( _textLength );*/

			Set("subtitleSourceCheck",	new idSWFScriptFunction_Text(ScriptFunction_subtitleSourceCheck));
			Set("subtitleStart",		new idSWFScriptFunction_Text(ScriptFunction_subtitleStart));
			Set("subtitleLength",		new idSWFScriptFunction_Text(ScriptFunction_subtitleLength));
			Set("killSubtitle",			new idSWFScriptFunction_Text(ScriptFunction_killSubtitle));
			Set("forceKillSubtitle",	new idSWFScriptFunction_Text(ScriptFunction_forceKillSubtitle));
			Set("subLastLine",			new idSWFScriptFunction_Text(ScriptFunction_subLastLine));
			Set("addSubtitleInfo",		new idSWFScriptFunction_Text(ScriptFunction_addSubtitleInfo));
			Set("terminateSubtitle",	new idSWFScriptFunction_Text(ScriptFunction_terminateSubtitle));
			Set("clearTimingInfo",		new idSWFScriptFunction_Text(ScriptFunction_clearTimingInfo));			
		}
		#endregion

		#region Script Functions
		private idSWFScriptVariable ScriptFunction_onKey(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_onChar(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_generateRnd(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_calcNumLines(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_subtitleSourceCheck(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_subtitleStart(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_subtitleLength(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_killSubtitle(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_forceKillSubtitle(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_subLastLine(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_addSubtitleInfo(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_terminateSubtitle(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_clearTimingInfo(idSWFScriptObject scriptObj, idSWFTextInstance textInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}