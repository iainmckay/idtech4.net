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

using Tao.OpenGl;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace idTech4.Renderer.Backends
{
	public class XNARenderBackend : idRenderBackendInterface
	{
		#region Members
		private GraphicsDevice _graphicsDevice;
		private GraphicsDeviceManager _graphicsDeviceManager;

		private BasicEffect _effect;

		private int _frameCount;

		private View _viewDef;
		private ViewEntity _currentSpace; // for detecting when a matrix must change.
		private idScreenRect _currentScissor;  // for scissor clipping, local inside renderView viewport.
		private MaterialStates _depthFunction; // GLS_DEPTHFUNC_EQUAL, or GLS_DEPTHFUNC_LESS for translucent.

		private bool _currentRenderCopied; // true if any material has already referenced CurrentRender.

		private State _state = new State(); // state deltas.

		private Vector2 _viewPortOffset;									// for doing larger-than-window tiled renderings
		private Vector2 _tiledViewPort;

		private int _stencilIncrement;
		private int _stencilDecrement;

		private float _maxLights = 999;								// 1.0 for standard, unlimited for floats
		#endregion

		#region Methods
		#region Frame control
		/// <summary>
		/// Any mirrored or portaled views have already been drawn, so prepare
		/// to actually render the visible surfaces for this view.
		/// </summary>
		private void BeginDrawingView()
		{
			Vector2 viewPortOffset = idE.RenderSystem.ViewPortOffset;

			// set the window clipping
			_graphicsDevice.Viewport = new Viewport(
				(int) viewPortOffset.X + _viewDef.ViewPort.X1,
				(int) viewPortOffset.Y + _viewDef.ViewPort.Y1,
				_viewDef.ViewPort.X2 + 1 - _viewDef.ViewPort.X1,
				_viewDef.ViewPort.Y2 + 1 - _viewDef.ViewPort.Y1);

			// the scissor may be smaller than the viewport for subviews
			_graphicsDevice.ScissorRectangle = new Rectangle(
				(int) viewPortOffset.X + _viewDef.ViewPort.X1 + _viewDef.Scissor.X1,
				(int) viewPortOffset.Y + _viewDef.ViewPort.Y1 + _viewDef.Scissor.Y1,
				_viewDef.Scissor.X2 + 1 - _viewDef.Scissor.X1,
				_viewDef.Scissor.Y2 + 1 - _viewDef.Scissor.Y1);

			_currentScissor = _viewDef.Scissor;

			// ensures that depth writes are enabled for the depth clear
			SetState(MaterialStates.DepthFunctionAlways);

			// we don't have to clear the depth / stencil buffer for 2D rendering
			if(_viewDef.ViewEntities.Count > 0)
			{
				idConsole.Warning("TODO: stencil mask");

				/*Gl.glStencilMask(0xFF);
				// some cards may have 7 bit stencil buffers, so don't assume this
				// should be 128
				Gl.glClearStencil(1 << (glConfig.stencilBits - 1));
				Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT | Gl.GL_STENCIL_BUFFER_BIT);
				Gl.glEnable(Gl.GL_DEPTH_TEST);*/
			}
			else
			{
				DepthStencilState s = new DepthStencilState();
				s.DepthBufferEnable = false;
				s.StencilEnable = false;

				_graphicsDevice.DepthStencilState = s;
			}
			
			_state.FaceCulling = CullType.None; // force face culling to set next time

			SetCull(CullType.Front);
		}
		#endregion

		#region Element drawing
		private void DrawElementsWithCounters(Surface tri)
		{
			// TODO: performance counters
			/*backEnd.pc.c_drawElements++;
			backEnd.pc.c_drawIndexes += tri->numIndexes;
			backEnd.pc.c_drawVertexes += tri->numVerts;*/

			/*if ( tri->ambientSurface != NULL  ) {
				if ( tri->indexes == tri->ambientSurface->indexes ) {
					backEnd.pc.c_drawRefIndexes += tri->numIndexes;
				}
				if ( tri->verts == tri->ambientSurface->verts ) {
					backEnd.pc.c_drawRefVertexes += tri->numVerts;
				}
			}*/

			if((tri.IndexCache != null) && (idE.CvarSystem.GetBool("r_useIndexBuffers") == true))
			{
				idConsole.Warning("TODO: indexCache");
				/*Gl.glDrawElements(Gl.GL_TRIANGLES,
					(idE.CvarSystem.GetBool("r_singleTriangle") == true) ? 3 : tri.Indexes.Length,
					Gl.GL_INDEX_ARRAY_TYPE,
					(int*) vertexCache.Position(tri->indexCache));*/

				// TODO: backEnd.pc.c_vboIndexes += tri->numIndexes;
			}
			else
			{
				if(idE.CvarSystem.GetBool("r_useIndexBuffers") == true)
				{
					UnbindIndex();
				}

				TextureUnit textureUnit = _state.TextureUnits[_state.CurrentTextureUnit];
				Texture texture = textureUnit.CurrentTexture;

				if(texture != null)
				{
					_effect.Texture = (Texture2D) texture;
					_effect.TextureEnabled = true;

					switch(textureUnit.Filter)
					{
						case TextureFilter.Default:
							switch(textureUnit.Repeat)
							{
								case TextureRepeat.Repeat:
									_graphicsDevice.SamplerStates[0] = idE.ImageManager.DefaultRepeatTextureSampler;
									break;

								default:
									_graphicsDevice.SamplerStates[0] = idE.ImageManager.DefaultClampTextureSampler;
									break;
							}
							break;

						case TextureFilter.Linear:
							switch(textureUnit.Repeat)
							{
								case TextureRepeat.Repeat:
									_graphicsDevice.SamplerStates[0] = idE.ImageManager.LinearRepeatTextureSampler;
									break;

								default:
									_graphicsDevice.SamplerStates[0] = idE.ImageManager.LinearClampTextureSampler;
									break;
							}
							break;

						case TextureFilter.Nearest:
							switch(textureUnit.Repeat)
							{
								case TextureRepeat.Repeat:
									_graphicsDevice.SamplerStates[0] = idE.ImageManager.LinearRepeatTextureSampler;
									break;

								default:
									_graphicsDevice.SamplerStates[0] = idE.ImageManager.LinearClampTextureSampler;
									break;
							}
							break;
					}
				}
				else
				{
					_effect.TextureEnabled = false;
				}

				//_effect.View = Matrix.CreateLookAt(new Vector3(128, 64, 0), Vector3.Zero, Vector3.Up);
				//_basicEffect.View = Matrix.CreateTranslation(128, 64, 0);
				_effect.Projection = _viewDef.ProjectionMatrix;
				_effect.World = Matrix.Identity; // _drawSurfModelViewMatrix;

				foreach(EffectPass p in _effect.CurrentTechnique.Passes)
				{
					p.Apply();

					_graphicsDevice.DrawUserIndexedPrimitives<Vertex>(PrimitiveType.TriangleList,
						tri.AmbientCache.Data, 0,
						tri.AmbientCache.Data.Length,
						tri.Indexes, 0, tri.Indexes.Length / 3);
				}
			}
		}

		/// <summary>
		/// Draw non-light dependent passes.
		/// </summary>
		/// <param name="surfaces"></param>
		/// <returns></returns>
		private int DrawMaterialPasses(DrawSurface[] surfaces)
		{
			// only obey skipAmbient if we are rendering a view
			if((_viewDef.ViewEntities.Count > 0) && (idE.CvarSystem.GetBool("r_skipAmbient") == true))
			{
				return surfaces.Length;
			}

			// RB_LogComment( "---------- RB_STD_DrawShaderPasses ----------\n" );

			// if we are about to draw the first surface that needs
			// the rendering in a texture, copy it over
			if(surfaces[0].Material.Sort >= (float) MaterialSort.PostProcess)
			{
				idConsole.Warning("TODO: PostProcess");
				/*if ( r_skipPostProcess.GetBool() ) {
					return 0;
				}

				// only dump if in a 3d view
				if ( backEnd.viewDef->viewEntitys && tr.backEndRenderer == BE_ARB2 ) {
					globalImages->currentRenderImage->CopyFramebuffer( backEnd.viewDef->viewport.x1,
						backEnd.viewDef->viewport.y1,  backEnd.viewDef->viewport.x2 -  backEnd.viewDef->viewport.x1 + 1,
						backEnd.viewDef->viewport.y2 -  backEnd.viewDef->viewport.y1 + 1, true );
				}
				backEnd.currentRenderCopied = true;*/
			}

			SetTextureUnit(1);
			idE.ImageManager.BindNullTexture();

			SetTextureUnit(0);
			//Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

			SetProgramEnvironment();

			// we don't use RB_RenderDrawSurfListWithFunction()
			// because we want to defer the matrix load because many
			// surfaces won't draw any ambient passes
			// TODO: backEnd.currentSpace = NULL;
			int i;
			int surfaceCount = surfaces.Length;

			for(i = 0; i < surfaceCount; i++)
			{
				DrawSurface surface = surfaces[i];

				// TODO: suppressInSubview
				/*if ( drawSurfs[i]->material->SuppressInSubview() ) {
					continue;
				}*/

				// TODO
				/*if ( backEnd.viewDef->isXraySubview && drawSurfs[i]->space->entityDef ) {
					if ( drawSurfs[i]->space->entityDef->parms.xrayIndex != 2 ) {
						continue;
					}
				}
				*/

				// we need to draw the post process shaders after we have drawn the fog lights
				if((surface.Material.Sort >= (float) MaterialSort.PostProcess) && (_currentRenderCopied == false))
				{
					break;
				}

				RenderMaterialPasses(surface);
			}

			SetCull(CullType.TwoSided);
			// TODO: Gl.glColor3f(1, 1, 1);

			return i;
		}

		private void DrawView()
		{
			// TODO: RB_LogComment( "---------- RB_STD_DrawView ----------\n" );

			_depthFunction = MaterialStates.DepthFunctionEqual;

			DrawSurface[] surfaces = _viewDef.DrawSurfaces.ToArray();
			int surfaceCount = surfaces.Length;

			// clear the z buffer, set the projection matrix, etc
			BeginDrawingView();

			// decide how much overbrighting we are going to do
			// TODO: RB_DetermineLightScale();

			// fill the depth buffer and clear color buffer to black except on
			// subviews
			FillDepthBuffer(surfaces);

			// main light renderer
			/*switch( tr.backEndRenderer ) {
			case BE_ARB:
				RB_ARB_DrawInteractions();
				break;
			case BE_ARB2:
				RB_ARB2_DrawInteractions();
				break;
			case BE_NV20:
				RB_NV20_DrawInteractions();
				break;
			case BE_NV10:
				RB_NV10_DrawInteractions();
				break;
			case BE_R200:
				RB_R200_DrawInteractions();
				break;
			}*/

			// disable stencil shadow test			
			//Gl.glStencilFunc(Gl.GL_ALWAYS, 128, 255);

			// uplight the entire screen to crutch up not having better blending range
			// TODO: RB_STD_LightScale();

			// now draw any non-light dependent shading passes
			int processed = DrawMaterialPasses(surfaces);

			// fob and blend lights
			// TODO: RB_STD_FogAllLights();

			// now draw any post-processing effects using _currentRender
			if(processed < surfaceCount)
			{
				idConsole.Warning("TODO: RB_STD_DrawShaderPasses( drawSurfs+processed, numDrawSurfs-processed );");
			}

			/*RB_RenderDebugTools( drawSurfs, numDrawSurfs );*/
		}

		/// <summary>
		/// The triangle functions can check backEnd.currentSpace != surf->space
		/// to see if they need to perform any new matrix setup.  The modelview
		/// matrix will already have been loaded, and backEnd.currentSpace will
		/// be updated after the triangle function completes.
		/// </summary>
		/// <param name="surfaces"></param>
		/// <param name="handler"></param>
		private void RenderDrawSurfaceListWithFunction(DrawSurface[] surfaces, RenderHandler handler)
		{
			int count = surfaces.Length;

			for(int i = 0; i < count; i++)
			{
				DrawSurface surface = surfaces[i];

				// change the matrix if needed
				if(surface.Space != _currentSpace)
				{
					_effect.View = surface.Space.ModelViewMatrix;
				}

				if(surface.Space.WeaponDepthHack == true)
				{
					idConsole.Warning("TODO: RB_EnterWeaponDepthHack();");
				}

				if(surface.Space.ModelDepthHack != 0.0f)
				{
					idConsole.Warning("TODO: RB_EnterModelDepthHack( drawSurf->space->modelDepthHack );");
				}

				// change the scissor if needed
				if((idE.CvarSystem.GetBool("r_useScissor") == true) && (_currentScissor != surface.ScissorRectangle))
				{
					_currentScissor = surface.ScissorRectangle;

					_graphicsDevice.ScissorRectangle = new Rectangle(
						_viewDef.ViewPort.X1 + _currentScissor.X1,
						_viewDef.ViewPort.Y1 + _currentScissor.Y1,
						_currentScissor.X2 + 1 - _currentScissor.X1,
						_currentScissor.Y2 + 1 - _currentScissor.Y1);
				}

				// render it
				handler(surface);

				if((surface.Space.WeaponDepthHack == true) || (surface.Space.ModelDepthHack != 0.0f))
				{
					idConsole.Warning("TODO: RB_LeaveDepthHack();");
				}

				_currentSpace = surface.Space;
			}
		}

		private void RenderMaterialPasses(DrawSurface surface)
		{
			Surface tri = surface.Geometry;
			idMaterial material = surface.Material;
			int count;

			if(material.HasAmbient == false)
			{
				// disabled because we don't do lighting right now
				//TODO: return;
			}

			if(material.IsPortalSky == true)
			{
				return;
			}

			// change the matrix if needed
			if(surface.Space != _currentSpace)
			{
				_effect.View = surface.Space.ModelViewMatrix;
				_currentSpace = surface.Space;
				//idConsole.Warning("TODO: RB_SetProgramEnvironmentSpace();");
			}

			// change the scissor if needed
			if((idE.CvarSystem.GetBool("r_useScissor") == true) && (_currentScissor != surface.ScissorRectangle))
			{
				_currentScissor = surface.ScissorRectangle;

				_graphicsDevice.ScissorRectangle = new Rectangle(
					_viewDef.ViewPort.X1 + _currentScissor.X1,
					_viewDef.ViewPort.Y1 + _currentScissor.Y1,
					_currentScissor.X2 + 1 - _currentScissor.X1,
					_currentScissor.Y2 + 1 - _currentScissor.Y1);
			}

			// some deforms may disable themselves by setting numIndexes = 0
			if(tri.Indexes.Length == 0)
			{
				return;
			}

			if(tri.AmbientCache == null)
			{
				idConsole.WriteLine("RenderShaderPasses: !tri.AmbientCache");
				return;
			}

			// get the expressions for conditionals / color / texcoords
			float[] registers = surface.MaterialRegisters;

			// set face culling appropriately
			SetCull(material.CullType);

			// set polygon offset if necessary
			if(material.TestMaterialFlag(MaterialFlags.PolygonOffset) == true)
			{
				idConsole.Warning("TODO: polygon offset fill");
				//Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
				//Gl.glPolygonOffset(idE.CvarSystem.GetFloat("r_offsetFactor"), idE.CvarSystem.GetFloat("r_offsetUnits") * material.PolygonOffset);
			}

			if(surface.Space.WeaponDepthHack == true)
			{
				idConsole.Warning("TODO: RB_EnterWeaponDepthHack();");
			}

			if(surface.Space.ModelDepthHack != 0.0f)
			{
				idConsole.Warning("TODO: RB_EnterModelDepthHack( surf->space->modelDepthHack );");
			}

			foreach(MaterialStage stage in material.Stages)
			{
				// check the enable condition
				if(registers[stage.ConditionRegister] == 0)
				{
					continue;
				}

				// skip the stages involved in lighting
				if(stage.Lighting != StageLighting.Ambient)
				{
					// disabled because we don't do lighting right now
					// TODO: continue;
				}

				// skip if the stage is ( GL_ZERO, GL_ONE ), which is used for some alpha masks
				if((stage.DrawStateBits & (MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits))
					== (MaterialStates.SourceBlendZero | MaterialStates.DestinationBlendOne))
				{
					continue;
				}

				// see if we are a new-style stage
				NewMaterialStage newStage = stage.NewStage;

				if(newStage.IsEmpty == false)
				{
					throw new Exception("THIS MIGHT NOT WORK!!!");
					//--------------------------
					//
					// new style stages
					//
					//--------------------------

					if(idE.CvarSystem.GetBool("r_skipNewAmbient") == true)
					{
						continue;
					}

					idConsole.Warning("TODO: render");
					/*Gl.glColorPointer(4, Gl.GL_UNSIGNED_BYTE, Marshal.SizeOf(typeof(Vertex)), (void*) &ambientCacheData->color);
					Gl.glVertexAttribPointerARB(9, 3, Gl.GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), ambientCacheData->tangents[0].ToFloatPtr());
					Gl.glVertexAttribPointerARB(10, 3, Gl.GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), ambientCacheData->tangents[1].ToFloatPtr());
					Gl.glNormalPointer(Gl.GL_FLOAT, Marshal.SizeOf(typeof(Vertex)), ambientCacheData->normal.ToFloatPtr());*/

					//Gl.glEnableClientState(Gl.GL_COLOR_ARRAY);
					//Gl.glEnableVertexAttribArray(9);
					//Gl.glEnableVertexAttribArray(10);
					//Gl.glEnableClientState(Gl.GL_NORMAL_ARRAY);

					SetState(stage.DrawStateBits);

					idConsole.Warning("TODO: glBindProgramARB");
					/*Gl.glBindProgramARB(Gl.GL_VERTEX_PROGRAM_ARB, newStage.VertexProgram);
					Gl.glEnable(Gl.GL_VERTEX_PROGRAM_ARB);*/

					// megaTextures bind a lot of images and set a lot of parameters
					// TODO: megatextures
					/*if ( newStage->megaTexture ) {
						newStage->megaTexture->SetMappingForSurface( tri );
						idVec3	localViewer;
						R_GlobalPointToLocal( surf->space->modelMatrix, backEnd.viewDef->renderView.vieworg, localViewer );
						newStage->megaTexture->BindForViewOrigin( localViewer );
					}*/

					count = newStage.VertexParameters.Length;

					for(int i = 0; i < count; i++)
					{
						float[] parm = new float[4];
						parm[0] = registers[newStage.VertexParameters[i, 0]];
						parm[1] = registers[newStage.VertexParameters[i, 1]];
						parm[2] = registers[newStage.VertexParameters[i, 2]];
						parm[3] = registers[newStage.VertexParameters[i, 3]];

						//Gl.glProgramLocalParameter4fvARB(Gl.GL_VERTEX_PROGRAM_ARB, i, parm);
					}

					count = newStage.FragmentProgramImages.Length;

					for(int i = 0; i < count; i++)
					{
						if(newStage.FragmentProgramImages[i] != null)
						{
							SetTextureUnit(i);
							newStage.FragmentProgramImages[i].Bind();
						}
					}

					//Gl.glBindProgramARB(Gl.GL_FRAGMENT_PROGRAM_ARB, newStage.FragmentProgram);
					//Gl.glEnable(Gl.GL_FRAGMENT_PROGRAM_ARB);

					// draw it
					DrawElementsWithCounters(tri);

					count = newStage.FragmentProgramImages.Length;

					for(int i = 1; i < count; i++)
					{
						if(newStage.FragmentProgramImages[i] != null)
						{
							SetTextureUnit(i);
							idE.ImageManager.BindNullTexture();
						}
					}

					// TODO: megatexture
					/*if ( newStage->megaTexture ) {
						newStage->megaTexture->Unbind();
					}*/

					SetTextureUnit(0);

					//Gl.glDisable(Gl.GL_VERTEX_PROGRAM_ARB);
					//Gl.glDisable(Gl.GL_FRAGMENT_PROGRAM_ARB);
					// Fixme: Hack to get around an apparent bug in ATI drivers.  Should remove as soon as it gets fixed.
					//Gl.glBindProgramARB(Gl.GL_VERTEX_PROGRAM_ARB, 0);

					//Gl.glDisableClientState(Gl.GL_COLOR_ARRAY);
					//Gl.glDisableVertexAttribArrayARB(9);
					//Gl.glDisableVertexAttribArrayARB(10);
					//Gl.glDisableClientState(Gl.GL_NORMAL_ARRAY);

					continue;
				}
				else
				{
					//--------------------------
					//
					// old style stages
					//
					//--------------------------

					// set the color
					float[] color = new float[4];
					color[0] = registers[stage.Color.Registers[0]];
					color[1] = registers[stage.Color.Registers[1]];
					color[2] = registers[stage.Color.Registers[2]];
					color[3] = registers[stage.Color.Registers[3]];

					// skip the entire stage if an add would be black
					if(((stage.DrawStateBits & (MaterialStates.SourceBlendBits & MaterialStates.DestinationBlendBits)) == (MaterialStates.SourceBlendOne | MaterialStates.DestinationBlendOne))
						&& (color[0] <= 0) && (color[1] <= 0) && (color[2] <= 0))
					{
						continue;
					}

					// skip the entire stage if a blend would be completely transparent
					if(((stage.DrawStateBits & (MaterialStates.SourceBlendBits & MaterialStates.DestinationBlendBits)) == (MaterialStates.SourceBlendSourceAlpha | MaterialStates.DestinationBlendOneMinusSourceAlpha))
						&& (color[3] <= 0))
					{
						continue;
					}

					// select the vertex color source
					if(stage.VertexColor == StageVertexColor.Ignore)
					{
						_effect.DiffuseColor = new Vector3(color[0], color[1], color[2]);
						_effect.Alpha = color[3];
					}
					else
					{
						if(stage.VertexColor == StageVertexColor.InverseModulate)
						{
							idConsole.Warning("TODO: InverseModulate");
							//GL_TextureEnvironment(Gl.GL_COMBINE_ARB);

							/*GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (int) All.Modulate);
							GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb, (int) All.Texture);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE1_RGB_ARB, Gl.GL_PRIMARY_COLOR_ARB);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND0_RGB_ARB, Gl.GL_SRC_COLOR);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND1_RGB_ARB, Gl.GL_ONE_MINUS_SRC_COLOR);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_RGB_SCALE_ARB, 1);*/
						}

						// for vertex color and modulated color, we need to enable a second texture stage
						if(color[0] != 1 || color[1] != 1 || color[2] != 1 || color[3] != 1)
						{
							SetTextureUnit(1);
							idE.ImageManager.WhiteImage.Bind();
							idConsole.Warning("TODO: vertex color");
							// GL_TextureEnvironment(Gl.GL_COMBINE_ARB);

							/*Gl.glTexEnvfv(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_COLOR, color);

							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_RGB_ARB, Gl.GL_MODULATE);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE0_RGB_ARB, Gl.GL_PREVIOUS_ARB);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE1_RGB_ARB, Gl.GL_CONSTANT_ARB);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND0_RGB_ARB, Gl.GL_SRC_COLOR);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND1_RGB_ARB, Gl.GL_SRC_COLOR);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_RGB_SCALE_ARB, 1);

							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_ALPHA_ARB, Gl.GL_MODULATE);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE0_ALPHA_ARB, Gl.GL_PREVIOUS_ARB);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_SOURCE1_ALPHA_ARB, Gl.GL_CONSTANT_ARB);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND0_ALPHA_ARB, Gl.GL_SRC_ALPHA);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_OPERAND1_ALPHA_ARB, Gl.GL_SRC_ALPHA);
							Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_ALPHA_SCALE, 1);

							GL_SelectTexture(0);*/
						}
					}

					// bind the texture
					BindVariableStageImage(stage.Texture, registers);

					// set the state
					SetState(stage.DrawStateBits);

					PrepareStageTexturing(stage, surface, tri.AmbientCache.Data);

					// draw it
					DrawElementsWithCounters(tri);

					FinishStageTexturing(stage, surface, tri.AmbientCache.Data);

					if(stage.VertexColor != StageVertexColor.Ignore)
					{
						idConsole.Warning("TODO: SVC ignore");
						/*GL.DisableClientState(ArrayCap.ColorArray);*/

						SetTextureUnit(1);
						SetTextureEnvironment(Gl.GL_MODULATE);

						idE.ImageManager.BindNullTexture();

						SetTextureUnit(0);
						SetTextureEnvironment(Gl.GL_MODULATE);
					}
				}

				// reset polygon offset
				if(material.TestMaterialFlag(MaterialFlags.PolygonOffset) == true)
				{
					// TODO: Gl.glDisable(Gl.GL_POLYGON_OFFSET_FILL);
				}

				if((surface.Space.WeaponDepthHack == true) || (surface.Space.ModelDepthHack != 0.0f))
				{
					idConsole.Warning("TODO: RB_LeaveDepthHack();");
				}
			}
		}
		#endregion

		#region Texturing
		/// <summary>
		/// Handles generating a cinematic frame if needed.
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="registers"></param>
		private void BindVariableStageImage(TextureStage texture, float[] registers)
		{
			/* TODO: if(texture.IsCinematic == true)*/
			if(false)
			{
				idConsole.Warning("TODO: BindVariableStageImage cinematic");
				/*cinData_t	cin;

				if ( r_skipDynamicTextures.GetBool() ) {
					globalImages->defaultImage->Bind();
					return;
				}

				// offset time by shaderParm[7] (FIXME: make the time offset a parameter of the shader?)
				// We make no attempt to optimize for multiple identical cinematics being in view, or
				// for cinematics going at a lower framerate than the renderer.
				cin = texture->cinematic->ImageForTime( (int)(1000 * ( backEnd.viewDef->floatTime + backEnd.viewDef->renderView.shaderParms[11] ) ) );

				if ( cin.image ) {
					globalImages->cinematicImage->UploadScratch( cin.image, cin.imageWidth, cin.imageHeight );
				} else {
					globalImages->blackImage->Bind();
				}*/
			}
			else
			{
				//FIXME: see why image is invalid
				if(texture.Image != null)
				{
					texture.Image.Bind();
				}
			}
		}

		private float[] GetMaterialTextureMatrix(float[] materialRegisters, TextureStage textureStage)
		{
			float[] matrix = new float[16];

			matrix[0] = materialRegisters[textureStage.Matrix[0, 0]];
			matrix[4] = materialRegisters[textureStage.Matrix[0, 1]];
			matrix[8] = 0;
			matrix[12] = materialRegisters[textureStage.Matrix[0, 2]];

			// we attempt to keep scrolls from generating incredibly large texture values, but
			// center rotations and center scales can still generate offsets that need to be > 1
			if((matrix[12] < -40) || (matrix[12] > 40))
			{
				matrix[12] -= (int) matrix[12];
			}

			matrix[1] = materialRegisters[textureStage.Matrix[1, 0]];
			matrix[5] = materialRegisters[textureStage.Matrix[1, 1]];
			matrix[9] = 0;
			matrix[13] = materialRegisters[textureStage.Matrix[1, 2]];

			if((matrix[13] < -40) || (matrix[13] > 40))
			{
				matrix[13] -= (int) matrix[13];
			}

			matrix[2] = 0;
			matrix[6] = 0;
			matrix[10] = 1;
			matrix[14] = 0;

			matrix[3] = 0;
			matrix[7] = 0;
			matrix[11] = 0;
			matrix[15] = 1;

			return matrix;
		}

		private void LoadMaterialTextureMatrix(float[] materialRegisters, TextureStage textureStage)
		{
			float[] matrix = GetMaterialTextureMatrix(materialRegisters, textureStage);

			// need texture uv transform
			// TODO: idConsole.WriteLine("TODO: LoadMaterialTextureMatrix");

			/*qglMatrixMode(GL_TEXTURE);
			qglLoadMatrixf(matrix);
			qglMatrixMode(GL_MODELVIEW);*/
		}

		private void PrepareStageTexturing(MaterialStage stage, DrawSurface surface, Vertex[] position)
		{
			// set privatePolygonOffset if necessary
			if(stage.PrivatePolygonOffset > 0)
			{
				// TODO: Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
				// TODO: Gl.glPolygonOffset(idE.CvarSystem.GetFloat("r_offsetFactor"), idE.CvarSystem.GetFloat("r_offsetUnits") * stage.PrivatePolygonOffset);
			}

			// set the texture matrix if needed
			if(stage.Texture.HasMatrix == true)
			{
				LoadMaterialTextureMatrix(surface.MaterialRegisters, stage.Texture);
			}

			// texgens
			if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.DiffuseCube)
			{
				idConsole.Warning("TODO: TexGen DiffuseCube");
				// TODO: Gl.glTexCoordPointer(3, Gl.GL_FLOAT, sizeof( idVertex ), new float[] { position.Normal.X, position.Normal.Y, position.Normal.Z });
			}
			else if((stage.Texture.TextureCoordinates == TextureCoordinateGeneration.SkyboxCube) || (stage.Texture.TextureCoordinates == TextureCoordinateGeneration.WobbleSkyCube))
			{
				idConsole.Warning("TODO: TexGen SkyboxCube | WobbleSky");
				// TODO: Gl.glTexCoordPointer(3, Gl.GL_FLOAT, 0, vertexCache.Position( surf->dynamicTexCoords));
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.Screen)
			{
				idConsole.Warning("TODO: TexGen Screen");

				/*qglEnable( GL_TEXTURE_GEN_S );
				qglEnable( GL_TEXTURE_GEN_T );
				qglEnable( GL_TEXTURE_GEN_Q );

				float	mat[16], plane[4];
				myGlMultMatrix( surf->space->modelViewMatrix, backEnd.viewDef->projectionMatrix, mat );

				plane[0] = mat[0];
				plane[1] = mat[4];
				plane[2] = mat[8];
				plane[3] = mat[12];
				qglTexGenfv( GL_S, GL_OBJECT_PLANE, plane );

				plane[0] = mat[1];
				plane[1] = mat[5];
				plane[2] = mat[9];
				plane[3] = mat[13];
				qglTexGenfv( GL_T, GL_OBJECT_PLANE, plane );

				plane[0] = mat[3];
				plane[1] = mat[7];
				plane[2] = mat[11];
				plane[3] = mat[15];
				qglTexGenfv( GL_Q, GL_OBJECT_PLANE, plane );*/
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.Screen2)
			{
				idConsole.Warning("TODO: TexGen Screen2");
				/*qglEnable( GL_TEXTURE_GEN_S );
				qglEnable( GL_TEXTURE_GEN_T );
				qglEnable( GL_TEXTURE_GEN_Q );

				float	mat[16], plane[4];
				myGlMultMatrix( surf->space->modelViewMatrix, backEnd.viewDef->projectionMatrix, mat );

				plane[0] = mat[0];
				plane[1] = mat[4];
				plane[2] = mat[8];
				plane[3] = mat[12];
				qglTexGenfv( GL_S, GL_OBJECT_PLANE, plane );

				plane[0] = mat[1];
				plane[1] = mat[5];
				plane[2] = mat[9];
				plane[3] = mat[13];
				qglTexGenfv( GL_T, GL_OBJECT_PLANE, plane );

				plane[0] = mat[3];
				plane[1] = mat[7];
				plane[2] = mat[11];
				plane[3] = mat[15];
				qglTexGenfv( GL_Q, GL_OBJECT_PLANE, plane );*/
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.GlassWarp)
			{
				idConsole.Warning("TODO: TexGen GlassWarp");

				/*if ( tr.backEndRenderer == BE_ARB2) {
					qglBindProgramARB( GL_FRAGMENT_PROGRAM_ARB, FPROG_GLASSWARP );
					qglEnable( GL_FRAGMENT_PROGRAM_ARB );

					GL_SelectTexture( 2 );
					globalImages->scratchImage->Bind();

					GL_SelectTexture( 1 );
					globalImages->scratchImage2->Bind();

					qglEnable( GL_TEXTURE_GEN_S );
					qglEnable( GL_TEXTURE_GEN_T );
					qglEnable( GL_TEXTURE_GEN_Q );

					float	mat[16], plane[4];
					myGlMultMatrix( surf->space->modelViewMatrix, backEnd.viewDef->projectionMatrix, mat );

					plane[0] = mat[0];
					plane[1] = mat[4];
					plane[2] = mat[8];
					plane[3] = mat[12];
					qglTexGenfv( GL_S, GL_OBJECT_PLANE, plane );

					plane[0] = mat[1];
					plane[1] = mat[5];
					plane[2] = mat[9];
					plane[3] = mat[13];
					qglTexGenfv( GL_T, GL_OBJECT_PLANE, plane );

					plane[0] = mat[3];
					plane[1] = mat[7];
					plane[2] = mat[11];
					plane[3] = mat[15];
					qglTexGenfv( GL_Q, GL_OBJECT_PLANE, plane );

					GL_SelectTexture( 0 );
				}*/
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.ReflectCube)
			{
				idConsole.Warning("TODO: TexGen ReflectCube");

				/*if ( tr.backEndRenderer == BE_ARB2 ) {
					// see if there is also a bump map specified
					const shaderStage_t *bumpStage = surf->material->GetBumpStage();
					if ( bumpStage ) {
						// per-pixel reflection mapping with bump mapping
						GL_SelectTexture( 1 );
						bumpStage->texture.image->Bind();
						GL_SelectTexture( 0 );

						qglNormalPointer( GL_FLOAT, sizeof( idDrawVert ), ac->normal.ToFloatPtr() );
						qglVertexAttribPointerARB( 10, 3, GL_FLOAT, false, sizeof( idDrawVert ), ac->tangents[1].ToFloatPtr() );
						qglVertexAttribPointerARB( 9, 3, GL_FLOAT, false, sizeof( idDrawVert ), ac->tangents[0].ToFloatPtr() );

						qglEnableVertexAttribArrayARB( 9 );
						qglEnableVertexAttribArrayARB( 10 );
						qglEnableClientState( GL_NORMAL_ARRAY );

						// Program env 5, 6, 7, 8 have been set in RB_SetProgramEnvironmentSpace

						qglBindProgramARB( GL_FRAGMENT_PROGRAM_ARB, FPROG_BUMPY_ENVIRONMENT );
						qglEnable( GL_FRAGMENT_PROGRAM_ARB );
						qglBindProgramARB( GL_VERTEX_PROGRAM_ARB, VPROG_BUMPY_ENVIRONMENT );
						qglEnable( GL_VERTEX_PROGRAM_ARB );
					} else {
						// per-pixel reflection mapping without a normal map
						qglNormalPointer( GL_FLOAT, sizeof( idDrawVert ), ac->normal.ToFloatPtr() );
						qglEnableClientState( GL_NORMAL_ARRAY );

						qglBindProgramARB( GL_FRAGMENT_PROGRAM_ARB, FPROG_ENVIRONMENT );
						qglEnable( GL_FRAGMENT_PROGRAM_ARB );
						qglBindProgramARB( GL_VERTEX_PROGRAM_ARB, VPROG_ENVIRONMENT );
						qglEnable( GL_VERTEX_PROGRAM_ARB );
					}
				} else {
					qglEnable( GL_TEXTURE_GEN_S );
					qglEnable( GL_TEXTURE_GEN_T );
					qglEnable( GL_TEXTURE_GEN_R );
					qglTexGenf( GL_S, GL_TEXTURE_GEN_MODE, GL_REFLECTION_MAP_EXT );
					qglTexGenf( GL_T, GL_TEXTURE_GEN_MODE, GL_REFLECTION_MAP_EXT );
					qglTexGenf( GL_R, GL_TEXTURE_GEN_MODE, GL_REFLECTION_MAP_EXT );
					qglEnableClientState( GL_NORMAL_ARRAY );
					qglNormalPointer( GL_FLOAT, sizeof( idDrawVert ), ac->normal.ToFloatPtr() );

					qglMatrixMode( GL_TEXTURE );
					float	mat[16];

					R_TransposeGLMatrix( backEnd.viewDef->worldSpace.modelViewMatrix, mat );

					qglLoadMatrixf( mat );
					qglMatrixMode( GL_MODELVIEW );
				}*/
			}
		}

		private void FinishStageTexturing(MaterialStage stage, DrawSurface surface, Vertex[] position)
		{
			// unset privatePolygonOffset if necessary
			if((stage.PrivatePolygonOffset > 0) && (surface.Material.TestMaterialFlag(MaterialFlags.PolygonOffset) == false))
			{
				// TODO: Gl.glDisable(Gl.GL_POLYGON_OFFSET_FILL);
			}

			if((stage.Texture.TextureCoordinates == TextureCoordinateGeneration.DiffuseCube) || (stage.Texture.TextureCoordinates == TextureCoordinateGeneration.SkyboxCube) || (stage.Texture.TextureCoordinates == TextureCoordinateGeneration.WobbleSkyCube))
			{
				idConsole.Warning("TODO: FinishStageTexturing DiffuseCube");

				// TODO qglTexCoordPointer( 2, GL_FLOAT, sizeof( idDrawVert ), (void *)&ac->st );
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.Screen)
			{
				idConsole.Warning("TODO: TexCoord Screen");

				// TODO: Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
				// TODO: Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
				// TODO: Gl.glDisable(Gl.GL_TEXTURE_GEN_Q);
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.Screen2)
			{
				idConsole.Warning("TODO: TexCoord Screen2");

				// TODO: Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
				// TODO: Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
				// TODO: Gl.glDisable(Gl.GL_TEXTURE_GEN_Q);
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.GlassWarp)
			{
				idConsole.Warning("TODO: FinishStageTexturing GlassWarp");

				/*if ( tr.backEndRenderer == BE_ARB2) {
					GL_SelectTexture( 2 );
					globalImages->BindNull();

					GL_SelectTexture( 1 );
					if ( pStage->texture.hasMatrix ) {
						RB_LoadShaderTextureMatrix( surf->shaderRegisters, &pStage->texture );
					}
					qglDisable( GL_TEXTURE_GEN_S );
					qglDisable( GL_TEXTURE_GEN_T );
					qglDisable( GL_TEXTURE_GEN_Q );
					qglDisable( GL_FRAGMENT_PROGRAM_ARB );
					globalImages->BindNull();
					GL_SelectTexture( 0 );
				}*/
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.ReflectCube)
			{
				idConsole.Warning("TODO: FinishStageTexturing ReflectCube");
				/*if ( tr.backEndRenderer == BE_ARB2 ) {
					// see if there is also a bump map specified
					const shaderStage_t *bumpStage = surf->material->GetBumpStage();
					if ( bumpStage ) {
						// per-pixel reflection mapping with bump mapping
						GL_SelectTexture( 1 );
						globalImages->BindNull();
						GL_SelectTexture( 0 );

						qglDisableVertexAttribArrayARB( 9 );
						qglDisableVertexAttribArrayARB( 10 );
					} else {
						// per-pixel reflection mapping without bump mapping
					}

					qglDisableClientState( GL_NORMAL_ARRAY );
					qglDisable( GL_FRAGMENT_PROGRAM_ARB );
					qglDisable( GL_VERTEX_PROGRAM_ARB );
					// Fixme: Hack to get around an apparent bug in ATI drivers.  Should remove as soon as it gets fixed.
					qglBindProgramARB( GL_VERTEX_PROGRAM_ARB, 0 );
				} else {
					qglDisable( GL_TEXTURE_GEN_S );
					qglDisable( GL_TEXTURE_GEN_T );
					qglDisable( GL_TEXTURE_GEN_R );
					qglTexGenf( GL_S, GL_TEXTURE_GEN_MODE, GL_OBJECT_LINEAR );
					qglTexGenf( GL_T, GL_TEXTURE_GEN_MODE, GL_OBJECT_LINEAR );
					qglTexGenf( GL_R, GL_TEXTURE_GEN_MODE, GL_OBJECT_LINEAR );
					qglDisableClientState( GL_NORMAL_ARRAY );

					qglMatrixMode( GL_TEXTURE );
					qglLoadIdentity();
					qglMatrixMode( GL_MODELVIEW );
				}*/
			}

			if(stage.Texture.HasMatrix == true)
			{
				//Gl.glMatrixMode(Gl.GL_TEXTURE);
				//Gl.glLoadIdentity();
				//Gl.glMatrixMode(Gl.GL_MODELVIEW);
			}
		}
		#endregion

		#region Depth buffer
		/// <summary>
		/// If we are rendering a subview with a near clip plane, use a second texture
		/// to force the alpha test to fail when behind that clip plane.
		/// </summary>
		/// <param name="surfaces"></param>
		private void FillDepthBuffer(DrawSurface[] surfaces)
		{
			// if we are just doing 2D rendering, no need to fill the depth buffer
			if(_viewDef.ViewEntities.Count == 0)
			{
				return;
			}

			// TODO: RB_LogComment("---------- RB_STD_FillDepthBuffer ----------\n");

			// enable the second texture for mirror plane clipping if needed
			// TODO: plane clipping
			/*if(backEnd.viewDef->numClipPlanes)
			{
				GL_SelectTexture(1);
				globalImages->alphaNotchImage->Bind();
				qglDisableClientState(GL_TEXTURE_COORD_ARRAY);
				qglEnable(GL_TEXTURE_GEN_S);
				qglTexCoord2f(1, 0.5);
			}*/

			// the first texture will be used for alpha tested surfaces
			SetTextureUnit(0);

			// decal surfaces may enable polygon offset
			// TODO: qglPolygonOffset(r_offsetFactor.GetFloat(), r_offsetUnits.GetFloat());

			SetState(MaterialStates.DepthFunctionLess);

			// Enable stencil test if we are going to be using it for shadows.
			// If we didn't do this, it would be legal behavior to get z fighting
			// from the ambient pass and the light passes.
			/*qglEnable(GL_STENCIL_TEST);
			qglStencilFunc(GL_ALWAYS, 1, 255);*/

			RenderDrawSurfaceListWithFunction(surfaces, FillDepthBufferHandler);

			/*if(backEnd.viewDef->numClipPlanes)
			{
				GL_SelectTexture(1);
				globalImages->BindNull();
				qglDisable(GL_TEXTURE_GEN_S);
				GL_SelectTexture(0);
			}*/
		}

		private void FillDepthBufferHandler(DrawSurface drawSurface)
		{
			Surface tri = drawSurface.Geometry;
			idMaterial material = drawSurface.Material;
			Vector4 color;

			// update the clip plane if needed
			// TODO
			/*if ( backEnd.viewDef->numClipPlanes && surf->space != backEnd.currentSpace ) {
				GL_SelectTexture( 1 );
		
				idPlane	plane;

				R_GlobalPlaneToLocal( surf->space->modelMatrix, backEnd.viewDef->clipPlanes[0], plane );
				plane[3] += 0.5;	// the notch is in the middle
				qglTexGenfv( GL_S, GL_OBJECT_PLANE, plane.ToFloatPtr() );
				GL_SelectTexture( 0 );
			}*/

			if(material.IsDrawn == false)
			{
				return;
			}

			// some deforms may disable themselves by setting numIndexes = 0
			if(tri.Indexes.Length == 0)
			{
				return;
			}

			// translucent surfaces don't put anything in the depth buffer and don't
			// test against it, which makes them fail the mirror clip plane operation
			if(material.Coverage == MaterialCoverage.Translucent)
			{
				return;
			}

			if(tri.AmbientCache == null)
			{
				idConsole.Warning("TODO: RB_T_FillDepthBuffer: !tri->ambientCache");
				return;
			}

			// get the expressions for conditionals / color / texcoords
			float[] regs = drawSurface.MaterialRegisters;

			// if all stages of a material have been conditioned off, don't do anything
			int stage;
			MaterialStage materialStage;

			for(stage = 0; stage < material.Stages.Length; stage++)
			{
				materialStage = material.GetStage(stage);

				// check the stage enable condition
				if(regs[materialStage.ConditionRegister] != 0)
				{
					break;
				}
			}

			if(stage == material.Stages.Length)
			{
				return;
			}

			// set polygon offset if necessary
			/*if ( shader->TestMaterialFlag(MF_POLYGONOFFSET) ) {
				qglEnable( GL_POLYGON_OFFSET_FILL );
				qglPolygonOffset( r_offsetFactor.GetFloat(), r_offsetUnits.GetFloat() * shader->GetPolygonOffset() );
			}*/

			// subviews will just down-modulate the color buffer by overbright
			if(material.Sort == (float) MaterialSort.Subview)
			{
				SetState(MaterialStates.SourceBlendDestinationColor | MaterialStates.DestinationBlendZero | MaterialStates.DepthFunctionLess);

				idConsole.Warning("TODO: subview color");

				/*color[0] =
				color[1] = 
				color[2] = ( 1.0 / backEnd.overBright );
				color[3] = 1;*/
			}
			else
			{
				// others just draw black
				color = new Vector4(0, 0, 0, 1);
			}

			/*idDrawVert *ac = (idDrawVert *)vertexCache.Position( tri->ambientCache );
			qglVertexPointer( 3, GL_FLOAT, sizeof( idDrawVert ), ac->xyz.ToFloatPtr() );
			qglTexCoordPointer( 2, GL_FLOAT, sizeof( idDrawVert ), reinterpret_cast<void *>(&ac->st) );*/

			bool drawSolid = false;

			if(material.Coverage == MaterialCoverage.Opaque)
			{
				drawSolid = true;
			}

			// we may have multiple alpha tested stages
			if(material.Coverage == MaterialCoverage.Perforated)
			{
				idConsole.Warning("TODO: perforated");

				/*// if the only alpha tested stages are condition register omitted,
				// draw a normal opaque surface
				bool	didDraw = false;

				qglEnable( GL_ALPHA_TEST );
				// perforated surfaces may have multiple alpha tested stages
				for ( stage = 0; stage < shader->GetNumStages() ; stage++ ) {		
					pStage = shader->GetStage(stage);

					if ( !pStage->hasAlphaTest ) {
						continue;
					}

					// check the stage enable condition
					if ( regs[ pStage->conditionRegister ] == 0 ) {
						continue;
					}

					// if we at least tried to draw an alpha tested stage,
					// we won't draw the opaque surface
					didDraw = true;

					// set the alpha modulate
					color[3] = regs[ pStage->color.registers[3] ];

					// skip the entire stage if alpha would be black
					if ( color[3] <= 0 ) {
						continue;
					}
					qglColor4fv( color );

					qglAlphaFunc( GL_GREATER, regs[ pStage->alphaTestRegister ] );

					// bind the texture
					pStage->texture.image->Bind();

					// set texture matrix and texGens
					RB_PrepareStageTexturing( pStage, surf, ac );

					// draw it
					RB_DrawElementsWithCounters( tri );

					RB_FinishStageTexturing( pStage, surf, ac );
				}
				qglDisable( GL_ALPHA_TEST );
				if ( !didDraw ) {
					drawSolid = true;
				}*/

			}


			// draw the entire surface solid
			if(drawSolid == true)
			{
				idConsole.Warning("TODO: qglColor4fv(color);");

				idE.ImageManager.WhiteImage.Bind();

				// draw it
				DrawElementsWithCounters(tri);
			}

			// reset polygon offset
			/*if ( shader->TestMaterialFlag(MF_POLYGONOFFSET) ) {
				qglDisable( GL_POLYGON_OFFSET_FILL );
			}*/

			// reset blending
			if(material.Sort == (float) MaterialSort.Subview)
			{
				SetState(MaterialStates.DepthFunctionLess);
			}
		}
		#endregion

		#region Display modes
		private bool GetModeInfo(ref int width, ref int height, int mode)
		{
			if((mode < -1) || (mode >= idRenderSystem.VideoModes.Length))
			{
				return false;
			}

			if(mode == -1)
			{
				width = idE.CvarSystem.GetInteger("r_customWidth");
				height = idE.CvarSystem.GetInteger("r_customHeight");

				return true;
			}

			VideoMode videoMode = idRenderSystem.VideoModes[mode];

			width = videoMode.Width;
			height = videoMode.Height;

			return true;
		}
		#endregion

		#region State management
		/// <summary>
		/// This should initialize all state that any part of the entire program
		/// may touch, including the editor.
		/// </summary>
		private void SetDefaultState()
		{
			// TODO: RB_LogComment("--- R_SetDefaultGLState ---\n");


			// TODO: Gl.glClearDepth(1.0f);

			_state = new State();
			_state.ForceState = true;

			_graphicsDevice.BlendState = BlendState.Opaque;
			_graphicsDevice.DepthStencilState = DepthStencilState.Default;
			_graphicsDevice.RasterizerState = RasterizerState.CullNone;
			_graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

			if(idE.CvarSystem.GetBool("r_useScissor") == true)
			{
				_graphicsDevice.ScissorRectangle = new Rectangle(0, 0, idE.GLConfig.VideoWidth, idE.GLConfig.VideoHeight);
			}

			for(int i = idE.GLConfig.MaxTextureUnits - 1; i >= 0; i--)
			{
				SetTextureUnit(i);

				// object linear texgen is our default
				// TODO: Gl.glTexGenf(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);
				// TODO: Gl.glTexGenf(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);
				// TODO: Gl.glTexGenf(Gl.GL_R, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);
				// TODO: Gl.glTexGenf(Gl.GL_Q, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);

				// TODO: GL_TextureEnvironment(Gl.GL_MODULATE);
				// TODO: Gl.glDisable(Gl.GL_TEXTURE_2D);
				_effect.TextureEnabled = false;
			}
		}

		/// <summary>
		/// This handles the flipping needed when the view being rendered is a mirrored view.
		/// </summary>
		/// <param name="type"></param>
		private void SetCull(CullType type)
		{
			if(_state.FaceCulling == type)
			{
				return;
			}

			if(type == CullType.TwoSided)
			{
				// TODO: Gl.glDisable(Gl.GL_CULL_FACE);
			}
			else
			{
				if(_state.FaceCulling == CullType.TwoSided)
				{
					// TODO: Gl.glDisable(Gl.GL_CULL_FACE);
				}

				if(type == CullType.TwoSided)
				{
					if(_viewDef.IsMirror == true)
					{
						// TODO: Gl.glCullFace(Gl.GL_FRONT);
					}
					else
					{
						// TODO: Gl.glCullFace(Gl.GL_BACK);
					}
				}
				else
				{
					if(_viewDef.IsMirror == true)
					{
						// TODO: Gl.glCullFace(Gl.GL_BACK);
					}
					else
					{
						// TODO: Gl.glCullFace(Gl.GL_FRONT);
					}
				}
			}

			_state.FaceCulling = type;
		}

		private void SetTextureUnit(int unit)
		{
			if(_state.CurrentTextureUnit == unit)
			{
				return;
			}

			if((unit < 0) || (unit >= idE.GLConfig.MaxTextureUnits) && (unit >= idE.GLConfig.MaxTextureImageUnits))
			{
				idConsole.Warning("GL_SelectTexture: unit = {0}", unit);
			}
			else
			{
				//Gl.glActiveTextureARB(Gl.GL_TEXTURE0_ARB + unit);
				//Gl.glClientActiveTextureARB(Gl.GL_TEXTURE0_ARB + unit);

				// TODO: RB_LogComment("glActiveTextureARB( %i );\nglClientActiveTextureARB( %i );\n", unit, unit);

				_state.CurrentTextureUnit = unit;
			}
		}

		/// <summary>
		/// This routine is responsible for setting the most commonly changed state.
		/// </summary>
		/// <param name="state"></param>
		private void SetState(MaterialStates state)
		{
			MaterialStates diff;
			int srcFactor;
			int dstFactor;

			BlendState blendState = new BlendState();

			if((idE.CvarSystem.GetBool("r_useStateCaching") == false) || (_state.ForceState == true))
			{
				// make sure everything is set all the time, so we
				// can see if our delta checking is screwing up
				diff = MaterialStates.Invalid;
				_state.ForceState = false;
			}
			else
			{
				diff = state ^ _state.StateBits;

				if(diff == 0)
				{
					return;
				}
			}

			//
			// check depthFunc bits
			//
			if((diff & (MaterialStates.DepthFunctionEqual | MaterialStates.DepthFunctionLess | MaterialStates.DepthFunctionAlways)) != 0)
			{
				if((state & MaterialStates.DepthFunctionEqual) == MaterialStates.DepthFunctionEqual)
				{
					//idConsole.Warning("TODO: DepthFuncEqual");
					//  TODO: Gl.glDepthFunc(Gl.GL_EQUAL);
				}
				else if((state & MaterialStates.DepthFunctionAlways) == MaterialStates.DepthFunctionAlways)
				{
					//idConsole.Warning("TODO: DepthFuncAlways");
					// TODO: Gl.glDepthFunc(Gl.GL_ALWAYS);
				}
				else
				{
					//idConsole.Warning("TODO: DepthFuncLEqual");
					// TODO: Gl.glDepthFunc(Gl.GL_LEQUAL);
				}
			}

			//
			// check blend bits
			//
			if((diff & (MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits)) != 0)
			{
				switch(state & MaterialStates.SourceBlendBits)
				{
					case MaterialStates.SourceBlendZero:
						blendState.AlphaSourceBlend = Blend.Zero;
						blendState.ColorSourceBlend = Blend.Zero;
						break;
					case MaterialStates.SourceBlendOne:
						blendState.AlphaSourceBlend = Blend.One;
						blendState.ColorSourceBlend = Blend.One;
						break;
					case MaterialStates.SourceBlendDestinationColor:
						blendState.AlphaSourceBlend = Blend.DestinationColor;
						blendState.ColorSourceBlend = Blend.DestinationColor;
						break;
					case MaterialStates.SourceBlendOneMinusDestinationColor:
						blendState.AlphaSourceBlend = Blend.InverseDestinationColor;
						blendState.ColorSourceBlend = Blend.InverseDestinationColor;
						break;
					case MaterialStates.SourceBlendSourceAlpha:
						blendState.AlphaSourceBlend = Blend.SourceAlpha;
						blendState.ColorSourceBlend = Blend.SourceAlpha;
						break;
					case MaterialStates.SourceBlendOneMinusSourceAlpha:
						blendState.AlphaSourceBlend = Blend.InverseSourceAlpha;
						blendState.ColorSourceBlend = Blend.InverseSourceAlpha;
						break;
					case MaterialStates.SourceBlendDestinationAlpha:
						blendState.AlphaSourceBlend = Blend.DestinationAlpha;
						blendState.ColorSourceBlend = Blend.DestinationAlpha;
						break;
					case MaterialStates.SourceBlendOneMinusDestinationAlpha:
						blendState.AlphaSourceBlend = Blend.InverseDestinationAlpha;
						blendState.ColorSourceBlend = Blend.InverseDestinationAlpha;
						break;
					case MaterialStates.SourceBlendAlphaSaturate:
						blendState.AlphaSourceBlend = Blend.SourceAlphaSaturation;
						blendState.ColorSourceBlend = Blend.SourceAlphaSaturation;
						break;
					default:
						idConsole.Error("GL_State: invalid source blend state bits");
						break;
				}

				switch(state & MaterialStates.DestinationBlendBits)
				{
					case MaterialStates.DestinationBlendZero:
						blendState.AlphaDestinationBlend = Blend.Zero;
						blendState.ColorDestinationBlend = Blend.Zero;
						break;
					case MaterialStates.DestinationBlendOne:
						blendState.AlphaDestinationBlend = Blend.One;
						blendState.ColorDestinationBlend = Blend.One;
						break;
					case MaterialStates.DestinationBlendSourceColor:
						blendState.AlphaDestinationBlend = Blend.SourceColor;
						blendState.ColorDestinationBlend = Blend.SourceColor;
						break;
					case MaterialStates.DestinationBlendOneMinusSourceColor:
						blendState.AlphaDestinationBlend = Blend.InverseSourceColor;
						blendState.ColorDestinationBlend = Blend.InverseSourceColor;
						break;
					case MaterialStates.DestinationBlendSourceAlpha:
						blendState.AlphaDestinationBlend = Blend.SourceAlpha;
						blendState.ColorDestinationBlend = Blend.SourceAlpha;
						break;
					case MaterialStates.DestinationBlendOneMinusSourceAlpha:
						blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
						blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
						break;
					case MaterialStates.DestinationBlendDestinationAlpha:
						blendState.AlphaDestinationBlend = Blend.DestinationAlpha;
						blendState.ColorDestinationBlend = Blend.DestinationAlpha;
						break;
					case MaterialStates.DestinationBlendOneMinusDestinationAlpha:
						blendState.AlphaDestinationBlend = Blend.InverseDestinationAlpha;
						blendState.ColorDestinationBlend = Blend.InverseDestinationAlpha;
						break;
					default:
						idConsole.Error("GL_State: invalid dst blend state bits");
						break;
				}
			}

			//
			// check depthmask
			//
			if((diff & MaterialStates.DepthMask) == MaterialStates.DepthMask)
			{
				//idConsole.Warning("TODO: depthmask");
				if((state & MaterialStates.DepthMask) == MaterialStates.DepthMask)
				{
					// TODO: Gl.glDepthMask(Gl.GL_FALSE);
				}
				else
				{
					// TODO: Gl.glDepthMask(Gl.GL_TRUE);
				}
			}

			//
			// check colormask
			//
			if((diff & (MaterialStates.RedMask | MaterialStates.GreenMask | MaterialStates.BlueMask | MaterialStates.AlphaMask)) != 0)
			{
				ColorWriteChannels colorChannels = ColorWriteChannels.None;

				if((diff & MaterialStates.RedMask) == 0)
				{
					colorChannels |= ColorWriteChannels.Red;
				}

				if((diff & MaterialStates.GreenMask) == 0)
				{
					colorChannels |= ColorWriteChannels.Green;
				}

				if((diff & MaterialStates.BlueMask) == 0)
				{
					colorChannels |= ColorWriteChannels.Blue;
				}

				if((diff & MaterialStates.AlphaMask) == 0)
				{
					colorChannels |= ColorWriteChannels.Alpha;
				}

				blendState.ColorWriteChannels = colorChannels;
			}

			//
			// fill/line mode
			//
			if((diff & MaterialStates.PolygonModeLine) == MaterialStates.PolygonModeLine)
			{
				if((state & MaterialStates.PolygonModeLine) == MaterialStates.PolygonModeLine)
				{
					// TODO: Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);					
				}
				else
				{
					// TODO: Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
				}
			}

			//
			// alpha test
			//
			if((diff & MaterialStates.AlphaTestBits) == MaterialStates.AlphaTestBits)
			{
				switch(state & MaterialStates.AlphaTestBits)
				{
					case 0:

						//_graphicsDevice.BlendState = BlendState.Opaque;
						break;

					case MaterialStates.AlphaTestEqual255:
						idConsole.Warning("TODO: glEnable ALPHA255");
						//Gl.glEnable(Gl.GL_ALPHA_TEST);
						//Gl.glAlphaFunc(Gl.GL_EQUAL, 1.0f);
						break;

					case MaterialStates.AlphaTestLessThan128:
						idConsole.Warning("TODO: glEnable ALPHA128");
						//Gl.glEnable(Gl.GL_ALPHA_TEST);
						//Gl.glAlphaFunc(Gl.GL_LESS, 0.5f);
						break;

					case MaterialStates.AlphaTestGreaterOrEqual128:
						idConsole.Warning("TODO: glEnable LEALPHA128");
						//Gl.glEnable(Gl.GL_ALPHA_TEST);
						//Gl.glAlphaFunc(Gl.GL_GEQUAL, 0.5f);
						break;

					default:
						break;
				}
			}

			_graphicsDevice.BlendState = blendState;

			_state.StateBits = state;
		}

		/// <summary>
		/// Sets variables that can be used by all vertex programs.
		/// </summary>
		private void SetProgramEnvironment()
		{
			if(idE.GLConfig.ArbVertexProgramAvailable == false)
			{
				return;
			}

			float[] parameters = new float[4];
			int pot;

			// screen power of two correction factor, assuming the copy to _currentRender
			// also copied an extra row and column for the bilerp
			int width = _viewDef.ViewPort.X2 - _viewDef.ViewPort.X1 + 1;
			pot = idE.ImageManager.CurrentRenderImage.Width;
			parameters[0] = width / pot;

			int height = _viewDef.ViewPort.Y2 - _viewDef.ViewPort.Y1 + 1;
			pot = idE.ImageManager.CurrentRenderImage.Height;
			parameters[1] = height / pot;

			parameters[2] = 0;
			parameters[3] = 1;

			//Gl.glProgramEnvParameter4fvARB(Gl.GL_VERTEX_PROGRAM_ARB, 0, parameters);
			//Gl.glProgramEnvParameter4fvARB(Gl.GL_FRAGMENT_PROGRAM_ARB, 0, parameters);

			// window coord to 0.0 to 1.0 conversion
			parameters[0] = 1.0f / width;
			parameters[1] = 1.0f / height;
			parameters[2] = 0;
			parameters[3] = 1;

			//Gl.glProgramEnvParameter4fvARB(Gl.GL_FRAGMENT_PROGRAM_ARB, 1, parameters);

			//
			// set eye position in global space
			//
			parameters[0] = _viewDef.RenderView.ViewOrigin.X;
			parameters[1] = _viewDef.RenderView.ViewOrigin.Y;
			parameters[2] = _viewDef.RenderView.ViewOrigin.Z;
			parameters[3] = 1.0f;

			//Gl.glProgramEnvParameter4fvARB(Gl.GL_VERTEX_PROGRAM_ARB, 1, parameters);
		}

		private void SetTextureEnvironment(int env)
		{
			if(env == _state.TextureUnits[_state.CurrentTextureUnit].TexEnv)
			{
				return;
			}

			_state.TextureUnits[_state.CurrentTextureUnit].TexEnv = env;

			switch(env)
			{
				case Gl.GL_COMBINE_EXT:
				case Gl.GL_MODULATE:
				case Gl.GL_REPLACE:
				case Gl.GL_DECAL:
				case Gl.GL_ADD:
					idConsole.Warning("TODO: Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, env);");
					break;
				default:
					idConsole.Error("GL_TexEnv: invalid env '{0}' passed\n", env);
					break;
			}
		}
		#endregion

		#region Render commands
		private void ProcessDrawViewCommand(DrawViewRenderCommand command)
		{
			_viewDef = command.View;

			// we will need to do a new copyTexSubImage of the screen
			// when a SS_POST_PROCESS material is used
			_currentRenderCopied = false;

			// if there aren't any drawsurfs, do nothing
			if(_viewDef.DrawSurfaces.Count == 0)
			{
				return;
			}

			// skip render bypasses everything that has models, assuming
			// them to be 3D views, but leaves 2D rendering visible

			if((idE.CvarSystem.GetBool("r_skipRender") == true) && (_viewDef.ViewEntities.Count > 0))
			{
				return;
			}

			// TODO: backEnd.pc.c_surfaces += backEnd.viewDef->numDrawSurfs;

			// TODO: RB_ShowOverdraw();

			// render the scene, jumping to the hardware specific interaction renderers
			DrawView();
		}

		private void ProcessSetBufferCommand(SetBufferRenderCommand cmd)
		{
			// see which draw buffer we want to render the frame to
			_frameCount = cmd.FrameCount;

			// clear screen for debugging
			// automatically enable this with several other debug tools
			// that might leave unrendered portions of the screen
			if((idE.CvarSystem.GetFloat("r_clear") > 0)
				|| (idE.CvarSystem.GetString("r_clear").Length != 1)
				|| (idE.CvarSystem.GetBool("r_lockSurfaces") == true)
				|| (idE.CvarSystem.GetBool("r_singleArea") == true)
				|| (idE.CvarSystem.GetBool("r_showOverDraw") == true))
			{
				string[] parts = idE.CvarSystem.GetString("r_clear").Split(' ');
				Color color;

				if(parts.Length == 3)
				{
					float tmp1, tmp2, tmp3;
					float.TryParse(parts[0], out tmp1);
					float.TryParse(parts[1], out tmp2);
					float.TryParse(parts[2], out tmp3);

					Vector4 tmp4 = new Vector4(tmp1, tmp2, tmp3, 1);
					color = Microsoft.Xna.Framework.Color.FromNonPremultiplied(tmp4);
				}
				else if(idE.CvarSystem.GetInteger("r_clear") == 2)
				{
					color = Microsoft.Xna.Framework.Color.FromNonPremultiplied(0, 0, 0, 255);
				}
				else if(idE.CvarSystem.GetBool("r_showOverDraw") == true)
				{
					color = Microsoft.Xna.Framework.Color.FromNonPremultiplied(255, 255, 255, 255);
				}
				else
				{
					color = Microsoft.Xna.Framework.Color.FromNonPremultiplied(102, 0, 64, 255);
				}

				_graphicsDevice.Clear(color);
			}
		}

		private void ProcessSwapBuffersCommand(SwapBuffersRenderCommand cmd)
		{
			// texture swapping test
			if(idE.CvarSystem.GetInteger("r_showImages") != 0)
			{
				ShowImages();
			}

			// TODO: RB_LogComment("***************** RB_SwapBuffers *****************\n\n\n");

			// don't flip if drawing to front buffer
			if(idE.CvarSystem.GetBool("r_frontBuffer") == false)
			{
				//
				// wglSwapinterval is a windows-private extension,
				// so we must check for it here instead of portably
				//
				if(idE.CvarSystem.IsModified("r_swapInterval") == true)
				{
					idE.CvarSystem.ClearModified("r_swapInterval");

					idConsole.Warning("TODO: r_swapInterval");

					// Wgl.wglSwapIntervalEXT(idE.CvarSystem.GetInteger("r_swapInterval"));
				}
			}
		}
		#endregion

		#region Misc
		private void CheckCapabilities()
		{
			idE.GLConfig.MultiTextureAvailable = true;
			idE.GLConfig.CubeMapAvailable = true;
			idE.GLConfig.TextureCompressionAvailable = true;
			idE.GLConfig.AnisotropicAvailable = true;

			idE.GLConfig.MaxTextureAnisotropy = 16;
			idE.GLConfig.MaxTextureImageUnits = 8;
			idE.GLConfig.MaxTextureUnits = 8;

			if(_graphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
			{
				idE.GLConfig.MaxTextureSize = 4096;
				idE.GLConfig.TextureNonPowerOfTwoAvailable = true;
				idE.GLConfig.Texture3DAvailable = true;
			}
			else
			{
				idE.GLConfig.MaxTextureSize = 2048;
				idE.GLConfig.TextureNonPowerOfTwoAvailable = false;
			}

			/*idE.GLConfig.TextureEnvCombineAvailable = CheckExtension("GL_ARB_texture_env_combine");
			idE.GLConfig.EnvDot3Available = CheckExtension("GL_ARB_texture_env_dot3");
			idE.GLConfig.TextureEnvAddAvailable = CheckExtension("GL_ARB_texture_env_add");
			idE.GLConfig.TextureNonPowerOfTwoAvailable = CheckExtension("GL_ARB_texture_non_power_of_two");
			idE.GLConfig.TextureLodBiasAvailable = true;*/

			// GL_EXT_texture_lod_bias
			// The actual extension is broken as specificed, storing the state in the texture unit instead
			// of the texture object.  The behavior in GL 1.4 is the behavior we use.
			/*if((((idE.GLConfig.VersionF.Major <= 1) && (idE.GLConfig.VersionF.Minor < 4)) == false) || (CheckExtension("GL_EXT_texture_lod") == true))
			{*/
			//idConsole.WriteLine("...using {0}", "GL_1.4_texture_lod_bias");

			/*}
			else
			{
				idConsole.WriteLine("X..{0} not found\n", "GL_1.4_texture_lod_bias");
				idE.GLConfig.TextureLodBiasAvailable = false;
			}*/

			// GL_EXT_shared_texture_palette
			//idE.GLConfig.SharedTexturePaletteAvailable = CheckExtension("GL_EXT_shared_texture_palette");

			_stencilIncrement = Gl.GL_INCR_WRAP_EXT;
			_stencilDecrement = Gl.GL_DECR_WRAP_EXT;

			// idE.GLConfig.RegisterCombinersAvailable = CheckExtension("GL_NV_register_combiners");
			// idE.GLConfig.TwoSidedStencilAvailable = CheckExtension("GL_EXT_stencil_two_side");

			/*if(idE.GLConfig.TwoSidedStencilAvailable == false)
			{
				idE.GLConfig.AtiTwoSidedStencilAvailable = CheckExtension("GL_ATI_separate_stencil");
			}

			idE.GLConfig.AtiFragmentShaderAvailable = CheckExtension("GL_ATI_fragment_shader");

			if(idE.GLConfig.AtiFragmentShaderAvailable == false)
			{
				// only on OSX: ATI_fragment_shader is faked through ATI_text_fragment_shader (macosx_glimp.cpp)
				idE.GLConfig.AtiFragmentShaderAvailable = CheckExtension("GL_ATI_text_fragment_shader");
			}

			idE.GLConfig.ArbVertexBufferObjectAvailable = CheckExtension("GL_ARB_vertex_buffer_object");
			idE.GLConfig.ArbVertexProgramAvailable = CheckExtension("GL_ARB_vertex_program");

			// ARB_fragment_program
			if(idE.CvarSystem.GetBool("r_inhibitFragmentProgram") == true)
			{
				idE.GLConfig.ArbFragmentProgramAvailable = false;
			}
			else
			{
				idE.GLConfig.ArbFragmentProgramAvailable = CheckExtension("GL_ARB_fragment_program");
			}

			// check for minimum set
			if((idE.GLConfig.MultiTextureAvailable == false)
				|| (idE.GLConfig.TextureEnvCombineAvailable == false)
				|| (idE.GLConfig.CubeMapAvailable == false)
				|| (idE.GLConfig.EnvDot3Available == false))
			{
				idConsole.Error(6780);
			}

			idE.GLConfig.DepthBoundsTestAvailable = CheckExtension("EXT_depth_bounds_test");*/
		}

		/// <summary>
		/// Draw all the images to the screen, on top of whatever
		/// was there.  This is used to test for texture thrashing.
		/// </summary>
		private void ShowImages()
		{
			idConsole.Warning("TODO: ShowImages");

			// TODO: showimages
			/*GL_Set2D();
			
			//Gl.glFinish();

			int x, y, w, h;
			int start = idE.System.Time;

			foreach(idImage image in idE.ImageManager.Images)
			{
				if((image.IsLoaded == true) && (image.PartialImage == null))
				{
					continue;
				}

				w = idE.GLConfig.VideoWidth / 20;
				h = idE.GLConfig.VideoHeight / 15;
				x = idE 

		w = glConfig.vidWidth / 20;
		h = glConfig.vidHeight / 15;
		x = i % 20 * w;
		y = i / 20 * h;

		// show in proportional size in mode 2
		if ( r_showImages.GetInteger() == 2 ) {
			w *= image->uploadWidth / 512.0f;
			h *= image->uploadHeight / 512.0f;
		}

		image->Bind();
		qglBegin (GL_QUADS);
		qglTexCoord2f( 0, 0 );
		qglVertex2f( x, y );
		qglTexCoord2f( 1, 0 );
		qglVertex2f( x + w, y );
		qglTexCoord2f( 1, 1 );
		qglVertex2f( x + w, y + h );
		qglTexCoord2f( 0, 1 );
		qglVertex2f( x, y + h );
		qglEnd();
	}

	qglFinish();

	end = Sys_Milliseconds();
	common->Printf( "%i msec to draw all images\n", end - start );*/
		}
		#endregion

		#region Buffer management
		private void UnbindIndex()
		{
			//Gl.glBindBufferARB(Gl.GL_ELEMENT_ARRAY_BUFFER_ARB, 0);
		}
		#endregion
		#endregion

		#region idRenderBackendInterface implementation
		#region Properties
		public int ScreenWidth
		{
			get
			{
				return _graphicsDevice.Viewport.Width;
			}
		}

		public int ScreenHeight
		{
			get
			{
				return _graphicsDevice.Viewport.Height;
			}
		}
		#endregion

		#region Methods
		public void Init()
		{
			_graphicsDeviceManager = new GraphicsDeviceManager(idE.System);

			GetModeInfo(ref idE.GLConfig.VideoWidth, ref idE.GLConfig.VideoHeight, idE.CvarSystem.GetInteger("r_mode"));

			_graphicsDeviceManager.PreferredBackBufferWidth = idE.GLConfig.VideoWidth;
			_graphicsDeviceManager.PreferredBackBufferHeight = idE.GLConfig.VideoHeight;
			_graphicsDeviceManager.PreferMultiSampling = idE.CvarSystem.GetInteger("r_multiSamples") > 1;
			_graphicsDeviceManager.IsFullScreen = idE.CvarSystem.GetBool("r_fullscreen");
			_graphicsDeviceManager.SupportedOrientations = DisplayOrientation.Default;
			_graphicsDeviceManager.ApplyChanges();

			_graphicsDevice = _graphicsDeviceManager.GraphicsDevice;

			idE.GLConfig.StencilBits = 8;
			idE.GLConfig.ColorBits = 32;
			idE.GLConfig.DepthBits = 24;
			idE.GLConfig.IsFullscreen = _graphicsDeviceManager.IsFullScreen;

			CheckCapabilities();

			_effect = new BasicEffect(_graphicsDevice);
			_effect.FogEnabled = false;
			_effect.LightingEnabled = false;
			_effect.TextureEnabled = true;
			_effect.VertexColorEnabled = false;
		}

		public void Execute(Queue<RenderCommand> commands)
		{
			// r_debugRenderToTexture
			int draw3DCount = 0, draw2DCount = 0, setBufferCount = 0, swapBufferCount = 0, copyRenderCount = 0;

			if((commands.Peek().CommandID == RenderCommandType.Nop) && (commands.Count == 1))
			{
				return;
			}

			// TODO: backEndStartTime = Sys_Milliseconds();

			// needed for editor rendering
			SetDefaultState();

			// upload any image loads that have completed
			idE.ImageManager.CompleteBackgroundLoading();

			foreach(RenderCommand cmd in commands)
			{
				switch(cmd.CommandID)
				{
					case RenderCommandType.Nop:
						break;

					case RenderCommandType.DrawView:
						ProcessDrawViewCommand((DrawViewRenderCommand) cmd);

						// TODO: perf counter
						/*
						if ( ((const drawSurfsCommand_t *)cmds)->viewDef->viewEntitys ) {
							c_draw3d++;
						}
						else {
							c_draw2d++;
						}*/
						break;

					case RenderCommandType.SetBuffer:
						ProcessSetBufferCommand((SetBufferRenderCommand) cmd);
						setBufferCount++;
						break;

					case RenderCommandType.SwapBuffers:
						ProcessSwapBuffersCommand((SwapBuffersRenderCommand) cmd);
						swapBufferCount++;
						break;

					case RenderCommandType.CopyRender:
						idConsole.Warning("TODO: RenderCommandType.CopyRender");
						/*RB_CopyRender( cmds );
						c_copyRenders++;*/
						break;
				}
			}

			// stop rendering on this thread
			// TODO: backEndFinishTime = Sys_Milliseconds();
			// TODO: backEnd.pc.msec = backEndFinishTime - backEndStartTime;

			// TODO: debugRenderToTexture
			/*if ( r_debugRenderToTexture.GetInteger() == 1 ) {
				common->Printf( "3d: %i, 2d: %i, SetBuf: %i, SwpBuf: %i, CpyRenders: %i, CpyFrameBuf: %i\n", c_draw3d, c_draw2d, c_setBuffers, c_swapBuffers, c_copyRenders, backEnd.c_copyFrameBuffer );
				backEnd.c_copyFrameBuffer = 0;
			}*/
		}

		public void Present()
		{
			if(_graphicsDevice != null)
			{
				_graphicsDevice.Present();
			}
		}

		public void BindTexture(idImage image)
		{
			TextureUnit unit = _state.TextureUnits[_state.CurrentTextureUnit];

			if(_state.CurrentTextureUnit < _state.TextureUnits.Length)
			{
				if(image == null)
				{
					unit.CurrentTexture = null;
					unit.Type = TextureType.Disabled;
				}
				else
				{
					unit.Type = image.Type;
					unit.CurrentTexture = image.Texture;
					unit.Filter = image.Filter;
					unit.Repeat = image.Repeat;

					// bump statistic counters
					image.LastFrameUsed = _frameCount;
					image.BindCount++;
				}
			}
		}

		public void ClearTextureUnits()
		{
			int unitCount = _state.TextureUnits.Length;

			for(int i = 0; i < unitCount; i++)
			{
				_state.TextureUnits[i].CurrentTexture = null;
			}
		}
		#endregion
		#endregion

		#region State
		private class State
		{
			public TextureUnit[] TextureUnits = new TextureUnit[8];
			public int CurrentTextureUnit;

			public CullType FaceCulling;
			public MaterialStates StateBits;
			public bool ForceState; // the next GL_State will ignore glStateBits and set everything.

			public State()
			{
				int count = TextureUnits.Length;

				for(int i = 0; i < count; i++)
				{
					TextureUnits[i] = new TextureUnit();
				}
			}
		}
		#endregion
	}
}