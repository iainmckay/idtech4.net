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
using System.Collections.Generic;

using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public abstract class idMenuScreen : idMenuWidget
	{
		#region Properties
		public idSWF UserInterface
		{
			get
			{
				return _menuGui;
			}
			protected set
			{
				_menuGui = value;
			}
		}

		public MainMenuTransition Transition
		{
			get
			{
				return _transition;
			}
			protected set
			{
				_transition = value;
			}
		}
		#endregion

		#region Members
		private idSWF _menuGui;
		private MainMenuTransition _transition;
		#endregion

		#region Constructor
		public idMenuScreen()
			: base()
		{
			_transition = MainMenuTransition.Invalid;
		}
		#endregion

		#region Methods
		public virtual void HideScreen(MainMenuTransition transitionType)
		{
			if(_menuGui == null)
			{
				return;
			}

			if(BindSprite(_menuGui.RootObject) == false)
			{
				return;
			}

			if(transitionType == MainMenuTransition.Simple)
			{
				this.Sprite.PlayFrame("rollOff");
			}
			else if(transitionType == MainMenuTransition.Advance)
			{
				this.Sprite.PlayFrame("rollOffBack");
			}
			else
			{
				this.Sprite.PlayFrame("rollOffFront");
			}

			Update();
		}

		public virtual void ShowScreen(MainMenuTransition transitionType)
		{
			if(_menuGui == null)
			{
				return;
			}

			if(BindSprite(_menuGui.RootObject) == false)
			{
				return;
			}

			this.Sprite.IsVisible = true;

			if(transitionType == MainMenuTransition.Simple)
			{
				if((_menuData != null) && (_menuData.ActiveScreen != ShellArea.Invalid))
				{
					idLog.Warning("TODO: menuData->PlaySound( GUI_SOUND_BUILD_ON );");
				}

				this.Sprite.PlayFrame("rollOn");
			}
			else if(transitionType == MainMenuTransition.Advance)
			{
				if((_menuData != null) && (_menuData.ActiveScreen != ShellArea.Invalid))
				{
					idLog.Warning("TODO: menuData->PlaySound( GUI_SOUND_BUILD_ON );");
				}

				this.Sprite.PlayFrame("rollOnFront");
			} 
			else 
			{
				if(_menuData != null)
				{
					idLog.Warning("TODO: menuData->PlaySound( GUI_SOUND_BUILD_OFF );");
				}

				this.Sprite.PlayFrame("rollOnBack");
			}

			Update();
			SetFocusIndex(this.FocusIndex, true);
		}

		public void UpdateCommands()
		{
			idSWF gui = _menuGui;

			idSWFScriptObject shortcutKeys = gui.GetGlobal("shortcutKeys").Object;

			if(shortcutKeys != null)
			{
				return;
			}

			idSWFScriptVariable clearFunction = shortcutKeys.Get("clear");

			if(clearFunction.IsFunction == true)
			{
				clearFunction.Function.Invoke(null, new idSWFParameterList());
			}

			// NAVIGATION: UP/DOWN, etc.
			idSWFScriptObject buttons = gui.RootObject.GetObject("buttons");

			if(buttons != null)
			{
				idSWFScriptObject btnUp = buttons.GetObject("btnUp");

				if(btnUp != null)
				{
					btnUp.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollUp, (int) ScrollType.Single));
					btnUp.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollUpRelease, 0));

					shortcutKeys.Set("UP",        btnUp);
					shortcutKeys.Set("MWHEEL_UP", btnUp);
				}

				idSWFScriptObject btnDown = buttons.GetObject("btnDown");

				if(btnDown != null)
				{
					btnDown.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollDown, (int) ScrollType.Single));
					btnDown.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollDownRelease, 0));

					shortcutKeys.Set("DOWN",        btnDown);
					shortcutKeys.Set("MWHEEL_DOWN", btnDown);
				}

				idSWFScriptObject btnUp_LStick = buttons.GetObject("btnUp_LStick");

				if(btnUp_LStick != null)
				{
					btnUp_LStick.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftStickUp, (int) ScrollType.Single));
					btnUp_LStick.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftStickUpRelease, 0));

					shortcutKeys.Set("STICK1_UP", btnUp_LStick);
				}

				idSWFScriptObject btnDown_LStick = buttons.GetObject("btnDown_LStick");

				if(btnDown_LStick != null)
				{
					btnDown_LStick.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftStickDown, (int) ScrollType.Single));
					btnDown_LStick.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftStickDownRelease, 0));

					shortcutKeys.Set("STICK1_DOWN", btnDown_LStick);
				}

				idSWFScriptObject btnUp_RStick = buttons.GetObject("btnUp_RStick");

				if(btnUp_RStick != null)
				{
					btnUp_RStick.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollRightStickUp, (int) ScrollType.Single));
					btnUp_RStick.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollRightStickUpRelease, 0));

					shortcutKeys.Set("STICK2_UP", btnUp_RStick);
				}

				idSWFScriptObject btnDown_RStick = buttons.GetObject("btnDown_RStick");

				if(btnDown_RStick != null)
				{
					btnDown_RStick.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollRightStickDown, (int) ScrollType.Page));
					btnDown_RStick.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollRightStickDownRelease, 0));

					shortcutKeys.Set("STICK2_DOWN", btnDown_RStick);
				}
				
				idSWFScriptObject btnPageUp = buttons.GetObject("btnPageUp");

				if(btnPageUp != null)
				{
					btnPageUp.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollPageUp, (int) ScrollType.Page));
					btnPageUp.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollPageUpRelease, 0));

					shortcutKeys.Set("PGUP", btnPageUp);
				}
				
				idSWFScriptObject btnPageDown = buttons.GetObject("btnPageDown");

				if(btnPageDown != null)
				{
					btnPageDown.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollPageDown, (int) ScrollType.Page));
					btnPageDown.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollPageDownRelease, 0));

					shortcutKeys.Set("PGDN", btnPageDown);
				}
				
				idSWFScriptObject btnHome = buttons.GetObject("btnHome");

				if(btnHome != null)
				{
					btnHome.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollUp, (int) ScrollType.Full));
					btnHome.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollUpRelease, 0));

					shortcutKeys.Set("HOME", btnHome);
				}
				
				idSWFScriptObject btnEnd = buttons.GetObject("btnEnd");

				if(btnEnd != null)
				{
					btnEnd.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollDown, (int) ScrollType.Full));
					btnEnd.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollDownRelease, 0));

					shortcutKeys.Set("END", btnEnd);
				}

				idSWFScriptObject btnLeft = buttons.GetObject("btnLeft");

				if(btnLeft != null)
				{
					btnLeft.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollLeft, 0));
					btnLeft.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftRelease, 0));

					shortcutKeys.Set("LEFT", btnLeft);
				}
				
				idSWFScriptObject btnRight = buttons.GetObject("btnRight");

				if(btnRight != null)
				{
					btnRight.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollRight, 0));
					btnRight.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollRightRelease, 0));

					shortcutKeys.Set("RIGHT", btnRight);
				}

				idSWFScriptObject btnLeft_LStick = buttons.GetObject("btnLeft_LStick");

				if(btnLeft_LStick != null)
				{
					btnLeft_LStick.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftStickLeft, 0));
					btnLeft_LStick.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftStickLeftRelease, 0));

					shortcutKeys.Set("STICK1_LEFT", btnLeft_LStick);
				}
				
				idSWFScriptObject btnRight_LStick = buttons.GetObject("btnRight_LStick");

				if(btnRight_LStick != null)
				{
					btnRight_LStick.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftStickRight, 0));
					btnRight_LStick.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftStickRightRelease, 0));

					shortcutKeys.Set("STICK1_RIGHT", btnRight_LStick);
				}

				idSWFScriptObject btnLeft_RStick = buttons.GetObject("btnLeft_RStick");

				if(btnLeft_RStick != null)
				{
					btnLeft_RStick.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollRightStickLeft, 0));
					btnLeft_RStick.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollRightStickLeftRelease, 0));

					shortcutKeys.Set("STICK2_LEFT", btnLeft_RStick);
				}

				idSWFScriptObject btnRight_RStick = buttons.GetObject("btnRight_RStick");

				if(btnRight_RStick != null)
				{
					btnRight_RStick.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.ScrollRightStickRight, 0));
					btnRight_RStick.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.ScrollRightStickRightRelease, 0));

					shortcutKeys.Set("STICK2_RIGHT", btnRight_RStick);
				}
			}

			idSWFScriptObject nav = gui.RootObject.GetObject("navBar");

			if(nav != null)
			{
				// TAB NEXT
				idSWFScriptObject btnTabNext = nav.GetNestedObject("options", "btnTabNext");
				
				if(btnTabNext != null) 
				{
					btnTabNext.Set("onPress", new idWrapWidgetEvent(this, WidgetEventType.TabNext, 0));
					shortcutKeys.Set("JOY6", btnTabNext);

					if((btnTabNext.Sprite != null) && (_menuData != null))
					{
						btnTabNext.Sprite.StopFrame(_menuData.GetPlatform() + 1);
					}
				}

				// TAB PREV
				idSWFScriptObject btnTabPrev = nav.GetNestedObject("options", "btnTabPrev");
				
				if(btnTabPrev != null) 
				{
					btnTabPrev.Set("onPress", new idWrapWidgetEvent(this, WidgetEventType.TabPrevious, 0));
					shortcutKeys.Set("JOY5", btnTabPrev);

					if((btnTabPrev.Sprite != null) && (_menuData != null))
					{
						btnTabPrev.Sprite.StopFrame(_menuData.GetPlatform() + 1);
					}
				}
			}
		}
		#endregion

		#region idMenuWidget implementation
		#region Frame
		public override void Update()
		{
			if(_menuGui == null)
			{
				return;
			}

			//
			// Display
			//
			for(int childIndex = 0; childIndex < this.Children.Length; ++childIndex)
			{
				this.Children[childIndex].Update();
			}

			if(_menuData != null)
			{
				_menuData.UpdateChildren();
			}
		}
		#endregion
		#endregion
	}

	public enum MainMenuTransition
	{
		Invalid = -1,
		Simple,
		Advance,
		Back,
		Force
	}
}