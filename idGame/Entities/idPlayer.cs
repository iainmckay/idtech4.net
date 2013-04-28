using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Collision;
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
		public float DefaultFieldOfView
		{
			get
			{
				float fov = idR.CvarSystem.GetFloat("g_fov");

				if(idR.Game.IsMultiplayer == true)
				{
					if(fov < 90.0f)
					{
						return 90.0f;
					}
					else if(fov > 110.0f)
					{
						return 110.0f;
					}
				}

				return fov;
			}
		}
				
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

		public idAngles ViewAngles
		{
			get
			{
				return _viewAngles;
			}
			set
			{
				UpdateDeltaViewAngles(value);
				_viewAngles = value;
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
		private idPlayerView _playerView;

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

		// the first person view values are always calculated, even
		// if a third person view is used
		private Vector3 _firstPersonViewOrigin;
		private Matrix _firstPersonViewAxis;

		private bool _spawnAnglesSet; // on first usercmd, we must set deltaAngles
		private idAngles _spawnAngles;
		private idAngles _viewAngles; // player view angles
		private idAngles _cmdAngles; // player cmd angles

		private int _smoothedFrame;
		private bool _smoothedOriginUpdated;
		private Vector3 _smoothedOrigin;
		private idAngles _smoothedAngles;

		private idAngles _viewBobAngles;
		private Vector3 _viewBob;
		#endregion

		#region Constructor
		public idPlayer()
			: base()
		{
			_view = new PlayerView(this);
			_physicsObject = new idPhysics_Player();
			_playerView = new idPlayerView(this);

			/* TODO: memset( &usercmd, 0, sizeof( usercmd ) );

			heartRate				= BASE_HEARTRATE;
			heartInfo.Init( 0, 0, 0, 0 );
			lastArmorPulse			= -10000;*/
			_colorBar = Vector3.Zero;
			_colorBarIndex = 0;

			/*
			hipJoint				= INVALID_JOINT;
			chestJoint				= INVALID_JOINT;
			headJoint				= INVALID_JOINT;

			legsForward				= true;
			currentWeapon			= -1;
			idealWeapon				= -1;
			previousWeapon			= -1;
			weaponEnabled			= true;
			weapon_soulcube			= -1;
			weapon_pda				= -1;
			weapon_fists			= -1;*/

			_showWeaponViewModel = true;

			_baseSkin = string.Empty;

			/*
			zoomFov.Init( 0, 0, 0, 0 );
			centerView.Init( 0, 0, 0, 0 );

			memset( loggedViewAngles, 0, sizeof( loggedViewAngles ) );
			memset( loggedAccel, 0, sizeof( loggedAccel ) );

			pdaAudio				= "";
			pdaVideo				= "";
			pdaVideoWave			= "";

			fl.networkSync			= true;

			latchedTeam				= -1;
			teleportKiller			= -1;*/
			/*
			lastTeleFX				= -9999;
			MPAim					= -1;
			lastMPAim				= -1;
*/
		}
		#endregion

		#region Methods
		#region Private
		private void Init()
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
				foreach(KeyValuePair<string, string> kvp in this.SpawnArgs.MatchPrefix("pm_"))
				{
					idR.CvarSystem.SetString(kvp.Key, kvp.Value);
				}
			}

			// disable stamina on hell levels
			if((idR.Game.World != null) && (idR.Game.World.SpawnArgs.GetBool("no_stamina") == true))
			{
				idR.CvarSystem.SetFloat("pm_stamina", 0.0f);
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
			_physicsObject.Gravity = idR.Game.Gravity;

			// start out standing
			/*SetEyeHeight(pm_normalviewheight.GetFloat());

			stepUpTime = 0;
			stepUpDelta = 0.0f;*/

			_viewBob = Vector3.Zero;
			_viewBobAngles = new idAngles();

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

			// reset the script object*/
			ConstructScriptObject();

			// execute the script so the script object's constructor takes effect immediately
			/*scriptThread->Execute();*/

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

		private void SetClipModel()
		{
			idBounds bounds;

			if(_spectating == true)
			{
				bounds = idBounds.Expand(idE.CvarSystem.GetFloat("pm_spectatebbox") * 0.5f);
			}
			else
			{
				float width = idE.CvarSystem.GetFloat("pm_bboxwidth");

				bounds.Min = new Vector3(-width * 0.5f, -width * 0.5f, 0);
				bounds.Max = new Vector3(width * 0.5f, width * 0.5f, idE.CvarSystem.GetFloat("pm_normalheight"));
			}

			// the origin of the clip model needs to be set before calling SetClipModel
			// otherwise our physics object's current origin value gets reset to 0
			idClipModel clipModel;

			if(idE.CvarSystem.GetBool("pm_usecylinder") == true)
			{
				clipModel = new idClipModel(new idTraceModel(bounds, 8));
				clipModel.Translate(_physicsObject.PlayerOrigin);
			}
			else
			{
				clipModel = new idClipModel(new idTraceModel(bounds));
				clipModel.Translate(_physicsObject.PlayerOrigin);
			}

			_physicsObject.SetClipModel(clipModel, 1.0f);
		}

		/// <summary>
		/// Fixed fov at intermissions, otherwise account for fov variable and zooms.
		/// </summary>
		/// <param name="honorZoom"></param>
		/// <returns></returns>
		private float CalculateFieldOfView(bool honorZoom)
		{
			// TODO: fov
			/*if(_fxFov == true)
			{
				return (this.DefaultFieldOfView + 10.0f + idMath.Cos((idR.Game.Time + 2000) * 0.01f) * 10.0f);
			}

			if(_influenceFov > 0)
			{
				return _influenceFov;
			}*/

			float fov = this.DefaultFieldOfView;
			 

			/*if ( zoomFov.IsDone( gameLocal.time ) ) {
				fov = ( honorZoom && usercmd.buttons & BUTTON_ZOOM ) && weapon.GetEntity() ? weapon.GetEntity()->GetZoomFov() : DefaultFov();
			} else {
				fov = zoomFov.GetCurrentValue( gameLocal.time );
			}*/

			// bound normal viewsize
			if(fov < 1)
			{
				fov = 1;
			}
			else if(fov > 179)
			{
				fov = 179;
			}

			return fov;
		}

		private void CalculateFirstPersonView()
		{
			int modelView = idR.CvarSystem.GetInteger("pm_modelView");

			if((modelView == 1) || ((modelView == 2) && (this.Health <= 0)))
			{
				// displays the view from the point of view of the "camera" joint in the player model

				idConsole.Warning("TODO: view from camera joint");

				/*idMat3 axis;
				idVec3 origin;
				idAngles ang;

				ang = viewBobAngles + playerView.AngleOffset();
				ang.yaw += viewAxis[ 0 ].ToYaw();
		
				jointHandle_t joint = animator.GetJointHandle( "camera" );
				animator.GetJointTransform( joint, gameLocal.time, origin, axis );
				firstPersonViewOrigin = ( origin + modelOffset ) * ( viewAxis * physicsObj.GetGravityAxis() ) + physicsObj.GetOrigin() + viewBob;
				firstPersonViewAxis = axis * ang.ToMat3() * physicsObj.GetGravityAxis();*/
			} 
			else 
			{
				// offset for local bobbing and kicks
				GetViewPosition(out _firstPersonViewOrigin, out _firstPersonViewAxis);

#if false
				// shakefrom sound stuff only happens in first person
				firstPersonViewAxis = firstPersonViewAxis * playerView.ShakeAxis();
#endif
			}
		}

		/// <summary>
		/// Create the renderView for the current tic.
		/// </summary>
		private void CalculateRenderView()
		{
			if(_renderView == null)
			{
				_renderView = new idRenderView();
			}

			_renderView.Clear();

			// copy global shader parms
			for(int i = 0; i < idE.MaxGlobalMaterialParameters; i++)
			{
				_renderView.MaterialParameters[i] = idR.Game.GlobalMaterialParameters[i];
			}

			_renderView.GlobalMaterial = idR.Game.GlobalMaterial;
			_renderView.Time = idR.Game.Time;

			// calculate size of 3D view
			_renderView.X = 0;
			_renderView.Y = 0;
			_renderView.Width = idE.VirtualScreenWidth;
			_renderView.Height = idE.VirtualScreenHeight;
			_renderView.ViewID = 0;

			// check if we should be drawing from a camera's POV
			// TODO: camera
			/*if ( !noclip && (gameLocal.GetCamera() || privateCameraView) ) {
				// get origin, axis, and fov
				if ( privateCameraView ) {
					privateCameraView->GetViewParms( renderView );
				} else {
					gameLocal.GetCamera()->GetViewParms( renderView );
				}
			} 
			else */
			{
				if(idR.CvarSystem.GetBool("g_stopTime") == true)
				{
					_renderView.ViewOrigin = _firstPersonViewOrigin;
					_renderView.ViewAxis = _firstPersonViewAxis;

					if(idR.CvarSystem.GetBool("pm_thirdPerson") == true)
					{
						// set the viewID to the clientNum + 1, so we can suppress the right player bodies and
						// allow the right player view weapons
						_renderView.ViewID = this.Index + 1;
					}
				}
				else if(idR.CvarSystem.GetBool("pm_thirdPerson") == true)
				{
					idConsole.Warning("TODO: third person");
					//OffsetThirdPersonView( pm_thirdPersonAngle.GetFloat(), pm_thirdPersonRange.GetFloat(), pm_thirdPersonHeight.GetFloat(), pm_thirdPersonClip.GetBool() );
				}
				else if(idR.CvarSystem.GetBool("pm_thirdPersonDeath") == true)
				{
					idConsole.Warning("TODO: third person death");

					//range = gameLocal.time < minRespawnTime ? ( gameLocal.time + RAGDOLL_DEATH_TIME - minRespawnTime ) * ( 120.0f / RAGDOLL_DEATH_TIME ) : 120.0f;
					//OffsetThirdPersonView( 0.0f, 20.0f + range, 0.0f, false );
				}
				else
				{
					_renderView.ViewOrigin = _firstPersonViewOrigin;
					_renderView.ViewAxis = _firstPersonViewAxis;

					// set the viewID to the clientNum + 1, so we can suppress the right player bodies and
					// allow the right player view weapons
					_renderView.ViewID = this.Index + 1;
				}

				// field of view
				idR.Game.CalculateFieldOfView(CalculateFieldOfView(true), out _renderView.FovX, out _renderView.FovY);
			}

			if(_renderView.FovY == 0)
			{
				idConsole.Error("renderView.FovY == 0");
			}

			if(idR.CvarSystem.GetBool("g_showviewpos") == true)
			{
				idConsole.WriteLine("{0}: {1}", _renderView.ViewOrigin, _renderView.ViewAxis.ToAngles());
			}
		}

		private void UpdateDeltaViewAngles(idAngles angles)
		{
			// set the delta angle
			idAngles delta = new idAngles();

			idConsole.Warning("TODO: delta.Pitch = angles.Pitch - idMath.ShortToAngle(_userCommand.Angles.Pitch);");
			idConsole.Warning("TODO: delta.Yaw = angles.Yaw - idMath.ShortToAngle(_userCommand.Angles.Yaw);");
			idConsole.Warning("TODO: delta.Pitch = angles.Roll - idMath.ShortToAngle(_userCommand.Angles.Roll);");

			this.DeltaViewAngles = delta;
		}
		#endregion

		#region Public
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

			_viewAngles = idAngles.Zero;

			this.DeltaViewAngles = idAngles.Zero;
			this.ViewAngles = angles;

			_spawnAngles = angles;
			_spawnAnglesSet = false;

			/*legsForward = true;
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
			if(this.IsSpectating == false)
			{
				_physicsObject.SetClipMask(ContentFlags.MaskPlayerSolid); // the clip mask is usually maintained in Move(), but KillBox requires it
				idConsole.Warning("TODO: gameLocal.KillBox( this );");
			}

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

			BecomeActive(EntityThinkFlags.Think);

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
		#endregion

		#region idActor implementation
		#region Properties
		public override Vector3 EyePosition
		{
			get
			{
				Vector3 origin;

				// use the smoothed origin if spectating another player in multiplayer
				if((idR.Game.IsClient == true) && (this.Index != idR.Game.LocalClientIndex))
				{
					origin = _smoothedOrigin;
				}
				else
				{
					origin = this.Physics.GetOrigin();
				}

				// TODO: remove this hack.  we need this because we dont do eye height
				origin.Z = 73f;

				return (origin + (this.Physics.GravityNormal * -_eyeOffset.Z));
			}
		}

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

				return _renderView;
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

			idConsole.Warning("TODO: if af.IsActive");
			/*if(af.IsActive())
			{
				af.GetPhysicsToVisualTransform(origin, axis);
				return true;
			}*/

			// smoothen the rendered origin and angles of other clients
			// smooth self origin if snapshots are telling us prediction is off
			if((idR.Game.IsClient == true) && /* TODO: gameLocal.framenum >= smoothedFrame && */ (this.Index != idR.Game.LocalClientIndex) /* TODO: || selfSmooth)*/)
			{
				idConsole.Warning("TODO: idPlayer.GetPhysicsToVisualTransform");

				// render origin and axis
				/*idMat3 renderAxis = viewAxis * GetPhysics()->GetAxis();
				idVec3 renderOrigin = GetPhysics()->GetOrigin() + modelOffset * renderAxis;

				// update the smoothed origin
				if(!smoothedOriginUpdated)
				{
					idVec2 originDiff = renderOrigin.ToVec2() - smoothedOrigin.ToVec2();
					if(originDiff.LengthSqr() < Square(100.0f))
					{
						// smoothen by pushing back to the previous position
						if(selfSmooth)
						{
							assert(entityNumber == gameLocal.localClientNum);
							renderOrigin.ToVec2() -= net_clientSelfSmoothing.GetFloat() * originDiff;
						}
						else
						{
							renderOrigin.ToVec2() -= gameLocal.clientSmoothing * originDiff;
						}
					}
					smoothedOrigin = renderOrigin;

					smoothedFrame = gameLocal.framenum;
					smoothedOriginUpdated = true;
				}

				axis = idAngles(0.0f, smoothedAngles.yaw, 0.0f).ToMat3();
				origin = (smoothedOrigin - GetPhysics()->GetOrigin()) * axis.Transpose();*/
			}
			else
			{
				axis = _viewAxis;
				origin = _modelOffset;
			}

			return true;
		}

		public override void GetViewPosition(out Vector3 origin, out Matrix axis)
		{
			idAngles angles = new idAngles();

			// if dead, fix the angle and don't add any kick
			if(this.Health <= 0)
			{
				angles.Yaw = _viewAngles.Yaw;
				angles.Roll = 40;
				angles.Pitch = -15;

				axis = angles.ToMatrix();
				origin = this.EyePosition;
			}
			else
			{
				origin = this.EyePosition + _viewBob;
				angles = _viewAngles + _viewBobAngles + _playerView.AngleOffset;
				axis = angles.ToMatrix() *_physicsObject.GravityAxis;

				// adjust the origin based on the camera nodal distance (eye distance from neck)
				float v = idR.CvarSystem.GetFloat("g_viewNodalZ");

				origin += _physicsObject.GravityNormal * v;
				origin += new Vector3(axis.M11, axis.M12, axis.M13) * v + new Vector3(axis.M31, axis.M32, axis.M33) * v;
			}
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

			SetClipModel();

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
				idConsole.Warning("TODO: SetupWeaponEntity();");
				SpawnFromSpawnSpot();
			}
			
			// trigger playtesting item gives, if we didn't get here from a previous level
			// the devmap key will be set on the first devmap, but cleared on any level
			// transitions
			if((idR.Game.IsMultiplayer == false) && (idR.Game.ServerInfo.ContainsKey("devmap") == true))
			{
				idConsole.Warning("TODO: devmap");

				// fire a trigger with the name "devmap"
				/*idEntity *ent = gameLocal.FindEntity( "devmap" );
				if ( ent ) {
					ent->ActivateTargets( this );
				}*/
			}

			if(_hud != null)
			{
				idConsole.Warning("TODO: soul cube");

				// we can spawn with a full soul cube, so we need to make sure the hud knows this
				/*if ( weapon_soulcube > 0 && ( inventory.weapons & ( 1 << weapon_soulcube ) ) ) {
					int max_souls = inventory.MaxAmmoForAmmoClass( this, "ammo_souls" );
					if ( inventory.ammo[ idWeapon::GetAmmoNumForName( "ammo_souls" ) ] >= max_souls ) {
						hud->HandleNamedEvent( "soulCubeReady" );
					}
				}*/

				_hud.HandleNamedEvent("itemPickup");
			}

			idConsole.Warning("TODO: GetPDA");
			/*if ( GetPDA() ) {
				// Add any emails from the inventory
				for ( int i = 0; i < inventory.emails.Num(); i++ ) {
					GetPDA()->AddEmail( inventory.emails[i] );
				}
				GetPDA()->SetSecurity( common->GetLanguageDict()->GetString( "#str_00066" ) );
			}*/

			if(idR.Game.World.SpawnArgs.GetBool("no_Weapons") == true)
			{
				idConsole.Warning("TODO: no_Weapons");
			
				/*hiddenWeapon = true;
				if ( weapon.GetEntity() ) {
					weapon.GetEntity()->LowerWeapon();
				}
				idealWeapon = 0;*/
			} 
			else 
			{
				idConsole.Warning("TODO: hiddenWeapon = false;");
			}

			if(_hud != null)
			{
				idConsole.Warning("TODO: UpdateHudWeapon();");
				_hud.StateChanged(idR.Game.Time);
			}
			
			idConsole.Warning("TODO: inventory");

			/*tipUp = false;
			objectiveUp = false;

			if ( inventory.levelTriggers.Num() ) {
				PostEventMS( &EV_Player_LevelTrigger, 0 );
			}

			inventory.pdaOpened = false;
			inventory.selPDA = 0;*/

			if(idR.Game.IsMultiplayer == false)
			{
				int skill = idR.CvarSystem.GetInteger("g_skill");

				if(skill < 2)
				{
					if(this.Health < 25)
					{
						this.Health = 25;
					}

					if(idR.CvarSystem.GetBool("g_useDynamicProtection") == true)
					{
						idR.CvarSystem.SetFloat("g_damageScale", 1.0f);
					}
				} 
				else 
				{
					idR.CvarSystem.SetFloat("g_damageScale", 1.0f);
					idR.CvarSystem.SetFloat("g_armorProtection", (skill < 2) ? 0.4f : 0.2f);

					if(skill == 3)
					{
						idConsole.Warning("TODO: this.HealthTake = true");
						idConsole.Warning("TODO: nextHealthTake = gameLocal.time + g_healthTakeTime.GetInteger() * 1000;");
					}
				}
			}
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
			}*/

			// calculate the exact bobbed view position, which is used to
			// position the view weapon, among other things
			CalculateFirstPersonView();

			// this may use firstPersonView, or a thirdPerson / camera view
			CalculateRenderView();

			/*inventory.UpdateArmor();

			if(spectating)
			{
				UpdateSpectating();
			}
			else if(health > 0)
			{
				UpdateWeapon();
			}

			UpdateAir();*/

			//UpdateHud();

			/*UpdatePowerUps();

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
		#region Public
		public void Draw(idUserInterface hud)
		{
			idRenderView renderView = _player.RenderView;

			if(idR.CvarSystem.GetBool("g_skipViewEffects") == true)
			{
				SingleView(hud, renderView);
			}
			else
			{
				// TODO:
				/*if(player->GetInfluenceMaterial() || player->GetInfluenceEntity())
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
				else*/
				{
					SingleView(hud, renderView);
				}

				// TODO: ScreenFade();
			}

			/* TODO: lagometer
			if(net_clientLagOMeter.GetBool() && lagoMaterial && gameLocal.isClient)
			{
				renderSystem->SetColor4(1.0f, 1.0f, 1.0f, 1.0f);
				renderSystem->DrawStretchPic(10.0f, 380.0f, 64.0f, 64.0f, 0.0f, 0.0f, 1.0f, 1.0f, lagoMaterial);
			}*/
		}
		#endregion

		#region Private
		private void SingleView(idUserInterface hud, idRenderView renderView)
		{
			// normal rendering
			if(renderView == null)
			{
				return;
			}

			// place the sound origin for the player
			// TODO: gameSoundWorld->PlaceListener( view->vieworg, view->viewaxis, player->entityNumber + 1, gameLocal.time, hud ? hud->State().GetString( "location" ) : "Undefined" );

			// if the objective system is up, don't do normal drawing
			// TODO: objectives
			/*if ( player->objectiveSystemOpen ) {
				player->objectiveSystem->Redraw( gameLocal.time );
				return;
			}
			*/

			// hack the shake in at the very last moment, so it can't cause any consistency problems
			idRenderView hackedView = renderView.Copy();
			// TODO: hackedView.viewaxis = hackedView.viewaxis * ShakeAxis();

			idR.Game.CurrentRenderWorld.RenderScene(hackedView);

			if(_player.IsSpectating == true)
			{
				return;
			}

			// draw screen blobs
			if((idR.CvarSystem.GetBool("pm_thirdPerson") == false) && (idR.CvarSystem.GetBool("g_skipViewEffects") == false))
			{
				idConsole.Warning("TODO: screen blobs");

				/*for ( int i = 0 ; i < MAX_SCREEN_BLOBS ; i++ ) {
					screenBlob_t	*blob = &screenBlobs[i];
					if ( blob->finishTime <= gameLocal.time ) {
						continue;
					}
			
					blob->y += blob->driftAmount;

					float	fade = (float)( blob->finishTime - gameLocal.time ) / ( blob->finishTime - blob->startFadeTime );
					if ( fade > 1.0f ) {
						fade = 1.0f;
					}
					if ( fade ) {
						renderSystem->SetColor4( 1,1,1,fade );
						renderSystem->DrawStretchPic( blob->x, blob->y, blob->w, blob->h,blob->s1, blob->t1, blob->s2, blob->t2, blob->material );
					}
				}
				player->DrawHUD( hud );

				// armor impulse feedback
				float	armorPulse = ( gameLocal.time - player->lastArmorPulse ) / 250.0f;

				if ( armorPulse > 0.0f && armorPulse < 1.0f ) {
					renderSystem->SetColor4( 1, 1, 1, 1.0 - armorPulse );
					renderSystem->DrawStretchPic( 0, 0, 640, 480, 0, 0, 1, 1, armorMaterial );
				}


				// tunnel vision
				float	health = 0.0f;
				if ( g_testHealthVision.GetFloat() != 0.0f ) {
					health = g_testHealthVision.GetFloat();
				} else {
					health = player->health;
				}
				float alpha = health / 100.0f;
				if ( alpha < 0.0f ) {
					alpha = 0.0f;
				}
				if ( alpha > 1.0f ) {
					alpha = 1.0f;
				}

				if ( alpha < 1.0f  ) {
					renderSystem->SetColor4( ( player->health <= 0.0f ) ? MS2SEC( gameLocal.time ) : lastDamageTime, 1.0f, 1.0f, ( player->health <= 0.0f ) ? 0.0f : alpha );
					renderSystem->DrawStretchPic( 0.0f, 0.0f, 640.0f, 480.0f, 0.0f, 0.0f, 1.0f, 1.0f, tunnelMaterial );
				}

				if ( player->PowerUpActive(BERSERK) ) {
					int berserkTime = player->inventory.powerupEndTime[ BERSERK ] - gameLocal.time;
					if ( berserkTime > 0 ) {
						// start fading if within 10 seconds of going away
						alpha = (berserkTime < 10000) ? (float)berserkTime / 10000 : 1.0f;
						renderSystem->SetColor4( 1.0f, 1.0f, 1.0f, alpha );
						renderSystem->DrawStretchPic( 0.0f, 0.0f, 640.0f, 480.0f, 0.0f, 0.0f, 1.0f, 1.0f, berserkMaterial );
					}
				}

				if ( bfgVision ) {
					renderSystem->SetColor4( 1.0f, 1.0f, 1.0f, 1.0f );
					renderSystem->DrawStretchPic( 0.0f, 0.0f, 640.0f, 480.0f, 0.0f, 0.0f, 1.0f, 1.0f, bfgMaterial );
				}*/
		
			}

			// test a single material drawn over everything
			if(idR.CvarSystem.GetString("g_testPostProcess") != string.Empty)
			{
				idMaterial material = idR.DeclManager.FindMaterial(idR.CvarSystem.GetString("g_testPostProcess"), false);

				if(material == null)
				{
					idConsole.Warning("Material not found.");
					idR.CvarSystem.SetString("g_testPostProcess", string.Empty);
				}
				else
				{
					idR.RenderSystem.Color = new Vector4(1, 1, 1, 1);
					idR.RenderSystem.DrawStretchPicture(0, 0, idE.VirtualScreenWidth, idE.VirtualScreenHeight, 0, 0, 1, 1, material);
				}
			}
		}
		#endregion
		#endregion
	}
}