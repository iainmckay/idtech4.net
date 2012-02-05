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
using System.Linq;
using System.Text;

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

		private bool _insideScreenUpdate;
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
		}
		#endregion

		#region Methods
		#region Public
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
				idConsole.WriteLine("TODO: DispatchCommand(guiActive, cmd);");
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

	#ifndef	ID_DEDICATED
		cmdSystem->AddCommand( "map", Session_Map_f, CMD_FL_SYSTEM, "loads a map", idCmdSystem::ArgCompletion_MapName );
		cmdSystem->AddCommand( "devmap", Session_DevMap_f, CMD_FL_SYSTEM, "loads a map in developer mode", idCmdSystem::ArgCompletion_MapName );
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

			// TODO: other gui
			/*_guiRestartMenu = idE.UIManager.FindInterface("guis/restart.gui", true, false, true);
			_guiGameOver = idE.UIManager.FindInterface("guis/gameover.gui", true, false, true);
			_guiMsg = idE.UIManager.FindInterface("guis/msg.gui", true, false, true);
			_guiTakeNotes = idE.UIManager.FindInterface("guis/takeNotes.gui", true, false, true);
			_guiIntro = idE.UIManager.FindInterface("guis/intro.gui", true, false, true);*/

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
			/*if(guiActive == guiMainMenu)
			{
				SetSaveGameGuiVars();
				SetMainMenuGuiVars();
			}
			else if(guiActive == guiRestartMenu)
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
		public void StartMenu(bool playIntro)
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
			// TODO: guiMainMenu->HandleNamedEvent(playIntro ? "playIntro" : "noIntro");


			/*// TODO: if(fileSystem->HasD3XP())
			{
				guiMainMenu->SetStateString("game_list", common->GetLanguageDict()->GetString("#str_07202"));
			}
			else
			{
				guiMainMenu->SetStateString("game_list", common->GetLanguageDict()->GetString("#str_07212"));
			}

			// TODO: console->Close();*/
		}

		public void UpdateScreen(bool outOfSequence)
		{
			if(_insideScreenUpdate == true)
			{
				return;
			}

			_insideScreenUpdate = true;

			// if this is a long-operation update and we are in windowed mode,
			// release the mouse capture back to the desktop
			// TODO
			/*if ( outOfSequence ) {
				Sys_GrabMouseCursor( false );
			}*/

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
			// TODO
			/*bool fullConsole = false;

			if(insideExecuteMapChange)
			{
				if(guiLoading)
				{
					guiLoading->Redraw(com_frameTime);
				}
				if(guiActive == guiMsg)
				{
					guiMsg->Redraw(com_frameTime);
				}
			}
			else if(guiTest)
			{
				// if testing a gui, clear the screen and draw it
				// clear the background, in case the tested gui is transparent
				// NOTE that you can't use this for aviGame recording, it will tick at real com_frameTime between screenshots..
				renderSystem->SetColor(colorBlack);
				renderSystem->DrawStretchPic(0, 0, 640, 480, 0, 0, 1, 1, declManager->FindMaterial("_white"));
				guiTest->Redraw(com_frameTime);
			}
			else*/
			if((_guiActive != null) && (_guiActive.State.GetBool("gameDraw") == false))
			{

				// draw the frozen gui in the background
				// TODO
				/*if(guiActive == guiMsg && guiMsgRestore)
				{
					guiMsgRestore->Redraw(com_frameTime);
				}*/

				// draw the menus full screen
				/*if(guiActive == guiTakeNotes && !com_skipGameDraw.GetBool())
				{
					game->Draw(GetLocalClientNum());
				}*/

				_guiActive.Draw(idE.System.FrameTime);
			}
			/*else if(readDemo)
			{
				rw->RenderScene(&currentDemoRenderView);
				renderSystem->DrawDemoPics();
			}
			else if(mapSpawned)
			{
				bool gameDraw = false;
				// normal drawing for both single and multi player
				if(!com_skipGameDraw.GetBool() && GetLocalClientNum() >= 0)
				{
					// draw the game view
					int start = Sys_Milliseconds();
					gameDraw = game->Draw(GetLocalClientNum());
					int end = Sys_Milliseconds();
					time_gameDraw += (end - start);	// note time used for com_speeds
				}
				if(!gameDraw)
				{
					renderSystem->SetColor(colorBlack);
					renderSystem->DrawStretchPic(0, 0, 640, 480, 0, 0, 1, 1, declManager->FindMaterial("_white"));
				}

				// save off the 2D drawing from the game
				if(writeDemo)
				{
					renderSystem->WriteDemoPics();
				}
			}
			else
			{

		/*if ( com_allowConsole.GetBool() ) {
			console->Draw( true );
		} else {
			emptyDrawCount++;
			if ( emptyDrawCount > 5 ) {
				// it's best if you can avoid triggering the watchgod by doing the right thing somewhere else
				assert( false );
				common->Warning( "idSession: triggering mainmenu watchdog" );
				emptyDrawCount = 0;
				StartMenu();
			}
			renderSystem->SetColor4( 0, 0, 0, 1 );
			renderSystem->DrawStretchPic( 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, 0, 0, 1, 1, declManager->FindMaterial( "_white" ) );
		}

				fullConsole = true;
			}

#if ID_CONSOLE_LOCK
	if ( !fullConsole && emptyDrawCount ) {
		common->DPrintf( "idSession: %d empty frame draws\n", emptyDrawCount );
		emptyDrawCount = 0;
	}
	fullConsole = false;
#endif

			// draw the wipe material on top of this if it hasn't completed yet
			DrawWipeModel();

			// draw debug graphs
			DrawCmdGraph();

			// draw the half console / notify console on top of everything
			if(!fullConsole)
			{
				console->Draw(false);
			}*/
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
		#endregion
		#endregion
	}
}