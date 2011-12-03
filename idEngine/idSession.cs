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

namespace idTech4
{
	public sealed class idSession
	{
		#region Properties
		public bool IsMultiplayer
		{
			get
			{
				return idE.AsyncNetwork.IsActive;
			}
		}
		#endregion

		#region Constructor
		public idSession()
		{
			new idCvar("com_showAngles", "0", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("com_minTics", "1", "", CvarFlags.System);
			new idCvar("com_showTics", "0", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("com_fixedTic", "0", "", 0, 10, CvarFlags.System | CvarFlags.Integer);
			new idCvar("com_showDemo", "0", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("com_skipGameDraw", "0", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("com_aviDemoSamples", "16", "", CvarFlags.System);
			new idCvar("com_aviDemoWidth", "256", "", CvarFlags.System);
			new idCvar("com_aviDemoHeight", "256", "", CvarFlags.System);
			new idCvar("com_aviDemoTics", "2", "", 1, 60, CvarFlags.System | CvarFlags.Integer);
			new idCvar("com_wipeSeconds", "1", "", CvarFlags.System);
			new idCvar("com_guid", "", "", CvarFlags.System | CvarFlags.Archive | CvarFlags.ReadOnly);
		}
		#endregion
	}
}
