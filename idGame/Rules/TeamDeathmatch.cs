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

using idTech4.Game.Entities;

namespace idTech4.Game.Rules
{
	public class TeamDeathmatch : Multiplayer
	{
		#region Constructor
		public TeamDeathmatch()
			: base()
		{

		}
		#endregion

		#region idGameRules implementation
		public override bool UserInfoChanged(int clientIndex, bool canModify)
		{
			bool modifiedInfo = base.UserInfoChanged(clientIndex, canModify);

			idPlayer player = (idPlayer) idR.Game.Entities[clientIndex];
			player.Team = (player.Info.GetString("ui_team").ToLower() == "blue") ? PlayerTeam.Blue : PlayerTeam.Red;

			// server maintains TDM balance
			if((canModify == true) && (this.IsInGame(clientIndex) == true) && (idR.CvarSystem.GetBool("g_balanceTDM") == true))
			{
				// TODO: modifiedInfo |= BalanceTeams();
			}

			// TODO: REFACTOR THIS AWAY
			if((idR.Game.IsClient == false) && (player.Team != player.LatchedTeam))
			{
				// TODO: gameLocal.mpGame.SwitchToTeam(entityNumber, latchedTeam, team);
			}

			player.LatchedTeam = player.Team;

			return modifiedInfo;
		}

		protected override string GetPlayerSkin(idPlayer player)
		{
			string baseName = string.Empty;
			
			if(player.Team == PlayerTeam.Blue)
			{
				baseName = "skins/characters/player/marine_mp_blue";
			}
			else
			{
				baseName = "skins/characters/player/marine_mp_red";
			}
						
			return baseName;
		}

		public override void SpawnPlayer(int clientIndex)
		{
			base.SpawnPlayer(clientIndex);

			if(idR.Game.IsClient == false)
			{
				// TODO SwitchToTeam(clientIndex, -1, idR.Game.Entites[clientIndex]->Team);
			}
		}
		#endregion
	}

	public enum PlayerTeam
	{
		Red,
		Blue
	}
}
