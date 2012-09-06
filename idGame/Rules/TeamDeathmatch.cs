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
				idConsole.Warning("TODO: modifiedInfo |= BalanceTeams();");
			}

			// TODO: REFACTOR THIS AWAY
			if((idR.Game.IsClient == false) && (player.Team != player.LatchedTeam))
			{
				idConsole.Warning("TODO: gameLocal.mpGame.SwitchToTeam(entityNumber, latchedTeam, team);");
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
				idConsole.Warning("TODO: SwitchToTeam(clientIndex, -1, idR.Game.Entites[clientIndex]->Team);");
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
