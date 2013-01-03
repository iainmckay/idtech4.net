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
using System.Reflection;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using idTech4.Input;
using idTech4.IO;
using idTech4.Services;

namespace idTech4
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
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
		private GraphicsDeviceManager _graphics;
		private string[] _rawCommandLineArguments;
		private CommandArguments[] _commandLineArguments = new CommandArguments[] { };

		// this is set if the player enables the console, which disables achievements
		private bool _consoleUsed;

		// for tracking errors
		private ErrorType _errorEntered;
		private long _lastErrorTime;
		private int _errorCount;
		private List<string> _errorList = new List<string>();

		private Stopwatch _gameTimer;
		private bool _shuttingDown;

		// engine timing
		private float _engineHzLatched = 60.0f; // latched version of cvar, updated between map loads
		private long _engineHzNumerator = 100 * 1000;
		private long _engineHzDenominator = 100 * 60;
		#endregion

		#region Constructor
		private idEngine()
		{
			_gameTimer = Stopwatch.StartNew();
			_graphics = new GraphicsDeviceManager(this);

			Content.RootDirectory = idLicensee.BaseGameDirectory;
		}
		#endregion
		
		#region Error Handling
		public void Error(string format, params object[] args)
		{
			ErrorType code = ErrorType.Drop;

			// always turn this off after an error
			idLog.RefreshOnPrint = false;

			// retrieve the services we need for this
			ICVarSystem cvarSystem = this.GetService<ICVarSystem>();
			ICommandSystem cmdSystem = this.GetService<ICommandSystem>();

			if(cvarSystem.GetInt("com_productionMode") == 3)
			{
				Sys_Quit();
			}
						
			// if we don't have the renderer running, make it a fatal error
			// TODO: important! if(idE.RenderSystem.IsRunning == false)
			/*if(idE.RenderSystem.IsRunning == false)
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
			ICVarSystem cvarSystem = this.Services.GetService<ICVarSystem>();
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
			/*idLog.WriteLine("TODO: Sys_CreateConsole();");

			idLog.WriteLine("TODO: optimalPCTBuffer( 0.5f );");
			idLog.WriteLine("TODO: currentGame( DOOM3_BFG );");
			idLog.WriteLine("TODO: idealCurrentGame( DOOM3_BFG );");

			idLog.WriteLine("TODO: snapCurrent.localTime = -1;");
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
				ICVarSystem cvarSystem = new idCVarSystem();
				ICommandSystem cmdSystem = new idCommandSystem();				
				IPlatformService platform = FindPlatform();
								
				this.Services.AddService(typeof(ICommandSystem), cmdSystem);
				this.Services.AddService(typeof(ICVarSystem), cvarSystem);
				this.Services.AddService(typeof(IPlatformService), platform);

				// register all static CVars
				CVars.Register();

				idLog.WriteLine("Command line: {0}", String.Join(" ", _rawCommandLineArguments));
												
				idLog.WriteLine("QA Timing INIT");
				idLog.WriteLine(idVersion.ToString(platform));

				// initialize key input/binding, done early so bind command exists
				this.Services.AddService(typeof(IInputSystem), new idInputSystem());

				// init the console so we can take prints
				this.Services.AddService(typeof(IConsole), new idConsole());

				// get architecture info
				idLog.WriteLine("TODO: Sys_Init();");

				// initialize networking
				idLog.WriteLine("TODO: Sys_InitNetworking();");

				// override cvars from command line
				StartupVariable(null);

				_consoleUsed = cvarSystem.GetBool("com_allowConsole");

				idLog.WriteLine("TODO: Sys_AlreadyRunning");
				/*if ( Sys_AlreadyRunning() ) {
					Sys_Quit();
				}*/

				// initialize processor specific SIMD implementation
				idLog.WriteLine("TODO: InitSIMD();");

				// initialize the file system
				this.Services.AddService(typeof(IFileSystem), new idFileSystem());

				/*const char * defaultLang = Sys_DefaultLanguage();
				com_isJapaneseSKU = ( idStr::Icmp( defaultLang, ID_LANG_JAPANESE ) == 0 );

				// Allow the system to set a default lanugage
				Sys_SetLanguageFromSystem();*/

				// pre-allocate our 20 MB save buffer here on time, instead of on-demand for each save....
				idLog.WriteLine("TOOD: savefile pre-allocation");
				/*saveFile.SetNameAndType( SAVEGAME_CHECKPOINT_FILENAME, SAVEGAMEFILE_BINARY );
				saveFile.PreAllocate( MIN_SAVEGAME_SIZE_BYTES );

				stringsFile.SetNameAndType( SAVEGAME_STRINGS_FILENAME, SAVEGAMEFILE_BINARY );
				stringsFile.PreAllocate( MAX_SAVEGAME_STRING_TABLE_SIZE );*/

				/*fileSystem->BeginLevelLoad( "_startup", saveFile.GetDataPtr(), saveFile.GetAllocated() );

				// initialize the declaration manager
				declManager->Init();

				// init journalling, etc
				eventLoop->Init();

				// init the parallel job manager
				parallelJobManager->Init();*/

				// exec the startup scripts
				cmdSystem.BufferCommandText("exec default.cfg");

				// skip the config file if "safe" is on the command line
				idLog.WriteLine("TODO: SafeMode()");

				if(/*!SafeMode() &&*/ (cvarSystem.GetBool("g_demoMode") == false))
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
		
				// init OpenGL, which will open a window and connect sound and input hardware
				idLog.WriteLine("TODO: renderSystem->InitOpenGL();");

				// support up to 2 digits after the decimal point
				_engineHzDenominator = 100 * cvarSystem.GetInt64("com_engineHz");
				_engineHzLatched = cvarSystem.GetFloat("com_engineHz");

				// start the sound system, but don't do any hardware operations yet
				idLog.WriteLine("TODO: soundSystem->Init();");

				idLog.WriteLine("TODO: REST OF INIT");

				/*// initialize the renderSystem data structures
				renderSystem->Init();

				whiteMaterial = declManager->FindMaterial( "_white" );

				if ( idStr::Icmp( sys_lang.GetString(), ID_LANG_FRENCH ) == 0 ) {
					// If the user specified french, we show french no matter what SKU
					splashScreen = declManager->FindMaterial( "guis/assets/splash/legal_french" );
				} else if ( idStr::Icmp( defaultLang, ID_LANG_FRENCH ) == 0 ) {
					// If the lead sku is french (ie: europe), display figs
					splashScreen = declManager->FindMaterial( "guis/assets/splash/legal_figs" );
				} else {
					// Otherwise show it in english
					splashScreen = declManager->FindMaterial( "guis/assets/splash/legal_english" );
				}

				const int legalMinTime = 4000;
				const bool showVideo = ( !com_skipIntroVideos.GetBool () && fileSystem->UsingResourceFiles() );
				if ( showVideo ) {
					RenderBink( "video\\loadvideo.bik" );
					RenderSplash();
					RenderSplash();
				} else {
					idLib::Printf( "Skipping Intro Videos!\n" );
					// display the legal splash screen
					// No clue why we have to render this twice to show up...
					RenderSplash();
					RenderSplash();
				}


				int legalStartTime = Sys_Milliseconds();
				declManager->Init2();

				// initialize string database so we can use it for loading messages
				InitLanguageDict();

				// spawn the game thread, even if we are going to run without SMP
				// one meg stack, because it can parse decls from gui surfaces (unfortunately)
				// use a lower priority so job threads can run on the same core
				gameThread.StartWorkerThread( "Game/Draw", CORE_1B, THREAD_BELOW_NORMAL, 0x100000 );
				// boost this thread's priority, so it will prevent job threads from running while
				// the render back end still has work to do

				// init the user command input code
				usercmdGen->Init();

				Sys_SetRumble( 0, 0, 0 );

				// initialize the user interfaces
				uiManager->Init();

				// startup the script debugger
				// DebuggerServerInit();

				// load the game dll
				LoadGameDLL();

				// On the PC touch them all so they get included in the resource build
				if ( !fileSystem->UsingResourceFiles() ) {
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
				menuSoundWorld->PlaceListener( vec3_origin, mat3_identity, 0 );

				// init the session
				session->Initialize();
				session->InitializeSoundRelatedSystems();

				InitializeMPMapsModes();

				// leaderboards need to be initialized after InitializeMPMapsModes, which populates the MP Map list.
				if( game != NULL ) {
					game->Leaderboards_Init();
				}

				CreateMainMenu();

				commonDialog.Init();

				// load the console history file
				consoleHistory.LoadHistoryFile();

				AddStartupCommands();

				StartMenu( true );

				while ( Sys_Milliseconds() - legalStartTime < legalMinTime ) {
					RenderSplash();
					Sys_GenerateEvents();
					Sys_Sleep( 10 );
				};

				// print all warnings queued during initialization
				PrintWarnings();

				// remove any prints from the notify lines
				console->ClearNotifyLines();

				CheckStartupStorageRequirements();


				if ( preload_CommonAssets.GetBool() && fileSystem->UsingResourceFiles() ) {
					idPreloadManifest manifest;
					manifest.LoadManifest( "_common.preload" );
					globalImages->Preload( manifest, false );
					soundSystem->Preload( manifest );
				}

				fileSystem->EndLevelLoad();

				// Initialize support for Doom classic.
				doomClassicMaterial = declManager->FindMaterial( "_doomClassic" );
				idImage *image = globalImages->GetImage( "_doomClassic" );
				if ( image != NULL ) {
					idImageOpts opts;
					opts.format = FMT_RGBA8;
					opts.colorFormat = CFM_DEFAULT;
					opts.width = DOOMCLASSIC_RENDERWIDTH;
					opts.height = DOOMCLASSIC_RENDERHEIGHT;
					opts.numLevels = 1;
					image->AllocImage( opts, TF_LINEAR, TR_REPEAT );
				}

				com_fullyInitialized = true;


				// No longer need the splash screen
				if ( splashScreen != NULL ) {
					for ( int i = 0; i < splashScreen->GetNumStages(); i++ ) {
						idImage * image = splashScreen->GetStage( i )->texture.image;
						if ( image != NULL ) {
							image->PurgeImage();
						}
					}
				}

				idLog.WriteLine("--- Common Initialization Complete ---");
				idLog.WriteLine("QA Timing IIS: {0:000000}ms", _gameTimer.ElapsedMilliseconds);
						
				if(win32.win_notaskkeys.GetInteger())
				{
					DisableTaskKeys(TRUE, FALSE, /*( win32.win_notaskkeys.GetInteger() == 2 )*/ /*FALSE);
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
			} 
			catch(Exception ex) 
			{
				throw new Exception("Uh oh!", ex);
				Sys_Error("Error during initialization");
			}
			
			base.Initialize();
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			base.OnExiting(sender, args);

			Shutdown();
		}

		private IPlatformService FindPlatform()
		{
			// TODO: clean this up
#if WINDOWS
			string assemblyName = "idTech4.Platform.Win32.dll";
			string typeName = "idTech4.Platform.Win32.Win32Platform";
#elif XBOX
			string assemblyName = "idTech4.Platform.Xbox360.dll";
			string typeName = "idTech4.Platform.Xbox360.Xbox360Platform";
#else
			return null;
#endif

			assemblyName = Path.Combine(Environment.CurrentDirectory, assemblyName);

			return Assembly.LoadFile(assemblyName).CreateInstance(typeName) as IPlatformService;
		}

		private void ParseCommandLine(string[] args)
		{
			List<CommandArguments> argList = new List<CommandArguments>();
			CommandArguments current = null;

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
		/// Show the early console as an error dialog.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		private void Sys_Error(string format, params object[] args)
		{
			string errorMessage = string.Format(format, args);

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
			// Allows the game to exit
			if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add your drawing code here

			base.Draw(gameTime);
		}
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