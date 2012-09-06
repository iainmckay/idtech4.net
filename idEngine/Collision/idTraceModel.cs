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

using idTech4;
using idTech4.Math;

namespace idTech4.Collision
{
	/// <summary>
	/// A trace model is an arbitrary polygonal model which is used by the
	/// collision detection system to find collisions, contacts or the contents
	/// of a volume. For collision detection speed reasons the number of vertices
	/// and edges are limited. The trace model can have any shape. However convex
	/// models are usually preferred.
	/// </summary>
	public class idTraceModel
	{
		#region Properties
		public idBounds Bounds
		{
			get
			{
				return _bounds;
			}
		}

		public TraceModelType Type
		{
			get
			{
				return _type;
			}
		}
		#endregion

		#region Members
		private TraceModelType _type;
		private idBounds _bounds;
		private Vector3 _offset; // offset to center of model.

		private TraceModelEdge[] _edges;
		private TraceModelPolygon[] _polygons;
		private Vector3[] _vertices;

		private bool _isConvex;
		#endregion

		#region Constructor
		public idTraceModel()
		{
			_type = TraceModelType.Invalid;
			_bounds = idBounds.Zero;

			_edges = new TraceModelEdge[0];
			_polygons = new TraceModelPolygon[0];
			_vertices = new Vector3[0];
		}

		/// <summary>
		/// Axial bounding box.
		/// </summary>
		/// <param name="bounds"></param>
		public idTraceModel(idBounds bounds)
		{
			InitBox();
			SetupBox(bounds);
		}
		#endregion

		#region Methods
		#region Public
		public void SetupBox(idBounds bounds)
		{
			if(_type != TraceModelType.Box)
			{
				InitBox();
			}

			// offset to center
			_offset = (bounds.Min + bounds.Max) * 0.5f;

			// set box vertices
			_vertices = new Vector3[8];

			for(int i = 0; i < 8; i++)
			{
				_vertices[i] = new Vector3(
						((i ^ (i >> 1)) == 0) ? bounds.Min.X : bounds.Max.X,
						(((i >> 1) & 1) == 0) ? bounds.Min.Y : bounds.Max.Y,
						(((i >> 2) & 1) == 0) ? bounds.Min.Z : bounds.Max.Z
					);
			}

			// set polygon plane distances
			_polygons = new TraceModelPolygon[6];
			_polygons[0].Distance = -_bounds.Min.Z;
			_polygons[1].Distance = bounds.Max.Z;
			_polygons[2].Distance = -bounds.Min.Y;
			_polygons[3].Distance = bounds.Max.X;
			_polygons[4].Distance = bounds.Max.Y;
			_polygons[5].Distance = -bounds.Min.X;

				// set polygon bounds
			for(int i = 0; i < 6; i++)
			{
				_polygons[i].Bounds = bounds;
			}

			_polygons[0].Bounds.Max.Z = bounds.Min.Z;
			_polygons[1].Bounds.Min.Z = bounds.Max.Z;
			_polygons[2].Bounds.Max.Y = bounds.Min.Y;
			_polygons[3].Bounds.Min.X = bounds.Max.X;
			_polygons[4].Bounds.Min.Y = bounds.Max.Y;
			_polygons[5].Bounds.Max.X = bounds.Min.X;

			_bounds = bounds;
		}
		#endregion

		#region Private
		private int GenerateEdgeNormals()
		{
			int edgeCount = _edges.Length;

			for(int i = 0; i < edgeCount; i++)
			{
				_edges[i].Normal = Vector3.Zero;
			}

			int sharpEdgeCount = 0;
			int polyCount = _polygons.Length;
			TraceModelPolygon poly;
			TraceModelEdge edge;

			for(int i = 0; i < polyCount; i++)
			{
				poly = _polygons[i];
				edgeCount = poly.Edges.Length;

				for(int j = 0; j < edgeCount; j++)
				{
					int edgeIndex = (int) idMath.Abs(poly.Edges[j]);
					edge = _edges[edgeIndex];

					if(edge.Normal == Vector3.Zero)
					{
						edge.Normal = poly.Normal;
					}
					else
					{
						float dot = Vector3.Dot(edge.Normal, poly.Normal);

						// if the two planes make a very sharp edge
						if(dot < -0.7f)
						{
							// max length normal pointing outside both polygons
							Vector3 direction = _vertices[edge.V[(edgeIndex > 0) ? 1 : 0]] - _vertices[edge.V[(edgeIndex < 0) ? 1 : 0]];

							edge.Normal = Vector3.Cross(edge.Normal, direction) + Vector3.Cross(poly.Normal, -direction);
							edge.Normal *= (0.5f / (0.5f + 0.5f * -0.7f)) / edge.Normal.Length();

							sharpEdgeCount++;
						}
						else
						{
							edge.Normal = (0.5f / (0.5f + 0.5f * dot)) * (edge.Normal + poly.Normal);
						}
					}
				}
			}

			return sharpEdgeCount;
		}

		private void InitBox()
		{
			_type = TraceModelType.Box;
			_vertices = new Vector3[8];
			_edges = new TraceModelEdge[13];
			_polygons = new TraceModelPolygon[6];

			// set box edges
			for(int i = 0; i < 4; i++)
			{
				_edges[i + 1].V = new int[] { i, (i + 1) & 3 };
				_edges[i + 5].V = new int[] { 4 + i, 4 + ((i + 1) & 3) };
				_edges[i + 9].V = new int[] { i, 4 + i };
			}

			// all edges of a polygon go counter clockwise
			_polygons[0].Normal = new Vector3(0, 0, -1);
			_polygons[0].Edges = new int[] {
					-4, -3, -2, -1
				};

			_polygons[1].Normal = new Vector3(0, 0, 1);
			_polygons[1].Edges = new int[] {
					5, 6, 7, 8
				};

			_polygons[2].Normal = new Vector3(0, -1, 0);
			_polygons[2].Edges = new int[] {
					1, 10, -5, -9
				};

			_polygons[3].Normal = new Vector3(1, 0, 0);
			_polygons[3].Edges = new int[] {
					2, 11, -6, -10
				};

			_polygons[4].Normal = new Vector3(0, 1, 0);
			_polygons[4].Edges = new int[] {
					3, 12, -7, -11
				};

			_polygons[5].Normal = new Vector3(-1, 0, 0);
			_polygons[5].Edges = new int[] {
					4, 9, -8, -12
				};

			// convex model
			_isConvex = true;

			GenerateEdgeNormals();
		}
		#endregion
		#endregion

		#region Operator overloads
		public override bool Equals(object obj)
		{
			if(obj is idTraceModel)
			{
				return Equals((idTraceModel) obj);
			}
 			 
			return base.Equals(obj);
		}

		public bool Equals(idTraceModel traceModel)
		{
			if((this._type != traceModel._type) 
				|| (this._vertices.Length != traceModel._vertices.Length)
				|| (this._edges.Length != traceModel._edges.Length)
				|| (this._polygons.Length != traceModel._polygons.Length))
			{
				return false;
			}

			if((_bounds != traceModel._bounds)
				|| (_offset != traceModel._offset))
			{
				return false;
			}

			switch(_type)
			{
				case TraceModelType.Bone:
				case TraceModelType.Polygon:
				case TraceModelType.PolygonVolume:
				case TraceModelType.Custom:
					int vertexCount = traceModel._vertices.Length;

					for(int i = 0; i < vertexCount; i++)
					{
						if((i >= _vertices.Length) || (_vertices[i] != traceModel._vertices[i]))
						{
							return false;
						}
					}
					break;
			}

			return true;
		}

		public static bool operator ==(idTraceModel a, idTraceModel b) 
		{
			return a.Equals(b);
		}

		public static bool operator !=(idTraceModel a, idTraceModel b) 
		{
			return (a.Equals(b) == false);
		}
		#endregion
	}

	public enum TraceModelType
	{
		/// <summary>Invalid trace model.</summary>
		Invalid,
		/// <summary>Box.</summary>
		Box,
		/// <summary>Octahedron</summary>
		Octahedron,
		/// <summary>Dodecahedron</summary>
		Dodecahedron,
		/// <summary>Cylinder approximation.</summary>
		Cylinder,
		/// <summary>Cone approximation.</summary>
		Cone,
		/// <summary>Two tetrahedrons attached to each other.</summary>
		Bone,
		/// <summary>Arbitrary convex polygon.</summary>
		Polygon,
		/// <summary>Volume for arbitrary convex polygon.</summary>
		PolygonVolume,
		/// <summary>Loaded from map model or ASE/LWO.</summary>
		Custom
	}

	public struct TraceModelEdge
	{
		public int[] V;
		public Vector3 Normal;
	}

	public struct TraceModelPolygon
	{
		public Vector3 Normal;
		public float Distance;
		public idBounds Bounds;
		public int[] Edges;
	}
}