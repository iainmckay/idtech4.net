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

namespace idTech4.Net
{
	public sealed class idAsyncServer
	{
		#region Properties
		public bool IsActive
		{
			get
			{
				// TODO: bool				IsActive( void ) const { return active; }
				return false;
			}
		}
		#endregion

		#region Constructor
		public idAsyncServer()
		{
			// TODO
		/*	int i;

			active = false;
			realTime = 0;
			serverTime = 0;
			serverId = 0;
			serverDataChecksum = 0;
			localClientNum = -1;
			gameInitId = 0;
			gameFrame = 0;
			gameTime = 0;
			gameTimeResidual = 0;
			memset(challenges, 0, sizeof(challenges));
			memset(userCmds, 0, sizeof(userCmds));
			for(i = 0; i < MAX_ASYNC_CLIENTS; i++)
			{
				ClearClient(i);
			}
			serverReloadingEngine = false;
			nextHeartbeatTime = 0;
			nextAsyncStatsTime = 0;
			noRconOutput = true;
			lastAuthTime = 0;

			memset(stats_outrate, 0, sizeof(stats_outrate));
			stats_current = 0;
			stats_average_sum = 0;
			stats_max = 0;
			stats_max_index = 0;*/
		}
		#endregion
	}
}
