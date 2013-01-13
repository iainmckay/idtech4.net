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
using Microsoft.Xna.Framework;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.Platform.Win32.Renderer.Backend
{
	public class XNARenderBackend : IRenderBackend
	{
		#region Members
		private long _prevBlockTime;
		private GraphicsDeviceManager _graphicsDeviceManager;
		#endregion

		#region Constructor
		public XNARenderBackend(GraphicsDeviceManager deviceManager)
			: base()
		{
			_graphicsDeviceManager = deviceManager;
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

		#region IRenderBackend implementation
		#region Methods
		/// <summary>
		/// We want to exit this with the GPU idle, right at vsync
		/// </summary>
		public void BlockingSwapBuffers()
		{
			idLog.Warning("TODO: RENDERLOG_PRINTF( \"***************** GL_BlockingSwapBuffers *****************\n\n\n\" );");

			idEngine engine = idEngine.Instance;
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
		#endregion
		#endregion
	}
}
