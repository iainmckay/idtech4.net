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

		private List<string> _errorList = new List<string>();
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
		public void Init(LaunchParameters args)
		{
			// TODO
			try
			{
				// initialize idLib
				// idLib::Init();
				
				// clear warning buffer
				idConsole.ClearWarnings(string.Format("{0} initialization", idE.GameName));

				idCvar test = new idCvar("test", "abc", "blah", CvarFlags.System);
				idCvar test2 = new idCvar("test", "abc2", "blah", CvarFlags.System);

				idE.CmdSystem.Init();
				idE.CvarSystem.Init();

				idConsole.WriteLine("test: {0}", test.ToString());
				idConsole.WriteLine("test2: {0}", test2.ToString());
				/*
		// start file logging right away, before early console or whatever
		StartupVariable( "win_outputDebugString", false );

		// register all static CVars
		idCVar::RegisterStaticVars();

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
		StartupVariable( NULL, false );

		if ( !idAsyncNetwork::serverDedicated.GetInteger() && Sys_AlreadyRunning() ) {
			Sys_Quit();
		}

		// initialize processor specific SIMD implementation
		InitSIMD();

		// init commands
		InitCommands();

#ifdef ID_WRITE_VERSION
		config_compressor = idCompressor::AllocArithmetic();
#endif

		// game specific initialization
		InitGame();

		// don't add startup commands if no CD key is present
#if ID_ENFORCE_KEY
		if ( !session->CDKeysAreValid( false ) || !AddStartupCommands() ) {
#else
		if ( !AddStartupCommands() ) {
#endif
			// if the user didn't give any commands, run default action
			session->StartMenu( true );
		}

		Printf( "--- Common Initialization Complete ---\n" );

		// print all warnings queued during initialization
		PrintWarnings();

#ifdef	ID_DEDICATED
		Printf( "\nType 'help' for dedicated server info.\n\n" );
#endif

		// remove any prints from the notify lines
		console->ClearNotifyLines();
		
		ClearCommandLine();

		com_fullyInitialized = true;
	}
*/
			}
			catch(Exception)
			{
				Error("Error during initialization");
			}
		}

		public void Quit()
		{
			// TODO
			/*#ifdef ID_ALLOW_TOOLS
			if ( com_editors & EDITOR_RADIANT ) {
				RadiantInit();
				return;
			}
			#endif*/

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
		#endregion

		#region Private
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
		#endregion
		#endregion
	}
}
