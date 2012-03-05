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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using OpenTK.Graphics;

using Tao.DevIl;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace idTech4.Renderer
{
	/// <summary>
	/// Responsible for managing the screen, which can have multiple idRenderWorld and 2D drawing done on it.
	/// </summary>
	public sealed class idRenderSystem
	{
		#region Constants
		private static readonly VideoMode[] VideoModes = new VideoMode[] {
			new VideoMode("Mode  0: 320x240",		320,	240),
			new VideoMode("Mode  1: 400x300",		400,	300),
			new VideoMode("Mode  2: 512x384",		512,	384),
			new VideoMode("Mode  3: 640x480",		640,	480),
			new VideoMode("Mode  4: 800x600",		800,	600),
			new VideoMode("Mode  5: 1024x768",		1024,	768),
			new VideoMode("Mode  6: 1152x864",		1152,	864),
			new VideoMode("Mode  7: 1280x1024",		1280,	1024),
			new VideoMode("Mode  8: 1600x1200",		1600,	1200),
		};
		#endregion

		#region Properties
		public Vector4 AmbientLightVector
		{
			get
			{
				return _ambientLightVector;
			}
		}

		public Vector4 Color
		{
			set
			{
				_guiModel.Color = value;
			}
		}

		public idMaterial DefaultMaterial
		{
			get
			{
				return _defaultMaterial;
			}
		}

		/// <summary>
		/// Shader time for all non-world 2D rendering.
		/// </summary>
		public float FrameShaderTime
		{
			get
			{
				return _frameShaderTime;
			}
		}

		/// <summary>
		/// Has the renderer been initialized and ready for graphic operations?
		/// </summary>
		public bool IsRunning
		{
			get
			{
				return _graphicsInitialized;
			}
		}

		public int ScreenWidth
		{
			get
			{
				return idE.GLConfig.VideoWidth;
			}
		}

		public int ScreenHeight
		{
			get
			{
				return idE.GLConfig.VideoHeight;
			}
		}

		public View ViewDefinition
		{
			get
			{
				return _viewDefinition;
			}
			set
			{
				_viewDefinition = value;
			}
		}
		#endregion

		#region Members
		private bool _registered;
		private bool _graphicsInitialized;

		private GraphicsDevice _graphicsDevice;
		private GraphicsDeviceManager _graphicsDeviceManager;

		private int _frameCount;											// incremented every frame
		private int _viewCount;												// incremented every view (twice a scene if subviewed) and every R_MarkFragments call
		private float _sortOffset;											// for determinist sorting of equal sort materials

		private Vector4 _ambientLightVector;								// used for "ambient bump mapping"

		private idMaterial _defaultMaterial;
		private ViewEntity _identitySpace;									// can use if we don't know viewDef->worldSpace is valid
		private View _viewDefinition;

		private List<idRenderWorld> _worlds = new List<idRenderWorld>();

		private BackEndRenderer _backEndRenderer;							// determines which back end to use, and if vertex programs are in use
		private bool _backEndRendererHasVertexPrograms;
		private float _backEndRendererMaxLight;								// 1.0 for standard, unlimited for floats

		private ushort[] _gammaTable = new ushort[256];						// brightness / gamma modify this
		// determines how much overbrighting needs
		// to be done post-process

		private Vector2 _viewPortOffset;									// for doing larger-than-window tiled renderings
		private Vector2 _tiledViewPort;

		private int _currentRenderCrop;
		private Rectangle[] _renderCrops = new Rectangle[idE.MaxRenderCrops];

		private int _stencilIncrement;
		private int _stencilDecrement;

		private int _fragmentDisplayListBase;								// FPROG_NUM_FRAGMENT_PROGRAMS lists

		// GUI drawing variables for surface creation
		private int _guiRecursionLevel;										// to prevent infinite overruns
		private idGuiModel _guiModel;
		private idGuiModel _demoGuiModel;

		private FrameData _frameData = new FrameData();
		private float _frameShaderTime;										// shader time for all non-world 2D rendering

		// vertex cache
		private List<VertexCache> _dynamicVertexCache = new List<VertexCache>();

		// a single file can have both a vertex program and a fragment program
		private GLProgram[] _programs = new GLProgram[] {
			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramTest, "test.vfp"),
			new GLProgram(Gl.GL_FRAGMENT_PROGRAM_ARB, GLProgramType.FragmentProgramTest, "test.vfp"),

			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramInteraction, "interaction.vfp"),
			new GLProgram(Gl.GL_FRAGMENT_PROGRAM_ARB, GLProgramType.FragmentProgramInteraction, "interaction.vfp"),

			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramBumpyEnvironment, "bumpyEnvironment.vfp"),
			new GLProgram(Gl.GL_FRAGMENT_PROGRAM_ARB, GLProgramType.FragmentProgramBumpyEnvironment, "bumpyEnvironment.vfp"),

			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramAmbient, "ambientLight.vfp"),
			new GLProgram(Gl.GL_FRAGMENT_PROGRAM_ARB, GLProgramType.FragmentProgramAmbient, "ambientLight.vfp"),

			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramStencilShadow, "shadow.vp"),

			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramR200Interaction, "R200_interaction.vp"),
			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramNV20BumpAndLight, "nv20_bumpAndLight.vp"),
			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramNV20DiffuseColor, "nv20_diffuseColor.vp"),
			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramNV20SpecularColor, "nv20_diffuseAndSpecularColor.vp"),
			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramNV20DiffuseAndSpecularColor, "nv20_diffuseAndSpecularColor.vp"),
			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramEnvironment, "environment.vfp"),
			new GLProgram(Gl.GL_FRAGMENT_PROGRAM_ARB, GLProgramType.FragmentProgramEnvironment, "environment.vfp"),
			new GLProgram(Gl.GL_VERTEX_PROGRAM_ARB, GLProgramType.VertexProgramGlassWarp, "arbVP_glasswarp.txt"),
			new GLProgram(Gl.GL_FRAGMENT_PROGRAM_ARB, GLProgramType.FragmentProgramGlassWarp, "arbFP_glasswarp.txt")

			// additional programs can be dynamically specified in materials
		};
		#endregion

		#region Constructor
		public idRenderSystem()
		{
			Clear();
			InitCvars();
		}
		#endregion

		#region Methods
		#region Public
		public void AddDrawSurface(Surface surface, ViewEntity space, RenderEntity renderEntity, idMaterial material, idScreenRect scissor)
		{
			float[] materialParameters;

			DrawSurface drawSurface = new DrawSurface();
			drawSurface.Geometry = surface;
			drawSurface.Space = space;
			drawSurface.Material = material;
			drawSurface.ScissorRectangle = scissor;
			drawSurface.Sort = (float) material.Sort + _sortOffset;

			// bumping this offset each time causes surfaces with equal sort orders to still
			// deterministically draw in the order they are added
			_sortOffset += 0.000001f;

			// process the shader expressions for conditionals / color / texcoords
			float[] constantRegisters = material.ConstantRegisters;

			if(constantRegisters != null)
			{
				// shader only uses constant values
				drawSurface.MaterialRegisters = constantRegisters;
			}
			else
			{
				drawSurface.MaterialRegisters = new float[material.RegisterCount];

				// a reference shader will take the calculated stage color value from another shader
				// and use that for the parm0-parm3 of the current shader, which allows a stage of
				// a light model and light flares to pick up different flashing tables from
				// different light shaders
				// TODO: reference shader

				/*if ( renderEntity->referenceShader ) {
					// evaluate the reference shader to find our shader parms
					const shaderStage_t *pStage;

					renderEntity->referenceShader->EvaluateRegisters( refRegs, renderEntity->shaderParms, tr.viewDef, renderEntity->referenceSound );
					pStage = renderEntity->referenceShader->GetStage(0);

					memcpy( generatedShaderParms, renderEntity->shaderParms, sizeof( generatedShaderParms ) );
					generatedShaderParms[0] = refRegs[ pStage->color.registers[0] ];
					generatedShaderParms[1] = refRegs[ pStage->color.registers[1] ];
					generatedShaderParms[2] = refRegs[ pStage->color.registers[2] ];

					shaderParms = generatedShaderParms;
				} else */
				{
					// evaluate with the entityDef's shader parms
					materialParameters = renderEntity.MaterialParameters;
				}

				float oldFloatTime;
				int oldTime;

				// TODO: entityDef
				/*if ( space->entityDef && space->entityDef->parms.timeGroup ) {
					oldFloatTime = tr.viewDef->floatTime;
					oldTime = tr.viewDef->renderView.time;

					tr.viewDef->floatTime = game->GetTimeGroupTime( space->entityDef->parms.timeGroup ) * 0.001;
					tr.viewDef->renderView.time = game->GetTimeGroupTime( space->entityDef->parms.timeGroup );
				}*/

				material.EvaluateRegisters(ref drawSurface.MaterialRegisters, materialParameters, idE.RenderSystem.ViewDefinition /* TODO: ,renderEntity->referenceSound*/);

				// TODO: entityDef
				/*if ( space->entityDef && space->entityDef->parms.timeGroup ) {
					tr.viewDef->floatTime = oldFloatTime;
					tr.viewDef->renderView.time = oldTime;
				}*/
			}

			// check for deformations
			// TODO: R_DeformDrawSurf( drawSurf );

			// skybox surfaces need a dynamic texgen
			// TODO: skybox
			/*switch( shader->Texgen() ) {
				case TG_SKYBOX_CUBE:
					R_SkyboxTexGen( drawSurf, tr.viewDef->renderView.vieworg );
					break;
				case TG_WOBBLESKY_CUBE:
					R_WobbleskyTexGen( drawSurf, tr.viewDef->renderView.vieworg );
					break;
			}*/

			// check for gui surfaces
			// TODO: gui surface
			/*idUserInterface	*gui = NULL;

			if ( !space->entityDef ) {
				gui = shader->GlobalGui();
			} else {
				int guiNum = shader->GetEntityGui() - 1;
				if ( guiNum >= 0 && guiNum < MAX_RENDERENTITY_GUI ) {
					gui = renderEntity->gui[ guiNum ];
				}
				if ( gui == NULL ) {
					gui = shader->GlobalGui();
				}
			}*/

			/*if ( gui ) {
				// force guis on the fast time
				float oldFloatTime;
				int oldTime;

				oldFloatTime = tr.viewDef->floatTime;
				oldTime = tr.viewDef->renderView.time;

				tr.viewDef->floatTime = game->GetTimeGroupTime( 1 ) * 0.001;
				tr.viewDef->renderView.time = game->GetTimeGroupTime( 1 );

				idBounds ndcBounds;

				if ( !R_PreciseCullSurface( drawSurf, ndcBounds ) ) {
					// did we ever use this to forward an entity color to a gui that didn't set color?
		//			memcpy( tr.guiShaderParms, shaderParms, sizeof( tr.guiShaderParms ) );
					R_RenderGuiSurf( gui, drawSurf );
				}

				tr.viewDef->floatTime = oldFloatTime;
				tr.viewDef->renderView.time = oldTime;
			}*/

			_viewDefinition.DrawSurfaces.Add(drawSurface);

			// we can't add subviews at this point, because that would
			// increment tr.viewCount, messing up the rest of the surface
			// adds for this view
		}

		/// <summary>
		/// This is the main 3D rendering command.  A single scene may
		/// have multiple views if a mirror, portal, or dynamic texture is present.
		/// </summary>
		/// <param name="view"></param>
		public void AddDrawViewCommand(View view)
		{
			DrawViewRenderCommand cmd = new DrawViewRenderCommand();
			cmd.View = view;

			_frameData.Commands.Enqueue(cmd);

			// TODO: lock surfaces
			/*if ( parms->viewEntitys ) {
				// save the command for r_lockSurfaces debugging
				tr.lockSurfacesCmd = *cmd;
			}*/

			// TODO: tr.pc.c_numViews++;

			// TODO: R_ViewStatistics(parms);
		}

		private BasicEffect _basicEffect;

		public void BeginFrame(int windowWidth, int windowHeight)
		{
			if(this.IsRunning == false)
			{
				return;
			}

			if(_basicEffect == null)
			{
				_basicEffect = new BasicEffect(_graphicsDevice);
				_basicEffect.FogEnabled = false;
				_basicEffect.LightingEnabled = false;
				_basicEffect.TextureEnabled = true;
				_basicEffect.VertexColorEnabled = false;
			}

			_guiModel.Clear();

			// for the larger-than-window tiled rendering screenshots
			if(_tiledViewPort.X > 0)
			{
				windowWidth = (int) _tiledViewPort.X;
				windowHeight = (int) _tiledViewPort.Y;
			}

			idE.GLConfig.VideoWidth = windowWidth;
			idE.GLConfig.VideoHeight = windowHeight;

			_currentRenderCrop = 0;
			_renderCrops[0] = new Rectangle(0, 0, windowWidth, windowHeight);

			// screenFraction is just for quickly testing fill rate limitations
			if(idE.CvarSystem.GetInteger("r_screenFraction") != 100)
			{
				int w = (int) (idE.VirtualScreenWidth * idE.CvarSystem.GetInteger("r_screenFraction") / 100.0f);
				int h = (int) (idE.VirtualScreenHeight * idE.CvarSystem.GetInteger("r_screenFraction") / 100.0f);

				// TODO: CropRenderSize(w, h);
				idConsole.WriteLine("idRenderSystem.CropRenderSize");
			}

			// this is the ONLY place this is modified
			_frameCount++;

			// just in case we did a common->Error while this
			// was set
			_guiRecursionLevel = 0;

			// the first rendering will be used for commands like
			// screenshot, rather than a possible subsequent remote
			// or mirror render
			//	primaryWorld = NULL;

			// set the time for shader effects in 2D rendering
			// TODO: frameShaderTime = eventLoop->Milliseconds() * 0.001;

			//
			// draw buffer stuff
			//
			SetBufferRenderCommand cmd = new SetBufferRenderCommand();
			cmd.FrameCount = _frameCount;

			if(idE.CvarSystem.GetBool("r_frontBuffer") == true)
			{
				cmd.Buffer = Gl.GL_FRONT;
			}
			else
			{
				cmd.Buffer = Gl.GL_BACK;
			}

			_frameData.Commands.Enqueue(cmd);
		}

		/// <summary>
		/// Creates a new renderWorld to be used for drawing.
		/// </summary>
		/// <returns></returns>
		public idRenderWorld CreateRenderWorld()
		{
			idRenderWorld world = new idRenderWorld();
			_worlds.Add(world);

			return world;
		}

		public void DrawStretchPicture(Vertex[] vertices, int[] indexes, idMaterial material)
		{
			DrawStretchPicture(vertices, indexes, material, true);
		}

		public void DrawStretchPicture(Vertex[] vertices, int[] indexes, idMaterial material, bool clip)
		{
			DrawStretchPicture(vertices, indexes, material, true, 0.0f, 0.0f, 640.0f, 0.0f);
		}

		public void DrawStretchPicture(Vertex[] vertices, int[] indexes, idMaterial material, bool clip, float minX, float minY, float maxX, float maxY)
		{
			_guiModel.DrawStretchPicture(vertices, indexes, material, clip, minX, minY, maxX, maxY);
		}

		public void DrawStretchPicture(float x, float y, float width, float height, float s, float t, float s2, float t2, idMaterial material)
		{
			_guiModel.DrawStretchPicture(x, y, width, height, s, t, s2, t2, material);
		}

		public void EndFrame()
		{
			int front, back;

			EndFrame(out front, out back);
		}

		public void EndFrame(out int frontendTime, out int backendTime)
		{
			frontendTime = 0;
			backendTime = 0;

			if(this.IsRunning == false)
			{
				return;
			}

			// close any gui drawing
			_guiModel.EmitFullScreen();
			_guiModel.Clear();

			// save out timing information			
			// TODO: timing
			/*if ( frontEndMsec ) {
				*frontEndMsec = pc.frontEndMsec;
			}
			if ( backEndMsec ) {
				*backEndMsec = backEnd.pc.msec;
			}*/

			// print any other statistics and clear all of them
			// TODO: R_PerformanceCounters();

			// check for dynamic changes that require some initialization
			// TODO: CheckCvars();

			// add the swapbuffers command
			_frameData.Commands.Enqueue(new SwapBuffersRenderCommand());

			// start the back end up again with the new command list
			IssueRenderCommands();

			// use the other buffers next frame, because another CPU
			// may still be rendering into the current buffers
			ToggleSmpFrame();

			// we can now release the vertexes used this frame
			// TODO: vertexCache.EndFrame();

			// TODO: demo
			/*if ( session->writeDemo ) {
				session->writeDemo->WriteInt( DS_RENDER );
				session->writeDemo->WriteInt( DC_END_FRAME );
				if ( r_showDemo.GetBool() ) {
					common->Printf( "write DC_END_FRAME\n" );
				}
			}*/
		}

		public void Init(GraphicsDeviceManager graphicsDeviceManager)
		{
			idConsole.WriteLine("------- Initializing renderSystem --------");

			// clear all our internal state
			_viewCount = 1;	// so cleared structures never match viewCount
			// we used to memset tr, but now that it is a class, we can't, so
			// there may be other state we need to reset

			_ambientLightVector = new Vector4(0.5f, 0.5f - 0.385f, 0.8925f, 1.0f);
			_frameData.Commands = new Queue<RenderCommand>();

			InitCommands();

			_guiModel = new idGuiModel();
			_demoGuiModel = new idGuiModel();

			_graphicsDeviceManager = graphicsDeviceManager;

			GetModeInfo(ref idE.GLConfig.VideoWidth, ref idE.GLConfig.VideoHeight, idE.CvarSystem.GetInteger("r_mode"));

			_graphicsDeviceManager.PreferredBackBufferWidth = idE.GLConfig.VideoWidth;
			_graphicsDeviceManager.PreferredBackBufferHeight = idE.GLConfig.VideoHeight;
			_graphicsDeviceManager.PreferMultiSampling = idE.CvarSystem.GetInteger("r_multiSamples") > 1;
			_graphicsDeviceManager.IsFullScreen = idE.CvarSystem.GetBool("r_fullscreen");
			_graphicsDeviceManager.SupportedOrientations = DisplayOrientation.Default;
			_graphicsDeviceManager.ApplyChanges();

			idE.GLConfig.StencilBits = 8;
			idE.GLConfig.ColorBits = 32;
			idE.GLConfig.DepthBits = 24;
			idE.GLConfig.IsFullscreen = _graphicsDeviceManager.IsFullScreen;

			// TODO: R_InitTriSurfData();

			idE.ImageManager.Init();

			// TODO: idCinematic::InitCinematic( );

			// build brightness translation tables
			SetColorMappings();

			InitMaterials();

			/*// TODO: renderModelManager->Init();*/

			// set the identity space
			_identitySpace = new ViewEntity();
			_identitySpace.ModelMatrix.M11 = 1.0f;
			_identitySpace.ModelMatrix.M21 = 1.0f;
			_identitySpace.ModelMatrix.M31 = 1.0f;

			idConsole.WriteLine("renderSystem initialized.");
			idConsole.WriteLine("--------------------------------------");
		}

		public void InitGraphics(GraphicsDevice graphicsDevice)
		{
			// start renderer now if it hasn't been started already
			if(this.IsRunning == true)
			{
				return;
			}

			idConsole.WriteLine("----- R_InitGraphics -----");

			_graphicsDevice = graphicsDevice;

			// in case we had an error while doing a tiled rendering
			_viewPortOffset = Vector2.Zero;
					
			// input and sound systems need to be tied to the new window
			// TODO: Sys_InitInput();
			// TODO: soundSystem->InitHW();
			
			// stubbed or broken drivers may have reported 0...
			if(idE.GLConfig.MaxTextureSize <= 0)
			{
				idE.GLConfig.MaxTextureSize = 256;
			}

			_graphicsInitialized = true;

			CheckCapabilities();

			// allocate the vertex array range or vertex objects
			/*// TODO: vertexCache.Init();*/

			ToggleSmpFrame();

			// reset our gamma
			/*SetColorMappings();*/
		}

		/// <summary>
		/// Converts from SCREEN_WIDTH / SCREEN_HEIGHT coordinates to current cropped pixel coordinates
		/// </summary>
		/// <param name="renderView"></param>
		/// <returns></returns>
		public idScreenRect RenderViewToViewPort(RenderView renderView)
		{
			Rectangle renderCrop = _renderCrops[_currentRenderCrop];

			float widthRatio = (float) renderCrop.Width / idE.VirtualScreenWidth;
			float heightRatio = (float) renderCrop.Height / idE.VirtualScreenHeight;

			idScreenRect viewPort = new idScreenRect();
			viewPort.X1 = (int) (renderCrop.X + renderView.X * widthRatio);
			viewPort.X2 = (int) ((renderCrop.X + idMath.Floor(renderView.X + renderView.Width) * widthRatio + 0.5f) - 1);
			viewPort.Y1 = (int) ((renderCrop.Y + renderCrop.Height) - idMath.Floor((renderView.Y + renderView.Height) * heightRatio + 0.5f));
			viewPort.Y2 = (int) ((renderCrop.Y + renderCrop.Height) - idMath.Floor(renderView.Y * heightRatio + 0.5f) - 1);

			return viewPort;
		}
		#endregion

		#region Internal
		internal VertexCache AllocateVertexCacheFrameTemporary(Vertex[] vertices)
		{
			VertexCache cache = new VertexCache();

			if(vertices.Length == 0)
			{
				idConsole.Error("AllocateVertexCacheFromTemorary: size = 0");
			}

			// TODO: vertex cache alloc > frameBytes
			/*if(dynamicAllocThisFrame + size > frameBytes)
			{
				// if we don't have enough room in the temp block, allocate a static block,
				// but immediately free it so it will get freed at the next frame
				tempOverflow = true;
				Alloc(data, size, &block);
				Free(block);
				return block;
			}*/

			// this data is just going on the shared dynamic list

			// TODO: i think we could have one massive vertex buffer for temporary frame data.  
			// save having all these creations and destructions of buffers

			// move it from the freeDynamicHeaders list to the dynamicHeaders list
			_dynamicVertexCache.Add(cache);

			// TODO: dynamicAllocThisFrame += block->size;
			// TODO: dynamicCountThisFrame++;

			cache.Tag = VertexCacheType.Temporary;
			cache.Data = vertices;

			return cache;
		}
		#endregion

		#region Private
		/// <summary>
		/// Any mirrored or portaled views have already been drawn, so prepare
		/// to actually render the visible surfaces for this view.
		/// </summary>
		private void BeginDrawingView()
		{
			Matrix projMatrix = idE.Backend.ViewDefinition.ProjectionMatrix;

			float[] tmpProjMatrix = new float[] {
				projMatrix.M11, projMatrix.M12, projMatrix.M13, projMatrix.M14,
				projMatrix.M21, projMatrix.M22, projMatrix.M23, projMatrix.M24,
				projMatrix.M31, projMatrix.M32, projMatrix.M33, projMatrix.M34,
				projMatrix.M41, projMatrix.M42, projMatrix.M43, projMatrix.M44
			};

			// set the modelview matrix for the viewer
			//Gl.glMatrixMode(Gl.GL_PROJECTION);
			//Gl.glLoadMatrixf(tmpProjMatrix);
			//Gl.glMatrixMode(Gl.GL_MODELVIEW);


			// set the window clipping
			/*Gl.glViewport((int) _viewPortOffset.X + idE.Backend.ViewDefinition.ViewPort.X1,
				(int) _viewPortOffset.Y + idE.Backend.ViewDefinition.ViewPort.Y1,
				idE.Backend.ViewDefinition.ViewPort.X2 + 1 - idE.Backend.ViewDefinition.ViewPort.X1,
				idE.Backend.ViewDefinition.ViewPort.Y2 + 1 - idE.Backend.ViewDefinition.ViewPort.Y1);*/


			// the scissor may be smaller than the viewport for subviews
			/*Gl.glScissor((int) _viewPortOffset.X + idE.Backend.ViewDefinition.ViewPort.X1 + idE.Backend.ViewDefinition.Scissor.X1,
				(int) _viewPortOffset.Y + idE.Backend.ViewDefinition.ViewPort.Y1 + idE.Backend.ViewDefinition.Scissor.Y1,
				idE.Backend.ViewDefinition.Scissor.X2 + 1 - idE.Backend.ViewDefinition.Scissor.X1,
				idE.Backend.ViewDefinition.Scissor.Y2 + 1 - idE.Backend.ViewDefinition.Scissor.Y1);*/

			idE.Backend.CurrentScissor = idE.Backend.ViewDefinition.Scissor;

			// ensures that depth writes are enabled for the depth clear
			GL_State(MaterialStates.DepthFunctionAlways);

			// we don't have to clear the depth / stencil buffer for 2D rendering
			// TODO:
			/*if ( backEnd.viewDef->viewEntitys ) {
				Gl.glStencilMask(0xFF);
				// some cards may have 7 bit stencil buffers, so don't assume this
				// should be 128
				Gl.glClearStencil(1 << (glConfig.stencilBits - 1));
				Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT | Gl.GL_STENCIL_BUFFER_BIT);
				Gl.glEnable(Gl.GL_DEPTH_TEST);
			} else */
			{
				DepthStencilState s = new DepthStencilState();
				s.DepthBufferEnable = false;
				s.StencilEnable = false;

				_graphicsDevice.DepthStencilState = s;
			}

			idE.Backend.GLState.FaceCulling = CullType.None; // force face culling to set next time

			GL_Cull(CullType.Front);
		}

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
				idConsole.WriteLine("TODO: BindVariableStageImage cinematic");
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
			
			// EXT_stencil_wrap
			// This isn't very important, but some pathological case might cause a clamp error and give a shadow bug.
			// Nvidia also believes that future hardware may be able to run faster with this enabled to avoid the
			// serialization of clamping.
			if(CheckExtension("GL_EXT_stencil_wrap") == true)
			{
				_stencilIncrement = Gl.GL_INCR_WRAP_EXT;
				_stencilDecrement = Gl.GL_DECR_WRAP_EXT;
			}
			else
			{
				_stencilIncrement = Gl.GL_INCR_WRAP;
				_stencilDecrement = Gl.GL_DECR_WRAP;
			}

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

		private bool CheckExtension(string name)
		{
			return true;
			if(idE.GLConfig.Extensions.Contains(name) == true)
			{
				idConsole.WriteLine("...using {0}", name);
				return true;
			}

			idConsole.WriteLine("X..{0} not found", name);

			return false;
		}
		
		private void Clear()
		{
			_frameCount = 0;
			_viewCount = 0;

			// TODO
			/*staticAllocCount = 0;*/
			_frameShaderTime = 0;

			_viewPortOffset = Vector2.Zero;
			_tiledViewPort = Vector2.Zero;

			_backEndRenderer = BackEndRenderer.Bad;
			_backEndRendererHasVertexPrograms = false;
			_backEndRendererMaxLight = 1.0f;

			_ambientLightVector = Vector4.Zero;
			_sortOffset = 0;
			_worlds.Clear();

			/*primaryWorld = NULL;
			memset( &primaryRenderView, 0, sizeof( primaryRenderView ) );
			primaryView = NULL;*/

			_defaultMaterial = null;

			/*
			testImage = NULL;
			ambientCubeImage = NULL;*/

			_viewDefinition = null;

			/*memset( &pc, 0, sizeof( pc ) );
			memset( &lockSurfacesCmd, 0, sizeof( lockSurfacesCmd ) );*/

			_identitySpace = new ViewEntity();

			/*logFile = NULL;*/

			_renderCrops = new Rectangle[idE.MaxRenderCrops];
			_currentRenderCrop = 0;
			_guiRecursionLevel = 0;
			_guiModel = null;
			/*demoGuiModel = NULL;
			memset( gammaTable, 0, sizeof( gammaTable ) );
			takingScreenshot = false;*/
		}

		/// <summary>
		/// Called after every buffer submission and by ToggleSmpFrame.
		/// </summary>
		private void ClearCommandChain()
		{
			// clear the command chain
			_frameData.Commands.Clear();
			_frameData.Commands.Enqueue(new NoOperationRenderCommand());
		}

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
				idConsole.WriteLine("TODO: indexCache");
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

				Texture texture = idE.Backend.GLState.TextureUnits[idE.Backend.GLState.CurrentTextureUnit].CurrentTexture;

				if(texture != null)
				{
					_basicEffect.Texture = (Texture2D) texture;
					_basicEffect.TextureEnabled = true;
				}
				else
				{
					_basicEffect.TextureEnabled = false;
				}
								
				_basicEffect.View = idE.Backend.ViewDefinition.WorldSpace.ModelViewMatrix;
				_basicEffect.Projection = idE.Backend.ViewDefinition.ProjectionMatrix;
				_basicEffect.World = Matrix.Identity;
				
				foreach(EffectPass p in _basicEffect.CurrentTechnique.Passes)
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
		private int DrawShaderPasses(DrawSurface[] surfaces)
		{
			// only obey skipAmbient if we are rendering a view
			// TODO
			/*if ( backEnd.viewDef->viewEntitys && r_skipAmbient.GetBool() ) {
				return numDrawSurfs;
			}*/

			// RB_LogComment( "---------- RB_STD_DrawShaderPasses ----------\n" );

			// if we are about to draw the first surface that needs
			// the rendering in a texture, copy it over
			if(surfaces[0].Material.Sort >= (float) MaterialSort.PostProcess)
			{
				idConsole.WriteLine("TODO: PostProcess");
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

			GL_SelectTexture(1);
			idE.ImageManager.BindNullTexture();

			GL_SelectTexture(0);
			//Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

			SetProgramEnvironment();

			// we don't use RB_RenderDrawSurfListWithFunction()
			// because we want to defer the matrix load because many
			// surfaces won't draw any ambient passes
			// TODO: backEnd.currentSpace = NULL;
			int i;

			for(i = 0; i < surfaces.Length; i++)
			{
				DrawSurface surface = surfaces[i];

				// TODO:
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
				if((surface.Material.Sort >= (float) MaterialSort.PostProcess) && (idE.Backend.CurrentRenderCopied == false))
				{
					break;
				}

				RenderShaderPasses(surface);
			}

			GL_Cull(CullType.TwoSided);
			//Gl.glColor3f(1, 1, 1);

			return i;
		}

		private void DrawView(DrawViewRenderCommand command)
		{
			idE.Backend.ViewDefinition = command.View;

			// we will need to do a new copyTexSubImage of the screen
			// when a SS_POST_PROCESS material is used
			idE.Backend.CurrentRenderCopied = false;

			// if there aren't any drawsurfs, do nothing
			if(idE.Backend.ViewDefinition.DrawSurfaces.Count == 0)
			{
				return;
			}

			// skip render bypasses everything that has models, assuming
			// them to be 3D views, but leaves 2D rendering visible
			if((idE.CvarSystem.GetBool("r_skipRender") == true) /* TODO: && backEnd.viewDef->viewEntitys*/)
			{
				return;
			}

			// skip render context sets the wgl context to NULL,
			// which should factor out the API cost, under the assumption
			// that all gl calls just return if the context isn't valid

			// TODO: r_skipRenderContext
			/*if ( r_skipRenderContext.GetBool() && backEnd.viewDef->viewEntitys ) {
				GLimp_DeactivateContext();
			}*/

			// TODO: backEnd.pc.c_surfaces += backEnd.viewDef->numDrawSurfs;

			// TODO: RB_ShowOverdraw();

			// render the scene, jumping to the hardware specific interaction renderers
			DrawViewActual();

			// restore the context for 2D drawing if we were stubbing it out
			// TODO: r_skipRenderContext
			/*if ( r_skipRenderContext.GetBool() && backEnd.viewDef->viewEntitys ) {
				GLimp_ActivateContext();
				RB_SetDefaultGLState();
			}*/
		}

		private void DrawViewActual()
		{
			// TODO: RB_LogComment( "---------- RB_STD_DrawView ----------\n" );

			idE.Backend.DepthFunction = MaterialStates.DepthFunctionEqual;

			DrawSurface[] surfaces = idE.Backend.ViewDefinition.DrawSurfaces.ToArray();
			int surfaceCount = surfaces.Length;

			// clear the z buffer, set the projection matrix, etc
			BeginDrawingView();

			// decide how much overbrighting we are going to do
			// TODO: RB_DetermineLightScale();

			// fill the depth buffer and clear color buffer to black except on
			// subviews
			// TODO: FillDepthBuffer(surfaces);

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
			int processed = DrawShaderPasses(surfaces);

			// fob and blend lights
			// TODO: RB_STD_FogAllLights();

			// now draw any post-processing effects using _currentRender
			/*if ( processed < numDrawSurfs ) {
				RB_STD_DrawShaderPasses( drawSurfs+processed, numDrawSurfs-processed );
			}

			RB_RenderDebugTools( drawSurfs, numDrawSurfs );*/

		}

		private void ExecuteBackEndCommands(Queue<RenderCommand> commands)
		{
			// r_debugRenderToTexture
			int draw3DCount = 0, draw2DCount = 0, setBufferCount = 0, swapBufferCount = 0, copyRenderCount = 0;

			if((commands.Peek().CommandID == RenderCommandType.Nop) && (commands.Count == 1))
			{
				return;
			}

			// TODO: backEndStartTime = Sys_Milliseconds();

			// needed for editor rendering
			SetDefaultGLState();

			// upload any image loads that have completed
			idE.ImageManager.CompleteBackgroundLoading();

			foreach(RenderCommand cmd in commands)
			{
				switch(cmd.CommandID)
				{
					case RenderCommandType.Nop:
						break;

					case RenderCommandType.DrawView:
						DrawView((DrawViewRenderCommand) cmd);

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
						SetBuffer((SetBufferRenderCommand) cmd);
						setBufferCount++;
						break;

					case RenderCommandType.SwapBuffers:
						SwapBuffers((SwapBuffersRenderCommand) cmd);
						swapBufferCount++;
						break;

					case RenderCommandType.CopyRender:
						idConsole.WriteLine("TODO: RenderCommandType.CopyRender");
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

		private void FinishStageTexturing(MaterialStage stage, DrawSurface surface, Vertex[] position)
		{
			// unset privatePolygonOffset if necessary
			if((stage.PrivatePolygonOffset > 0) && (surface.Material.TestMaterialFlag(MaterialFlags.PolygonOffset) == false))
			{
				//Gl.glDisable(Gl.GL_POLYGON_OFFSET_FILL);
			}

			if((stage.Texture.TextureCoordinates == TextureCoordinateGeneration.DiffuseCube) || (stage.Texture.TextureCoordinates == TextureCoordinateGeneration.SkyboxCube) || (stage.Texture.TextureCoordinates == TextureCoordinateGeneration.WobbleSkyCube))
			{
				idConsole.WriteLine("TODO: FinishStageTexturing DiffuseCube");

				// TODO qglTexCoordPointer( 2, GL_FLOAT, sizeof( idDrawVert ), (void *)&ac->st );
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.Screen)
			{
				//Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
				//Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
				//Gl.glDisable(Gl.GL_TEXTURE_GEN_Q);
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.Screen2)
			{
				//Gl.glDisable(Gl.GL_TEXTURE_GEN_S);
				//Gl.glDisable(Gl.GL_TEXTURE_GEN_T);
				//Gl.glDisable(Gl.GL_TEXTURE_GEN_Q);
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.GlassWarp)
			{
				idConsole.WriteLine("TODO: FinishStageTexturing GlassWarp");

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
				idConsole.WriteLine("TODO: FinishStageTexturing ReflectCube");
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


		private bool GetModeInfo(ref int width, ref int height, int mode)
		{
			if((mode < -1) || (mode >= VideoModes.Length))
			{
				return false;
			}

			if(mode == -1)
			{
				width = idE.CvarSystem.GetInteger("r_customWidth");
				height = idE.CvarSystem.GetInteger("r_customHeight");

				return true;
			}

			VideoMode videoMode = VideoModes[mode];

			width = videoMode.Width;
			height = videoMode.Height;

			return true;
		}

		/// <summary>
		/// This handles the flipping needed when the view being rendered is a mirored view.
		/// </summary>
		/// <param name="type"></param>
		private void GL_Cull(CullType type)
		{
			if(idE.Backend.GLState.FaceCulling == type)
			{
				return;
			}

			if(type == CullType.TwoSided)
			{
				//Gl.glDisable(Gl.GL_CULL_FACE);
			}
			else
			{
				if(idE.Backend.GLState.FaceCulling == CullType.TwoSided)
				{
					//Gl.glDisable(Gl.GL_CULL_FACE);
				}

				if(type == CullType.TwoSided)
				{
					if(idE.Backend.ViewDefinition.IsMirror == true)
					{
						//Gl.glCullFace(Gl.GL_FRONT);
					}
					else
					{
						//Gl.glCullFace(Gl.GL_BACK);
					}
				}
				else
				{
					if(idE.Backend.ViewDefinition.IsMirror == true)
					{
						//Gl.glCullFace(Gl.GL_BACK);
					}
					else
					{
						//Gl.glCullFace(Gl.GL_FRONT);
					}
				}
			}

			idE.Backend.GLState.FaceCulling = type;
		}

		private void GL_SelectTexture(int unit)
		{
			if(idE.Backend.GLState.CurrentTextureUnit == unit)
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

				idE.Backend.GLState.CurrentTextureUnit = unit;
			}
		}

		/// <summary>
		/// This routine is responsible for setting the most commonly changed state.
		/// </summary>
		/// <param name="state"></param>
		private void GL_State(MaterialStates state)
		{
			MaterialStates diff;
			int srcFactor;
			int dstFactor;

			if((idE.CvarSystem.GetBool("r_useStateCaching") == false) || (idE.Backend.GLState.ForceState == true))
			{
				// make sure everything is set all the time, so we
				// can see if our delta checking is screwing up
				diff = MaterialStates.Invalid;
				idE.Backend.GLState.ForceState = false;
			}
			else
			{
				diff = state ^ idE.Backend.GLState.StateBits;

				if(diff == 0)
				{
					return;
				}
			}

			//
			// check depthFunc bits
			//
			if(diff.HasFlag(MaterialStates.DepthFunctionEqual | MaterialStates.DepthFunctionLess | MaterialStates.DepthFunctionAlways) == true)
			{
				if(state.HasFlag(MaterialStates.DepthFunctionEqual) == true)
				{
					//Gl.glDepthFunc(Gl.GL_EQUAL);
				}
				else if(state.HasFlag(MaterialStates.DepthFunctionAlways) == true)
				{
					//Gl.glDepthFunc(Gl.GL_ALWAYS);
				}
				else
				{
					//Gl.glDepthFunc(Gl.GL_LEQUAL);
				}
			}

			//
			// check blend bits
			//
			if(diff.HasFlag(MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits) == true)
			{
				switch(state & MaterialStates.SourceBlendBits)
				{
					case MaterialStates.SourceBlendZero:
						srcFactor = Gl.GL_ZERO;
						break;
					case MaterialStates.SourceBlendOne:
						srcFactor = Gl.GL_ONE;
						break;
					case MaterialStates.SourceBlendDestinationColor:
						srcFactor = Gl.GL_DST_COLOR;
						break;
					case MaterialStates.SourceBlendOneMinusDestinationColor:
						srcFactor = Gl.GL_ONE_MINUS_DST_COLOR;
						break;
					case MaterialStates.SourceBlendSourceAlpha:
						srcFactor = Gl.GL_SRC_ALPHA;
						break;
					case MaterialStates.SourceBlendOneMinusSourceAlpha:
						srcFactor = Gl.GL_ONE_MINUS_SRC_ALPHA;
						break;
					case MaterialStates.SourceBlendDestinationAlpha:
						srcFactor = Gl.GL_DST_ALPHA;
						break;
					case MaterialStates.SourceBlendOneMinusDestinationAlpha:
						srcFactor = Gl.GL_ONE_MINUS_DST_ALPHA;
						break;
					case MaterialStates.SourceBlendAlphaSaturate:
						srcFactor = Gl.GL_SRC_ALPHA_SATURATE;
						break;
					default:
						srcFactor = Gl.GL_ONE; // to get warning to shut up

						idConsole.Error("GL_State: invalid src blend state bits");
						break;
				}

				switch(state & MaterialStates.DestinationBlendBits)
				{
					case MaterialStates.DestinationBlendZero:
						dstFactor = Gl.GL_ZERO;
						break;
					case MaterialStates.DestinationBlendOne:
						dstFactor = Gl.GL_ONE;
						break;
					case MaterialStates.DestinationBlendSourceColor:
						dstFactor = Gl.GL_SRC_COLOR;
						break;
					case MaterialStates.DestinationBlendOneMinusSourceColor:
						dstFactor = Gl.GL_ONE_MINUS_SRC_COLOR;
						break;
					case MaterialStates.DestinationBlendSourceAlpha:
						dstFactor = Gl.GL_SRC_ALPHA;
						break;
					case MaterialStates.DestinationBlendOneMinusSourceAlpha:
						dstFactor = Gl.GL_ONE_MINUS_SRC_ALPHA;
						break;
					case MaterialStates.DestinationBlendDestinationAlpha:
						dstFactor = Gl.GL_DST_ALPHA;
						break;
					case MaterialStates.DestinationBlendOneMinusDestinationAlpha:
						dstFactor = Gl.GL_ONE_MINUS_DST_ALPHA;
						break;
					default:
						dstFactor = Gl.GL_ONE; // to get warning to shut up

						idConsole.Error("GL_State: invalid dst blend state bits");
						break;
				}

				//Gl.glBlendFunc(srcFactor, dstFactor);
			}

			//
			// check depthmask
			//
			if(diff.HasFlag(MaterialStates.DepthMask) == true)
			{
				if(state.HasFlag(MaterialStates.DepthMask) == true)
				{
					//Gl.glDepthMask(Gl.GL_FALSE);
				}
				else
				{
					//Gl.glDepthMask(Gl.GL_TRUE);
				}
			}

			//
			// check colormask
			//
			if(diff.HasFlag(MaterialStates.RedMask | MaterialStates.GreenMask | MaterialStates.BlueMask | MaterialStates.AlphaMask) == true)
			{
				int r = (diff.HasFlag(MaterialStates.RedMask) == true) ? 1 : 0;
				int g = (diff.HasFlag(MaterialStates.GreenMask) == true) ? 1 : 0;
				int b = (diff.HasFlag(MaterialStates.BlueMask) == true) ? 1 : 0;
				int a = (diff.HasFlag(MaterialStates.AlphaMask) == true) ? 1 : 0;

				//Gl.glColorMask(r, g, b, a);
			}

			//
			// fill/line mode
			//
			if(diff.HasFlag(MaterialStates.PolygonModeLine) == true)
			{
				if(state.HasFlag(MaterialStates.PolygonModeLine) == true)
				{
					//Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);					
				}
				else
				{
					//Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
				}
			}

			//
			// alpha test
			//
			if(diff.HasFlag(MaterialStates.AlphaTestBits) == true)
			{
				switch(state & MaterialStates.AlphaTestBits)
				{
					case 0:
						////Gl.glDisable(Gl.GL_ALPHA_TEST);
						break;

					case MaterialStates.AlphaTestEqual255:
						//Gl.glEnable(Gl.GL_ALPHA_TEST);
						//Gl.glAlphaFunc(Gl.GL_EQUAL, 1.0f);
						break;

					case MaterialStates.AlphaTestLessThan128:
						//Gl.glEnable(Gl.GL_ALPHA_TEST);
						//Gl.glAlphaFunc(Gl.GL_LESS, 0.5f);
						break;

					case MaterialStates.AlphaTestGreaterOrEqual128:
						//Gl.glEnable(Gl.GL_ALPHA_TEST);
						//Gl.glAlphaFunc(Gl.GL_GEQUAL, 0.5f);
						break;

					default:
						break;
				}
			}

			idE.Backend.GLState.StateBits = state;
		}

		private void GL_TextureEnvironment(int env)
		{
			if(env == idE.Backend.GLState.TextureUnits[idE.Backend.GLState.CurrentTextureUnit].TexEnv)
			{
				return;
			}

			idE.Backend.GLState.TextureUnits[idE.Backend.GLState.CurrentTextureUnit].TexEnv = env;

			switch(env)
			{
				case Gl.GL_COMBINE_EXT:
				case Gl.GL_MODULATE:
				case Gl.GL_REPLACE:
				case Gl.GL_DECAL:
				case Gl.GL_ADD:
					//Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, env);
					break;
				default:
					idConsole.Error("GL_TexEnv: invalid env '{0}' passed\n", env);
					break;
			}
		}

		private void InitCommands()
		{
			idE.CmdSystem.AddCommand("reloadARBPrograms", "reloads ARB programs", CommandFlags.Renderer, new EventHandler<CommandEventArgs>(Cmd_ReloadArbPrograms));

			// TODO
			/*cmdSystem->AddCommand( "MakeMegaTexture", idMegaTexture::MakeMegaTexture_f, CMD_FL_RENDERER|CMD_FL_CHEAT, "processes giant images" );
			cmdSystem->AddCommand( "sizeUp", R_SizeUp_f, CMD_FL_RENDERER, "makes the rendered view larger" );
			cmdSystem->AddCommand( "sizeDown", R_SizeDown_f, CMD_FL_RENDERER, "makes the rendered view smaller" );
			cmdSystem->AddCommand( "reloadGuis", R_ReloadGuis_f, CMD_FL_RENDERER, "reloads guis" );
			cmdSystem->AddCommand( "listGuis", R_ListGuis_f, CMD_FL_RENDERER, "lists guis" );
			cmdSystem->AddCommand( "touchGui", R_TouchGui_f, CMD_FL_RENDERER, "touches a gui" );
			cmdSystem->AddCommand( "screenshot", R_ScreenShot_f, CMD_FL_RENDERER, "takes a screenshot" );
			cmdSystem->AddCommand( "envshot", R_EnvShot_f, CMD_FL_RENDERER, "takes an environment shot" );
			cmdSystem->AddCommand( "makeAmbientMap", R_MakeAmbientMap_f, CMD_FL_RENDERER|CMD_FL_CHEAT, "makes an ambient map" );
			cmdSystem->AddCommand( "benchmark", R_Benchmark_f, CMD_FL_RENDERER, "benchmark" );
			cmdSystem->AddCommand( "gfxInfo", GfxInfo_f, CMD_FL_RENDERER, "show graphics info" );
			cmdSystem->AddCommand( "modulateLights", R_ModulateLights_f, CMD_FL_RENDERER | CMD_FL_CHEAT, "modifies shader parms on all lights" );
			cmdSystem->AddCommand( "testImage", R_TestImage_f, CMD_FL_RENDERER | CMD_FL_CHEAT, "displays the given image centered on screen", idCmdSystem::ArgCompletion_ImageName );
			cmdSystem->AddCommand( "testVideo", R_TestVideo_f, CMD_FL_RENDERER | CMD_FL_CHEAT, "displays the given cinematic", idCmdSystem::ArgCompletion_VideoName );
			cmdSystem->AddCommand( "reportSurfaceAreas", R_ReportSurfaceAreas_f, CMD_FL_RENDERER, "lists all used materials sorted by surface area" );
			cmdSystem->AddCommand( "reportImageDuplication", R_ReportImageDuplication_f, CMD_FL_RENDERER, "checks all referenced images for duplications" );
			cmdSystem->AddCommand( "regenerateWorld", R_RegenerateWorld_f, CMD_FL_RENDERER, "regenerates all interactions" );
			cmdSystem->AddCommand( "showInteractionMemory", R_ShowInteractionMemory_f, CMD_FL_RENDERER, "shows memory used by interactions" );
			cmdSystem->AddCommand( "showTriSurfMemory", R_ShowTriSurfMemory_f, CMD_FL_RENDERER, "shows memory used by triangle surfaces" );
			cmdSystem->AddCommand( "vid_restart", R_VidRestart_f, CMD_FL_RENDERER, "restarts renderSystem" );
			cmdSystem->AddCommand( "listRenderEntityDefs", R_ListRenderEntityDefs_f, CMD_FL_RENDERER, "lists the entity defs" );
			cmdSystem->AddCommand( "listRenderLightDefs", R_ListRenderLightDefs_f, CMD_FL_RENDERER, "lists the light defs" );
			cmdSystem->AddCommand( "listModes", R_ListModes_f, CMD_FL_RENDERER, "lists all video modes" );
			cmdSystem->AddCommand( "reloadSurface", R_ReloadSurface_f, CMD_FL_RENDERER, "reloads the decl and images for selected surface" );*/
		}

		private void InitCvars()
		{
			string[] renderArgs = new string[] { "best", "arb", "arb2", "Cg", "exp", "nv10", "nv20", "r200" };

			new idCvar("r_inhibitFragmentProgram", "0", "ignore the fragment program extension", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_glDriver", "", "\"opengl32\", etc.", CvarFlags.Renderer);
			new idCvar("r_useLightPortalFlow", "1", "use a more precise area reference determination", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_multiSamples", "0", "number of antialiasing samples", CvarFlags.ReadOnly | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("r_mode", "3", "video mode number", CvarFlags.ReadOnly | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("r_displayRefresh", "0", 0.0f, 200.0f, "optional display refresh rate option for vid mode", CvarFlags.Renderer | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("r_fullscreen", "1", "0 = windowed, 1 = full screen", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("r_customWidth", "720", "custom screen width. set r_mode to -1 to activate", CvarFlags.ReadOnly | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("r_customHeight", "486", "custom screen height. set r_mode to -1 to activate", CvarFlags.ReadOnly | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("r_singleTriangle", "0", "only draw a single triangle per primitive", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_checkBounds", "0", "compare all surface bounds with precalculated ones", CvarFlags.ReadOnly | CvarFlags.Bool);

			new idCvar("r_useNV20MonoLights", "1", "use pass optimization for mono lights", CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_useConstantMaterials", "1", "use pre-calculated material registers if possible", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useTripleTextureARB", "1", "cards with 3+ texture units do a two pass instead of three pass", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useSilRemap", "1", "consider verts with the same XYZ, but different ST the same for shadows", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useNodeCommonChildren", "1", "stop pushing reference bounds early when possible", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useShadowProjectedCull", "1", "discard triangles outside light volume before shadowing", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useShadowVertexProgram", "1", "do the shadow projection in the vertex program on capable cards", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useShadowSurfaceScissor", "1", "scissor shadows by the scissor rect of the interaction surfaces", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useInteractionTable", "1", "create a full entityDefs * lightDefs table to make finding interactions faster", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useTurboShadow", "1", "use the infinite projection with W technique for dynamic shadows", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useTwoSidedStencil", "1", "do stencil shadows in one pass with different ops on each side", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useDeferredTangents", "1", "defer tangents calculations after deform", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useCachedDynamicModels", "1", "cache snapshots of dynamic models", CvarFlags.ReadOnly | CvarFlags.Bool);

			new idCvar("r_useVertexBuffers", "1", 0, 1, "use ARB_vertex_buffer_object for vertexes", new ArgCompletion_Integer(0, 1), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_useIndexBuffers", "0", 0, 1, "use ARB_vertex_buffer_object for indexes", new ArgCompletion_Integer(0, 1), CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Integer);

			new idCvar("r_useStateCaching", "1", "avoid redundant state changes in GL_*() calls", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useInfiniteFarZ", "1", "use the no-far-clip-plane trick", CvarFlags.ReadOnly | CvarFlags.Bool);

			new idCvar("r_znear", "3", 0.001f, 200.0f, "near Z clip plane distance", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_swapInterval", "0", "changes wglSwapIntarval", CvarFlags.ReadOnly | CvarFlags.Archive | CvarFlags.Integer);

			new idCvar("r_gamma", "1", 0.5f, 3.0f, "changes gamma tables", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Float);
			new idCvar("r_brightness", "1", 0.5f, 2.0f, "changes gamma tables", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Float);

			new idCvar("r_renderer", "best", renderArgs, "hardware specific renderer path to use", new ArgCompletion_String(renderArgs), CvarFlags.Renderer | CvarFlags.Archive);

			new idCvar("r_jitter", "0", "randomly subpixel jitter the projection matrix", CvarFlags.ReadOnly | CvarFlags.Bool);

			new idCvar("r_skipSuppress", "0", "ignore the per-view suppressions", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipPostProcess", "0", "skip all post-process renderings", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipLightScale", "0", "don't do any post-interaction light scaling, makes things dim on low-dynamic range cards", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipInteractions", "0", "skip all light/surface interaction drawing", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipDynamicTextures", "0", "don't dynamically create textures", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipCopyTexture", "0", "do all rendering, but don't actually copyTexSubImage2D", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipBackEnd", "0", "don't draw anything", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipRender", "0", "skip 3D rendering, but pass 2D", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipRenderContext", "0", "NULL the rendering context during backend 3D rendering", CvarFlags.Renderer | CvarFlags.Bool);
			new idCvar("r_skipTranslucent", "0", "skip the translucent interaction rendering", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipAmbient", "0", "bypasses all non-interaction drawing", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipNewAmbient", "0", "bypasses all vertex/fragment program ambient drawing", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("r_skipBlendLights", "0", "skip all blend lights", CvarFlags.Renderer | CvarFlags.Bool);
			new idCvar("r_skipFogLights", "0", "skip all fog lights", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipDeforms", "0", "leave all deform materials in their original state", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipFrontEnd", "0", "bypasses all front end work, but 2D gui rendering still draws", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipUpdates", "0", "1 = don't accept any entity or light updates, making everything static", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipOverlays", "0", "skip overlay surfaces", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipSpecular", "0", "use black for specular1", CvarFlags.Renderer | CvarFlags.Bool | CvarFlags.Cheat | CvarFlags.Archive);
			new idCvar("r_skipBump", "0", "uses a flat surface instead of the bump map", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("r_skipDiffuse", "0", "use black for diffuse", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipROQ", "0", "skip ROQ decoding", CvarFlags.ReadOnly | CvarFlags.Bool);

			new idCvar("r_ignore", "0", "used for random debugging without defining new vars", CvarFlags.Renderer);
			new idCvar("r_ignore2", "0", "used for random debugging without defining new vars", CvarFlags.Renderer);
			new idCvar("r_usePreciseTriangleInteractions", "0", "1 = do winding clipping to determine if each ambiguous tri should be lit", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useCulling", "2", 0, 2, "0 = none, 1 = sphere, 2 = sphere + box", new ArgCompletion_Integer(0, 2), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_useLightCulling", "3", 0, 3, "0 = none, 1 = box, 2 = exact clip of polyhedron faces, 3 = also areas", new ArgCompletion_Integer(0, 3), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_useLightScissors", "1", "1 = use custom scissor rectangle for each light", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useClippedLightScissors", "1", 0, 2, "0 = full screen when near clipped, 1 = exact when near clipped, 2 = exact always", new ArgCompletion_Integer(0, 2), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_useEntityCulling", "1", "0 = none, 1 = box", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useEntityScissors", "0", "1 = use custom scissor rectangle for each entity", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useInteractionCulling", "1", "1 = cull interactions", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useInteractionScissors", "2", -2, 2, "1 = use a custom scissor rectangle for each shadow interaction, 2 = also crop using portal scissors", new ArgCompletion_Integer(-2, 2), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_useShadowCulling", "1", "try to cull shadows from partially visible lights", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useFrustumFarDistance", "0", "if != 0 force the view frustum far distance to this distance", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_logFile", "0", "number of frames to emit GL logs", CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_clear", "2", "force screen clear every frame, 1 = purple, 2 = black, 'r g b' = custom", CvarFlags.Renderer);
			new idCvar("r_offsetfactor", "0", "polygon offset parameter", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_offsetunits", "-600", "polygon offset parameter", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_shadowPolygonOffset", "-1", "bias value added to depth test for stencil shadow drawing", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_shadowPolygonFactor", "0", "scale value for stencil shadow drawing", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_frontBuffer", "0", "draw to front buffer for debugging", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_skipSubviews", "0", "1 = don't render any gui elements on surfaces", CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_skipGuiShaders", "0", 0, 3, "1 = skip all gui elements on surfaces, 2 = skip drawing but still handle events, 3 = draw but skip events", new ArgCompletion_Integer(0, 3), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_skipParticles", "0", 0, 1, "1 = skip all particle systems", new ArgCompletion_Integer(0, 1), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_subviewOnly", "0", "1 = don't render main view, allowing subviews to be debugged", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_shadows", "1", "enable shadows", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("r_testARBProgram", "0", "experiment with vertex/fragment programs", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_testGamma", "0", 0, 195, "if > 0 draw a grid pattern to test gamma levels", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_testGammaBias", "0", "if > 0 draw a grid pattern to test gamma levels", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_testStepGamma", "0", "if > 0 draw a grid pattern to test gamma levels", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_lightScale", "2", "all light intensities are multiplied by this", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_lightSourceRadius", "0", "for soft-shadow sampling", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_flareSize", "1", "scale the flare deforms from the material def", CvarFlags.Renderer | CvarFlags.Float);

			new idCvar("r_useExternalShadows", "1", 0, 2, "1 = skip drawing caps when outside the light volume, 2 = force to no caps for testing", new ArgCompletion_Integer(0, 2), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_useOptimizedShadows", "1", "use the dmap generated static shadow volumes", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useScissor", "1", "scissor clip as portals and lights are processed", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useCombinerDisplayLists", "1", "put all nvidia register combiner programming in display lists", CvarFlags.Renderer | CvarFlags.Bool | CvarFlags.NoCheat);
			new idCvar("r_useDepthBoundsTest", "1", "use depth bounds test to reduce shadow fill", CvarFlags.ReadOnly | CvarFlags.Bool);

			new idCvar("r_screenFraction", "100", "for testing fill rate, the resolution of the entire screen can be changed", CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_demonstrateBug", "0", "used during development to show IHV's their problems", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_usePortals", "1", " 1 = use portals to perform area culling, otherwise draw everything", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_singleLight", "-1", "suppress all but one light", CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_singleEntity", "-1", "suppress all but one entity", CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_singleSurface", "-1", "suppress all but one surface on each entity", CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_singleArea", "0", "only draw the portal area the view is actually in", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_forceLoadImages", "0", "draw all images to screen after registration", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("r_orderIndexes", "1", "perform index reorganization to optimize vertex use", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_lightAllBackFaces", "0", "light all the back faces, even when they would be shadowed", CvarFlags.ReadOnly | CvarFlags.Bool);

			// visual debugging info
			new idCvar("r_showPortals", "0", "draw portal outlines in color based on passed / not passed", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showUnsmoothedTangents", "0", "if 1, put all nvidia register combiner programming in display lists", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showSilhouette", "0", "highlight edges that are casting shadow planes", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showVertexColor", "0", "draws all triangles with the solid vertex color", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showUpdates", "0", "report entity and light updates and ref counts", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showDemo", "0", "report reads and writes to the demo file", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showDynamic", "0", "report stats on dynamic surface generation", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showLightScale", "0", "report the scale factor applied to drawing for overbrights", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showDefs", "0", "report the number of modeDefs and lightDefs in view", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showTrace", "0", "show the intersection of an eye trace with the world", new ArgCompletion_Integer(0, 2), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showIntensity", "0", "draw the screen colors based on intensity, red = 0, green = 128, blue = 255", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showImages", "0", 0, 2, "1 = show all images instead of rendering, 2 = show in proportional size", new ArgCompletion_Integer(0, 2), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showSmp", "0", "show which end (front or back) is blocking", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showLights", "0", 0, 3, "1 = just print volumes numbers, highlighting ones covering the view, 2 = also draw planes of each volume, 3 = also draw edges of each volume", new ArgCompletion_Integer(0, 3), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showShadows", "0", 0, 3, "1 = visualize the stencil shadow volumes, 2 = draw filled in", new ArgCompletion_Integer(0, 3), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showShadowCount", "0", 0, 4, "colors screen based on shadow volume depth complexity, >= 2 = print overdraw count based on stencil index values, 3 = only show turboshadows, 4 = only show static shadows", new ArgCompletion_Integer(0, 4), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showLightScissors", "0", "show light scissor rectangles", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showEntityScissors", "0", "show entity scissor rectangles", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showInteractionFrustums", "0", 0, 3, "1 = show a frustum for each interaction, 2 = also draw lines to light origin, 3 = also draw entity bbox", new ArgCompletion_Integer(0, 3), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showInteractionScissors", "0", 0, 2, "1 = show screen rectangle which contains the interaction frustum, 2 = also draw construction lines", new ArgCompletion_Integer(0, 2), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showLightCount", "0", 0, 3, "1 = colors surfaces based on light count, 2 = also count everything through walls, 3 = also print overdraw", new ArgCompletion_Integer(0, 3), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showViewEntitys", "0", "1 = displays the bounding boxes of all view models, 2 = print index numbers", CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showTris", "0", 0, 3, "enables wireframe rendering of the world, 1 = only draw visible ones, 2 = draw all front facing, 3 = draw all", new ArgCompletion_Integer(0, 3), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showSurfaceInfo", "0", "show surface material name under crosshair", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showNormals", "0", "draws wireframe normals", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_showMemory", "0", "print frame memory utilization", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showCull", "0", "report sphere and box culling stats", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showInteractions", "0", "report interaction generation activity", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showDepth", "0", "display the contents of the depth buffer and the depth range", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showSurfaces", "0", "report surface/light/shadow counts", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showPrimitives", "0", "report drawsurf/index/vertex counts", CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showEdges", "0", "draw the sil edges", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showTexturePolarity", "0", "shade triangles by texture area polarity", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showTangentSpace", "0", 0, 3, "shade triangles by tangent space, 1 = use 1st tangent vector, 2 = use 2nd tangent vector, 3 = use normal vector", new ArgCompletion_Integer(0, 3), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_showDominantTri", "0", "draw lines from vertexes to center of dominant triangles", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showAlloc", "0", "report alloc/free counts", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_showTextureVectors", "0", " if > 0 draw each triangles texture (tangent) vectors", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_showOverDraw", "0", 0, 3, "1 = geometry overdraw, 2 = light interaction overdraw, 3 = geometry and light interaction overdraw", new ArgCompletion_Integer(0, 3), CvarFlags.Renderer | CvarFlags.Integer);

			new idCvar("r_lockSurfaces", "0", "allow moving the view point without changing the composition of the scene, including culling", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_useEntityCallbacks", "1", "if 0, issue the callback immediately at update time, rather than defering", CvarFlags.ReadOnly | CvarFlags.Bool);

			new idCvar("r_showSkel", "0", 0, 2, "draw the skeleton when model animates, 1 = draw model with skeleton, 2 = draw skeleton only", new ArgCompletion_Integer(0, 2), CvarFlags.Renderer | CvarFlags.Integer);
			new idCvar("r_jointNameScale", "0.02", "size of joint names when r_showskel is set to 1", CvarFlags.Renderer | CvarFlags.Float);
			new idCvar("r_jointNameOffset", "0.5", "offset of joint names when r_showskel is set to 1", CvarFlags.Renderer | CvarFlags.Float);

			new idCvar("r_cgVertexProfile", "best", "arbvp1, vp20, vp30", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("r_cgFragmentProfile", "best", "arbfp1, fp30", CvarFlags.Renderer | CvarFlags.Archive);

			new idCvar("r_debugLineDepthTest", "0", "perform depth test on debug lines", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("r_debugLineWidth", "1", "width of debug lines", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("r_debugArrowStep", "120", 0, 120, "step size of arrow cone line rotation in degrees", CvarFlags.ReadOnly | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("r_debugPolygonFilled", "1", "draw a filled polygon", CvarFlags.ReadOnly | CvarFlags.Bool);

			new idCvar("r_materialOverride", "", "overrides all materials", CvarFlags.Renderer/* TODO: , idCmdSystem::ArgCompletion_Decl<DECL_MATERIAL> */);

			new idCvar("r_debugRenderToTexture", "0", "", CvarFlags.Renderer | CvarFlags.Integer);
		}

		private void InitMaterials()
		{
			_defaultMaterial = idE.DeclManager.FindMaterial("_default", false);

			if(_defaultMaterial == null)
			{
				idConsole.FatalError("_default material not found");
			}

			idE.DeclManager.FindMaterial("_default", false);

			// needed by R_DeriveLightData
			idE.DeclManager.FindMaterial("lights/defaultPointLight");
			idE.DeclManager.FindMaterial("lights/defaultProjectedLight");
		}

		private void IssueRenderCommands()
		{
			if((_frameData.Commands.Peek().CommandID == RenderCommandType.Nop) && (_frameData.Commands.Count == 1))
			{
				// nothing to issue
				return;
			}

			// r_skipBackEnd allows the entire time of the back end
			// to be removed from performance measurements, although
			// nothing will be drawn to the screen.  If the prints
			// are going to a file, or r_skipBackEnd is later disabled,
			// usefull data can be received.

			// r_skipRender is usually more usefull, because it will still
			// draw 2D graphics
			if(idE.CvarSystem.GetBool("r_skipBackEnd") == false)
			{
				ExecuteBackEndCommands(_frameData.Commands);
			}

			ClearCommandChain();
		}

		private void LoadArbProgram(int index)
		{
			int startOffset = 0, endOffset = 0;
			GLProgram prog = _programs[index];
			string progPath = Path.Combine("glprogs", prog.Name);

			// load the program even if we don't support it, so
			// fs_copyfiles can generate cross-platform data dumps
			byte[] data = idE.FileSystem.ReadFile(progPath);

			if(data == null)
			{
				idConsole.WriteLine("{0}: File not found", progPath);
			}
			else
			{
				string buffer = Encoding.UTF8.GetString(data);

				if(idE.RenderSystem.IsRunning == false)
				{
					return;
				}

				//
				// submit the program string at start to GL
				//
				if(prog.Type == 0)
				{
					// allocate a new identifier for this program
					_programs[index].Type = prog.Type = (int) GLProgramType.User + index;
				}

				// vertex and fragment programs can both be present in a single file, so
				// scan for the proper header to be the start point, and stamp a 0 in after the end

				if(prog.Target == Gl.GL_VERTEX_PROGRAM_ARB)
				{
					if(idE.GLConfig.ArbVertexProgramAvailable == false)
					{
						idConsole.WriteLine("{0}: GL_VERTEX_PROGRAM_ARB not available", progPath);
						return;
					}

					startOffset = buffer.IndexOf("!!ARBvp");
				}
				else if(prog.Target == Gl.GL_FRAGMENT_PROGRAM_ARB)
				{
					if(idE.GLConfig.ArbFragmentProgramAvailable == false)
					{
						idConsole.WriteLine("{0}: GL_FRAGMENT_PROGRAM_ARB not available", progPath);
						return;
					}

					startOffset = buffer.IndexOf("!!ARBfp");
				}

				if(startOffset == -1)
				{
					idConsole.WriteLine("{0}: !!ARB not found", progPath);
					return;
				}

				endOffset = buffer.IndexOf("END", startOffset);

				if(endOffset == -1)
				{
					idConsole.WriteLine("{0}: END not found", progPath);
					return;
				}

				endOffset += 3;

				//Gl.glBindProgramARB(prog.Target, prog.Type);
				//Gl.glGetError();

				int progBufferLength = endOffset - startOffset;
				byte[] progBuffer = new byte[progBufferLength];
				Array.Copy(data, startOffset, progBuffer, 0, progBufferLength);

				//Gl.glProgramStringARB(prog.Target, Gl.GL_PROGRAM_FORMAT_ASCII_ARB, progBufferLength, progBuffer);

				int error = 0;//Gl.glGetError();
				int programErrorPosition = 0;

				//Gl.glGetIntegerv(Gl.GL_PROGRAM_ERROR_POSITION_ARB, out programErrorPosition);

				if(error == Gl.GL_INVALID_OPERATION)
				{
					string errorString = Gl.glGetString(Gl.GL_PROGRAM_ERROR_STRING_ARB);

					idConsole.WriteLine("{0}:", progPath);
					idConsole.WriteLine("GL_PROGRAM_ERROR_STRING_ARB: {0}", errorString);

					if(programErrorPosition < 0)
					{
						idConsole.WriteLine("GL_PROGRAM_ERROR_POSITION_ARB < 0 with error");
					}
					else if(programErrorPosition >= progBufferLength)
					{
						idConsole.WriteLine("error at end of program");
					}
					else
					{
						idConsole.WriteLine("error at {0}:", progBufferLength);
						idConsole.WriteLine(Encoding.UTF8.GetString(progBuffer).Substring(programErrorPosition));
					}

					return;
				}

				if(programErrorPosition != -1)
				{
					idConsole.WriteLine("{0}:", progPath);
					idConsole.WriteLine("GL_PROGRAM_ERROR_POSITION_ARB != -1 without error");
				}
				else
				{
					idConsole.WriteLine(progPath);
				}
			}
		}

		private void PrepareStageTexturing(MaterialStage stage, DrawSurface surface, Vertex[] position)
		{
			// set privatePolygonOffset if necessary
			if(stage.PrivatePolygonOffset > 0)
			{
				//Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
				//Gl.glPolygonOffset(idE.CvarSystem.GetFloat("r_offsetFactor"), idE.CvarSystem.GetFloat("r_offsetUnits") * stage.PrivatePolygonOffset);
			}

			// set the texture matrix if needed
			if(stage.Texture.HasMatrix == true)
			{
				idConsole.WriteLine("TODO: LoadShaderTextureMatrix(surface.ShaderRegisters, stage.Texture);");
			}

			// texgens
			if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.DiffuseCube)
			{
				idConsole.WriteLine("TODO: TexGen DiffuseCube");
				// TODO: Gl.glTexCoordPointer(3, Gl.GL_FLOAT, sizeof( idVertex ), new float[] { position.Normal.X, position.Normal.Y, position.Normal.Z });
			}
			else if((stage.Texture.TextureCoordinates == TextureCoordinateGeneration.SkyboxCube) || (stage.Texture.TextureCoordinates == TextureCoordinateGeneration.WobbleSkyCube))
			{
				idConsole.WriteLine("TODO: TexGen SkyboxCube | WobbleSky");
				// TODO: Gl.glTexCoordPointer(3, Gl.GL_FLOAT, 0, vertexCache.Position( surf->dynamicTexCoords));
			}
			else if(stage.Texture.TextureCoordinates == TextureCoordinateGeneration.Screen)
			{
				idConsole.WriteLine("TODO: TexGen Screen");

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
				idConsole.WriteLine("TODO: TexGen Screen2");
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
				idConsole.WriteLine("TODO: TexGen GlassWarp");

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
				idConsole.WriteLine("TODO: TexGen ReflectCube");

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

		private void RenderShaderPasses(DrawSurface surface)
		{
			Surface tri = surface.Geometry;
			idMaterial material = surface.Material;

			if(material.HasAmbient == false)
			{
				return;
			}

			if(material.IsPortalSky == true)
			{
				return;
			}

			// change the matrix if needed
			// TODO
			/*if ( surf->space != backEnd.currentSpace ) {
				qglLoadMatrixf( surf->space->modelViewMatrix );
				backEnd.currentSpace = surf->space;
				RB_SetProgramEnvironmentSpace();
			}*/

			// change the scissor if needed
			if((idE.CvarSystem.GetBool("r_useScissor") == true) && (idE.Backend.CurrentScissor != surface.ScissorRectangle))
			{
				idE.Backend.CurrentScissor = surface.ScissorRectangle;

				/*//Gl.glScissor(idE.Backend.ViewDefinition.ViewPort.X1 + idE.Backend.CurrentScissor.X1,
					idE.Backend.ViewDefinition.ViewPort.Y1 + idE.Backend.CurrentScissor.Y1,
					idE.Backend.CurrentScissor.X2 + 1 - idE.Backend.CurrentScissor.X1,
					idE.Backend.CurrentScissor.Y2 + 1 - idE.Backend.CurrentScissor.Y1);*/
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
			GL_Cull(material.CullType);

			// set polygon offset if necessary
			if(material.TestMaterialFlag(MaterialFlags.PolygonOffset) == true)
			{
				idConsole.WriteLine("TODO: polygon offset fill");
				//Gl.glEnable(Gl.GL_POLYGON_OFFSET_FILL);
				//Gl.glPolygonOffset(idE.CvarSystem.GetFloat("r_offsetFactor"), idE.CvarSystem.GetFloat("r_offsetUnits") * material.PolygonOffset);
			}

			// TODO: weapon depth hack
			/*if ( surf->space->weaponDepthHack ) {
				RB_EnterWeaponDepthHack();
			}

			if ( surf->space->modelDepthHack != 0.0f ) {
				RB_EnterModelDepthHack( surf->space->modelDepthHack );
			}*/

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
					continue;
				}

				// skip if the stage is ( GL_ZERO, GL_ONE ), which is used for some alpha masks
				if(stage.DrawStateBits.HasFlag(MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits) == true)
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

					idConsole.WriteLine("TODO: render");
					/*Gl.glColorPointer(4, Gl.GL_UNSIGNED_BYTE, Marshal.SizeOf(typeof(Vertex)), (void*) &ambientCacheData->color);
					Gl.glVertexAttribPointerARB(9, 3, Gl.GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), ambientCacheData->tangents[0].ToFloatPtr());
					Gl.glVertexAttribPointerARB(10, 3, Gl.GL_FLOAT, false, Marshal.SizeOf(typeof(Vertex)), ambientCacheData->tangents[1].ToFloatPtr());
					Gl.glNormalPointer(Gl.GL_FLOAT, Marshal.SizeOf(typeof(Vertex)), ambientCacheData->normal.ToFloatPtr());*/

					//Gl.glEnableClientState(Gl.GL_COLOR_ARRAY);
					//Gl.glEnableVertexAttribArray(9);
					//Gl.glEnableVertexAttribArray(10);
					//Gl.glEnableClientState(Gl.GL_NORMAL_ARRAY);

					GL_State(stage.DrawStateBits);

					idConsole.WriteLine("TODO: glBindProgramARB");
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

					for(int i = 0; i < newStage.VertexParameters.Length; i++)
					{
						float[] parm = new float[4];
						parm[0] = registers[newStage.VertexParameters[i, 0]];
						parm[1] = registers[newStage.VertexParameters[i, 1]];
						parm[2] = registers[newStage.VertexParameters[i, 2]];
						parm[3] = registers[newStage.VertexParameters[i, 3]];

						//Gl.glProgramLocalParameter4fvARB(Gl.GL_VERTEX_PROGRAM_ARB, i, parm);
					}

					for(int i = 0; i < newStage.FragmentProgramImages.Length; i++)
					{
						if(newStage.FragmentProgramImages[i] != null)
						{
							GL_SelectTexture(i);
							newStage.FragmentProgramImages[i].Bind();
						}
					}

					//Gl.glBindProgramARB(Gl.GL_FRAGMENT_PROGRAM_ARB, newStage.FragmentProgram);
					//Gl.glEnable(Gl.GL_FRAGMENT_PROGRAM_ARB);

					// draw it
					DrawElementsWithCounters(tri);

					for(int i = 1; i < newStage.FragmentProgramImages.Length; i++)
					{
						if(newStage.FragmentProgramImages[i] != null)
						{
							GL_SelectTexture(i);
							idE.ImageManager.BindNullTexture();
						}
					}

					// TODO: megatexture
					/*if ( newStage->megaTexture ) {
						newStage->megaTexture->Unbind();
					}*/

					GL_SelectTexture(0);

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
					if((stage.DrawStateBits.HasFlag(MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits) == true)
						&& (color[0] <= 0) && (color[1] <= 0) && (color[2] <= 0))
					{
						continue;
					}

					// skip the entire stage if a blend would be completely transparent
					if((stage.DrawStateBits.HasFlag(MaterialStates.SourceBlendBits | MaterialStates.DestinationBlendBits) == true)
						&& (color[3] <= 0))
					{
						continue;
					}

					// select the vertex color source
					if(stage.VertexColor == StageVertexColor.Ignore)
					{
						_basicEffect.DiffuseColor = new Vector3(color[0], color[1], color[2]);
						_basicEffect.Alpha = color[3];						
					}
					else
					{
						if(stage.VertexColor == StageVertexColor.InverseModulate)
						{
							idConsole.WriteLine("TODO: InverseModulate");
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
							GL_SelectTexture(1);
							idE.ImageManager.WhiteImage.Bind();
							idConsole.WriteLine("TODO: vertex color");
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
					GL_State(stage.DrawStateBits);

					PrepareStageTexturing(stage, surface, tri.AmbientCache.Data);

					// draw it
					DrawElementsWithCounters(tri);

					FinishStageTexturing(stage, surface, tri.AmbientCache.Data);

					if(stage.VertexColor != StageVertexColor.Ignore)
					{
						idConsole.WriteLine("TODO: SVC ignore");
						/*GL.DisableClientState(ArrayCap.ColorArray);*/

						GL_SelectTexture(1);
						GL_TextureEnvironment(Gl.GL_MODULATE);

						idE.ImageManager.BindNullTexture();

						GL_SelectTexture(0);
						GL_TextureEnvironment(Gl.GL_MODULATE);
					}
				}

				// reset polygon offset
				if(material.TestMaterialFlag(MaterialFlags.PolygonOffset) == true)
				{
					//Gl.glDisable(Gl.GL_POLYGON_OFFSET_FILL);
				}

				// TODO: weapon depth hack
				/*if ( surf->space->weaponDepthHack || surf->space->modelDepthHack != 0.0f ) {
					RB_LeaveDepthHack();
				}*/
			}
		}

		/// <summary>
		/// Check for changes in the back end renderSystem, possibly invalidating cached data.
		/// </summary>
		private void SetBackEndRenderer()
		{
			if(idE.CvarSystem.IsModified("r_renderer") == false)
			{
				return;
			}

			bool oldVPState = _backEndRendererHasVertexPrograms;
			string renderer = idE.CvarSystem.GetString("r_renderer").ToLower();

			_backEndRenderer = BackEndRenderer.Bad;

			if(renderer == "arb")
			{
				_backEndRenderer = BackEndRenderer.ARB;
			}
			else if(renderer == "arb2")
			{
				if(idE.GLConfig.AllowArb2Path == true)
				{
					_backEndRenderer = BackEndRenderer.ARB2;
				}
			}
			else if(renderer == "nv10")
			{
				if(idE.GLConfig.AllowNV10Path == true)
				{
					_backEndRenderer = BackEndRenderer.NV10;
				}
			}
			else if(renderer == "nv20")
			{
				if(idE.GLConfig.AllowNV20Path == true)
				{
					_backEndRenderer = BackEndRenderer.NV20;
				}
			}
			else if(renderer == "r200")
			{
				if(idE.GLConfig.AllowR200Path == true)
				{
					_backEndRenderer = BackEndRenderer.R200;
				}
			}

			// fallback
			if(_backEndRenderer == BackEndRenderer.Bad)
			{
				// choose the best
				if(idE.GLConfig.AllowArb2Path == true)
				{
					_backEndRenderer = BackEndRenderer.ARB2;
				}
				else if(idE.GLConfig.AllowR200Path == true)
				{
					_backEndRenderer = BackEndRenderer.R200;
				}
				else if(idE.GLConfig.AllowNV20Path == true)
				{
					_backEndRenderer = BackEndRenderer.NV20;
				}
				else if(idE.GLConfig.AllowNV10Path == true)
				{
					_backEndRenderer = BackEndRenderer.NV10;
				}
				else
				{
					// the others are considered experimental
					_backEndRenderer = BackEndRenderer.ARB;
				}
			}

			_backEndRendererHasVertexPrograms = false;
			_backEndRendererMaxLight = 1.0f;

			switch(_backEndRenderer)
			{
				case BackEndRenderer.ARB:
					idConsole.WriteLine("using ARB renderSystem");
					break;
				case BackEndRenderer.NV10:
					idConsole.WriteLine("using NV10 renderSystem");
					break;
				case BackEndRenderer.NV20:
					idConsole.WriteLine("using NV20 renderSystem");

					_backEndRendererHasVertexPrograms = true;
					break;
				case BackEndRenderer.R200:
					idConsole.WriteLine("using R200 renderSystem");

					_backEndRendererHasVertexPrograms = true;
					break;
				case BackEndRenderer.ARB2:
					idConsole.WriteLine("using ARB2 renderSystem");

					_backEndRendererHasVertexPrograms = true;
					_backEndRendererMaxLight = 999;
					break;
			}

			// clear the vertex cache if we are changing between
			// using vertex programs and not, because specular and
			// shadows will be different data
			// TODO
			/*if ( oldVPstate != backEndRendererHasVertexPrograms ) {
				vertexCache.PurgeAll();
				if ( primaryWorld ) {
					primaryWorld->FreeInteractions();
				}
			}*/

			idE.CvarSystem.ClearModified("r_renderer");
		}

		private void SetBuffer(SetBufferRenderCommand cmd)
		{
			// see which draw buffer we want to render the frame to
			idE.Backend.FrameCount = cmd.FrameCount;

			// clear screen for debugging
			// automatically enable this with several other debug tools
			// that might leave unrendered portions of the screen
			if((idE.CvarSystem.GetFloat("r_clear") > 0)
				|| (idE.CvarSystem.GetString("r_clear").Length != 1)
				|| (idE.CvarSystem.GetBool("r_lockSurfaces") == true)
				|| (idE.CvarSystem.GetBool("r_singleArea") == true)
				|| (idE.CvarSystem.GetBool("r_showOverDraw") == true))
			{
				Color color = Microsoft.Xna.Framework.Color.Gold;
				string[] parts = idE.CvarSystem.GetString("r_clear").Split(' ');

				// TODO: clear color
				/*if(parts.Length == 3)
				{
					float.TryParse(parts[0], out color[0]);
					float.TryParse(parts[1], out color[1]);
					float.TryParse(parts[2], out color[2]);

					color = Color.FromNonPremultiplied(color[0], color[1], color[2], 1);
				}
				else if(idE.CvarSystem.GetInteger("r_clear") == 2)
				{
					color = Color.FromNonPremultiplied(0.0f, 0.0f, 0.0f, 1.0f);
				}
				else if(idE.CvarSystem.GetBool("r_showOverDraw") == true)
				{
					color = Color.FromNonPremultiplied(1.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					color = Color.FromNonPremultiplied(0.4f, 0.0f, 0.25f, 1.0f);
				}*/

				_graphicsDevice.Clear(color);
			}
		}

		private void SetColorMappings()
		{
			float g = idE.CvarSystem.GetFloat("r_gamma");
			float b = idE.CvarSystem.GetFloat("r_brightness");
			int f, j, inf;

			for(int i = 0; i < 256; i++)
			{
				j = (int) (i * b);

				if(j > 255)
				{
					j = 255;
				}

				if(g == 1)
				{
					inf = (j << 8) | j;
				}
				else
				{
					inf = (int) (0xffff * idMath.Pow(j / 255.0f, 1.0f / g) + 0.5f);
				}

				if(inf < 0)
				{
					inf = 0;
				}

				if(inf > 0xffff)
				{
					inf = 0xffff;
				}

				_gammaTable[i] = (ushort) inf;
			}

			SetGamma(_gammaTable, _gammaTable, _gammaTable);
		}

		/// <summary>
		/// This should initialize all GL state that any part of the entire program
		/// may touch, including the editor.
		/// </summary>
		private void SetDefaultGLState()
		{
			// TODO: RB_LogComment("--- R_SetDefaultGLState ---\n");

			//Gl.glClearDepth(1.0f);
			//Gl.glColor4f(1, 1, 1, 1);

			// the vertex array is always enabled
			//Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
			//Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);
			//Gl.glDisableClientState(Gl.GL_COLOR_ARRAY);

			//
			// make sure our GL state vector is set correctly
			//
			idE.Backend.GLState = new GLState();
			idE.Backend.GLState.ForceState = true;

			//Gl.glColorMask(1, 1, 1, 1);

			//Gl.glEnable(Gl.GL_DEPTH_TEST);
			//Gl.glEnable(Gl.GL_BLEND);
			//Gl.glEnable(Gl.GL_SCISSOR_TEST);
			//Gl.glEnable(Gl.GL_CULL_FACE);
			//Gl.glDisable(Gl.GL_LIGHTING);
			//Gl.glDisable(Gl.GL_LINE_STIPPLE);
			//Gl.glDisable(Gl.GL_STENCIL_TEST);

			//Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
			//Gl.glDepthMask(Gl.GL_TRUE);
			//Gl.glDepthFunc(Gl.GL_ALWAYS);

			//Gl.glCullFace(Gl.GL_FRONT_AND_BACK);
			//Gl.glShadeModel(Gl.GL_SMOOTH);

			if(idE.CvarSystem.GetBool("r_useScissor") == true)
			{
				//	_graphicsDevice.ScissorRectangle = new Rectangle(0, 0, idE.GLConfig.VideoWidth, idE.GLConfig.VideoHeight);
			}

			for(int i = idE.GLConfig.MaxTextureUnits - 1; i >= 0; i--)
			{
				GL_SelectTexture(i);

				// object linear texgen is our default
				//Gl.glTexGenf(Gl.GL_S, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);
				//Gl.glTexGenf(Gl.GL_T, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);
				//Gl.glTexGenf(Gl.GL_R, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);
				//Gl.glTexGenf(Gl.GL_Q, Gl.GL_TEXTURE_GEN_MODE, Gl.GL_OBJECT_LINEAR);

				//GL_TextureEnvironment(Gl.GL_MODULATE);
				//Gl.glDisable(Gl.GL_TEXTURE_2D);

				if(idE.GLConfig.Texture3DAvailable == true)
				{
					//Gl.glDisable(Gl.GL_TEXTURE_3D);
				}

				if(idE.GLConfig.CubeMapAvailable == true)
				{
					//Gl.glDisable(Gl.GL_TEXTURE_CUBE_MAP_EXT);
				}
			}
		}

		/// <summary>
		/// Draw all the images to the screen, on top of whatever
		/// was there.  This is used to test for texture thrashing.
		/// </summary>
		private void ShowImages()
		{
			idConsole.WriteLine("TODO: ShowImages");

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

		private void SwapBuffers(SwapBuffersRenderCommand cmd)
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

					idConsole.WriteLine("TODO: r_swapInterval");

					// Wgl.wglSwapIntervalEXT(idE.CvarSystem.GetInteger("r_swapInterval"));
				}
			}
		}

		private void ToggleSmpFrame()
		{
			if(idE.CvarSystem.GetBool("r_lockSurfaces") == true)
			{
				return;
			}

			// idConsole.WriteLine("TODO: ToggleSmpFrame");

			// TODO
			/*R_FreeDeferredTriSurfs( frameData );

			// clear frame-temporary data
			frameData_t		*frame;
			frameMemoryBlock_t	*block;

			// update the highwater mark
			R_CountFrameData();

			frame = frameData;

			// reset the memory allocation to the first block
			frame->alloc = frame->memory;

			// clear all the blocks
			for ( block = frame->memory ; block ; block = block->next ) {
				block->used = 0;
			}*/

			ClearCommandChain();
		}

		private void UnbindIndex()
		{
			//Gl.glBindBufferARB(Gl.GL_ELEMENT_ARRAY_BUFFER_ARB, 0);
		}
		#endregion

		#region GLimp* - to be refactored in to separate render lib
		/// <summary>
		/// The renderer calls this when the user adjusts r_gamma or r_brightness.
		/// </summary>
		/// <param name="red"></param>
		/// <param name="green"></param>
		/// <param name="blue"></param>
		private void SetGamma(ushort[] red, ushort[] green, ushort[] blue)
		{
			ushort[,] table = new ushort[3, 256];

			for(int i = 0; i < 256; i++)
			{
				table[0, i] = red[i];
				table[1, i] = green[i];
				table[2, i] = blue[i];
			}

			idConsole.WriteLine("TODO: SetDeviceGammaRamp");
			/*if ( !SetDeviceGammaRamp( win32.hDC, table ) ) {
				common->Printf( "WARNING: SetDeviceGammaRamp failed.\n" );
			}*/
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
			int width = idE.Backend.ViewDefinition.ViewPort.X2 - idE.Backend.ViewDefinition.ViewPort.X1 + 1;
			pot = idE.ImageManager.CurrentRenderImage.Width;
			parameters[0] = width / pot;

			int height = idE.Backend.ViewDefinition.ViewPort.Y2 - idE.Backend.ViewDefinition.ViewPort.Y1 + 1;
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
			parameters[0] = idE.Backend.ViewDefinition.RenderView.ViewOrigin.X;
			parameters[1] = idE.Backend.ViewDefinition.RenderView.ViewOrigin.Y;
			parameters[2] = idE.Backend.ViewDefinition.RenderView.ViewOrigin.Z;
			parameters[3] = 1.0f;

			//Gl.glProgramEnvParameter4fvARB(Gl.GL_VERTEX_PROGRAM_ARB, 1, parameters);
		}
		#endregion

		#region NV10
		private void InitNV10()
		{
			idE.GLConfig.AllowNV10Path = idE.GLConfig.RegisterCombinersAvailable;
		}
		#endregion

		#region NV20
		private void InitNV20()
		{
			idE.GLConfig.AllowNV20Path = false;

			idConsole.WriteLine("---------- R_NV20_Init ----------");

			if((idE.GLConfig.RegisterCombinersAvailable == false) || (idE.GLConfig.ArbVertexProgramAvailable == false) || (idE.GLConfig.MaxTextureUnits < 4))
			{
				idConsole.WriteLine("Not available.");
			}
			else
			{
				//CheckOpenGLErrors();

				// create our "fragment program" display lists
				//_fragmentDisplayListBase = Gl.glGenLists((int) FragmentProgram.Count);

				// force them to issue commands to build the list
				bool temp = idE.CvarSystem.GetBool("r_useCombinerDisplayLists");
				idE.CvarSystem.SetBool("r_useCombinerDisplayLists", false);

				/*//Gl.glNewList(_fragmentDisplayListBase + (int) FragmentProgram.BumpAndLight, Gl.GL_COMPILE);
				NV20_BumpAndLightFragment();
				//Gl.glEndList();

				Gl.glNewList(_fragmentDisplayListBase + (int) FragmentProgram.DiffuseColor, Gl.GL_COMPILE);
				NV20_DiffuseColorFragment();
				Gl.glEndList();

				Gl.glNewList(_fragmentDisplayListBase + (int) FragmentProgram.SpecularColor, Gl.GL_COMPILE);
				NV20_SpecularColorFragment();
				Gl.glEndList();

				Gl.glNewList(_fragmentDisplayListBase + (int) FragmentProgram.DiffuseAndSpecularColor, Gl.GL_COMPILE);
				NV20_DiffuseAndSpecularColorFragment();
				Gl.glEndList();*/

				idE.CvarSystem.SetBool("r_useCombinerDisplayLists", temp);

				idConsole.WriteLine("---------------------------------");

				idE.GLConfig.AllowNV20Path = true;
			}
		}

		private void NV20_BumpAndLightFragment()
		{
			if(idE.CvarSystem.GetBool("r_useCombinerDisplayLists") == true)
			{
				Gl.glCallList(_fragmentDisplayListBase + (int) FragmentProgram.BumpAndLight);
			}
			else
			{
				// program the nvidia register combiners
				Gl.glCombinerParameteriNV(Gl.GL_NUM_GENERAL_COMBINERS_NV, 3);

				// stage 0 rgb performs the dot product
				// SPARE0 = TEXTURE0 dot TEXTURE1
				Gl.glCombinerInputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_TEXTURE1_ARB, Gl.GL_EXPAND_NORMAL_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_TEXTURE0_ARB, Gl.GL_EXPAND_NORMAL_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB,
					Gl.GL_SPARE0_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_NONE, Gl.GL_NONE, Gl.GL_TRUE, Gl.GL_FALSE, Gl.GL_FALSE);


				// stage 1 rgb multiplies texture 2 and 3 together
				// SPARE1 = TEXTURE2 * TEXTURE3
				Gl.glCombinerInputNV(Gl.GL_COMBINER1_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_TEXTURE2_ARB, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER1_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_TEXTURE3_ARB, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER1_NV, Gl.GL_RGB,
					Gl.GL_SPARE1_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_NONE, Gl.GL_NONE, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				// stage 1 alpha does nohing

				// stage 2 color multiplies spare0 * spare 1 just for debugging
				// SPARE0 = SPARE0 * SPARE1
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_SPARE0_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_SPARE1_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB,
					Gl.GL_SPARE0_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_NONE, Gl.GL_NONE, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				// stage 2 alpha multiples spare0 * spare 1
				// SPARE0 = SPARE0 * SPARE1
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_ALPHA, Gl.GL_VARIABLE_A_NV,
					Gl.GL_SPARE0_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_BLUE);
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_ALPHA, Gl.GL_VARIABLE_B_NV,
					Gl.GL_SPARE1_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_BLUE);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER2_NV, Gl.GL_ALPHA,
					Gl.GL_SPARE0_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_NONE, Gl.GL_NONE, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				// final combiner
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_D_NV, Gl.GL_SPARE0_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_A_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_B_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_C_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_G_NV, Gl.GL_SPARE0_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_ALPHA);
			}
		}

		private void NV20_DiffuseAndSpecularColorFragment()
		{
			if(idE.CvarSystem.GetBool("r_useCombinerDisplayLists") == true)
			{
				Gl.glCallList(_fragmentDisplayListBase + (int) FragmentProgram.DiffuseAndSpecularColor);
			}
			else
			{
				// program the nvidia register combiners
				Gl.glCombinerParameteriNV(Gl.GL_NUM_GENERAL_COMBINERS_NV, 3);

				// GL_CONSTANT_COLOR0_NV will be the diffuse color
				// GL_CONSTANT_COLOR1_NV will be the specular color

				// stage 0 rgb performs the dot product
				// GL_SECONDARY_COLOR_NV = ( TEXTURE0 dot TEXTURE1 - 0.5 ) * 2
				// the scale and bias steepen the specular curve
				Gl.glCombinerInputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_TEXTURE1_ARB, Gl.GL_EXPAND_NORMAL_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_TEXTURE0_ARB, Gl.GL_EXPAND_NORMAL_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB,
					Gl.GL_SECONDARY_COLOR_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_SCALE_BY_TWO_NV, Gl.GL_BIAS_BY_NEGATIVE_ONE_HALF_NV, Gl.GL_TRUE, Gl.GL_FALSE, Gl.GL_FALSE);

				// stage 0 alpha does nothing

				// stage 1 color takes bump * bump
				// PRIMARY_COLOR = ( GL_SECONDARY_COLOR_NV * GL_SECONDARY_COLOR_NV - 0.5 ) * 2
				// the scale and bias steepen the specular curve
				Gl.glCombinerInputNV(Gl.GL_COMBINER1_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_SECONDARY_COLOR_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER1_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_SECONDARY_COLOR_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER1_NV, Gl.GL_RGB,
					Gl.GL_SECONDARY_COLOR_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_SCALE_BY_TWO_NV, Gl.GL_BIAS_BY_NEGATIVE_ONE_HALF_NV, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				// stage 1 alpha does nothing

				// stage 2 color
				// PRIMARY_COLOR = ( PRIMARY_COLOR * TEXTURE3 ) * 2
				// SPARE0 = 1.0 * 1.0 (needed for final combiner)
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_SECONDARY_COLOR_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_TEXTURE3_ARB, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_C_NV,
					Gl.GL_ZERO, Gl.GL_UNSIGNED_INVERT_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_D_NV,
					Gl.GL_ZERO, Gl.GL_UNSIGNED_INVERT_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB,
					Gl.GL_SECONDARY_COLOR_NV, Gl.GL_SPARE0_NV, Gl.GL_DISCARD_NV,
					Gl.GL_SCALE_BY_TWO_NV, Gl.GL_NONE, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				// stage 2 alpha does nothing

				// final combiner = TEXTURE2_ARB * CONSTANT_COLOR0_NV + PRIMARY_COLOR_NV * CONSTANT_COLOR1_NV
				// alpha = GL_ZERO
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_A_NV, Gl.GL_CONSTANT_COLOR1_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_B_NV, Gl.GL_SECONDARY_COLOR_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_C_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_D_NV, Gl.GL_E_TIMES_F_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_E_NV, Gl.GL_TEXTURE2_ARB,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_F_NV, Gl.GL_CONSTANT_COLOR0_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_G_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_ALPHA);
			}
		}

		private void NV20_DiffuseColorFragment()
		{
			if(idE.CvarSystem.GetBool("r_useCombinerDisplayLists") == true)
			{
				Gl.glCallList(_fragmentDisplayListBase + (int) FragmentProgram.DiffuseColor);
			}
			else
			{
				// program the nvidia register combiners
				Gl.glCombinerParameteriNV(Gl.GL_NUM_GENERAL_COMBINERS_NV, 1);

				// stage 0 is free, so we always do the multiply of the vertex color
				// when the vertex color is inverted, Gl.glCombinerInputNV(GL_VARIABLE_B_NV) will be changed
				Gl.glCombinerInputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_TEXTURE0_ARB, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_PRIMARY_COLOR_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB,
					Gl.GL_TEXTURE0_ARB, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_NONE, Gl.GL_NONE, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				Gl.glCombinerOutputNV(Gl.GL_COMBINER0_NV, Gl.GL_ALPHA,
					Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_NONE, Gl.GL_NONE, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				// for GL_CONSTANT_COLOR0_NV * TEXTURE0 * TEXTURE1
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_A_NV, Gl.GL_CONSTANT_COLOR0_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_B_NV, Gl.GL_E_TIMES_F_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_C_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_D_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_E_NV, Gl.GL_TEXTURE0_ARB,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_F_NV, Gl.GL_TEXTURE1_ARB,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_G_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_ALPHA);
			}
		}

		private void NV20_SpecularColorFragment()
		{
			if(idE.CvarSystem.GetBool("r_useCombinerDisplayLists") == true)
			{
				Gl.glCallList(_fragmentDisplayListBase + (int) FragmentProgram.SpecularColor);
			}
			else
			{
				// program the nvidia register combiners
				Gl.glCombinerParameteriNV(Gl.GL_NUM_GENERAL_COMBINERS_NV, 4);

				// we want GL_CONSTANT_COLOR1_NV * PRIMARY_COLOR * TEXTURE2 * TEXTURE3 * specular( TEXTURE0 * TEXTURE1 )

				// stage 0 rgb performs the dot product
				// GL_SPARE0_NV = ( TEXTURE0 dot TEXTURE1 - 0.5 ) * 2
				// TEXTURE2 = TEXTURE2 * PRIMARY_COLOR
				// the scale and bias steepen the specular curve
				Gl.glCombinerInputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_TEXTURE1_ARB, Gl.GL_EXPAND_NORMAL_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_TEXTURE0_ARB, Gl.GL_EXPAND_NORMAL_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER0_NV, Gl.GL_RGB,
					Gl.GL_SPARE0_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_SCALE_BY_TWO_NV, Gl.GL_BIAS_BY_NEGATIVE_ONE_HALF_NV, Gl.GL_TRUE, Gl.GL_FALSE, Gl.GL_FALSE);

				// stage 0 alpha does nothing

				// stage 1 color takes bump * bump
				// GL_SPARE0_NV = ( GL_SPARE0_NV * GL_SPARE0_NV - 0.5 ) * 2
				// the scale and bias steepen the specular curve
				Gl.glCombinerInputNV(Gl.GL_COMBINER1_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_SPARE0_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER1_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_SPARE0_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER1_NV, Gl.GL_RGB,
					Gl.GL_SPARE0_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_SCALE_BY_TWO_NV, Gl.GL_BIAS_BY_NEGATIVE_ONE_HALF_NV, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				// stage 1 alpha does nothing

				// stage 2 color
				// GL_SPARE0_NV = GL_SPARE0_NV * TEXTURE3
				// SECONDARY_COLOR = CONSTANT_COLOR * TEXTURE2
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_SPARE0_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_TEXTURE3_ARB, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_C_NV,
					Gl.GL_CONSTANT_COLOR1_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB, Gl.GL_VARIABLE_D_NV,
					Gl.GL_TEXTURE2_ARB, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER2_NV, Gl.GL_RGB,
					Gl.GL_SPARE0_NV, Gl.GL_SECONDARY_COLOR_NV, Gl.GL_DISCARD_NV,
					Gl.GL_NONE, Gl.GL_NONE, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				// stage 2 alpha does nothing


				// stage 3 scales the texture by the vertex color
				Gl.glCombinerInputNV(Gl.GL_COMBINER3_NV, Gl.GL_RGB, Gl.GL_VARIABLE_A_NV,
					Gl.GL_SECONDARY_COLOR_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerInputNV(Gl.GL_COMBINER3_NV, Gl.GL_RGB, Gl.GL_VARIABLE_B_NV,
					Gl.GL_PRIMARY_COLOR_NV, Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glCombinerOutputNV(Gl.GL_COMBINER3_NV, Gl.GL_RGB,
					Gl.GL_SECONDARY_COLOR_NV, Gl.GL_DISCARD_NV, Gl.GL_DISCARD_NV,
					Gl.GL_NONE, Gl.GL_NONE, Gl.GL_FALSE, Gl.GL_FALSE, Gl.GL_FALSE);

				// stage 3 alpha does nothing

				// final combiner = GL_SPARE0_NV * SECONDARY_COLOR + PRIMARY_COLOR * SECONDARY_COLOR
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_A_NV, Gl.GL_SPARE0_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_B_NV, Gl.GL_SECONDARY_COLOR_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_C_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_D_NV, Gl.GL_E_TIMES_F_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_E_NV, Gl.GL_SPARE0_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_F_NV, Gl.GL_SECONDARY_COLOR_NV,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_RGB);
				Gl.glFinalCombinerInputNV(Gl.GL_VARIABLE_G_NV, Gl.GL_ZERO,
					Gl.GL_UNSIGNED_IDENTITY_NV, Gl.GL_ALPHA);
			}
		}
		#endregion

		#region R200
		private void InitR200()
		{
			idE.GLConfig.AllowR200Path = false;

			idConsole.WriteLine("----------- R200_Init -----------");

			if((idE.GLConfig.AtiFragmentShaderAvailable == false) || (idE.GLConfig.ArbFragmentProgramAvailable == false) || (idE.GLConfig.ArbVertexBufferObjectAvailable == false))
			{
				idConsole.WriteLine("Not available.");
			}
			else
			{
				idConsole.WriteLine("TODO: R200_Init");
				// TODO: R200
				/*Gl.glGetIntegerv( Gl.GL_NUM_FRAGMENT_REGISTERS_ATI, &fsi.numFragmentRegisters );
				Gl.glGetIntegerv( Gl.GL_NUM_FRAGMENT_CONSTANTS_ATI, &fsi.numFragmentConstants );
				Gl.glGetIntegerv( Gl.GL_NUM_PASSES_ATI, &fsi.numPasses );
				Gl.glGetIntegerv( Gl.GL_NUM_INSTRUCTIONS_PER_PASS_ATI, &fsi.numInstructionsPerPass );
				Gl.glGetIntegerv( Gl.GL_NUM_INSTRUCTIONS_TOTAL_ATI, &fsi.numInstructionsTotal );
				Gl.glGetIntegerv( Gl.GL_COLOR_ALPHA_PAIRING_ATI, &fsi.colorAlphaPairing );
				Gl.glGetIntegerv( Gl.GL_NUM_LOOPBACK_COMPONENTS_ATI, &fsi.numLoopbackComponenets );
				Gl.glGetIntegerv( Gl.GL_NUM_INPUT_INTERPOLATOR_COMPONENTS_ATI, &fsi.numInputInterpolatorComponents );

				common->Printf( "GL_NUM_FRAGMENT_REGISTERS_ATI: %i\n", fsi.numFragmentRegisters );
				common->Printf( "GL_NUM_FRAGMENT_CONSTANTS_ATI: %i\n", fsi.numFragmentConstants );
				common->Printf( "GL_NUM_PASSES_ATI: %i\n", fsi.numPasses );
				common->Printf( "GL_NUM_INSTRUCTIONS_PER_PASS_ATI: %i\n", fsi.numInstructionsPerPass );
				common->Printf( "GL_NUM_INSTRUCTIONS_TOTAL_ATI: %i\n", fsi.numInstructionsTotal );
				common->Printf( "GL_COLOR_ALPHA_PAIRING_ATI: %i\n", fsi.colorAlphaPairing );
				common->Printf( "GL_NUM_LOOPBACK_COMPONENTS_ATI: %i\n", fsi.numLoopbackComponenets );
				common->Printf( "GL_NUM_INPUT_INTERPOLATOR_COMPONENTS_ATI: %i\n", fsi.numInputInterpolatorComponents );

				common->Printf( "FPROG_FAST_PATH\n" );
				R_BuildSurfaceFragmentProgram( FPROG_FAST_PATH );*/

				idConsole.WriteLine("---------------------");

				idE.GLConfig.AllowR200Path = true;
			}
		}
		#endregion

		#region ARB2
		private void InitARB2()
		{
			idE.GLConfig.AllowArb2Path = false;

			idConsole.WriteLine("---------- R_ARB2_Init ----------");

			if((idE.GLConfig.ArbVertexProgramAvailable == false) || (idE.GLConfig.ArbFragmentProgramAvailable == false))
			{
				idConsole.WriteLine("Not available.");
			}
			else
			{
				idConsole.WriteLine("Available.");
				idConsole.WriteLine("---------------------------------");

				idE.GLConfig.AllowArb2Path = true;
			}
		}
		#endregion

		#region Command handlers
		private void Cmd_ReloadArbPrograms(object sender, CommandEventArgs e)
		{
			idConsole.WriteLine("----- R_ReloadARBPrograms -----");

			for(int i = 0; i < _programs.Length; i++)
			{
				LoadArbProgram(i);
			}

			idConsole.WriteLine("-------------------------------");
		}
		#endregion
		#endregion
	}

	internal enum BackEndRenderer
	{
		ARB,
		NV10,
		NV20,
		R200,
		ARB2,
		Bad
	}

	/// <summary>
	/// All state modified by the back end is separated from the front end state.
	/// </summary>
	internal class BackEndState
	{
		public int FrameCount; // used to track all images used in a frame.

		public View ViewDefinition;
		/*backEndCounters_t	pc;*/

		/*const viewEntity_t *currentSpace;		// for detecting when a matrix must change*/

		public idScreenRect CurrentScissor;
		// for scissor clipping, local inside renderView viewport

		/*viewLight_t *		vLight;*/
		public MaterialStates DepthFunction;	// GLS_DEPTHFUNC_EQUAL, or GLS_DEPTHFUNC_LESS for translucent
		/*float				lightTextureMatrix[16];	// only if lightStage->texture.hasMatrix
		float				lightColor[4];		// evaluation of current light's color stage

		float				lightScale;			// Every light color calaculation will be multiplied by this,
												// which will guarantee that the result is < tr.backEndRendererMaxLight
												// A card with high dynamic range will have this set to 1.0
		float				overBright;			// The amount that all light interactions must be multiplied by
												// with post processing to get the desired total light level.
												// A high dynamic range card will have this set to 1.0.*/

		public bool CurrentRenderCopied;		// true if any material has already referenced CurrentRender

		// our OpenGL state deltas.
		public GLState GLState = new GLState();

		//int					c_copyFrameBuffer;*/
	}

	internal class GLState
	{
		public TextureUnit[] TextureUnits = new TextureUnit[8];
		public int CurrentTextureUnit;

		public CullType FaceCulling;
		public MaterialStates StateBits;
		public bool ForceState; // the next GL_State will ignore glStateBits and set everything.

		public GLState()
		{
			for(int i = 0; i < TextureUnits.Length; i++)
			{
				TextureUnits[i] = new TextureUnit();
			}
		}
	}

	/// <summary>
	/// Contains variables specific to the OpenGL configuration being run right now.
	/// </summary>
	internal class GLConfig
	{
		public string Renderer;
		public string Vendor;
		public string Version;
		public Version VersionF;
		public string Extensions;

		public int MaxTextureSize;
		public int MaxTextureUnits;
		public int MaxTextureCoordinates;
		public int MaxTextureImageUnits;
		public float MaxTextureAnisotropy;

		public int ColorBits;
		public int DepthBits;
		public int StencilBits;

		public bool MultiTextureAvailable;
		public bool TextureCompressionAvailable;
		public bool AnisotropicAvailable;
		public bool TextureLodBiasAvailable;
		public bool TextureEnvAddAvailable;
		public bool TextureEnvCombineAvailable;
		public bool RegisterCombinersAvailable;
		public bool CubeMapAvailable;
		public bool EnvDot3Available;
		public bool Texture3DAvailable;
		public bool SharedTexturePaletteAvailable;
		public bool ArbVertexBufferObjectAvailable;
		public bool ArbVertexProgramAvailable;
		public bool ArbFragmentProgramAvailable;
		public bool TwoSidedStencilAvailable;
		public bool TextureNonPowerOfTwoAvailable;
		public bool DepthBoundsTestAvailable;

		// ati r200 extensions
		public bool AtiFragmentShaderAvailable;

		// ati r300
		public bool AtiTwoSidedStencilAvailable;

		public int VideoWidth;
		public int VideoHeight;

		public int DisplayFrequency;

		public bool IsFullscreen;

		public bool AllowNV30Path;
		public bool AllowNV20Path;
		public bool AllowNV10Path;
		public bool AllowR200Path;
		public bool AllowArb2Path;
	}

	internal class TextureUnit
	{		
		public int TexEnv;

		public Texture CurrentTexture;
		public TextureType Type;
	}

	public class View
	{
		public RenderView RenderView;

		public Matrix ProjectionMatrix;
		public ViewEntity WorldSpace;
		public idRenderWorld RenderWorld;

		public float FloatTime;

		public Vector3 InitialViewAreaOrigin;
		// Used to find the portalArea that view flooding will take place from.
		// for a normal view, the initialViewOrigin will be renderView.viewOrg,
		// but a mirror may put the projection origin outside
		// of any valid area, or in an unconnected area of the map, so the view
		// area must be based on a point just off the surface of the mirror / subview.
		// It may be possible to get a failed portal pass if the plane of the
		// mirror intersects a portal, and the initialViewAreaOrigin is on
		// a different side than the renderView.viewOrg is.

		public bool IsSubview;				// true if this view is not the main view
		public bool IsMirror;				// the portal is a mirror, invert the face culling
		public bool IsXraySubview;
		public bool IsEditor;

		/*int					numClipPlanes;			// mirrors will often use a single clip plane
		idPlane				clipPlanes[MAX_CLIP_PLANES];		// in world space, the positive side
													// of the plane is the visible side
		 * */

		public idScreenRect ViewPort; // in real pixels and proper Y flip
		public idScreenRect Scissor;
		// for scissor clipping, local inside renderView viewport
		// subviews may only be rendering part of the main view
		// these are real physical pixel values, possibly scaled and offset from the
		// renderView x/y/width/height

		/*struct viewDef_s *	superView;				// never go into an infinite subview loop 
		struct drawSurf_s *	subviewSurface;*/

		// drawSurfs are the visible surfaces of the viewEntities, sorted
		// by the material sort parameter
		public List<DrawSurface> DrawSurfaces = new List<DrawSurface>();

		/*struct viewLight_s	*viewLights;			// chain of all viewLights effecting view
		struct viewEntity_s	*viewEntitys;			// chain of all viewEntities effecting view, including off screen ones casting shadows
		// we use viewEntities as a check to see if a given view consists solely
		// of 2D rendering, which we can optimize in certain ways.  A 2D view will
		// not have any viewEntities

		idPlane				frustum[5];				// positive sides face outward, [4] is the front clip plane
		idFrustum			viewFrustum;

		int					areaNum;				// -1 = not in a valid area

		bool *				connectedAreas;
		// An array in frame temporary memory that lists if an area can be reached without
		// crossing a closed door.  This is used to avoid drawing interactions
		// when the light is behind a closed door.
*/
	}

	public struct RenderView
	{
		// player views will set this to a non-zero integer for model suppress / allow
		// subviews (mirrors, cameras, etc) will always clear it to zero
		public int ViewID;

		// sized from 0 to SCREEN_WIDTH / SCREEN_HEIGHT (640/480), not actual resolution
		public int X;
		public int Y;
		public int Width;
		public int Height;

		/*float					fov_x, fov_y;*/

		public Vector3 ViewOrigin;
		public Matrix ViewAxis;						// transformation matrix, view looks down the positive X axis

		/*bool					cramZNear;			// for cinematics, we want to set ZNear much lower
		bool					forceUpdate;		// for an update 

		// time in milliseconds for shader effects and other time dependent rendering issues
		int						time;*/

		/// <summary>Can be used in any way by the shader.</summary>
		public float[] MaterialParameters;

		/// <summary>Used to override everything draw.</summary>
		public idMaterial GlobalMaterial;
	}

	public struct RenderEntity
	{

		/*idRenderModel *			hModel;				// this can only be null if callback is set

		int						entityNum;
		int						bodyId;

		// Entities that are expensive to generate, like skeletal models, can be
		// deferred until their bounds are found to be in view, in the frustum
		// of a shadowing light that is in view, or contacted by a trace / overlay test.
		// This is also used to do visual cueing on items in the view
		// The renderView may be NULL if the callback is being issued for a non-view related
		// source.
		// The callback function should clear renderEntity->callback if it doesn't
		// want to be called again next time the entity is referenced (ie, if the
		// callback has now made the entity valid until the next updateEntity)
		idBounds				bounds;					// only needs to be set for deferred models and md5s
		deferredEntityCallback_t	callback;

		void *					callbackData;			// used for whatever the callback wants

		// player bodies and possibly player shadows should be suppressed in views from
		// that player's eyes, but will show up in mirrors and other subviews
		// security cameras could suppress their model in their subviews if we add a way
		// of specifying a view number for a remoteRenderMap view
		int						suppressSurfaceInViewID;
		int						suppressShadowInViewID;

		// world models for the player and weapons will not cast shadows from view weapon
		// muzzle flashes
		int						suppressShadowInLightID;

		// if non-zero, the surface and shadow (if it casts one)
		// will only show up in the specific view, ie: player weapons
		int						allowSurfaceInViewID;

		// positioning
		// axis rotation vectors must be unit length for many
		// R_LocalToGlobal functions to work, so don't scale models!
		// axis vectors are [0] = forward, [1] = left, [2] = up
		idVec3					origin;
		idMat3					axis;

		// texturing
		const idMaterial *		customShader;			// if non-0, all surfaces will use this
		const idMaterial *		referenceShader;		// used so flares can reference the proper light shader
		const idDeclSkin *		customSkin;				// 0 for no remappings
		class idSoundEmitter *	referenceSound;			// for shader sound tables, allowing effects to vary with sounds*/
		public float[] MaterialParameters;				// can be used in any way by material or model generation

		// networking: see WriteGUIToSnapshot / ReadGUIFromSnapshot
		/*class idUserInterface * gui[ MAX_RENDERENTITY_GUI ];

		struct renderView_s	*	remoteRenderView;		// any remote camera surfaces will use this

		int						numJoints;
		idJointMat *			joints;					// array of joints that will modify vertices.
														// NULL if non-deformable model.  NOT freed by renderer

		float					modelDepthHack;			// squash depth range so particle effects don't clip into walls

		// options to override surface shader flags (replace with material parameters?)
		bool					noSelfShadow;			// cast shadows onto other objects,but not self
		bool					noShadow;				// no shadow at all

		bool					noDynamicInteractions;	// don't create any light / shadow interactions after
														// the level load is completed.  This is a performance hack
														// for the gigantic outdoor meshes in the monorail map, so
														// all the lights in the moving monorail don't touch the meshes*/

		public bool WeaponDepthHack;					// squash depth range so view weapons don't poke into walls
		// this automatically implies noShadow
		/*int						forceUpdate;			// force an update (NOTE: not a bool to keep this struct a multiple of 4 bytes)
		int						timeGroup;
		int						xrayIndex;
	} renderEntity_t;*/

		public void Init()
		{
			MaterialParameters = new float[idE.MaxEntityMaterialParameters];
		}
	}

	public struct ViewEntity
	{
		/*struct viewEntity_s	*next;

	// back end should NOT reference the entityDef, because it can change when running SMP
	idRenderEntityLocal	*entityDef;*/

		// for scissor clipping, local inside renderView viewport
		// scissorRect.Empty() is true if the viewEntity_t was never actually
		// seen through any portals, but was created for shadow casting.
		// a viewEntity can have a non-empty scissorRect, meaning that an area
		// that it is in is visible, and still not be visible.
		public idScreenRect ScissorRectangle;

		public bool WeaponDepthHack;
		public float ModelDepthHack;

		public Matrix ModelMatrix; // local coords to global coords
		public Matrix ModelViewMatrix; // local coords to eye coords
	}

	public struct DrawSurface
	{
		public Surface Geometry;
		public ViewEntity Space;
		public float Sort;						// material->sort, modified by gui / entity sort offsets

		public idMaterial Material;				// may be null for shadow volumes
		public float[] MaterialRegisters;			// evaluated and adjusted for referenceShaders

		public idScreenRect ScissorRectangle;	// for scissor clipping, local inside renderView viewport

		/*
		 const struct drawSurf_s	*nextOnLight;	// viewLight chains
		 * int						dsFlags;			// DSF_VIEW_INSIDE_SHADOW, etc
		struct vertCache_s		*dynamicTexCoords;	// float * in vertex cache memory
		// specular directions for non vertex program cards, skybox texcoords, etc
		 * */
	}

	public struct Surface
	{
		/*idBounds					bounds;					// for culling

	int							ambientViewCount;		// if == tr.viewCount, it is visible this view

	bool						generateNormals;		// create normals from geometry, instead of using explicit ones
	bool						tangentsCalculated;		// set when the vertex tangents have been calculated
	bool						facePlanesCalculated;	// set when the face planes have been calculated
	bool						perfectHull;			// true if there aren't any dangling edges
	bool						deformedSurface;		// if true, indexes, silIndexes, mirrorVerts, and silEdges are
														// pointers into the original surface, and should not be freed*/

		public Vertex[] Vertices;
		public int[] Indexes;							// for shadows, this has both front and rear end caps and silhouette planes

		/*
		glIndex_t *					silIndexes;				// indexes changed to be the first vertex with same XYZ, ignoring normal and texcoords

		int							numMirroredVerts;		// this many verts at the end of the vert list are tangent mirrors
		int *						mirroredVerts;			// tri->mirroredVerts[0] is the mirror of tri->numVerts - tri->numMirroredVerts + 0

		int							numDupVerts;			// number of duplicate vertexes
		int *						dupVerts;				// pairs of the number of the first vertex and the number of the duplicate vertex

		int							numSilEdges;			// number of silhouette edges
		silEdge_t *					silEdges;				// silhouette edges

		idPlane *					facePlanes;				// [numIndexes/3] plane equations

		dominantTri_t *				dominantTris;			// [numVerts] for deformed surface fast tangent calculation

		int							numShadowIndexesNoFrontCaps;	// shadow volumes with front caps omitted
		int							numShadowIndexesNoCaps;			// shadow volumes with the front and rear caps omitted

		int							shadowCapPlaneBits;		// bits 0-5 are set when that plane of the interacting light has triangles
															// projected on it, which means that if the view is on the outside of that
															// plane, we need to draw the rear caps of the shadow volume
															// turboShadows will have SHADOW_CAP_INFINITE

		shadowCache_t *				shadowVertexes;			// these will be copied to shadowCache when it is going to be drawn.
															// these are NULL when vertex programs are available

		struct srfTriangles_s *		ambientSurface;			// for light interactions, point back at the original surface that generated
															// the interaction, which we will get the ambientCache from

		struct srfTriangles_s *		nextDeferredFree;		// chain of tris to free next frame

		// data in vertex object space, not directly readable by the CPU*/

		public VertexCache IndexCache;
		public VertexCache AmbientCache;
		/*struct vertCache_s *		lightingCache;			// lightingCache_t
		struct vertCache_s *		shadowCache;			// shadowCache_t
	*/
	}

	public struct Z : IVertexType
	{
		#region IVertexType Members

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}

	public struct Vertex : IVertexType
	{
		#region Vertex declaration
		public VertexDeclaration VertexDeclaration
		{
			get
			{
				return new VertexDeclaration(
					new VertexElement[] {
						new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
						new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
						new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
						//new VertexElement(32, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
						new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0)
					}
				);
			}
		}
		#endregion

		#region Fields
		public Vector3 Position;
		public Vector2 TextureCoordinates;
		public Vector3 Normal;
		//public Vector3[] Tangents;
		public Color Color;
		#endregion
	}

	public abstract class BaseVertexCache
	{
		public VertexCacheType Tag;
	}

	public sealed class VertexCache : BaseVertexCache
	{
		public Vertex[] Data;
	}

	public enum VertexCacheType
	{
		Free,
		Used,
		Fixed,
		Temporary
	}

	internal struct FrameData
	{

		/*// one or more blocks of memory for all frame
		// temporary allocations
		frameMemoryBlock_t	*memory;

		// alloc will point somewhere into the memory chain
		frameMemoryBlock_t	*alloc;

		srfTriangles_t *	firstDeferredFreeTriSurf;
		srfTriangles_t *	lastDeferredFreeTriSurf;

		int					memoryHighwater;	// max used on any frame*/

		// the currently building command list 
		// commands can be inserted at the front if needed, as for required
		// dynamically generated textures
		public Queue<RenderCommand> Commands;
	}

	internal struct VideoMode
	{
		public string Description;
		public int Width;
		public int Height;

		public VideoMode(string description, int width, int height)
		{
			this.Description = description;
			this.Width = width;
			this.Height = height;
		}
	}

	internal enum FragmentProgram
	{
		BumpAndLight,
		DiffuseColor,
		SpecularColor,
		DiffuseAndSpecularColor,

		Count
	}

	internal enum RenderCommandType
	{
		Nop,
		DrawView,
		SetBuffer,
		CopyRender,
		SwapBuffers		// can't just assume swap at end of list because
		// of forced list submission before syncs
	}

	public enum GLProgramType
	{
		None = -1,
		Invalid = 0,

		VertexProgramInteraction,
		VertexProgramEnvironment,
		VertexProgramBumpyEnvironment,
		VertexProgramR200Interaction,
		VertexProgramStencilShadow,
		VertexProgramNV20BumpAndLight,
		VertexProgramNV20DiffuseColor,
		VertexProgramNV20SpecularColor,
		VertexProgramNV20DiffuseAndSpecularColor,
		VertexProgramTest,

		FragmentProgramInteraction,
		FragmentProgramEnvironment,
		FragmentProgramBumpyEnvironment,
		FragmentProgramTest,

		VertexProgramAmbient,
		FragmentProgramAmbient,
		VertexProgramGlassWarp,
		FragmentProgramGlassWarp,

		User
	}

	public struct GLProgram
	{
		public int Target;
		public int Type;
		public string Name;

		public GLProgram(int target, GLProgramType type, string name)
		{
			Target = target;
			Type = (int) type;
			Name = name;
		}
	}

	internal abstract class RenderCommand
	{
		#region Properties
		public abstract RenderCommandType CommandID
		{
			get;
		}
		#endregion

		#region Constructor
		public RenderCommand()
		{

		}
		#endregion
	}

	internal sealed class DrawViewRenderCommand : RenderCommand
	{
		public View View;

		#region Constructor
		public DrawViewRenderCommand()
			: base()
		{

		}
		#endregion

		#region RenderCommand
		public override RenderCommandType CommandID
		{
			get
			{
				return RenderCommandType.DrawView;
			}
		}
		#endregion
	}

	internal sealed class NoOperationRenderCommand : RenderCommand
	{
		#region Constructor
		public NoOperationRenderCommand()
			: base()
		{

		}
		#endregion

		#region RenderCommand
		public override RenderCommandType CommandID
		{
			get
			{
				return RenderCommandType.Nop;
			}
		}
		#endregion
	}

	internal sealed class SetBufferRenderCommand : RenderCommand
	{
		public int Buffer;
		public int FrameCount;

		#region Constructor
		public SetBufferRenderCommand()
			: base()
		{

		}
		#endregion

		#region RenderCommand
		public override RenderCommandType CommandID
		{
			get
			{
				return RenderCommandType.SetBuffer;
			}
		}
		#endregion
	}

	internal sealed class SwapBuffersRenderCommand : RenderCommand
	{
		#region Constructor
		public SwapBuffersRenderCommand()
			: base()
		{

		}
		#endregion

		#region RenderCommand
		public override RenderCommandType CommandID
		{
			get
			{
				return RenderCommandType.SwapBuffers;
			}
		}
		#endregion
	}
}