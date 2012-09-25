using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Collision;
using idTech4.Net;
using idTech4.Renderer;

namespace idTech4.Game.Physics
{
	/// <summary>
	/// Physics base for a moving object using one or more collision models.
	/// </summary>
	public class idPhysics_Base : idPhysics
	{
		#region Properties
		/// <summary>
		/// True if the whole physics object is outside the world bounds.
		/// </summary>
		public bool IsOutsideWorld
		{
			get
			{
				idConsole.Warning("idPhysics_Base.IsOutsideWorld");

				return false;
				/*if(idBounds.Expand(idR.Game.Clip.WorldBounds, 128.0f).IntersectsBounds(GetAbsoluteBounds()) == false)
				{
					return true;
				}

				return false;*/
			}
		}
		#endregion

		#region Members
		protected idEntity _self;					// entity using this physics object
		protected ContentFlags _clipMask;			// contents the physics object collides with
		protected Vector3 _gravityVector;			// direction and magnitude of gravity
		protected Vector3 _gravityNormal;			// normalized direction of gravity
	
		protected List<ContactInfo> _contacts = new List<ContactInfo>();	// contacts with other physics objects
		protected List<idEntity> _contactEntities = new List<idEntity>();	// entities touching this physics object
		#endregion

		#region Constructor
		public idPhysics_Base()
			: base()
		{
			this.Gravity = idR.Game.Gravity;
			ClearContacts();
		}

		~idPhysics_Base()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Protected
		protected void AddGroundContacts(idClipModel clipModel)
		{
			idConsole.Warning("TODO: idPhysics_Base.AddGroundContacts");

			/*idVec6 dir;
			int index, num;

			index = contacts.Num();
			contacts.SetNum(index + 10, false);

			dir.SubVec3(0) = gravityNormal;
			dir.SubVec3(1) = vec3_origin;
			num = gameLocal.clip.Contacts(&contacts[index], 10, clipModel->GetOrigin(),
							dir, CONTACT_EPSILON, clipModel, clipModel->GetAxis(), clipMask, self);
			contacts.SetNum(index + num, false);*/
		}

		/// <summary>
		/// Add ground contacts for the clip model.
		/// </summary>
		/// <param name="clipModel"></param>
		protected void AddGroupContacts(idClipModel clipModel)
		{
			idConsole.Warning("idPhysics_Base.AddGroupContacts");

			/*idVec6 dir;
			int index, num;

			index = contacts.Num();
			contacts.SetNum( index + 10, false );

			dir.SubVec3(0) = gravityNormal;
			dir.SubVec3(1) = vec3_origin;
			num = gameLocal.clip.Contacts( &contacts[index], 10, clipModel->GetOrigin(),
							dir, CONTACT_EPSILON, clipModel, clipModel->GetAxis(), clipMask, self );
			contacts.SetNum( index + num, false );*/
		}

		/// <summary>
		/// Add contact entity links to contact entities.
		/// </summary>
		protected void AddContactEntitiesForContacts()
		{
			int count = _contacts.Count;

			for(int i = 0; i < count; i++)
			{
				idEntity ent = idR.Game.Entities[_contacts[i].EntityIndex];

				if((ent != null) && (ent != this.Self))
				{
					ent.AddContactEntity(this.Self);
				}
			}
		}

		/// <summary>
		/// Active all contact entities.
		/// </summary>
		protected void ActivateContactEntities()
		{
			for(int i = 0; i < _contactEntities.Count; i++)
			{
				idEntity ent = _contactEntities[i];

				if(ent != null)
				{
					ent.ActivatePhysics(this.Self);
				}
				else
				{
					_contactEntities.RemoveAt(i--);
				}
			}
		}

		/// <summary>
		/// Draw linear and angular velocity.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="linearScale"></param>
		/// <param name="angularScale"></param>
		protected void DrawVelocity(int id, float linearScale, float angularScale)
		{
			idConsole.Warning("idPhysics_Base.DrawVelocity");

			/*idVec3 dir, org, vec, start, end;
			idMat3 axis;
			float length, a;

			dir = GetLinearVelocity( id );
			dir *= linearScale;
			if ( dir.LengthSqr() > Square( 0.1f ) ) {
				dir.Truncate( 10.0f );
				org = GetOrigin( id );
				gameRenderWorld->DebugArrow( colorRed, org, org + dir, 1 );
			}

			dir = GetAngularVelocity( id );
			length = dir.Normalize();
			length *= angularScale;
			if ( length > 0.1f ) {
				if ( length < 60.0f ) {
					length = 60.0f;
				}
				else if ( length > 360.0f ) {
					length = 360.0f;
				}
				axis = GetAxis( id );
				vec = axis[2];
				if ( idMath::Fabs( dir * vec ) > 0.99f ) {
					vec = axis[0];
				}
				vec -= vec * dir * vec;
				vec.Normalize();
				vec *= 4.0f;
				start = org + vec;
				for ( a = 20.0f; a < length; a += 20.0f ) {
					end = org + idRotation( vec3_origin, dir, -a ).ToMat3() * vec;
					gameRenderWorld->DebugLine( colorBlue, start, end, 1 );
					start = end;
				}
				end = org + idRotation( vec3_origin, dir, -length ).ToMat3() * vec;
				gameRenderWorld->DebugArrow( colorBlue, start, end, 1 );
			}*/
		}
		#endregion
		#endregion

		#region idPhysics implementation
		#region Properties
		public override int AngularEndTime
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return 0;
			}
		}

		public override idEntity BlockingEntity
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
		
		public override TraceResult BlockingInfo
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

		public override int ClipModelCount
		{
			get
			{
				return 0;
			}
		}

		public override int ContactCount
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _contacts.Count;
			}
		}

		public override Vector3 Gravity
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _gravityVector;
			}

			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_gravityVector = value;
				_gravityNormal = value;
				_gravityNormal.Normalize();
			}
		}

		public override Vector3 GravityNormal
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _gravityNormal;
			}
		}

		public override bool HasGroundContacts
		{
			get
			{
				int count = _contacts.Count;

				for(int i = 0; i < count; i++)
				{
					if((_contacts[i].Normal * -_gravityNormal).Length() > 0.0f)
					{
						return true;
					}
				}

				return false;
			}
		}

		public override bool IsAtRest
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return true;
			}
		}
		
		public override bool IsPushable
		{
			get
			{
				return true;
			}
		}

		public override int LinearEndTime
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return 0;
			}
		}

		public override int RestStartTime
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return 0;
			}
		}

		public override idEntity Self
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _self;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_self = value;
			}
		}

		public override int Time
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return 0;
			}
		}
		#endregion

		#region Methods
		public override void Save(object saveFile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Base.Save");

			/*int i;

			savefile->WriteObject( self );
			savefile->WriteInt( clipMask );
			savefile->WriteVec3( gravityVector );
			savefile->WriteVec3( gravityNormal );

			savefile->WriteInt( contacts.Num() );
			for ( i = 0; i < contacts.Num(); i++ ) {
				savefile->WriteContactInfo( contacts[i] );
			}

			savefile->WriteInt( contactEntities.Num() );
			for ( i = 0; i < contactEntities.Num(); i++ ) {
				contactEntities[i].Save( savefile );
			}*/
		}

		public override void Restore(object saveFile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Base.Restore");


			/*int i, num;

			savefile->ReadObject( reinterpret_cast<idClass *&>( self ) );
			savefile->ReadInt( clipMask );
			savefile->ReadVec3( gravityVector );
			savefile->ReadVec3( gravityNormal );

			savefile->ReadInt( num );
			contacts.SetNum( num );
			for ( i = 0; i < contacts.Num(); i++ ) {
				savefile->ReadContactInfo( contacts[i] );
			}

			savefile->ReadInt( num );
			contactEntities.SetNum( num );
			for ( i = 0; i < contactEntities.Num(); i++ ) {
				contactEntities[i].Restore( savefile );
			}*/
		}

		public override void SetClipModel(idClipModel model, float density, int id = 0, bool disposeOld = true)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override idClipModel GetClipModel(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return null;
		}

		public override void SetMass(float mass, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override float GetMass(int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return 0.0f;
		}

		public override void SetContents(ContentFlags contents, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override ContentFlags GetContents(int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return 0;
		}

		public override void SetClipMask(ContentFlags mask, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_clipMask = mask;
		}

		public override ContentFlags GetClipMask(int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _clipMask;
		}

		public override idBounds GetBounds(int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return idBounds.Zero;
		}

		public override idBounds GetAbsoluteBounds(int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return idBounds.Zero;
		}

		public override bool Evaluate(int timeStep, int endTime)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return false;
		}

		public override void UpdateTime(int endTime)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override ImpactInfo GetImpactInfo(int id, Vector3 point)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return new ImpactInfo();
		}

		public override void ApplyImpulse(int id, Vector3 point, Vector3 impulse)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void AddForce(int id, Vector3 point, Vector3 force)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void Activate()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void PutToRest()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void SaveState()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void RestoreState()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void SetOrigin(Vector3 origin, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void SetAxis(Matrix axis, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void Translate(Vector3 translation, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void Rotate(idRotation rotation, int id = 1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override Vector3 GetOrigin(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return Vector3.Zero;
		}

		public override Matrix GetAxis(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return Matrix.Identity;
		}

		public override void SetLinearVelocity(Vector3 velocity, int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void SetAngularVelocity(Vector3 velocity, int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override Vector3 GetLinearVelocity(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return Vector3.Zero;
		}

		public override Vector3 GetAngularVelocity(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return Vector3.Zero;
		}

		public override TraceResult ClipTranslation(Vector3 translation, idClipModel model)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return new TraceResult();
		}

		public override TraceResult ClipRotation(idRotation rotation, idClipModel model)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return new TraceResult();
		}

		public override ContentFlags ClipContents(idClipModel model)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return ContentFlags.None;
		}

		public override void DisableClip()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void EnableClip()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void UnlinkClip()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void LinkClip()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override bool EvaluateContacts()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return false;
		}

		public override ContactInfo GetContact(int index)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _contacts[index];
		}

		public override void ClearContacts()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			int count = _contacts.Count;

			for(int i = 0; i < count; i++)
			{
				idEntity ent = idR.Game.Entities[_contacts[i].EntityIndex];

				if(ent != null)
				{
					ent.RemoveContactEntity(_self);
				}
			}

			_contacts.Clear();
		}

		public override void AddContactEntity(idEntity entity)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			bool found = false;

			for(int i = 0; i < _contactEntities.Count; i++)
			{
				idEntity ent = _contactEntities[i];

				if(ent == null)
				{
					_contactEntities.RemoveAt(i--);
				}

				if(ent == entity)
				{
					found = true;
				}
			}

			if(found == false)
			{
				_contactEntities.Add(entity);
			}
		}

		public override void RemoveContactEntity(idEntity entity)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			for(int i = 0; i < _contactEntities.Count; i++)
			{
				idEntity ent = _contactEntities[i];

				if(ent == null)
				{
					_contactEntities.RemoveAt(i--);
				}
				else if(ent == entity)
				{
					_contactEntities.RemoveAt(i--);
					return;
				}
			}				
		}

		public override bool IsGroundEntity(int index)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			int count = _contacts.Count;

			for(int i = 0; i < count; i++)
			{
				if((_contacts[i].EntityIndex == i) && ((_contacts[i].Normal * -_gravityNormal).X > 0.0f))
				{
					return true;
				}
			}

			return false;
		}

		public override bool IsGroundClipModel(int index, int id)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			int count = _contacts.Count;

			for(int i = 0; i < count; i++)
			{
				if((_contacts[i].EntityIndex == index) && (_contacts[i].ID == id) && ((_contacts[i].Normal * -_gravityNormal).X > 0.0f))
				{
					return true;
				}
			}

			return false;
		}

		public override void SetPushed(int deltaTime)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override Vector3 GetPushedLinearVelocity(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return Vector3.Zero;
		}

		public override Vector3 GetPushedAngularVelocity(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return Vector3.Zero;
		}

		public override void SetMaster(idEntity master, bool orientated = true)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public override void WriteToSnapshot(idBitMsgDelta msg)
		{
			
		}

		public override void ReadFromSnapshot(idBitMsgDelta msg)
		{
			
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			
			if((_self != null) && (_self.Physics == this))
			{
				idConsole.Warning("TODO: _self.Physics = null;");
			}

			idConsole.Warning("TODO: idForce::DeletePhysics( this );");
		}
		#endregion
		#endregion
	}
}