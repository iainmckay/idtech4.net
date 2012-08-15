using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Game.Animation;
using idTech4.Net;
using idTech4.Renderer;

namespace idTech4.Game.Entities
{
	public class idAnimatedEntity : idEntity
	{
		#region Properties
		public virtual SurfaceTypes DefaultSurfaceType
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return SurfaceTypes.Metal;
			}
		}
		#endregion

		#region Constructor
		public idAnimatedEntity()
			: base()
		{
			// TODO
			idConsole.Warning("TODO: idAnimatedEntity");
			/*animator.SetEntity(this);*/
		}

		~idAnimatedEntity()
		{
			Dispose(false);
		}
		#endregion

		#region idEntity implementation
		#region Properties
		public override idAnimator Animator
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idAnimatedEntity.Animator");

				return null;
			}
		}
		
		public override idRenderModel Model
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idAnimatedEntity.Model get");

				return null;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idAnimatedEntity.Model set");
			}
		}
		#endregion

		#region Methods
		public override void AddDamageEffect(object collision, Microsoft.Xna.Framework.Vector3 velocity, string damageDefName)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAnimatedEntity.AddDamageEffect");
		}

		public override void ClientPredictionThink()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAnimatedEntity.ClientPredictionThink");
		}

		public override bool ClientReceiveEvent(int ev, int time, idBitMsg msg)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAnimatedEntity.ClientReceiveEvent");

			return false;
		}

		public override void Restore(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAnimatedEntity.Restore");
		}

		public override void Save(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAnimatedEntity.Save");
		}

		public override void Think()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idAnimatedEntity.Think");
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
			if(disposing == true)
			{
				idConsole.Warning("TODO: idAnimatedEntity.Dispose");
				/*damageEffect_t* de;

				for(de = damageEffects; de; de = damageEffects)
				{
					damageEffects = de->next;
					delete de;
				}*/
			}
		}
		#endregion
		#endregion
	}
}