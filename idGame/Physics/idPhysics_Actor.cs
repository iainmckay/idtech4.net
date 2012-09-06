using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Collision;
using idTech4.Renderer;

namespace idTech4.Game.Physics
{
	/// <summary>
	/// An actor typically uses one collision model which is aligned with the gravity
	/// direction. The collision model is usually a simple box with the origin at the
	/// bottom center.
	/// </summary>
	public class idPhysics_Actor : idPhysics_Base
	{
		#region Members
		private idClipModel _clipModel; // clip model used for collision detection
		private Matrix _clipModelAxis; // axis of clip model aligned with gravity direction

		// derived properties
		private float _mass;
		private float _inverseMass;

		// master
		private idEntity _masterEntity;
		private float _masterYaw;
		private float _masterDeltaYaw;

		// results of last evaluate
		private idEntity _groundEntity;
		#endregion

		#region Constructor
		public idPhysics_Actor()
			: base()
		{
			SetClipModelAxis();

			_mass = 100.0f;
			_inverseMass = 1.0f / _mass;
		}

		~idPhysics_Actor()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		public void SetClipModelAxis()
		{
			// align clip model to gravity direction
			if((_gravityNormal.Z == -1.0f) || (_gravityNormal == Vector3.Zero))
			{
				_clipModelAxis = Matrix.Identity;
			}
			else
			{
				Vector3 v1 = new Vector3(_clipModelAxis.M11, _clipModelAxis.M12, _clipModelAxis.M13);
				Vector3 v2 = new Vector3(_clipModelAxis.M21, _clipModelAxis.M22, _clipModelAxis.M23);
				Vector3 v3 = -_gravityNormal;

				idHelper.NormalVectors(v3, ref v1, ref v2);

				_clipModelAxis.M31 = v3.X;
				_clipModelAxis.M32 = v3.Y;
				_clipModelAxis.M33 = v3.Z;
				_clipModelAxis.M34 = 1;

				_clipModelAxis.M21 = v2.X;
				_clipModelAxis.M22 = v2.Y;
				_clipModelAxis.M23 = v2.Z;
				_clipModelAxis.M24 = 1;

				_clipModelAxis.M11 = -v1.X;
				_clipModelAxis.M12 = -v1.Y;
				_clipModelAxis.M13 = -v1.Z;
				_clipModelAxis.M14 = 1;
			}

			if(_clipModel != null)
			{
				_clipModel.Link(idR.Game.Clip, _self, 0, _clipModel.Origin, _clipModelAxis);
			}
		}
		#endregion
		#endregion

		#region idPhysics implementation
		#region Properties
		public override int ClipModelCount
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return 1;
			}
		}

		public override bool IsPushable
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return (_masterEntity == null);
			}
		}
		#endregion

		#region Methods
		public override void SetClipModel(idClipModel model, float density, int id = 0, bool disposeOld = true)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(model.IsTraceModel == false)
			{
				throw new ArgumentException("model should be a trace model");
			}

			if(density <= 0.0f)
			{
				throw new ArgumentException("density must be valid");
			}

			if((_clipModel != null) && (_clipModel != model) && (disposeOld == true))
			{
				_clipModel.Dispose();
			}

			_clipModel = model;
			_clipModel.Link(idR.Game.Clip, this.Self, 0, _clipModel.Origin, _clipModelAxis);
		}

		public override idClipModel GetClipModel(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _clipModel;
		}

		public override void SetMass(float mass, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(mass <= 0.0f)
			{
				throw new ArgumentException("mass must be greater than 0");
			}

			_mass = mass;
			_inverseMass = 1.0f / mass;
		}

		public override float GetMass(int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _mass;
		}

		public override void SetContents(ContentFlags contents, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_clipModel.Contents = contents;
		}

		public override ContentFlags GetContents(int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _clipModel.Contents;
		}

		public override idBounds GetBounds(int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _clipModel.Bounds;
		}

		public override idBounds GetAbsoluteBounds(int id = -1)
		{
			return _clipModel.AbsoluteBounds;
		}

		public override Vector3 GetOrigin(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _clipModel.Origin;
		}

		public override Matrix GetAxis(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _clipModel.Axis;
		}

		public override Vector3 Gravity
		{
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				if(value != _gravityVector)
				{
					base.Gravity = value;
					SetClipModelAxis();
				}
			}
		}

		public override TraceResult ClipTranslation(Vector3 translation, idClipModel model)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(model != null)
			{
				idConsole.Warning("TODO: return idR.Game.Clip.TranslationModel(_clipModel.Origin, _clipModel.Origin + translation, _clipModel, _clipModel.Axis, _clipMask, model, model.Origin, model.Axis);");
			}
			else
			{
				idConsole.Warning("TODO: return idR.Game.Clip.Translation(_clipModel.Origin, _clipModel.Origin + translation, _clipModel, _clipModel.Axis, _clipMask, _self);");
			}

			return new TraceResult();
		}

		public override TraceResult ClipRotation(idRotation rotation, idClipModel model)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(model != null)
			{
				idConsole.Warning("TODO: return idR.Game.Clip.RotationModel(_clipModel.Origin, rotation, _clipModel, _clipModel.Axis, _clipMask, model, model.Origin, model.Axis);");
			}
			else
			{
				idConsole.Warning("TODO: return idR.Game.Clip.Rotation(_clipModel.Origin, rotation, _clipModel, _clipModel.Axis, _clipMask, _self);");
			}

			return new TraceResult();
		}

		public override ContentFlags ClipContents(idClipModel model)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(model != null)
			{
				return idR.Game.Clip.ContentsModel(_clipModel.Origin, _clipModel, _clipModel.Axis, ContentFlags.None, model, model.Origin, model.Axis);
			}
			else
			{
				return idR.Game.Clip.Contents(_clipModel.Origin, _clipModel, _clipModel.Axis, ContentFlags.None, null);
			}
		}

		public override void DisableClip()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_clipModel.Enabled = false;
		}

		public override void EnableClip()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_clipModel.Enabled = true;
		}

		public override void UnlinkClip()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_clipModel.Unlink();
		}

		public override void LinkClip()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_clipModel.Link(idR.Game.Clip, _self, 0, _clipModel.Origin, _clipModel.Axis);
		}

		public override bool EvaluateContacts()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// get all the ground contacts
			ClearContacts();
			AddGroundContacts(_clipModel);
			AddContactEntitiesForContacts();

			return (_contacts.Count != 0);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if(_clipModel != null)
			{
				_clipModel.Dispose();
				_clipModel = null;
			}
		}
		#endregion
		#endregion
	}
}