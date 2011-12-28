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
using System.Windows.Forms;

using Microsoft.Xna.Framework;

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

		public bool IsRunning
		{
			get
			{
				return idE.GLConfig.IsInitialized;
			}
		}
		#endregion

		#region Members
		private bool _registered;							// cleared at shutdown, set at InitOpenGL

		private int _frameCount;							// incremented every frame
		private int _viewCount;								// incremented every view (twice a scene if subviewed) and every R_MarkFragments call

		private Vector4 _ambientLightVector;				// used for "ambient bump mapping"

		private idMaterial _defaultMaterial;
		private ViewEntity _identitySpace;					// can use if we don't know viewDef->worldSpace is valid

		private List<idRenderWorld> _worlds = new List<idRenderWorld>();

		private BackEndRenderer _backEndRenderer;			// determines which back end to use, and if vertex programs are in use
		private bool _backEndRendererHasVertexPrograms;
		private float _backEndRendererMaxLight;				// 1.0 for standard, unlimited for floats

		private ushort[] _gammaTable = new ushort[256];		// brightness / gamma modify this
		// determines how much overbrighting needs
		// to be done post-process

		private int[] _viewPortOffset = new int[2];			// for doing larger-than-window tiled renderings
		private int[] _tiledViewPort = new int[2];

		private int _stencilIncrement;
		private int _stencilDecrement;						// GL_INCR / INCR_WRAP_EXT, GL_DECR / GL_DECR_EXT

		private int _fragmentDisplayListBase;				// FPROG_NUM_FRAGMENT_PROGRAMS lists
		
		// GUI drawing variables for surface creation
		private int _guiRecursionLevel;			// to prevent infinite overruns
		private idGuiModel _guiModel;
		private idGuiModel _demoGuiModel;

		private Form _renderForm;
		private SimpleOpenGlControl _renderControl;
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
		public void Init()
		{
			idConsole.WriteLine("------- Initializing renderSystem --------");

			// clear all our internal state
			_viewCount = 1;	// so cleared structures never match viewCount
			// we used to memset tr, but now that it is a class, we can't, so
			// there may be other state we need to reset

			_ambientLightVector = new Vector4(0.5f, 0.5f - 0.385f, 0.8925f, 1.0f);

			InitCommands();

			_guiModel = new idGuiModel();
			_demoGuiModel = new idGuiModel();

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

			// determine which back end we will use
			// ??? this is invalid here as there is not enough information to set it up correctly
			SetBackEndRenderer();

			idConsole.WriteLine("renderSystem initialized.");
			idConsole.WriteLine("--------------------------------------");
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
		#endregion

		#region Private
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

		public void InitGraphics()
		{
			// if OpenGL isn't started, start it now
			if(idE.GLConfig.IsInitialized == true)
			{
				return;
			}

			InitOpenGL();
			// TODO: idE.ImageManager.ReloadImages();

			int error = Gl.glGetError();

			if(error != Gl.GL_NO_ERROR)
			{
				idConsole.WriteLine("glGetError() = 0x{0:X}", error);
			}
		}

		private void InitOpenGL()
		{
			idConsole.WriteLine("----- R_InitOpenGL -----");

			if(idE.GLConfig.IsInitialized == true)
			{
				idConsole.FatalError("R_InitOpenGL called while active");
			}

			// in case we had an error while doing a tiled rendering
			_viewPortOffset[0] = 0;
			_viewPortOffset[1] = 0;

			//
			// initialize OS specific portions of the renderSystem
			//
			for(int i = 0; i < 2; i++)
			{
				// set the parameters we are trying
				GetModeInfo(ref idE.GLConfig.VideoWidth, ref idE.GLConfig.VideoHeight, idE.CvarSystem.GetInteger("r_mode"));

				if(InitOpenGLContext(
					idE.GLConfig.VideoWidth,
					idE.GLConfig.VideoHeight,
					idE.CvarSystem.GetBool("r_fullscreen"),
					idE.CvarSystem.GetInteger("r_displayRefresh"),
					idE.CvarSystem.GetInteger("r_multiSamples"),
					false) == true)
				{
					break;
				}

				if(i == 1)
				{
					idConsole.FatalError("Unable to initialize OpenGL");
				}

				// if we failed, set everything back to "safe mode" and try again
				idE.CvarSystem.SetInteger("r_mode", 3);
				idE.CvarSystem.SetInteger("r_fullscreen", 0);
				idE.CvarSystem.SetInteger("r_displayRefresh", 0);
				idE.CvarSystem.SetInteger("r_multiSamples", 0);
			}

			// input and sound systems need to be tied to the new window
			// TODO: Sys_InitInput();
			// TODO: soundSystem->InitHW();

			// get our config strings
			idE.GLConfig.Vendor = Gl.glGetString(Gl.GL_VENDOR);
			idE.GLConfig.Renderer = Gl.glGetString(Gl.GL_RENDERER);
			idE.GLConfig.Version = Gl.glGetString(Gl.GL_VERSION);
			idE.GLConfig.Extensions = Gl.glGetString(Gl.GL_EXTENSIONS);

			// OpenGL driver constants
			int temp;
			Gl.glGetIntegerv(Gl.GL_MAX_TEXTURE_SIZE, out temp);

			idE.GLConfig.MaxTextureSize = temp;

			// stubbed or broken drivers may have reported 0...
			if(idE.GLConfig.MaxTextureSize <= 0)
			{
				idE.GLConfig.MaxTextureSize = 256;
			}

			idE.GLConfig.IsInitialized = true;

			// recheck all the extensions (FIXME: this might be dangerous)
			CheckPortableExtensions();

			// parse our vertex and fragment programs, possibly disably support for
			// one of the paths if there was an error
			InitNV10();
			InitNV20();
			InitR200();
			InitARB2();

			// TODO: cmdSystem->AddCommand( "reloadARBprograms", R_ReloadARBPrograms_f, CMD_FL_RENDERER, "reloads ARB programs" );

			// TODO: R_ReloadARBPrograms_f( idCmdArgs() );

			// allocate the vertex array range or vertex objects
			// TODO: vertexCache.Init();*/

			// select which renderSystem we are going to use
			idE.CvarSystem.IsModified("r_renderer");

			// allocate the frame data, which may be more if smp is enabled
			// TODO: R_InitFrameData();

			// Reset our gamma
			SetColorMappings();

			// TODO: ogl error
			/*#ifdef _WIN32
				static bool glCheck = false;
				if ( !glCheck && win32.osversion.dwMajorVersion == 6 ) {
					glCheck = true;
					if ( !idStr::Icmp( glConfig.vendor_string, "Microsoft" ) && idStr::FindText( glConfig.renderer_string, "OpenGL-D3D" ) != -1 ) {
						if ( cvarSystem->GetCVarBool( "r_fullscreen" ) ) {
							cmdSystem->BufferCommandText( CMD_EXEC_NOW, "vid_restart partial windowed\n" );
							Sys_GrabMouseCursor( false );
						}
						int ret = MessageBox( NULL, "Please install OpenGL drivers from your graphics hardware vendor to run " GAME_NAME ".\nYour OpenGL functionality is limited.",
							"Insufficient OpenGL capabilities", MB_OKCANCEL | MB_ICONWARNING | MB_TASKMODAL );
						if ( ret == IDCANCEL ) {
							cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "quit\n" );
							cmdSystem->ExecuteCommandBuffer();
						}
						if ( cvarSystem->GetCVarBool( "r_fullscreen" ) ) {
							cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "vid_restart\n" );
						}
					}
				}
			#endif*/
		}

		private bool InitOpenGLContext(int width, int height, bool fullscreen, int refreshRate, int multiSamples, bool stereoMode)
		{
			idConsole.WriteLine("Initializing OpenGL subsystem");

			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemConsole));

			_renderForm = new Form();
			_renderForm.Text = "DOOM 3";
			_renderForm.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
			_renderForm.Width = width;
			_renderForm.Height = height;
			_renderForm.FormClosed += delegate(object sender, FormClosedEventArgs e) { idE.System.Quit(); };

			idConsole.WriteLine("...created window @ {0},{1} ({2},{3})", _renderForm.Location.X, _renderForm.Location.Y, _renderForm.Width, _renderForm.Height);
			idConsole.WriteLine("...creating GL context");

			_renderControl = new SimpleOpenGlControl();
			_renderControl.AutoSize = true;
			_renderControl.Dock = DockStyle.Fill;
			_renderControl.ColorBits = 32;
			_renderControl.DepthBits = 24;
			_renderControl.StencilBits = 8;
			_renderControl.AccumBits = 0;

			_renderForm.Controls.Add(_renderControl);

			_renderControl.InitializeContexts();
			_renderForm.Show();

			idConsole.WriteLine("...making context current");

			_renderControl.MakeCurrent();

			idE.GLConfig.IsFullscreen = fullscreen;

			// TODO
			/* = GetDC( GetDesktopWindow() );
			win32.desktopBitsPixel = GetDeviceCaps( hDC, BITSPIXEL );
			win32.desktopWidth = GetDeviceCaps( hDC, HORZRES );
			win32.desktopHeight = GetDeviceCaps( hDC, VERTRES );
			ReleaseDC( GetDesktopWindow(), hDC );

			// we can't run in a window unless it is 32 bpp
			if ( win32.desktopBitsPixel < 32 && !parms.fullScreen ) {
				common->Printf("^3Windowed mode requires 32 bit desktop depth^0\n");
				return false;
			}*/

			// save the hardware gamma so it can be
			// restored on exit
			// TODO: GLimp_SaveGamma();

			// create our window classes if we haven't already
			/*GLW_CreateWindowClasses();

			// this will load the dll and set all our Gl.* function pointers,
			// but doesn't create a window

			// r_glDriver is only intended for using instrumented OpenGL
			// dlls.  Normal users should never have to use it, and it is
			// not archived.
			driverName = r_glDriver.GetString()[0] ? r_glDriver.GetString() : "opengl32";
			if ( !Gl._Init( driverName ) ) {
				common->Printf( "^3GLimp_Init() could not load r_glDriver \"%s\"^0\n", driverName );
				return false;
			}

			// getting the wgl extensions involves creating a fake window to get a context,
			// which is pretty disgusting, and seems to mess with the AGP VAR allocation
			GLW_GetWGLExtensionsWithFakeWindow();*/

			// try to change to fullscreen
			/* TODO: if ( parms.fullScreen ) {
				if ( !GLW_SetFullScreen( parms ) ) {
					GLimp_Shutdown();
					return false;
				}
			}

			// try to create a window with the correct pixel format
			// and init the renderer context
			if ( !GLW_CreateWindow( parms ) ) {
				GLimp_Shutdown();
				return false;
			}

			// wglSwapinterval, etc
			GLW_CheckWGLExtensions( win32.hDC );

			// check logging
			GLimp_EnableLogging( ( r_logFile.GetInteger() != 0 ) );*/

			return true;
		}

		private void InitNV10()
		{
			idE.GLConfig.AllowNV10Path = idE.GLConfig.RegisterCombinersAvailable;
		}

		private void GL_CheckErrors()
		{
			List<string> errors = new List<string>();

			// check for up to 10 errors pending
			for(int i = 0; i < 10; i++)
			{
				int error = Gl.glGetError();

				if(error == Gl.GL_NO_ERROR)
				{
					return;
				}

				switch(error)
				{
					case Gl.GL_INVALID_ENUM:
						errors.Add("GL_INVALID_ENUM");
						break;

					case Gl.GL_INVALID_VALUE:
						errors.Add("GL_INVALID_VALUE");
						break;

					case Gl.GL_INVALID_OPERATION:
						errors.Add("GL_INVALID_OPERATION");
						break;

					case Gl.GL_STACK_OVERFLOW:
						errors.Add("GL_STACK_OVERFLOW");
						break;

					case Gl.GL_STACK_UNDERFLOW:
						errors.Add("GL_STACK_UNDERFLOW");
						break;

					case Gl.GL_OUT_OF_MEMORY:
						errors.Add("GL_OUT_OF_MEMORY");
						break;

					default:
						errors.Add(error.ToString("X"));
						break;
				}
			}

			if(idE.CvarSystem.GetBool("r_ignoreGLErrors") == false)
			{
				idConsole.WriteLine("GL_CheckErrors: {0}", String.Join(",", errors));
			}
		}

		private void CheckPortableExtensions()
		{
			idE.GLConfig.VersionF = new Version(idE.GLConfig.Version);
			idE.GLConfig.MultiTextureAvailable = CheckExtension("GL_ARB_multitexture");

			if(idE.GLConfig.MultiTextureAvailable == true)
			{
				Gl.glGetIntegerv(Gl.GL_MAX_TEXTURE_UNITS_ARB, out idE.GLConfig.MaxTextureUnits);

				if(idE.GLConfig.MaxTextureUnits < 2)
				{
					idE.GLConfig.MultiTextureAvailable = false; // shouldn't ever happen
				}

				Gl.glGetIntegerv(Gl.GL_MAX_TEXTURE_COORDS_ARB, out idE.GLConfig.MaxTextureCoordinates);
				Gl.glGetIntegerv(Gl.GL_MAX_TEXTURE_IMAGE_UNITS_ARB, out idE.GLConfig.MaxTextureImageUnits);
			}

			idE.GLConfig.TextureEnvCombineAvailable = CheckExtension("GL_ARB_texture_env_combine");
			idE.GLConfig.CubeMapAvailable = CheckExtension("GL_ARB_texture_cube_map");
			idE.GLConfig.EnvDot3Available = CheckExtension("GL_ARB_texture_env_dot3");
			idE.GLConfig.TextureEnvAddAvailable = CheckExtension("GL_ARB_texture_env_add");
			idE.GLConfig.TextureNonPowerOfTwoAvailable = CheckExtension("GL_ARB_texture_non_power_of_two");

			// GL_ARB_texture_compression + GL_S3_s3tc
			// DRI drivers may have GL_ARB_texture_compression but no GL_EXT_texture_compression_s3tc
			if((CheckExtension("GL_ARB_texture_compression") == true) && (CheckExtension("GL_EXT_texture_compression_s3tc") == true))
			{
				idE.GLConfig.TextureCompressionAvailable = true;
			}
			else
			{
				idE.GLConfig.TextureCompressionAvailable = false;
			}

			idE.GLConfig.AnisotropicAvailable = CheckExtension("GL_EXT_texture_filter_anisotropic");

			if(idE.GLConfig.AnisotropicAvailable == true)
			{
				Gl.glGetFloatv(Gl.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, out idE.GLConfig.MaxTextureAnisotropy);

				idConsole.WriteLine("   maxTextureAnisotropy: {0}", idE.GLConfig.MaxTextureAnisotropy);
			}
			else
			{
				idE.GLConfig.MaxTextureAnisotropy = 1;
			}

			// GL_EXT_texture_lod_bias
			// The actual extension is broken as specificed, storing the state in the texture unit instead
			// of the texture object.  The behavior in GL 1.4 is the behavior we use.
			if((((idE.GLConfig.VersionF.Major <= 1) && (idE.GLConfig.VersionF.Minor < 4)) == false) || (CheckExtension("GL_EXT_texture_lod") == true))
			{
				idConsole.WriteLine("...using {0}", "GL_1.4_texture_lod_bias");
				idE.GLConfig.TextureLodBiasAvailable = true;
			}
			else
			{
				idConsole.WriteLine("X..{0} not found\n", "GL_1.4_texture_lod_bias");
				idE.GLConfig.TextureLodBiasAvailable = false;
			}

			// GL_EXT_shared_texture_palette
			idE.GLConfig.SharedTexturePaletteAvailable = CheckExtension("GL_EXT_shared_texture_palette");

			// GL_EXT_texture3D (not currently used for anything)
			idE.GLConfig.Texture3DAvailable = CheckExtension("GL_EXT_texture3D");

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
				_stencilIncrement = Gl.GL_INCR;
				_stencilDecrement = Gl.GL_DECR;
			}

			idE.GLConfig.RegisterCombinersAvailable = CheckExtension("GL_NV_register_combiners");
			idE.GLConfig.TwoSidedStencilAvailable = CheckExtension("GL_EXT_stencil_two_side");

			if(idE.GLConfig.TwoSidedStencilAvailable == false)
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
				// TODO: idConsole.Error(common->GetLanguageDict()->GetString( "#str_06780" ) );
			}

			idE.GLConfig.DepthBoundsTestAvailable = CheckExtension("EXT_depth_bounds_test");
		}

		private bool CheckExtension(string name)
		{
			if(idE.GLConfig.Extensions.Contains(name) == true)
			{
				idConsole.WriteLine("...using {0}", name);
				return true;
			}

			idConsole.WriteLine("X..{0} not found", name);

			return false;
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

		private void Clear()
		{
			_registered = false;
			_frameCount = 0;
			_viewCount = 0;

			// TODO
			/*staticAllocCount = 0;
			frameShaderTime = 0.0f;*/
			_viewPortOffset[0] = 0;
			_viewPortOffset[1] = 0;
			_tiledViewPort[0] = 0;
			_tiledViewPort[1] = 0;

			_backEndRenderer = BackEndRenderer.Bad;
			_backEndRendererHasVertexPrograms = false;
			_backEndRendererMaxLight = 1.0f;

			_ambientLightVector = Vector4.Zero;

			// TODO
			/*sortOffset = 0;*/
			_worlds.Clear();

			/*primaryWorld = NULL;
			memset( &primaryRenderView, 0, sizeof( primaryRenderView ) );
			primaryView = NULL;
			defaultMaterial = NULL;
			testImage = NULL;
			ambientCubeImage = NULL;
			viewDef = NULL;
			memset( &pc, 0, sizeof( pc ) );
			memset( &lockSurfacesCmd, 0, sizeof( lockSurfacesCmd ) );*/

			_identitySpace = new ViewEntity();

			/*logFile = NULL;
			stencilIncr = 0;
			stencilDecr = 0;
			memset( renderCrops, 0, sizeof( renderCrops ) );
			currentRenderCrop = 0;
			guiRecursionLevel = 0;
			guiModel = NULL;
			demoGuiModel = NULL;
			memset( gammaTable, 0, sizeof( gammaTable ) );
			takingScreenshot = false;*/
		}

		private void InitCommands()
		{
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

			new idCvar("r_ignoreGLErrors", "1", "ignore GL errors", CvarFlags.ReadOnly | CvarFlags.Bool);
			new idCvar("r_finish", "0", "force a call to glFinish() every frame", CvarFlags.ReadOnly | CvarFlags.Bool);
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

			// TODO: SetDeviceGammaRamp
			/*if ( !SetDeviceGammaRamp( win32.hDC, table ) ) {
				common->Printf( "WARNING: SetDeviceGammaRamp failed.\n" );
			}*/
		}
		#endregion

		#region NV20
		private void InitNV20()
		{
			idE.GLConfig.AllowNV20Path = false;

			GL_CheckErrors();
			idConsole.WriteLine("---------- R_NV20_Init ----------");

			if((idE.GLConfig.RegisterCombinersAvailable == false) || (idE.GLConfig.ArbVertexProgramAvailable == false) || (idE.GLConfig.MaxTextureUnits < 4))
			{
				idConsole.WriteLine("Not available.");
			}
			else
			{
				// create our "fragment program" display lists
				_fragmentDisplayListBase = Gl.glGenLists((int) FragmentProgram.Count);

				// force them to issue commands to build the list
				bool temp = idE.CvarSystem.GetBool("r_useCombinerDisplayLists");
				idE.CvarSystem.SetBool("r_useCombinerDisplayLists", false);

				Gl.glNewList(_fragmentDisplayListBase + (int) FragmentProgram.BumpAndLight, Gl.GL_COMPILE);
				NV20_BumpAndLightFragment();
				Gl.glEndList();

				Gl.glNewList(_fragmentDisplayListBase + (int) FragmentProgram.DiffuseColor, Gl.GL_COMPILE);
				NV20_DiffuseColorFragment();
				Gl.glEndList();

				Gl.glNewList(_fragmentDisplayListBase + (int) FragmentProgram.SpecularColor, Gl.GL_COMPILE);
				NV20_SpecularColorFragment();
				Gl.glEndList();

				Gl.glNewList(_fragmentDisplayListBase + (int) FragmentProgram.DiffuseAndSpecularColor, Gl.GL_COMPILE);
				NV20_DiffuseAndSpecularColorFragment();
				Gl.glEndList();

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

			GL_CheckErrors();
			idConsole.WriteLine("----------- R200_Init -----------");

			if((idE.GLConfig.AtiFragmentShaderAvailable == false) || (idE.GLConfig.ArbFragmentProgramAvailable == false) || (idE.GLConfig.ArbVertexBufferObjectAvailable == false))
			{
				idConsole.WriteLine("Not available.");
			}
			else
			{
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

			GL_CheckErrors();

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

		/*const viewDef_t	*	viewDef;
		backEndCounters_t	pc;*/

		/*const viewEntity_t *currentSpace;		// for detecting when a matrix must change
		idScreenRect		currentScissor;*/
		// for scissor clipping, local inside renderView viewport

		/*viewLight_t *		vLight;
		int					depthFunc;			// GLS_DEPTHFUNC_EQUAL, or GLS_DEPTHFUNC_LESS for translucent
		float				lightTextureMatrix[16];	// only if lightStage->texture.hasMatrix
		float				lightColor[4];		// evaluation of current light's color stage

		float				lightScale;			// Every light color calaculation will be multiplied by this,
												// which will guarantee that the result is < tr.backEndRendererMaxLight
												// A card with high dynamic range will have this set to 1.0
		float				overBright;			// The amount that all light interactions must be multiplied by
												// with post processing to get the desired total light level.
												// A high dynamic range card will have this set to 1.0.

		bool				currentRenderCopied;	// true if any material has already referenced _currentRender*/

		// our OpenGL state deltas.
		public GLState GLState = new GLState();

		//int					c_copyFrameBuffer;*/
	}

	internal class GLState
	{
		public TextureUnit[] TextureUnits = new TextureUnit[8];
		public int CurrentTextureUnit;

		public int FaceCulling;
		public int StateBits;
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
		public string WGLExtensions;

		public float VersionNumber;

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

		public bool IsInitialized;
	}

	internal class TextureUnit
	{
		public int Current2DMap;
		public int Current3DMap;
		public int CurrentCubeMap;
		public int TexEnv;

		public TextureType Type;
	}

	internal struct View
	{
		public RenderView RenderView;
		/*// viewDefs are allocated on the frame temporary stack memory
	typedef struct viewDef_s {
		// specified in the call to DrawScene()
		renderView_t		renderView;

		float				projectionMatrix[16];
		viewEntity_t		worldSpace;

		idRenderWorldLocal *renderWorld;*/

		public float FloatTime;

		/*
		idVec3				initialViewAreaOrigin;
		// Used to find the portalArea that view flooding will take place from.
		// for a normal view, the initialViewOrigin will be renderView.viewOrg,
		// but a mirror may put the projection origin outside
		// of any valid area, or in an unconnected area of the map, so the view
		// area must be based on a point just off the surface of the mirror / subview.
		// It may be possible to get a failed portal pass if the plane of the
		// mirror intersects a portal, and the initialViewAreaOrigin is on
		// a different side than the renderView.viewOrg is.

		bool				isSubview;				// true if this view is not the main view
		bool				isMirror;				// the portal is a mirror, invert the face culling
		bool				isXraySubview;

		bool				isEditor;

		int					numClipPlanes;			// mirrors will often use a single clip plane
		idPlane				clipPlanes[MAX_CLIP_PLANES];		// in world space, the positive side
													// of the plane is the visible side
		idScreenRect		viewport;				// in real pixels and proper Y flip

		idScreenRect		scissor;
		// for scissor clipping, local inside renderView viewport
		// subviews may only be rendering part of the main view
		// these are real physical pixel values, possibly scaled and offset from the
		// renderView x/y/width/height

		struct viewDef_s *	superView;				// never go into an infinite subview loop 
		struct drawSurf_s *	subviewSurface;

		// drawSurfs are the visible surfaces of the viewEntities, sorted
		// by the material sort parameter
		drawSurf_t **		drawSurfs;				// we don't use an idList for this, because
		int					numDrawSurfs;			// it is allocated in frame temporary memory
		int					maxDrawSurfs;			// may be resized

		struct viewLight_s	*viewLights;			// chain of all viewLights effecting view
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

	} viewDef_t;*/
	}

	public struct RenderView
	{
		/*typedef struct renderView_s {
		// player views will set this to a non-zero integer for model suppress / allow
		// subviews (mirrors, cameras, etc) will always clear it to zero
		int						viewID;

		// sized from 0 to SCREEN_WIDTH / SCREEN_HEIGHT (640/480), not actual resolution
		int						x, y, width, height;

		float					fov_x, fov_y;
		idVec3					vieworg;
		idMat3					viewaxis;			// transformation matrix, view looks down the positive X axis

		bool					cramZNear;			// for cinematics, we want to set ZNear much lower
		bool					forceUpdate;		// for an update 

		// time in milliseconds for shader effects and other time dependent rendering issues
		int						time;*/

		/// <summary>Can be used in any way by the shader.</summary>
		public float[] ShaderParameters;

		/// <summary>Used to override everything draw.</summary>
		public idMaterial GlobalMaterial;
	}

	public struct ViewEntity
	{
		/*struct viewEntity_s	*next;

	// back end should NOT reference the entityDef, because it can change when running SMP
	idRenderEntityLocal	*entityDef;

	// for scissor clipping, local inside renderView viewport
	// scissorRect.Empty() is true if the viewEntity_t was never actually
	// seen through any portals, but was created for shadow casting.
	// a viewEntity can have a non-empty scissorRect, meaning that an area
	// that it is in is visible, and still not be visible.
	idScreenRect		scissorRect;

	bool				weaponDepthHack;
	float				modelDepthHack;*/
		public Matrix ModelMatrix; // local coords to global coords
		public Matrix ModelViewMatrix; // local coords to eye coords
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
}