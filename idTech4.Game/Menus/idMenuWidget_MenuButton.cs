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
	public class idMenuWidget_MenuButton : idMenuWidget_Button
	{
		#region Properties
		public float Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
			}
		}
		#endregion

		#region Members
		private float _position;
		#endregion

		#region Constructor
		public idMenuWidget_MenuButton() 
			: base()
		{

		}
		#endregion

		#region idMenuWidget_Button implementation
		public override void Update()
		{
			if(this.Sprite == null)
			{
				return;
			}

			if(string.IsNullOrEmpty(this.Label) == true)
			{
				this.Sprite.IsVisible = false;
			}
			else
			{
				this.Sprite.IsVisible = true;

				idSWFScriptObject spriteObject = this.Sprite.ScriptObject;
				idSWFTextInstance text         = spriteObject.GetNestedText("txtVal");

				if(text != null)
				{
					text.Text = this.Label;
					text.SetStrokeInfo(true, 0.7f, 1.25f);

					idSWFSpriteInstance selectionBar = spriteObject.GetNestedSprite("sel", "bar");
					idSWFSpriteInstance hoverBar     = spriteObject.GetNestedSprite("hover", "bar");

					if(selectionBar != null)
					{
						selectionBar.PositionX = text.TextLength / 2.0f;
						selectionBar.SetScale(100.0f * (text.TextLength / 300.0f), 100.0f);
					}

					if(hoverBar != null)
					{
						hoverBar.PositionX = text.TextLength / 2.0f;
						hoverBar.SetScale(100.0f * (text.TextLength / 352.0f), 100.0f);
					}

					idSWFSpriteInstance hitBox = spriteObject.GetNestedSprite("hitBox");

					if(hitBox != null)
					{
						hitBox.SetScale(100.0f * (text.TextLength / 235), 100.0f);
					}
				}

				this.Sprite.PositionX = _position;

				idSWFScriptObject textObj = spriteObject.GetNestedObject("txtVal");

				if(textObj != null)
				{
					idLog.Warning("TODO: menu button events");

					/*textObj->Set( "onPress", new ( TAG_SWF ) WrapWidgetSWFEvent( this, WIDGET_EVENT_PRESS, 0 ) );
					textObj->Set( "onRelease", new ( TAG_SWF ) WrapWidgetSWFEvent( this, WIDGET_EVENT_RELEASE, 0 ) );

					idSWFScriptObject * hitBox = spriteObject->GetObject( "hitBox" );
					if ( hitBox == NULL ) {
						hitBox = textObj;
					}

					hitBox->Set( "onRollOver", new ( TAG_SWF ) WrapWidgetSWFEvent( this, WIDGET_EVENT_ROLL_OVER, 0 ) );
					hitBox->Set( "onRollOut", new ( TAG_SWF ) WrapWidgetSWFEvent( this, WIDGET_EVENT_ROLL_OUT, 0 ) );*/
				}
			}
		}
		#endregion
	}
}