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
	public class idMenuScreen_Shell_GameLobby : idMenuScreen
	{
		#region Constants
		private const int LobbyOptionCount = 8;
		#endregion

		#region Members
		private int _longCountdown;
		private int	_longCountRemaining;
		private int	_shortCountdown;

		private bool _isHost;
		private bool _isPeer;
		private bool _privateGameLobby;

		private idMenuWidget_DynamicList _options;
		private idMenuWidget_LobbyList _lobby;
		private idMenuWidget_Button	_btnBack;

		private List<List<string>> _menuOptions = new List<List<string>>();
		#endregion

		#region Constructor
		public idMenuScreen_Shell_GameLobby() 
			: base()
		{
			_privateGameLobby = true;
		}
		#endregion

		#region idMenuScreen implementation
		public override void Initialize(idMenuHandler data)
		{
			base.Initialize(data);

			if(data != null)
			{
				this.UserInterface = data.UserInterface;
			}

			this.SetSpritePath("menuGameLobby");

			_options = new idMenuWidget_DynamicList();
			_options.VisibleOptionCount = LobbyOptionCount;
			_options.SetSpritePath(this.SpritePath, "info", "options");
			_options.IsWrappingAllowed = true;

			AddChild(_options);

			idMenuWidget_Help helpWidget = new idMenuWidget_Help();
			helpWidget.SetSpritePath(this.SpritePath, "info", "helpTooltip");
	
			AddChild(helpWidget);

			while(_options.Children.Length < LobbyOptionCount)
			{
				idMenuWidget_Button buttonWidget = new idMenuWidget_Button();
				buttonWidget.Initialize(data);
				buttonWidget.RegisterEventObserver(helpWidget);
		
				_options.AddChild(buttonWidget);
			}

			_options.Initialize(data);

			_lobby = new idMenuWidget_LobbyList();
			_lobby.VisibleOptionCount = 8;
			_lobby.SetSpritePath(this.SpritePath, "options");
			_lobby.IsWrappingAllowed = true;
			_lobby.Initialize(data);
	
			while(_lobby.Children.Length < 8)
			{
				idMenuWidget_LobbyButton buttonWidget = new idMenuWidget_LobbyButton();
				buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.SelectGamerTag, _lobby.Children.Length);
				buttonWidget.AddEventAction(WidgetEventType.Command).Set(WidgetActionType.MutePlayer, _lobby.Children.Length);
				buttonWidget.Initialize(data);
				
				_lobby.AddChild(buttonWidget);
			}
	
			AddChild(_lobby);

			_btnBack = new idMenuWidget_Button();
			_btnBack.Initialize( data );
			_btnBack.Label = "#str_swf_multiplayer";
			_btnBack.SetSpritePath(this.SpritePath, "info", "btnBack");
			_btnBack.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.GoBack);
	
			AddChild(_btnBack);

			AddEventAction(WidgetEventType.ScrollDown).Set(new idWidgetActionHandler(this,                    WidgetActionType.ScrollDownStartRepeater, WidgetEventType.ScrollDown));
			AddEventAction(WidgetEventType.ScrollUp).Set(new idWidgetActionHandler(this,                      WidgetActionType.ScrollUpStartRepeater,   WidgetEventType.ScrollUp));
			AddEventAction(WidgetEventType.ScrollDownRelease).Set(new idWidgetActionHandler(this,             WidgetActionType.StopRepeater,            WidgetEventType.ScrollDownRelease));
			AddEventAction(WidgetEventType.ScrollUpRelease).Set(new idWidgetActionHandler(this,               WidgetActionType.StopRepeater,            WidgetEventType.ScrollUpRelease));
			AddEventAction(WidgetEventType.ScrollLeftStickDown).Set(new idWidgetActionHandler(this,           WidgetActionType.ScrollDownStartRepeater, WidgetEventType.ScrollLeftStickDown));
			AddEventAction(WidgetEventType.ScrollLeftStickUp).Set(new idWidgetActionHandler(this,             WidgetActionType.ScrollUpStartRepeater,   WidgetEventType.ScrollLeftStickUp));
			AddEventAction(WidgetEventType.ScrollLeftStickDownRelease).Set(new idWidgetActionHandler(this,    WidgetActionType.StopRepeater,            WidgetEventType.ScrollLeftStickDownRelease));
			AddEventAction(WidgetEventType.ScrollLeftStickUpRelease).Set(new idWidgetActionHandler(this,      WidgetActionType.StopRepeater,            WidgetEventType.ScrollLeftStickUpRelease));

			AddEventAction(WidgetEventType.ScrollRightStickDown).Set(new idWidgetActionHandler(_lobby,        WidgetActionType.ScrollDownStartRepeater, WidgetEventType.ScrollRightStickDown));
			AddEventAction(WidgetEventType.ScrollRightStickUp).Set(new idWidgetActionHandler(_lobby,          WidgetActionType.ScrollUpStartRepeater,   WidgetEventType.ScrollRightStickUp));
			AddEventAction(WidgetEventType.ScrollRightStickDownRelease).Set(new idWidgetActionHandler(_lobby, WidgetActionType.StopRepeater,            WidgetEventType.ScrollRightStickDownRelease));
			AddEventAction(WidgetEventType.ScrollRightStickUpRelease).Set(new idWidgetActionHandler(_lobby,   WidgetActionType.StopRepeater,            WidgetEventType.ScrollRightStickUpRelease));
		}

		public override void Update()
		{
			idLog.Warning("TODO: idLobbyBase & activeLobby = session->GetActivePlatformLobbyBase();");
	
			/*if ( lobby != NULL ) {

				if ( activeLobby.GetNumActiveLobbyUsers() != 0 ) {
					if ( lobby->GetFocusIndex() >= activeLobby.GetNumActiveLobbyUsers()  ) {
						lobby->SetFocusIndex( activeLobby.GetNumActiveLobbyUsers() - 1 );
						lobby->SetViewIndex( lobby->GetViewOffset() + lobby->GetFocusIndex() );
					}
				}
			}*/

			idSWFScriptObject root = this.SWFObject.RootObject;

			if(BindSprite(root) == true)
			{
				idSWFTextInstance heading = this.Sprite.ScriptObject.GetNestedText("info", "txtHeading");

				if(heading != null)
				{
					heading.Text = "#str_swf_multiplayer";	// MULTIPLAYER
					heading.SetStrokeInfo(true, 0.75f, 1.75f);
				}

				idSWFSpriteInstance gradient = this.Sprite.ScriptObject.GetNestedSprite("info", "gradient");

				if((gradient != null) && (heading != null))
				{
					gradient.PositionX = heading.TextLength;
				}
			}

			if((_privateGameLobby == true) && (_options != null))
			{
				idLog.Warning("TODO: if ( session->GetActivePlatformLobbyBase().IsHost() && !isHost ) {");
			
				/*if ( session->GetActivePlatformLobbyBase().IsHost() && !isHost ) {
					menuOptions.Clear();
					idList< idStr > option;

					isHost = true;
					isPeer = false;

					option.Append( "#str_swf_start_match" );	// Start match
					menuOptions.Append( option );
					option.Clear();

					option.Append( "#str_swf_match_settings" );	// Match Settings
					menuOptions.Append( option );
					option.Clear();

					option.Append( "#str_swf_invite_only" );	// Toggle privacy
					menuOptions.Append( option );
					option.Clear();

					option.Append( "#str_swf_invite_friends" );	// Invite Friends
					menuOptions.Append( option );
					option.Clear();

					idMenuWidget_Button * buttonWidget = NULL;
					int index = 0;
					options->GetChildByIndex( index ).ClearEventActions();
					options->GetChildByIndex( index ).AddEventAction( WIDGET_EVENT_PRESS ).Set( WIDGET_ACTION_COMMAND, GAME_CMD_START, 0 );
					buttonWidget = dynamic_cast< idMenuWidget_Button * >( &options->GetChildByIndex( index ) );
					if ( buttonWidget != NULL ) {
						buttonWidget->SetDescription( "#str_swf_quick_start_desc" );
					}
					index++;
					options->GetChildByIndex( index ).ClearEventActions();
					options->GetChildByIndex( index ).AddEventAction( WIDGET_EVENT_PRESS ).Set( WIDGET_ACTION_COMMAND, GAME_CMD_SETTINGS, 1 );
					buttonWidget = dynamic_cast< idMenuWidget_Button * >( &options->GetChildByIndex( index ) );
					if ( buttonWidget != NULL ) {
						buttonWidget->SetDescription( "#str_swf_match_setting_desc" );
					}
					index++;
					options->GetChildByIndex( index ).ClearEventActions();
					options->GetChildByIndex( index ).AddEventAction( WIDGET_EVENT_PRESS ).Set( WIDGET_ACTION_COMMAND, GAME_CMD_TOGGLE_PRIVACY, 2 );
					buttonWidget = dynamic_cast< idMenuWidget_Button * >( &options->GetChildByIndex( index ) );
					if ( buttonWidget != NULL ) {
						buttonWidget->SetDescription( "#str_swf_toggle_privacy_desc" );
					}
					index++;
					options->GetChildByIndex( index ).ClearEventActions();
					options->GetChildByIndex( index ).AddEventAction( WIDGET_EVENT_PRESS ).Set( WIDGET_ACTION_COMMAND, GAME_CMD_INVITE, 3 );
					buttonWidget = dynamic_cast< idMenuWidget_Button * >( &options->GetChildByIndex( index ) );
					if ( buttonWidget != NULL ) {
						buttonWidget->SetDescription( "#str_swf_invite_desc" );
					}
					index++;

					options->SetListData( menuOptions );

				} else if ( session->GetActivePlatformLobbyBase().IsPeer() ) {

					if ( !isPeer ) {			

						menuOptions.Clear();
						idList< idStr > option;

						option.Append( "#str_swf_invite_friends" );	// Invite Friends
						menuOptions.Append( option );
						option.Clear();

						idMenuWidget_Button * buttonWidget = NULL;
						int index = 0;
						options->GetChildByIndex( index ).ClearEventActions();
						options->GetChildByIndex( index ).AddEventAction( WIDGET_EVENT_PRESS ).Set( WIDGET_ACTION_COMMAND, GAME_CMD_INVITE, 0 );
						buttonWidget = dynamic_cast< idMenuWidget_Button * >( &options->GetChildByIndex( index ) );
						if ( buttonWidget != NULL ) {
							buttonWidget->SetDescription( "#str_swf_invite_desc" );
						}

						options->SetListData( menuOptions );
					}
			
					isPeer = true;
					isHost = false;
				}*/
			}

			if(_menuData != null)
			{
				idMenuWidget_CommandBar cmdBar = _menuData.CommandBar;

				if(cmdBar != null)
				{
					cmdBar.ClearAllButtons();

					ButtonInfo buttonInfo = cmdBar.GetButton(Button.Joystick2);
					buttonInfo.Action.Set(WidgetActionType.GoBack);

					if(_menuData.GetPlatform() != 2)
					{
						buttonInfo.Label = "#str_00395";
					}
					
					buttonInfo = cmdBar.GetButton(Button.Joystick3);
					buttonInfo.Action.Set(WidgetActionType.SelectGamerTag);

					if(_menuData.GetPlatform() != 2)
					{
						buttonInfo.Label = "#str_swf_view_profile";
					}
					
					buttonInfo = cmdBar.GetButton(Button.Joystick1);
					buttonInfo.Action.Set(WidgetActionType.PressFocused);

					if(_menuData.GetPlatform() != 2)
					{
						buttonInfo.Label = "#str_SWF_SELECT";
					}

					idLog.Warning("TODO: lobbyUserID_t luid;");

					/*if ( isHost && CanKickSelectedPlayer( luid ) ) {
						buttonInfo = cmdBar->GetButton( idMenuWidget_CommandBar::BUTTON_JOY4 );
						buttonInfo->label = "#str_swf_kick";
						buttonInfo->action.Set( WIDGET_ACTION_JOY4_ON_PRESS );
					}*/
				}
			}		

			if(_btnBack != null)
			{
				_btnBack.BindSprite(root);
			}

			base.Update();
		}

		public override void ShowScreen(MainMenuTransition transitionType)
		{
			idLog.Warning("TODO: GameLobby ShowScreen");
		}

		public override void HideScreen(MainMenuTransition transitionType)
		{
			idLog.Warning("TODO: GameLobby HideScreen");
		}

		public override bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			idLog.Warning("TODO: GameLobby HandleAction");

			return true;
		}
		#endregion
	}
}