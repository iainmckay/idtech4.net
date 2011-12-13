using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace idTech4
{
	public class idMath
	{
		public static readonly float Radian = MathHelper.Pi / 180.0f;

		public static float ToRadians(float v)
		{
			return MathHelper.ToRadians(v);
		}

		public static float Sqrt(float v)
		{
			return (float) Math.Sqrt(v);
		}

		public static float Sin(float s)
		{
			return (float) Math.Sin(s);
		}

		public static float Cos(float c)
		{
			return (float) Math.Cos(c);
		}

		public static float Abs(float a)
		{
			return Math.Abs(a);
		}

		public static float Min(float a, float b)
		{
			return Math.Min(a, b);
		}

		public static float Max(float a, float b)
		{
			return Math.Max(a, b);
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