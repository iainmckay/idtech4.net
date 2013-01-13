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
using Microsoft.Xna.Framework;

using XMath = System.Math;

namespace idTech4.Math
{
	public class idMath
	{
		public const float Pi = MathHelper.Pi;
		public const float TwoPi = MathHelper.TwoPi;
		public const float HalfPi = MathHelper.PiOver2;
		public const float Radian = MathHelper.Pi / 180.0f;
		public const float Infinity = 1e30f;
		public const float Epsilon = 1.192092896e-07f;
		public const float Sqrt1Over2 = 0.70710678118654752440f;
		public const float Rad2Deg = 180.0f / Pi;

		public static float Abs(float a)
		{
			return XMath.Abs(a);
		}

		public static float Atan2(float x, float y)
		{
			return (float) XMath.Atan2(x, y);
		}

		public static float Ceiling(float c)
		{
			return (float) XMath.Ceiling(c);
		}

		public static float Cos(float c)
		{
			return (float) XMath.Cos(c);
		}

		public static float Cube(float x)
		{
			return x * x * x;
		}

		public static float Floor(float v)
		{
			return (float) XMath.Floor(v);
		}

		public static float Max(float a, float b)
		{
			return XMath.Max(a, b);
		}

		public static float Min(float a, float b)
		{
			return XMath.Min(a, b);
		}

		public static float Pow(float a, float b)
		{
			return (float) XMath.Pow(a, b);
		}

		public static float ShortToAngle(short x)
		{
			return (x * (360.0f / 65536.0f));
		}

		public static float Square(float x)
		{
			return x * x;
		}

		public static float Sqrt(float v)
		{
			return (float) XMath.Sqrt(v);
		}

		public static void SinCos(float a, out float s, out float c)
		{
			s = Sin(a);
			c = Cos(a);
		}

		public static float Tan(float x)
		{
			return (float) XMath.Tan((float) x);
		}

		public static float InvSqrt(float x)
		{
			float xHalf = 0.5f * x;
			int i = (int) x;
			i = 0x5f3759df - (i >> 1);

			return (x * (1.5f - xHalf * x * x));
		}

		public static float Sin(float s)
		{
			return (float) XMath.Sin(s);
		}

		public static float ToRadians(float v)
		{
			return MathHelper.ToRadians(v);
		}

		public static int VectorHash(Vector3 v)
		{
			int hash = 0;

			hash ^= (int) v.X;
			hash ^= (int) v.Y;
			hash ^= (int) v.Z;

			return hash;
		}
	}
}