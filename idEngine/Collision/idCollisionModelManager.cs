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

using idTech4.Renderer;

namespace idTech4.Collision
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Trace model vs. polygonal model collision detection.
	/// <p/>
	/// Short translations are the least expensive. Retrieving contact points is
	/// about as cheap as a short translation. Position tests are more expensive
	/// and rotations are most expensive.
	/// <p/>
	/// There is no position test at the start of a translation or rotation. In other
	/// words if a translation with start != end or a rotation with angle != 0 starts
	/// in solid, this goes unnoticed and the collision result is undefined.
	/// <p/>
	/// A translation with start == end or a rotation with angle == 0 performs
	/// a position test and fills in the trace_t structure accordingly.
	/// </remarks>
	public sealed class idCollisionModelManager
	{
	}

	public enum ContactType
	{
		/// <summary>No contact.</summary>
		None,
		/// <summary>Trace model edge hits model edge.</summary>
		Edge,
		/// <summary>Model vertex hits trace model polygon.</summary>
		ModelVertex,
		/// <summary>Trace model vertex hits model polygon.</summary>
		TraceModelVertex
	}

	public struct ContactInfo
	{
		/// <summary>Contact type.</summary>
		public ContactType Type;
		/// <summary>Point of contact.</summary>
		public Vector3	Point;
		/// <summary>Contact plane normal.</summary>
		public Vector3 Normal;
		/// <summary>Contact plane distance.</summary>
		public float Distance;
		/// <summary>Contents at other side of surface.</summary>
		public int	Contents;
		/// <summary>Surface material.</summary>
		public idMaterial Material;
		/// <summary>Contact feature on model.</summary>
		public int	ModelFeature;
		/// <summary>Contact feature on trace model.</summary>
		public int	TraceModelFeature;
		/// <summary>Entity the contact surface is a part of.</summary>
		public int	EntityIndex;
		/// <summary>ID of the clip model the contact surface is part of.</summary>
		public int ID;
	}

	public struct TraceResult
	{
		/// <summary>Fraction of movement completed, 1.0 = didn't hit anything.</summary>
		public float Fraction;
		/// <summary>Final position of trace model.</summary>
		public Vector3 EndPosition;
		/// <summary>Final axis of trace model.</summary>
		public Matrix EndAxis;
		/// <summary>Contact information, only valid if fraction < 1.0.</summary>
		public ContactInfo ContactInformation;
	}
}