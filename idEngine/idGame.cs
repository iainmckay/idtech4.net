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
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using idTech4.Services;

namespace idTech4
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class idGame : Microsoft.Xna.Framework.Game
	{
		#region Members
		private GraphicsDeviceManager _graphics;
		private string[] _rawCommandLineArguments;
		private CommandArguments[] _commandLineArguments = new CommandArguments[] { };
		#endregion

		#region Constructor
		public idGame(string[] args)
		{
			_graphics = new GraphicsDeviceManager(this);
			_rawCommandLineArguments = args;

			Content.RootDirectory = idLicensee.BaseGameDirectory;
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
			idLog.WriteLine("TODO: Sys_CreateConsole();");

			optimalPCTBuffer( 0.5f );
			currentGame( DOOM3_BFG );
			idealCurrentGame( DOOM3_BFG );

			snapCurrent.localTime = -1;
			snapPrevious.localTime = -1;
			snapCurrent.serverTime = -1;
			snapPrevious.serverTime = -1;

			com_errorEntered = ERP_NONE;
			timeDemo = TD_NO;

			ClearWipe();

			try 
			{
				// clear warning buffer
				idLog.ClearWarnings(idLicensee.GameName + " initialization");				
				idLog.WriteLine("Command line: {0}", String.Join(" ", _rawCommandLineArguments));

				// parse command line options
				ParseCommandLine(_rawCommandLineArguments);

				// init console command system
				ICommandSystemService cmdSystem = new idCommandSystem();
							
				// init CVar system
				ICVarSystemService cvarSystem = new idCVarSystem();

				this.Services.AddService(typeof(ICommandSystemService), cmdSystem);
				this.Services.AddService(typeof(ICVarSystemService), cvarSystem);

				// register all static CVars
				CVars.Register(this);
								
				idLog.WriteLine("QA Timing INIT");

				Stopwatch stopWatch = new Stopwatch();
				stopWatch.Start();

				// print engine version
				Printf( "%s\n", version.string );

				// initialize key input/binding, done early so bind command exists
				idKeyInput::Init();

				// init the console so we can take prints
				console->Init();

				// get architecture info
				Sys_Init();

				// initialize networking
				Sys_InitNetworking();

		// override cvars from command line
		StartupVariable( NULL );

		consoleUsed = com_allowConsole.GetBool();

		if ( Sys_AlreadyRunning() ) {
			Sys_Quit();
		}

		// initialize processor specific SIMD implementation
		InitSIMD();

		// initialize the file system
		fileSystem->Init();

		const char * defaultLang = Sys_DefaultLanguage();
		com_isJapaneseSKU = ( idStr::Icmp( defaultLang, ID_LANG_JAPANESE ) == 0 );

		// Allow the system to set a default lanugage
		Sys_SetLanguageFromSystem();

		// Pre-allocate our 20 MB save buffer here on time, instead of on-demand for each save....

		saveFile.SetNameAndType( SAVEGAME_CHECKPOINT_FILENAME, SAVEGAMEFILE_BINARY );
		saveFile.PreAllocate( MIN_SAVEGAME_SIZE_BYTES );

		stringsFile.SetNameAndType( SAVEGAME_STRINGS_FILENAME, SAVEGAMEFILE_BINARY );
		stringsFile.PreAllocate( MAX_SAVEGAME_STRING_TABLE_SIZE );

		fileSystem->BeginLevelLoad( "_startup", saveFile.GetDataPtr(), saveFile.GetAllocated() );

		// initialize the declaration manager
		declManager->Init();

		// init journalling, etc
		eventLoop->Init();

		// init the parallel job manager
		parallelJobManager->Init();

		// exec the startup scripts
		cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "exec default.cfg\n" );

#ifdef CONFIG_FILE
		// skip the config file if "safe" is on the command line
		if ( !SafeMode() && !g_demoMode.GetBool() ) {
			cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "exec " CONFIG_FILE "\n" );
		}
#endif

		cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "exec autoexec.cfg\n" );

		// run cfg execution
		cmdSystem->ExecuteCommandBuffer();

		// re-override anything from the config files with command line args
		StartupVariable( NULL );

		// if any archived cvars are modified after this, we will trigger a writing of the config file
		cvarSystem->ClearModifiedFlags( CVAR_ARCHIVE );
		
		// init OpenGL, which will open a window and connect sound and input hardware
		renderSystem->InitOpenGL();

		// Support up to 2 digits after the decimal point
		com_engineHz_denominator = 100LL * com_engineHz.GetFloat();
		com_engineHz_latched = com_engineHz.GetFloat();

		// start the sound system, but don't do any hardware operations yet
		soundSystem->Init();

		// initialize the renderSystem data structures
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
				idLog.WriteLine("QA Timing IIS: {0:000000}ms", stopWatch.ElapsedMilliseconds);

				stopWatch.Stop();
	} catch( idException & ) {
		Sys_Error( "Error during initialization" );
	}

		
			if(win32.win_notaskkeys.GetInteger())
			{
				DisableTaskKeys(TRUE, FALSE, /*( win32.win_notaskkeys.GetInteger() == 2 )*/ FALSE);
			}

			// hide or show the early console as necessary
			/*if(win32.win_viewlog.GetInteger())
			{
				Sys_ShowConsole(1, true);
			}
			else
			{
				Sys_ShowConsole(0, false);
			}*/
			
			base.Initialize();
		}

		private void ParseCommandLine(string[] args)
		{
			List<CommandArguments> argList = new List<CommandArguments>();
			idCmdArgs current = null;

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
						current = new idCmdArgs();
						argList.Add(current);
					}

					current.AppendArg(arg);
				}
			}

			_commandLineArguments = argList.ToArray();
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
}