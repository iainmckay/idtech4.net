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

namespace idTech4.Math
{
	public sealed class idExtrapolate<T>
	{
		#region Properties
		public dynamic BaseSpeed
		{
			get
			{
				return _baseSpeed;
			}
		}

		public dynamic Speed
		{
			get
			{
				return _speed;
			}
		}

		public ExtrapolationType Type
		{
			get
			{
				return _extrapolationType;
			}
		}
		#endregion

		#region Members
		private ExtrapolationType _extrapolationType;
		private float _startTime;
		private float _duration;
		private dynamic _startValue;
		private dynamic _baseSpeed;
		private dynamic _speed;
		private float _currentTime;
		private dynamic _currentValue;
		#endregion

		#region Constructor
		public idExtrapolate()
		{

		}
		#endregion

		#region Methods
		#region Public
		public T GetCurrentValue(float time)
		{
			float deltaTime, s;

			if(time == _currentTime)
			{
				return _currentValue;
			}

			_currentTime = time;

			if(time < _startTime)
			{
				return _startValue;
			}

			if(((_extrapolationType & ExtrapolationType.NoStop) == 0) && (time > (_startTime + _duration)))
			{
				time = _startTime + _duration;
			}

			switch(_extrapolationType & ~ExtrapolationType.NoStop)
			{
				case ExtrapolationType.None:
					deltaTime = (time - _startTime) * 0.001f;
					_currentValue = _startValue + deltaTime * _baseSpeed;
					break;

				case ExtrapolationType.Linear:
					deltaTime = (time - _startTime) * 0.001f;
					_currentValue = _startValue + deltaTime * (_baseSpeed + Speed);
					break;

				case ExtrapolationType.AccelerationLinear:
					if(_duration == 0)
					{
						_currentValue = _startValue;
					}
					else
					{
						deltaTime = (time - _startTime) / _duration;
						s = (0.5f * deltaTime * deltaTime) * (_duration * 0.001f);
						_currentValue = _startValue + deltaTime * _baseSpeed + s * _speed;
					}
					break;

				case ExtrapolationType.DecelerationLinear:
					if(_duration == 0)
					{
						_currentValue = _startValue;
					}
					else
					{
						deltaTime = (time - _startTime) / _duration;
						s = (deltaTime - (0.5f * deltaTime * deltaTime)) * (_duration * 0.001f);
						_currentValue = _startValue + deltaTime * _baseSpeed + s * _speed;
					}
					break;

				case ExtrapolationType.AccelerationSine:
					if(_duration == 0)
					{
						_currentValue = _startValue;
					}
					else
					{
						deltaTime = (time - _startTime) / _duration;
						s = (1.0f - idMath.Cos(deltaTime * idMath.HalfPi)) * _duration * 0.001f * idMath.Sqrt1Over2;
						_currentValue = _startValue + deltaTime * _baseSpeed + s * _speed;
					}
					break;

				case ExtrapolationType.DecelerationSine:
					if(_duration == 0)
					{
						_currentValue = _startValue;
					}
					else
					{
						deltaTime = (time - _startTime) / _duration;
						s = idMath.Sin(deltaTime * idMath.HalfPi) * _duration * 0.001f * idMath.Sqrt1Over2;
						_currentValue = _startValue + deltaTime * _baseSpeed + s * _speed;
					}
					break;
			}

			return _currentValue;
		}

		public void Init(float startTime, float duration, T startValue, T baseSpeed, T speed, ExtrapolationType extrapolationType)
		{
			_extrapolationType = extrapolationType;
			_startTime = startTime;
			_duration = duration;
			_startValue = startValue;
			_baseSpeed = baseSpeed;
			_speed = speed;

			_currentTime = -1;
			_currentValue = startValue;
		}	
		#endregion
		#endregion
	}

	[Flags]
	public enum ExtrapolationType
	{
		/// <summary>No extrapolation, covered distance = duration * 0.001 * ( baseSpeed ).</summary>
		None = 0x01,
		/// <summary>Linear extrapolation, covered distance = duration * 0.001 * ( baseSpeed + speed ).</summary>
		Linear = 0x02,
		/// <summary>Linear acceleration, covered distance = duration * 0.001 * ( baseSpeed + 0.5 * speed ).</summary>
		AccelerationLinear = 0x04,
		/// <summary>Linear deceleration, covered distance = duration * 0.001 * ( baseSpeed + 0.5 * speed ).</summary>
		DecelerationLinear = 0x08,
		/// <summary>Sinusoidal acceleration, covered distance = duration * 0.001 * ( baseSpeed + sqrt( 0.5 ) * speed ).</summary>
		AccelerationSine = 0x10,
		/// <summary>Sinusoidal deceleration, covered distance = duration * 0.001 * ( baseSpeed + sqrt( 0.5 ) * speed ).</summary>
		DecelerationSine = 0x20,
		/// <summary>Do not stop at startTime + duration.</summary>
		NoStop = 0x40
	}
}
