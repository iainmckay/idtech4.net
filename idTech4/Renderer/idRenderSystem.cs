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

		// GUI drawing variables for surface creation
		private int _guiRecursionLevel;		// to prevent infinite overruns
		//private idGuiModel _guiModel;
		#endregion

		#region Constructor
		public idRenderSystem(GraphicsDeviceManager deviceManager)
		{
			Clear();

			idLog.WriteLine("------- Initializing renderSystem --------\n");

			// clear all our internal state
			_viewCount = 1;		// so cleared structures never match viewCount
								// we used to memset tr, but now that it is a class, we can't, so
								// there may be other state we need to reset

			_ambientLightVector = new Vector4(0.5f, 0.5f - 0.385f, 0.8925f, 1.0f);
			_backend = FindBackend(deviceManager);

			idLog.Warning("TODO: _guiModel = new idGuiModel();");

			idLog.Warning("TODO: globalImages->Init();");

			idLog.Warning("TODO: idCinematic::InitCinematic( );");

			// build brightness translation tables
			idLog.Warning("TODO: R_SetColorMappings();");

			InitMaterials();

			idLog.Warning("TODO: renderModelManager->Init();");

			// set the identity space
			_identitySpace = new idViewEntity();

			// make sure the tr.unitSquareTriangles data is current in the vertex / index cache
			idLog.Warning("TODO: if ( unitSquareTriangles == NULL ) { unitSquareTriangles = R_MakeFullScreenTris(); }");

			// make sure the tr.zeroOneCubeTriangles data is current in the vertex / index cache
			idLog.Warning("TODO: if ( zeroOneCubeTriangles == NULL ) { zeroOneCubeTriangles = R_MakeZeroOneCubeTris(); }");

			// make sure the tr.testImageTriangles data is current in the vertex / index cache
			idLog.Warning("TODO: if ( testImageTriangles == NULL )  { testImageTriangles = R_MakeTestImageTriangles(); }");

			idLog.Warning("TODO: frontEndJobList = parallelJobManager->AllocJobList( JOBLIST_RENDERER_FRONTEND, JOBLIST_PRIORITY_MEDIUM, 2048, 0, null);");

			// make sure the command buffers are ready to accept the first screen update
			SwapCommandBuffers();

			idLog.WriteLine("renderSystem initialized.");
			idLog.WriteLine("--------------------------------------");
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
		private IRenderBackend FindBackend(GraphicsDeviceManager deviceManager)
		{
			return idEngine.Instance.GetService<IPlatformService>().CreateRenderBackend(deviceManager);
		}

		private void InitMaterials()
		{
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

			_defaultMaterial = declManager.FindMaterial("_default", false);

			if(_defaultMaterial == null)
			{
				idEngine.Instance.FatalError("_default material not found");
			}
						
			_defaultPointLight = declManager.FindMaterial("lights/defaultPointLight");
			_defaultProjectedLight = declManager.FindMaterial("lights/defaultProjectedLight");
			_whiteMaterial = declManager.FindMaterial("_white");
			_charSetMaterial = declManager.FindMaterial("textures/bigchars");
		}
		#endregion
		#endregion

		#region State Management
		private void Clear()
		{
			idLog.WriteLine("TODO: clear");

			//registered = false;
			_frameCount = 0;
			_viewCount = 0;
			_frameShaderTime = 0.0f;
			/*ambientLightVector.Zero();
			worlds.Clear();
			primaryWorld = NULL;
			memset(&primaryRenderView, 0, sizeof(primaryRenderView));
			primaryView = NULL*;*/
			_defaultMaterial = null;
			/*testImage = NULL;
			ambientCubeImage = NULL;
			viewDef = NULL;
			memset(&pc, 0, sizeof(pc));*/

			_identitySpace = new idViewEntity();
			/*memset(renderCrops, 0, sizeof(renderCrops));
			currentRenderCrop = 0;
			currentColorNativeBytesOrder = 0xFFFFFFFF;
			currentGLState = 0;*/
			_guiRecursionLevel = 0;
			/*guiModel = NULL;
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

		private void SwapCommandBuffers_FinishRendering(out ulong frontEnd, out ulong backEnd, out ulong shadow, out ulong gpu)
		{
			// TODO: SCOPED_PROFILE_EVENT( "SwapCommandBuffers" );

			gpu = 0;		// until shown otherwise
			frontEnd = 0;
			backEnd = 0;
			shadow = 0;
		
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
			backEnd = 0;
			shadow = 0;

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

		private LinkedListNode<idRenderCommand> SwapCommandBuffers_FinishCommandBuffers() 
		{
			if(this.IsInitialized == false)
			{
				return null;
			}

			// close any gui drawing
			idLog.Warning("TODO: guiModel->EmitFullScreen();");
			idLog.Warning("guiModel->Clear();");

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
			idLog.Warning("guiModel->BeginFrame();");

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
			/*renderCrops[0].x1 = 0;
			renderCrops[0].y1 = 0;
			renderCrops[0].x2 = GetWidth() - 1;
			renderCrops[0].y2 = GetHeight() - 1;
			currentRenderCrop = 0;*/

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
		//public idScreenRect ViewPort;

		/// <summary>
		/// For scissor clipping, local inside renderView viewport.
		/// </summary>
		/// <remarks>
		/// Subviews may only be rendering part of the main view
		/// these are real physical pixel values, possibly scaled and offset from the
		/// renderView x/y/width/height
		/// </remarks>
		//public idScreenRect Scissor;

		/*struct viewDef_s *	superView;				// never go into an infinite subview loop 
		struct drawSurf_s *	subviewSurface;*/

		/// <summary>
		/// DrawSurfs are the visible surfaces of the viewEntities, sorted
		/// by the material sort parameter
		/// </summary>
		/*drawSurf_t **		drawSurfs;				// we don't use an idList for this, because
	int					numDrawSurfs;			// it is allocated in frame temporary memory
	int					maxDrawSurfs;			// may be resized*/

		/*struct viewLight_s	*viewLights;		// chain of all viewLights effecting view*/
		//viewEntity_t* viewEntitys;			// chain of all viewEntities effecting view, including off screen ones casting shadows
		// chain of all viewEntities effecting view, including off screen ones casting shadows
		// we use viewEntities as a check to see if a given view consists solely
		// of 2D rendering, which we can optimize in certain ways.  A 2D view will
		// not have any viewEntities
		
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
		/*viewEntity_t *			next;

		// back end should NOT reference the entityDef, because it can change when running SMP
		idRenderEntityLocal	*	entityDef;

		// for scissor clipping, local inside renderView viewport
		// scissorRect.Empty() is true if the viewEntity_t was never actually
		// seen through any portals, but was created for shadow casting.
		// a viewEntity can have a non-empty scissorRect, meaning that an area
		// that it is in is visible, and still not be visible.
		idScreenRect			scissorRect;

		bool					isGuiSurface;			// force two sided and vertex colors regardless of material setting

		bool					skipMotionBlur;

		bool					weaponDepthHack;
		float					modelDepthHack;
		
		float					modelMatrix[16];		// local coords to global coords
		float					modelViewMatrix[16];	// local coords to eye coords

		idRenderMatrix			mvp;

		// parallelAddModels will build a chain of surfaces here that will need to
		// be linked to the lights or added to the drawsurf list in a serial code section
		drawSurf_t *			drawSurfs;

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

		public idRenderView()
		{
			Clear();
		}

		public void Clear()
		{
			this.ViewID = 0;
			this.FovX = 0;
			this.FovY = 0;

			this.ViewOrigin = Vector3.Zero;
			this.ViewAxis = Matrix.Identity;

			this.CramZNear = false;
			this.ForceUpdate = false;
			this.Time[0] = this.Time[1] = 0;

			for(int i = 0; i < Constants.MaxGlobalMaterialParameters; i++)
			{
				this.MaterialParameters[i] = 0;
			}

			this.GlobalMaterial = null;
		}

		/// <summary>
		/// Creates a shallow copy of this instance.
		/// </summary>
		public idRenderView Copy()
		{
			idRenderView view = new idRenderView();
			view.ViewID = this.ViewID;
			view.FovX = this.FovX;
			view.FovY = this.FovY;

			view.ViewOrigin = this.ViewOrigin;
			view.ViewAxis = this.ViewAxis;

			view.CramZNear = this.CramZNear;
			view.ForceUpdate = this.ForceUpdate;
			view.Time = (int[]) this.Time.Clone(); ;

			view.MaterialParameters = (float[]) this.MaterialParameters;
			view.GlobalMaterial = this.GlobalMaterial;

			return view;
		}
	}
}