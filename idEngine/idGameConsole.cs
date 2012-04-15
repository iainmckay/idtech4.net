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

using idTech4.Renderer;
using idTech4.UI;

namespace idTech4
{
	public sealed class idGameConsole
	{
		#region Constants
		private const int Repeat = 100;
		private const int FirstRepeat = 200;
		#endregion

		#region Properties
		/// <summary>
		/// Causes the console to start opening the desired amount.
		/// </summary>
		public float DisplayFraction
		{
			set
			{
				_finalFraction = value;
				_fractionTime = idE.System.FrameTime;
			}
		}
		#endregion

		#region Members
		private idInputField _consoleField = new idInputField();
		private List<string> _buffer = new List<string>();
		private Queue<int> _notificationTimes = new Queue<int>();

		private bool _keyCatching;

		private float _displayFraction;	// approaches finalFrac at scr_conspeed
		private float _finalFraction;	// 0.0 to 1.0 lines of console to display
		private int _fractionTime;		// time of last displayFrac update

		private int _currentLine;		// line where next message will be printed
		private int _currentPositionX;	// offset in current line for next print
		private int _display;			// bottom of console displays this line
		private int _lastKeyEvent;		// time of last key event for scroll delay
		private int _nextKeyEvent;		// keyboard repeat rate

		// fps counter
		private int _previousFrameTime;
		private int[] _previousFrameTimes = new int[4];
		private int _frameIndex;

		private idMaterial _charSetShader;
		private idMaterial _whiteShader;
		private idMaterial _consoleShader;
		#endregion

		#region Constructor
		public idGameConsole()
		{
			new idCvar("con_speed", "3", "speed at which the console moves up and down", CvarFlags.System);
			new idCvar("con_notifyTime", "3", "time messages are displayed onscreen when console is pulled up", CvarFlags.System);
			new idCvar("con_noPrint", (idE.Platform.IsDebug == true) ? "0" : "1", "print on the console but not onscreen when console is pulled up", CvarFlags.Bool | CvarFlags.System | CvarFlags.NoCheat);
		}
		#endregion

		#region Methods
		#region Public
		public void Close()
		{
			this.DisplayFraction = 0.0f;

			_keyCatching = false;
			_displayFraction = 0;	// don't scroll to that point, go immediately

			// TODO: ClearNotifyLines();
		}

		public void Draw(bool forceFullScreen)
		{
			float y = 0.0f;

			if(_charSetShader == null)
			{
				return;
			}

			if(forceFullScreen == true)
			{
				// if we are forced full screen because of a disconnect, 
				// we want the console closed when we go back to a session state
				Close();

				// we are however catching keyboard input
				_keyCatching = true;
			}

			Scroll();
			// TODO
			/*UpdateDisplayFraction();

			if(forceFullScreen == true)
			{
				DrawSolidConsole(1.0f);
			}
			else if(_displayFraction > 0)
			{
				DrawSolidConsole(_displayFraction);
			}
			else
			{
				// only draw the notify lines if the developer cvar is set,
				// or we are a debug build
				if(idE.CvarSystem.GetBool("con_noPrint") == false)
				{
					DrawNotify();
				}
			}*/

			if(idE.CvarSystem.GetBool("com_showFPS") == true)
			{
				y = DrawFPS(0);
			}

			// TODO
			/*if(com_showMemoryUsage.GetBool())
			{
				y = SCR_DrawMemoryUsage(y);
			}

			if(com_showAsyncStats.GetBool())
			{
				y = SCR_DrawAsyncStats(y);
			}

			if(com_showSoundDecoders.GetBool())
			{
				y = SCR_DrawSoundDecoders(y);
			}*/
		}

		public void Dump(string file)
		{
			Stream f = idE.FileSystem.OpenFileWrite(file);

			if(f == null)
			{
				idConsole.Warning("couldn't open {0}", file);
			}
			else
			{
				using(StreamWriter w = new StreamWriter(f))
				{
					foreach(string str in _buffer)
					{
						w.WriteLine(idHelper.RemoveColors(str));
					}
				}
			}
		}

		public void Init()
		{
			idE.CmdSystem.AddCommand("clear", "clears the console", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Clear));
			idE.CmdSystem.AddCommand("conDump", "dumps the console text to a file", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Dump));
		}

		/// <summary>
		/// Can't be combined with init, because init happens before the renderSystem is initialized.
		/// </summary>
		public void LoadGraphics()
		{
			_charSetShader = idE.DeclManager.FindMaterial("textures/bigchars");
			_whiteShader = idE.DeclManager.FindMaterial("_white");
			_consoleShader = idE.DeclManager.FindMaterial("console");
		}

		public void Print(string msg)
		{
			// TODO: colors
			List<string> parts = new List<string>(msg.Replace("\r", "").Split('\n'));
			parts.RemoveAll(m => m.Length == 0);

			_buffer.AddRange(parts);

			// mark time for transparent overlay
			/*if ( current >= 0 ) {
				times[current % NUM_CON_TIMES] = com_frameTime;
			}*/
		}
		#endregion

		#region Private
		private float DrawFPS(float y)
		{
			// don't use serverTime, because that will be drifting to
			// correct for internet lag changes, timescales, timedemos, etc
			int t = idE.System.Milliseconds;
			int frameTime = t - _previousFrameTime;
			int fpsFrames = _previousFrameTimes.Length;

			_previousFrameTime = t;
			_previousFrameTimes[_frameIndex % fpsFrames] = frameTime;
			_frameIndex++;
	
			if(_frameIndex > fpsFrames)
			{
				// average multiple frames together to smooth changes out a bit
				int total = 0;

				for(int i = 0; i < fpsFrames; i++)
				{
					total += _previousFrameTimes[i];
				}

				if(total == 0)
				{
					total = 1;
				}

				int fps = (10000 * fpsFrames) / total;
				fps = (fps + 5) / 10;

				string s = string.Format("{0}fps", fps);
				int width = s.Length * idE.BigCharacterWidth;
			
				idE.RenderSystem.DrawBigString(635 - width, (int) y + 2, s, idColor.White, true, _charSetShader);
			}

			return (y + idE.BigCharacterHeight + 4);
		}

		/// <summary>
		/// Deals with scrolling text because we don't have key repeat.
		/// </summary>
		private void Scroll()
		{
			if((_lastKeyEvent == -1) || ((_lastKeyEvent + 200) > idE.EventLoop.Milliseconds))
			{
				return;
			}

			// console scrolling
			// TODO: console scrolling
			/*if ( idKeyInput::IsDown( K_PGUP ) ) {
				PageUp();
				nextKeyEvent = CONSOLE_REPEAT;
				return;
			}

			if ( idKeyInput::IsDown( K_PGDN ) ) {
				PageDown();
				nextKeyEvent = CONSOLE_REPEAT;
				return;
			}*/
		}
		#endregion

		#region Command handlers
		private void Cmd_Clear(object sender, CommandEventArgs e)
		{
			_consoleField.Clear();
			_buffer.Clear();
		}

		private void Cmd_Dump(object sender, CommandEventArgs e)
		{
			if(e.Args.Length != 2)
			{
				idConsole.WriteLine("usage: conDump <filename>");
			}
			else
			{
				string fileName = e.Args.Get(1);

				if(Path.HasExtension(fileName) == false)
				{
					fileName += ".txt";
				}

				idConsole.WriteLine("Dumped console text to {0}.", fileName);
				Dump(fileName);
			}
		}
		#endregion
		#endregion
	}
}