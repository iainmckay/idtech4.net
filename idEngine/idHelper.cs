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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.Xna.Framework;

using idTech4;
using idTech4.Geometry;
using idTech4.Math;
using idTech4.Renderer;

using View = idTech4.Renderer.View;

namespace idTech4
{
	public static class idHelper
	{
		public static idAngles AxisToAngles(Matrix axis)
		{
			idAngles angles = new idAngles();

			float sp = axis.M13;

			// cap off our sin value so that we don't get any NANs

			if(sp > 1.0f)
			{
				sp = 1.0f;
			}
			else if(sp < -1.0f)
			{
				sp = -1.0f;
			}

			double theta = -System.Math.Asin(sp);
			double cp = System.Math.Cos(theta);

			if(cp > 8192.0f * idMath.Epsilon)
			{
				angles.Pitch = MathHelper.ToDegrees((float) theta);
				angles.Yaw = MathHelper.ToDegrees((float) System.Math.Atan2(axis.M12, axis.M11));
				angles.Roll = MathHelper.ToDegrees((float) System.Math.Atan2(axis.M23, axis.M33));
			}
			else
			{
				angles.Pitch = MathHelper.ToDegrees((float) theta);
				angles.Yaw = MathHelper.ToDegrees((float) -System.Math.Atan2(axis.M21, axis.M22));
				angles.Roll = 0;
			}

			return angles;
		}

		public static Matrix AxisToModelMatrix(Matrix axis, Vector3 origin)
		{
			Matrix modelMatrix = new Matrix();
			modelMatrix.M11 = axis.M11;
			modelMatrix.M21 = axis.M21;
			modelMatrix.M31 = axis.M31;
			modelMatrix.M41 = origin.X;

			modelMatrix.M12 = axis.M12;
			modelMatrix.M22 = axis.M22;
			modelMatrix.M32 = axis.M32;
			modelMatrix.M42 = origin.Y;

			modelMatrix.M13 = axis.M13;
			modelMatrix.M23 = axis.M23;
			modelMatrix.M33 = axis.M33;
			modelMatrix.M43 = origin.Z;
			
			modelMatrix.M14 = 0;
			modelMatrix.M24 = 0;
			modelMatrix.M34 = 0;
			modelMatrix.M44 = 1;

			return modelMatrix;
		}

		public static float CalculateW(Quaternion q)
		{
			// take the absolute value because floating point rounding may cause the dot of x,y,z to be larger than 1
			return idMath.Sqrt(idMath.Abs(1.0f - (q.X * q.X + q.Y * q.Y + q.Z * q.Z)));
		}

		public static char CharacterFromKeyCode(Keys key, Keys modifiers)
		{
			char c = '\0';

			if((key >= Keys.D0) && (key <= Keys.Z))
			{
				c = (char) key;

				if((modifiers & Keys.Shift) == 0)
				{
					c = Char.ToLower(c);
				}
				else
				{
					switch(key)
					{
						case Keys.D0:
							return ')';   
						case Keys.D1:
							return '!';
						case Keys.D2:
							return '"';
						case Keys.D3:
							return '£';
						case Keys.D4:
							return '$';
						case Keys.D5:
							return '%';
						case Keys.D6:
							return '^';
						case Keys.D7:
							return '&';
						case Keys.D8:
							return '*';
						case Keys.D9:
							return '(';
					}
				}
			}
			else if((modifiers & Keys.Shift) == Keys.Shift)
			{
				switch(key)
				{
					case Keys.OemOpenBrackets:
						return '[';
					case Keys.OemCloseBrackets:
						return ']';
					case Keys.OemSemicolon:
						return ';';
					case Keys.OemQuotes:
						return '#';
					case Keys.Oemtilde:
						return '\'';
					case Keys.OemPeriod:
						return '.';
					case Keys.Oemcomma:
						return ',';
					case Keys.OemQuestion:
						return '/';
					case Keys.OemMinus:
						return '-';
					case Keys.Oemplus:
						return '=';
					case Keys.Oem5:
						return '\\';
					case Keys.Oem8:
						return '`';

					case Keys.NumPad0:
						return '0';
					case Keys.NumPad1:
						return '1';
					case Keys.NumPad2:
						return '2';
					case Keys.NumPad3:
						return '3';
					case Keys.NumPad4:
						return '4';
					case Keys.NumPad5:
						return '5';
					case Keys.NumPad6:
						return '6';
					case Keys.NumPad7:
						return '7';
					case Keys.NumPad8:
						return '8';
					case Keys.NumPad9:
						return '9';
				}
			}
			else
			{
				switch(key)
				{
					case Keys.OemOpenBrackets:
						return '{';
					case Keys.OemCloseBrackets:
						return '}';
					case Keys.OemSemicolon:
						return ':';
					case Keys.OemQuotes:
						return '~';
					case Keys.Oemtilde:
						return '@';
					case Keys.OemPeriod:
						return '>';
					case Keys.Oemcomma:
						return '<';
					case Keys.OemQuestion:
						return '?';
					case Keys.OemMinus:
						return '_';
					case Keys.Oemplus:
						return '+';
					case Keys.Oem5:
						return '|';
					case Keys.Oem8:
						return '¬';
				}
			}

			switch(key)
			{
				case Keys.Multiply:
					return '*';
				case Keys.Divide:
					return '/';
				case Keys.Add:
					return '+';
				case Keys.Subtract:
					return '-';
				case Keys.Decimal:
					return '.';
				case Keys.Space:
					return ' ';
			}

			return c;
		}

		public static bool CharacterIsPrintable(char c)
		{
			// test for regular ascii and western European high-ascii chars
			return (((c >= 0x20) && (c <= 0x7E)) || ((c >= 0xA1) && (c <= 0xFF)));
		}

		public static Vector4 ColorForIndex(int index)
		{
			idColorIndex tmp;

			if(Enum.TryParse<idColorIndex>(index.ToString(), out tmp) == true)
			{
				switch(tmp)
				{
					case idColorIndex.Red:
						return idColor.Red;

					case idColorIndex.Green:
						return idColor.Green;

					case idColorIndex.Yellow:
						return idColor.Yellow;

					case idColorIndex.Blue:
						return idColor.Blue;

					case idColorIndex.Cyan:
						return idColor.Cyan;

					case idColorIndex.Magenta:
						return idColor.Magenta;
				
					case idColorIndex.White:
						return idColor.White;

					case idColorIndex.Gray:
						return idColor.Grey;

					case idColorIndex.Black:
						return idColor.Black;
				}
			}

			return idColor.White;
		}

		public static int ColorIndex(idColorIndex color)
		{
			return ((int) color & 15);
		}

		public static void ComputeAxisBase(Vector3 normal, out Vector3 texS, out Vector3 texT)
		{
			// do some cleaning
			Vector3 n = new Vector3(
				(idMath.Abs(normal.X) < 1e-6f) ? 0.0f : normal.X,
				(idMath.Abs(normal.Y) < 1e-6f) ? 0.0f : normal.Y,
				(idMath.Abs(normal.Z) < 1e-6f) ? 0.0f : normal.Z
			);

			float rotY = (float) -System.Math.Atan2(n.Z, idMath.Sqrt(n.Y * n.Y + n.X * n.X));
			float rotZ = (float) System.Math.Atan2(n.Y, n.X);

			// rotate (0,1,0) and (0,0,1) to compute texS and texT
			texS = new Vector3(-idMath.Sin(rotZ), idMath.Cos(rotZ), 0);

			// the texT vector is along -Z ( T texture coorinates axis )
			texT = new Vector3(-idMath.Sin(rotY) * idMath.Cos(rotZ),
				-idMath.Sin(rotY) * idMath.Sin(rotZ),
				-idMath.Cos(rotY));
		}

		public static void ConvertMatrix(float[] a, float[] b, out Matrix m) 
		{
			m.M11 = a[0*4+0]*b[0*4+0] + a[0*4+1]*b[1*4+0] + a[0*4+2]*b[2*4+0] + a[0*4+3]*b[3*4+0];
			m.M12 = a[0*4+0]*b[0*4+1] + a[0*4+1]*b[1*4+1] + a[0*4+2]*b[2*4+1] + a[0*4+3]*b[3*4+1];
			m.M13 = a[0*4+0]*b[0*4+2] + a[0*4+1]*b[1*4+2] + a[0*4+2]*b[2*4+2] + a[0*4+3]*b[3*4+2];
			m.M14 = a[0*4+0]*b[0*4+3] + a[0*4+1]*b[1*4+3] + a[0*4+2]*b[2*4+3] + a[0*4+3]*b[3*4+3];
			
			m.M21 = a[1*4+0]*b[0*4+0] + a[1*4+1]*b[1*4+0] + a[1*4+2]*b[2*4+0] + a[1*4+3]*b[3*4+0];
			m.M22 = a[1*4+0]*b[0*4+1] + a[1*4+1]*b[1*4+1] + a[1*4+2]*b[2*4+1] + a[1*4+3]*b[3*4+1];
			m.M23 = a[1*4+0]*b[0*4+2] + a[1*4+1]*b[1*4+2] + a[1*4+2]*b[2*4+2] + a[1*4+3]*b[3*4+2];
			m.M24 = a[1*4+0]*b[0*4+3] + a[1*4+1]*b[1*4+3] + a[1*4+2]*b[2*4+3] + a[1*4+3]*b[3*4+3];
			
			m.M31 = a[2*4+0]*b[0*4+0] + a[2*4+1]*b[1*4+0] + a[2*4+2]*b[2*4+0] + a[2*4+3]*b[3*4+0];
			m.M32 = a[2*4+0]*b[0*4+1] + a[2*4+1]*b[1*4+1] + a[2*4+2]*b[2*4+1] + a[2*4+3]*b[3*4+1];
			m.M33 = a[2*4+0]*b[0*4+2] + a[2*4+1]*b[1*4+2] + a[2*4+2]*b[2*4+2] + a[2*4+3]*b[3*4+2];
			m.M34 = a[2*4+0]*b[0*4+3] + a[2*4+1]*b[1*4+3] + a[2*4+2]*b[2*4+3] + a[2*4+3]*b[3*4+3];
			
			m.M41 = a[3*4+0]*b[0*4+0] + a[3*4+1]*b[1*4+0] + a[3*4+2]*b[2*4+0] + a[3*4+3]*b[3*4+0];
			m.M42 = a[3*4+0]*b[0*4+1] + a[3*4+1]*b[1*4+1] + a[3*4+2]*b[2*4+1] + a[3*4+3]*b[3*4+1];
			m.M43 = a[3*4+0]*b[0*4+2] + a[3*4+1]*b[1*4+2] + a[3*4+2]*b[2*4+2] + a[3*4+3]*b[3*4+2];
			m.M44 = a[3*4+0]*b[0*4+3] + a[3*4+1]*b[1*4+3] + a[3*4+2]*b[2*4+3] + a[3*4+3]*b[3*4+3];
		}

		public static void ConvertJointQuaternionsToJointMatrices(idJointMatrix[] jointMatrices, idJointQuaternion[] jointQuaternions)
		{
			int count = jointMatrices.Length;

			for(int i = 0; i < count; i++)
			{
				jointMatrices[i].Rotation = jointQuaternions[i].Quaternion.ToMatrix();
				jointMatrices[i].Translation = jointQuaternions[i].Translation;
			}
		}

		public static void ConvertMatrix(Matrix a, Matrix b, out Matrix m)
		{
			m.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41;
			m.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42;
			m.M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43;
			m.M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44;

			m.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41;
			m.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42;
			m.M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43;
			m.M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44;

			m.M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41;
			m.M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42;
			m.M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43;
			m.M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44;

			m.M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41;
			m.M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42;
			m.M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43;
			m.M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44;
		}

		/// <summary>
		/// Tests all corners against the frustum.
		/// </summary>
		/// <remarks>
		/// Can still generate a few false positives when the box is outside a corner.
		/// </remarks>
		/// <param name="bounds"></param>
		/// <param name="modelMatrix"></param>
		/// <param name="planeCount"></param>
		/// <param name="planes"></param>
		/// <returns>Returns true if the box is outside the given global frustum, (positive sides are out).</returns>
		public static bool CornerCullLocalBox(idBounds bounds, Matrix modelMatrix, int planeCount, Plane[] planes)
		{

			// we can disable box culling for experimental timing purposes
			if(idE.CvarSystem.GetInteger("r_useCulling") < 2)
			{
				return false;
			}

			Vector3 v = Vector3.Zero;
			Vector3[] transformed = new Vector3[8];
			float[] distances = new float[8];
			int i, j;
			
			// transform into world space
			for(i = 0; i < 8; i++)
			{
				v.X = ((i & 1) == 0) ? bounds.Min.X : bounds.Max.X;
				v.Y = (((i >> 1) & 1) == 0) ? bounds.Min.Y : bounds.Max.Y;
				v.Z = (((i >> 2) & 1) == 0) ? bounds.Min.Z : bounds.Max.Z;

				LocalPointToGlobal(modelMatrix, v, out transformed[i]);
			}
			
			// check against frustum planes
			for(i = 0; i < planeCount; i++)
			{
				Plane frust = planes[i];

				for(j = 0; j < 8; j++)
				{
					distances[j] = frust.Distance(transformed[j]);

					if(distances[j] < 0)
					{
						break;
					}
				}

				if(j == 8)
				{
					// all points were behind one of the planes
					// TODO: tr.pc.c_box_cull_out++;
					return true;
				}
			}

			// TODO: tr.pc.c_box_cull_in++;

			return false;		// not culled
		}


		/// <summary>
		/// Performs quick test before expensive test.
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="modelMAtrix"></param>
		/// <param name="planeCount"></param>
		/// <param name="planes"></param>
		/// <returns>Returns true if the box is outside the given global frustum, (positive sides are out).</returns>
		public static bool CullLocalBox(idBounds bounds, Matrix modelMatrix, int planeCount, Plane[] planes)
		{
			if(RadiusCullLocalBox(bounds, modelMatrix, planeCount, planes) == true)
			{
				return true;
			}

			return CornerCullLocalBox(bounds, modelMatrix, planeCount, planes);
		}

		/// <summary>
		/// Converts from 24 frames per second to milliseconds.
		/// </summary>
		/// <param name="frameNumber"></param>
		/// <returns></returns>
		public static int FrameToTime(int frameNumber)
		{
			return (frameNumber * 1000) / 24;
		}

		/// <summary>
		/// -1 to 1 range in x, y, and z.
		/// </summary>
		/// <param name="global"></param>
		/// <param name="ndc"></param>
		public static void GlobalToNormalizedDeviceCoordinates(Vector3 global, out Vector3 ndc)
		{
			Plane view, clip;

			// _D3XP added work on primaryView when no viewDef
			if(idE.RenderSystem.ViewDefinition == null)
			{
				View primaryView = idE.RenderSystem.PrimaryView;
				
				view.Normal.X = global.X * primaryView.WorldSpace.ModelViewMatrix.M11 
									+ global.Y * primaryView.WorldSpace.ModelViewMatrix.M12
									+ global.Z * primaryView.WorldSpace.ModelViewMatrix.M13
									+ primaryView.WorldSpace.ModelViewMatrix.M14;
				
				view.Normal.Y = global.X * primaryView.WorldSpace.ModelViewMatrix.M21 
									+ global.Y * primaryView.WorldSpace.ModelViewMatrix.M22
									+ global.Z * primaryView.WorldSpace.ModelViewMatrix.M23
									+ primaryView.WorldSpace.ModelViewMatrix.M24;

				view.Normal.Z = global.X * primaryView.WorldSpace.ModelViewMatrix.M31 
									+ global.Y * primaryView.WorldSpace.ModelViewMatrix.M32
									+ global.Z * primaryView.WorldSpace.ModelViewMatrix.M33
									+ primaryView.WorldSpace.ModelViewMatrix.M34;

				view.D = global.X * primaryView.WorldSpace.ModelViewMatrix.M41 
									+ global.Y * primaryView.WorldSpace.ModelViewMatrix.M42
									+ global.Z * primaryView.WorldSpace.ModelViewMatrix.M43
									+ primaryView.WorldSpace.ModelViewMatrix.M44;

				clip.Normal.X = view.Normal.X * primaryView.WorldSpace.ModelViewMatrix.M11 
									+ view.Normal.Y * primaryView.WorldSpace.ModelViewMatrix.M12
									+ view.Normal.Z * primaryView.WorldSpace.ModelViewMatrix.M13
									+ primaryView.WorldSpace.ModelViewMatrix.M14;
				
				clip.Normal.Y = view.Normal.X * primaryView.WorldSpace.ModelViewMatrix.M21 
									+ view.Normal.Y * primaryView.WorldSpace.ModelViewMatrix.M22
									+ view.Normal.Z * primaryView.WorldSpace.ModelViewMatrix.M23
									+ view.D * primaryView.WorldSpace.ModelViewMatrix.M24;

				clip.Normal.Z = view.Normal.X * primaryView.WorldSpace.ModelViewMatrix.M31 
									+ view.Normal.Y * primaryView.WorldSpace.ModelViewMatrix.M32
									+ view.Normal.Z * primaryView.WorldSpace.ModelViewMatrix.M33
									+ view.D * primaryView.WorldSpace.ModelViewMatrix.M34;

				clip.D = view.Normal.X * primaryView.WorldSpace.ModelViewMatrix.M41 
									+ view.Normal.Y * primaryView.WorldSpace.ModelViewMatrix.M42
									+ view.Normal.Z * primaryView.WorldSpace.ModelViewMatrix.M43
									+ view.D * primaryView.WorldSpace.ModelViewMatrix.M44;
			} 
			else 
			{
				View viewDef = idE.RenderSystem.ViewDefinition;
				
				view.Normal.X = global.X * viewDef.WorldSpace.ModelViewMatrix.M11 
									+ global.Y * viewDef.WorldSpace.ModelViewMatrix.M12
									+ global.Z * viewDef.WorldSpace.ModelViewMatrix.M13
									+ viewDef.WorldSpace.ModelViewMatrix.M14;
				
				view.Normal.Y = global.X * viewDef.WorldSpace.ModelViewMatrix.M21 
									+ global.Y * viewDef.WorldSpace.ModelViewMatrix.M22
									+ global.Z * viewDef.WorldSpace.ModelViewMatrix.M23
									+ viewDef.WorldSpace.ModelViewMatrix.M24;

				view.Normal.Z = global.X * viewDef.WorldSpace.ModelViewMatrix.M31 
									+ global.Y * viewDef.WorldSpace.ModelViewMatrix.M32
									+ global.Z * viewDef.WorldSpace.ModelViewMatrix.M33
									+ viewDef.WorldSpace.ModelViewMatrix.M34;

				view.D = global.X * viewDef.WorldSpace.ModelViewMatrix.M41 
									+ global.Y * viewDef.WorldSpace.ModelViewMatrix.M42
									+ global.Z * viewDef.WorldSpace.ModelViewMatrix.M43
									+ viewDef.WorldSpace.ModelViewMatrix.M44;

				clip.Normal.X = view.Normal.X * viewDef.WorldSpace.ModelViewMatrix.M11 
									+ view.Normal.Y * viewDef.WorldSpace.ModelViewMatrix.M12
									+ view.Normal.Z * viewDef.WorldSpace.ModelViewMatrix.M13
									+ view.D * viewDef.WorldSpace.ModelViewMatrix.M14;
				
				clip.Normal.Y = view.Normal.X * viewDef.WorldSpace.ModelViewMatrix.M21 
									+ view.Normal.Y * viewDef.WorldSpace.ModelViewMatrix.M22
									+ view.Normal.Z * viewDef.WorldSpace.ModelViewMatrix.M23
									+ view.D * viewDef.WorldSpace.ModelViewMatrix.M24;

				clip.Normal.Z = view.Normal.X * viewDef.WorldSpace.ModelViewMatrix.M31 
									+ view.Normal.Y * viewDef.WorldSpace.ModelViewMatrix.M32
									+ view.Normal.Z * viewDef.WorldSpace.ModelViewMatrix.M33
									+ view.D * viewDef.WorldSpace.ModelViewMatrix.M34;

				clip.D = view.Normal.X * viewDef.WorldSpace.ModelViewMatrix.M41 
									+ view.Normal.Y * viewDef.WorldSpace.ModelViewMatrix.M42
									+ view.Normal.Z * viewDef.WorldSpace.ModelViewMatrix.M43
									+ view.D * viewDef.WorldSpace.ModelViewMatrix.M44;
			}

			ndc.X = clip.Normal.X / clip.D;
			ndc.Y = clip.Normal.Y / clip.D;
			ndc.Z = (clip.Normal.Z + clip.D) / (2 * clip.D);
		}

		public static void LocalPointToGlobal(Matrix m, Vector3 local, out Vector3 world)
		{
			world = Vector3.Zero;
			world.X = local.X * m.M11 + local.Y * m.M21 + local.Z * m.M31 + m.M41;
			world.Y = local.X * m.M12 + local.Y * m.M22 + local.Z * m.M32 + m.M42;
			world.Z = local.X * m.M13 + local.Y * m.M23 + local.Z * m.M33 + m.M43;
		}

		/// <summary>
		/// A fast, conservative center-to-corner culling test.
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="modelMatrix"></param>
		/// <param name="planeCount"></param>
		/// <param name="planes"></param>
		/// <returns>Returns true if the box is outside the given global frustum, (positive sides are out).</returns>
		public static bool RadiusCullLocalBox(idBounds bounds, Matrix modelMatrix, int planeCount, Plane[] planes)
		{
			if(idE.CvarSystem.GetInteger("r_useCulling") == 0)
			{
				return false;
			}

			// transform the surface bounds into world space
			Vector3 localOrigin = (bounds.Min + bounds.Max) * 0.5f;
			Vector3 worldOrigin;

			LocalPointToGlobal(modelMatrix, localOrigin, out worldOrigin);

			float worldRadius = (bounds.Min - localOrigin).Length(); // FIXME: won't be correct for scaled objects
			
			for(int i = 0; i < planeCount; i++)
			{
				Plane frust = planes[i];
				float d = frust.Distance(worldOrigin);

				if(d > worldRadius)
				{
					return true; // culled
				}
			}
	
			return false;		// no culled
		}

		public static T[] Flatten<T>(T[,] source)
		{
			int d1 = source.GetUpperBound(0) + 1;
			int d2 = source.GetUpperBound(1) + 1;

			T[] flat = new T[d1 * d2];
			
			for(int y = 0; y < d1; y++)
			{
				for(int x = 0; x < d2; x++)
				{
					flat[(y * d2) + x] = source[y, x];
				}
			}

			return flat;
		}

		public static T[] Flatten<T>(T[,,] source)
		{
			int d1 = source.GetUpperBound(0) + 1;
			int d2 = source.GetUpperBound(1) + 1;
			int d3 = source.GetUpperBound(2) + 1;

			T[] flat = new T[d1 * d2 * d3];
			
			for(int x = 0; x < d1; x++)
			{
				for(int y = 0; y < d2; y++)
				{
					for(int z = 0; z < d3; z++)
					{
						flat[y + d1 * (x + d2 * z)] = source[x, y, z];
					}
				}
			}

			return flat;
		}

		public static char GetBufferCharacter(string buffer, int position)
		{
			if((position < 0) || (position >= buffer.Length))
			{
				return '\0';
			}

			return buffer[position];
		}

		public static bool IsColor(string buffer, int index)
		{
			if((index + 1) >= buffer.Length)
			{
				return false;
			}

			return ((buffer[index] == (int) idColorIndex.Escape) && (buffer[index + 1] != '\0') && (buffer[index + 1] != ' '));
		}

		public static int MakePowerOfTwo(int num)
		{
			int pot = 0;

			for(pot = 1; pot < num; pot <<= 1)
			{

			}

			return pot;
		}

		public static void NormalVectors(Vector3 n, ref Vector3 left, ref Vector3 down)
		{
			float d = n.X * n.X + n.Y * n.Y;

			if(d == 0)
			{
				left = new Vector3(1, 0, 0);
			}
			else
			{
				d = idMath.InvSqrt(d);
				left = new Vector3(-n.Y * d, n.X * d, 0);
			}

			down = Vector3.Cross(left, n);
		}

		public static void BoundTriangleSurface(Surface surf)
		{
			MinMax(ref surf.Bounds.Min, ref surf.Bounds.Max, surf.Vertices, surf.Vertices.Length);
		}

		public static void MinMax(ref Vector3 min, ref Vector3 max, Vertex[] src, int count)
		{
			min = new Vector3(idMath.Infinity, idMath.Infinity, idMath.Infinity);
			max = new Vector3(-idMath.Infinity, -idMath.Infinity, -idMath.Infinity);

			for(int i = 0; i < count; i++)
			{
				Vector3 v = src[i].Position;

				if(v.X < min.X)
				{
					min.X = v.X;
				}

				if(v.X > max.X)
				{
					max.X = v.X;
				}

				if(v.Y < min.Y)
				{
					min.Y = v.Y;
				}

				if(v.Y > max.Y)
				{
					max.Y = v.Y;
				}

				if(v.Z < min.Z)
				{
					min.Z = v.Z;
				}

				if(v.Z > max.Z)
				{
					max.Z = v.Z;
				}
			}
		}

		public static idRectangle ParseRectangle(string str)
		{
			try
			{
				string[] parts = null;

				if(str.Contains(",") == true)
				{
					parts = str.Replace(" ", "").Split(',');
				}
				else
				{
					parts = str.Split(' ');
				}

				if(parts.Length == 4)
				{
					return new idRectangle(
						float.Parse(parts[0]),
						float.Parse(parts[1]),
						float.Parse(parts[2]),
						float.Parse(parts[3]));
				}
			}
			catch(Exception x)
			{
				Debug.Write(x.ToString());
			}

			return idRectangle.Empty;
		}

		public static Vector2 ParseVector2(string str)
		{
			try
			{
				string[] parts = null;

				if(str.Contains(",") == true)
				{
					parts = str.Replace(" ", "").Split(',');
				}
				else
				{
					parts = str.Split(' ');
				}

				if(parts.Length == 2)
				{
					return new Vector2(
						float.Parse(parts[0]),
						float.Parse(parts[1]));
				}
			}
			catch(Exception x)
			{
				Debug.Write(x.ToString());
			}

			return Vector2.Zero;
		}

		public static Vector3 ParseVector3(string str)
		{
			try
			{
				string[] parts = null;

				if(str.Contains(",") == true)
				{
					parts = str.Replace(" ", "").Split(',');
				}
				else
				{
					parts = str.Split(' ');
				}

				if(parts.Length == 3)
				{
					return new Vector3(
						float.Parse(parts[0]),
						float.Parse(parts[1]),
						float.Parse(parts[2]));
				}
			}
			catch(Exception x)
			{
				Debug.Write(x.ToString());
			}

			return Vector3.Zero;
		}

		public static Vector4 ParseVector4(string str)
		{
			try
			{
				string[] parts = null;

				if(str.Contains(",") == true)
				{
					parts = str.Replace(" ", "").Split(',');
				}
				else
				{
					parts = str.Split(' ');
				}

				if(parts.Length == 4)
				{
					return new Vector4(
						float.Parse(parts[0]),
						float.Parse(parts[1]),
						float.Parse(parts[2]),
						float.Parse(parts[3]));
				}
			}
			catch(Exception x)
			{
				Debug.Write(x.ToString());
			}

			return Vector4.Zero;
		}

		public static string RemoveColors(string str)
		{
			StringBuilder newStr = new StringBuilder();
			int length = str.Length;

			for(int i = 0; i < length; i++)
			{
				char c = str[i];

				if(IsColor(str, i) == true)
				{
					i++;
				}
				else
				{
					newStr.Append(c);
				}
			}

			return newStr.ToString();
		}

		public static void TransformJoints(idJointMatrix[] jointMatrices, int[] parents, int firstJoint, int lastJoint)
		{
			for(int i = firstJoint; i <= lastJoint; i++)
			{
				jointMatrices[i] = idJointMatrix.Transform(jointMatrices[i], jointMatrices[parents[i]]);
			}
		}

		public static string WrapText(string text, int columnWidth, int offset)
		{
			string str = string.Empty;
			int lineCount = text.Length / columnWidth;

			if((text.Length % columnWidth) != 0)
			{
				lineCount++;
			}

			for(int i = 0; i < lineCount; i++)
			{
				int width = columnWidth;

				if(((i * columnWidth) + columnWidth) > text.Length)
				{
					width = text.Length - (i * columnWidth);
				}

				str += text.Substring(i * columnWidth, width).PadLeft(offset);
			}

			return str;
		}
	}

	public class idColor
	{
		public static readonly Vector4 Black = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
		public static readonly Vector4 White = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
		public static readonly Vector4 Red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
		public static readonly Vector4 Green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
		public static readonly Vector4 Blue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
		public static readonly Vector4 Yellow = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
		public static readonly Vector4 Magenta = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
		public static readonly Vector4 Cyan = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);
		public static readonly Vector4 Orange = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
		public static readonly Vector4 Purple = new Vector4(0.6f, 0.0f, 0.6f, 1.0f);
		public static readonly Vector4 Pink = new Vector4(0.73f, 0.4f, 0.48f, 1.0f);
		public static readonly Vector4 Brown = new Vector4(0.4f, 0.35f, 0.08f, 1.0f);
		public static readonly Vector4 Grey = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
		public static readonly Vector4 LightGrey = new Vector4(0.75f, 0.75f, 0.75f, 1.0f);
		public static readonly Vector4 MdGrey = new Vector4(0.0f, 0.5f, 0.5f, 1.0f);
		public static readonly Vector4 DarkGrey = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);
	}

	public enum idColorIndex
	{
		Escape = '^',
		Default = '0',
		Red = '1',
		Green = '2',
		Yellow = '3',
		Blue = '4',
		Cyan = '5',
		Magenta = '6',
		White = '7',
		Gray = '8',
		Black = '9'
	}

	public static class idColorString
	{
		public const string Default = "^0";
		public const string Red = "^1";
		public const string Green = "^2";
		public const string Yellow = "^3";
		public const string Blue = "^4";
		public const string Cyan = "^5";
		public const string Magenta = "^6";
		public const string White = "^7";
		public const string Gray = "^8";
		public const string Black = "^9";
	}
}