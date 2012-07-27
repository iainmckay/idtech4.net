using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4.Game.Entities
{
	public class idAnimatedEntity : idEntity
	{
		#region Constructor
		public idAnimatedEntity()
			: base()
		{
			// TODO
			/*animator.SetEntity(this);
			damageEffects = NULL;*/
		}
		#endregion

		#region idEntity implementation
		public override void Think()
		{
			// TODO
			/*RunPhysics();
			UpdateAnimation();
			Present();
			UpdateDamageEffects();*/
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// TODO
			/*damageEffect_t* de;

			for(de = damageEffects; de; de = damageEffects)
			{
				damageEffects = de->next;
				delete de;
			}*/
		}
		#endregion
	}
}