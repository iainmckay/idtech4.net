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

namespace idTech4.Game
{
	public abstract class idBaseGame
	{
		/// <summary>
		/// Initialize the game for the first time.
		/// </summary>
		public abstract void Init();
	}

	public struct GameReturn
	{
		/// <summary>"map", "disconnect", "victory", etc.</summary>
		public string SessionCommand;
		/// <summary>Used to check for network game divergence.</summary>
		public int ConsistencyHash;

		public int Health;
		public int HeartRate;
		public int Stamina;
		public int Combat;

		/// <summary>
		/// Used when cinematics are skipped to prevent session from simulating several game frames to
		/// keep the game time in sync with real time.
		/// </summary>
		public bool SyncNextGameFrame;
	}
}