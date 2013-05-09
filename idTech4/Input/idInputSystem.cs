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
using System.Text;

using idTech4.Services;

namespace idTech4.Input
{
	public class idInputSystem : IInputSystem
	{
		#region Key Names
		private struct KeyName
		{
			public Keys Key;
			public string Name;
			public string StringID;

			public KeyName(Keys key, string name)
				: this(key, name, name)
			{

			}

			public KeyName(Keys key, string name, string stringID)
			{
				this.Key      = key;
				this.Name     = name;
				this.StringID = stringID;
			}
		}

		// names not in this list can either be lowercase ascii, or '0xnn' hex sequences
		private readonly KeyName[] _keyNames = {
			new KeyName(Keys.Escape, "ESCAPE", "#str_07020"),
			new KeyName(Keys.D1, "1"),
			new KeyName(Keys.D2, "2"),
			new KeyName(Keys.D3, "3"),
			new KeyName(Keys.D4, "4"),
			new KeyName(Keys.D5, "5"),
			new KeyName(Keys.D6, "6"),
			new KeyName(Keys.D7, "7"),
			new KeyName(Keys.D8, "8"),
			new KeyName(Keys.D9, "9"),
			new KeyName(Keys.D0, "0"),
			new KeyName(Keys.Minus, "MINUS", "-"),
			new KeyName(Keys.Equals, "EQUALS", "="),
			new KeyName(Keys.Backspace, "BACKSPACE", "#str_07022"),
			new KeyName(Keys.Q, "Q"),
			new KeyName(Keys.W, "W"),
			new KeyName(Keys.E, "E"),
			new KeyName(Keys.R, "R"),
			new KeyName(Keys.T, "T"),
			new KeyName(Keys.Y, "Y"),
			new KeyName(Keys.U, "U"),
			new KeyName(Keys.I, "I"),
			new KeyName(Keys.O, "O"),
			new KeyName(Keys.P, "P"),
			new KeyName(Keys.LeftBracket, "LBRACKET", "["),
			new KeyName(Keys.RightBracket, "RBRACKET", "]"),
			new KeyName(Keys.Enter, "ENTER", "#str_07019"),
			new KeyName(Keys.LeftControl, "LCTRL", "#str_07028"),
			new KeyName(Keys.A, "A"),
			new KeyName(Keys.S, "S"),
			new KeyName(Keys.D, "D"),
			new KeyName(Keys.F, "F"),
			new KeyName(Keys.G, "G"),
			new KeyName(Keys.H, "H"),
			new KeyName(Keys.J, "J"),
			new KeyName(Keys.K, "K"),
			new KeyName(Keys.L, "L"),
			new KeyName(Keys.SemiColon, "SEMICOLON", "#str_07129"),
			new KeyName(Keys.Apostrophe, "APOSTROPHE", "#str_07130"),
			new KeyName(Keys.Grave, "GRAVE", "`"),
			new KeyName(Keys.LeftShift, "LSHIFT", "#str_07029"),
			new KeyName(Keys.Backslash, "BACKSLASH", "\\"),
			new KeyName(Keys.Z, "Z"),
			new KeyName(Keys.X, "X"),
			new KeyName(Keys.C, "C"),
			new KeyName(Keys.V, "V"),
			new KeyName(Keys.B, "B"),
			new KeyName(Keys.N, "N"),
			new KeyName(Keys.M, "M"),
			new KeyName(Keys.Comma, "COMMA", ","),
			new KeyName(Keys.Period, "PERIOD", "."),
			new KeyName(Keys.Slash, "SLASH", "/"),
			new KeyName(Keys.RightShift, "RSHIFT", "#str_bind_RSHIFT"),
			new KeyName(Keys.KeypadStar, "KP_STAR", "#str_07126"),
			new KeyName(Keys.LeftAlt, "LALT", "#str_07027"),
			new KeyName(Keys.Space, "SPACE", "#str_07021"),
			new KeyName(Keys.CapsLock, "CAPSLOCK", "#str_07034"),
			new KeyName(Keys.F1, "F1", "#str_07036"),
			new KeyName(Keys.F2, "F2", "#str_07037"),
			new KeyName(Keys.F3, "F3", "#str_07038"),
			new KeyName(Keys.F4, "F4", "#str_07039"),
			new KeyName(Keys.F5, "F5", "#str_07040"),
			new KeyName(Keys.F6, "F6", "#str_07041"),
			new KeyName(Keys.F7, "F7", "#str_07042"),
			new KeyName(Keys.F8, "F8", "#str_07043"),
			new KeyName(Keys.F9, "F9", "#str_07044"),
			new KeyName(Keys.F10, "F10", "#str_07045"),
			new KeyName(Keys.NumLock, "NUMLOCK", "#str_07125"),
			new KeyName(Keys.Scroll, "SCROLL", "#str_07035"),
			new KeyName(Keys.Keypad7, "KP_7", "#str_07110"),
			new KeyName(Keys.Keypad8, "KP_8", "#str_07111"),
			new KeyName(Keys.Keypad9, "KP_9", "#str_07112"),
			new KeyName(Keys.Minus, "KP_MINUS", "#str_07123"),
			new KeyName(Keys.Keypad4, "KP_4", "#str_07113"),
			new KeyName(Keys.Keypad5, "KP_5", "#str_07114"),
			new KeyName(Keys.Keypad6, "KP_6", "#str_07115"),
			new KeyName(Keys.KeypadPlus, "KP_PLUS", "#str_07124"),
			new KeyName(Keys.Keypad1, "KP_1", "#str_07116"),
			new KeyName(Keys.Keypad2, "KP_2", "#str_07117"),
			new KeyName(Keys.Keypad3, "KP_3", "#str_07118"),
			new KeyName(Keys.Keypad0, "KP_0", "#str_07120"),
			new KeyName(Keys.KeypadDot, "KP_DOT", "#str_07121"),
			new KeyName(Keys.F11, "F11", "#str_07046"),
			new KeyName(Keys.F12, "F12", "#str_07047"),
			new KeyName(Keys.F13, "F13", "F13"),
			new KeyName(Keys.F14, "F14", "F14"),
			new KeyName(Keys.F15, "F15", "F15"),
			new KeyName(Keys.Kana, "KANA"),
			new KeyName(Keys.Convert, "CONVERT"),
			new KeyName(Keys.NoConvert, "NOCONVERT"),
			new KeyName(Keys.Yen, "YEN"),
			new KeyName(Keys.KeypadEquals, "KP_EQUALS", "#str_07127"),
			new KeyName(Keys.Circumflex, "CIRCUMFLEX"),
			new KeyName(Keys.AT, "AT", "@"),
			new KeyName(Keys.Colon, "COLON", ":"),
			new KeyName(Keys.Underline, "UNDERLINE", "_"),
			new KeyName(Keys.Kanji, "KANJI"),
			new KeyName(Keys.Stop, "STOP"),
			new KeyName(Keys.AX, "AX"),
			new KeyName(Keys.Unlabeled, "UNLABELED"),
			new KeyName(Keys.KeypadEnter, "KP_ENTER", "#str_07119"),
			new KeyName(Keys.RightControl, "RCTRL", "#str_bind_RCTRL"),
			new KeyName(Keys.KeypadComma, "KP_COMMA", ","),
			new KeyName(Keys.KeypadSlash, "KP_SLASH", "#str_07122"),
			new KeyName(Keys.PrintScreen, "PRINTSCREEN", "#str_07179"),
			new KeyName(Keys.RightAlt, "RALT", "#str_bind_RALT"),
			new KeyName(Keys.Pause, "PAUSE", "#str_07128"),
			new KeyName(Keys.Home, "HOME", "#str_07052"),
			new KeyName(Keys.UpArrow, "UPARROW", "#str_07023"),
			new KeyName(Keys.PageUp, "PGUP", "#str_07051"),
			new KeyName(Keys.LeftArrow, "LEFTARROW", "#str_07025"),
			new KeyName(Keys.RightArrow, "RIGHTARROW", "#str_07026"),
			new KeyName(Keys.End, "END", "#str_07053"),
			new KeyName(Keys.DownArrow, "DOWNARROW", "#str_07024"),
			new KeyName(Keys.PageDown, "PGDN", "#str_07050"),
			new KeyName(Keys.Insert, "INS", "#str_07048"),
			new KeyName(Keys.Delete, "DEL", "#str_07049"),
			new KeyName(Keys.LeftWindow, "LWIN", "#str_07030"),
			new KeyName(Keys.RightWindow, "RWIN", "#str_07031"),
			new KeyName(Keys.Apps, "APPS", "#str_07032"),
			new KeyName(Keys.Power, "POWER"),
			new KeyName(Keys.Slash, "SLEEP"),

			// --

			new KeyName(Keys.Mouse1, "MOUSE1", "#str_07054"),
			new KeyName(Keys.Mouse2, "MOUSE2", "#str_07055"),
			new KeyName(Keys.Mouse3, "MOUSE3", "#str_07056"),
			new KeyName(Keys.Mouse4, "MOUSE4", "#str_07057"),
			new KeyName(Keys.Mouse5, "MOUSE5", "#str_07058"),
			new KeyName(Keys.Mouse6, "MOUSE6", "#str_07059"),
			new KeyName(Keys.Mouse7, "MOUSE7", "#str_07060"),
			new KeyName(Keys.Mouse8, "MOUSE8", "#str_07061"),

			new KeyName(Keys.MouseWheelDown, "MWHEELDOWN", "#str_07132"),
			new KeyName(Keys.MouseWheelUp, "MWHEELUP", "#str_07131"),

			new KeyName(Keys.Joystick1, "JOY1", "#str_07062"),
			new KeyName(Keys.Joystick2, "JOY2", "#str_07063"),
			new KeyName(Keys.Joystick3, "JOY3", "#str_07064"),
			new KeyName(Keys.Joystick4, "JOY4", "#str_07065"),
			new KeyName(Keys.Joystick5, "JOY5", "#str_07066"),
			new KeyName(Keys.Joystick6, "JOY6", "#str_07067"),
			new KeyName(Keys.Joystick7, "JOY7", "#str_07068"),
			new KeyName(Keys.Joystick8, "JOY8", "#str_07069"),
			new KeyName(Keys.Joystick9, "JOY9", "#str_07070"),
			new KeyName(Keys.Joystick10, "JOY10", "#str_07071"),
			new KeyName(Keys.Joystick11, "JOY11", "#str_07072"),
			new KeyName(Keys.Joystick12, "JOY12", "#str_07073"),
			new KeyName(Keys.Joystick13, "JOY13", "#str_07074"),
			new KeyName(Keys.Joystick14, "JOY14", "#str_07075"),
			new KeyName(Keys.Joystick15, "JOY15", "#str_07076"),
			new KeyName(Keys.Joystick16, "JOY16", "#str_07077"),

			new KeyName(Keys.JoystickDPadUp, "JOY_DPAD_UP"),
			new KeyName(Keys.JoystickDPadDown, "JOY_DPAD_DOWN"),
			new KeyName(Keys.JoystickDPadLeft, "JOY_DPAD_LEFT"),
			new KeyName(Keys.JoystickDPadRight, "JOY_DPAD_RIGHT"),

			new KeyName(Keys.Joystick1Up, "JOY_STICK1_UP"),
			new KeyName(Keys.Joystick1Down, "JOY_STICK1_DOWN"),
			new KeyName(Keys.Joystick1Left, "JOY_STICK1_LEFT"),
			new KeyName(Keys.Joystick1Right, "JOY_STICK1_RIGHT"),

			new KeyName(Keys.Joystick2Up, "JOY_STICK2_UP"),
			new KeyName(Keys.Joystick2Down, "JOY_STICK2_DOWN"),
			new KeyName(Keys.Joystick2Left, "JOY_STICK2_LEFT"),
			new KeyName(Keys.Joystick2Right, "JOY_STICK2_RIGHT"),

			new KeyName(Keys.JoystickTrigger1, "JOY_TRIGGER1"),
			new KeyName(Keys.JoystickTrigger2, "JOY_TRIGGER2"),

			//------------------------
			// Aliases to make it easier to bind or to support old configs
			//------------------------
			new KeyName(Keys.LeftAlt, "ALT", string.Empty),
			new KeyName(Keys.RightAlt, "RIGHTALT", string.Empty),

			new KeyName(Keys.LeftControl, "CTRL", string.Empty),
			new KeyName(Keys.LeftShift, "SHIFT", string.Empty),
			new KeyName(Keys.Apps, "MENU", string.Empty),
			new KeyName(Keys.LeftAlt, "COMMAND", string.Empty),

			new KeyName(Keys.Keypad7, "KP_HOME", string.Empty),
			new KeyName(Keys.Keypad8, "KP_UPARROW", string.Empty),
			new KeyName(Keys.Keypad9, "KP_PGUP", string.Empty),
			new KeyName(Keys.Keypad4, "KP_LEFTARROW", string.Empty),
			new KeyName(Keys.Keypad6, "KP_RIGHTARROW", string.Empty),
			new KeyName(Keys.Keypad1, "KP_END", string.Empty),
			new KeyName(Keys.Keypad2, "KP_DOWNARROW", string.Empty),
			new KeyName(Keys.Keypad3, "KP_PGDN", string.Empty),
			new KeyName(Keys.Keypad0, "KP_INS", string.Empty),
			new KeyName(Keys.KeypadDot, "KP_DEL", string.Empty),
			new KeyName(Keys.NumLock, "KP_NUMLOCK", string.Empty),

			new KeyName(Keys.Minus, "-", string.Empty),
			new KeyName(Keys.Equals, "=", string.Empty),
			new KeyName(Keys.LeftBracket, "[", string.Empty),
			new KeyName(Keys.RightBracket, "]", string.Empty),
			new KeyName(Keys.Backslash, "\\", string.Empty),
			new KeyName(Keys.Slash, "/", string.Empty),
			new KeyName(Keys.Comma, ",", string.Empty),
			new KeyName(Keys.Period, ".", string.Empty),

			new KeyName(Keys.None, null, null)
		};
		#endregion

		#region Members
		private bool _initialized;
		private KeyState[] _keys;

		private bool _mouseGrabbed;
		private bool _mouseReleased;
		private bool _mouseMovingWindow;

		private idUserCommandGenerator _userCommandGenerator;
		#endregion


		#region Binding
		public string GetBinding(Keys key)
		{
			if(key == Keys.Invalid)
			{
				return string.Empty;
			}

			return _keys[(int) key].Binding;
		}

		public void SetBinding(Keys key, string binding)
		{
			if(key == Keys.Invalid)
			{
				return;
			}

			ICVarSystem cvarSystem   = idEngine.Instance.GetService<ICVarSystem>();
			IInputSystem inputSystem = idEngine.Instance.GetService<IInputSystem>();

			// clear out all button states so we aren't stuck forever thinking this key is held down
			inputSystem.ClearGenerated();

			// allocate memory for new binding
			_keys[(int) key].Binding = binding;

			// find the action for the async command generation
			idLog.Warning("TODO: keys[keynum].usercmdAction = usercmdGen->CommandStringUsercmdData( binding );");

			// consider this like modifying an archived cvar, so the
			// file write will be triggered at the next oportunity
			cvarSystem.SetModifiedFlags(CVarFlags.Archive);
		}
		#endregion

		#region Frame
		public void ProcessFrame()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			bool shouldGrab        = true;

			if(cvarSystem.GetBool("in_mouse") == false)
			{
				shouldGrab = false;
			}

			// if fullscreen, we always want the mouse

			// TODO: fullscreen check
			//if(!win32.cdsFullscreen)
			{
				if(_mouseReleased == true)
				{
					shouldGrab = false;
				}

				if(_mouseMovingWindow == true)
				{
					shouldGrab = false;
				}

				if(idEngine.Instance.IsActive == false)
				{
					shouldGrab = false;
				}
			}

			if(shouldGrab != _mouseGrabbed)
			{
				_userCommandGenerator.Clear();

				if(_mouseGrabbed == true)
				{
					idEngine.Instance.IsMouseVisible = true;

					_mouseGrabbed = false;
				}
				else
				{
					if((cvarSystem.GetBool("in_mouse") == true) || (_mouseGrabbed == false))
					{
						idEngine.Instance.IsMouseVisible = false;
						_mouseGrabbed = true;
					}
				}
			}
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
			_initialized          = true;
			_keys                 = new KeyState[(int) Keys.LastKey];

			_userCommandGenerator = new idUserCommandGenerator();
			_userCommandGenerator.Init();

			for(int i = 0; i < _keys.Length; i++)
			{
				_keys[i] = new KeyState();
			}
		}
		#endregion
		#endregion

		#region Misc.
		public Keys GetKeyFromString(string key)
		{
			if(string.IsNullOrEmpty(key) == true)
			{
				return Keys.None;
			}

			// scan for a text match
			foreach(KeyName keyName in _keyNames)
			{
				if(key.Equals(keyName.Name, StringComparison.OrdinalIgnoreCase) == true)
				{
					return keyName.Key;
				}
			}

			return Keys.None;
		}
		#endregion

		#region State
		#region Properties
		public idUserCommand CurrentUserCommand
		{
			get
			{
				return _userCommandGenerator.CurrentUserCommand;
			}
		}
		#endregion

		#region Methods
		public void ClearGenerated()
		{
			_userCommandGenerator.Clear();
		}
		#endregion
		#endregion

		#region KeyState
		private class KeyState
		{
			public bool Down;

			/// <summary>
			/// If > 1, it is autorepeating.
			/// </summary>
			public int Repeats;

			public string Binding;

			/// <summary>
			/// For testing by the asyncronous usercmd generation.
			/// </summary>
			public int UserCommandAction;
		}
		#endregion
	}
}