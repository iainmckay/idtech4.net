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
using idTech4.UI.SWF;

namespace idTech4.Game.Menus
{
	public class idMenuHandler_Shell : idMenuHandler
	{
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
		idList<mpMap_t, TAG_IDLIB_LIST_MENU>			mpGameMaps;
		idMenuWidget_MenuBar *	menuBar;
		idMenuWidget *			pacifier;*/
	
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
		// TODO: idList< idStr, TAG_IDLIB_LIST_MENU>			navOptions;
		#endregion

		#region Constructor
		public idMenuHandler_Shell()
			: base()
		{
			_state             = ShellState.Invalid;
			_nextState         = ShellState.Invalid;
			_backgroundShowing = true;
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
						idLog.Warning("TODO: idSWFSpriteInstance mars = _gui.RootObject.GetNestedSprite(\"mars\");");
						idSWFSpriteInstance mars = null;

						if(mars != null)
						{
							idLog.Warning("TODO: ActivateMenu mars");
				
							/*mars->stereoDepth = STEREO_DEPTH_TYPE_FAR;

							idSWFSpriteInstance * planet = mars->GetScriptObject()->GetNestedSprite( "planet" );

							if ( marsRotation != NULL && planet != NULL ) {
								const idMaterial * mat = marsRotation;
								if ( mat != NULL ) {
									int c = mat->GetNumStages();
									for ( int i = 0; i < c; i++ ) {
										const shaderStage_t *stage = mat->GetStage( i );
										if ( stage != NULL && stage->texture.cinematic ) {
											stage->texture.cinematic->ResetTime( Sys_Milliseconds() );
										}
									}
								}

								planet->SetMaterial( mat );
							}*/
						}
					}
				}

				idLog.Warning("TODO: SetupPCOptions();");
		
				idLog.Warning("TODO: ActivateMenu cmdBar");
				
				/*if ( cmdBar != NULL ) {
					cmdBar->ClearAllButtons();
					cmdBar->Update();
				}*/
			}
			else
			{
				idLog.Warning("TODO: ClearWidgetActionRepeater();");
				/*nextScreen = SHELL_AREA_INVALID;
				activeScreen = SHELL_AREA_INVALID;
				nextState = SHELL_STATE_INVALID;*/

				_state             = ShellState.Invalid;
				_smallFrameShowing = false;
				_largeFrameShowing = false;
				_backgroundShowing = true;

				idLog.Warning("TODO: common->Dialog().ClearDialog( GDM_LEAVE_LOBBY_RET_NEW_PARTY );");
			}
		}

		public override void Update()
		{			
//#if defined ( ID_360 )
//	if ( deviceRequestedSignal.Wait( 0 ) ) {
//		// This clears the delete save dialog to catch the case of a delete confirmation for an old device after we've changed the device.
//		common->Dialog().ClearDialog( GDM_DELETE_SAVE );
//		common->Dialog().ClearDialog( GDM_DELETE_CORRUPT_SAVEGAME );
//		common->Dialog().ClearDialog( GDM_RESTORE_CORRUPT_SAVEGAME );
//		common->Dialog().ClearDialog( GDM_LOAD_DAMAGED_FILE );
//		common->Dialog().ClearDialog( GDM_OVERWRITE_SAVE );
//
//	}
//#endif
			if((_gui == null) || (_gui.IsActive == false))
			{
				return;
			}

			// TODO: widget
			/*if ( ( IsPacifierVisible() || common->Dialog().IsDialogActive() ) && actionRepeater.isActive ) {
				ClearWidgetActionRepeater();
			} */

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
					// TODO: HidePacifier();

					_nextScreen = ShellArea.Start;
					_transition = MainMenuTransition.Simple;
					_state      = _nextState;

					// TODO: menuBar
					/*if ( menuBar != NULL && gui != NULL ) {			
						menuBar->ClearSprite();
					}*/
				} 
				else if(_nextState == ShellState.Idle)
				{
					idLog.Warning("TODO: nextState idle");

					/*HidePacifier();
					if ( nextScreen == SHELL_AREA_START || nextScreen == SHELL_AREA_PARTY_LOBBY || nextScreen == SHELL_AREA_GAME_LOBBY || nextScreen == SHELL_AREA_INVALID )  {
						nextScreen = SHELL_AREA_ROOT;
					}

					if ( menuBar != NULL && gui != NULL ) {			
						idSWFScriptObject & root = gui->GetRootObject();
						menuBar->BindSprite( root );
						SetupPCOptions();
					}
					transition = MENU_TRANSITION_SIMPLE;
					state = nextState;*/
				} 
				else if(_nextState == ShellState.PartyLobby)
				{
					idLog.Warning("TODO: nextState party lobby");

					/*HidePacifier();
					nextScreen = SHELL_AREA_PARTY_LOBBY;
					transition = MENU_TRANSITION_SIMPLE;
					state = nextState;*/
				} 
				else if(_nextState == ShellState.GameLobby)
				{
					idLog.Warning("TODO: nextState game lobby");
					
					/*HidePacifier();
					if ( state != SHELL_STATE_IN_GAME ) {
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
					idLog.Warning("TODO: nextState paused");

					/*HidePacifier();
					transition = MENU_TRANSITION_SIMPLE;

					if ( gameComplete ) {
						nextScreen = SHELL_AREA_CREDITS;
					} else {
						nextScreen = SHELL_AREA_ROOT;
					}

					state = nextState;*/
				} 
				else if(_nextState == ShellState.Connecting)
				{
					idLog.Warning("TODO: nextState connecting");

					/*ShowPacifier( "#str_dlg_connecting" );
					state = nextState;*/
				} 
				else if(_nextState == ShellState.Searching)
				{
					idLog.Warning("TODO: nextState searching");

					/*ShowPacifier( "#str_online_mpstatus_searching" );
					state = nextState;*/
				}
			}

			if(_activeScreen != _nextScreen)
			{
				// TODO: ClearWidgetActionRepeater();
				UpdateBackgroundState();

				if(_nextScreen == ShellArea.Invalid)
				{
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

					/*if ( activeScreen > SHELL_AREA_INVALID && activeScreen < SHELL_NUM_AREAS && menuScreens[ activeScreen ] != NULL ) {
						menuScreens[ activeScreen ]->HideScreen( static_cast<mainMenuTransition_t>(transition) );
					}

					if ( nextScreen > SHELL_AREA_INVALID && nextScreen < SHELL_NUM_AREAS && menuScreens[ nextScreen ] != NULL ) {
						menuScreens[ nextScreen ]->UpdateCmds();
						menuScreens[ nextScreen ]->ShowScreen( static_cast<mainMenuTransition_t>(transition) );			
					}*/
				}

				_transition   = MainMenuTransition.Invalid;
				_activeScreen = _nextScreen;
			}

			// TODO cmdBar

			/*if ( cmdBar != NULL && cmdBar->GetSprite() ) {
				if ( common->Dialog().IsDialogActive() ) {		
					cmdBar->GetSprite()->SetVisible( false );
				} else {
					cmdBar->GetSprite()->SetVisible( true );
				}
			}*/

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

		private void UpdateBackgroundState()
		{
			if(_smallFrameShowing == true)
			{
				if((_nextScreen != ShellArea.Playstation) && (_nextScreen != ShellArea.Settings) && (_nextScreen != ShellArea.Campaign) && (_nextScreen != ShellArea.Dev))
				{
					if((_nextScreen != ShellArea.Resolution) && (_nextScreen != ShellArea.GamePad) && (_nextScreen != ShellArea.Difficulty) && (_nextScreen != ShellArea.SystemOptions) && (_nextScreen != ShellArea.GameOptions) && (_nextScreen != ShellArea.NewGame) && (_nextScreen != ShellArea.Stereoscopics)
						&& (_nextScreen != ShellArea.Controls))
					{
						idLog.Warning("TODO: ShowSmallFrame(false);");
					}
				}
			} 
			else 
			{
				if((_nextScreen == ShellArea.Resolution) || (_nextScreen == ShellArea.GamePad) || (_nextScreen == ShellArea.Playstation) || (_nextScreen == ShellArea.Settings) || (_nextScreen == ShellArea.Campaign) || (_nextScreen == ShellArea.Controls) || (_nextScreen == ShellArea.Dev) || (_nextScreen == ShellArea.Difficulty))
				{
					idLog.Warning("TODO: ShowSmallFrame(true);");
				}		
			}

			if(_largeFrameShowing == true)
			{
				if((_nextScreen != ShellArea.PartyLobby) && (_nextScreen != ShellArea.GameLobby) && (_nextScreen != ShellArea.ControllerLayout) && (_nextScreen != ShellArea.Keyboard) && (_nextScreen != ShellArea.LeaderBoards) && (_nextScreen != ShellArea.MatchSettings) && (_nextScreen != ShellArea.ModeSelect) &&
					(_nextScreen != ShellArea.Browser) && (_nextScreen != ShellArea.Load) && (_nextScreen != ShellArea.Save) && (_nextScreen != ShellArea.Credits))
				{
					idLog.Warning("TODO: ShowMPFrame(false);");
				}
			} 
			else 
			{
				if((_nextScreen == ShellArea.PartyLobby) || (_nextScreen == ShellArea.ControllerLayout) || (_nextScreen == ShellArea.GameLobby) || (_nextScreen == ShellArea.Keyboard) || (_nextScreen == ShellArea.LeaderBoards) || (_nextScreen == ShellArea.MatchSettings) || (_nextScreen == ShellArea.ModeSelect) ||
					(_nextScreen == ShellArea.Browser) || (_nextScreen == ShellArea.Load) || (_nextScreen == ShellArea.Save) || (_nextScreen == ShellArea.Credits))
				{
					idLog.Warning("TODO: ShowMPFrame(true);");
				}
			}

			if((_smallFrameShowing == true) || (_largeFrameShowing ==true) || (_nextScreen == ShellArea.Start))
			{
				idLog.Warning("TODO: ShowLogo(false);");
			}
 			else
			{
				idLog.Warning("TODO: ShowLogo(true);");
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
		Dev,
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
		Credits
	}
}