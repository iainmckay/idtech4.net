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
using System.Management;

using Microsoft.Xna.Framework;

namespace idTech4
{
	public sealed class idSystem
	{
		#region Properties
		public int Time
		{
			get
			{
				return idE.Game.Time.ElapsedGameTime.Milliseconds;
			}
		}
		#endregion

		#region Members
		private bool _fullyInitialized;
		private bool _shuttingDown;

		private bool _refreshOnPoint;
		private bool _refreshOnPrint;

		private ErrorType _errorEntered;

		private int _lastErrorTime;
		private int _errorCount;

		private int _frameTime; // time for the current frame in milliseconds.
		private int _frameNumber; // variable frame number.
		private int _ticNumber; // 60 hz tics

		private List<string> _errorList = new List<string>();

		private idCmdArgs[] _commandLineArguments = new idCmdArgs[] { };
		#endregion

		#region Constructor
		public idSystem()
		{
			// TODO
			/*logFile = NULL;

			strcpy(errorMessage, "");

			rd_buffer = NULL;
			rd_buffersize = 0;
			rd_flush = NULL;

			gameDLL = 0;*/
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Initialize everything.
		/// </summary>
		/// <param name="args"></param>
		public void Init(string[] args)
		{
			// TODO
			try
			{
				InitCvars();

				// initialize idLib
				// idLib::Init();

				// clear warning buffer
				idConsole.ClearWarnings(string.Format("{0} initialization", idE.GameName));

				ParseCommandLine(args);

				idE.CmdSystem.Init();
				idE.CvarSystem.Init();

				// start file logging right away, before early console or whatever
				StartupVariable("win_outputDebugString", false);

				// print engine version
				idConsole.WriteLine(idE.Version);

				// initialize key input/binding, done early so bind command exists
				/*idKeyInput::Init();*/

				// init the console so we can take prints
				// TODO: console->Init();

				// get architecture info
				SysInit();

				/*
				// initialize networking
				Sys_InitNetworking();*/

				// override cvars from command line
				StartupVariable(null, false);

				/*if ( !idAsyncNetwork::serverDedicated.GetInteger() && Sys_AlreadyRunning() ) {
					Sys_Quit();
				}

				// initialize processor specific SIMD implementation
				InitSIMD();*/

				// init commands
				InitCommands();

				// TODO #ifdef ID_WRITE_VERSION
				/*config_compressor = idCompressor::AllocArithmetic();
				#endif*/

				// game specific initialization
				InitGame();

				// don't add startup commands if no CD key is present
				if(AddStartupCommands() == false)
				{
					// if the user didn't give any commands, run default action
					// TODO: session->StartMenu( true );
				}

				idConsole.WriteLine("--- Common Initialization Complete ---");

				// print all warnings queued during initialization
				idConsole.PrintWarnings();

				// TODO
				/*
#ifdef	ID_DEDICATED
				Printf( "\nType 'help' for dedicated server info.\n\n" );
#endif

				// remove any prints from the notify lines
				console->ClearNotifyLines();
				*/

				_commandLineArguments = null;
				_fullyInitialized = true;
			}
			catch(Exception)
			{
				Error("Error during initialization");
			}
		}

		public void Frame()
		{
			try
			{
				// pump all the events
				// TODO: Sys_GenerateEvents();

				// write config file if anything changed
				// TODO: WriteConfiguration(); 

				// change SIMD implementation if required
				// TODO
				/*if ( com_forceGenericSIMD.IsModified() ) {
					InitSIMD();
				}*/

				/*eventLoop->RunEventLoop();*/

				_frameTime = _ticNumber * idE.UserCommandMillseconds;

				/*idAsyncNetwork::RunFrame();

				if ( idAsyncNetwork::IsActive() ) {
					if ( idAsyncNetwork::serverDedicated.GetInteger() != 1 ) {
						session->GuiFrameEvents();
						session->UpdateScreen( false );
					}
				} else {
					session->Frame();

					// normal, in-sequence screen update
					session->UpdateScreen( false );
				}

				// report timing information
				if ( com_speeds.GetBool() ) {
					static int	lastTime;
					int		nowTime = Sys_Milliseconds();
					int		com_frameMsec = nowTime - lastTime;
					lastTime = nowTime;
					Printf( "frame:%i all:%3i gfr:%3i rf:%3i bk:%3i\n", com_frameNumber, com_frameMsec, time_gameFrame, time_frontend, time_backend );
					time_gameFrame = 0;
					time_gameDraw = 0;
				}	*/

				_frameNumber++;
			}
			catch(Exception)
			{
				// an ERP_DROP was thrown
			}
		}

		public void Quit()
		{
			// don't try to shutdown if we are in a recursive error
			if(_errorEntered == ErrorType.None)
			{
				Shutdown();
			}

			SysQuit();
		}

		public void Shutdown()
		{
			_shuttingDown = true;

			// TODO
			/*idAsyncNetwork::server.Kill();
			idAsyncNetwork::client.Shutdown();*/

			// game specific shut down
			// TODO: ShutdownGame( false );

			// shut down non-portable system services
			// TODO: Sys_Shutdown();

			// shut down the console
			// TODO: console->Shutdown();

			// shut down the key system
			// TODO: idKeyInput::Shutdown();

			// shut down the cvar system
			// TODO: cvarSystem->Shutdown();

			// shut down the console command system
			// TODO: cmdSystem->Shutdown();

			// TODO
			/*#ifdef ID_WRITE_VERSION
				delete config_compressor;
				config_compressor = NULL;
			#endif*/

			// free any buffered warning messages
			idConsole.ClearWarnings(string.Format("{0} shutdown", idE.GameName));

			_errorList.Clear();

			// free language dictionary
			// TODO: languageDict.Clear();

			// enable leak test
			// TODO: Mem_EnableLeakTest( "doom" );

			// shutdown idLib
			// TODO: idLib::ShutDown();
		}

		/// <summary>
		/// Searches for command line parameters that are set commands.
		/// </summary>
		/// <remarks>
		/// If match is not NULL, only that cvar will be looked for.
		/// That is necessary because cddir and basedir need to be set before the filesystem is started, but all other sets should
		/// be after execing the config and default.
		/// </remarks>
		/// <param name="match"></param>
		/// <param name="once"></param>
		public void StartupVariable(string match, bool once)
		{
			List<idCmdArgs> final = new List<idCmdArgs>();

			foreach(idCmdArgs args in _commandLineArguments)
			{
				if(args.Get(0).ToLower() != "set")
				{
					continue;
				}

				string s = args.Get(1);

				if((match == null) || (StringComparer.InvariantCultureIgnoreCase.Compare(s, match) == 0))
				{
					idE.CvarSystem.SetString(s, args.Get(2));

					if(once == false)
					{
						final.Add(args);
					}
				}
				else
				{
					final.Add(args);
				}
			}

			_commandLineArguments = final.ToArray();
		}
		#endregion

		#region Internal
		internal void Error(string format, params object[] args)
		{
			ErrorType code = ErrorType.Drop;

			// always turn this off after an error
			_refreshOnPrint = false;

			// when we are running automated scripts, make sure we
			// know if anything failed
			if(idE.CvarSystem.GetInt("fs_copyfiles") > 0)
			{
				code = ErrorType.Fatal;
			}

			// if we don't have GL running, make it a fatal error
			// TODO: MAJOR
			/*if(idE.RenderSystem.IsRunning() == false)
			{
				code = ErrorType.Fatal;
			}*/

			// if we got a recursive error, make it fatal
			if(_errorEntered > 0)
			{
				// if we are recursively erroring while exiting
				// from a fatal error, just kill the entire
				// process immediately, which will prevent a
				// full screen rendering window covering the
				// error dialog
				if(_errorEntered == ErrorType.Fatal)
				{
					Quit();
				}

				code = ErrorType.Fatal;
			}

			// if we are getting a solid stream of ERP_DROP, do an ERP_FATAL
			int currentTime = this.Time;

			if((currentTime - _lastErrorTime) < 100)
			{
				if(++_errorCount > 3)
				{
					code = ErrorType.Fatal;
				}
			}
			else
			{
				_errorCount = 0;
			}

			_lastErrorTime = currentTime;
			_errorEntered = code;

			string errorMessage = string.Format(format, args);

			// copy the error message to the clip board
			// TODO: SetClipboardData(errorMessage);

			// add the message to the error list
			if(_errorList.Contains(errorMessage) == false)
			{
				_errorList.Add(errorMessage);
			}

			// dont shut down the session for gui editor or debugger
			// TODO
			/*if ( !( com_editors & ( EDITOR_GUI | EDITOR_DEBUGGER ) ) ) {
				session->Stop();
			}*/

			if(code == ErrorType.Disconnect)
			{
				_errorEntered = ErrorType.None;

				throw new Exception(errorMessage);
			}
			// The gui editor doesnt want thing to com_error so it handles exceptions instead
			// TODO
			/*} 
			 * else if( com_editors & ( EDITOR_GUI | EDITOR_DEBUGGER ) ) {
				com_errorEntered = 0;
				throw idException( errorMessage );
			 */
			else if(code == ErrorType.Drop)
			{
				idConsole.WriteLine("********************\nERROR: {0}\n********************", errorMessage);

				_errorEntered = ErrorType.None;

				throw new Exception(errorMessage);
			}
			else
			{
				idConsole.WriteLine("********************\nERROR: {0}\n********************", errorMessage);
			}

			if(idE.CvarSystem.GetBool("r_fullscreen") == true)
			{
				idE.CmdSystem.BufferCommandText(Execute.Now, "vid_restart partial windowed\n");
			}

			Shutdown();
			SysError(errorMessage);
		}

		internal void FatalError(string format, params object[] args)
		{
			// if we got a recursive error, make it fatal
			if(_errorEntered != ErrorType.None)
			{
				// if we are recursively erroring while exiting
				// from a fatal error, just kill the entire
				// process immediately, which will prevent a
				// full screen rendering window covering the
				// error dialog
				idConsole.WriteLine("FATAL: recursed fatal error:\n{0}", string.Format(format, args));

				// write the console to a log file?
				SysQuit();
			}

			_errorEntered = ErrorType.Fatal;

			if(idE.CvarSystem.GetBool("r_fullscreen") == true)
			{
				idE.CmdSystem.BufferCommandText(Execute.Now, "vid_restart partial windowed");
			}

			Shutdown();
			SysError(format, args);
		}
		#endregion

		#region Private
		private void InitCvars()
		{
			// these win_* cvars may need ifdef'd out on other platforms.  only targetting windows+xbox right now so
			// shouldn't be an issue.
			new idCvar("sys_arch", "", "", CvarFlags.System | CvarFlags.Init);
			new idCvar("sys_cpustring", "detect", "", CvarFlags.System | CvarFlags.Init);
			new idCvar("in_mouse", "1", "enable mouse input", CvarFlags.System | CvarFlags.Bool);
			new idCvar("win_allowAltTab", "0", "allow Alt-Tab when fullscreen", CvarFlags.System | CvarFlags.Bool);
			new idCvar("win_notaskkeys", "0", "disable windows task keys", CvarFlags.System | CvarFlags.Integer);
			new idCvar("win_username", "", "windows user name", CvarFlags.System | CvarFlags.Init);
			new idCvar("win_xpos", "3", "horizontal position of window", CvarFlags.System | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("win_ypos", "22", "vertical position of window", CvarFlags.System | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("win_outputDebugString", "0", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("win_outputEditString", "1", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("win_viewlog", "0", "", CvarFlags.System | CvarFlags.Integer);
			new idCvar("win_timerUpdate", "0", "allows the game to be updated while dragging the window", CvarFlags.System | CvarFlags.Bool);
			new idCvar("win_allowMultipleInstances", "0", "allow multiple instances running concurrently", CvarFlags.System | CvarFlags.Bool);

			new idCvar("si_version", idE.Version, "engine version", CvarFlags.System | CvarFlags.ReadOnly | CvarFlags.ServerInfo);
			new idCvar("com_skipRenderer", "0", "skip the renderer completely", CvarFlags.Bool | CvarFlags.System);
			new idCvar("com_machineSpec", "-1", "hardware classification, -1 = not detected, 0 = low quality, 1 = medium quality, 2 = high quality, 3 = ultra quality", CvarFlags.Integer | CvarFlags.Archive | CvarFlags.System);
			new idCvar("com_purgeAll", "0", "purge everything between level loads", CvarFlags.Bool | CvarFlags.Archive | CvarFlags.System);
			new idCvar("com_memoryMarker", "-1", "used as a marker for memory stats", CvarFlags.Integer | CvarFlags.System | CvarFlags.Init);
			new idCvar("com_preciseTic", "1", "run one game tick every async thread update", CvarFlags.Bool | CvarFlags.System);
			new idCvar("com_asyncInput", "0", "sample input from the async thread", CvarFlags.Bool | CvarFlags.System);
			new idCvar("com_asyncSound", "1", "0: mix sound inline, 1: memory mapped async mix, 2: callback mixing, 3: write async mix", 0, 1, CvarFlags.Integer | CvarFlags.System);
			new idCvar("com_forceGenericSIMD", "0", "force generic platform independent SIMD", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("developer", "0", "developer mode", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_allowConsole", "0", "allow toggling console with the tilde key", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_speeds", "0", "show engine timings", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_showFPS", "0", "show frames rendered per second", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_showMemoryUsage", "0", "show total and per frame memory usage", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_showAsyncStats", "0", "show async network stats", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_showSoundDecoders", "0", "show sound decoders", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_timestampPrints", "0", "print time with each console print, 1 = msec, 2 = sec", 0, 2, /* TODO: idCmdSystem::ArgCompletion_Integer<0,2>, */CvarFlags.System);
			new idCvar("timescale", "1", "scales the time", 0.1f, 10.0f, CvarFlags.System | CvarFlags.Float);
			new idCvar("logFile", "0", "1 = buffer log, 2 = flush after each print", 0, 2, /* TODO: idCmdSystem::ArgCompletion_Integer<0,2>,*/ CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("logFileName", "qconsole.log", "name of log file, if empty, qconsole.log will be used", CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_makingBuild", "0", "1 when making a build", CvarFlags.Bool | CvarFlags.System);
			new idCvar("com_updateLoadSize", "0", "update the load size after loading a map", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_videoRam", "64", "holds the last amount of detected video ram", CvarFlags.Integer | CvarFlags.System | CvarFlags.NoCheat | CvarFlags.Archive);

			new idCvar("com_product_lang_ext", "1", "Extension to use when creating language files.", CvarFlags.Integer | CvarFlags.System | CvarFlags.Archive);
		}

		private void InitCommands()
		{
			idE.CmdSystem.AddCommand("error", "causes an error", CommandFlags.System | CommandFlags.Cheat, new EventHandler<CommandEventArgs>(Com_Error));

			/*
			cmdSystem->AddCommand( "crash", Com_Crash_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "causes a crash" );
			cmdSystem->AddCommand( "freeze", Com_Freeze_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "freezes the game for a number of seconds" );*/

			idE.CmdSystem.AddCommand("quit", "quits the game", CommandFlags.System, new EventHandler<CommandEventArgs>(Com_Quit));
			idE.CmdSystem.AddCommand("exit", "exits the game", CommandFlags.System, new EventHandler<CommandEventArgs>(Com_Quit));

			/*cmdSystem->AddCommand( "writeConfig", Com_WriteConfig_f, CMD_FL_SYSTEM, "writes a config file" );
			cmdSystem->AddCommand( "reloadEngine", Com_ReloadEngine_f, CMD_FL_SYSTEM, "reloads the engine down to including the file system" );
			cmdSystem->AddCommand( "setMachineSpec", Com_SetMachineSpec_f, CMD_FL_SYSTEM, "detects system capabilities and sets com_machineSpec to appropriate value" );
			cmdSystem->AddCommand( "execMachineSpec", Com_ExecMachineSpec_f, CMD_FL_SYSTEM, "execs the appropriate config files and sets cvars based on com_machineSpec" );

		#if	!defined( ID_DEMO_BUILD ) && !defined( ID_DEDICATED )
			// compilers
			cmdSystem->AddCommand( "dmap", Dmap_f, CMD_FL_TOOL, "compiles a map", idCmdSystem::ArgCompletion_MapName );
			cmdSystem->AddCommand( "renderbump", RenderBump_f, CMD_FL_TOOL, "renders a bump map", idCmdSystem::ArgCompletion_ModelName );
			cmdSystem->AddCommand( "renderbumpFlat", RenderBumpFlat_f, CMD_FL_TOOL, "renders a flat bump map", idCmdSystem::ArgCompletion_ModelName );
			cmdSystem->AddCommand( "runAAS", RunAAS_f, CMD_FL_TOOL, "compiles an AAS file for a map", idCmdSystem::ArgCompletion_MapName );
			cmdSystem->AddCommand( "runAASDir", RunAASDir_f, CMD_FL_TOOL, "compiles AAS files for all maps in a folder", idCmdSystem::ArgCompletion_MapName );
			cmdSystem->AddCommand( "runReach", RunReach_f, CMD_FL_TOOL, "calculates reachability for an AAS file", idCmdSystem::ArgCompletion_MapName );
			cmdSystem->AddCommand( "roq", RoQFileEncode_f, CMD_FL_TOOL, "encodes a roq file" );
		#endif

			cmdSystem->AddCommand( "printMemInfo", PrintMemInfo_f, CMD_FL_SYSTEM, "prints memory debugging data" );

			// idLib commands
			cmdSystem->AddCommand( "memoryDump", Mem_Dump_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "creates a memory dump" );
			cmdSystem->AddCommand( "memoryDumpCompressed", Mem_DumpCompressed_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "creates a compressed memory dump" );
			cmdSystem->AddCommand( "showStringMemory", idStr::ShowMemoryUsage_f, CMD_FL_SYSTEM, "shows memory used by strings" );
			cmdSystem->AddCommand( "showDictMemory", idDict::ShowMemoryUsage_f, CMD_FL_SYSTEM, "shows memory used by dictionaries" );
			cmdSystem->AddCommand( "listDictKeys", idDict::ListKeys_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "lists all keys used by dictionaries" );
			cmdSystem->AddCommand( "listDictValues", idDict::ListValues_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "lists all values used by dictionaries" );
			cmdSystem->AddCommand( "testSIMD", idSIMD::Test_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "test SIMD code" );

			// localization
			cmdSystem->AddCommand( "localizeGuis", Com_LocalizeGuis_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "localize guis" );
			cmdSystem->AddCommand( "localizeMaps", Com_LocalizeMaps_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "localize maps" );
			cmdSystem->AddCommand( "reloadLanguage", Com_ReloadLanguage_f, CMD_FL_SYSTEM, "reload language dict" );

			//D3XP Localization
			cmdSystem->AddCommand( "localizeGuiParmsTest", Com_LocalizeGuiParmsTest_f, CMD_FL_SYSTEM, "Create test files that show gui parms localized and ignored." );
			cmdSystem->AddCommand( "localizeMapsTest", Com_LocalizeMapsTest_f, CMD_FL_SYSTEM, "Create test files that shows which strings will be localized." );

			// build helpers
			cmdSystem->AddCommand( "startBuild", Com_StartBuild_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "prepares to make a build" );
			cmdSystem->AddCommand( "finishBuild", Com_FinishBuild_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "finishes the build process" );

		#ifdef ID_DEDICATED
			cmdSystem->AddCommand( "help", Com_Help_f, CMD_FL_SYSTEM, "shows help" );
		#endif*/
		}

		private void InitGame()
		{
			// initialize the file system
			idE.FileSystem.Init();

			// initialize the declaration manager
			/*declManager->Init();

			// force r_fullscreen 0 if running a tool
			CheckToolMode();

			idFile *file = fileSystem->OpenExplicitFileRead( fileSystem->RelativePathToOSPath( CONFIG_SPEC, "fs_savepath" ) );
			bool sysDetect = ( file == NULL );
			if ( file ) {
				fileSystem->CloseFile( file );
			} else {
				file = fileSystem->OpenFileWrite( CONFIG_SPEC );
				fileSystem->CloseFile( file );
			}
	
			idCmdArgs args;
			if ( sysDetect ) {
				SetMachineSpec();
				Com_ExecMachineSpec_f( args );
			}

			// initialize the renderSystem data structures, but don't start OpenGL yet
			renderSystem->Init();

			// initialize string database right off so we can use it for loading messages
			InitLanguageDict();

			PrintLoadingMessage( common->GetLanguageDict()->GetString( "#str_04344" ) );

			// load the font, etc
			console->LoadGraphics();

			// init journalling, etc
			eventLoop->Init();

			PrintLoadingMessage( common->GetLanguageDict()->GetString( "#str_04345" ) );

			// exec the startup scripts
			cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "exec editor.cfg\n" );
			cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "exec default.cfg\n" );

			// skip the config file if "safe" is on the command line
			if ( !SafeMode() ) {
				cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "exec " CONFIG_FILE "\n" );
			}
			cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "exec autoexec.cfg\n" );

			// reload the language dictionary now that we've loaded config files
			cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "reloadLanguage\n" );

			// run cfg execution
			cmdSystem->ExecuteCommandBuffer();

			// re-override anything from the config files with command line args
			StartupVariable( NULL, false );

			// if any archived cvars are modified after this, we will trigger a writing of the config file
			cvarSystem->ClearModifiedFlags( CVAR_ARCHIVE );

			// cvars are initialized, but not the rendering system. Allow preference startup dialog
			Sys_DoPreferences();

			// init the user command input code
			usercmdGen->Init();

			PrintLoadingMessage( common->GetLanguageDict()->GetString( "#str_04346" ) );

			// start the sound system, but don't do any hardware operations yet
			soundSystem->Init();

			PrintLoadingMessage( common->GetLanguageDict()->GetString( "#str_04347" ) );

			// init async network
			idAsyncNetwork::Init();

		#ifdef	ID_DEDICATED
			idAsyncNetwork::server.InitPort();
			cvarSystem->SetCVarBool( "s_noSound", true );
		#else
			if ( idAsyncNetwork::serverDedicated.GetInteger() == 1 ) {
				idAsyncNetwork::server.InitPort();
				cvarSystem->SetCVarBool( "s_noSound", true );
			} else {
				// init OpenGL, which will open a window and connect sound and input hardware
				PrintLoadingMessage( common->GetLanguageDict()->GetString( "#str_04348" ) );
				InitRenderSystem();
			}
		#endif

			PrintLoadingMessage( common->GetLanguageDict()->GetString( "#str_04349" ) );

			// initialize the user interfaces
			uiManager->Init();

			// startup the script debugger
			// DebuggerServerInit();

			PrintLoadingMessage( common->GetLanguageDict()->GetString( "#str_04350" ) );

			// load the game dll
			LoadGameDLL();
	
			PrintLoadingMessage( common->GetLanguageDict()->GetString( "#str_04351" ) );

			// init the session
			session->Init();

			// have to do this twice.. first one sets the correct r_mode for the renderer init
			// this time around the backend is all setup correct.. a bit fugly but do not want
			// to mess with all the gl init at this point.. an old vid card will never qualify for 
			if ( sysDetect ) {
				SetMachineSpec();
				Com_ExecMachineSpec_f( args );
				cvarSystem->SetCVarInteger( "s_numberOfSpeakers", 6 );
				cmdSystem->BufferCommandText( CMD_EXEC_NOW, "s_restart\n" );
				cmdSystem->ExecuteCommandBuffer();
			}*/
		}

		private void ParseCommandLine(string[] commandLineArgs)
		{
			List<idCmdArgs> argList = new List<idCmdArgs>();
			idCmdArgs current = null;

			foreach(string arg in commandLineArgs)
			{
				if(arg.StartsWith("+") == true)
				{
					current = new idCmdArgs();
					current.AppendArg(arg.Substring(1));

					argList.Add(current);
				}
				else
				{
					if(current == null)
					{
						current = new idCmdArgs();
						argList.Add(current);
					}

					current.AppendArg(arg);
				}
			}

			_commandLineArguments = argList.ToArray();
		}

		/// <summary>
		/// Adds command line parameters as script statements. Commands are separated by + signs.
		/// </summary>
		/// <returns>Returns true if any late commands were added, which will keep the demoloop from immediately starting.</returns>
		private bool AddStartupCommands()
		{
			bool added = false;

			// quote every token, so args with semicolons can work
			foreach(idCmdArgs args in _commandLineArguments)
			{
				if(args.Length == 0)
				{
					continue;
				}

				// set commands won't override menu startup
				if(args.Get(0).ToLower().StartsWith("set") == true)
				{
					added = true;
				}

				// directly as tokenized so nothing gets screwed
				idE.CmdSystem.BufferCommandText(Execute.Append, args.ToString());
			}

			return added;
		}

		/// <summary>
		/// Show the early console as an error dialog.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		private void SysError(string format, params object[] args)
		{
			string errorMessage = string.Format(format, args);

			idE.SystemConsole.Append(errorMessage);
			idE.SystemConsole.Show(1, true);

			// TODO: Win_SetErrorText( text );
			// TODO: timeEndPeriod( 1 );

			// TODO: Sys_ShutdownInput();
			// TODO: GLimp_Shutdown();
		}

		private void SysQuit()
		{
			// TODO: timeEndPeriod( 1 );
			// TODO: Sys_ShutdownInput();

			idE.Game.Exit();
		}

		private void SysInit()
		{
			// TODO
			/*cmdSystem->AddCommand( "in_restart", Sys_In_Restart_f, CMD_FL_SYSTEM, "restarts the input system" );
#ifdef DEBUG
			cmdSystem->AddCommand( "createResourceIDs", CreateResourceIDs_f, CMD_FL_TOOL, "assigns resource IDs in _resouce.h files" );
#endif
#if 0
			cmdSystem->AddCommand( "setAsyncSound", Sys_SetAsyncSound_f, CMD_FL_SYSTEM, "set the async sound option" );
#endif*/

			// not bothering with fetching the windows username.
			idE.CvarSystem.SetString("win_username", "player");

			//
			// Windows version
			//
			OperatingSystem osInfo = Environment.OSVersion;

			if((osInfo.Version.Major < 4)
				|| (osInfo.Platform == PlatformID.Win32S)
				|| (osInfo.Platform == PlatformID.Win32Windows))
			{
				idConsole.Error("{0} requires Windows XP or above", idE.GameName);
			}
			else if(osInfo.Platform == PlatformID.Win32NT)
			{
				if(osInfo.Version.Major <= 4)
				{
					idE.CvarSystem.SetString("sys_arch", "WinNT (NT)");
				}
				else if((osInfo.Version.Major == 5) && (osInfo.Version.Minor == 0))
				{
					idE.CvarSystem.SetString("sys_arch", "Win2K (NT)");
				}
				else if((osInfo.Version.Major == 5) && (osInfo.Version.Minor == 1))
				{
					idE.CvarSystem.SetString("sys_arch", "WinXP (NT)");
				}
				else if((osInfo.Version.Major == 6) && (osInfo.Version.Minor == 0))
				{
					idE.CvarSystem.SetString("sys_arch", "Vista");
				}
				else if((osInfo.Version.Major == 6) && (osInfo.Version.Minor == 1))
				{
					idE.CvarSystem.SetString("sys_arch", "Windows 7");
				}
				else
				{
					idE.CvarSystem.SetString("sys_arch", "Unknown NT variant");
				}
			}

			//
			// CPU type
			//			
			if(Environment.OSVersion.Version.Major >= 6)
			{
				idConsole.WriteLine("{0} MHz, {1} cores, {2} threads", idE.Platform.ClockSpeed, idE.Platform.CoreCount, idE.Platform.ThreadCount);
			}
			else
			{
				idConsole.WriteLine("{0} MHz", idE.Platform.ClockSpeed);
			}

			CpuCapabilities caps = idE.Platform.GetCpuCapabilities();

			string capabilities = string.Empty;

			if((caps & CpuCapabilities.AMD) != 0)
			{
				capabilities += "AMD CPU";
			}
			else if((caps & CpuCapabilities.Intel) != 0)
			{
				capabilities += "Intel CPU";
			}
			else if((caps & CpuCapabilities.Unsupported) != 0)
			{
				capabilities += "unsupported CPU";
			}
			else
			{
				capabilities += "generic CPU";
			}

			// TODO: can't make use of any of these features but nice to identify them anyway.
			/*string += " with ";
			if ( win32.cpuid & CPUID_MMX ) {
				string += "MMX & ";
			}
			if ( win32.cpuid & CPUID_3DNOW ) {
				string += "3DNow! & ";
			}
			if ( win32.cpuid & CPUID_SSE ) {
				string += "SSE & ";
			}
			if ( win32.cpuid & CPUID_SSE2 ) {
				string += "SSE2 & ";
			}
			if ( win32.cpuid & CPUID_SSE3 ) {
				string += "SSE3 & ";
			}
			if ( win32.cpuid & CPUID_HTT ) {
				string += "HTT & ";
			}
			string.StripTrailing( " & " );
			string.StripTrailing( " with " );*/

			idE.CvarSystem.SetString("sys_cpustring", capabilities);

			idConsole.WriteLine(capabilities);
			idConsole.WriteLine("{0} MB System Memory", idE.Platform.TotalPhysicalMemory);
			idConsole.WriteLine("{0} MB Video Memory", idE.Platform.TotalVideoMemory);
		}
		#endregion

		#region Command handlers
		/// <summary>
		/// Just throw a fatal error to test error shutdown procedures.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Com_Error(object sender, CommandEventArgs e)
		{
			if(idE.CvarSystem.GetBool("developer") == false)
			{
				idConsole.WriteLine("error may only be used in developer mode");
			}
			else if(e.Args.Length > 1)
			{
				idConsole.FatalError("Testing fatal error");
			}
			else
			{
				idConsole.Error("Testing drop error");
			}
		}

		private void Com_Quit(object sender, CommandEventArgs e)
		{
			Quit();
		}
		#endregion
		#endregion
	}
}