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

namespace idTech4
{
	public sealed class RedirectBufferEventArgs : EventArgs
	{
		#region Properties
		public string Text
		{
			get
			{
				return _text;
			}
		}
		#endregion

		#region Members
		private string _text;
		#endregion

		#region Constructor
		public RedirectBufferEventArgs(string text)
			: base()
		{
			_text = text;
		}
		#endregion
	}

	public sealed class idConsole
	{
		#region Members
		private static Queue<string> _inputHistory = new Queue<string>();

		private static string _warningCaption;
		private static List<string> _warningList = new List<string>();
		
		private static StringBuilder _redirectBuffer = null;
		private static EventHandler<RedirectBufferEventArgs> _redirectFlushHandler;

		private static StreamWriter _logFile;
		private static bool _logFileFailed;
		private static bool _recursingLogFileOpen;
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Throws an exception. Normal errors just abort to the game loop,
		/// which is appropriate for media or dynamic logic errors.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Error(string format, params object[] args)
		{
			idE.System.Error(format, args);
		}

		public static void Error(int localizationKey)
		{
			Error(idE.Language.Get(string.Format("#str_{0:00000}")));
		}

		public static void Error(int localizationKey, params object[] args)
		{
			Error(idE.Language.Get(string.Format("#str_{0:00000}")), args);
		}

		/// <summary>
		/// Dump out of the game to a system dialog.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void FatalError(string format, params object[] args)
		{
			idE.System.FatalError(format, args);			
		}

		/// <summary>
		/// Prints WARNING messages and adds the message to a queue to be printed later on.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Warning(string format, params object[] args)
		{
			string msg = string.Format(format, args);

			WriteLine("{0}WARNING: {1}{2}", idColorString.Yellow, idColorString.Red, msg);

			if(_warningList.Contains(msg) == false)
			{
				_warningList.Add(msg);
			}
		}

		/// <summary>
		/// Prints message that only shows up if the "developer" cvar is set.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void DeveloperWriteLine(string format, params object[] args)
		{
			DeveloperWrite(format + '\n', args);
		}

		/// <summary>
		/// Prints message that only shows up if the "developer" cvar is set.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void DeveloperWrite(string format, params object[] args)
		{
			if((idE.CvarSystem.IsInitialized == false) || (idE.CvarSystem.GetBool("developer") == false))
			{
				return; // don't confuse non-developers with techie stuff...
			}

			// never refresh the screen, which could cause reentrency problems
			bool temp = idE.System.RefreshOnPrint;

			Write(string.Format("{0}{1}", idColorString.Red, string.Format(format, args)));

			idE.System.RefreshOnPrint = temp;
		}

		/// <summary>
		/// Both client and server can use this, and it will output to the appropriate place.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void WriteLine(string format, params object[] args)
		{
			Write(format + Environment.NewLine, args);
		}

		/// <summary>
		/// Both client and server can use this, and it will output to the appropriate place.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Write(string format, params object[] args)
		{
			int timeLength = 0;

			// if the cvar system is not initialized
			if(idE.CvarSystem.IsInitialized == false)
			{
				return;
			}

			// optionally put a timestamp at the beginning of each print,
			// so we can see how long different init sections are taking

			// TODO	
			/*if ( com_timestampPrints.GetInteger() ) {
				int	t = Sys_Milliseconds();
				if ( com_timestampPrints.GetInteger() == 1 ) {
					t /= 1000;
				}
				sprintf( msg, "[%i]", t );
				timeLength = strlen( msg );
			} else {
				timeLength = 0;
			}*/

			// don't overflow
			string msg = string.Format(format, args);

			if(msg.Length >= (idE.MaxPrintMessageSize - timeLength))
			{
				msg = msg.Substring(0, idE.MaxPrintMessageSize - timeLength - 2);
				msg += Environment.NewLine;

				WriteLine("idConsole.Write: truncated to {0} characters", msg.Length);
			}

			if(_redirectBuffer != null)
			{
				if((_redirectBuffer.Length + msg.Length) >= _redirectBuffer.MaxCapacity)
				{
					_redirectFlushHandler(null, new RedirectBufferEventArgs(_redirectBuffer.ToString()));
					_redirectBuffer.Clear();
				}

				_redirectBuffer.Append(msg);

				return;
			}

			// echo to console buffer
			AddToScreenBuffer(msg);
			AddToDedicatedBuffer(idHelper.RemoveColors(msg));

			// print to script debugger server
			// DebuggerServerPrint( msg );

			// logFile
			if((idE.CvarSystem.GetInteger("logFile") != 0) && (_logFileFailed == false) && (idE.FileSystem.IsInitialized == true))
			{
				if((_logFile == null) && (_recursingLogFileOpen == false))
				{
					string fileName = "qconsole.log";

					if(idE.CvarSystem.GetString("logFileName") != string.Empty)
					{
						fileName = idE.CvarSystem.GetString("logFileName");
					}

					// fileSystem->OpenFileWrite can cause recursive prints into here
					_recursingLogFileOpen = true;

					Stream s = idE.FileSystem.OpenFileWrite(fileName);

					if(s == null)
					{
						_logFileFailed = true;
						FatalError("failed to open log file '{0}'", fileName);
					}

					_recursingLogFileOpen = false;
					_logFile = new StreamWriter(s);
					_logFile.AutoFlush = true;				

					WriteLine("log file '{0}' opened on {1}", fileName, DateTime.Now.ToString());
				}

				if(_logFile != null)
				{
					_logFile.Write(idHelper.RemoveColors(msg));
				}
			}

			// TODO
			// don't trigger any updates if we are in the process of doing a fatal error
			/*if ( com_errorEntered != ERP_FATAL ) {
				// update the console if we are in a long-running command, like dmap
				if ( com_refreshOnPrint ) {
					session->UpdateScreen();
				}

				// let session redraw the animated loading screen if necessary
				session->PacifierUpdate();
			}*/
		}

		public static void PrintWarnings()
		{
			if(_warningList.Count == 0)
			{
				return;
			}

			_warningList.Sort();

			idConsole.WriteLine("------------- Warnings ---------------");
			idConsole.WriteLine("during {0}...", _warningCaption);

			foreach(string warning in _warningList)
			{
				idConsole.WriteLine("{0}WARNING: {1}{2}", idColorString.Yellow, idColorString.Red, warning);
			}

			if(_warningList.Count > 0)
			{
				if(_warningList.Count >= idE.MaxWarningList)
				{
					idConsole.WriteLine("more than {0} warnings", idE.MaxWarningList);
				}
				else
				{
					idConsole.WriteLine("{0} warnings", _warningList.Count);
				}
			}
		}

		public static void ClearInputHistory()
		{
			_inputHistory.Clear();
		}

		public static void ClearWarnings(string reason)
		{
			_warningCaption = reason;
			_warningList.Clear();
		}
		#endregion

		#region Private
		private static void AddToDedicatedBuffer(string msg)
		{
			// TODO
			/*if ( win32.win_outputDebugString.GetBool() ) {
				OutputDebugString( msg );
			}
			if ( win32.win_outputEditString.GetBool() ) {*/
			idE.SystemConsole.Append(msg);
			/*}*/
		}

		private static void AddToScreenBuffer(string msg)
		{
			idE.Console.Print(msg);
		}		
		#endregion
		#endregion
	}
}