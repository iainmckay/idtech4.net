/*
===========================================================================

Doom 3 GPL Source Code
Copyright (C) 1999-2011 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 GPL Source Code (?Doom 3 Source Code?).  

Doom 3 Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Renderer;

namespace idTech4.Text.Decl
{
	public class idDeclParticle : idDecl
	{
		#region Properties
		public idBounds Bounds
		{
			get
			{
				return _bounds;
			}
		}

		public float DepthHack
		{
			get
			{
				return _depthHack;
			}
		}
		#endregion

		#region Members
		private float _depthHack;
		private idBounds _bounds;
		private idParticleStage[] _stages;
		#endregion

		#region Constructor
		public idDeclParticle()
			: base()
		{

		}
		#endregion

		#region Methods
		#region Private
		private idParticleStage ParseParticleStage(idLexer lexer)
		{
			idToken token;
			idParticleParameter parm;
			string tokenLower;
			
			idParticleStage stage = new idParticleStage();
			stage.Default();

			while(true)
			{
				if(lexer.HadError == true)
				{
					break;
				}
				else if((token = lexer.ReadToken()) == null)
				{
					break;
				}
				else
				{
					tokenLower = token.ToString().ToLower();

					if(tokenLower == "}")
					{
						break;
					}
					else if(tokenLower == "material")
					{
						token = lexer.ReadToken();
						stage.Material = idE.DeclManager.FindMaterial(token.ToString());
					}
					else if(tokenLower == "count")
					{
						stage.TotalParticles = lexer.ParseInt();
					}
					else if(tokenLower == "time")
					{
						stage.ParticleLife = lexer.ParseFloat();
					}
					else if(tokenLower == "cycles")
					{
						stage.Cycles = lexer.ParseFloat();
					}
					else if(tokenLower == "timeoffset")
					{
						stage.TimeOffset = lexer.ParseFloat();
					}
					else if(tokenLower == "deadtime")
					{
						stage.DeadTime = lexer.ParseFloat();
					}
					else if(tokenLower == "randomdistribution")
					{
						stage.RandomDistribution = lexer.ParseBool();
					}
					else if(tokenLower == "bunching")
					{
						stage.SpawnBunching = lexer.ParseFloat();
					}
					else if(tokenLower == "distribution")
					{
						token = lexer.ReadToken();
						tokenLower = token.ToString().ToLower();

						if(tokenLower == "rect")
						{
							stage.Distribution = ParticleDistribution.Rectangle;
						}
						else if(tokenLower == "cyclinder")
						{
							stage.Distribution = ParticleDistribution.Cyclinder;
						}
						else if(tokenLower == "sphere")
						{
							stage.Distribution = ParticleDistribution.Sphere;
						}
						else
						{
							lexer.Error("bad distribution type: {0}", token.ToString());
						}

						stage.DistributionParameters = ParseParams(lexer, stage.DistributionParameters.Length);
					}
					else if(tokenLower == "direction")
					{
						token = lexer.ReadToken();
						tokenLower = token.ToString().ToLower();

						if(tokenLower == "cone")
						{
							stage.Direction = ParticleDirection.Cone;
						}
						else if(tokenLower == "outward")
						{
							stage.Direction = ParticleDirection.Outward;
						}
						else
						{
							lexer.Error("bad direction type: {0}", token.ToString());
						}

						stage.DirectionParameters = ParseParams(lexer, stage.DirectionParameters.Length);
					}
					else if(tokenLower == "orientation")
					{
						token = lexer.ReadToken();
						tokenLower = token.ToString().ToLower();

						if(tokenLower == "view")
						{
							stage.Orientation = ParticleOrientation.View;
						}
						else if(tokenLower == "aimed")
						{
							stage.Orientation = ParticleOrientation.Aimed;
						}
						else if(tokenLower == "x")
						{
							stage.Orientation = ParticleOrientation.X;
						}
						else if(tokenLower == "y")
						{
							stage.Orientation = ParticleOrientation.Y;
						}
						else if(tokenLower == "z")
						{
							stage.Orientation = ParticleOrientation.Z;
						}
						else 
						{
							lexer.Error("bad orientation type: {0}", token.ToString());
						}

						stage.OrientationParameters = ParseParams(lexer, stage.OrientationParameters.Length);
					}
					else if(tokenLower == "custompath")
					{
						token = lexer.ReadToken();
						tokenLower = tokenLower.ToLower().ToLower();

						if(tokenLower == "standard")
						{
							stage.CustomPath = ParticleCustomPath.Standard;
						}
						else if(tokenLower == "helix")
						{
							stage.CustomPath = ParticleCustomPath.Helix;
						}
						else if(tokenLower == "flies")
						{
							stage.CustomPath = ParticleCustomPath.Flies;
						}
						else if(tokenLower == "spherical")
						{
							stage.CustomPath = ParticleCustomPath.Orbit;
						}
						else
						{
							lexer.Error("bad path type: {0}", token.ToString());
						}

						stage.CustomPathParameters = ParseParams(lexer, stage.CustomPathParameters.Length);
					}
					else if(tokenLower == "speed")
					{					
						ParseParametric(lexer, stage.Speed);
					}
					else if(tokenLower == "rotation")
					{
						ParseParametric(lexer, stage.RotationSpeed);
					}
					else if(tokenLower == "angle")
					{
						stage.InitialAngle = lexer.ParseFloat();
					}
					else if(tokenLower == "entitycolor")
					{
						stage.EntityColor = lexer.ParseBool();
					}
					else if(tokenLower == "size")
					{
						ParseParametric(lexer, stage.Size);
					}
					else if(tokenLower == "aspect")
					{
						ParseParametric(lexer, stage.Aspect);
					}
					else if(tokenLower == "fadein")
					{
						stage.FadeInFraction = lexer.ParseFloat();
					}
					else if(tokenLower == "fadeout")
					{
						stage.FadeOutFraction = lexer.ParseFloat();
					}
					else if(tokenLower == "fadeindex")
					{
						stage.FadeIndexFraction = lexer.ParseFloat();
					}
					else if(tokenLower == "color")
					{
						stage.Color = new Vector4(lexer.ParseFloat(), lexer.ParseFloat(), lexer.ParseFloat(), lexer.ParseFloat());
					}
					else if(tokenLower == "fadecolor")
					{
						stage.FadeColor = new Vector4(lexer.ParseFloat(), lexer.ParseFloat(), lexer.ParseFloat(), lexer.ParseFloat());
					}
					else if(tokenLower == "offset")
					{
						stage.Offset = new Vector3(lexer.ParseFloat(), lexer.ParseFloat(), lexer.ParseFloat());
					}
					else if(tokenLower == "animationframes")
					{
						stage.AnimationFrames = lexer.ParseInt();
					}
					else if(tokenLower == "animationrate")
					{
						stage.AnimationRate = lexer.ParseFloat();
					}
					else if(tokenLower == "boundsexpansion")
					{
						stage.BoundsExpansion = lexer.ParseFloat();
					}
					else if(tokenLower == "gravity")
					{
						token = lexer.ReadToken();
						tokenLower = token.ToString().ToLower();

						if(tokenLower == "world")
						{
							stage.WorldGravity = true;
						}
						else
						{
							lexer.UnreadToken = token;
						}

						stage.Gravity = lexer.ParseFloat();
					}
					else
					{
						lexer.Error("unknown token {0}", token.ToString());
					}
				}
			}

			// derive values.
			stage.CycleTime = (int) (stage.ParticleLife + stage.DeadTime) * 1000;

			return stage;
		}

		private void ParseParametric(idLexer lexer, idParticleParameter parm)
		{
			idToken token;

			if((token = lexer.ReadToken()) == null)
			{
				lexer.Error("not enough parameters");
				return;
			}

			if(token.IsNumeric == true)
			{
				// can have a to + 2nd parm.
				float tmp;
				float.TryParse(token.ToString(), out tmp);

				parm.From = tmp;
				parm.To = tmp;

				if((token = lexer.ReadToken()) != null)
				{
					if(token.ToString().ToLower() == "to")
					{
						if((token = lexer.ReadToken()) == null)
						{
							lexer.Error("missing second parameter");
							return;
						}

						float.TryParse(token.ToString(), out tmp);
						parm.To = tmp;
					}
					else
					{
						lexer.UnreadToken = token;
					}
				}
			}
			else
			{
				parm.Table = (idDeclTable) idE.DeclManager.FindType(DeclType.Table, token.ToString(), false);
			}
		}

		/// <summary>
		/// Parses a variable length list of parms on one line.
		/// </summary>
		/// <param name="lexer"></param>
		/// <param name="parms"></param>
		/// <param name="maxParms"></param>
		private float[] ParseParams(idLexer lexer, int maxParms)
		{
			idToken token;
			List<float> parms = new List<float>();
			int count = 0;
			float tmp;

			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}
				else if(count == maxParms)
				{
					lexer.Error("too many parms on line");
					break;
				}
				else
				{
					token.StripQuotes();
					float.TryParse(token.ToString(), out tmp);
					
					parms.Add(tmp);
					count++;
				}
			}

			return parms.ToArray();
		}
		#endregion
		#endregion

		#region idDecl implementation
		#region Properties
		public override string DefaultDefinition
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return "{\n\t{\n\tmaterial\t_default\n\t\tcount\t20\n\n\ttime\t\t1.0\n\t}\n}";
			}
		}

		public override int MemoryUsage
		{
			get
			{
				idConsole.Warning("TODO: idDeclParticle.MemoryUsage");
				return 0;
			}
		}
		#endregion

		#region Methods
		#region Public
		public override bool Parse(string text)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idToken token;
			string tokenLower;

			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.LoadMemory(text, this.FileName, this.LineNumber);
			lexer.SkipUntilString("{");

			List<idParticleStage> stages = new List<idParticleStage>();

			_depthHack = 0.0f;

			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenLower = token.ToString().ToLower();

				if(tokenLower == "}")
				{
					break;
				}
				else if(tokenLower == "{")
				{
					idParticleStage stage = ParseParticleStage(lexer);

					if(stage == null)
					{
						lexer.Warning("Particle stage parse failed");
						MakeDefault();

						return false;
					}

					stages.Add(stage);
				}
				else if(tokenLower == "depthhack")
				{
					_depthHack = lexer.ParseFloat();
				}
				else
				{
					lexer.Warning("bad token {0}", token.ToString());
					MakeDefault();

					return false;
				}
			}

			_stages = stages.ToArray();

			//
			// calculate the bounds
			//
			_bounds.Clear();

			for(int i = 0; i < _stages.Length; i++)
			{
				idConsole.Warning("TODO: GetStageBounds");
				// TODO: GetStageBounds(stages[i]);
				_bounds += _stages[i].Bounds;
			}

			if(_bounds.Volume <= 0.1f)
			{
				_bounds = idBounds.Expand(idBounds.Zero, 8.0f);
			}

			return true;
		}
		#endregion

		#region Protected
		protected override void ClearData()
		{
			base.ClearData();

			_stages = null;
		}
		#endregion
		#endregion
		#endregion
	}

	public sealed class idParticleStage
	{
		#region Properties
		public idMaterial Material
		{
			get
			{
				return _material;
			}
			set
			{
				_material = value;
			}
		}

		/// <summary>
		/// Gets or sets the total number of particles, there may some be invisible at a given time.
		/// </summary>
		public int TotalParticles
		{
			get
			{
				return _totalParticles;
			}
			set
			{
				_totalParticles = value;
			}
		}

		/// <summary>
		/// Allows things to oneShot ( 1 cycle ) or run for a set number of cycles
		/// on a per stage basis.
		/// </summary>
		public float Cycles
		{
			get
			{
				return _cycles;
			}
			set
			{
				_cycles = value;
			}
		}

		/// <summary>
		/// 0.0 = all come out at first instant, 1.0 = evenly spaced over cycle time.
		/// </summary>
		public float SpawnBunching
		{
			get
			{
				return _spawnBunching;
			}
			set
			{
				_spawnBunching = value;
			}
		}

		/// <summary>
		/// Gets or sets the total seconds of life for each particle.
		/// </summary>
		public float ParticleLife
		{
			get
			{
				return _particleLife;
			}
			set
			{
				_particleLife = value;
			}
		}

		/// <summary>
		/// Gets or sets the time offset from system start for the first particle to spawn.
		/// </summary>
		public float TimeOffset
		{
			get
			{
				return _timeOffset;
			}
			set
			{
				_timeOffset = value;
			}
		}

		/// <summary>
		/// Gets or sets the time after particleLife before respawning.
		/// </summary>
		public float DeadTime
		{
			get
			{
				return _deadTime;
			}
			set
			{
				_deadTime = value;
			}
		}

		public ParticleDistribution Distribution
		{
			get
			{
				return _distribution;
			}
			set
			{
				_distribution = value;
			}
		}

		public float[] DistributionParameters
		{
			get
			{
				return _distributionParms;
			}
			set
			{
				_distributionParms = value;
			}
		}

		public ParticleDirection Direction
		{
			get
			{
				return _direction;
			}
			set
			{
				_direction = value;
			}
		}

		public float[] DirectionParameters
		{
			get
			{
				return _directionParms;
			}
			set
			{
				_directionParms = value;
			}
		}

		public ParticleCustomPath CustomPath
		{
			get
			{
				return _customPath;
			}
			set
			{
				_customPath = value;
			}
		}

		public float[] CustomPathParameters
		{
			get
			{
				return _customPathParms;
			}
			set
			{
				_customPathParms = value;
			}
		}

		public ParticleOrientation Orientation
		{
			get
			{
				return _orientation;
			}
			set
			{
				_orientation = value;
			}
		}

		public float[] OrientationParameters
		{
			get
			{
				return _orientationParms;
			}
			set
			{
				_orientationParms = value;
			}
		}

		/// <summary>
		/// If > 1, subdivide the texture S axis into frames and crossfade.
		/// </summary>
		public int AnimationFrames
		{
			get
			{
				return _animationFrames;
			}
			set
			{
				_animationFrames = value; 
			}
		}

		public Vector4 Color
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not to force color from render entity (fadeColor is still valid).
		/// </summary>
		public bool EntityColor
		{
			get
			{
				return _entityColor;
			}
			set
			{
				_entityColor = value;
			}
		}

		/// <summary>
		/// Either 0 0 0 0 for additive, or 1 1 1 0 for blended materials.
		/// </summary>
		public Vector4 FadeColor
		{
			get
			{
				return _fadeColor;
			}
			set
			{
				_fadeColor = value;
			}
		}

		/// <summary>
		/// Gets or sets the rate to play the animation in frames per second.
		/// </summary>
		public float AnimationRate
		{
			get
			{
				return _animationRate;
			}
			set
			{
				_animationRate = value;
			}
		}

		/// <summary>
		/// Gets or sets the offset from origin to spawn all particles, also applies to customPath.
		/// </summary>
		public Vector3 Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		/// <summary>
		/// Gets or sets the gravity to apply; can be negative to float up.
		/// </summary>
		public float Gravity
		{
			get
			{
				return _gravity;
			}
			set
			{
				_gravity = value;
			}
		}

		/// <summary>
		/// Gets or sets the time for the next particle ( particleLife + deadTime ) in msec.
		/// </summary>
		public int CycleTime
		{
			get
			{
				return _cycleTime;
			}
			set
			{
				_cycleTime = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not to apply gravity in world space.
		/// </summary>
		public bool WorldGravity
		{
			get
			{
				return _worldGravity;
			}
			set
			{
				_worldGravity = value;
			}
		}

		public idBounds Bounds
		{
			get
			{
				return _bounds;
			}
			set
			{
				_bounds = value;
			}
		}

		/// <summary>
		/// User tweak to fix poorly calculated bounds.
		/// </summary>
		public float BoundsExpansion
		{
			get
			{
				return _boundsExpansion;
			}
			set
			{
				_boundsExpansion = value;
			}
		}

		/// <summary>
		/// In 0.0 to 1.0 range, causes later index smokes to be more faded.
		/// </summary>
		public float FadeIndexFraction
		{
			get
			{
				return _fadeIndexFraction;
			}
			set
			{
				_fadeIndexFraction = value;
			}
		}
		
		/// <summary>
		/// Gets or sets the fade out step in 0.0 to 1.0 range.
		/// </summary>
		public float FadeOutFraction
		{
			get
			{
				return _fadeOutFraction;
			}
			set
			{
				_fadeOutFraction = value;
			}
		}

		/// <summary>
		/// Gets or sets the fade in step in 0.0 to 1.0 range.
		/// </summary>
		public float FadeInFraction
		{
			get
			{
				return _fadeInFraction;
			}
			set
			{
				_fadeInFraction = value;
			}
		}
		
		/// <summary>
		/// Gets or sets, in degrees, the random angle is used if zero (default).
		/// </summary>
		public float InitialAngle
		{
			get
			{
				return _initialAngle;
			}
			set
			{
				_initialAngle = value;
			}
		}

		/// <summary>
		/// Gets or sets whether or not to randomly orient the quad on emission (defaults to true).
		/// </summary>
		public bool RandomDistribution
		{
			get
			{
				return _randomDistribution;
			}
			set
			{
				_randomDistribution = value;
			}
		}

		/// <summary>
		/// Greater than 1 makes the T axis longer.
		/// </summary>
		public idParticleParameter Aspect
		{
			get
			{
				return _aspect;
			}
			set
			{
				_aspect = value;
			}
		}

		public idParticleParameter Speed
		{
			get
			{
				return _speed;
			}
			set
			{
				_speed = value;
			}
		}

		/// <summary>
		/// Half the particles will have negative rotation speeds.
		/// </summary>
		public idParticleParameter RotationSpeed
		{
			get
			{
				return _rotationSpeed;
			}
			set
			{
				_rotationSpeed = value;
			}
		}

		public idParticleParameter Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
			}
		}
		#endregion

		#region Members
		private idMaterial _material;
		private int _totalParticles;						// total number of particles, although some may be invisible at a given time.
		private float _cycles;								// allows things to oneShot ( 1 cycle ) or run for a set number of cycles on a per stage basis.

		private int _cycleTime;								// ( particleLife + deadTime ) in msec.

		private float _spawnBunching;						// 0.0 = all come out at first instant, 1.0 = evenly spaced over cycle time
		private float _particleLife;						// total seconds of life for each particle
		private float _timeOffset;							// time offset from system start for the first particle to spawn
		private float _deadTime;							// time after particleLife before respawning

		private ParticleDistribution _distribution;
		private float[] _distributionParms;

		private ParticleDirection _direction;
		private float[] _directionParms;

		private idParticleParameter _speed;
		private float _gravity;								// can be negative to float up
		private bool _worldGravity;							// apply gravity in world space
		private bool _randomDistribution;					// randomly orient the quad on emission ( defaults to true ) 
		private bool _entityColor;							// force color from render entity ( fadeColor is still valid )

		//------------------------------					// custom path will completely replace the standard path calculations

		private ParticleCustomPath _customPath;				// use custom C code routines for determining the origin
		private float[] _customPathParms;

		//--------------------------------

		private Vector3 _offset;							// offset from origin to spawn all particles, also applies to customPath

		private int _animationFrames;						// if > 1, subdivide the texture S axis into frames and crossfade
		private float _animationRate;						// frames per second

		private float _initialAngle;						// in degrees, random angle is used if zero ( default ) 
		private idParticleParameter _rotationSpeed;			// half the particles will have negative rotation speeds

		private ParticleOrientation _orientation;			// view, aimed, or axis fixed
		private float[] _orientationParms;

		private idParticleParameter _size;
		private idParticleParameter _aspect;					// greater than 1 makes the T axis longer

		private Vector4 _color;
		private Vector4 _fadeColor;							// either 0 0 0 0 for additive, or 1 1 1 0 for blended materials
		private float _fadeInFraction;						// in 0.0 to 1.0 range
		private float _fadeOutFraction;						// in 0.0 to 1.0 range
		private float _fadeIndexFraction;					// in 0.0 to 1.0 range, causes later index smokes to be more faded 

		private bool _hidden;								// for editor use
		//-----------------------------------

		private float _boundsExpansion;						// user tweak to fix poorly calculated bounds.
		private idBounds _bounds;							// derived.
		#endregion

		#region Constructor
		public idParticleStage()
		{
			_distribution = ParticleDistribution.Rectangle;
			_distributionParms = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

			_direction = ParticleDirection.Cone;
			_directionParms = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

			_customPath = ParticleCustomPath.Standard;
			_customPathParms = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

			_orientation = ParticleOrientation.View;
			_orientationParms = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

			_randomDistribution = true;
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Sets the stage to a default state.
		/// </summary>
		public void Default()
		{
			_material = idE.DeclManager.FindMaterial("_default");
			_totalParticles = 100;
			_spawnBunching = 1.0f;
			_particleLife = 1.5f;
			_timeOffset = 0.0f;
			_deadTime = 0.0f;

			_distribution = ParticleDistribution.Rectangle;
			_distributionParms[0] = 8.0f;
			_distributionParms[1] = 8.0f;
			_distributionParms[2] = 8.0f;
			_distributionParms[3] = 0.0f;

			_direction = ParticleDirection.Cone;
			_directionParms[0] = 90.0f;
			_directionParms[1] = 0.0f;
			_directionParms[2] = 0.0f;
			_directionParms[3] = 0.0f;

			_orientation = ParticleOrientation.View;
			_orientationParms[0] = 0.0f;
			_orientationParms[1] = 0.0f;
			_orientationParms[2] = 0.0f;
			_orientationParms[3] = 0.0f;

			_customPath = ParticleCustomPath.Standard;
			_customPathParms[0] = 0.0f;
			_customPathParms[1] = 0.0f;
			_customPathParms[2] = 0.0f;
			_customPathParms[3] = 0.0f;
			_customPathParms[4] = 0.0f;
			_customPathParms[5] = 0.0f;
			_customPathParms[6] = 0.0f;
			_customPathParms[7] = 0.0f;

			_gravity = 1.0f;
			_worldGravity = false;

			_offset = Vector3.Zero;

			_animationFrames = 0;
			_animationRate = 0.0f;
			_initialAngle = 0.0f;

			_speed = new idParticleParameter(150.0f, 150.0f, null);
			_rotationSpeed = new idParticleParameter(0.0f, 0.0f, null);
			_size = new idParticleParameter(4.0f, 4.0f, null);
			_aspect = new idParticleParameter(1.0f, 1.0f, null);

			_color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			_fadeColor = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
			_fadeInFraction = 0.1f;
			_fadeOutFraction = 0.25f;
			_fadeIndexFraction = 0.0f;
			_boundsExpansion = 0.0f;
			_randomDistribution = true;
			_entityColor = false;
			_cycleTime = (int) (_particleLife + _deadTime) * 1000;
		}
		#endregion
		#endregion
	}

	public class idParticleParameter
	{
		#region Properties
		public idDeclTable Table
		{
			get
			{
				return _table;
			}
			set
			{
				_table = value;
			}
		}

		public float From
		{
			get
			{
				return _from;
			}
			set
			{
				_from = value;
			}
		}

		public float To
		{
			get
			{
				return _to;
			}
			set
			{
				_to = value;
			}
		}
		#endregion

		#region Members
		private idDeclTable _table;
		private float _from;
		private float _to;
		#endregion

		#region Constructor
		public idParticleParameter(float from, float to, idDeclTable table)
		{
			_from = from;
			_to = to;
			_table = table;
		}
		#endregion
	}

	public enum ParticleDistribution
	{
		/// <summary>( sizeX sizeY sizeZ )</summary>
		Rectangle,
		/// <summary>( sizeX sizeY sizeZ )</summary>
		Cyclinder,
		/// <summary>( sizeX sizeY sizeZ ringFraction )</summary>
		/// <remarks>
		/// A ringFraction of zero allows the entire sphere, 0.9 would only
		/// allow the outer 10% of the sphere.
		/// </remarks>
		Sphere
	}

	public enum ParticleDirection
	{
		/// <summary>parm0 is the solid cone angle.</summary>
		Cone,
		/// <summary>Direction is relative to the offset from the origin, parm0 is an upward bias.</summary>
		Outward
	}

	public enum ParticleCustomPath
	{
		Standard,
		Helix,
		/// <summary>( sizeX sizeY sizeZ radialSpeed climbSpeed )</summary>
		Flies,
		Orbit,
		Drip
	}

	public enum ParticleOrientation
	{
		View,
		/// <summary>Angle and aspect are disregarded.</summary>
		Aimed,
		X,
		Y,
		Z
	}
}