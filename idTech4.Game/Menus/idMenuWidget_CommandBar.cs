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
	public class idMenuWidget_CommandBar : idMenuWidget
	{
		#region Properties
		public Alignment Alignment
		{
			get
			{
				return _alignment;
			}
			set
			{
				_alignment = value;
			}
		}
		#endregion

		#region Members
		// TODO: idStaticList< buttonInfo_t, MAX_BUTTONS >	buttons;
		private Alignment _alignment;
		#endregion

		#region Constructor
		public idMenuWidget_CommandBar() : base()
		{
			_alignment = Alignment.Left;
			// TODO: buttons.SetNum( MAX_BUTTONS );
		}
		#endregion

		#region idMenuWidget implementation
		public override void Update()
		{
			if(this.SWFObject == null)
			{
				return;
			}

			idSWFScriptObject root = this.SWFObject.RootObject;
			
			if(BindSprite(root) == false)
			{
				return;
			}

			idLog.Warning("TODO: command bar update");

			/*int basePadding      = 35;
			int perButtonPadding = 65;
			int alignmentScale   = (this.Alignment == Menus.Alignment.Left) ? 1 : -1;
			int xPosition        = alignmentScale * basePadding;
			
			// setup the button order.
			idStaticList< button_t, MAX_BUTTONS > buttonOrder;
			for ( int i = 0; i < buttonOrder.Max(); ++i ) {
				buttonOrder.Append( static_cast< button_t >( i ) );
			}

			// NOTE: Special consideration is done for JPN PS3 where the standard accept button is
			// swapped with the standard back button.  i.e. In US: X = Accept, O = Back, but in JPN
			// X = Back, O = Accept.
			// TODO
			/*if ( GetSWFObject()->UseCircleForAccept() ) {
				buttonOrder[ BUTTON_JOY2 ] = BUTTON_JOY1;
				buttonOrder[ BUTTON_JOY1 ] = BUTTON_JOY2;
			}*/

			// FIXME: handle animating in of the button bar?
			/*this.Sprite.IsVisible = true;

			string shortcutName;

			for(int i = 0; i < buttonOrder.Count; ++i)
			{
				string buttonName              = BUTTON_NAMES[buttonOrder[i]];

				idSWFSpriteInstance buttonSprite = this.Sprite.ScriptObject.GetSprite(buttonName);

				if(buttonSprite == null)
				{
					continue;
				}

				idSWFTextInstance buttonText = buttonSprite.ScriptObject.GetText("txt_info");

				if(buttonText == null)
				{
					continue;
				}

				idSWFSpriteInstance imageSprite = buttonSprite.ScriptObject.GetSprite("img");

				if(imageSprite == null)
				{
					continue;
				}


				if(buttons[i].Action.Type != WidgetActionType.None)
				{
					idLog.Warning("TODO: button action");
			
					/*idSWFScriptObject * const shortcutKeys = GetSWFObject()->GetGlobal( "shortcutKeys" ).GetObject();
					if ( verify( shortcutKeys != NULL ) ) {
						buttonSprite->GetScriptObject()->Set( "onPress", new WrapWidgetSWFEvent( this, WIDGET_EVENT_COMMAND, i ) );

						// bind the main action - need to use all caps here because shortcuts are stored that way
						shortcutName = buttonName;
						shortcutName.ToUpper();
						shortcutKeys->Set( shortcutName, buttonSprite->GetScriptObject()  );

						// Some other keys have additional bindings. Remember that the button here is
						// actually the virtual button, and the physical button could be swapped based
						// on the UseCircleForAccept business on JPN PS3.
						switch ( i ) {
							case BUTTON_JOY1: {
								shortcutKeys->Set( "ENTER", buttonSprite->GetScriptObject() );
								break;
							}
							case BUTTON_JOY2: {
								shortcutKeys->Set( "ESCAPE", buttonSprite->GetScriptObject() );
								shortcutKeys->Set( "BACKSPACE", buttonSprite->GetScriptObject() );
								break;
							}
							case BUTTON_TAB: {
								shortcutKeys->Set( "K_TAB", buttonSprite->GetScriptObject() );
								break;
							}
						}
					}

					if ( buttons[ i ].label.IsEmpty() ) {
						buttonSprite->SetVisible( false );
					} else {
						imageSprite->SetVisible( true );
						imageSprite->StopFrame( menuData->GetPlatform() + 1 );
						buttonSprite->SetVisible( true );
						buttonSprite->SetXPos( xPos );
						buttonText->SetText( buttons[ i ].label );
						xPos += ALIGNMENT_SCALE * ( buttonText->GetTextLength() + PER_BUTTON_PADDING );
					}	*//*		
				}
				else
				{
					buttonSprite.IsVisible = false;

					idLog.Warning("TODO: idSWFScriptObject shortcutKeys = this.SWFObject.GetGlobal(\"shortcutKeys\").Object;");

					/*idSWFScriptObject * const shortcutKeys = GetSWFObject()->GetGlobal( "shortcutKeys" ).GetObject();
					if ( verify( shortcutKeys != NULL ) ) {
						buttonSprite->GetScriptObject()->Set( "onPress", NULL );
						 // bind the main action - need to use all caps here because shortcuts are stored that way
						shortcutName = buttonName;
						shortcutName.ToUpper();
						shortcutKeys->Set( shortcutName, buttonSprite->GetScriptObject()  );
					}*//*
				}
			}*/
		}

		public override bool ExecuteEvent(idWidgetEvent ev)
		{
			if(ev.Type == WidgetEventType.Command)
			{
				idLog.Warning("TODO: command bar execute command");

				/*if ( verify( event.arg >= 0 && event.arg < buttons.Num() ) ) {
					HandleAction( buttons[ event.arg ].action, event, this );
				}*/

				return true;
			} 
			else 
			{
				return base.ExecuteEvent(ev);
			}
		}
		#endregion
	}

	public enum Alignment
	{
		Left,
		Right
	}

	public enum Button
	{
		Joystick1,
		Joystick2,
		Joystick3,
		Joystick4,
		Joystick10,
		Tab
	}
}