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
		#endregion

		#region Initialization
		#region Properties
		bool IsInitialized { get; }
		#endregion

		#region Methods
		void Initialize();
		#endregion
		#endregion

		#region Misc.
		Keys GetKeyFromString(string key);
		#endregion

		#region State
		#region Properties
		idUserCommand CurrentUserCommand { get; }
		#endregion

		#region Methods
		void ClearGenerated();
		#endregion
		#endregion
	}

	public enum Keys
	{
		Invalid = -1,
		None,

		Escape,
		D1,
		D2,
		D3,
		D4,
		D5,
		D6,
		D7, 
		D8,
		D9,
		D0,
		Minus,
		Equals,
		Backspace,
		Tab,
		Q,
		W,
		E,
		R,
		T,
		Y,
		U,
		I,
		O,
		P,
		LeftBracket,
		RightBracket,
		Enter,
		LeftControl,
		A,
		S,
		D,
		F,
		G,
		H,
		J,
		K,
		L,
		SemiColon,
		Apostrophe,
		Grave,
		LeftShift,
		Backslash,
		Z,
		X,
		C,
		V,
		B,
		N,
		M,
		Comma,
		Period,
		Slash,
		RightShift,
		KeypadStar,
		LeftAlt,
		Space,
		CapsLock,
		F1,
		F2,
		F3,
		F4,
		F5,
		F6,
		F7,
		F8,
		F9,
		F10,
		NumLock,
		Scroll,
		Keypad7,
		Keypad8,
		Keypad9,
		KeypadMinus,
		Keypad4,
		Keypad5,
		Keypad6,
		KeypadPlus,
		Keypad1,
		Keypad2,
		Keypad3,
		Keypad0,
		KeypadDot,
		F11          = 0x57,
		F12          = 0x58,
		F13          = 0x64,
		F14          = 0x65,
		F15          = 0x66,
		Kana         = 0x70,
		Convert      = 0x79,
		NoConvert    = 0x7B,
		Yen          = 0x7D,
		KeypadEquals = 0x8D,
		Circumflex   = 0x90,
		AT           = 0x91,
		Colon        = 0x92,
		Underline    = 0x93,
		Kanji        = 0x94,
		Stop         = 0x95,
		AX           = 0x96,
		Unlabeled    = 0x97,
		KeypadEnter  = 0x9C,
		RightControl = 0x9D,
		KeypadComma  = 0xB3,
		KeypadSlash  = 0xB5,
		PrintScreen  = 0xB7,
		RightAlt     = 0xB8,
		Pause        = 0xC5,
		Home         = 0xC7,
		UpArrow      = 0xC8,
		PageUp       = 0xC9,
		LeftArrow    = 0xCB,
		RightArrow   = 0xCD,
		End          = 0xCF,
		DownArrow    = 0xD0,
		PageDown     = 0xD1,
		Insert       = 0xD2,
		Delete       = 0xD3,
		LeftWindow   = 0xDB,
		RightWindow  = 0xDC,
		Apps         = 0xDD,
		Power        = 0xDE,
		Sleep        = 0xDF,

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