using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Game.Physics;
using idTech4.Game.Rules;
using idTech4.Game.Scripting;
using idTech4.Math;
using idTech4.Renderer;

namespace idTech4.Game.Entities
{
	public class idActor : idAFEntity_Gibbable
	{
		#region Properties
		/// <summary>
		/// Delta angles relative to view input angles.
		/// </summary>
		public idAngles DeltaViewAngles
		{
			get
			{
				return _deltaViewAngles;
			}
			set
			{
				_deltaViewAngles = value;
			}
		}

		public virtual Vector3 EyePosition
		{
			get
			{
				return (this.Physics.GetOrigin() + (this.Physics.GravityNormal * -_eyeOffset.Z));
			}
		}

		public float Fov
		{
			set
			{
				_fovDot = (float) idMath.Cos(MathHelper.ToRadians(value * 0.5f));
			}
		}

		public virtual bool IsOnLadder
		{
			get
			{
				return false;
			}
		}

		public virtual PlayerTeam Team
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _team;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_team = value;
			}
		}

		public virtual Matrix ViewAxis
		{
			get
			{
				return _viewAxis;
			}
			set
			{
				_viewAxis = value;
			}
		}
		#endregion

		#region Members
		private PlayerTeam _team;
		private int _rank;

		private int _painDebounceTime;	// next time the actor can show pain
		private int _painDelay;			// time between playing pain sound
		private int _painThreshold;		// how much damage monster can take at any one time before playing pain animation
		
		protected Matrix _viewAxis;		// view axis of the actor

		private idAngles _deltaViewAngles;	// delta angles relative to view input angles
		
		protected float _fovDot;			// cos( fovDegrees )
		protected Vector3 _eyeOffset;		// offset of eye relative to physics origin
		protected Vector3 _modelOffset;	// offset of visual model relative to the physics origin
			
		private bool _useCombatBoundingBox;	// whether to use the bounding box for combat collision

		private bool _allowPain;
		private bool _allowEyeFocus;
		private bool _finalBoss;

		private int _leftEyeJoint;
		private int _rightEyeJoint;
		private int _soundJoint;

		private string _painAnim;
		private string _animPrefix;

		// script variables
		private string _waitState;
		private idThread _scriptThread;
		#endregion

		#region Constructor
		public idActor()
			: base()
		{
			_team = PlayerTeam.Red;
			_waitState = string.Empty;

			_leftEyeJoint = -1;
			_rightEyeJoint = -1;
			_soundJoint = -1;
		}

		~idActor()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		public virtual void GetAASLocation(object aas, Vector3 position, ref int areaNum)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.GetAASLocation");
		}

		/// <summary>
		/// Gets positions for the AI to aim at.
		/// </summary>
		/// <param name="lastSightPosition"></param>
		/// <param name="headPosition"></param>
		/// <param name="chestPosition"></param>
		public void GetAIAimTargets(Vector3 lastSightPosition, ref Vector3 headPosition, Vector3 chestPosition)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.GetAIAimTargets");
		}

		public virtual void GetViewPosition(out Vector3 origin, out Matrix axis)
		{
			origin = this.EyePosition;
			axis = _viewAxis;
		}
		
		public virtual void Restart()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.Restart");
		}
		#endregion

		#region Private
		private void FinishSetup()
		{
			// setup script object
			string scriptObjectName = this.SpawnArgs.GetString("scriptobject");

			if(scriptObjectName != string.Empty)
			{
				idConsole.Warning("TODO: finish setup - script object");
				/*if ( !scriptObject.SetType( scriptObjectName ) ) {
					gameLocal.Error( "Script object '%s' not found on entity '%s'.", scriptObjectName, name.c_str() );
				}

				ConstructScriptObject();*/
			}

			idConsole.Warning("TODO: SetupBody();");
		}
		#endregion
		#endregion

		#region idAFEntity_Gibbable implementation
		#region Properties
		public override idClipModel CombatModel
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idActor.CombatModel get");

				return null;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idActor.CombatModel set");
			}
		}

		public override SurfaceTypes DefaultSurfaceType
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return Renderer.SurfaceTypes.Flesh;
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

				idConsole.Warning("TODO: idActor.RenderView get");

				return null;
			}
		}

		public override bool ShouldConstructScriptObjectAtSpawn
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Methods
		public override idThread ConstructScriptObject()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.ConstructScriptObject");

			return null;
		}

		public override void Damage(idEntity inflictor, idEntity attacker, Vector3 direction, string damageDefName, float damageScale, int location)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.Damage");
		}

		public override bool GetPhysicsToSoundTransform(ref Vector3 origin, ref Matrix axis)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.GetPhysicsToSoundTransform");

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

			origin = _modelOffset;
			axis = _viewAxis;

			return true;
		}

		public override void Hide()
		{
			base.Hide();

			// TODO	
			idConsole.Warning("TODO: idActor.HideHead");
			idConsole.Warning("TODO: idActor.HideTeamEntities");
			/*idAFEntity_Base::Hide();
			if ( head.GetEntity() ) {
				head.GetEntity()->Hide();
			}

			for( ent = GetNextTeamEntity(); ent != NULL; ent = next ) {
				next = ent->GetNextTeamEntity();
				if ( ent->GetBindMaster() == this ) {
					ent->Hide();
					if ( ent->IsType( idLight::Type ) ) {
						static_cast<idLight *>( ent )->Off();
					}
				}
			}*/

			UnlinkCombat();
		}

		public override void LinkCombat()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.LinkCombat");
		}		

		public override bool LoadAF()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.LoadAF");

			return false;
		}

		public override bool Pain(idEntity inflictor, idEntity attacker, int damage, Vector3 direction, int location)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.Pain");

			return false;
		}

		public override void ProjectOverlay(Microsoft.Xna.Framework.Vector3 origin, Microsoft.Xna.Framework.Vector3 direction, float size, string material)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.ProjectOverlay");
		}

		public override void Restore(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.Restore");
		}

		public override void Save(object savefile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.Save");
		}

		public override void Show()
		{
			base.Show();

			// TODO
			idConsole.Warning("TODO: idActor.Show");
			/*idEntity *ent;
	idEntity *next;

	idAFEntity_Base::Show();
	if ( head.GetEntity() ) {
		head.GetEntity()->Show();
	}
	for( ent = GetNextTeamEntity(); ent != NULL; ent = next ) {
		next = ent->GetNextTeamEntity();
		if ( ent->GetBindMaster() == this ) {
			ent->Show();
			if ( ent->IsType( idLight::Type ) ) {
				static_cast<idLight *>( ent )->On();
			}
		}
	}
	LinkCombat();*/
		}

		public override void Spawn()
		{
			base.Spawn();

			// TODO
			idConsole.Warning("TODO: idActor.Spawn");
			/*
			state = NULL;
			idealState = NULL;*/

			_animPrefix = null;

			_rank = this.SpawnArgs.GetInteger("rank", 0);
			_team = (PlayerTeam) this.SpawnArgs.GetInteger("team", 0);
			_modelOffset = this.SpawnArgs.GetVector3("offsetModel", Vector3.Zero);
			_useCombatBoundingBox = this.SpawnArgs.GetBool("use_combat_bbox", false);
			_viewAxis = this.Physics.GetAxis();			
			_finalBoss = this.SpawnArgs.GetBool("finalBoss");

			_painDebounceTime = 0;
			_painDelay = (int) (this.SpawnArgs.GetFloat("pain_delay") * 1000.0f);
			_painThreshold = this.SpawnArgs.GetInteger("pain_threshold");

			this.Fov = this.SpawnArgs.GetFloat("fov", 90);

			/*LoadAF();

			walkIK.Init(this, IK_ANIM, modelOffset);

			// the animation used to be set to the IK_ANIM at this point, but that was fixed, resulting in
			// attachments not binding correctly, so we're stuck setting the IK_ANIM before attaching things.
			animator.ClearAllAnims(gameLocal.time, 0);
			animator.SetFrame(ANIMCHANNEL_ALL, animator.GetAnim(IK_ANIM), 0, 0, 0);

			// spawn any attachments we might have
			const idKeyValue* kv = spawnArgs.MatchPrefix("def_attach", NULL);
			while(kv)
			{
				idDict args;

				args.Set("classname", kv->GetValue().c_str());

				// make items non-touchable so the player can't take them out of the character's hands
				args.Set("no_touch", "1");

				// don't let them drop to the floor
				args.Set("dropToFloor", "0");

				gameLocal.SpawnEntityDef(args, &ent);
				if(!ent)
				{
					gameLocal.Error("Couldn't spawn '%s' to attach to entity '%s'", kv->GetValue().c_str(), name.c_str());
				}
				else
				{
					Attach(ent);
				}
				kv = spawnArgs.MatchPrefix("def_attach", kv);
			}

			SetupDamageGroups();
			SetupHead();

			// clear the bind anim
			animator.ClearAllAnims(gameLocal.time, 0);

			idEntity* headEnt = head.GetEntity();
			idAnimator* headAnimator;
			if(headEnt)
			{
				headAnimator = headEnt->GetAnimator();
			}
			else
			{
				headAnimator = &animator;
			}

			if(headEnt)
			{
				// set up the list of joints to copy to the head
				for(kv = spawnArgs.MatchPrefix("copy_joint", NULL); kv != NULL; kv = spawnArgs.MatchPrefix("copy_joint", kv))
				{
					if(kv->GetValue() == "")
					{
						// probably clearing out inherited key, so skip it
						continue;
					}

					jointName = kv->GetKey();
					if(jointName.StripLeadingOnce("copy_joint_world "))
					{
						copyJoint.mod = JOINTMOD_WORLD_OVERRIDE;
					}
					else
					{
						jointName.StripLeadingOnce("copy_joint ");
						copyJoint.mod = JOINTMOD_LOCAL_OVERRIDE;
					}

					copyJoint.from = animator.GetJointHandle(jointName);
					if(copyJoint.from == INVALID_JOINT)
					{
						gameLocal.Warning("Unknown copy_joint '%s' on entity %s", jointName.c_str(), name.c_str());
						continue;
					}

					jointName = kv->GetValue();
					copyJoint.to = headAnimator->GetJointHandle(jointName);
					if(copyJoint.to == INVALID_JOINT)
					{
						gameLocal.Warning("Unknown copy_joint '%s' on head of entity %s", jointName.c_str(), name.c_str());
						continue;
					}

					copyJoints.Append(copyJoint);
				}
			}

			// set up blinking
			blink_anim = headAnimator->GetAnim("blink");
			blink_time = 0;	// it's ok to blink right away
			blink_min = SEC2MS(spawnArgs.GetFloat("blink_min", "0.5"));
			blink_max = SEC2MS(spawnArgs.GetFloat("blink_max", "8"));

			// set up the head anim if necessary
			int headAnim = headAnimator->GetAnim("def_head");
			if(headAnim)
			{
				if(headEnt)
				{
					headAnimator->CycleAnim(ANIMCHANNEL_ALL, headAnim, gameLocal.time, 0);
				}
				else
				{
					headAnimator->CycleAnim(ANIMCHANNEL_HEAD, headAnim, gameLocal.time, 0);
				}
			}

			if(spawnArgs.GetString("sound_bone", "", jointName))
			{
				soundJoint = animator.GetJointHandle(jointName);
				if(soundJoint == INVALID_JOINT)
				{
					gameLocal.Warning("idAnimated '%s' at (%s): cannot find joint '%s' for sound playback", name.c_str(), GetPhysics()->GetOrigin().ToString(0), jointName.c_str());
				}
			}*/			

			FinishSetup();
		}

		public override void SpawnGibs(Vector3 direction, string damageDefName)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.SpawnGibs");
		}

		public override void Teleport(Vector3 origin, idAngles angles, idEntity destination)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.Teleport");
		}

		public override void UnlinkCombat()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.UnlinkCombat");
		}

		public override bool UpdateAnimationControllers()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idActor.UpdateAnimationControllers");

			return false;
		}
		
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// TODO
			idConsole.Warning("TODO: idActor.Dispose");
		
				/*int i;
				idEntity* ent;

				DeconstructScriptObject();
				scriptObject.Free();

				StopSound(SND_CHANNEL_ANY, false);

				delete combatModel;
				combatModel = NULL;

				if(head.GetEntity())
				{
					head.GetEntity()->ClearBody();
					head.GetEntity()->PostEventMS(&EV_Remove, 0);
				}

				// remove any attached entities
				for(i = 0; i < attachments.Num(); i++)
				{
					ent = attachments[i].ent.GetEntity();
					if(ent)
					{
						ent->PostEventMS(&EV_Remove, 0);
					}
				}
	

			ShutdownThreads();*/
		}
		#endregion
		#endregion
	}
}