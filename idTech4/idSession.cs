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
using System.Diagnostics;

using idTech4.Services;

using XState = idTech4.State;

namespace idTech4
{
	public abstract class idSession : ISession
	{
		#region Members
		private State _localState;
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

		private long _lastPumpTime;
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
			_localState     = XState.PressStart;
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

		#region Misc
		public void Pump()
		{
			// TODO: SCOPED_PROFILE_EVENT( "Session::Pump" );

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			_lastPumpTime           = -1;
			long time               = idEngine.Instance.ElapsedTime;
			long elapsedPumpSeconds = (time - _lastPumpTime) / 1000;
			
			if((_lastPumpTime != -1) && (elapsedPumpSeconds > 2))
			{
				idLog.Warning("idSession::Pump was not called for {0} seconds", elapsedPumpSeconds);
			}

			_lastPumpTime = time;

			if(cvarSystem.GetInt("net_migrateHost") >= 0)
			{
				idLog.Warning("TODO: net_migrateHost");

				/*if ( net_migrateHost.GetInteger() <= 2 ) {
					if ( net_migrateHost.GetInteger() == 0 ) {
						GetPartyLobby().PickNewHost( true, true );				
					} else {
						GetGameLobby().PickNewHost( true, true );				
					}
				} else {
					GetPartyLobby().PickNewHost( true, true );				
					GetGameLobby().PickNewHost( true, true );				
				}
				net_migrateHost.SetInteger( -1 );*/
			}

			OnPump();

			// TODO
			/*if ( HasAchievementSystem() ) {
				GetAchievementSystem().Pump();
			}

			// Send any voice packets if it's time
			SendVoiceAudio();*/

			bool shouldContinue = true;

			while(shouldContinue == true) 
			{
				// Each iteration, validate the session instances
				// TODO: ValidateLobbies();

				// Pump state
				shouldContinue = HandleState();

				// Pump lobbies
				// TODO: PumpLobbies();
			} 

			// TODO:
			/*if ( GetPartyLobby().lobbyBackend != NULL ) {
				// Make sure game properties aren't set on the lobbyBackend if we aren't in a game lobby.
				// This is so we show up properly in search results in Play with Friends option
				GetPartyLobby().lobbyBackend->SetInGame( GetGameLobby().IsLobbyActive() );

				// Temp location
				UpdateMasterUserHeadsetState();
			}*/

			// Do some last minute checks, make sure everything about the current state and lobbyBackend state is valid, otherwise, take action
			/*ValidateLobbies();

			GetActingGameStateLobby().UpdateSnaps();

			idLobby * activeLobby = GetActivePlatformLobby();

			// Pump pings for the active lobby
			if ( activeLobby != NULL ) {
				activeLobby->PumpPings();
			}

			// Pump packet processing for all lobbies
			GetPartyLobby().PumpPackets();
			GetGameLobby().PumpPackets();
			GetGameStateLobby().PumpPackets();*/

			/*int currentTime = Sys_Milliseconds();

			const int SHOW_MIGRATING_INFO_IN_SECONDS = 3;	// Show for at least this long once we start showing it

			if ( ShouldShowMigratingDialog() ) {
				showMigratingInfoStartTime = currentTime;
			} else if ( showMigratingInfoStartTime > 0 && ( ( currentTime - showMigratingInfoStartTime ) > SHOW_MIGRATING_INFO_IN_SECONDS * 1000 ) ) {
				showMigratingInfoStartTime = 0;
			}

			bool isShowingMigrate = common->Dialog().HasDialogMsg( GDM_MIGRATING, NULL );

			if ( showMigratingInfoStartTime != 0 ) {
				if ( !isShowingMigrate ) {
					common->Dialog().AddDialog( GDM_MIGRATING, DIALOG_WAIT, NULL, NULL, false, "", 0, false, false, true );
				}
			} else if ( isShowingMigrate ) {
				common->Dialog().ClearDialog( GDM_MIGRATING );
			}

			// Update possible pending invite
			UpdatePendingInvite();*/

			// Check to see if we should coalesce the lobby
			if(_nextGameCoalesceTime != 0)
			{
				idLog.Warning("TODO: coalesce lobby");

				/*if ( GetGameLobby().IsLobbyActive() &&
					 GetGameLobby().IsHost() &&
					 GetState() == idSession::GAME_LOBBY &&
					 GetPartyLobby().GetNumLobbyUsers() <= 1 &&
					 GetGameLobby().GetNumLobbyUsers() == 1 &&
					 MatchTypeIsRanked( GetGameLobby().parms.matchFlags ) &&
					 Sys_Milliseconds() > nextGameCoalesceTime ) {
			
					// If the player doesn't care about the mode or map,
					// make sure the search is broadened.
					idMatchParameters newGameParms = GetGameLobby().parms;
					newGameParms.gameMap = GAME_MAP_RANDOM;

					// Assume that if the party lobby's mode is random,
					// the player chose "Quick Match" and doesn't care about the mode.
					// If the player chose "Find Match" and a specific mode,
					// the party lobby mode will be set to non-random.
					if ( GetPartyLobby().parms.gameMode == GAME_MODE_RANDOM ) {
						newGameParms.gameMode = GAME_MODE_RANDOM;
					}

					FindOrCreateMatch( newGameParms );

					gameLobbyWasCoalesced	= true;		// Remember that this round was coalesced.  We so this so main menu doesn't randomize the map, which looks odd
					nextGameCoalesceTime	= 0;
				}*/
			}
		}

		protected virtual void OnPump()
		{

		}

		public void ProcessSnapAckQueue()
		{
			// TODO:
			/*if ( GetActingGameStateLobby().IsLobbyActive() ) {
				GetActingGameStateLobby().ProcessSnapAckQueue();
			}*/
		}

		public void UpdateSignInManager() 
		{	
			// TODO:
			/*if ( !HasSignInManager() ) {
				return;
			}

			if ( net_headlessServer.GetBool() ) {
				return;
			}
			
			// FIXME: We need to ask the menu system for this info.  Just making a best guess for now
			// (assume we are allowed to join the party as a splitscreen user if we are in the party lobby)
			bool allowJoinParty	= ( localState == STATE_PARTY_LOBBY_HOST || localState == STATE_PARTY_LOBBY_PEER ) && GetPartyLobby().state == idLobby::STATE_IDLE;
			bool allowJoinGame	= ( localState == STATE_GAME_LOBBY_HOST || localState == STATE_GAME_LOBBY_PEER ) && GetGameLobby().state == idLobby::STATE_IDLE;

			bool eitherLobbyRunning	= GetActivePlatformLobby() != NULL && ( GetPartyLobby().IsLobbyActive() || GetGameLobby().IsLobbyActive() );
			bool onlineMatch		= eitherLobbyRunning && MatchTypeIsOnline( GetActivePlatformLobby()->parms.matchFlags );

			//=================================================================================
			// Get the number of desired signed in local users depending on what mode we're in.
			//=================================================================================
			int minDesiredUsers = 0;
			int maxDesiredUsers = Max( 1, signInManager->GetNumLocalUsers() );
	
			if ( si_splitscreen.GetInteger() != 0 ) {
				// For debugging, force 2 splitscreen players
				minDesiredUsers = 2;
				maxDesiredUsers = 2;
				allowJoinGame = true;
			} else if ( onlineMatch || ( eitherLobbyRunning == false ) ) {
				// If this an online game, then only 1 user can join locally.
				// Also, if no sessions are active, remove any extra players.
				maxDesiredUsers = 1;
			} else if ( allowJoinParty || allowJoinGame ) {
				// If we are in the party lobby, allow 2 splitscreen users to join
				maxDesiredUsers = 2;
			}

			// Set the number of desired users
			signInManager->SetDesiredLocalUsers( minDesiredUsers, maxDesiredUsers );
	
			//=================================================================================
			// Update signin manager
			//=================================================================================
	
			// Update signin mgr.  This manager tracks signed in local users, which the session then uses
			// to determine who should be in the lobby.
			signInManager->Pump();
			
			// Get the master local user
			idLocalUser * masterUser = signInManager->GetMasterLocalUser();

			if ( onlineMatch && masterUser != NULL && !masterUser->CanPlayOnline() && !masterUser->HasOwnerChanged() ) { 
				if ( localState > STATE_IDLE ) {
					// User is still valid, just no longer online
					if ( offlineTransitionTimerStart == 0 ) {
						offlineTransitionTimerStart = Sys_Milliseconds();
					}

					if ( ( Sys_Milliseconds() - offlineTransitionTimerStart ) > net_offlineTransitionThreshold.GetInteger() ) {
						MoveToMainMenu();
						common->Dialog().ClearDialogs();
						common->Dialog().AddDialog( GDM_CONNECTION_LOST, DIALOG_ACCEPT, NULL, NULL, false, "", 0, true );
					}
				}
				return;		// Bail out so signInManager->ValidateLocalUsers below doesn't prematurely remove the master user before we can detect loss of connection
			} else {
				offlineTransitionTimerStart = 0;
			}

			// Remove local users (from the signin manager) who aren't allowed to be online if this is an online match.
			// Remove local user (from the signin manager) who are not properly signed into a profile.
			signInManager->ValidateLocalUsers( onlineMatch );

			//=================================================================================
			// Check to see if we need to go to "Press Start"
			//=================================================================================

			// Get the master local user (again, after ValidateOnlineLocalUsers, to make sure he is still valid)
			masterUser = signInManager->GetMasterLocalUser();
	
			if ( masterUser == NULL ) { 
				// If we don't have a master user at all, then we need to be at "Press Start"
				MoveToPressStart( GDM_SP_SIGNIN_CHANGE_POST );
				return;
			} else if ( localState == STATE_PRESS_START ) {


				// If we have a master user, and we are at press start, move to the menu area
				SetState( STATE_IDLE );

			}

			// See if the master user either isn't persistent (but needs to be), OR, if the owner changed
			// RequirePersistentMaster is poorly named, this really means RequireSignedInMaster
			if ( masterUser->HasOwnerChanged() || ( RequirePersistentMaster() && !masterUser->IsProfileReady() ) ) {
				MoveToPressStart( GDM_SP_SIGNIN_CHANGE_POST );
				return;
			}

			//=================================================================================
			// Sync lobby users with the signed in users
			// The initial list of session users are normally determined at connect or create time.
			// These functions allow splitscreen users to join in, or check to see if existing
			// users (including the master) need to be removed.
			//=================================================================================
			GetPartyLobby().SyncLobbyUsersWithLocalUsers( allowJoinParty, onlineMatch );
			GetGameLobby().SyncLobbyUsersWithLocalUsers( allowJoinGame, onlineMatch );
			GetGameStateLobby().SyncLobbyUsersWithLocalUsers( allowJoinGame, onlineMatch );*/
		}
		#endregion

		#region State Management
		#region Properties
		public SessionState State
		{
			get
			{
				// convert our internal state to one of the external states
				switch(_localState)
				{
					case XState.PressStart:
						return SessionState.PressStart;

					case XState.Idle:
						return SessionState.Idle;

					case XState.PartyLobbyHost:
					case XState.PartyLobbyPeer:
						return SessionState.PartyLobby;

					case XState.GameLobbyHost:
					case XState.GameLobbyPeer:
					case XState.GameStateLobbyHost:
					case XState.GameStateLobbyPeer:
						return SessionState.GameLobby;

					case XState.Loading:
						return SessionState.Loading;

					case XState.InGame:
						return SessionState.InGame;

					case XState.CreateAndMoveToPartyLobby:
					case XState.CreateAndMoveToGameLobby:
					case XState.CreateAndMoveToGameStateLobby:
						return SessionState.Connecting;

					case XState.FindOrCreateMatch:
						return SessionState.Searching;

					case XState.ConnectAndMoveToParty:
					case XState.ConnectAndMoveToGame:
					case XState.ConnectAndMoveToGameState:
						return SessionState.Connecting;

					case XState.Busy:
						return SessionState.Busy;

					default:
						idEngine.Instance.Error("idSession::State: unknown state");
						break;
				}

				return SessionState.Idle;
			}
		}
		#endregion

		#region Methods
		public void MoveToPressStart()
		{
			if(_localState != XState.PressStart)
			{
				// TODO: Debug.Assert(_signInManager != null);
				/*signInManager->RemoveAllLocalUsers();
				hasShownVoiceRestrictionDialog = false;*/
		
				MoveToMainMenu();
		
				// TODO: session->FinishDisconnect();

				SetState(XState.PressStart);
			}
		}

		private void MoveToMainMenu() 
		{
			// TODO:
			/*GetPartyLobby().Shutdown();
			GetGameLobby().Shutdown();
			GetGameStateLobby().Shutdown();*/

			SetState(XState.Idle);
		}

		private bool HandleState()
		{
			// TODO:
			// Handle individual lobby states
			/*GetPartyLobby().Pump();
			GetGameLobby().Pump();
			GetGameStateLobby().Pump();*/

			// Let IsHost be authoritative on the qualification of peer/host state types
			/*if ( GetPartyLobby().IsHost() && localState == STATE_PARTY_LOBBY_PEER ) {
				SetState( STATE_PARTY_LOBBY_HOST );
			} else if ( GetPartyLobby().IsPeer() && localState == STATE_PARTY_LOBBY_HOST ) {
				SetState( STATE_PARTY_LOBBY_PEER );
			}

			// Let IsHost be authoritative on the qualification of peer/host state types
			if ( GetGameLobby().IsHost() && localState == STATE_GAME_LOBBY_PEER ) {
				SetState( STATE_GAME_LOBBY_HOST );
			} else if ( GetGameLobby().IsPeer() && localState == STATE_GAME_LOBBY_HOST ) {
				SetState( STATE_GAME_LOBBY_PEER );
			}*/

			switch(_localState) 
			{
				case XState.PressStart:
					return false;

				/*case STATE_IDLE:								HandlePackets(); return false;		// Call handle packets, since packets from old sessions could still be in flight, which need to be emptied
				case STATE_PARTY_LOBBY_HOST:					return State_Party_Lobby_Host();
				case STATE_PARTY_LOBBY_PEER:					return State_Party_Lobby_Peer();
				case STATE_GAME_LOBBY_HOST:						return State_Game_Lobby_Host();
				case STATE_GAME_LOBBY_PEER:						return State_Game_Lobby_Peer();
				case STATE_GAME_STATE_LOBBY_HOST:				return State_Game_State_Lobby_Host();
				case STATE_GAME_STATE_LOBBY_PEER:				return State_Game_State_Lobby_Peer();
				case STATE_LOADING:								return State_Loading();
				case STATE_INGAME:								return State_InGame();
				case STATE_CREATE_AND_MOVE_TO_PARTY_LOBBY:		return State_Create_And_Move_To_Party_Lobby();
				case STATE_CREATE_AND_MOVE_TO_GAME_LOBBY:		return State_Create_And_Move_To_Game_Lobby();
				case STATE_CREATE_AND_MOVE_TO_GAME_STATE_LOBBY:	return State_Create_And_Move_To_Game_State_Lobby();
				case STATE_FIND_OR_CREATE_MATCH:				return State_Find_Or_Create_Match();
				case STATE_CONNECT_AND_MOVE_TO_PARTY:			return State_Connect_And_Move_To_Party();
				case STATE_CONNECT_AND_MOVE_TO_GAME:			return State_Connect_And_Move_To_Game();
				case STATE_CONNECT_AND_MOVE_TO_GAME_STATE:		return State_Connect_And_Move_To_Game_State();
				case STATE_BUSY:	return State_Busy();*/

				default:
					idEngine.Instance.Error("HandleState:  Unknown state in idSession");
					break;
			}

			return false;
		}

		private void SetState(XState newState)
		{
			if((int) newState == (int) _localState)
			{
				idLog.Warning("NET_VERBOSE_PRINT( \"NET: SetState: State SAME %s\n\", stateToString[ newState ] );");
				return;
			}

			// Set the current state
			idLog.Warning("TODO: NET_VERBOSE_PRINT( \"NET: SetState: State changing from %s to %s\n\", stateToString[ localState ], stateToString[ newState ] );");

			if((_localState < XState.Loading) && (newState >= XState.Loading))
			{
				// tell lobby instances that the match has started
				idLog.Warning("TODO: StartSessions();");

				// clear certain dialog boxes we don't want to see in-game
				idLog.Warning("TODO: common->Dialog().ClearDialog( GDM_LOBBY_DISBANDED );");	// the lobby you were previously in has disbanded
			}
			else if((_localState >= XState.Loading) && (newState < XState.Loading))
			{
				// Tell lobby instances that the match has ended
				idLog.Warning("TODO: end match");
				/*if ( !WasMigrationGame() ) { // Don't end the session if we are going right back into the game
					EndSessions();
				}*/
			}

			if((newState == XState.GameLobbyHost) || (newState == XState.GameLobbyPeer))
			{
				idLog.Warning("TODO: ComputeNextGameCoalesceTime();");
			}

			_localState = newState;
		}
		#endregion
		#endregion
	}

	public enum State
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

	public enum SessionState
	{
		PressStart,
		Idle,
		Searching,
		Connecting,
		PartyLobby,
		GameLobby,
		Loading,
		InGame,
		Busy
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

	public enum VoiceStateDisplay
	{
		None,
		NotTalking,
		Talking,
		TalkingGlobal,
		Muted
	}
}