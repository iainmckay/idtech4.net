using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Game.Entities;

namespace idTech4.Game.Rules
{
	public class Tourney : Multiplayer
	{
		#region Constructor
		public Tourney()
			: base()
		{

		}
		#endregion

		#region idGameRules implementation
		public override void SpawnPlayer(int clientIndex)
		{
			base.SpawnPlayer(clientIndex);

			if(idR.Game.IsClient == false)
			{
				if(this.State == MultiplayerGameState.GameOn)
				{
					idPlayer player = (idPlayer) idR.Game.Entities[clientIndex];
					player.TourneyRank = 1;
				}
			}
		}
		#endregion
	}
}
