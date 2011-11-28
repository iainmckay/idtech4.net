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

namespace idTech4.Game.Entities
{
	public class idAFEntity : idAnimatedEntity
	{
		#region Constructor
		public idAFEntity()
			: base()
		{
			// TODO
			/*combatModel = NULL;
			combatModelContents = 0;
			nextSoundTime = 0;
			spawnOrigin.Zero();
			spawnAxis.Identity();*/
		}
		#endregion

		#region idEntity implementation
		public override void Spawn()
		{
			base.Spawn();

			// TODO
			/*spawnOrigin = GetPhysics()->GetOrigin();
			spawnAxis = GetPhysics()->GetAxis();
			nextSoundTime = 0;*/
		}

		public override void Think()
		{
			// TODO
			/*RunPhysics();
			UpdateAnimation();
			if(thinkFlags & TH_UPDATEVISUALS)
			{
				Present();
				LinkCombat();
			}*/
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// TODO
			/*delete combatModel;
			combatModel = NULL;*/
		}
		#endregion
	}
}
