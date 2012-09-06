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
	/// Physics abstract class.
	/// </summary>
	/// <remarks>
	/// A physics object is a tool to manipulate the position and orientation of
	/// an entity. The physics object is a container for idClipModels used for
	/// collision detection. The physics deals with moving these collision models
	/// through the world according to the laws of physics or other rules.
	/// <para/>
	/// The mass of a clip model is the volume of the clip model times the density.
	/// An arbitrary mass can however be set for specific clip models or the
	/// whole physics object. The contents of a clip model is a set of bit flags
	/// that define the contents. The clip mask defines the contents a clip model
	/// collides with.
	/// <para/>
	/// The linear velocity of a physics object is a vector that defines the
	/// translation of the center of mass in units per second. The angular velocity
	/// of a physics object is a vector that passes through the center of mass. The
	/// direction of this vector defines the axis of rotation and the magnitude
	/// defines the rate of rotation about the axis in radians per second.
	/// The gravity is the change in velocity per second due to gravitational force.
	/// <para/>
	/// Entities update their visual position and orientation from the physics
	/// using GetOrigin() and GetAxis(). Direct origin and axis changes of
	/// entities should go through the physics. In other words the physics origin
	/// and axis are updated first and the entity updates it's visual position
	/// from the physics.
	/// </remarks>
	public abstract class idPhysics : IDisposable
	{
		#region Properties
		/// <summary>
		/// Mvement end time in msec for reached events at the end of predefined motion.
		/// </summary>
		public abstract int AngularEndTime
		{
			get;
		}

		public abstract idEntity BlockingEntity
		{
			get;
		}

		/// <summary>
		/// Get blocking info, returns NULL if the object is not blocked.
		/// </summary>
		public abstract TraceResult BlockingInfo
		{
			get;
		}

		public abstract int ClipModelCount
		{
			get;
		}

		public abstract int ContactCount
		{
			get;
		}

		public abstract Vector3 Gravity
		{
			get;
			set;
		}

		public abstract Vector3 GravityNormal
		{
			get;
		}

		public abstract bool HasGroundContacts
		{
			get;
		}
		
		public abstract bool IsAtRest
		{
			get;
		}

		public abstract bool IsPushable
		{
			get;
		}

		/// <summary>
		/// Mvement end time in msec for reached events at the end of predefined motion.
		/// </summary>
		public abstract int LinearEndTime
		{
			get;
		}

		public abstract int RestStartTime
		{
			get;
		}


		public abstract idEntity Self
		{
			get;
			set;
		}

		/// <summary>
		/// Get the last physics update time.
		/// </summary>
		public abstract int Time
		{
			get;
		}
		#endregion

		#region Constructor
		public idPhysics()
		{

		}

		~idPhysics()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region General
		public virtual void Save(object saveFile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public virtual void Restore(object saveFile)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public static int SnapTimeToPhysicsFrame(int t)
		{
			int s = t + idR.UserCommandRate - 1;

			return (s - s % idR.UserCommandRate);
		}
		#endregion

		public abstract void SetClipModel(idClipModel model, float density, int id = 0, bool disposeOld = true);

		public virtual void SetClipBox(idBounds bounds, float density)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			SetClipModel(new idClipModel(new idTraceModel(bounds)), density);
		}

		public abstract idClipModel GetClipModel(int id = 0);
		
		/// <summary>
		/// Set the mass of a specific clip model or the whole physics object.
		/// </summary>
		/// <param name="mass"></param>
		/// <param name="id"></param>
		public abstract void SetMass(float mass, int id = -1);
		
		/// <summary>
		/// Get the mass of a specific clip model or the whole physics object.
		/// </summary>
		/// <param name="id"></param>
		public abstract float GetMass(int id = -1);
		
		/// <summary>
		/// Set the contents of a specific clip model or the whole physics object.
		/// </summary>
		/// <param name="contents"></param>
		/// <param name="id"></param>
		public abstract void SetContents(ContentFlags contents, int id = -1);

		/// <summary>
		/// Get the contents of a specific clip model or the whole physics object.
		/// </summary>
		/// <param name="id"></param>
		public abstract ContentFlags GetContents(int id = -1);

		/// <summary>
		/// Set the contents a specific clip model or the whole physics object collides with.
		/// </summary>
		/// <param name="mask"></param>
		/// <param name="id"></param>
		public abstract void SetClipMask(ContentFlags mask, int id = -1);

		/// <summary>
		/// Get the contents a specific clip model or the whole physics object collides with.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public abstract ContentFlags GetClipMask(int id = -1);
		
		/// <summary>
		/// Get the bounds of a specific clip model or the whole physics object.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public abstract idBounds GetBounds(int id = -1);

		/// <summary>
		/// Get the bounds of a specific clip model or the whole physics object.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public abstract idBounds GetAbsoluteBounds(int id = -1);

		/// <summary>
		/// Evaluate the physics with the given time step, returns true if the object moved.
		/// </summary>
		/// <param name="timeStep"></param>
		/// <param name="endTime"></param>
		/// <returns></returns>
		public abstract bool Evaluate(int timeStep, int endTime);

		/// <summary>
		/// Update the time without moving.
		/// </summary>
		/// <param name="endTime"></param>
		public abstract void UpdateTime(int endTime);

		public abstract ImpactInfo GetImpactInfo(int id, Vector3 point);

		public abstract void ApplyImpulse(int id, Vector3 point, Vector3 impulse);
		public abstract void AddForce(int id, Vector3 point, Vector3 force);
		public abstract void Activate();
		public abstract void PutToRest();

		public abstract void SaveState();
		public abstract void RestoreState();
		
		/// <summary>
		/// Set the position in master space or world space if no master set.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="id"></param>
		public abstract void SetOrigin(Vector3 origin, int id = -1);

		/// <summary>
		/// Set the orientation in master space or world space if no master set.
		/// </summary>
		/// <param name="axis"></param>
		/// <param name="id"></param>
		public abstract void SetAxis(Matrix axis, int id = -1);
		
		/// <summary>
		/// Translate the physics object in world space.
		/// </summary>
		/// <param name="translation"></param>
		/// <param name="id"></param>
		public abstract void Translate(Vector3 translation, int id = -1);

		/// <summary>
		/// Rotate the physics object in world space.
		/// </summary>
		/// <param name="rotation"></param>
		/// <param name="id"></param>
		public abstract void Rotate(idRotation rotation, int id = 1);

		public abstract Vector3 GetOrigin(int id = 0);		
		public abstract Matrix GetAxis(int id = 0);
		
		public abstract void SetLinearVelocity(Vector3 velocity, int id = 0);
		public abstract void SetAngularVelocity(Vector3 velocity, int id = 0);

		public abstract Vector3 GetLinearVelocity(int id = 0);
		public abstract Vector3 GetAngularVelocity(int id = 0);

		public abstract TraceResult ClipTranslation(Vector3 translation, idClipModel model);
		public abstract TraceResult ClipRotation(idRotation rotation, idClipModel model);
		public abstract ContentFlags ClipContents(idClipModel model);

		/// <summary>
		/// Disable the clip models contained by this physics object.
		/// </summary>
		public abstract void DisableClip();
		/// <summary>
		/// Enable the clip models contained by this physics object.
		/// </summary>
		public abstract void EnableClip();

		/// <summary>
		/// Unlink the clip models contained by this physics object.
		/// </summary>
		public abstract void UnlinkClip();

		/// <summary>
		/// Link the clip models contained by this physics object.
		/// </summary>
		public abstract void LinkClip();

		public abstract bool EvaluateContacts();

		public abstract ContactInfo GetContact(int index);

		public abstract void ClearContacts();
		public abstract void AddContactEntity(idEntity entity);
		public abstract void RemoveContactEntity(idEntity entity);

		public abstract bool IsGroundEntity(int index);
		public abstract bool IsGroundClipModel(int index, int id);

		public abstract void SetMaster(idEntity master, bool orientated = true);
		public abstract void SetPushed(int deltaTime);

		public abstract Vector3 GetPushedLinearVelocity(int id = 0);
		public abstract Vector3 GetPushedAngularVelocity(int id = 0);

		public abstract void WriteToSnapshot(idBitMsgDelta msg);
		public abstract void ReadFromSnapshot(idBitMsgDelta msg);
		#endregion

		#region IDisposable implementation
		#region Properties
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}
		#endregion

		#region Members
		private bool _disposed;
		#endregion

		#region Methods
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

			_disposed = true;
		}
		#endregion
		#endregion
	}

	public struct ImpactInfo
	{
		public float InverseMass;
		public Matrix InverseInertiaTensor;

		/// <summary>
		/// Impact position relative to the center of mass.
		/// </summary>
		public Vector3 Position;

		/// <summary>
		/// Velocity at the impact position.
		/// </summary>
		public Vector3 Velocity;
	}
}