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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace idTech4.Renderer
{
	public sealed class idGuiModel
	{
		#region Properties
		public Vector4 Color
		{
			set
			{
				if(idE.RenderSystem.IsRunning == false)
				{
					return;
				}

				if(value == _surface.Color)
				{
					return;
				}

				if(_surface.VertexCount > 0)
				{
					AdvanceSurface();
				}

				// change the parms
				_surface.Color = value;
			}
		}
		#endregion

		#region Members
		private GuiModelSurface _surface;

		private List<GuiModelSurface> _surfaces = new List<GuiModelSurface>();
		private List<int> _indexes = new List<int>();
		private List<Vertex> _vertices = new List<Vertex>();
		#endregion

		#region Constructor
		public idGuiModel()
		{
			Clear();
		}
		#endregion

		#region Methods
		#region Public
		public void Clear()
		{
			_surfaces.Clear();
			_indexes.Clear();
			_vertices.Clear();

			AdvanceSurface();
		}

		public void DrawStretchPicture(Vertex[] vertices, int[] indexes, idMaterial material, bool clip, float minX, float minY, float maxX, float maxY)
		{
			if((vertices == null) || (indexes == null) || (material == null))
			{
				return;
			}

			// break the current surface if we are changing to a new material
			if(material != _surface.Material)
			{
				if(_surface.VertexCount > 0)
				{
					AdvanceSurface();
				}

				_surface.Material = material;
				_surface.Material.EnsureNotPurged(); // in case it was a gui item started before a level change
			}

			// TODO: remove
			clip = false;

			// add the verts and indexes to the current surface
			if(clip == true)
			{
				idConsole.WriteLine("idGuiModle.DrawStretchPicture clip");

				/*int i, j;

		// FIXME:	this is grim stuff, and should be rewritten if we have any significant
		//			number of guis asking for clipping
		idFixedWinding w;
		for ( i = 0; i < indexCount; i += 3 ) {
			w.Clear();
			w.AddPoint(idVec5(dverts[dindexes[i]].xyz.x, dverts[dindexes[i]].xyz.y, dverts[dindexes[i]].xyz.z, dverts[dindexes[i]].st.x, dverts[dindexes[i]].st.y));
			w.AddPoint(idVec5(dverts[dindexes[i+1]].xyz.x, dverts[dindexes[i+1]].xyz.y, dverts[dindexes[i+1]].xyz.z, dverts[dindexes[i+1]].st.x, dverts[dindexes[i+1]].st.y));
			w.AddPoint(idVec5(dverts[dindexes[i+2]].xyz.x, dverts[dindexes[i+2]].xyz.y, dverts[dindexes[i+2]].xyz.z, dverts[dindexes[i+2]].st.x, dverts[dindexes[i+2]].st.y));

			for ( j = 0; j < 3; j++ ) {
				if ( w[j].x < min_x || w[j].x > max_x ||
					w[j].y < min_y || w[j].y > max_y ) {
					break;
				}
			}
			if ( j < 3 ) {
				idPlane p;
				p.Normal().y = p.Normal().z = 0.0f; p.Normal().x = 1.0f; p.SetDist( min_x );
				w.ClipInPlace( p );
				p.Normal().y = p.Normal().z = 0.0f; p.Normal().x = -1.0f; p.SetDist( -max_x );
				w.ClipInPlace( p );
				p.Normal().x = p.Normal().z = 0.0f; p.Normal().y = 1.0f; p.SetDist( min_y );
				w.ClipInPlace( p );
				p.Normal().x = p.Normal().z = 0.0f; p.Normal().y = -1.0f; p.SetDist( -max_y );
				w.ClipInPlace( p );
			}

			int	numVerts = verts.Num();
			verts.SetNum( numVerts + w.GetNumPoints(), false );
			for ( j = 0 ; j < w.GetNumPoints() ; j++ ) {
				idDrawVert *dv = &verts[numVerts+j];

				dv->xyz.x = w[j].x;
				dv->xyz.y = w[j].y;
				dv->xyz.z = w[j].z;
				dv->st.x = w[j].s;
				dv->st.y = w[j].t;
				dv->normal.Set(0, 0, 1);
				dv->tangents[0].Set(1, 0, 0);
				dv->tangents[1].Set(0, 1, 0);
			}
			surf->numVerts += w.GetNumPoints();

			for ( j = 2; j < w.GetNumPoints(); j++ ) {
				indexes.Append( numVerts - surf->firstVert );
				indexes.Append( numVerts + j - 1 - surf->firstVert );
				indexes.Append( numVerts + j - surf->firstVert );
				surf->numIndexes += 3;
			}
		}*/

			}
			else
			{
				int currentVertexCount = _vertices.Count;
				int currentIndexCount = _indexes.Count;
				int vertexCount = vertices.Length;
				int indexCount = indexes.Length;

				_surface.VertexCount += vertexCount;
				_surface.IndexCount += indexCount;

				for(int i = 0; i < indexCount; i++)
				{
					_indexes.Add(currentVertexCount + indexes[i] - _surface.FirstVertex);
				}

				_vertices.AddRange(vertices);
			}
		}

		public void DrawStretchPicture(float x, float y, float width, float height, float s, float t, float s2, float t2, idMaterial material)
		{
			Vertex[] vertices = new Vertex[4];
			int[] indexes = new int[6];

			if(material == null)
			{
				return;
			}

			// clip to edges, because the pic may be going into a guiShader
			// instead of full screen
			if(x < 0)
			{
				s += (s2 - s) * -x / width;
				width += x;
				x = 0;
			}

			if(y < 0)
			{
				t += (t2 - t) * -y / height;
				height += y;
				y = 0;
			}
			if((x + width) > 640)
			{
				s2 -= (s2 - s) * (x + width - 640) / width;
				width = 640 - x;
			}

			if((y + height) > 480)
			{
				t2 -= (t2 - t) * (y + height - 480) / height;
				height = 480 - y;
			}

			if((width <= 0) || (height <= 0))
			{
				// completely clipped away
				return;		
			}

			indexes[0] = 3;
			indexes[1] = 0;
			indexes[2] = 2;
			indexes[3] = 2;
			indexes[4] = 0;
			indexes[5] = 1;

			vertices[0].Position = new Vector3(x, y, 0);
			vertices[0].TextureCoordinates = new Vector2(s, t);
			vertices[0].Normal = new Vector3(0, 0, 1);

			// TODO: tangents
			/*vertices[0].tangents[0][0] = 1;
			vertices[0].tangents[0][1] = 0;
			vertices[0].tangents[0][2] = 0;
			vertices[0].tangents[1][0] = 0;
			vertices[0].tangents[1][1] = 1;
			vertices[0].tangents[1][2] = 0;*/


			vertices[1].Position = new Vector3(x + width, y, 0);
			vertices[1].TextureCoordinates = new Vector2(s2, t);
			vertices[1].Normal = new Vector3(0, 0, 1);
			/*vertices[1].tangents[0][0] = 1;
			vertices[1].tangents[0][1] = 0;
			vertices[1].tangents[0][2] = 0;
			vertices[1].tangents[1][0] = 0;
			vertices[1].tangents[1][1] = 1;
			vertices[1].tangents[1][2] = 0;*/

			vertices[2].Position = new Vector3(x + width, y + height, 0);
			vertices[2].TextureCoordinates = new Vector2(s2, t2);
			vertices[2].Normal = new Vector3(0, 0, 1);
			/*vertices[2].tangents[0][0] = 1;
			vertices[2].tangents[0][1] = 0;
			vertices[2].tangents[0][2] = 0;
			vertices[2].tangents[1][0] = 0;
			vertices[2].tangents[1][1] = 1;
			vertices[2].tangents[1][2] = 0;*/

			vertices[3].Position = new Vector3(x, y + height, 0);
			vertices[3].TextureCoordinates = new Vector2(s, t2);
			vertices[3].Normal = new Vector3(0, 0, 1);
			/*vertices[3].tangents[0][0] = 1;
			vertices[3].tangents[0][1] = 0;
			vertices[3].tangents[0][2] = 0;
			vertices[3].tangents[1][0] = 0;
			vertices[3].tangents[1][1] = 1;
			vertices[3].tangents[1][2] = 0;*/

			DrawStretchPicture(vertices, indexes, material, false, 0, 0, 640.0f, 480.0f);
		}

		/// <summary>
		/// Creates a view that covers the screen and emit the surfaces.
		/// </summary>
		public void EmitFullScreen()
		{
			if(_surfaces[0].VertexCount == 0)
			{
				return;
			}

			View viewDef = new View();

			// for gui editor
			if((idE.RenderSystem.ViewDefinition == null) || (idE.RenderSystem.ViewDefinition.IsEditor == false))
			{
				viewDef.RenderView.X = 0;
				viewDef.RenderView.Y = 0;
				viewDef.RenderView.Width = idE.VirtualScreenWidth;
				viewDef.RenderView.Height = idE.VirtualScreenHeight;

				viewDef.ViewPort = idE.RenderSystem.RenderViewToViewPort(viewDef.RenderView);

				viewDef.Scissor.X1 = 0;
				viewDef.Scissor.Y1 = 0;
				viewDef.Scissor.X2 = (short) (viewDef.ViewPort.X2 - viewDef.ViewPort.X1);
				viewDef.Scissor.Y2 = (short) (viewDef.ViewPort.Y2 - viewDef.ViewPort.Y1);
			}
			else
			{
				viewDef.RenderView.X = idE.RenderSystem.ViewDefinition.RenderView.X;
				viewDef.RenderView.Y = idE.RenderSystem.ViewDefinition.RenderView.Y;
				viewDef.RenderView.Width = idE.RenderSystem.ViewDefinition.RenderView.Width;
				viewDef.RenderView.Height = idE.RenderSystem.ViewDefinition.RenderView.Height;

				viewDef.ViewPort.X1 = (short) viewDef.RenderView.X;
				viewDef.ViewPort.X2 = (short) (viewDef.RenderView.X + idE.RenderSystem.ViewDefinition.RenderView.Width);
				viewDef.ViewPort.Y1 = (short) viewDef.RenderView.Y;
				viewDef.ViewPort.Y2 = (short) (viewDef.RenderView.Y += idE.RenderSystem.ViewDefinition.RenderView.Height);

				viewDef.Scissor.X1 = idE.RenderSystem.ViewDefinition.Scissor.X1;
				viewDef.Scissor.Y1 = idE.RenderSystem.ViewDefinition.Scissor.Y1;
				viewDef.Scissor.X2 = idE.RenderSystem.ViewDefinition.Scissor.X2;
				viewDef.Scissor.Y2 = idE.RenderSystem.ViewDefinition.Scissor.Y2;
			}

			Vector2 center = new Vector2(idE.VirtualScreenWidth * 0.5f, idE.VirtualScreenHeight * 0.5f);

			viewDef.FloatTime = idE.RenderSystem.FrameShaderTime;
			viewDef.ProjectionMatrix = Matrix.CreateOrthographic(idE.VirtualScreenWidth, idE.VirtualScreenHeight, -0.5f, 1);
			viewDef.WorldSpace.ModelViewMatrix =  Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));

			View oldView = idE.RenderSystem.ViewDefinition;
			idE.RenderSystem.ViewDefinition = viewDef;

			// add the surfaces to this view
			int count = _surfaces.Count;

			for(int i = 0; i < count; i++)
			{
				EmitSurface(_surfaces[i], viewDef.WorldSpace.ModelMatrix, viewDef.WorldSpace.ModelViewMatrix, false);
			}

			idE.RenderSystem.ViewDefinition = oldView;

			// add the command to draw this view
			idE.RenderSystem.AddDrawViewCommand(viewDef);
		}
		#endregion

		#region Private
		private void AdvanceSurface()
		{
			GuiModelSurface s = new GuiModelSurface();

			if(_surfaces.Count > 0)
			{
				s.Color = _surface.Color;
				s.Material = _surface.Material;
			}
			else
			{
				s.Color = new Vector4(1, 1, 1, 1);
				s.Material = idE.RenderSystem.DefaultMaterial;
			}

			s.IndexCount = 0;
			s.FirstIndex = _indexes.Count;
			s.VertexCount = 0;
			s.FirstVertex = _vertices.Count;

			_surfaces.Add(s);
			_surface = s;
		}

		private void EmitSurface(GuiModelSurface surface, Matrix modelMatrix, Matrix modelViewMatrix, bool depthHack)
		{
			if(surface.VertexCount == 0)
			{
				// nothing in the surface
				return;
			}

			// copy verts and indexes
			Surface tri = new Surface();
			tri.Indexes = new int[surface.IndexCount];
			tri.Vertices = new Vertex[surface.VertexCount];

			_indexes.CopyTo(surface.FirstIndex, tri.Indexes, 0, surface.IndexCount);

			// we might be able to avoid copying these and just let them reference the list vars
			// but some things, like deforms and recursive
			// guis, need to access the verts in cpu space, not just through the vertex range
			_vertices.CopyTo(surface.FirstVertex, tri.Vertices, 0, surface.VertexCount);

			// move the verts to the vertex cache
			tri.AmbientCache = idE.RenderSystem.AllocateVertexCacheFrameTemporary(tri.Vertices);

			// if we are out of vertex cache, don't create the surface
			if(tri.AmbientCache == null)
			{
				return;
			}

			RenderEntityComponent renderEntity = new RenderEntityComponent();
			renderEntity.MaterialParameters[0] = surface.Color.X;
			renderEntity.MaterialParameters[1] = surface.Color.Y;
			renderEntity.MaterialParameters[2] = surface.Color.Z;
			renderEntity.MaterialParameters[3] = surface.Color.W;

			ViewEntity guiSpace = new ViewEntity();
			guiSpace.ModelMatrix = modelMatrix;
			guiSpace.ModelViewMatrix = modelViewMatrix;
			guiSpace.WeaponDepthHack = depthHack;

			// add the surface, which might recursively create another gui
			idE.RenderSystem.AddDrawSurface(tri, guiSpace, renderEntity, surface.Material, idE.RenderSystem.ViewDefinition.Scissor);
		}
		#endregion
		#endregion
	}

	internal class GuiModelSurface
	{
		public idMaterial Material;
		public Vector4 Color;

		public int FirstVertex;
		public int VertexCount;

		public int FirstIndex;
		public int IndexCount;
	}
}