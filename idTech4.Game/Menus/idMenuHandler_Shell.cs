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

using idTech4.Renderer;
using idTech4.Services;
using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public class idMenuHandler_Shell : idMenuHandler
	{
		#region Constants
		public const int MaxMenuOptions = 6;
		#endregion

		#region Properties
		public bool IsPacifierVisible
		{
			get
			{
				return ((_pacifier != null) && (_pacifier.Sprite != null)) ? _pacifier.Sprite.IsVisible : false;
			}
		}

		public idMenuWidget_MenuBar MenuBar
		{
			get
			{
				return _menuBar;
			}
		}

		public idMenuWidget Pacifier
		{
			get
			{
				return _pacifier;
			}
		}
		#endregion

		#region Members
		private ShellState _state;
		private ShellState _nextState;

		private bool _smallFrameShowing;
		private bool _largeFrameShowing;
		private bool _backgroundShowing;
		private bool _waitForBinding;
		private string _waitBind;
		//idSysSignal				deviceRequestedSignal;
		
		// TODO:
		/*idList<const char *, TAG_IDLIB_LIST_MENU>	mpGameModes;
		idList<mpMap_t, TAG_IDLIB_LIST_MENU>			mpGameMaps;*/
		private idMenuWidget_MenuBar _menuBar;
		private idMenuWidget _pacifier;
	
		private int	_timeRemaining;
		private int	_nextPeerUpdateMs;
		private int	_newGameType;
		private bool _inGame;
		private bool _showingIntro;
		private bool _continueWaitForEnumerate;
		private bool _gameComplete;

		//private idSWF _introGui;
		// TODO: const idSoundShader *	typeSoundShader;
		private idMaterial _doom3Intro;
		private idMaterial _roeIntro;
		private idMaterial _lmIntro;
		private idMaterial _marsRotation;
		private List<string> _navOptions;
		#endregion

		#region Constructor
		public idMenuHandler_Shell()
			: base()
		{
			_state             = ShellState.Invalid;
			_nextState         = ShellState.Invalid;
			_backgroundShowing = true;

			_navOptions = new List<string>();
		}
		#endregion

		#region Events
		public override bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			if(_activeScreen == ShellArea.Invalid)
			{
				return true;
			}

			WidgetActionType actionType   = action.Type;
			idSWFParameterList parameters = action.Parameters;
			ICVarSystem cvarSystem        = idEngine.Instance.GetService<ICVarSystem>();
			ICommandSystem cmdSystem      = idEngine.Instance.GetService<ICommandSystem>();

			if(ev.Type == WidgetEventType.Command)
			{
				/*if((_activeScreen == ShellArea.Root) && (_navOptions.Count > 0))
				{
					return true;
				}*/

				if((_menuScreens[(int) _activeScreen] != null) && (forceHandled == false))
				{
					if(_menuScreens[(int) _activeScreen].HandleAction(action, ev, widget, true) == true)
					{
						if(actionType == WidgetActionType.GoBack)
						{
							idLog.Warning("TODO: PlaySound( GUI_SOUND_BACK );");
						} 
						else 
						{
							idLog.Warning("TODO: PlaySound( GUI_SOUND_ADVANCE );");
						}
				
						return true;
					}
				}
			}

			switch(actionType)
			{
				case WidgetActionType.Command:
					if(parameters.Count < 2)
					{
						return true;
					}

					ShellCommand cmd = (ShellCommand) parameters[0].ToInt32();

					if(((_activeScreen == ShellArea.GameLobby) || (_activeScreen == ShellArea.MatchSettings))
						&& (cmd != ShellCommand.Quit) && (cmd != ShellCommand.Multiplayer))
					{
						idLog.Warning("TODO: session->Cancel();");
						idLog.Warning("TODO: session->Cancel();");
					}
					else if(((_activeScreen == ShellArea.PartyLobby) || (_activeScreen == ShellArea.LeaderBoards) || (_activeScreen == ShellArea.Browser) || (_activeScreen == ShellArea.ModeSelect))
						&& (cmd != ShellCommand.Quit) && (cmd != ShellCommand.Multiplayer))
					{
						idLog.Warning("TODO: session->Cancel();");
					}

					if((cmd != ShellCommand.Quit) && ((_nextScreen == ShellArea.Stereoscopics) || (_nextScreen == ShellArea.SystemOptions) || (_nextScreen == ShellArea.GameOptions) || (_nextScreen == ShellArea.GamePad) || (_nextScreen == ShellArea.MatchSettings)))
					{
						cvarSystem.SetModifiedFlags(CVarFlags.Archive);
					}

					int index = parameters[1].ToInt32();

					_menuBar.SetFocusIndex(index);
					_menuBar.ViewIndex = index;
					
					idMenuScreen_Shell_Root menu = _menuScreens[(int) ShellArea.Root] as idMenuScreen_Shell_Root;

					if(menu != null)
					{
						menu.RootIndex = index;
					}

					switch(cmd)
					{
						case ShellCommand.Demo0:
							cmdSystem.BufferCommandText(string.Format("devmap {0} {1}", "demo/enpro_e3_2012", 1));
							break;

						case ShellCommand.Demo1:
							cmdSystem.BufferCommandText(string.Format("devmap {0} {1}", "game/le_hell", 2));
							break;

						case ShellCommand.Developer:
							_nextScreen = ShellArea.Developer;
							_transition = MainMenuTransition.Simple;
							break;
				
						case ShellCommand.Campaign:
							_nextScreen = ShellArea.Campaign;
							_transition = MainMenuTransition.Simple;
							break;

						case ShellCommand.Multiplayer:
							idLog.Warning("TODO: shell command mp");

							/*idMatchParameters matchParameters;
							matchParameters.matchFlags = DefaultPartyFlags;
							session->CreatePartyLobby( matchParameters );*/
							break;
				
						case ShellCommand.Settings:
							_nextScreen = ShellArea.Settings;
							_transition = MainMenuTransition.Simple;
							break;
					
						case ShellCommand.Credits:
							_nextScreen = ShellArea.Credits;
							_transition = MainMenuTransition.Simple;
							break;

						case ShellCommand.Quit:
							idLog.Warning("TODO: HandleExitGameBtn();");
							break;
					}
						
					return true;
				}
			
				return base.HandleAction(action, ev, widget, forceHandled);
		}
		#endregion

		#region Initialization
		public override void Initialize(string swfFile)
		{
			base.Initialize(swfFile);
			
			idLog.Warning("TODO: idMenuHandler_Shell.Initialize");

			//---------------------
			// Initialize the menus
			//---------------------
			for(int i = 0; i < (int) ShellArea.AreaCount; i++)
			{
				_menuScreens[i] = null;
			}

			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

			if(_inGame == true)
			{
				idLog.Warning("TODO: idMenuHandler_Shell ingame");
		
				/*BIND_SHELL_SCREEN( SHELL_AREA_ROOT, idMenuScreen_Shell_Pause, this );
				BIND_SHELL_SCREEN( SHELL_AREA_SETTINGS, idMenuScreen_Shell_Settings, this );
				BIND_SHELL_SCREEN( SHELL_AREA_LOAD, idMenuScreen_Shell_Load, this );
				BIND_SHELL_SCREEN( SHELL_AREA_SYSTEM_OPTIONS, idMenuScreen_Shell_SystemOptions, this );
				BIND_SHELL_SCREEN( SHELL_AREA_GAME_OPTIONS, idMenuScreen_Shell_GameOptions, this );
				BIND_SHELL_SCREEN( SHELL_AREA_SAVE, idMenuScreen_Shell_Save, this );
				BIND_SHELL_SCREEN( SHELL_AREA_STEREOSCOPICS, idMenuScreen_Shell_Stereoscopics, this );		
				BIND_SHELL_SCREEN( SHELL_AREA_CONTROLS, idMenuScreen_Shell_Controls, this );
				BIND_SHELL_SCREEN( SHELL_AREA_KEYBOARD, idMenuScreen_Shell_Bindings, this );
				BIND_SHELL_SCREEN( SHELL_AREA_RESOLUTION, idMenuScreen_Shell_Resolution, this );
				BIND_SHELL_SCREEN( SHELL_AREA_CONTROLLER_LAYOUT, idMenuScreen_Shell_ControllerLayout, this );

				BIND_SHELL_SCREEN( SHELL_AREA_GAMEPAD, idMenuScreen_Shell_Gamepad, this );		
				BIND_SHELL_SCREEN( SHELL_AREA_CREDITS, idMenuScreen_Shell_Credits, this );*/
			} 
			else 
			{
				RegisterShellScreen<idMenuScreen_Shell_PressStart>(ShellArea.Start, this);
				RegisterShellScreen<idMenuScreen_Shell_Root>(ShellArea.Root, this);

				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_CAMPAIGN, idMenuScreen_Shell_Singleplayer, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_SETTINGS, idMenuScreen_Shell_Settings, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_LOAD, idMenuScreen_Shell_Load, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_NEW_GAME, idMenuScreen_Shell_NewGame, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_SYSTEM_OPTIONS, idMenuScreen_Shell_SystemOptions, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_GAME_OPTIONS, idMenuScreen_Shell_GameOptions, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_PARTY_LOBBY, idMenuScreen_Shell_PartyLobby, this );");
				RegisterShellScreen<idMenuScreen_Shell_GameLobby>(ShellArea.GameLobby, this);
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_STEREOSCOPICS, idMenuScreen_Shell_Stereoscopics, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_DIFFICULTY, idMenuScreen_Shell_Difficulty, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_CONTROLS, idMenuScreen_Shell_Controls, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_KEYBOARD, idMenuScreen_Shell_Bindings, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_RESOLUTION, idMenuScreen_Shell_Resolution, this );");
				RegisterShellScreen<idMenuScreen_Shell_ControllerLayout>(ShellArea.ControllerLayout, this);
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_DEV, idMenuScreen_Shell_Dev, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_LEADERBOARDS, idMenuScreen_Shell_Leaderboards, this );");
				RegisterShellScreen<idMenuScreen_Shell_GamePad>(ShellArea.GamePad, this);
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_MATCH_SETTINGS, idMenuScreen_Shell_MatchSettings, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_MODE_SELECT, idMenuScreen_Shell_ModeSelect, this );");
				idLog.Warning("TODO: BIND_SHELL_SCREEN( SHELL_AREA_BROWSER, idMenuScreen_Shell_GameBrowser, this );");
				RegisterShellScreen<idMenuScreen_Shell_Credits>(ShellArea.Credits, this);

				_doom3Intro = declManager.FindMaterial("gui/intro/introloop");
				_roeIntro   = declManager.FindMaterial("gui/intro/marsflyby");

				//typeSoundShader = declManager->FindSound( "gui/teletype/print_text", true );
				idLog.Warning("TODO: typeSoundShader = declManager->FindSound( \"gui/teletype/print_text\", true );");
				idLog.Warning("TODO: declManager->FindSound( \"gui/doomintro\", true );");

				_marsRotation = declManager.FindMaterial("gui/shell/mars_rotation");
			}

			_menuBar = new idMenuWidget_MenuBar();
			_menuBar.SetSpritePath("pcBar");
			_menuBar.Initialize(this);
			_menuBar.VisibleOptionCount = MaxMenuOptions;
			_menuBar.IsWrappingAllowed  = true;
			_menuBar.ButtonSpacing      = 45.0f;

			while(_menuBar.Children.Length < MaxMenuOptions)
			{
				idMenuWidget_MenuButton navButton = new idMenuWidget_MenuButton();
				
				idMenuScreen_Shell_Root rootScreen = _menuScreens[(int) ShellArea.Root] as idMenuScreen_Shell_Root;

				if(rootScreen != null)
				{
					navButton.RegisterEventObserver(rootScreen.HelpWidget);
				}

				_menuBar.AddChild(navButton);
			}

			AddChild(_menuBar);

			//
			// command bar
			//
			_cmdBar            = new idMenuWidget_CommandBar();
			_cmdBar.Alignment  = Alignment.Left;
			_cmdBar.SetSpritePath("prompts");
			_cmdBar.Initialize(this);
			
			AddChild(_cmdBar);

			_pacifier = new idMenuWidget();
			_pacifier.SetSpritePath("pacifier");
		
			AddChild(_pacifier);

			// precache sounds
			// don't load gui music for the pause menu to save some memory
			/*const idSoundShader * soundShader = NULL;
			if ( !inGame ) {
				soundShader = declManager->FindSound( "gui/menu_music", true );
				if ( soundShader != NULL ) {
					sounds[ GUI_SOUND_MUSIC ] = soundShader->GetName();
				}
			} else {
				idStrStatic< MAX_OSPATH > shortMapName = gameLocal.GetMapFileName();
				shortMapName.StripFileExtension();
				shortMapName.StripLeading( "maps/" );
				shortMapName.StripLeading( "game/" );
				if ( ( shortMapName.Icmp( "le_hell_post" ) == 0 ) || ( shortMapName.Icmp( "hellhole" ) == 0 ) || ( shortMapName.Icmp( "hell" ) == 0 ) ) {
					soundShader = declManager->FindSound( "hell_music_credits", true );
					if ( soundShader != NULL ) {
						sounds[ GUI_SOUND_MUSIC ] = soundShader->GetName();
					}
				}
			}

			soundShader = declManager->FindSound( "gui/list_scroll", true );
			if ( soundShader != NULL ) {
				sounds[ GUI_SOUND_SCROLL ] = soundShader->GetName();
			}
			soundShader = declManager->FindSound( "gui/btn_PDA_advance", true );
			if ( soundShader != NULL ) {
				sounds[ GUI_SOUND_ADVANCE ] = soundShader->GetName();
			}
			soundShader = declManager->FindSound( "gui/btn_PDA_back", true );
			if ( soundShader != NULL ) {
				sounds[ GUI_SOUND_BACK ] = soundShader->GetName();
			}
			soundShader = declManager->FindSound( "gui/menu_build_on", true );
			if ( soundShader != NULL ) {
				sounds[ GUI_SOUND_BUILD_ON ] = soundShader->GetName();
			}
			soundShader = declManager->FindSound( "gui/pda_next_tab", true );
			if ( soundShader != NULL ) {
				sounds[ GUI_SOUND_BUILD_ON ] = soundShader->GetName();
			}
			soundShader = declManager->FindSound( "gui/btn_set_focus", true );
			if ( soundShader != NULL ) {
				sounds[ GUI_SOUND_FOCUS ] = soundShader->GetName();
			}
			soundShader = declManager->FindSound( "gui/btn_roll_over", true );
			if ( soundShader != NULL ) {
				sounds[ GUI_SOUND_ROLL_OVER ] = soundShader->GetName();
			}
			soundShader = declManager->FindSound( "gui/btn_roll_out", true );
			if ( soundShader != NULL ) {
				sounds[ GUI_SOUND_ROLL_OUT ] = soundShader->GetName();
			}

			class idPauseGUIClose : public idSWFScriptFunction_RefCounted {
			public:
				idSWFScriptVar Call( idSWFScriptObject * thisObject, const idSWFParmList & parms ) {
					gameLocal.Shell_Show( false );
					return idSWFScriptVar();
				}
			};	

			if ( gui != NULL ) {
				gui->SetGlobal( "closeMenu", new idPauseGUIClose() );
			}*/
		}

		private void RegisterShellScreen<T>(ShellArea shellArea, idMenuHandler handler) where T : idMenuScreen, new()
		{
			_menuScreens[(int) shellArea] = new T();
			_menuScreens[(int) shellArea].Initialize(handler);
		}

		protected override void Cleanup() 
		{
			base.Cleanup();
	
			// TODO: _introGui.Dispose();
			_introGui = null;
		}

		public void SetupPCOptions()
		{
			if(_inGame == true)
			{
				return;
			}

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			_navOptions.Clear();

			if((GetPlatform() == 2) && (_menuBar != null))
			{
				if(cvarSystem.GetBool("g_demoMode") == true)
				{
					_navOptions.Add("START DEMO"); // START DEMO
			
					if(cvarSystem.GetInt("g_demoMode") == 2)
					{
						_navOptions.Add("START PRESS DEMO");	// START DEMO
					}
			
					_navOptions.Add("#str_swf_settings");	// settings
					_navOptions.Add("#str_swf_quit");	// quit

					idMenuWidget_MenuButton buttonWidget = null;
					int index                            = 0;

					buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

					if(buttonWidget != null)
					{
						buttonWidget.ClearEventActions();
						buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Demo0, index);
						buttonWidget.Description = "Launch the demo";
					}

					if(cvarSystem.GetInt("g_demoMode") == 2)
					{
						index++;
						buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

						if(buttonWidget != null)
						{
							buttonWidget.ClearEventActions();
							buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Demo1, index);
							buttonWidget.Description = "Launch the press Demo";
						}
					}
			
					index++;
					buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

					if(buttonWidget != null)
					{
						buttonWidget.ClearEventActions();
						buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Settings, index);
						buttonWidget.Description = "#str_02206";
					}
					
					index++;
					buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

					if(buttonWidget != null)
					{
						buttonWidget.ClearEventActions();
						buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Quit, index);
						buttonWidget.Description = "#str_01976";
					}
				}
				else
				{
#if !ID_RETAIL
					_navOptions.Add("DEV");
#endif

					_navOptions.Add("#str_swf_campaign");	 // singleplayer
					_navOptions.Add("#str_swf_multiplayer"); // multiplayer
					_navOptions.Add("#str_swf_settings");    // settings
					_navOptions.Add("#str_swf_credits");     // credits
					_navOptions.Add("#str_swf_quit");        // quit

			
					idMenuWidget_MenuButton buttonWidget = null;
					int index = 0;

#if !ID_RETAIL
					buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

					if(buttonWidget != null)
					{
						buttonWidget.ClearEventActions();
						buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Developer, index);
						buttonWidget.Description = "View a list of maps available for play";
					}

					index++;
#endif

					buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

					if(buttonWidget != null)
					{
						buttonWidget.ClearEventActions();
						buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Campaign, index);
						buttonWidget.Description = "#str_swf_campaign_desc";
					}
	
					index++;
					buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

					if(buttonWidget != null)
					{
						buttonWidget.ClearEventActions();
						buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Multiplayer, index);
						buttonWidget.Description = "#str_02215";
					}

					index++;
					buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

					if(buttonWidget != null)
					{
						buttonWidget.ClearEventActions();
						buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Settings, index);
						buttonWidget.Description = "#str_02206";
					}	

					index++;
					buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

					if(buttonWidget != null)
					{
						buttonWidget.ClearEventActions();
						buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Credits, index);
						buttonWidget.Description = "#str_02219";
					}	
		
					index++;
					buttonWidget = _menuBar.GetChildByIndex(index) as idMenuWidget_MenuButton;

					if(buttonWidget != null)
					{
						buttonWidget.ClearEventActions();
						buttonWidget.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) ShellCommand.Quit, index);
						buttonWidget.Description = "#str_01976";
					}	
				}
			}

			if((_menuBar != null) && (_gui != null))
			{
				idSWFScriptObject root = _gui.RootObject;

				if(_menuBar.BindSprite(root) == true)
				{
					_menuBar.Sprite.IsVisible = true;
					_menuBar.SetListHeadings(_navOptions);
					_menuBar.Update();	
			
					idMenuScreen_Shell_Root menu = _menuScreens[(int) ShellArea.Root] as idMenuScreen_Shell_Root;

					if(menu != null)
					{
						int activeIndex = menu.RootIndex;

						_menuBar.ViewIndex = activeIndex;
						_menuBar.SetFocusIndex(activeIndex);		
					}
				}
			}
		}
		#endregion

		#region State
		#region Properties
		public bool IsInGame
		{
			get
			{
				return _inGame;
			}
			set
			{
				_inGame = value;
			}
		}

		public ShellState State
		{
			get
			{
				return _state;
			}
			set
			{
				_nextState = value;
			}
		}
		#endregion

		#region Methods
		public override void ActivateMenu(bool show)
		{
			if((show == true) && (_gui != null) && (_gui.IsActive == true))
			{
				return;
			}
			else if((show == false) && (_gui != null) && (_gui.IsActive == false))
			{
				return;
			}

			if(_inGame == true)
			{
				idLog.Warning("TODO: ActivateMenu ingame");
	
				/*idPlayer * player = gameLocal.GetLocalPlayer();
				if ( player != NULL ) {
					if ( !show ) {
						bool isDead = false;			
						if ( player->health <= 0 ) {
							isDead = true;
						}
			
						if ( isDead && !common->IsMultiplayer() ) {
							return;
						}
					}
				}*/
			}
	
			base.ActivateMenu(show);

			if(show == true)
			{
				if(_inGame == false)
				{
					idLog.Warning("TODO: PlaySound( GUI_SOUND_MUSIC );");

					if(_gui != null)
					{
						idSWFSpriteInstance mars = _gui.RootObject.GetNestedSprite("mars");

						if(mars != null)
						{
							mars.StereoDepth = StereoDepthType.Far;

							idSWFSpriteInstance planet = mars.ScriptObject.GetNestedSprite("planet");

							if((_marsRotation != null) && (planet != null))
							{
								idMaterial mat = _marsRotation;

								if(mat != null)
								{
									int c = mat.StageCount;

									for( int i = 0; i < c; i++) 
									{
										MaterialStage stage = mat.GetStage(i);

										if((stage != null) && (stage.Texture.Cinematic != null))
										{
											idLog.Warning("TODO stage->texture.cinematic->ResetTime( Sys_Milliseconds() );");
										}
									}
								}

								planet.SetMaterial(mat);
							}
						}
					}
				}

				SetupPCOptions();
		
				if(_cmdBar != null)
				{
					_cmdBar.ClearAllButtons();
					_cmdBar.Update();
				}
			}
			else
			{
				ClearWidgetActionRepeater();

				_nextScreen   = ShellArea.Invalid;
				_activeScreen = ShellArea.Invalid;
				_nextState    = ShellState.Invalid;

				_state             = ShellState.Invalid;
				_smallFrameShowing = false;
				_largeFrameShowing = false;
				_backgroundShowing = true;

				idLog.Warning("TODO: common->Dialog().ClearDialog( GDM_LEAVE_LOBBY_RET_NEW_PARTY );");
			}
		}

		public void HidePacifier()
		{
			if(this.Pacifier != null)
			{
				this.Pacifier.Hide();
			}
		}

		public override void Update()
		{			
#if ID_360
	if ( deviceRequestedSignal.Wait( 0 ) ) {
		// This clears the delete save dialog to catch the case of a delete confirmation for an old device after we've changed the device.
		common->Dialog().ClearDialog( GDM_DELETE_SAVE );
		common->Dialog().ClearDialog( GDM_DELETE_CORRUPT_SAVEGAME );
		common->Dialog().ClearDialog( GDM_RESTORE_CORRUPT_SAVEGAME );
		common->Dialog().ClearDialog( GDM_LOAD_DAMAGED_FILE );
		common->Dialog().ClearDialog( GDM_OVERWRITE_SAVE );

	}
#endif
			if((_gui == null) || (_gui.IsActive == false))
			{
				return;
			}

			IDialog dialog = idEngine.Instance.GetService<IDialog>();
			
			if(((this.IsPacifierVisible == true) || (dialog.IsActive == true)) && (_actionRepeater.IsActive == true)) 
			{
				ClearWidgetActionRepeater();
			} 

			if(_nextState != _state)
			{	
				if((_introGui != null) && (_introGui.IsActive == true))
				{
					idLog.Warning("TODO: introgui");
				
					/*gui->StopSound();
					showingIntro = false;
					introGui->Activate( false );
					PlaySound( GUI_SOUND_MUSIC );*/
				}

				if(_nextState == ShellState.PressStart)
				{
					HidePacifier();

					_nextScreen = ShellArea.Start;
					_transition = MainMenuTransition.Simple;
					_state      = _nextState;

					if((_menuBar != null) && (_gui != null))
					{
						_menuBar.ClearSprite();
					}
				} 
				else if(_nextState == ShellState.Idle)
				{
					HidePacifier();

					if((_nextScreen == ShellArea.Start) || (_nextScreen == ShellArea.PartyLobby) || (_nextScreen == ShellArea.GameLobby) || (_nextScreen == ShellArea.Invalid))
					{
						_nextScreen = ShellArea.Root;
					}

					if((_menuBar != null) && (_gui != null))
					{
						idSWFScriptObject root = _gui.RootObject;
						_menuBar.BindSprite(root);

						SetupPCOptions();
					}

					_transition = MainMenuTransition.Simple;
					_state      = _nextState;
				} 
				else if(_nextState == ShellState.PartyLobby)
				{
					idLog.Warning("TODO: nextState party lobby");

					HidePacifier();

					_nextState  = ShellState.PartyLobby;
					_transition = MainMenuTransition.Simple;
					_state      = _nextState;
				} 
				else if(_nextState == ShellState.GameLobby)
				{
					idLog.Warning("TODO: nextState game lobby");
					
					HidePacifier();
					
					/*if ( state != SHELL_STATE_IN_GAME ) {
						timeRemaining = WAIT_START_TIME_LONG;
						idMatchParameters matchParameters = session->GetActivePlatformLobbyBase().GetMatchParms();*/
						/*if ( MatchTypeIsPrivate( matchParameters.matchFlags ) && ActiveScreen() == SHELL_AREA_PARTY_LOBBY ) {
							timeRemaining = 0;
							session->StartMatch();
							state = SHELL_STATE_IN_GAME;
						} else {*/
						/*nextScreen = SHELL_AREA_GAME_LOBBY;
						transition = MENU_TRANSITION_SIMPLE;
						//}

						state = nextState;
					}*/
				} 
				else if(_nextState == ShellState.Paused)
				{
					HidePacifier();

					_transition = MainMenuTransition.Simple;

					if(_gameComplete == true)
					{
						_nextScreen = ShellArea.Credits;
					}
					else
					{
						_nextScreen = ShellArea.Root;
					}

					_state = _nextState;
				} 
				else if(_nextState == ShellState.Connecting)
				{
					idLog.Warning("TODO: nextState connecting");

					/*ShowPacifier( "#str_dlg_connecting" );*/
					_state = _nextState;
				} 
				else if(_nextState == ShellState.Searching)
				{
					idLog.Warning("TODO: nextState searching");

					/*ShowPacifier( "#str_online_mpstatus_searching" );*/
					_state = _nextState;
				}
			}

			if(_activeScreen != _nextScreen)
			{
				ClearWidgetActionRepeater();
				UpdateBackgroundState();

				if(_nextScreen == ShellArea.Invalid)
				{
					idLog.Warning("TODO: invalid shell area");
					// TODO: 
					/*if ( activeScreen > SHELL_AREA_INVALID && activeScreen < SHELL_NUM_AREAS && menuScreens[ activeScreen ] != NULL ) {
						menuScreens[ activeScreen ]->HideScreen( static_cast<mainMenuTransition_t>(transition) );
					}*/
					
					/*if ( cmdBar != NULL ) {
						cmdBar->ClearAllButtons();
						cmdBar->Update();
					}*/

					/*idSWFSpriteInstance * bg = gui->GetRootObject().GetNestedSprite( "pause_bg" );
					idSWFSpriteInstance * edging = gui->GetRootObject().GetNestedSprite( "_fullscreen" );*/
			
					/*if ( bg != NULL )  {
						bg->PlayFrame( "rollOff" );
					}

					if ( edging != NULL ) {
						edging->PlayFrame( "rollOff" );
					}*/
				} 
				else
				{
					if((_activeScreen > ShellArea.Invalid) && (_activeScreen < ShellArea.AreaCount) && (_menuScreens[(int) _activeScreen] != null))
					{
						_menuScreens[(int) _activeScreen].HideScreen(_transition);
					}

					if((_nextScreen > ShellArea.Invalid) && (_nextScreen < ShellArea.AreaCount) && (_menuScreens[(int) _nextScreen] != null))
					{
						_menuScreens[(int) _nextScreen].UpdateCommands();
						_menuScreens[(int) _nextScreen].ShowScreen(_transition);
					}
				}

				_transition   = MainMenuTransition.Invalid;
				_activeScreen = _nextScreen;
			}

			if((_cmdBar != null) && (_cmdBar.Sprite != null))
			{
				IDialog systemDialog = idEngine.Instance.GetService<IDialog>();

				if(systemDialog.IsActive == true)
				{
					_cmdBar.Sprite.IsVisible = false;
				} 
				else 
				{
					_cmdBar.Sprite.IsVisible = true;
				}
			}

			base.Update();

			if((_activeScreen == _nextScreen) && (_activeScreen == ShellArea.LeaderBoards))
			{
				idLog.Warning("TODO: active screen leaderboards");
				
				/*idMenuScreen_Shell_Leaderboards * screen = dynamic_cast< idMenuScreen_Shell_Leaderboards * >( menuScreens[ SHELL_AREA_LEADERBOARDS ] );
				if ( screen != NULL ) {
					screen->PumpLBCache();
					screen->RefreshLeaderboard();
				}*/
			}
			else if((_activeScreen == _nextScreen) && (_activeScreen == ShellArea.PartyLobby))
			{
				idLog.Warning("TODO: active screen party lobby");
				
				/*idMenuScreen_Shell_PartyLobby * screen = dynamic_cast< idMenuScreen_Shell_PartyLobby * >( menuScreens[ SHELL_AREA_PARTY_LOBBY ] );
				if ( screen != NULL ) {
					screen->UpdateLobby();
				}*/
			}
			else if((_activeScreen == _nextScreen) && (_activeScreen == ShellArea.GameLobby))
			{
				idLog.Warning("TODO: active screen game lobby");

				/*if ( session->GetActingGameStateLobbyBase().IsHost() ) {

					if ( timeRemaining <= 0 && state != SHELL_STATE_IN_GAME ) {
						session->StartMatch();
						state = SHELL_STATE_IN_GAME;
					}

					idMatchParameters matchParameters = session->GetActivePlatformLobbyBase().GetMatchParms();
					if ( !MatchTypeIsPrivate( matchParameters.matchFlags ) ) {
						if ( Sys_Milliseconds() >= nextPeerUpdateMs ) {
							nextPeerUpdateMs = Sys_Milliseconds() + PEER_UPDATE_INTERVAL;
							byte buffer[ 128 ];
							idBitMsg msg;
							msg.InitWrite( buffer, sizeof( buffer ) );
							msg.WriteLong( timeRemaining );
							session->GetActingGameStateLobbyBase().SendReliable( GAME_RELIABLE_MESSAGE_LOBBY_COUNTDOWN, msg, false );
						}
					}
				}

				idMenuScreen_Shell_GameLobby * screen = dynamic_cast< idMenuScreen_Shell_GameLobby * >( menuScreens[ SHELL_AREA_GAME_LOBBY ] );
				if ( screen != NULL ) {
					screen->UpdateLobby();
				}*/
			}

			if((_introGui != null) && (_introGui.IsActive == true))
			{
				idLog.Warning("TODO: introGui->Render( renderSystem, Sys_Milliseconds() );");
			}

			if(_continueWaitForEnumerate == true)
			{
				idLog.Warning("TODO: continueWaitForEnumerate");
	
				/*if ( !session->GetSaveGameManager().IsWorking() ) {
					continueWaitForEnumerate = false;
					common->Dialog().ClearDialog( GDM_REFRESHING );
					idMenuScreen_Shell_Singleplayer * screen = dynamic_cast< idMenuScreen_Shell_Singleplayer * >( menuScreens[ SHELL_AREA_CAMPAIGN ] );
					if ( screen != NULL ) {
						screen->ContinueGame();
					}
				}*/
			}
		}

		private void ShowLogo(bool show)
		{
			if(_gui == null)
			{
				return;
			}

			if(show == _backgroundShowing)
			{
				return;
			}

			idSWFSpriteInstance logo = _gui.RootObject.GetNestedSprite("logoInfo");
			idSWFSpriteInstance bg   = _gui.RootObject.GetNestedSprite("background");

			if((logo != null) && (bg != null))
			{
				bg.StereoDepth = StereoDepthType.Mid;

				if((show == true) && (_backgroundShowing == false))
				{
					logo.PlayFrame("rollOn");
					bg.PlayFrame("rollOff");
				}
				else if((show == false) && (_backgroundShowing == true))
				{
					logo.PlayFrame("rollOff");
					bg.PlayFrame("rollOn");
				}
			}

			_backgroundShowing = show;
		}

		private void UpdateBackgroundState()
		{
			if(_smallFrameShowing == true)
			{
				if((_nextScreen != ShellArea.Playstation) && (_nextScreen != ShellArea.Settings) 
					&& (_nextScreen != ShellArea.Campaign) && (_nextScreen != ShellArea.Developer))
				{
					if((_nextScreen != ShellArea.Resolution) && (_nextScreen != ShellArea.GamePad) 
						&& (_nextScreen != ShellArea.Difficulty) && (_nextScreen != ShellArea.SystemOptions) 
						&& (_nextScreen != ShellArea.GameOptions) && (_nextScreen != ShellArea.NewGame) 
						&& (_nextScreen != ShellArea.Stereoscopics) && (_nextScreen != ShellArea.Controls))
					{
						idLog.Warning("TODO: ShowSmallFrame(false);");
					}
				}
			} 
			else 
			{
				if((_nextScreen == ShellArea.Resolution) || (_nextScreen == ShellArea.GamePad) 
					|| (_nextScreen == ShellArea.Playstation) || (_nextScreen == ShellArea.Settings) 
					|| (_nextScreen == ShellArea.Campaign) || (_nextScreen == ShellArea.Controls) 
					|| (_nextScreen == ShellArea.Developer) || (_nextScreen == ShellArea.Difficulty))
				{
					idLog.Warning("TODO: ShowSmallFrame(true);");
				}		
			}
			
			if(_largeFrameShowing == true)
			{
				if((_nextScreen != ShellArea.PartyLobby) && (_nextScreen != ShellArea.GameLobby) 
					&& (_nextScreen != ShellArea.ControllerLayout) && (_nextScreen != ShellArea.Keyboard) 
					&& (_nextScreen != ShellArea.LeaderBoards) && (_nextScreen != ShellArea.MatchSettings) 
					&& (_nextScreen != ShellArea.ModeSelect) && (_nextScreen != ShellArea.Browser) 
					&& (_nextScreen != ShellArea.Load) && (_nextScreen != ShellArea.Save) && (_nextScreen != ShellArea.Credits))
				{
					idLog.Warning("TODO: ShowMPFrame(false);");
				}
			} 
			else 
			{
				if((_nextScreen == ShellArea.PartyLobby) || (_nextScreen == ShellArea.ControllerLayout) 
					|| (_nextScreen == ShellArea.GameLobby) || (_nextScreen == ShellArea.Keyboard) 
					|| (_nextScreen == ShellArea.LeaderBoards) || (_nextScreen == ShellArea.MatchSettings) 
					|| (_nextScreen == ShellArea.ModeSelect) || (_nextScreen == ShellArea.Browser) 
					|| (_nextScreen == ShellArea.Load) || (_nextScreen == ShellArea.Save) || (_nextScreen == ShellArea.Credits))
				{
					idLog.Warning("TODO: ShowMPFrame(true);");
				}
			}
			
			if((_smallFrameShowing == true) || (_largeFrameShowing == true) || (_nextScreen == ShellArea.Start))
			{
				ShowLogo(false);
			}
 			else
			{
				ShowLogo(true);
			}
		}
		#endregion
		#endregion
	}

	public enum ShellState
	{
		Invalid = -1,
		PressStart,
		Idle,
		PartyLobby,
		GameLobby,
		Paused,
		Connecting,
		Searching,
		Loading,
		Busy,
		InGame
	}

	public enum ShellArea
	{
		Invalid = -1,
		Start,
		Root,
		Developer,
		Campaign,
		Load,
		Save,
		NewGame,
		GameOptions,
		SystemOptions,
		Multiplayer,
		GameLobby,
		Stereoscopics,
		PartyLobby,
		Settings,
		Audio,
		Video,
		Keyboard,
		Controls,
		ControllerLayout,
		GamePad,
		Pause,
		LeaderBoards,
		Playstation,
		Difficulty,
		Resolution,
		MatchSettings,
		ModeSelect,
		Browser,
		Credits,
		AreaCount
	}

	public enum ShellCommand
	{
		Demo0,
		Demo1,
		Developer,
		Campaign,
		Multiplayer,
		Settings,
		Credits,
		Quit
	}
}