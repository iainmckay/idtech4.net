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
	/// <summary>
	/// Provides a paged view of this widgets children.  
	/// </summary>
	/// <remarks>
	/// Each child is expected to take on the following naming scheme.  Children 
	/// outside of the given window size (NumVisibleOptions) are not rendered,
	/// and will affect which type of arrow indicators are shown.
	/// <para/>
	/// This transparently supports the "UseCircleForAccept" behavior that we need for Japanese PS3 SKU.
	/// <para/>
	/// SWF object structure
	/// --------------------
	/// COMMANDBAR
	/// 	joy#
	/// 		img (Frames: platform)
	/// 		txt_info (Text)
	/// </remarks>
	public class idMenuWidget_CommandBar : idMenuWidget
	{
		#region Constants
		private const int MaxButtons = 6;
		private static readonly string[] ButtonNames = {
			"joy1",
			"joy2",
			"joy3",
			"joy4",
			"joy10",
			"tab"
		};
		#endregion

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
		private ButtonInfo[] _buttons;
		private Alignment _alignment;
		#endregion

		#region Constructor
		public idMenuWidget_CommandBar() 
			: base()
		{
			_alignment = Alignment.Left;
			_buttons   = new ButtonInfo[MaxButtons];

			for(int index = 0; index < MaxButtons; ++index)
			{
				_buttons[index] = new ButtonInfo();
			}
		}
		#endregion

		#region Methods
		public void ClearAllButtons()
		{
			for(int index = 0; index < MaxButtons; ++index)
			{
				_buttons[index].Label = string.Empty;
				_buttons[index].Action.Set(WidgetActionType.None);
			}
		}

		public ButtonInfo GetButton(Button button)
		{
			return _buttons[(int) button];
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
			
			int basePadding      = 35;
			int perButtonPadding = 65;
			int alignmentScale   = (this.Alignment == Menus.Alignment.Left) ? 1 : -1;
			int xPosition        = alignmentScale * basePadding;
			
			// setup the button order.
			Button[] buttonOrder = new Button[MaxButtons];

			for(int i = 0; i < buttonOrder.Length; ++i)
			{
				buttonOrder[i] = (Button) i;
			}

			// NOTE: Special consideration is done for JPN PS3 where the standard accept button is
			// swapped with the standard back button.  i.e. In US: X = Accept, O = Back, but in JPN
			// X = Back, O = Accept.
			if(this.SWFObject.UseCircleForAccept == true)
			{
				buttonOrder[(int) Button.Joystick2] = Button.Joystick1;
				buttonOrder[(int) Button.Joystick1] = Button.Joystick2;
			}

			// FIXME: handle animating in of the button bar?
			this.Sprite.IsVisible = true;

			string shortcutName;

			for(int i = 0; i < buttonOrder.Length; ++i)
			{
				string buttonName = ButtonNames[(int) buttonOrder[i]];

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


				if(_buttons[i].Action.Type != WidgetActionType.None)
				{
					idSWFScriptObject shortcutKeys = this.SWFObject.GetGlobal("shortcutKeys").Object;

					if(shortcutKeys != null)
					{
						buttonSprite.ScriptObject.Set("onPress", new idWrapWidgetEvent(this, WidgetEventType.Command, i));

						// bind the main action - need to use all caps here because shortcuts are stored that way
						shortcutName = buttonName.ToUpper();

						shortcutKeys.Set(shortcutName, buttonSprite.ScriptObject);

						// Some other keys have additional bindings. Remember that the button here is
						// actually the virtual button, and the physical button could be swapped based
						// on the UseCircleForAccept business on JPN PS3.
						switch((Button) i) 
						{
							case Button.Joystick1:
								shortcutKeys.Set("ENTER", buttonSprite.ScriptObject);
								break;

							case Button.Joystick2:
								shortcutKeys.Set("ESCAPE", buttonSprite.ScriptObject);
								shortcutKeys.Set("BACKSPACE", buttonSprite.ScriptObject);
								break;
							
							case Button.Tab:
								shortcutKeys.Set("K_TAB", buttonSprite.ScriptObject);
								break;
						}
					}

					if(_buttons[i].Label == string.Empty)
					{
						buttonSprite.IsVisible = false;
					}
					else
					{
						imageSprite.IsVisible = true;
						imageSprite.StopFrame(_menuData.GetPlatform() + 1);

						buttonSprite.IsVisible = true;
						buttonSprite.PositionX = xPosition;
						buttonText.Text        = _buttons[i].Label;

						xPosition += (int) (alignmentScale * (buttonText.TextLength + perButtonPadding));
					}
				}
				else
				{
					buttonSprite.IsVisible = false;

					idSWFScriptObject shortcutKeys = this.SWFObject.GetGlobal("shortcutKeys").Object;

					if(shortcutKeys != null)
					{
						buttonSprite.ScriptObject.SetNull("onPress");

						 // bind the main action - need to use all caps here because shortcuts are stored that way
						shortcutName = buttonName.ToUpper();

						shortcutKeys.Set(shortcutName, buttonSprite.ScriptObject);
					}
				}
			}
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

	public class ButtonInfo
	{
		public string Label;			// empty labels are treated as hidden buttons
		public idWidgetAction Action;

		public ButtonInfo()
		{
			this.Action = new idWidgetAction();
			this.Action.Set(WidgetActionType.None);
		}
	}
}