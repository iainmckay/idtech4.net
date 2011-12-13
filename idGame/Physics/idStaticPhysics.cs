using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace idTech4.Game.Physics
{
	public class idStaticPhysics : idPhysics
	{
		#region Members
		private idEntity _self;
		private idClipModel _clipModel;

		private Vector3 _origin;
		private Matrix _axis;
		private Vector3 _localOrigin;
		private Matrix _localAxis;

		private bool _hasMaster;
		private bool _isOrientated;
		#endregion

		#region Constructor
		public idStaticPhysics()
			: base()
		{

		}

		~idStaticPhysics()
		{
			// TODO
			/*if ( self && self->GetPhysics() == this ) {
				self->SetPhysics( NULL );
			}
			idForce::DeletePhysics( this );
			if ( clipModel ) {
				delete clipModel;
			}*/
		}
		#endregion

		#region idPhysics implementation
		#region Properties
		public override idEntity Self
		{
			get
			{
				return _self;
			}
			set
			{
				_self = value;
			}
		}

		public override int ClipModelCount
		{
			get
			{
				if(_clipModel != null)
				{
					return 1;
				}

				return 0;
			}
		}

		public override int Time
		{
			get
			{
				return 0;
			}
		}

		public override bool IsAtRest
		{
			get
			{
				return true;
			}
		}

		public override int RestStartTime
		{
			get
			{
				return 0;
			}
		}

		public override bool IsPushable
		{
			get
			{
				return false;
			}
		}

		public override Vector3 Gravity
		{
			get
			{
				return new Vector3(0, 0, -idR.CvarSystem.GetFloat("g_gravity"));
			}
			set
			{

			}
		}

		public override Vector3 GravityNormal
		{
			get
			{
				return new Vector3(0, 0, -1);
			}
		}

		public override int ContactCount
		{
			get
			{
				return 0;
			}
		}

		public override bool HasGroundContacts
		{
			get
			{
				return false;
			}
		}

		public override TraceResult BlockingInfo
		{
			get
			{
				return new TraceResult();
			}
		}

		public override idEntity BlockingEntity
		{
			get
			{
				return null;
			}
		}

		public override int LinearEndTime
		{
			get
			{
				return 0;
			}
		}

		public override int AngularEndTime
		{
			get
			{
				return 0;
			}
		}
		#endregion

		#region Methods
		public override void Save(idSaveGame saveFile)
		{
			base.Save(saveFile);

			// TODO
			/*savefile->WriteObject( self );

			savefile->WriteVec3( current.origin );
			savefile->WriteMat3( current.axis );
			savefile->WriteVec3( current.localOrigin );
			savefile->WriteMat3( current.localAxis );
			savefile->WriteClipModel( clipModel );

			savefile->WriteBool( hasMaster );
			savefile->WriteBool( isOrientated );*/
		}

		public override void Restore(idRestoreGame saveFile)
		{
			base.Restore(saveFile);

			// TODO
			/*savefile->ReadObject( reinterpret_cast<idClass *&>( self ) );

			savefile->ReadVec3( current.origin );
			savefile->ReadMat3( current.axis );
			savefile->ReadVec3( current.localOrigin );
			savefile->ReadMat3( current.localAxis );
			savefile->ReadClipModel( clipModel );

			savefile->ReadBool( hasMaster );
			savefile->ReadBool( isOrientated );*/
		}

		public override void SetClipModel(idClipModel model, float density, int id, bool disposeOld)
		{
			if((_clipModel != null) && (_clipModel != model) && (disposeOld == true))
			{
				_clipModel.Dispose();
			}

			_clipModel = model;

			if(_clipModel != null)
			{
				_clipModel.Link(idR.Game.Clip, _self, 0, _origin, _axis);
			}
		}

		public override idClipModel GetClipModel(int id)
		{

			if(_clipModel != null)
			{
				return _clipModel;
			}

			return idR.Game.Clip.DefaultClipModel;
		}

		public override void SetMass(float mass, int id)
		{

		}

		public override float GetMass(int id)
		{
			return 0.0f;
		}

		public override void SetContents(ContentFlags contents, int id)
		{
			if(_clipModel != null)
			{
				_clipModel.Contents = contents;
			}
		}

		public override ContentFlags GetContents(int id)
		{
			if(_clipModel != null)
			{
				return _clipModel.Contents;
			}

			return ContentFlags.None;
		}

		public override void SetClipMask(int mask, int id)
		{

		}

		public override int GetClipMask(int id)
		{
			return 0;
		}

		public override idBounds GetBounds(int id)
		{
			if(_clipModel != null)
			{
				return _clipModel.Bounds;
			}

			return new idBounds();
		}

		public override idBounds GetAbsoluteBounds(int id)
		{
			if(_clipModel != null)
			{
				return _clipModel.AbsoluteBounds;
			}

			return new idBounds(_origin, _origin);
		}

		public override bool Evaluate(int timeStep, int endTime)
		{
			Vector3 masterOrigin, oldOrigin;
			Matrix masterAxis, oldAxis;

			if(_hasMaster == true)
			{
				oldOrigin = _origin;
				oldAxis = _axis;

				_self.GetMasterPosition(out masterOrigin, out masterAxis);
				_origin = masterOrigin + _localOrigin * masterAxis;

				if(_isOrientated == true)
				{
					_axis = _localAxis * masterAxis;
				}
				else
				{
					_axis = _localAxis;
				}

				if(_clipModel != null)
				{
					_clipModel.Link(idR.Game.Clip, _self, 0, _origin, _axis);
				}

				return ((_origin != oldOrigin) || (_axis != oldAxis));
			}

			return false;
		}

		public override void UpdateTime(int endTime)
		{

		}

		public override ImpactInfo GetImpactInfo(int id, Vector3 point)
		{
			return new ImpactInfo();
		}

		public override void ApplyImpulse(int id, Vector3 point, Vector3 impulse)
		{

		}

		public override void AddForce(int id, Vector3 point, Vector3 force)
		{

		}

		public override void Activate()
		{

		}

		public override void PutToRest()
		{

		}

		public override void SaveState()
		{

		}

		public override void RestoreState()
		{

		}

		public override void SetOrigin(Vector3 origin, int id)
		{
			Vector3 masterOrigin;
			Matrix masterAxis;

			_localOrigin = origin;

			if(_hasMaster == true)
			{
				_self.GetMasterPosition(out masterOrigin, out masterAxis);
				_origin = masterOrigin + origin * masterAxis;
			}
			else
			{
				_origin = origin;
			}

			if(_clipModel != null)
			{
				_clipModel.Link(idR.Game.Clip, _self, 0, _origin, _axis);
			}
		}

		public override void SetAxis(Matrix axis, int id)
		{
			Vector3 masterOrigin;
			Matrix masterAxis;

			_localAxis = axis;

			if((_hasMaster == true) && (_isOrientated == true))
			{
				_self.GetMasterPosition(out masterOrigin, out masterAxis);
				_axis = axis * masterAxis;
			}
			else
			{
				_axis = axis;
			}

			if(_clipModel != null)
			{
				_clipModel.Link(idR.Game.Clip, _self, 0, _origin, _axis);
			}
		}

		public override void Translate(Vector3 translation, int id)
		{
			_localOrigin += translation;
			_origin += translation;

			if(_clipModel != null)
			{
				_clipModel.Link(idR.Game.Clip, _self, 0, _origin, _axis);
			}
		}

		public override void Rotate(idRotation rotation, int id)
		{
			Vector3 masterOrigin;
			Matrix masterAxis;

			_origin *= rotation;
			_axis *= rotation.ToMatrix();

			if(_hasMaster == true)
			{
				_self.GetMasterPosition(out masterOrigin, out masterAxis);

				_localAxis *= rotation.ToMatrix();
				_localOrigin = (_origin - masterOrigin) * Matrix.Transpose(masterAxis);
			}
			else
			{
				_localAxis = _axis;
				_localOrigin = _origin;
			}

			if(_clipModel != null)
			{
				_clipModel.Link(idR.Game.Clip, _self, 0, _origin, _axis);
			}
		}

		public override Vector3 GetOrigin(int id)
		{
			return _origin;
		}

		public override Matrix GetAxis(int id)
		{
			return _axis;
		}

		public override void SetLinearVelocity(Vector3 velocity, int id)
		{

		}

		public override void SetAngularVelocity(Vector3 velocity, int id)
		{

		}

		public override Vector3 GetLinearVelocity(int id)
		{
			return Vector3.Zero;
		}

		public override Vector3 GetAngularVelocity(int id)
		{
			return Vector3.Zero;
		}

		public override TraceResult ClipTranslation(Vector3 translation, idClipModel model)
		{
			TraceResult result;

			if(model != null)
			{
				result = idR.Game.Clip.TranslationModel(_origin, _origin + translation, _clipModel,
					_axis, ContentFlags.MaskSolid, model.Handle, model.Origin, model.Axis);
			}
			else
			{
				idR.Game.Clip.Translation(out result, _origin, _origin + translation, _clipModel, _axis, ContentFlags.MaskSolid, _self);
			}

			return result;
		}

		public override TraceResult ClipRotation(idRotation rotation, idClipModel model)
		{
			TraceResult result;

			if(model != null)
			{
				result = idR.Game.Clip.RotationModel(_origin, rotation, _clipModel, _axis, ContentFlags.MaskSolid,
					model.Handle, model.Origin, model.Axis);
			}
			else
			{
				idR.Game.Clip.Rotation(out result, _origin, rotation, _clipModel, _axis, ContentFlags.MaskSolid, _self);
			}

			return result;
		}

		public override ContentFlags ClipContents(idClipModel model)
		{
			if(_clipModel != null)
			{
				if(model != null)
				{
					return idR.Game.Clip.ContentsModel(_clipModel.Origin, _clipModel, _clipModel.Axis, ContentFlags.None,
						model.Handle, model.Origin, model.Axis);
				}

				return idR.Game.Clip.Contents(_clipModel.Origin, _clipModel, _clipModel.Axis, ContentFlags.None, null);
			}

			return 0;
		}

		public override void DisableClip()
		{
			if(_clipModel != null)
			{
				_clipModel.Enabled = false;
			}
		}

		public override void EnableClip()
		{
			if(_clipModel != null)
			{
				_clipModel.Enabled = true;
			}
		}

		public override void UnlinkClip()
		{
			if(_clipModel != null)
			{
				_clipModel.Unlink();
			}
		}

		public override void LinkClip()
		{
			if(_clipModel != null)
			{
				_clipModel.Link(idR.Game.Clip, _self, 0, _origin, _axis);
			}
		}

		public override bool EvaluateContacts()
		{
			return false;
		}

		public override ContactInfo GetContact(int index)
		{
			return new ContactInfo();
		}

		public override void ClearContacts()
		{

		}

		public override void AddContactEntity(idEntity entity)
		{

		}

		public override void RemoveContactEntity(idEntity entity)
		{

		}

		public override bool IsGroundEntity(int entityIndex)
		{
			return false;
		}

		public override bool IsGroundClipModel(int entityIndex, int id)
		{
			return false;
		}

		public override void SetPushed(int deltaTime)
		{

		}

		public override Vector3 GetPushedAngularVelocity(int id)
		{
			return Vector3.Zero;
		}

		public override Vector3 GetPushedLinearVelocity(int id)
		{
			return Vector3.Zero;
		}

		public override void SetMaster(idEntity master, bool orientated)
		{
			Vector3 masterOrigin;
			Matrix masterAxis;

			if(master != null)
			{
				if(_hasMaster == false)
				{
					// transform from world space to master space
					_self.GetMasterPosition(out masterOrigin, out masterAxis);
					_localOrigin = (_origin - masterOrigin) * Matrix.Transpose(masterAxis);

					if(orientated == true)
					{
						_localAxis = _axis * Matrix.Transpose(masterAxis);
					}
					else
					{
						_localAxis = _axis;
					}

					_hasMaster = true;
					_isOrientated = orientated;
				}
			}
			else
			{
				if(_hasMaster == true)
				{
					_hasMaster = false;
				}
			}
		}

		public override void WriteToSnapshot(idBitMsgDelta msg)
		{
			// TODO
			/*idCQuat quat, localQuat;

			quat = current.axis.ToCQuat();
			localQuat = current.localAxis.ToCQuat();

			msg.WriteFloat( current.origin[0] );
			msg.WriteFloat( current.origin[1] );
			msg.WriteFloat( current.origin[2] );
			msg.WriteFloat( quat.x );
			msg.WriteFloat( quat.y );
			msg.WriteFloat( quat.z );
			msg.WriteDeltaFloat( current.origin[0], current.localOrigin[0] );
			msg.WriteDeltaFloat( current.origin[1], current.localOrigin[1] );
			msg.WriteDeltaFloat( current.origin[2], current.localOrigin[2] );
			msg.WriteDeltaFloat( quat.x, localQuat.x );
			msg.WriteDeltaFloat( quat.y, localQuat.y );
			msg.WriteDeltaFloat( quat.z, localQuat.z );*/
		}

		public override void ReadFromSnapshot(idBitMsgDelta msg)
		{
			// TODO	
			/*idCQuat quat, localQuat;

			current.origin[0] = msg.ReadFloat();
			current.origin[1] = msg.ReadFloat();
			current.origin[2] = msg.ReadFloat();
			quat.x = msg.ReadFloat();
			quat.y = msg.ReadFloat();
			quat.z = msg.ReadFloat();
			current.localOrigin[0] = msg.ReadDeltaFloat( current.origin[0] );
			current.localOrigin[1] = msg.ReadDeltaFloat( current.origin[1] );
			current.localOrigin[2] = msg.ReadDeltaFloat( current.origin[2] );
			localQuat.x = msg.ReadDeltaFloat( quat.x );
			localQuat.y = msg.ReadDeltaFloat( quat.y );
			localQuat.z = msg.ReadDeltaFloat( quat.z );

			current.axis = quat.ToMat3();
			current.localAxis = localQuat.ToMat3();*/
		}
		#endregion
		#endregion
	}
}