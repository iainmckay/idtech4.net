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
using idTech4;

namespace idTech4.Platform.Windows
{
	public class WindowsSession : idSession
	{
		#region Constructor
		public WindowsSession()
		{
			// TODO:
			/*signInManager		= new (TAG_SYSTEM) idSignInManagerWin;
			saveGameManager		= new (TAG_SAVEGAMES) idSaveGameManager();
			voiceChat			= new (TAG_SYSTEM) idVoiceChatMgrWin();
			lobbyToSessionCB	= new (TAG_SYSTEM) idLobbyToSessionCBLocal( this );
			
			lobbyBackends.Zero();*/
		}

		~WindowsSession()
		{
			// TODO: 
			/*delete voiceChat;
			delete lobbyToSessionCB;*/
		}
		#endregion

		#region idSession implementation
		#region Initialization
		public override void Initialize()
		{
			base.Initialize();

			// TODO:
			// The shipping path doesn't load title storage
			// Instead, we inject values through code which is protected through steam DRM
			/*titleStorageVars.Set( "MAX_PLAYERS_ALLOWED", "8" );
			titleStorageLoaded = true;

			// First-time check for downloadable content once game is launched
			EnumerateDownloadableContent();

			GetPartyLobby().Initialize( idLobby::TYPE_PARTY, sessionCallbacks );
			GetGameLobby().Initialize( idLobby::TYPE_GAME, sessionCallbacks );
			GetGameStateLobby().Initialize( idLobby::TYPE_GAME_STATE, sessionCallbacks );

			achievementSystem = new (TAG_SYSTEM) idAchievementSystemWin();
			achievementSystem->Init();*/
		}
		#endregion
		#endregion
	}
}