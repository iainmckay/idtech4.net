using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

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
	}
}
