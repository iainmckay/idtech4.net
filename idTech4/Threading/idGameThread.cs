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
using System.Threading;

using idTech4.Services;

namespace idTech4.Threading
{
	public class idGameThread : idSysThread
	{
		#region Members
		private long _gameTime;
		private long _drawTime;
		private long _threadTime;				// total time : game time + foreground render time
		private long _threadGameTime;			// game time only
		private long _threadRenderTime;			// render fg time only
	
		private GameReturn _ret;

		private int	_gameFrameCount;
		private bool _isClient;
		#endregion

		#region Constructor
		public idGameThread() 
			: base()
		{

		}
		#endregion

		#region Methods
		// the gameReturn_t is from the previous frame, the
		// new frame will be running in parallel on exit
		public GameReturn RunGameAndDraw(int gameFrameCount, object /*TODO: idUserCmdMgr*/ userCmdMgr, bool isClient, long startGameFrame)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			// this should always immediately return
			WaitForThread();

			// save the usercmds for the background thread to pick up
			// TODO: userCmdMgr = &userCmdMgr_;

			_isClient = isClient;

			// grab the return value created by the last thread execution
			GameReturn latchedRet = _ret;

			_gameFrameCount = gameFrameCount;

			// start the thread going
			if(cvarSystem.GetBool("com_smp") == false)
			{
				// run it in the main thread so PIX profiling catches everything
				Run();
			}
			else
			{
				SignalWork();
			}

			// return the latched result while the thread runs in the background
			return latchedRet;
		}
		#endregion

		#region idSysThread implementation
		/// <summary>
		/// Run in a background thread for performance, but can also be called directly in the foreground thread for comparison.
		/// </summary>
		/// <returns></returns>
		protected override int Run()
		{
 			// TODO: commonLocal.frameTiming.startGameTime = Sys_Microseconds();

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			// debugging tool to test frame dropping behavior
			if(cvarSystem.GetInt("com_sleepGame") > 0)
			{
				Thread.Sleep(cvarSystem.GetInt("com_sleepGame"));
			}

			if(_gameFrameCount == 0)
			{
				// ensure there's no stale gameReturn data from a paused game
				_ret = new GameReturn();
			}

			if(_isClient == true)
			{
				// run the game logic
				for(int i = 0; i < _gameFrameCount; i++)
				{
					// TODO: SCOPED_PROFILE_EVENT( "Client Prediction" );
					// TODO: userCmdMgr
					/*if ( userCmdMgr ) {
						game->ClientRunFrame( *userCmdMgr, ( i == numGameFrames - 1 ), ret );
					}*/

					if((_ret.SynchronizeNextGameFrame == true) && (string.IsNullOrEmpty(_ret.SessionCommand) == false))
					{
						break;
					}
				}
			} 
			else 
			{
				// run the game logic
				for(int i = 0; i < _gameFrameCount; i++)
				{
					// TODO: SCOPED_PROFILE_EVENT( "GameTic" );
					// TODO: userCmdMgr
					/*if ( userCmdMgr ) {
						game->RunFrame( *userCmdMgr, ret );
					}*/

					if((_ret.SynchronizeNextGameFrame == true) && (string.IsNullOrEmpty(_ret.SessionCommand) == false))
					{
						break;
					}
				}
			}

			// we should have consumed all of our usercmds
			// TODO: userCmdMgr
			/*if ( userCmdMgr ) {
				if ( userCmdMgr->HasUserCmdForPlayer( game->GetLocalClientNum() ) && common->GetCurrentGame() == DOOM3_BFG ) {
					idLib::Printf( "idGameThread::Run: didn't consume all usercmds\n" );
				}
			}*/

			// TODO: commonLocal.frameTiming.finishGameTime = Sys_Microseconds();

			// TODO: SetThreadGameTime( ( commonLocal.frameTiming.finishGameTime - commonLocal.frameTiming.startGameTime ) / 1000 );

			// build render commands and geometry
			{
				// TODO: SCOPED_PROFILE_EVENT( "Draw" );
				idEngine.Instance.Draw();
			}

			// TODO:
			/*commonLocal.frameTiming.finishDrawTime = Sys_Microseconds();

			SetThreadRenderTime( ( commonLocal.frameTiming.finishDrawTime - commonLocal.frameTiming.finishGameTime ) / 1000 );
			SetThreadTotalTime( ( commonLocal.frameTiming.finishDrawTime - commonLocal.frameTiming.startGameTime ) / 1000 );*/

			return 0;
		}
		#endregion
	}
}