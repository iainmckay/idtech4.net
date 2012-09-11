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

namespace idTech4.Game.Animation
{
	public class idAnimBlend
	{
		#region Members
		private idDeclModel _modelDef;

		private int _startTime;
		private int _endTime;
		private int _timeOffset;
		private float _rate;

		private int _blendStartTime;
		private int _blendDuration;
		private float _blendStartValue;
		private float _blendEndValue;

		private float[] _animWeights;

		private short _cycle;
		private short _frame;
		private short _animNumber;
		private bool _allowMove;
		private bool _allowFrameCommands;
		#endregion

		#region Constructor
		public idAnimBlend()
		{
			Reset(null);
		}
		#endregion

		#region Methods
		#region Public
		public bool AddBounds(int currentTime, ref idBounds bounds, bool removeOriginOffset)
		{
			if((_endTime > 0) && (currentTime > _endTime))
			{
				return false;
			}

			idAnim anim = GetAnimation();

			if(anim == null)
			{
				return false;
			}

			float weight = GetWeight(currentTime);

			if(weight == 0)
			{
				return false;
			}

			int time = GetAnimationTime(currentTime);
			int animCount = anim.AnimationCount;
			bool addOrigin = (_allowMove == false) || (removeOriginOffset == false);

			idBounds b;
			Vector3 pos;

			for(int i = 0; i < animCount; i++)
			{
				if(anim.GetBounds(out b, i, time, _cycle) == true)
				{
					if(addOrigin == true)
					{
						anim.GetOrigin(out pos, i, time, _cycle);
						b.Translate(pos);
					}

					bounds.AddBounds(b);
				}
			}

			return true;
		}

		public idAnim GetAnimation()
		{
			if(_modelDef == null)
			{
				return null;
			}

			return _modelDef.GetAnimation(_animNumber);
		}

		public int GetAnimationTime(int currentTime)
		{
			idAnim anim = GetAnimation();

			if(anim != null)
			{
				if(_frame != 0)
				{
					return idHelper.FrameToTime(_frame - 1);
				}
				
				int time = 0;

				// most of the time we're running at the original frame rate, so avoid the int-to-float-to-int conversion
				if(_rate == 1.0f)
				{
					time = currentTime - _startTime + _timeOffset;
				}
				else
				{
					time = (int) (((currentTime - _startTime) * _rate) + _timeOffset);
				}

				// given enough time, we can easily wrap time around in our frame calculations, so
				// keep cycling animations' time within the length of the anim.
				int length = anim.Length;

				if((_cycle < 0) && (length > 0))
				{
					time %= length;

					// time will wrap after 24 days (oh no!), resulting in negative results for the %.
					// adding the length gives us the proper result.
					if(time < 0)
					{
						time += length;
					}
				}
			
				return time;
			}

			return 0;
		}

		public float GetWeight(int currentTime)
		{
			int timeDelta = currentTime - _blendStartTime;
			float w;

			if(timeDelta <= 0)
			{
				w = _blendStartValue;
			}
			else if(timeDelta >= _blendDuration)
			{
				w = _blendEndValue;
			}
			else
			{
				float frac = (float) timeDelta / (float) _blendDuration;
				w = _blendStartValue + (_blendEndValue - _blendStartValue) * frac;
			}

			return w;
		}

		public void Reset(idDeclModel modelDef)
		{
			_modelDef = modelDef;
			_cycle = 1;
			_startTime = 0;
			_endTime = 0;
			_timeOffset = 0;
			_rate = 1.0f;
			_frame = 0;
			_allowMove = true;
			_allowFrameCommands = true;
			_animNumber = 0;

			_animWeights = new float[idR.MaxSyncedAnimations];

			_blendStartValue = 0.0f;
			_blendEndValue = 0.0f;
			_blendStartTime = 0;
			_blendDuration = 0;
		}
		#endregion
		#endregion
	}
}