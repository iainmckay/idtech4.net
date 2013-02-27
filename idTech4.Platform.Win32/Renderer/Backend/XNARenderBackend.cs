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
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.Platform.Win32.Renderer.Backend
{
	public class XNARenderBackend : IRenderBackend
	{
		#region Members
		private long _prevBlockTime;
		private GraphicsDeviceManager _graphicsDeviceManager;
		private idRenderCapabilities _renderCaps;
		#endregion

		#region Constructor
		public XNARenderBackend()
			: base()
		{
			_renderCaps = new idRenderCapabilities();
			_graphicsDeviceManager = new GraphicsDeviceManager(idEngine.Instance);
		}
		#endregion

		#region Buffer
		private void SwapBuffers()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			idCVar swapInterval = cvarSystem.Find("r_swapInterval");

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
			_renderCaps = new idRenderCapabilities();
			_renderCaps.MaxTextureAnisotropy = 16;
			_renderCaps.MaxTextureImageUnits = 8;

			if(_graphicsDeviceManager.GraphicsProfile == GraphicsProfile.HiDef)
			{
				_renderCaps.MaxTextureSize = 4096;
				_renderCaps.OcclusionQueryAvailable = true;
				_renderCaps.TextureNonPowerOfTwoAvailable = true;
				_renderCaps.ShaderModel = 3;
			}
			else
			{
				_renderCaps.MaxTextureSize = 2048;
				_renderCaps.ShaderModel = 2;
			}
		}
		#endregion
		#endregion

		#region IRenderBackend implementation
		#region Methods
		/// <summary>
		/// We want to exit this with the GPU idle, right at vsync
		/// </summary>
		public void BlockingSwapBuffers()
		{
			idLog.Warning("TODO: RENDERLOG_PRINTF( \"***************** GL_BlockingSwapBuffers *****************\n\n\n\n" );

			idEngine engine        = idEngine.Instance;
			ICVarSystem cvarSystem = engine.GetService<ICVarSystem>();

			long beforeFinish = engine.ElapsedTime;
			
			// TODO: sync would be nice but not sure there is a way with XNA
			/*if ( !glConfig.syncAvailable ) {
				glFinish();
			}*/

			long beforeSwap = engine.ElapsedTime;
	
			if((cvarSystem.GetBool("r_showSwapBuffers") == true) && ((beforeSwap - beforeFinish) > 1))
			{
				idLog.WriteLine("{0} msec to glFinish", beforeSwap - beforeFinish);
			}

			SwapBuffers();

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
				idLog.WriteLine("blcokToBlock: {0}", exitBlockTime - _prevBlockTime);
			}

			_prevBlockTime = exitBlockTime;
		}

		public void Init()
		{			
			idLog.WriteLine("----- R_InitDevice -----");
									
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
			for(i = 0 ; i < 3 ; i++) 
			{
				// TODO: stereo
				/*if ( i == 0 && stereoRender_enable.GetInteger() != STEREO3D_QUAD_BUFFER ) {
					continue;		// don't even try for a stereo mode
				}*/

				if(cvarSystem.GetInt("r_fullscreen") <= 0)
				{
					// use explicit position / size for window
					width = cvarSystem.GetInt("r_windowWidth");
					height = cvarSystem.GetInt("r_windowHeight");
					
					x = cvarSystem.GetInt("r_windowX");
					y = cvarSystem.GetInt("r_windowY");

					// may still be -1 to force a borderless window
					fullscreen = cvarSystem.GetInt("r_fullscreen");					
					displayHz = 0;		// ignored
				} 
				else 
				{					
					// get the mode list for this monitor
					List<XNADisplayMode> modeList = new List<XNADisplayMode>();
					int fullMode = cvarSystem.GetInt("r_fullscreen");
					
					if(GetModeListForDisplay(fullMode - 1, modeList) == false) 
					{
						idLog.WriteLine("r_fullscreen reset from {0} to 1 because mode list failed.", fullMode);
						cvarSystem.Set("r_fullscreen", 1);
						GetModeListForDisplay(0, modeList);
					}

					if(modeList.Count < 1 ) 
					{
						idLog.WriteLine("Going to safe mode because mode list failed.");
						goto safeMode;
					}

					x = 0; // ignored
					y = 0; // ignored
					fullscreen = cvarSystem.GetInt("r_fullscreen");

					// set the parameters we are trying
					int vidMode = cvarSystem.GetInt("r_vidMode");

					if(vidMode < 0)
					{
						// try forcing a specific mode, even if it isn't on the list
						width = cvarSystem.GetInt("r_customWidth");
						height = cvarSystem.GetInt("r_customHeight");
						displayHz = cvarSystem.GetInt("r_displayRefresh");
					} 
					else 
					{
						if(vidMode > modeList.Count)
						{
							idLog.WriteLine("r_vidMode reset from {0} to 0.", vidMode);
							cvarSystem.Set("r_vidMode", 0);
						}

						adapter = modeList[vidMode].Adapter;
						width = modeList[vidMode].DisplayMode.Width;
						height = modeList[vidMode].DisplayMode.Height;
						displayHz = /*modeList[vidMode].DisplayHz;*/ 0;
					}
				}

				int multiSamples = cvarSystem.GetInt("r_multiSamples");
				bool stereo = false;

				if(i == 0)
				{
					idLog.Warning("TODO: parms.stereo = ( stereoRender_enable.GetInteger() == STEREO3D_QUAD_BUFFER );");
				}
			
				if(fullInit == true)
				{
					// create the context as well as setting up the window
					if(ContextInit(adapter, x, y, width, height, multiSamples, fullscreen, stereo)  == true)
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

			_graphicsDeviceManager.PreferMultiSampling = (multiSamples > 1);
			_graphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
			_graphicsDeviceManager.PreparingDeviceSettings += delegate(object sender, PreparingDeviceSettingsEventArgs args)
			{
				args.GraphicsDeviceInformation.Adapter = adapter;
				args.GraphicsDeviceInformation.GraphicsProfile = adapter.IsProfileSupported(GraphicsProfile.HiDef) ? GraphicsProfile.HiDef : GraphicsProfile.Reach;

				PresentationParameters p = args.GraphicsDeviceInformation.PresentationParameters;
				p.BackBufferWidth = width;
				p.BackBufferHeight = height;
				p.MultiSampleCount = multiSamples;
				p.IsFullScreen = (fullScreen > 0);
			};

			_graphicsDeviceManager.ApplyChanges();
						
			Form window = (Form) Form.FromHandle(idEngine.Instance.Window.Handle);
			window.Text = idLicensee.GameName;
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

			_renderCaps.IsFullscreen = fullScreen;
			_renderCaps.IsStereoPixelFormat = stereo;
			_renderCaps.NativeScreenWidth = width;
			_renderCaps.NativeScreenHeight = height;
			_renderCaps.Multisamples = multiSamples;

			// FIXME: some monitor modes may be distorted. should side-by-side stereo modes be consider aspect 0.5?
			_renderCaps.PixelAspect = 1.0f; 

			_renderCaps.StencilBits = 8;
			_renderCaps.ColorBits = 32;
			_renderCaps.DepthBits = 24;

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
				Screen monitor = Screen.FromHandle(adapter.MonitorHandle);

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
	}
}