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
		private	idUserInterface _guiMainMenu;
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

			_guiRestartMenu = idE.UIManager.FindInterface("guis/restart.gui", true, false, true);
			_guiGameOver = idE.UIManager.FindInterface("guis/gameover.gui", true, false, true);
			_guiMsg = idE.UIManager.FindInterface("guis/msg.gui", true, false, true);
			_guiTakeNotes = idE.UIManager.FindInterface("guis/takeNotes.gui", true, false, true);
			_guiIntro = idE.UIManager.FindInterface("guis/intro.gui", true, false, true);

			_whiteMaterial = idE.DeclManager.FindMaterial("_white");
			
			idConsole.WriteLine("session initialized");
			idConsole.WriteLine("--------------------------------------");
		}
		#endregion
		#endregion
	}
}