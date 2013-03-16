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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using idTech4.Services;

namespace idTech4.Renderer
{
	/// <summary>
	/// Responsible for managing the screen, which can have multiple idRenderWorld and 2D drawing done on it.
	/// </summary>
	public sealed class idRenderSystem : IRenderSystem
	{
		#region Members
		private bool _initialized;
		private IRenderBackend _backend;

		private idFrameData _frameData;
		private idFrameData[] _smpFrameData = new idFrameData[2];

		private idMaterial _whiteMaterial;
		private idMaterial _charSetMaterial;
		private idMaterial _defaultPointLight;
		private idMaterial _defaultProjectedLight;
		private idMaterial _defaultMaterial;
		
		private int _frameCount;				// incremented every frame
		private int _smpFrame;
		private int _viewCount;					// incremented every view (twice a scene if subviewed)
												// and every R_MarkFragments call

		private float _frameShaderTime;			// shader time for all non-world 2D rendering

		private Vector4 _ambientLightVector;	// used for "ambient bump mapping"

		private idViewEntity _identitySpace;		// can use if we don't know viewDef->worldSpace is valid
		private idViewDefinition _viewDefinition;

		private idScreenRect[] _renderCrops = new idScreenRect[Constants.MaxRenderCrops];
		private int _currentRenderCrop;

		// GUI drawing variables for surface creation
		private int _guiRecursionLevel;		// to prevent infinite overruns
		private idGuiModel _guiModel;
		private Color _currentColor;

		private ushort[] _quadIndexes = { 3, 0, 2, 2, 0, 1 };
		#endregion

		#region Constructor
		public idRenderSystem()
		{
					
		}
		#endregion

		#region Initialization
		#region Properties
		/// <summary>
		/// Has the graphics device been initialized yet?
		/// </summary>
		public bool IsInitialized
		{
			get
			{
				return _initialized;
			}
		}
		#endregion

		#region Methods
		private IRenderBackend FindBackend()
		{
			return idEngine.Instance.GetService<IPlatformService>().CreateRenderBackend();
		}

		private void Init()
		{
			idLog.WriteLine("------- Initializing renderSystem --------");

			// clear all our internal state
			_viewCount = 1;		// so cleared structures never match viewCount
			// we used to memset tr, but now that it is a class, we can't, so
			// there may be other state we need to reset

			_ambientLightVector = new Vector4(0.5f, 0.5f - 0.385f, 0.8925f, 1.0f);			
			
			idImageManager imageManager = new idImageManager();

			idEngine.Instance.Services.AddService(typeof(IImageManager), imageManager);

			imageManager.Init();

			idLog.Warning("TODO: idCinematic::InitCinematic( );");

			// build brightness translation tables
			idLog.Warning("TODO: R_SetColorMappings();");

			// allocate the frame data, which may be more if smp is enabled
			InitFrameData();
			InitMaterials();

			idLog.Warning("TODO: renderModelManager->Init();");

			// set the identity space
			_identitySpace = new idViewEntity();
			_guiModel      = new idGuiModel();

			// make sure the tr.unitSquareTriangles data is current in the vertex / index cache
			idLog.Warning("TODO: if ( unitSquareTriangles == NULL ) "); //{ unitSquareTriangles = R_MakeFullScreenTris(); }");

			// make sure the tr.zeroOneCubeTriangles data is current in the vertex / index cache
			idLog.Warning("TODO: if ( zeroOneCubeTriangles == NULL ) "); //{ zeroOneCubeTriangles = R_MakeZeroOneCubeTris(); }");

			// make sure the tr.testImageTriangles data is current in the vertex / index cache
			idLog.Warning("TODO: if ( testImageTriangles == NULL ) "); // { testImageTriangles = R_MakeTestImageTriangles(); }");

			idLog.Warning("TODO: frontEndJobList = parallelJobManager->AllocJobList( JOBLIST_RENDERER_FRONTEND, JOBLIST_PRIORITY_MEDIUM, 2048, 0, null);");

			// make sure the command buffers are ready to accept the first screen update
			SwapCommandBuffers();

			idLog.WriteLine("renderSystem initialized.");
			idLog.WriteLine("--------------------------------------");
		}

		private void InitBackend()
		{
			_initialized = true;

			_backend = FindBackend();
			_backend.Init();
		}

		private void InitFrameData()
		{
			idLog.Warning("TODO: R_ShutdownFrameData();");

			for(int i = 0; i < _smpFrameData.Length; i++)
			{
				_smpFrameData[i] = new idFrameData();
			}
			
			// must be set before calling R_ToggleSmpFrame()
			_frameData = _smpFrameData[0];

			ToggleSmpFrame();
		}

		private void InitMaterials()
		{
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

			_defaultMaterial = declManager.FindMaterial("_default", false);

			if(_defaultMaterial == null)
			{
				idEngine.Instance.FatalError("_default material not found");
			}
						
			_defaultPointLight     = declManager.FindMaterial("lights/defaultPointLight");
			_defaultProjectedLight = declManager.FindMaterial("lights/defaultProjectedLight");
			_whiteMaterial         = declManager.FindMaterial("_white");
			_charSetMaterial       = declManager.FindMaterial("textures/bigchars");
		}
		#endregion
		#endregion

		#region State Management
		private void Clear()
		{
			idLog.WriteLine("TODO: clear");

			//registered     = false;
			_frameCount      = 0;
			_viewCount       = 0;
			_frameShaderTime = 0.0f;
			/*ambientLightVector.Zero();
			worlds.Clear();
			primaryWorld = NULL;
			memset(&primaryRenderView, 0, sizeof(primaryRenderView));
			primaryView = NULL*;*/
			_defaultMaterial = null;
			/*testImage = NULL;
			ambientCubeImage = NULL;*/
			_viewDefinition = null;
			/*memset(&pc, 0, sizeof(pc));*/

			_identitySpace     = new idViewEntity();
			_currentRenderCrop = 0;
			_currentColor      = Color.White;
			_guiRecursionLevel = 0;
			_guiModel          = null;

			/*
			memset(gammaTable, 0, sizeof(gammaTable));
			takingScreenshot = false;

			if(unitSquareTriangles != NULL)
			{
				Mem_Free(unitSquareTriangles);
				unitSquareTriangles = NULL;
			}

			if(zeroOneCubeTriangles != NULL)
			{
				Mem_Free(zeroOneCubeTriangles);
				zeroOneCubeTriangles = NULL;
			}

			if(testImageTriangles != NULL)
			{
				Mem_Free(testImageTriangles);
				testImageTriangles = NULL;
			}*/

			//_frontEndJobList = null;
		}
		#endregion

		#region Command Buffer
		/// <summary>
		/// Performs final closeout of any gui models being defined.
		/// </summary>
		/// <returns>The head of the linked command list that was just closed off.</returns>
		/// <remarks>
		/// Returns timing information from the previous frame.
		/// <para/>
		/// After this is called, new command buffers can be built up in parallel
		/// with the rendering of the closed off command buffers by RenderCommandBuffers()
		/// </remarks>
		public LinkedListNode<idRenderCommand> SwapCommandBuffers()
		{
			ulong frontEnd;
			ulong backEnd;
			ulong shadow;
			ulong gpu;

			SwapCommandBuffers_FinishRendering(out frontEnd, out backEnd, out shadow, out gpu);

			return SwapCommandBuffers_FinishCommandBuffers();
		}

		/// <summary>
		/// Performs final closeout of any gui models being defined.
		/// </summary>
		/// <returns>The head of the linked command list that was just closed off.</returns>
		/// <remarks>
		/// Returns timing information from the previous frame.
		/// <para/>
		/// After this is called, new command buffers can be built up in parallel
		/// with the rendering of the closed off command buffers by RenderCommandBuffers()
		/// </remarks>
		public LinkedListNode<idRenderCommand> SwapCommandBuffers(out ulong frontEnd, out ulong backEnd, out ulong shadow, out ulong gpu)
		{
			SwapCommandBuffers_FinishRendering(out frontEnd, out backEnd, out shadow, out gpu);

			return SwapCommandBuffers_FinishCommandBuffers();
		}

		public void SwapCommandBuffers_FinishRendering(out ulong frontEnd, out ulong backEnd, out ulong shadow, out ulong gpu)
		{
			// TODO: SCOPED_PROFILE_EVENT( "SwapCommandBuffers" );

			gpu      = 0;		// until shown otherwise
			frontEnd = 0;
			backEnd  = 0;
			shadow   = 0;
		
			if(this.IsInitialized == false)
			{
				return;
			}

			// after coming back from an autoswap, we won't have anything to render
			if(_frameData.First.Next != null)
			{
				// wait for our fence to hit, which means the swap has actually happened
				// We must do this before clearing any resources the GPU may be using
				_backend.BlockingSwapBuffers();
			}

			// read back the start and end timer queries from the previous frame
			// TODO: timer information
			/*if ( glConfig.timerQueryAvailable ) {
				uint64 drawingTimeNanoseconds = 0;
				if ( tr.timerQueryId != 0 ) {
					qglGetQueryObjectui64vEXT( tr.timerQueryId, GL_QUERY_RESULT, &drawingTimeNanoseconds );
				}
				if ( gpuMicroSec != NULL ) {
					*gpuMicroSec = drawingTimeNanoseconds / 1000;
				}
			}*/

			//------------------------------

			// save out timing information
			// TODO: timing information
			frontEnd = 0;
			backEnd  = 0;
			shadow   = 0;

			/**frontEndMicroSec = pc.frontEndMicroSec;
			*backEndMicroSec = backEnd.pc.totalMicroSec;
			*shadowMicroSec = backEnd.pc.shadowMicroSec;*/	

			// print any other statistics and clear all of them
			// TODO: R_PerformanceCounters();

			// check for dynamic changes that require some initialization
			// TODO: R_CheckCvars();

			// check for errors
			// TODO: GL_CheckErrors();
		}

		public LinkedListNode<idRenderCommand> SwapCommandBuffers_FinishCommandBuffers() 
		{
			if(this.IsInitialized == false)
			{
				return null;
			}

			// close any gui drawing
			_guiModel.EmitFullscreen();
			_guiModel.Clear();

			// unmap the buffer objects so they can be used by the GPU
			idLog.Warning("vertexCache.BeginBackEnd();");

			// save off this command buffer	
			LinkedListNode<idRenderCommand> commandBufferHead = _frameData.First;

			// copy the code-used drawsurfs that were
			// allocated at the start of the buffer memory to the backEnd referenced locations
			idLog.Warning("backEnd.unitSquareSurface = tr.unitSquareSurface_;");
			idLog.Warning("backEnd.zeroOneCubeSurface = tr.zeroOneCubeSurface_;");
			idLog.Warning("backEnd.testImageSurface = tr.testImageSurface_;");

			// use the other buffers next frame, because another CPU
			// may still be rendering into the current buffers
			ToggleSmpFrame();

			// possibly change the stereo3D mode
			// PC
			// TODO: stereo mode
			/*if ( glConfig.nativeScreenWidth == 1280 && glConfig.nativeScreenHeight == 1470 ) {
				glConfig.stereo3Dmode = STEREO3D_HDMI_720;
			} else {
				glConfig.stereo3Dmode = GetStereoScopicRenderingMode();
			}*/

			// prepare the new command buffer
			_guiModel.BeginFrame();

			//------------------------------
			// Make sure that geometry used by code is present in the buffer cache.
			// These use frame buffer cache (not static) because they may be used during
			// map loads.
			//
			// It is important to do this first, so if the buffers overflow during
			// scene generation, the basic surfaces needed for drawing the buffers will
			// always be present.
			//------------------------------
			idLog.Warning("R_InitDrawSurfFromTri( tr.unitSquareSurface_, *tr.unitSquareTriangles );");
			idLog.Warning("R_InitDrawSurfFromTri( tr.zeroOneCubeSurface_, *tr.zeroOneCubeTriangles );");
			idLog.Warning("R_InitDrawSurfFromTri( tr.testImageSurface_, *tr.testImageTriangles );");

			// reset render crop to be the full screen
			// TODO: important! render crop
			_renderCrops[0].X1 = 0;
			_renderCrops[0].Y1 = 0;
			_renderCrops[0].X2 = (short) (this.Width - 1);
			_renderCrops[0].Y2 = (short) (this.Height - 1);
			
			_currentRenderCrop = 0;

			// this is the ONLY place this is modified
			_frameCount++;

			// just in case we did a common->Error while this was set
			_guiRecursionLevel = 0;

			// the first rendering will be used for commands like
			// screenshot, rather than a possible subsequent remote
			// or mirror render
			//	primaryWorld = NULL;

			// set the time for shader effects in 2D rendering
			_frameShaderTime = idEngine.Instance.ElapsedTime * 0.001f;

			_frameData.AddLast(new idSetBufferRenderCommand(RenderBuffer.Back));

			// the old command buffer can now be rendered, while the new one can be built in parallel
			return commandBufferHead;
		}

		private void ToggleSmpFrame() 
		{
			// TODO: smp highwater mark
			// update the highwater mark
			/*if ( frameData->frameMemoryAllocated.GetValue() > frameData->highWaterAllocated ) {
				frameData->highWaterAllocated = frameData->frameMemoryAllocated.GetValue();
		#if defined( TRACK_FRAME_ALLOCS )
				frameData->highWaterUsed = frameData->frameMemoryUsed.GetValue();
				for ( int i = 0; i < FRAME_ALLOC_MAX; i++ ) {
					frameHighWaterTypeCount[i] = frameAllocTypeCount[i].GetValue();
				}
		#endif
			}*/

			// switch to the next frame
			_smpFrame++;
			_frameData = _smpFrameData[_smpFrame % _smpFrameData.Length];

			// reset the memory allocation
			// TODO: reset frame memory allocation

			/*const unsigned int bytesNeededForAlignment = FRAME_ALLOC_ALIGNMENT - ( (unsigned int)frameData->frameMemory & ( FRAME_ALLOC_ALIGNMENT - 1 ) );
			frameData->frameMemoryAllocated.SetValue( bytesNeededForAlignment );
			frameData->frameMemoryUsed.SetValue( 0 );

		#if defined( TRACK_FRAME_ALLOCS )
			for ( int i = 0; i < FRAME_ALLOC_MAX; i++ ) {
				frameAllocTypeCount[i].SetValue( 0 );
			}
		#endif*/

			// clear the command chain and make a RC_NOP command the only thing on the list
			_frameData.Clear();
			_frameData.AddLast(new idEmptyRenderCommand());
		}
		#endregion

		#region IRenderSystem implementation
		#region Initialization
		public void Initialize()
		{
			if(this.IsInitialized == true)
			{
				throw new Exception("idRenderSystem has already been initialized.");
			}

			Clear();

			InitBackend();
			Init();
		}
		#endregion

		#region Other
		#region Methods
		public void BeginLevelLoad()
		{
			IImageManager imageManager = idEngine.Instance.GetService<IImageManager>();

			imageManager.BeginLevelLoad();
			// TODO: renderModelManager->BeginLevelLoad();

			// re-Initialize the Default Materials if needed. 
			InitMaterials();
		}

		public void EndLevelLoad()
		{
			IImageManager imageManager = idEngine.Instance.GetService<IImageManager>();

			// TODO: renderModelManager->EndLevelLoad();
			imageManager.EndLevelLoad();
		}

		/// <summary>
		/// Returns the current cropped pixel coordinates.
		/// </summary>
		public idScreenRect GetCroppedViewport()
		{
			return _renderCrops[_currentRenderCrop];
		}
		#endregion
		#endregion

		#region Rendering
		#region Properties
		public Color Color
		{
			get
			{
				return _currentColor;
			}
			set
			{
				_currentColor = value;
			}
		}

		public int FrameCount
		{
			get
			{
				return _frameCount;
			}
		}

		public int Width
		{
			get
			{
				// TODO: sterep
				/*if ( glConfig.stereo3Dmode == STEREO3D_SIDE_BY_SIDE || glConfig.stereo3Dmode == STEREO3D_SIDE_BY_SIDE_COMPRESSED ) {
					return glConfig.nativeScreenWidth >> 1;
				}*/

				return _backend.Capabilities.NativeScreenWidth;
			}
		}

		public int Height
		{
			get
			{
				// TODO: stereo
				/*if ( glConfig.stereo3Dmode == STEREO3D_HDMI_720 ) {
					return 720;
				}
				extern idCVar stereoRender_warp;
				if ( glConfig.stereo3Dmode == STEREO3D_SIDE_BY_SIDE && stereoRender_warp.GetBool() ) {
					// for the Rift, render a square aspect view that will be symetric for the optics
					return glConfig.nativeScreenWidth >> 1;
				}
				if ( glConfig.stereo3Dmode == STEREO3D_INTERLACED || glConfig.stereo3Dmode == STEREO3D_TOP_AND_BOTTOM_COMPRESSED ) {
					return glConfig.nativeScreenHeight >> 1;
				}*/

				return _backend.Capabilities.NativeScreenHeight;
			}
		}

		public float PixelAspect
		{
			get
			{
				// TODO: stereo
				/*switch( glConfig.stereo3Dmode ) {
					case STEREO3D_SIDE_BY_SIDE_COMPRESSED:
						return glConfig.pixelAspect * 2.0f;
					case STEREO3D_TOP_AND_BOTTOM_COMPRESSED:
					case STEREO3D_INTERLACED:
						return glConfig.pixelAspect * 0.5f;
					default:
						
					}*/

				return _backend.Capabilities.PixelAspect;
			}
		}

		public idMaterial DefaultMaterial
		{
			get
			{
				return _defaultMaterial;
			}
		}

		public idViewDefinition ViewDefinition
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

		public idRenderCapabilities Capabilities
		{
			get
			{
				return _backend.Capabilities;
			}
		}
		#endregion

		#region Methods
		public DynamicIndexBuffer CreateDynamicIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
		{
			return _backend.CreateDynamicIndexBuffer(indexElementSize, indexCount, usage);
		}

		public DynamicVertexBuffer CreateDynamicVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
		{
			return _backend.CreateDynamicVertexBuffer(vertexDeclaration, vertexCount, usage);
		}

		public IndexBuffer CreateIndexBuffer(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
		{
			return _backend.CreateIndexBuffer(indexElementSize, indexCount, usage);
		}

		public VertexBuffer CreateVertexBuffer(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
		{
			return _backend.CreateVertexBuffer(vertexDeclaration, vertexCount, usage);
		}

		public void DrawStretchPicture(float x, float y, float width, float height, float s1, float t1, float s2, float t2, idMaterial material) 
		{
			DrawStretchPicture(new Vector4(x, y, s1, t1), new Vector4(x + width, y, s2, t1), new Vector4(x + width, y + height, s2, t2), new Vector4(x, y + height, s1, t2), material);
		}

		public void DrawStretchPicture(Vector4 topLeft, Vector4 topRight, Vector4 bottomRight, Vector4 bottomLeft, idMaterial material) 
		{
			if(material == null) 
			{
				return;
			}

			idVertex[] localVertices = new idVertex[4];

			localVertices[0]                    = new idVertex();
			localVertices[0].Clear();
			localVertices[0].ClearColor2();
			localVertices[0].Position.X         = topLeft.X;
			localVertices[0].Position.Y         = topLeft.Y;
			localVertices[0].TextureCoordinates = new HalfVector2(topLeft.Z, topLeft.W);
			localVertices[0].Color              = _currentColor;

			localVertices[1] = new idVertex();
			localVertices[1].Clear();
			localVertices[1].ClearColor2();
			localVertices[1].Position.X         = topRight.X;
			localVertices[1].Position.Y         = topRight.Y;
			localVertices[1].TextureCoordinates = new HalfVector2(topRight.Z, topRight.W);
			localVertices[1].Color              = _currentColor;
			localVertices[1].ClearColor2();

			localVertices[2] = new idVertex();
			localVertices[2].Clear();
			localVertices[2].ClearColor2();
			localVertices[2].Position.X         = bottomRight.X;
			localVertices[2].Position.Y         = bottomRight.Y;
			localVertices[2].TextureCoordinates = new HalfVector2(bottomRight.Z, bottomRight.W);
			localVertices[2].Color              = _currentColor;
			localVertices[2].ClearColor2();

			localVertices[3] = new idVertex();
			localVertices[3].Clear();
			localVertices[3].ClearColor2();
			localVertices[3].Position.X         = bottomLeft.X;
			localVertices[3].Position.Y         = bottomLeft.Y;
			localVertices[3].TextureCoordinates = new HalfVector2(bottomLeft.Z, bottomLeft.W);
			localVertices[3].Color              = _currentColor;
			localVertices[3].ClearColor2();

			_guiModel.AddPrimitive(localVertices, _quadIndexes, material, _backend.State/*, TODO: STEREO_DEPTH_TYPE_NONE*/);
		}

		/// <summary>
		/// This is the main 3D rendering command.  A single scene may have multiple views 
		/// if a mirror, portal, or dynamic texture is present.
		/// </summary>
		/// <param name="view"></param>
		/// <param name="guiParms"></param>
		public void DrawView(idViewDefinition view, bool guiOnly)
		{
			idDrawViewRenderCommand cmd;

			if(guiOnly == true)
			{
				cmd = new idDrawGuiRenderCommand();
			}
			else
			{
				cmd = new idDraw3DRenderCommand();
			}

			cmd.ViewDefinition = view;

			_frameData.AddLast(cmd);

			// TODO: tr.pc.c_numViews++;

			PrintViewStatistics(view);
		}

		private void PrintViewStatistics(idViewDefinition view)
		{
			// report statistics about this view
			if(idEngine.Instance.GetService<ICVarSystem>().GetBool("r_showSurfaces") == true)
			{
				idLog.WriteLine("view:{0:X} surfs:{1}", view.GetHashCode(), view.DrawSurfaces.Count);
			}
		}
	
		/// <summary>
		/// Issues GPU commands to render a built up list of command buffers returned
		/// by SwapCommandBuffers().
		/// </summary>
		/// <remarks>
		/// No references should be made to the current frameData, so new scenes and GUIs can be built up in parallel with the rendering.
		/// </remarks>
		/// <param name="commandBuffers"></param>
		public void RenderCommandBuffers(LinkedListNode<idRenderCommand> commandBuffers)
		{
			// if there isn't a draw view command, do nothing to avoid swapping a bad frame
			bool hasView = false;

			for(LinkedListNode<idRenderCommand> cmd = commandBuffers; cmd != null; cmd = cmd.Next)
			{
				if(cmd.Value is idDrawViewRenderCommand)
				{
					hasView = true;
					break;
				}
			}

			if(hasView == false)
			{
				return;
			}

			// r_skipBackEnd allows the entire time of the back end to be removed from performance measurements, although
			// nothing will be drawn to the screen.  If the prints are going to a file, or r_skipBackEnd is later disabled,
			// usefull data can be received.

			// r_skipRender is usually more usefull, because it will still draw 2D graphics
			if(idEngine.Instance.GetService<ICVarSystem>().GetBool("r_skipBackEnd") == false)
			{
				// TODO: timerQueryAvailable
				/*if ( glConfig.timerQueryAvailable ) {
					if ( tr.timerQueryId == 0 ) {
						qglGenQueriesARB( 1, & tr.timerQueryId );
					}
					qglBeginQueryARB( GL_TIME_ELAPSED_EXT, tr.timerQueryId );
					RB_ExecuteBackEndCommands( cmdHead );
					qglEndQueryARB( GL_TIME_ELAPSED_EXT );
					qglFlush();
				} else {*/
					_backend.ExecuteBackendCommands(commandBuffers);
				/*}*/
			}

			// pass in null for now - we may need to do some map specific hackery in the future
			idEngine.Instance.GetService<IResolutionScale>().InitForMap(null);
		}
		#endregion
		#endregion

		#region Texturing
		public Texture2D CreateTexture(int width, int height, bool mipmap = false, SurfaceFormat format = SurfaceFormat.Color)
		{
			return _backend.CreateTexture(width, height, mipmap, format);
		}
		#endregion
		#endregion
	}

	/// <summary>
	/// All of the information needed by the back end must be
	/// contained in a idFrameData.
	/// </summary>
	/// <remarks>
	/// This entire structure is
	/// duplicated so the front and back end can run in parallel
	/// on an SMP machine.
	/// </remarks>
	public class idFrameData : LinkedList<idRenderCommand>
	{

	}

	public abstract class idRenderCommand
	{
		#region Constructor
		public idRenderCommand()
		{

		}
		#endregion
	}

	public sealed class idEmptyRenderCommand : idRenderCommand
	{
		#region Constructor
		public idEmptyRenderCommand()
			: base()
		{

		}
		#endregion
	}

	public sealed class idSetBufferRenderCommand : idRenderCommand
	{
		public RenderBuffer Buffer;

		#region Constructor
		public idSetBufferRenderCommand(RenderBuffer buffer)
		{
			Buffer = buffer;
		}
		#endregion
	}

	public abstract class idDrawViewRenderCommand : idRenderCommand
	{
		public idViewDefinition ViewDefinition;
	}

	public sealed class idDrawGuiRenderCommand : idDrawViewRenderCommand
	{

	}

	public sealed class idDraw3DRenderCommand : idDrawViewRenderCommand
	{

	}

	public enum RenderBuffer
	{
		Back,
		BackLeft,
		BackRight
	}

	public class idViewDefinition
	{
		public idRenderView RenderView = new idRenderView();

		public Matrix ProjectionMatrix = Matrix.Identity;
		//idRenderMatrix projectionRenderMatrix;	// tech5 version of projectionMatrix
		public idViewEntity WorldSpace = new idViewEntity();
		//public idRenderWorld RenderWorld;
		
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
		public bool Is2DGui;

		/*int					numClipPlanes;			// mirrors will often use a single clip plane
		idPlane				clipPlanes[MAX_CLIP_PLANES];		// in world space, the positive side
													// of the plane is the visible side
		 * */

		/// <summary>
		/// In real pixels and proper Y flip.
		/// </summary>
		public idScreenRect Viewport;

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
		public List<idDrawSurface> DrawSurfaces = new List<idDrawSurface>();

		/*struct viewLight_s	*viewLights;		// chain of all viewLights effecting view
		*/

		/// <summary>
		/// Chain of all viewEntities effecting view, including off screen ones casting shadows.
		/// </summary>
		/// <remarks>
		/// We use ViewEntities as a check to see if a given view consists solely
		/// of 2D rendering, which we can optimize in certain ways.  A 2D view will
		/// not have any viewEntities.
		/// </remarks>
		public idViewEntity ViewEntities;
		
		public Plane[] Frustum = new Plane[6];		// positive sides face outward, [4] is the front clip plane

		public int AreaNumber = -1; // -1 = not in a valid area

		// An array in frame temporary memory that lists if an area can be reached without
		// crossing a closed door.  This is used to avoid drawing interactions
		// when the light is behind a closed door.
		public bool[] ConnectedAreas;		
	}

	/// <summary>
	/// A viewEntity is created whenever a idRenderEntityLocal is considered for inclusion
	/// in the current view, but it may still turn out to be culled.
	/// </summary>
	/// <remarks>
	/// ViewEntity are allocated on the frame temporary stack memory
	/// a viewEntity contains everything that the back end needs out of a idRenderEntityLocal,
	/// which the front end may be modifying simultaneously if running in SMP mode.
	/// <para/>
	/// A single entityDef can generate multiple viewEntity_t in a single frame, as when seen in a mirror.
	/// </remarks>
	public class idViewEntity
	{
		public idViewEntity Next;

		// back end should NOT reference the entityDef, because it can change when running SMP
		/*idRenderEntityLocal	*	entityDef;*/

		// for scissor clipping, local inside renderView viewport
		// scissorRect.Empty() is true if the viewEntity_t was never actually
		// seen through any portals, but was created for shadow casting.
		// a viewEntity can have a non-empty scissorRect, meaning that an area
		// that it is in is visible, and still not be visible.
		public idScreenRect	Scissor;

		/// <summary>
		/// Force two sided and vertex colors regardless of material setting.
		/// </summary>
		public bool	IsGuiSurface;

		public bool SkipMotionBlur;
		
		public bool WeaponDepthHack;
		public float ModelDepthHack;
		
		/// <summary>
		/// Local coords to global coords.
		/// </summary>
		public Matrix ModelMatrix;

		/// <summary>
		/// Local coords to eye coords.
		/// </summary>
		public Matrix ModelViewMatrix;

		//idRenderMatrix			mvp;

		// parallelAddModels will build a chain of surfaces here that will need to
		// be linked to the lights or added to the drawsurf list in a serial code section
		/*drawSurf_t *			drawSurfs;

		// R_AddSingleModel will build a chain of parameters here to setup shadow volumes
		staticShadowVolumeParms_t *		staticShadowVolumes;
		dynamicShadowVolumeParms_t *	dynamicShadowVolumes;*/
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
		public int[] Time = new int[2];

		/// <summary>
		/// Can be used in any way by the material.
		/// </summary>
		public float[] MaterialParameters = new float[Constants.MaxGlobalMaterialParameters];

		/// <summary>
		/// Override everything when drawing.
		/// </summary>
		/// 
		public idMaterial GlobalMaterial;

		/// <summary>
		/// The viewEyeBuffer may be of a different polarity than stereoScreenSeparation if the eyes have been swapped.
		/// </summary>
		/// <remarks>
		/// -1 = left eye, 1 = right eye, 0 = monoscopic view or GUI.
		/// </remarks>
		public int ViewEyeBuffer;	
			
		/// <summary>
		/// Projection matrix horizontal offset, positive or negative based on camera eye.
		/// </summary>
		public float StereoScreenSeparation;

		public idRenderView()
		{
			Clear();
		}

		public void Clear()
		{
			this.ViewID      = 0;
			this.FovX        = 0;
			this.FovY        = 0;

			this.ViewOrigin  = Vector3.Zero;
			this.ViewAxis    = Matrix.Identity;

			this.CramZNear   = false;
			this.ForceUpdate = false;
			this.Time[0]     = this.Time[1] = 0;

			for(int i = 0; i < Constants.MaxGlobalMaterialParameters; i++)
			{
				this.MaterialParameters[i] = 0;
			}

			this.GlobalMaterial = null;

			this.ViewEyeBuffer          = 0;
			this.StereoScreenSeparation = 0;
		}

		/// <summary>
		/// Creates a shallow copy of this instance.
		/// </summary>
		public idRenderView Copy()
		{
			idRenderView view       = new idRenderView();
			view.ViewID             = this.ViewID;
			view.FovX               = this.FovX;
			view.FovY               = this.FovY;

			view.ViewOrigin         = this.ViewOrigin;
			view.ViewAxis           = this.ViewAxis;
			view.CramZNear          = this.CramZNear;
			view.ForceUpdate        = this.ForceUpdate;
			view.Time               = (int[]) this.Time.Clone();

			view.MaterialParameters = (float[]) this.MaterialParameters;
			view.GlobalMaterial     = this.GlobalMaterial;

			view.ViewEyeBuffer          = this.ViewEyeBuffer;
			view.StereoScreenSeparation = this.StereoScreenSeparation;

			return view;
		}
	}

	public class idRenderCapabilities
	{
		public float ShaderModel;

		public int MaxVertexBufferElements;
		public int MaxIndexBufferElements;

		public int MaxTextureSize;
		public int MaxTextureCoords;
		public int MaxTextureImageUnits;
		public int MaxTextureAnisotropy;

		public bool AnisotropicAvailable;
		public bool MultiTextureAvailable;
		public bool	OcclusionQueryAvailable;
		public bool TextureCompressionAvailable;
		public bool TextureNonPowerOfTwoAvailable;
		
		public int UniformBufferOffsetAlignment;
		
		public int ColorBits;
		public int DepthBits;
		public int StencilBits;

		//stereo3DMode_t		stereo3Dmode;
		public int NativeScreenWidth; // this is the native screen width resolution of the renderer
		public int NativeScreenHeight; // this is the native screen height resolution of the renderer

		public int DisplayFrequency;

		public int IsFullscreen;					// monitor number
		public bool	IsStereoPixelFormat;
		public bool	IstereoPixelFormatAvailable;
		public int Multisamples;

		// Screen separation for stereoscopic rendering is set based on this.
		// PC vid code sets this, converting from diagonals / inches / whatever as needed.
		// If the value can't be determined, set something reasonable, like 50cm.
		public float PhysicalScreenWidthInCentimeters;	
		public float PixelAspect;
	}

	public struct idVertex
	{
		public Vector3 Position;
		public HalfVector2 TextureCoordinates;
		public Byte4 Normal;
		public Byte4 Tangent;
		public Color Color;
		public Color Color2;
		
		public void Clear()
		{
			this.Position.X         = 0;
			this.Position.Y         = 0;
			this.Position.Z         = 0;

			this.TextureCoordinates = new HalfVector2(0, 0);

			this.Normal             = new Byte4(0, 0, 1, 0);
			this.Tangent            = new Byte4(1, 0, 0, 0);

			this.Color              = Color.Black;
			this.Color2             = Color.Black;
		}

		public void ClearColor2()
		{
			this.Color2 = Color.Gray;
		}

		public static VertexDeclaration VertexDeclaration = new VertexDeclaration(
					new VertexElement[] {
						new VertexElement(0,  VertexElementFormat.Vector3,     VertexElementUsage.Position, 0),
						new VertexElement(12, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 0),
						new VertexElement(16, VertexElementFormat.Byte4,       VertexElementUsage.Normal, 0),
						new VertexElement(20, VertexElementFormat.Byte4,       VertexElementUsage.Tangent, 0),
						new VertexElement(24, VertexElementFormat.Color,       VertexElementUsage.Color, 0),
						new VertexElement(28, VertexElementFormat.Color,       VertexElementUsage.Color, 1)
					}
				);
	}

	/// <summary>
	/// Command the back end to render surfaces
	/// </summary>
	/// <remarks>
	/// A given srfTriangles_t may be used with multiple viewEntity_t,
	/// as when viewed in a subview or multiple viewport render, or
	/// with multiple shaders when skinned, or, possibly with multiple
	/// lights, although currently each lighting interaction creates
	/// unique srfTriangles_t
	/// drawSurf_t are always allocated and freed every frame, they are never cached
	/// </remarks>
	public struct idDrawSurface
	{
		//const srfTriangles_t *	frontEndGeo;		// don't use on the back end, it may be updated by the front end!
		public int FirstIndex;
		public int IndexCount;
		public int FirstVertex;
		public int VertexCount;
		public IndexBuffer IndexBuffer;
		public VertexBuffer VertexBuffer;

		//vertCacheHandle_t		shadowCache;		// idShadowVert / idShadowVertSkinned
		//vertCacheHandle_t		jointCache;			// idJointMat
		public idViewEntity Space;
		
		/// <summary>
		/// May be NULL for shadow volumes.
		/// </summary>
		public idMaterial Material;	

		/// <summary>
		/// Extra GL state |'d with material->stage[].drawStateBits.
		/// </summary>
		public ulong ExtraState;

		/// <summary>
		/// Material->sort, modified by gui / entity sort offsets.
		/// </summary>
		public float Sort;

		/// <summary>
		/// Evaluated and adjusted for referenceShaders.
		/// </summary>
		public float[] MaterialRegisters;

		// drawSurf_t *			nextOnLight;		// viewLight chains
		// drawSurf_t **			linkChain;			// defer linking to lights to a serial section to avoid a mutex

		/// <summary>
		/// For scissor clipping, local inside renderView viewport.
		/// </summary>
		public idScreenRect Scissor;
		public int RenderZFail;
		//volatile shadowVolumeState_t shadowVolumeState;
	}
}