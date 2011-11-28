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
	public enum TraceModelType
	{
		Invalid,
		Box,
		Octahedron,
		Dodecahedon,
		Cylinder,
		Cone,
		/// <summary>
		/// Two tetrahedrons attached to each other
		/// </summary>
		Bone,
		/// <summary>
		/// Arbitrary convex polygon.
		/// </summary>
		Polygon,
		/// <summary>
		/// Volume for arbitrary convex polygon.
		/// </summary>
		PolygonVolume,
		/// <summary>
		/// Loaded from map model or ASE/LWO.
		/// </summary>
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

	public class TraceModelCache
	{
		public idTraceModel TraceModel;
		public float Volume;
		public Vector3 CenterOfMass;
		public Matrix InertiaTensor;
		public int ReferenceCount;
	}

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
		#endregion

		#region Members
		private TraceModelType _type;

		private Vector3[] _vertices;
		private TraceModelEdge[] _edges;
		private TraceModelPolygon[] _polygons;

		private idBounds _bounds;
		private Vector3 _offset; // offset to center of model.

		private bool _isConvex;

		private static List<TraceModelCache> _traceModelCache = new List<TraceModelCache>();
		private static Dictionary<int, int> _traceModelHashCache = new Dictionary<int, int>();
		#endregion

		#region Constructor
		public idTraceModel()
		{
			_type = TraceModelType.Invalid;
		}

		/// <summary>
		/// Axial bounding box.
		/// </summary>
		/// <param name="boxBounds"></param>
		public idTraceModel(idBounds boxBounds)
		{
			InitBox();
			SetupBox(boxBounds);
		}

		/// <summary>
		/// Cylinder approximation.
		/// </summary>
		/// <param name="cylBounds"></param>
		/// <param name="sideCount"></param>
		/*public idTraceModel(idBounds cylBounds, int sideCount)
		{
			SetupCylinder(cylBounds, sideCount);
		}

		/// <summary>
		/// Bone.
		/// </summary>
		/// <param name="lenfth"></param>
		/// <param name="width"></param>
		public idTraceModel(float lenfth, float width)
		{
			InitBone();
			SetupBone(length, width);
		}*/
		#endregion

		#region Methods
		#region Public
		public void GetMassProperties(float density, out float mass, out Vector3 centerOfMass, out Matrix inertiaTensor)
		{
			// TODO
			mass = 0;
			centerOfMass = Vector3.Zero;
			inertiaTensor = Matrix.Identity;

			/*volumeIntegrals_t integrals;

			// if polygon trace model
			if ( type == TRM_POLYGON ) {
				idTraceModel trm;

				VolumeFromPolygon( trm, 1.0f );
				trm.GetMassProperties( density, mass, centerOfMass, inertiaTensor );
				return;
			}

			VolumeIntegrals( integrals );

			// if no volume
			if ( integrals.T0 == 0.0f ) {
				mass = 1.0f;
				centerOfMass.Zero();
				inertiaTensor.Identity();
				return;
			}

			// mass of model
			mass = density * integrals.T0;
			// center of mass
			centerOfMass = integrals.T1 / integrals.T0;
			// compute inertia tensor
			inertiaTensor[0][0] = density * (integrals.T2[1] + integrals.T2[2]);
			inertiaTensor[1][1] = density * (integrals.T2[2] + integrals.T2[0]);
			inertiaTensor[2][2] = density * (integrals.T2[0] + integrals.T2[1]);
			inertiaTensor[0][1] = inertiaTensor[1][0] = - density * integrals.TP[0];
			inertiaTensor[1][2] = inertiaTensor[2][1] = - density * integrals.TP[1];
			inertiaTensor[2][0] = inertiaTensor[0][2] = - density * integrals.TP[2];
			// translate inertia tensor to center of mass
			inertiaTensor[0][0] -= mass * (centerOfMass[1]*centerOfMass[1] + centerOfMass[2]*centerOfMass[2]);
			inertiaTensor[1][1] -= mass * (centerOfMass[2]*centerOfMass[2] + centerOfMass[0]*centerOfMass[0]);
			inertiaTensor[2][2] -= mass * (centerOfMass[0]*centerOfMass[0] + centerOfMass[1]*centerOfMass[1]);
			inertiaTensor[0][1] = inertiaTensor[1][0] += mass * centerOfMass[0] * centerOfMass[1];
			inertiaTensor[1][2] = inertiaTensor[2][1] += mass * centerOfMass[1] * centerOfMass[2];
			inertiaTensor[2][0] = inertiaTensor[0][2] += mass * centerOfMass[2] * centerOfMass[0];*/
		}
		#endregion

		#region Private
		private void InitBox()
		{
			_type = TraceModelType.Box;

			_vertices = new Vector3[8];
			_edges = new TraceModelEdge[12];
			_polygons = new TraceModelPolygon[6];

			for(int i = 0; i < _edges.Length; i++)
			{
				_edges[i].V = new int[2];
			}

			// set box edges
			for(int i = 0; i < 4; i++)
			{
				_edges[i + 1].V[0] = i;
				_edges[i + 1].V[1] = (i + 1) & 3;
				_edges[i + 5].V[0] = 4 + i;
				_edges[i + 5].V[1] = 4 + ((i + 1) & 3);
				_edges[i + 9].V[0] = i;
				_edges[i + 9].V[1] = 4 + i;
			}

			// all edges of a polygon go counter clockwise
			_polygons[0].Edges = new int[4];
			_polygons[0].Edges[0] = -4;
			_polygons[0].Edges[1] = -3;
			_polygons[0].Edges[2] = -2;
			_polygons[0].Edges[3] = -1;
			_polygons[0].Normal = new Vector3(0, 0, -1);

			_polygons[1].Edges = new int[4];
			_polygons[1].Edges[0] = 5;
			_polygons[1].Edges[1] = 6;
			_polygons[1].Edges[2] = 7;
			_polygons[1].Edges[3] = 8;
			_polygons[1].Normal = new Vector3(0, 0, -1);

			_polygons[2].Edges = new int[4];
			_polygons[2].Edges[0] = 1;
			_polygons[2].Edges[1] = 10;
			_polygons[2].Edges[2] = -5;
			_polygons[2].Edges[3] = -9;
			_polygons[2].Normal = new Vector3(0, -1, 0);

			_polygons[3].Edges = new int[4];
			_polygons[3].Edges[0] = 2;
			_polygons[3].Edges[1] = 11;
			_polygons[3].Edges[2] = -6;
			_polygons[3].Edges[3] = -10;
			_polygons[3].Normal = new Vector3(1, 0, 0);

			_polygons[4].Edges = new int[4];
			_polygons[4].Edges[0] = 3;
			_polygons[4].Edges[1] = 12;
			_polygons[4].Edges[2] = -7;
			_polygons[4].Edges[3] = -11;
			_polygons[4].Normal = new Vector3(0, 1, 0);

			_polygons[5].Edges = new int[4];
			_polygons[5].Edges[0] = 4;
			_polygons[5].Edges[1] = 9;
			_polygons[5].Edges[2] = -8;
			_polygons[5].Edges[3] = -12;
			_polygons[5].Normal = new Vector3(-1, 0, 0);

			// convex model
			_isConvex = true;

			GenerateEdgeNormals();
		}

		private void SetupBox(idBounds bounds)
		{
			if(_type != TraceModelType.Box)
			{
				InitBox();
			}

			// offset to center
			_offset = (bounds.Min + bounds.Max) * 0.5f;

			// set box vertices
			for(int i = 0; i < 8; i++)
			{
				_vertices[i].X = (((i ^ (i >> 1)) & 1) == 0) ? bounds.Min.X : bounds.Max.X;
				_vertices[i].Y = (((i >> 1) & 1) == 0) ? bounds.Min.Y : bounds.Max.Y;
				_vertices[i].Z = (((i >> 2) & 1) == 0) ? bounds.Min.Z : bounds.Max.Z;
			}

			// set polygon plane distances
			_polygons[0].Distance = -bounds.Min.Z;
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

		private int GenerateEdgeNormals()
		{
			float sharpEdgeDot = -0.7f;

			for(int i = 0; i < _edges.Length; i++)
			{
				_edges[i].Normal = Vector3.Zero;
			}

			int sharpEdgeCount = 0;

			for(int i = 0; i < _polygons.Length; i++)
			{
				TraceModelPolygon poly = _polygons[i];

				for(int j = 0; j < poly.Edges.Length; j++)
				{
					int edgeCount = poly.Edges[j];
					TraceModelEdge edge = _edges[(int) idMath.Abs(edgeCount)];

					if((edge.Normal.X == 0.0f) && (edge.Normal.X == 0.0f) && (edge.Normal.Z == 0.0f))
					{
						edge.Normal = poly.Normal;
					}
					else
					{
						float dot = Vector3.Dot(edge.Normal, poly.Normal);

						// if the two planes make a very sharp edge
						if(dot < sharpEdgeDot)
						{
							// max length normal pointing outside both polygons
							Vector3 direction = _vertices[edge.V[(edgeCount > 0) ? 1 : 0]] - _vertices[edge.V[(edgeCount < 0) ? 1 : 0]];

							edge.Normal = Vector3.Cross(edge.Normal, direction) + Vector3.Cross(poly.Normal, -direction);
							edge.Normal *= (0.5f / (0.5f + 0.5f * sharpEdgeDot)) / edge.Normal.Length();

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
		#endregion

		#region Static
		private static int GetTraceModelHashKey(idTraceModel traceModel)
		{
			Vector3 v = traceModel.Bounds.Min;

			return (((int) traceModel._type << 8) ^ (traceModel._vertices.Length << 4) ^ (traceModel._edges.Length << 2) ^ (traceModel._polygons.Length << 0) ^ idMath.VectorHash(v));
		}

		private static int AllocateTraceModel(idTraceModel traceModel)
		{
			int traceModelIndex;
			int hashKey = GetTraceModelHashKey(traceModel);

			if(_traceModelHashCache.ContainsKey(hashKey) == true)
			{
				traceModelIndex = _traceModelHashCache[hashKey];
				_traceModelCache[traceModelIndex].ReferenceCount++;

				return traceModelIndex;
			}

			TraceModelCache entry = new TraceModelCache();
			entry.TraceModel = traceModel;
			entry.TraceModel.GetMassProperties(1.0f, out entry.Volume, out entry.CenterOfMass, out entry.InertiaTensor);
			entry.ReferenceCount = 1;

			traceModelIndex = _traceModelCache.Count;

			_traceModelCache.Add(entry);
			_traceModelHashCache.Add(hashKey, traceModelIndex);

			return traceModelIndex;
		}
		#endregion
		#endregion
	}
}