/*
===========================================================================

Doom 3 GPL Source Code
Copyright (C) 1999-2011 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 GPL Source Code (?Doom 3 Source Code?).  

Doom 3 Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Input;
using idTech4.Renderer;
using idTech4.UI;

namespace idTech4
{
	/// <summary>
	/// The session is the glue that holds games together between levels.
	/// </summary>
	public sealed class idSession
	{
		#region Properties
		public bool IsMultiplayer
		{
			get
			{
				return idE.AsyncNetwork.IsActive;
			}
		}

		public int LocalClientIndex
		{
			get
			{
				if(idE.AsyncNetwork.Client.IsActive == true)
				{
					idConsole.Warning("TODO: return idAsyncNetwork::client.GetLocalClientNum();");
				}
				else if(idE.AsyncNetwork.Server.IsActive == true)
				{
					idConsole.Warning("TODO: serverDrawClient");

					if(idE.CvarSystem.GetInteger("net_serverDedicated") == 0)
					{
						return 0;
					}

					
		/*} else if ( idAsyncNetwork::server.IsClientInGame( idAsyncNetwork::serverDrawClient.GetInteger() ) ) {
			return idAsyncNetwork::serverDrawClient.GetInteger();
		} */
					else
					{
						return -1;
					}
				}
				
				return 0;
			}
		}
		#endregion

		#region Members
		// render and sound world to use for this session
		private idRenderWorld _renderWorld;

		private idUserInterface _guiActive;

		//HandleGuiCommand_t guiHandle;

		private idUserInterface _guiInGame;
		private idUserInterface _guiMainMenu;
		// TODO: idListGUI* guiMainMenu_MapList;		// easy map list handling
		private idUserInterface _guiRestartMenu;
		private idUserInterface _guiLoading;
		private idUserInterface _guiIntro;
		private idUserInterface _guiGameOver;
		private idUserInterface _guiTest;
		private idUserInterface _guiTakeNotes;

		private idUserInterface _guiMsg;
		private idUserInterface _guiMsgRestore;			// store the calling GUI for restore

		private idMaterial _whiteMaterial;

		private idMaterial _wipeMaterial;
		private int _wipeStartTic;
		private int _wipeStopTic;
		private bool _wipeHold;

		private bool _insideExecuteMapChange;
		private bool _insideScreenUpdate;

		private bool _loadingSaveGame;	// currently loading map from a SaveGame
		/*idFile* savegameFile;		// this is the savegame file to load from
		int savegameVersion;*/

		// from serverInfo
		private int _clientCount;

		// watchdog to force the main menu to restart
		private int _emptyDrawCount;

		// this is the information required to be set before ExecuteMapChange() is called,
		// which can be saved off at any time with the following commands so it can all be played back
		private MapSpawnData _mapSpawnData = new MapSpawnData();

		private string _currentMapName; // for checking reload on same level
		private bool _mapSpawned; // cleared on Stop()
		#endregion

		#region Constructor
		public idSession()
		{
			new idCvar("com_showAngles", "0", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("com_minTics", "1", "", CvarFlags.System);
			new idCvar("com_showTics", "0", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("com_fixedTic", "0", 0, 10, "", CvarFlags.System | CvarFlags.Integer);
			new idCvar("com_showDemo", "0", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("com_skipGameDraw", "0", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("com_aviDemoSamples", "16", "", CvarFlags.System);
			new idCvar("com_aviDemoWidth", "256", "", CvarFlags.System);
			new idCvar("com_aviDemoHeight", "256", "", CvarFlags.System);
			new idCvar("com_aviDemoTics", "2", 1, 60, "", CvarFlags.System | CvarFlags.Integer);
			new idCvar("com_wipeSeconds", "1", "", CvarFlags.System);
			new idCvar("com_guid", "", "", CvarFlags.System | CvarFlags.Archive | CvarFlags.ReadOnly);

			ClearWipe();
		}
		#endregion

		#region Methods
		#region Public
		public void Frame()
		{
			// TODO: Sound
			/*if ( com_asyncSound.GetInteger() == 0 ) {
				soundSystem->AsyncUpdate( Sys_Milliseconds() );
			}*/

			// if the console is down, we don't need to hold
			// the mouse cursor
			/* TODO: if ( console->Active() || com_editorActive ) {
				Sys_GrabMouseCursor( false );
			} else {
				Sys_GrabMouseCursor( true );
			}*/

			// save the screenshot and audio from the last draw if needed
			/* TODO: if ( aviCaptureMode ) {
				idStr	name;

				name = va("demos/%s/%s_%05i.tga", aviDemoShortName.c_str(), aviDemoShortName.c_str(), aviTicStart );

				float ratio = 30.0f / ( 1000.0f / USERCMD_MSEC / com_aviDemoTics.GetInteger() );
				aviDemoFrameCount += ratio;
				if ( aviTicStart + 1 != ( int )aviDemoFrameCount ) {
					// skipped frames so write them out
					int c = aviDemoFrameCount - aviTicStart;
					while ( c-- ) {
						renderSystem->TakeScreenshot( com_aviDemoWidth.GetInteger(), com_aviDemoHeight.GetInteger(), name, com_aviDemoSamples.GetInteger(), NULL );
						name = va("demos/%s/%s_%05i.tga", aviDemoShortName.c_str(), aviDemoShortName.c_str(), ++aviTicStart );
					}
				}
				aviTicStart = aviDemoFrameCount;

				// remove any printed lines at the top before taking the screenshot
				console->ClearNotifyLines();

				// this will call Draw, possibly multiple times if com_aviDemoSamples is > 1
				renderSystem->TakeScreenshot( com_aviDemoWidth.GetInteger(), com_aviDemoHeight.GetInteger(), name, com_aviDemoSamples.GetInteger(), NULL );
			}*/

			// at startup, we may be backwards
			/*TODO: if ( latchedTicNumber > com_ticNumber ) {
				latchedTicNumber = com_ticNumber;
			}

			// see how many tics we should have before continuing
			int	minTic = latchedTicNumber + 1;
			if ( com_minTics.GetInteger() > 1 ) {
				minTic = lastGameTic + com_minTics.GetInteger();
			}
	
			if ( readDemo ) {
				if ( !timeDemo && numDemoFrames != 1 ) {
					minTic = lastDemoTic + USERCMD_PER_DEMO_FRAME;
				} else {
					// timedemos and demoshots will run as fast as they can, other demos
					// will not run more than 30 hz
					minTic = latchedTicNumber;
				}
			} else if ( writeDemo ) {
				minTic = lastGameTic + USERCMD_PER_DEMO_FRAME;		// demos are recorded at 30 hz
			}
	
			// fixedTic lets us run a forced number of usercmd each frame without timing
			if ( com_fixedTic.GetInteger() ) {
				minTic = latchedTicNumber;
			}

			// FIXME: deserves a cleanup and abstraction
		#if defined( _WIN32 )
			// Spin in place if needed.  The game should yield the cpu if
			// it is running over 60 hz, because there is fundamentally
			// nothing useful for it to do.
			while( 1 ) {
				latchedTicNumber = com_ticNumber;
				if ( latchedTicNumber >= minTic ) {
					break;
				}
				Sys_Sleep( 1 );
			}
		#else
			while( 1 ) {
				latchedTicNumber = com_ticNumber;
				if ( latchedTicNumber >= minTic ) {
					break;
				}
				Sys_WaitForEvent( TRIGGER_EVENT_ONE );
			}
		#endif*/

			// send frame and mouse events to active guis
			GuiFrameEvents();

			// advance demos
			/* TODO: if ( readDemo ) {
				AdvanceRenderDemo( false );
				return;
			}*/

			//------------ single player game tics --------------
			if((_mapSpawned == false) || (_guiActive != null))
			{
				if(idE.CvarSystem.GetBool("com_asyncInput") == false)
				{
					// early exit, won't do RunGameTic .. but still need to update mouse position for GUIs
					idE.UserCommandGenerator.GetDirectCommand();
				}
			}

			if(_mapSpawned == false)
			{
				return;
			}

			if(_guiActive != null)
			{
				// TODO: lastGameTic = latchedTicNumber;
				return;
			}

			idConsole.Warning("TODO: REST OF FRAME");
			/*// in message box / GUIFrame, idSessionLocal::Frame is used for GUI interactivity
			// but we early exit to avoid running game frames
			if ( idAsyncNetwork::IsActive() ) {
				return;
			}

			// check for user info changes
			if ( cvarSystem->GetModifiedFlags() & CVAR_USERINFO ) {
				mapSpawnData.userInfo[0] = *cvarSystem->MoveCVarsToDict( CVAR_USERINFO );
				game->SetUserInfo( 0, mapSpawnData.userInfo[0], false, false );
				cvarSystem->ClearModifiedFlags( CVAR_USERINFO );
			}

			// see how many usercmds we are going to run
			int	numCmdsToRun = latchedTicNumber - lastGameTic;

			// don't let a long onDemand sound load unsync everything
			if ( timeHitch ) {
				int	skip = timeHitch / USERCMD_MSEC;
				lastGameTic += skip;
				numCmdsToRun -= skip;
				timeHitch = 0;
			}

			// don't get too far behind after a hitch
			if ( numCmdsToRun > 10 ) {
				lastGameTic = latchedTicNumber - 10;
			}

			// never use more than USERCMD_PER_DEMO_FRAME,
			// which makes it go into slow motion when recording
			if ( writeDemo ) {
				int fixedTic = USERCMD_PER_DEMO_FRAME;
				// we should have waited long enough
				if ( numCmdsToRun < fixedTic ) {
					common->Error( "idSessionLocal::Frame: numCmdsToRun < fixedTic" );
				}
				// we may need to dump older commands
				lastGameTic = latchedTicNumber - fixedTic;
			} else if ( com_fixedTic.GetInteger() > 0 ) {
				// this may cause commands run in a previous frame to
				// be run again if we are going at above the real time rate
				lastGameTic = latchedTicNumber - com_fixedTic.GetInteger();
			} else if (	aviCaptureMode ) {
				lastGameTic = latchedTicNumber - com_aviDemoTics.GetInteger();
			}

			// force only one game frame update this frame.  the game code requests this after skipping cinematics
			// so we come back immediately after the cinematic is done instead of a few frames later which can
			// cause sounds played right after the cinematic to not play.
			if ( syncNextGameFrame ) {
				lastGameTic = latchedTicNumber - 1;
				syncNextGameFrame = false;
			}

			// create client commands, which will be sent directly
			// to the game
			if ( com_showTics.GetBool() ) {
				common->Printf( "%i ", latchedTicNumber - lastGameTic );
			}

			int	gameTicsToRun = latchedTicNumber - lastGameTic;
			int i;
			for ( i = 0 ; i < gameTicsToRun ; i++ ) {
				RunGameTic();
				if ( !mapSpawned ) {
					// exited game play
					break;
				}
				if ( syncNextGameFrame ) {
					// long game frame, so break out and continue executing as if there was no hitch
					break;
				}*/
		}

		public void GuiFrameEvents()
		{
			// stop generating move and button commands when a local console or menu is active
			// running here so SP, async networking and no game all go through it
			// TODO:
			/*if ( console->Active() || guiActive ) {
				usercmdGen->InhibitUsercmd( INHIBIT_SESSION, true );
			} else {
				usercmdGen->InhibitUsercmd( INHIBIT_SESSION, false );
			}*/

			idUserInterface gui;

			if(_guiTest != null)
			{
				gui = _guiTest;
			}
			else if(_guiActive != null)
			{
				gui = _guiActive;
			}
			else
			{
				return;
			}

			string cmd = gui.HandleEvent(new SystemEvent(SystemEventType.None), idE.System.FrameTime);

			if(cmd != string.Empty)
			{
				idConsole.Warning("TODO: DispatchCommand(guiActive, cmd);");
			}
		}

		/// <summary>
		/// Called in an orderly fashion at system startup, so commands, cvars, files, etc are all available.
		/// </summary>
		public void Init()
		{
			idConsole.WriteLine("-------- Initializing Session --------");

			// TODO: commands
			/*cmdSystem->AddCommand( "writePrecache", Sess_WritePrecache_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "writes precache commands" );

	#ifndef	ID_DEDICATED*/

			idE.CmdSystem.AddCommand("map", "loads a map", CommandFlags.System, Cmd_Map /* TODO: idCmdSystem::ArgCompletion_MapName*/);

		/*cmdSystem->AddCommand( "devmap", Session_DevMap_f, CMD_FL_SYSTEM, "loads a map in developer mode", idCmdSystem::ArgCompletion_MapName );
		cmdSystem->AddCommand( "testmap", Session_TestMap_f, CMD_FL_SYSTEM, "tests a map", idCmdSystem::ArgCompletion_MapName );

		cmdSystem->AddCommand( "writeCmdDemo", Session_WriteCmdDemo_f, CMD_FL_SYSTEM, "writes a command demo" );
		cmdSystem->AddCommand( "playCmdDemo", Session_PlayCmdDemo_f, CMD_FL_SYSTEM, "plays back a command demo" );
		cmdSystem->AddCommand( "timeCmdDemo", Session_TimeCmdDemo_f, CMD_FL_SYSTEM, "times a command demo" );
		cmdSystem->AddCommand( "exitCmdDemo", Session_ExitCmdDemo_f, CMD_FL_SYSTEM, "exits a command demo" );
		cmdSystem->AddCommand( "aviCmdDemo", Session_AVICmdDemo_f, CMD_FL_SYSTEM, "writes AVIs for a command demo" );
		cmdSystem->AddCommand( "aviGame", Session_AVIGame_f, CMD_FL_SYSTEM, "writes AVIs for the current game" );

		cmdSystem->AddCommand( "recordDemo", Session_RecordDemo_f, CMD_FL_SYSTEM, "records a demo" );
		cmdSystem->AddCommand( "stopRecording", Session_StopRecordingDemo_f, CMD_FL_SYSTEM, "stops demo recording" );
		cmdSystem->AddCommand( "playDemo", Session_PlayDemo_f, CMD_FL_SYSTEM, "plays back a demo", idCmdSystem::ArgCompletion_DemoName );
		cmdSystem->AddCommand( "timeDemo", Session_TimeDemo_f, CMD_FL_SYSTEM, "times a demo", idCmdSystem::ArgCompletion_DemoName );
		cmdSystem->AddCommand( "timeDemoQuit", Session_TimeDemoQuit_f, CMD_FL_SYSTEM, "times a demo and quits", idCmdSystem::ArgCompletion_DemoName );
		cmdSystem->AddCommand( "aviDemo", Session_AVIDemo_f, CMD_FL_SYSTEM, "writes AVIs for a demo", idCmdSystem::ArgCompletion_DemoName );
		cmdSystem->AddCommand( "compressDemo", Session_CompressDemo_f, CMD_FL_SYSTEM, "compresses a demo file", idCmdSystem::ArgCompletion_DemoName );
	#endif

		cmdSystem->AddCommand( "disconnect", Session_Disconnect_f, CMD_FL_SYSTEM, "disconnects from a game" );

		cmdSystem->AddCommand( "demoShot", Session_DemoShot_f, CMD_FL_SYSTEM, "writes a screenshot for a demo" );
		cmdSystem->AddCommand( "testGUI", Session_TestGUI_f, CMD_FL_SYSTEM, "tests a gui" );

	#ifndef	ID_DEDICATED
		cmdSystem->AddCommand( "saveGame", SaveGame_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "saves a game" );
		cmdSystem->AddCommand( "loadGame", LoadGame_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "loads a game", idCmdSystem::ArgCompletion_SaveGame );
	#endif

		cmdSystem->AddCommand( "takeViewNotes", TakeViewNotes_f, CMD_FL_SYSTEM, "take notes about the current map from the current view" );
		cmdSystem->AddCommand( "takeViewNotes2", TakeViewNotes2_f, CMD_FL_SYSTEM, "extended take view notes" );

		cmdSystem->AddCommand( "rescanSI", Session_RescanSI_f, CMD_FL_SYSTEM, "internal - rescan serverinfo cvars and tell game" );

		cmdSystem->AddCommand( "promptKey", Session_PromptKey_f, CMD_FL_SYSTEM, "prompt and sets the CD Key" );

		cmdSystem->AddCommand( "hitch", Session_Hitch_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "hitches the game" );*/

			// the same idRenderWorld will be used for all games
			// and demos, insuring that level specific models
			// will be freed
			_renderWorld = idE.RenderSystem.CreateRenderWorld();

			// TODO: sound
			/*sw = soundSystem->AllocSoundWorld( rw );

			menuSoundWorld = soundSystem->AllocSoundWorld( rw );*/

			// we have a single instance of the main menu
			_guiMainMenu = idE.UIManager.FindInterface("guis/mainmenu.gui", true, false, true);

			// TODO: map list
			/*guiMainMenu_MapList = uiManager->AllocListGUI();
			guiMainMenu_MapList->Config(guiMainMenu, "mapList");
			idAsyncNetwork::client.serverList.GUIConfig(guiMainMenu, "serverList");*/

			_guiRestartMenu = idE.UIManager.FindInterface("guis/restart.gui", true, false, true);
			_guiGameOver = idE.UIManager.FindInterface("guis/gameover.gui", true, false, true);
			_guiMsg = idE.UIManager.FindInterface("guis/msg.gui", true, false, true);
			_guiTakeNotes = idE.UIManager.FindInterface("guis/takeNotes.gui", true, false, true);
			_guiIntro = idE.UIManager.FindInterface("guis/intro.gui", true, false, true);

			_whiteMaterial = idE.DeclManager.FindMaterial("_white");

			idConsole.WriteLine("session initialized");
			idConsole.WriteLine("--------------------------------------");
		}

		public bool ProcessEvent(SystemEvent ev)
		{
			// hitting escape anywhere brings up the menu
			// TODO: process event
			/*if ( !guiActive && event->evType == SE_KEY && event->evValue2 == 1 && event->evValue == K_ESCAPE ) {
				console->Close();
				if ( game ) {
					idUserInterface	*gui = NULL;
					escReply_t		op;
					op = game->HandleESC( &gui );
					if ( op == ESC_IGNORE ) {
						return true;
					} else if ( op == ESC_GUI ) {
						SetGUI( gui, NULL );
						return true;
					}
				}
				StartMenu();
				return true;
			}

			// let the pull-down console take it if desired
			if ( console->ProcessEvent( event, false ) ) {
				return true;
			}

			// if we are testing a GUI, send all events to it
			if ( guiTest ) {
				// hitting escape exits the testgui
				if ( event->evType == SE_KEY && event->evValue2 == 1 && event->evValue == K_ESCAPE ) {
					guiTest = NULL;
					return true;
				}
		
				static const char *cmd;
				cmd = guiTest->HandleEvent( event, com_frameTime );
				if ( cmd && cmd[0] ) {
					common->Printf( "testGui event returned: '%s'\n", cmd );
				}
				return true;
			}*/

			// menus / etc
			if(_guiActive != null)
			{
				ProcessMenuEvent(ev);
				return true;
			}

			idConsole.Warning("TODO: process event");
			// if we aren't in a game, force the console to take it
			/*if ( !mapSpawned ) {
				console->ProcessEvent( event, true );
				return true;
			}

			// in game, exec bindings for all key downs
			if ( event->evType == SE_KEY && event->evValue2 == 1 ) {
				idKeyInput::ExecKeyBinding( event->evValue );
				return true;
			}

			return false;*/

			return false;
		}

		public void SetUserInterface(idUserInterface ui, /* TODO: HandleGuiCommand_t*/ object handle)
		{
			_guiActive = ui;
			// TODO: guiHandle = handle;

			/*TODO: if ( guiMsgRestore ) {
				common->DPrintf( "idSessionLocal::SetGUI: cleared an active message box\n" );
				guiMsgRestore = NULL;
			}*/

			if(_guiActive == null)
			{
				return;
			}

			// TODO
			if(_guiActive == _guiMainMenu)
			{
				//SetSaveGameGuiVars();
				SetMainMenuVariables();
			}
			/* TODO: else if(guiActive == guiRestartMenu)
			{
				SetSaveGameGuiVars();
			}*/

			_guiActive.HandleEvent(new SystemEvent(SystemEventType.None), idE.System.FrameTime);
			_guiActive.Activate(true, idE.System.FrameTime);
		}

		/// <summary>
		/// Activates the main menu.
		/// </summary>
		/// <param name="playIntro"></param>
		public void StartMenu(bool playIntro = false)
		{
			if(_guiActive == _guiMainMenu)
			{
				return;
			}

			// TODO: demo
			/*if(readDemo)
			{
				// if we're playing a demo, esc kills it
				UnloadMap();
			}*/

			// pause the game sound world
			// TODO: sound
			/*if(sw != NULL && !sw->IsPaused())
			{
				sw->Pause();
			}*/

			// start playing the menu sounds
			// TODO: soundSystem->SetPlayingSoundWorld(menuSoundWorld);

			SetUserInterface(_guiMainMenu, null);
			playIntro = false;
			_guiMainMenu.HandleNamedEvent((playIntro == true) ? "playIntro" : "noIntro");


			/*// TODO: if(fileSystem->HasD3XP())
			{
				guiMainMenu->SetStateString("game_list", common->GetLanguageDict()->GetString("#str_07202"));
			}
			else*/
			{
				_guiMainMenu.State.Set("game_list", idE.Language.Get("#str_07212"));
			}

			idE.Console.Close();
		}

		public void StartNewGame(string mapName, bool devMap)
		{
#if ID_DEDICATED
			idConsole.WriteLine("Dedicated servers cannot start singleplayer games." );
#else
			
			if(idE.AsyncNetwork.Server.IsActive == true)
			{
				idConsole.WriteLine("Server running, use si_map / serverMapRestart");
				return;
			}

			if(idE.AsyncNetwork.Client.IsActive == true)
			{
				idConsole.WriteLine("Client running, disconnect from server first");
				return;
			}

			// clear the userInfo so the player starts out with the defaults
			_mapSpawnData.UserInformation[0].Clear();
			_mapSpawnData.PersistentPlayerInformation[0].Clear();						
			_mapSpawnData.ServerInformation.Clear();
			_mapSpawnData.SyncedCvars.Clear();

			idE.CvarSystem.CopyCvarsToDictionary(_mapSpawnData.UserInformation[0], CvarFlags.UserInfo);
			idE.CvarSystem.CopyCvarsToDictionary(_mapSpawnData.ServerInformation, CvarFlags.ServerInfo);
			idE.CvarSystem.CopyCvarsToDictionary(_mapSpawnData.SyncedCvars, CvarFlags.NetworkSync);

			_mapSpawnData.ServerInformation.Set("si_gameType", "singleplayer");

			// set the devmap key so any play testing items will be given at
			// spawn time to set approximately the right weapons and ammo
			if(devMap == true)
			{
				_mapSpawnData.ServerInformation.Set("devmap", "1");
			}
			
			MoveToNewMap(mapName);
#endif
		}

		public void UpdateScreen(bool outOfSequence = true)
		{
			if(_insideScreenUpdate == true)
			{
				return;
			}

			_insideScreenUpdate = true;

			idE.RenderSystem.BeginFrame(idE.RenderSystem.ScreenWidth, idE.RenderSystem.ScreenHeight);

			// draw everything
			Draw();

			// TODO: com_speeds
			/*if ( com_speeds.GetBool() ) {
				renderSystem->EndFrame( &time_frontend, &time_backend );
			} else {*/
			idE.RenderSystem.EndFrame();
			/*}*/

			_insideScreenUpdate = false;
		}
		#endregion

		#region Private
		private void ClearWipe()
		{
			_wipeHold = false;
			_wipeStopTic = 0;
			_wipeStartTic = _wipeStopTic + 1;
		}

		private void DispatchCommand(idUserInterface gui, string menuCommand)
		{
			DispatchCommand(gui, menuCommand, true);
		}

		private void DispatchCommand(idUserInterface gui, string menuCommand, bool doIngame)
		{
			if(gui == null)
			{
				gui = _guiActive;
			}

			if(gui == _guiMainMenu)
			{
				idConsole.Warning("TODO: HandleMainMenuCommands");
				// TODO: HandleMainMenuCommands(menuCommand);
			}

			// TODO: other menus
			/*else if ( gui == guiIntro) {
				HandleIntroMenuCommands( menuCommand );
			} else if ( gui == guiMsg ) {
				HandleMsgCommands( menuCommand );
			} else if ( gui == guiTakeNotes ) {
				HandleNoteCommands( menuCommand );
			} else if ( gui == guiRestartMenu ) {
				HandleRestartMenuCommands( menuCommand );
			} else if ( game && guiActive && guiActive->State().GetBool( "gameDraw" ) ) {
				const char *cmd = game->HandleGuiCommands( menuCommand );
				if ( !cmd ) {
					guiActive = NULL;
				} else if ( idStr::Icmp( cmd, "main" ) == 0 ) {
					StartMenu();
				} else if ( strstr( cmd, "sound " ) == cmd ) {
					// pipe the GUI sound commands not handled by the game to the main menu code
					HandleMainMenuCommands( cmd );
				}
			} else if ( guiHandle ) {
				if ( (*guiHandle)( menuCommand ) ) {
					return;
				}
			}*/

			else if(doIngame == false)
			{
				idConsole.DeveloperWriteLine("idSessionLocal.DispatchCommand: no dispatch found for command '{0}'", menuCommand);
			}
			else
			{
				// TODO: HandleInGameCommands( menuCommand );
			}
		}

		private void Draw()
		{			
			bool fullConsole = false;

			if(_insideExecuteMapChange == true)
			{
				if(_guiLoading != null)
				{
					_guiLoading.Draw(idE.System.FrameTime);
				}
				
				if(_guiActive == _guiMsg)
				{
					_guiMsg.Draw(idE.System.FrameTime);
				}
			}
			else if(_guiTest != null)
			{
				// if testing a gui, clear the screen and draw it
				// clear the background, in case the tested gui is transparent
				// NOTE that you can't use this for aviGame recording, it will tick at real com_frameTime between screenshots.
				idE.RenderSystem.Color = idColor.Black;
				idE.RenderSystem.DrawStretchPicture(0, 0, 640, 480, 0, 0, 1, 1, idE.DeclManager.FindMaterial("_white"));

				_guiTest.Draw(idE.System.FrameTime);
			}
			else if((_guiActive != null) && (_guiActive.State.GetBool("gameDraw") == false))
			{
				// draw the frozen gui in the background
				if((_guiActive == _guiMsg) && (_guiMsgRestore != null))
				{
					_guiMsgRestore.Draw(idE.System.FrameTime);
				}

				// draw the menus full screen
				if((_guiActive == _guiTakeNotes) && (idE.CvarSystem.GetBool("com_skipGameDraw") == false))
				{
					idE.Game.Draw(this.LocalClientIndex);
				}

				_guiActive.Draw(idE.System.FrameTime);
			}
			/*else if(readDemo)
			{
				rw->RenderScene(&currentDemoRenderView);
				renderSystem->DrawDemoPics();
			}*/
			else if(_mapSpawned == true)
			{
				bool gameDraw = false;

				// normal drawing for both single and multi player
				if((idE.CvarSystem.GetBool("com_skipGameDraw") == false) && (this.LocalClientIndex >= 0))
				{
					// draw the game view
					int start = idE.System.Milliseconds;
					gameDraw = idE.Game.Draw(this.LocalClientIndex);
					int end = idE.System.Milliseconds;

					// TODO: time_gameDraw += (end - start);	// note time used for com_speeds
				}

				if(gameDraw == false)
				{
					idE.RenderSystem.Color = idColor.Black;
					idE.RenderSystem.DrawStretchPicture(0, 0, 640, 480, 0, 0, 1, 1, idE.DeclManager.FindMaterial("_white"));
				}

				// save off the 2D drawing from the game
				// TODO: writedemo
				/*if(writeDemo)
				{
					renderSystem->WriteDemoPics();
				}*/
			}
			else
			{
				if(idE.CvarSystem.GetBool("com_allowConsole") == true)
				{
					idE.Console.Draw(true);
				}
				else
				{
					_emptyDrawCount++;

					if(_emptyDrawCount > 5)
					{
						// it's best if you can avoid triggering the watchgod by doing the right thing somewhere else
						idConsole.Warning("idSession: triggering mainmenu watchdog");
				
						_emptyDrawCount = 0;
						StartMenu();
					}
			
					idE.RenderSystem.Color = new Vector4(0, 0, 0, 1);
					idE.RenderSystem.DrawStretchPicture(0, 0, idE.VirtualScreenWidth, idE.VirtualScreenHeight, 0, 0, 1, 1, idE.DeclManager.FindMaterial("_white"));
				}

				fullConsole = true;
			}

			if((fullConsole == false) && (_emptyDrawCount > 0))
			{
				idConsole.DeveloperWriteLine("idSession: {0} empty frame draws", _emptyDrawCount);
				_emptyDrawCount = 0;
			}
	
			fullConsole = false;

			// draw the wipe material on top of this if it hasn't completed yet
			DrawWipeModel();

			// draw debug graphs
			// TODO: DrawCmdGraph();

			// draw the half console / notify console on top of everything
			if(fullConsole == false)
			{
				idE.Console.Draw(false);
			}
		}

		/// <summary>
		/// Draw the fade material over everything that has been drawn.
		/// </summary>
		private void DrawWipeModel()
		{
			int latchedTic = idE.System.TicNumber;

			if(_wipeStartTic >= _wipeStopTic)
			{
				return;
			}

			if((_wipeHold == false) && (latchedTic >= _wipeStopTic))
			{
				return;
			}

			float fade = (float) ((latchedTic - _wipeStartTic) / (_wipeStopTic - _wipeStartTic));

			idE.RenderSystem.Color = new Vector4(1, 1, 1, fade);
			idE.RenderSystem.DrawStretchPicture(0, 0, 640, 480, 0, 0, 1, 1, _wipeMaterial);
		}

		/// <summary>
		/// Performs the initialization of a game based on mapSpawnData, used for both single
		/// player and multiplayer, but not for renderDemos, which don't create a game at all.
		/// </summary>
		/// <param name="noFadeWipe"></param>
		private void ExecuteMapChange(bool noFadeWipe = false)
		{
			bool	reloadingSameMap;

			// close console and remove any prints from the notify lines
			idE.Console.Close();

			if(this.IsMultiplayer == true)
			{
				// make sure the mp GUI isn't up, or when players get back in the
				// map, mpGame's menu and the gui will be out of sync.
				SetUserInterface(null, null);
			}

			// mute sound
			// TODO: soundSystem->SetMute( true );

			// clear all menu sounds
			// TODO: menuSoundWorld->ClearAllSoundEmitters();

			// unpause the game sound world
			// NOTE: we UnPause again later down. not sure this is needed
			// TODO: sound
			/*if ( sw->IsPaused() ) {
				sw->UnPause();
			}*/

			if(noFadeWipe == false)
			{
				// capture the current screen and start a wipe
				// TODO: StartWipe( "wipeMaterial", true );

				// immediately complete the wipe to fade out the level transition
				// run the wipe to completion
				// TODO: CompleteWipe();
			}

			// extract the map name from serverinfo
			string mapString = _mapSpawnData.ServerInformation.GetString("si_map");
			string mapFullName = string.Format("maps/{0}", Path.Combine(Path.GetDirectoryName(mapString), Path.GetFileNameWithoutExtension(mapString)));

			// shut down the existing game if it is running
			// TODO: UnloadMap();

			// don't do the deferred caching if we are reloading the same map
			if(mapFullName == _currentMapName)
			{
				reloadingSameMap = true;
			}
			else
			{
				reloadingSameMap = false;
				_currentMapName = mapFullName;
			}

			// note which media we are going to need to load
			if(reloadingSameMap == false)
			{
				idE.DeclManager.BeginLevelLoad();
				idE.RenderSystem.BeginLevelLoad();
				// TODO: soundSystem->BeginLevelLoad();
			}

			idE.UIManager.BeginLevelLoad();
			// TODO: idE.UIManager.Reload(true);

			// set the loading gui that we will wipe to
			LoadLoadingInterface(mapString);

			// cause prints to force screen updates as a pacifier,
			// and draw the loading gui instead of game draws
			_insideExecuteMapChange = true;

			// if this works out we will probably want all the sizes in a def file although this solution will 
			// work for new maps etc. after the first load. we can also drop the sizes into the default.cfg
			
			// TODO: bytesneeded
			/*fileSystem->ResetReadCount();
			if ( !reloadingSameMap  ) {
				bytesNeededForMapLoad = GetBytesNeededForMapLoad( mapString.c_str() );
			} else {
				bytesNeededForMapLoad = 30 * 1024 * 1024;
			}*/

			ClearWipe();

			// let the loading gui spin for 1 second to animate out
			ShowLoadingInterface();

			// note any warning prints that happen during the load process
			idConsole.ClearWarnings(mapString);

			// if net play, we get the number of clients during mapSpawnInfo processing
			if(idE.AsyncNetwork.IsActive == false)
			{
				_clientCount = 1;
			} 
	
			int start = idE.System.Milliseconds;

			idConsole.WriteLine("--------- Map Initialization ---------");
			idConsole.WriteLine("Map: {0}", mapString);

			// let the renderSystem load all the geometry
			if(_renderWorld.InitFromMap(mapFullName) == false)
			{
				idConsole.Error("couldn't load {0}", mapFullName);
			}

			// for the synchronous networking we needed to roll the angles over from
			// level to level, but now we can just clear everything
			idE.UserCommandGenerator.InitForNewMap();

			_mapSpawnData.MapSpawnUserCommand = new idUserCommand[idE.MaxAsynchronousClients];

			for(int i = 0; i < _mapSpawnData.MapSpawnUserCommand.Length; i++)
			{
				_mapSpawnData.MapSpawnUserCommand[i] = new idUserCommand();
			}

			// set the user info
			for(int i = 0; i < _clientCount; i++)
			{
				idE.Game.SetUserInformation(i, _mapSpawnData.UserInformation[i], idE.AsyncNetwork.Client.IsActive, false);
				idE.Game.SetPersistentPlayerInformation(i, _mapSpawnData.PersistentPlayerInformation[i]);
			}

			// load and spawn all other entities ( from a savegame possibly )
			// TODO: save game
			/*if ( loadingSaveGame && savegameFile ) {
				if ( game->InitFromSaveGame( fullMapName + ".map", rw, sw, savegameFile ) == false ) {
					// If the loadgame failed, restart the map with the player persistent data
					loadingSaveGame = false;
					fileSystem->CloseFile( savegameFile );
					savegameFile = NULL;

					game->SetServerInfo( mapSpawnData.serverInfo );
					game->InitFromNewMap( fullMapName + ".map", rw, sw, idAsyncNetwork::server.IsActive(), idAsyncNetwork::client.IsActive(), Sys_Milliseconds() );
				}
			} else */
			{
				// TODO: game
				/*game->SetServerInfo( mapSpawnData.serverInfo );
				game->InitFromNewMap( fullMapName + ".map", rw, sw, idAsyncNetwork::server.IsActive(), idAsyncNetwork::client.IsActive(), Sys_Milliseconds() );*/
			}

			if((idE.AsyncNetwork.IsActive == false) && (_loadingSaveGame == false))
			{
				// spawn players
				for(int i = 0; i < _clientCount; i++)
				{
					idE.Game.SpawnPlayer(i);
				}
			}

			// actually purge/load the media
			if(reloadingSameMap == false)
			{
				idE.RenderSystem.EndLevelLoad();
				// TODO: :soundSystem->EndLevelLoad( mapString.c_str() );
				idE.DeclManager.EndLevelLoad();

				// TODO: SetBytesNeededForMapLoad( mapString.c_str(), fileSystem->GetReadCount() );
			}

			idE.UIManager.EndLevelLoad();

			if((idE.AsyncNetwork.IsActive == false) && (_loadingSaveGame == false))
			{
				// run a few frames to allow everything to settle
				for(int i = 0; i < 10; i++)
				{
					idE.Game.RunFrame(_mapSpawnData.MapSpawnUserCommand);
				}
			}

			int msec = idE.System.Milliseconds - start;

			idConsole.WriteLine("-----------------------------------");
			idConsole.WriteLine("{0} msec to load {1}", msec, mapString);

			// let the renderSystem generate interactions now that everything is spawned
			_renderWorld.GenerateInteractions();

			idConsole.PrintWarnings();

			if((_guiLoading != null) /* TODO: bytesNeededForMapLoad*/) 
			{
				float pct = _guiLoading.State.GetFloat("map_loading");

				if(pct < 0.0f)
				{
					pct = 0.0f;
				}

				while(pct < 1.0f)
				{
					_guiLoading.State.Set("map_loading", pct);
					_guiLoading.StateChanged(idE.System.FrameTime);

					// TODO: Sys_GenerateEvents();
					UpdateScreen();
					
					pct += 0.05f;
				}
			}

			// capture the current screen and start a wipe
			// TODO: StartWipe( "wipe2Material" );

			idE.UserCommandGenerator.Clear();

			// start saving commands for possible writeCmdDemo usage
			// TODO: log index
			/*logIndex = 0;
			statIndex = 0;
			lastSaveIndex = 0;*/

			// don't bother spinning over all the tics we spent loading
			// TODO: lastGameTic = latchedTicNumber = com_ticNumber;

			// remove any prints from the notify lines
			// TODO: console->ClearNotifyLines();

			// stop drawing the laoding screen
			_insideExecuteMapChange = false;
			
			// TODO: sound system
			// set the game sound world for playback
			/*soundSystem->SetPlayingSoundWorld( sw );

			// when loading a save game the sound is paused
			if ( sw->IsPaused() ) {
				// unpause the game sound world
				sw->UnPause();
			}*/

			// restart entity sound playback
			// TODO: soundSystem->SetMute( false );

			// we are valid for game draws now
			_mapSpawned = true;

			idE.EventLoop.ClearEvents();
		}

		private void LoadLoadingInterface(string mapName)
		{
			// load / program a gui to stay up on the screen while loading
			string mapPath = string.Format("guis/map/{0}.gui", Path.GetFileNameWithoutExtension(mapName));

			// give the gamecode a chance to override
			mapPath = idE.Game.GetMapLoadingInterface(mapPath);

			if(idE.UIManager.Exists(mapPath) == true)
			{
				_guiLoading = idE.UIManager.FindInterface(mapPath, true, false, true);
			}
			else
			{
				_guiLoading = idE.UIManager.FindInterface("guis/map/loading.gui", true, false, true);
			}

			_guiLoading.State.Set("map_loading", 0.0f);
	}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Leaves the existing userinfo and serverinfo.
		/// </remarks>
		/// <param name="mapName"></param>
		private void MoveToNewMap(string mapName)
		{
			_mapSpawnData.ServerInformation.Set("si_map", mapName);

			ExecuteMapChange();

			if(_mapSpawnData.ServerInformation.GetBool("devmap") == false)
			{
				// Autosave at the beginning of the level
				// TODO: SaveGame( GetAutoSaveName( mapName ), true );
			}

			this.SetUserInterface(null, null);
		}

		private bool ProcessMenuEvent(SystemEvent ev)
		{
			if(_guiActive == null)
			{
				return true;
			}

			string menuCommand = _guiActive.HandleEvent(ev, idE.System.FrameTime);

			if((menuCommand != null) && (menuCommand.Length > 0))
			{
				// if the menu didn't handle the event, and it's a key down event for an F key, run the bind
				// TODO: keys
				/*if ( event->evType == SE_KEY && event->evValue2 == 1 && event->evValue >= K_F1 && event->evValue <= K_F12 ) {
					idKeyInput::ExecKeyBinding( event->evValue );
				}*/
			}
			else
			{
				DispatchCommand(_guiActive, menuCommand);
			}

			return true;
		}

		private void SetMainMenuVariables()
		{
			idConsole.Warning("TODO: SetMainMenuVariables");

			_guiMainMenu.State.Set("serverlist_sel_0", "-1");
			_guiMainMenu.State.Set("serverlist_selid_0", "-1");

			_guiMainMenu.State.Set("com_machineSpec", idE.CvarSystem.GetInteger("com_machineSpec"));

			// "inetGame" will hold a hand-typed inet address, which is not archived to a cvar
			_guiMainMenu.State.Set("inetGame", "");

			// key bind names
			// TODO: guiMainMenu->SetKeyBindingNames();

			// flag for in-game menu
			/*if ( mapSpawned ) {
				guiMainMenu->SetStateString( "inGame", IsMultiplayer() ? "2" : "1" );
			} else*/ 
			{
				_guiMainMenu.State.Set("inGame", "0");
			}

			// TODO: SetCDKeyGuiVars( );

			_guiMainMenu.State.Set("nightmare", (idE.CvarSystem.GetBool("g_nightmare") == true) ? "1" : "0");
			_guiMainMenu.State.Set("browser_levelshot", "guis/assets/splash/pdtempa");

			// TODO: SetMainMenuSkin();
			// TODO: SetModsMenuGuiVars();

			// TODO: guiMsg->SetStateString( "visible_hasxp", fileSystem->HasD3XP() ? "1" : "0" );
			
			_guiMainMenu.State.Set("driver_prompt", "0");
		}

		private void ShowLoadingInterface()
		{
			if(idE.System.TicNumber == 0)
			{
				return;
			}

			idE.Console.Close();

	// introduced in D3XP code. don't think it actually fixes anything, but doesn't hurt either
	// Try and prevent the while loop from being skipped over (long hitch on the main thread?)
			int stop = idE.System.Milliseconds + 1000;
			int force = 10;

			// TODO: 
			/*while((idE.System.Milliseconds < stop) || (force-- > 0))
			{
				com_frameTime = com_ticNumber * USERCMD_MSEC;

				Frame();
				UpdateScreen(false);
			}*/
		}
		#endregion

		#region Commands
		private void Cmd_Map(object sender, CommandEventArgs e)
		{
			string map = e.Args.Get(1);

			if(map == string.Empty)
			{
				return;
			}

			map = Path.Combine(Path.GetDirectoryName(map), Path.GetFileNameWithoutExtension(map));

			// make sure the level exists before trying to change, so that
			// a typo at the server console won't end the game
			// handle addon packs through reloadEngine
			string mapPath = string.Format("maps/{0}.map", map);

			if(idE.FileSystem.FileExists(mapPath) == false)
			{
				idConsole.WriteLine("Can't find map {0}", mapPath);
			}
			else
			{

				// TODO: FIND_ADDON
				/*case FIND_ADDON:
					common->Printf( "map %s is in an addon pak - reloading\n", string.c_str() );
					rl_args.AppendArg( "map" );
					rl_args.AppendArg( map );
					cmdSystem->SetupReloadEngine( rl_args );
					return;*/
			}

			idE.CvarSystem.SetBool("developer", false);
			idE.Session.StartNewGame(map, true);
		}
		#endregion
		#endregion

		#region MapSpawnData
		private class MapSpawnData
		{
			public idDict ServerInformation = new idDict();
			public idDict SyncedCvars = new idDict();
			public idDict[] UserInformation = new idDict[idE.MaxAsynchronousClients];
			public idDict[] PersistentPlayerInformation = new idDict[idE.MaxAsynchronousClients];
			public idUserCommand[] MapSpawnUserCommand = new idUserCommand[idE.MaxAsynchronousClients]; // needed for tracking delta angles
		
			public MapSpawnData()
			{
				for(int i = 0; i < idE.MaxAsynchronousClients; i++)
				{
					UserInformation[i] = new idDict();
					PersistentPlayerInformation[i] = new idDict();
					MapSpawnUserCommand[i] = new idUserCommand();
				}
			}
		}
		#endregion
	}
}