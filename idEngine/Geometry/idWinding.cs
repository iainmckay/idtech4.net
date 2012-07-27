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

namespace idTech4.Geometry
{
	/// <summary>
	/// A winding is an arbitrary convex polygon defined by an array of points.
	/// </summary>
	public class idWinding
	{
		#region Properties
		public float this [int x, int y]
		{
			get
			{
				return _points[x, y];
			}
			set
			{
				_points[x, y] = value;
			}
		}

		public Vector3 Center
		{
			get
			{
				Vector3 center = Vector3.Zero;

				for(int i = 0; i < _pointCount; i++)
				{
					center += new Vector3(_points[i,0], _points[i,1], _points[i,2]);
				}

				center *= (1.0f / _pointCount);

				return center;
			}
		}
		#endregion

		#region Members
		private int _pointCount;
		private float[,] _points;
		#endregion

		#region Constructor
		public idWinding()
		{

		}

		public idWinding(int pointCount)
		{
			_pointCount = pointCount;
			_points = new float[pointCount,5];
		}
		#endregion

		#region Methods
		#region Public
		public Plane GetPlane()
		{
			if(_pointCount < 3)
			{
				return new Plane();
			}

			Vector3 center = this.Center;
			Vector3 v = new Vector3(_points[0, 0], _points[0, 1], _points[0, 2]);
			Vector3 v1 = v - center;
			Vector3 v2 = new Vector3(_points[1, 0], _points[1, 1], _points[1, 2]) - center;


			Plane plane = new Plane();
			plane.Normal = Vector3.Cross(v2, v1);
			plane.Normalize();

			Vector3 tmp = plane.Normal * v;

			plane.D = tmp.X + tmp.Y + tmp.Z;

			return plane;
		}
		#endregion
		#endregion
	}
}
