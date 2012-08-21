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
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

using Microsoft.Xna.Framework;

using idTech4.Game;
using idTech4.IO;

namespace idTech4
{
	public sealed class idSystem : Microsoft.Xna.Framework.Game
	{
		#region Properties
		#region Public
		public int FrameTime
		{
			get
			{
				return _frameTime;
			}
		}

		public int Milliseconds
		{
			get
			{
				if(_gameTime != null)
				{
					return (int) _gameTime.TotalGameTime.TotalMilliseconds;
				}

				return 1;
			}
		}

		public int TicNumber
		{
			get
			{
				return _ticNumber;
			}
		}
		#endregion

		#region Internal
		internal bool RefreshOnPrint
		{
			get
			{
				return _refreshOnPrint;
			}
			set
			{
				_refreshOnPrint = value;
			}
		}
		#endregion
		#endregion

		#region Members
		private bool _firstTick = true;
		private bool _fullyInitialized;
		private bool _shuttingDown;

		private bool _refreshOnPrint;

		private ErrorType _errorEntered;

		private int _lastErrorTime;
		private int _errorCount;

		private int _frameTime;				// time for the current frame in milliseconds
		private int _frameNumber;			// variable frame number
		private int _ticNumber;				// 60 hz tics

		private List<string> _errorList = new List<string>();

		private string[] _rawCommandLineArguments;
		private idCmdArgs[] _commandLineArguments = new idCmdArgs[] { };

		private GameTime _gameTime;
		private GraphicsDeviceManager _graphics;
		#endregion

		#region Constructor
		public idSystem(string[] args)
		{
			idE.System = this;

			InitCvars();
			
			_graphics = new GraphicsDeviceManager(this);
			_rawCommandLineArguments = args;

			this.TargetElapsedTime = TimeSpan.FromMilliseconds(idE.UserCommandMillseconds);
			this.Content.RootDirectory = "base";
		}
		#endregion

		#region Methods
		#region Public
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
					final.Add(args);
					continue;
				}

				string s = args.Get(1);

				if((match == null) || (StringComparer.InvariantCultureIgnoreCase.Compare(s, match) == 0))
				{
					idE.CvarSystem.SetString(s, args.Get(2));
				}

				if(once == false)
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
			if(idE.CvarSystem.GetInteger("fs_copyfiles") > 0)
			{
				code = ErrorType.Fatal;
			}

			// if we don't have GL running, make it a fatal error
			if(idE.RenderSystem.IsRunning == false)
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
					this.Exit();
				}

				code = ErrorType.Fatal;
			}

			// if we are getting a solid stream of ERP_DROP, do an ERP_FATAL
			int currentTime = this.Milliseconds;

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

			if(code == ErrorType.Disconnect)
			{
				_errorEntered = ErrorType.None;

				throw new Exception(errorMessage);
			}
			else if(code == ErrorType.Drop)
			{
				idConsole.WriteLine("********************");
				idConsole.WriteLine("ERROR: {0}", errorMessage);
				idConsole.WriteLine("********************");

				_errorEntered = ErrorType.None;

				// TODO throw new Exception(errorMessage);
			}
			else
			{
				idConsole.WriteLine("********************");
				idConsole.WriteLine("ERROR: {0}", errorMessage);
				idConsole.WriteLine("********************");
			}

			if(idE.CvarSystem.GetBool("r_fullscreen") == true)
			{
				idE.CmdSystem.BufferCommandText(Execute.Now, "vid_restart partial windowed\n");
			}
			
			Shutdown();
			Sys_Error(errorMessage);
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
				this.Exit();
			}

			_errorEntered = ErrorType.Fatal;

			if(idE.CvarSystem.GetBool("r_fullscreen") == true)
			{
				idE.CmdSystem.BufferCommandText(Execute.Now, "vid_restart partial windowed");
			}

			Shutdown();
			Sys_Error(format, args);
		}
		#endregion

		#region Private
		private void InitCvars()
		{
			// these win_* cvars may need ifdef'd out on other platforms.  only targetting windows+xbox right now so
			// shouldn't be an issue.
			new idCvar("sys_arch", "", "", CvarFlags.System | CvarFlags.Init);
			new idCvar("sys_cpustring", "detect", "", CvarFlags.System | CvarFlags.Init);
			new idCvar("sys_lang", "english", "", CvarFlags.System | CvarFlags.Archive/* TODO: , sysLanguageNames, idCmdSystem::ArgCompletion_String<sysLanguageNames> */);

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
			new idCvar("com_asyncSound", "1", 0, 1, "0: mix sound inline, 1: memory mapped async mix, 2: callback mixing, 3: write async mix", CvarFlags.Integer | CvarFlags.System);
			new idCvar("com_forceGenericSIMD", "0", "force generic platform independent SIMD", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("developer", "0", "developer mode", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_allowConsole", "0", "allow toggling console with the tilde key", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_speeds", "0", "show engine timings", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_showFPS", "0", "show frames rendered per second", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_showMemoryUsage", "0", "show total and per frame memory usage", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_showAsyncStats", "0", "show async network stats", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_showSoundDecoders", "0", "show sound decoders", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_timestampPrints", "0", 0, 2, "print time with each console print, 1 = msec, 2 = sec", new ArgCompletion_Integer(0, 2), CvarFlags.System);
			new idCvar("timescale", "1", 0.1f, 10.0f, "scales the time", CvarFlags.System | CvarFlags.Float);
			new idCvar("logFile", "0", 0, 2, "1 = buffer log, 2 = flush after each print", new ArgCompletion_Integer(0, 2), CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("logFileName", "qconsole.log", "name of log file, if empty, qconsole.log will be used", CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_makingBuild", "0", "1 when making a build", CvarFlags.Bool | CvarFlags.System);
			new idCvar("com_updateLoadSize", "0", "update the load size after loading a map", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
			new idCvar("com_videoRam", "64", "holds the last amount of detected video ram", CvarFlags.Integer | CvarFlags.System | CvarFlags.NoCheat | CvarFlags.Archive);

			new idCvar("com_product_lang_ext", "1", "Extension to use when creating language files.", CvarFlags.Integer | CvarFlags.System | CvarFlags.Archive);
		}

		private void InitCommands()
		{
			idE.CmdSystem.AddCommand("error", "causes an error", CommandFlags.System | CommandFlags.Cheat, new EventHandler<CommandEventArgs>(Cmd_Error));

			/*
			cmdSystem->AddCommand( "crash", Com_Crash_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "causes a crash" );
			cmdSystem->AddCommand( "freeze", Com_Freeze_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "freezes the game for a number of seconds" );*/

			idE.CmdSystem.AddCommand("quit", "quits the game", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Quit));
			idE.CmdSystem.AddCommand("exit", "exits the game", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Quit));

			// TODO: commands
			/*cmdSystem->AddCommand( "writeConfig", Com_WriteConfig_f, CMD_FL_SYSTEM, "writes a config file" );
			cmdSystem->AddCommand( "reloadEngine", Com_ReloadEngine_f, CMD_FL_SYSTEM, "reloads the engine down to including the file system" );
			cmdSystem->AddCommand( "setMachineSpec", Com_SetMachineSpec_f, CMD_FL_SYSTEM, "detects system capabilities and sets com_machineSpec to appropriate value" );*/

			idE.CmdSystem.AddCommand("execMachineSpec", "execs the appropriate config files and sets cvars based on com_machineSpec", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_ExecMachineSpec));

			/*
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
			cmdSystem->AddCommand( "localizeMaps", Com_LocalizeMaps_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "localize maps" );*/
			idE.CmdSystem.AddCommand("reloadLanguage", "reload language dictionary", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_ReloadLanguage));

			//D3XP Localization
			/*cmdSystem->AddCommand( "localizeGuiParmsTest", Com_LocalizeGuiParmsTest_f, CMD_FL_SYSTEM, "Create test files that show gui parms localized and ignored." );
			cmdSystem->AddCommand( "localizeMapsTest", Com_LocalizeMapsTest_f, CMD_FL_SYSTEM, "Create test files that shows which strings will be localized." );

			// build helpers
			cmdSystem->AddCommand( "startBuild", Com_StartBuild_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "prepares to make a build" );
			cmdSystem->AddCommand( "finishBuild", Com_FinishBuild_f, CMD_FL_SYSTEM|CMD_FL_CHEAT, "finishes the build process" );

		#ifdef ID_DEDICATED
			cmdSystem->AddCommand( "help", Com_Help_f, CMD_FL_SYSTEM, "shows help" );
		#endif*/
		}

		private void InitConsole()
		{
			// don't show it now that we have a splash screen up
			if(idE.CvarSystem.GetBool("win32_viewlog") == true)
			{
				idE.SystemConsole.Show();
				idE.SystemConsole.FocusInput();
			}

			idConsole.ClearInputHistory();

			// hide or show the early console as necessary
			if((idE.CvarSystem.GetInteger("win_viewlog") > 0) || (idE.CvarSystem.GetBool("com_skipRenderer") == true) /* TODO: || idAsyncNetwork::serverDedicated.GetInteger()*/)
			{
				idE.SystemConsole.Show(1, true);
			}
			else
			{
				idE.SystemConsole.Show(0, false);
			}
		}

		private void InitGame()
		{
			PrintLoadingMessage(idE.Language.Get("#str_04349"));
			
			// initialize the user interfaces
			idE.UIManager.Init();

			// startup the script debugger
			idConsole.Warning("TODO: DebuggerServerInit();");

			PrintLoadingMessage(idE.Language.Get("#str_04350"));

			// load the game dll
			LoadGameDLL();

			PrintLoadingMessage(idE.Language.Get("#str_04351"));

			// init the session
			idE.Session.Init();

			// have to do this twice.. first one sets the correct r_mode for the renderer init
			// this time around the backend is all setup correct.. a bit fugly but do not want
			// to mess with all the gl init at this point.. an old vid card will never qualify for 

			/*if(sysDetect == true)
			{
				SetMachineSpec();
				Cmd_ExecMachineSpec(this, new CommandEventArgs(new idCmdArgs()));

				idE.CvarSystem.SetInteger("s_numberOfSpeakers", 6);

				idE.CmdSystem.BufferCommandText(Execute.Now, "s_restart");
				idE.CmdSystem.ExecuteCommandBuffer();
			}*/

			// don't add startup commands if no CD key is present
			if(AddStartupCommands() == false)
			{
				// if the user didn't give any commands, run default action
				idE.Session.StartMenu(true);
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
			// TODO: console->ClearNotifyLines();
			*/
			
			_rawCommandLineArguments = null;
			_fullyInitialized = true;
		}

		private void InitLanguageDict()
		{
			idE.Language.Clear();

			// D3XP: Instead of just loading a single lang file for each language
			// we are going to load all files that begin with the language name
			// similar to the way pak files work. So you can place english001.lang
			// to add new strings to the english language dictionary
			idFileList files = idE.FileSystem.GetFiles("strings", ".lang", true);
			string[] langFiles = files.Files;

			// let it be set on the command line - this is needed because this init happens very early
			StartupVariable("sys_lang", false);

			string langName = idE.CvarSystem.GetString("sys_lang");

			// loop through the list and filter
			string[] currentLanguageList = langFiles.Where(c => c.StartsWith(langName)).ToArray();

			if(currentLanguageList.Length == 0)
			{
				// reset cvar to default and try to load again
				idE.CmdSystem.BufferCommandText(Execute.Now, "reset sys_lang");

				langName = idE.CvarSystem.GetString("sys_lang");
				currentLanguageList = langFiles.Where(c => c.StartsWith(langName)).ToArray();
			}

			foreach(string lang in currentLanguageList)
			{
				idE.Language.Load(Path.Combine(files.BaseDirectory, lang), false);
			}

			idConsole.Warning("TODO: Sys_InitScanTable");
		}

		private void InitRenderSystem()
		{
			if(idE.CvarSystem.GetBool("com_skipRenderer") == true)
			{
				return;
			}

			idE.RenderSystem.InitGraphics(this.GraphicsDevice);
			PrintLoadingMessage(idE.Language.Get("#str_04343"));
		}

		private void LoadGameDLL()
		{
			// from executable directory first - this is handy for developement
			string dllName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			dllName = Path.Combine(dllName, "game.dll");

			if(File.Exists(dllName) == false)
			{
				dllName = null;
			}

			if(dllName == null)
			{
				dllName = idE.FileSystem.RelativePathToOSPath("game.dll", "fs_savedir");
			}

			idConsole.DeveloperWriteLine("Loading game DLL: '{0}'", dllName);

			Assembly asm = Assembly.LoadFile(Path.GetFullPath(dllName));
			
			idE.Game = (idBaseGame) asm.CreateInstance("idTech4.Game.idGame");
			idE.GameEdit = (idBaseGameEdit) asm.CreateInstance("idTech4.Game.idGameEdit");

			idE.Game.Init();
		}

		private void ParseCommandLine(string[] args)
		{
			List<idCmdArgs> argList = new List<idCmdArgs>();
			idCmdArgs current = null;

			foreach(string arg in args)
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

		private void PrintLoadingMessage(string msg)
		{
			if(idE.RenderSystem.IsRunning == false)
			{
				return;
			}

			idE.RenderSystem.BeginFrame(idE.RenderSystem.ScreenWidth, idE.RenderSystem.ScreenHeight);
			idE.RenderSystem.DrawStretchPicture(0, 0, idE.VirtualScreenWidth, idE.VirtualScreenHeight, 0, 0, 1, 1, idE.DeclManager.FindMaterial("splashScreen"));
			idE.RenderSystem.DrawSmallString((640 - msg.Length * idE.SmallCharacterWidth) / 2, 410, msg, new Vector4(0.0f, 0.81f, 0.94f, 1.0f), true, idE.DeclManager.FindMaterial("textures/bigchars"));
			idE.RenderSystem.EndFrame();

			// we have to manually present otherwise nothing gets shown in xna
			// this is usually done by Draw() but we're not at that stage yet
			if(_fullyInitialized == false)
			{
				idE.RenderSystem.Present();
			}
		}

		private void Shutdown()
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
				if(args.Get(0).ToLower().StartsWith("set") == false)
				{
					added = true;
				}

				// directly as tokenized so nothing gets screwed
				idE.CmdSystem.BufferCommandArgs(Execute.Append, args);
			}

			return added;
		}

		private void CreateConsole()
		{
			// don't show it now that we have a splash screen up
			if(idE.CvarSystem.GetBool("win32_viewlog") == true)
			{
				idE.SystemConsole.Show(1, true);
			}

			idConsole.ClearInputHistory();
		}

		private void SetMachineSpec()
		{
			bool oldCard = false;
			bool nv10or20 = false;

			uint physicalMemory = idE.Platform.TotalPhysicalMemory;
			uint videoMemory = idE.Platform.TotalVideoMemory;
			float clockSpeed = idE.Platform.ClockSpeed / 1000.0f;

			// TODO: renderSystem->GetCardCaps( oldCard, nv10or20 );

			idConsole.WriteLine("Detected:");
			idConsole.WriteLine("\t{0:2} GHz CPU", clockSpeed);
			idConsole.WriteLine("\t{0}MB of system memory", physicalMemory);
			idConsole.WriteLine("\t{0}MB of video memory on {1}", videoMemory, (oldCard == true) ? "a less than optimal video architecture" : "an optimal video architecture");

			if((clockSpeed >= 1.9f) && (videoMemory >= 512) && (physicalMemory >= 1024) && (oldCard == false))
			{
				idConsole.WriteLine("This system qualifies for Ultra quality!");
				idE.CvarSystem.SetInteger("com_machineSpec", 3);
			}
			else if((clockSpeed >= 1.6f) && (videoMemory >= 256) && (physicalMemory >= 512) && (oldCard == false))
			{
				idConsole.WriteLine("This system qualifies for High quality!");
				idE.CvarSystem.SetInteger("com_machineSpec", 2);
			}
			else if((clockSpeed >= 1.1f) && (videoMemory >= 128) && (physicalMemory >= 384))
			{
				idConsole.WriteLine("This system qualifies for Medium quality.");
				idE.CvarSystem.SetInteger("com_machineSpec", 1);
			}
			else
			{
				idConsole.WriteLine("This system qualifies for Low quality.");
				idE.CvarSystem.SetInteger("com_machineSpec", 0);
			}

			idE.CvarSystem.SetInteger("com_videoRam", (int) videoMemory);
		}

		private void SetUltraHighQuality()
		{
			idE.CvarSystem.SetString("image_filter", "GL_LINEAR_MIPMAP_LINEAR", CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_anisotropy", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_lodbias", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_forceDownSize", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_roundDown", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_preload", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useAllFormats", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeSpecular", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeBump", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeSpecularLimit", 64, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeBumpLimit", 256, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_usePrecompressedTextures", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downsize", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_anisotropy", 8, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useCompression", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_ignoreHighQuality", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("s_maxSoundsPerShader", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("r_mode", 5, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useNormalCompression", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("r_multiSamples", 0, CvarFlags.Archive);
		}

		private void SetHighQuality()
		{
			idE.CvarSystem.SetString("image_filter", "GL_LINEAR_MIPMAP_LINEAR", CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_anisotropy", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_lodbias", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_forceDownSize", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_roundDown", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_preload", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useAllFormats", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeSpecular", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeBump", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeSpecularLimit", 64, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeBumpLimit", 256, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_usePrecompressedTextures", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downsize", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_anisotropy", 8, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useCompression", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_ignoreHighQuality", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("s_maxSoundsPerShader", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useNormalCompression", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("r_mode", 4, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("r_multiSamples", 0, CvarFlags.Archive);
		}

		private void SetMediumQuality()
		{
			idE.CvarSystem.SetString("image_filter", "GL_LINEAR_MIPMAP_LINEAR", CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_anisotropy", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_lodbias", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSize", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_forceDownSize", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_roundDown", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_preload", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useCompression", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useAllFormats", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_usePrecompressedTextures", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeSpecular", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeBump", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeSpecularLimit", 64, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeBumpLimit", 256, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useNormalCompression", 2, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("r_mode", 3, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("r_multiSamples", 0, CvarFlags.Archive);
		}

		private void SetLowQuality()
		{
			idE.CvarSystem.SetString("image_filter", "GL_LINEAR_MIPMAP_LINEAR", CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_anisotropy", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_lodbias", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_roundDown", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_preload", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useAllFormats", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_usePrecompressedTextures", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSize", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_anisotropy", 0, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useCompression", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_ignoreHighQuality", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("s_maxSoundsPerShader", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeSpecular", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeBump", 1, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeSpecularLimit", 64, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_downSizeBumpLimit", 256, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("r_mode", 3, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("image_useNormalCompression", 2, CvarFlags.Archive);
			idE.CvarSystem.SetInteger("r_multiSamples", 0, CvarFlags.Archive);
		}

		/// <summary>
		/// Show the early console as an error dialog.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		private void Sys_Error(string format, params object[] args)
		{
			string errorMessage = string.Format(format, args);

			idE.SystemConsole.Append(errorMessage + "\n");
			idE.SystemConsole.Show(1, true);

			// TODO: Sys_ShutdownInput();

			// wait for the user to quit
			while(true)
			{
				if(idE.SystemConsole.IsDisposed == true)
				{
					this.Exit();
					break;
				}

				Application.DoEvents();
				Thread.Sleep(0);
			}

			this.Exit();
		}

		private void Sys_Init()
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

			idE.CvarSystem.SetString("sys_cpustring", capabilities);

			idConsole.WriteLine(capabilities);
			idConsole.WriteLine("{0} MB system memory", idE.Platform.TotalPhysicalMemory);
			idConsole.WriteLine("{0} MB video memory", idE.Platform.TotalVideoMemory);
		}

		private void Sys_InitNetworking()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Command handlers
		/// <summary>
		/// Just throw a fatal error to test error shutdown procedures.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cmd_Error(object sender, CommandEventArgs e)
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

		private void Cmd_Quit(object sender, CommandEventArgs e)
		{
			this.Exit();
		}

		private void Cmd_ReloadLanguage(object sender, CommandEventArgs e)
		{
			InitLanguageDict();
		}

		private void Cmd_ExecMachineSpec(object sender, CommandEventArgs e)
		{
			switch(idE.CvarSystem.GetInteger("com_machineSpec"))
			{
				case 3:
					SetUltraHighQuality();
					break;

				case 2:
					SetHighQuality();
					break;

				case 1:
					SetMediumQuality();
					break;

				default:
					SetLowQuality();
					break;
			}

			if(idE.Platform.TotalVideoMemory < 128)
			{
				idE.CvarSystem.SetBool("image_ignoreHighQuality", true, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSize", 1, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSizeLimit", 256, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSizeSpecular", 1, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSizeSpecularLimit", 64, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSizeBump", 1, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSizeBumpLimit", 256, CvarFlags.Archive);
			}

			if(idE.Platform.TotalVideoMemory < 512)
			{
				idE.CvarSystem.SetBool("image_ignoreHighQuality", true, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("s_maxSoundsPerShader", 1, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSize", 1, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSizeLimit", 256, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSizeSpecular", 1, CvarFlags.Archive);
				idE.CvarSystem.SetInteger("image_downSizeSpecularLimit", 64, CvarFlags.Archive);
				idE.CvarSystem.SetBool("com_purgeAll", true, CvarFlags.Archive);
				idE.CvarSystem.SetBool("r_forceLoadImages", true, CvarFlags.Archive);
			}
			else
			{
				idE.CvarSystem.SetBool("com_purgeAll", false, CvarFlags.Archive);
				idE.CvarSystem.SetBool("r_forceLoadImages", false, CvarFlags.Archive);
			}

			// TODO
			/*bool oldCard = false;
			bool nv10or20 = false;
			renderSystem->GetCardCaps( oldCard, nv10or20 );
			if ( oldCard ) {
				cvarSystem->SetCVarBool( "g_decals", false, CVAR_ARCHIVE );
				cvarSystem->SetCVarBool( "g_projectileLights", false, CVAR_ARCHIVE );
				cvarSystem->SetCVarBool( "g_doubleVision", false, CVAR_ARCHIVE );
				cvarSystem->SetCVarBool( "g_muzzleFlash", false, CVAR_ARCHIVE );
			} else {*/
			idE.CvarSystem.SetBool("g_decals", true, CvarFlags.Archive);
			idE.CvarSystem.SetBool("g_projectileLights", true, CvarFlags.Archive);
			idE.CvarSystem.SetBool("g_doubleVision", true, CvarFlags.Archive);
			idE.CvarSystem.SetBool("g_muzzleFlash", true, CvarFlags.Archive);
			/*}
			if ( nv10or20 ) {*/
			idE.CvarSystem.SetInteger("image_useNormalCompression", 1, CvarFlags.Archive);
			/*}*/

#if MACOS_X
			// TODO MACOS
			// On low settings, G4 systems & 64MB FX5200/NV34 Systems should default shadows off
			bool oldArch;
			int vendorId, deviceId, cpuId;
			OSX_GetVideoCard( vendorId, deviceId );
			OSX_GetCPUIdentification( cpuId, oldArch );
			bool isFX5200 = vendorId == 0x10DE && ( deviceId & 0x0FF0 ) == 0x0320;
			if ( ( oldArch || ( isFX5200 && Sys_GetVideoRam() < 128 ) ) && com_machineSpec.GetInteger() == 0 ) {
				cvarSystem->SetCVarBool( "r_shadows", false, CVAR_ARCHIVE );
			} else {
				cvarSystem->SetCVarBool( "r_shadows", true, CVAR_ARCHIVE );
			}
#endif
		}
		#endregion
		#endregion

		#region Game implementation
		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);
		}

		protected override void Initialize()
		{
			// TODO
			//try
			{
				InitConsole();

				// clear warning buffer
				idConsole.ClearWarnings(string.Format("{0} initialization", idE.GameName));

				ParseCommandLine(_rawCommandLineArguments);

				idE.CmdSystem.Init();
				idE.CvarSystem.Init();

				// start file logging right away, before early console or whatever
				StartupVariable("win_outputDebugString", false);

				// register all static CVars
				idE.CvarSystem.RegisterStatics();

				// print engine version
				idConsole.WriteLine(idE.Version);

				// initialize key input/binding, done early so bind command exists
				idE.Input.Init();

				// init the console so we can take prints
				idE.Console.Init();

				// get architecture info
				Sys_Init();

				// initialize networking
				// TODO: Sys_InitNetworking();

				// override cvars from command line
				StartupVariable(null, false);

				// initialize processor specific SIMD implementation
				// TODO: InitSIMD();

				// init commands
				InitCommands();

				idE.FileSystem.Init();
				idE.DeclManager.Init();

				bool sysDetect = idE.FileSystem.FileExists(idE.ConfigSpecification, "fs_savepath") == false;

				if(sysDetect == true)
				{
					Stream s = idE.FileSystem.OpenFileWrite(idE.ConfigSpecification);

					if(s != null)
					{
						s.Dispose();
						s = null;
					}
				}

				if(sysDetect == true)
				{
					SetMachineSpec();
					Cmd_ExecMachineSpec(this, new CommandEventArgs(new idCmdArgs()));
				}

				// initialize the renderSystem data structures, but don't start OpenGL yet
				idE.RenderSystem.Init(_graphics);

				// initialize string database right off so we can use it for loading messages
				InitLanguageDict();

				PrintLoadingMessage(idE.Language.Get("#str_04344"));

				// load the font, etc
				idE.Console.LoadGraphics();

				// init journalling, etc
				idE.EventLoop.Init();

				PrintLoadingMessage(idE.Language.Get("#str_04345"));

				// exec the startup scripts
				idE.CmdSystem.BufferCommandText(Execute.Append, "exec editor.cfg");
				idE.CmdSystem.BufferCommandText(Execute.Append, "exec default.cfg");

				// skip the config file if "safe" is on the command line
				/* TODO: if ( !SafeMode() ) {*/
				idE.CmdSystem.BufferCommandText(Execute.Append, string.Format("exec {0}", idE.ConfigFile));
				/*}*/

				idE.CmdSystem.BufferCommandText(Execute.Append, "exec autoexec.cfg");

				// reload the language dictionary now that we've loaded config files
				idE.CmdSystem.BufferCommandText(Execute.Append, "reloadLanguage");

				// run cfg execution
				idE.CmdSystem.ExecuteCommandBuffer();

				// re-override anything from the config files with command line args
				StartupVariable(null, false);

				// if any archived cvars are modified after this, we will trigger a writing of the config file
				idE.CvarSystem.ModifiedFlags |= CvarFlags.Archive;

				// init the user command input code
				idE.UserCommandGenerator.Init();

				PrintLoadingMessage(idE.Language.Get("#str_04346"));

				// start the sound system, but don't do any hardware operations yet
				idE.SoundSystem.Init();

				PrintLoadingMessage(idE.Language.Get("#str_04347"));

				// init async network
				idE.AsyncNetwork.Init();

#if ID_DEDICATED
			throw new NotImplementedException("don't do dedicated");
			/*idAsyncNetwork::server.InitPort();*/
			idE.CvarSystem.SetBool("s_noSound", true);
#else
				if(idE.CvarSystem.GetInteger("net_serverDedicated") == 1)
				{
					throw new NotImplementedException("don't do dedicated");

					/*idAsyncNetwork::server.InitPort();*/
					idE.CvarSystem.SetBool("s_noSound", true);
				}
				else
				{
					// init OpenGL, which will open a window and connect sound and input hardware
					PrintLoadingMessage(idE.Language.Get("#str_04348"));
					InitRenderSystem();
				}
#endif

				base.Initialize();
			}
			/*catch(Exception x)
			{
				Error("Error during initialization");

				idConsole.WriteLine(x.ToString());
			}*/
		}

		protected override void LoadContent()
		{
			base.LoadContent();
						
			idE.ImageManager.ReloadImages();
		}

		protected override void Update(GameTime gameTime)
		{
			// FIXME: this is a hack to get the render window up so we can show the loading messages.
			// it doesn't usually come up until all initialization has been completed and one tick has been run.
			// this causes none of the loading messages to appear and it looks like the program isn't loading!
			if(_firstTick == true)
			{
				_firstTick = false;
				return;
			}
			else if(_fullyInitialized == false)
			{
				// game specific initialization
				InitGame();

				_frameTime = 0;
				_ticNumber = 0;

				return;
			}
			
			_gameTime = gameTime;
		
			// if "viewlog" has been modified, show or hide the log console
			if(idE.CvarSystem.IsModified("win_viewlog") == true)
			{
				if((idE.CvarSystem.GetBool("com_skipRenderer") == false) && (idE.CvarSystem.GetInteger("net_serverDedicated") != 1))
				{
					idE.SystemConsole.Show(idE.CvarSystem.GetInteger("win_viewlog"), false);
				}

				idE.CvarSystem.ClearModified("win_viewlog");
			}

			//try
			{
				// pump all the events
				idE.Input.Update();

				// write config file if anything changed
				// TODO: WriteConfiguration(); 

				// change SIMD implementation if required
				// TODO
				/*if ( com_forceGenericSIMD.IsModified() ) {
					InitSIMD();
				}*/
				
				idE.EventLoop.RunEventLoop();

				// TODO: _ticNumber++ is temp, supposed to be in async thread
				_ticNumber++;

				_frameTime = _ticNumber * idE.UserCommandMillseconds;
				//_frameTime = this.Milliseconds;
				
				/*idAsyncNetwork::RunFrame();*/

				if(idE.AsyncNetwork.IsActive == true)
				{
					if(idE.CvarSystem.GetInteger("net_serverDedicated") != 1)
					{
						idE.Session.GuiFrameEvents();
						idE.Session.UpdateScreen(false);
					}
				}
				else
				{
					idE.Session.Frame();

					// normal, in-sequence screen update
					idE.Session.UpdateScreen(false);
				}

				// report timing information
				// TODO: com_speeds, remember drawing is in Draw now!!
				/*if ( com_speeds.GetBool() ) {
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
			/*catch(Exception)
			{
				// an ERP_DROP was thrown
			}*/

			base.Update(gameTime);
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			base.OnExiting(sender, args);

			// don't try to shutdown if we are in a recursive error			
			if(_errorEntered == ErrorType.None)
			{
				Shutdown();
			}
		}
		#endregion
	}
}