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
			idConsole.WriteLine("TODO: idAFEntity");
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
			idConsole.WriteLine("TODO: idAFEntity.Spawn");
			/*spawnOrigin = GetPhysics()->GetOrigin();
			spawnAxis = GetPhysics()->GetAxis();
			nextSoundTime = 0;*/
		}

		public override void Think()
		{
			// TODO
			idConsole.WriteLine("TODO: idAFEntity.Think");
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
			if(disposing == true)
			{
				idConsole.WriteLine("TODO: idAFEntity.Dispose");
				/*delete combatModel;
				combatModel = NULL;*/
			}
		}
		#endregion
	}
}
