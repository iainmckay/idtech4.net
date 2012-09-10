﻿/*
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
		#region Plane
		public static float Distance(this Plane plane, Vector3 point)
		{
			return Vector3.Dot(Vector3.Normalize(plane.Normal), point) - plane.D;
		}

		public static void FitThroughPoint(this Plane plane, Vector3 point)
		{
			plane.D =  -Vector3.Multiply(plane.Normal, point).Length();
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