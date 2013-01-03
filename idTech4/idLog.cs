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
using System.Text;

using idTech4.Services;
using idTech4.Threading;

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

	public sealed class idLog
	{
		#region Properties
		public static bool RefreshOnPrint
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

		#region Members
		private static Queue<string> _inputHistory = new Queue<string>();

		private static string _warningCaption;
		private static List<string> _warningList = new List<string>(Constants.MaxWarningList);

		private static StringBuilder _redirectBuffer = null;
		private static EventHandler<RedirectBufferEventArgs> _redirectFlushHandler;

		private static StreamWriter _logFile;
		private static bool _logFileFailed;
		private static bool _recursingLogFileOpen;
		private static bool _refreshOnPrint;
		#endregion

		#region Methods
		#region Public
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
			if(idEngine.Instance.GetService<ICVarSystem>().GetBool("developer") == false)
			{
				return; // don't confuse non-developers with techie stuff...
			}

			// never refresh the screen, which could cause reentrency problems
			bool temp = _refreshOnPrint;
			_refreshOnPrint = false;

			Write(string.Format("{0}{1}", idColorString.Red, string.Format(format, args)));

			_refreshOnPrint = temp;
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

			string msg = string.Format(format, args);
			
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

			// TODO: com_printFilter
		/*	#ifndef ID_RETAIL
	if ( com_printFilter.GetString() != NULL && com_printFilter.GetString()[ 0 ] != '\0' ) {
		idStrStatic< 4096 > filterBuf = com_printFilter.GetString();
		idStrStatic< 4096 > msgBuf = msg;
		filterBuf.ToLower();
		msgBuf.ToLower();
		char *sp = strtok( &filterBuf[ 0 ], ";" );
		bool p = false;
		for( ; sp != NULL ; ) {
			if ( strstr( msgBuf, sp ) != NULL ) {
				p = true;
				break;
			}
			sp = strtok( NULL, ";" );
		}
		if ( !p ) {
			return;
		}
	}
#endif*/

			if(idThread.IsMainThread == false)
			{
				Debug.WriteLine(msg);
				return;
			}

			// echo to console buffer
			AddToConsoleBuffer(msg);

			// remove any color codes
			msg = idColor.StripColors(msg);

			// echo to dedicated console and early console
			Print(msg);

			// print to script debugger server
			// DebuggerServerPrint( msg );

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			IFileSystem fileSystem = idEngine.Instance.GetService<IFileSystem>();

			// logFile
			if((cvarSystem != null) && (cvarSystem.GetInt("logFile") != 0) && (_logFileFailed == false))
			{
				if((_logFile == null) && (_recursingLogFileOpen == false))
				{
					string fileName = "qconsole.log";

					if(cvarSystem.GetString("logFileName") != string.Empty)
					{
						fileName = cvarSystem.GetString("logFileName");
					}

					// fileSystem->OpenFileWrite can cause recursive prints into here
					_recursingLogFileOpen = true;

					Stream s = fileSystem.OpenFileWrite(fileName);

					if(s == null)
					{
						_logFileFailed = true;
						idEngine.Instance.FatalError("failed to open log file '{0}'", fileName);
					}

					_recursingLogFileOpen = false;
					_logFile = new StreamWriter(s);
					_logFile.AutoFlush = true;

					WriteLine("log file '{0}' opened on {1}", fileName, DateTime.Now.ToString());
				}

				if(_logFile != null)
				{
					_logFile.Write(idColor.StripColors(msg));
				}
			}

			// TODO: IMPORTANT!
			// don't trigger any updates if we are in the process of doing a fatal error
			/*if ( com_errorEntered != ERP_FATAL ) {
				// update the console if we are in a long-running command, like dmap
				if ( com_refreshOnPrint ) {
					const bool captureToImage = false;
					UpdateScreen( captureToImage );
				}
			}*/
		}

		public static void PrintWarnings()
		{
			if(_warningList.Count == 0)
			{
				return;
			}

			_warningList.Sort();

			WriteLine("------------- Warnings ---------------");
			WriteLine("during {0}...", _warningCaption);

			foreach(string warning in _warningList)
			{
				WriteLine("{0}WARNING: {1}{2}", idColorString.Yellow, idColorString.Red, warning);
			}

			if(_warningList.Count > 0)
			{
				if(_warningList.Count >= _warningList.Capacity)
				{
					WriteLine("more than {0} warnings", _warningList.Capacity);
				}
				else
				{
					WriteLine("{0} warnings", _warningList.Count);
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
		private static void AddToConsoleBuffer(string msg)
		{
			// TODO: important! idE.Console.WriteLine(msg);
		}

		public static void Print(string format, params object[] args)
		{
			string msg = string.Format(format, args);

			Debug.WriteLine(msg);;

			// TODO: IMPORTANT! conbuf_appendtext
			/*if ( win32.win_outputEditString.GetBool() && idLib::IsMainThread() ) {
				Conbuf_AppendText( msg );
			}*/
		}
		#endregion
		#endregion
	}
}