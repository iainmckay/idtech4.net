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
	/// <summary>
	/// Responsible for managing the screen, which can have multiple idRenderWorld and 2D drawing done on it.
	/// </summary>
	public sealed class idRenderSystem
	{
		#region Members
		private bool _registered;				// cleared at shutdown, set at InitOpenGL.

		private int _frameCount;				// incremented every frame
		private int _viewCount;					// incremented every view (twice a scene if subviewed) and every R_MarkFragments call.

		private Vector4 _ambientLightVector;	// used for "ambient bump mapping".

		private BackendState _backEnd;

		// GUI drawing variables for surface creation
		private int _guiRecursionLevel;			// to prevent infinite overruns.
		private idGuiModel _guiModel;
		private idGuiModel _demoGuiModel;
		#endregion

		#region Constructor
		public idRenderSystem()
		{
			Clear();
		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			return;
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
		/*	R_SetColorMappings();

	R_InitMaterials();

	renderModelManager->Init();

	// set the identity space
	identitySpace.modelMatrix[0*4+0] = 1.0f;
	identitySpace.modelMatrix[1*4+1] = 1.0f;
	identitySpace.modelMatrix[2*4+2] = 1.0f;

	// determine which back end we will use
	// ??? this is invalid here as there is not enough information to set it up correctly
	SetBackEndRenderer();*/

			idConsole.WriteLine("renderSystem initialized.");
			idConsole.WriteLine("--------------------------------------");
	}
		#endregion

		#region Private
		private void Clear()
		{
			_registered = false;
			_frameCount = 0;
			_viewCount = 0;
			/*staticAllocCount = 0;
			frameShaderTime = 0.0f;
			viewportOffset[0] = 0;
			viewportOffset[1] = 0;
			tiledViewport[0] = 0;
			tiledViewport[1] = 0;
			backEndRenderer = BE_BAD;
			backEndRendererHasVertexPrograms = false;
			backEndRendererMaxLight = 1.0f;*/

			_ambientLightVector = Vector4.Zero;

			/*sortOffset = 0;
			worlds.Clear();
			primaryWorld = NULL;
			memset( &primaryRenderView, 0, sizeof( primaryRenderView ) );
			primaryView = NULL;
			defaultMaterial = NULL;
			testImage = NULL;
			ambientCubeImage = NULL;
			viewDef = NULL;
			memset( &pc, 0, sizeof( pc ) );
			memset( &lockSurfacesCmd, 0, sizeof( lockSurfacesCmd ) );
			memset( &identitySpace, 0, sizeof( identitySpace ) );
			logFile = NULL;
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
		#endregion
		#endregion
	}

	/// <summary>
	/// All state modified by the back end is separated from the front end state.
	/// </summary>
	internal struct BackendState
	{
		/*int					frameCount;		// used to track all images used in a frame
	const viewDef_t	*	viewDef;
	backEndCounters_t	pc;

	const viewEntity_t *currentSpace;		// for detecting when a matrix must change
	idScreenRect		currentScissor;
	// for scissor clipping, local inside renderView viewport

	viewLight_t *		vLight;
	int					depthFunc;			// GLS_DEPTHFUNC_EQUAL, or GLS_DEPTHFUNC_LESS for translucent
	float				lightTextureMatrix[16];	// only if lightStage->texture.hasMatrix
	float				lightColor[4];		// evaluation of current light's color stage

	float				lightScale;			// Every light color calaculation will be multiplied by this,
											// which will guarantee that the result is < tr.backEndRendererMaxLight
											// A card with high dynamic range will have this set to 1.0
	float				overBright;			// The amount that all light interactions must be multiplied by
											// with post processing to get the desired total light level.
											// A high dynamic range card will have this set to 1.0.

	bool				currentRenderCopied;	// true if any material has already referenced _currentRender

	// our OpenGL state deltas
	glstate_t			glState;

	int					c_copyFrameBuffer;*/
	}
}
