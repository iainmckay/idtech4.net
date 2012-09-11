using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Game.Physics;

namespace idTech4.Game.Entities
{
	public class idAFEntity : idAnimatedEntity
	{
		#region Properties
		public virtual idClipModel CombatModel
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idAFEntity.CombatModel get");

				return null;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idAFEntity.CombatModel set");
			}
		}
		#endregion

		#region Members
		private Vector3 _spawnOrigin;	// spawn origin
		private Matrix _spawnAxis;		// rotation axis used when spawned
		private int _nextSoundTime;		// next time this can make a sound
		#endregion

		#region Constructor
		public idAFEntity()
			: base()
		{
			
		}
		#endregion

		#region Methods
		#region Public
		public virtual void LinkCombat()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.LinkCombat");
		}

		public virtual bool LoadAF()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.LoadAF");

			return false;
		}

		public virtual void UnlinkCombat()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.UnlinkCombat");
		}
		#endregion
		#endregion

		#region idEntity implementation
		#region Methods
		public override void AddForce(idEntity entity, int id, Vector3 point, Vector3 force)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.AddForce");
		}

		public override void ApplyImpulse(idEntity entity, int id, Vector3 point, Vector3 impulse)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.ApplyImpulse");	
		}

		public override bool Collide(object collision, Vector3 velocity)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.Collide");

			return false;
		}

		public override void FreeModelDef()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.FreeModelDef");
		}

		public override object GetImpactInfo(idEntity entity, int id, Vector3 point)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.GetImpactInfo");

			return null;
		}

		public override bool GetPhysicsToVisualTransform(ref Vector3 origin, ref Matrix axis)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.GetPhysicsToVisualTransform");

			return false;
		}

		public override void Restore(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.Restore");
		}

		public override void Save(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.Restore");
		}

		public override void Spawn()
		{
			base.Spawn();

			_spawnOrigin = this.Physics.GetOrigin();
			_spawnAxis = this.Physics.GetAxis();
			_nextSoundTime = 0;
		}

		public override void Think()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// TODO
			idConsole.Warning("TODO: idAFEntity.Think");
			/*RunPhysics();
			UpdateAnimation();
			if(thinkFlags & TH_UPDATEVISUALS)
			{
				Present();
				LinkCombat();
			}*/
		}

		public override bool UpdateAnimationControllers()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAFEntity.UpdateAnimationControllers");

			return false;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// TODO
			if(disposing == true)
			{
				idConsole.Warning("TODO: idAFEntity.Dispose");
				/*delete combatModel;
				combatModel = NULL;*/
			}
		}
		#endregion
		#endregion
	}
}