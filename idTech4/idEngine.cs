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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using idTech4.Input;
using idTech4.IO;
using idTech4.Math;
using idTech4.Renderer;
using idTech4.Services;
using idTech4.Text;
using idTech4.Threading;
using idTech4.UI;

using Keys = idTech4.Services.Keys;

namespace idTech4
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	/// <remarks>
	/// New for tech4x:
	/// <para/>
	/// Unlike previous SMP work, the actual GPU command drawing is done in the main thread, which avoids the
	/// OpenGL problems with needing windows to be created by the same thread that creates the context, as well
	/// as the issues with passing context ownership back and forth on the 360.
	/// <para/>
	/// The game tic and the generation of the draw command list is now run in a separate thread, and overlapped
	/// with the interpretation of the previous draw command list.
	/// <para/>
	/// While the game tic should be nicely contained, the draw command generation winds through the user interface
	/// code, and is potentially hazardous.  For now, the overlap will be restricted to the renderer back end,
	/// which should also be nicely contained.
	/// </remarks>
	public class idEngine : Game
	{
		#region Singleton
		public static idEngine Instance
		{
			get
			{
				return _instance;
			}
		}

		private static idEngine _instance;

		static idEngine()
		{
			_instance = new idEngine();
		}
		#endregion

		#region Properties
		#region Network
		/// <summary>
		/// Is this a multiplayer game?
		/// </summary>
		public bool IsMultiplayer
		{
			get
			{
				return false;
			}
		}
		#endregion
		
		#region Timing
		public int FrameNumber
		{
			get
			{
				return _frameNumber;
			}
		}

		/// <summary>
		/// Gets the total amount of time elapsed since the game started in milliseconds.
		/// </summary>
		public long ElapsedTime
		{
			get
			{
				return _gameTimer.ElapsedMilliseconds;
			}
		}
		#endregion
		#endregion

		#region Members
		private GraphicsDeviceManager _graphicsDeviceManager;

		private bool _firstTick = true;
		private bool _initialized;

		private int _frameNumber;
		private long _lastFrameTime;

		private string[] _rawCommandLineArguments;
		private CommandArguments[] _commandLineArguments = new CommandArguments[] { };

		private idMaterial _splashScreen;
		private idMaterial _whiteMaterial;

		// this is set if the player enables the console, which disables achievements
		private bool _consoleUsed;

		private idGameThread _gameThread;	// the game and draw code can be run in parallel

		private int	_gameFrame;				// frame number of the local game
		private double _gameTimeResidual;	// left over msec from the last game frame
		private bool _syncNextGameFrame;

		// for tracking errors
		private ErrorType _errorEntered;
		private long _lastErrorTime;
		private int _errorCount;
		private List<string> _errorList = new List<string>();

		private Stopwatch _gameTimer;
		private bool _shuttingDown;

		private bool _generateEventsEntered;

		// engine timing
		private float _engineHzLatched    = 60.0f; // latched version of cvar, updated between map loads
		private long _engineHzNumerator   = 100 * 1000;
		private long _engineHzDenominator = 100 * 60;

		private bool _isJapaneseSKU;
		
		private CurrentGame _currentGame;
		private CurrentGame _idealCurrentGame; // defer game switching so that bad things don't happen in the middle of the frame.
		private idMaterial _doomClassicMaterial;
		
		// com_speeds times
		private int	_speeds_GameFrameCount;	    // total number of game frames that were run
		private int	_speeds_GameFrame;			// game logic time
		private int	_speeds_MaxGameFrame;		// maximum single frame game logic time
		private int	_speeds_GameDraw;			// game present time
		private long _speeds_Frontend;			// renderer frontend time
		private long _speeds_Backend;			// renderer backend time
		private long _speeds_Shadows;			// renderer backend waiting for shadow volumes to be created
		private long _speeds_Gpu;				// total gpu time, at least for PC
		private long _speeds_LastTime;
		
		private bool _showShellRequested;

		private string _currentMapName;			// for checking reload on same level
		private bool _mapSpawned;				// cleared on Stop()
		#endregion

		#region Constructor
		private idEngine()
		{
			this.IsFixedTimeStep = false;
			this.Content.RootDirectory = idLicensee.BaseGameDirectory;

			_gameTimer = Stopwatch.StartNew();
			_graphicsDeviceManager = new GraphicsDeviceManager(this);
		}
		#endregion

		#region Error Handling
		public void Error(string format, params object[] args)
		{
			ErrorType code = ErrorType.Drop;

			// always turn this off after an error
			idLog.RefreshOnPrint = false;

			// retrieve the services we need for this
			ICVarSystem cvarSystem     = this.GetService<ICVarSystem>();
			ICommandSystem cmdSystem   = this.GetService<ICommandSystem>();
			IRenderSystem renderSystem = this.GetService<IRenderSystem>();

			if(cvarSystem.GetInt("com_productionMode") == 3)
			{
				Sys_Quit();
			}

			// if we don't have the renderer running, make it a fatal error
			if(renderSystem.IsInitialized == false)
			{
				code = ErrorType.Fatal;
			}

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
					Sys_Quit();
				}

				code = ErrorType.Fatal;
			}

			// if we are getting a solid stream of ERP_DROP, do an ERP_FATAL
			long currentTime = this.ElapsedTime;

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

			Stop();

			if(code == ErrorType.Disconnect)
			{
				_errorEntered = ErrorType.None;

				throw new Exception(errorMessage);
			}
			else if(code == ErrorType.Drop)
			{
				idLog.WriteLine("********************");
				idLog.WriteLine("ERROR: {0}", errorMessage);
				idLog.WriteLine("********************");

				_errorEntered = ErrorType.None;

				throw new Exception(errorMessage);
			}
			else
			{
				idLog.WriteLine("********************");
				idLog.WriteLine("ERROR: {0}", errorMessage);
				idLog.WriteLine("********************");
			}

			if(cvarSystem.GetBool("r_fullscreen") == true)
			{
				cmdSystem.BufferCommandText("vid_restart partial windowed\n", Execute.Now);
			}

			Sys_Error(errorMessage);
		}

		public void FatalError(string format, params object[] args)
		{
			ICVarSystem cvarSystem   = this.Services.GetService<ICVarSystem>();
			ICommandSystem cmdSystem = this.Services.GetService<ICommandSystem>();

			if(cvarSystem.GetInt("com_productionMode") == 3)
			{
				Sys_Quit();
			}

			// if we got a recursive error, make it fatal
			if(_errorEntered != ErrorType.None)
			{
				// if we are recursively erroring while exiting
				// from a fatal error, just kill the entire
				// process immediately, which will prevent a
				// full screen rendering window covering the
				// error dialog
				idLog.Print("FATAL: recursed fatal error:\n{0}", string.Format(format, args));

				// write the console to a log file?
				Sys_Quit();
			}

			_errorEntered = ErrorType.Fatal;

			if(cvarSystem.GetBool("r_fullscreen") == true)
			{
				cmdSystem.BufferCommandText("vid_restart partial windowed", Execute.Now);
			}

			// TODO: Sys_SetFatalError( errorMessage );
			Sys_Error(format, args);
		}
		#endregion

		#region Events
		public bool ProcessEvent(SystemEvent ev)
		{
			idLog.Warning("TODO: ProcessEvent");

			IGame game               = GetService<IGame>();
			IConsole console         = GetService<IConsole>();
			IInputSystem inputSystem = GetService<IInputSystem>();

			// hitting escape anywhere brings up the menu
			// TODO: ingame
			/*if ( game && game->IsInGame() ) {
				if ( event->evType == SE_KEY && event->evValue2 == 1 && ( event->evValue == K_ESCAPE || event->evValue == K_JOY9 ) ) {
					if ( !game->Shell_IsActive() ) {

						// menus / etc
						if ( MenuEvent( event ) ) {
							return true;
						}

						console->Close();

						StartMenu();
						return true;
					} else {
						console->Close();

						// menus / etc
						if ( MenuEvent( event ) ) {
							return true;
						}

						game->Shell_ClosePause();
					}
				} 
			}*/

			// let the pull-down console take it if desired
			if(console.ProcessEvent(ev, false) == true) 
			{
				return true;
			}

			// TODO: shell
			/*if ( session->ProcessInputEvent( event ) ) {
				return true;
			}*/
	
			// TODO: dialog
			/*if ( Dialog().IsDialogActive() ) {
				Dialog().HandleDialogEvent( event );
				return true;
			}*/

			// Let Doom classic run events.
			// TODO: classic doom
			/*if ( IsPlayingDoomClassic() ) {
				// Translate the event to Doom classic format.
				event_t classicEvent;
				if ( event->evType == SE_KEY ) {

					if( event->evValue2 == 1 ) {
						classicEvent.type = ev_keydown;
					} else if( event->evValue2 == 0 ) {
						classicEvent.type = ev_keyup;
					}

					DoomLib::SetPlayer( 0 );
			
					extern Globals * g;
					if ( g != NULL ) {
						classicEvent.data1 =  DoomLib::RemapControl( event->GetKey() );
											
						D_PostEvent( &classicEvent );
					}
					DoomLib::SetPlayer( -1 );
				}

				// Let the classics eat all events.
				return true;
			}*/

			// menus / etc
			if(ProcessMenuEvent(ev) == true)
			{
				return true;
			}

			// if we aren't in a game, force the console to take it
			if(_mapSpawned == false)
			{
				console.ProcessEvent(ev, true);
				return true;
			}

			// in game, exec bindings for all key downs
			if((ev.Type == SystemEventType.Key) && (ev.Value2 == 1))
			{
				inputSystem.ExecuteBinding((Keys) ev.Value);
				return true;
			}

			return false;
		}

		private bool ProcessMenuEvent(SystemEvent ev)
		{
			// TODO: signin manager
			/*if ( session->GetSignInManager().ProcessInputEvent( event ) ) {
				return true;
			}*/

			IGame game = GetService<IGame>();

			if((game != null) && (game.Shell_IsActive() == true))
			{
				return game.Shell_HandleGuiEvent(ev);
			}

			if(game != null)
			{
				idLog.Warning("TODO: return game->HandlePlayerGuiEvent( event );");
			}

			return false;
		}

		private void GuiFrameEvents()
		{
			IGame game = idEngine.Instance.GetService<IGame>();

			if(game != null)
			{
				game.Shell_SyncWithSession();
			}
		}
		#endregion

		#region Flow Control
		public new void Exit()
		{
			base.Exit();

			// don't try to shutdown if we are in a recursive error
			if(_errorEntered == ErrorType.None)
			{
				Shutdown();
			}

			Sys_Quit();
		}

		public void Run(string[] args)
		{
			_rawCommandLineArguments = args;

			base.Run();
		}

		private void Shutdown()
		{
			if(_shuttingDown == true)
			{
				return;
			}

			_shuttingDown = true;

			idLog.WriteLine("TODO: important! shutdown");

			/*// Kill any pending saves...
			printf( "session->GetSaveGameManager().CancelToTerminate();\n" );
			session->GetSaveGameManager().CancelToTerminate();

			// kill sound first
			printf( "soundSystem->StopAllSounds();\n" );
			soundSystem->StopAllSounds();

			// shutdown the script debugger
			// DebuggerServerShutdown();

			if ( aviCaptureMode ) {
				printf( "EndAVICapture();\n" );
				EndAVICapture();
			}

			printf( "Stop();\n" );
			Stop();

			printf( "CleanupShell();\n" );
			CleanupShell();

			printf( "delete loadGUI;\n" );
			delete loadGUI;
			loadGUI = NULL;

			printf( "delete renderWorld;\n" );
			delete renderWorld;
			renderWorld = NULL;

			printf( "delete soundWorld;\n" );
			delete soundWorld;
			soundWorld = NULL;

			printf( "delete menuSoundWorld;\n" );
			delete menuSoundWorld;
			menuSoundWorld = NULL;

			// shut down the session
			printf( "session->ShutdownSoundRelatedSystems();\n" );
			session->ShutdownSoundRelatedSystems();
			printf( "session->Shutdown();\n" );
			session->Shutdown();

			// shutdown, deallocate leaderboard definitions.
			if( game != NULL ) {
				printf( "game->Leaderboards_Shutdown();\n" );
				game->Leaderboards_Shutdown();
			}

			// shut down the user interfaces
			printf( "uiManager->Shutdown();\n" );
			uiManager->Shutdown();

			// shut down the sound system
			printf( "soundSystem->Shutdown();\n" );
			soundSystem->Shutdown();

			// shut down the user command input code
			printf( "usercmdGen->Shutdown();\n" );
			usercmdGen->Shutdown();

			// shut down the event loop
			printf( "eventLoop->Shutdown();\n" );
			eventLoop->Shutdown();
	
			// shutdown the decl manager
			printf( "declManager->Shutdown();\n" );
			declManager->Shutdown();

			// shut down the renderSystem
			printf( "renderSystem->Shutdown();\n" );
			renderSystem->Shutdown();

			printf( "commonDialog.Shutdown();\n" );
			commonDialog.Shutdown();
	
			// unload the game dll
			printf( "UnloadGameDLL();\n" );
			UnloadGameDLL();

			printf( "saveFile.Clear( true );\n" );
			saveFile.Clear( true );
			printf( "stringsFile.Clear( true );\n" );
			stringsFile.Clear( true );

			// only shut down the log file after all output is done
			printf( "CloseLogFile();\n" );
			CloseLogFile();

			// shut down the file system
			printf( "fileSystem->Shutdown( false );\n" );
			fileSystem->Shutdown( false );

			// shut down non-portable system services
			printf( "Sys_Shutdown();\n" );
			Sys_Shutdown();

			// shut down the console
			printf( "console->Shutdown();\n" );
			console->Shutdown();

			// shut down the key system
			printf( "idKeyInput::Shutdown();\n" );
			idKeyInput::Shutdown();

			// shut down the cvar system
			printf( "cvarSystem->Shutdown();\n" );
			cvarSystem->Shutdown();

			// shut down the console command system
			printf( "cmdSystem->Shutdown();\n" );
			cmdSystem->Shutdown();*/

			// free any buffered warning messages
			idLog.ClearWarnings(idLicensee.GameName + " shutdown");
		}

		/// <summary>
		/// Called on errors and game exits.
		/// </summary>
		/// <param name="resetSession"></param>
		private void Stop(bool resetSession = true)
		{
			idLog.WriteLine("TODO: Stop");

			/*ClearWipe();

			// clear mapSpawned and demo playing flags
			UnloadMap();

			soundSystem->StopAllSounds();

			insideUpdateScreen = false;
			insideExecuteMapChange = false;

			// drop all guis
			ExitMenu();

			if ( resetSession ) {
				session->QuitMatchToTitle();
			}*/
		}
		#endregion

		#region Initialization
		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// done before Com/Sys_Init since we need this for error output
			// TODO:
			/*idLog.WriteLine("TODO: Sys_CreateConsole();");

			idLog.WriteLine("TODO: optimalPCTBuffer( 0.5f );");*/

			_currentGame      = CurrentGame.Doom3BFG;
			_idealCurrentGame = CurrentGame.Doom3BFG;

			/*idLog.WriteLine("TODO: snapCurrent.localTime = -1;");
			idLog.WriteLine("TODO: snapPrevious.localTime = -1;");
			idLog.WriteLine("TODO: snapCurrent.serverTime = -1;");
			idLog.WriteLine("TODO: snapPrevious.serverTime = -1;");

			idLog.WriteLine("TODO: ClearWipe();");*/

			try
			{
				// clear warning buffer
				idLog.ClearWarnings(idLicensee.GameName + " initialization");

				// parse command line options
				ParseCommandLine(_rawCommandLineArguments);

				// init some systems
				ICVarSystem cvarSystem                     = new idCVarSystem();
				ICommandSystem cmdSystem                   = new idCommandSystem();
				IPlatformService platform                  = FindPlatform();
				IFileSystem fileSystem                     = new idFileSystem();
				ILocalization localization                 = new idLocalization();
				IInputSystem inputSystem                   = new idInputSystem();
				IConsole console                           = new idConsole();
				IDeclManager declManager                   = new idDeclManager();
				IRenderSystem renderSystem                 = new idRenderSystem(_graphicsDeviceManager);
				IResolutionScale resolutionScale           = new idResolutionScale();
				IUserInterfaceManager userInterfaceManager = new idUserInterfaceManager();
				IEventLoop eventLoop                       = new idEventLoop();
				ISession session                           = FindSession();

				this.Services.AddService(typeof(ICVarSystem), cvarSystem);
				this.Services.AddService(typeof(ICommandSystem), cmdSystem);
				this.Services.AddService(typeof(IPlatformService), platform);
				this.Services.AddService(typeof(ILocalization), localization);
				this.Services.AddService(typeof(IFileSystem), fileSystem);
				this.Services.AddService(typeof(IInputSystem), inputSystem);
				this.Services.AddService(typeof(IConsole), console);
				this.Services.AddService(typeof(IDeclManager), declManager);
				this.Services.AddService(typeof(IRenderSystem), renderSystem);
				this.Services.AddService(typeof(IResolutionScale), resolutionScale);
				this.Services.AddService(typeof(IUserInterfaceManager), userInterfaceManager);
				this.Services.AddService(typeof(ISession), session);
				this.Services.AddService(typeof(IEventLoop), eventLoop);

				cvarSystem.Initialize();
				cmdSystem.Initialize();
				platform.Initialize();
				localization.Initialize();
				fileSystem.Initialize();

				// register all static CVars
				CVars.Register();

				// scan for commands
				cmdSystem.Scan();

				idLog.WriteLine("Command line: {0}", String.Join(" ", _rawCommandLineArguments));

				idLog.WriteLine("QA Timing INIT");
				idLog.WriteLine(idVersion.ToString(platform));

				// init journalling, etc
				eventLoop.Initialize();

				// initialize key input/binding, done early so bind command exists
				// init the console so we can take prints
				inputSystem.Initialize();
				console.Initialize();

				// get architecture info
				Sys_Init();

				// initialize networking
				idLog.WriteLine("TODO: Sys_InitNetworking();");

				// override cvars from command line
				StartupVariable(null);

				_consoleUsed = cvarSystem.GetBool("com_allowConsole");

				if(Sys_AlreadyRunning() == true)
				{
					Sys_Quit();
				}

				// initialize processor specific SIMD implementation
				idLog.WriteLine("TODO: InitSIMD();");

				string defaultLang = Sys_DefaultLanguage();
				_isJapaneseSKU = defaultLang.Equals(idLanguage.Japanese, StringComparison.OrdinalIgnoreCase);

				// Allow the system to set a default lanugage
				Sys_SetLanguageFromSystem();

				// pre-allocate our 20 MB save buffer here on time, instead of on-demand for each save....
				idLog.WriteLine("TOOD: savefile pre-allocation");
				/*saveFile.SetNameAndType( SAVEGAME_CHECKPOINT_FILENAME, SAVEGAMEFILE_BINARY );
				saveFile.PreAllocate( MIN_SAVEGAME_SIZE_BYTES );

				stringsFile.SetNameAndType( SAVEGAME_STRINGS_FILENAME, SAVEGAMEFILE_BINARY );
				stringsFile.PreAllocate( MAX_SAVEGAME_STRING_TABLE_SIZE );*/

				fileSystem.BeginLevelLoad("_startup"/* TODO: , saveFile.GetDataPtr(), saveFile.GetAllocated()*/);

				// initialize the declaration manager
				declManager.Initialize();
							
				// init the parallel job manager
				idLog.WriteLine("WARNING: parallelJobManager->Init();");

				// exec the startup scripts
				cmdSystem.BufferCommandText("exec default.cfg");

				// skip the config file if "safe" is on the command line
				if((IsSafeMode() == false) && (cvarSystem.GetBool("g_demoMode") == false))
				{
					cmdSystem.BufferCommandText(string.Format("exec {0}", idLicensee.ConfigFile));
				}

				cmdSystem.BufferCommandText("exec autoexec.cfg");

				// run cfg execution
				cmdSystem.ExecuteCommandBuffer();

				// re-override anything from the config files with command line args
				StartupVariable(null);

				// if any archived cvars are modified after this, we will trigger a writing of the config file
				cvarSystem.ClearModifiedFlags(CVarFlags.Archive);

				// support up to 2 digits after the decimal point
				_engineHzDenominator = 100 * cvarSystem.GetInt64("com_engineHz");
				_engineHzLatched = cvarSystem.GetFloat("com_engineHz");

				// init renderer
				renderSystem.Initialize();

				// start the sound system, but don't do any hardware operations yet
				idLog.WriteLine("TODO: soundSystem->Init();");

				_whiteMaterial = declManager.FindMaterial("_white");

				string sysLang = cvarSystem.GetString("sys_lang");

				if(sysLang.Equals(idLanguage.French, StringComparison.OrdinalIgnoreCase) == true)
				{
					// if the user specified french, we show french no matter what SKU
					_splashScreen = declManager.FindMaterial("guis/assets/splash/legal_french");
				}
				else if(defaultLang.Equals(idLanguage.French, StringComparison.OrdinalIgnoreCase) == true)
				{
					// if the lead sku is french (ie: europe), display figs
					_splashScreen = declManager.FindMaterial("guis/assets/splash/legal_figs");
				}
				else
				{
					// otherwise show it in english
					_splashScreen = declManager.FindMaterial("guis/assets/splash/legal_english");
				}
			}
			catch(Exception ex)
			{
				throw new Exception("Uh oh!", ex);
				Sys_Error("Error during initialization");
			}

			base.Initialize();
		}

		private void Initialize2()
		{
			try
			{
				ICVarSystem cvarSystem                     = this.GetService<ICVarSystem>();
				IDeclManager declManager                   = this.GetService<IDeclManager>();
				IFileSystem fileSystem                     = this.GetService<IFileSystem>();
				IUserInterfaceManager userInterfaceManager = this.GetService<IUserInterfaceManager>();
				ISession session		                   = this.GetService<ISession>();

				int legalMinTime = 4000;
				legalMinTime = 0;
				bool showVideo   = ((cvarSystem.GetBool("com_skipIntroVideos") == false) && (fileSystem.UsingResourceFiles == true));

				if(showVideo == true)
				{
					idLog.Warning("TODO: RenderBink( \"video\\loadvideo.bik\" );");
					RenderSplash();
					RenderSplash();
				}
				else
				{
					idLog.WriteLine("Skipping Intro Videos!");

					// display the legal splash screen
					RenderSplash();
					RenderSplash();
				}

				long legalStartTime = this.ElapsedTime;

				declManager.RegisterDeclFolder("skins", ".skin", DeclType.Skin);
				declManager.RegisterDeclFolder("sound", ".sndshd", DeclType.Sound);

				// initialize string database so we can use it for loading messages
				InitLanguageDict();

				idLog.Warning("TODO: REST OF INIT");
				
				// spawn the game thread, even if we are going to run without SMP
				// one meg stack, because it can parse decls from gui surfaces (unfortunately)
				// use a lower priority so job threads can run on the same core
				_gameThread = new idGameThread();
				_gameThread.StartWorkerThread( "Game/Draw", ThreadCore.C_1B, ThreadPriority.BelowNormal, 0x100000);
				// boost this thread's priority, so it will prevent job threads from running while
				// the render back end still has work to do
				
				 idLog.Warning("TODO: Sys_SetRumble( 0, 0, 0 );");

				// initialize the user interfaces
				userInterfaceManager.Init();

				// startup the script debugger
				// DebuggerServerInit();

				// load the game dll
				LoadGameDLL();

				// On the PC touch them all so they get included in the resource build
				/*if ( !fileSystem->UsingResourceFiles() ) {
					declManager->FindMaterial( "guis/assets/splash/legal_english" );
					declManager->FindMaterial( "guis/assets/splash/legal_french" );
					declManager->FindMaterial( "guis/assets/splash/legal_figs" );
					// register the japanese font so it gets included
					renderSystem->RegisterFont( "DFPHeiseiGothicW7" );
					// Make sure all videos get touched because you can bring videos from one map to another, they need to be included in all maps
					for ( int i = 0; i < declManager->GetNumDecls( DECL_VIDEO ); i++ ) {
						declManager->DeclByIndex( DECL_VIDEO, i );
					}
				}

				fileSystem->UnloadResourceContainer( "_ordered" );

				// the same idRenderWorld will be used for all games
				// and demos, insuring that level specific models
				// will be freed
				renderWorld = renderSystem->AllocRenderWorld();
				soundWorld = soundSystem->AllocSoundWorld( renderWorld );

				menuSoundWorld = soundSystem->AllocSoundWorld( NULL );
				menuSoundWorld->PlaceListener( vec3_origin, mat3_identity, 0 );*/

				// init the session
				session.Initialize();
				/*session->InitializeSoundRelatedSystems();

				InitializeMPMapsModes();

				// leaderboards need to be initialized after InitializeMPMapsModes, which populates the MP Map list.
				if( game != NULL ) {
					game->Leaderboards_Init();
				}*/

				CreateMainMenu();
				CreateDialog();

				/*

				// load the console history file
				consoleHistory.LoadHistoryFile();*/

				AddStartupCommands();
				StartMenu(true);

				while((this.ElapsedTime - legalStartTime) < legalMinTime)
				{
					RenderSplash();
			
					Sys_GenerateEvents();
					Thread.Sleep(10);
				}

				// print all warnings queued during initialization
				/*	PrintWarnings();

					// remove any prints from the notify lines
					console->ClearNotifyLines();

					CheckStartupStorageRequirements();


					if ( preload_CommonAssets.GetBool() && fileSystem->UsingResourceFiles() ) {
						idPreloadManifest manifest;
						manifest.LoadManifest( "_common.preload" );
						globalImages->Preload( manifest, false );
						soundSystem->Preload( manifest );
					}*/

				fileSystem.EndLevelLoad();

				// initialize support for Doom classic.				
				//_doomClassicMaterial = declManager.FindMaterial("_doomClassic");
				
				/*idImage *image = globalImages->GetImage( "_doomClassic" );
				if ( image != NULL ) {
					idImageOpts opts;
					opts.format = FMT_RGBA8;
					opts.colorFormat = CFM_DEFAULT;
					opts.width = DOOMCLASSIC_RENDERWIDTH;
					opts.height = DOOMCLASSIC_RENDERHEIGHT;
					opts.numLevels = 1;
					image->AllocImage( opts, TF_LINEAR, TR_REPEAT );
				}*/

				// no longer need the splash screen
				if(_splashScreen != null)
				{
					for(int i = 0; i < _splashScreen.StageCount; i++)
					{
						idImage image = _splashScreen.GetStage(i).Texture.Image;

						if(image != null)
						{
							image.Purge();
						}
					}
				}

				idLog.WriteLine("--- Common Initialization Complete ---");
				idLog.WriteLine("QA Timing IIS: {0}ms", _gameTimer.ElapsedMilliseconds);

				// TODO:
				/*if(win32.win_notaskkeys.GetInteger())
				{
					DisableTaskKeys(TRUE, FALSE, /*( win32.win_notaskkeys.GetInteger() == 2 )*/
				/*FALSE);
	}*/

				// hide or show the early console as necessary
				/*if(win32.win_viewlog.GetInteger())
				{
					Sys_ShowConsole(1, true);
				}
				else
				{
					Sys_ShowConsole(0, false);
				}*/
				
				_initialized = true;
			}
			catch(Exception ex)
			{
				throw new Exception("Uh oh!", ex);
				Sys_Error("Error during initialization");
			}
		}

		private void CreateDialog()
		{
			IGame game     = this.GetService<IGame>();
			IDialog dialog = game.CreateDialog();

			this.Services.AddService(typeof(IDialog), dialog);
		}

		private void CreateMainMenu()
		{
			IGame game = this.GetService<IGame>();

			if(game != null)
			{
				IDeclManager declManager                   = this.GetService<IDeclManager>();
				IRenderSystem renderSystem                 = this.GetService<IRenderSystem>();
				// TODO: soundSystem->BeginLevelLoad();
				IUserInterfaceManager userInterfaceManager = this.GetService<IUserInterfaceManager>();

				// note which media we are going to need to load
				declManager.BeginLevelLoad();
				renderSystem.BeginLevelLoad();
				// TODO: soundSystem->BeginLevelLoad();
				userInterfaceManager.BeginLevelLoad();

				// create main inside an "empty" game level load - so assets get
				// purged automagically when we transition to a "real" map
				game.Shell_CreateMenu(false);
				game.Shell_Show(true);
				game.Shell_SyncWithSession();

				// load
				renderSystem.EndLevelLoad();
				// TODO: soundSystem->EndLevelLoad();
				declManager.EndLevelLoad();
				userInterfaceManager.EndLevelLoad("");
			}
		}

		private void StartMenu(bool playIntro)
		{
			IGame game       = this.GetService<IGame>();
			IConsole console = this.GetService<IConsole>();

			if((game != null) && (game.Shell_IsActive() == true))
			{
				return;
			}
			
			// TODO: readDemo
			/*if ( readDemo ) {
				// if we're playing a demo, esc kills it
				UnloadMap();
			}*/

			game.Shell_Show(true);
			game.Shell_SyncWithSession();

			console.Close();
		}

		/// <summary>
		/// Check for "safe" on the command line, which will
		/// skip loading of config file (DoomConfig.cfg)
		/// </summary>
		private bool IsSafeMode()
		{
			foreach(CommandArguments args in _commandLineArguments)
			{
				if((args.Get(0).Equals("safe", StringComparison.OrdinalIgnoreCase) == true)
					|| (args.Get(0).Equals("cvar_restart", StringComparison.OrdinalIgnoreCase) == true))
				{
					args.Clear();
					return true;
				}
			}

			return false;
		}

		private void InitLanguageDict()
		{
			ICommandSystem cmdSystem = GetService<ICommandSystem>();
			ICVarSystem cvarSystem   = GetService<ICVarSystem>();
			IFileSystem fileSystem   = GetService<IFileSystem>();
			ILocalization loc        = GetService<ILocalization>();

			// D3XP: Instead of just loading a single lang file for each language
			// we are going to load all files that begin with the language name
			// similar to the way pak files work. So you can place english001.lang
			// to add new strings to the english language dictionary
			idFileList files   = fileSystem.ListFiles("strings", ".lang", true);
			string[] langFiles = files.Files;
			string langName    = cvarSystem.GetString("sys_lang");

			// loop through the list and filter
			string[] currentLanguageList = langFiles.Where(c => c.StartsWith(langName)).ToArray();

			if(currentLanguageList.Length == 0)
			{
				// reset to english and try to load again
				cvarSystem.Set("sys_lang", idLanguage.English);

				langName            = cvarSystem.GetString("sys_lang");
				currentLanguageList = langFiles.Where(c => c.StartsWith(langName)).ToArray();
			}

			loc.Clear();

			foreach(string lang in currentLanguageList)
			{
				byte[] buffer = fileSystem.ReadFile(Path.Combine("strings", lang));

				if(buffer != null)
				{
					loc.Load(Encoding.UTF8.GetString(buffer), lang);
				}
			}
		}

		private void LoadGameDLL()
		{
			IFileSystem fileSystem = this.GetService<IFileSystem>();
			ICVarSystem cvarSystem = this.GetService<ICVarSystem>();

			// from executable directory first - this is handy for developement
			string dllName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			dllName        = Path.Combine(dllName, "idTech4.Game.dll");

			if(File.Exists(dllName) == false)
			{
				dllName = null;
			}

			if(dllName == null)
			{
				dllName = fileSystem.GetAbsolutePath(null, idLicensee.BaseGameDirectory, "idTech4.Game.dll");
			}

			// register some extra assembly paths
			AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs e) => {
				string name = e.Name;

				if(name.IndexOf(",") != -1)
				{
					name = e.Name.Substring(0, e.Name.IndexOf(","));
				}

				if(name.EndsWith(".dll") == false)
				{
					name = name + ".dll";
				}

				return Assembly.LoadFile(fileSystem.GetAbsolutePath(fileSystem.BasePath, idLicensee.BaseGameDirectory, name));
			};
						
			idLog.WriteLine("Loading game DLL: '{0}'", dllName);

			Assembly asm = Assembly.LoadFile(Path.GetFullPath(dllName));

			IGame game         = (IGame) asm.CreateInstance("idTech4.Game.idGame");
			IGameEdit gameEdit = (IGameEdit) asm.CreateInstance("idTech4.Game.idGameEdit");

			this.Services.AddService(typeof(IGame), game);
			this.Services.AddService(typeof(IGameEdit), gameEdit);

			game.Init();
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			base.OnExiting(sender, args);

			Shutdown();
		}

		private IPlatformService FindPlatform()
		{
			// TODO: clean this up
#if WINDOWS || LINUX
			string assemblyName = "idTech4.Platform.PC.dll";
			string typeName     = "idTech4.Platform.PC.PCPlatform";
#else
			return null;
#endif

			assemblyName = Path.Combine(Environment.CurrentDirectory, assemblyName);

			return Assembly.LoadFile(assemblyName).CreateInstance(typeName) as IPlatformService;
		}

		private ISession FindSession()
		{
			// TODO: clean this up
#if WINDOWS || LINUX
			string assemblyName = "idTech4.Platform.PC.dll";
			string typeName     = "idTech4.Platform.PC.PCSession";
#else
			return null;
#endif

			assemblyName = Path.Combine(Environment.CurrentDirectory, assemblyName);

			return Assembly.LoadFile(assemblyName).CreateInstance(typeName) as ISession;
		}

		/// <summary>
		/// Adds command line parameters as script statements commands are separated by + signs
		/// </summary>
		/// <remarks>
		/// Returns true if any late commands were added, which will keep the demoloop from immediately starting.
		/// </remarks>
		private void AddStartupCommands()
		{
			ICommandSystem cmdSystem = this.GetService<ICommandSystem>();

			foreach(CommandArguments args in _commandLineArguments)
			{
				if(args.Length == 0)
				{
					return;
				}

				// directly as tokenized so nothing gets screwed
				cmdSystem.BufferCommandArgs(args, Execute.Append);
			}
		}

		private void ParseCommandLine(string[] args)
		{
			List<CommandArguments> argList = new List<CommandArguments>();
			CommandArguments current       = null;

			foreach(string arg in args)
			{
				if(arg.StartsWith("+") == true)
				{
					current = new CommandArguments();
					current.AppendArg(arg.Substring(1));

					argList.Add(current);
				}
				else
				{
					if(current == null)
					{
						current = new CommandArguments();
						argList.Add(current);
					}

					current.AppendArg(arg);
				}
			}

			_commandLineArguments = argList.ToArray();
		}

		private void PerformGameSwitch()
		{
			// if the session state is past the menu, we should be in Doom 3.
			// this will happen if, for example, we accept an invite while playing
			// Doom or Doom 2.
			ISession session       = GetService<ISession>();
			ICVarSystem cvarSystem = GetService<ICVarSystem>();

			if(session.State > SessionState.Idle)
			{
				_idealCurrentGame = CurrentGame.Doom3BFG;
			}

			if(_currentGame == _idealCurrentGame)
			{
				return;
			}
			
			if((_idealCurrentGame == CurrentGame.DoomClassic) || (_idealCurrentGame == CurrentGame.Doom2Classic))
			{
				idLog.Warning("TODO: switch to doom classic");

				// Pause Doom 3 sound.
				/*if ( menuSoundWorld != NULL ) {
					menuSoundWorld->Pause();
				}

				DoomLib::skipToNew = false;
				DoomLib::skipToLoad = false;

				// Reset match parameters for the classics.
				DoomLib::matchParms = idMatchParameters();

				// The classics use the usercmd manager too, clear it.
				userCmdMgr.SetDefaults();

				// Classics need a local user too.
				session->UpdateSignInManager();
				session->GetSignInManager().RegisterLocalUser( 0 );

				com_engineHz_denominator = 100LL * DOOM_CLASSIC_HZ;
				com_engineHz_latched = DOOM_CLASSIC_HZ;

				DoomLib::SetCurrentExpansion( idealCurrentGame );*/
			}
			else if(_idealCurrentGame == CurrentGame.Doom3BFG)
			{
				idLog.Warning("TODO: DoomLib::Interface.Shutdown();");

				_engineHzDenominator = (long) (100L * cvarSystem.GetFloat("com_engineHz"));
				_engineHzLatched     = cvarSystem.GetFloat("com_engineHz");
		
				// don't MoveToPressStart if we have an invite, we need to go directly to the lobby
				if(session.State <= SessionState.Idle)
				{
					session.MoveToPressStart();
				}

				// unpause Doom 3 sound
				idLog.Warning("TODO: unpause sound");
				
				/*if ( menuSoundWorld != NULL ) {
					menuSoundWorld->UnPause();
				}*/
			}

			_currentGame = _idealCurrentGame;
		}

		private void RenderSplash()
		{
			IRenderSystem renderSystem = GetService<IRenderSystem>();

			float sysWidth     = renderSystem.Width * renderSystem.PixelAspect;
			float sysHeight    = renderSystem.Height;
			float sysAspect    = sysWidth / sysHeight;
			float splashAspect = 16.0f / 9.0f;
			float adjustment   = sysAspect / splashAspect;
			float barHeight    = (adjustment >= 1.0f) ? 0.0f : (1.0f - adjustment) * (float) Constants.ScreenHeight * 0.25f;
			float barWidth     = (adjustment <= 1.0f) ? 0.0f : (adjustment - 1.0f) * (float) Constants.ScreenWidth * 0.25f;

			if(barHeight > 0.0f)
			{
				renderSystem.Color = idColor.Black;
				renderSystem.DrawStretchPicture(0, 0, Constants.ScreenWidth, barHeight, 0, 0, 1, 1, _whiteMaterial);
				renderSystem.DrawStretchPicture(0, Constants.ScreenHeight - barHeight, Constants.ScreenWidth, barHeight, 0, 0, 1, 1, _whiteMaterial);
			}

			if(barWidth > 0.0f)
			{
				renderSystem.Color = idColor.Black;
				renderSystem.DrawStretchPicture(0, 0, barWidth, Constants.ScreenHeight, 0, 0, 1, 1, _whiteMaterial);
				renderSystem.DrawStretchPicture(Constants.ScreenWidth - barWidth, 0, barWidth, Constants.ScreenHeight, 0, 0, 1, 1, _whiteMaterial);
			}

			renderSystem.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			renderSystem.DrawStretchPicture(barWidth, barHeight, Constants.ScreenWidth - barWidth * 2.0f, Constants.ScreenHeight - barHeight * 2.0f, 0, 0, 1, 1, _splashScreen);
			
			LinkedListNode<idRenderCommand> cmd = renderSystem.SwapCommandBuffers(out _speeds_Frontend, out _speeds_Backend, out _speeds_Shadows, out _speeds_Gpu);
			renderSystem.RenderCommandBuffers(cmd);
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
		public void StartupVariable(string match)
		{
			List<CommandArguments> final = new List<CommandArguments>();

			foreach(CommandArguments args in _commandLineArguments)
			{
				if(args.Get(0).ToLower() != "set")
				{
					continue;
				}

				string s = args.Get(1);

				if((match == null) || (s.Equals(match, StringComparison.OrdinalIgnoreCase) == true))
				{
					this.GetService<ICVarSystem>().Set(s, args.Get(2));
				}
			}
		}
		#endregion

		#region System
		public T GetService<T>() where T : class
		{
			return this.Services.GetService<T>();
		}

		/// <summary>
		/// Checks if a copy of D3 is running already.
		/// </summary>
		/// <returns></returns>
		private bool Sys_AlreadyRunning()
		{
#if !DEBUG
			if(GetService<ICVarSystem>().GetBool("win_allowMultipleInstances") == false)
			{
				bool created;
				_appMutex = new Mutex(false, "DOOM3", out created);

				if(created == false)
				{
					return true;
				}
			}
#endif

			return false;
		}

		private string Sys_DefaultLanguage()
		{
			// sku breakdowns are as follows
			//  EFIGS	Digital
			//  EF  S	North America
			//   FIGS	EU
			//  E		UK
			// JE    	Japan

			// If japanese exists, default to japanese
			// else if english exists, defaults to english
			// otherwise, french
			return idLanguage.English;
		}

		/// <summary>
		/// Show the early console as an error dialog.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		private void Sys_Error(string format, params object[] args)
		{
			string errorMessage = string.Format(format, args);

			idLog.WriteLine("=========================================");
			idLog.WriteLine("ERROR: {0}", errorMessage);
			idLog.WriteLine("=========================================");

			idLog.WriteLine("TODO: systemConsole");
			// TODO: idE.SystemConsole.Append(errorMessage + Environment.NewLine);
			// TODO: idE.SystemConsole.Show(1, true);

			_gameTimer.Stop();
			// TODO: Sys_ShutdownInput();

			ICVarSystem cvarSystem = this.GetService<ICVarSystem>();

			if((cvarSystem != null) && (cvarSystem.GetInt("com_productionMode") == 0))
			{
				// wait for the user to quit
				// TODO: important!
				/*while(true)
				{
					// TODO: if(idE.SystemConsole.IsDisposed == true)
					{
						Exit();
						break;
					}

					Application.DoEvents();
					Thread.Sleep(0);
				}*/
			}

			// TODO: Sys_DestroyConsole();

			Environment.Exit(1);
		}

		private void Sys_GenerateEvents() 
		{
			if(_generateEventsEntered == true)
			{
				return;
			}

			_generateEventsEntered = true;

			// pump the message loop
			//Sys_PumpEvents();

			// grab or release the mouse cursor if necessary
			idEngine.Instance.GetService<IInputSystem>().ProcessFrame();

			// check for console commands
			// TODO: console commands
			/*s = Sys_ConsoleInput();
			if ( s ) {
				char	*b;
				int		len;

				len = strlen( s ) + 1;
				b = (char *)Mem_Alloc( len, TAG_EVENTS );
				strcpy( b, s );
				Sys_QueEvent( SE_CONSOLE, 0, 0, len, b, 0 );
			}*/

			_generateEventsEntered = false;
		}

		private void Sys_Init()
		{
			ICVarSystem cvarSystem    = GetService<ICVarSystem>();
			IPlatformService platform = GetService<IPlatformService>();

			// not bothering with fetching the windows username.
			cvarSystem.Set("win_username", Environment.UserName);

			//
			// Windows version
			//
			OperatingSystem osInfo = Environment.OSVersion;

			if((osInfo.Version.Major < 4)
				|| (osInfo.Platform == PlatformID.Win32S)
				|| (osInfo.Platform == PlatformID.Win32Windows))
			{
				Error("{0} requires Windows XP or above", idLicensee.GameName);
			}
			else if(osInfo.Platform == PlatformID.Win32NT)
			{
				if(osInfo.Version.Major <= 4)
				{
					cvarSystem.Set("sys_arch", "WinNT (NT)");
				}
				else if((osInfo.Version.Major == 5) && (osInfo.Version.Minor == 0))
				{
					cvarSystem.Set("sys_arch", "Win2K (NT)");
				}
				else if((osInfo.Version.Major == 5) && (osInfo.Version.Minor == 1))
				{
					cvarSystem.Set("sys_arch", "WinXP (NT)");
				}
				else if((osInfo.Version.Major == 6) && (osInfo.Version.Minor == 0))
				{
					cvarSystem.Set("sys_arch", "Vista");
				}
				else if((osInfo.Version.Major == 6) && (osInfo.Version.Minor == 1))
				{
					cvarSystem.Set("sys_arch", "Windows 7");
				}
				else
				{
					cvarSystem.Set("sys_arch", "Unknown NT variant");
				}
			}

			//
			// CPU type
			//			
			if(cvarSystem.GetString("sys_cpustring").Equals("detect", StringComparison.OrdinalIgnoreCase) == true)
			{
				if(Environment.OSVersion.Version.Major >= 6)
				{
					idLog.WriteLine("{0} MHz, {1} cores, {2} threads", platform.ClockSpeed, platform.CoreCount, platform.ThreadCount);
				}
				else
				{
					idLog.WriteLine("{0} MHz", platform.ClockSpeed);
				}

				CpuCapabilities caps = platform.CpuCapabilities;
				string capabilities  = string.Empty;

				if((caps & CpuCapabilities.AMD) == CpuCapabilities.AMD)
				{
					capabilities += "AMD CPU";
				}
				else if((caps & CpuCapabilities.Intel) == CpuCapabilities.Intel)
				{
					capabilities += "Intel CPU";
				}
				else if((caps & CpuCapabilities.Unsupported) == CpuCapabilities.Unsupported)
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

				cvarSystem.Set("sys_cpustring", capabilities);
			}

			idLog.WriteLine(cvarSystem.GetString("sys_cpustring"));
			idLog.WriteLine("{0} MB system memory", platform.TotalPhysicalMemory);
			idLog.WriteLine("{0} MB video memory", platform.TotalVideoMemory);
		}

		private void Sys_SetLanguageFromSystem()
		{
			GetService<ICVarSystem>().Set("sys_lang", Sys_DefaultLanguage());
		}

		private void Sys_Quit()
		{
			_gameTimer.Stop();

			// TODO: Sys_ShutdownInput();
			// TODO: Sys_DestroyConsole();

			Environment.Exit(0);
		}
		#endregion

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// FIXME: this is a hack to get the render window up so we can show the loading messages.
			// it doesn't usually come up until all initialization has been completed and one tick has been run.
			// this causes none of the loading messages to appear and it looks like the program isn't loading!
			if(_firstTick == true)
			{
				_firstTick     = false;
				_lastFrameTime = this.ElapsedTime;

				base.Update(gameTime);
				return;
			}
			else if(_initialized == false)
			{
				Initialize2();
				base.Update(gameTime);
				return;
			}

			IRenderSystem renderSystem = this.GetService<IRenderSystem>();
			ICVarSystem cvarSystem     = this.GetService<ICVarSystem>();
			IInputSystem inputSystem   = this.GetService<IInputSystem>();
			ISession session           = this.GetService<ISession>();
			IGame game                 = this.GetService<IGame>();
			IDialog dialog             = this.GetService<IDialog>();
			IEventLoop eventLoop       = this.GetService<IEventLoop>();

			LinkedListNode<idRenderCommand> renderCommands = null;

			/*try*/
			{
				// TODO: SCOPED_PROFILE_EVENT( "Common::Frame" );

				// This is the only place this is incremented
				_frameNumber++;

				// allow changing SIMD usage on the fly
				if(cvarSystem.IsModified("com_forceGenericSIMD") == true)
				{
					idLog.Warning("TODO: idSIMD::InitProcessor( \"doom\", com_forceGenericSIMD.GetBool() );");

					cvarSystem.ClearModified("com_forceGenericSIMD");
				}

				// Do the actual switch between Doom 3 and the classics here so
				// that things don't get confused in the middle of the frame.
				PerformGameSwitch();

				// pump all the events
				Sys_GenerateEvents();

				// write config file if anything changed
				// TODO: WriteConfiguration(); 

				eventLoop.RunEventLoop();

				// activate the shell if it's been requested
				if((_showShellRequested == true) && (game != null))
				{
					game.Shell_Show(true);
					_showShellRequested = false;
				}

				// if the console or another gui is down, we don't need to hold the mouse cursor
				/*bool chatting = false;
				if ( console->Active() || Dialog().IsDialogActive() || session->IsSystemUIShowing() || ( game && game->InhibitControls() && !IsPlayingDoomClassic() ) ) {
					Sys_GrabMouseCursor( false );
					usercmdGen->InhibitUsercmd( INHIBIT_SESSION, true );
					chatting = true;
				} else {
					Sys_GrabMouseCursor( true );
					usercmdGen->InhibitUsercmd( INHIBIT_SESSION, false );
				}

				const bool pauseGame = ( !mapSpawned || ( !IsMultiplayer() && ( Dialog().IsDialogPausing() || session->IsSystemUIShowing() || ( game && game->Shell_IsActive() ) ) ) ) && !IsPlayingDoomClassic();
				*/
				bool pauseGame = false;
				/*
				// save the screenshot and audio from the last draw if needed
				if ( aviCaptureMode ) {
					idStr name = va("demos/%s/%s_%05i.tga", aviDemoShortName.c_str(), aviDemoShortName.c_str(), aviDemoFrameCount++ );
					renderSystem->TakeScreenshot( com_aviDemoWidth.GetInteger(), com_aviDemoHeight.GetInteger(), name, com_aviDemoSamples.GetInteger(), NULL );

					// remove any printed lines at the top before taking the screenshot
					console->ClearNotifyLines();

					// this will call Draw, possibly multiple times if com_aviDemoSamples is > 1
					renderSystem->TakeScreenshot( com_aviDemoWidth.GetInteger(), com_aviDemoHeight.GetInteger(), name, com_aviDemoSamples.GetInteger(), NULL );
				}*/

				//--------------------------------------------
				// wait for the GPU to finish drawing
				//
				// It is imporant to minimize the time spent between this
				// section and the call to renderSystem->RenderCommandBuffers(),
				// because the GPU is completely idle.
				//--------------------------------------------
				// this should exit right after vsync, with the GPU idle and ready to draw
				// This may block if the GPU isn't finished renderng the previous frame.
				// TODO: frametiming
				/*frameTiming.startSyncTime = Sys_Microseconds();*/

				ulong timeFrontend, timeBackend, timeShadows, timeGPU;

				if(cvarSystem.GetBool("com_smp") == true)
				{
					renderCommands = renderSystem.SwapCommandBuffers(out _speeds_Frontend, out _speeds_Backend, out _speeds_Shadows, out _speeds_Gpu);
				}
				else
				{
					// the GPU will stay idle through command generation for minimal input latency
					renderSystem.SwapCommandBuffers_FinishRendering(out _speeds_Frontend, out _speeds_Backend, out _speeds_Shadows, out _speeds_Gpu);
				}

				/*frameTiming.finishSyncTime = Sys_Microseconds();	*/

				//--------------------------------------------
				// Determine how many game tics we are going to run,
				// now that the previous frame is completely finished.
				//
				// It is important that any waiting on the GPU be done
				// before this, or there will be a bad stuttering when
				// dropping frames for performance management.
				//--------------------------------------------

				// input:
				// thisFrameTime
				// com_noSleep
				// com_engineHz
				// com_fixedTic
				// com_deltaTimeClamp
				// IsMultiplayer
				//
				// in/out state:
				// gameFrame
				// gameTimeResidual
				// lastFrameTime
				// syncNextFrame
				//
				// Output:
				// numGameFrames

				// How many game frames to run
				int gameFrameCount = 0;

				for(; ; )
				{
					long thisFrameTime = this.ElapsedTime;
					long deltaMilliseconds = thisFrameTime - _lastFrameTime;
					_lastFrameTime = thisFrameTime;

					// if there was a large gap in time since the last frame, or the frame
					// rate is very very low, limit the number of frames we will run
					int clampedDeltaMilliseconds = (int) idMath.Min((float) deltaMilliseconds, cvarSystem.GetFloat("com_deltaTimeClamp"));

					_gameTimeResidual += clampedDeltaMilliseconds * cvarSystem.GetFloat("timescale");

					// don't run any frames when paused
					if(pauseGame)
					{
						_gameFrame++;
						_gameTimeResidual = 0;

						break;
					}

					// debug cvar to force multiple game tics
					if(cvarSystem.GetInt("com_fixedTic") > 0)
					{
						gameFrameCount = cvarSystem.GetInt("com_fixedTic");

						_gameFrame += gameFrameCount;
						_gameTimeResidual = 0;

						break;
					}

					if(_syncNextGameFrame == true)
					{
						// don't sleep at all
						_syncNextGameFrame = false;
						_gameTimeResidual = 0;
						_gameFrame++;
						gameFrameCount++;

						break;
					}

					for(; ; )
					{
						// how much time to wait before running the next frame, based on com_engineHz
						int frameDelay = idHelper.FrameToMillsecond(_gameFrame + 1) - idHelper.FrameToMillsecond(_gameFrame);

						if(_gameTimeResidual < frameDelay)
						{
							break;
						}

						_gameTimeResidual -= frameDelay;
						_gameFrame++;
						gameFrameCount++;

						// if there is enough residual left, we may run additional frames
					}

					if(gameFrameCount > 0)
					{
						// ready to actually run them
						break;
					}

					// if we are vsyncing, we always want to run at least one game
					// frame and never sleep, which might happen due to scheduling issues
					// if we were just looking at real time.
					if(cvarSystem.GetBool("com_noSleep") == true)
					{
						gameFrameCount = 1;
						_gameFrame += gameFrameCount;
						_gameTimeResidual = 0;

						break;
					}

					// not enough time has passed to run a frame, as might happen if
					// we don't have vsync on, or the monitor is running at 120hz while
					// com_engineHz is 60, so sleep a bit and check again
					Thread.Sleep(0);
				}

				//--------------------------------------------
				// It would be better to push as much of this as possible
				// either before or after the renderSystem->SwapCommandBuffers(),
				// because the GPU is completely idle.
				//--------------------------------------------

				// Update session and syncronize to the new session state after sleeping
				session.UpdateSignInManager();
				session.Pump();
				session.ProcessSnapAckQueue();

				if(session.State == SessionState.Loading)
				{
					idLog.Warning("TODO: sessionState loading");

					// If the session reports we should be loading a map, load it!
					/*ExecuteMapChange();
					mapSpawnData.savegameFile = NULL;
					mapSpawnData.persistentPlayerInfo.Clear();*/
					return;
				} 
				else if((session.State != SessionState.InGame) && (_mapSpawned == true))
				{
					idLog.Warning("TODO: sessionState ingame");

					// If the game is running, but the session reports we are not in a game, disconnect
					// This happens when a server disconnects us or we sign out
					//LeaveGame();
					return;
				}

				if((_mapSpawned == true) && (pauseGame == false))
				{
					idLog.Warning("TODO: runNetworkSnapshotFrame");

					/*if ( IsClient() ) {
						RunNetworkSnapshotFrame();
					}*/
				}

				/*ExecuteReliableMessages();*/

				// send frame and mouse events to active guis
				GuiFrameEvents();

				//--------------------------------------------
				// Prepare usercmds and kick off the game processing
				// in a background thread
				//--------------------------------------------

				// get the previous usercmd for bypassed head tracking transform
				/*const usercmd_t	previousCmd = usercmdGen->GetCurrentUsercmd();

				// build a new usercmd
				int deviceNum = session->GetSignInManager().GetMasterInputDevice();*/
				int deviceNum = 0;

				inputSystem.BuildCurrentUserCommand(deviceNum);

				/*if ( deviceNum == -1 ) {
					for ( int i = 0; i < MAX_INPUT_DEVICES; i++ ) {
						Sys_PollJoystickInputEvents( i );
						Sys_EndJoystickInputEvents();
					}
				}*/

				if(pauseGame == true)
				{
					inputSystem.ClearGenerated();
				}
				
				idUserCommand newCmd = inputSystem.CurrentUserCommand;

				// store server game time - don't let time go past last SS time in case we are extrapolating
				/*if ( IsClient() ) {
					newCmd.serverGameMilliseconds = std::min( Game()->GetServerGameTimeMs(), Game()->GetSSEndTime() );
				} else {
					newCmd.serverGameMilliseconds = Game()->GetServerGameTimeMs();
				}*/

				// TODO: userCmdMgr.MakeReadPtrCurrentForPlayer(game.LocalClientNumber);

				// stuff a copy of this userCmd for each game frame we are going to run.
				// ideally, the usercmds would be built in another thread so you could
				// still get 60hz control accuracy when the game is running slower.

				// TODO: user command injection
				/*for ( int i = 0 ; i < numGameFrames ; i++ ) {
					newCmd.clientGameMilliseconds = FRAME_TO_MSEC( *gameFrame-numGameFrames+i+1 );
					userCmdMgr.PutUserCmdForPlayer(game.LocalClientNumber, newCmd );
				}*/

				// if we're in Doom or Doom 2, run tics and upload the new texture.
				// TODO:
				/*if ( ( GetCurrentGame() == DOOM_CLASSIC || GetCurrentGame() == DOOM2_CLASSIC ) && !( Dialog().IsDialogPausing() || session->IsSystemUIShowing() ) ) {
					RunDoomClassicFrame();
				}*/
		
				// start the game / draw command generation thread going in the background
				GameReturn ret = _gameThread.RunGameAndDraw(gameFrameCount, /*userCmdMgr*/ null, /*TODO: IsClient()*/ true, _gameFrame - gameFrameCount);

				if(cvarSystem.GetBool("com_smp") == false)
				{
					// in non-smp mode, run the commands we just generated, instead of
					// frame-delayed ones from a background thread
					renderCommands = renderSystem.SwapCommandBuffers_FinishCommandBuffers();
				}

				//----------------------------------------
				// Run the render back end, getting the GPU busy with new commands
				// ASAP to minimize the pipeline bubble.
				//----------------------------------------
				//frameTiming.startRenderTime = Sys_Microseconds();

				renderSystem.RenderCommandBuffers(renderCommands);

				if(cvarSystem.GetInt("com_sleepRender") > 0)
				{
					// debug tool to test frame adaption
					Thread.Sleep(cvarSystem.GetInt("com_sleepRender"));
				}

				// frameTiming.finishRenderTime = Sys_Microseconds();

				// make sure the game / draw thread has completed
				// This may block if the game is taking longer than the render back end
				_gameThread.WaitForThread();

				// Send local usermds to the server.
				// This happens after the game frame has run so that prediction data is up to date.
				/*SendUsercmds( Game()->GetLocalClientNum() );*/

				// Now that we have an updated game frame, we can send out new snapshots to our clients
				session.Pump(); // Pump to get updated usercmds to relay
				/*SendSnapshots();

				// Render the sound system using the latest commands from the game thread
				if ( pauseGame ) {
					soundWorld->Pause();
					soundSystem->SetPlayingSoundWorld( menuSoundWorld );
				} else {
					soundWorld->UnPause();
					soundSystem->SetPlayingSoundWorld( soundWorld );
				}
				soundSystem->Render();

				// process the game return for map changes, etc
				ProcessGameReturn( ret );

				idLobbyBase & lobby = session->GetActivePlatformLobbyBase();
				if ( lobby.HasActivePeers() ) {
					if ( net_drawDebugHud.GetInteger() == 1 ) {
						lobby.DrawDebugNetworkHUD();
					}
					if ( net_drawDebugHud.GetInteger() == 2 ) {
						lobby.DrawDebugNetworkHUD2();
					}
					lobby.DrawDebugNetworkHUD_ServerSnapshotMetrics( net_drawDebugHud.GetInteger() == 3 );
				}*/

				// report timing information
				if(cvarSystem.GetBool("com_speeds") == true)
				{
					long nowTime     = this.ElapsedTime;
					long frameMsec   = nowTime - _speeds_LastTime;
					_speeds_LastTime = nowTime;

					idLog.WriteLine("frame:{0} all:{1:000} gfr:{2} rf:{3:000} bk:{4:000}", _frameNumber, frameMsec, _speeds_GameFrame, (_speeds_Frontend / 1000), (_speeds_Backend / 1000));
					
					_speeds_GameFrame = 0;
					_speeds_GameDraw  = 0;
				}
			
				// the FPU stack better be empty at this point or some bad code or compiler bug left values on the stack
				/*if ( !Sys_FPU_StackIsEmpty() ) {
					Printf( Sys_FPU_GetState() );
					FatalError( "idCommon::Frame: the FPU stack is not empty at the end of the frame\n" );
				}

				mainFrameTiming = frameTiming;

				session->GetSaveGameManager().Pump();*/
			}
			/*catch
			{
				return;			// an ERP_DROP was thrown
			}*/
		}

		public void Draw()
		{
			ICVarSystem cvarSystem      = this.GetService<ICVarSystem>();
			IRenderSystem renderSystem  = this.GetService<IRenderSystem>();
			IGame game                  = this.GetService<IGame>();
			IConsole console            = this.GetService<IConsole>();

			// debugging tool to test frame dropping behavior
			if(cvarSystem.GetInt("com_sleepDraw") > 0)
			{
				Thread.Sleep(cvarSystem.GetInt("com_sleepDraw"));
			}

			// TODO: loadGui
			/*if ( loadGUI != NULL ) 
			{
				loadGUI->Render( renderSystem, Sys_Milliseconds() );
			} */
			// TODO: doom classic
			/*else if (	currentGame == DOOM_CLASSIC || currentGame == DOOM2_CLASSIC ) 
			{
				const float sysWidth = renderSystem->GetWidth() * renderSystem->GetPixelAspect();
				const float sysHeight = renderSystem->GetHeight();
				const float sysAspect = sysWidth / sysHeight;
				const float doomAspect = 4.0f / 3.0f;
				const float adjustment = sysAspect / doomAspect;
				const float barHeight = ( adjustment >= 1.0f ) ? 0.0f : ( 1.0f - adjustment ) * (float)SCREEN_HEIGHT * 0.25f;
				const float barWidth = ( adjustment <= 1.0f ) ? 0.0f : ( adjustment - 1.0f ) * (float)SCREEN_WIDTH * 0.25f;
				if ( barHeight > 0.0f ) {
					renderSystem->SetColor( colorBlack );
					renderSystem->DrawStretchPic( 0, 0, SCREEN_WIDTH, barHeight, 0, 0, 1, 1, whiteMaterial );
					renderSystem->DrawStretchPic( 0, SCREEN_HEIGHT - barHeight, SCREEN_WIDTH, barHeight, 0, 0, 1, 1, whiteMaterial );
				}
				if ( barWidth > 0.0f ) {
					renderSystem->SetColor( colorBlack );
					renderSystem->DrawStretchPic( 0, 0, barWidth, SCREEN_HEIGHT, 0, 0, 1, 1, whiteMaterial );
					renderSystem->DrawStretchPic( SCREEN_WIDTH - barWidth, 0, barWidth, SCREEN_HEIGHT, 0, 0, 1, 1, whiteMaterial );
				}
				renderSystem->SetColor4( 1, 1, 1, 1 );
				renderSystem->DrawStretchPic( barWidth, barHeight, SCREEN_WIDTH - barWidth * 2.0f, SCREEN_HEIGHT - barHeight * 2.0f, 0, 0, 1, 1, doomClassicMaterial );
			}*/
			else if((game != null) && (game.Shell_IsActive() == true))
			{
				bool gameDraw = game.Draw(game.LocalClientNumber);

				if(gameDraw == false)
				{
					renderSystem.Color = idColor.Black;
					renderSystem.DrawStretchPicture(0, 0, Constants.ScreenWidth, Constants.ScreenHeight, 0, 0, 1, 1, _whiteMaterial);
				}

				game.Shell_Render();
			} 
			// TODO: readDemo
			/*else if(readDemo == true) 
			{
				renderWorld->RenderScene( &currentDemoRenderView );
				renderSystem->DrawDemoPics();
			} */
			else if(_mapSpawned == true)
			{
				idLog.Warning("TODO: mapSpawned");

				/*bool gameDraw = false;
				// normal drawing for both single and multi player
				if ( !com_skipGameDraw.GetBool() && Game()->GetLocalClientNum() >= 0 ) {
					// draw the game view
					int	start = Sys_Milliseconds();
					if ( game ) {
						gameDraw = game->Draw( Game()->GetLocalClientNum() );
					}
					int end = Sys_Milliseconds();
					time_gameDraw += ( end - start );	// note time used for com_speeds
				}
				if ( !gameDraw ) {
					renderSystem->SetColor( colorBlack );
					renderSystem->DrawStretchPic( 0, 0, 640, 480, 0, 0, 1, 1, whiteMaterial );
				}

				// save off the 2D drawing from the game
				if ( writeDemo ) {
					renderSystem->WriteDemoPics();
				}*/
			} 
			else 
			{
				idLog.Warning("DRAWING NOOOOOOOOOOOOOOOOOOOOOOTHIN");
				renderSystem.Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
				renderSystem.DrawStretchPicture(0, 0, Constants.ScreenWidth, Constants.ScreenHeight, 0, 0, 1, 1, _whiteMaterial);
			}

			// TODO: post draw
			{
				/*SCOPED_PROFILE_EVENT( "Post-Draw" );

				// draw the wipe material on top of this if it hasn't completed yet
				DrawWipeModel();

				Dialog().Render( loadGUI != NULL );*/

				// draw the half console / notify console on top of everything
				console.Draw(false);
			}
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			//base.Draw(gameTime);		
		}
	}

	public enum CurrentGame
	{
		DoomClassic,
		Doom2Classic,
		Doom3BFG
	}

	public enum ErrorType
	{
		None = 0,

		/// <summary>Exit the entire game with a popup window.</summary>
		Fatal,

		/// <summary>Print to console and disconnect from the game.</summary>
		Drop,

		/// <summary>Don't kill the server.</summary>
		Disconnect
	}
}