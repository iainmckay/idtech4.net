using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Game.Physics;

namespace idTech4.Game
{
	public class idEntity : IDisposable
	{
		#region Properties
		/// <summary>
		/// Index in to the entity list.
		/// </summary>
		public int Index
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}
				
				return _entityIndex;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				_entityIndex = value;
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
					throw new ObjectDisposedException("idEntity");
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
					throw new ObjectDisposedException("idEntity");
				}

				if(this.DefIndex < 0)
				{
					return "*unknown";
				}

				return idR.DeclManager.DeclByIndex(DeclType.EntityDef, this.DefIndex, false).Name;
			}
		}

		/// <summary>
		/// Name of the entity;
		/// </summary>
		public string Name
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				return _name;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

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

		public string ClassName
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				return _className;
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
					throw new ObjectDisposedException("idEntity");
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
					throw new ObjectDisposedException("idEntity");
				}

				return _spawnNode;
			}

			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				_spawnNode = value;
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
					throw new ObjectDisposedException("idEntity");
				}

				return _cinematic;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				_cinematic = value;
			}
		}

		public int Health
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				return _health;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				_health = value;
			}
		}

		/// <summary>
		///For camera views from this entity.
		/// </summary>
		public idRenderView RenderView
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				return _renderView;
			}
		}

		/// <summary>
		/// Used to present a model to the renderer
		/// </summary>
		public idRenderEntity RenderEntity
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				return _renderEntity;
			}
		}

		public idDeclSkin Skin
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				return _renderEntity.CustomSkin;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				_renderEntity.CustomSkin = value;


				UpdateVisuals();
			}
		}

		public Vector3 Origin
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				// TODO:
				return Vector3.Zero;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				// TODO GetPhysics()->SetOrigin(org);

				UpdateVisuals();
			}
		}

		public Matrix Axis
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				// TODO
				return Matrix.Identity;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException("idEntity");
				}

				// TODO
				/*if ( GetPhysics()->IsType( idPhysics_Actor::Type ) ) {
		static_cast<idActor *>(this)->viewAxis = axis;
	} else {
		GetPhysics()->SetAxis( axis );
	}*/

				UpdateVisuals();
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
		private idRenderEntity _renderEntity;

		private idDict _spawnArgs = new idDict();

		// for being linked into spawnedEntities list
		private LinkedListNode<idEntity> _spawnNode;
		// for being linked into activeEntities list
		private LinkedListNode<idEntity> _activeNode;

		private idStaticPhysics _defaultPhysicsObject;
		#endregion

		#region Constructor
		public idEntity()
		{
			_entityIndex = idR.MaxGameEntities - 1;
			_entityDefIndex = -1;

			_className = "unknown";

			_renderEntity = new idRenderEntity();

			/*			
			snapshotNode.SetOwner(this);
			snapshotSequence = -1;
			snapshotBits = 0;

			thinkFlags = 0;
			dormantStart = 0;*/
			_cinematic = false;
			/*renderView = NULL;
			cameraTarget = NULL;
			health = 0;

			physics = NULL;
			bindMaster = NULL;
			bindJoint = INVALID_JOINT;
			bindBody = -1;
			teamMaster = NULL;
			teamChain = NULL;
			signals = NULL;

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
		private void UpdateVisuals()
		{
			// TODO
			/*UpdateModel();
			UpdateSound();*/
		}

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

			if(_spawnArgs.GetBool("noclipmodel", "0") == false)
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

			// TODO
			/*defaultPhysicsObj.SetSelf( this );
			defaultPhysicsObj.SetClipModel( clipModel, 1.0f );
			defaultPhysicsObj.SetOrigin( origin );
			defaultPhysicsObj.SetAxis( axis );

			physics = &defaultPhysicsObj;*/
		}
		#endregion

		#region Public
		public virtual void Think()
		{
			// TODO
			/*RunPhysics();
			Present();*/
		}

		public virtual void Spawn()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idEntity");
			}
			/*int					i;
			const char			*temp;
			idVec3				origin;
			idMat3				axis;
			const idKeyValue	*networkSync;
			const char			*classname;
			const char			*scriptObjectName;*/

			idR.Game.RegisterEntity(this);

			_className = _spawnArgs.GetString("classname", null);

			idDeclEntityDef def = idR.Game.FindEntityDef(_className, false);

			if(def != null)
			{
				_entityDefIndex = def.Index;
			}
			/*
			TODO
			FixupLocalizedStrings();
			*/
			// parse static models the same way the editor display does

			idR.GameEdit.ParseSpawnArgsToRenderEntity(_spawnArgs, _renderEntity);

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

			_health = _spawnArgs.GetInt("health");

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
			// TODO
			/*
	if ( IsHidden() ) {
		fl.hidden = false;
		UpdateVisuals();
	}*/
		}

		public virtual void Hide()
		{
			// TODO
			/*if(!IsHidden())
			{
				fl.hidden = true;
				FreeModelDef();
				UpdateVisuals();
			}*/

		}

		public bool GetMasterPosition(out Vector3 masterOrigin, out Matrix masterAxis)
		{
			Vector3 localOrigin;
			Matrix localAxis;

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
			_disposed = true;
		}
		#endregion
	}
}