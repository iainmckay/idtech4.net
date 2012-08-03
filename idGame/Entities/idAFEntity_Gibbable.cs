using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4.Game.Entities
{
	public class idAFEntity_Gibbable : idAFEntity
	{
		#region Constructor
		public idAFEntity_Gibbable() : base()
		{
			// TODO
			idConsole.Warning("TODO: idAFEntity_Gibbable");
			/*skeletonModel = NULL;
			skeletonModelDefHandle = -1;
			gibbed = false;*/
		}
		#endregion

		#region idEntity implementation
		public override void Spawn()
		{
			base.Spawn();

			// TODO
			idConsole.Warning("TODO: idAFEntity_Gibbable.Spawn");
			/*InitSkeletonModel();

			gibbed = false;*/
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// TODO
			idConsole.Warning("TODO: idAFEntity_Gibbable.Dispose");
			/*if(skeletonModelDefHandle != -1)
			{
				gameRenderWorld->FreeEntityDef(skeletonModelDefHandle);
				skeletonModelDefHandle = -1;
			}*/
		}
		#endregion
	}
}