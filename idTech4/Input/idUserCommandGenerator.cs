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

using idTech4.Services;

namespace idTech4.Input
{
	public sealed class idUserCommandGenerator
	{
		#region Properties
		public idUserCommand CurrentUserCommand
		{
			get
			{
				return _currentCommand;
			}
		}

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

		private int _continuousMouseX;
		private int _continuousMouseY;
		private int	_mouseButton;
		private bool _mouseDown;

		private Impulse _impulse;
		private int _impulseSequence;
		private Vector3 _viewAngles;

		private bool[] _keyState;
		private int[] _commandButtonState;

		private idUserCommand _currentCommand;
		#endregion

		#region Command map
		private Dictionary<string, UserCommandButton> _commandMap = new Dictionary<string, UserCommandButton>(StringComparer.OrdinalIgnoreCase) {
			{ "_moveUp",		UserCommandButton.MoveUp },
			{ "_moveDown",		UserCommandButton.MoveDown },
			{ "_left",			UserCommandButton.MoveLeft },
			{ "_right",			UserCommandButton.MoveRight },
			{ "_forward",		UserCommandButton.MoveForward },
			{ "_back",			UserCommandButton.MoveBack },
			{ "_lookUp",		UserCommandButton.LookUp },
			{ "_lookDown",		UserCommandButton.LookDown },
			{ "_moveLeft",		UserCommandButton.MoveLeft },
			{ "_moveRight",		UserCommandButton.MoveRight },

			{ "_attack",		UserCommandButton.Attack },
			{ "_speed",			UserCommandButton.Speed },
			{ "_zoom",			UserCommandButton.Zoom },
			{ "_showScores",	UserCommandButton.ShowScores },

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
			{ "_impulse31",		UserCommandButton.Impulse31 }
		};
		#endregion

		#region Constructor
		public idUserCommandGenerator()
		{
			// TODO
			/*toggled_crouch.Clear();
			toggled_run.Clear();
			toggled_zoom.Clear();
			toggled_run.on = false;

			ClearAngles();*/
			Clear();
		}
		#endregion

		#region Methods
		#region Public
		public void Clear()
		{
			// clears all key states 
			_keyState           = new bool[(int) Keys.LastKey];
			_commandButtonState = new int[(int) UserCommandButton.MaxButtons];
			// TODO: memset(joystickAxis, 0, sizeof(joystickAxis));

			_inhibitCommands = 0;

			_mouseDeltaX     = _mouseDeltaY = 0;
			_mouseButton     = 0;
			_mouseDown       = true;
		}
				
		public void Init()
		{
			_initialized = true;
		}

		public void InitForNewMap()
		{
			_impulseSequence = 0;
			_impulse         = 0;

			// TODO: toggle input
			/*toggled_crouch.Clear();
			toggled_run.Clear();
			toggled_zoom.Clear();
			toggled_run.on = false;*/

			Clear();
			idLog.Warning("TODO: cmd ClearAngles();");
		}
		#endregion

		#region Private
		private void MakeCurrent()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			Vector3 oldAngles      = _viewAngles;

			if(this.Inhibited == false)
			{
				// update toggled key states
				// TODO: toggled
				// update toggled key states
				/*toggled_crouch.SetKeyState(ButtonState(UB_MOVEDOWN), in_toggleCrouch.GetBool());
				toggled_run.SetKeyState(ButtonState(UB_SPEED), in_toggleRun.GetBool() && common->IsMultiplayer());
				toggled_zoom.SetKeyState(ButtonState(UB_ZOOM), in_toggleZoom.GetBool());*/

				// get basic movement from mouse
				idLog.Warning("TODO: MouseMove();");

				// get basic movement from joystick and set key bits
				// must be done before CmdButtons!
				if(cvarSystem.GetBool("joy_newCode") == true)
				{
					idLog.Warning("TODO: JoystickMove2();");
				}
				else
				{
					idLog.Warning("TODO: JoystickMove();");
				}

				// keyboard angle adjustment
				idLog.Warning("TODO: AdjustAngles();");

				// set button bits
				idLog.Warning("TODO: CmdButtons();");

				// get basic movement from keyboard
				idLog.Warning("TODO: KeyMove();");

				// aim assist
				idLog.Warning("TODO: AimAssist();");


				idLog.Warning("TODO: PITCH IMPORTANT");

				// check to make sure the angles haven't wrapped
				/*if(viewangles[PITCH] - oldAngles[PITCH] > 90)
				{
					viewangles[PITCH] = oldAngles[PITCH] + 90;
				}
				else if(oldAngles[PITCH] - viewangles[PITCH] > 90)
				{
					viewangles[PITCH] = oldAngles[PITCH] - 90;
				}*/ 
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

			_currentCommand.MouseX = (short) _continuousMouseX;
			_currentCommand.MouseY = (short) _continuousMouseY;

			_impulseSequence = _currentCommand.ImpulseSequence;
			_impulse         = _currentCommand.Impulse;
		}		
		#endregion
		#endregion
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
}