using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Microsoft.Xna.Framework;

using idTech4.Game.Animation;
using idTech4.Game.Entities;
using idTech4.Game.Physics;
using idTech4.Game.Rules;
using idTech4.Input;
using idTech4.Renderer;
using idTech4.Sound;
using idTech4.Text;
using idTech4.Text.Decl;
using idTech4.UI;

namespace idTech4.Game
{
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
	}

	public class idGame : idBaseGame
	{
		#region Constants
		public const string GameVersion = "sharp";
		public const float DefaultGravity = 1066.0f;
		public const int InitialSpawnCount = 1;
		public const int MultiplayerMaxFrags = 100;
		#endregion

		#region Properties
		public int ClientCount
		{
			get
			{
				return _clientCount;
			}
		}

		// TODO
		public idClip Clip
		{
			get
			{
				return _clip;
			}
		}

		public idEntity[] Entities
		{
			get
			{
				return _entities;
			}
		}

		public Vector3 Gravity
		{
			get
			{
				return _gravity;
			}
		}

		/// <summary>
		/// Is the game run for a client.
		/// </summary>
		/// <remarks>
		/// Discriminates between the RunFrame path and the ClientPrediction path
		/// NOTE: on a listen server, isClient is false.
		/// </remarks>
		public bool IsClient
		{
			get
			{
				return _isClient;
			}
		}

		/// <summary>
		/// Is the game running in multiplayer mode?
		/// </summary>
		public bool IsMultiplayer
		{
			get
			{
				return _isMultiplayer;
			}
		}

		/// <summary>
		/// Is the game running for a dedicated or listen server?
		/// </summary>
		public bool IsServer
		{
			get
			{
				return _isServer;
			}
		}

		/// <summary>
		/// Number of the local client. MP: -1 on a dedicated.
		/// </summary>
		public int LocalClientIndex
		{
			get
			{
				return _localClientIndex;
			}
		}

		/// <summary>
		/// Nothing in the game tic should EVER make a decision based on what the
		/// local client number is, it shouldn't even be aware that there is a
		/// draw phase even happening.  This just returns client 0, which will
		/// be correct for single player.
		/// </summary>
		public idPlayer LocalPlayer
		{
			get
			{
				if(_localClientIndex < 0)
				{
					return null;
				}

				if((_entities[_localClientIndex] == null) || ((_entities[_localClientIndex] is idPlayer) == false))
				{
					// not fully in game yet
					return null;
				}

				return (idPlayer) _entities[_localClientIndex];
			}
		}

		public string MapName
		{
			get
			{
				return _currentMapFileName;
			}
		}

		public PlayerState[] PlayerStates
		{
			get
			{
				return _playerStates;
			}
		}

		/// <summary>
		/// All drawing is done to this world.
		/// </summary>
		public idRenderWorld RenderWorld
		{
			get
			{
				return _currentRenderWorld;
			}
		}

		public idGameRules Rules
		{
			get
			{
				return _gameRules;
			}
		}

		public idDict ServerInfo
		{
			get
			{
				return _serverInfo;
			}
		}

		/// <summary>
		/// All audio goes to this world.
		/// </summary>
		public idSoundWorld SoundWorld
		{
			get
			{
				return _currentSoundWorld;
			}
		}

		/// <summary>
		/// Keeps track of whether we're spawning, shutting down, or normal gameplay.
		/// </summary>
		public GameState State
		{
			get
			{
				return _gameState;
			}
		}

		/// <summary>
		/// Current time in msec.
		/// </summary>
		public int Time
		{
			get
			{
				return _time;
			}
		}

		public idDict[] UserInfo
		{
			get
			{
				return _userInfo;
			}
		}

		public idWorldSpawn World
		{
			get
			{
				return _world;
			}
			set
			{
				_world = value;
			}
		}
		#endregion

		#region Members
		private GameState _gameState;
		private idGameRules _gameRules;

		private int _time;
		private int _previousTime; // time in msec of last frame
		private int _realClientTime;

		private int _msec = idR.UserCommandRate; // time since last update in milliseconds

		private bool _isClient;
		private bool _isServer;
		private bool _isMultiplayer;

		private int _clientCount;
		private int _localClientIndex;

		private int _frameCount;
		private int _entityCount;
		private int _spawnCount;
		private int _firstFreeIndex;

		private SpawnPoint[] _spawnPoints = new SpawnPoint[] { };
		private idEntity[] _initialSpawnPoints = new idEntity[] { };
		private int _currentInitialSpawnPoint;

		private idWorldSpawn _world;
		private Vector3 _gravity; // global gravity vector

		private Random _random;

		private string _sessionCommand; // a target_sessionCommand can set this to return something to the session 

		// name of the map, empty string if no map loaded.
		private string _currentMapFileName;
		private idMapFile _currentMapFile;

		private idRenderWorld _currentRenderWorld;
		private idSoundWorld _currentSoundWorld;

		private idDict _serverInfo = new idDict();
		private idDict[] _userInfo = new idDict[idR.MaxClients];
		private idDict[] _persistentPlayerInfo = new idDict[idR.MaxClients];

		// this was moved out of mpgame because of the changes that were made to implement
		// the abstracted game rules.  CreateGameType gets called multiple times and would reset
		// this array would be reset and null references would ensue.
		private PlayerState[] _playerStates = new PlayerState[idR.MaxClients];

		private idEntity[] _entities = null;
		private LinkedList<idEntity> _spawnedEntities = new LinkedList<idEntity>();
		private int[] _spawnIds = null;
		private idUserCommand[] _userCommands = null;

		private idDict _spawnArgs = new idDict();

		private List<int>[,] _clientDeclRemap;

		private idClip _clip = new idClip(); // collision detection
		#endregion

		#region Constructor
		public idGame()
		{
			idR.Game = this;
			idR.GameEdit = new idGameEdit();

			InitCvars();

			int count = _userInfo.Length;

			for(int i = 0; i < count; i++)
			{
				_userInfo[i] = new idDict();
				_persistentPlayerInfo[i] = new idDict();
			}

			_gameRules = new Singleplayer();

			Clear();
		}
		#endregion

		#region Methods
		private void InitCvars()
		{
#if DEBUG
			new idCvar("g_version", String.Format("{0}.{1}-debug {3}", idR.EngineVersion, idVersion.BuildCount, idVersion.BuildDate, idVersion.BuildTime), "game version", CvarFlags.Game | CvarFlags.ReadOnly);
#else
			new idCvar("g_version", String.Format("{0}.{1}-debug {3}", idR.EngineVersion, idVersion.BuildCount, idVersion.BuildDate, idVersion.BuildTime), "game version", CvarFlags.Game | CvarFlags.ReadOnly);
#endif

			// noset vars
			new idCvar("gamename", idGame.GameVersion, "", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.ReadOnly);
			new idCvar("gamedate", idVersion.BuildDate, "", CvarFlags.Game | CvarFlags.ReadOnly);

			// server info
			new idCvar("si_name", "DOOM Server", "name of the server", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive);
			new idCvar("si_gameType", idStrings.GameTypes[0], idStrings.GameTypes, "game type - singleplayer, deathmatch, Tourney, Team DM or Last Man", new ArgCompletion_String(idStrings.GameTypes), CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive);
			new idCvar("si_map", "game/mp/d3dm1", "map to be played next on server", /* TODO: new MapNameArgCompletion(), */CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive);
			new idCvar("si_maxPlayers", "4", 1, 4, "max number of players allowed on the server", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("si_fragLimit", "10", 1, MultiplayerMaxFrags, "frag limit", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("si_timeLimit", "10", 0, 60, "time limit in minutes", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("si_teamDamage", "0", "enable team damage", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("si_warmup", "0", "do pre-game warmup", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("si_usePass", "0", "enable client password checking", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("si_pure", "1", "server is pure and does not allow modified data", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Bool);
			new idCvar("si_spectators", "1", "allow spectators or require all clients to play", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("si_serverURL", "", "where to reach the server admins and get information about the server", CvarFlags.Game | CvarFlags.ServerInfo | CvarFlags.Archive);

			// user info
			new idCvar("ui_name", "Player", "player name", CvarFlags.Game | CvarFlags.UserInfo | CvarFlags.Archive);
			new idCvar("ui_skin", idStrings.Skins[0], idStrings.Skins, "player skin", new ArgCompletion_String(idStrings.Skins), CvarFlags.Game | CvarFlags.UserInfo | CvarFlags.Archive);
			new idCvar("ui_team", idStrings.Teams[0], idStrings.Teams, "player team", new ArgCompletion_String(idStrings.Teams), CvarFlags.Game | CvarFlags.UserInfo | CvarFlags.Archive);
			new idCvar("ui_autoSwitch", "1", "auto switch weapon", CvarFlags.Game | CvarFlags.UserInfo | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("ui_autoReload", "1", "auto reload weapon", CvarFlags.Game | CvarFlags.UserInfo | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("ui_showgun", "1", "show gun", CvarFlags.Game | CvarFlags.UserInfo | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("ui_ready", idStrings.Ready[0], idStrings.Ready, "player is ready to start playing", new ArgCompletion_String(idStrings.Ready), CvarFlags.Game | CvarFlags.UserInfo);
			new idCvar("ui_spectate", idStrings.Spectate[0], idStrings.Spectate, "play or spectate", new ArgCompletion_String(idStrings.Spectate), CvarFlags.Game | CvarFlags.UserInfo);
			new idCvar("ui_chat", "0", "player is chatting", CvarFlags.Game | CvarFlags.UserInfo | CvarFlags.Bool | CvarFlags.ReadOnly | CvarFlags.Cheat);

			// change anytime vars
			new idCvar("r_aspectRatio", "0", 0, 2, "aspect ratio of view:\n0 = 4:3\n1 = 16:9\n2 = 16:10", CvarFlags.Renderer | CvarFlags.Integer | CvarFlags.Archive);

			new idCvar("g_cinematic", "1", "skips updating entities that aren't marked 'cinematic' '1' during cinematics", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_cinematicMaxSkipTime", "600", 0, 3600, "# of seconds to allow game to run when skipping cinematic.  prevents lock-up when cinematic doesn't end.", CvarFlags.Game | CvarFlags.Float);

			new idCvar("g_muzzleFlash", "1", "show muzzle flashes", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_projectileLights", "1", "show dynamic lights on projectiles", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_bloodEffects", "1", "show blood splats, sprays and gibs", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_doubleVision", "1", "show double vision when taking damage", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_monsters", "1", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_decals", "1", "show decals such as bullet holes", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_knockback", "1000", "", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("g_skill", "1", "", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("g_nightmare", "0", "if nightmare mode is allowed", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_gravity", DefaultGravity.ToString(), "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_skipFX", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_skipParticles", "0", "", CvarFlags.Game | CvarFlags.Bool);

			new idCvar("g_disasm", "0", "disassemble script into base/script/disasm.txt on the local drive when script is compiled", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_debugBounds", "0", "checks for models with bounds > 2048", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_debugAnim", "-1", "displays information on which animations are playing on the specified entity number.  set to -1 to disable.", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("g_debugMove", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_debugDamage", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_debugWeapon", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_debugScript", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_debugMover", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_debugTriggers", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_debugCinematic", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_stopTime", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_damageScale", "1", "scale final damage on player by this factor", CvarFlags.Game | CvarFlags.Float | CvarFlags.Archive);
			new idCvar("g_armorProtection", "0.3", "armor takes this percentage of damage", CvarFlags.Game | CvarFlags.Float | CvarFlags.Archive);
			new idCvar("g_armorProtectionMP", "0.6", "armor takes this percentage of damage in mp", CvarFlags.Game | CvarFlags.Float | CvarFlags.Archive);
			new idCvar("g_useDynamicProtection", "1", "scale damage and armor dynamically to keep the player alive more often", CvarFlags.Game | CvarFlags.Bool | CvarFlags.Archive);
			new idCvar("g_healthTakeTime", "5", "how often to take health in nightmare mode", CvarFlags.Game | CvarFlags.Integer | CvarFlags.Archive);
			new idCvar("g_healthTakeAmt", "5", "how much health to take in nightmare mode", CvarFlags.Game | CvarFlags.Integer | CvarFlags.Archive);
			new idCvar("g_healthTakeLimit", "25", "how low can health get taken in nightmare mode", CvarFlags.Game | CvarFlags.Integer | CvarFlags.Archive);

			new idCvar("g_showPVS", "0", 0, 2, "", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("g_showTargets", "0", "draws entities and thier targets.  hidden entities are drawn grey.", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_showTriggers", "0", "draws trigger entities (orange) and thier targets (green).  disabled triggers are drawn grey.", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_showCollisionWorld", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_showCollisionModels", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_showCollisionTraces", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_maxShowDistance", "128", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_showEntityInfo", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_showviewpos", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_showcamerainfo", "0", "displays the current frame # for the camera when playing cinematics", CvarFlags.Game | CvarFlags.Archive);
			new idCvar("g_showTestModelFrame", "0", "displays the current animation and frame # for testmodels", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_showActiveEntities", "0", "draws boxes around thinking entities.  dormant entities (outside of pvs) are drawn yellow.  non-dormant are green.", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_showEnemies", "0", "draws boxes around monsters that have targeted the the player", CvarFlags.Game | CvarFlags.Bool);

			new idCvar("g_frametime", "0", "displays timing information for each game frame", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_timeEntities", "0", "when non-zero, shows entities whose think functions exceeded the # of milliseconds specified", CvarFlags.Game | CvarFlags.Float);

			new idCvar("ai_debugScript", "-1", "displays script calls for the specified monster entity number", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("ai_debugMove", "0", "draws movement information for monsters", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("ai_debugTrajectory", "0", "draws trajectory tests for monsters", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("ai_testPredictPath", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("ai_showCombatNodes", "0", "draws attack cones for monsters", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("ai_showPaths", "0", "draws path_* entities", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("ai_showObstacleAvoidance", "0", 0, 2, "draws obstacle avoidance information for monsters.  if 2, draws obstacles for player, as well", new ArgCompletion_Integer(0, 2), CvarFlags.Game | CvarFlags.Integer);
			new idCvar("ai_blockedFailSafe", "1", "enable blocked fail safe handling", CvarFlags.Game | CvarFlags.Bool);

			new idCvar("g_dvTime", "1", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_dvAmplitude", "0.001", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_dvFrequency", "0.5", "", CvarFlags.Game | CvarFlags.Float);

			new idCvar("g_kickTime", "1", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_kickAmplitude", "0.0001", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_blobTime", "1", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_blobSize", "1", "", CvarFlags.Game | CvarFlags.Float);

			new idCvar("g_testHealthVision", "0", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_editEntityMode", "0", 0, 7, "0 = off\n1 = lights\n2 = sounds\n3 = articulated figures\n4 = particle systems\n5 = monsters\n6 = entity names\n7 = entity models", new ArgCompletion_Integer(0, 7), CvarFlags.Game | CvarFlags.Integer);
			new idCvar("g_dragEntity", "0", "allows dragging physics objects around by placing the crosshair over them and holding the fire button", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_dragDamping", "0.5", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_dragShowSelection", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_dropItemRotation", "", "", CvarFlags.Game);

			new idCvar("g_vehicleVelocity", "1000", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_vehicleForce", "50000", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_vehicleSuspensionUp", "32", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_vehicleSuspensionDown", "20", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_vehicleSuspensionKCompress", "200", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_vehicleSuspensionDamping", "400", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_vehicleTireFriction", "0.8", "", CvarFlags.Game | CvarFlags.Float);

			new idCvar("ik_enable", "1", "enable IK", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("ik_debug", "0", "show IK debug lines", CvarFlags.Game | CvarFlags.Bool);

			new idCvar("af_useLinearTime", "1", "use linear time algorithm for tree-like structures", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_useImpulseFriction", "0", "use impulse based contact friction", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_useJointImpulseFriction", "0", "use impulse based joint friction", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_useSymmetry", "1", "use constraint matrix symmetry", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_skipSelfCollision", "0", "skip self collision detection", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_skipLimits", "0", "skip joint limits", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_skipFriction", "0", "skip friction", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_forceFriction", "-1", "force the given friction value", CvarFlags.Game | CvarFlags.Float);
			new idCvar("af_maxLinearVelocity", "128", "maximum linear velocity", CvarFlags.Game | CvarFlags.Float);
			new idCvar("af_maxAngularVelocity", "1.57", "maximum angular velocity", CvarFlags.Game | CvarFlags.Float);
			new idCvar("af_timeScale", "1", "scales the time", CvarFlags.Game | CvarFlags.Float);
			new idCvar("af_jointFrictionScale", "0", "scales the joint friction", CvarFlags.Game | CvarFlags.Float);
			new idCvar("af_contactFrictionScale", "0", "scales the contact friction", CvarFlags.Game | CvarFlags.Float);
			new idCvar("af_highlightBody", "", "name of the body to highlight", CvarFlags.Game);
			new idCvar("af_highlightConstraint", "", "name of the constraint to highlight", CvarFlags.Game);
			new idCvar("af_showTimings", "0", "show articulated figure cpu usage", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showConstraints", "0", "show constraints", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showConstraintNames", "0", "show constraint names", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showConstrainedBodies", "0", "show the two bodies contrained by the highlighted constraint", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showPrimaryOnly", "0", "show primary constraints only", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showTrees", "0", "show tree-like structures", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showLimits", "0", "show joint limits", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showBodies", "0", "show bodies", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showBodyNames", "0", "show body names", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showMass", "0", "show the mass of each body", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showTotalMass", "0", "show the total mass of each articulated figure", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showInertia", "0", "show the inertia tensor of each body", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showVelocity", "0", "show the velocity of each body", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_showActive", "0", "show tree-like structures of articulated figures not at rest", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("af_testSolid", "1", "test for bodies initially stuck in solid", CvarFlags.Game | CvarFlags.Bool);

			new idCvar("rb_showTimings", "0", "show rigid body cpu usage", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("rb_showBodies", "0", "show rigid bodies", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("rb_showMass", "0", "show the mass of each rigid body", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("rb_showInertia", "0", "show the inertia tensor of each rigid body", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("rb_showVelocity", "0", "show the velocity of each rigid body", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("rb_showActive", "0", "show rigid bodies that are not at rest", CvarFlags.Game | CvarFlags.Bool);

			// the default values for player movement cvars are set in def/player.def
			new idCvar("pm_jumpheight", "48", "approximate hieght the player can jump", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_stepsize", "16", "maximum height the player can step up without jumping", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_crouchspeed", "80", "speed the player can move while crouched", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_walkspeed", "140", "speed the player can move while walking", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_runspeed", "220", "speed the player can move while running", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_noclipspeed", "200", "speed the player can move while in noclip", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_spectatespeed", "450", "speed the player can move while spectating", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_spectatebbox", "32", "size of the spectator bounding box", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_usecylinder", "0", "use a cylinder approximation instead of a bounding box for player collision detection", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Bool);
			new idCvar("pm_minviewpitch", "-89", "amount player's view can look up (negative values are up)", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_maxviewpitch", "89", "amount player's view can look down", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_stamina", "24", "length of time player can run", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_staminathreshold", "45", "when stamina drops below this value, player gradually slows to a walk", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_staminarate", "0.75", "rate that player regains stamina. divide pm_stamina by this value to determine how long it takes to fully recharge.", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_crouchheight", "38", "height of player's bounding box while crouched", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_crouchviewheight", "32", "height of player's view while crouched", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_normalheight", "74", "height of player's bounding box while standing", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_normalviewheight", "68", "height of player's view while standing", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_deadheight", "20", "height of player's bounding box while dead", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_deadviewheight", "10", "height of player's view while dead", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_crouchrate", "0.87", "time it takes for player's view to change from standing to crouching", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_bboxwidth", "32", "x/y size of player's bounding box", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_crouchbob", "0.5", "bob much faster when crouched", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_walkbob", "0.3", "bob slowly when walking", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_runbob", "0.4", "bob faster when running", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_runpitch", "0.002", "", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_runroll", "0.005", "", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_bobup", "0.005", "", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_bobpitch", "0.002", "", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_bobroll", "0.002", "", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_thirdPersonRange", "80", "camera distance from player in 3rd person", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_thirdPersonHeight", "0", "height of camera from normal view height in 3rd person", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_thirdPersonAngle", "0", "direction of camera from player in 3rd person in degrees (0 = behind player, 180 = in front)", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Float);
			new idCvar("pm_thirdPersonClip", "1", "clip third person view into world space", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Bool);
			new idCvar("pm_thirdPerson", "0", "enables third person view", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Bool);
			new idCvar("pm_thirdPersonDeath", "0", "enables third person view when player dies", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Bool);
			new idCvar("pm_modelView", "0", 0, 2, "draws camera from POV of player model (1 = always, 2 = when dead)", new ArgCompletion_Integer(0, 2), CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Integer);
			new idCvar("pm_air", "1800", "how long in milliseconds the player can go without air before he starts taking damage", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Integer);

			new idCvar("g_showPlayerShadow", "0", "enables shadow of player model", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_showHud", "1", "", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_showProjectilePct", "0", "enables display of player hit percentage", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_showBrass", "1", "enables ejected shells from weapon", CvarFlags.Game | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("g_gunX", "0", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_gunY", "0", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_gunZ", "0", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_viewNodalX", "0", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_viewNodalZ", "0", "", CvarFlags.Game | CvarFlags.Float);
			new idCvar("g_fov", "90", "", CvarFlags.Game | CvarFlags.Integer | CvarFlags.NoCheat);
			new idCvar("g_skipViewEffects", "0", "skip damage and other view effects", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_mpWeaponAngleScale", "0", "control the weapon sway in MP", CvarFlags.Game | CvarFlags.Float);

			new idCvar("g_testParticle", "0", "test particle visualation, set by the particle editor", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("g_testParticleName", "", "name of the particle being tested by the particle editor", CvarFlags.Game);
			new idCvar("g_testModelRotate", "0", "test model rotation speed", CvarFlags.Game);
			new idCvar("g_testPostProcess", "", "name of material to draw over screen", CvarFlags.Game);
			new idCvar("g_testModelAnimate", "0", 0, 4, "test model animation,\n0 = cycle anim with origin reset\n1 = cycle anim with fixed origin\n2 = cycle anim with continuous origin\n3 = frame by frame with continuous origin\n4 = play anim once", new ArgCompletion_Integer(0, 4), CvarFlags.Game | CvarFlags.Integer);
			new idCvar("g_testModelBlend", "0", "number of frames to blend", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("g_testDeath", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("g_exportMask", "", "", CvarFlags.Game);
			new idCvar("g_flushSave", "0", "1 = don't buffer file writing for save games.", CvarFlags.Game | CvarFlags.Bool);

			new idCvar("aas_test", "0", "", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("aas_showAreas", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("aas_showPath", "0", "", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("aas_showFlyPath", "0", "", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("aas_showWallEdges", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("aas_showHideArea", "0", "", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("aas_pullPlayer", "0", "", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("aas_randomPullPlayer", "0", "", CvarFlags.Game | CvarFlags.Bool);
			new idCvar("aas_goalArea", "0", "", CvarFlags.Game | CvarFlags.Integer);
			new idCvar("aas_showPushIntoArea", "0", "", CvarFlags.Game | CvarFlags.Bool);

			new idCvar("g_password", "", "game password", CvarFlags.Game | CvarFlags.Archive);
			new idCvar("password", "", "client password used when connecting", CvarFlags.Game | CvarFlags.NoCheat);

			new idCvar("g_countDown", "10", 4, 3600, "pregame countdown in seconds", CvarFlags.Game | CvarFlags.Integer | CvarFlags.Archive);
			new idCvar("g_gameReviewPause", "10", 2, 3600, "scores review time in seconds (at end game)", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Integer | CvarFlags.Archive);
			new idCvar("g_TDMArrows", "1", "draw arrows over teammates in team deathmatch", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Bool);
			new idCvar("g_balanceTDM", "1", "maintain even teams", CvarFlags.Game | CvarFlags.Bool);

			new idCvar("net_clientPredictGUI", "1", "test guis in networking without prediction", CvarFlags.Game | CvarFlags.Bool);

			new idCvar("g_voteFlags", "0", "vote flags. bit mask of votes not allowed on this server\nbit 0 (+1)   restart now\nbit 1 (+2)   time limit\nbit 2 (+4)   frag limit\nbit 3 (+8)   game type\nbit 4 (+16)  kick player\nbit 5 (+32)  change map\nbit 6 (+64)  spectators\nbit 7 (+128) next map", CvarFlags.Game | CvarFlags.NetworkSync | CvarFlags.Integer | CvarFlags.Archive);
			new idCvar("g_mapCycle", "mapcycle", "map cycling script for multiplayer games - see mapcycle.scriptcfg", CvarFlags.Game | CvarFlags.Archive);
			new idCvar("mod_validSkins", "skins/characters/player/marine_mp;skins/characters/player/marine_mp_green;skins/characters/player/marine_mp_blue;skins/characters/player/marine_mp_red;skins/characters/player/marine_mp_yellow", "valid skins for the game", CvarFlags.Game | CvarFlags.Archive);
			new idCvar("net_serverDownload", "0", "enable server download redirects. 0: off 1: redirect to si_serverURL 2: use builtin download. see net_serverDl cvars for configuration", CvarFlags.Game | CvarFlags.Integer | CvarFlags.Archive);
			new idCvar("net_serverDlBaseURL", "", "base URL for the download redirection", CvarFlags.Game | CvarFlags.Archive);
			new idCvar("net_serverDlTable", "", "pak names for which download is provided, seperated by ;", CvarFlags.Game | CvarFlags.Archive);

			idE.CvarSystem.RegisterStatics();
		}

		private void InitAsyncNetwork()
		{
			idConsole.Warning("TODO: InitAsyncNetwork");
			
			_clientDeclRemap = new List<int>[idR.MaxClients, 32];

			for(int i = 0; i < idR.MaxClients; i++)
			{
				for(int type = 0; type < idR.DeclManager.GetDeclTypeCount(); type++)
				{
					_clientDeclRemap[i, type] = new List<int>();
				}
			}

			/*memset( clientEntityStates, 0, sizeof( clientEntityStates ) );
			memset( clientPVS, 0, sizeof( clientPVS ) );
			memset( clientSnapshots, 0, sizeof( clientSnapshots ) );

	eventQueue.Init();
	savedEventQueue.Init();

	entityDefBits = -( idMath::BitsForInteger( declManager->GetNumDecls( DECL_ENTITYDEF ) ) + 1 );*/

			_localClientIndex = 0; // on a listen server SetLocalUser will set this right
			_realClientTime = 0;
			/*isNewFrame = true;
			clientSmoothing = net_clientSmoothing.GetFloat();*/
		}

		private void Clear()
		{
			_serverInfo.Clear();

			int count = _userInfo.Length;

			for(int i = 0; i < count; i++)
			{
				_userInfo[i].Clear();
				_persistentPlayerInfo[i].Clear();
			}

			_isClient = false;
			_isServer = false;
			_isMultiplayer = false;

			_clientCount = 0;
			_spawnCount = idGame.InitialSpawnCount;
			_localClientIndex = 0;

			_currentMapFileName = string.Empty;

			if(_currentMapFile != null)
			{
				_currentMapFile.Dispose();
				_currentMapFile = null;
			}

			_userCommands = null;
			_spawnIds = new int[idR.MaxGameEntities];
			_entities = new idEntity[idR.MaxGameEntities];
			_playerStates = new PlayerState[idR.MaxClients];

			count = _playerStates.Length;

			for(int i = 0; i < count; i++)
			{
				_playerStates[i] = new PlayerState();
			}

			_spawnArgs.Clear();
			_spawnedEntities.Clear();

			_gameState = GameState.Uninitialized;
			_firstFreeIndex = 0;
			_entityCount = 0;

			

			/*activeEntities.Clear();
			numEntitiesToDeactivate = 0;
			sortPushers = false;
			sortTeamMasters = false;
			persistentLevelInfo.Clear();
			memset(globalShaderParms, 0, sizeof(globalShaderParms));*/

			_random = new Random(0);

			_world = null;
			/*
			
			frameCommandThread = NULL;
			testmodel = NULL;
			testFx = NULL;*/
			// TODO: _clip.Shutdown();
			/*pvs.Shutdown();*/
			_sessionCommand = string.Empty;
			/*locationEntities = NULL;
			smokeParticles = NULL;
			editEntities = NULL;
			entityHash.Clear(1024, MAX_GENTITIES);
			inCinematic = false;
			cinematicSkipTime = 0;
			cinematicStopTime = 0;
			cinematicMaxSkipTime = 0;*/

			_frameCount = 0;
			_previousTime = 0;
			_time = 0;

			/*vacuumAreaNum = 0;
			
			mapSpawnCount = 0;
			camera = NULL;
			aasList.Clear();
			aasNames.Clear();
			lastAIAlertEntity = NULL;
			lastAIAlertTime = 0;

			gravity.Set(0, 0, -1);
			playerPVS.h = -1;
			playerConnectedAreas.h = -1;
			skipCinematic = false;
			influenceActive = false;*/

			_realClientTime = 0;

			/*isNewFrame = true;
			clientSmoothing = 0.1f;
			entityDefBits = 0;

			nextGibTime = 0;
			globalMaterial = NULL;
			newInfo.Clear();
			lastGUIEnt = NULL;
			lastGUI = 0;

			memset(clientEntityStates, 0, sizeof(clientEntityStates));
			memset(clientPVS, 0, sizeof(clientPVS));
			memset(clientSnapshots, 0, sizeof(clientSnapshots));

			eventQueue.Init();
			savedEventQueue.Init();

			memset(lagometer, 0, sizeof(lagometer));*/
		}

		private void CreateGameRules()
		{
			if(_gameRules != null)
			{
				_gameRules.Dispose();
				_gameRules = null;
			}

			string gameType = _serverInfo.GetString("si_gameType");
			
			switch(gameType.ToLower())
			{
				case "deathmatch":
					_gameRules = new Deathmatch();
					break;

				case "tourney":
				case "team dm":
				case "last man":
					throw new Exception(gameType + " gametype not handled");

				default:
					_gameRules = new Singleplayer();
					break;
			}

			/*if ( gameType == GAME_LASTMAN ) {
				if ( !serverInfo.GetInt( "si_warmup" ) ) {
					common->Warning( "Last Man Standing - forcing warmup on" );
					serverInfo.SetInt( "si_warmup", 1 );
				}
				if ( serverInfo.GetInt( "si_fraglimit" ) <= 0 ) {
					common->Warning( "Last Man Standing - setting fraglimit 1" );
					serverInfo.SetInt( "si_fraglimit", 1 );
				}
			}*/
		}

		private void LoadMap(string mapName, int randomSeed)
		{
			bool isSameMap = ((_currentMapFile != null) && (_currentMapFileName.ToLower() == mapName.ToLower()));

			// clear the sound system
			_currentSoundWorld.ClearAllSoundEmitters();

			InitAsyncNetwork();

			if((isSameMap == false) || ((_currentMapFile != null) && (_currentMapFile.NeedsReload == true)))
			{
				// load the .map file
				if(_currentMapFile != null)
				{
					_currentMapFile.Dispose();
					_currentMapFile = null;
				}

				_currentMapFile = new idMapFile();

				if(_currentMapFile.Parse(mapName) == false)
				{
					_currentMapFile.Dispose();
					_currentMapFile = null;

					idConsole.Error("Couldn't load {0}", mapName);
				}
			}

			_currentMapFileName = _currentMapFile.Name;

			// load the collision map
			idR.CollisionModelManager.LoadMap(_currentMapFile);

			_clientCount = 0;

			// initialize all entities for this game
			_entities = new idEntity[idR.MaxGameEntities];
			_spawnIds = new int[idR.MaxGameEntities];

			_userCommands = new idUserCommand[0];
			_spawnCount = idGame.InitialSpawnCount;

			_spawnedEntities.Clear();
			/*activeEntities.Clear();
			numEntitiesToDeactivate = 0;
			sortTeamMasters = false;
			sortPushers = false;
			lastGUIEnt = NULL;
			lastGUI = 0;

			globalMaterial = NULL;*/
			
			/*memset( globalShaderParms, 0, sizeof( globalShaderParms ) );*/

			// always leave room for the max number of clients,
			// even if they aren't all used, so numbers inside that
			// range are NEVER anything but clients

			_entityCount = idR.MaxClients;
			_firstFreeIndex = idR.MaxClients;

			// reset the random number generator.
			_random = new Random((this.IsMultiplayer == true) ? randomSeed : 0);

			/*

			camera			= NULL;
			world			= NULL;
			testmodel		= NULL;
			testFx			= NULL;

			lastAIAlertEntity = NULL;
			lastAIAlertTime = 0;*/
			_previousTime = 0;
			_time = 0;
			_frameCount = 0;

			_sessionCommand = string.Empty;
			/*
			nextGibTime		= 0;

			vacuumAreaNum = -1;		// if an info_vacuum is spawned, it will set this

			if ( !editEntities ) {
				editEntities = new idEditEntities;
			}*/

			_gravity = new Vector3(0, 0, -idR.CvarSystem.GetFloat("g_gravity"));
			_spawnArgs.Clear();

			/*skipCinematic = false;
			inCinematic = false;
			cinematicSkipTime = 0;
			cinematicStopTime = 0;
			cinematicMaxSkipTime = 0;*/

			_clip.Init();

			/*pvs.Init();
			playerPVS.i = -1;
			playerConnectedAreas.i = -1;

			// load navigation system for all the different monster sizes
			for( i = 0; i < aasNames.Num(); i++ ) {
				aasList[ i ]->Init( idStr( mapFileName ).SetFileExtension( aasNames[ i ] ).c_str(), mapFile->GetGeometryCRC() );
			}

			// clear the smoke particle free list
			smokeParticles->Init();*/

			// cache miscellanious media references
			FindEntityDef("preCacheExtras", false);

			if(isSameMap == false)
			{
				_currentMapFile.RemovePrimitiveData();
			}
		}

		private void MapPopulate()
		{
			_gameRules.MapPopulate();

			// parse the key/value pairs and spawn entities
			SpawnMapEntities();

			// mark location entities in all connected areas
			/* TODO: SpreadLocations();*/

			// prepare the list of randomized initial spawn spots
			RandomizeInitialSpawns();

			// spawnCount - 1 is the number of entities spawned into the map, their indexes started at MAX_CLIENTS (included)
			// mapSpawnCount is used as the max index of map entities, it's the first index of non-map entities
			/*mapSpawnCount = MAX_CLIENTS + spawnCount - 1;

			// execute pending events before the very first game frame
			// this makes sure the map script main() function is called
			// before the physics are run so entities can bind correctly
			Printf( "==== Processing events ====\n" );
			idEvent::ServiceEvents();*/
		}

		/// <summary>
		/// Parses textual entity definitions out of an entstring and spawns gentities.
		/// </summary>
		private void SpawnMapEntities()
		{
			idConsole.WriteLine("Spawning entities");

			if(_currentMapFile == null)
			{
				idConsole.WriteLine("No mapfile present");
				return;
			}

			// TODO: SetSkill( g_skill.GetInteger() );

			int entityCount = _currentMapFile.EntityCount;

			if(_entityCount == 0)
			{
				idConsole.Error("...no entities");
			}

			// the worldspawn is a special that performs any global setup needed by a level
			idMapEntity mapEntity = _currentMapFile.GetEntity(0);

			idDict args = mapEntity.Dict;
			args.Set("spawn_entnum", idR.EntityIndexWorld);

			if((SpawnEntityDef(args) == null) || (_entities[idR.EntityIndexWorld] == null) || ((_entities[idR.EntityIndexWorld] is idWorldSpawn) == false))
			{
				idConsole.Error("Problem spawning world entity");
			}

			int count = 1;
			int inhibitCount = 0;

			for(int i = 1; i < entityCount; i++)
			{
				mapEntity = _currentMapFile.GetEntity(i);
				args = mapEntity.Dict;

				if(InhibitEntitySpawn(args) == false)
				{
					// precache any media specified in the map entity
					CacheDictionaryMedia(args);

					if(SpawnEntityDef(args) != null)
					{
						count++;
					}
				}
				else
				{
					inhibitCount++;
				}
			}

			idConsole.WriteLine("...{0} entities spawned, {1} inhibited", count, inhibitCount);
		}

		private bool InhibitEntitySpawn(idDict args)
		{
			bool result = false;

			if(this.IsMultiplayer == true)
			{
				result = args.GetBool("not_multiplayer", false);
			}
			else if(idR.CvarSystem.GetInteger("g_skill") == 0)
			{
				result = args.GetBool("not_easy", false);
			}
			else if(idR.CvarSystem.GetInteger("g_skill") == 1)
			{
				result = args.GetBool("not_medium", false);
			}
			else
			{
				result = args.GetBool("not_hard", false);
			}

			string name;

			if(idR.CvarSystem.GetInteger("g_skill") == 3)
			{
				name = args.GetString("classname").ToLower();

				if((name == "item_medkit") || (name == "item_medkit_small"))
				{
					result = true;
				}
			}

			if(this.IsMultiplayer == true)
			{
				name = args.GetString("classname").ToLower();

				if((name == "weapon_bfg") || (name == "weapon_soulcube"))
				{
					result = true;
				}
			}

			return result;
		}

		private void RandomizeInitialSpawns()
		{
			if((this.IsMultiplayer == false) || (this.IsClient == true))
			{
				return;
			}

			List<SpawnPoint> points = new List<SpawnPoint>();
			List<idEntity> initialPoints = new List<idEntity>();

			SpawnPoint point = new SpawnPoint();
			point.Entity = FindEntityUsingDef(null, "info_player_deathmatch");

			while(point.Entity != null)
			{
				points.Add(point);

				if(point.Entity.SpawnArgs.GetBool("initial") == true)
				{
					initialPoints.Add(point.Entity);
				}

				idEntity ent = point.Entity;

				point = new SpawnPoint();
				point.Entity = FindEntityUsingDef(ent, "info_player_deathmatch");
			}

			if(points.Count == 0)
			{
				idConsole.Warning("no info_player_deathmatch in map");
				return;
			}

			idConsole.WriteLine("{0} spawns ({1} initials)", points.Count, initialPoints.Count);

			// if there are no initial spots in the map, consider they can all be used as initial
			if(initialPoints.Count == 0)
			{
				idConsole.Warning("no info_player_deathmatch entities marked initial in map");

				foreach(SpawnPoint p in points)
				{
					initialPoints.Add(p.Entity);
				}
			}

			_spawnPoints = points.ToArray();
			_initialSpawnPoints = initialPoints.ToArray();

			int count = _initialSpawnPoints.Length;

			for(int i = 0; i < count; i++)
			{
				int j = _random.Next(_initialSpawnPoints.Length);
				idEntity ent = _initialSpawnPoints[i];

				_initialSpawnPoints[i] = _initialSpawnPoints[j];
				_initialSpawnPoints[j] = ent;
			}

			// reset the counter
			_currentInitialSpawnPoint = 0;
		}

		private void InitClientDeclRemap(int clientIndex)
		{
			for(int type = 0; type < idR.DeclManager.GetDeclTypeCount(); type++)
			{
				// only implicit materials and sound shaders decls are used
				DeclType declType = (DeclType) type;

				if((declType != DeclType.Material) && (declType != DeclType.Sound))
				{
					continue;
				}

				int count = idR.DeclManager.GetDeclCount(declType);

				_clientDeclRemap[clientIndex, type].Clear();

				// pre-initialize the remap with non-implicit decls, all non-implicit decls are always going
				// to be in order and in sync between server and client because of the decl manager checksum
				for(int i = 0; i < count; i++)
				{
					idDecl decl = idR.DeclManager.DeclByIndex(declType, i, false);

					if(decl.IsImplicit == true)
					{
						// once the first implicit decl is found all remaining decls are considered implicit as well
						break;
					}

					_clientDeclRemap[clientIndex, type].Add(i);
				}
			}
		}

		/// <summary>
		/// spectators are spawned randomly anywhere.
		/// in-game clients are spawned based on distance to active players (randomized on the first half)
		/// upon map restart, initial spawns are used (randomized ordered list of spawns flagged "initial").
		/// if there are more players than initial spots, overflow to regular spawning.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public idEntity SelectInitialSpawnPoint(idPlayer player)
		{
			if((this.IsMultiplayer == false) || (_spawnPoints.Length == 0))
			{
				idEntity ent = FindEntityUsingDef(null, "info_player_start");

				if(ent == null)
				{
					idConsole.Error("No info_player_start on map.");
				}

				return ent;
			}

			if(player.IsSpectating == true)
			{
				// plain random spot, don't bother
				return _spawnPoints[_random.Next(_spawnPoints.Length)].Entity;
				/*} else if ( player->useInitialSpawns && currentInitialSpot < initialSpots.Num() ) {
			return initialSpots[ currentInitialSpot++ ];*/
			}
			else
			{
				// check if we are alone in map
				bool alone = true;

				for(int j = 0; j < idR.MaxClients; j++)
				{
					if((_entities[j] != null) && (_entities[j] != player))
					{
						alone = false;
					}
				}

				if(alone == true)
				{
					// don't do distance-based
					return _spawnPoints[_random.Next(_spawnPoints.Length)].Entity;
				}

				// TODO
				// find the distance to the closest active player for each spawn spot
				/*for( i = 0; i < spawnSpots.Num(); i++ ) {
					pos = spawnSpots[ i ].ent->GetPhysics()->GetOrigin();
					spawnSpots[ i ].dist = 0x7fffffff;
					for( j = 0; j < MAX_CLIENTS; j++ ) {
						if ( !entities[ j ] || !entities[ j ]->IsType( idPlayer::Type )
							|| entities[ j ] == player
							|| static_cast< idPlayer * >( entities[ j ] )->spectating ) {
							continue;
						}
				
						dist = ( pos - entities[ j ]->GetPhysics()->GetOrigin() ).LengthSqr();
						if ( dist < spawnSpots[ i ].dist ) {
							spawnSpots[ i ].dist = dist;
						}
					}
				}

		// sort the list
		qsort( ( void * )spawnSpots.Ptr(), spawnSpots.Num(), sizeof( spawnSpot_t ), ( int (*)(const void *, const void *) )sortSpawnPoints );

		// choose a random one in the top half
		which = random.RandomInt( spawnSpots.Num() / 2 );
		spot = spawnSpots[ which ];*/
			}

			// TODO: return spot.ent;
			return null;
		}

		public idDeclEntity FindEntityDef(string name, bool makeDefault)
		{
			idDeclEntity decl = null;

			// TODO: refactor in to mpgame
			if(this.IsMultiplayer == true)
			{
				decl = idR.DeclManager.FindType<idDeclEntity>(DeclType.EntityDef, string.Format("{0}_mp", name), false);
			}

			if(decl == null)
			{
				decl = idR.DeclManager.FindType<idDeclEntity>(DeclType.EntityDef, name, makeDefault);
			}

			return decl;
		}

		public idDict FindEntityDefDict(string name, bool makeDefault)
		{
			idDeclEntity decl = FindEntityDef(name, makeDefault);

			if(decl.Dict != null)
			{
				return decl.Dict;
			}

			return null;
		}

		/// <summary>
		/// Searches all active entities for the next one using the specified entityDef.
		/// </summary>
		/// <remarks>
		/// Searches beginning at the entity after from, or the beginning if NULL
		/// NULL will be returned if the end of the list is reached.
		/// </remarks>
		/// <param name="from"></param>
		/// <param name="match"></param>
		/// <returns></returns>
		public idEntity FindEntityUsingDef(idEntity from, string match)
		{
			idEntity ent = null;
			LinkedListNode<idEntity> node = null;

			if(from == null)
			{
				node = _spawnedEntities.First;
				
				if(node != null)
				{
					ent = node.Value;
				}
			}
			else
			{
				node = from.SpawnNode.Next;

				if(node != null)
				{
					ent = node.Value;
				}
			}

			while(ent != null)
			{
				if(ent.DefName.ToLower() == match.ToLower())
				{
					return ent;
				}

				node = ent.SpawnNode.Next;
				ent = null;

				if(node != null)
				{
					ent = node.Value;
				}
			}

			return null;
		}
		
		public idEntity SpawnEntityDef(idDict args, bool setDefaults = true)
		{
			_spawnArgs = args;

			string error = string.Empty;
			string name = string.Empty;
			string className = string.Empty;

			if(_spawnArgs.ContainsKey("name") == false)
			{
				error = string.Format(" on '{0}'", name);
			}

			name = _spawnArgs.GetString("name", "");
			className = _spawnArgs.GetString("classname");

			idDeclEntity def = FindEntityDef(className, false);

			if(def == null)
			{
				idConsole.Warning("Unknown classname '{0}'{1}.", className, error);
				return null;
			}

			_spawnArgs.SetDefaults(def.Dict);

			// check if we should spawn a class object
			string spawn = _spawnArgs.GetString("spawnclass", null);

			if(spawn != null)
			{
				// replaced with .net reflection instead of native d3 class framework.

				// need to check all loaded assemblies to find the type we're looking for.
				Type type = null;

				foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					// all entity types must be in the idTech4.Game.Entities namespace.
					string typeName = string.Format("idTech4.Game.Entities.{0}", spawn);
					type = asm.GetType(typeName, false, true);

					if(type != null)
					{
						break;
					}
				}

				if(type == null)
				{
					idConsole.Warning("Could not spawn '{0}'.  Class '{1}' not found{2}.", className, spawn, error);
					return null;
				}

				object obj = type.Assembly.CreateInstance(type.FullName);

				if(obj == null)
				{
					idConsole.Warning("Could not spawn '{0}'. Instance could not be created{1}.", className, error);
					return null;
				}

				idEntity ent = (idEntity) obj;
				ent.Spawn();

				idConsole.DeveloperWriteLine("Spawned {0} ({1})", ent.ClassName, ent.GetType().FullName);

				return ent;
			}

			idConsole.Warning("TODO: Spawnfunc");
			// TODO: spawnfunc
			// check if we should call a script function to spawn
			/*spawnArgs.GetString( "spawnfunc", NULL, &spawn );
			if ( spawn ) {
				const function_t *func = program.FindFunction( spawn );
				if ( !func ) {
					Warning( "Could not spawn '%s'.  Script function '%s' not found%s.", classname, spawn, error.c_str() );
					return false;
				}
				idThread *thread = new idThread( func );
				thread->DelayedStart( 0 );
				return true;
			}

			Warning( "%s doesn't include a spawnfunc or spawnclass%s.", classname, error.c_str() );
			return false;*/
		
			return null;
		}

		public void ServerSendChatMessage(int to, string name, string text)
		{
			idConsole.Warning("TODO: ServerSendChatMessage");
			// TODO: 
			/*idBitMsg outMsg = new idBitMsg();
			outMsg.InitGame();
			outMsg.BeginWriting();
			outMsg.WriteByte((int) GameReliableMessage.Chat);
			outMsg.WriteString(name);
			outMsg.WriteString(text, -1, false);

			idR.NetworkSystem.ServerSendReliableMessage(to, outMsg);

			if((to == -1) || (to == _localClientIndex))
			{
				((Multiplayer) _gameRules).AddChatLine("{0}^0: {1}", name, text);
			}*/		
		}

		public void RegisterEntity(idEntity entity)
		{
			int entitySpawnIndex;

			if(_spawnCount >= (1 << (32 - idR.GameEntityBits)))
			{
				idConsole.Error("idGameLocal.RegisterEntity: spawn count overflow");
			}

			if(_spawnArgs.ContainsKey("spawn_entnum") == true)
			{
				entitySpawnIndex = _spawnArgs.GetInteger("spawn_entnum", 0);
			}
			else
			{
				while((_entities[_firstFreeIndex] != null) && (_firstFreeIndex < idR.EntityCountNormalMax))
				{
					_firstFreeIndex++;
				}

				if(_firstFreeIndex >= idR.EntityCountNormalMax)
				{
					idConsole.Error("no free entities");
				}

				entitySpawnIndex = _firstFreeIndex++;
			}

			_entities[entitySpawnIndex] = entity;
			_spawnIds[entitySpawnIndex] = _spawnCount++;

			entity.Index = entitySpawnIndex;
			entity.SpawnArgs.TransferKeyValues(_spawnArgs);

			entity.SpawnNode = _spawnedEntities.AddLast(entity);


			if(entitySpawnIndex >= _entityCount)
			{
				_entityCount++;
			}
		}
		#endregion

		#region idGame implementation
		public override void Init()
		{
			idE.CvarSystem.RegisterStatics();

			// TODO: initialize processor specific SIMD
			// idSIMD::InitProcessor( "game", com_forceGenericSIMD.GetBool() );

			idConsole.WriteLine("--------- Initializing Game ----------");
			idConsole.WriteLine("gamename: {0}", idGame.GameVersion);
			idConsole.WriteLine("gamedate: {0}", idVersion.BuildDate);

			// TODO: register game specific decl types
			idR.DeclManager.RegisterDeclType("model", DeclType.ModelDef, new idDeclAllocator<idDeclModel>());
			idR.DeclManager.RegisterDeclType("export", DeclType.ModelExport, new idDeclAllocator<idDecl>());

			// register game specific decl folders
			idR.DeclManager.RegisterDeclFolder("def", ".def", DeclType.EntityDef);
			// TODO: idR.DeclManager.RegisterDeclFolder("fx", ".fx", DeclType.Fx);
			idR.DeclManager.RegisterDeclFolder("particles", ".prt", DeclType.Particle);
			// TODO: idR.DeclManager.RegisterDeclFolder("af", ".af", DeclType.Af);
			idR.DeclManager.RegisterDeclFolder("newpdas", ".pda", DeclType.Pda);

			/*cmdSystem->AddCommand( "listModelDefs", idListDecls_f<DECL_MODELDEF>, CMD_FL_SYSTEM|CMD_FL_GAME, "lists model defs" );
			cmdSystem->AddCommand( "printModelDefs", idPrintDecls_f<DECL_MODELDEF>, CMD_FL_SYSTEM|CMD_FL_GAME, "prints a model def", idCmdSystem::ArgCompletion_Decl<DECL_MODELDEF> );
			*/
			Clear();

			idConsole.Warning("TODO: events, class, console");
			/*
			TODO: 
			idEvent::Init();
			idClass::Init();

			InitConsoleCommands();*/

			idConsole.Warning("TODO: AAS");

			// TODO: AAS
			/*
				// TODO: load default scripts
				// program.Startup( SCRIPT_DEFAULT );
	
				// TODO: smokeParticles = new idSmokeParticles;

				// TODO: set up the aas
				dict = FindEntityDefDict( "aas_types" );
				if ( !dict ) {
					Error( "Unable to find entityDef for 'aas_types'" );
				}

				// allocate space for the aas
				const idKeyValue *kv = dict->MatchPrefix( "type" );
				while( kv != NULL ) {
					aas = idAAS::Alloc();
					aasList.Append( aas );
					aasNames.Append( kv->GetValue() );
					kv = dict->MatchPrefix( "type", kv );
				}
				Printf( "...%d aas types\n", aasList.Num() );*/

			_gameState = GameState.NoMap;

			idConsole.WriteLine("game initialized.");
			idConsole.WriteLine("--------------------------------------");
		}

		public override GameReturn RunFrame(idUserCommand[] userCommands)
		{
			GameReturn gameReturn = new GameReturn();
			idPlayer player = this.LocalPlayer;

			// set the user commands for this frame
			_userCommands = (idUserCommand[]) userCommands.Clone();

			if((this.IsMultiplayer == false) && (idR.CvarSystem.GetBool("g_stopTime") == true))
			{
				// clear any debug lines from a previous frame
				_currentRenderWorld.DebugClearLines(_time + 1);

				if(player != null)
				{
					player.Think();
				}
			}
			else
			{
				do
				{
					// update the game time
					_frameCount++;
					_previousTime = _time;

					_time += _msec;
					_realClientTime = _time;

					// TODO
					// allow changing SIMD usage on the fly
					/*if ( com_forceGenericSIMD.IsModified() ) {
						idSIMD::InitProcessor( "game", com_forceGenericSIMD.GetBool() );
					}*/


					// make sure the random number counter is used each frame so random events
					// are influenced by the player's actions
					_random.Next();

					if(player != null)
					{
						// update the renderview so that any gui videos play from the right frame
						idRenderView view = player.RenderView;

						if(view != null)
						{
							_currentRenderWorld.RenderView = view;
						}
					}

					// clear any debug lines from a previous frame
					_currentRenderWorld.DebugClearLines(_time);

					// clear any debug polygons from a previous frame
					_currentRenderWorld.DebugClearPolygons(_time);

					// free old smoke particles
					// TODO: smokeParticles->FreeSmokes();

					// TODO
					// process events on the server
					/*ServerProcessEntityNetworkEventQueue();*/

					// update our gravity vector if needed.
					/* TODO: UpdateGravity();

					// create a merged pvs for all players
					TODO: SetupPlayerPVS();

					// sort the active entity list
					TODO: SortActiveEntityList();*/

					/*timer_think.Clear();
					TODO: timer_think.Start();*/

					// let entities think
					if(idR.CvarSystem.GetFloat("g_timeentities") > 0)
					{
						int count = 0;

						/*for( ent = activeEntities.Next(); ent != NULL; ent = ent->activeNode.Next() ) {
							if ( g_cinematic.GetBool() && inCinematic && !ent->cinematic ) {
								ent->GetPhysics()->UpdateTime( time );
								continue;
							}
							timer_singlethink.Clear();
							timer_singlethink.Start();
							ent->Think();
							timer_singlethink.Stop();
							ms = timer_singlethink.Milliseconds();
							if ( ms >= g_timeentities.GetFloat() ) {
								Printf( "%d: entity '%s': %.1f ms\n", time, ent->name.c_str(), ms );
							}
							num++;
						}*/
					}
					else
					{
						/*if ( inCinematic ) {
							num = 0;
							for( ent = activeEntities.Next(); ent != NULL; ent = ent->activeNode.Next() ) {
								if ( g_cinematic.GetBool() && !ent->cinematic ) {
									ent->GetPhysics()->UpdateTime( time );
									continue;
								}
								ent->Think();
								num++;
							}
						} else {
							num = 0;
							for( ent = activeEntities.Next(); ent != NULL; ent = ent->activeNode.Next() ) {
								ent->Think();
								num++;
							}
						}*/
					}

					// remove any entities that have stopped thinking
					/*if ( numEntitiesToDeactivate ) {
						idEntity *next_ent;
						int c = 0;
						for( ent = activeEntities.Next(); ent != NULL; ent = next_ent ) {
							next_ent = ent->activeNode.Next();
							if ( !ent->thinkFlags ) {
								ent->activeNode.Remove();
								c++;
							}
						}
						//assert( numEntitiesToDeactivate == c );
						numEntitiesToDeactivate = 0;
					}

					timer_think.Stop();
					timer_events.Clear();
					timer_events.Start();

					// service any pending events
					idEvent::ServiceEvents();

					timer_events.Stop();

					// free the player pvs
					FreePlayerPVS();*/

					_gameRules.Run();

					// display how long it took to calculate the current game frame
					/*if ( g_frametime.GetBool() ) {
						Printf( "game %d: all:%.1f th:%.1f ev:%.1f %d ents \n",
							time, timer_think.Milliseconds() + timer_events.Milliseconds(),
							timer_think.Milliseconds(), timer_events.Milliseconds(), num );
					}*/

					// build the return value
					gameReturn.ConsistencyHash = 0;
					gameReturn.SessionCommand = string.Empty;

					/*if ( !isMultiplayer && player ) {
						ret.health = player->health;
						ret.heartRate = player->heartRate;
						ret.stamina = idMath::FtoiFast( player->stamina );
						// combat is a 0-100 value based on lastHitTime and lastDmgTime
						// each make up 50% of the time spread over 10 seconds
						ret.combat = 0;
						if ( player->lastDmgTime > 0 && time < player->lastDmgTime + 10000 ) {
							ret.combat += 50.0f * (float) ( time - player->lastDmgTime ) / 10000;
						}
						if ( player->lastHitTime > 0 && time < player->lastHitTime + 10000 ) {
							ret.combat += 50.0f * (float) ( time - player->lastHitTime ) / 10000;
						}
					}*/

					// see if a target_sessionCommand has forced a changelevel
					if(_sessionCommand != string.Empty)
					{
						gameReturn.SessionCommand = _sessionCommand;
						break;
					}

					// make sure we don't loop forever when skipping a cinematic
					/*if ( skipCinematic && ( time > cinematicMaxSkipTime ) ) {
						Warning( "Exceeded maximum cinematic skip length.  Cinematic may be looping infinitely." );
						skipCinematic = false;
						break;
					}*/
				}
				while(1 == 0/* TODO: ( inCinematic || ( time < cinematicStopTime ) ) && skipCinematic */);
			}

			/*ret.syncNextGameFrame = skipCinematic;
			if ( skipCinematic ) {
				soundSystem->SetMute( false );
				skipCinematic = false;		
			}*/

			// show any debug info for this frame
			/*RunDebugInfo();
			D_DrawDebugLines();*/

			return gameReturn;
		}

		public override bool Draw(int clientIndex)
		{
			return _gameRules.Draw(clientIndex);
		}

		public /*override*/ void HandleMainMenuCommands(string menuCommand, idUserInterface gui)
		{
			idConsole.DeveloperWriteLine("HandleMainMenuCommands");
		}

		public /*override*/ string GetBestGameType(string map, string gameType)
		{
			idConsole.DeveloperWriteLine("GetBestGameType");

			return gameType;
		}

		public override string GetMapLoadingInterface(string defaultInterface)
		{
			idConsole.DeveloperWriteLine("GetMapLoadingGui");

			return defaultInterface;
		}

		/// <summary>
		/// This is called after parsing an EntityDef and for each entity spawnArgs before
		/// merging the entitydef.  It could be done post-merge, but that would
		/// avoid the fast pre-cache check associated with each entityDef.
		/// </summary>
		/// <param name="dict"></param>
		public override void CacheDictionaryMedia(idDict dict)
		{			
			/*idKeyValue kv = null;
			// TODO
			/*if ( dict == NULL ) {
					if ( cvarSystem->GetCVarBool( "com_makingBuild") ) {
						DumpOggSounds();
					}
					return;
				}*/

			/*if ( cvarSystem->GetCVarBool( "com_makingBuild" ) ) {
				GetShakeSounds( dict );
			}*/

			#region Model
			foreach(KeyValuePair<string, string> kvp in dict.MatchPrefix("model"))
			{
				// precache model/animations
				if((kvp.Value != string.Empty) && (idR.DeclManager.FindType<idDecl>(DeclType.ModelDef, kvp.Value, false) == null))
				{
					// precache the render model
					idR.RenderModelManager.FindModel(kvp.Value);

					// precache .cm files only
					idR.CollisionModelManager.LoadModel(kvp.Value, true);
				}
			}
			#endregion

			#region Gui
			foreach(KeyValuePair<string, string> kvp in dict.MatchPrefix("gui"))
			{
				string keyLower = kvp.Key.ToLower();

				if((keyLower == "gui_noninteractive")
					|| (keyLower.StartsWith("gui_parm") == true)
					|| (keyLower == "gui_inventory"))
				{
					// unfortunate flag names, they aren't actually a gui
				}
				else
				{
					idR.DeclManager.MediaPrint(string.Format("Precaching gui {0}", kvp.Value));

					idUserInterface gui = new idUserInterface();
					
					if(gui != null)
					{
						gui.InitFromFile(kvp.Value);
						idE.UIManager.Remove(gui);
					}					
				}
			}
			#endregion

			#region Fx
			foreach(KeyValuePair<string, string> kvp in dict.MatchPrefix("fx"))
			{
				idR.DeclManager.MediaPrint(string.Format("Precaching fx {0}", kvp.Value));
				idR.DeclManager.FindType<idDecl>(DeclType.Fx, kvp.Value);
			}
			#endregion

			#region Smoke
			foreach(KeyValuePair<string, string> kvp in dict.MatchPrefix("smoke"))
			{
				string prtName = kvp.Value;
				int dash = prtName.IndexOf('-');

				if(dash > 0)
				{
					prtName = prtName.Substring(0, dash);
				}

				idR.DeclManager.FindType(DeclType.Particle, prtName);
			}
			#endregion

			#region Skin
			foreach(KeyValuePair<string, string> kvp in dict.MatchPrefix("skin"))
			{
				idR.DeclManager.MediaPrint(string.Format("Precaching skin {0}", kvp.Value));
				idR.DeclManager.FindType(DeclType.Skin, kvp.Value);
			}
			#endregion

			#region Def
			foreach(KeyValuePair<string, string> kvp in dict.MatchPrefix("def"))
			{
				FindEntityDef(kvp.Value, false);
			}
			#endregion

			#region Misc
			string value = dict.GetString("s_shader");

			if(value != string.Empty)
			{
				idR.DeclManager.FindType<idDecl>(DeclType.Sound, value);
			}

			value = dict.GetString("texture");

			if(value != string.Empty)
			{
				idR.DeclManager.FindType<idDecl>(DeclType.Material, value);
			}

			Dictionary<string, DeclType> cacheElements = new Dictionary<string, DeclType>() {
				{ "snd", DeclType.Sound },
				{ "mtr", DeclType.Material },
				{ "inv_icon", DeclType.Material },
				{ "pda_name", DeclType.Pda },
				{ "video", DeclType.Video },
				{ "audio", DeclType.Audio }
			};

			foreach(KeyValuePair<string, DeclType> element in cacheElements)
			{
				foreach(KeyValuePair<string, string> kvp in dict.MatchPrefix(element.Key))
				{
					idR.DeclManager.FindType<idDecl>(element.Value, kvp.Value);
				}
			}
			#endregion
		}

		public override void InitFromNewMap(string mapName, idRenderWorld renderWorld, idSoundWorld soundWorld, bool isServer, bool isClient, int randomSeed)
		{
			idConsole.DeveloperWriteLine("InitFromNewMap");

			_isServer = isServer;
			_isClient = isClient;
			_isMultiplayer = isServer || isClient;

			if(_currentMapFileName != null)
			{
				idConsole.Warning("TODO: MapShutdown");
				// TODO: MapShutdown();
			}

			idConsole.WriteLine("----------- Game Map Init ------------");

			_gameState = GameState.Startup;

			_currentRenderWorld = renderWorld;
			_currentSoundWorld = soundWorld;

			LoadMap(mapName, randomSeed);

			idConsole.Warning("TODO: InitScriptForMap");
			// TODO: InitScriptForMap();

			MapPopulate();

			_gameRules.Reset();
			_gameRules.Precache();

			// free up any unused animations
			// TODO: animationLib.FlushUnusedAnims();

			_gameState = GameState.Active;

			idConsole.WriteLine("--------------------------------------");
		}

		public /*override*/ void SetLocalClient(int clientIndex)
		{
			_localClientIndex = clientIndex;
		}

		public override void SetServerInfo(idDict serverInfo)
		{
			_serverInfo = serverInfo;

			CreateGameRules();

			if(this.IsClient == false)
			{
				idConsole.Warning("TODO: SetServerInfo");
				// TODO
				/*idBitMsg outMsg = new idBitMsg();
				outMsg.InitGame();
				outMsg.WriteByte((int) GameReliableMessage.ServerInfo);
				outMsg.WriteDeltaDict(_serverInfo, null);

				idR.NetworkSystem.ServerSendReliableMessage(-1, outMsg);*/
			}
		}

		public override void SetPersistentPlayerInformation(int clientIndex, idDict playerInfo)
		{
			_persistentPlayerInfo[clientIndex] = playerInfo;
		}

		public override idDict SetUserInformation(int clientIndex, idDict userInfo, bool isClient, bool canModify)
		{
			bool modifiedInfo = false;

			_isClient = isClient;

			if((clientIndex >= 0) && (clientIndex < idR.MaxClients))
			{
				_userInfo[clientIndex] = userInfo;

				// server sanity
				if(canModify == true)
				{
					int number;

					modifiedInfo = true;

					// don't let numeric nicknames, it can be exploited to go around kick and ban commands from the server
					if(Int32.TryParse(_userInfo[clientIndex].GetString("ui_name"), out number) == true)
					{
						_userInfo[clientIndex].Set("ui_name", String.Format("{0}_", _userInfo[clientIndex].GetString("ui_name")));
						modifiedInfo = true;
					}

					// don't allow dupe nicknames
					for(int i = 0; i < _clientCount; i++)
					{
						if(i == clientIndex)
						{
							continue;
						}

						if((_entities[i] != null) && (_entities[i] is idPlayer))
						{
							if(_userInfo[clientIndex].GetString("ui_name").ToLower() == _userInfo[i].GetString("ui_name"))
							{
								_userInfo[clientIndex].Set("ui_name", string.Format("{0}_", _userInfo[clientIndex].GetString("ui_name")));
								modifiedInfo = true;
								i = -1; // rescan
								continue;
							}
						}
					}
				}

				if((_entities[clientIndex] != null) && (_entities[clientIndex] is idPlayer))
				{
					modifiedInfo |= ((idPlayer) _entities[clientIndex]).UserInfoChanged(canModify);
					modifiedInfo |= idR.Game.Rules.UserInfoChanged(clientIndex, canModify);
				}

				if(this.IsClient == false)
				{
					// now mark this client in game
					_gameRules.EnterGame(clientIndex);
				}
			}

			if(modifiedInfo == true)
			{
				return _userInfo[clientIndex];
			}

			return null;
		}

		public /*override*/ void ServerClientConnect(int clientIndex, string guid)
		{
			idConsole.DeveloperWriteLine("ServerClientConnect");

			// make sure no parasite entity is left
			if(_entities[clientIndex] != null)
			{
				idConsole.DeveloperWriteLine("ServerClientConnect: remove old player entity");

				_entities[clientIndex].Dispose();
				_entities[clientIndex] = null;
			}

			_userInfo[clientIndex].Clear();
			_playerStates[clientIndex].Clear();
			_gameRules.ClientConnect(clientIndex);

			idConsole.WriteLine("client {0} connected.", clientIndex);
		}

		public /*override*/ void ServerClientBegin(int clientIndex)
		{
			// initialize the decl remap
			InitClientDeclRemap(clientIndex);

			idConsole.Warning("TODO: ServerClientBegin");
			// send message to initialize decl remap at the client (this is always the very first reliable game message)
			// TODO
			/*idBitMsg outMsg = new idBitMsg();
			outMsg.InitGame();
			outMsg.BeginWriting();
			outMsg.WriteByte((int) GameReliableMessage.InitDeclRemap);

			idR.NetworkSystem.ServerSendReliableMessage(clientIndex, outMsg);*/

			// spawn the player
			SpawnPlayer(clientIndex);

			if(clientIndex == _localClientIndex)
			{
				this.Rules.EnterGame(clientIndex);
			}

			// send message to spawn the player at the clients
			/*outMsg = new idBitMsg();
			outMsg.InitGame();
			outMsg.BeginWriting();
			outMsg.WriteByte((int) GameReliableMessage.SpawnPlayer);
			outMsg.WriteByte(clientIndex);
			outMsg.WriteLong(_spawnIds[clientIndex]);*/

			/*idR.NetworkSystem.ServerSendReliableMessage(-1, outMsg);*/
		}

		public override void SpawnPlayer(int clientIndex)
		{
			idConsole.WriteLine("SpawnPlayer: {0}", clientIndex);

			idDict args = new idDict();
			args.Set("spawn_entnum", clientIndex);
			args.Set("name", string.Format("player{0}", clientIndex + 1));

			// TODO: refactor in to idGameRules
			args.Set("classname", (this.IsMultiplayer == true) ? "player_doommarine_mp" : "player_doommarine");

			idEntity ent = SpawnEntityDef(args);

			if((ent == null) || (_entities[clientIndex] == null))
			{
				idConsole.Error("Failed to spawn player as {0}", args.GetString("classname"));
			}

			// make sure it's a compatible class
			if((ent is idPlayer) == false)
			{
				idConsole.Error("'{0}' spawn the player as a '{1}'.  Player spawnclass must be a subclass of Player.", args.GetString("classname"), ent.ClassName);
			}

			if(clientIndex >= _clientCount)
			{
				_clientCount++;
			}

			this.Rules.SpawnPlayer(clientIndex);
		}
		#endregion
	}
}