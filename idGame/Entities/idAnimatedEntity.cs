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

		#region Members
		private idAnimator _animator;
		#endregion

		#region Constructor
		public idAnimatedEntity()
			: base()
		{
			_animator = new idAnimator(this);
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

				return _animator;
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

		protected override void SetModel(string modelName)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			FreeModelDef();

			this.RenderEntity.Model = _animator.SetModel(modelName);

			if(this.RenderEntity.Model == null)
			{
				base.SetModel(modelName);
			}
			else
			{
				if(this.RenderEntity.CustomSkin == null)
				{
					this.RenderEntity.CustomSkin = _animator.ModelDefinition.DefaultSkin;
				}

				// set the callback to update the joints
				this.RenderEntity.Callback = ModelCallback;
				this.RenderEntity.Joints = _animator.GetJoints();

				_animator.GetBounds(idR.Game.Time, out this.RenderEntity.Bounds);

				UpdateVisuals();
			}
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