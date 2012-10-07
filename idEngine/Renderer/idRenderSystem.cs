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

using Tao.OpenGl;

using idTech4.Geometry;
using idTech4.Math;
using idTech4.Text.Decl;
using idTech4.UI;

namespace idTech4.Renderer
{
	/// <summary>
	/// Responsible for managing the screen, which can have multiple idRenderWorld and 2D drawing done on it.
	/// </summary>
	public sealed class idRenderSystem
	{
		#region Constants
		public static readonly VideoMode[] VideoModes = new VideoMode[] {
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

		private static readonly float[] FlipMatrix = new float[] {
			// convert from our coordinate system (looking down X)
			// to OpenGL's coordinate system (looking down -Z)
			0, 0, -1, 0,
			-1, 0, 0, 0,
			0, 1, 0, 0,
			0, 0, 0, 1
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

		public int FrameCount
		{
			get
			{
				return _frameCount;
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
			set
			{
				_frameShaderTime = value;
			}
		}

		public idGuiModel GuiModel
		{
			get
			{
				return _guiModel;
			}
		}

		public ViewEntity IdentitySpace
		{
			get
			{
				return _identitySpace;
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

		public idRenderView PrimaryRenderView
		{
			get
			{
				return _primaryRenderView;
			}
			set
			{
				_primaryRenderView = value;
			}
		}

		public idRenderWorld PrimaryRenderWorld
		{
			get
			{
				return _primaryRenderWorld;
			}
			set
			{
				_primaryRenderWorld = value;
			}
		}

		public View PrimaryView
		{
			get
			{
				return _primaryView;
			}
			set
			{
				_primaryView = value;
			}
		}

		public int ScreenWidth
		{
			get
			{
				return _backendRenderer.ScreenWidth;
			}
		}

		public int ScreenHeight
		{
			get
			{
				return _backendRenderer.ScreenHeight;
			}
		}

		public int ViewCount
		{
			get
			{
				return _viewCount;
			}
			internal set
			{
				_viewCount = value;
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

		public Vector2 ViewPortOffset
		{
			get
			{
				return _viewPortOffset;
			}
		}
		#endregion

		#region Members
		private bool _registered;
		private bool _graphicsInitialized;
						
		private int _frameCount;											// incremented every frame
		private int _viewCount;												// incremented every view (twice a scene if subviewed) and every R_MarkFragments call
		private float _sortOffset;											// for determinist sorting of equal sort materials

		private Vector4 _ambientLightVector;								// used for "ambient bump mapping"

		private idMaterial _defaultMaterial;
		private ViewEntity _identitySpace;									// can use if we don't know viewDef->worldSpace is valid
		private View _viewDefinition;

		private List<idRenderWorld> _worlds = new List<idRenderWorld>();

		private idRenderBackendInterface _backendRenderer;					// determines which back end to use, and if vertex programs are in use

		private ushort[] _gammaTable = new ushort[256];						// brightness / gamma modify this
																			// determines how much overbrighting needs
																			// to be done post-process

		private Vector2 _viewPortOffset;									// for doing larger-than-window tiled renderings
		private Vector2 _tiledViewPort;

		private int _currentRenderCrop;
		private idRectangle[] _renderCrops = new idRectangle[idE.MaxRenderCrops];
				
		// GUI drawing variables for surface creation
		private int _guiRecursionLevel;										// to prevent infinite overruns
		private idGuiModel _guiModel;
		private idGuiModel _demoGuiModel;

		private FrameData _frameData = new FrameData();
		private float _frameShaderTime;										// shader time for all non-world 2D rendering

		private int _staticAllocCount;

		// many console commands need to know which world they should operate on
		private idRenderWorld _primaryRenderWorld;
		private idRenderView _primaryRenderView;
		private View _primaryView;

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
		#region Debug Visualization
		public void DebugClearLines(int time)
		{
			idConsole.Warning("TODO: DebugClearLines");
		}

		public void DebugClearPolygons(int time)
		{
			idConsole.Warning("TODO: DebugClearPolygons");
		}

		public void DebugClearText(int time)
		{
			idConsole.Warning("TODO: DebugClearText");
		}
		#endregion

		#region Public
		public void AddDrawSurface(Surface surface, ViewEntity space, RenderEntityComponent renderEntity, idMaterial material, idScreenRect scissor)
		{
			float[] materialParameters;
			float[] referenceRegisters = new float[idE.MaxExpressionRegisters];
			float[] generatedMaterialParameters = new float[idE.MaxEntityMaterialParameters];

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
				if(renderEntity.ReferenceMaterial != null)
				{
					// evaluate the reference shader to find our shader parms
					//renderEntity.ReferenceMaterial.EvaluateRegisters(ref referenceRegisters, renderEntity.MaterialParameters, this.ViewDefinition, renderEntity.ReferenceSound);

					idConsole.Warning("TODO: ref material");
					/*MaterialStage stage = renderEntity.ReferenceMaterial.GetStage(0);

					memcpy( generatedShaderParms, renderEntity->shaderParms, sizeof( generatedShaderParms ) );
					generatedShaderParms[0] = refRegs[ pStage->color.registers[0] ];
					generatedShaderParms[1] = refRegs[ pStage->color.registers[1] ];
					generatedShaderParms[2] = refRegs[ pStage->color.registers[2] ];*/

					materialParameters = generatedMaterialParameters;
				} 
				else
				{
					// evaluate with the entityDef's shader parms
					materialParameters = renderEntity.MaterialParameters;
				}

				float oldFloatTime = 0;
				int oldTime = 0;

				if((space.EntityDef != null) && (space.EntityDef.Parameters.TimeGroup != 0))
				{
					oldFloatTime = this.ViewDefinition.FloatTime;
					oldTime = this.ViewDefinition.RenderView.Time;

					this.ViewDefinition.FloatTime = idE.Game.GetTimeGroupTime(space.EntityDef.Parameters.TimeGroup) * 0.001f;
					this.ViewDefinition.RenderView.Time = idE.Game.GetTimeGroupTime(space.EntityDef.Parameters.TimeGroup);
				}

				material.EvaluateRegisters(ref drawSurface.MaterialRegisters, materialParameters, idE.RenderSystem.ViewDefinition /* TODO: ,renderEntity->referenceSound*/);

				if((space.EntityDef != null) && (space.EntityDef.Parameters.TimeGroup != 0))
				{
					this.ViewDefinition.FloatTime = oldFloatTime;
					this.ViewDefinition.RenderView.Time = oldTime;
				}
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
			idUserInterface	gui = null;

			if(space.EntityDef == null)
			{
				gui = material.GlobalInterface;
			}
			else
			{
				idConsole.Warning("TODO: global gui");
				/*int guiNum = shader->GetEntityGui() - 1;
				if ( guiNum >= 0 && guiNum < MAX_RENDERENTITY_GUI ) {
					gui = renderEntity->gui[ guiNum ];
				}
				if ( gui == NULL ) {
					gui = shader->GlobalGui();
				}*/
			}

			if(gui != null)
			{
				// force guis on the fast time
				float oldFloatTime = this.ViewDefinition.FloatTime;
				int oldTime = this.ViewDefinition.RenderView.Time;

				this.ViewDefinition.FloatTime = idE.Game.GetTimeGroupTime(1) * 0.001f;
				this.ViewDefinition.RenderView.Time = idE.Game.GetTimeGroupTime(1);

				idBounds ndcBounds;

				idConsole.Warning("TODO: precise cull + render gui surface");

				/*if ( !R_PreciseCullSurface( drawSurf, ndcBounds ) ) {
					// did we ever use this to forward an entity color to a gui that didn't set color?
		//			memcpy( tr.guiShaderParms, shaderParms, sizeof( tr.guiShaderParms ) );
					R_RenderGuiSurf( gui, drawSurf );
				}*/

				this.ViewDefinition.FloatTime = oldFloatTime;
				this.ViewDefinition.RenderView.Time = oldTime;
			}

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

		public void BeginFrame(int windowWidth, int windowHeight)
		{
			if(this.IsRunning == false)
			{
				return;
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
			_renderCrops[0] = new idRectangle(0, 0, windowWidth, windowHeight);

			// screenFraction is just for quickly testing fill rate limitations
			if(idE.CvarSystem.GetInteger("r_screenFraction") != 100)
			{
				int w = (int) (idE.VirtualScreenWidth * idE.CvarSystem.GetInteger("r_screenFraction") / 100.0f);
				int h = (int) (idE.VirtualScreenHeight * idE.CvarSystem.GetInteger("r_screenFraction") / 100.0f);

				// TODO: CropRenderSize(w, h);
				idConsole.Warning("idRenderSystem.CropRenderSize");
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
			_frameShaderTime = idE.EventLoop.Milliseconds * 0.001f;

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

		public void BeginLevelLoad()
		{
			idE.RenderModelManager.BeginLevelLoad();
			idE.ImageManager.BeginLevelLoad();
		}

		public void BindTexture(idImage texture)
		{
			_backendRenderer.BindTexture(texture);
		}
		
		public void ClearTextureUnits()
		{
			_backendRenderer.ClearTextureUnits();
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

		public void DrawBigCharacter(int x, int y, int ch, idMaterial material)
		{
			ch &= 255;

			if(ch == ' ')
			{
				return;
			}

			if(y < -idE.BigCharacterHeight)
			{
				return;
			}

			int row = ch >> 4;
			int col = ch & 15;

			float frow = row * 0.0625f;
			float fcol = col * 0.0625f;
			float size = 0.0625f;

			DrawStretchPicture(x, y, idE.BigCharacterWidth, idE.BigCharacterHeight, fcol, frow, fcol + size, frow + size, material);
		}

		/// <summary>
		/// Draws a multi-colored string with a drop shadow, optionally forcing to a fixed color.
		/// <para/>
		/// Coordinates are at 640 by 480 virtual resolution.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="str"></param>
		/// <param name="color"></param>
		/// <param name="forceColor"></param>
		/// <param name="material"></param>
		public void DrawBigString(int x, int y, string str, Vector4 color, bool forceColor, idMaterial material)
		{
			Vector4 tmpColor;
			int xx = x;
			int length = str.Length;

			this.Color = color;

			for(int i = 0; i < length; i++)
			{
				if(idHelper.IsColor(str, i) == true)
				{
					if(forceColor == false)
					{
						if(str[i + 1] == (char) idColorIndex.Default)
						{
							this.Color = color;
						}
						else
						{
							tmpColor = idHelper.ColorForIndex(str[i + 1]);
							tmpColor.W = color.W;

							this.Color = tmpColor;
						}
					}

					i += 2;
					continue;
				}

				DrawBigCharacter(xx, y, str[i], material);

				xx += idE.BigCharacterWidth;
			}

			this.Color = idColor.White;
		}

		/// <summary>
		/// Small characters are drawn at native screen resolution.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="c"></param>
		/// <param name="material"></param>
		public void DrawSmallCharacter(int x, int y, int c, idMaterial material)
		{
			c &= 255;

			if(c == ' ')
			{
				return;
			}

			if(y < -idE.SmallCharacterHeight)
			{
				return;
			}

			int row = c >> 4;
			int col = c & 15;

			float actualRow = row * 0.0625f;
			float actualCol = col * 0.0625f;
			float size = 0.0625f;

			DrawStretchPicture(x, y, idE.SmallCharacterWidth, idE.SmallCharacterHeight, actualCol, actualRow, actualCol + size, actualRow + size, material);
		}

		/// <summary>
		/// Draws a multi-colored string with a drop shadow, optionally forcing to a fixed color.
		/// </summary>
		/// <remarks>
		/// Coordinates are at 640x480 virtual resolution.
		/// </remarks>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="str"></param>
		/// <param name="setColor"></param>
		/// <param name="forceColor"></param>
		/// <param name="material"></param>
		public void DrawSmallString(int x, int y, string str, Vector4 setColor, bool forceColor, idMaterial material)
		{
			Vector4 color;
			char c;
			int xx = x;
			int length = str.Length;

			this.Color = setColor;

			for(int i = 0; i < length; i++)
			{
				c = str[i];

				if(idHelper.IsColor(str, i) == true)
				{
					if(forceColor == false)
					{
						if(str[i + 1] == (char) idColorIndex.Default)
						{
							this.Color = setColor;
						}
						else
						{
							color = idHelper.ColorForIndex(str[i + 1]);
							color.Z = setColor.Z;

							this.Color = color;
						}
					}

					i += 2;
				}
				else
				{
					DrawSmallCharacter(xx, y, str[i], material);
					xx += idE.SmallCharacterWidth;
				}
			}

			this.Color = idColor.White;
		}
				
		public void DrawStretchPicture(Vertex[] vertices, int[] indexes, idMaterial material, bool clip = true, float minX = 0.0f, float minY = 0.0f, float maxX = 640.0f, float maxY = 0.0f)
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

		public void EndLevelLoad()
		{
			idE.RenderModelManager.EndLevelLoad();
			idE.ImageManager.EndLevelLoad();

			if(idE.CvarSystem.GetBool("r_forceLoadImages") == true)
			{
				idConsole.Warning("TODO: RB_ShowImages();");
			}
		}

		public void Init()
		{
			idConsole.WriteLine("------- Initializing renderSystem --------");

			// clear all our internal state
			_viewCount = 1;	// so cleared structures never match viewCount
			// we used to memset tr, but now that it is a class, we can't, so
			// there may be other state we need to reset

			_ambientLightVector = new Vector4(0.5f, 0.5f - 0.385f, 0.8925f, 1.0f);
			_frameData.Commands = new Queue<RenderCommand>();
			_backendRenderer = new Backends.XNARenderBackend();

			InitCommands();
			InitRenderer();

			_guiModel = new idGuiModel();
			_demoGuiModel = new idGuiModel();
			
			// TODO: R_InitTriSurfData();

			idE.ImageManager.Init();

			// TODO: idCinematic::InitCinematic( );

			// build brightness translation tables
			SetColorMappings();

			InitMaterials();

			idE.RenderModelManager.Init();

			// set the identity space
			_identitySpace = new ViewEntity();

			idConsole.WriteLine("renderSystem initialized.");
			idConsole.WriteLine("--------------------------------------");
		}

		public void InitRenderer()
		{
			// start renderer now if it hasn't been started already
			if(this.IsRunning == true)
			{
				return;
			}

			idConsole.WriteLine("----- R_InitGraphics -----");

			_backendRenderer.Init();

			// in case we had an error while doing a tiled rendering
			_viewPortOffset = Vector2.Zero;

			// input and sound systems need to be tied to the new window
			// TODO: Sys_InitInput();
			// TODO: soundSystem->InitHW();
			
			_graphicsInitialized = true;

			// allocate the vertex array range or vertex objects
			/*// TODO: vertexCache.Init();*/

			ToggleSmpFrame();

			// reset our gamma
			SetColorMappings();

			idE.ImageManager.ChangeTextureFilter();
		}

		public void Present()
		{
			_backendRenderer.Present();
		}

		public idFontFamily RegisterFont(string fontName, string fileName)
		{
			float glyphScale;
			byte[] data;
			int pointSize;
			idFont outFont;
			idFontFamily fontFamily = new idFontFamily(fontName);
			string filePath;

			for(int fontCount = 0; fontCount < 3; fontCount++)
			{
				if(fontCount == 0)
				{
					pointSize = 12;
				}
				else if(fontCount == 1)
				{
					pointSize = 24;
				}
				else
				{
					pointSize = 48;
				}

				// we also need to adjust the scale based on point size relative to 48 points as the ui scaling is based on a 48 point font.
				// change the scale to be relative to 1 based on 72 dpi ( so dpi of 144 means a scale of .5 )
				glyphScale = 1.0f;
				glyphScale *= 48.0f / pointSize;

				filePath = string.Format("{0}/fontImage_{1}.dat", fileName, pointSize);
				data = idE.FileSystem.ReadFile(filePath);

				if(data == null)
				{
					idConsole.Warning("RegisterFont: couldn't find font: {0}", fileName);
					return null;
				}

				using(BinaryReader r = new BinaryReader(new MemoryStream(data)))
				{
					outFont = new idFont(filePath);
					outFont.Init(r, fileName);

					if(fontCount == 0)
					{
						fontFamily.Small = outFont;
					}
					else if(fontCount == 1)
					{
						fontFamily.Medium = outFont;
					}
					else
					{
						fontFamily.Large = outFont;
					}
				}
			}

			return fontFamily;
		}

		/// <summary>
		/// A view may be either the actual camera view,
		/// a mirror / remote location, or a 3D view on a gui surface.
		/// </summary>
		/// <param name="parms"></param>
		public void RenderView(View parms)
		{
			if((parms.RenderView.Width <= 0) || (parms.RenderView.Height <= 0))
			{
				return;
			}
		
			// save view in case we are a subview
			View oldView = _viewDefinition;

			_viewCount++;
			_viewDefinition = parms;
			_sortOffset = 0;

			// set the matrix for world space to eye space
			SetViewMatrix(_viewDefinition);

			// the four sides of the view frustum are needed
			// for culling and portal visibility
			SetupViewFrustum();

			// we need to set the projection matrix before doing
			// portal-to-screen scissor box calculations
			SetupProjection();

			// identify all the visible portalAreas, and the entityDefs and
			// lightDefs that are in them and pass culling.
			parms.RenderWorld.FindViewLightsAndEntities();

			// constrain the view frustum to the view lights and entities
			ConstrainViewFrustum();

			// make sure that interactions exist for all light / entity combinations
			// that are visible
			// add any pre-generated light shadows, and calculate the light shader values
			// TODO: R_AddLightSurfaces();

			// adds ambient surfaces and create any necessary interaction surfaces to add to the light
			// lists
			AddModelSurfaces();

			// any viewLight that didn't have visible surfaces can have it's shadows removed
			// TODO: R_RemoveUnecessaryViewLights();

			// sort all the ambient surfaces for translucency ordering
			// TODO: R_SortDrawSurfs();

			// generate any subviews (mirrors, cameras, etc) before adding this view
			// TODO: R_GenerateSubViews
			/*if(R_GenerateSubViews())
			{
				// if we are debugging subviews, allow the skipping of the
				// main view draw
				if(idE.CvarSystem.GetBool("r_subviewOnly") == true)
				{
					return;
				}
			}*/

			// write everything needed to the demo file
			// TODO: demo
			/*if ( session->writeDemo ) {
				static_cast<idRenderWorldLocal *>(parms->renderWorld)->WriteVisibleDefs( tr.viewDef );
			}*/

			// add the rendering commands for this viewDef
			AddDrawViewCommand(parms);

			// restore view in case we are a subview
			_viewDefinition = oldView;
		}

		/// <summary>
		/// Converts from SCREEN_WIDTH / SCREEN_HEIGHT coordinates to current cropped pixel coordinates
		/// </summary>
		/// <param name="renderView"></param>
		/// <returns></returns>
		public idScreenRect RenderViewToViewPort(idRenderView renderView)
		{
			idRectangle renderCrop = _renderCrops[_currentRenderCrop];

			float widthRatio = (float) renderCrop.Width / idE.VirtualScreenWidth;
			float heightRatio = (float) renderCrop.Height / idE.VirtualScreenHeight;

			idScreenRect viewPort = new idScreenRect();
			viewPort.X1 = (short) (renderCrop.X + renderView.X * widthRatio);
			viewPort.X2 = (short) ((renderCrop.X + idMath.Floor(renderView.X + renderView.Width) * widthRatio + 0.5f) - 1);
			viewPort.Y1 = (short) ((renderCrop.Y + renderCrop.Height) - idMath.Floor((renderView.Y + renderView.Height) * heightRatio + 0.5f));
			viewPort.Y2 = (short) ((renderCrop.Y + renderCrop.Height) - idMath.Floor(renderView.Y * heightRatio + 0.5f) - 1);

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
		/// Adds surfaces for the given viewEntity
		/// Walks through the viewEntitys list and creates drawSurf_t for each surface of
		/// each viewEntity that has a non-empty scissorRect.
		/// </summary>
		/// <param name="viewEntity"></param>
		private void AddAmbientDrawSurfaces(ViewEntity viewEntity)
		{
			idRenderEntity def = viewEntity.EntityDef;
			idRenderModel model;
			idMaterial material;
			Surface geometry;

			if(def.DynamicModel != null)
			{
				model = def.DynamicModel;
			}
			else
			{
				model = def.Parameters.Model;
			}

			// add all the surfaces
			int total = model.SurfaceCount;

			for(int i = 0; i < total; i++)
			{
				RenderModelSurface surface = model.GetSurface(i);

				// for debugging, only show a single surface at a time
				if((idE.CvarSystem.GetInteger("r_singleSurface") >= 0) && (i != idE.CvarSystem.GetInteger("r_singleSurface")))
				{
					continue;
				}

				geometry = surface.Geometry;

				if(geometry == null)
				{
					continue;
				}
				else if(geometry.Indexes.Length == 0)
				{
					continue;
				}

				material = surface.Material;				
				material = RemapMaterialBySkin(material, def.Parameters.CustomSkin, def.Parameters.CustomMaterial);
				material = GlobalMaterialOverride(material);

				if(material == null)
				{
					continue;
				}
				else if(material.IsDrawn == false)
				{
					continue;
				}

				// debugging tool to make sure we are have the correct pre-calculated bounds
				if(idE.CvarSystem.GetBool("r_checkBounds") == true)
				{
					idConsole.Warning("TODO: r_checkBounds");
					/*int j, k;
					for ( j = 0 ; j < tri->numVerts ; j++ ) {
						for ( k = 0 ; k < 3 ; k++ ) {
							if ( tri->verts[j].xyz[k] > tri->bounds[1][k] + CHECK_BOUNDS_EPSILON
								|| tri->verts[j].xyz[k] < tri->bounds[0][k] - CHECK_BOUNDS_EPSILON ) {
								common->Printf( "bad tri->bounds on %s:%s\n", def->parms.hModel->Name(), shader->GetName() );
								break;
							}
							if ( tri->verts[j].xyz[k] > def->referenceBounds[1][k] + CHECK_BOUNDS_EPSILON
								|| tri->verts[j].xyz[k] < def->referenceBounds[0][k] - CHECK_BOUNDS_EPSILON ) {
								common->Printf( "bad referenceBounds on %s:%s\n", def->parms.hModel->Name(), shader->GetName() );
								break;
							}
						}
						if ( k != 3 ) {
							break;
						}
					}*/
				}

				// TODO: CullLocalBox
				// if ( !R_CullLocalBox( tri->bounds, vEntity->modelMatrix, 5, tr.viewDef->frustum ) ) {
				{
					def.ViewCount = this.ViewCount;

					// make sure we have an ambient cache
					if(CreateAmbientCache(geometry, /* TODO: shader->ReceivesLighting() */ false) == false)
					{
						// don't add anything if the vertex cache was too full to give us an ambient cache
						return;
					}
										
					// touch it so it won't get purged
					//vertexCache.Touch( tri->ambientCache );

					/*if ( r_useIndexBuffers.GetBool() && !tri->indexCache ) {
						vertexCache.Alloc( tri->indexes, tri->numIndexes * sizeof( tri->indexes[0] ), &tri->indexCache, true );
					}
					if ( tri->indexCache ) {
						vertexCache.Touch( tri->indexCache );
					}*/
					
					// add the surface for drawing					
					AddDrawSurface(geometry, viewEntity, viewEntity.EntityDef.Parameters, material, viewEntity.ScissorRectangle);

					// ambientViewCount is used to allow light interactions to be rejected
					// if the ambient surface isn't visible at all
					geometry.AmbientViewCount = this.ViewCount;
				}
			}

			// add the lightweight decal surfaces
			// TODO: decals
			/*for ( idRenderModelDecal *decal = def->decals; decal; decal = decal->Next() ) {
				decal->AddDecalDrawSurf( vEntity );
			}*/
		}


		/// <remarks>
		/// Here is where dynamic models actually get instantiated, and necessary
		/// interactions get created.  This is all done on a sort-by-model basis
		/// to keep source data in cache (most likely L2) as any interactions and
		/// shadows are generated, since dynamic models will typically be lit by
		/// two or more lights.
		/// </remarks>
		private void AddModelSurfaces()
		{	
			// go through each entity that is either visible to the view, or to
			// any light that intersects the view (for shadows)
			foreach(ViewEntity viewEntity in _viewDefinition.ViewEntities)
			{
				if(idE.CvarSystem.GetBool("r_useEntityScissors") == true)
				{
					idConsole.Warning("TODO: entity scissor rect");
					
					/*// calculate the screen area covered by the entity
					idScreenRect scissorRect = R_CalcEntityScissorRectangle( vEntity );
					// intersect with the portal crossing scissor rectangle
					vEntity->scissorRect.Intersect( scissorRect );

					if ( r_showEntityScissors.GetBool() ) {
						R_ShowColoredScreenRect( vEntity->scissorRect, vEntity->entityDef->index );
					}*/
				}

				float oldFloatTime = 0;
				int oldTime = 0;

				idE.Game.SelectTimeGroup(viewEntity.EntityDef.Parameters.TimeGroup);

				if(viewEntity.EntityDef.Parameters.TimeGroup > 0)
				{
					oldFloatTime = _viewDefinition.FloatTime;
					oldTime = _viewDefinition.RenderView.Time;

					_viewDefinition.FloatTime = idE.Game.GetTimeGroupTime(viewEntity.EntityDef.Parameters.TimeGroup) * 0.001f;
					_viewDefinition.RenderView.Time = idE.Game.GetTimeGroupTime(viewEntity.EntityDef.Parameters.TimeGroup);
				}

				if((_viewDefinition.IsXraySubview == true) && (viewEntity.EntityDef.Parameters.XrayIndex == 1))
				{
					if(viewEntity.EntityDef.Parameters.TimeGroup > 0)
					{
						_viewDefinition.FloatTime = oldFloatTime;
						_viewDefinition.RenderView.Time = oldTime;
					}

					continue;
				} 
				else if((_viewDefinition.IsXraySubview == false) && (viewEntity.EntityDef.Parameters.XrayIndex == 2))
				{
					if(viewEntity.EntityDef.Parameters.TimeGroup > 0)
					{
						_viewDefinition.FloatTime = oldFloatTime;
						_viewDefinition.RenderView.Time = oldTime;
					}
				
					continue;
				}

				// add the ambient surface if it has a visible rectangle
				if(viewEntity.ScissorRectangle.IsEmpty == false)
				{
					idRenderModel model = EntityDefinitionDynamicModel(viewEntity.EntityDef);

					if((model == null) | (model.SurfaceCount <= 0))
					{
						if(viewEntity.EntityDef.Parameters.TimeGroup != 0)
						{
							this.ViewDefinition.FloatTime = oldFloatTime;
							this.ViewDefinition.RenderView.Time = oldTime;
						}

						continue;
					}

					AddAmbientDrawSurfaces(viewEntity);
					
					// TODO: tr.pc.c_visibleViewEntities++;
				} 
				else 
				{
					// TODO: tr.pc.c_shadowViewEntities++;
				}

				//
				// for all the entity / light interactions on this entity, add them to the view
				//
				if(_viewDefinition.IsXraySubview == true)
				{
					if(viewEntity.EntityDef.Parameters.XrayIndex == 2)
					{
						idConsole.Warning("TODO: xrayindex == 2");

						/*for ( inter = vEntity->entityDef->firstInteraction; inter != NULL && !inter->IsEmpty(); inter = next ) {
							next = inter->entityNext;
							if ( inter->lightDef->viewCount != tr.viewCount ) {
								continue;
							}
							inter->AddActiveInteraction();
						}*/
					}
				} 
				else
				{
					idConsole.Warning("TODO: interactions");

					// all empty interactions are at the end of the list so once the
					// first is encountered all the remaining interactions are empty
					/*for ( inter = vEntity->entityDef->firstInteraction; inter != NULL && !inter->IsEmpty(); inter = next ) {
						next = inter->entityNext;

						// skip any lights that aren't currently visible
						// this is run after any lights that are turned off have already
						// been removed from the viewLights list, and had their viewCount cleared
						if ( inter->lightDef->viewCount != tr.viewCount ) {
							continue;
						}
						inter->AddActiveInteraction();
					}*/
				}

				if(viewEntity.EntityDef.Parameters.TimeGroup > 0)
				{
					_viewDefinition.FloatTime = oldFloatTime;
					_viewDefinition.RenderView.Time = oldTime;
				}
			}
		}
		
		private void Clear()
		{
			_frameCount = 0;
			_viewCount = 0;

			_staticAllocCount = 0;
			_frameShaderTime = 0;

			_viewPortOffset = Vector2.Zero;
			_tiledViewPort = Vector2.Zero;

			_ambientLightVector = Vector4.Zero;
			_sortOffset = 0;
			_worlds.Clear();

			_primaryRenderWorld = null;
			_primaryView = null;
			_primaryRenderView = new idRenderView();

			_defaultMaterial = null;

			/*
			testImage = NULL;
			ambientCubeImage = NULL;*/

			_viewDefinition = null;

			/*memset( &pc, 0, sizeof( pc ) );
			memset( &lockSurfacesCmd, 0, sizeof( lockSurfacesCmd ) );*/

			_identitySpace = new ViewEntity();

			/*logFile = NULL;*/

			_renderCrops = new idRectangle[idE.MaxRenderCrops];
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

		private void ConstrainViewFrustum()
		{
			idBounds bounds = new idBounds();

			// constrain the view frustum to the total bounds of all visible lights and visible entities
			
			// TODO: lights
			/*for(viewLight_t* vLight = tr.viewDef->viewLights; vLight; vLight = vLight->next)
			{
				bounds.AddBounds(vLight->lightDef->frustumTris->bounds);
			}*/
				
			foreach(ViewEntity viewEntity in _viewDefinition.ViewEntities)
			{
				bounds.AddBounds(viewEntity.EntityDef.ReferenceBounds);
			}
			
			_viewDefinition.ViewFrustum.ConstrainToBounds(bounds);

			float farDistance = idE.CvarSystem.GetFloat("r_useFrustumFarDistance");

			if(farDistance > 0.0f)
			{
				_viewDefinition.ViewFrustum.MoveFarDistance(farDistance);
			}
		}

		private bool CreateAmbientCache(Surface geometry, bool needsLighting)
		{
			if(geometry.AmbientCache != null)
			{
				return true;
			}

			// we are going to use it for drawing, so make sure we have the tangents and normals
			// TODO: lighting
			/*if ( needsLighting && !tri->tangentsCalculated ) {
				R_DeriveTangents( tri );
			}*/

			geometry.AmbientCache = new VertexCache();
			geometry.AmbientCache.Data = geometry.Vertices;

			return true;
		}
		
		/// <summary>
		/// Issues a deferred entity callback if necessary.
		/// If the model isn't dynamic, it returns the original.
		/// Returns the cached dynamic model if present, otherwise creates
		/// it and any necessary overlays
		/// </summary>
		/// <param name="def"></param>
		/// <returns></returns>
		private idRenderModel EntityDefinitionDynamicModel(idRenderEntity def) 
		{
			bool callbackUpdate;

			// allow deferred entities to construct themselves
			if(def.Parameters.Callback != null)
			{
				callbackUpdate = false;
				idConsole.Warning("TODO: R_IssueEntityDefCallback( def );");
			} 
			else 
			{
				callbackUpdate = false;
			}

			idRenderModel model = def.Parameters.Model;

			if(model == null)
			{
				idConsole.Error("EntityDefinitionDynamicModel: null model");
			}

			if(model.IsDynamic == DynamicModel.Static)
			{
				def.DynamicModel = null;
				def.DynamicModelFrameCount = 0;

				return model;
			}

			idConsole.Warning("TODO: dynamic model rendering!");
	// continously animating models (particle systems, etc) will have their snapshot updated every single view
	/*if ( callbackUpdate || ( model->IsDynamicModel() == DM_CONTINUOUS && def->dynamicModelFrameCount != tr.frameCount ) ) {
		R_ClearEntityDefDynamicModel( def );
	}

	// if we don't have a snapshot of the dynamic model, generate it now
	if ( !def->dynamicModel ) {

		// instantiate the snapshot of the dynamic model, possibly reusing memory from the cached snapshot
		def->cachedDynamicModel = model->InstantiateDynamicModel( &def->parms, tr.viewDef, def->cachedDynamicModel );

		if ( def->cachedDynamicModel ) {

			// add any overlays to the snapshot of the dynamic model
			if ( def->overlay && !r_skipOverlays.GetBool() ) {
				def->overlay->AddOverlaySurfacesToModel( def->cachedDynamicModel );
			} else {
				idRenderModelOverlay::RemoveOverlaySurfacesFromModel( def->cachedDynamicModel );
			}

			if ( r_checkBounds.GetBool() ) {
				idBounds b = def->cachedDynamicModel->Bounds();
				if (	b[0][0] < def->referenceBounds[0][0] - CHECK_BOUNDS_EPSILON ||
						b[0][1] < def->referenceBounds[0][1] - CHECK_BOUNDS_EPSILON ||
						b[0][2] < def->referenceBounds[0][2] - CHECK_BOUNDS_EPSILON ||
						b[1][0] > def->referenceBounds[1][0] + CHECK_BOUNDS_EPSILON ||
						b[1][1] > def->referenceBounds[1][1] + CHECK_BOUNDS_EPSILON ||
						b[1][2] > def->referenceBounds[1][2] + CHECK_BOUNDS_EPSILON ) {
					common->Printf( "entity %i dynamic model exceeded reference bounds\n", def->index );
				}
			}
		}

		def->dynamicModel = def->cachedDynamicModel;
		def->dynamicModelFrameCount = tr.frameCount;
	}

	// set model depth hack value
	if ( def->dynamicModel && model->DepthHack() != 0.0f && tr.viewDef ) {
		idPlane eye, clip;
		idVec3 ndc;
		R_TransformModelToClip( def->parms.origin, tr.viewDef->worldSpace.modelViewMatrix, tr.viewDef->projectionMatrix, eye, clip );
		R_TransformClipToDevice( clip, tr.viewDef, ndc );
		def->parms.modelDepthHack = model->DepthHack() * ( 1.0f - ndc.z );
	}
*/
	
			// FIXME: if any of the surfaces have deforms, create a frame-temporary model with references to the
			// undeformed surfaces.  This would allow deforms to be light interacting.

			return def.DynamicModel;
		}	

		public idMaterial GlobalMaterialOverride(idMaterial material)
		{
			if(material.IsDrawn == false)
			{
				return material;
			}
			else if(this.PrimaryRenderView.GlobalMaterial != null)
			{
				return this.PrimaryRenderView.GlobalMaterial;
			}
			else if(idE.CvarSystem.GetString("r_materialOverride") != string.Empty)
			{
				return idE.DeclManager.FindMaterial(idE.CvarSystem.GetString("r_materialOverride"));
			}

			return material;
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
				_backendRenderer.Execute(_frameData.Commands);
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
		
		public idMaterial RemapMaterialBySkin(idMaterial material, idDeclSkin skin, idMaterial customMaterial)
		{
			if(material == null)
			{
				return null;
			}

			// never remap surfaces that were originally nodraw, like collision hulls
			if(material.IsDrawn == false)
			{
				return material;
			}

			if(customMaterial != null)
			{
				// this is sort of a hack, but cause deformed surfaces to map to empty surfaces,
				// so the item highlight overlay doesn't highlight the autosprite surface
				if(material.Deform != DeformType.None)
				{
					return null;
				}

				return customMaterial;
			}

			if((skin == null) || (material == null))
			{
				return material;
			}

			return skin.RemapShaderBySkin(material);
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
		
		private void ToggleSmpFrame()
		{
			if(idE.CvarSystem.GetBool("r_lockSurfaces") == true)
			{
				return;
			}

			// idConsole.Warning("TODO: ToggleSmpFrame");

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

			idConsole.Warning("TODO: SetDeviceGammaRamp");
			/*if ( !SetDeviceGammaRamp( win32.hDC, table ) ) {
				common->Printf( "WARNING: SetDeviceGammaRamp failed.\n" );
			}*/
		}
		
		private void SetupProjection()
		{
			float jitterX = 0;
			float jitterY = 0;

			// random jittering is usefull when multiple
			// frames are going to be blended together
			// for motion blurred anti-aliasing
			if(idE.CvarSystem.GetBool("r_jitter") == true)
			{
				Random r = new Random();

				jitterX = (float) r.NextDouble();
				jitterY = (float) r.NextDouble();
			}

			//
			// set up projection matrix
			//
			float zNear = idE.CvarSystem.GetFloat("r_znear");

			if(_viewDefinition.RenderView.CramZNear == true)
			{
				zNear *= 0.25f;
			}

			float yMax = zNear * idMath.Tan(_viewDefinition.RenderView.FovY * idMath.Pi / 360.0f);
			float yMin = -yMax;

			float xMax = zNear * idMath.Tan(_viewDefinition.RenderView.FovX * idMath.Pi / 360.0f);
			float xMin = -xMax;

			float width = xMax = xMin;
			float height = yMax - yMin;

			jitterX = jitterX * width / (_viewDefinition.ViewPort.X2 - _viewDefinition.ViewPort.X1 + 1);

			xMin += jitterX;
			xMax += jitterX;

			jitterY = jitterY * height / (_viewDefinition.ViewPort.Y2 - _viewDefinition.ViewPort.Y1 + 1);

			yMin += jitterY;
			yMax += jitterY;

			Matrix m = new Matrix();
			m.M11 = 2 * zNear / width;
			m.M21 = 0;
			m.M31 = (xMax + xMin) / width; // normally 0
			m.M41 = 0;

			m.M12 = 0;
			m.M22 = 2 * zNear / height;
			m.M32 = (yMax + yMin) / height; // normally 0
			m.M42 = 0;

			// this is the far-plane-at-infinity formulation, and
			// crunches the Z range slightly so w=0 vertexes do not
			// rasterize right at the wraparound point
			m.M13 = 0;
			m.M23 = 0;
			m.M33 = -0.999f;
			m.M43 = -2.0f * zNear;

			m.M14 = 0;
			m.M24 = 0;
			m.M34 = -1;
			m.M44 = 0;

			_viewDefinition.ProjectionMatrix = m;
		}

		private void SetupViewFrustum()
		{
			float xs, xc;
			float ang = MathHelper.ToRadians(_viewDefinition.RenderView.FovX) * 0.5f;

			idMath.SinCos(ang, out xs, out xc);

			Vector3 tmp = xs * new Vector3(_viewDefinition.RenderView.ViewAxis.M11, _viewDefinition.RenderView.ViewAxis.M12, _viewDefinition.RenderView.ViewAxis.M13)
							+ xc * new Vector3(_viewDefinition.RenderView.ViewAxis.M21, _viewDefinition.RenderView.ViewAxis.M22, _viewDefinition.RenderView.ViewAxis.M23);

			Vector3 tmp2 = xs * new Vector3(_viewDefinition.RenderView.ViewAxis.M11, _viewDefinition.RenderView.ViewAxis.M12, _viewDefinition.RenderView.ViewAxis.M13)
							- xc * new Vector3(_viewDefinition.RenderView.ViewAxis.M21, _viewDefinition.RenderView.ViewAxis.M22, _viewDefinition.RenderView.ViewAxis.M23);

			_viewDefinition.Frustum[0] = new Plane(tmp.X, tmp.Y, tmp.Z, 0);
			_viewDefinition.Frustum[1] = new Plane(tmp2.X, tmp2.Y, tmp2.Z, 0);

			ang = MathHelper.ToRadians(_viewDefinition.RenderView.FovY) * 0.5f;

			idMath.SinCos(ang, out xs, out xc);

			tmp = xs * new Vector3(_viewDefinition.RenderView.ViewAxis.M11, _viewDefinition.RenderView.ViewAxis.M12, _viewDefinition.RenderView.ViewAxis.M13)
							+ xc * new Vector3(_viewDefinition.RenderView.ViewAxis.M21, _viewDefinition.RenderView.ViewAxis.M22, _viewDefinition.RenderView.ViewAxis.M23);

			tmp2 = xs * new Vector3(_viewDefinition.RenderView.ViewAxis.M11, _viewDefinition.RenderView.ViewAxis.M12, _viewDefinition.RenderView.ViewAxis.M13)
							- xc * new Vector3(_viewDefinition.RenderView.ViewAxis.M21, _viewDefinition.RenderView.ViewAxis.M22, _viewDefinition.RenderView.ViewAxis.M23);

			_viewDefinition.Frustum[2] = new Plane(tmp.X, tmp.Y, tmp.Z, 0);
			_viewDefinition.Frustum[3] = new Plane(tmp2.X, tmp2.Y, tmp2.Z, 0);

			// plane four is the front clipping plane
			tmp = new Vector3(_viewDefinition.RenderView.ViewAxis.M11, _viewDefinition.RenderView.ViewAxis.M12, _viewDefinition.RenderView.ViewAxis.M13);

			_viewDefinition.Frustum[4] = new Plane(tmp.X, tmp.Y, tmp.Z, 0);

			for(int i = 0; i < 5; i++)
			{
				tmp = -_viewDefinition.Frustum[i].Normal;
				tmp.Z = -(_viewDefinition.RenderView.ViewOrigin * _viewDefinition.Frustum[i].Normal).Length();

				// flip direction so positive side faces out (FIXME: globally unify this)
				_viewDefinition.Frustum[i] = new Plane(tmp.X, tmp.Y, tmp.Z, 0);
			}

			// eventually, plane five will be the rear clipping plane for fog

			float dNear = idE.CvarSystem.GetFloat("r_znear");

			if(_viewDefinition.RenderView.CramZNear == true)
			{
				dNear *= 0.25f;
			}

			float dFar = idE.MaxWorldSize;
			float dLeft = dFar * idMath.Tan(MathHelper.ToRadians(_viewDefinition.RenderView.FovX * 0.5f));
			float dUp = dFar * idMath.Tan(MathHelper.ToRadians(_viewDefinition.RenderView.FovY * 0.5f));

			_viewDefinition.ViewFrustum = new idFrustum();
			_viewDefinition.ViewFrustum.Origin = _viewDefinition.RenderView.ViewOrigin;
			_viewDefinition.ViewFrustum.Axis = _viewDefinition.RenderView.ViewAxis;
			_viewDefinition.ViewFrustum.SetSize(dNear, dFar, dLeft, dUp);
		}

		private void SetViewMatrix(View view)
		{
			float[]	viewerMatrix = new float[16];

			ViewEntity world = new ViewEntity();
			
			// transform by the camera placement
			Vector3 origin = view.RenderView.ViewOrigin;
			
			viewerMatrix[0] = _viewDefinition.RenderView.ViewAxis.M11;
			viewerMatrix[4] = _viewDefinition.RenderView.ViewAxis.M12;
			viewerMatrix[8] = _viewDefinition.RenderView.ViewAxis.M13;
			viewerMatrix[12] = -origin.X * viewerMatrix[0] + -origin.Y * viewerMatrix[4] + -origin.Z * viewerMatrix[8];

			viewerMatrix[1] = _viewDefinition.RenderView.ViewAxis.M21;
			viewerMatrix[5] = _viewDefinition.RenderView.ViewAxis.M22;
			viewerMatrix[9] = _viewDefinition.RenderView.ViewAxis.M23;
			viewerMatrix[13] = -origin.X * viewerMatrix[1] + -origin.Y * viewerMatrix[5] + -origin.Z * viewerMatrix[9];

			viewerMatrix[2] = _viewDefinition.RenderView.ViewAxis.M31;
			viewerMatrix[6] = _viewDefinition.RenderView.ViewAxis.M32;
			viewerMatrix[10] = _viewDefinition.RenderView.ViewAxis.M33;
			viewerMatrix[14] = -origin.X * viewerMatrix[2] + -origin.Y * viewerMatrix[6] + -origin.Z * viewerMatrix[10];

			viewerMatrix[3] = 0;
			viewerMatrix[7] = 0;
			viewerMatrix[11] = 0;
			viewerMatrix[15] = 1;

			view.WorldSpace = world;
			
			// convert from our coordinate system (looking down X)
			// to OpenGL's coordinate system (looking down -Z)
			idHelper.ConvertMatrix(viewerMatrix, FlipMatrix, out view.WorldSpace.ModelViewMatrix);
		}
		#endregion
		
		#region Command handlers
		private void Cmd_ReloadArbPrograms(object sender, CommandEventArgs e)
		{
			idConsole.WriteLine("----- R_ReloadARBPrograms -----");

			int count = _programs.Length;

			for(int i = 0; i < count; i++)
			{
				LoadArbProgram(i);
			}

			idConsole.WriteLine("-------------------------------");
		}
		#endregion
		#endregion
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
		public int MaxTextureAnisotropy;

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
		public TextureFilter Filter = TextureFilter.Default;
		public TextureRepeat Repeat = TextureRepeat.Clamp;
	}

	public class View
	{
		public idRenderView RenderView = new idRenderView();

		public Matrix ProjectionMatrix = Matrix.Identity;
		public ViewEntity WorldSpace = new ViewEntity();
		public idRenderWorld RenderWorld;

		public float FloatTime;

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Used to find the portalArea that view flooding will take place from.
		/// for a normal view, the initialViewOrigin will be renderView.viewOrg,
		/// but a mirror may put the projection origin outside
		/// of any valid area, or in an unconnected area of the map, so the view
		/// area must be based on a point just off the surface of the mirror/subview.
		/// <para/>
		/// It may be possible to get a failed portal pass if the plane of the
		/// mirror intersects a portal, and the initialViewAreaOrigin is on
		/// a different side than the renderView.viewOrg is.
		/// </remarks>
		public Vector3 InitialViewAreaOrigin;

		/// <summary>
		/// True if this view is not the main view.
		/// </summary>
		public bool IsSubview;

		/// <summary>
		/// True if the portal is a mirror, invert the face culling.
		/// </summary>
		public bool IsMirror;
		public bool IsXraySubview;
		public bool IsEditor;

		/*int					numClipPlanes;			// mirrors will often use a single clip plane
		idPlane				clipPlanes[MAX_CLIP_PLANES];		// in world space, the positive side
													// of the plane is the visible side
		 * */

		/// <summary>
		/// In real pixels and proper Y flip.
		/// </summary>
		public idScreenRect ViewPort; // i

		/// <summary>
		/// For scissor clipping, local inside renderView viewport.
		/// </summary>
		/// <remarks>
		/// Subviews may only be rendering part of the main view
		/// these are real physical pixel values, possibly scaled and offset from the
		/// renderView x/y/width/height
		/// </remarks>
		public idScreenRect Scissor;

		/*struct viewDef_s *	superView;				// never go into an infinite subview loop 
		struct drawSurf_s *	subviewSurface;*/

		/// <summary>
		/// DrawSurfs are the visible surfaces of the viewEntities, sorted
		/// by the material sort parameter
		/// </summary>
		public List<DrawSurface> DrawSurfaces = new List<DrawSurface>();

		/*struct viewLight_s	*viewLights;		// chain of all viewLights effecting view*/
		public List<ViewEntity> ViewEntities = new List<ViewEntity>();
													// chain of all viewEntities effecting view, including off screen ones casting shadows
													// we use viewEntities as a check to see if a given view consists solely
													// of 2D rendering, which we can optimize in certain ways.  A 2D view will
													// not have any viewEntities
		

		public Plane[] Frustum = new Plane[5];		// positive sides face outward, [4] is the front clip plane
		public idFrustum ViewFrustum;

		public int AreaNumber = -1; // -1 = not in a valid area

		public bool[] ConnectedAreas;
		// An array in frame temporary memory that lists if an area can be reached without
		// crossing a closed door.  This is used to avoid drawing interactions
		// when the light is behind a closed door.
	}
	
	public delegate bool DeferredEntityCallback(idRenderEntity renderEntity, idRenderView renderView);

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Entities that are expensive to generate, like skeletal models, can be
	/// deferred until their bounds are found to be in view, in the frustum
	/// of a shadowing light that is in view, or contacted by a trace / overlay test.
	/// This is also used to do visual cueing on items in the view
	/// The renderView may be NULL if the callback is being issued for a non-view related
	/// source.
	/// <para/>
	/// The callback function should clear renderEntity->callback if it doesn't
	/// want to be called again next time the entity is referenced (ie, if the
	/// callback has now made the entity valid until the next updateEntity)
	/// </remarks>
	public class RenderEntityComponent
	{
		/// <remarks>
		/// This can only be null if callback is set.
		/// </remarks>
		public idRenderModel Model;

		public int EntityIndex;
		public int BodyID;

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Only needs to be set for deferred models and md5s.
		/// </remarks>
		public idBounds	Bounds;
		
		public DeferredEntityCallback	Callback;
		public object CallbackData;
		
		/// <summary>
		/// Suppress the model in the given view.
		/// </summary>
		/// <remarks>
		/// Security cameras could suppress their model in their subviews if we add a way
		/// of specifying a view number for a remoteRenderMap view.
		/// </remarks>
		public int SuppressSurfaceInViewID;

		/// <summary>
		/// Suppress shadows from this entity in the given view.
		/// </summary>
		/// <remarks>
		/// Player bodies and possibly player shadows should be suppressed in views from
		/// that player's eyes, but will show up in mirrors and other subviews.
		/// </remarks>
		public int SuppressShadowInViewID;

		/// <summary>
		/// Suppress shadows for the given light.
		/// </summary>
		/// <remarks>
		/// World models for the player and weapons will not cast shadows from view weapon
		/// muzzle flashes.
		/// </remarks>
		public int SuppressShadowInLightID;

		/// <summary>
		/// Draw the model only in this view.
		/// </summary>
		/// <remarks>
		/// If non-zero, the surface and shadow (if it casts one)
		/// will only show up in the specific view, ie: player weapons.
		/// </remarks>
		public int AllowSurfaceInViewID;

		/// <summary>
		/// Position.
		/// </summary>
		public Vector3 Origin;

		/// <summary>
		/// Orientation.
		/// </summary>
		/// <remarks>
		/// Axis rotation vectors must be unit length for many R_LocalToGlobal functions to work, so don't scale models!
		/// <para/>
		/// Axis vectors are [0] = forward, [1] = left, [2] = up.
		/// </remarks>
		public Matrix Axis;

		/// <summary>
		/// Force a specific material.
		/// </summary>
		public idMaterial CustomMaterial;

		/// <summary>
		/// Used so flares can reference the proper light material.
		/// </summary>
		public idMaterial ReferenceMaterial;

		/// <summary>
		/// Force a specific skin.
		/// </summary>
		public idDeclSkin CustomSkin;

		/*class idSoundEmitter *	referenceSound;			// for shader sound tables, allowing effects to vary with sounds*/

		/// <summary>
		/// Can be used in any way by material or model generation.
		/// </summary>
		public float[] MaterialParameters = new float[idE.MaxEntityMaterialParameters];

		// networking: see WriteGUIToSnapshot / ReadGUIFromSnapshot
		public idUserInterface[] Gui = new idUserInterface[idE.MaxRenderEntityGui];

		/*struct renderView_s	*	remoteRenderView;		// any remote camera surfaces will use this

		int						numJoints;*/

		public idJointMatrix[] Joints;						// array of joints that will modify vertices.
															// NULL if non-deformable model.  NOT freed by renderer

		/// <summary>
		/// Squash depth range so particle effects don't clip into walls.
		/// </summary>
		public float ModelDepthHack;

		/// <summary>
		/// Cast shadows onto other objects,but not self.
		/// </summary>
		/// <remarks>
		/// Overrides surface material flags.
		/// </remarks>
		public bool NoSelfShadow;

		/// <summary>
		/// No shadows at all.
		/// </summary>
		/// <remarks>
		/// Overrides surface material flags.
		/// </remarks>
		public bool NoShadow;

		/// <summary>
		/// Don't create any light / shadow interactions after the level load is completed.
		/// </summary>
		/// <remarks>
		/// This is a performance hack for the gigantic outdoor meshes in the monorail 
		/// map, so all the lights in the moving monorail don't touch the meshes
		/// </remarks>
		public bool NoDynamicInteractions;

		/// <summary>
		/// Squash depth range so view weapons don't poke into walls.
		/// </summary>
		/// <remarks>
		/// This automatically implies noShadow.
		/// </remarks>
		public bool WeaponDepthHack;

		public bool ForceUpdate; // force an update
		public int TimeGroup;
		public int XrayIndex;
	}

	public class idRenderView
	{
		/// <summary>
		/// Model/Subview suppression.
		/// </summary>
		/// <remarks>
		/// Player views will set this to a non-zero integer for model suppress/allow.
		/// <para/>
		/// Subviews (mirrors, cameras, etc) will always clear it to zero.
		/// </remarks>
		public int ViewID;

		/// <summary>
		/// Sized from 0 to SCREEN_WIDTH / SCREEN_HEIGHT (640/480), not actual resolution.
		/// </summary>
		public int X;

		/// <summary>
		/// Sized from 0 to SCREEN_WIDTH / SCREEN_HEIGHT (640/480), not actual resolution.
		/// </summary>
		public int Y;

		/// <summary>
		/// Sized from 0 to SCREEN_WIDTH / SCREEN_HEIGHT (640/480), not actual resolution.
		/// </summary>
		public int Width;

		/// <summary>
		/// Sized from 0 to SCREEN_WIDTH / SCREEN_HEIGHT (640/480), not actual resolution.
		/// </summary>
		public int Height;

		public float FovX;
		public float FovY;

		public Vector3 ViewOrigin;

		/// <summary>
		/// Transformation matrix, view looks down the positive X axis.
		/// </summary>
		public Matrix ViewAxis;

		/// <summary>
		/// For cinematics, we want to set ZNear much lower.
		/// </summary>
		public bool CramZNear;

		/// <summary>
		/// Force an update.
		/// </summary>
		public bool ForceUpdate;

		/// <summary>
		/// Time in milliseconds for material effects and other time dependent rendering issues.
		/// </summary>
		public int Time;

		/// <summary>
		/// Can be used in any way by the material.
		/// </summary>
		public float[] MaterialParameters;

		/// <summary>
		/// Override everything when drawing.
		/// </summary>
		/// 
		public idMaterial GlobalMaterial;

		public idRenderView()
		{
			Clear();
		}

		public void Clear()
		{
			this.ViewID = 0;
			this.X = 0;
			this.Y = 0;
			this.Width = 0;
			this.Height = 0;
			this.FovX = 0;
			this.FovY = 0;

			this.ViewOrigin = Vector3.Zero;
			this.ViewAxis = Matrix.Identity;

			this.CramZNear = false;
			this.ForceUpdate = false;
			this.Time = 0;

			this.MaterialParameters = new float[idE.MaxGlobalMaterialParameters];
			this.GlobalMaterial = null;
		}

		/// <summary>
		/// Creates a shallow copy of this instance.
		/// </summary>
		public idRenderView Copy()
		{
			idRenderView view = new idRenderView();
			view.ViewID = this.ViewID;
			view.X = this.X;
			view.Y = this.Y;
			view.Width = this.Width;
			view.Height = this.Height;
			view.FovX = this.FovX;
			view.FovY = this.FovY;

			view.ViewOrigin = this.ViewOrigin;
			view.ViewAxis = this.ViewAxis;

			view.CramZNear = this.CramZNear;
			view.ForceUpdate = this.ForceUpdate;
			view.Time = this.Time;

			view.MaterialParameters = this.MaterialParameters;
			view.GlobalMaterial = this.GlobalMaterial;

			return view;
		}
	}

	public class ViewEntity
	{
		// back end should NOT reference the entityDef, because it can change when running SMP
		public idRenderEntity EntityDef;

		// for scissor clipping, local inside renderView viewport
		// scissorRect.Empty() is true if the viewEntity_t was never actually
		// seen through any portals, but was created for shadow casting.
		// a viewEntity can have a non-empty scissorRect, meaning that an area
		// that it is in is visible, and still not be visible.
		public idScreenRect ScissorRectangle;

		public bool WeaponDepthHack;
		public float ModelDepthHack;

		public Matrix ModelMatrix = Matrix.Identity; // local coords to global coords
		public Matrix ModelViewMatrix = Matrix.Identity; // local coords to eye coords
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

	// TODO: IM - making this a class is possibly a bit heavy handed
	public class Surface : IDisposable
	{
		~Surface()
		{
			Dispose(false);
		}

		/// <summary>
		/// Used for culling.
		/// </summary>
		public idBounds Bounds;


		/// <summary>
		/// Create normals from geometry, instead of using explicit ones.
		/// </summary>
		public bool GenerateNormals;
		public int AmbientViewCount;		// if == tr.viewCount, it is visible this view

	/*bool						tangentsCalculated;		// set when the vertex tangents have been calculated
	bool						facePlanesCalculated;	// set when the face planes have been calculated
	bool						perfectHull;			// true if there aren't any dangling edges
	bool						deformedSurface;		// if true, indexes, silIndexes, mirrorVerts, and silEdges are
														// pointers into the original surface, and should not be freed*/

		public Vertex[] Vertices;

		/// <summary>
		/// For shadows, this has both front and rear end caps and silhouette planes.
		/// </summary>
		public int[] Indexes;

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
		*/
	
		/// <summary>
		/// Shadow volumes with front caps omitted.
		/// </summary>
		public int ShadowIndexesNoFrontCapsCount;

		/// <summary>
		/// Shadow volumes with the front and rear caps omitted.
		/// </summary>
		public int ShadowIndexesNoCapsCount;

		/// <summary>
		/// Plane flags.
		/// </summary>
		/// <remarks>
		/// Bits 0-5 are set when that plane of the interacting light has triangles
		/// projected on it, which means that if the view is on the outside of that
		/// plane, we need to draw the rear caps of the shadow volume
		/// turboShadows will have SHADOW_CAP_INFINITE.
		/// </remarks>
		public int ShadowCapPlaneBits;


		/// <summary>
		/// These will be copied to shadowCache when it is going to be drawn. 
		/// </summary>
		/// <remarks>
		/// NULL when vertex programs are available.
		/// </remarks>
		public ShadowVertex[] ShadowVertices;	

		/*struct srfTriangles_s *		ambientSurface;			// for light interactions, point back at the original surface that generated
															// the interaction, which we will get the ambientCache from

		struct srfTriangles_s *		nextDeferredFree;		// chain of tris to free next frame

		// data in vertex object space, not directly readable by the CPU*/

		public VertexCache IndexCache;
		public VertexCache AmbientCache;
		/*struct vertCache_s *		lightingCache;			// lightingCache_t
		struct vertCache_s *		shadowCache;			// shadowCache_t
	*/

		#region IDisposable implementation
		#region Properties
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}
		#endregion

		#region Members
		private bool _disposed;
		#endregion

		#region Methods
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(disposing == true)
			{
				this.Bounds = idBounds.Zero;
				this.Indexes = null;
				this.Vertices = null;
				this.ShadowVertices = null;
				this.IndexCache = null;
				this.AmbientCache = null;
			}

			_disposed = true;
		}
		#endregion
		#endregion
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

		#region Methods
		public void Clear()
		{
			this.Position = Vector3.Zero;
			this.TextureCoordinates = Vector2.Zero;
			this.Normal = Vector3.Zero;
			
			// TODO: tangents[0].Zero();
			// TODO: tangents[1].Zero();

			this.Color = Color.Black;
		}
		#endregion
	}

	public struct ShadowVertex : IVertexType
	{
		#region Vertex declaration
		public VertexDeclaration VertexDeclaration
		{
			get
			{
				return new VertexDeclaration(
					new VertexElement[] {
						new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0)
					}
				);
			}
		}
		#endregion

		#region Fields
		public Vector4 Position; // we use homogenous coordinate tricks
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

	public struct VideoMode
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

	public enum RenderCommandType
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

	public abstract class RenderCommand
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

	public delegate void RenderHandler(DrawSurface surface);
}