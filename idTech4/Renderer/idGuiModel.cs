/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using idTech4.Services;

namespace idTech4.Renderer
{
	public class idGuiModel
	{
		#region Members
		private GuiModelSurface _surface;
		private float[] _materialParameters = new float[Constants.MaxEntityMaterialParameters];

		private DynamicVertexBuffer _vertexBuffer;
		private DynamicIndexBuffer _indexBuffer;

		private List<GuiModelSurface> _surfaces = new List<GuiModelSurface>();
		private int _vertexCount;
		private int _indexCount;

		private int _warningFrame;
		#endregion

		#region Constructor
		public idGuiModel()
		{
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();
			_vertexBuffer              = renderSystem.CreateDynamicVertexBuffer(idVertex.VertexDeclaration, renderSystem.Capabilities.MaxVertexBufferElements, BufferUsage.WriteOnly);			
			_indexBuffer               = renderSystem.CreateDynamicIndexBuffer(IndexElementSize.SixteenBits, renderSystem.Capabilities.MaxIndexBufferElements, BufferUsage.WriteOnly);
			
			// identity color for drawsurf register evaluation
			for(int i = 0; i < Constants.MaxEntityMaterialParameters; i++)
			{
				_materialParameters[i] = 1.0f;
			}

			Clear();
		}
		#endregion

		#region Methods
		#region Rendering
		public void AddPrimitive(idVertex[] vertices, ushort[] indexes, idMaterial material, ulong state, StereoDepthType stereoType)
		{
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();

			if(material == null)
			{
				return;
			}
			else if((vertices.Length == 0) || (indexes.Length == 0))
			{
				return;
			}

			if((indexes.Length + _indexCount) > Constants.MaxGuiIndexes)
			{
				if(_warningFrame != renderSystem.FrameCount)
				{
					_warningFrame = renderSystem.FrameCount;
					idLog.Warning("idGuiModel.AddPrimitive: MAX_INDEXES exceeded");
				}

				return;
			}

			if((vertices.Length + _vertexCount) > Constants.MaxGuiVertices)
			{
				if(_warningFrame != renderSystem.FrameCount)
				{
					_warningFrame = renderSystem.FrameCount;
					idLog.Warning("idGuiModel.AddPrimitive: MAX_VERTS exceeded");
				}

				return;
			}

			// break the current surface if we are changing to a new material or we can't
			// fit the data into our allocated block
			if((material != _surface.Material) || (state != _surface.State) /* TODO: || stereoType != surf->stereoType*/)
			{
				if(_surface.IndexCount > 0)
				{
					AdvanceSurface();
				}

				_surface.Material   = material;
				_surface.State      = state;
				_surface.StereoType = stereoType;
			}

			int startVertex = _vertexCount;
			int startIndex  = _indexCount;

			_vertexBuffer.SetData<idVertex>(_vertexCount * idVertex.VertexDeclaration.VertexStride, vertices, 0, vertices.Length, idVertex.VertexDeclaration.VertexStride, SetDataOptions.NoOverwrite);
			_indexBuffer.SetData<ushort>(_indexCount * sizeof(ushort), indexes, 0, indexes.Length, SetDataOptions.NoOverwrite);

			_vertexCount         += vertices.Length;
			_indexCount          += indexes.Length;
			_surface.IndexCount  += indexes.Length;
			_surface.VertexCount += vertices.Length;
		}

		public void BeginFrame()
		{			
			//_vertexCount = 0;
			//_indexCount  = 0;

			Clear();
		}

		/// <summary>
		/// Creates a view that covers the screen and emit the surfaces.
		/// </summary>
		public void EmitFullscreen()
		{
			if(_surfaces[0].IndexCount == 0)
			{
				return;
			}

			// TODO: SCOPED_PROFILE_EVENT( "Gui::EmitFullScreen" );

			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();

			idViewDefinition viewDef   = new idViewDefinition();
			viewDef.Is2DGui            = true;
			viewDef.Viewport           = renderSystem.GetCroppedViewport();

			// TODO: stereo
			bool stereoEnabled = false; /*( renderSystem->GetStereo3DMode() != STEREO3D_OFF );
			if ( stereoEnabled ) {
				float	GetScreenSeparationForGuis();
				const float screenSeparation = GetScreenSeparationForGuis();

				// this will be negated on the alternate eyes, both rendered each frame
				viewDef->renderView.stereoScreenSeparation = screenSeparation;

				extern idCVar stereoRender_swapEyes;
				viewDef->renderView.viewEyeBuffer = 0;	// render to both buffers
				if ( stereoRender_swapEyes.GetBool() ) {
					viewDef->renderView.stereoScreenSeparation = -screenSeparation;
				}
			}*/

			viewDef.Scissor.X1       = 0;
			viewDef.Scissor.Y1       = 0;
			viewDef.Scissor.X2       = (short) (viewDef.Viewport.X2 - viewDef.Viewport.X1);
			viewDef.Scissor.Y2       = (short) (viewDef.Viewport.Y2 - viewDef.Viewport.Y1);
			
			viewDef.ProjectionMatrix = Matrix.CreateOrthographic(Constants.ScreenWidth, Constants.ScreenHeight, -1, 1);
			
			// make a tech5 renderMatrix for faster culling
			// TODO: idRenderMatrix::Transpose( *(idRenderMatrix *)viewDef->projectionMatrix, viewDef->projectionRenderMatrix );

			Vector2 center = new Vector2(Constants.ScreenWidth * 0.5f, Constants.ScreenHeight * 0.5f);

			viewDef.WorldSpace.ModelMatrix     = Matrix.Identity;
			viewDef.WorldSpace.ModelViewMatrix = Matrix.CreateLookAt(new Vector3(center, 0), new Vector3(center, 1), new Vector3(0, -1, 0));

			idViewDefinition oldViewDef = renderSystem.ViewDefinition;
			renderSystem.ViewDefinition = viewDef;

			EmitSurfaces(viewDef.WorldSpace.ModelMatrix, viewDef.WorldSpace.ModelViewMatrix, false /* depthHack */ , stereoEnabled /* stereoDepthSort */, false /* link as entity */ );

			renderSystem.ViewDefinition = oldViewDef;

			// add the command to draw this view
			renderSystem.DrawView(viewDef, true);
		}

		/// <summary>
		/// For full screen GUIs, we can add in per-surface stereoscopic depth effects.
		/// </summary>
		/// <param name="modelMatrix"></param>
		/// <param name="modelViewMatrix"></param>
		/// <param name="depthHack"></param>
		/// <param name="allowFullscreenStereoDepth"></param>
		/// <param name="linkAsEntity"></param>
		private void EmitSurfaces(Matrix modelMatrix, Matrix modelViewMatrix, bool depthHack, bool allowFullscreenStereoDepth, bool linkAsEntity)
		{
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();
			ICVarSystem cvarSystem     = idEngine.Instance.GetService<ICVarSystem>();

			idViewEntity guiSpace    = new idViewEntity();
			guiSpace.ModelMatrix     = modelMatrix;
			guiSpace.ModelViewMatrix = modelViewMatrix;
			guiSpace.WeaponDepthHack = depthHack;
			guiSpace.IsGuiSurface    = true;

			// if this is an in-game gui, we need to be able to find the matrix again for head mounted
			// display bypass matrix fixup.
			if(linkAsEntity == true) 
			{
				guiSpace.Next                            = renderSystem.ViewDefinition.ViewEntities;
				renderSystem.ViewDefinition.ViewEntities = guiSpace;
			}

			//---------------------------
			// make a tech5 renderMatrix
			//---------------------------
			// TODO: idRenderMatrix
			/*idRenderMatrix viewMat;
			idRenderMatrix::Transpose( *(idRenderMatrix *)modelViewMatrix, viewMat );
			idRenderMatrix::Multiply( tr.viewDef->projectionRenderMatrix, viewMat, guiSpace->mvp );
			if ( depthHack ) {
				idRenderMatrix::ApplyDepthHack( guiSpace->mvp );
			}*/

			// to allow 3D-TV effects in the menu system, we define surface flags to set
			// depth fractions between 0=screen and 1=infinity, which directly modulate the
			// screenSeparation parameter for an X offset.
			// the value is stored in the drawSurf sort value, which adjusts the matrix in the
			// backend.
			float defaultStereoDepth = cvarSystem.GetFloat("stereoRender_defaultGuiDepth"); // default to at-screen

			// add the surfaces to this view
			for(int i = 0; i < _surfaces.Count; i++ ) 
			{
				GuiModelSurface guiSurface = _surfaces[i];

				if(guiSurface.IndexCount == 0)
				{
					continue;
				}

				idMaterial material       = guiSurface.Material;
								
				idDrawSurface drawSurface = new idDrawSurface();
				drawSurface.FirstIndex    = guiSurface.FirstIndex;
				drawSurface.IndexCount    = guiSurface.IndexCount;
				drawSurface.FirstVertex   = guiSurface.FirstVertex;
				drawSurface.VertexCount   = guiSurface.VertexCount;
				drawSurface.VertexBuffer  = _vertexBuffer;
				drawSurface.IndexBuffer   = _indexBuffer;
				drawSurface.Space         = guiSpace;
				drawSurface.Material      = material;
				drawSurface.ExtraState    = guiSurface.State;
				drawSurface.Scissor       = renderSystem.ViewDefinition.Scissor;
				drawSurface.Sort          = material.Sort;
				drawSurface.RenderZFail   = 0;

				// process the shader expressions for conditionals / color / texcoords
				float[] constantRegisters = material.ConstantRegisters;

				if(constantRegisters != null)
				{
					// shader only uses constant values
					drawSurface.MaterialRegisters = constantRegisters;
				} 
				else 
				{
					float[] registers             = new float[material.RegisterCount];
					drawSurface.MaterialRegisters = registers;

					material.EvaluateRegisters(registers, _materialParameters, renderSystem.ViewDefinition.RenderView.MaterialParameters, renderSystem.ViewDefinition.RenderView.Time[1] * 0.001f, null);
				}
				
				if(allowFullscreenStereoDepth == true) 
				{
					// override sort with the stereoDepth
					//drawSurf->sort = stereoDepth;

					// TODO: stereo
					/*switch ( guiSurf.stereoType ) {
					case STEREO_DEPTH_TYPE_NEAR: drawSurf->sort = STEREO_DEPTH_NEAR; break;
					case STEREO_DEPTH_TYPE_MID: drawSurf->sort = STEREO_DEPTH_MID; break;
					case STEREO_DEPTH_TYPE_FAR: drawSurf->sort = STEREO_DEPTH_FAR; break;
					case STEREO_DEPTH_TYPE_NONE:
					default:*/
						drawSurface.Sort = defaultStereoDepth;
						/*break;
					}*/
				}

				renderSystem.ViewDefinition.DrawSurfaces.Add(drawSurface);
			}
		}
		#endregion

		#region Surface Management
		/// <summary>
		/// Begins collecting draw commands into surfaces.
		/// </summary>
		public void Clear()
		{
			_surfaces.Clear();

			AdvanceSurface();
		}

		private void AdvanceSurface()
		{
			GuiModelSurface s = new GuiModelSurface();

			if(_surfaces.Count > 0)
			{
				s.Material = _surface.Material;
				s.State = _surface.State;
			}
			else
			{
				s.Material = idEngine.Instance.GetService<IRenderSystem>().DefaultMaterial;
				s.State = 0;
			}

			s.IndexCount  = 0;
			s.FirstIndex  = _indexCount;

			s.VertexCount = 0;
			s.FirstVertex = _vertexCount;

			_surfaces.Add(s);
			_surface = s;
		}
		#endregion
		#endregion
	}

	internal class GuiModelSurface
	{
		public idMaterial Material;
		public ulong State;

		public int FirstIndex;
		public int IndexCount;

		public int FirstVertex;
		public int VertexCount;

		public StereoDepthType StereoType;
	}
}