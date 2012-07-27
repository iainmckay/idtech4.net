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

using idTech4.Input;

namespace idTech4.Game
{
	public abstract class idBaseGame
	{
		public abstract bool Draw(int clientIndex);

		/// <summary>
		/// Initialize the game for the first time.
		/// </summary>
		public abstract void Init();

		/// <summary>
		/// Caches media referenced from in key/value pairs in the given dictionary.
		/// </summary>
		/// <remarks>
		/// This is called after parsing an EntityDef and for each entity spawnArgs before
		/// merging the entitydef.  It could be done post-merge, but that would
		/// avoid the fast pre-cache check associated with each entityDef.
		/// </remarks>
		/// <param name="dict"></param>
		public abstract void CacheDictionaryMedia(idDict dict);

		public abstract string GetMapLoadingInterface(string defaultInterface);

		/// <summary>
		/// Runs a game frame, may return a session command for level changing, etc.
		/// </summary>
		/// <param name="userCommands"></param>
		/// <returns></returns>
		public abstract GameReturn RunFrame(idUserCommand[] userCommands);

		/// <summary>
		/// The session calls this right before a new level is loaded.
		/// </summary>
		/// <param name="clientIndex"></param>
		/// <param name="playerInfo"></param>
		public abstract void SetPersistentPlayerInformation(int clientIndex, idDict playerInfo);

		/// <summary>
		/// Sets the user info for a client.
		/// </summary>
		/// <param name="clientIndex"></param>
		/// <param name="userInfo"></param>
		/// <param name="isClient"></param>
		/// <param name="canModify">If true, the game can modify the user info.  Never true on a network client.</param>
		/// <returns></returns>
		public abstract idDict SetUserInformation(int clientIndex, idDict userInfo, bool isClient, bool canModify);

		/// <summary>
		/// Spawns the player entity to be used by the client.
		/// </summary>
		/// <param name="clientIndex"></param>
		public abstract void SpawnPlayer(int clientIndex);
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