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

using Microsoft.Xna.Framework;

using idTech4.Input;
using idTech4.Services;

namespace idTech4
{
	public class idConsole : IConsole
	{
		#region Constants
		private const int FrameCount  = 6;
		private const int NotifyTimes = 4;
		#endregion

		#region Properties
		/// <summary>
		/// Causes the console to start opening the desired amount.
		/// </summary>
		public float DisplayFraction
		{
			get
			{
				return _finalFraction;
			}
			set
			{
				_finalFraction = value;
				_fractionTime  = idEngine.Instance.ElapsedTime;
			}
		}
		#endregion

		#region Members
		private bool _initialized;

		private float _displayFraction;	// approaches finalFrac at con_speed
		private float _finalFraction;	// 0.0 to 1.0 lines of console to display
		private long _fractionTime;		// time of last displayFrac update
		private long _lastKeyEvent;     // time of last key event for scroll delay
		private long _nextKeyEvent;	    // keyboard repeat rate
		private int	_currentLine;       // line where next message will be printed
		private int _visibleLines;      // in scanlines
		
		private int _currentX;          // offset in current line for next print
		private int _currentDisplay;    // bottom of console displays this line

		// allow these constants to be adjusted for HMD
		private int _localSafeLeft;
		private int _localSafeRight;
		private int _localSafeTop;
		private int _localSafeBottom;
		private int _localSafeWidth;
		private int _localSafeHeight;

		private int _lineWidth;
		private int _totalLines;

		private bool _keyCatching;

		// fps meter
		private long[] _fpsPreviousTimes = new long[FrameCount];
		private long _fpsPreviousTime;
		private int _fpsIndex;

		// notifications
		private long[] _notifyTimes = new long[NotifyTimes];
		
		private idInputField _consoleField;
		private List<string> _overlayText = new List<string>();

		private char[] _text = new char[Constants.ConsoleTextSize];
		#endregion
		
		#region Constructor
		public idConsole()
		{

		}
		#endregion

		#region Methods
		/// <summary>
		/// Deals with scrolling text because we don't have key repeat.
		/// </summary>
		private void Scroll()
		{
			IEventLoop eventLoop     = idEngine.Instance.GetService<IEventLoop>();
			IInputSystem inputSystem = idEngine.Instance.GetService<IInputSystem>();

			if((_lastKeyEvent == -1) || ((_lastKeyEvent + 200) > eventLoop.ElapsedTime))
			{
				return;
			}

			// console scrolling
			if(inputSystem.IsKeyDown(Keys.PageUp) == true)
			{
				idLog.Warning("TODO: PageUp();");

				_nextKeyEvent = Constants.ConsoleRepeat;
			}
			else if(inputSystem.IsKeyDown(Keys.PageDown) == true)
			{
				idLog.Warning("TODO: PageDown();");

				_nextKeyEvent = Constants.ConsoleRepeat;
			}
		}

		private void UpdateDisplayFraction()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			float speed            = cvarSystem.GetFloat("con_speed");
			long elapsedTime       = idEngine.Instance.ElapsedTime;

			if(speed <= 0.1f)
			{
				_fractionTime = elapsedTime;
				_displayFraction = _finalFraction;
			}
			else
			{
				// scroll towards the destination height
				if(_finalFraction < _displayFraction)
				{
					_displayFraction -= (speed * (elapsedTime - _fractionTime) * 0.001f);

					if(_finalFraction > _displayFraction)
					{
						_displayFraction = _finalFraction;
					}

					_fractionTime = elapsedTime;
				}
				else if(_finalFraction > _displayFraction)
				{
					_displayFraction += (speed * (elapsedTime - _fractionTime) * 0.001f);

					if(_finalFraction < _displayFraction)
					{
						_displayFraction = _finalFraction;
					}

					_fractionTime = elapsedTime;
				}
			}
		}
		#endregion

		#region IConsole implementation
		#region Drawing
		public void Draw(bool forceFullScreen)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

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
			else if(_displayFraction != 0)
			{
				DrawSolidConsole(_displayFraction);
			} 
			else 
			{
				// only draw the notify lines if the developer cvar is set,
				// or we are a debug build
				if(cvarSystem.GetBool("con_noPrint") == false)
				{
					DrawNotify();
				}
			}

			float leftY   = _localSafeTop;
			float rightY  = _localSafeTop;
			float centerY = _localSafeTop;

			if(cvarSystem.GetBool("com_showFPS") == true)
			{
				rightY = DrawFPS(rightY);
			}

			if(cvarSystem.GetBool("com_showMemoryUsage") == true)
			{
				rightY = DrawMemoryUsage(rightY);
			}

			DrawOverlayText(ref leftY, ref rightY, ref centerY);
			// TODO: DrawDebugGraphs();
		}

		private float DrawFPS(float y)
		{
			IRenderSystem renderSystem       = idEngine.Instance.GetService<IRenderSystem>();
			IResolutionScale resolutionScale = idEngine.Instance.GetService<IResolutionScale>();

			// ----------------------------------------------
			// don't use serverTime, because that will be drifting to
			// correct for internet lag changes, timescales, timedemos, etc
			{
				long elapsed   = idEngine.Instance.ElapsedTime;
				long frameTime = elapsed - _fpsPreviousTime;

				_fpsPreviousTime                          = elapsed;
				_fpsPreviousTimes[_fpsIndex % FrameCount] = frameTime;

				_fpsIndex++;

				if(_fpsIndex > FrameCount)
				{
					// average multiple frames together to smooth changes out a bit
					long total = 0;

					for(int i = 0; i < FrameCount; i++)
					{
						total += _fpsPreviousTimes[i];
					}

					if(total == 0)
					{
						total = 1;
					}

					long fps = (1000000 * FrameCount) / total;
					fps      = (fps + 500) / 1000;

					string s  = string.Format("{0}fps", fps);
					int width = s.Length * Constants.BigCharacterWidth;

					renderSystem.DrawBigString(_localSafeRight - width, (int) (y + 2), s, idColor.White, true);
				}

				y += Constants.BigCharacterHeight + 4;

				return y;
			}

			// ----------------------------------------------
			// print the resolution scale so we can tell when we are at reduced resolution
			{
				string resolutionText = resolutionScale.GetConsoleText();
				int width             = resolutionText.Length * Constants.BigCharacterWidth;

				renderSystem.DrawBigString(_localSafeRight - width, (int) (y + 2), resolutionText, idColor.White, true);

				int gameThreadTotalTime  = 0; // TODO: commonLocal.GetGameThreadTotalTime();
				int gameThreadGameTime   = 0; // TODO: commonLocal.GetGameThreadGameTime();
				int gameThreadRenderTime = 0; // TODO: commonLocal.GetGameThreadRenderTime();
				int rendererBackEndTime  = 0; // TODO: commonLocal.GetRendererBackEndMicroseconds();
				int rendererShadowsTime  = 0; // TODO: commonLocal.GetRendererShadowsMicroseconds();
				int rendererGPUIdleTime  = 0; // TODO: commonLocal.GetRendererIdleMicroseconds();
				int rendererGPUTime      = 0; // TODO: commonLocal.GetRendererGPUMicroseconds();
				int maxTime              = 16;

				y += Constants.SmallCharacterHeight + 4;
	
				// ---------------------------------------------------
				// G+RF
				string str = string.Format("{0}G+RF: {1:0000}", (gameThreadTotalTime > maxTime) ? idColorString.Red : "", gameThreadTotalTime);
				width      = str.Length(false) * Constants.SmallCharacterWidth;

				renderSystem.DrawSmallString(_localSafeRight - width, (int) (y + 2), str, idColor.White, false);

				y += Constants.SmallCharacterHeight + 4;

				// ---------------------------------------------------
				// G
				str   = string.Format("{0}G: {1:0000}", (gameThreadGameTime > maxTime) ? idColorString.Red : "", gameThreadGameTime);
				width = str.Length(false) * Constants.SmallCharacterWidth;

				renderSystem.DrawSmallString(_localSafeRight - width, (int) (y + 2), str, idColor.White, false);

				y += Constants.SmallCharacterHeight + 4;

				// ---------------------------------------------------
				// RF
				str   = string.Format("{0}RF: {1:0000}", (gameThreadRenderTime > maxTime) ? idColorString.Red : "", gameThreadRenderTime);
				width = str.Length(false) * Constants.SmallCharacterWidth;

				renderSystem.DrawSmallString(_localSafeRight - width, (int) (y + 2), str, idColor.White, false);

				y += Constants.SmallCharacterHeight + 4;

				// ---------------------------------------------------
				// RB
				str   = string.Format("{0}RF: {1:0000}", (rendererBackEndTime > maxTime) ? idColorString.Red : "", rendererBackEndTime);
				width = str.Length(false) * Constants.SmallCharacterWidth;

				renderSystem.DrawSmallString(_localSafeRight - width, (int) (y + 2), str, idColor.White, false);

				y += Constants.SmallCharacterHeight + 4;

				// ---------------------------------------------------
				// SV
				str   = string.Format("{0}SV: {1:0000.0}", (rendererShadowsTime > maxTime) ? idColorString.Red : "", rendererShadowsTime);
				width = str.Length(false) * Constants.SmallCharacterWidth;

				renderSystem.DrawSmallString(_localSafeRight - width, (int) (y + 2), str, idColor.White, false);

				y += Constants.SmallCharacterHeight + 4;

				// ---------------------------------------------------
				// IDLE
				str   = string.Format("{0}IDLE: {1:0000.0}", (rendererGPUIdleTime > (maxTime * 1000)) ? idColorString.Red : "", rendererGPUIdleTime / 1000.0f);
				width = str.Length(false) * Constants.SmallCharacterWidth;

				renderSystem.DrawSmallString(_localSafeRight - width, (int) (y + 2), str, idColor.White, false);

				y += Constants.SmallCharacterHeight + 4;

				// ---------------------------------------------------
				// GPU
				str   = string.Format("{0}GPU: {1:0000.0}", (rendererGPUTime > (maxTime * 1000)) ? idColorString.Red : "", rendererGPUTime / 1000.0f);
				width = str.Length(false) * Constants.SmallCharacterWidth;

				renderSystem.DrawSmallString(_localSafeRight - width, (int) (y + 2), str, idColor.White, false);

				y += Constants.SmallCharacterHeight + 4;

				return y;
			}
		}

		/// <summary>
		/// Draw the editline after a ] prompt.
		/// </summary>
		private void DrawInput() 
		{
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();

			int y = _visibleLines - (Constants.SmallCharacterHeight * 2);

			// TODO: autocomplete
			/*if ( consoleField.GetAutoCompleteLength() != 0 ) {
				autoCompleteLength = strlen( consoleField.GetBuffer() ) - consoleField.GetAutoCompleteLength();

				if ( autoCompleteLength > 0 ) {
					renderSystem->DrawFilled( idVec4( 0.8f, 0.2f, 0.2f, 0.45f ),
						LOCALSAFE_LEFT + 2 * SMALLCHAR_WIDTH + consoleField.GetAutoCompleteLength() * SMALLCHAR_WIDTH,
						y + 2, autoCompleteLength * SMALLCHAR_WIDTH, SMALLCHAR_HEIGHT - 2 );
				}
			}*/

			renderSystem.Color = idColor.Cyan;
			renderSystem.DrawSmallCharacter(_localSafeLeft + 1 * Constants.SmallCharacterWidth, y, ']');

			// TODO: console field
			// consoleField.Draw( LOCALSAFE_LEFT + 2 * SMALLCHAR_WIDTH, y, SCREEN_WIDTH - 3 * SMALLCHAR_WIDTH, true );
		}

		private float DrawMemoryUsage(float y)
		{
			return y;
		}

		/// <summary>
		/// Draws the last few lines of output transparently over the game top.
		/// </summary>
		private void DrawNotify()
		{
			ICVarSystem cvarSystem     = idEngine.Instance.GetService<ICVarSystem>();
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();

			if(cvarSystem.GetBool("con_noPrint") == true)
			{
				return;
			}
			
			idColorIndex currentColor = idColorIndex.White;
			long time                 = 0;
			float notifyTime          = cvarSystem.GetFloat("con_notifyTime");

			renderSystem.Color = idColor.FromIndex(currentColor);

			int v = 0;

			for(int i = (_currentLine - (NotifyTimes + 1)); i <= _currentLine; i++)
			{
				if(i < 0)
				{
					continue;
				}

				time = _notifyTimes[i % NotifyTimes];

				if(time == 0)
				{
					continue;
				}

				time = idEngine.Instance.ElapsedTime - time;

				if(time > (notifyTime * 1000))
				{
					continue;
				}

				int textOffset = (i % _totalLines) * _lineWidth;
		
				for(int x = 0; x < _lineWidth; x++)
				{
					char c = _text[textOffset + x];

					if((c & 0xFF) == ' ')
					{
						continue;
					}
					
					if((idColorIndex) (c >> 8) != currentColor) 
					{
						currentColor       = (idColorIndex) (c >> 8);
						renderSystem.Color = idColor.FromIndex(currentColor);
					}

					renderSystem.DrawSmallCharacter((int) (_localSafeLeft + (x + 1) * Constants.SmallCharacterWidth), (int) v, (char) (c * 0xFF));
				}

				v += Constants.SmallCharacterHeight;
			}

			renderSystem.Color = idColor.Cyan;
		}

		private void DrawOverlayText(ref float leftY, ref float rightY, ref float centerY)
		{
			if(_overlayText.Count > 0)
			{
				idLog.Warning("TODO: DrawOverlayText");
			}

			_overlayText.Clear();
		}

		/// <summary>
		/// Draws the console with the solid background.
		/// </summary>
		/// <param name="frac"></param>
		public void DrawSolidConsole(float fraction) 
		{
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();

			int lines = (int) (Constants.ScreenHeight * fraction);

			if(lines <= 0)
			{
				return;
			}

			if(lines > Constants.ScreenHeight)
			{
				lines = Constants.ScreenHeight;
			}

			// draw the background
			float y = fraction * Constants.ScreenHeight - 2;

			if(y < 1.0f)
			{
				y = 0.0f;
			}
			else
			{
				renderSystem.DrawFilled(new Vector4(0, 0, 0, 0.75f), 0, 0, Constants.ScreenWidth, y);
			}

			renderSystem.DrawFilled(idColor.Cyan, 0, y, Constants.ScreenWidth, 2);
			
			// draw the version number
			renderSystem.Color = idColor.Cyan;

			string version = string.Format("{0}.{1}.{2}", idLicensee.EngineVersion, /* TODO: BUILD_NUMBER */ 1, /* TODO: BUILD_NUMBER_MINOR */ 1);
			int i          = version.Length;
			int x          = 0;
			int row        = 0;
			int rows       = 0;

			for(x = 0; x < i; x++)
			{
				renderSystem.DrawSmallCharacter(_localSafeWidth - (i - x) * Constants.SmallCharacterWidth, (lines - (Constants.SmallCharacterHeight + Constants.SmallCharacterHeight / 4)), version[x]);
			}

			// draw the text
			_visibleLines = lines;
			rows          = (lines - Constants.SmallCharacterWidth) / Constants.SmallCharacterWidth; // rows of text to draw
			y             = lines - (Constants.SmallCharacterHeight * 3);

			// draw from the bottom up
			if(_currentDisplay != _currentLine)
			{
				// draw arrows to show the buffer is backscrolled
				renderSystem.Color = idColor.FromIndex(idColorIndex.Cyan);

				for(x = 0; x < _lineWidth; x += 4)
				{
					renderSystem.DrawSmallCharacter((int) (_localSafeLeft + (x + 1) * Constants.SmallCharacterWidth), (int) y, '^');
				}

				y -= Constants.SmallCharacterHeight;
				rows--;
			}

			row = _currentDisplay;
			
			if(x == 0)
			{
				row--;
			}

			idColorIndex currentColor = idColorIndex.White;
			renderSystem.Color        = idColor.FromIndex(currentColor);

			for(i = 0; i < rows; i++, y -= Constants.SmallCharacterHeight, row--)
			{
				if(row < 0)
				{
					break;
				}

				if((_currentLine - row) >= _totalLines)
				{
					// past scrollback wrap point
					continue;	
				}

				int textOffset = (row + _totalLines) * _lineWidth;
				char c         = _text[textOffset];

				for(x = 0; x < _lineWidth; x++)
				{
					if((c & 0xFF) == ' ')
					{
						continue;
					}

					if((idColorIndex) (c >> 8) != currentColor)
					{
						currentColor       = (idColorIndex) (c >> 8);
						renderSystem.Color = idColor.FromIndex(currentColor);
					}

					renderSystem.DrawSmallCharacter((int) (_localSafeLeft + (x + 1) * Constants.SmallCharacterWidth), (int) y, (char) (c & 0xFF));
				}
			}

			// draw the input prompt, user text, and cursor if desired
			DrawInput();

			renderSystem.Color = idColor.Cyan;
		}
		#endregion

		#region Events
		public bool ProcessEvent(SystemEvent ev, bool forceAccept)
		{
			ICVarSystem cvarSystem   = idEngine.Instance.GetService<ICVarSystem>();
			IInputSystem inputSystem = idEngine.Instance.GetService<IInputSystem>();

			bool consoleKey = ((ev.Type == SystemEventType.Key) && (ev.Value == (int) Keys.Grave) && (cvarSystem.GetBool("com_allowConsole") == true));

			// we always catch the console key event
			if((forceAccept == false) && (consoleKey == true))
			{
				// ignore up events
				if(ev.Value2 == 0)
				{
					return true;
				}

				idLog.Warning("TODO: consoleField.ClearAutoComplete();");

				// a down event will toggle the destination lines
				if(_keyCatching == true)
				{
					Close();
					inputSystem.GrabMouse = true;
				} 
				else 
				{
					idLog.Warning("TODO: consoleField.Clear();");
			
					_keyCatching = true;

					if((inputSystem.IsKeyDown(Keys.LeftShift) == true) || (inputSystem.IsKeyDown(Keys.RightShift) == true))
					{
						// if the shift key is down, don't open the console as much
						this.DisplayFraction = 0.2f;
					}
					else
					{
						this.DisplayFraction = 0.5f;
					}
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
				idLog.Warning("TODO: console typing");
	
				/*// never send the console key as a character
				if ( event->evValue != '`' && event->evValue != '~' ) {
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

				idLog.Warning("TODO: ProcessKeyDownEvent((Keys) ev.Value);");
		
				return true;
			}

			// we don't handle things like mouse, joystick, and network packets
			return false;
		}
		#endregion

		#region Initialization
		#region Properties
		public bool IsInitialized
		{
			get
			{
				return _initialized;
			}
		}
		#endregion

		#region Methods
		public void Initialize()
		{
			if(this.IsInitialized == true)
			{
				throw new Exception("idConsole has already been initialized.");
			}

			_localSafeLeft   = 32;
			_localSafeRight  = 608;
			_localSafeTop    = 24;
			_localSafeBottom = 456;
			_localSafeWidth  = _localSafeRight - _localSafeLeft;
			_localSafeHeight = _localSafeBottom - _localSafeTop;

			_lineWidth       = ((_localSafeWidth / Constants.SmallCharacterWidth) - 2);
			_totalLines      = Constants.ConsoleTextSize / _lineWidth;

			_consoleField = new idInputField();
			_consoleField.WidthInCharacters = _lineWidth;

			_initialized = true;
		}

		public void Close()
		{
			idLog.Warning("TODO: console.close");
		}
		#endregion
		#endregion
		#endregion
	}
}