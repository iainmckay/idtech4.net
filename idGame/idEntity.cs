using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Game.Animation;
using idTech4.Game.Physics;
using idTech4.Game.Scripting;
using idTech4.Math;
using idTech4.Net;
using idTech4.Renderer;
using idTech4.Text;
using idTech4.Text.Decl;

namespace idTech4.Game
{
	public class idEntity : IDisposable
	{
		#region Properties
		/// <summary>
		/// Subclasses will be responsible for allocating animator.
		/// </summary>
		public virtual idAnimator Animator
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return null;
			}
		}

		public Matrix Axis
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				// TODO
				idConsole.Warning("TODO: idEntity.Axis");
				return Matrix.Identity;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idEntity.Axis set");
				// TODO
				/*if ( GetPhysics()->IsType( idPhysics_Actor::Type ) ) {
		static_cast<idActor *>(this)->viewAxis = axis;
	} else {
		GetPhysics()->SetAxis( axis );
	}*/

				UpdateVisuals();
			}
		}
		
		/// <summary>
		/// Index into the entity def list.
		/// </summary>
		public int DefIndex
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _entityDefIndex;
			}
		}

		public string DefName
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				if(this.DefIndex < 0)
				{
					return "*unknown";
				}

				return idR.DeclManager.DeclByIndex(DeclType.EntityDef, this.DefIndex, false).Name;
			}
		}		
		
		/// <summary>
		/// During cinematics, entity will only think if cinematic is true.
		/// </summary>
		public bool Cinematic
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _cinematic;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_cinematic = value;
			}
		}

		/// <summary>
		/// Gets the text classname of the entity.
		/// </summary>
		public string ClassName
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _className;
			}
		}

		/// <summary>
		/// Gets/Sets the health of the entity.
		/// </summary>
		public int Health
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _health;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_health = value;
			}
		}

		/// <summary>
		/// Index in to the entity list.
		/// </summary>
		public int Index
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _entityIndex;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_entityIndex = value;
			}
		}

		public virtual idRenderModel Model
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idEntity.Model get");

				return null;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idEntity.Model set");
			}
		}

		/// <summary>
		/// Gets/Sets the name of the entity.
		/// </summary>
		public string Name
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _name;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idEntity.Name set");
				// TODO
				/*if(name.Length())
				{
					gameLocal.RemoveEntityFromHash(name.c_str(), this);
					gameLocal.program.SetEntity(name, NULL);
				}*/

				_name = value;

				if(_name != string.Empty)
				{
					if((_name == "NULL") || (_name == "null_entity"))
					{
						idConsole.Error("Cannot name entity '{0}'. '{1}' is reserved for script.", _name, _name);
					}

					// TODO
					/*gameLocal.AddEntityToHash(name.c_str(), this);
					gameLocal.program.SetEntity(name, this);*/
				}
			}
		}

		public Vector3 Origin
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				// TODO:
				idConsole.Warning("TODO: idEntity.GetOrigin");

				return Vector3.Zero;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				this.Physics.SetOrigin(value);
				UpdateVisuals();
			}
		}

		/// <summary>
		/// Gets the physics object used by this entity.
		/// </summary>
		public idPhysics Physics
		{
			get
			{
				return _physics;
			}
		}

		/// <summary>
		/// Gets the view used for camera views from this entity.
		/// </summary>
		public virtual idRenderView RenderView
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

		/// <summary>
		/// Used to present a model to the renderer
		/// </summary>
		public RenderEntityComponent RenderEntity
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _renderEntity;
			}
		}

		/// <summary>
		/// Called during idEntity::Spawn to see if it should construct the script object or not.
		/// </summary>
		public virtual bool ShouldConstructScriptObjectAtSpawn
		{
			get
			{
				return true;
			}
		}

		public idDeclSkin Skin
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _renderEntity.CustomSkin;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_renderEntity.CustomSkin = value;


				UpdateVisuals();
			}
		}

		/// <summary>
		/// Key/value pairs used to spawn and initialize entity.
		/// </summary>
		public idDict SpawnArgs
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _spawnArgs;
			}
		}

		public LinkedListNode<idEntity> SpawnNode
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _spawnNode;
			}

			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_spawnNode = value;
			}
		}
		#endregion

		#region Constants
		public readonly int IndexNone = idR.MaxGameEntities - 1;
		#endregion

		#region Members
		private int _entityIndex;
		private int _entityDefIndex;

		private int _health;

		private string _name;
		private string _className;

		private bool _cinematic;

		private idRenderView _renderView;
		private RenderEntityComponent _renderEntity;

		private idDict _spawnArgs = new idDict();

		// for being linked into spawnedEntities list
		private LinkedListNode<idEntity> _spawnNode;
		// for being linked into activeEntities list
		private LinkedListNode<idEntity> _activeNode;

		private idPhysics _defaultPhysicsObject;
		private idPhysics _physics;
		#endregion

		#region Constructor
		public idEntity()
		{
			_entityIndex = idR.MaxGameEntities - 1;
			_entityDefIndex = -1;

			_className = "unknown";

			_renderEntity = new RenderEntityComponent();

			idConsole.Warning("TODO: idEntity");
			/*			
			snapshotNode.SetOwner(this);
			snapshotSequence = -1;*/
	
			/*
			bindJoint = INVALID_JOINT;
			bindBody = -1;

			memset(PVSAreas, 0, sizeof(PVSAreas));
			numPVSAreas = -1;

			memset(&fl, 0, sizeof(fl));
			fl.neverDormant = true;			// most entities never go dormant

			modelDefHandle = -1;
			memset(&refSound, 0, sizeof(refSound));

			mpGUIState = -1;*/
		}

		~idEntity()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Private
		private void InitDefaultPhysics(Vector3 origin, Matrix axis)
		{
			string temp = _spawnArgs.GetString("clipmodel", "");
			idClipModel clipModel = null;

			// check if a clipmodel key/value pair is set
			if(temp != string.Empty)
			{
				if(idClipModel.CheckModel(temp) >= 0)
				{
					clipModel = new idClipModel(temp);
				}
			}

			if(_spawnArgs.GetBool("noclipmodel", false) == false)
			{
				// check if mins/maxs or size key/value pairs are set
				if(clipModel == null)
				{
					throw new NotImplementedException();

					/*idVec3 size;
					idBounds bounds;
					bool setClipModel = false;*/

					/*if ( spawnArgs.GetVector( "mins", NULL, bounds[0] ) &&
						spawnArgs.GetVector( "maxs", NULL, bounds[1] ) ) {
						setClipModel = true;
						if ( bounds[0][0] > bounds[1][0] || bounds[0][1] > bounds[1][1] || bounds[0][2] > bounds[1][2] ) {
							gameLocal.Error( "Invalid bounds '%s'-'%s' on entity '%s'", bounds[0].ToString(), bounds[1].ToString(), name.c_str() );
						}
					} else if ( spawnArgs.GetVector( "size", NULL, size ) ) {
						if ( ( size.x < 0.0f ) || ( size.y < 0.0f ) || ( size.z < 0.0f ) ) {
							gameLocal.Error( "Invalid size '%s' on entity '%s'", size.ToString(), name.c_str() );
						}
						bounds[0].Set( size.x * -0.5f, size.y * -0.5f, 0.0f );
						bounds[1].Set( size.x * 0.5f, size.y * 0.5f, size.z );
						setClipModel = true;
					}*/

					/*if ( setClipModel ) {
						int numSides;
						idTraceModel trm;

						if ( spawnArgs.GetInt( "cylinder", "0", numSides ) && numSides > 0 ) {
							trm.SetupCylinder( bounds, numSides < 3 ? 3 : numSides );
						} else if ( spawnArgs.GetInt( "cone", "0", numSides ) && numSides > 0 ) {
							trm.SetupCone( bounds, numSides < 3 ? 3 : numSides );
						} else {
							trm.SetupBox( bounds );
						}
						clipModel = new idClipModel( trm );
					}*/
				}

				// check if the visual model can be used as collision model
				if(clipModel == null)
				{
					temp = _spawnArgs.GetString("model");

					if(temp != string.Empty)
					{
						if(idClipModel.CheckModel(temp) >= 0)
						{
							clipModel = new idClipModel(temp);
						}
					}
				}
			}

			_defaultPhysicsObject.Self = this;
			_defaultPhysicsObject.SetClipModel(clipModel, 1.0f);
			_defaultPhysicsObject.SetOrigin(origin);
			_defaultPhysicsObject.SetAxis(axis);

			_physics = _defaultPhysicsObject;
		}

		private void UpdateVisuals()
		{
			idConsole.Warning("TODO: UpdateVisuals");
			// TODO
			/*UpdateModel();
			UpdateSound();*/
		}
		#endregion

		#region Public
		public virtual void AddDamageEffect(object collision, Vector3 velocity, string damageDefName)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.AddDamageEffect");	
		}

		public virtual void AddForce(idEntity entity, int id, Vector3 point, Vector3 force)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.AddForce");
		}
		
		public virtual void ApplyImpulse(idEntity entity, int id, Vector3 point, Vector3 impulse)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.ApplyImpulse");	
		}

		public virtual void ClientPredictionThink()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.ClientPredictionThink");
		}

		public virtual bool ClientReceiveEvent(int ev, int time, idBitMsg msg)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.ClientReceiveEvent");

			return false;
		}

		public virtual bool Collide(object collision, Vector3 velocity)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// this entity collides with collision.c.entityNum
			return false;
		}

		/// <summary>
		/// Called during idEntity::Spawn.  Calls the constructor on the script object.
		/// </summary>
		/// <returns></returns>
		public virtual idThread ConstructScriptObject()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.ConstructScriptObject");

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inflictor">Entity that is causing the damage.</param>
		/// <param name="attacker">Entity that caused the inflictor to cause damage to us.</param>
		/// <param name="direction">Direction of the attack for knockback in global space.</param>
		/// <param name="damageDefName"></param>
		/// <param name="damageScale"></param>
		/// <param name="location"></param>
		public virtual void Damage(idEntity inflictor, idEntity attacker, Vector3 direction, string damageDefName, float damageScale, int location)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.Damage");
		}

		/// <summary>
		/// Callback function for when another entity received damage from this entity.  damage can be adjusted and returned to the caller.
		/// </summary>
		/// <param name="victim"></param>
		/// <param name="inflictor"></param>
		/// <param name="damage"></param>
		public virtual void DamageFeedback(idEntity victim, idEntity inflictor, ref int damage)
		{

		}

		public virtual object GetImpactInfo(idEntity entity, int id, Vector3 point)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.GetImpactInfo");

			return null;
		}

		public bool GetMasterPosition(out Vector3 masterOrigin, out Matrix masterAxis)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			Vector3 localOrigin;
			Matrix localAxis;

			idConsole.Warning("TODO: idEntity.GetMasterPosition");
			// TODO
			/*idAnimator	*masterAnimator;

			if ( bindMaster ) {
				// if bound to a joint of an animated model
				if ( bindJoint != INVALID_JOINT ) {
					masterAnimator = bindMaster->GetAnimator();
					if ( !masterAnimator ) {
						masterOrigin = vec3_origin;
						masterAxis = mat3_identity;
						return false;
					} else {
						masterAnimator->GetJointTransform( bindJoint, gameLocal.time, masterOrigin, masterAxis );
						masterAxis *= bindMaster->renderEntity.axis;
						masterOrigin = bindMaster->renderEntity.origin + masterOrigin * bindMaster->renderEntity.axis;
					}
				} else if ( bindBody >= 0 && bindMaster->GetPhysics() ) {
					masterOrigin = bindMaster->GetPhysics()->GetOrigin( bindBody );
					masterAxis = bindMaster->GetPhysics()->GetAxis( bindBody );
				} else {
					masterOrigin = bindMaster->renderEntity.origin;
					masterAxis = bindMaster->renderEntity.axis;
				}
				return true;
			} 
			else*/
			{
				masterOrigin = Vector3.Zero;
				masterAxis = Matrix.Identity;

				return false;
			}
		}

		public virtual bool GetPhysicsToSoundTransform(ref Vector3 origin, ref Matrix axis)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.GetPhysicsToSoundTransform");

			return false;
		}

		public virtual bool GetPhysicsToVisualTransform(ref Vector3 origin, ref Matrix axis)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.GetPhysicsToVisualTransform");

			return false;
		}

		public virtual void FreeModelDef()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.FreeModelDef");
		}

		public virtual bool HandleSingleGuiCommand(idEntity entityGui, idLexer lexer)
		{
			return false;
		}

		public virtual void Hide()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: Hide");
			// TODO
			/*if(!IsHidden())
			{
				fl.hidden = true;
				FreeModelDef();
				UpdateVisuals();
			}*/
		}

		/// <summary>
		/// Called whenever an entity recieves damage.  Returns whether the entity responds to the pain.
		/// </summary>
		/// <param name="inflictor"></param>
		/// <param name="attacker"></param>
		/// <param name="damage"></param>
		/// <param name="direction"></param>
		/// <param name="location"></param>
		/// <returns></returns>
		public virtual bool Pain(idEntity inflictor, idEntity attacker, int damage, Vector3 direction, int location)
		{
			return false;
		}

		/// <summary>
		/// Present is called to allow entities to generate refEntities, lights, etc for the renderer.
		/// </summary>
		public virtual void Present()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.Present");
		}

		public virtual void ProjectOverlay(Vector3 origin, Vector3 direction, float size, string material)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.ProjectOverlay");
		}

		public virtual void ReadFromSnapshot(idBitMsgDelta msg)
		{

		}

		public virtual void Restore(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.Restore");
		}

		public virtual void Save(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.Save");
		}
			
		public virtual bool ServerReceiveEvent(int ev, int time, idBitMsg msg)
		{
			return false;
		}

		public virtual void Spawn()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.WriteLine("TODO: idEntity.Spawn");
			/*int					i;
			const char			*temp;
			idVec3				origin;
			idMat3				axis;
			const idKeyValue	*networkSync;
			const char			*classname;
			const char			*scriptObjectName;*/

			idR.Game.RegisterEntity(this);

			_className = _spawnArgs.GetString("classname", null);

			idDeclEntity def = idR.Game.FindEntityDef(_className, false);

			if(def != null)
			{
				_entityDefIndex = def.Index;
			}

			/*
			TODO
			FixupLocalizedStrings();
			*/

			// parse static models the same way the editor display does
			_renderEntity = idR.GameEdit.ParseSpawnArgsToRenderEntity(_spawnArgs);
			_renderEntity.EntityIndex = this.Index;

			// TODO
			/*
					// go dormant within 5 frames so that when the map starts most monsters are dormant
					dormantStart = gameLocal.time - DELAY_DORMANT_TIME + gameLocal.msec * 5;*/

			/*

			// do the audio parsing the same way dmap and the editor do
			gameEdit->ParseSpawnArgsToRefSound( &spawnArgs, &refSound );

			// only play SCHANNEL_PRIVATE when sndworld->PlaceListener() is called with this listenerId
			// don't spatialize sounds from the same entity
			refSound.listenerId = entityNumber + 1;

			cameraTarget = NULL;
			temp = spawnArgs.GetString( "cameraTarget" );
			if ( temp && temp[0] ) {
				// update the camera taget
				PostEventMS( &EV_UpdateCameraTarget, 0 );
			}

			for ( i = 0; i < MAX_RENDERENTITY_GUI; i++ ) {
				UpdateGuiParms( renderEntity.gui[ i ], &spawnArgs );
			}

			fl.solidForTeam = spawnArgs.GetBool( "solidForTeam", "0" );
			fl.neverDormant = spawnArgs.GetBool( "neverDormant", "0" );
			fl.hidden = spawnArgs.GetBool( "hide", "0" );
			if ( fl.hidden ) {
				// make sure we're hidden, since a spawn function might not set it up right
				PostEventMS( &EV_Hide, 0 );
			}
			cinematic = spawnArgs.GetBool( "cinematic", "0" );

			networkSync = spawnArgs.FindKey( "networkSync" );
			if ( networkSync ) {
				fl.networkSync = ( atoi( networkSync->GetValue() ) != 0 );
			}

		#if 0
			if ( !gameLocal.isClient ) {
				// common->DPrintf( "NET: DBG %s - %s is synced: %s\n", spawnArgs.GetString( "classname", "" ), GetType()->classname, fl.networkSync ? "true" : "false" );
				if ( spawnArgs.GetString( "classname", "" )[ 0 ] == '\0' && !fl.networkSync ) {
					common->DPrintf( "NET: WRN %s entity, no classname, and no networkSync?\n", GetType()->classname );
				}
			}
		#endif*/

			// every object will have a unique name
			this.Name = _spawnArgs.GetString("name", string.Format("{0}_{1}_{2}", this.ClassName, _spawnArgs.GetString("classname"), this.Index));

			// if we have targets, wait until all entities are spawned to get them
			// TODO
			/*if ( spawnArgs.MatchPrefix( "target" ) || spawnArgs.MatchPrefix( "guiTarget" ) ) {
				if ( gameLocal.GameState() == GAMESTATE_STARTUP ) {
					PostEventMS( &EV_FindTargets, 0 );
				} else {
					// not during spawn, so it's ok to get the targets
					FindTargets();
				}
			}*/

			_health = _spawnArgs.GetInteger("health");

			Vector3 origin = _renderEntity.Origin;
			Matrix axis = _renderEntity.Axis;
			
			InitDefaultPhysics(origin, axis);

			this.Origin = origin;
			this.Axis = axis;

			// TODO
			/*temp = spawnArgs.GetString( "model" );
			if ( temp && *temp ) {
				SetModel( temp );
			}

			if ( spawnArgs.GetString( "bind", "", &temp ) ) {
				PostEventMS( &EV_SpawnBind, 0 );
			}

			// auto-start a sound on the entity
			if ( refSound.shader && !refSound.waitfortrigger ) {
				StartSoundShader( refSound.shader, SND_CHANNEL_ANY, 0, false, NULL );
			}

			// setup script object
			if ( ShouldConstructScriptObjectAtSpawn() && spawnArgs.GetString( "scriptobject", NULL, &scriptObjectName ) ) {
				if ( !scriptObject.SetType( scriptObjectName ) ) {
					gameLocal.Error( "Script object '%s' not found on entity '%s'.", scriptObjectName, name.c_str() );
				}

				ConstructScriptObject();
			}*/
		}
				
		public virtual void Show()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// TODO
			idConsole.Warning("TODO: Show");
			/*
	if ( IsHidden() ) {
		fl.hidden = false;
		UpdateVisuals();
	}*/
		}

		public virtual void Teleport(Vector3 origin, idAngles angles, idEntity destination)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.Teleport");
		}

		public virtual void Think()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: Think");

			// TODO
			/*RunPhysics();
			Present();*/
		}

		public virtual bool UpdateAnimationControllers()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// any ragdoll and IK animation controllers should be updated here
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Any key/value pair that might change during the course of the game (e.g. via a gui)
		/// should be initialize here so a gui or other trigger can change something and have it updated
		/// properly. 
		/// <para/>
		/// An optional source may be provided if the values reside in an outside dictionary and
		/// first need copied over to spawnArgs.
		/// </remarks>
		/// <param name="source"></param>
		public virtual void UpdateChangeableSpawnArgs(idDict source)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.UpdateChangeableSpawnArgs");
		}

		public virtual void WriteToSnapshot(idBitMsgDelta msg)
		{

		}
		#endregion
		#endregion

		#region IDisposable implementation
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idEntity.Dispose");

			/*if ( gameLocal.GameState() != GAMESTATE_SHUTDOWN && !gameLocal.isClient && fl.networkSync && entityNumber >= MAX_CLIENTS ) {
					idBitMsg	msg;
					byte		msgBuf[ MAX_GAME_MESSAGE_SIZE ];

					msg.Init( msgBuf, sizeof( msgBuf ) );
					msg.WriteByte( GAME_RELIABLE_MESSAGE_DELETE_ENT );
					msg.WriteBits( gameLocal.GetSpawnId( this ), 32 );
					networkSystem->ServerSendReliableMessage( -1, msg );
				}

				DeconstructScriptObject();
				scriptObject.Free();

				if ( thinkFlags ) {
					BecomeInactive( thinkFlags );
				}
				activeNode.Remove();

				Signal( SIG_REMOVED );

				// we have to set back the default physics object before unbinding because the entity
				// specific physics object might be an entity variable and as such could already be destroyed.
				SetPhysics( NULL );

				// remove any entities that are bound to me
				RemoveBinds();

				// unbind from master
				Unbind();
				QuitTeam();

				gameLocal.RemoveEntityFromHash( name.c_str(), this );

				delete renderView;
				renderView = NULL;

				delete signals;
				signals = NULL;

				FreeModelDef();
				FreeSoundEmitter( false );

				gameLocal.UnregisterEntity( this );*/
		
			_disposed = true;
		}
		#endregion
	}
}