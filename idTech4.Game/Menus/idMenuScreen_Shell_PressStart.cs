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
using idTech4.Renderer;
using idTech4.Services;
using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public class idMenuScreen_Shell_PressStart : idMenuScreen
	{
		#region Members
		private idMenuWidget_Button _startButton;
		private idMenuWidget_DynamicList _options;
		private idMenuWidget_Carousel _itemList;
		private idMaterial _doomCover;
		private idMaterial _doom2Cover;
		private idMaterial _doom3Cover;
		#endregion

		#region Constructor
		public idMenuScreen_Shell_PressStart()
			: base()
		{

		}
		#endregion

		#region idMenuScreen implementation
		public override void Initialize(idMenuHandler data)
		{
			base.Initialize(data);

			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

			if(data != null)
			{
				this.UserInterface = data.UserInterface;
			}

			SetSpritePath("menuStart");
			
			_itemList = new idMenuWidget_Carousel();
			_itemList.SetSpritePath(this.SpritePath, "info", "options");
			_itemList.VisibleOptionCount = 3;

			for(int i = 0; i < 3; i++)
			{
				idMenuWidget_Button buttonWidget = new idMenuWidget_Button();
				buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.PressFocused, _itemList.Children.Length);
				buttonWidget.Initialize(data);

				_itemList.AddChild(buttonWidget);
			}

			_itemList.Initialize(data);

			AddChild(_itemList);

			AddEventAction(WidgetEventType.ScrollLeft).Set(new idWidgetActionHandler(this,                 WidgetActionType.ScrollLeftStartRepeater,  WidgetEventType.ScrollLeft));
			AddEventAction(WidgetEventType.ScrollRight).Set(new idWidgetActionHandler(this,                WidgetActionType.ScrollRightStartRepeater, WidgetEventType.ScrollRight));
			AddEventAction(WidgetEventType.ScrollLeftRelease).Set(new idWidgetActionHandler(this,          WidgetActionType.StopRepeater,             WidgetEventType.ScrollLeftRelease));
			AddEventAction(WidgetEventType.ScrollRightRelease).Set(new idWidgetActionHandler(this,         WidgetActionType.StopRepeater,             WidgetEventType.ScrollRightRelease));			

			AddEventAction(WidgetEventType.ScrollUp).Set(new idWidgetActionHandler(this,                   WidgetActionType.ScrollUpStartRepeater,    WidgetEventType.ScrollUp));
			AddEventAction(WidgetEventType.ScrollDown).Set(new idWidgetActionHandler(this,                 WidgetActionType.ScrollDownStartRepeater,  WidgetEventType.ScrollDown));
			AddEventAction(WidgetEventType.ScrollUpRelease).Set(new idWidgetActionHandler(this,            WidgetActionType.StopRepeater,             WidgetEventType.ScrollUpRelease));
			AddEventAction(WidgetEventType.ScrollDownRelease).Set(new idWidgetActionHandler(this,          WidgetActionType.StopRepeater,             WidgetEventType.ScrollDownRelease));			

			AddEventAction(WidgetEventType.ScrollLeftStickDown).Set(new idWidgetActionHandler(this,        WidgetActionType.ScrollDownStartRepeater,  WidgetEventType.ScrollLeftStickDown));
			AddEventAction(WidgetEventType.ScrollLeftStickUp).Set(new idWidgetActionHandler(this,          WidgetActionType.ScrollUpStartRepeater,    WidgetEventType.ScrollLeftStickUp));
			AddEventAction(WidgetEventType.ScrollLeftStickDownRelease).Set(new idWidgetActionHandler(this, WidgetActionType.StopRepeater,             WidgetEventType.ScrollLeftStickDownRelease));
			AddEventAction(WidgetEventType.ScrollLeftStickUpRelease).Set(new idWidgetActionHandler(this,   WidgetActionType.StopRepeater,             WidgetEventType.ScrollLeftStickUpRelease));
			
			_doomCover  = declManager.FindMaterial("guis/assets/mainmenu/doom_cover");
			_doom2Cover = declManager.FindMaterial("guis/assets/mainmenu/doom2_cover");
			_doom3Cover = declManager.FindMaterial("guis/assets/mainmenu/doom3_cover");

			_startButton = new idMenuWidget_Button();
			_startButton.SetSpritePath(this.SpritePath, "info", "btnStart");

			AddChild(_startButton);
		}

		public override void Update()
		{
 			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			if(cvarSystem.GetBool("g_demoMode") == false)
			{
				if(_menuData != null)
				{
					idMenuWidget_CommandBar cmdBar = _menuData.CommandBar;

					if(cmdBar != null)
					{
						cmdBar.ClearAllButtons();
						
						ButtonInfo buttonInfo = cmdBar.GetButton(Button.Joystick1);

						if(_menuData.GetPlatform() != 2)
						{
							buttonInfo.Label = "#str_SWF_SELECT";
						}

						buttonInfo.Action.Set(WidgetActionType.PressFocused);
					}
				}
			}

			base.Update();
		}

		public override void ShowScreen(MainMenuTransition transitionType)
		{
			ICVarSystem cvarSystem     = idEngine.Instance.GetService<ICVarSystem>();
			ILocalization localization = idEngine.Instance.GetService<ILocalization>();
			idSWFScriptObject root     = this.SWFObject.RootObject;

			if(BindSprite(root) == true)
			{
				if(cvarSystem.GetBool("g_demoMode") == true)
				{
					if(_itemList != null)
					{
						_itemList.Images = new idMaterial[] {};
					}

					if(_startButton != null)
					{
						_startButton.BindSprite(root);
						_startButton.Label = localization.Get("#str_swf_press_start");
					}
			
					idSWFSpriteInstance backing = this.Sprite.ScriptObject.GetNestedSprite("backing");
					
					if(backing != null)
					{
						backing.IsVisible = false;
					}
				}
				else
				{
					idMaterial[] coverIcons = { _doomCover, _doom3Cover, _doom2Cover };

					if(_itemList != null)
					{
						_itemList.Images = coverIcons;
						_itemList.SetFocusIndex(1, true);
						_itemList.ViewIndex         = 1;
						_itemList.MoveToIndexTarget = 1;
					}

					if(_startButton != null)
					{
						_startButton.BindSprite(root);
						_startButton.Label = "";
					}

					idSWFSpriteInstance backing = this.Sprite.ScriptObject.GetNestedSprite("backing");

					if(backing != null)
					{
						backing.IsVisible = true;
					}
				}
			}

			base.ShowScreen(transitionType);
		}

		public override void HideScreen(MainMenuTransition transitionType)
		{
			base.HideScreen(transitionType);
		}

		public override bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			if(_menuData == null)
			{
				return true;
			}

			if(_menuData.ActiveScreen != ShellArea.Start)
			{
				return false;
			}

			WidgetActionType actionType   = action.Type;
			

			switch(actionType)
			{
				case WidgetActionType.PressFocused:
					return OnPressFocused(action, ev, widget, forceHandled);
					
				case WidgetActionType.StartRepeater:
					idLog.Warning("TODO: start repeater");
					/*idWidgetAction repeatAction;
					widgetAction_t repeatActionType = static_cast< widgetAction_t >( parms[ 0 ].ToInteger() );
					assert( parms.Num() == 2 );
					repeatAction.Set( repeatActionType, parms[ 1 ] );
					menuData->StartWidgetActionRepeater( widget, repeatAction, event );*/
					return true;
		
				case WidgetActionType.StopRepeater:
					idLog.Warning("TODO: menuData->ClearWidgetActionRepeater();");
					return true;
				
				case WidgetActionType.ScrollHorizontal:
					return OnScrollHorizontal(action, ev, widget, forceHandled);
			}

			return base.HandleAction(action, ev, widget, forceHandled);
		}

		private bool OnPressFocused(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled)
		{
			if(_itemList == null)
			{
				return true;;
			}

			if(ev.Parameters.Count != 1)
			{
				return true;
			}

			if(_itemList.MoveToIndexTarget != _itemList.ViewIndex)
			{
				return true;
			}

			idSWFParameterList parameters = action.Parameters;

			if(parameters.Count > 0)
			{
				int index = parameters[0].ToInt32();

				if(index != 0)
				{
					_itemList.MoveToIndex(index);
					Update();
				}
			}
			
			if(_itemList.MoveToIndexTarget == 0)
			{
				idLog.Warning("TODO: common->SwitchToGame( DOOM_CLASSIC );");
			}
			else if(_itemList.MoveToIndexTarget == 1)
			{
				// TODO: signin manager
				/*if ( session->GetSignInManager().GetMasterLocalUser() == NULL ) {
					const int device = event.parms[ 0 ].ToInteger();
					session->GetSignInManager().RegisterLocalUser( device );
				} else {*/
					_menuData.SetNextScreen(ShellArea.Root, MainMenuTransition.Simple);
				/*}*/
			}
			else if(_itemList.MoveToIndexTarget == 2)
			{
				idLog.Warning("TODO: common->SwitchToGame( DOOM2_CLASSIC );");
			}

			return true;
		}

		private bool OnScrollHorizontal(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled)
		{
			if(_itemList == null)
			{
				return true;
			}

			if(_itemList.TotalNumberOfOptions <= 1)
			{
				return true;
			}

			if(_itemList.MoveDiff > 0)
			{
				_itemList.MoveToIndex(_itemList.MoveToIndexTarget, true);
			}

			idSWFParameterList parameters = action.Parameters;
			int direction                 = parameters[0].ToInt32();

			if(direction == 1) 
			{					
				if(_itemList.ViewIndex == (_itemList.TotalNumberOfOptions - 1)) 
				{
					return true;
				} 
				else
				{
					_itemList.MoveToIndex(1);
				}
			} 
			else 
			{
				if(_itemList.ViewIndex == 0) 
				{
					return true;
				} 
				else 
				{
					_itemList.MoveToIndex((_itemList.VisibleOptionCount / 2 ) + 1);
				}
			}

			return true;
		}
		#endregion
	}
}