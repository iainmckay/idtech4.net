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
using idTech4.Renderer;
using idTech4.UI.SWF;

namespace idTech4.Game.Menus
{
	public class idMenuHandler_Shell : idMenuHandler
	{
		#region Members
		private ShellState _state;
		private ShellState _nextState;

		private bool _smallFrameShowing;
		private bool _largeFrameShowing;
		private bool _backgroundShowing;
		private bool _waitForBinding;
		private string _waitBind;
		//idSysSignal				deviceRequestedSignal;
		
		// TODO:
		/*idList<const char *, TAG_IDLIB_LIST_MENU>	mpGameModes;
		idList<mpMap_t, TAG_IDLIB_LIST_MENU>			mpGameMaps;
		idMenuWidget_MenuBar *	menuBar;
		idMenuWidget *			pacifier;*/
	
		private int	_timeRemaining;
		private int	_nextPeerUpdateMs;
		private int	_newGameType;
		private bool _inGame;
		private bool _showingIntro;
		private bool _continueWaitForEnumerate;
		private bool _gameComplete;

		//private idSWF _introGui;
		// TODO: const idSoundShader *	typeSoundShader;
		private idMaterial _doom3Intro;
		private idMaterial _roeIntro;
		private idMaterial _lmIntro;
		private idMaterial _marsRotation;
		// TODO: idList< idStr, TAG_IDLIB_LIST_MENU>			navOptions;
		#endregion

		#region Constructor
		public idMenuHandler_Shell()
			: base()
		{
			_state             = ShellState.Invalid;
			_nextState         = ShellState.Invalid;
			_backgroundShowing = true;
		}
		#endregion

		#region State
		#region Properties
		public bool IsInGame
		{
			get
			{
				return _inGame;
			}
			set
			{
				_inGame = value;
			}
		}

		public ShellState State
		{
			get
			{
				return _state;
			}
			set
			{
				_nextState = value;
			}
		}
		#endregion

		#region Methods
		public override void ActivateMenu(bool show)
		{
			if((show == true) && (_gui != null) && (_gui.IsActive == true))
			{
				return;
			}
			else if((show == false) && (_gui != null) && (_gui.IsActive == false))
			{
				return;
			}

			if(_inGame == true)
			{
				idLog.Warning("TODO: ActivateMenu ingame");
	
				/*idPlayer * player = gameLocal.GetLocalPlayer();
				if ( player != NULL ) {
					if ( !show ) {
						bool isDead = false;			
						if ( player->health <= 0 ) {
							isDead = true;
						}
			
						if ( isDead && !common->IsMultiplayer() ) {
							return;
						}
					}
				}*/
			}
	
			base.ActivateMenu(show);

			if(show == true)
			{
				if(_inGame == false)
				{
					idLog.Warning("TODO: PlaySound( GUI_SOUND_MUSIC );");

					if(_gui != null)
					{
						idLog.Warning("TODO: idSWFSpriteInstance mars = _gui.RootObject.GetNestedSprite(\"mars\");");
						idSWFSpriteInstance mars = null;

						if(mars != null)
						{
							idLog.Warning("TODO: ActivateMenu mars");
				
							/*mars->stereoDepth = STEREO_DEPTH_TYPE_FAR;

							idSWFSpriteInstance * planet = mars->GetScriptObject()->GetNestedSprite( "planet" );

							if ( marsRotation != NULL && planet != NULL ) {
								const idMaterial * mat = marsRotation;
								if ( mat != NULL ) {
									int c = mat->GetNumStages();
									for ( int i = 0; i < c; i++ ) {
										const shaderStage_t *stage = mat->GetStage( i );
										if ( stage != NULL && stage->texture.cinematic ) {
											stage->texture.cinematic->ResetTime( Sys_Milliseconds() );
										}
									}
								}

								planet->SetMaterial( mat );
							}*/
						}
					}
				}

				idLog.Warning("TODO: SetupPCOptions();");
		
				idLog.Warning("TODO: ActivateMenu cmdBar");
				
				/*if ( cmdBar != NULL ) {
					cmdBar->ClearAllButtons();
					cmdBar->Update();
				}*/
			}
			else
			{
				idLog.Warning("TODO: ClearWidgetActionRepeater();");
				/*nextScreen = SHELL_AREA_INVALID;
				activeScreen = SHELL_AREA_INVALID;
				nextState = SHELL_STATE_INVALID;*/

				_state             = ShellState.Invalid;
				_smallFrameShowing = false;
				_largeFrameShowing = false;
				_backgroundShowing = true;

				idLog.Warning("TODO: common->Dialog().ClearDialog( GDM_LEAVE_LOBBY_RET_NEW_PARTY );");
			}
		}
		#endregion
		#endregion
	}

	public enum ShellState
	{
		Invalid = -1,
		PressStart,
		Idle,
		PartyLobby,
		GameLobby,
		Paused,
		Connecting,
		Searching,
		Loading,
		Busy,
		InGame
	}
}