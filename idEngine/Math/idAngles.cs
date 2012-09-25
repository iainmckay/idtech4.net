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

namespace idTech4.Math
{
	public struct idAngles
	{
		public static idAngles Zero
		{
			get
			{
				return new idAngles();
			}
		}

		public float Pitch;
		public float Yaw;
		public float Roll;

		#region Constructor
		public idAngles(float pitch, float yaw, float roll)
		{
			this.Pitch = pitch;
			this.Yaw = yaw;
			this.Roll = roll;
		}
		#endregion

		#region Methods
		public float Get(int index)
		{
			if(index == 0)
			{
				return this.Pitch;
			}
			else if(index == 1)
			{
				return this.Yaw;
			}
			else if(index == 2)
			{
				return this.Roll;
			}

			throw new ArgumentOutOfRangeException("index");
		}

		public void Set(int index, float value)
		{
			if(index == 0)
			{
				this.Pitch = value;
			}
			else if(index == 1)
			{
				this.Yaw = value;
			}
			else if(index == 2)
			{
				this.Roll = value;
			}

			throw new ArgumentOutOfRangeException("index");
		}

		public Matrix ToMatrix()
		{
			float sr, sp, sy, cr, cp, cy;

			idMath.SinCos(MathHelper.ToRadians(this.Yaw), out sy, out cy);
			idMath.SinCos(MathHelper.ToRadians(this.Pitch), out sp, out cp);
			idMath.SinCos(MathHelper.ToRadians(this.Roll), out sr, out cr);

			return new Matrix(
				cp * cy, cp * sy, -sp, 0,
				sr * sp * cy + cr * -sy, sr * sp * sy + cr * cy, sr * cp, 0,
				cr * sp * cy + -sr * -sy, cr * sp * sy + -sr * cy, cr * cp, 0,
				0, 0, 0, 1);
		}
		#endregion

		#region Overloads
		public static idAngles operator +(idAngles a, idAngles b)
		{
			return new idAngles(a.Pitch + b.Pitch, a.Yaw + b.Yaw, a.Roll + b.Roll);
		}

		public static idAngles operator *(idAngles a, float s)
		{
			return new idAngles(a.Pitch * s, a.Yaw * s, a.Roll * s);
		}

		public override string ToString()
		{
			return string.Format("Yaw:{0} Pitch:{1} Roll:{2}", this.Yaw, this.Pitch, this.Roll);
		}
		#endregion
	}
}