/*
===========================================================================

Doom 3 GPL Source Code
Copyright (C) 1999-2011 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 GPL Source Code (?Doom 3 Source Code?).  

Doom 3 Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Geometry;

namespace idTech4.Renderer
{
	public class idRenderModel_Static : idRenderModel
	{
		#region Members
		private List<RenderModelSurface> _surfaces = new List<RenderModelSurface>();
		private int _overlaysAdded;

		private int _lastModifiedFrame;
		private int _lastArchivedFrame;
		private List<Surface> _shadowHull;
		private bool _isStaticWordModel;
		private bool _defaulted;		
		private bool _fastLoad; // don't generate tangents and shadow data.
		private bool _reloadable; // if not, reloadModels won't check timestamp
		private bool _levelLoadReferenced; // for determining if it needs to be freed.

		protected string _name;
		protected idBounds _bounds;
		protected bool _purged; // eventually we will have dynamic reloading.
		#endregion

		#region Constructor
		public idRenderModel_Static()
		{
			_name = "<undefined>";
			_reloadable = true;
		}
		#endregion

		#region Methods
		#region Private

		#endregion
		#endregion

		#region idRenderModel implementation
		#region Properties
		public override idJointQuaternion[] DefaultPose
		{
			get
			{
				return null;
			}
		}

		public override float DepthHack
		{
			get
			{
				return 0.0f;
			}
		}
		
		public override bool IsDefaultModel
		{
			get 
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _defaulted;
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

				return DynamicModel.Static;
			}
		}

		public override bool IsLoaded
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return (_purged == false);
			}
		}

		public override bool IsReloadable
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _reloadable;
			}
		}

		public override bool IsStaticWordModel
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _isStaticWordModel;
			}
		}

		public override idMD5Joint[] Joints
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return null;
			}
		}

		public override int JointCount
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return 0;
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

				idConsole.Warning("TODO: idStaticRenderModel.MemoryUsage");
				return 0;
			}
		}

		public override string Name
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _name;
			}
		}
		#endregion

		#region Methods
		public override void AddSurface(RenderModelSurface surface)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idStaticRenderModel.AddSurface");
		}

		public override void FinishSurfaces()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idStaticRenderModel.FinishSurfaces");
		}

		public override void FreeVertexCache()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idStaticRenderModel.FreeVertexCache");
		}

		public override idBounds GetBounds(idRenderEntity renderEntity = null)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _bounds;
		}

		public override int GetJointIndex(string name)
		{
			return -1;
		}

		public override int GetJointIndex(idMD5Joint joint)
		{
			return -1;
		}

		public override string GetJointName(int index)
		{
			return string.Empty;
		}

		public override int GetNearestJoint(int surfaceIndex, int a, int c, int b)
		{
			return -1;
		}

		public override void InitEmpty(string name)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// model names of the form _area* are static parts of the
			// world, and have already been considered for optimized shadows
			// other model names are inline entity models, and need to be
			// shadowed normally
			_isStaticWordModel = (name.StartsWith("_area") == true);

			_name = name;
			_reloadable = false; // if it didn't come from a file, we can't reload it

			Purge();

			_purged = false;
			_bounds = idBounds.Zero;
		}

		public override void InitFromFile(string fileName)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			InitEmpty(fileName);

			// FIXME: load new .proc map format

			string extension = Path.GetExtension(fileName);
			bool loaded = false;

			/*if(extension.Icmp("ase") == 0)
			{
				loaded = LoadASE(name);
				reloadable = true;
			}
			else if(extension.Icmp("lwo") == 0)
			{
				loaded = LoadLWO(name);
				reloadable = true;
			}
			else if(extension.Icmp("flt") == 0)
			{
				loaded = LoadFLT(name);
				reloadable = true;
			}
			else if(extension.Icmp("ma") == 0)
			{
				loaded = LoadMA(name);
				reloadable = true;
			}
			else
			{
				common->Warning("idRenderModelStatic::InitFromFile: unknown type for model: \'%s\'", name.c_str());
				loaded = false;
			}*/

			if(loaded == false)
			{
				idConsole.DeveloperWriteLine("Couldn't load model: '{0}'", fileName);
				MakeDefault();

				return;
			}

			// it is now available for use
			_purged = false;

			// create the bounds for culling and dynamic surface creation
			FinishSurfaces();
		}

		public override idRenderModel InstantiateDynamicModel(idRenderEntity renderEntity, View view, idRenderModel cachedModel)
		{
			if(cachedModel != null)
			{
				cachedModel.Dispose();
			}

			idConsole.Error("InstantiateDynamicModel called on static model '{0}'", this.Name);

			return null;
		}

		public override void List()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idStaticRenderModel.List");
		}

		public override void Load()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			Purge();
			InitFromFile(_name);
		}

		public override void MakeDefault()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idStaticRenderModel.MakeDefault");
		}

		public override void PartialInitFromFile(string fileName)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_fastLoad = true;

			InitFromFile(fileName);
		}

		public override void Print()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idStaticRenderModel.Print");
		}

		public override void Purge()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			RenderModelSurface surf;

			for(int i = 0; i < _surfaces.Count; i++)
			{
				surf = _surfaces[i];

				if(surf.Geometry != null)
				{
					surf.Geometry.Dispose();
					surf.Geometry = null;
				}
			}

			_surfaces.Clear();
			_purged = true;
		}

		public override void Reset()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void TouchData()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			foreach(RenderModelSurface surf in _surfaces)
			{
				// re-find the material to make sure it gets added to the
				// level keep list.
				idE.DeclManager.FindMaterial(surf.Material.Name);
			}
		}
		#endregion
		#endregion
	}
}