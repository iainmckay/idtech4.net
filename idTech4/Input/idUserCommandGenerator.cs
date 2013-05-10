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
using Microsoft.Xna.Framework.Input;

using idTech4.Services;

using Keys  = idTech4.Services.Keys;
using XKeys = Microsoft.Xna.Framework.Input.Keys;

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
		private bool _mouseLeftClientArea;

		private Impulse _impulse;
		private int _impulseSequence;
		private Vector3 _viewAngles;

		private bool[] _keyState;
		private bool[] _previousKeyState;
		private int[] _commandButtonState;

		private idUserCommand _currentCommand;

		private long	_pollTime;
		private long	_lastPollTime;
		private float _lastLookValuePitch;
		private float _lastLookValueYaw;

		private idEventLoop _eventLoop;
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
		public void BuildCurrentUserCommand(int deviceNum)
		{
			_pollTime = idEngine.Instance.ElapsedTime;

			if((_pollTime - _lastPollTime) > 100)
			{
				_lastPollTime = _pollTime - 100;
			}

			// initialize current usercmd
			InitCurrent();

			// process the system mouse events
			ProcessMouse();

			// process the system keyboard events
			ProcessKeyboard();

			// process the system joystick events
			// TODO: joystick
			/*if ( deviceNum >= 0 && in_useJoystick.GetBool() ) {
				Joystick( deviceNum );
			}*/

			// create the usercmd
			MakeCurrent();

			_lastPollTime = _pollTime;
		}

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
				
		public void Init(idEventLoop eventLoop)
		{
			_initialized = true;
			_eventLoop   = eventLoop;
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
		private void InitCurrent()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			_currentCommand                 = new idUserCommand();
			_currentCommand.ImpulseSequence = (byte) _impulseSequence;
			_currentCommand.Impulse         = _impulse;
			_currentCommand.Buttons	       |= ((cvarSystem.GetBool("in_alwaysRun") == true) && (idEngine.Instance.IsMultiplayer == true)) ? Button.Run : 0;
		}

		private void MakeCurrent()
		{
			ICVarSystem cvarSystem   = idEngine.Instance.GetService<ICVarSystem>();
			Vector3 oldAngles        = _viewAngles;

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

		private void ProcessKeyboard()
		{
			KeyboardState keyState = Keyboard.GetState();
			MouseState mouseState  = Mouse.GetState();

			int mouse1 = (int) Keys.Mouse1;
			int mouse2 = (int) Keys.Mouse2;
			int mouse3 = (int) Keys.Mouse3;
			
			_previousKeyState = (bool[]) _keyState.Clone();

			foreach(Keys key in Enum.GetValues(typeof(Keys)))
			{
				if((key != Keys.Invalid) && (key != Keys.LastKey))
				{
					_keyState[(int) key] = false;
				}
			}

			foreach(XKeys key in keyState.GetPressedKeys())
			{
				_keyState[(int) key] = true;
			}

			_keyState[mouse1] = (mouseState.LeftButton   == ButtonState.Pressed);
			_keyState[mouse2] = (mouseState.MiddleButton == ButtonState.Pressed);
			_keyState[mouse3] = (mouseState.RightButton  == ButtonState.Pressed);

			foreach(Keys key in Enum.GetValues(typeof(Keys)))
			{
				if((key != Keys.Invalid) && (key != Keys.LastKey))
				{
					int keyCode = (int) key;

					if(_previousKeyState[keyCode] != _keyState[keyCode])
					{
						Key(key, _keyState[keyCode]);
					}
				}
			}
		}

		private void ProcessMouse()
		{
			IInputSystem inputSystem   = idEngine.Instance.GetService<IInputSystem>();
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();
			MouseState mouseState      = Mouse.GetState();
			
			int screenHalfWidth  = renderSystem.Width / 2;
			int screenHalfHeight = renderSystem.Height / 2;

			_mouseDeltaX = (mouseState.X - screenHalfWidth);
			_mouseDeltaY = (mouseState.Y - screenHalfHeight);

			_continuousMouseX += _mouseDeltaX;
			_continuousMouseY += _mouseDeltaY;

			bool mouse1State = mouseState.LeftButton   == ButtonState.Pressed;
			bool mouse2State = mouseState.RightButton  == ButtonState.Pressed;
			bool mouse3State = mouseState.MiddleButton == ButtonState.Pressed;

			if(mouse1State != _keyState[(int) Keys.Mouse1])
			{
				_eventLoop.Queue(SystemEventType.Key, (int) Keys.Mouse1, mouse1State ? 1 : 0, 0);
			}

			if(mouse2State != _keyState[(int) Keys.Mouse2])
			{
				_eventLoop.Queue(SystemEventType.Key, (int) Keys.Mouse2, mouse2State ? 1 : 0, 0);
			}

			if(mouse3State != _keyState[(int) Keys.Mouse3])
			{
				_eventLoop.Queue(SystemEventType.Key, (int) Keys.Mouse3, mouse3State ? 1 : 0, 0);
			}

			// is the mouse still within the bounds of the client area?
			if((mouseState.IsInsideWindow() == false) && (_mouseLeftClientArea == false))
			{
				_eventLoop.Queue(SystemEventType.MouseLeave, 0, 0, 0);
				_mouseLeftClientArea = true;
			}
			else if((mouseState.IsInsideWindow() == true) && (_mouseLeftClientArea == true))
			{
				_mouseLeftClientArea = false;
			}

			if(_mouseLeftClientArea == false)
			{
				if((_mouseDeltaX != 0) || (_mouseDeltaY != 0))
				{
					_eventLoop.Queue(SystemEventType.Mouse, _mouseDeltaX, _mouseDeltaY, 0);
				}
			}

			if(inputSystem.GrabMouse == true)
			{
				Mouse.SetPosition(screenHalfWidth, screenHalfHeight);
			}
		}

		private void Key(Keys key, bool state)
		{
			IInputSystem inputSystem = idEngine.Instance.GetService<IInputSystem>();
			int keyCode              = (int) key;

			// sanity check, sometimes we get double message :(
			if(_keyState[keyCode] == state)
			{
				return;
			}

			_keyState[keyCode] = state;

			UserCommandButton action = inputSystem.GetUserCommandButtonFromKey(key);

			if(state == false)
			{
				_commandButtonState[(int) action]++;

				if(this.Inhibited == false)
				{
					if((action >= UserCommandButton.Impulse0) && (action <= UserCommandButton.Impulse31))
					{
						_currentCommand.Impulse = (Impulse) (action - UserCommandButton.Impulse0);
						_currentCommand.ImpulseSequence++;
		
					}
				}
			}
			else
			{
				_commandButtonState[(int) action]--;
		
				// we might have one held down across an app active transition
				if(_commandButtonState[(int) action] < 0) 
				{
					_commandButtonState[(int) action] = 0;
				}
			}
		}
		#endregion
		#endregion
	}
}