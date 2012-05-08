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

using Microsoft.Xna.Framework.Input;

namespace idTech4.Input
{
	public class idInputSystem
	{
		#region Properties
		public int MouseX
		{
			get
			{
				return _mouseX;
			}
		}

		public int MouseY
		{
			get
			{
				return _mouseY;
			}
		}

		public int MouseDeltaX
		{
			get
			{
				return _mouseDeltaX;
			}
		}

		public int MouseDeltaY
		{
			get
			{
				return _mouseDeltaY;
			}
		}
		#endregion

		#region Members
		private bool _initialized;

		private int _mouseX;
		private int _mouseY;
		private int _mouseDeltaX;
		private int _mouseDeltaY;

		private Key[] _keyState = new Key[(int) Keys.MaxButtons];
		private Key[] _previousKeyState = new Key[(int) Keys.MaxButtons];
		#endregion

		#region Key map
		private KeyDefinition[] _keyDefinitions = new KeyDefinition[] {
			new KeyDefinition("TAB",							Keys.Tab,									"#str_07018"),
			new KeyDefinition("ENTER",					Keys.Enter,									"#str_07019"),
			new KeyDefinition("ESCAPE",					Keys.Escape,								"#str_07020"),
			new KeyDefinition("SPACE",					Keys.Space,								"#str_07021"),
			new KeyDefinition("BACKSPACE",			Keys.Back,									"#str_07022"),
			new KeyDefinition("UPARROW",				Keys.Up,										"#str_07023"),
			new KeyDefinition("DOWNARROW",		Keys.Down,								"#str_07024"),
			new KeyDefinition("LEFTARROW",			Keys.Left,									"#str_07025"),
			new KeyDefinition("RIGHTARROW",		Keys.Right,									"#str_07026"),

			new KeyDefinition("ALT",							Keys.LeftAlt,								"#str_07027"),
			new KeyDefinition("RIGHTALT",				Keys.RightAlt,							"#str_07027"),
			new KeyDefinition("CTRL",						Keys.LeftControl,						"#str_07028"),
			new KeyDefinition("SHIFT",						Keys.LeftShift,							"#str_07029"),

			new KeyDefinition("LWIN", 						Keys.LeftWindows, 					"#str_07030"),
			new KeyDefinition("RWIN", 						Keys.RightWindows, 				"#str_07031"),
		
			// TODO: new KeyDefinition("MENU", 					K_MENU, 					"#str_07032"),
			// TODO: new KeyDefinition("COMMAND",			K_COMMAND,			"#str_07033"),

			new KeyDefinition("CAPSLOCK",			Keys.CapsLock,							"#str_07034"),
			new KeyDefinition("SCROLL",					Keys.Scroll,								"#str_07035"),
			new KeyDefinition("PRINTSCREEN",		Keys.PrintScreen,						"#str_07179"),

			new KeyDefinition("F1", 							Keys.F1, 										"#str_07036"),
			new KeyDefinition("F2", 							Keys.F2, 										"#str_07037"),
			new KeyDefinition("F3", 							Keys.F3, 										"#str_07038"),
			new KeyDefinition("F4", 							Keys.F4, 										"#str_07039"),
			new KeyDefinition("F5", 							Keys.F5, 										"#str_07040"),
			new KeyDefinition("F6", 							Keys.F6, 										"#str_07041"),
			new KeyDefinition("F7", 							Keys.F7, 										"#str_07042"),
			new KeyDefinition("F8", 							Keys.F8, 										"#str_07043"),
			new KeyDefinition("F9", 							Keys.F9, 										"#str_07044"),
			new KeyDefinition("F10", 						Keys.F10, 									"#str_07045"),
			new KeyDefinition("F11", 						Keys.F11, 									"#str_07046"),
			new KeyDefinition("F12", 						Keys.F12, 									"#str_07047"),

			new KeyDefinition("INS", 							Keys.Insert, 								"#str_07048"),
			new KeyDefinition("DEL", 						Keys.Delete, 								"#str_07049"),
			new KeyDefinition("PGDN", 						Keys.PageDown, 						"#str_07050"),
			new KeyDefinition("PGUP", 						Keys.PageUp, 							"#str_07051"),
			new KeyDefinition("HOME", 					Keys.Home, 								"#str_07052"),
			new KeyDefinition("END",						Keys.End,									"#str_07053"),

			new KeyDefinition("MOUSE1", 				Keys.Mouse1, 							"#str_07054"),
			new KeyDefinition("MOUSE2", 				Keys.Mouse2, 							"#str_07055"),
			new KeyDefinition("MOUSE3", 				Keys.Mouse3, 							"#str_07056"),

			// TODO: MOUSE4-8
			/*new KeyDefinition("MOUSE4", 				K_MOUSE4, 			"#str_07057"),
			new KeyDefinition("MOUSE5", 				K_MOUSE5, 			"#str_07058"),
			new KeyDefinition("MOUSE6", 				K_MOUSE6, 			"#str_07059"),
			new KeyDefinition("MOUSE7", 				K_MOUSE7, 			"#str_07060"),
			new KeyDefinition("MOUSE8", 				K_MOUSE8, 			"#str_07061"),*/

			new KeyDefinition("MWHEELUP",			Keys.MouseWheelUp,				"#str_07131"),
			new KeyDefinition("MWHEELDOWN",		Keys.MouseWheelDown,			"#str_07132"),

			// TODO: K_JOY1-32
			/*new KeyDefinition("JOY1", 						K_JOY1, 			"#str_07062"),
			new KeyDefinition("JOY2", 						K_JOY2, 			"#str_07063"),
			new KeyDefinition("JOY3", 						K_JOY3, 			"#str_07064"),
			new KeyDefinition("JOY4", 						K_JOY4, 			"#str_07065"),
			new KeyDefinition("JOY5", 						K_JOY5, 			"#str_07066"),
			new KeyDefinition("JOY6", 						K_JOY6, 			"#str_07067"),
			new KeyDefinition("JOY7", 						K_JOY7, 			"#str_07068"),
			new KeyDefinition("JOY8", 						K_JOY8, 			"#str_07069"),
			new KeyDefinition("JOY9", 						K_JOY9, 			"#str_07070"),
			new KeyDefinition("JOY10", 					K_JOY10, 			"#str_07071"),
			new KeyDefinition("JOY11", 					K_JOY11, 			"#str_07072"),
			new KeyDefinition("JOY12", 					K_JOY12, 			"#str_07073"),
			new KeyDefinition("JOY13", 					K_JOY13, 			"#str_07074"),
			new KeyDefinition("JOY14", 					K_JOY14, 			"#str_07075"),
			new KeyDefinition("JOY15", 					K_JOY15, 			"#str_07076"),
			new KeyDefinition("JOY16", 					K_JOY16, 			"#str_07077"),
			new KeyDefinition("JOY17", 					K_JOY17, 			"#str_07078"),
			new KeyDefinition("JOY18", 					K_JOY18, 			"#str_07079"),
			new KeyDefinition("JOY19", 					K_JOY19, 			"#str_07080"),
			new KeyDefinition("JOY20", 					K_JOY20, 			"#str_07081"),
			new KeyDefinition("JOY21", 					K_JOY21, 			"#str_07082"),
			new KeyDefinition("JOY22", 					K_JOY22, 			"#str_07083"),
			new KeyDefinition("JOY23", 					K_JOY23, 			"#str_07084"),
			new KeyDefinition("JOY24", 					K_JOY24, 			"#str_07085"),
			new KeyDefinition("JOY25", 					K_JOY25, 			"#str_07086"),
			new KeyDefinition("JOY26", 					K_JOY26, 			"#str_07087"),
			new KeyDefinition("JOY27", 					K_JOY27, 			"#str_07088"),
			new KeyDefinition("JOY28", 					K_JOY28, 			"#str_07089"),
			new KeyDefinition("JOY29", 					K_JOY29, 			"#str_07090"),
			new KeyDefinition("JOY30", 					K_JOY30, 			"#str_07091"),
			new KeyDefinition("JOY31", 					K_JOY31, 			"#str_07092"),
			new KeyDefinition("JOY32", 					K_JOY32, 			"#str_07093"),*/

			// TODO: K_1-16
			/*new KeyDefinition("AUX1", 					K_AUX1, 			"#str_07094"),
			new KeyDefinition("AUX2", 						K_AUX2, 			"#str_07095"),
			new KeyDefinition("AUX3", 						K_AUX3, 			"#str_07096"),
			new KeyDefinition("AUX4", 						K_AUX4, 			"#str_07097"),
			new KeyDefinition("AUX5", 						K_AUX5, 			"#str_07098"),
			new KeyDefinition("AUX6", 						K_AUX6, 			"#str_07099"),
			new KeyDefinition("AUX7", 						K_AUX7, 			"#str_07100"),
			new KeyDefinition("AUX8", 						K_AUX8, 			"#str_07101"),
			new KeyDefinition("AUX9", 						K_AUX9, 			"#str_07102"),
			new KeyDefinition("AUX10", 					K_AUX10, 			"#str_07103"),
			new KeyDefinition("AUX11", 					K_AUX11, 			"#str_07104"),
			new KeyDefinition("AUX12", 					K_AUX12, 			"#str_07105"),
			new KeyDefinition("AUX13", 					K_AUX13, 			"#str_07106"),
			new KeyDefinition("AUX14", 					K_AUX14, 			"#str_07107"),
			new KeyDefinition("AUX15", 					K_AUX15, 			"#str_07108"),
			new KeyDefinition("AUX16", 					K_AUX16, 			"#str_07109"),*/

			// TODO: numpad
			/*new KeyDefinition("KP_HOME",				K_KP_HOME,			"#str_07110"),
			new KeyDefinition("KP_UPARROW",		K_KP_UPARROW,		"#str_07111"),
			new KeyDefinition("KP_PGUP",				K_KP_PGUP,			"#str_07112"),
			new KeyDefinition("KP_LEFTARROW",	K_KP_LEFTARROW, 	"#str_07113"),
			new KeyDefinition("KP_5",						K_KP_5,				"#str_07114"),
			new KeyDefinition("KP_RIGHTARROW",	K_KP_RIGHTARROW,	"#str_07115"),
			new KeyDefinition("KP_END",					K_KP_END,			"#str_07116"),
			new KeyDefinition("KP_DOWNARROW",	K_KP_DOWNARROW,		"#str_07117"),
			new KeyDefinition("KP_PGDN",				K_KP_PGDN,			"#str_07118"),
			new KeyDefinition("KP_ENTER",				K_KP_ENTER,			"#str_07119"),
			new KeyDefinition("KP_INS",					K_KP_INS, 			"#str_07120"),
			new KeyDefinition("KP_DEL",					K_KP_DEL, 			"#str_07121"),
			new KeyDefinition("KP_SLASH",				K_KP_SLASH, 		"#str_07122"),
			new KeyDefinition("KP_MINUS",				K_KP_MINUS, 		"#str_07123"),
			new KeyDefinition("KP_PLUS",					K_KP_PLUS,			"#str_07124"),
			new KeyDefinition("KP_NUMLOCK",		K_KP_NUMLOCK,		"#str_07125"),
			new KeyDefinition("KP_STAR",				K_KP_STAR,			"#str_07126"),
			new KeyDefinition("KP_EQUALS",			K_KP_EQUALS,		"#str_07127"),*/

			new KeyDefinition("PAUSE",					Keys.Pause,								"#str_07128"),
	
			// TODO: asterisk
			// new KeyDefinition("*",								Keys										""),

			new KeyDefinition(",",								Keys.OemComma,					""),
			new KeyDefinition("-",								Keys.OemMinus,						""),
			new KeyDefinition("=",								Keys.OemPlus,							""),
			new KeyDefinition(".",								Keys.OemPeriod,						""),
			new KeyDefinition("/",								Keys.OemQuestion,					""),
			new KeyDefinition("[",								Keys.OemOpenBrackets,			""),
			new KeyDefinition("\\",							Keys.OemPipe,							""),
			new KeyDefinition("]",								Keys.OemCloseBrackets,		""),
			new KeyDefinition("1",								Keys.D1,										""),
			new KeyDefinition("2",								Keys.D2,										""),
			new KeyDefinition("3",								Keys.D3,										""),
			new KeyDefinition("4",								Keys.D4,										""),
			new KeyDefinition("5",								Keys.D5,										""),
			new KeyDefinition("6",								Keys.D6,										""),
			new KeyDefinition("7",								Keys.D7,										""),
			new KeyDefinition("8",								Keys.D8,										""),
			new KeyDefinition("9",								Keys.D9,										""),
			new KeyDefinition("0",								Keys.D0,										""),
			new KeyDefinition("A",								Keys.A,										""),
			new KeyDefinition("B",								Keys.B,										""),
			new KeyDefinition("C",								Keys.C,										""),
			new KeyDefinition("D",								Keys.D,										""),
			new KeyDefinition("E",								Keys.E,										""),
			new KeyDefinition("F",								Keys.F,										""),
			new KeyDefinition("G",								Keys.G,										""),
			new KeyDefinition("H",								Keys.H,										""),
			new KeyDefinition("I",								Keys.I,											""),
			new KeyDefinition("J",								Keys.J,											""),
			new KeyDefinition("K",								Keys.K,										""),
			new KeyDefinition("L",								Keys.L,										""),
			new KeyDefinition("M",								Keys.M,										""),
			new KeyDefinition("N",								Keys.N,										""),
			new KeyDefinition("O",								Keys.O,										""),
			new KeyDefinition("P",								Keys.P,										""),
			new KeyDefinition("Q",								Keys.Q,										""),
			new KeyDefinition("R",								Keys.R,										""),
			new KeyDefinition("S",								Keys.S,										""),
			new KeyDefinition("T",								Keys.T,										""),
			new KeyDefinition("U",								Keys.U,										""),
			new KeyDefinition("V",								Keys.V,										""),
			new KeyDefinition("W",								Keys.W,										""),
			new KeyDefinition("X",								Keys.X,										""),
			new KeyDefinition("Y",								Keys.Y,										""),
			new KeyDefinition("Z",								Keys.Z,										""),

			new KeyDefinition("SEMICOLON",			Keys.OemSemicolon,				"#str_07129"),	// because a raw semicolon separates commands
			new KeyDefinition("APOSTROPHE",		Keys.OemTilde,							"#str_07130"),	// because a raw apostrophe messes with parsing
		};
		#endregion

		#region Constructor
		public idInputSystem()
		{
			InitCommands();
		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			_initialized = true;
		}

		public UserCommandButton GetCommandButton(Keys key)
		{
			return _keyState[(int) key].Button;
		}

		public Keys GetKeyFromString(string identifier)
		{
			identifier = identifier.ToUpper();

			foreach(KeyDefinition keyDefinition in _keyDefinitions)
			{
				if(identifier == keyDefinition.Name)
				{
					return keyDefinition.Key;
				}
			}

			return Keys.None;
		}

		public bool IsKeyDown(Keys key)
		{
			return _keyState[(int) key].Down;
		}

		public void SetBinding(Keys key, string binding)
		{
			if((key == Keys.None) || (key == Keys.MaxButtons))
			{
				return;
			}

			// Clear out all button states so we aren't stuck forever thinking this key is held down
			idE.UserCommandGenerator.Clear();

			int keyCode = (int) key;

			_keyState[keyCode].Binding = binding;

			// find the action for the async command generation
			_keyState[keyCode].Button = idE.UserCommandGenerator.GetButtonFromBinding(binding);

			// consider this like modifying an archived cvar, so the
			// file write will be triggered at the next oportunity
			idE.CvarSystem.ModifiedFlags = CvarFlags.Archive;
		}

		public void Update()
		{
			if(idE.System.IsActive == true)
			{
				ProcessKeyboard();
				ProcessMouse();
			}
		}
		#endregion

		#region Private
		private void InitCommands()
		{
			idE.CmdSystem.AddCommand("bind", "binds a command to a key", CommandFlags.System, Cmd_Bind /* TODO: idKeyInput::ArgCompletion_KeyName */ );
			idE.CmdSystem.AddCommand("unbindall", "unbinds any commands from all keys", CommandFlags.System, Cmd_UnbindAll);

			// TODO: commands
			/*cmdSystem->AddCommand( "bindunbindtwo", Key_BindUnBindTwo_f, CMD_FL_SYSTEM, "binds a key but unbinds it first if there are more than two binds" );
			cmdSystem->AddCommand( "unbind", Key_Unbind_f, CMD_FL_SYSTEM, "unbinds any command from a key", idKeyInput::ArgCompletion_KeyName );			
			cmdSystem->AddCommand( "listBinds", Key_ListBinds_f, CMD_FL_SYSTEM, "lists key bindings" );*/
		}

		private void ProcessKeyboard()
		{
			KeyboardState keyState = Keyboard.GetState();
			MouseState mouseState = Mouse.GetState();

			int mouse1 = (int) Keys.Mouse1;
			int mouse2 = (int) Keys.Mouse2;
			int mouse3 = (int) Keys.Mouse3;

			_previousKeyState = (Key[]) _keyState.Clone();

			foreach(Keys key in Enum.GetValues(typeof(Keys)))
			{
				if(key != Keys.MaxButtons)
				{
					_keyState[(int) key].Down = false;
				}
			}

			foreach(Keys key in keyState.GetPressedKeys())
			{
				_keyState[(int) key].Down = true;
			}

			_keyState[mouse1].Down = (mouseState.LeftButton == ButtonState.Pressed);
			_keyState[mouse2].Down = (mouseState.MiddleButton == ButtonState.Pressed);
			_keyState[mouse3].Down = (mouseState.RightButton == ButtonState.Pressed);

			foreach(Keys key in Enum.GetValues(typeof(Keys)))
			{
				if(key != Keys.MaxButtons)
				{
					int keyCode = (int) key;

					if(_previousKeyState[keyCode].Down != _keyState[keyCode].Down)
					{
						idE.EventLoop.Queue(SystemEventType.Key, keyCode, (_keyState[keyCode].Down == true) ? 1 : 0);
					}
				}
			}
		}

		private void ProcessMouse()
		{			
			MouseState state = Mouse.GetState();

			int screenHalfWidth = idE.RenderSystem.ScreenWidth / 2;
			int screenHalfHeight = idE.RenderSystem.ScreenHeight / 2;
			
			_mouseDeltaX = (state.X - screenHalfWidth);
			_mouseDeltaY = (state.Y - screenHalfHeight);

			_mouseX += _mouseDeltaX;
			_mouseY += _mouseDeltaY;

			if((_mouseDeltaX != 0) || (_mouseDeltaY != 0))
			{
				idE.EventLoop.Queue(SystemEventType.Mouse, _mouseDeltaX, _mouseDeltaY);
			}
			
			Mouse.SetPosition(screenHalfWidth, screenHalfHeight);
		}
		#endregion

		#region Command handlers
		private void Cmd_Bind(object sender, CommandEventArgs e)
		{
			if(e.Args.Length < 2)
			{
				idConsole.WriteLine("bind <key> [command]: attach a command to a key");
			}
			else
			{
				Keys key = GetKeyFromString(e.Args.Get(1));

				if(key == Keys.None)
				{
					idConsole.WriteLine("\"{0}\" isn't a valid key", e.Args.Get(1));
				}
				else
				{
					if(e.Args.Length == 2)
					{
						if(_keyState[(int) key].Binding != string.Empty)
						{
							idConsole.WriteLine("\"{0}\" = \"{1}\"", e.Args.Get(1), _keyState[(int) key].Binding);
						}
						else
						{
							idConsole.WriteLine("\"{0}\" is not bound", e.Args.Get(1));
						}
					}
					else
					{
						// copy the rest of the command line
						SetBinding(key, e.Args.Get(2, e.Args.Length - 1));
					}
				}
			}
		}

		private void Cmd_UnbindAll(object sender, CommandEventArgs e)
		{
			foreach(Keys key in Enum.GetValues(typeof(Keys)))
			{
				SetBinding(key, "");
			}
		}
		#endregion
		#endregion

		#region Private structures
		private struct Key
		{
			public bool Down;
			public int Repeats; // if > 1, it is autorepeating
			public string Binding;
			public UserCommandButton Button; // for testing by the asyncronous usercmd generation
		}

		private struct KeyDefinition
		{
			public string Name;
			public Keys Key;
			public string Description;

			public KeyDefinition(string name, Keys key, string description)
			{
				this.Name = name;
				this.Key = key;
				this.Description = description;
			}
		}
		#endregion
	}

	public enum Keys
	{
		None = 0,
		Back = 8,
		Tab = 9,
		Enter = 13,
		Pause = 19,
		CapsLock = 20,
		Kana = 21,
		Kanji = 25,
		Escape = 27,
		ImeConvert = 28,
		ImeNoConvert = 29,
		Space = 32,
		PageUp = 33,
		PageDown = 34,
		End = 35,
		Home = 36,
		Left = 37,
		Up = 38,
		Right = 39,
		Down = 40,
		Select = 41,
		Print = 42,
		Execute = 43,
		PrintScreen = 44,
		Insert = 45,
		Delete = 46,
		Help = 47,
		D0 = 48,
		D1 = 49,
		D2 = 50,
		D3 = 51,
		D4 = 52,
		D5 = 53,
		D6 = 54,
		D7 = 55,
		D8 = 56,
		D9 = 57,
		A = 65,
		B = 66,
		C = 67,
		D = 68,
		E = 69,
		F = 70,
		G = 71,
		H = 72,
		I = 73,
		J = 74,
		K = 75,
		L = 76,
		M = 77,
		N = 78,
		O = 79,
		P = 80,
		Q = 81,
		R = 82,
		S = 83,
		T = 84,
		U = 85,
		V = 86,
		W = 87,
		X = 88,
		Y = 89,
		Z = 90,
		LeftWindows = 91,
		RightWindows = 92,
		Apps = 93,
		Sleep = 95,
		NumPad0 = 96,
		NumPad1 = 97,
		NumPad2 = 98,
		NumPad3 = 99,
		NumPad4 = 100,
		NumPad5 = 101,
		NumPad6 = 102,
		NumPad7 = 103,
		NumPad8 = 104,
		NumPad9 = 105,
		Multiply = 106,
		Add = 107,
		Separator = 108,
		Subtract = 109,
		Decimal = 110,
		Divide = 111,
		F1 = 112,
		F2 = 113,
		F3 = 114,
		F4 = 115,
		F5 = 116,
		F6 = 117,
		F7 = 118,
		F8 = 119,
		F9 = 120,
		F10 = 121,
		F11 = 122,
		F12 = 123,
		F13 = 124,
		F14 = 125,
		F15 = 126,
		F16 = 127,
		F17 = 128,
		F18 = 129,
		F19 = 130,
		F20 = 131,
		F21 = 132,
		F22 = 133,
		F23 = 134,
		F24 = 135,
		NumLock = 144,
		Scroll = 145,
		LeftShift = 160,
		RightShift = 161,
		LeftControl = 162,
		RightControl = 163,
		LeftAlt = 164,
		RightAlt = 165,
		BrowserBack = 166,
		BrowserForward = 167,
		BrowserRefresh = 168,
		BrowserStop = 169,
		BrowserSearch = 170,
		BrowserFavorites = 171,
		BrowserHome = 172,
		VolumeMute = 173,
		VolumeDown = 174,
		VolumeUp = 175,
		MediaNextTrack = 176,
		MediaPreviousTrack = 177,
		MediaStop = 178,
		MediaPlayPause = 179,
		LaunchMail = 180,
		SelectMedia = 181,
		LaunchApplication1 = 182,
		LaunchApplication2 = 183,
		OemSemicolon = 186,
		OemPlus = 187,
		OemComma = 188,
		OemMinus = 189,
		OemPeriod = 190,
		OemQuestion = 191,
		OemTilde = 192,
		ChatPadGreen = 202,
		ChatPadOrange = 203,
		OemOpenBrackets = 219,
		OemPipe = 220,
		OemCloseBrackets = 221,
		OemQuotes = 222,
		Oem8 = 223,
		OemBackslash = 226,
		ProcessKey = 229,
		OemCopy = 242,
		OemAuto = 243,
		OemEnlW = 244,
		Attn = 246,
		Crsel = 247,
		Exsel = 248,
		EraseEof = 249,
		Play = 250,
		Zoom = 251,
		Pa1 = 253,
		OemClear = 254,

		Mouse1 = 300,
		Mouse2 = 301,
		Mouse3 = 302,

		MouseWheelUp = 303,
		MouseWheelDown = 304,

		MaxButtons
	}
}