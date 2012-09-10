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
	public class idSphere
	{
		#region Properties
		public Vector3 Origin
		{
			get
			{
				return _origin;
			}
		}

		public float Radius
		{
			get
			{
				return _radius;
			}
		}
		#endregion

		#region Members
		private Vector3 _origin;
		private float _radius;
		#endregion

		#region Constructor
		public idSphere()
		{

		}

		public idSphere(Vector3 point)
		{
			_origin = point;
			_radius = 0.0f;
		}

		public idSphere(Vector3 point, float r)
		{
			_origin = point;
			_radius = r;
		}
		#endregion

		#region Overloads
		public override bool Equals(object obj)
		{
			if(obj is idSphere)
			{
				return Equals((idSphere) obj);
			}

 			return base.Equals(obj);
		}

		/// <summary>
		/// Exact compare, no epsilon.
		/// </summary>
		/// <param name="sphere"></param>
		/// <returns></returns>
		public bool Equals(idSphere sphere)
		{
			return ((_origin == sphere.Origin) && (_radius == sphere.Radius));
		}

		/// <summary>
		/// Compare with epsilon.
		/// </summary>
		/// <param name="sphere"></param>
		/// <param name="epsilon"></param>
		/// <returns></returns>
		public bool Equals(idSphere sphere, float epsilon)
		{
			return ((_origin.Compare(sphere.Origin, epsilon) == true) && (idMath.Abs(_radius - sphere.Radius) <= epsilon));
		}

		public static bool operator ==(idSphere s1, idSphere s2)
		{
			return s1.Equals(s2);
		}

		public static bool operator !=(idSphere s1, idSphere s2)
		{
			return !s1.Equals(s2);
		}
		#endregion
	}
}