using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Text;
using idTech4.Text.Decl;

namespace idTech4.Renderer
{
	public class idRenderModel_PRT : idRenderModel_Static
	{
		#region Members
		private idDeclParticle _particleSystem;
		#endregion

		#region Constructor
		public idRenderModel_PRT()
			: base()
		{

		}
		#endregion

		#region idRenderModel_Static implementation
		#region Properties
		public override float DepthHack
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _particleSystem.DepthHack;
			}

		}

		public override DynamicModel IsDynamicModel
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return DynamicModel.Continuous;
			}
		}

		public override int MemoryUsage
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idRenderModel_PRT.MemoryUsage");

				return 0;
			}
		}
		#endregion

		#region Methods
		public override idBounds GetBounds(idRenderEntity renderEntity = null)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _particleSystem.Bounds;
		}

		public override void InitFromFile(string fileName)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_name = fileName;
			_particleSystem = idE.DeclManager.FindType<idDeclParticle>(DeclType.Particle, fileName);
		}

		public override idRenderModel InstantiateDynamicModel(idTech4.Renderer.idRenderEntity renderEntity, View view, idRenderModel cachedModel)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idRenderModel_PRT.InstantiateDynamicModel");

			return null;
		}

		public override void TouchData()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// ensure our particle system is added to the list of referenced decls
			_particleSystem = idE.DeclManager.FindType<idDeclParticle>(DeclType.Particle, _name);
		}
		#endregion
		#endregion

		/*
	
	virtual idRenderModel *		InstantiateDynamicModel( const struct renderEntity_s *ent, const struct viewDef_s *view, idRenderModel *cachedModel );
	
};*/
	}
}
