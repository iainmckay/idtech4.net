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

using Microsoft.Xna.Framework;

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
		private void AddCubeFace(Surface tri, int faceNumber, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
		{
			int verticeOffset = faceNumber * 4;
			int indexOffset = faceNumber * 6;

			tri.Vertices[verticeOffset + 0].Clear();
			tri.Vertices[verticeOffset + 0].Position = v1 * 8;
			tri.Vertices[verticeOffset + 0].TextureCoordinates = new Vector2(0, 0);

			tri.Vertices[verticeOffset + 1].Clear();
			tri.Vertices[verticeOffset + 1].Position = v2 * 8;
			tri.Vertices[verticeOffset + 1].TextureCoordinates = new Vector2(1, 0);

			tri.Vertices[verticeOffset + 2].Clear();
			tri.Vertices[verticeOffset + 2].Position = v3 * 8;
			tri.Vertices[verticeOffset + 2].TextureCoordinates = new Vector2(1, 1);

			tri.Vertices[verticeOffset + 3].Clear();
			tri.Vertices[verticeOffset + 3].Position = v4 * 8;
			tri.Vertices[verticeOffset + 3].TextureCoordinates = new Vector2(0, 1);

			tri.Indexes[indexOffset + 0] = verticeOffset + 0;
			tri.Indexes[indexOffset + 1] = verticeOffset + 1;
			tri.Indexes[indexOffset + 2] = verticeOffset + 2;
			tri.Indexes[indexOffset + 3] = verticeOffset + 0;
			tri.Indexes[indexOffset + 4] = verticeOffset + 2;
			tri.Indexes[indexOffset + 5] = verticeOffset + 3;
		}
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

		public override int SurfaceCount
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _surfaces.Count;
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

			_surfaces.Add(surface);

			if(surface.Geometry != null)
			{
				_bounds += surface.Geometry.Bounds;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// The mergeShadows option allows surfaces with different textures to share
		/// silhouette edges for shadow calculation, instead of leaving shared edges
		/// hanging.
		/// <para/>
		/// If any of the original shaders have the noSelfShadow flag set, the surfaces
		/// can't be merged, because they will need to be drawn in different order.
		/// <para/>
		/// If there is only one surface, a separate merged surface won't be generated.
		/// <para/>
		/// A model with multiple surfaces can't later have a skinned shader change the
		/// state of the noSelfShadow flag.
		/// <para/>
		/// -----------------
		/// <para/>
		/// Creates mirrored copies of two sided surfaces with normal maps, which would
		/// otherwise light funny.
		/// <para/>
		/// Extends the bounds of deformed surfaces so they don't cull incorrectly at screen edges.
		/// </remarks>
		public override void FinishSurfaces()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_purged = false;

			// make sure we don't have a huge bounds even if we don't finish everything
			_bounds = idBounds.Zero;

			if(_surfaces.Count == 0)
			{
				return;
			}
			
			// renderBump doesn't care about most of this
			if(_fastLoad == true)
			{
				_bounds = idBounds.Zero;

				foreach(RenderModelSurface surf in _surfaces)
				{
					idHelper.BoundTriangleSurface(surf.Geometry);
					_bounds.AddBounds(surf.Geometry.Bounds);
				}

				return;
			}

			// cleanup all the final surfaces, but don't create sil edges
			int totalVerts = 0;
			int totalIndexes = 0;

			// decide if we are going to merge all the surfaces into one shadower
			int	numOriginalSurfaces = _surfaces.Count;

			// make sure there aren't any NULL shaders or geometry
			for(int i = 0; i < numOriginalSurfaces; i++)
			{
				RenderModelSurface surf = _surfaces[i];

				if((surf.Geometry == null) || (surf.Material == null))
				{
					MakeDefault();
					idConsole.Error("Model {0}, surface {1} had NULL goemetry", this.Name, i);
				}

				if(surf.Material == null)
				{
					MakeDefault();
					idConsole.Error("Model {0}, surface {1} had NULL material", this.Name, i);
				}
			}

			// duplicate and reverse triangles for two sided bump mapped surfaces
			// note that this won't catch surfaces that have their shaders dynamically
			// changed, and won't work with animated models.
			// It is better to create completely separate surfaces, rather than
			// add vertexes and indexes to the existing surface, because the
			// tangent generation wouldn't like the acute shared edges
			for(int i = 0; i < numOriginalSurfaces; i++)
			{
				RenderModelSurface surf = _surfaces[i];
				
				if(surf.Material.ShouldCreateBackSides == true)
				{
					idConsole.Warning("TODO: should create back sides");

					/*srfTriangles_t *newTri;

					newTri = R_CopyStaticTriSurf( surf->geometry );
					R_ReverseTriangles( newTri );

					modelSurface_t	newSurf;

					newSurf.shader = surf->shader;
					newSurf.geometry = newTri;

					AddSurface( newSurf );*/
				}
			}

			// clean the surfaces
			// TODO: clean surfaces	
			/*for ( i = 0 ; i < surfaces.Num() ; i++ ) {
				const modelSurface_t	*surf = &surfaces[i];

				R_CleanupTriangles( surf->geometry, surf->geometry->generateNormals, true, surf->shader->UseUnsmoothedTangents() );
				if ( surf->shader->SurfaceCastsShadow() ) {
					totalVerts += surf->geometry->numVerts;
					totalIndexes += surf->geometry->numIndexes;
				}
			}*/

			// add up the total surface area for development information
			// TODO: surf dev info
			/*for ( i = 0 ; i < surfaces.Num() ; i++ ) {
				const modelSurface_t	*surf = &surfaces[i];
				srfTriangles_t	*tri = surf->geometry;

				for ( int j = 0 ; j < tri->numIndexes ; j += 3 ) {
					float	area = idWinding::TriangleArea( tri->verts[tri->indexes[j]].xyz,
						 tri->verts[tri->indexes[j+1]].xyz,  tri->verts[tri->indexes[j+2]].xyz );
					const_cast<idMaterial *>(surf->shader)->AddToSurfaceArea( area );
				}
			}*/

			// calculate the bounds
			int surfaceCount = _surfaces.Count;

			if(surfaceCount == 0)
			{
				_bounds = idBounds.Zero;
			}
			else
			{
				_bounds.Clear();

				for(int i = 0; i < surfaceCount; i++)
				{
					RenderModelSurface surf = _surfaces[i];

					// if the surface has a deformation, increase the bounds
					// the amount here is somewhat arbitrary, designed to handle
					// autosprites and flares, but could be done better with exact
					// deformation information.
					// Note that this doesn't handle deformations that are skinned in
					// at run time...
					if(surf.Material.Deform != DeformType.None)
					{
						idConsole.Warning("TODO: deform");

						/*srfTriangles_t	*tri = surf->geometry;
						idVec3	mid = ( tri->bounds[1] + tri->bounds[0] ) * 0.5f;
						float	radius = ( tri->bounds[0] - mid ).Length();
						radius += 20.0f;

						tri->bounds[0][0] = mid[0] - radius;
						tri->bounds[0][1] = mid[1] - radius;
						tri->bounds[0][2] = mid[2] - radius;

						tri->bounds[1][0] = mid[0] + radius;
						tri->bounds[1][1] = mid[1] + radius;
						tri->bounds[1][2] = mid[2] + radius;*/
					}

					// add to the model bounds
					_bounds.AddBounds(surf.Geometry.Bounds);
				}
			}
		}

		public override RenderModelSurface GetSurface(int index)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _surfaces[index];
		}

		public override void FreeVertexCache()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idStaticRenderModel.FreeVertexCache");
		}

		public override idBounds GetBounds(RenderEntityComponent renderEntity = null)
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

			string extension = Path.GetExtension(fileName).ToLower();
			bool loaded = false;

			if(extension == "ase")
			{
				idConsole.Warning("TODO: ase");

				/*loaded = LoadASE(name);
				reloadable = true;*/
			}
			else if(extension == "lwo")
			{
				idConsole.Warning("TODO: lwo");

				/*loaded = LoadLWO(fileName);
				_reloadable = true;*/
			}
			else if(extension == "flt")
			{
				idConsole.Warning("TODO: flt");
				/*loaded = LoadFLT(name);
				reloadable = true;*/
			}
			else if(extension == "ma")
			{
				idConsole.Warning("TODO: ma");
				/*loaded = LoadMA(name);
				reloadable = true;*/
			}
			else
			{
				idConsole.Warning("idRenderModel_Static::InitFromFile: unknown type for  model: '{0}'", fileName);
				loaded = false;
			}

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

			_defaulted = true;

			// throw out any surfaces we already have
			Purge();

			// create one new surface
			RenderModelSurface surf = new RenderModelSurface();
			surf.Material = idE.RenderSystem.DefaultMaterial;
			surf.Geometry = new Surface();
			surf.Geometry.Vertices = new Vertex[24];
			surf.Geometry.Indexes = new int[36];

			AddCubeFace(surf.Geometry, 0, new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1));
			AddCubeFace(surf.Geometry, 1, new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(-1, 1, -1));

			AddCubeFace(surf.Geometry, 2, new Vector3(1, -1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, -1), new Vector3(1, -1, -1));
			AddCubeFace(surf.Geometry, 3, new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1));

			AddCubeFace(surf.Geometry, 4, new Vector3(-1, -1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1), new Vector3(-1, -1, -1));
			AddCubeFace(surf.Geometry, 5, new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 1));

			surf.Geometry.GenerateNormals = true;

			AddSurface(surf);
			FinishSurfaces();
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
			int surfaceCount = _surfaces.Count;

			for(int i = 0; i < surfaceCount; i++)
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