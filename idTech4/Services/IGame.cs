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
namespace idTech4.Services
{
	public interface IGame
	{
		#region Game
		/// <summary>
		/// Makes rendering and sound system calls.
		/// </summary>
		bool Draw(int clientNum);
		#endregion

		#region Initialization
		/// <summary>
		/// Initialize the game for the first time.
		/// </summary>
		void Init();
		#endregion

		#region Main Menu
		/*bool InhibitControls() = 0;
		void Shell_Init( const char * filename, idSoundWorld * sw ) = 0;
		void Shell_Cleanup() = 0;*/
		void Shell_CreateMenu(bool inGame);
		/*void Shell_ClosePause() = 0;*/
		void Shell_Show(bool show);
		bool Shell_IsActive();
		/*bool Shell_HandleGuiEvent( const sysEvent_t * sev ) = 0;*/
		void Shell_Render();
		/*void Shell_ResetMenu() = 0;*/
		void Shell_SyncWithSession();
		/*void Shell_UpdateSavedGames() = 0;
		void Shell_SetCanContinue( bool valid ) = 0;
		void Shell_UpdateClientCountdown( int countdown ) = 0;
		void Shell_UpdateLeaderboard( const idLeaderboardCallback * callback ) = 0;
		void Shell_SetGameComplete();*/
		#endregion

		#region Misc.
		int LocalClientNumber { get; }
		#endregion
	}

	public struct GameReturn
	{
		/// <summary>
		/// "map", "disconnect", "victory", etc
		/// </summary>
		public string SessionCommand;

		/// <summary>
		/// Used when cinematics are skipped to prevent session from simulating several game frames to keep the game time in sync with real time.
		/// </summary>
		public bool SynchronizeNextGameFrame;

		public int VibrationLow;
		public int VibrationHigh;
	}
}