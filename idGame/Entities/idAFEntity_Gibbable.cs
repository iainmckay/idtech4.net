﻿using System;
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
			/*InitSkeletonModel();

			gibbed = false;*/
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// TODO
			/*if(skeletonModelDefHandle != -1)
			{
				gameRenderWorld->FreeEntityDef(skeletonModelDefHandle);
				skeletonModelDefHandle = -1;
			}*/
		}
		#endregion
	}
}