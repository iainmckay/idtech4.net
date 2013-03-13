/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System.Diagnostics;

using idTech4.Services;

namespace idTech4.Renderer
{
	public class idResolutionScale : IResolutionScale
	{
		#region Members
		private float _dropMilliseconds;
		private float _raiseMilliseconds;
		private int _framesAboveRaise;
		private float _currentResolution;
		#endregion

		#region Constructor
		public idResolutionScale()
		{
			_dropMilliseconds  = 15.0f;
			_raiseMilliseconds = 13.0f;
			_framesAboveRaise  = 0;
			_currentResolution = 1.0f;
		}
		#endregion

		#region IResolutionScale implementation
		#region Methods
		public void InitForMap(string mapName)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			_dropMilliseconds = cvarSystem.GetFloat("rs_dropMilliseconds");
			_raiseMilliseconds = cvarSystem.GetFloat("rs_raiseMilliseconds");
		}

		public void ResetToFullResolution()
		{
			_currentResolution = 1.0f;
		}

		public void GetCurrentResolutionScale(out float x, out float y)
		{
			Debug.Assert(_currentResolution >= Constants.MinimumResolutionScale);
			Debug.Assert(_currentResolution <= Constants.MaximumResolutionScale);

			x = Constants.MaximumResolutionScale;
			y = Constants.MaximumResolutionScale;

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			switch(cvarSystem.GetInt("rs_enable"))
			{
				case 0: return;
				case 1:
					x = _currentResolution;
					break;
				case 2:
					y = _currentResolution;
					break;

				case 3:
					float middle = (Constants.MinimumResolutionScale + Constants.MaximumResolutionScale) * 0.5f;

					if(_currentResolution >= middle)
					{
						// first scale horizontally from max to min
						x = Constants.MinimumResolutionScale + (_currentResolution - middle) * 2.0f;
					}
					else
					{
						// then scale vertically from max to min
						x = Constants.MinimumResolutionScale;
						y = Constants.MinimumResolutionScale + (_currentResolution - Constants.MinimumResolutionScale) * 2.0f;
					}
					break;
			}
		
			float forceFraction = cvarSystem.GetFloat("rs_forceFractionX");

			if((forceFraction > 0.0f) && (forceFraction <= Constants.MaximumResolutionScale))
			{
				x = forceFraction;
			}

			forceFraction = cvarSystem.GetFloat("rs_forceFractionY");

			if((forceFraction > 0.0f) && (forceFraction <= Constants.MaximumResolutionScale))
			{
				y = forceFraction;
			}
		}

		public void SetCurrentGPUFrameTime(int microSeconds)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			float old              = _currentResolution;
			float milliSeconds     = microSeconds * 0.001f;

			if(milliSeconds > _dropMilliseconds)
			{
				// we missed our target, so drop the resolution.
				// the target should be set conservatively so this does not
				// necessarily imply a missed VBL.
				//
				// we might consider making the drop in some way
				// proportional to how badly we missed
				_currentResolution -= cvarSystem.GetFloat("rs_dropFraction");

				if(_currentResolution < Constants.MinimumResolutionScale)
				{
					_currentResolution = Constants.MinimumResolutionScale;
				}
			} 
			else if(milliSeconds < _raiseMilliseconds)
			{
				// we seem to have speed to spare, so increase the resolution
				// if we stay here consistantly.  the raise fraction should
				// be smaller than the drop fraction to avoid ping-ponging
				// back and forth.
				if(++_framesAboveRaise >= cvarSystem.GetInt("rs_raiseFrames"))
				{
					_framesAboveRaise   = 0;
					_currentResolution += cvarSystem.GetFloat("rs_raiseFraction");

					if(_currentResolution > Constants.MaximumResolutionScale)
					{
						_currentResolution = Constants.MaximumResolutionScale;
					}
				}
			}
			else
			{
				// we are inside the target range
				_framesAboveRaise = 0;
			}

			if((cvarSystem.GetInt("rs_showResolutionChanges") > 1)
				|| ((cvarSystem.GetInt("rs_showResolutionChanges") == 1) && (_currentResolution != old)))
			{
				idLog.WriteLine("GPU msec: {0:000000} resolutionScale: {1:000000}", milliSeconds, _currentResolution);
			}
		}

		public string GetConsoleText()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			float x;
			float y;

			int enable = cvarSystem.GetInt("rs_enable");

			if(enable == 0)
			{
				return "rs-off";
			}
			else
			{
				string s    = string.Empty;
				int display = cvarSystem.GetInt("rs_display");

				GetCurrentResolutionScale(out x, out y);

				if(display > 0)
				{
					x *= 1280.0f;
					y *= 720.0f;

					if(enable == 1)
					{
						y = 1.0f;
					}
					else if(enable == 2)
					{
						x = 1.0f;
					}

					return string.Format("rs-pixels {0}", (int) (x * y));
				}
				else
				{
					if(enable == 3)
					{
						return string.Format("{0:00}h,{1:00}v", (int) (100.0f * x), (int) (100.0f * y));
					}
					else
					{
						return string.Format("{0:00}{1}", (enable == 1) ? (int) (100.0f * x) : (int) (100.0f * y), (enable == 1) ? "h" : "v");
					}
				}
			}
		}
		#endregion
		#endregion
	}
}