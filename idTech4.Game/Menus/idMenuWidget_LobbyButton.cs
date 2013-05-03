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
using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public class idMenuWidget_LobbyButton : idMenuWidget_Button 
	{
		#region Properties
		public bool IsValid
		{
			get
			{
				return (string.IsNullOrEmpty(_name) == false);
			}
		}
		#endregion

		#region Members
		private string _name;
		private VoiceStateDisplay _voiceState;
		#endregion

		#region Constructor
		public idMenuWidget_LobbyButton()
			: base()
		{
			_voiceState = VoiceStateDisplay.None;
		}
		#endregion

		#region idMenuWidget_Button implementation
		public override void Update()
		{
			if(this.Sprite == null)
			{
				return;
			}
			
			idSWFScriptObject spriteObject = this.Sprite.ScriptObject;
			idSWFTextInstance txtName      = spriteObject.GetNestedText("itemName", "txtVal");
			idSWFSpriteInstance talkIcon   = spriteObject.GetNestedSprite("chaticon");

			if(txtName != null)
			{
				txtName.Text = _name;
			}
			
			if(talkIcon != null)
			{
				talkIcon.StopFrame((int) _voiceState + 1);
				talkIcon.ScriptObject.Set("onPress", new idWrapWidgetEvent(this, WidgetEventType.Command, (int) WidgetActionType.MutePlayer));
			}

			// events
			spriteObject.Set("onPress", new idWrapWidgetEvent(this, WidgetEventType.Press, 0));
			spriteObject.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.Release, 0));

			idSWFScriptObject hitBox = spriteObject.GetObject("hitBox");
	
			if(hitBox == null)
			{
				hitBox = spriteObject;
			}

			hitBox.Set("onRollOver", new idWrapWidgetEvent(this, WidgetEventType.RollOver, 0));
			hitBox.Set("onRollOut", new idWrapWidgetEvent(this, WidgetEventType.RollOut, 0));
		}
		#endregion
	}
}