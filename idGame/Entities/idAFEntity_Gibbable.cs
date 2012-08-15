using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

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

		~idAFEntity_Gibbable()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		public virtual void SpawnGibs(Vector3 direction, string damageDefName)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity_Gibbable.SpawnGibs");
		}
		#endregion
		#endregion

		#region idEntity implementation
		#region Methods
		public override void Damage(idEntity inflictor, idEntity attacker, Microsoft.Xna.Framework.Vector3 direction, string damageDefName, float damageScale, int location)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity_Gibbable.Damage");
		}

		public override void Present()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity_Gibbable.Present");
		}

		public override void Restore(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity_Gibbable.Restore");
		}

		public override void Save(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity_Gibbable.Save");
		}

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
		#endregion
	}
}