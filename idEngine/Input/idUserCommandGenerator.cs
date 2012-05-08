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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace idTech4.Input
{
	public sealed class idUserCommandGenerator
	{
		#region Properties
		public bool Inhibited
		{
			get
			{
				return (_inhibitCommands != 0);
			}
		}
		#endregion

		#region Members
		private bool _initialized;
		private int _inhibitCommands;
		private int _lastCommandTime;

		private int _mouseDeltaX;
		private int _mouseDeltaY;

		private int _mouseX;
		private int _mouseY;

		private Impulse _impulse;
		private UserCommandFlags _flags;
		private Vector3 _viewAngles;

		private bool[] _keyState;
		private int[] _commandButtonState;

		private idUserCommand _currentCommand;
		#endregion

		#region Command map
		private Dictionary<string, UserCommandButton> _commandMap = new Dictionary<string, UserCommandButton>(StringComparer.OrdinalIgnoreCase) {
			{ "_moveUp",			UserCommandButton.Up },
			{ "_moveDown",	UserCommandButton.Down },
			{ "_left",					UserCommandButton.Left },
			{ "_right",				UserCommandButton.Right },
			{ "_forward",			UserCommandButton.Forward },
			{ "_back",				UserCommandButton.Back },
			{ "_lookUp",			UserCommandButton.LookUp },
			{ "_lookDown",		UserCommandButton.LookDown },
			{ "_strafe",				UserCommandButton.Strafe },
			{ "_moveLeft",		UserCommandButton.MoveLeft },
			{ "_moveRight",		UserCommandButton.MoveRight },

			{ "_attack",				UserCommandButton.Attack },
			{ "_speed",				UserCommandButton.Speed },
			{ "_zoom",				UserCommandButton.Zoom },
			{ "_showScores",	UserCommandButton.ShowScores },
			{ "_mlook",				UserCommandButton.MouseLook },

			{ "_button0",			UserCommandButton.Button0 },
			{ "_button1",			UserCommandButton.Button1 },
			{ "_button2",			UserCommandButton.Button2 },
			{ "_button3",			UserCommandButton.Button3 },
			{ "_button4",			UserCommandButton.Button4 },
			{ "_button5",			UserCommandButton.Button5 },
			{ "_button6",			UserCommandButton.Button6 },
			{ "_button7",			UserCommandButton.Button7 },

			{ "_impulse0",		UserCommandButton.Impulse0 },
			{ "_impulse1",		UserCommandButton.Impulse1 },
			{ "_impulse2",		UserCommandButton.Impulse2 },
			{ "_impulse3",		UserCommandButton.Impulse3 },
			{ "_impulse4",		UserCommandButton.Impulse4 },
			{ "_impulse5",		UserCommandButton.Impulse5 },
			{ "_impulse6",		UserCommandButton.Impulse6 },
			{ "_impulse7",		UserCommandButton.Impulse7 },
			{ "_impulse8",		UserCommandButton.Impulse8 },
			{ "_impulse9",		UserCommandButton.Impulse9 },
			{ "_impulse10",		UserCommandButton.Impulse10 },
			{ "_impulse11",		UserCommandButton.Impulse11 },
			{ "_impulse12",		UserCommandButton.Impulse12 },
			{ "_impulse13",		UserCommandButton.Impulse13 },
			{ "_impulse14",		UserCommandButton.Impulse14 },
			{ "_impulse15",		UserCommandButton.Impulse15 },
			{ "_impulse16",		UserCommandButton.Impulse16 },
			{ "_impulse17",		UserCommandButton.Impulse17 },
			{ "_impulse18",		UserCommandButton.Impulse18 },
			{ "_impulse19",		UserCommandButton.Impulse19 },
			{ "_impulse20",		UserCommandButton.Impulse20 },
			{ "_impulse21",		UserCommandButton.Impulse21 },
			{ "_impulse22",		UserCommandButton.Impulse22 },
			{ "_impulse23",		UserCommandButton.Impulse23 },
			{ "_impulse24",		UserCommandButton.Impulse24 },
			{ "_impulse25",		UserCommandButton.Impulse25 },
			{ "_impulse26",		UserCommandButton.Impulse26 },
			{ "_impulse27",		UserCommandButton.Impulse27 },
			{ "_impulse28",		UserCommandButton.Impulse28 },
			{ "_impulse29",		UserCommandButton.Impulse29 },
			{ "_impulse30",		UserCommandButton.Impulse30 },
			{ "_impulse31",		UserCommandButton.Impulse31 },
			{ "_impulse32",		UserCommandButton.Impulse32 },
			{ "_impulse33",		UserCommandButton.Impulse33 },
			{ "_impulse34",		UserCommandButton.Impulse34 },
			{ "_impulse35",		UserCommandButton.Impulse35 },
			{ "_impulse36",		UserCommandButton.Impulse36 },
			{ "_impulse37",		UserCommandButton.Impulse37 },
			{ "_impulse38",		UserCommandButton.Impulse38 },
			{ "_impulse39",		UserCommandButton.Impulse39 },
			{ "_impulse40",		UserCommandButton.Impulse40 },
			{ "_impulse41",		UserCommandButton.Impulse41 },
			{ "_impulse42",		UserCommandButton.Impulse42 },
			{ "_impulse43",		UserCommandButton.Impulse43 },
			{ "_impulse44",		UserCommandButton.Impulse44 },
			{ "_impulse45",		UserCommandButton.Impulse45 },
			{ "_impulse46",		UserCommandButton.Impulse46 },
			{ "_impulse47",		UserCommandButton.Impulse47 },
			{ "_impulse48",		UserCommandButton.Impulse48 },
			{ "_impulse49",		UserCommandButton.Impulse49 },
			{ "_impulse50",		UserCommandButton.Impulse50 },
			{ "_impulse51",		UserCommandButton.Impulse51 },
			{ "_impulse52",		UserCommandButton.Impulse52 },
			{ "_impulse53",		UserCommandButton.Impulse53 },
			{ "_impulse54",		UserCommandButton.Impulse54 },
			{ "_impulse55",		UserCommandButton.Impulse55 },
			{ "_impulse56",		UserCommandButton.Impulse56 },
			{ "_impulse57",		UserCommandButton.Impulse57 },
			{ "_impulse58",		UserCommandButton.Impulse58 },
			{ "_impulse59",		UserCommandButton.Impulse59 },
			{ "_impulse60",		UserCommandButton.Impulse60 },
			{ "_impulse61",		UserCommandButton.Impulse61 },
			{ "_impulse62",		UserCommandButton.Impulse62 },
			{ "_impulse63",		UserCommandButton.Impulse63 },
		};
		#endregion

		#region Constructor
		public idUserCommandGenerator()
		{
			InitCvars();

			/*toggled_crouch.Clear();
			toggled_run.Clear();
			toggled_zoom.Clear();
			toggled_run.on = in_alwaysRun.GetBool();

			ClearAngles();*/
			Clear();
		}
		#endregion

		#region Methods
		#region Public
		public void Clear()
		{
			// clears all key states 
			_keyState = new bool[(int) Keys.MaxButtons];
			_commandButtonState = new int[(int) UserCommandButton.MaxButtons];

			_inhibitCommands = 0;
			_mouseDeltaX = _mouseDeltaY = 0;
		}

		/// <summary>
		/// Returns the button if the command string is used by the async usercmd generator.
		/// </summary>
		/// <param name="binding"></param>
		/// <returns></returns>
		public UserCommandButton GetButtonFromBinding(string binding)
		{
			if(_commandMap.ContainsKey(binding) == true)
			{
				return _commandMap[binding];
			}

			return UserCommandButton.None;
		}

		public idUserCommand GetDirectCommand()
		{
			idUserCommand cmd = ProcessInput();
			cmd.DuplicateCount = 0;

			return cmd;
		}

		public void Init()
		{
			_initialized = true;
		}
		#endregion

		#region Private
		private void InitCvars()
		{
			new idCvar("in_yawspeed", "140", "yaw change speed when holding down _left or _right button", CvarFlags.System | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("in_pitchspeed", "140", "pitch change speed when holding down look _lookUp or _lookDown button", CvarFlags.System | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("in_anglespeedkey", "1.5", "angle change scale when holding down _speed button", CvarFlags.System | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("in_freeLook", "1", "look around with mouse (reverse _mlook button)", CvarFlags.System | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("in_alwaysRun", "0", "always run (reverse _speed button) - only in MP", CvarFlags.System | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("in_toggleRun", "0", "pressing _speed button toggles run on/off - only in MP", CvarFlags.System | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("in_toggleCrouch", "0", "pressing _movedown button toggles player crouching/standing", CvarFlags.System | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("in_toggleZoom", "0", "pressing _zoom button toggles zoom on/off", CvarFlags.System | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("sensitivity", "5", "mouse view sensitivity", CvarFlags.System | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("m_pitch", "0.022", "mouse pitch scale", CvarFlags.System | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("m_yaw", "0.022", "mouse yaw scale", CvarFlags.System | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("m_strafeScale", "6.25", "mouse strafe movement scale", CvarFlags.System | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("m_smooth", "1", 1, 8, "number of samples blended for mouse viewing", new ArgCompletion_Integer(1, 8), CvarFlags.System | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("m_strafeSmooth", "4", 1, 8, "number of samples blended for mouse moving", new ArgCompletion_Integer(1, 8), CvarFlags.System | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("m_showMouseRate", "0", "shows mouse movement", CvarFlags.System | CvarFlags.Bool);
		}

		private void MakeCurrent()
		{
			Vector3 oldAngles = _viewAngles;

			if(this.Inhibited == false)
			{
				// update toggled key states
				// TODO: toggled
				/*toggled_crouch.SetKeyState( ButtonState( UB_DOWN ), in_toggleCrouch.GetBool() );
				toggled_run.SetKeyState( ButtonState( UB_SPEED ), in_toggleRun.GetBool() && idAsyncNetwork::IsActive() );
				toggled_zoom.SetKeyState( ButtonState( UB_ZOOM ), in_toggleZoom.GetBool() );*/

				// TODO: keyboard gen
				// keyboard angle adjustment
				/*AdjustAngles();

				// set button bits
				CmdButtons();

				// get basic movement from keyboard
				KeyMove();*/

				// get basic movement from mouse
				// TODO: MouseMove();

				// get basic movement from joystick
				/*JoystickMove();

				// check to make sure the angles haven't wrapped
				if ( viewangles[PITCH] - oldAngles[PITCH] > 90 ) {
					viewangles[PITCH] = oldAngles[PITCH] + 90;
				} else if ( oldAngles[PITCH] - viewangles[PITCH] > 90 ) {
					viewangles[PITCH] = oldAngles[PITCH] - 90;
				} */
			}
			else
			{
				_mouseDeltaX = 0;
				_mouseDeltaY = 0;
			}

			// TODO: input
			/*for ( i = 0; i < 3; i++ ) {
				cmd.angles[i] = ANGLE2SHORT( viewangles[i] );
			}*/

			_currentCommand.MouseX = (short) _mouseX;
			_currentCommand.MouseY = (short) _mouseY;

			_flags = _currentCommand.Flags;
			_impulse = _currentCommand.Impulse;
		}

		private idUserCommand ProcessInput()
		{
			// initialize current usercmd
			_currentCommand = new idUserCommand();
			_currentCommand.Flags = _flags;
			_currentCommand.Impulse = _impulse;
			_currentCommand.Buttons |= ((idE.CvarSystem.GetBool("in_alwaysRun") == true) && (idE.AsyncNetwork.IsActive == true)) ? Button.Run : 0;
			_currentCommand.Buttons |= (idE.CvarSystem.GetBool("in_freeLook") == true) ? Button.MouseLook : 0;

			ProcessMouse();

			// process the system keyboard events
			// TODO: Keyboard();

			// process the system joystick events
			// TODO: Joystick();

			// create the usercmd
			MakeCurrent();

			return _currentCommand;
		}

		private void ProcessKey(Keys key, bool down)
		{
			int keyCode = (int) key;

			if(_keyState[keyCode] == down)
			{
				return;
			}

			_keyState[keyCode] = down;

			UserCommandButton commandButton = idE.Input.GetCommandButton(key);

			if(down == true)
			{
				_commandButtonState[(int) commandButton]++;

				if(this.Inhibited == false)
				{
					if((commandButton >= UserCommandButton.Impulse0) && (commandButton <= UserCommandButton.Impulse61))
					{
						_currentCommand.Impulse = (Impulse) (commandButton - UserCommandButton.Impulse0);
						_currentCommand.Flags ^= UserCommandFlags.ImpulseSequence;
					}
				}
				else
				{
					_commandButtonState[(int) commandButton]--;

					// we might have one held down across an app active transition
					if(_commandButtonState[(int) commandButton] < 0)
					{
						_commandButtonState[(int) commandButton] = 0;
					}
				}
			}
		}

		private void ProcessMouse()
		{
			MouseState state = Mouse.GetState();

			ProcessKey(Keys.Mouse1, idE.Input.IsKeyDown(Keys.Mouse1));
			ProcessKey(Keys.Mouse2, idE.Input.IsKeyDown(Keys.Mouse2));
			ProcessKey(Keys.Mouse3, idE.Input.IsKeyDown(Keys.Mouse3));

			_mouseDeltaX = idE.Input.MouseDeltaX;
			_mouseDeltaY = idE.Input.MouseDeltaY;

			_mouseX = idE.Input.MouseX;
			_mouseY = idE.Input.MouseY;

			// TODO: mouse wheel
			/*
					int key = value < 0 ? K_MWHEELDOWN : K_MWHEELUP;
					value = abs(value);
					while(value-- > 0)
					{
						Key(key, true);
						Key(key, false);
						mouseButton = key;
						mouseDown = true;
					}
					break;
			}*/

		}
		#endregion
		#endregion
	}

	public sealed class idUserCommand
	{
		/// <summary>Frame number.</summary>
		public int GameFrame;

		/// <summary>Game time.</summary>
		public int	GameTime;

		/// <summary>Duplication count for networking</summary>
		public int	DuplicateCount;

		/// <summary>Buttons</summary>
		public Button Buttons;

		/// <summary>Forward/backward movement.</summary>
		public bool ForwardMove;

		/// <summary>Left/right movement.</summary>
		public bool RightMove;

		/// <summary>Up/down movement.</summary>
		public bool UpMove;

		/// <summary>View angles.</summary>
		public short[]	Angles = new short[3];

		/// <summary>Mouse delta X.</summary>
		public short MouseX;

		/// <summary>Mouse delta Y</summary>
		public short MouseY;

		/// <summary>Impulse command.</summary>
		public Impulse Impulse;

		/// <summary>Additional flags.</summary>
		public UserCommandFlags Flags;

		/// <summary>Just for debugging.</summary>
		public int	Sequence;

		public static bool operator ==(idUserCommand c1, idUserCommand c2)
		{
			return ((c1.Buttons == c2.Buttons)
				&& (c1.ForwardMove == c2.ForwardMove)
				&& (c1.RightMove == c2.RightMove)
				&& (c1.UpMove == c2.UpMove)
				&& (c1.Angles[0] == c2.Angles[0])
				&& (c1.Angles[1] == c2.Angles[1])
				&& (c1.Angles[2] == c2.Angles[2])
				&& (c1.Impulse == c2.Impulse)
				&& (c1.Flags == c2.Flags)
				&& (c1.MouseX == c2.MouseX)
				&& (c1.MouseY == c2.MouseY));
		}

		public static bool operator !=(idUserCommand c1, idUserCommand c2)
		{
			return !(c1 == c2);
		}
	}

	[Flags]
	public enum Button : byte
	{
		Attack = 1 << 0,
		Run = 1 << 1,
		Zoom = 1 << 2,
		Scores = 1 << 3,
		MouseLook = 1 << 4,
		B5 = 1 << 5,
		B6 = 1 << 6,
		B7 = 1 << 7
	}

	[Flags]
	public enum Impulse
	{
		Weapon0 = 0,
		Weapon1,
		Weapon2,
		Weapon3,
		Weapon4,
		Weapon5,
		Weapon6,
		Weapon7,
		Weapon8,
		Weapon9,
		Weapon10,
		Weapon11,
		Weapon12,
		WeaponReload,
		WeaponNext,
		WeaponPrevious,
		Unused,
		Ready,
		CenterView,
		ShowInterface,
		ToggleTeam,
		Unused2,
		Spectate,
		Unused3,
		Unused4,
		Unused5,
		Unused6,
		Unused7,
		VoteYes,
		VoteNo,
		UseVehicle
	}

	public enum UserCommandButton
	{
		None,

		Up,
		Down,
		Left,
		Right,
		Forward,
		Back,
		LookUp,
		LookDown,
		Strafe,
		MoveLeft,
		MoveRight,

		Button0,
		Button1,
		Button2,
		Button3,
		Button4,
		Button5,
		Button6,
		Button7,

		Attack,
		Speed,
		Zoom,
		ShowScores,
		MouseLook,

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
		Impulse32,
		Impulse33,
		Impulse34,
		Impulse35,
		Impulse36,
		Impulse37,
		Impulse38,
		Impulse39,
		Impulse40,
		Impulse41,
		Impulse42,
		Impulse43,
		Impulse44,
		Impulse45,
		Impulse46,
		Impulse47,
		Impulse48,
		Impulse49,
		Impulse50,
		Impulse51,
		Impulse52,
		Impulse53,
		Impulse54,
		Impulse55,
		Impulse56,
		Impulse57,
		Impulse58,
		Impulse59,
		Impulse60,
		Impulse61,
		Impulse62,
		Impulse63,

		MaxButtons
	}

	[Flags]
	public enum UserCommandFlags : byte
	{
		/// <summary>Toggled every time an impulse command is sent.</summary>
		ImpulseSequence = 0x0001
	}
}