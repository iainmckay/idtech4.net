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

namespace idTech4.Input
{
	public class idUserInput
	{
		#region Members
		private bool _initialized;
		#endregion

		#region Constructor
		public idUserInput()
		{
			InitCvars();

			// TODO
			/*lastCommandTime = 0;
			initialized = false;

			flags = 0;
			impulse = 0;

			toggled_crouch.Clear();
			toggled_run.Clear();
			toggled_zoom.Clear();
			toggled_run.on = in_alwaysRun.GetBool(); // THIS WON'T WORK!!!! in_alwaysRun not set until Init()

			ClearAngles();
			Clear();*/
		}
		#endregion

		#region Methods
		#region Public
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
		#endregion
		#endregion
	}
}
