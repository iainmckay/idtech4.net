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
using System.Linq;
using System.Text;

namespace idTech4.Services
{
	public interface IInputSystem
	{
		#region Binding
		string GetBinding(Keys key);
		void SetBinding(Keys key, string binding);
		#endregion

		#region Frame
		void ProcessFrame();
		void BuildCurrentUserCommand(int deviceNum);
		#endregion

		#region Initialization
		#region Properties
		bool IsInitialized { get; }
		#endregion

		#region Methods
		void Initialize(idEventLoop eventLoop);
		#endregion
		#endregion

		#region Misc.
		UserCommandButton GetUserCommandButtonFromKey(Keys key);
		Keys GetKeyFromString(string key);
		string GetStringFromKey(Keys key);
		#endregion

		#region State
		#region Properties
		bool GrabMouse { get; }
		idUserCommand CurrentUserCommand { get; }
		#endregion

		#region Methods
		void ClearGenerated();
		#endregion
		#endregion
	}

	public enum Keys
	{
		Invalid      = -1,
		None         = 0,

		Escape       = 27,
		Minus        = 189,
		Equals       = 187,
		Backspace    = 8,
		Tab          = 9,
		D1           = 49,
		D2           = 50,
		D3           = 51,
		D4           = 52,
		D5           = 53,
		D6           = 54,
		D7           = 55,
		D8           = 56,
		D9           = 57,
		D0           = 48,
		A            = 65,
		B            = 66,
		C            = 67,
		D            = 68,
		E            = 69,
		F            = 70,
		G            = 71,
		H            = 72,
		I            = 73,
		J            = 74,
		K            = 75,
		L            = 76,
		M            = 77,
		N            = 78,
		O            = 79,
		P            = 80,
		Q            = 81,
		R            = 82,
		S            = 83,
		T            = 84,
		U            = 85,
		V            = 86,
		W            = 87,
		X            = 88,
		Y            = 89,
		Z            = 90,
		LeftBracket  = 219,
		RightBracket = 221,
		LeftControl  = 162,
		RightControl = 163,
		LeftShift    = 160,
		RightShift   = 161,
		LeftAlt      = 164,
		RightAlt     = 165,
		Enter        = 13,		
		SemiColon    = 186,
		Apostrophe   = 192,		
		Backslash    = 226,
		Comma        = 188,
		Period       = 190,
		Slash        = 191,						
		Space        = 32,
		CapsLock     = 20,
		NumLock      = 144,
		F1           = 112,
		F2           = 113,
		F3           = 114,
		F4           = 115,
		F5           = 116,
		F6           = 117,
		F7           = 118,
		F8           = 119,
		F9           = 120,
		F10          = 121,
		F11          = 122,
		F12          = 123,
		F13          = 124,
		F14          = 125,
		F15          = 126,
		F16          = 127,
		F17          = 128,
		F18          = 129,
		F19          = 130,
		F20          = 131,
		F21          = 132,
		F22          = 133,
		F23          = 134,
		F24          = 135,
		
		Scroll       = 145,
		NumPad0      = 96,
		NumPad1      = 97,
		NumPad2      = 98,
		NumPad3      = 99,
		NumPad4      = 100,
		NumPad5      = 101,
		NumPad6      = 102,
		NumPad7      = 103,
		NumPad8      = 104,
		NumPad9      = 105,
		KeypadMinus, // TODO:
		KeypadPlus, // TODO:
		KeypadDot, // TODO:
		KeypadStar, // TODO:
		KeypadEquals, // TODO:
		KeypadEnter, // TODO:
		KeypadComma, // TODO:
		KeypadSlash, // TODO:
		Kana         = 21,
		Convert      = 28,
		NoConvert    = 29,
		Colon, // TODO:
		Underline, // TODO:
		Kanji        = 25,
		Stop         = 178,		
		PrintScreen  = 44,		
		Pause        = 19,
		Home         = 36,
		End          = 35,
		UpArrow      = 38,
		DownArrow    = 40,
		LeftArrow    = 37,
		RightArrow   = 39,		
		PageUp       = 33,
		PageDown     = 34,
		Insert       = 45,
		Delete       = 46,
		LeftWindow   = 91,
		RightWindow  = 92,
		Apps         = 93,
		Power, // TODO:
		Sleep        = 95,

		//------------------------
		// K_JOY codes must be contiguous, too
		//------------------------

		Joystick1 = 256,
		Joystick2,
		Joystick3,
		Joystick4,
		Joystick5,
		Joystick6,
		Joystick7,
		Joystick8,
		Joystick9,
		Joystick10,
		Joystick11,
		Joystick12,
		Joystick13,
		Joystick14,
		Joystick15,
		Joystick16,

		Joystick1Up,
		Joystick1Down,
		Joystick1Left,
		Joystick1Right,

		Joystick2Up,
		Joystick2Down,
		Joystick2Left,
		Joystick2Right,

		JoystickTrigger1,
		JoystickTrigger2,

		JoystickDPadUp,
		JoystickDPadDown,
		JoystickDPadLeft,
		JoystickDPadRight,

		//------------------------
		// K_MOUSE enums must be contiguous (no char codes in the middle)
		//------------------------

		Mouse1,
		Mouse2,
		Mouse3,
		Mouse4,
		Mouse5,
		Mouse6,
		Mouse7,
		Mouse8,

		MouseWheelDown,
		MouseWheelUp,

		LastKey
	}

	public enum UserCommandButton
	{
		None,

		MoveUp,
		MoveDown,
		LookLeft,
		LookRight,
		MoveForward,
		MoveBack,
		LookUp,
		LookDown,
		MoveLeft,
		MoveRight,

		Attack,
		Speed,
		Zoom,
		ShowScores,
		Use,

		Impulse0,
		Impulse1,
		Impulse2,
		Impulse3,
		Impulse4,
		Impulse5,
		Impulse6,
		Impulse7,
		Impulse8,
		Impulse9,
		Impulse10,
		Impulse11,
		Impulse12,
		Impulse13,
		Impulse14,
		Impulse15,
		Impulse16,
		Impulse17,
		Impulse18,
		Impulse19,
		Impulse20,
		Impulse21,
		Impulse22,
		Impulse23,
		Impulse24,
		Impulse25,
		Impulse26,
		Impulse27,
		Impulse28,
		Impulse29,
		Impulse30,
		Impulse31,

		MaxButtons
	}

	public sealed class idUserCommand
	{
		/// <summary>Buttons</summary>
		public Button Buttons;

		/// <summary>Forward/backward movement.</summary>
		public bool ForwardMove;

		/// <summary>Left/right movement.</summary>
		public bool RightMove;

		/// <summary>Number of times we've fired.</summary>
		public ushort FireCount;

		/// <summary>View angles.</summary>
		public short[] Angles = new short[3];

		/// <summary>Mouse delta X.</summary>
		public short MouseX;

		/// <summary>Mouse delta Y</summary>
		public short MouseY;

		/// <summary>Impulse command.</summary>
		public Impulse Impulse;

		/// <summary>Incremented every time there's a new impulse.</summary>
		public byte ImpulseSequence;

		public float SpeedSquared;

		public static bool operator ==(idUserCommand c1, idUserCommand c2)
		{
			return ((c1.Buttons == c2.Buttons)
				&& (c1.ForwardMove == c2.ForwardMove)
				&& (c1.RightMove == c2.RightMove)
				&& (c1.Angles[0] == c2.Angles[0])
				&& (c1.Angles[1] == c2.Angles[1])
				&& (c1.Angles[2] == c2.Angles[2])
				&& (c1.Impulse == c2.Impulse)
				&& (c1.ImpulseSequence == c2.ImpulseSequence)
				&& (c1.MouseX == c2.MouseX)
				&& (c1.MouseY == c2.MouseY)
				&& (c1.FireCount == c2.FireCount)
				&& (c1.SpeedSquared == c2.SpeedSquared));
		}

		public static bool operator !=(idUserCommand c1, idUserCommand c2)
		{
			return !(c1 == c2);
		}
	}

	[Flags]
	public enum Button : byte
	{
		Attack   = 1 << 0,
		Run      = 1 << 1,
		Zoom     = 1 << 2,
		Scores   = 1 << 3,
		Use      = 1 << 4,
		Jump     = 1 << 5,
		Crouch   = 1 << 6,
		Chatting = 1 << 7,
	}

	public enum Impulse : byte
	{
		Impulse0,
		Impulse1,
		Impulse2,
		Impulse3,
		Impulse4,
		Impulse5,
		Impulse6,
		Impulse7,
		Impulse8,
		Impulse9,
		Impulse10,
		Impulse11,
		Impulse12,
		Impulse13,
		Impulse14,
		Impulse15,
		Impulse16,
		Impulse17,
		Impulse18,
		Impulse19,
		Impulse20,
		Impulse21,
		Impulse22,
		Impulse23,
		Impulse24,
		Impulse25,
		Impulse26,
		Impulse27,
		Impulse28,
		Impulse29,
		Impulse30,
		Impulse31
	}
}