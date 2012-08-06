using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace idTech4.Math
{
	public struct idCompressedQuaternion
	{
		#region Fields
		public float X;
		public float Y;
		public float Z;
		#endregion

		#region Constructor
		public idCompressedQuaternion(float x, float y, float z)
		{
			this.X = x;
			this.Y = y;
			this.Z = z;
		}
		#endregion

		#region Methods
		public Quaternion ToQuaternion()
		{
			// take the absolute value because floating point rounding may cause the dot of x,y,z to be larger than 1
			return new Quaternion(this.X, this.Y, this.Z, idMath.Sqrt(idMath.Abs(1.0f - (this.X * this.X + this.Y * this.Y + this.Z * this.Z))));
		}
		#endregion

		#region Overloads
		public override bool Equals(object obj)
		{
			if(obj is idCompressedQuaternion)
			{
 				return Equals((idCompressedQuaternion) obj);
			}
			
			return base.Equals(obj);
		}

		public bool Equals(idCompressedQuaternion q)
		{
			return ((this.X == q.X ) && (this.Y == q.Y) && (this.Z == q.Z));
		}

		public bool Equals(idCompressedQuaternion q, float epsilon)
		{
			if(idMath.Abs(this.X - q.X) > epsilon)
			{
				return false;
			}

			if(idMath.Abs(this.Y - q.Y) > epsilon)
			{
				return false;
			}

			if(idMath.Abs(this.Z - q.Z) > epsilon)
			{
				return false;
			}

			return true;			
		}

		public static bool operator ==(idCompressedQuaternion x, idCompressedQuaternion y)
		{
			return x.Equals(y);
		}

		public static bool operator !=(idCompressedQuaternion x, idCompressedQuaternion y)
		{
			return !x.Equals(y);
		}
		#endregion
	}
}
