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
		#region Constants
		private const int LineWidth = 78;
		private const int TextSize = 0x30000;
		private const int TotalLines = TextSize / LineWidth;

		private const int Repeat = 100;
		private const int FirstRepeat = 200;
		#endregion

		#region Members
		private static Queue<string> _inputHistory = new Queue<string>();

		private static string _warningCaption;
		private static List<string> _warningList = new List<string>();

		private static bool _logFileFailed;

		private static StringBuilder _redirectBuffer = null;
		private static EventHandler<RedirectBufferEventArgs> _redirectFlushHandler;

		private static int _screenConsoleCurrentLine; // line where next message will be printed.
		private static int _screenConsoleCurrentPosition; // offset in current line for next print.
		private static int _screenConsoleDisplay; // bottom of console displays this line.

		private static StringBuilder _screenConsoleBuffer = new StringBuilder(0, TextSize);
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
			/*if(idE.CvarSystem.IsInitialized == false)
			{
				return;
			}*/

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
			// TODO: AddToScreenBuffer(msg);
			AddToDedicatedBuffer(idHelper.RemoveColors(msg));

			// print to script debugger server
			// DebuggerServerPrint( msg );

			// TODO
			// logFile
			/*if ( com_logFile.GetInteger() && !logFileFailed && fileSystem->IsInitialized() ) {
				static bool recursing;

				if ( !logFile && !recursing ) {
					struct tm *newtime;
					ID_TIME_T aclock;
					const char *fileName = com_logFileName.GetString()[0] ? com_logFileName.GetString() : "qconsole.log";

					// fileSystem->OpenFileWrite can cause recursive prints into here
					recursing = true;

					logFile = fileSystem->OpenFileWrite( fileName );
					if ( !logFile ) {
						logFileFailed = true;
						FatalError( "failed to open log file '%s'\n", fileName );
					}

					recursing = false;

					if ( com_logFile.GetInteger() > 1 ) {
						// force it to not buffer so we get valid
						// data even if we are crashing
						logFile->ForceFlush();
					}

					time( &aclock );
					newtime = localtime( &aclock );
					Printf( "log file '%s' opened on %s\n", fileName, asctime( newtime ) );
				}
				if ( logFile ) {
					logFile->Write( msg, strlen( msg ) );
					logFile->Flush();	// ForceFlush doesn't help a whole lot
				}
			}*/

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


			// TODO
			/*#ifdef _WIN32

				if ( com_outputMsg ) {
					if ( com_msgID == -1 ) {
						com_msgID = ::RegisterWindowMessage( DMAP_MSGID );
						if ( !FindEditor() ) {
							com_outputMsg = false;
						} else {
							Sys_ShowWindow( false );
						}
					}
					if ( com_hwndMsg ) {
						ATOM atom = ::GlobalAddAtom( msg );
						::PostMessage( com_hwndMsg, com_msgID, 0, static_cast<LPARAM>(atom) );
					}
				}

			#endif*/
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
#if ID_ALLOW_TOOLS
			// TODO
			/*RadiantPrint( txt );

			if( com_editors & EDITOR_MATERIAL ) {
				MaterialEditorPrintConsole(txt);
			}*/
#endif

			int color = idHelper.ColorIndex(idColor.Cyan);
			int y = 0, c = 0, l = 0;

			for(int i = 0; i < msg.Length; i++)
			{
				c = msg[i];

				if(idHelper.IsColor(msg, i) == true)
				{
					if(msg[i + 1] == (int) idColor.Default)
					{
						color = idHelper.ColorIndex(idColor.Cyan);
					}
					else
					{
						color = idHelper.ColorIndex((idColor) msg[i + 1]);
					}

					i += 1;
					continue;
				}

				y = _screenConsoleCurrentLine % TotalLines;

				// if we are about to print a new word, check to see
				// if we should wrap to the new line
				if((c > ' ') && (_screenConsoleCurrentPosition == 0) || (_screenConsoleBuffer[y * LineWidth + _screenConsoleCurrentPosition - 1] <= ' '))
				{
					// count word length
					for(l = 0; l < LineWidth; l++)
					{
						if(msg[l] <= ' ')
						{
							break;
						}
					}

					// word wrap
					if((l != LineWidth) && ((_screenConsoleCurrentPosition + l) >= LineWidth))
					{
						ScreenLineFeed();
					}
				}

				switch(c)
				{
					case '\n':
						ScreenLineFeed();
						break;

					case '\t':
						do
						{
							_screenConsoleBuffer[y * LineWidth + _screenConsoleCurrentPosition] = (char) ((color << 8) | ' ');
							_screenConsoleCurrentPosition++;

							if(_screenConsoleCurrentPosition >= LineWidth)
							{
								ScreenLineFeed();
								_screenConsoleCurrentPosition = 0;
							}
						}
						while((_screenConsoleCurrentPosition & 3) > 0);
						break;

					case '\r':
						_screenConsoleCurrentPosition = 0;
						break;

					default:
						// display character and advance
						_screenConsoleBuffer[y * LineWidth + _screenConsoleCurrentPosition] = (char) ((color << 8) | c);
						_screenConsoleCurrentPosition++;

						if(_screenConsoleCurrentPosition >= LineWidth)
						{
							ScreenLineFeed();
							_screenConsoleCurrentPosition = 0;
						}
						break;
				}
			}

			// mark time for transparent overlay
			// TODO
			/*if ( current >= 0 ) {
				times[current % NUM_CON_TIMES] = com_frameTime;
			}*/
		}

		private static void ScreenLineFeed()
		{
			// TODO
			// mark time for transparent overlay
			/*if ( current >= 0 ) {
				times[current % NUM_CON_TIMES] = com_frameTime;
			}*/

			_screenConsoleCurrentPosition = 0;

			if(_screenConsoleDisplay == _screenConsoleCurrentLine)
			{
				_screenConsoleDisplay++;
			}

			_screenConsoleCurrentLine++;

			for(int i = 0; i < LineWidth; i++)
			{
				_screenConsoleBuffer[(_screenConsoleCurrentLine % TotalLines) * LineWidth + i] = (char) ((idHelper.ColorIndex(idColor.Cyan) << 8) | ' ');
			}
		}
		#endregion
		#endregion
	}
}