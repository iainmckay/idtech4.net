using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Collision;
using idTech4.Input;
using idTech4.Math;
using idTech4.Net;
using idTech4.Renderer;

namespace idTech4.Game.Physics
{
	/// <summary>
	/// Simulates the motion of a player through the environment. Input from the
	/// player is used to allow a certain degree of control over the motion.
	/// </summary>
	public class idPhysics_Player : idPhysics_Actor
	{
		#region Properties
		public Vector3 PlayerOrigin
		{
			get
			{
				return _current.Origin;
			}
		}
		#endregion

		#region Members
		// player physics state
		private PlayerPhysicsState _current;
		private PlayerPhysicsState _saved;

		// properties
		private float _walkSpeed;
		private float _crouchSpeed;
		private float _maxStepHeight;
		private float _maxJumpHeight;
		private int _debugLevel; // if set, diagnostic output will be printed

		// player input
		private idUserCommand _command;
		private idAngles _viewAngles;

		// run-time variables
		private int _frameMS;
		private float _frameTime;
		private float _playerSpeed;

		private Vector3 _viewForward;
		private Vector3 _viewRight;

		// walk movement
		private bool _walking;
		private bool _groundPlane;
		private TraceResult _groundTrace;
		private idMaterial _groundMaterial;

		// ladder movement
		private bool _ladder;
		private Vector3 _ladderNormal;

		// results of last evaluate
		private WaterLevel _waterLevel;
		private int _waterType;
		#endregion

		#region Constructor
		public idPhysics_Player()
			: base()
		{
			_current = new PlayerPhysicsState();
			_saved = _current;

			_command = new idUserCommand();
			_groundTrace = new TraceResult();
			_waterLevel = WaterLevel.None;
		}
		#endregion

		#region Methods
		#region Public
		public void ClearPushedVelocity()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_current.PushVelocity = Vector3.Zero;
		}
		#endregion
		#endregion

		#region idPhysics_Actor implementation
		#region Properties
		public override bool IsAtRest
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				idConsole.Warning("TODO: idPhysics_Actor.IsAtRest");

				return false;
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

				idConsole.Warning("TODO: idPhysics_Player.RestStartTime");

				return 0;
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

				idConsole.Warning("TODO: idPhysics_Player.Time");

				return base.Time;
			}
		}
		#endregion

		#region Methods
		public override bool Evaluate(int timeStep, int endTime)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.Evaluate");

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

			idConsole.Warning("TODO: idPhysics_Player.GetImpactInfo");

			return new ImpactInfo();
		}

		public override void ApplyImpulse(int id, Vector3 point, Vector3 impulse)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.ApplyImpulse");
		}

		public override void SaveState()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.SaveState");
		}

		public override void RestoreState()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.RestoreState");
		}

		public override void SetOrigin(Vector3 origin, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_current.LocalOrigin = origin;

			if(_masterEntity != null)
			{
				Vector3 masterOrigin;
				Matrix masterAxis;

				this.Self.GetMasterPosition(out masterOrigin, out masterAxis);

				_current.Origin = Vector3.Transform(masterOrigin + origin, masterAxis);
			}
			else
			{
				_current.Origin = origin;
			}

			_clipModel.Link(idR.Game.Clip, this.Self, 0, origin, _clipModel.Axis);
		}

		public override void SetAxis(Matrix axis, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.SetAxis");
		}

		public override void Translate(Vector3 translation, int id = -1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.Translate");
		}

		public override void Rotate(idRotation rotation, int id = 1)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.Rotate");
		}

		public override void SetLinearVelocity(Vector3 velocity, int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_current.Velocity = velocity;
		}

		public override Vector3 GetLinearVelocity(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _current.Velocity;
		}

		public override void SetPushed(int deltaTime)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.SetPushed");
		}

		public override Vector3 GetPushedLinearVelocity(int id = 0)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.GetPushedLinearVelocity");

			return Vector3.Zero;
		}

		public override void SetMaster(idEntity master, bool orientated = true)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.SetMaster");
		}

		public override void WriteToSnapshot(idBitMsgDelta msg)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.WriteToSnapshot");
		}

		public override void ReadFromSnapshot(idBitMsgDelta msg)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: idPhysics_Player.ReadFromSnapshot");
		}
		#endregion
		#endregion
	}

	public class PlayerPhysicsState
	{
		public Vector3 Origin;
		public Vector3 Velocity;
		public Vector3 LocalOrigin;
		public Vector3 PushVelocity;

		public float StepUp;

		public PlayerMovementType MovementType;
		public int MovementFlags;
		public int MovementTime;
	}

	public enum PlayerMovementType
	{
		/// <summary>Normal physics.</summary>
		Normal,
		/// <summary>No acceleration or turning, but free falling.</summary>
		Dead,
		/// <summary>Flying without gravity but with collision detection.</summary>
		Spectator,
		/// <summary>Stuck in place without control.</summary>
		Freeze,
		/// <summary>Flying without collision detection nor gravity.</summary>
		NoClip
	}

	public enum WaterLevel
	{
		None,
		Feet,
		Waist,
		Head
	}
}