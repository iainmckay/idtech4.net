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
		#region Constants
		public const int MaxEdges = 32;
		public const int MaxPolygons = 16;
		public const int MaxVertices = 32;
		#endregion

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

		public idTraceModel(idBounds bounds, int sideCount)
		{
			SetupCylinder(bounds, sideCount);
		}
		#endregion

		#region Methods
		#region Public
		public idTraceModel Copy()
		{
			idTraceModel traceModel = new idTraceModel();
			traceModel._type = _type;
			traceModel._bounds = _bounds;
			traceModel._offset = _offset;
			traceModel._edges = (TraceModelEdge[]) _edges.Clone();

			traceModel._polygons = (TraceModelPolygon[]) _polygons.Clone();
			traceModel._vertices = (Vector3[]) _vertices.Clone();

			traceModel._isConvex = _isConvex;

			return traceModel;
		}

		public void GetMassProperties(float density, out float mass, out Vector3 centerOfMass, out Matrix inertiaTensor)
		{
			inertiaTensor = Matrix.Identity;

			// if polygon trace model
			if(_type == TraceModelType.Polygon)
			{
				idTraceModel traceModel = VolumeFromPolygon(1.0f);
				traceModel.GetMassProperties(density, out mass, out centerOfMass, out inertiaTensor);
			}
			else
			{
				VolumeIntegrals integrals = GetVolumeIntegrals();

				// if no volume
				if(integrals.T0 == 0.0f)
				{
					mass = 1.0f;
					centerOfMass = Vector3.Zero;
					inertiaTensor = Matrix.Identity;
				}
				else
				{
					// mass of model
					mass = density * integrals.T0;

					// center of mass
					centerOfMass = integrals.T1 / integrals.T0;

					// compute inertia tensor
					inertiaTensor.M11 = density * (integrals.T2.Y + integrals.T2.Z);
					inertiaTensor.M22 = density * (integrals.T2.Z + integrals.T2.X);
					inertiaTensor.M33 = density * (integrals.T2.X + integrals.T2.Y);
					inertiaTensor.M12 = inertiaTensor.M21 = -density * integrals.TP.X;
					inertiaTensor.M23 = inertiaTensor.M32 = -density * integrals.TP.Y;
					inertiaTensor.M31 = inertiaTensor.M13 = -density * integrals.TP.Z;

					// translate inertia tensor to center of mass
					inertiaTensor.M11 -= mass * (centerOfMass.Y * centerOfMass.Y + centerOfMass.Z * centerOfMass.Z);
					inertiaTensor.M22 -= mass * (centerOfMass.Z * centerOfMass.Z + centerOfMass.X * centerOfMass.X);
					inertiaTensor.M33 -= mass * (centerOfMass.X * centerOfMass.X + centerOfMass.Y * centerOfMass.Y);
					inertiaTensor.M12 = inertiaTensor.M21 += mass * centerOfMass.X * centerOfMass.Y;
					inertiaTensor.M23 = inertiaTensor.M32 += mass * centerOfMass.Y * centerOfMass.Z;
					inertiaTensor.M31 = inertiaTensor.M13 += mass * centerOfMass.Z * centerOfMass.X;
				}
			}
		}

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
				_polygons[i].Edges = new int[0];
			}

			_polygons[0].Bounds.Max.Z = bounds.Min.Z;
			_polygons[1].Bounds.Min.Z = bounds.Max.Z;
			_polygons[2].Bounds.Max.Y = bounds.Min.Y;
			_polygons[3].Bounds.Min.X = bounds.Max.X;
			_polygons[4].Bounds.Min.Y = bounds.Max.Y;
			_polygons[5].Bounds.Max.X = bounds.Min.X;

			_bounds = bounds;
		}

		public void SetupCylinder(idBounds bounds, int sideCount)
		{
			int n = sideCount;

			if(n < 3)
			{
				n = 3;
			}

			if((n * 2) > MaxVertices)
			{
				idConsole.WriteLine("WARNING: idTraceModel::SetupCylinder: too many vertices");
				n = MaxVertices / 2;
			}

			if((n * 3) > MaxEdges)
			{
				idConsole.WriteLine("WARNING: idTraceModel::SetupCylinder: too many sides");
				n = MaxEdges / 3;
			}

			if((n + 2) > MaxPolygons)
			{
				idConsole.WriteLine("WARNING: idTraceModel::SetupCylinder: too many polygons");
				n = MaxPolygons - 2;
			}

			_type = TraceModelType.Cylinder;

			_vertices = new Vector3[n * 2];
			_edges = new TraceModelEdge[n * 3];
			_polygons = new TraceModelPolygon[n + 2];

			_offset = (bounds.Min + bounds.Max) * 0.5f;

			Vector3 halfSize = bounds.Max - _offset;

			for(int i = 0; i < n; i++)
			{
				// verts
				float angle = idMath.TwoPi * i / n;

				_vertices[i].X = idMath.Cos(angle) * halfSize.X + _offset.X;
				_vertices[i].Y = idMath.Sin(angle) * halfSize.Y + _offset.Y;
				_vertices[i].Z = -halfSize.Z + _offset.Z;

				_vertices[n + i].X = _vertices[i].X;
				_vertices[n + i].Y = _vertices[i].Y;
				_vertices[n + i].Z = halfSize.Z + _offset.Z;

				// edges
				int ii = i + 1;
				int n2 = n << 1;

				_edges[ii].V[0] = i;
				_edges[ii].V[1] = ii % n;
				_edges[n + ii].V[0] = _edges[ii].V[0] + n;
				_edges[n + ii].V[1] = _edges[ii].V[1] + n;
				_edges[n2 + ii].V[0] = i;
				_edges[n2 + ii].V[1] = n + i;

				// vertical polygon edges
				_polygons[i].Edges = new int[4];
				_polygons[i].Edges[0] = ii;
				_polygons[i].Edges[1] = n2 + (ii % n) + 1;
				_polygons[i].Edges[2] = -(n + ii);
				_polygons[i].Edges[3] = -(n2 + ii);

				// bottom and top polygon edges
				_polygons[n].Edges[i] = -(n - i);
				_polygons[n + 1].Edges[i] = n + ii;
			}

			// bottom and top polygon numEdges
			_polygons[n].Edges = new int[n];
			_polygons[n + 1].Edges = new int[n];

			// polygons
			for(int i = 0; i < n; i++)
			{
				// vertical polygon plane
				_polygons[i].Normal = Vector3.Cross(_vertices[(i + 1) % n] - _vertices[i], _vertices[n + i] - _vertices[i]);
				_polygons[i].Normal.Normalize();

				// vertical polygon bounds
				_polygons[i].Bounds.Clear();
				_polygons[i].Bounds.AddPoint(_vertices[i]);
				_polygons[i].Bounds.AddPoint(_vertices[(i + 1) % n]);
				_polygons[i].Bounds.Min.Z = -halfSize.Z + _offset.Z;
				_polygons[i].Bounds.Max.Z = halfSize.Z + _offset.Z;
			}

			// bottom and top polygon plane
			_polygons[n].Normal = new Vector3(0, 0, -1.0f);
			_polygons[n].Distance = -bounds.Min.Z;
			_polygons[n + 1].Normal = new Vector3(0, 0, 1.0f);
			_polygons[n + 1].Distance = bounds.Max.Z;

			// trm bounds
			_bounds = bounds;

			// bottom and top polygon bounds
			_polygons[n].Bounds = bounds;
			_polygons[n].Bounds.Max.Z = bounds.Min.Z;
			_polygons[n + 1].Bounds = bounds;
			_polygons[n + 1].Bounds.Min.Z = bounds.Max.Z;

			// convex model
			_isConvex = true;
			
			GenerateEdgeNormals();
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

		private PolygonIntegrals GetPolygonIntegrals(int polyNumber, int a, int b, int c)
		{
			ProjectionIntegrals pi = GetProjectionIntegrals(polyNumber, a, b);

			Vector3 n = _polygons[polyNumber].Normal;
			float w = -_polygons[polyNumber].Distance;
			float k1 = 1.0f / n.Get(c);
			float k2 = k1 * k1;
			float k3 = k2 * k1;
			float k4 = k3 * k1;

			PolygonIntegrals integrals = new PolygonIntegrals();
			integrals.Fa = k1 * pi.Pa;
			integrals.Fb = k1 * pi.Pb;
			integrals.Fc = -k2 * (n.Get(a) * pi.Pa + n.Get(b) * pi.Pb + w * pi.P1);
			
			integrals.Faa = k1 * pi.Paa;
			integrals.Fbb = k1 * pi.Pbb;
			integrals.Fcc = k3 * (idMath.Square(n.Get(a)) * pi.Paa + 2 * n.Get(a) * n.Get(b) * pi.Pab + idMath.Square(n.Get(b)) * pi.Pbb
				+ w * (2 * (n.Get(a) * pi.Pa + n.Get(b) * pi.Pb) + w * pi.P1));

			integrals.Faaa = k1 * pi.Paaa;
			integrals.Fbbb = k1 * pi.Pbbb;
			integrals.Fccc = -k4 * (idMath.Cube(n.Get(a)) * pi.Paaa + 3 * idMath.Square(n.Get(a)) * n.Get(b) * pi.Paab
					+ 3 * n.Get(a) * idMath.Square(n.Get(b)) * pi.Pabb + idMath.Cube(n.Get(b)) * pi.Pbbb
					+ 3 * w * (idMath.Square(n.Get(a)) * pi.Paa + 2 * n.Get(a) * n.Get(b) * pi.Pab + idMath.Square(n.Get(b)) * pi.Pbb)
					+ w * w * (3 * (n.Get(a) * pi.Pa + n.Get(b) * pi.Pb) + w * pi.P1));

			integrals.Faab = k1 * pi.Paab;
			integrals.Fbbc = -k2 * (n.Get(a) * pi.Pabb + n.Get(b) * pi.Pbbb + w * pi.Pbb);
			integrals.Fcca = k3 * (idMath.Square(n.Get(a)) * pi.Paaa + 2 * n.Get(a) * n.Get(b) * pi.Paab + idMath.Square(n.Get(b)) * pi.Pabb
					+ w * (2 * (n.Get(a) * pi.Paa + n.Get(b) * pi.Pab) + w * pi.Pa));

			return integrals;
		}

		private ProjectionIntegrals GetProjectionIntegrals(int polyNumber, int a, int b)
		{
			ProjectionIntegrals integrals = new ProjectionIntegrals();
			TraceModelPolygon polygon = _polygons[polyNumber];
			int count = polygon.Edges.Length;

			for(int i = 0; i < count; i++)
			{
				int edgeNumber = polygon.Edges[i];

				Vector3 v1 = _vertices[_edges[(int) idMath.Abs(edgeNumber)].V[(edgeNumber < 0) ? 1 : 0]];
				Vector3 v2 = _vertices[_edges[(int) idMath.Abs(edgeNumber)].V[(edgeNumber > 0) ? 1 : 0]];

				float a0 = v1.Get(a);
				float b0 = v1.Get(b);
				float a1 = v2.Get(a);
				float b1 = v2.Get(b);
				float da = a1 - a0;
				float db = b1 - b0;
				float a0_2 = a0 * a0;
				float a0_3 = a0_2 * a0;
				float a0_4 = a0_3 * a0;
				float b0_2 = b0 * b0;
				float b0_3 = b0_2 * b0;
				float b0_4 = b0_3 * b0;
				float a1_2 = a1 * a1;
				float a1_3 = a1_2 * a1;
				float b1_2 = b1 * b1;
				float b1_3 = b1_2 * b1;

				float C1 = a1 + a0;
				float Ca = a1 * C1 + a0_2;
				float Caa = a1 * Ca + a0_3;
				float Caaa = a1 * Caa + a0_4;
				float Cb = b1 * (b1 + b0) + b0_2;
				float Cbb = b1 * Cb + b0_3;
				float Cbbb = b1 * Cbb + b0_4;
				float Cab = 3 * a1_2 + 2 * a1 * a0 + a0_2;
				float Kab = a1_2 + 2 * a1 * a0 + 3 * a0_2;
				float Caab = a0 * Cab + 4 * a1_3;
				float Kaab = a1 * Kab + 4 * a0_3;
				float Cabb = 4 * b1_3 + 3 * b1_2 * b0 + 2 * b1 * b0_2 + b0_3;
				float Kabb = b1_3 + 2 * b1_2 * b0 + 3 * b1 * b0_2 + 4 * b0_3;

				integrals.P1 += db * C1;
				integrals.Pa += db * Ca;
				integrals.Paa += db * Caa;
				integrals.Paaa += db * Caaa;
				integrals.Pb += da * Cb;
				integrals.Pbb += da * Cbb;
				integrals.Pbbb += da * Cbbb;
				integrals.Pab += db * (b1 * Cab + b0 * Kab);
				integrals.Paab += db * (b1 * Caab + b0 * Kaab);
				integrals.Pabb += da * (a1 * Cabb + a0 * Kabb);
			}

			integrals.P1 *= (1.0f / 2.0f);
			integrals.Pa *= (1.0f / 6.0f);
			integrals.Paa *= (1.0f / 12.0f);
			integrals.Paaa *= (1.0f / 20.0f);
			integrals.Pb *= (1.0f / -6.0f);
			integrals.Pbb *= (1.0f / -12.0f);
			integrals.Pbbb *= (1.0f / -20.0f);
			integrals.Pab *= (1.0f / 24.0f);
			integrals.Paab *= (1.0f / 60.0f);
			integrals.Pabb *= (1.0f / -60.0f);

			return integrals;
		}

		private VolumeIntegrals GetVolumeIntegrals()
		{
			VolumeIntegrals integrals = new VolumeIntegrals();
			int polyCount = _polygons.Length;

			for(int i = 0; i < polyCount; i++)
			{
				TraceModelPolygon poly = _polygons[i];

				float nx = idMath.Abs(poly.Normal.X);
				float ny = idMath.Abs(poly.Normal.Y);
				float nz = idMath.Abs(poly.Normal.Z);
				int c = 0;

				if((nx > ny) && (nx > nz))
				{
					c = 0;
				}
				else
				{
					c = (ny > nz) ? 1 : 2;
				}

				int a = (c + 1) % 3;
				int b = (a + 1) % 3;

				PolygonIntegrals pi = GetPolygonIntegrals(i, a, b, c);

				integrals.T0 += poly.Normal.X * ((a == 0) ? pi.Fa : ((b == 0) ? pi.Fb : pi.Fc));

				if(a == 0)
				{
					integrals.T1.X += poly.Normal.X * pi.Faa;
					integrals.T2.X += poly.Normal.X * pi.Faaa;
					integrals.TP.X += poly.Normal.X * pi.Faab;
				}
				else if(a == 1)
				{
					integrals.T1.Y += poly.Normal.Y * pi.Faa;
					integrals.T2.Y += poly.Normal.Y * pi.Faaa;
					integrals.TP.Y += poly.Normal.Y * pi.Faab;
				}
				else if(a == 2)
				{
					integrals.T1.Z += poly.Normal.Z * pi.Faa;
					integrals.T2.Z += poly.Normal.Z * pi.Faaa;
					integrals.TP.Z += poly.Normal.Z * pi.Faab;
				}

				if(b == 0)
				{
					integrals.T1.X += poly.Normal.X * pi.Fbb;
					integrals.T2.X += poly.Normal.X * pi.Fbbb;
					integrals.TP.X += poly.Normal.X * pi.Fbbc;
				}
				else if(b == 1)
				{
					integrals.T1.Y += poly.Normal.Y * pi.Fbb;
					integrals.T2.Y += poly.Normal.Y * pi.Fbbb;
					integrals.TP.Y += poly.Normal.Y * pi.Fbbc;
				}
				else if(b == 2)
				{
					integrals.T1.Z += poly.Normal.Z * pi.Fbb;
					integrals.T2.Z += poly.Normal.Z * pi.Fbbb;
					integrals.TP.Z += poly.Normal.Z * pi.Fbbc;
				}

				if(c == 0)
				{
					integrals.T1.X += poly.Normal.X * pi.Fcc;
					integrals.T2.X += poly.Normal.X * pi.Fccc;
					integrals.TP.X += poly.Normal.X * pi.Fcca;
				}
				else if(c == 1)
				{
					integrals.T1.Y += poly.Normal.Y * pi.Fcc;
					integrals.T2.Y += poly.Normal.Y * pi.Fccc;
					integrals.TP.Y += poly.Normal.Y * pi.Fcca;
				}
				else if(c == 2)
				{
					integrals.T1.Z += poly.Normal.Z * pi.Fcc;
					integrals.T2.Z += poly.Normal.Z * pi.Fccc;
					integrals.TP.Z += poly.Normal.Z * pi.Fcca;
				}
			}

			integrals.T1 *= 0.5f;
			integrals.T2 *= (1.0f / 3.0f);
			integrals.TP *= 0.5f;

			return integrals;
		}

		private idTraceModel VolumeFromPolygon(float thickness)
		{
			idTraceModel traceModel = this.Copy();
			traceModel._type = TraceModelType.PolygonVolume;
			traceModel._vertices = new Vector3[_vertices.Length * 2];
			traceModel._edges = new TraceModelEdge[_edges.Length * 3];
			traceModel._polygons = new TraceModelPolygon[_edges.Length + 2];

			int edgeCount = _edges.Length;
			int vertCount = _vertices.Length;

			for(int i = 0; i < edgeCount; i++)
			{
				traceModel._vertices[vertCount + i] = _vertices[i] - thickness * _polygons[0].Normal;

				traceModel._edges[edgeCount + i + 1].V[0] = vertCount + i;
				traceModel._edges[edgeCount + i + 1].V[1] = vertCount + (i + 1) % vertCount;
				traceModel._edges[edgeCount * 2 + i + 1].V[0] = i;
				traceModel._edges[edgeCount * 2 + i + 1].V[1] = vertCount + i;

				traceModel._polygons[1].Edges[i] = -(edgeCount + i + 1);
				traceModel._polygons[2 + i].Edges = new int[4];
				traceModel._polygons[2 + i].Edges[0] = -(i + 1);
				traceModel._polygons[2 + i].Edges[1] = edgeCount * 2 + i + 1;
				traceModel._polygons[2 + i].Edges[2] = edgeCount + i + 1;
				traceModel._polygons[2 + i].Edges[3] = -(edgeCount * 2 + (i + 1) % edgeCount + 1);
				traceModel._polygons[2 + i].Normal = Vector3.Cross(_vertices[(i + 1) % vertCount] - _vertices[i], _polygons[0].Normal);
				traceModel._polygons[2 + i].Normal.Normalize();
				traceModel._polygons[2 + i].Distance = (traceModel._polygons[2 + i].Normal * _vertices[i]).Length();
			}

			traceModel._polygons[1].Distance = (traceModel._polygons[1].Normal * traceModel._vertices[edgeCount]).Length();
			traceModel.GenerateEdgeNormals();

			return traceModel;
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

		#region VolumeIntegrals
		private struct VolumeIntegrals
		{
			public float T0;
			public Vector3 T1;
			public Vector3 T2;
			public Vector3 TP;
		}
		#endregion

		#region ProjectionIntegrals
		private struct ProjectionIntegrals
		{
			public float P1;
			public float Pa;
			public float Pb;

			public float Paa;
			public float Pab;
			public float Pbb;

			public float Paaa;
			public float Paab;
			public float Pabb;
			public float Pbbb;
		}
		#endregion

		#region PolygonIntegrals
		private struct PolygonIntegrals
		{
			public float Fa;
			public float Fb;
			public float Fc;

			public float Faa;
			public float Fbb;
			public float Fcc;

			public float Faaa;
			public float Fbbb;
			public float Fccc;

			public float Faab;
			public float Fbbc;
			public float Fcca;
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