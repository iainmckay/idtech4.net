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

using Microsoft.Xna.Framework;

using idTech4.Input;
using idTech4.Renderer;
using idTech4.UI;

namespace idTech4
{
	public sealed class idGameConsole
	{
		#region Constants
		private const int Repeat = 100;
		private const int FirstRepeat = 200;
		private const int LineWidth = 78;
		private const int TextSize = 0x30000;
		private const int TotalLines = (TextSize / LineWidth);
		private const int NotificationCount = 4;
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

		public bool IsActive
		{
			get
			{
				return _keyCatching;
			}
		}
		#endregion

		#region Members
		private idInputField _consoleField = new idInputField();
		private List<string> _buffer = new List<string>();
		private int[] _notificationTimes = new int[idGameConsole.NotificationCount];

		private bool _keyCatching;

		private float _displayFraction;	// approaches finalFrac at scr_conspeed
		private float _finalFraction;	// 0.0 to 1.0 lines of console to display
		private int _fractionTime;		// time of last displayFrac update

		private int _currentLine;		// line where next message will be printed
		private int _currentPositionX;	// offset in current line for next print
		private int _display;			// bottom of console displays this line
		private int _lastKeyEvent;		// time of last key event for scroll delay
		private int _nextKeyEvent;		// keyboard repeat rate

		private int _visibleLines;

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

			ClearNotificationLines(); 
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
			UpdateDisplayFraction();

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
			}

			if(idE.CvarSystem.GetBool("com_showFPS") == true)
			{
				y = DrawFPS(0);
			}

			if(idE.CvarSystem.GetBool("com_showMemoryUsage") == true)
			{
				idConsole.Warning("TODO: y = SCR_DrawMemoryUsage(y);");
			}

			if(idE.CvarSystem.GetBool("com_showAsyncStats") == true)
			{
				idConsole.Warning("TODO: y = SCR_DrawAsyncStats(y);");
			}

			if(idE.CvarSystem.GetBool("com_showSoundDecoders") == true)
			{
				idConsole.Warning("TODO: y = SCR_DrawSoundDecoders(y);");
			}
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

		public void WriteLine(string msg)
		{
			List<string> parts = new List<string>(msg.Replace("\r", "").Split('\n'));
			parts.RemoveAll(m => m.Length == 0);

			if(_display == _currentLine)
			{
				_display += parts.Count;
			}

			_currentLine += parts.Count;
			_buffer.AddRange(parts);

			// mark time for transparent overlay
			if(_currentLine >= 0)
			{
				_notificationTimes[_currentLine % idGameConsole.NotificationCount] = idE.System.FrameTime;
			}
		}

		public bool ProcessEvent(SystemEvent ev, bool forceAccept)
		{
			 bool consoleKey = (ev.Type == SystemEventType.Key)
									&& ((Keys) ev.Value == Keys.Oem8);

			// we always catch the console key event
			if((forceAccept == false) && (consoleKey == true))
			{
				// ignore up events
				if(ev.Value2 == 0)
				{
					return true;
				}			

				idConsole.Warning("TODO: _consoleField.ClearAutoComplete();");

				// a down event will toggle the destination lines
				if(_keyCatching == true)
				{
					Close();

					idE.Input.GrabMouse = true;
					idE.CvarSystem.SetBool("ui_chat", false);
				}
				else
				{
					_consoleField.Clear();
					_keyCatching = true;

					if(idE.Input.IsKeyDown(Keys.LeftShift) == true)
					{
						// if the shift key is down, don't open the console as much
						SetDisplayFraction(0.2f);
					}
					else
					{
						SetDisplayFraction(0.5f);
					}

					idE.CvarSystem.SetBool("ui_chat", true);
				}

				return true;
			}

			// if we aren't key catching, dump all the other events
			if((forceAccept == false) && (_keyCatching == false))
			{
				return false;
			}

			// handle key and character events
			if(ev.Type == SystemEventType.Char)
			{
				idConsole.Warning("TODO: ev char");

				// never send the console key as a character
				/*if ( event->evValue != Sys_GetConsoleKey( false ) && event->evValue != Sys_GetConsoleKey( true ) ) {
					consoleField.CharEvent( event->evValue );
				}*/
				return true;
			}

			if(ev.Type == SystemEventType.Key)
			{
				// ignore up key events
				if(ev.Value2 == 0)
				{
					return true;
				}

				idConsole.Warning("TODO: KeyDownEvent( event->evValue );");
		
				return true;
			}

			// we don't handle things like mouse, joystick, and network packets
			return false;
		}
		#endregion

		#region Private
		private void ClearNotificationLines()
		{
			int count = _notificationTimes.Length;

			for(int i = 0; i < count; i++)
			{
				_notificationTimes[i] = 0;
			}
		}

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

		private void DrawInput()
		{
			int y = _visibleLines - (idE.SmallCharacterHeight * 2);

			// TODO
			/*if ( consoleField.GetAutoCompleteLength() != 0 ) {
				autoCompleteLength = strlen( consoleField.GetBuffer() ) - consoleField.GetAutoCompleteLength();

				if ( autoCompleteLength > 0 ) {
					renderSystem->SetColor4( .8f, .2f, .2f, .45f );

					renderSystem->DrawStretchPic( 2 * SMALLCHAR_WIDTH + consoleField.GetAutoCompleteLength() * SMALLCHAR_WIDTH,
									y + 2, autoCompleteLength * SMALLCHAR_WIDTH, SMALLCHAR_HEIGHT - 2, 0, 0, 0, 0, whiteShader );

				}
			}*/

			idE.RenderSystem.Color = idColor.Cyan;
			idE.RenderSystem.DrawSmallCharacter(1 * idE.SmallCharacterWidth, y, ']', _charSetShader);

			//_consoleField.Draw(2 * idE.SmallCharacterWidth, y, idE.VirtualScreenWidth - 3 * idE.SmallCharacterWidth, true, _charSetShader);
		}

		private void DrawNotify()
		{
			if(idE.CvarSystem.GetBool("con_noPrint") == true)
			{
				return;
			}

			Vector4 color = idColor.White;
			idE.RenderSystem.Color = color;

			int y = 0;

			for(int i = _currentLine - (idGameConsole.NotificationCount + 1); i <= _currentLine; i++)
			{
				if(i < 0)
				{
					continue;
				}

				int time = _notificationTimes[i % idGameConsole.NotificationCount];

				if(time == 0)
				{
					continue;
				}

				time = idE.System.FrameTime - time;

				if(time > (idE.CvarSystem.GetFloat("con_notifyTime") * 1000))
				{
					continue;
				}

				if(i >= _buffer.Count)
				{
					break;
				}

				string text = _buffer[i];
				int length = text.Length;

				for(int n = 0, x = 0; n < length; n++, x++)
				{
					if(idHelper.IsColor(text, n) == true)
					{
						color = idHelper.ColorForIndex(text[n + 1]);
						idE.RenderSystem.Color = color;
						n += 1;
		
						continue;
					}

					idE.RenderSystem.DrawSmallCharacter((x + 1) * idE.SmallCharacterWidth, y, text[n], _charSetShader);
				}

				y += idE.SmallCharacterHeight;
			}

			idE.RenderSystem.Color = idColor.Cyan;
		}

		/// <summary>
		/// Draws the console with the solid background.
		/// </summary>
		/// <param name="fraction"></param>
		private void DrawSolidConsole(float fraction)
		{
			int lines = (int) ((float) idE.VirtualScreenHeight * fraction);

			if(lines <= 0)
			{
				return;
			}

			if(lines > idE.VirtualScreenHeight)
			{
				lines = idE.VirtualScreenHeight;
			}

			// draw the background
			float y = fraction * idE.VirtualScreenHeight - 2;

			if(y < 1.0f)
			{
				y = 0.0f;
			}
			else
			{
				idE.RenderSystem.DrawStretchPicture(0, 0, idE.VirtualScreenWidth, y, 0, 1.0f - _displayFraction, 1, 1, _consoleShader);
			}

			idE.RenderSystem.Color = idColor.Cyan;
			idE.RenderSystem.DrawStretchPicture(0, y, idE.VirtualScreenWidth, 2, 0, 0, 0, 0, _whiteShader);
			idE.RenderSystem.Color = idColor.White;

			DrawVersion(lines);
			DrawText(lines, y);

			// draw the input prompt, user text, and cursor if desired
			DrawInput();

			idE.RenderSystem.Color = idColor.Cyan;
		}

		private void DrawText(int lines, float y)
		{
			_visibleLines = lines;

			int rows = (lines - idE.SmallCharacterWidth) / idE.SmallCharacterWidth; // rows of text to draw
			int n, x = 0;
			y = lines - (idE.SmallCharacterHeight * 3);
			Vector4 color = idColor.Cyan;

			// draw from the bottom up
			if(_display != _currentLine)
			{
				// draw arrows to show the buffer is backscrolled
				idE.RenderSystem.Color = idColor.Cyan;

				for(x = 0; x < LineWidth; x += 4)
				{
					idE.RenderSystem.DrawSmallCharacter((x + 1) * idE.SmallCharacterWidth, (int) y, '^', _charSetShader);
				}

				y -= idE.SmallCharacterHeight;
				rows--;
			}

			int row = _display;

			if(x == 0)
			{
				row--;
			}

			Vector4 currentColor = idColor.White;
			idE.RenderSystem.Color = currentColor;
			
			for(int i = 0; i < rows; i++, y -= idE.SmallCharacterHeight, row--)
			{
				if((row < 0) || (row > _buffer.Count))
				{
					break;
				}

				if((_currentLine - row) >= TotalLines)
				{
					// past scrollback wrap point
					continue;
				}

				string text = _buffer[row];
				int length = text.Length;

				for(n = 0, x = 0; n < length; n++, x++)
				{
					if(idHelper.IsColor(text, n) == true)
					{
						color = idHelper.ColorForIndex(text[n + 1]);
						n += 1;

						continue;
					}

					if(color != currentColor)
					{
						currentColor = color;
						idE.RenderSystem.Color = color;
					}

					idE.RenderSystem.DrawSmallCharacter((x + 1) * idE.SmallCharacterWidth, (int) y, text[n], _charSetShader);
				}
			}
		}

		private void DrawVersion(int lines)
		{
			string version = string.Format("{0}.{1}", idE.EngineVersion, idVersion.BuildCount);

			idE.RenderSystem.Color = idColor.Cyan;
			idE.RenderSystem.DrawSmallString(
				idE.VirtualScreenWidth - (version.Length * idE.SmallCharacterWidth),
				(lines - (idE.SmallCharacterHeight + (idE.SmallCharacterHeight / 2))),
				version, idColor.White, false, _charSetShader);
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

		/// <summary>
		/// Causes the console to start opening the desired amount.
		/// </summary>
		/// <param name="fraction"></param>
		private void SetDisplayFraction(float fraction)
		{
			_finalFraction = fraction;
			_fractionTime = idE.System.FrameTime;
		}

		/// <summary>
		/// Scrolls the console up or down based on conspeed.
		/// </summary>
		private void UpdateDisplayFraction()
		{
			if(idE.CvarSystem.GetFloat("con_speed") <= 0.1f)
			{
				_fractionTime = idE.System.FrameTime;
				_displayFraction = _finalFraction;
			}
			else
			{
				// scroll towards the destination height
				if(_finalFraction < _displayFraction)
				{
					_displayFraction -= idE.CvarSystem.GetFloat("con_speed") * (idE.System.FrameTime - _fractionTime) * 0.001f;

					if(_finalFraction > _displayFraction)
					{
						_displayFraction = _finalFraction;
					}

					_fractionTime = idE.System.FrameTime;
				}
				else if(_finalFraction > _displayFraction)
				{
					_displayFraction += idE.CvarSystem.GetFloat("con_speed") * (idE.System.FrameTime - _fractionTime) * 0.001f;

					if(_finalFraction < _displayFraction)
					{
						_displayFraction = _finalFraction;
					}

					_fractionTime = idE.System.FrameTime;
				}
			}
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

				Dump(fileName);

				idConsole.WriteLine("Dumped console text to {0}.", fileName);
			}
		}
		#endregion
		#endregion
	}
}