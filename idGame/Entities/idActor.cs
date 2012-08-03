using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Game.Rules;

namespace idTech4.Game.Entities
{
	public class idActor : idAFEntity_Gibbable
	{
		#region Properties
		public PlayerTeam Team
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
		#endregion

		#region Members
		private PlayerTeam _team;
		#endregion

		#region Constructor
		public idActor()
			: base()
		{
			// TODO
			idConsole.Warning("TODO: idActor");
			/*viewAxis.Identity();

			scriptThread = NULL;		// initialized by ConstructScriptObject, which is called by idEntity::Spawn

			use_combat_bbox = false;
			head = NULL;*/

			_team = PlayerTeam.Red;
			/*rank = 0;
			fovDot = 0.0f;
			eyeOffset.Zero();
			pain_debounce_time = 0;
			pain_delay = 0;
			pain_threshold = 0;

			state = NULL;
			idealState = NULL;

			leftEyeJoint = INVALID_JOINT;
			rightEyeJoint = INVALID_JOINT;
			soundJoint = INVALID_JOINT;

			modelOffset.Zero();
			deltaViewAngles.Zero();

			painTime = 0;
			allowPain = false;
			allowEyeFocus = false;

			waitState = "";

			blink_anim = NULL;
			blink_time = 0;
			blink_min = 0;
			blink_max = 0;

			finalBoss = false;

			attachments.SetGranularity(1);

			enemyNode.SetOwner(this);
			enemyList.SetOwner(this);*/
		}
		#endregion

		#region idEntity implementation
		public override void Spawn()
		{
			base.Spawn();

			// TODO
			idConsole.Warning("TODO: idActor.Spawn");
			/*idEntity* ent;
			idStr jointName;
			float fovDegrees;
			copyJoints_t copyJoint;

			animPrefix = "";
			state = NULL;
			idealState = NULL;

			spawnArgs.GetInt("rank", "0", rank);
			spawnArgs.GetInt("team", "0", team);
			spawnArgs.GetVector("offsetModel", "0 0 0", modelOffset);

			spawnArgs.GetBool("use_combat_bbox", "0", use_combat_bbox);

			viewAxis = GetPhysics()->GetAxis();

			spawnArgs.GetFloat("fov", "90", fovDegrees);
			SetFOV(fovDegrees);

			pain_debounce_time = 0;

			pain_delay = SEC2MS(spawnArgs.GetFloat("pain_delay"));
			pain_threshold = spawnArgs.GetInt("pain_threshold");

			LoadAF();

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
			}

			finalBoss = spawnArgs.GetBool("finalBoss");

			FinishSetup();*/
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

		public override void Hide()
		{
			base.Hide();

		// TODO	
			idConsole.Warning("TODO: idActor.Hide");
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
	}
	UnlinkCombat();*/
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
	}
}
