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
using idTech4.Services;

namespace idTech4
{
	public abstract class idSession : ISession
	{
		#region Members
		private SessionState _localState;
		private uint _sessionOptions;

		private SessionConnectType _connectType;
		private int _connectTime;

		/*idLobby					partyLobby;
		idLobby					gameLobby;
		idLobby					gameStateLobby;
		idLobbyStub				stubLobby;				// We use this when we request the active lobby when we are not in a lobby (i.e at press start)*/

		private int	_currentID;				// The host used this to send out a unique id to all users so we can identify them

		private int	_lastVoiceSendtime;
		private bool _hasShownVoiceRestrictionDialog;

		private SessionPendingInviteMode _pendingInviteMode;
		private int	_pendingInviteDevice;
		//lobbyConnectInfo_t		pendingInviteConnectInfo;

		private bool _isSysUIShowing;

		private idDict _titleStorageVars;
		private bool _titleStorageLoaded;

		private int	_showMigratingInfoStartTime;

		private int	_nextGameCoalesceTime;
		private bool _gameLobbyWasCoalesced;
		private int	_fullSnapsReceivedCount;

		private bool _flushedStats;
		private int	_loadingID;
		private bool _inviteInfoRequested;

		private float _upstreamDropRate;		// instant rate in B/s at which we are dropping packets due to simulated upstream saturation
		private int	_upstreamDropRateTime;

		private float _upstreamQueueRate;		// instant rate in B/s at which queued packets are coming out after local buffering due to upstream saturation
		private int _upstreamQueueRateTime;

		private int _queuedBytes;

		private int	_waitingOnGameStateMembersToLeaveTime;
		private int _waitingOnGameStateMembersToJoinTime;
		#endregion

		#region Constructor
		public idSession()
		{
			// TODO:
			/*processorSaveFiles( new (TAG_SAVEGAMES) idSaveGameProcessorSaveFiles ),
			processorLoadFiles( new (TAG_SAVEGAMES) idSaveGameProcessorLoadFiles ),
			processorDelete(	new (TAG_SAVEGAMES) idSaveGameProcessorDelete ),
			processorEnumerate( new (TAG_SAVEGAMES) idSaveGameProcessorEnumerateGames ) */

			InitBaseState();
		}

		~idSession()
		{
			// TODO: 
			/*delete processorSaveFiles;
			delete processorLoadFiles;
			delete processorDelete;
			delete processorEnumerate;
			delete sessionCallbacks;*/
		}
		#endregion

		#region Initialization
		private void InitBaseState()
		{
			_localState     = SessionState.PressStart;
			_sessionOptions = 0;
			_currentID      = 0;

			// TODO: sessionCallbacks				= new (TAG_NETWORKING) idSessionLocalCallbacks( this );

			_connectType                          = SessionConnectType.None;
			_connectTime                          = 0;

			_upstreamDropRate                     = 0.0f;
			_upstreamDropRateTime                 = 0;
			_upstreamQueueRate                    = 0.0f;
			_upstreamQueueRateTime                = 0;
			_queuedBytes                          = 0;

			_lastVoiceSendtime                    = 0;
			_hasShownVoiceRestrictionDialog       = false;

			_isSysUIShowing                       = false;

			_pendingInviteDevice                  = 0;
			_pendingInviteMode                    = SessionPendingInviteMode.None;

			// TODO: downloadedContent.Clear();
			// TODO: _marketplaceHasNewContent = false;

			// TODO: _offlineTransitionTimerStart = 0;
			_showMigratingInfoStartTime           = 0;
			_nextGameCoalesceTime                 = 0;
			_gameLobbyWasCoalesced                = false;
			_fullSnapsReceivedCount               = 0;

			_flushedStats                         = false;

			_titleStorageLoaded                   = false;

			// TODO: _droppedByHost = false;
			_loadingID                            = 0;

			// TODO:
			/*_storedPeer = -1;
			_storedMsgType = -1;*/
			
			_inviteInfoRequested                  = false;

			// TODO: _enumerationHandle = 0;

			_waitingOnGameStateMembersToLeaveTime = 0;
			_waitingOnGameStateMembersToJoinTime  = 0;
		}

		public virtual void Initialize()
		{

		}
		#endregion
	}

	public enum SessionState
	{
		/// <summary>We are at press start.</summary>
		PressStart,
		/// <summary>We are at the main menu.</summary>
		Idle,
		/// <summary>We are in the party lobby menu as host.</summary>
		PartyLobbyHost,
		/// <summary>We are in the party lobby menu as a peer.</summary>
		PartyLobbyPeer,
		/// <summary>We are in the game lobby as a host.</summary>
		GameLobbyHost,
		/// <summary>We are in the game lobby as a peer.</summary>
		GameLobbyPeer,
		/// <summary>We are in the game state lobby as a host.</summary>
		GameStateLobbyHost,
		/// <summary>We are in the game state lobby as a peer.</summary>
		GameStateLobbyPeer,
		/// <summary>We are creating a party lobby, and will move to that state when done.</summary>
		CreateAndMoveToPartyLobby,
		/// <summary>We are creating a game lobby, and will move to that state when done.</summary>
		CreateAndMoveToGameLobby,
		/// <summary>We are creating a game state lobby, and will move to that state when done.</summary>
		CreateAndMoveToGameStateLobby,
		FindOrCreateMatch,
		ConnectAndMoveToParty,
		ConnectAndMoveToGame,
		ConnectAndMoveToGameState,
		/// <summary>Doing something internally like a QoS/bandwidth challenge.</summary>
		Busy,

		// These are last, so >= STATE_LOADING tests work
		/// <summary>We are loading the map, preparing to go into a match.</summary>
		Loading,
		/// <summary>We are currently in a match.</summary>
		InGame
	}

	public enum SessionConnectType
	{
		None,
		Direct,
		FindOrCreate
	}

	public enum SessionPendingInviteMode
	{
		/// <summary>
		/// No invite waiting.
		/// </summary>
		None,
		/// <summary>
		/// Invite is waiting.
		/// </summary>
		Waiting,
		/// <summary>
		/// We invited ourselves to a match.
		/// </summary>
		SelfWaiting
	}
}