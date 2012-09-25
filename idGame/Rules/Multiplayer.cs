using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Game.Entities;

namespace idTech4.Game.Rules
{
	public abstract class Multiplayer : idGameRules
	{
		#region Constants
		public const int ChatNotifyCount = 5;
		#endregion

		#region Properties
		public MultiplayerGameState State
		{
			get
			{
				return _state;
			}
		}
		#endregion

		#region Members
		private MultiplayerGameState _state = MultiplayerGameState.InActive;

		private MultiplayerChatLine[] _chatHistory = new MultiplayerChatLine[ChatNotifyCount];
		private int _chatHistoryIndex;
		private int _chatHistorySize;
		private bool _chatDataUpdated;

		private int _lastChatLineTime;
		#endregion

		#region Constructor
		public Multiplayer()
			: base()
		{
			/*scoreBoard = NULL;
			spectateGui = NULL;
			guiChat = NULL;
			mainGui = NULL;
			mapList = NULL;
			msgmodeGui = NULL;
			lastGameType = GAME_SP;*/
			Clear();
		}
		#endregion

		#region Methods
		public void Clear()
		{
			_state = MultiplayerGameState.InActive;
			/*int i;

			gameState = INACTIVE;
			nextState = INACTIVE;
			pingUpdateTime = 0;
			vote = VOTE_NONE;
			voteTimeOut = 0;
			voteExecTime = 0;
			nextStateSwitch = 0;
			matchStartedTime = 0;
			currentTourneyPlayer[0] = -1;
			currentTourneyPlayer[1] = -1;
			one = two = three = false;
			
			lastWinner = -1;
			currentMenu = 0;
			bCurrentMenuMsg = false;
			nextMenu = 0;
			pureReady = false;
			scoreBoard = NULL;
			spectateGui = NULL;
			guiChat = NULL;
			mainGui = NULL;
			msgmodeGui = NULL;
			if(mapList)
			{
				uiManager->FreeListGUI(mapList);
				mapList = NULL;
			}
			fragLimitTimeout = 0;
			memset(&switchThrottle, 0, sizeof(switchThrottle));
			voiceChatThrottle = 0;*/

			for(int i = 0; i < ChatNotifyCount; i++)
			{
				_chatHistory[i].Line = string.Empty;
			}

			/*warmupText.Clear();
			voteValue.Clear();
			voteString.Clear();
			startFragLimit = -1;*/
		}

		public void AddChatLine(string format, params object[] args)
		{
			idConsole.WriteLine(format, args);

			_chatHistory[_chatHistoryIndex % ChatNotifyCount].Line = string.Format(format, args);
			_chatHistory[_chatHistoryIndex % ChatNotifyCount].Fade = 6;

			_chatHistoryIndex++;

			if(_chatHistorySize < ChatNotifyCount)
			{
				_chatHistorySize++;
			}

			_chatDataUpdated = true;
			_lastChatLineTime = idR.Game.Time;
		}

		private void UpdatePlayerSkin(idPlayer player, bool restart)
		{
			if(restart == true)
			{
				player.Team = (player.Info.GetString("ui_team").ToLower() == "blue") ? PlayerTeam.Blue : PlayerTeam.Red;
			}

			player.BaseSkin = this.GetPlayerSkin(player);

			if(player.BaseSkin == string.Empty)
			{
				player.BaseSkin = "skins/characters/player/marine_mp";
			}

			player.Skin = idR.DeclManager.FindSkin(player.BaseSkin, false);

			// match the skin to a color band for scoreboard
			if(player.BaseSkin.Contains("red") == true)
			{
				player.ColorBarIndex = 1;
			}
			else if(player.BaseSkin.Contains("green") == true)
			{
				player.ColorBarIndex = 2;
			}
			else if(player.BaseSkin.Contains("blue") == true)
			{
				player.ColorBarIndex = 3;
			}
			else if(player.BaseSkin.Contains("yellow") == true)
			{
				player.ColorBarIndex = 4;
			}
			else
			{
				player.ColorBarIndex = 0;
			}

			player.ColorBar = idPlayer.ColorBarTable[player.ColorBarIndex];

			idConsole.Warning("TODO: powerup active");
			/*if(PowerUpActive(BERSERK))
			{
				powerUpSkin = declManager->FindSkin(baseSkinName + "_berserk");
			}*/
		}

		protected virtual string GetPlayerSkin(idPlayer player)
		{
			return player.Info.GetString("ui_skin");
		}
		#endregion

		#region idGameType implementation
		public override void EnterGame(int clientIndex)
		{
			if(idR.Game.PlayerStates[clientIndex].InGame == false)
			{
				idR.Game.PlayerStates[clientIndex].InGame = true;

				// can't use PrintMessageEvent as clients don't know the nickname yet
				idR.Game.ServerSendChatMessage(-1, idR.Language.Get("#str_02047"), string.Format(idR.Language.Get("#str_07177"), idR.Game.UserInfo[clientIndex].GetString("ui_name")));
			}
		}

		public override void Run()
		{
			idConsole.Warning("TODO: multiplayer run");
			base.Run();

			/*	int i, timeLeft;
	idPlayer *player;
	int gameReviewPause;

	assert( gameLocal.isMultiplayer );
	assert( !gameLocal.isClient );

	pureReady = true;

	if ( gameState == INACTIVE ) {
		lastGameType = gameLocal.gameType;
		NewState( WARMUP );
	}

	CheckVote();

	CheckRespawns();

	if ( nextState != INACTIVE && gameLocal.time > nextStateSwitch ) {
		NewState( nextState );
		nextState = INACTIVE;
	}

	// don't update the ping every frame to save bandwidth
	if ( gameLocal.time > pingUpdateTime ) {
		for ( i = 0; i < gameLocal.numClients; i++ ) {
			playerState[i].ping = networkSystem->ServerGetClientPing( i );
		}
		pingUpdateTime = gameLocal.time + 1000;
	}

	warmupText = "";

	switch( gameState ) {
		case GAMEREVIEW: {
			if ( nextState == INACTIVE ) {
				gameReviewPause = cvarSystem->GetCVarInteger( "g_gameReviewPause" );
				nextState = NEXTGAME;
				nextStateSwitch = gameLocal.time + 1000 * gameReviewPause;
			}
			break;
		}
		case NEXTGAME: {
			if ( nextState == INACTIVE ) {
				// game rotation, new map, gametype etc.
				if ( gameLocal.NextMap() ) {
					cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "serverMapRestart\n" );
					return;
				}
				NewState( WARMUP );
				if ( gameLocal.gameType == GAME_TOURNEY ) {
					CycleTourneyPlayers();
				}
				// put everyone back in from endgame spectate
				for ( i = 0; i < gameLocal.numClients; i++ ) {
					idEntity *ent = gameLocal.entities[ i ];
					if ( ent && ent->IsType( idPlayer::Type ) ) {
						if ( !static_cast< idPlayer * >( ent )->wantSpectate ) {
							CheckRespawns( static_cast<idPlayer *>( ent ) );
						}
					}
				}
			}
			break;
		}
		case WARMUP: {
			if ( AllPlayersReady() ) {
				NewState( COUNTDOWN );
				nextState = GAMEON;
				nextStateSwitch = gameLocal.time + 1000 * cvarSystem->GetCVarInteger( "g_countDown" );
			}
			warmupText = "Warming up.. waiting for players to get ready";
			one = two = three = false;
			break;
		}
		case COUNTDOWN: {
			timeLeft = ( nextStateSwitch - gameLocal.time ) / 1000 + 1;
			if ( timeLeft == 3 && !three ) {
				PlayGlobalSound( -1, SND_THREE );
				three = true;
			} else if ( timeLeft == 2 && !two ) {
				PlayGlobalSound( -1, SND_TWO );
				two = true;
			} else if ( timeLeft == 1 && !one ) {
				PlayGlobalSound( -1, SND_ONE );
				one = true;
			}
			warmupText = va( "Match starts in %i", timeLeft );
			break;
		}
		case GAMEON: {
			player = FragLimitHit();
			if ( player ) {
				// delay between detecting frag limit and ending game. let the death anims play
				if ( !fragLimitTimeout ) {
					common->DPrintf( "enter FragLimit timeout, player %d is leader\n", player->entityNumber );
					fragLimitTimeout = gameLocal.time + FRAGLIMIT_DELAY;
				}
				if ( gameLocal.time > fragLimitTimeout ) {
					NewState( GAMEREVIEW, player );
					PrintMessageEvent( -1, MSG_FRAGLIMIT, player->entityNumber );
				}
			} else {
				if ( fragLimitTimeout ) {
					// frag limit was hit and cancelled. means the two teams got even during FRAGLIMIT_DELAY
					// enter sudden death, the next frag leader will win
					SuddenRespawn();
					PrintMessageEvent( -1, MSG_HOLYSHIT );
					fragLimitTimeout = 0;
					NewState( SUDDENDEATH );
				} else if ( TimeLimitHit() ) {
					player = FragLeader();
					if ( !player ) {
						NewState( SUDDENDEATH );
					} else {
						NewState( GAMEREVIEW, player );
						PrintMessageEvent( -1, MSG_TIMELIMIT );
					}
				}
			}
			break;
		}
		case SUDDENDEATH: {
			player = FragLeader();
			if ( player ) {
				if ( !fragLimitTimeout ) {
					common->DPrintf( "enter sudden death FragLeader timeout, player %d is leader\n", player->entityNumber );
					fragLimitTimeout = gameLocal.time + FRAGLIMIT_DELAY;
				}
				if ( gameLocal.time > fragLimitTimeout ) {
					NewState( GAMEREVIEW, player );
					PrintMessageEvent( -1, MSG_FRAGLIMIT, player->entityNumber );
				}
			} else if ( fragLimitTimeout ) {
				SuddenRespawn();
				PrintMessageEvent( -1, MSG_HOLYSHIT );
				fragLimitTimeout = 0;
			}
			break;
		}
	}*/
		}

		public override bool Draw(int clientIndex)
		{
			idConsole.Warning("TODO: multiplayer draw");
			return base.Draw(clientIndex);
			/*
			if(base.Draw(clientIndex) == false)
			{
				return false;
			}

			Player player = null;
			Player viewPlayer = null;

			// clear the render entities for any players that don't need
			// icons and which might not be thinking because they weren't in
			// the last snapshot.
			for(int i = 0; i < idR.Game.ClientCount; i++)
			{
				Player player = idR.Game.Entities[i] as Player;

				if((player != null) && (player.NeedsIcons == false))
				{
					player.HidePlayerIcons = true;
				}
			}

			player = viewPlayer = idR.Game.Entities[clientIndex] as Player;

			if(player == null)
			{
				return false;
			}

			if(player.IsSpectating == true)
			{
				viewPlayer = idR.Game.Entities[player.Spectator] as Player;

				if(viewPlayer == null)
				{
					return false;
				}
			}

			// TODO: UpdatePlayerRanks();
			UpdateHud(viewPlayer, player.Hud);

			// use the hud of the local player
			viewPlayer.View.RenderPlayerView(player.Hud);
	
	/*if ( currentMenu ) {
#if 0
		// uncomment this if you want to track when players are in a menu
		if ( !bCurrentMenuMsg ) {
			idBitMsg	outMsg;
			byte		msgBuf[ 128 ];

			outMsg.Init( msgBuf, sizeof( msgBuf ) );
			outMsg.WriteByte( GAME_RELIABLE_MESSAGE_MENU );
			outMsg.WriteBits( 1, 1 );
			networkSystem->ClientSendReliableMessage( outMsg );

			bCurrentMenuMsg = true;
		}
#endif
		if ( player->wantSpectate ) {
			mainGui->SetStateString( "spectext", common->GetLanguageDict()->GetString( "#str_04249" ) );
		} else {
			mainGui->SetStateString( "spectext", common->GetLanguageDict()->GetString( "#str_04250" ) );
		}
		DrawChat();
		if ( currentMenu == 1 ) {
			UpdateMainGui();
			mainGui->Redraw( gameLocal.time );
		} else {
			msgmodeGui->Redraw( gameLocal.time );
		}
	} else {
#if 0
		// uncomment this if you want to track when players are in a menu
		if ( bCurrentMenuMsg ) {
			idBitMsg	outMsg;
			byte		msgBuf[ 128 ];

			outMsg.Init( msgBuf, sizeof( msgBuf ) );
			outMsg.WriteByte( GAME_RELIABLE_MESSAGE_MENU );
			outMsg.WriteBits( 0, 1 );
			networkSystem->ClientSendReliableMessage( outMsg );

			bCurrentMenuMsg = false;
		}
#endif
		if ( player->spectating ) {
			idStr spectatetext[ 2 ];
			int ispecline = 0;
			if ( gameLocal.gameType == GAME_TOURNEY ) {
				if ( !player->wantSpectate ) {
					spectatetext[ 0 ] = common->GetLanguageDict()->GetString( "#str_04246" );
					switch ( player->tourneyLine ) {
						case 0:
							spectatetext[ 0 ] += common->GetLanguageDict()->GetString( "#str_07003" );
							break;
						case 1:
							spectatetext[ 0 ] += common->GetLanguageDict()->GetString( "#str_07004" );
							break;
						case 2:
							spectatetext[ 0 ] += common->GetLanguageDict()->GetString( "#str_07005" );
							break;
						default:
							spectatetext[ 0 ] += va( common->GetLanguageDict()->GetString( "#str_07006" ), player->tourneyLine );
							break;
					}
					ispecline++;
				}
			} else if ( gameLocal.gameType == GAME_LASTMAN ) {
				if ( !player->wantSpectate ) {
					spectatetext[ 0 ] = common->GetLanguageDict()->GetString( "#str_07007" );
					ispecline++;
				}
			}
			if ( player->spectator != player->entityNumber ) {
				spectatetext[ ispecline ] = va( common->GetLanguageDict()->GetString( "#str_07008" ), viewPlayer->GetUserInfo()->GetString( "ui_name" ) );
			} else if ( !ispecline ) {
				spectatetext[ 0 ] = common->GetLanguageDict()->GetString( "#str_04246" );
			}
			spectateGui->SetStateString( "spectatetext0", spectatetext[0].c_str() );
			spectateGui->SetStateString( "spectatetext1", spectatetext[1].c_str() );
			if ( vote != VOTE_NONE ) {
				spectateGui->SetStateString( "vote", va( "%s (y: %d n: %d)", voteString.c_str(), (int)yesVotes, (int)noVotes ) );
			} else {
				spectateGui->SetStateString( "vote", "" );
			}
			spectateGui->Redraw( gameLocal.time );
		}
		DrawChat();
		DrawScoreBoard( player );
	}*/

			//return true;
		}

		public override bool UserInfoChanged(int clientIndex, bool canModify)
		{
			bool modifiedInfo = base.UserInfoChanged(clientIndex, canModify);

			idPlayer player = (idPlayer) idR.Game.Entities[clientIndex];
			bool spectate = (player.Info.GetString("ui_spectate").ToLower() == "spectate");

			if(idR.Game.ServerInfo.GetBool("si_spectators") == true)
			{
				// never let spectators go back to game while sudden death is on
				if((canModify == true) && (this.State == MultiplayerGameState.SuddenDeath) && (spectate == false) && (player.WantToSpectate == true))
				{
					player.Info.Set("ui_spectate", "Spectate");
					modifiedInfo |= true;
				}
				else
				{
					if((spectate != player.WantToSpectate) && (spectate == false))
					{
						// returning from spectate, set forceRespawn so we don't get stuck in spectate forever
						player.ForceRespawn = true;
					}

					player.WantToSpectate = spectate;
				}
			}
			else
			{
				if((canModify == true) && (spectate == true))
				{
					player.Info.Set("ui_spectate", "Play");
					modifiedInfo |= true;
				}
				else if(player.IsSpectating)
				{
					// allow player to leaving spectator mode if they were in it when si_spectators got turned off
					player.ForceRespawn = true;
				}

				player.WantToSpectate = false;
			}

			bool newReady = (player.Info.GetString("ui_ready").ToLower() == "ready");

			if((player.IsReady != newReady) && (this.State == MultiplayerGameState.WarmUp) && (player.WantToSpectate == false))
			{
				this.AddChatLine(idR.Language.Get("#str_07180"), player.Info.GetString("ui_name"), (newReady == true) ? idR.Language.Get("#str_04300") : idR.Language.Get("#str_04301"));
			}

			player.IsReady = newReady;
			player.IsChatting = player.Info.GetBool("ui_chat", false);

			UpdatePlayerSkin(player, false);
			
			if((canModify == true) && (player.IsChatting == true) /* TODO: && AI_DEAD*/)
			{
				// if dead, always force chat icon off.
				player.IsChatting = false;
				player.Info.Set("ui_chat", false);

				modifiedInfo |= true;
			}

			return modifiedInfo;
		}

		public override void SpawnPlayer(int clientIndex)
		{
			base.SpawnPlayer(clientIndex);

			idPlayer player = (idPlayer) idR.Game.Entities[clientIndex];
			player.SpawnedTime = idR.Game.Time;
		}

		public override void MapPopulate()
		{
			idR.CvarSystem.SetBool("r_skipSpecular", false);

			base.MapPopulate();
		}

		public override void Reset()
		{
			base.Reset();

			Clear();

			idConsole.Warning("TODO: multiplayer rest");
			/*
			memset(&playerState, 0, sizeof(playerState)); 
			assert( !scoreBoard && !spectateGui && !guiChat && !mainGui && !mapList );
			scoreBoard = uiManager->FindGui( "guis/scoreboard.gui", true, false, true );
			spectateGui = uiManager->FindGui( "guis/spectate.gui", true, false, true );
			guiChat = uiManager->FindGui( "guis/chat.gui", true, false, true );
			mainGui = uiManager->FindGui( "guis/mpmain.gui", true, false, true );
			mapList = uiManager->AllocListGUI( );
			mapList->Config( mainGui, "mapList" );
			// set this GUI so that our Draw function is still called when it becomes the active/fullscreen GUI
			mainGui->SetStateBool( "gameDraw", true );
			mainGui->SetKeyBindingNames();
			mainGui->SetStateInt( "com_machineSpec", cvarSystem->GetCVarInteger( "com_machineSpec" ) );
			SetMenuSkin();
			msgmodeGui = uiManager->FindGui( "guis/mpmsgmode.gui", true, false, true );
			msgmodeGui->SetStateBool( "gameDraw", true );
			ClearGuis();
			ClearChatData();
			warmupEndTime = 0;*/
		}

		public override void Precache()
		{
			base.Precache();

			idR.Game.FindEntityDefDict("player_doommarine", false);

			// skins
			foreach(string skin in idR.CvarSystem.GetString("mod_validSkins").Split(';'))
			{
				idR.DeclManager.FindSkin(skin, false);
			}

			foreach(string skin in idStrings.Skins)
			{
				idR.DeclManager.FindSkin(skin, false);
			}

			// mp game sounds
			idConsole.Warning("TODO: mp game sounds");
			/*foreach(string sound in idStrings.GlobalSoundStrings)
			{
				idFile f = idR.FileSystem.OpenFileRead(sound);

				if(f != null)
				{
					idR.FileSystem.CloseFile(f);
				}
			}*/

			// mp guis. just make sure we hit all of them
			foreach(string iface in idStrings.MultiplayerInterfaces)
			{
				idR.UIManager.FindInterface(iface, true);
			}
		}
		#endregion
	}

	public struct MultiplayerChatLine
	{
		public string Line;
		public short Fade; // starts high and decreases, line is removed once reached 0
	}

	public enum MultiplayerGameState
	{
		/// <summary>
		/// Not running.
		/// </summary>
		InActive = 0,

		/// <summary>
		/// Warming up.
		/// </summary>
		WarmUp,

		/// <summary>
		/// Post warmup pre-game.
		/// </summary>
		CountDown,

		/// <summary>
		/// Game is on.
		/// </summary>
		GameOn,

		/// <summary>
		/// Game is on but in sudden death, first frag wins.
		/// </summary>
		SuddenDeath,

		/// <summary>
		/// Game is over, scoreboard is up.  We wait si_gameReviewPause seconds (which has a min value).
		/// </summary>
		GameReview,

		NextGame,
		StateCount
	}
}
