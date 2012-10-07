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
	public static class Extensions
	{
		#region Matrix
		public static idAngles ToAngles(this Matrix m)
		{
			float sp = m.M13;

			// cap off our sin value so that we don't get any NANs
			if(sp > 1.0f)
			{
				sp = 1.0f;
			}
			else if(sp < -1.0f)
			{
				sp = -1.0f;
			}

			double theta = -System.Math.Asin(sp);
			double cp = System.Math.Cos(theta);

			idAngles angles = new idAngles();

			if(cp > (8192.0f * idMath.Epsilon))
			{
				angles.Pitch = MathHelper.ToDegrees((float) theta);
				angles.Yaw = MathHelper.ToDegrees(idMath.Atan2(m.M12, m.M11));
				angles.Roll = MathHelper.ToDegrees(idMath.Atan2(m.M23, m.M33));
			}
			else
			{
				angles.Pitch = MathHelper.ToDegrees((float) theta);
				angles.Yaw = MathHelper.ToDegrees(-idMath.Atan2(m.M21, m.M22));
				angles.Roll = 0;
			}

			return angles;
		}
		#endregion

		#region Plane
		public static float Distance(this Plane plane, Vector3 point)
		{
			return Vector3.Dot(Vector3.Normalize(plane.Normal), point) - plane.D;
		}

		public static void FitThroughPoint(this Plane plane, Vector3 point)
		{
			plane.D = -Vector3.Multiply(plane.Normal, point).Length();
		}

		public static bool FromPoints(this Plane plane, Vector3 p1, Vector3 p2, Vector3 p3, bool fixDegenerate = true)
		{
			plane.Normal = Vector3.Cross(p1 - p2, p3 - p2);

			if(plane.Normalize(fixDegenerate) == 0.0f)
			{
				return false;
			}

			plane.D = -(plane.Normal * p2).Length();

			return true;
		}

		public static float Normalize(this Plane plane, bool fixDegenerate)
		{
			plane.Normalize();
			float length = plane.Normal.Length();

			if(fixDegenerate == true)
			{
				plane.Normal.FixDegenerateNormal();
			}

			return length;
		}
		#endregion

		#region Quaternion
		public static Matrix ToMatrix(this Quaternion q)
		{
			float x2 = q.X + q.X;
			float y2 = q.Y + q.Y;
			float z2 = q.Z + q.Z;

			float xx = q.X * x2;
			float xy = q.X * y2;
			float xz = q.X * z2;

			float yy = q.Y * y2;
			float yz = q.Y * z2;
			float zz = q.Z * z2;

			float wx = q.W * x2;
			float wy = q.W * y2;
			float wz = q.W * z2;

			return new Matrix(
				1.0f - (yy + zz), xy - wz, xz + wy, 0,
				xy + wz, 1.0f - (xx + zz), yz - wx, 0,
				xz - wy, yz + wx, 1.0f - (xx + yy), 0,
				0, 0, 0, 1);
		}
		#endregion

		#region Vector3
		public static bool Compare(this Vector3 v2, Vector3 v, float epsilon)
		{
			if(idMath.Abs(v2.X - v.X) > epsilon)
			{
				return false;
			}

			if(idMath.Abs(v2.Y - v.Y) > epsilon)
			{
				return false;
			}

			if(idMath.Abs(v2.Z - v.Z) > epsilon)
			{
				return false;
			}

			return true;
		}

		public static bool FixDegenerateNormal(this Vector3 v)
		{
			if(v.X == 0.0f)
			{
				if(v.Y == 0.0f)
				{
					if(v.Z > 0.0f)
					{
						if(v.Z != 1.0f)
						{
							v.Z = 1.0f;
							return true;
						}
					}
					else
					{
						if(v.Z != -1.0f)
						{
							v.Z = -1.0f;
							return true;
						}
					}
					return false;
				}
				else if(v.Z == 0.0f)
				{
					if(v.Y > 0.0f)
					{
						if(v.Y != 1.0f)
						{
							v.Y = 1.0f;
							return true;
						}
					}
					else
					{
						if(v.Y != -1.0f)
						{
							v.Y = -1.0f;
							return true;
						}
					}
					return false;
				}
			}
			else if(v.Y == 0.0f)
			{
				if(v.Z == 0.0f)
				{
					if(v.X > 0.0f)
					{
						if(v.X != 1.0f)
						{
							v.X = 1.0f;
							return true;
						}
					}
					else
					{
						if(v.X != -1.0f)
						{
							v.X = -1.0f;
							return true;
						}
					}
					return false;
				}
			}
			if(idMath.Abs(v.X) == 1.0f)
			{
				if((v.X != 0.0f) || (v.Z != 0.0f))
				{
					v.Y = v.Z = 0.0f;
					return true;
				}
				return false;
			}
			else if(idMath.Abs(v.Y) == 1.0f)
			{
				if((v.X != 0.0f) || (v.Z != 0.0f))
				{
					v.X = v.Z = 0.0f;
					return true;
				}
				return false;
			}
			else if(idMath.Abs(v.Z) == 1.0f)
			{
				if((v.X != 0.0f) || (v.Y != 0.0f))
				{
					v.X = v.Y = 0.0f;
					return true;
				}
				return false;
			}
			return false;
		}

		public static float Get(this Vector3 v, int index)
		{
			if(index == 0)
			{
				return v.X;
			}
			else if(index == 1)
			{
				return v.Y;
			}
			else if(index == 2)
			{
				return v.Z;
			}
			else
			{
				throw new ArgumentOutOfRangeException("index");
			}
		}

		public static Matrix ToMatrix(this Vector3 v)
		{
			Matrix m = new Matrix();
			m.M11 = v.X;
			m.M12 = v.Y;
			m.M13 = v.Z;

			float d = v.X * v.X + v.Y * v.Y;

			if(d == 0)
			{
				m.M21 = 1.0f;
				m.M22 = 0.0f;
				m.M23 = 0.0f;
			}
			else
			{
				d = idMath.InvSqrt(d);

				m.M21 = -v.Y * d;
				m.M22 = v.X * d;
				m.M23 = 0.0f;
			}

			Vector3 tmp = Vector3.Cross(v, new Vector3(m.M21, m.M22, m.M23));

			m.M31 = tmp.X;
			m.M32 = tmp.Y;
			m.M33 = tmp.Z;

			m.M44 = 1;

			return m;
		}
		#endregion
	}
}