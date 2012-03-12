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

using idTech4.Math;

namespace idTech4
{
	public struct idRotation
	{
		#region Properties
		public Vector3 Origin
		{
			get
			{
				return _origin;
			}
		}

		public Vector3 Vector
		{
			get
			{
				return _vector;
			}
		}

		public float Angle
		{
			get
			{
				return _angle;
			}
		}
		#endregion

		#region Members
		private Vector3 _origin;
		private Vector3 _vector;
		private float _angle;

		private Matrix _axis;
		private bool _axisValid;
		#endregion

		#region Constructor
		public idRotation(Vector3 origin, Vector3 vector, float angle)
		{
			_origin = origin;
			_vector = vector;
			_angle = angle;
			_axis = Matrix.Identity;
			_axisValid = false;
		}
		#endregion

		#region Methods
		public Matrix ToMatrix()
		{
			if(_axisValid == true)
			{
				return _axis;
			}

			float a = _angle * (idMath.Radian * 0.5f);
			float s = idMath.Sin(a);
			float c = idMath.Cos(a);

			float x = _vector.X * s;
			float y = _vector.Y * s;
			float z = _vector.Z * s;

			float x2 = x + x;
			float y2 = y + y;
			float z2 = z + z;

			float xx = x * x2;
			float xy = x * y2;
			float xz = x * z2;

			float yy = y * y2;
			float yz = y * z2;
			float zz = z * z2;

			float wx = c * x2;
			float wy = c * y2;
			float wz = c * z2;

			_axis = new Matrix();
			_axis.M11 = 1.0f - (yy + zz);
			_axis.M12 = xy - wz;
			_axis.M13 = xz + wy;

			_axis.M21 = xy + wz;
			_axis.M22 = 1.0f - (xx + zz);
			_axis.M23 = yz - wx;

			_axis.M31 = xz - wy;
			_axis.M32 = yz + wx;
			_axis.M33 = 1.0f - (xx + yy);

			_axisValid = true;

			return _axis;
		}
		#endregion

		#region Operator overloads
		public static idRotation operator -(idRotation r)
		{
			return new idRotation(r.Origin, r.Vector, -r.Angle);
		}

		public static idRotation operator *(idRotation r, float s)
		{
			return new idRotation(r.Origin, r.Vector, r.Angle * s);
		}

		public static idRotation operator /(idRotation r, float s)
		{
			return new idRotation(r.Origin, r.Vector, r.Angle / s);
		}

		public static Vector3 operator *(Vector3 v, idRotation r)
		{
			return (r * v);
		}

		public static Vector3 operator *(idRotation r, Vector3 v)
		{
			if(r._axisValid == false)
			{
				r.ToMatrix();
			}

			return (Vector3.Transform(v - r.Origin, r._axis) + r.Origin);
		}
		#endregion
	}
}