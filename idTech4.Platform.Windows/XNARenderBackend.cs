﻿/*
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.Platform.Windows
{
	public class XNARenderBackend : IRenderBackend
	{
		#region Constants
		public const int StencilShadowTestValue = 128;
		public const int StencilShadowMaskValue = 255;
		#endregion

		#region Members
		private long _prevBlockTime;
		private ulong _currentState;
		private BackendState _backendState = new BackendState();
		private GraphicsDeviceManager _graphicsDeviceManager;
		private idRenderCapabilities _renderCaps;

		private idViewDefinition _viewDef;
		private idScreenRect _currentScissor;
		private bool _currentRenderCopied;
		private idViewEntity _currentSpace;
		#endregion

		#region Constructor
		public XNARenderBackend()
			: base()
		{
			_renderCaps            = new idRenderCapabilities();
			_graphicsDeviceManager = new GraphicsDeviceManager(idEngine.Instance);
		}
		#endregion

		#region Buffer
		private void SwapBuffers()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			idCVar swapInterval    = cvarSystem.Find("r_swapInterval");

			if(swapInterval.IsModified == true)
			{
				swapInterval.IsModified = false;

				_graphicsDeviceManager.SynchronizeWithVerticalRetrace = (swapInterval.ToInt() > 0);
				_graphicsDeviceManager.ApplyChanges();
			}

			_graphicsDeviceManager.GraphicsDevice.Present();
		}
		#endregion

		#region Initialization
		#region Methods
		private void CheckCapabilities()
		{
			_renderCaps.MaxTextureAnisotropy    = 16;
			_renderCaps.MaxTextureImageUnits    = 8;
			_renderCaps.MaxVertexBufferElements = 80000;
			_renderCaps.MaxIndexBufferElements  = 120000;

			if(_graphicsDeviceManager.GraphicsProfile == GraphicsProfile.HiDef)
			{
				_renderCaps.MaxTextureSize                = 4096;
				_renderCaps.OcclusionQueryAvailable       = true;
				_renderCaps.TextureNonPowerOfTwoAvailable = true;
				_renderCaps.ShaderModel                   = 3;
			}
			else
			{
				_renderCaps.MaxTextureSize = 2048;
				_renderCaps.ShaderModel    = 2;
			}
		}
		#endregion
		#endregion

		#region Rendering
		public void BindTexture(idImage image)
		{
			// TODO: RENDERLOG_PRINTF( "idImage::Bind( %s )\n", GetName() );

			GraphicsDevice graphicsDevice = _graphicsDeviceManager.GraphicsDevice;

			// load the image if necessary (FIXME: not SMP safe!)
			// TODO: isloaded
			/*if ( !IsLoaded() ) {
				// load the image on demand here, which isn't our normal game operating mode
				ActuallyLoadImage( true );
			}*/

			int textureUnitIndex    = _backendState.CurrentTextureUnit;
			TextureUnit textureUnit = _backendState.TextureUnits[textureUnitIndex];

			// bind the texture
			if(image.Type == TextureType.TwoD)
			{
				if(textureUnit.CurrentTexture != image.Texture)
				{
					textureUnit.CurrentTexture                 = image.Texture;
					graphicsDevice.Textures[textureUnitIndex] = image.Texture;
				}
			}
			else if(image.Type == TextureType.Cubic)
			{
				idLog.Warning("TODO: cubic");
				
				/*if ( tmu->currentCubeMap != texnum ) {
					tmu->currentCubeMap = texnum;
					qglBindMultiTextureEXT( GL_TEXTURE0_ARB + texUnit, GL_TEXTURE_CUBE_MAP_EXT, texnum );
				}*/
			}
		}

		/// <summary>
		/// Handles generating a cinematic frame if needed.
		/// </summary>
		/// <param name="textureStage"></param>
		/// <param name="registers"></param>
		private void BindVariableStageImage(TextureStage textureStage, float[] registers)
		{
			// TODO: cinematic
			/*if ( texture->cinematic ) {
				cinData_t cin;

				if ( r_skipDynamicTextures.GetBool() ) {
					globalImages->defaultImage->Bind();
					return;
				}

				// offset time by shaderParm[7] (FIXME: make the time offset a parameter of the shader?)
				// We make no attempt to optimize for multiple identical cinematics being in view, or
				// for cinematics going at a lower framerate than the renderer.
				cin = texture->cinematic->ImageForTime( backEnd.viewDef->renderView.time[0] + idMath::Ftoi( 1000.0f * backEnd.viewDef->renderView.shaderParms[11] ) );
				if ( cin.imageY != NULL ) {
					GL_SelectTexture( 0 );
					cin.imageY->Bind();
					GL_SelectTexture( 1 );
					cin.imageCr->Bind();
					GL_SelectTexture( 2 );
					cin.imageCb->Bind();
				} else {
					globalImages->blackImage->Bind();
					// because the shaders may have already been set - we need to make sure we are not using a bink shader which would 
					// display incorrectly.  We may want to get rid of RB_BindVariableStageImage and inline the code so that the
					// SWF GUI case is handled better, too
					renderProgManager.BindShader_TextureVertexColor();
				}
			} else {*/
				// FIXME: see why image is invalid
				if(textureStage.Image != null)
				{
					BindTexture(textureStage.Image);
				}
			/*}*/
		}

		private void ChangeState(MaterialStates state, bool force = false)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			MaterialStates diff    = state ^ _backendState.StateBits;

			if((cvarSystem.GetBool("r_useStateCaching") == false) || (force == false))
			{
				// make sure everything is set all the time, so we
				// can see if our delta checking is screwing up
				diff = (MaterialStates) 0xFFFFFFFFFFFFFFFF;
			}
			else if(diff == 0)
			{
				return;
			}

			// TODO: minimize state changes when not required
			DepthStencilState depthState    = new DepthStencilState();
			BlendState blendState           = new BlendState();
			RasterizerState rasterizerState = new RasterizerState();

			//
			// check depthFunc bits
			//
			if((diff & MaterialStates.DepthFunctionBits) != 0)
			{
				switch(state & MaterialStates.DepthFunctionBits)
				{
					case MaterialStates.DepthFunctionEqual:
						depthState.DepthBufferFunction = CompareFunction.Equal;
						break;

					case MaterialStates.DepthFunctionAlways:
						depthState.DepthBufferFunction = CompareFunction.Always;
						break;

					case MaterialStates.DepthFunctionLess:
						depthState.DepthBufferFunction = CompareFunction.LessEqual;
						break;

					case MaterialStates.DepthFunctionGreater:
						depthState.DepthBufferFunction = CompareFunction.GreaterEqual;
						break;
				}
			}

			//
			// check depthmask
			//
			if((diff & MaterialStates.DepthMask) != 0)
			{
				if((state & MaterialStates.DepthMask) != 0)
				{
					depthState.DepthBufferWriteEnable = false;
				}
				else
				{
					depthState.DepthBufferWriteEnable = true;
				}
			}

			//
			// check blend bits
			//
			if((diff & (MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits)) == 0)
			{
				switch(state & MaterialStates.SourceBlendBits)
				{
					case MaterialStates.SourceBlendZero:
						blendState.ColorSourceBlend = Blend.Zero;
						blendState.AlphaSourceBlend = Blend.Zero;
						break;

					case MaterialStates.SourceBlendOne:
						blendState.ColorSourceBlend = Blend.One;
						blendState.AlphaSourceBlend = Blend.One;
						break;

					case MaterialStates.SourceBlendDestinationColor:
						blendState.ColorSourceBlend = Blend.DestinationColor;
						blendState.AlphaSourceBlend = Blend.DestinationColor;
						break;

					case MaterialStates.SourceBlendOneMinusDestinationColor:
						blendState.ColorSourceBlend = Blend.InverseDestinationColor;
						blendState.AlphaSourceBlend = Blend.InverseDestinationColor;
						break;

					case MaterialStates.SourceBlendSourceAlpha:
						blendState.ColorSourceBlend = Blend.SourceAlpha;
						blendState.AlphaSourceBlend = Blend.SourceAlpha;
						break;

					case MaterialStates.SourceBlendOneMinusSourceAlpha:
						blendState.ColorSourceBlend = Blend.InverseSourceAlpha;
						blendState.AlphaSourceBlend = Blend.InverseSourceAlpha;
						break;

					case MaterialStates.SourceBlendDestinationAlpha:
						blendState.ColorSourceBlend = Blend.DestinationAlpha;
						blendState.AlphaSourceBlend = Blend.DestinationAlpha;
						break;

					case MaterialStates.SourceBlendOneMinusDestinationAlpha:
						blendState.ColorSourceBlend = Blend.InverseDestinationAlpha;
						blendState.AlphaSourceBlend = Blend.InverseDestinationAlpha;
						break;

					default:
						Debug.Assert(false, "ChangeState: invalid src blend state bits");
						break;
				}

				switch(state & MaterialStates.DestinationBlendBits)
				{
					case MaterialStates.DestinationBlendZero:
						blendState.ColorDestinationBlend = Blend.Zero;
						blendState.AlphaDestinationBlend = Blend.Zero;
						break;

					case MaterialStates.DestinationBlendOne:
						blendState.ColorDestinationBlend = Blend.One;
						blendState.AlphaDestinationBlend = Blend.One;
						break;

					case MaterialStates.DestinationBlendSourceColor:
						blendState.ColorDestinationBlend = Blend.SourceColor;
						blendState.AlphaDestinationBlend = Blend.SourceColor;
						break;

					case MaterialStates.DestinationBlendOneMinusSourceColor:
						blendState.ColorDestinationBlend = Blend.InverseSourceColor;
						blendState.AlphaDestinationBlend = Blend.InverseSourceColor;
						break;

					case MaterialStates.DestinationBlendSourceAlpha:
						blendState.ColorDestinationBlend = Blend.SourceAlpha;
						blendState.AlphaDestinationBlend = Blend.SourceAlpha;
						break;

					case MaterialStates.DestinationBlendOneMinusSourceAlpha:
						blendState.ColorSourceBlend = Blend.InverseSourceAlpha;
						blendState.AlphaSourceBlend = Blend.InverseSourceAlpha;
						break;

					case MaterialStates.DestinationBlendDestinationAlpha:
						blendState.ColorDestinationBlend = Blend.DestinationAlpha;
						blendState.AlphaDestinationBlend = Blend.DestinationAlpha;
						break;

					case MaterialStates.DestinationBlendOneMinusDestinationAlpha:
						blendState.ColorDestinationBlend = Blend.InverseDestinationAlpha;
						blendState.AlphaDestinationBlend = Blend.InverseDestinationAlpha;
						break;

					default:
						Debug.Assert(false, "ChangeState: invalid dst blend state bits");
						break;
				}
			}

			//
			// check colormask
			//
			if((diff & (MaterialStates.RedMask | MaterialStates.GreenMask | MaterialStates.BlueMask | MaterialStates.AlphaMask)) != 0)
			{
				ColorWriteChannels writeChannels = ColorWriteChannels.None;

				if((state & MaterialStates.RedMask) == 0)
				{
					writeChannels |= ColorWriteChannels.Red;
				}

				if((state & MaterialStates.GreenMask) == 0)
				{
					writeChannels |= ColorWriteChannels.Green;
				}

				if((state & MaterialStates.BlueMask) == 0)
				{
					writeChannels |= ColorWriteChannels.Blue;
				}

				if((state & MaterialStates.AlphaMask) == 0)
				{
					writeChannels |= ColorWriteChannels.Alpha;
				}

				blendState.ColorWriteChannels = writeChannels;
				blendState.ColorWriteChannels1 = writeChannels;
				blendState.ColorWriteChannels2 = writeChannels;
				blendState.ColorWriteChannels3 = writeChannels;
			}

			//
			// fill/line mode
			//
			if((diff & MaterialStates.PolygonLineMode) != 0)
			{
				if((state & MaterialStates.PolygonLineMode) != 0)
				{
					rasterizerState.FillMode = FillMode.WireFrame;
				}
				else
				{
					rasterizerState.FillMode = FillMode.Solid;
				}
			}

			// FIXME: turn off culling because Cull() is commented out
			rasterizerState.CullMode = CullMode.None;

			//
			// polygon offset
			//
			if((diff & MaterialStates.PolygonOffset) != 0)
			{
				if((state & MaterialStates.PolygonOffset) != 0)
				{
					rasterizerState.DepthBias           = _backendState.PolyOfsBias;
					rasterizerState.SlopeScaleDepthBias = _backendState.PolyOfsScale;
				}
				else
				{
					rasterizerState.DepthBias = 0;
				}
			}

			//
			// stencil
			//
			if((diff & (MaterialStates.StencilOperationBits)) != 0)
			{
				if((state & (MaterialStates.StencilFunctionBits | MaterialStates.StencilOperationBits)) != 0)
				{
					depthState.StencilEnable = true;
				}
				else
				{
					depthState.StencilEnable = false;
				}
			}

			if((diff & (MaterialStates.StencilFunctionBits | MaterialStates.StencilFunctionReferenceBits | MaterialStates.StencilFunctionMaskBits)) != 0)
			{
				depthState.ReferenceStencil = (int) (state & MaterialStates.StencilFunctionReferenceBits) >> (int) MaterialStates.StencilFunctionReferenceShift;
				depthState.StencilMask = (int) (state & MaterialStates.StencilFunctionMaskBits) >> (int) MaterialStates.StencilFunctionMaskShift;

				switch(state & MaterialStates.StencilFunctionBits)
				{
					case MaterialStates.StencilFunctionNever:
						depthState.StencilFunction = CompareFunction.Never;
						break;

					case MaterialStates.StencilFunctionLess:
						depthState.StencilFunction = CompareFunction.Less;
						break;

					case MaterialStates.StencilFunctionEqual:
						depthState.StencilFunction = CompareFunction.Equal;
						break;

					case MaterialStates.StencilFunctionLessEqual:
						depthState.StencilFunction = CompareFunction.LessEqual;
						break;

					case MaterialStates.StencilFunctionGreater:
						depthState.StencilFunction = CompareFunction.Greater;
						break;

					case MaterialStates.StencilFunctionNotEqual:
						depthState.StencilFunction = CompareFunction.NotEqual;
						break;

					case MaterialStates.StencilFunctionGreaterEqual:
						depthState.StencilFunction = CompareFunction.GreaterEqual;
						break;

					case MaterialStates.StencilFunctionAlways:
						depthState.StencilFunction = CompareFunction.Always;
						break;
				}
			}

			if((diff & (MaterialStates.StencilOperationFailBits | MaterialStates.StencilOperationZFailBits | MaterialStates.StencilOperationPassBits)) != 0)
			{
				switch(state & MaterialStates.StencilOperationFailBits)
				{
					case MaterialStates.StencilOperationFailKeep:
						depthState.StencilFail = StencilOperation.Keep;
						break;

					case MaterialStates.StencilOperationFailZero:
						depthState.StencilFail = StencilOperation.Zero;
						break;

					case MaterialStates.StencilOperationFailReplace:
						depthState.StencilFail = StencilOperation.Replace;
						break;

					case MaterialStates.StencilOperationFailIncrement:
						depthState.StencilFail = StencilOperation.Increment;
						break;

					case MaterialStates.StencilOperationFailDecrement:
						depthState.StencilFail = StencilOperation.Decrement;
						break;

					case MaterialStates.StencilOperationFailInvert:
						depthState.StencilFail = StencilOperation.Invert;
						break;

					case MaterialStates.StencilOperationFailIncrementWrap:
						depthState.StencilFail = StencilOperation.IncrementSaturation;
						break;

					case MaterialStates.StencilOperationFailDecrementWrap:
						depthState.StencilFail = StencilOperation.DecrementSaturation;
						break;
				}

				switch(state & MaterialStates.StencilOperationZFailBits)
				{
					case MaterialStates.StencilOperationZFailKeep:
						depthState.StencilDepthBufferFail = StencilOperation.Keep;
						break;

					case MaterialStates.StencilOperationZFailZero:
						depthState.StencilDepthBufferFail = StencilOperation.Zero;
						break;

					case MaterialStates.StencilOperationZFailReplace:
						depthState.StencilDepthBufferFail = StencilOperation.Replace;
						break;

					case MaterialStates.StencilOperationZFailIncrement:
						depthState.StencilDepthBufferFail = StencilOperation.Increment;
						break;

					case MaterialStates.StencilOperationZFailDecrement:
						depthState.StencilDepthBufferFail = StencilOperation.Decrement;
						break;

					case MaterialStates.StencilOperationZFailInvert:
						depthState.StencilDepthBufferFail = StencilOperation.Invert;
						break;

					case MaterialStates.StencilOperationZFailIncrementWrap:
						depthState.StencilDepthBufferFail = StencilOperation.IncrementSaturation;
						break;

					case MaterialStates.StencilOperationZFailDecrementWrap:
						depthState.StencilDepthBufferFail = StencilOperation.DecrementSaturation;
						break;
				}

				switch(state & MaterialStates.StencilOperationPassBits)
				{
					case MaterialStates.StencilOperationPassKeep:
						depthState.StencilPass = StencilOperation.Keep;
						break;

					case MaterialStates.StencilOperationPassZero:
						depthState.StencilPass = StencilOperation.Zero;
						break;

					case MaterialStates.StencilOperationPassReplace:
						depthState.StencilPass = StencilOperation.Replace;
						break;

					case MaterialStates.StencilOperationPassIncrement:
						depthState.StencilPass = StencilOperation.Increment;
						break;

					case MaterialStates.StencilOperationPassDecrement:
						depthState.StencilPass = StencilOperation.Decrement;
						break;

					case MaterialStates.StencilOperationPassInvert:
						depthState.StencilPass = StencilOperation.Invert;
						break;

					case MaterialStates.StencilOperationPassIncrementWrap:
						depthState.StencilPass = StencilOperation.IncrementSaturation;
						break;

					case MaterialStates.StencilOperationPassDecrementWrap:
						depthState.StencilPass = StencilOperation.DecrementSaturation;
						break;
				}
			}

			_graphicsDeviceManager.GraphicsDevice.DepthStencilState = depthState;
			_graphicsDeviceManager.GraphicsDevice.BlendState        = blendState;
			_graphicsDeviceManager.GraphicsDevice.RasterizerState   = rasterizerState;

			_backendState.StateBits = state;
		}

		private void Clear(bool color, bool depth, bool stencil, byte stencilValue, float r, float g, float b, float a) 
		{
			ClearOptions clearOptions = 0;
			Vector4 clearColor        = new Vector4(r, g, b, a);

			if(color == true)
			{
				_graphicsDeviceManager.GraphicsDevice.Clear(new Color(r, g, b, a));
			}

			if(depth == true)
			{
				if(_graphicsDeviceManager.GraphicsDevice.DepthStencilState.DepthBufferEnable == true)
				{
					clearOptions |= ClearOptions.DepthBuffer;
				}
			}

			if(stencil == true)
			{
				if(_graphicsDeviceManager.GraphicsDevice.DepthStencilState.StencilEnable == true)
				{
					clearOptions |= ClearOptions.Stencil;
				}
			}

			if(clearOptions != 0)
			{
			//	_graphicsDeviceManager.GraphicsDevice.Clear(clearOptions, clearColor, 0, stencilValue);
			}
		}

		/// <summary>
		/// This handles the flipping needed when the view being rendered is a mirored view.
		/// </summary>
		/// <param name="cullType"></param>
		private void Cull(CullType cullType)
		{
			if(_backendState.FaceCulling == cullType)
			{
				return;
			}

			GraphicsDevice graphicsDevice = _graphicsDeviceManager.GraphicsDevice;

			// FIXME: can't change cullmode after state has been bound
			/*if(cullType == CullType.Two)
			{
				graphicsDevice.RasterizerState.CullMode = CullMode.None;
			}
			else
			{
				if(_backendState.FaceCulling == CullType.Back)
				{
					if(_viewDef.IsMirror == true)
					{
						graphicsDevice.RasterizerState.CullMode = CullMode.CullClockwiseFace;
					}
					else
					{
						graphicsDevice.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
					}
				}
				else
				{
					if(_viewDef.IsMirror == true)
					{
						graphicsDevice.RasterizerState.CullMode = CullMode.CullCounterClockwiseFace;
					}
					else
					{
						graphicsDevice.RasterizerState.CullMode = CullMode.CullClockwiseFace;
					}
				}
			}*/

			_backendState.FaceCulling = cullType;
		}

		private BasicEffect _effect;

		private void DrawElementsWithCounters(idDrawSurface surface)
		{
			ICVarSystem cvarSystem        = idEngine.Instance.GetService<ICVarSystem>();
			GraphicsDevice graphicsDevice = _graphicsDeviceManager.GraphicsDevice;

			//if(_effect == null)
			{
				_effect                    = new BasicEffect(graphicsDevice);
				_effect.FogEnabled         = false;
				_effect.LightingEnabled    = false;
				_effect.TextureEnabled     = true;
				_effect.VertexColorEnabled = false;
			}

			// TODO: RENDERLOG_PRINTF( "Binding Buffers: %p:%i %p:%i\n", vertexBuffer, vertOffset, indexBuffer, indexOffset );

			// TODO: skinning
			/*if ( surf->jointCache ) {
				if ( !verify( renderProgManager.ShaderUsesJoints() ) ) {
					return;
				}
			} else {
				if ( !verify( !renderProgManager.ShaderUsesJoints() || renderProgManager.ShaderHasOptionalSkinning() ) ) {
					return;
				}
			}*/
			
			// TODO: skinning
			/*if ( surf->jointCache ) {
				idJointBuffer jointBuffer;
				if ( !vertexCache.GetJointBuffer( surf->jointCache, &jointBuffer ) ) {
					idLib::Warning( "RB_DrawElementsWithCounters, jointBuffer == NULL" );
					return;
				}
				assert( ( jointBuffer.GetOffset() & ( glConfig.uniformBufferOffsetAlignment - 1 ) ) == 0 );

				const GLuint ubo = reinterpret_cast< GLuint >( jointBuffer.GetAPIObject() );
				qglBindBufferRange( GL_UNIFORM_BUFFER, 0, ubo, jointBuffer.GetOffset(), jointBuffer.GetNumJoints() * sizeof( idJointMat ) );
			}*/

			// TODO: renderProgManager.CommitUniforms();

			if((_backendState.CurrentIndexBuffer != surface.IndexBuffer) || (cvarSystem.GetBool("r_useStateCaching") == false))
			{
				graphicsDevice.Indices           = surface.IndexBuffer;
				_backendState.CurrentIndexBuffer = surface.IndexBuffer;
			}

			if((_backendState.CurrentVertexBuffer != surface.VertexBuffer) || (cvarSystem.GetBool("r_useStateCaching") == false))
			{
				graphicsDevice.SetVertexBuffer(surface.VertexBuffer);

				_backendState.CurrentVertexBuffer = surface.VertexBuffer;
				_backendState.VertexLayout        = VertexLayout.DrawVertex;
			}
	
			TextureUnit textureUnit = _backendState.TextureUnits[_backendState.CurrentTextureUnit];
			Texture texture         = textureUnit.CurrentTexture;

			if(texture != null)
			{
				_effect.Texture        = (Texture2D) texture;
				_effect.TextureEnabled = true;
			}
			else
			{
				_effect.TextureEnabled = false;
			}
			
			_effect.Projection  = _viewDef.ProjectionMatrix;
			_effect.View        = _viewDef.WorldSpace.ModelViewMatrix;
			_effect.World       = _viewDef.WorldSpace.ModelMatrix;
							
			foreach(EffectPass p in _effect.CurrentTechnique.Passes)
			{
				p.Apply();

				graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, sizeof(ushort),
					surface.FirstVertex, surface.VertexCount, surface.FirstIndex, surface.IndexCount / 3);
			}
		}

		private void DrawFlickerBox()
		{
			if(idEngine.Instance.GetService<ICVarSystem>().GetBool("r_drawFlickerBox") == false)
			{
				return;
			}

			idLog.WriteLine("TODO: DrawFlickerBox");

			/*if(tr.frameCount & 1)
			{
				qglClearColor(1, 0, 0, 1);
			}
			else
			{
				qglClearColor(0, 1, 0, 1);
			}
			qglScissor(0, 0, 256, 256);
			qglClear(GL_COLOR_BUFFER_BIT);*/
		}

		/// <summary>
		/// Draw non-light dependent passes
		/// </summary>
		/// <remarks>
		/// If we are rendering Guis, the drawSurf_t::sort value is a depth offset that can
		/// be multiplied by guiEye for polarity and screenSeparation for scale.
		/// </remarks>
		private int DrawShaderPasses(List<idDrawSurface> surfaces, float guiStereoScreenOffset, int stereoEye)
		{
			ICVarSystem cvarSystem        = idEngine.Instance.GetService<ICVarSystem>();
			IImageManager imageManager    = idEngine.Instance.GetService<IImageManager>();
			GraphicsDevice graphicsDevice = _graphicsDeviceManager.GraphicsDevice;

			// only obey skipAmbient if we are rendering a view
			if((_viewDef.ViewEntities != null) && (cvarSystem.GetBool("r_skipAmbient") == true))
			{
				return surfaces.Count;
			}

			// TODO: renderLog.OpenBlock( "RB_DrawShaderPasses" );

			SelectTextureUnit(1);
			imageManager.BindNullTexture();
			SelectTextureUnit(0);

			_currentSpace = null;

			float currentGuiStereoOffset = 0.0f;
			int i                        = 0;
			int surfaceCount             = surfaces.Count;

			for(; i < surfaceCount; i++)
			{
				idDrawSurface surface = surfaces[i];
				idMaterial material   = surface.Material;

				if((material.HasAmbient == false) || (material.IsPortalSky == true))
				{
					continue;
				}

				// some deforms may disable themselves by setting numIndexes = 0
				if(surface.IndexCount == 0)
				{
					continue;
				}

				if(material.SuppressInSubView == true)
				{
					continue;
				}
								
				// TODO: xray
				/*if ( backEnd.viewDef->isXraySubview && surf->space->entityDef ) {
					if ( surf->space->entityDef->parms.xrayIndex != 2 ) {
						continue;
					}
				}*/

				// we need to draw the post process shaders after we have drawn the fog lights
				if((material.Sort >= (float) MaterialSort.PostProcess) && (_currentRenderCopied == false))
				{
					break;
				}

				// if we are rendering a 3D view and the surface's eye index doesn't match 
				// the current view's eye index then we skip the surface
				// if the stereoEye value of a surface is 0 then we need to draw it for both eyes.
				int materialStereoEye = material.StereoEye;
				bool isEyeValid       = cvarSystem.GetBool("stereoRender_swapEyes") ? (materialStereoEye == stereoEye) : (materialStereoEye != stereoEye);

				if((stereoEye != 0) && (materialStereoEye != 0) && (isEyeValid == true))
				{
					continue;
				}

				// TOO: renderLog.OpenBlock( shader->GetName() );

				// determine the stereoDepth offset 
				// guiStereoScreenOffset will always be zero for 3D views, so the !=
				// check will never force an update due to the current sort value.
				float thisGuiStereoOffset = guiStereoScreenOffset * surface.Sort;

				// change the matrix and other space related vars if needed
				if((surface.Space != _currentSpace) || (thisGuiStereoOffset != currentGuiStereoOffset))
				{
					_currentSpace          = surface.Space;
					currentGuiStereoOffset = thisGuiStereoOffset;

					idViewEntity space = _currentSpace;

					if(guiStereoScreenOffset != 0.0f)
					{
						idLog.WriteLine("TODO: RB_SetMVPWithStereoOffset( space->mvp, currentGuiStereoOffset );");
					} 
					else 
					{
						idLog.WriteLine("TODO: RB_SetMVP( space->mvp );");
					}

					// set eye position in local space
					/*Vector4 localViewOrigin = new Vector4(1, 1, 1, 1);
					R_GlobalPointToLocal( space->modelMatrix, backEnd.viewDef->renderView.vieworg, localViewOrigin.ToVec3() );
					SetVertexParm( RENDERPARM_LOCALVIEWORIGIN, localViewOrigin.ToFloatPtr() );

					// set model Matrix
					float modelMatrixTranspose[16];
					R_MatrixTranspose( space->modelMatrix, modelMatrixTranspose );
					SetVertexParms( RENDERPARM_MODELMATRIX_X, modelMatrixTranspose, 4 );

					// set ModelView Matrix
					float modelViewMatrixTranspose[16];
					R_MatrixTranspose( space->modelViewMatrix, modelViewMatrixTranspose );
					SetVertexParms( RENDERPARM_MODELVIEWMATRIX_X, modelViewMatrixTranspose, 4 );*/
				}

				// change the scissor if needed
				if((_currentScissor.Equals(surface.Scissor) == false) && (cvarSystem.GetBool("r_useScissor") == true))
				{
					graphicsDevice.ScissorRectangle = new Rectangle(
						_viewDef.Viewport.X1 + surface.Scissor.X1,
						_viewDef.Viewport.Y1 + surface.Scissor.Y1,
						surface.Scissor.X2 + 1 - surface.Scissor.X1,
						surface.Scissor.Y2 + 1 - surface.Scissor.Y1
					);

					_currentScissor = surface.Scissor;
				}

				// get the expressions for conditionals / color / texcoords
				float[] registers = surface.MaterialRegisters;

				// set face culling appropriately
				if(surface.Space.IsGuiSurface == true)
				{
					Cull(CullType.Two);
				}
				else
				{
					Cull(material.CullType);
				}

				MaterialStates surfaceState = (MaterialStates) surface.ExtraState;

				// set polygon offset if necessary
				if(material.TestMaterialFlag(MaterialFlags.PolygonOffset) == true)
				{
					idLog.WriteLine("TODO: PolygonOffset");
					/*GL_PolygonOffset( r_offsetFactor.GetFloat(), r_offsetUnits.GetFloat() * shader->GetPolygonOffset() );
					surfGLState = GLS_POLYGON_OFFSET;*/
				}

				for(int stageIndex = 0; stageIndex < material.StageCount; stageIndex++)
				{
					MaterialStage stage = material.GetStage(stageIndex);

					// check the enable condition
					if(registers[stage.ConditionRegister] == 0)
					{
						continue;
					}

					// skip the stages involved in lighting
					if(stage.Lighting != StageLighting.Ambient)
					{
						continue;
					}

					MaterialStates stageState = (MaterialStates) surfaceState;

					if((surfaceState  & MaterialStates.Override) == 0)
					{
						stageState |= stage.DrawStateBits;
					}

					// skip if the stage is ( GL_ZERO, GL_ONE ), which is used for some alpha masks
					if((stageState & (MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits)) == (MaterialStates.SourceBlendZero | MaterialStates.DestinationBlendOne))
					{
						continue;
					}

					// see if we are a new-style stage
					NewMaterialStage newStage = stage.NewStage;

					if(newStage.IsEmpty == false)
					{
						//--------------------------
						//
						// new style stages
						//
						//--------------------------
						if(cvarSystem.GetBool("r_skipNewAmbient") == true)
						{
							continue;
						}
						
						// TODO: renderLog.OpenBlock( "New Shader Stage" );

						ChangeState(stageState);
			
						// TODO: renderProgManager.BindShader( newStage->glslProgram, newStage->glslProgram );

						for(int j = 0; j < newStage.VertexParameters.Length; j++ ) {
							float[] parm = {
								registers[ newStage.VertexParameters[j, 0]],
								registers[ newStage.VertexParameters[j, 1]],
								registers[ newStage.VertexParameters[j, 2]],
								registers[ newStage.VertexParameters[j, 3]]
							};

							// TODO: SetVertexParameter((renderParm_t)( RENDERPARM_USER + j ), parm );
						}

						// set rpEnableSkinning if the shader has optional support for skinning
						// TODO: skinning
						/*if ( surf->jointCache && renderProgManager.ShaderHasOptionalSkinning() ) {
							const idVec4 skinningParm( 1.0f );
							SetVertexParm( RENDERPARM_ENABLE_SKINNING, skinningParm.ToFloatPtr() );
						}*/

						// bind texture units
						for(int j = 0; j < newStage.FragmentProgramImages.Length; j++)
						{
							idImage image = newStage.FragmentProgramImages[j];

							if(image != null)
							{
								SelectTextureUnit(j);
								BindTexture(image);
							}
						}

						// draw it
						DrawElementsWithCounters(surface);

						// unbind texture units
						for(int j = 0; j < newStage.FragmentProgramImages.Length; j++)
						{
							idImage image = newStage.FragmentProgramImages[j];

							if(image != null)
							{
								SelectTextureUnit(j);
								imageManager.BindNullTexture();
							}
						}

						// clear rpEnableSkinning if it was set
						/*if ( surf->jointCache && renderProgManager.ShaderHasOptionalSkinning() ) {
							const idVec4 skinningParm( 0.0f );
							SetVertexParm( RENDERPARM_ENABLE_SKINNING, skinningParm.ToFloatPtr() );
						}*/

						SelectTextureUnit(0);
						
						// TODO: renderProgManager.Unbind();

						// TODO: renderLog.CloseBlock();
						continue;
					}

					//--------------------------
					//
					// old style stages
					//
					//--------------------------
					
					// set the color
					float[] color = {
						registers[stage.Color.Registers[0]],
						registers[stage.Color.Registers[1]],
						registers[stage.Color.Registers[2]],
						registers[stage.Color.Registers[3]]
					};

					// skip the entire stage if an add would be black
					if(((stageState & (MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits)) == (MaterialStates.SourceBlendOne | MaterialStates.DestinationBlendOne))
						&& (color[0] <= 0) && (color[1] <= 0) && (color[2] <= 0))
					{
						continue;
					}

					// skip the entire stage if a blend would be completely transparent
					if(((stageState & (MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits)) == (MaterialStates.SourceBlendSourceAlpha | MaterialStates.DestinationBlendOneMinusSourceAlpha))
						&& (color[3] <= 0))
					{
						continue;
					}

					StageVertexColor stageVertexColor = stage.VertexColor;

					// TODO: renderLog.OpenBlock( "Old Shader Stage" );
					idLog.Warning("TODO: SetColor(color);");

					if(surface.Space.IsGuiSurface == true)
					{
						// Force gui surfaces to always be SVC_MODULATE
						stageVertexColor = StageVertexColor.Modulate;

						// use special shaders for bink cinematics
						// TODO: cinematic
						/*if ( pStage->texture.cinematic ) {
							if ( ( stageGLState & GLS_OVERRIDE ) != 0 ) {
								// This is a hack... Only SWF Guis set GLS_OVERRIDE
								// Old style guis do not, and we don't want them to use the new GUI renderProg
								renderProgManager.BindShader_BinkGUI();
							} else {
								renderProgManager.BindShader_Bink();
							}
						} else {*/
							if((stageState & MaterialStates.Override) != 0)
							{
								// This is a hack... Only SWF Guis set GLS_OVERRIDE
								// Old style guis do not, and we don't want them to use the new GUI renderProg
								idLog.Warning("TODO: renderProgManager.BindShader_GUI();");
							} 
							else 
							{
								// TODO: skinning
								/*if ( surf->jointCache ) {
									renderProgManager.BindShader_TextureVertexColorSkinned();
								} else {
									renderProgManager.BindShader_TextureVertexColor();
								}*/
								idLog.Warning("TODO: BindShader_TextureVertexColor");
							}
						/*}*/
					} 
					else if((stage.Texture.TextureCoordinates == TextureCoordinateGeneration.Screen) || (stage.Texture.TextureCoordinates == TextureCoordinateGeneration.Screen2))
					{
						idLog.Warning("TODO: renderProgManager.BindShader_TextureTexGenVertexColor();");
					} 
					// TODO: cinematic
					/*else if(pStage->texture.cinematic) 
					{
						renderProgManager.BindShader_Bink();
					}*/ 
					else
					{
						// TODO: skinning
						/*if ( surf->jointCache ) {
							renderProgManager.BindShader_TextureVertexColorSkinned();
						} else {
							renderProgManager.BindShader_TextureVertexColor();
						}*/

						idLog.Warning("TODO: BindShader_TextureVertexColor");
					}
		
					idLog.Warning("TODO: SetVertexColorParameters(stageVertexColor);");

					// bind the texture
					BindVariableStageImage(stage.Texture, registers);

					// set privatePolygonOffset if necessary
					if(stage.PrivatePolygonOffset > 0)
					{
						idLog.Warning("TODO: polygonoffset");
						/*GL_PolygonOffset( r_offsetFactor.GetFloat(), r_offsetUnits.GetFloat() * pStage->privatePolygonOffset );*/
						stageState |= MaterialStates.PolygonOffset;
					}

					// set the state
					ChangeState(stageState);

					idLog.Warning("TODO: PrepareStageTexturing(stage, surface);");

					// draw it
					DrawElementsWithCounters(surface);

					idLog.Warning("TODO: FinishStageTexturing(stage, surface);");

					// unset privatePolygonOffset if necessary
					if(stage.PrivatePolygonOffset > 0)
					{
						idLog.Warning("TODO: GL_PolygonOffset( r_offsetFactor.GetFloat(), r_offsetUnits.GetFloat() * shader->GetPolygonOffset() );");
					}

					// TODO: renderLog.CloseBlock();
				}

				// TODO: renderLog.CloseBlock();			
			}

			Cull(CullType.Front);
			idLog.WriteLine("TODO: important! Color(1.0f, 1.0f, 1.0f);");

			// TODO: renderLog.CloseBlock();
			
			return i;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// StereoEye will always be 0 in mono modes, or -1 / 1 in stereo modes.
		/// If the view is a GUI view that is repeated for both eyes, the viewDef.stereoEye value
		/// is 0, so the stereoEye parameter is not always the same as that.
		/// </remarks>
		/// <param name="cmd"></param>
		/// <param name="stereoEye"></param>
		private void DrawView(idDrawViewRenderCommand cmd, int stereoEye)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			_viewDef = cmd.ViewDefinition;

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
			if((cvarSystem.GetBool("r_skipRender") == true) && (_viewDef.ViewEntities != null))
			{
				return;
			}


			// TODO: backEnd.pc.c_surfaces += backEnd.viewDef->numDrawSurfs;

			// TODO: RB_ShowOverdraw();

			// render the scene
			DrawViewInternal(cmd.ViewDefinition, stereoEye);

			// TODO: RB_MotionBlur();

			// optionally draw a box colored based on the eye number
			if(cvarSystem.GetBool("r_drawEyeColor") == true)
			{
				idLog.WriteLine("TODO: r_drawEyeColor");

				/*const idScreenRect & r = backEnd.viewDef->viewport;
				GL_Scissor( ( r.x1 + r.x2 ) / 2, ( r.y1 + r.y2 ) / 2, 32, 32 );
				switch ( stereoEye ) {
					case -1:
						GL_Clear( true, false, false, 0, 1.0f, 0.0f, 0.0f, 1.0f );
						break;
					case 1:
						GL_Clear( true, false, false, 0, 0.0f, 1.0f, 0.0f, 1.0f );
						break;
					default:
						GL_Clear( true, false, false, 0, 0.5f, 0.5f, 0.5f, 1.0f );
						break;
				}*/
			}
		}

		private void DrawViewInternal(idViewDefinition viewDef, int stereoEye)
		{
			// TODO: renderLog.OpenBlock( "RB_DrawViewInternal" );

			ICVarSystem cvarSystem        = idEngine.Instance.GetService<ICVarSystem>();
			GraphicsDevice graphicsDevice = _graphicsDeviceManager.GraphicsDevice;

			//-------------------------------------------------
			// guis can wind up referencing purged images that need to be loaded.
			// this used to be in the gui emit code, but now that it can be running
			// in a separate thread, it must not try to load images, so do it here.
			//-------------------------------------------------
			int surfaceCount = viewDef.DrawSurfaces.Count;

			for(int i = 0; i < surfaceCount; i++)
			{
				idDrawSurface surface = viewDef.DrawSurfaces[i];

				if(surface.Material != null)
				{
					surface.Material.EnsureNotPurged();
				}
			}

			//-------------------------------------------------
			// RB_BeginDrawingView
			//
			// Any mirrored or portaled views have already been drawn, so prepare
			// to actually render the visible surfaces for this view
			//
			// clear the z buffer, set the projection matrix, etc
			//-------------------------------------------------

			// set the window clipping
			graphicsDevice.Viewport = new Viewport(
				viewDef.Viewport.X1,
				viewDef.Viewport.Y1,
				viewDef.Viewport.X2 + 1 - viewDef.Viewport.X1,
				viewDef.Viewport.Y2 + 1 - viewDef.Viewport.Y1
			);

			// the scissor may be smaller than the viewport for subviews
			graphicsDevice.ScissorRectangle = new Rectangle(
				_viewDef.Viewport.X1 + viewDef.Scissor.X1,
				_viewDef.Viewport.Y1 + viewDef.Scissor.Y1,
				viewDef.Scissor.X2 + 1 - viewDef.Scissor.X1,
				viewDef.Scissor.Y2 + 1 - viewDef.Scissor.Y1
			);

			_currentScissor = viewDef.Scissor;

			// TODO: backEnd.glState.faceCulling = -1;		// force face culling to set next time

			// ensures that depth writes are enabled for the depth clear
			ChangeState(0);
			
			// clear the depth buffer and clear the stencil to 128 for stencil shadows as well as gui masking
			Clear(false, true, true, StencilShadowTestValue, 0.0f, 0.0f, 0.0f, 0.0f);

			// normal face culling
			Cull(CullType.Front);

/*#ifdef USE_CORE_PROFILE
	// bind one global Vertex Array Object (VAO)
	qglBindVertexArray( glConfig.global_vao );
#endif*/

			//------------------------------------
			// sets variables that can be used by all programs
			//------------------------------------
			
			// TODO
			/*{
				//
				// set eye position in global space
				//
				float parm[4];
				parm[0] = backEnd.viewDef->renderView.vieworg[0];
				parm[1] = backEnd.viewDef->renderView.vieworg[1];
				parm[2] = backEnd.viewDef->renderView.vieworg[2];
				parm[3] = 1.0f;

				SetVertexParm( RENDERPARM_GLOBALEYEPOS, parm ); // rpGlobalEyePos

				// sets overbright to make world brighter
				// This value is baked into the specularScale and diffuseScale values so
				// the interaction programs don't need to perform the extra multiply,
				// but any other renderprogs that want to obey the brightness value
				// can reference this.
				float overbright = r_lightScale.GetFloat() * 0.5f;
				parm[0] = overbright;
				parm[1] = overbright;
				parm[2] = overbright;
				parm[3] = overbright;
				SetFragmentParm( RENDERPARM_OVERBRIGHT, parm );

				// Set Projection Matrix
				float projMatrixTranspose[16];
				R_MatrixTranspose( backEnd.viewDef->projectionMatrix, projMatrixTranspose );
				SetVertexParms( RENDERPARM_PROJMATRIX_X, projMatrixTranspose, 4 );
			}*/

			//-------------------------------------------------
			// fill the depth buffer and clear color buffer to black except on subviews
			//-------------------------------------------------
			// TODO: RB_FillDepthBufferFast( drawSurfs, numDrawSurfs );

			//-------------------------------------------------
			// main light renderer
			//-------------------------------------------------
			// TODO: RB_DrawInteractions();

			//-------------------------------------------------
			// now draw any non-light dependent shading passes
			//-------------------------------------------------
			int processed = 0;

			if(cvarSystem.GetBool("r_skipShaderPasses") == false)
			{
				// TODO: renderLog.OpenMainBlock( MRB_DRAW_SHADER_PASSES );
				float guiScreenOffset;
		
				if(viewDef.ViewEntities != null)
				{
					// guiScreenOffset will be 0 in non-gui views
					guiScreenOffset = 0.0f;
				}
				else
				{
					guiScreenOffset = stereoEye * viewDef.RenderView.StereoScreenSeparation;
				}
		
				processed = DrawShaderPasses(viewDef.DrawSurfaces, guiScreenOffset, stereoEye);
		
				// TODO: renderLog.CloseMainBlock();
			}

			//-------------------------------------------------
			// fog and blend lights, drawn after emissive surfaces
			// so they are properly dimmed down
			//-------------------------------------------------
			// TODO: RB_FogAllLights();

			//-------------------------------------------------
			// capture the depth for the motion blur before rendering any post process surfaces that may contribute to the depth
			//-------------------------------------------------
			if(cvarSystem.GetInt("r_motionBlur") > 0)
			{
				idLog.WriteLine("TODO: r_motionBlur");
				
				/*const idScreenRect & viewport = backEnd.viewDef->viewport;
				globalImages->currentDepthImage->CopyDepthbuffer( viewport.x1, viewport.y1, viewport.GetWidth(), viewport.GetHeight() );*/
			}

			//-------------------------------------------------
			// now draw any screen warping post-process effects using _currentRender
			//-------------------------------------------------
			if((processed < viewDef.DrawSurfaces.Count) && (cvarSystem.GetBool("r_skipPostProcess") == false))
			{
				idLog.WriteLine("TODO: post processing");

				/*int x = backEnd.viewDef->viewport.x1;
				int y = backEnd.viewDef->viewport.y1;
				int	w = backEnd.viewDef->viewport.x2 - backEnd.viewDef->viewport.x1 + 1;
				int	h = backEnd.viewDef->viewport.y2 - backEnd.viewDef->viewport.y1 + 1;

				RENDERLOG_PRINTF( "Resolve to %i x %i buffer\n", w, h );

				GL_SelectTexture( 0 );

				// resolve the screen
				globalImages->currentRenderImage->CopyFramebuffer( x, y, w, h );
				backEnd.currentRenderCopied = true;

				// RENDERPARM_SCREENCORRECTIONFACTOR amd RENDERPARM_WINDOWCOORD overlap
				// diffuseScale and specularScale

				// screen power of two correction factor (no longer relevant now)
				float screenCorrectionParm[4];
				screenCorrectionParm[0] = 1.0f;
				screenCorrectionParm[1] = 1.0f;
				screenCorrectionParm[2] = 0.0f;
				screenCorrectionParm[3] = 1.0f;
				SetFragmentParm( RENDERPARM_SCREENCORRECTIONFACTOR, screenCorrectionParm ); // rpScreenCorrectionFactor

				// window coord to 0.0 to 1.0 conversion
				float windowCoordParm[4];
				windowCoordParm[0] = 1.0f / w;
				windowCoordParm[1] = 1.0f / h;
				windowCoordParm[2] = 0.0f;
				windowCoordParm[3] = 1.0f;
				SetFragmentParm( RENDERPARM_WINDOWCOORD, windowCoordParm ); // rpWindowCoord

				// render the remaining surfaces
				renderLog.OpenMainBlock( MRB_DRAW_SHADER_PASSES_POST );
				RB_DrawShaderPasses( drawSurfs + processed, numDrawSurfs - processed, 0.0f /* definitely not a gui *//*, stereoEye );
				// TODO: renderLog.CloseMainBlock();*/
			}

			//-------------------------------------------------
			// render debug tools
			//-------------------------------------------------
			// TODO: RB_RenderDebugTools( drawSurfs, numDrawSurfs );

			// TODO: renderLog.CloseBlock();
		}

		private void SelectTextureUnit(int unit)
		{
			if(_backendState.CurrentTextureUnit == unit)
			{
				return;
			}

			if((unit < 0) || (unit >= _renderCaps.MaxTextureImageUnits))
			{
				idLog.Warning("SelectTextureUnit: unit = {0}", unit);
			}
			else
			{
				// TODO: RENDERLOG_PRINTF( "GL_SelectTexture( %i );\n", unit );
				_backendState.CurrentTextureUnit = unit;
			}
		}

		private void SetDefaultState()
		{
			return;
			// TODO: RENDERLOG_PRINTF( "--- GL_SetDefaultState ---\n" );

			ICVarSystem cvarSystem        = idEngine.Instance.GetService<ICVarSystem>();
			IRenderSystem renderSystem    = idEngine.Instance.GetService<IRenderSystem>();

			GraphicsDevice graphicsDevice = _graphicsDeviceManager.GraphicsDevice;

			// TODO: qglClearDepth( 1.0f );

			// make sure our GL state vector is set correctly
			_backendState.Clear();

			ChangeState(0, true);

			// TODO: replace with proper default cull mode
			// these are changed by ChangeCullMode
			graphicsDevice.RasterizerState = RasterizerState.CullNone;

			// these are changed by ChangeState
			// TODO: make sure these are covered
			/*qglColorMask( GL_TRUE, GL_TRUE, GL_TRUE, GL_TRUE );
			qglBlendFunc( GL_ONE, GL_ZERO );
			qglDepthMask( GL_TRUE );
			qglDepthFunc( GL_LESS );
			qglDisable( GL_STENCIL_TEST );
			qglDisable( GL_POLYGON_OFFSET_FILL );
			qglDisable( GL_POLYGON_OFFSET_LINE );
			qglPolygonMode( GL_FRONT_AND_BACK, GL_FILL );*/

			// these should never be changed
			graphicsDevice.BlendState        = BlendState.Opaque;
			graphicsDevice.DepthStencilState = DepthStencilState.Default;

			if(cvarSystem.GetBool("r_useScissor") == true)
			{
				graphicsDevice.ScissorRectangle = new Rectangle(0, 0, renderSystem.Width, renderSystem.Height);
			}
		}
		#endregion

		#region IRenderBackend implementation
		#region Properties
		public idRenderCapabilities Capabilities
		{
			get
			{
				return _renderCaps;
			}
		}

		public ulong State
		{
			get
			{
				return _currentState;
			}
			set
			{
				_currentState = value;
			}
		}

		public float PixelAspect
		{
			get
			{
				return _renderCaps.PixelAspect;
			}
		}
		#endregion

		#region Methods
		public Texture2D CreateTexture(int width, int height, bool mipmap = false, SurfaceFormat format = SurfaceFormat.Color)
		{
			return new Texture2D(_graphicsDeviceManager.GraphicsDevice, width, height, mipmap, format);
		}

		public DynamicIndexBuffer CreateDynamicIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
		{
			return new DynamicIndexBuffer(_graphicsDeviceManager.GraphicsDevice, indexElementSize, indexCount, usage);
		}

		public DynamicVertexBuffer CreateDynamicVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
		{
			return new DynamicVertexBuffer(_graphicsDeviceManager.GraphicsDevice, vertexDeclaration, vertexCount, usage);
		}

		public IndexBuffer CreateIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
		{
			return new IndexBuffer(_graphicsDeviceManager.GraphicsDevice, indexElementSize, indexCount, usage);
		}

		public VertexBuffer CreateVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
		{
			return new VertexBuffer(_graphicsDeviceManager.GraphicsDevice, vertexDeclaration, vertexCount, usage);
		}

		/// <summary>
		/// We want to exit this with the GPU idle, right at vsync
		/// </summary>
		public void BlockingSwapBuffers()
		{
			idLog.Warning("TODO: RENDERLOG_PRINTF( \"***************** GL_BlockingSwapBuffers *****************");

			idEngine engine        = idEngine.Instance;
			ICVarSystem cvarSystem = engine.GetService<ICVarSystem>();

			long beforeFinish = engine.ElapsedTime;			
			long beforeSwap   = engine.ElapsedTime;

			if((cvarSystem.GetBool("r_showSwapBuffers") == true) && ((beforeSwap - beforeFinish) > 1))
			{
				idLog.WriteLine("{0} msec to glFinish", beforeSwap - beforeFinish);
			}

			SwapBuffers();
			Clear(true, false, false, 0, 0, 0, 0, 0);

			long beforeFence = engine.ElapsedTime;

			if((cvarSystem.GetBool("r_showSwapBuffers") == true) && ((beforeFence - beforeSwap) > 1))
			{
				idLog.WriteLine("{0} msec to swapBuffers", beforeFence - beforeSwap);
			}

			// TODO: glsync
			/*if ( glConfig.syncAvailable ) {
				swapIndex ^= 1;

				if ( qglIsSync( renderSync[swapIndex] ) ) {
					qglDeleteSync( renderSync[swapIndex] );
				}
				// draw something tiny to ensure the sync is after the swap
				const int start = Sys_Milliseconds();
				qglScissor( 0, 0, 1, 1 );
				qglEnable( GL_SCISSOR_TEST );
				qglClear( GL_COLOR_BUFFER_BIT );
				renderSync[swapIndex] = qglFenceSync( GL_SYNC_GPU_COMMANDS_COMPLETE, 0 );
				const int end = Sys_Milliseconds();
				if ( r_showSwapBuffers.GetBool() && end - start > 1 ) {
					common->Printf( "%i msec to start fence\n", end - start );
				}

				GLsync	syncToWaitOn;
				if ( r_syncEveryFrame.GetBool() ) {
					syncToWaitOn = renderSync[swapIndex];
				} else {
					syncToWaitOn = renderSync[!swapIndex];
				}

				if ( qglIsSync( syncToWaitOn ) ) {
					for ( GLenum r = GL_TIMEOUT_EXPIRED; r == GL_TIMEOUT_EXPIRED; ) {
						r = qglClientWaitSync( syncToWaitOn, GL_SYNC_FLUSH_COMMANDS_BIT, 1000 * 1000 );
					}
				}
			}*/

			long afterFence = engine.ElapsedTime;

			if((cvarSystem.GetBool("r_showSwapBuffers") == true) && ((afterFence - beforeFence) > 1))
			{
				idLog.WriteLine("{0} msec to wait on fence", afterFence - beforeFence);
			}

			long exitBlockTime = engine.ElapsedTime;

			if((cvarSystem.GetBool("r_showSwapBuffers") == true) && (_prevBlockTime != 0))
			{
				idLog.WriteLine("blockToBlock: {0}", exitBlockTime - _prevBlockTime);
			}

			_prevBlockTime = exitBlockTime;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// This function will be called syncronously if running without smp extensions, or 
		/// asyncronously by another thread.
		/// </remarks>
		/// <param name="commands"></param>
		public void ExecuteBackendCommands(LinkedListNode<idRenderCommand> commands)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			// r_debugRenderToTexture
			int c_draw3d = 0;
			int c_draw2d = 0;
			int c_setBuffers = 0;
			int c_copyRenders = 0;

			idLog.WriteLine("TODO: resolutionScale.SetCurrentGPUFrameTime( commonLocal.GetRendererGPUMicroseconds() );");

			// TODO: renderLog.StartFrame();

			if((commands.Value is idEmptyRenderCommand) && (commands.Next == null))
			{
				return;
			}

			// TODO: stereo
			/*if ( renderSystem->GetStereo3DMode() != STEREO3D_OFF ) {
				RB_StereoRenderExecuteBackEndCommands( cmds );
				renderLog.EndFrame();
				return;
			}*/

			long backendStartTime = idEngine.Instance.ElapsedTime;

			// needed for editor rendering
			SetDefaultState();

			// if we have a stereo pixel format, this will draw to both the back left and back 
			// right buffers, which will have a performance penalty.
			// TODO: qglDrawBuffer(GL_BACK);

			for(; commands != null; commands = commands.Next)
			{
				if(commands.Value is idEmptyRenderCommand)
				{
					// FIXME: break;
				}
				else if(commands.Value is idDrawViewRenderCommand)
				{
					DrawView((idDrawViewRenderCommand) commands.Value, 0);

					if(((idDrawViewRenderCommand) commands.Value).ViewDefinition.ViewEntities != null)
					{
						c_draw3d++;
					}
					else
					{
						c_draw2d++;
					}
				}
				else if(commands.Value is idSetBufferRenderCommand)
				{
					c_setBuffers++;
				}
				// TODO
				/*case RC_COPY_RENDER:
					RB_CopyRender( cmds );
					c_copyRenders++;
					break;
				case RC_POST_PROCESS:
					RB_PostProcess( cmds );
					break;*/
				else
				{
					idEngine.Instance.Error("ExecuteBackendCommands: bad command type");
					break;
				}
			}

			DrawFlickerBox();

			// Fix for the steam overlay not showing up while in game without Shell/Debug/Console/Menu also rendering
			// TODO: qglColorMask( 1, 1, 1, 1 );

			// stop rendering on this thread
			long backendFinishTime = idEngine.Instance.ElapsedTime;
			// TODO: backEnd.pc.totalMicroSec = backendFinishTime - backendStartTime;

			if(cvarSystem.GetInt("r_debugRenderToTexture") == 1)
			{
				idLog.WriteLine("3d: {0}, 2d: {1}, SetBuf: {2}, CpyRenders: {3}, CpyFrameBuf: {4}", c_draw3d, c_draw2d, c_setBuffers, c_copyRenders, 0 /* TODO: backEnd.pc.c_copyFrameBuffer*/);
				//backEnd.pc.c_copyFrameBuffer = 0;
			}

			// TODO: renderLog.EndFrame();
		}

		public void Init()
		{
			idLog.WriteLine("----- R_InitDevice -----");

			_renderCaps = new idRenderCapabilities();

			SetNewMode(true);

			// input and sound systems need to be tied to the new window
			idLog.WriteLine("TODO: Sys_InitInput();");

			// recheck all the extensions (FIXME: this might be dangerous)
			CheckCapabilities();

			idLog.WriteLine("Device      : {0}", _graphicsDeviceManager.GraphicsDevice.Adapter.Description);
			idLog.WriteLine("Profile     : {0}", _graphicsDeviceManager.GraphicsProfile);
			idLog.WriteLine("Shader Model: {0}", _renderCaps.ShaderModel);

			idLog.Warning("TODO: renderProgManager.Init();");

			// allocate the vertex array range or vertex objects
			idLog.Warning("TODO: vertexCache.Init();");

			// reset our gamma
			idLog.Warning("TODO: R_SetColorMappings();");
		}

		/// <summary>
		/// Sets up the display mode.
		/// </summary>
		/// <remarks>
		/// r_fullScreen -1		borderless window at exact desktop coordinates
		/// r_fullScreen 0		bordered window at exact desktop coordinates
		/// r_fullScreen 1		fullscreen on monitor 1 at r_vidMode
		/// r_fullScreen 2		fullscreen on monitor 2 at r_vidMode
		/// ...
		/// <para/>
		/// r_vidMode -1		use r_customWidth / r_customHeight, even if they don't appear on the mode list
		/// r_vidMode 0			use first mode returned by EnumDisplaySettings()
		/// r_vidMode 1			use second mode returned by EnumDisplaySettings()
		/// ...
		/// <para/>
		/// r_displayRefresh 0	don't specify refresh
		/// r_displayRefresh 70	specify 70 hz, etc
		/// </remarks>
		public void SetNewMode(bool fullInit)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			GraphicsAdapter adapter = null;
			int width, height;
			int x, y, displayHz;
			int fullscreen;
			int i;

			// try up to three different configurations
			for(i = 0; i < 3; i++)
			{
				// TODO: stereo
				/*if ( i == 0 && stereoRender_enable.GetInteger() != STEREO3D_QUAD_BUFFER ) {
					continue;		// don't even try for a stereo mode
				}*/

				if(cvarSystem.GetInt("r_fullscreen") <= 0)
				{
					// use explicit position / size for window
					width  = cvarSystem.GetInt("r_windowWidth");
					height = cvarSystem.GetInt("r_windowHeight");

					x = cvarSystem.GetInt("r_windowX");
					y = cvarSystem.GetInt("r_windowY");

					// may still be -1 to force a borderless window
					fullscreen = cvarSystem.GetInt("r_fullscreen");
					displayHz  = 0;		// ignored
				}
				else
				{
					// get the mode list for this monitor
					List<XNADisplayMode> modeList = new List<XNADisplayMode>();
					int fullMode                  = cvarSystem.GetInt("r_fullscreen");

					if(GetModeListForDisplay(fullMode - 1, modeList) == false)
					{
						idLog.WriteLine("r_fullscreen reset from {0} to 1 because mode list failed.", fullMode);
						cvarSystem.Set("r_fullscreen", 1);
						GetModeListForDisplay(0, modeList);
					}

					if(modeList.Count < 1)
					{
						idLog.WriteLine("Going to safe mode because mode list failed.");
						goto safeMode;
					}

					x          = 0; // ignored
					y          = 0; // ignored
					fullscreen = cvarSystem.GetInt("r_fullscreen");

					// set the parameters we are trying
					int vidMode = cvarSystem.GetInt("r_vidMode");

					if(vidMode < 0)
					{
						// try forcing a specific mode, even if it isn't on the list
						width     = cvarSystem.GetInt("r_customWidth");
						height    = cvarSystem.GetInt("r_customHeight");
						displayHz = cvarSystem.GetInt("r_displayRefresh");
					}
					else
					{
						if(vidMode > modeList.Count)
						{
							idLog.WriteLine("r_vidMode reset from {0} to 0.", vidMode);
							cvarSystem.Set("r_vidMode", 0);
						}

						adapter   = modeList[vidMode].Adapter;
						width     = modeList[vidMode].DisplayMode.Width;
						height    = modeList[vidMode].DisplayMode.Height;
						displayHz = /*modeList[vidMode].DisplayHz;*/ 0;
					}
				}

				int multiSamples = cvarSystem.GetInt("r_multiSamples");
				bool stereo      = false;

				if(i == 0)
				{
					idLog.Warning("TODO: parms.stereo = ( stereoRender_enable.GetInteger() == STEREO3D_QUAD_BUFFER );");
				}

				if(fullInit == true)
				{
					// create the context as well as setting up the window
					if(ContextInit(adapter, x, y, width, height, multiSamples, fullscreen, stereo) == true)
					{
						// it worked
						break;
					}
				}
				else
				{
					// just rebuild the window
					throw new Exception("TODO");
					/*if ( GLimp_SetScreenParms( parms ) ) {
						// it worked
						break;
					}*/
				}

				if(i == 2)
				{
					idEngine.Instance.FatalError("Unable to initialize XNA");
				}

				if(i == 0)
				{
					// same settings, no stereo
					continue;
				}

			safeMode:
				// if we failed, set everything back to "safe mode" and try again
				cvarSystem.Set("r_vidMode", 0);
				cvarSystem.Set("r_fullscreen", 1);
				cvarSystem.Set("r_displayRefresh", 0);
				cvarSystem.Set("r_multiSamples", 0);
			}
		}

		private bool ContextInit(GraphicsAdapter adapter, int x, int y, int width, int height, int multiSamples, int fullScreen, bool stereo)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			idLog.WriteLine("Initializing render subsystem with multisamples:{0} stereo:{1} fullscreen:{2}", multiSamples, stereo ? 1 : 0, fullScreen);

			// save the hardware gamma so it can be restored on exit
			idLog.Warning("TODO: GLimp_SaveGamma");

			if(adapter == null)
			{
				adapter = GraphicsAdapter.DefaultAdapter;
			}

			_graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
			_graphicsDeviceManager.PreferredBackBufferWidth       = width;
			_graphicsDeviceManager.PreferredBackBufferHeight      = height;
			_graphicsDeviceManager.PreferMultiSampling            = (multiSamples > 1);
			_graphicsDeviceManager.IsFullScreen                   = (fullScreen > 0);
			_graphicsDeviceManager.GraphicsProfile                = adapter.IsProfileSupported(GraphicsProfile.HiDef) ? GraphicsProfile.HiDef : GraphicsProfile.Reach;
						
			/*_graphicsDeviceManager.PreparingDeviceSettings       += delegate(object sender, PreparingDeviceSettingsEventArgs args)
			{
				args.GraphicsDeviceInformation.Adapter         = adapter;
			
				PresentationParameters p            = args.GraphicsDeviceInformation.PresentationParameters;
				p.MultiSampleCount                  = multiSamples;
			};*/

			_graphicsDeviceManager.ApplyChanges();

			Form window     = (Form) Form.FromHandle(idEngine.Instance.Window.Handle);
			window.Text     = idLicensee.GameName;
			window.Location = new System.Drawing.Point(x, y);

			idLog.WriteLine("...created window @ {0},{1} ({2}x{3})", x, y, width, height);

			// check to see if we can get a stereo pixel format, even if we aren't going to use it,
			// so the menu option can be 
			idLog.Warning("TODO: check stereo");
			/*if(GLW_ChoosePixelFormat(win32.hDC, parms.multiSamples, true) != -1)
			{
				glConfig.stereoPixelFormatAvailable = true;
			}
			else
			{
				glConfig.stereoPixelFormatAvailable = false;
			}*/

			_renderCaps.IsFullscreen        = fullScreen;
			_renderCaps.IsStereoPixelFormat = stereo;
			_renderCaps.NativeScreenWidth   = width;
			_renderCaps.NativeScreenHeight  = height;
			_renderCaps.Multisamples        = multiSamples;

			// FIXME: some monitor modes may be distorted. should side-by-side stereo modes be consider aspect 0.5?
			_renderCaps.PixelAspect = 1.0f;

			_renderCaps.StencilBits = 8;
			_renderCaps.ColorBits   = 32;
			_renderCaps.DepthBits   = 24;

			idLog.Warning("TODO: physical screen width");

			_renderCaps.PhysicalScreenWidthInCentimeters = 100.0f;

			// force a set next frame
			cvarSystem.SetModified("r_swapInterval");

			/*if(mmWide == 0)
			{
				glConfig.physicalScreenWidthInCentimeters = 100.0f;
			}
			else
			{
				glConfig.physicalScreenWidthInCentimeters = 0.1f * mmWide;
			}*/

			// check logging
			idLog.Warning("TODO: GLimp_EnableLogging((r_logFile.GetInteger() != 0));");

			return true;
		}

		private bool GetModeListForDisplay(int requestedDisplayNum, List<XNADisplayMode> modeList)
		{
			modeList.Clear();

			bool verbose = idEngine.Instance.GetService<ICVarSystem>().GetBool("developer");

			for(int displayNum = requestedDisplayNum; ; displayNum++)
			{
				GraphicsAdapter adapter = GraphicsAdapter.Adapters[displayNum];
				Screen monitor          = Screen.FromHandle(adapter.MonitorHandle);

				if(monitor == null)
				{
					continue;
				}

				if(verbose == true)
				{
					idLog.WriteLine("display device: {0}", displayNum);
					idLog.WriteLine("  DeviceName  : {0}", adapter.DeviceName);
					idLog.WriteLine("  DeviceID    : {0}", adapter.DeviceId);
					idLog.WriteLine("      DeviceName  : {0}", monitor.DeviceName);
				}

				int modeNum = 0;

				foreach(DisplayMode displayMode in adapter.SupportedDisplayModes[SurfaceFormat.Color])
				{
					if(displayMode.Height < 720)
					{
						continue;
					}

					if(verbose == true)
					{
						Rectangle safeArea = displayMode.TitleSafeArea;

						idLog.WriteLine("          -------------------");
						idLog.WriteLine("          modeNum             : {0}", modeNum);
						idLog.WriteLine("          width               : {0}", displayMode.Width);
						idLog.WriteLine("          height              : {0}", displayMode.Height);
						idLog.WriteLine("          safearea.x          : {0}", safeArea.X);
						idLog.WriteLine("          safearea.y          : {0}", safeArea.Y);
						idLog.WriteLine("          safearea.width      : {0}", safeArea.Width);
						idLog.WriteLine("          safearea.height     : {0}", safeArea.Height);
					}

					XNADisplayMode newDisplayMode = new XNADisplayMode();
					newDisplayMode.Adapter = adapter;
					newDisplayMode.DisplayMode = displayMode;

					modeList.Add(newDisplayMode);
					modeNum++;
				}

				if(modeList.Count > 0)
				{
					return true;
				}
			}

			// never gets here
		}
		#endregion
		#endregion

		#region DisplayMode
		private class XNADisplayMode
		{
			public GraphicsAdapter Adapter;
			public DisplayMode DisplayMode;
		}
		#endregion

		#region State
		private class BackendState
		{
			public TextureUnit[] TextureUnits = new TextureUnit[8];
			public int CurrentTextureUnit;

			public VertexBuffer CurrentVertexBuffer;
			public IndexBuffer CurrentIndexBuffer;

			public CullType FaceCulling;
			public MaterialStates StateBits;

			public VertexLayout VertexLayout;

			public float PolyOfsScale;
			public float PolyOfsBias;

			public BackendState()
			{
				int count = TextureUnits.Length;

				for(int i = 0; i < count; i++)
				{
					TextureUnits[i] = new TextureUnit();
				}
			}

			public void Clear()
			{
				CurrentTextureUnit = 0;
				CurrentVertexBuffer = null;
				CurrentIndexBuffer = null;
				FaceCulling = CullType.Front;
				VertexLayout = VertexLayout.Unknown;
				StateBits = 0;

				PolyOfsScale = 0;
				PolyOfsBias = 0;

				int count = TextureUnits.Length;

				for(int i = 0; i < count; i++)
				{
					TextureUnits[i].CurrentTexture = null;
				}
			}
		}

		private class TextureUnit
		{
			public Texture CurrentTexture;
		}
		
		private enum VertexLayout
		{
			Unknown,
			DrawVertex,
			DrawShadowVertex,
			DrawShadowVertexSkinned
		}
		#endregion
	}
}