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

using idTech4.UI;

namespace idTech4.Math
{
	/// <summary>
	/// Continuous interpolation with linear acceleration and deceleration phase.
	/// The velocity is continuous but the acceleration is not.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class idInterpolateAccelerationDecelerationLinear<T>
	{
		#region Properties
		public float EndTime
		{
			get
			{
				return (_startTime + _accelTime + _linearTime + _decelTime);
			}
		}

		public T EndValue
		{
			get
			{
				return _endValue;
			}
		}

		public float StartTime
		{
			get
			{
				return _startTime;
			}
		}

		public T StartValue
		{
			get
			{
				return _startValue;
			}
		}
		#endregion

		#region Member
		private float _startTime;
		private float _accelTime;
		private float _linearTime;
		private float _decelTime;
		private dynamic _startValue;
		private dynamic _endValue;
		private idExtrapolate<T> _extrapolate = new idExtrapolate<T>();
		#endregion

		#region Constructor
		public idInterpolateAccelerationDecelerationLinear()
		{

		}
		#endregion

		#region Methods
		#region Public
		public T GetCurrentValue(float time)
		{
			SetPhase(time);

			return _extrapolate.GetCurrentValue(time);
		}
		
		public void Init(float startTime, float accelTime, float decelTime, float duration, dynamic startValue, dynamic endValue)
		{
			T speed;

			_startTime = startTime;
			_accelTime = accelTime;
			_decelTime = decelTime;
			_startValue = startValue;
			_endValue = endValue;

			if(duration <= 0.0f)
			{
				return;
			}

			if((_accelTime + _decelTime) > duration)
			{
				_accelTime = (_accelTime * duration) / (_accelTime + _decelTime);
				_decelTime = duration - _accelTime;
			}

			_linearTime = duration - _accelTime - _decelTime;
			speed = (_endValue - _startValue) * (1000.0f / (_linearTime + (_accelTime + _decelTime) * 0.5f));

			if(_accelTime > 0)
			{
				_extrapolate.Init(_startTime, _accelTime, _startValue, (_startValue - _startValue), speed, ExtrapolationType.AccelerationLinear);
			}
			else if(_linearTime > 0)
			{
				_extrapolate.Init(_startTime, _linearTime, _startValue, (_startValue - _startValue), speed, ExtrapolationType.Linear);
			}
			else
			{
				_extrapolate.Init(_startTime, _decelTime, _startValue, (_startValue - _startValue), speed, ExtrapolationType.DecelerationLinear);
			}
		}

		public bool IsDone(float time)
		{
			return (_startTime >= (_startTime + _accelTime + _linearTime + _decelTime));
		}
		#endregion

		#region Private
		private void SetPhase(float time)
		{
			float deltaTime = time - _startTime;

			if(deltaTime < _accelTime)
			{
				if(_extrapolate.Type != ExtrapolationType.AccelerationLinear)
				{
					_extrapolate.Init(_startTime, _accelTime, _startValue, _extrapolate.BaseSpeed, _extrapolate.Speed, ExtrapolationType.AccelerationLinear);
				}
			}
			else if(deltaTime < (_accelTime + _linearTime))
			{
				if(_extrapolate.Type != ExtrapolationType.Linear)
				{
					_extrapolate.Init(_startTime + _accelTime, _linearTime, _startValue + _extrapolate.Speed * (_accelTime * 0.001f * 0.5f), _extrapolate.BaseSpeed, _extrapolate.Speed, ExtrapolationType.Linear);
				}
			}
			else
			{
				if(_extrapolate.Type != ExtrapolationType.DecelerationLinear)
				{
					_extrapolate.Init(_startTime + _accelTime + _linearTime, _decelTime, _endValue - (_extrapolate.Speed * (_decelTime * 0.001f* 0.5f)), _extrapolate.BaseSpeed, _extrapolate.Speed, ExtrapolationType.DecelerationLinear);
				}
			}
		}
		#endregion
		#endregion
	}
}
