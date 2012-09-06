using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Game;
using idTech4.Game.Physics;
using idTech4.Game.Rules;
using idTech4.Math;
using idTech4.Net;
using idTech4.Renderer;
using idTech4.Text.Decl;
using idTech4.UI;

namespace idTech4.Game.Entities
{
	public class idPlayer : idActor
	{
		#region Constants
		public static readonly int SpectateRaise = 25;

		public static readonly Vector3[] ColorBarTable = new Vector3[] {
			new Vector3(0.25f, 0.25f, 0.25f),
			new Vector3(1.00f, 0.00f, 0.00f),
			new Vector3(0.00f, 0.80f, 0.10f),
			new Vector3(0.20f, 0.50f, 0.80f),
			new Vector3(1.00f, 0.80f, 0.10f)
		};
		#endregion

		#region Properties
		#region General
		public idUserInterface Hud
		{
			get
			{
				return _hud;
			}
		}

		public idDict Info
		{
			get
			{
				return idR.Game.UserInfo[this.Index];
			}
		}

		public PlayerView View
		{
			get
			{
				return _view;
			}
		}
		#endregion

		#region Multiplayer
		public string BaseSkin
		{
			get
			{
				return _baseSkin;
			}
			set
			{
				_baseSkin = value;
			}
		}

		public Vector3 ColorBar
		{
			get
			{
				return _colorBar;
			}
			set
			{
				_colorBar = value;
			}
		}

		public int ColorBarIndex
		{
			get
			{
				return _colorBarIndex;
			}
			set
			{
				_colorBarIndex = value;
			}
		}

		public bool ForceReady
		{
			get
			{
				return _forcedReady;
			}
			set
			{
				_forcedReady = value;
			}
		}

		public bool ForceRespawn
		{
			get
			{
				return _forceRespawn;
			}
			set
			{
				_forceRespawn = value;
			}
		}

		/// <summary>
		/// Replicated from server, true if the player is chatting.
		/// </summary>
		public bool IsChatting
		{
			get
			{
				return _isChatting;
			}
			set
			{
				_isChatting = value;
			}
		}

		public bool IsReady
		{
			get
			{
				return _ready;
			}
			set
			{
				_ready = value;
			}
		}

		public bool IsSpectating
		{
			get
			{
				return _spectating;
			}
			set
			{
				_spectating = value;
			}
		}

		public PlayerTeam LatchedTeam
		{
			get
			{
				return _latchedTeam;
			}
			set
			{
				_latchedTeam = value;
			}
		}

		/// <summary>
		/// When the client first enters the game.
		/// </summary>
		public int SpawnedTime
		{
			get
			{
				return _spawnedTime;
			}
			set
			{
				_spawnedTime = value;
			}
		}

		/// <summary>
		/// For tourney cycling - the higher, the more likely to play next - server.
		/// </summary>
		public int TourneyRank
		{
			get
			{
				return _tourneyRank;
			}
			set
			{
				_tourneyRank = value;
			}
		}

		public bool WantToSpectate
		{
			get
			{
				return _wantToSpectate;
			}
			set
			{
				_wantToSpectate = value;
			}
		}
		#endregion
		#endregion

		#region Members
		private idPhysics_Player _physicsObject; // player physics

		private bool _showWeaponViewModel;

		// handles damage kicks and effects
		private PlayerView _view;

		private idUserInterface _hud;
		private idUserInterface _cursor;

		private idDeclSkin _skin;
		private idDeclSkin _powerUpSkin;
		private string _baseSkin;

		// mp
		private bool _spectating;
		private bool _wantToSpectate;

		private bool _forceRespawn;
		private bool _respawning;

		private bool _ready;
		private bool _forcedReady;

		private bool _isChatting;

		private PlayerTeam _latchedTeam;

		private Vector3 _colorBar;
		private int _colorBarIndex;

		private int _spawnedTime;
		private int _tourneyRank;
		#endregion

		#region Constructor
		public idPlayer()
			: base()
		{
			_view = new PlayerView(this);
			/* TODO: memset( &usercmd, 0, sizeof( usercmd ) );

			 
			noclip					= false;
			godmode					= false;

			spawnAnglesSet			= false;
			spawnAngles				= ang_zero;
			viewAngles				= ang_zero;
			cmdAngles				= ang_zero;

			oldButtons				= 0;
			buttonMask				= 0;
			oldFlags				= 0;

			lastHitTime				= 0;
			lastSndHitTime			= 0;
			lastSavingThrowTime		= 0;

			objectiveSystemOpen		= false;

			heartRate				= BASE_HEARTRATE;
			heartInfo.Init( 0, 0, 0, 0 );
			lastHeartAdjust			= 0;
			lastHeartBeat			= 0;
			lastDmgTime				= 0;
			deathClearContentsTime	= 0;
			lastArmorPulse			= -10000;
			stamina					= 0.0f;
			healthPool				= 0.0f;
			nextHealthPulse			= 0;
			healthPulse				= false;
			nextHealthTake			= 0;
			healthTake				= false;

			scoreBoardOpen			= false;
			forceScoreBoard			= false;*/
			/*spectator				= 0;*/
			_colorBar = Vector3.Zero;
			_colorBarIndex = 0;

			/*lastHitToggle			= false;

			minRespawnTime			= 0;
			maxRespawnTime			= 0;

			firstPersonViewOrigin	= vec3_zero;
			firstPersonViewAxis		= mat3_identity;

			hipJoint				= INVALID_JOINT;
			chestJoint				= INVALID_JOINT;
			headJoint				= INVALID_JOINT;

			bobFoot					= 0;
			bobFrac					= 0.0f;
			bobfracsin				= 0.0f;
			bobCycle				= 0;
			xyspeed					= 0.0f;
			stepUpTime				= 0;
			stepUpDelta				= 0.0f;
			idealLegsYaw			= 0.0f;
			legsYaw					= 0.0f;
			legsForward				= true;
			oldViewYaw				= 0.0f;
			viewBobAngles			= ang_zero;
			viewBob					= vec3_zero;
			landChange				= 0;
			landTime				= 0;

			currentWeapon			= -1;
			idealWeapon				= -1;
			previousWeapon			= -1;
			weaponSwitchTime		=  0;
			weaponEnabled			= true;
			weapon_soulcube			= -1;
			weapon_pda				= -1;
			weapon_fists			= -1;*/

			_showWeaponViewModel = true;

			_baseSkin = string.Empty;

			/*numProjectilesFired		= 0;
			numProjectileHits		= 0;

			airless					= false;
			airTics					= 0;
			lastAirDamage			= 0;

			gibDeath				= false;
			gibsLaunched			= false;
			gibsDir					= vec3_zero;

			zoomFov.Init( 0, 0, 0, 0 );
			centerView.Init( 0, 0, 0, 0 );
			fxFov					= false;

			influenceFov			= 0;
			influenceActive			= 0;
			influenceRadius			= 0.0f;
			influenceEntity			= NULL;
			influenceMaterial		= NULL;
			influenceSkin			= NULL;

			privateCameraView		= NULL;

			memset( loggedViewAngles, 0, sizeof( loggedViewAngles ) );
			memset( loggedAccel, 0, sizeof( loggedAccel ) );
			currentLoggedAccel	= 0;

			focusTime				= 0;
			talkCursor				= 0;
	
			oldMouseX				= 0;
			oldMouseY				= 0;

			pdaAudio				= "";
			pdaVideo				= "";
			pdaVideoWave			= "";

			lastDamageDef			= 0;
			lastDamageDir			= vec3_zero;
			lastDamageLocation		= 0;
			smoothedFrame			= 0;
			smoothedOriginUpdated	= false;
			smoothedOrigin			= vec3_zero;
			smoothedAngles			= ang_zero;

			fl.networkSync			= true;

			latchedTeam				= -1;
			doingDeathSkin			= false;
			weaponGone				= false;
			useInitialSpawns		= false;
			tourneyRank				= 0;
			lastSpectateTeleport	= 0;
			tourneyLine				= 0;
			hiddenWeapon			= false;
			tipUp					= false;
			objectiveUp				= false;
			teleportEntity			= NULL;
			teleportKiller			= -1;*/
			_ready = false;
			/*leader					= false;
			lastSpectateChange		= 0;
			lastTeleFX				= -9999;
			weaponCatchup			= false;
			lastSnapshotSequence	= 0;

			MPAim					= -1;
			lastMPAim				= -1;
			lastMPAimTime			= 0;
			MPAimFadeTime			= 0;
			MPAimHighlight			= false;
*/
			_spawnedTime = 0;
			/*
			lastManOver				= false;
			lastManPlayAgain		= false;
			lastManPresent			= false;

			isTelefragged			= false;

			isLagged				= false;*/
			_isChatting = false;

			/*selfSmooth				= false;*/
		}
		#endregion

		#region Methods
		public void Init()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.Init");

			string value;
			// TODO
			// TODO: idKeyValue kv;
			/*
const idKeyValue	*kv;

noclip					= false;
godmode					= false;

oldButtons				= 0;
oldFlags				= 0;

currentWeapon			= -1;
idealWeapon				= -1;
previousWeapon			= -1;
weaponSwitchTime		= 0;
weaponEnabled			= true;
weapon_soulcube			= SlotForWeapon( "weapon_soulcube" );
weapon_pda				= SlotForWeapon( "weapon_pda" );
weapon_fists			= SlotForWeapon( "weapon_fists" );
showWeaponViewModel		= GetUserInfo()->GetBool( "ui_showGun" );


lastDmgTime				= 0;
lastArmorPulse			= -10000;
lastHeartAdjust			= 0;
lastHeartBeat			= 0;
heartInfo.Init( 0, 0, 0, 0 );

bobCycle				= 0;
bobFrac					= 0.0f;
landChange				= 0;
landTime				= 0;
zoomFov.Init( 0, 0, 0, 0 );
centerView.Init( 0, 0, 0, 0 );
fxFov					= false;

influenceFov			= 0;
influenceActive			= 0;
influenceRadius			= 0.0f;
influenceEntity			= NULL;
influenceMaterial		= NULL;
influenceSkin			= NULL;

currentLoggedAccel		= 0;

focusTime				= 0;
focusGUIent				= NULL;
focusUI					= NULL;
focusCharacter			= NULL;
talkCursor				= 0;
focusVehicle			= NULL;

// remove any damage effects
playerView.ClearEffects();

// damage values
fl.takedamage			= true;
ClearPain();

// restore persistent data
RestorePersistantInfo();

bobCycle		= 0;
stamina			= 0.0f;
healthPool		= 0.0f;
nextHealthPulse = 0;
healthPulse		= false;
nextHealthTake	= 0;
healthTake		= false;

SetupWeaponEntity();
currentWeapon = -1;
previousWeapon = -1;

heartRate = BASE_HEARTRATE;
AdjustHeartRate( BASE_HEARTRATE, 0.0f, 0.0f, true );

idealLegsYaw = 0.0f;
legsYaw = 0.0f;
legsForward	= true;
oldViewYaw = 0.0f;*/

			// set the pm_ cvars
			if((idR.Game.IsMultiplayer == false) || (idR.Game.IsServer == true))
			{
				idConsole.Warning("TODO: player pm_");
				/*kv = this.SpawnArgs.MatchPrefix("pm_", null);

				while(kv != null)
				{
					idR.CvarSystem.SetString(kv.Key, kv.Value);
					kv = this.SpawnArgs.MatchPrefix("pm_", kv);
				}*/
			}

			// disable stamina on hell levels
			/*if(gameLocal.world && gameLocal.world->spawnArgs.GetBool("no_stamina"))
			{
				pm_stamina.SetFloat(0.0f);
			}

			// TODO
			// stamina always initialized to maximum
			/*stamina = pm_stamina.GetFloat();

			// air always initialized to maximum too
			airTics = pm_airTics.GetFloat();
			airless = false;

			gibDeath = false;
			gibsLaunched = false;
			gibsDir.Zero();*/

			// set the gravity
			_physicsObject = new idPhysics_Player();
			_physicsObject.Gravity = idR.Game.Gravity;

			// start out standing
			/*SetEyeHeight(pm_normalviewheight.GetFloat());

			stepUpTime = 0;
			stepUpDelta = 0.0f;
			viewBobAngles.Zero();
			viewBob.Zero();*/

			value = this.SpawnArgs.GetString("model", "");

			if(value != string.Empty)
			{
				SetModel(value);
			}

			if(_cursor != null)
			{
				_cursor.State.Set("talkcursor", 0);
				_cursor.State.Set("combatcursor", "1");
				_cursor.State.Set("itemcursor", "0");
				_cursor.State.Set("guicursor", "0");
			}

			if(((idR.Game.IsMultiplayer == true) || (idR.CvarSystem.GetBool("g_testDeath") == true)) && (_skin != null))
			{
				this.Skin = _skin;
				this.RenderEntity.MaterialParameters[6] = 0.0f;
			}
			else
			{
				string skin = this.SpawnArgs.GetString("spawn_skin", null);

				if(skin != null)
				{
					_skin = idR.DeclManager.FindSkin(skin);

					this.Skin = _skin;
					this.RenderEntity.MaterialParameters[6] = 0.0f;
				}
			}

			/* TODO: value = spawnArgs.GetString("bone_hips", "");
			hipJoint = animator.GetJointHandle(value);
			if(hipJoint == INVALID_JOINT)
			{
				gameLocal.Error("Joint '%s' not found for 'bone_hips' on '%s'", value, name.c_str());
			}

			value = spawnArgs.GetString("bone_chest", "");
			chestJoint = animator.GetJointHandle(value);
			if(chestJoint == INVALID_JOINT)
			{
				gameLocal.Error("Joint '%s' not found for 'bone_chest' on '%s'", value, name.c_str());
			}

			value = spawnArgs.GetString("bone_head", "");
			headJoint = animator.GetJointHandle(value);
			if(headJoint == INVALID_JOINT)
			{
				gameLocal.Error("Joint '%s' not found for 'bone_head' on '%s'", value, name.c_str());
			}

			// initialize the script variables
			AI_FORWARD = false;
			AI_BACKWARD = false;
			AI_STRAFE_LEFT = false;
			AI_STRAFE_RIGHT = false;
			AI_ATTACK_HELD = false;
			AI_WEAPON_FIRED = false;
			AI_JUMP = false;
			AI_DEAD = false;
			AI_CROUCH = false;
			AI_ONGROUND = true;
			AI_ONLADDER = false;
			AI_HARDLANDING = false;
			AI_SOFTLANDING = false;
			AI_RUN = false;
			AI_PAIN = false;
			AI_RELOAD = false;
			AI_TELEPORT = false;
			AI_TURN_LEFT = false;
			AI_TURN_RIGHT = false;

			// reset the script object
			ConstructScriptObject();

			// execute the script so the script object's constructor takes effect immediately
			scriptThread->Execute();*/

			// TODO: forceScoreBoard = false;
			this.ForceReady = false;


			/*privateCameraView = NULL;

			lastSpectateChange = 0;
			lastTeleFX = -9999;

			hiddenWeapon = false;
			tipUp = false;
			objectiveUp = false;
			teleportEntity = NULL;
			teleportKiller = -1;
			leader = false;

			SetPrivateCameraView(NULL);

			lastSnapshotSequence = 0;

			MPAim = -1;
			lastMPAim = -1;
			lastMPAimTime = 0;
			MPAimFadeTime = 0;
			MPAimHighlight = false;*/

			if(_hud != null)
			{
				_hud.HandleNamedEvent("aim_clear");
			}

			idR.CvarSystem.SetBool("ui_chat", false);
		}

		/// <summary>
		/// Try to find a spawn point marked 'initial', otherwise use normal spawn selection.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="angles"></param>
		public void SelectInitialSpawnPoint(out Vector3 origin, out idAngles angles)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idEntity spot = idR.Game.SelectInitialSpawnPoint(this);
			string skin = spot.SpawnArgs.GetString("skin", null);

			// set the player skin from the spawn location
			if(skin != null)
			{
				this.SpawnArgs.Set("spawn_skin", skin);
			}

			// activate the spawn locations targets
			idConsole.Warning("TODO: spot->PostEventMS(&EV_ActivateTargets, 0, this);");

			origin = spot.Physics.GetOrigin();
			origin.Z += 4.0f + idClipModel.BoxEpsilon; // move up to make sure the player is at least an epsilon above the floor

			angles = idHelper.AxisToAngles(spot.Physics.GetAxis());
		}

		/// <summary>
		/// Chooses a spawn point and spawns the player.
		/// </summary>
		public void SpawnFromSpawnSpot()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			Vector3 origin;
			idAngles angles;

			SelectInitialSpawnPoint(out origin, out angles);
			SpawnToPoint(origin, angles);
		}

		/// <summary>
		/// Called every time a client is placed fresh in the world: after the first ClientBegin, and after each respawn
		/// Initializes all non-persistant parts of playerState when called here with spectating set to true, just place yourself and init.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="angles"></param>
		public void SpawnToPoint(Vector3 origin, idAngles angles)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			Vector3 spectatorOrigin = Vector3.Zero;

			_respawning = true;

			Init();

			// TODO
			/*fl.noknockback = false;

			// stop any ragdolls being used
			StopRagdoll();*/

			// set back the player physics
			SetPhysics(_physicsObject);

			_physicsObject.SetClipModelAxis();
			_physicsObject.EnableClip();

			/*if ( !spectating ) {
				SetCombatContents( true );
			}*/

			_physicsObject.SetLinearVelocity(Vector3.Zero);

			// setup our initial view
			if(this.IsSpectating == false)
			{
				this.Origin = origin;
			}
			else
			{
				spectatorOrigin = origin;
				spectatorOrigin.Z += idR.CvarSystem.GetFloat("pm_normalheight");
				spectatorOrigin.Z += SpectateRaise;

				this.Origin = spectatorOrigin;
			}

			// if this is the first spawn of the map, we don't have a usercmd yet,
			// so the delta angles won't be correct.  This will be fixed on the first think.
			// TODO
			/*viewAngles = ang_zero;
			SetDeltaViewAngles( ang_zero );
			SetViewAngles( spawn_angles );
			spawnAngles = spawn_angles;
			spawnAnglesSet = false;

			legsForward = true;
			legsYaw = 0.0f;
			idealLegsYaw = 0.0f;
			oldViewYaw = viewAngles.yaw;*/
			
			if(this.IsSpectating == true)
			{
				Hide();
			}
			else
			{
				Show();
			}

			// TODO
			/*if ( gameLocal.isMultiplayer ) {
				if ( !spectating ) {
					// we may be called twice in a row in some situations. avoid a double fx and 'fly to the roof'
					if ( lastTeleFX < gameLocal.time - 1000 ) {
						idEntityFx::StartFx( spawnArgs.GetString( "fx_spawn" ), &spawn_origin, NULL, this, true );
						lastTeleFX = gameLocal.time;
					}
				}
				AI_TELEPORT = true;
			} else {
				AI_TELEPORT = false;
			}*/

			// TODO
			// kill anything at the new position
			/*if ( !spectating ) {
				physicsObj.SetClipMask( MASK_PLAYERSOLID ); // the clip mask is usually maintained in Move(), but KillBox requires it
				gameLocal.KillBox( this );
			}*/

			// don't allow full run speed for a bit
			//physicsObj.SetKnockBack( 100 );

			// set our respawn time and buttons so that if we're killed we don't respawn immediately
			/*minRespawnTime = gameLocal.time;
			maxRespawnTime = gameLocal.time;*/

			if(this.IsSpectating == false)
			{
				this.ForceRespawn = false;
			}

			// TODO: privateCameraView = NULL;

			// TODO: BecomeActive( TH_THINK );

			// run a client frame to drop exactly to the floor,
			// initialize animations and other things
			Think();

			_respawning = false;

			/*lastManOver			= false;
			lastManPlayAgain	= false;
			isTelefragged		= false;*/
		}

		public bool UserInfoChanged(bool canModify)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			bool modifiedInfo = false;
			idDict userInfo = this.Info;

			_showWeaponViewModel = userInfo.GetBool("ui_showGun");

			return modifiedInfo;
		}
		#endregion

		#region idActor implementation
		#region Properties
		public override bool IsOnLadder
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idPlayer.IsOnLadder");

				return false;
			}
		}

		public override idRenderView RenderView
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idPlayer.RenderView");

				return null;
			}
		}
		#endregion

		#region Methods
		public override void ClientPredictionThink()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.ClientPredictionThink");
		}

		public override bool ClientReceiveEvent(int ev, int time, idBitMsg msg)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.ClientReceiveEvent");

			return false;
		}

		public override bool Collide(object collision, Vector3 velocity)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.Collide");

			return false;
		}

		public override void Damage(idEntity inflictor, idEntity attacker, Vector3 direction, string damageDefName, float damageScale, int location)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.Damage");
		}

		public override void DamageFeedback(idEntity victim, idEntity inflictor, ref int damage)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.GetAASLocation");
		}

		public override void GetAASLocation(object aas, Vector3 position, ref int areaNum)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.GetAASLocation");
		}

		public override bool GetPhysicsToSoundTransform(ref Vector3 origin, ref Matrix axis)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.GetPhysicsToSoundTransform");

			return false;
		}

		public override bool GetPhysicsToVisualTransform(ref Vector3 origin, ref Matrix axis)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.GetPhysicsToVisualTransform");

			return false;
		}

		public override bool HandleSingleGuiCommand(idEntity entityGui, Text.idLexer lexer)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.HandleSingleGuiCommand");

			return false;
		}

		public override void Hide()
		{
			base.Hide();

			idConsole.Warning("TODO: idPlayer.Hide");
			// TODO
			/*	idActor::Hide();
	weap = weapon.GetEntity();
	if ( weap ) {
		weap->HideWorldModel();
	}*/
		}

		public override void ReadFromSnapshot(idBitMsgDelta msg)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.ReadFromSnapshot");
		}

		public override void Restart()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.Restart");
		}

		public override void Restore(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.Restore");
		}

		public override void Save(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.Save");
		}

		public override bool ServerReceiveEvent(int ev, int time, idBitMsg msg)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.ServerReceiveEvent");

			return false;
		}

		public override void Show()
		{
			base.Show();

			idConsole.Warning("TODO: idPlayer.Show");
			// TODO
			/*idWeapon *weap;
	
idActor::Show();
weap = weapon.GetEntity();
if ( weap ) {
	weap->ShowWorldModel();
}*/
		}

		public override void Spawn()
		{
			base.Spawn();

			if(this.Index >= idR.MaxClients)
			{
				idConsole.Error("entityIndex > MAX_CLIENTS for player.  Player may only be spawned with a client.");
			}

			// allow thinking during cinematics
			this.Cinematic = true;

			if(idR.Game.IsMultiplayer == true)
			{
				// always start in spectating state waiting to be spawned in
				// do this before SetClipModel to get the right bounding box
				this.IsSpectating = true;
			}

			// set our collision model
			_physicsObject.Self = this;
			// TODO
			/*
			SetClipModel();*/

			_physicsObject.SetMass(this.SpawnArgs.GetFloat("mass", 100));
			_physicsObject.SetContents(ContentFlags.Body);
			_physicsObject.SetClipMask(ContentFlags.MaskPlayerSolid);

			SetPhysics(_physicsObject);

			idConsole.Warning("TODO: InitAASLocation();");

			_skin = this.RenderEntity.CustomSkin;

			// only the local player needs guis
			if((idR.Game.IsMultiplayer == false) || (this.Index == idR.Game.LocalClientIndex))
			{
				// load HUD
				if(idR.Game.IsMultiplayer == true)
				{
					_hud = idR.UIManager.FindInterface("guis/mphud.gui", true, false, true);
				}
				else
				{
					string temp = this.SpawnArgs.GetString("hud", "");

					if(temp != string.Empty)
					{
						_hud = idR.UIManager.FindInterface(temp, true, false, true);
					}
				}

				if(_hud != null)
				{
					_hud.Activate(true, idR.Game.Time);
				}

				// load cursor
				string cursor = this.SpawnArgs.GetString("cursor", "");

				if(cursor != string.Empty)
				{
					_cursor = idR.UIManager.FindInterface(cursor, true, idR.Game.IsMultiplayer, idR.Game.IsMultiplayer);
				}

				if(_cursor != null)
				{
					_cursor.Activate(true, idR.Game.Time);
				}

				// TODO
				// objectiveSystem = uiManager->FindGui( "guis/pda.gui", true, false, true );
				// objectiveSystemOpen = false;
			}

			/*SetLastHitTime( 0 );

			// load the armor sound feedback
			declManager->FindSound( "player_sounds_hitArmor" );

			// set up conditions for animation
			LinkScriptVariables();

			animator.RemoveOriginOffset( true );*/

			// initialize user info related settings
			// on server, we wait for the userinfo broadcast, as this controls when the player is initially spawned in game
			if((idR.Game.IsClient == true) || (this.Index == idR.Game.LocalClientIndex))
			{
				UserInfoChanged(false);
			}

			// create combat collision hull for exact collision detection
			/*SetCombatModel();*/

			// supress model in non-player views, but allow it in mirrors and remote views
			this.RenderEntity.SuppressSurfaceInViewID = this.Index + 1;

			// don't project shadow on self or weapon
			this.RenderEntity.NoSelfShadow = true;

			/*idAFAttachment *headEnt = head.GetEntity();
			if ( headEnt ) {
				headEnt->GetRenderEntity()->suppressSurfaceInViewID = entityNumber+1;
				headEnt->GetRenderEntity()->noSelfShadow = true;
			}*/

			if(idR.Game.IsMultiplayer == true)
			{
				Init();
				Hide();	// properly hidden if starting as a spectator

				if(idR.Game.IsClient == false)
				{
					// set yourself ready to spawn. idMultiplayerGame will decide when/if appropriate and call SpawnFromSpawnSpot
					// TODO
					/*SetupWeaponEntity();*/
					SpawnFromSpawnSpot();

					_forceRespawn = true;
				}
			}
			else
			{
				// TODO: SetupWeaponEntity();
				SpawnFromSpawnSpot();
			}
			/*
			// trigger playtesting item gives, if we didn't get here from a previous level
			// the devmap key will be set on the first devmap, but cleared on any level
			// transitions
			if ( !gameLocal.isMultiplayer && gameLocal.serverInfo.FindKey( "devmap" ) ) {
				// fire a trigger with the name "devmap"
				idEntity *ent = gameLocal.FindEntity( "devmap" );
				if ( ent ) {
					ent->ActivateTargets( this );
				}
			}
			if ( hud ) {
				// We can spawn with a full soul cube, so we need to make sure the hud knows this
				if ( weapon_soulcube > 0 && ( inventory.weapons & ( 1 << weapon_soulcube ) ) ) {
					int max_souls = inventory.MaxAmmoForAmmoClass( this, "ammo_souls" );
					if ( inventory.ammo[ idWeapon::GetAmmoNumForName( "ammo_souls" ) ] >= max_souls ) {
						hud->HandleNamedEvent( "soulCubeReady" );
					}
				}
				hud->HandleNamedEvent( "itemPickup" );
			}

			if ( GetPDA() ) {
				// Add any emails from the inventory
				for ( int i = 0; i < inventory.emails.Num(); i++ ) {
					GetPDA()->AddEmail( inventory.emails[i] );
				}
				GetPDA()->SetSecurity( common->GetLanguageDict()->GetString( "#str_00066" ) );
			}

			if ( gameLocal.world->spawnArgs.GetBool( "no_Weapons" ) ) {
				hiddenWeapon = true;
				if ( weapon.GetEntity() ) {
					weapon.GetEntity()->LowerWeapon();
				}
				idealWeapon = 0;
			} else {
				hiddenWeapon = false;
			}*/

			if(_hud != null)
			{
				// TODO: UpdateHudWeapon();
				_hud.StateChanged(idR.Game.Time);
			}

			/*tipUp = false;
			objectiveUp = false;

			if ( inventory.levelTriggers.Num() ) {
				PostEventMS( &EV_Player_LevelTrigger, 0 );
			}

			inventory.pdaOpened = false;
			inventory.selPDA = 0;

			if ( !gameLocal.isMultiplayer ) {
				if ( g_skill.GetInteger() < 2 ) {
					if ( health < 25 ) {
						health = 25;
					}
					if ( g_useDynamicProtection.GetBool() ) {
						g_damageScale.SetFloat( 1.0f );
					}
				} else {
					g_damageScale.SetFloat( 1.0f );
					g_armorProtection.SetFloat( ( g_skill.GetInteger() < 2 ) ? 0.4f : 0.2f );
		#ifndef ID_DEMO_BUILD
					if ( g_skill.GetInteger() == 3 ) {
						healthTake = true;
						nextHealthTake = gameLocal.time + g_healthTakeTime.GetInteger() * 1000;
					}
		#endif
				}
			}*/
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Use exitEntityNum to specify a teleport with private camera view and delayed exit.
		/// </remarks>
		/// <param name="origin"></param>
		/// <param name="angles"></param>
		/// <param name="destination"></param>
		public override void Teleport(Vector3 origin, idAngles angles, idEntity destination)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.Teleport");
		}

		public override void Think()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.Think");
			// TODO
			/*renderEntity_t* headRenderEnt;

			UpdatePlayerIcons();

			// latch button actions
			oldButtons = usercmd.buttons;

			// grab out usercmd
			usercmd_t oldCmd = usercmd;
			usercmd = gameLocal.usercmds[entityNumber];
			buttonMask &= usercmd.buttons;
			usercmd.buttons &= ~buttonMask;

			if(gameLocal.inCinematic && gameLocal.skipCinematic)
			{
				return;
			}

			// clear the ik before we do anything else so the skeleton doesn't get updated twice
			walkIK.ClearJointMods();

			// if this is the very first frame of the map, set the delta view angles
			// based on the usercmd angles
			if(!spawnAnglesSet && (gameLocal.GameState() != GAMESTATE_STARTUP))
			{
				spawnAnglesSet = true;
				SetViewAngles(spawnAngles);
				oldFlags = usercmd.flags;
			}

			if(objectiveSystemOpen || gameLocal.inCinematic || influenceActive)
			{
				if(objectiveSystemOpen && AI_PAIN)
				{
					TogglePDA();
				}
				usercmd.forwardmove = 0;
				usercmd.rightmove = 0;
				usercmd.upmove = 0;
			}

			// log movement changes for weapon bobbing effects
			if(usercmd.forwardmove != oldCmd.forwardmove)
			{
				loggedAccel_t* acc = &loggedAccel[currentLoggedAccel & (NUM_LOGGED_ACCELS - 1)];
				currentLoggedAccel++;
				acc->time = gameLocal.time;
				acc->dir[0] = usercmd.forwardmove - oldCmd.forwardmove;
				acc->dir[1] = acc->dir[2] = 0;
			}

			if(usercmd.rightmove != oldCmd.rightmove)
			{
				loggedAccel_t* acc = &loggedAccel[currentLoggedAccel & (NUM_LOGGED_ACCELS - 1)];
				currentLoggedAccel++;
				acc->time = gameLocal.time;
				acc->dir[1] = usercmd.rightmove - oldCmd.rightmove;
				acc->dir[0] = acc->dir[2] = 0;
			}

			// freelook centering
			if((usercmd.buttons ^ oldCmd.buttons) & BUTTON_MLOOK)
			{
				centerView.Init(gameLocal.time, 200, viewAngles.pitch, 0);
			}

			// zooming
			if((usercmd.buttons ^ oldCmd.buttons) & BUTTON_ZOOM)
			{
				if((usercmd.buttons & BUTTON_ZOOM) && weapon.GetEntity())
				{
					zoomFov.Init(gameLocal.time, 200.0f, CalcFov(false), weapon.GetEntity()->GetZoomFov());
				}
				else
				{
					zoomFov.Init(gameLocal.time, 200.0f, zoomFov.GetCurrentValue(gameLocal.time), DefaultFov());
				}
			}

			// if we have an active gui, we will unrotate the view angles as
			// we turn the mouse movements into gui events
			idUserInterface* gui = ActiveGui();
			if(gui && gui != focusUI)
			{
				RouteGuiMouse(gui);
			}

			// set the push velocity on the weapon before running the physics
			if(weapon.GetEntity())
			{
				weapon.GetEntity()->SetPushVelocity(physicsObj.GetPushedLinearVelocity());
			}

			EvaluateControls();

			if(!af.IsActive())
			{
				AdjustBodyAngles();
				CopyJointsFromBodyToHead();
			}

			Move();

			if(!g_stopTime.GetBool())
			{

				if(!noclip && !spectating && (health > 0) && !IsHidden())
				{
					TouchTriggers();
				}

				// not done on clients for various reasons. don't do it on server and save the sound channel for other things
				if(!gameLocal.isMultiplayer)
				{
					SetCurrentHeartRate();
					float scale = g_damageScale.GetFloat();
					if(g_useDynamicProtection.GetBool() && scale < 1.0f && gameLocal.time - lastDmgTime > 500)
					{
						if(scale < 1.0f)
						{
							scale += 0.05f;
						}
						if(scale > 1.0f)
						{
							scale = 1.0f;
						}
						g_damageScale.SetFloat(scale);
					}
				}

				// update GUIs, Items, and character interactions
				UpdateFocus();

				UpdateLocation();

				// update player script
				UpdateScript();

				// service animations
				if(!spectating && !af.IsActive() && !gameLocal.inCinematic)
				{
					UpdateConditions();
					UpdateAnimState();
					CheckBlink();
				}

				// clear out our pain flag so we can tell if we recieve any damage between now and the next time we think
				AI_PAIN = false;
			}

			// calculate the exact bobbed view position, which is used to
			// position the view weapon, among other things
			CalculateFirstPersonView();

			// this may use firstPersonView, or a thirdPerson / camera view
			CalculateRenderView();

			inventory.UpdateArmor();

			if(spectating)
			{
				UpdateSpectating();
			}
			else if(health > 0)
			{
				UpdateWeapon();
			}

			UpdateAir();

			UpdateHud();

			UpdatePowerUps();

			UpdateDeathSkin(false);

			if(gameLocal.isMultiplayer)
			{
				DrawPlayerIcons();
			}

			if(head.GetEntity())
			{
				headRenderEnt = head.GetEntity()->GetRenderEntity();
			}
			else
			{
				headRenderEnt = NULL;
			}

			if(headRenderEnt)
			{
				if(influenceSkin)
				{
					headRenderEnt->customSkin = influenceSkin;
				}
				else
				{
					headRenderEnt->customSkin = NULL;
				}
			}

			if(gameLocal.isMultiplayer || g_showPlayerShadow.GetBool())
			{
				renderEntity.suppressShadowInViewID = 0;
				if(headRenderEnt)
				{
					headRenderEnt->suppressShadowInViewID = 0;
				}
			}
			else
			{
				renderEntity.suppressShadowInViewID = entityNumber + 1;
				if(headRenderEnt)
				{
					headRenderEnt->suppressShadowInViewID = entityNumber + 1;
				}
			}
			// never cast shadows from our first-person muzzle flashes
			renderEntity.suppressShadowInLightID = LIGHTID_VIEW_MUZZLE_FLASH + entityNumber;
			if(headRenderEnt)
			{
				headRenderEnt->suppressShadowInLightID = LIGHTID_VIEW_MUZZLE_FLASH + entityNumber;
			}

			if(!g_stopTime.GetBool())
			{
				UpdateAnimation();

				Present();

				UpdateDamageEffects();

				LinkCombat();

				playerView.CalculateShake();
			}

			if(!(thinkFlags & TH_THINK))
			{
				gameLocal.Printf("player %d not thinking?\n", entityNumber);
			}

			if(g_showEnemies.GetBool())
			{
				idActor* ent;
				int num = 0;
				for(ent = enemyList.Next(); ent != NULL; ent = ent->enemyNode.Next())
				{
					gameLocal.Printf("enemy (%d)'%s'\n", ent->entityNumber, ent->name.c_str());
					gameRenderWorld->DebugBounds(colorRed, ent->GetPhysics()->GetBounds().Expand(2), ent->GetPhysics()->GetOrigin());
					num++;
				}
				gameLocal.Printf("%d: enemies\n", num);
			}*/
		}

		public override void WriteToSnapshot(idBitMsgDelta msg)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPlayer.WriteToSnapshot");
		}
		#endregion
		#endregion
	}

	public class PlayerView
	{
		#region Members
		private idPlayer _player;
		#endregion

		#region Constructor
		public PlayerView(idPlayer player)
		{
			_player = player;

			/*memset(screenBlobs, 0, sizeof(screenBlobs));
			dvMaterial = declManager->FindMaterial("_scratch");
			tunnelMaterial = declManager->FindMaterial("textures/decals/tunnel");
			armorMaterial = declManager->FindMaterial("armorViewEffect");
			berserkMaterial = declManager->FindMaterial("textures/decals/berserk");
			irGogglesMaterial = declManager->FindMaterial("textures/decals/irblend");
			bloodSprayMaterial = declManager->FindMaterial("textures/decals/bloodspray");
			bfgMaterial = declManager->FindMaterial("textures/decals/bfgvision");
			lagoMaterial = declManager->FindMaterial(LAGO_MATERIAL, false);
			bfgVision = false;
			dvFinishTime = 0;
			kickFinishTime = 0;
			kickAngles.Zero();
			lastDamageTime = 0.0f;
			fadeTime = 0;
			fadeRate = 0.0;
			fadeFromColor.Zero();
			fadeToColor.Zero();
			fadeColor.Zero();
			shakeAng.Zero();

			ClearEffects();*/
		}
		#endregion

		#region Methods
		public void Draw(idUserInterface hud)
		{
			/*const renderView_t* view = player->GetRenderView();

			if(g_skipViewEffects.GetBool())
			{
				SingleView(hud, view);
			}
			else
			{
				if(player->GetInfluenceMaterial() || player->GetInfluenceEntity())
				{
					InfluenceVision(hud, view);
				}
				else if(gameLocal.time < dvFinishTime)
				{
					DoubleVision(hud, view, dvFinishTime - gameLocal.time);
				}
				else if(player->PowerUpActive(BERSERK))
				{
					BerserkVision(hud, view);
				}
				else
				{
					SingleView(hud, view);
				}
				ScreenFade();
			}

			if(net_clientLagOMeter.GetBool() && lagoMaterial && gameLocal.isClient)
			{
				renderSystem->SetColor4(1.0f, 1.0f, 1.0f, 1.0f);
				renderSystem->DrawStretchPic(10.0f, 380.0f, 64.0f, 64.0f, 0.0f, 0.0f, 1.0f, 1.0f, lagoMaterial);
			}	*/
		}
		#endregion
	}
}