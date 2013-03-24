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
using idTech4.Game.Menus;
using idTech4.Services;
using idTech4.Text;
using idTech4.UI.SWF;

namespace idTech4.Game
{
	public class idGame : IGame
	{
		#region Constants
		/// <summary>
		/// The "gameversion" client command will print this plus compile date.
		/// </summary>
		public const string Version = "baseDOOM-1";
		#endregion

		#region Members
		private GameState _gameState;

		private idMenuHandler_Shell _shellHandler;
		#endregion

		#region Constructor
		public idGame()
		{
			Clear();
		}
		#endregion

		#region Initialization
		private void Clear()
		{
			_gameState = GameState.Uninitialized;

			/*int i;

			serverInfo.Clear();
			numClients = 0;
			for ( i = 0; i < MAX_CLIENTS; i++ ) {
				persistentPlayerInfo[i].Clear();
			}
			memset( entities, 0, sizeof( entities ) );
			memset( spawnIds, -1, sizeof( spawnIds ) );
			firstFreeEntityIndex[0] = 0;
			firstFreeEntityIndex[1] = ENTITYNUM_FIRST_NON_REPLICATED;
			num_entities = 0;
			spawnedEntities.Clear();
			activeEntities.Clear();
			numEntitiesToDeactivate = 0;
			sortPushers = false;
			sortTeamMasters = false;
			persistentLevelInfo.Clear();
			memset( globalShaderParms, 0, sizeof( globalShaderParms ) );
			random.SetSeed( 0 );
			world = NULL;
			frameCommandThread = NULL;
			testmodel = NULL;
			testFx = NULL;
			clip.Shutdown();
			pvs.Shutdown();
			sessionCommand.Clear();
			locationEntities = NULL;
			smokeParticles = NULL;
			editEntities = NULL;
			entityHash.Clear( 1024, MAX_GENTITIES );
			inCinematic = false;
			framenum = 0;
			previousTime = 0;
			time = 0;
			vacuumAreaNum = 0;
			mapFileName.Clear();
			mapFile = NULL;
			spawnCount = INITIAL_SPAWN_COUNT;
			mapSpawnCount = 0;
			camera = NULL;
			aasList.Clear();
			aasNames.Clear();
			lastAIAlertEntity = NULL;
			lastAIAlertTime = 0;
			spawnArgs.Clear();
			gravity.Set( 0, 0, -1 );
			playerPVS.h = (unsigned int)-1;
			playerConnectedAreas.h = (unsigned int)-1;*/
			/*influenceActive = false;

			realClientTime = 0;
			isNewFrame = true;
			clientSmoothing = 0.1f;
			entityDefBits = 0;

			nextGibTime = 0;
			globalMaterial = NULL;
			newInfo.Clear();
			lastGUIEnt = NULL;
			lastGUI = 0;

			eventQueue.Init();
			savedEventQueue.Init();*/
	
			_shellHandler = null;
			/*selectedGroup = 0;
			portalSkyEnt			= NULL;
			portalSkyActive			= false;

			ResetSlowTimeVars();

			lastCmdRunTimeOnClient.Zero();
			lastCmdRunTimeOnServer.Zero();*/
		}

		/// <summary>
		/// Initialize the game for the first time.
		/// </summary>
		public void Init()
		{
			ICVarSystem cvarSystem   = idEngine.Instance.GetService<ICVarSystem>();
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();
			ICommandSystem cmdSystem = idEngine.Instance.GetService<ICommandSystem>();

			// we're using SWF's for this game
			idSWFManager swfManager  = new idSWFManager();
			swfManager.Initialize();

			idEngine.Instance.Services.AddService(typeof(idSWFManager), swfManager);

			// initialize processor specific SIMD
			idLog.Warning("TODO: idSIMD::InitProcessor( game, com_forceGenericSIMD.GetBool() );");

			idLog.WriteLine("--------- Initializing Game ----------");
			idLog.WriteLine("gamename: {0}", Version);
			// TODO: idLog.WriteLine("gamedate: {0}", __DATE__ );

			// register game specific decl types
			// TODO: declManager.RegisterDeclType("model",   DeclType.ModelDef,    new idDeclAllocator<idDeclModel>());
			declManager.RegisterDeclType("export",  DeclType.ModelExport, new idDeclAllocator<idDecl>());

			// register game specific decl folders
			declManager.RegisterDeclFolder("def",       ".def", DeclType.EntityDef);
			declManager.RegisterDeclFolder("fx",        ".fx",  DeclType.Fx);
			declManager.RegisterDeclFolder("particles", ".prt", DeclType.Particle);
			declManager.RegisterDeclFolder("af",        ".af",  DeclType.ArticulatedFigure);
			declManager.RegisterDeclFolder("newpdas",   ".pda", DeclType.Pda);

			Clear();

			_shellHandler = new idMenuHandler_Shell();

			if(cvarSystem.GetBool("g_xp_bind_run_once") == false)
			{
				// the default config file contains remapped controls that support the XP weapons.
				// we want to run this once after the base doom config file has run so we can
				// have the correct xp binds
				cmdSystem.BufferCommandText("exec default.cfg");
				cmdSystem.BufferCommandText("seta g_xp_bind_run_once 1");
				cmdSystem.ExecuteCommandBuffer();
			}

			// load default scripts
			idLog.Warning("TODO: program.Startup( SCRIPT_DEFAULT );");
	
			idLog.Warning("TODO: smokeParticles = new (TAG_PARTICLE) idSmokeParticles;");

			// set up the aas
			// TODO: aas
			/*dict = FindEntityDefDict( "aas_types" );
			if ( dict == NULL ) {
				Error( "Unable to find entityDef for 'aas_types'" );
				return;
			}

			// allocate space for the aas
			const idKeyValue *kv = dict->MatchPrefix( "type" );
			while( kv != NULL ) {
				aas = idAAS::Alloc();
				aasList.Append( aas );
				aasNames.Append( kv->GetValue() );
				kv = dict->MatchPrefix( "type", kv );
			}*/

			_gameState = GameState.NoMap;

			// TODO: Printf( "...%d aas types\n", aasList.Num() );
			idLog.WriteLine("game initialized.");
			idLog.WriteLine("--------------------------------------");
		}
		#endregion

		#region Main Menu
		public void Shell_CreateMenu(bool inGame)
		{
			Shell_ResetMenu();

			if(_shellHandler != null)
			{
				if(inGame == false)
				{
					_shellHandler.IsInGame = false;

					Shell_Init("shell" /* TODO: common->MenuSW()*/);
				} 
				else 
				{
					_shellHandler.IsInGame = true;

					// TODO: multiplayer
					/*if ( common->IsMultiplayer() ) {
						Shell_Init( "pause", common->SW() );
					} else {*/
						Shell_Init("pause" /* TODO: , common->MenuSW()*/);
					/*}*/
				}
			}
		}

		public bool Shell_IsActive()
		{
			if(_shellHandler != null)
			{
				return _shellHandler.IsActive;
			}

			return false;
		}

		public void Shell_Show(bool show)
		{
			if(_shellHandler != null)
			{
				_shellHandler.ActivateMenu(show);
			}
		}

		public void Shell_SyncWithSession()
		{
			if(_shellHandler == null)
			{
				return;
			}

			ISession session = idEngine.Instance.GetService<ISession>();

			switch(session.State)
			{
				case SessionState.PressStart:
					_shellHandler.State = ShellState.PressStart;
					break;

				case SessionState.InGame:
					_shellHandler.State = ShellState.Paused;
					break;

				case SessionState.Idle:
					_shellHandler.State = ShellState.Idle;
					break;

				case SessionState.PartyLobby:
					_shellHandler.State = ShellState.PartyLobby;
					break;

				case SessionState.GameLobby:
					_shellHandler.State = ShellState.GameLobby;
					break;

				case SessionState.Searching:
					_shellHandler.State = ShellState.Searching;
					break;

				case SessionState.Loading:
					_shellHandler.State = ShellState.Loading;
					break;

				case SessionState.Connecting:
					_shellHandler.State = ShellState.Connecting;
					break;

				case SessionState.Busy:
					_shellHandler.State = ShellState.Busy;
					break;
			}
		}

		public void Shell_ResetMenu()
		{
			if(_shellHandler != null)
			{
				// TODO: _shellHandler.Dispose();
				_shellHandler = new idMenuHandler_Shell();
			}
		}

		public void Shell_Init(string fileName)
		{
			if(_shellHandler != null)
			{
				_shellHandler.Init(fileName/* TODO:, sw*/);
			}
		}
		#endregion
	}

	public enum GameState
	{
		/// <summary>
		/// Prior to Init being called.
		/// </summary>
		Uninitialized,
		/// <summary>
		/// No map loaded.
		/// </summary>
		NoMap,
		/// <summary>
		/// Inside InitFromNewMap().  Spawning map entities.
		/// </summary>
		Startup,
		/// <summary>
		/// Normal gameplay.
		/// </summary>
		Active,
		/// <summary>
		/// Inside MapShutdown().  Clearing memory.
		/// </summary>
		Shutdown
	}
}