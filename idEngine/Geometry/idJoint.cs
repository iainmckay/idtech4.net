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

namespace idTech4.Geometry
{
	public struct idJointQuaternion
	{
		public Quaternion Quaternion;
		public Vector3 Translation;
	}

	public struct idJointMatrix
	{
		#region Properties
		public Matrix Rotation
		{
			set
			{		
				// NOTE: idMat3 is transposed because it is column-major
				_m[0 * 4 + 0] = value.M11;
				_m[0 * 4 + 1] = value.M21;
				_m[0 * 4 + 2] = value.M31;
				_m[1 * 4 + 0] = value.M12;
				_m[1 * 4 + 1] = value.M22;
				_m[1 * 4 + 2] = value.M32;
				_m[2 * 4 + 0] = value.M13;
				_m[2 * 4 + 1] = value.M23;
				_m[2 * 4 + 2] = value.M33;
			}
		}

		public Vector3 Translation
		{
			set
			{
				_m[0 * 4 + 3] = value.X;
				_m[1 * 4 + 3] = value.Y;
				_m[2 * 4 + 3] = value.Z;
			}
		}

		public static idJointMatrix Zero
		{
			get
			{
				return new idJointMatrix(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
			}
		}
		#endregion

		#region Members
		private float[] _m;
		#endregion

		#region Constructor
		public idJointMatrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34)
		{
			_m = new float[] {
				m11, m12, m13, m14,
				m21, m22, m23, m24,
				m31, m32, m33, m34
			};
		}
		#endregion
				
		#region Methods
		public Matrix ToMatrix()
		{
			return new Matrix(_m[0 * 4 + 0], _m[1 * 4 + 0], _m[2 * 4 + 0], 0,
					_m[0 * 4 + 1], _m[1 * 4 + 1], _m[2 * 4 + 1], 0,
					_m[0 * 4 + 2], _m[1 * 4 + 2], _m[2 * 4 + 2], 0,
					0, 0, 0, 0);
		}

		public Vector3 ToVector3()
		{
			return new Vector3(_m[0 * 4 + 3], _m[1 * 4 + 3], _m[2 * 4 + 3] );
		}
		#endregion

		#region Overloads
		public static bool operator ==(idJointMatrix x, idJointMatrix y) 
		{
			return x.Equals(y);
		}

		public static bool operator !=(idJointMatrix x, idJointMatrix y) 
		{
			return !x.Equals(y);
		}

		public override bool Equals(object obj)
		{
			if(obj is idJointMatrix)
			{
				return Equals((idJointMatrix) obj);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Exact compare, no epsilon.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool Equals(idJointMatrix m)
		{
			for(int i = 0; i < 12; i++)
			{
				if(_m[i] != m._m[i])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Compare, with epsilon.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="epsilon"></param>
		/// <returns></returns>
		public bool Equals(idJointMatrix m, float epsilon)
		{
			for(int i = 0; i < 12; i++)
			{
				if(idMath.Abs(_m[i] - m._m[i]) > epsilon)
				{
					return false;
				}
			}

			return true;
		}
		#endregion
	}
}