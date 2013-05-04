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

using idTech4.Services;
using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public class idMenuScreen_Shell_Root : idMenuScreen
	{
		#region Constants
		private const int MainOptionCount = 6;
		#endregion

		#region Properties
		public idMenuWidget_Help HelpWidget
		{
			get
			{
				return _helpWidget;
			}
		}

		public int RootIndex
		{
			get
			{
				if(_options != null)
				{
					return _options.FocusIndex;
				}

				return 0;
			}
			set
			{
				if(_options != null)
				{
					_options.SetFocusIndex(value);
				}
			}
		}
		#endregion

		#region Members
		private idMenuWidget_DynamicList _options;
		private idMenuWidget_Help _helpWidget;
		#endregion

		#region Constructor
		public idMenuScreen_Shell_Root() 
			: base()
		{

		}
		#endregion

		#region Methods
		private bool HandleAction_Command(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled)
		{
			ICommandSystem cmdSystem = idEngine.Instance.GetService<ICommandSystem>();
			idSWFParameterList parms = action.Parameters;

			switch(parms[0].ToInt32())
			{
				case RootMenuCommand.StartDemo:
					cmdSystem.BufferCommandText(string.Format("devmap {0} {1}", "demo/enpro_e3_2012", 1));
					break;
				
				case RootMenuCommand.StartDemo2:
					cmdSystem.BufferCommandText(string.Format("devmap {0} {1}", "game/le_hell", 2));
					break;

				case RootMenuCommand.Settings:
					_menuData.SetNextScreen(ShellArea.Settings, MainMenuTransition.Simple);
					break;

				case RootMenuCommand.Quit:
					idLog.Warning("TODO: HandleExitGame();");
					break;
				
				case RootMenuCommand.Developer:
					_menuData.SetNextScreen(ShellArea.Developer, MainMenuTransition.Simple);
					break;

				case RootMenuCommand.Campaign:
					_menuData.SetNextScreen(ShellArea.Campaign, MainMenuTransition.Simple);
					break;

				case RootMenuCommand.Multiplayer:
					idLog.Warning("TODO: root multiplayer");

					/*const idLocalUser * masterUser = session->GetSignInManager().GetMasterLocalUser();

					if ( masterUser == NULL ) {
						break;
					}

					if ( masterUser->GetOnlineCaps() & CAP_BLOCKED_PERMISSION ) {
						common->Dialog().AddDialog( GDM_ONLINE_INCORRECT_PERMISSIONS, DIALOG_CONTINUE, NULL, NULL, true, __FUNCTION__, __LINE__, false );
					} else if ( !masterUser->CanPlayOnline() ) { 
						class idSWFScriptFunction_Accept : public idSWFScriptFunction_RefCounted {
						public:
							idSWFScriptFunction_Accept() { }
							idSWFScriptVar Call( idSWFScriptObject * thisObject, const idSWFParmList & parms ) {
								common->Dialog().ClearDialog( GDM_PLAY_ONLINE_NO_PROFILE );
								session->ShowOnlineSignin();
								return idSWFScriptVar();
							}
						};
						class idSWFScriptFunction_Cancel : public idSWFScriptFunction_RefCounted {
						public:
							idSWFScriptFunction_Cancel() { }
							idSWFScriptVar Call( idSWFScriptObject * thisObject, const idSWFParmList & parms ) {
								common->Dialog().ClearDialog( GDM_PLAY_ONLINE_NO_PROFILE );
								return idSWFScriptVar();
							}
						};

						common->Dialog().AddDialog( GDM_PLAY_ONLINE_NO_PROFILE, DIALOG_ACCEPT_CANCEL, new (TAG_SWF) idSWFScriptFunction_Accept(), new (TAG_SWF) idSWFScriptFunction_Cancel(), false );
					} else {
						idMatchParameters matchParameters;
						matchParameters.matchFlags = DefaultPartyFlags;
						session->CreatePartyLobby( matchParameters );
					}*/
					break;
				
				case RootMenuCommand.Playstation:
					_menuData.SetNextScreen(ShellArea.Playstation, MainMenuTransition.Simple);
					break;

				case RootMenuCommand.Credits:
					_menuData.SetNextScreen(ShellArea.Credits, MainMenuTransition.Simple);
					break;
			}

			return true;
		}

		private bool HandleAction_GoBack(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled)
		{
			idEngine.Instance.GetService<ISession>().MoveToPressStart();
			return true;
		}

		private bool HandleAction_PressFocused(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled)
		{
			if(_menuData.GetPlatform() == 2)
			{
				idMenuHandler_Shell shell = _menuData as idMenuHandler_Shell;

				if(shell != null)
				{
					idMenuWidget_MenuBar menuBar = shell.MenuBar;
					
					if(menuBar != null)
					{
						idMenuWidget_MenuButton buttonWidget = menuBar.GetChildByIndex(menuBar.FocusIndex) as idMenuWidget_MenuButton;

						if(buttonWidget != null)
						{
							menuBar.ReceiveEvent(new idWidgetEvent(WidgetEventType.Press, 0, null, new idSWFParameterList()));
						}
					}
				}
			} 

			return true;
		}

		private bool HandleAction_ScrollHorizontal(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled)
		{
			if(_menuData.GetPlatform() != 2)
			{
				return true;
			}

			idMenuHandler_Shell shell = _menuData as idMenuHandler_Shell;

			if(shell == null)
			{
				return true;
			}

			idMenuWidget_MenuBar menuBar = shell.MenuBar;

			if(menuBar == null)
			{
				return true;
			}

			idSWFParameterList parms = action.Parameters;
			int index                = menuBar.ViewIndex;
			int direction            = parms[0].ToInt32();

#if ID_RETAIL
			int totalCount = menuBar.TotalNumberOfOptions - 1;
#else
			int totalCount = menuBar.TotalNumberOfOptions;
#endif

			index += direction;

			if(index < 0) 
			{
				index = totalCount - 1;
			} 
			else if(index >= totalCount) 
			{
				index = 0;
			}

			this.RootIndex = index;

			menuBar.ViewIndex  = index;
			menuBar.SetFocusIndex(index);

			return true;
		}
		#endregion

		#region idMenuScreen implementation
		public override bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			if(_menuData == null)
			{
				return true;
			}

			if(_menuData.ActiveScreen != ShellArea.Root)
			{
				return false;
			}

			WidgetActionType actionType = action.Type;
			idSWFParameterList parms    = action.Parameters;

			switch(actionType)
			{
				case WidgetActionType.GoBack:
					return HandleAction_GoBack(action, ev, widget, forceHandled);
		
				case WidgetActionType.PressFocused:
					return HandleAction_PressFocused(action, ev, widget, forceHandled);

				case WidgetActionType.ScrollHorizontal:
					return HandleAction_ScrollHorizontal(action, ev, widget, forceHandled);

				case WidgetActionType.Command:
					return HandleAction_Command(action, ev, widget, forceHandled);
			}

			return base.HandleAction(action, ev, widget, forceHandled);
		}

		public override void Initialize(idMenuHandler data)
		{
			base.Initialize(data);

			if(data != null)
			{
				this.UserInterface = data.UserInterface;
			}

			SetSpritePath("menuMain");

			_options = new idMenuWidget_DynamicList();
			_options.VisibleOptionCount = MainOptionCount;
			_options.SetSpritePath(this.SpritePath, "info", "options");
			_options.Initialize(data);
			_options.IsWrappingAllowed = true;

			AddChild(_options);

			_helpWidget = new idMenuWidget_Help();
			_helpWidget.SetSpritePath(this.SpritePath, "info", "helpTooltip");
			
			AddChild(_helpWidget);

			while(_options.Children.Length < MainOptionCount)
			{
				idMenuWidget_Button buttonWidget = new idMenuWidget_Button();
				buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.PressFocused, _options.Children.Length);
				buttonWidget.Initialize(data);				
				buttonWidget.RegisterEventObserver(_helpWidget);
				
				_options.AddChild(buttonWidget);
			}

			_options.AddEventAction(WidgetEventType.ScrollDown).Set(new idWidgetActionHandler(_options,                 WidgetActionType.ScrollDownStartRepeater, WidgetEventType.ScrollDown));
			_options.AddEventAction(WidgetEventType.ScrollUp).Set(new idWidgetActionHandler(_options,                   WidgetActionType.ScrollUpStartRepeater,   WidgetEventType.ScrollUp));
			_options.AddEventAction(WidgetEventType.ScrollDownRelease).Set(new idWidgetActionHandler(_options,          WidgetActionType.StopRepeater,            WidgetEventType.ScrollDownRelease));
			_options.AddEventAction(WidgetEventType.ScrollUpRelease).Set(new idWidgetActionHandler(_options,            WidgetActionType.StopRepeater,            WidgetEventType.ScrollUpRelease));
			_options.AddEventAction(WidgetEventType.ScrollLeftStickDown).Set(new idWidgetActionHandler(_options,        WidgetActionType.ScrollDownStartRepeater, WidgetEventType.ScrollLeftStickDown));
			_options.AddEventAction(WidgetEventType.ScrollLeftStickUp).Set(new idWidgetActionHandler(_options,          WidgetActionType.ScrollUpStartRepeater,   WidgetEventType.ScrollLeftStickUp));
			_options.AddEventAction(WidgetEventType.ScrollLeftStickDownRelease).Set(new idWidgetActionHandler(_options, WidgetActionType.StopRepeater,            WidgetEventType.ScrollLeftStickDownRelease));
			_options.AddEventAction(WidgetEventType.ScrollLeftStickUpRelease).Set(new idWidgetActionHandler(_options,   WidgetActionType.StopRepeater,            WidgetEventType.ScrollLeftStickUpRelease));

			AddEventAction(WidgetEventType.ScrollRight).Set(new idWidgetActionHandler(this,        WidgetActionType.ScrollRightStartRepeater, WidgetEventType.ScrollRight));
			AddEventAction(WidgetEventType.ScrollRightRelease).Set(new idWidgetActionHandler(this, WidgetActionType.StopRepeater,             WidgetEventType.ScrollRightRelease));
			AddEventAction(WidgetEventType.ScrollLeft).Set(new idWidgetActionHandler(this,         WidgetActionType.ScrollLeftStartRepeater,  WidgetEventType.ScrollLeft));
			AddEventAction(WidgetEventType.ScrollLeftRelease).Set(new idWidgetActionHandler(this,  WidgetActionType.StopRepeater,             WidgetEventType.ScrollLeftRelease));
			AddEventAction(WidgetEventType.Press).Set(WidgetActionType.PressFocused, 0);
		}

		public override void ShowScreen(MainMenuTransition transitionType)
		{
			if((_menuData != null) && (_menuData.GetPlatform() != 2))
			{
				ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

				List<List<string>> menuOptions = new List<List<string>>();
				List<string> option            = new List<string>();

				int index = 0;

				if(cvarSystem.GetBool("g_demoMode") == true)
				{
					idLog.Warning("TODO: demo mode");

					/*idMenuWidget_Button * buttonWidget = NULL;

					option.Append( "START DEMO" );	// START DEMO
					menuOptions.Append( option );
					options->GetChildByIndex( index ).ClearEventActions();
					options->GetChildByIndex( index ).AddEventAction( WIDGET_EVENT_PRESS ).Set( WIDGET_ACTION_COMMAND, ROOT_CMD_START_DEMO );
					buttonWidget = dynamic_cast< idMenuWidget_Button * >( &options->GetChildByIndex( index ) );
					if ( buttonWidget != NULL ) {
						buttonWidget->SetDescription( "Launch the demo" );
					}
					index++;

					if ( g_demoMode.GetInteger() == 2 ) {
						option.Clear();
						option.Append( "START PRESS DEMO" );	// START DEMO
						menuOptions.Append( option );
						options->GetChildByIndex( index ).ClearEventActions();
						options->GetChildByIndex( index ).AddEventAction( WIDGET_EVENT_PRESS ).Set( WIDGET_ACTION_COMMAND, ROOT_CMD_START_DEMO2 );
						buttonWidget = dynamic_cast< idMenuWidget_Button * >( &options->GetChildByIndex( index ) );
						if ( buttonWidget != NULL ) {
							buttonWidget->SetDescription( "Launch the press demo" );
						}
						index++;
					}

					option.Clear();
					option.Append( "#str_swf_settings" );	// settings
					menuOptions.Append( option );
					options->GetChildByIndex( index ).ClearEventActions();
					options->GetChildByIndex( index ).AddEventAction( WIDGET_EVENT_PRESS ).Set( WIDGET_ACTION_COMMAND, ROOT_CMD_SETTINGS );
					buttonWidget = dynamic_cast< idMenuWidget_Button * >( &options->GetChildByIndex( index ) );
					if ( buttonWidget != NULL ) {
						buttonWidget->SetDescription( "#str_02206" );
					}
					index++;

					option.Clear();
					option.Append( "#str_swf_quit" );	// quit
					menuOptions.Append( option );
					options->GetChildByIndex( index ).ClearEventActions();
					options->GetChildByIndex( index ).AddEventAction( WIDGET_EVENT_PRESS ).Set( WIDGET_ACTION_COMMAND, ROOT_CMD_QUIT );
					buttonWidget = dynamic_cast< idMenuWidget_Button * >( &options->GetChildByIndex( index ) );
					if ( buttonWidget != NULL ) {
						buttonWidget->SetDescription( "#str_01976" );
					}
					index++;*/
				}
				else
				{
					idMenuWidget_Button buttonWidget = null;

#if !ID_RETAIL
					option.Add("DEV"); // DEV
					menuOptions.Add(option);

					_options.GetChildByIndex(index).ClearEventActions();
					_options.GetChildByIndex(index).AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, RootMenuCommand.Developer);

					buttonWidget = _options.GetChildByIndex(index) as idMenuWidget_Button;

					if(buttonWidget != null)
					{
						buttonWidget.Description = "View a list of maps available for play";
					}

					index++;
#endif

					// ------------------------------
					// SINGLEPLAYER
					option.Clear();
					option.Add("#str_swf_campaign");
					menuOptions.Add(option);

					_options.GetChildByIndex(index).ClearEventActions();
					_options.GetChildByIndex(index).AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, RootMenuCommand.Campaign);

					buttonWidget = _options.GetChildByIndex(index) as idMenuWidget_Button;

					if(buttonWidget != null)
					{
						buttonWidget.Description = "#str_swf_campaign_desc";
					}

					index++;

					// ------------------------------
					// MULTIPLAYER
					option.Clear();
					option.Add("#str_swf_multiplayer");
					menuOptions.Add(option);

					_options.GetChildByIndex(index).ClearEventActions();
					_options.GetChildByIndex(index).AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, RootMenuCommand.Multiplayer);

					buttonWidget = _options.GetChildByIndex(index) as idMenuWidget_Button;

					if(buttonWidget != null)
					{
						buttonWidget.Description = "#str_02215";
					}

					index++;

					// ------------------------------
					// SETTINGS
					option.Clear();
					option.Add("#str_swf_settings");
					menuOptions.Add(option);

					_options.GetChildByIndex(index).ClearEventActions();
					_options.GetChildByIndex(index).AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, RootMenuCommand.Settings);

					buttonWidget = _options.GetChildByIndex(index) as idMenuWidget_Button;

					if(buttonWidget != null)
					{
						buttonWidget.Description = "#str_02206";
					}

					index++;

					// ------------------------------
					// CREDITS
					option.Clear();
					option.Add("#str_swf_credits");
					menuOptions.Add(option);

					_options.GetChildByIndex(index).ClearEventActions();
					_options.GetChildByIndex(index).AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, RootMenuCommand.Credits);

					buttonWidget = _options.GetChildByIndex(index) as idMenuWidget_Button;

					if(buttonWidget != null)
					{
						buttonWidget.Description = "#str_02219";
					}

					index++;

					// ------------------------------
					// QUIT
					// only add quit option for PC
					option.Clear();
					option.Add("#str_swf_quit");
					menuOptions.Add(option);

					_options.GetChildByIndex(index).ClearEventActions();
					_options.GetChildByIndex(index).AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, RootMenuCommand.Quit);

					buttonWidget = _options.GetChildByIndex(index) as idMenuWidget_Button;

					if(buttonWidget != null)
					{
						buttonWidget.Description = "#str_01976";
					}

					index++;
				}

				_options.SetListData(menuOptions);
			}
			else
			{
				_options.SetListData(new List<List<string>>());
			}

			base.ShowScreen(transitionType);

			if((_menuData != null) && (_menuData.GetPlatform() == 2))
			{
				idMenuHandler_Shell shell = _menuData as idMenuHandler_Shell;

				if(shell != null)
				{
					idMenuWidget_MenuBar menuBar = shell.MenuBar;

					if(menuBar != null)
					{
						menuBar.SetFocusIndex(this.RootIndex);
					}
				}
			}
		}

		public override void Update()
		{
			if(_menuData != null)
			{
				idMenuWidget_CommandBar cmdBar = _menuData.CommandBar;

				if(cmdBar != null)
				{
					cmdBar.ClearAllButtons();
					ButtonInfo buttonInfo;

					if(idEngine.Instance.GetService<ICVarSystem>().GetBool("g_demoMode") == false)
					{
						buttonInfo = cmdBar.GetButton(Button.Joystick2);

						if(_menuData.GetPlatform() != 2)
						{
							buttonInfo.Label = "#str_00395";
						}

						buttonInfo.Action.Set(WidgetActionType.GoBack);
					}

					buttonInfo = cmdBar.GetButton(Button.Joystick1);

					if(_menuData.GetPlatform() != 2) 
					{
						buttonInfo.Label = "#str_SWF_SELECT";
					}

					buttonInfo.Action.Set(WidgetActionType.PressFocused);
				}		
			}

			base.Update();
		}
		#endregion
	}

	public class RootMenuCommand
	{
		public const int StartDemo   = 1;
		public const int StartDemo2  = 2;
		public const int Settings    = 3;
		public const int Quit        = 4;
		public const int Developer   = 5;
		public const int Campaign    = 6;
		public const int Multiplayer = 7;
		public const int Playstation = 8;
		public const int Credits     = 9;
	}
}