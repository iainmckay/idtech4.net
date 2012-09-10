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

		public Vector3 this[int x]
		{
			get
			{
				return new Vector3(_points[x, 0], _points[x, 1], _points[x, 2]);
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

		public int PointCount
		{
			get
			{
				return _points.GetUpperBound(0);
			}
		}
		#endregion

		#region Members
		protected int _pointCount;
		protected float[,] _points;
		#endregion

		#region Constructor
		public idWinding()
		{

		}

		public idWinding(int pointCount)
		{
			_pointCount = pointCount;
			_points = new float[pointCount, 5];
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Cuts off the part at the back side of the plane, returns true if some part was at the front.
		/// If there is nothing at the front the number of points is set to zero.
		/// </summary>
		/// <param name="plane"></param>
		/// <param name="epsilon"></param>
		/// <param name="keepOn"></param>
		/// <returns></returns>
		public bool ClipInPlace(Plane plane, float epsilon = idE.OnPlaneEpsilon, bool keepOn = false)
		{
			// TODO: clip in place  important!!!!

			/*float*		dists;
			byte *		sides;
			idVec5 *	newPoints;
			int			newNumPoints;
			int			counts[3];
			float		dot;
			int			i, j;
			idVec5 *	p1, *p2;
			idVec5		mid;
			int			maxpts;

			assert( this );

			dists = (float *) _alloca( (numPoints+4) * sizeof( float ) );
			sides = (byte *) _alloca( (numPoints+4) * sizeof( byte ) );

			counts[SIDE_FRONT] = counts[SIDE_BACK] = counts[SIDE_ON] = 0;

			// determine sides for each point
			for ( i = 0; i < numPoints; i++ ) {
				dists[i] = dot = plane.Distance( p[i].ToVec3() );
				if ( dot > epsilon ) {
					sides[i] = SIDE_FRONT;
				} else if ( dot < -epsilon ) {
					sides[i] = SIDE_BACK;
				} else {
					sides[i] = SIDE_ON;
				}
				counts[sides[i]]++;
			}
			sides[i] = sides[0];
			dists[i] = dists[0];
	
			// if the winding is on the plane and we should keep it
			if ( keepOn && !counts[SIDE_FRONT] && !counts[SIDE_BACK] ) {
				return true;
			}
			// if nothing at the front of the clipping plane
			if ( !counts[SIDE_FRONT] ) {
				numPoints = 0;
				return false;
			}
			// if nothing at the back of the clipping plane
			if ( !counts[SIDE_BACK] ) {
				return true;
			}

			maxpts = numPoints + 4;		// cant use counts[0]+2 because of fp grouping errors

			newPoints = (idVec5 *) _alloca16( maxpts * sizeof( idVec5 ) );
			newNumPoints = 0;

			for ( i = 0; i < numPoints; i++ ) {
				p1 = &p[i];

				if ( newNumPoints+1 > maxpts ) {
					return true;		// can't split -- fall back to original
				}
		
				if ( sides[i] == SIDE_ON ) {
					newPoints[newNumPoints] = *p1;
					newNumPoints++;
					continue;
				}
	
				if ( sides[i] == SIDE_FRONT ) {
					newPoints[newNumPoints] = *p1;
					newNumPoints++;
				}

				if ( sides[i+1] == SIDE_ON || sides[i+1] == sides[i] ) {
					continue;
				}
			
				if ( newNumPoints+1 > maxpts ) {
					return true;		// can't split -- fall back to original
				}

				// generate a split point
				p2 = &p[(i+1)%numPoints];
		
				dot = dists[i] / (dists[i] - dists[i+1]);
				for ( j = 0; j < 3; j++ ) {
					// avoid round off error when possible
					if ( plane.Normal()[j] == 1.0f ) {
						mid[j] = plane.Dist();
					} else if ( plane.Normal()[j] == -1.0f ) {
						mid[j] = -plane.Dist();
					} else {
						mid[j] = (*p1)[j] + dot * ( (*p2)[j] - (*p1)[j] );
					}
				}
				mid.s = p1->s + dot * ( p2->s - p1->s );
				mid.t = p1->t + dot * ( p2->t - p1->t );
			
				newPoints[newNumPoints] = mid;
				newNumPoints++;
			}

			if ( !EnsureAlloced( newNumPoints, false ) ) {
				return true;
			}

			numPoints = newNumPoints;
			memcpy( p, newPoints, newNumPoints * sizeof(idVec5) );

			return true;*/

			return false;
		}

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