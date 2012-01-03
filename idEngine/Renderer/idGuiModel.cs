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
		public Color Color
		{
			set
			{
				if(idE.GLConfig.IsInitialized == false)
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
		private List<idVertex> _vertices = new List<idVertex>();
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
				viewDef.RenderView.Width = idE.ScreenWidth;
				viewDef.RenderView.Height = idE.ScreenHeight;

				viewDef.ViewPort = idE.RenderSystem.RenderViewToViewPort(viewDef.RenderView);

				viewDef.Scissor.X1 = 0;
				viewDef.Scissor.Y1 = 0;
				viewDef.Scissor.X2 = viewDef.ViewPort.X2 - viewDef.ViewPort.X1;
				viewDef.Scissor.Y2 = viewDef.ViewPort.Y2 - viewDef.ViewPort.Y1;
			}
			else
			{
				viewDef.RenderView.X = idE.RenderSystem.ViewDefinition.RenderView.X;
				viewDef.RenderView.Y = idE.RenderSystem.ViewDefinition.RenderView.Y;
				viewDef.RenderView.Width = idE.RenderSystem.ViewDefinition.RenderView.Width;
				viewDef.RenderView.Height = idE.RenderSystem.ViewDefinition.RenderView.Height;

				viewDef.ViewPort.X1 = viewDef.RenderView.X;
				viewDef.ViewPort.X2 = viewDef.RenderView.X + idE.RenderSystem.ViewDefinition.RenderView.Width;
				viewDef.ViewPort.Y1 = viewDef.RenderView.Y;
				viewDef.ViewPort.Y2 = viewDef.RenderView.Y += idE.RenderSystem.ViewDefinition.RenderView.Height;

				viewDef.Scissor.X1 = idE.RenderSystem.ViewDefinition.Scissor.X1;
				viewDef.Scissor.Y1 = idE.RenderSystem.ViewDefinition.Scissor.Y1;
				viewDef.Scissor.X2 = idE.RenderSystem.ViewDefinition.Scissor.X2;
				viewDef.Scissor.Y2 = idE.RenderSystem.ViewDefinition.Scissor.Y2;
			}

			viewDef.FloatTime = idE.RenderSystem.FrameShaderTime;

			// qglOrtho( 0, 640, 480, 0, 0, 1 );		// always assume 640x480 virtual coordinates
			viewDef.ProjectionMatrix = new Matrix();
			viewDef.ProjectionMatrix.M11 = 2.0f / 640.0f;
			viewDef.ProjectionMatrix.M22 = -2.0f / 480.0f;
			viewDef.ProjectionMatrix.M33 = -2.0f / 1.0f;
			viewDef.ProjectionMatrix.M41 = -1.0f;
			viewDef.ProjectionMatrix.M42 = 1.0f;
			viewDef.ProjectionMatrix.M43 = -1.0f;
			viewDef.ProjectionMatrix.M44 = 1.0f;

			viewDef.WorldSpace.ModelViewMatrix.M11 = 1.0f;
			viewDef.WorldSpace.ModelViewMatrix.M22 = 1.0f;
			viewDef.WorldSpace.ModelViewMatrix.M33 = 1.0f;
			viewDef.WorldSpace.ModelViewMatrix.M44 = 1.0f;

			viewDef.DrawSurfaces.Clear();

			View oldView = idE.RenderSystem.ViewDefinition;
			idE.RenderSystem.ViewDefinition = viewDef;

			// add the surfaces to this view
			for(int i = 0; i < _surfaces.Count; i++)
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
				s.Color = new Color(1, 1, 1, 1);
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
				return;	// nothing in the surface
			}

			// copy verts and indexes
			Surface tri = new Surface();
			tri.Indexes = new int[surface.IndexCount];
			tri.Vertices = new idVertex[surface.VertexCount];

			_indexes.CopyTo(surface.FirstIndex, tri.Indexes, 0, surface.IndexCount);

			// we might be able to avoid copying these and just let them reference the list vars
			// but some things, like deforms and recursive
			// guis, need to access the verts in cpu space, not just through the vertex range
			_vertices.CopyTo(surface.FirstVertex, tri.Vertices, 0, surface.VertexCount);

			// move the verts to the vertex cache
			// TODO: tri->ambientCache = vertexCache.AllocFrameTemp( tri->verts, tri->numVerts * sizeof( tri->verts[0] ) );

			// if we are out of vertex cache, don't create the surface
			/*if ( !tri->ambientCache ) {
				return;
			}*/

			RenderEntity renderEntity = new RenderEntity();
			renderEntity.Init();
			renderEntity.ShaderParameters[0] = surface.Color.R;
			renderEntity.ShaderParameters[1] = surface.Color.G;
			renderEntity.ShaderParameters[2] = surface.Color.B;
			renderEntity.ShaderParameters[3] = surface.Color.A;

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

	internal struct GuiModelSurface
	{
		public idMaterial Material;
		public Color Color;

		public int FirstVertex;
		public int VertexCount;

		public int FirstIndex;
		public int IndexCount;
	}
}
