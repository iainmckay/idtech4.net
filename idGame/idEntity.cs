using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Collision;
using idTech4.Game.Animation;
using idTech4.Game.Entities;
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

				if(this.Physics is idPhysics_Actor)
				{
					((idActor) this).ViewAxis = value;
				}
				else
				{
					this.Physics.SetAxis(value);
				}

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

		public bool IsActive
		{
			get
			{
				return idR.Game.ActiveEntities.Contains(this);
			}
		}

		public bool IsAtRest
		{
			get
			{
				return this.Physics.IsAtRest;
			}
		}

		public bool IsHidden
		{
			get
			{
				return _flags.Hidden;
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

		protected idRenderView _renderView;
		protected RenderEntityComponent _renderEntity;
		protected idRenderEntity _renderModel;						// handle to static renderer model

		private idPhysics_Static _defaultPhysicsObject;
		private idPhysics _physics;

		private idDict _spawnArgs = new idDict();
		private EntityFlags _flags = new EntityFlags();
		private EntityThinkFlags _thinkFlags;

		private idEntity _bindMaster;		// entity bound to if unequal NULL
		private object _bindJoint;			// joint bound to if unequal INVALID_JOINT
		private int _bindBody;				// body bound to if unequal -1
		private idEntity _teamMaster;		// master of the physics team
		private idEntity _teamChain;		// next entity in physics team

		private int	_pvsAreaCount;			// number of renderer areas the entity covers
		#endregion

		#region Constructor
		public idEntity()
		{
			_entityIndex = idR.MaxGameEntities - 1;
			_entityDefIndex = -1;

			_className = "unknown";

			_renderEntity = new RenderEntityComponent();
			_defaultPhysicsObject = new idPhysics_Static();

			idConsole.Warning("TODO: idEntity");
			/*			
			snapshotNode.SetOwner(this);
			snapshotSequence = -1;*/
	
			/*
			bindJoint = INVALID_JOINT;*/
			_bindBody = -1;

			/*memset(PVSAreas, 0, sizeof(PVSAreas));*/
			_pvsAreaCount = -1;

			_flags.NeverDormant = true; // most entities never go dormant

			/* mpGUIState = -1;*/
		}

		~idEntity()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Activate the physics object.
		/// </summary>
		/// <param name="entity">Entity activating us.</param>
		public void ActivatePhysics(idEntity entity)
		{
			this.Physics.Activate();
		}

		public void AddContactEntity(idEntity entity)
		{
			this.Physics.AddContactEntity(entity);
		}

		public void RemoveContactEntity(idEntity entity)
		{
			this.Physics.RemoveContactEntity(entity);
		}
		#endregion

		#region Private
		private void ClearPVSAreas()
		{
			_pvsAreaCount = -1;
		}

		private void InitDefaultPhysics(Vector3 origin, Matrix axis)
		{
			string temp = _spawnArgs.GetString("clipmodel", "");
			idClipModel clipModel = null;

			// check if a clipmodel key/value pair is set
			if(temp != string.Empty)
			{
				if(idClipModel.CheckModel(temp) != null)
				{
					clipModel = new idClipModel(temp);
				}
			}

			if(_spawnArgs.GetBool("noclipmodel", false) == false)
			{
				// check if mins/maxs or size key/value pairs are set
				if(clipModel == null)
				{
					idBounds bounds = idBounds.Zero;
					bool setClipModel = false;

					if((_spawnArgs.ContainsKey("mins") == true)
						&& (_spawnArgs.ContainsKey("maxs") == true))
					{
						bounds = new idBounds(_spawnArgs.GetVector3("mins"), _spawnArgs.GetVector3("maxs"));
						setClipModel = true;

						if((bounds.Min.X > bounds.Max.X)
							|| (bounds.Min.Y > bounds.Max.Y)
							|| (bounds.Min.Z > bounds.Max.Z))
						{
							idConsole.Error("Invalid bounds '{0}'-'{1}' on entity '{2}'", bounds.Min, bounds.Max, this.Name);
						}
					}
					else if(_spawnArgs.ContainsKey("size") == true)
					{
						Vector3 size = _spawnArgs.GetVector3("size");

						if((size.X < 0.0f)
							|| (size.Y < 0.0f)
							|| (size.Z < 0.0f))
						{
							idConsole.Error("Invalid size '{0}' on entity '{1}'", size, this.Name);
						}

						setClipModel = true;
						bounds = new idBounds(
									new Vector3(size.X * -0.5f, size.Y * -0.5f, 0.0f),
									new Vector3(size.X * 0.5f, size.Y * 0.5f, size.Z)
								);
					}

					if(setClipModel == true)
					{
						int sideCount = _spawnArgs.GetInteger("cyclinder", 0);

						idTraceModel traceModel = new idTraceModel();

						if(sideCount > 0)
						{
							idConsole.Warning("TODO: traceModel.SetupCyclinder(bounds, (sideCount < 3) ? 3 : sideCount);");
						}
						else if((sideCount = _spawnArgs.GetInteger("cone", 0)) > 0)
						{
							idConsole.Warning("TODO: traceModel.SetupCone(bounds, (sideCount < 3) ? 3 : sideCount);");
						}
						else
						{
							traceModel.SetupBox(bounds);
						}

						clipModel = new idClipModel(traceModel);
					}
				}

				// check if the visual model can be used as collision model
				if(clipModel == null)
				{
					temp = _spawnArgs.GetString("model");

					if(temp != string.Empty)
					{
						if(idClipModel.CheckModel(temp) != null)
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

		private void UpdateModel()
		{
			UpdateModelTransform();

			// check if the entity has an MD5 model
			idAnimator animator = this.Animator;

			if((animator != null) && (animator.Model != null))
			{
				// set the callback to update the joints
				_renderEntity.Callback = ModelCallback;
			}

			// set to invalid number to force an update the next time the PVS areas are retrieved
			ClearPVSAreas();

			// ensure that we call Present this frame
			BecomeActive(EntityThinkFlags.UpdateVisuals);
		}

		private void UpdateModelTransform()
		{
			Vector3 origin = Vector3.Zero;
			Matrix axis = Matrix.Identity;

			if(GetPhysicsToVisualTransform(ref origin, ref axis) == true)
			{
				_renderEntity.Axis = axis * this.Physics.GetAxis();
				_renderEntity.Origin = this.Physics.GetOrigin() + Vector3.Transform(origin, _renderEntity.Axis);
			}
			else
			{
				_renderEntity.Axis = this.Physics.GetAxis();
				_renderEntity.Origin = this.Physics.GetOrigin();
			}
		}
		#endregion

		#region Protected
		protected virtual void SetModel(string modelName)
		{
			FreeModelDef();

			_renderEntity.Model = idE.RenderModelManager.FindModel(modelName);

			if(_renderEntity.Model != null)
			{
				_renderEntity.Model.Reset();
			}

			_renderEntity.Callback = null;
			_renderEntity.Joints = null;

			if(_renderEntity.Model != null)
			{
				_renderEntity.Bounds = _renderEntity.Model.GetBounds(_renderEntity);
			}
			else
			{
				_renderEntity.Bounds = idBounds.Zero;
			}

			UpdateVisuals();
		}

		protected virtual void SetPhysics(idPhysics phys)
		{
			// clear any contacts the current physics object has
			if(_physics != null)
			{
				_physics.ClearContacts();
			}

			// set new physics object or set the default physics if NULL
			if(phys != null)
			{
				_defaultPhysicsObject.SetClipModel(null, 1.0f);

				_physics = phys;
				_physics.Activate();
			}
			else
			{
				_physics = _defaultPhysicsObject;
			}

			_physics.UpdateTime(idR.Game.Time);

			idConsole.Warning("TODO: _physics.SetMaster(bindMaster, fl.bindOrientated);");
		}

		protected void UpdateVisuals()
		{
			UpdateModel();
			idConsole.Warning("TODO: UpdateSound();");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// May not change the game state whatsoever!
		/// </remarks>
		/// <param name="renderEntity"></param>
		/// <param name="renderView"></param>
		/// <returns></returns>
		protected bool ModelCallback(idRenderEntity renderEntity, idRenderView renderView)
		{
			idEntity ent = idR.Game.Entities[renderEntity.EntityIndex];

			if(ent == null)
			{
				idConsole.Error("idEntity::ModelCallback: callback with null game entity");
			}

			return ent.UpdateRenderEntity(renderEntity, renderView);
		}

		protected bool UpdateRenderEntity(idRenderEntity renderEntity, idRenderView renderView)
		{
			// TODO: cinematic
			/*if ( gameLocal.inCinematic && gameLocal.skipCinematic ) {
				return false;
			}*/

			idAnimator animator = this.Animator;

			if(animator != null)
			{
				return animator.CreateFrame(idR.Game.Time, false);
			}

			return false;
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

		public void BecomeActive(EntityThinkFlags flags)
		{
			if((flags & EntityThinkFlags.Physics) == EntityThinkFlags.Physics)
			{
				// enable the team master if this entity is part of a physics team
				if((_teamMaster != null) && (_teamMaster != this))
				{
					_teamMaster.BecomeActive(EntityThinkFlags.Physics);
				}
				else if((_thinkFlags & EntityThinkFlags.Physics) == 0)
				{
					// if this is a pusher
					if((_physics is idPhysics_Parametric) || (_physics is idPhysics_Actor))
					{
						idR.Game.SortPushers = true;
					}
				}
			}

			EntityThinkFlags oldFlags = _thinkFlags;

			_thinkFlags |= flags;

			if(_thinkFlags != 0)
			{
				if(this.IsActive == false)
				{
					idR.Game.ActiveEntities.Add(this);
				}
				else if(oldFlags == 0)
				{
					// we became inactive this frame, so we have to decrease the count of entities to deactivate
					idR.Game.EntitiesToDeactivate--;
				}
			}
		}

		public void BecomeInactive(EntityThinkFlags flags)
		{
			if((flags & EntityThinkFlags.Physics) == EntityThinkFlags.Physics)
			{
				// may only disable physics on a team master if no team members are running physics or bound to a joints
				if(_teamMaster == this)
				{
					idConsole.Warning("TODO: teamMaster");
		
					/*for ( idEntity *ent = teamMaster->teamChain; ent; ent = ent->teamChain ) {
						if ( ( ent->thinkFlags & TH_PHYSICS ) || ( ( ent->bindMaster == this ) && ( ent->bindJoint != INVALID_JOINT ) ) ) {
							flags &= ~TH_PHYSICS;
							break;
						}
					}*/
				}
			}

			if(_thinkFlags != 0)
			{
				_thinkFlags &= ~flags;

				if((_thinkFlags == 0) && (this.IsActive == true))
				{
					idR.Game.EntitiesToDeactivate++;
				}
			}

			if((flags & EntityThinkFlags.Physics) == EntityThinkFlags.Physics)
			{
				// if this entity has a team master
				if((_teamMaster != null) && (_teamMaster != this))
				{
					// if the team master is at rest
					if(_teamMaster.IsAtRest == true)
					{
						_teamMaster.BecomeInactive(EntityThinkFlags.Physics);
					}
				}
			}
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

			if(this.IsHidden == false)
			{
				_flags.Hidden = true;

				FreeModelDef();
				UpdateVisuals();
			}
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

			// TODO
			/*if ( !gameLocal.isNewFrame ) {
				return;
			}*/

			// don't present to the renderer if the entity hasn't changed
			/*if ( !( thinkFlags & TH_UPDATEVISUALS ) ) {
				return;
			}
			BecomeInactive( TH_UPDATEVISUALS );*/

			// camera target for remote render views
			/*if ( cameraTarget && gameLocal.InPlayerPVS( this ) ) {
				renderEntity.remoteRenderView = cameraTarget->GetRenderView();
			}*/

			// if set to invisible, skip
			if((_renderEntity.Model == null) || (this.IsHidden == true))
			{
				return;
			}

			// add to refresh list
			if(_renderModel == null)
			{
				_renderModel = idR.Game.CurrentRenderWorld.AddEntityDefinition(_renderEntity);
			}
			else
			{
				idR.Game.CurrentRenderWorld.UpdateEntityDefinition(_renderModel, _renderEntity);
			}
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
			}*/

			_flags.SolidForTeam = _spawnArgs.GetBool("solidForTeam", false);
			_flags.NeverDormant = _spawnArgs.GetBool("neverDormant", false);
			_flags.Hidden = _spawnArgs.GetBool("hide", false);

			if(_flags.Hidden == true)
			{
				// make sure we're hidden, since a spawn function might not set it up right
				idConsole.Warning("TODO: PostEventMS( &EV_Hide, 0 );");
			}
			/*cinematic = spawnArgs.GetBool( "cinematic", "0" );

			networkSync = spawnArgs.FindKey( "networkSync" );
			if ( networkSync ) {
				fl.networkSync = ( atoi( networkSync->GetValue() ) != 0 );
			}
		*/

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

			string temp = _spawnArgs.GetString("model");

			if(temp != string.Empty)
			{
				this.SetModel(temp);
			}

			if(_spawnArgs.GetString("bind", string.Empty) == string.Empty)
			{
				idConsole.Warning("TODO: PostEventMS( &EV_SpawnBind, 0 );");
			}

			// auto-start a sound on the entity
			// TODO
			/*if ( refSound.shader && !refSound.waitfortrigger ) {
				StartSoundShader( refSound.shader, SND_CHANNEL_ANY, 0, false, NULL );
			}*/

			// setup script object
			string scriptObjectName = _spawnArgs.GetString("scriptobject", string.Empty);

			if((this.ShouldConstructScriptObjectAtSpawn == true) && (scriptObjectName != string.Empty))
			{
				idConsole.Warning("TODO: script object");
				/*if ( !scriptObject.SetType( scriptObjectName ) ) {
					gameLocal.Error( "Script object '%s' not found on entity '%s'.", scriptObjectName, name.c_str() );
				}

				ConstructScriptObject();*/
			}
		}
				
		public virtual void Show()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(this.IsHidden == true)
			{
				_flags.Hidden = false;
				UpdateVisuals();
			}
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

			idConsole.Warning("TODO: RunPhysics();");
			Present();
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

	[Flags]
	public enum EntityThinkFlags
	{
		All = -1,
		/// <summary>Run think function each frame.</summary>
		Think = 1,
		/// <summary>Run physics each frame.</summary>
		Physics = 2,
		/// <summary>Update animation each frame.</summary>
		Animate = 4,
		/// <summary>Update renderEntity.</summary>
		UpdateVisuals,
		UpdateParticles = 16
	}

	public class EntityFlags
	{
		/// <summary>If true never attack or target this entity.</summary>
		public bool NoTarget;

		/// <summary>If true no knockback from hits.</summary>
		public bool NoKnockBack;

		/// <summary>If true this entity can be damaged.</summary>
		public bool TakeDamage;

		/// <summary>If true this entity is not visible.</summary>
		public bool Hidden;

		/// <summary>If true both the master orientation is used for binding.</summary>
		public bool BindOrientated;

		/// <summary>If true this entity is considered solid when a physics team mate pushes entities.</summary>
		public bool SolidForTeam;

		/// <summary>If true always update from the physics whether the object moved or not.</summary>
		public bool ForcePhysicsUpdate;

		/// <summary>If true the entity is selected for editing.</summary>
		public bool Selected;

		/// <summary>If true the entity never goes dormant.</summary>
		public bool NeverDormant;

		/// <summary>If true the entity is dormant.</summary>
		public bool IsDormant;

		/// <summary>Before a monster has been awakened the first time, use full PVS for dormant instead of area-connected.</summary>
		public bool HasAwakened;

		/// <summary>If true the entity is synchronized over the network.</summary>
		public bool NetworkSync;
	}
}