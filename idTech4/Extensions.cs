/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace idTech4
{
	public static class Extensions
	{
		#region GameServiceContainer
		public static T GetService<T>(this GameServiceContainer s) where T : class
		{
			return (s.GetService(typeof(T)) as T);
		}
		#endregion

		#region Matrix
		public static void ApplyDepthHack(this Matrix m)
		{
			// scale projected z by 25%
			m.M31 *= 0.25f;
			m.M32 *= 0.25f;
			m.M33 *= 0.25f;
			m.M34 *= 0.25f;
		}

		public static Vector4 Get(this Matrix m, int row)
		{
			if(row == 0)
			{
				return new Vector4(m.M11, m.M12, m.M13, m.M14);
			}
			else if(row == 1)
			{
				return new Vector4(m.M21, m.M22, m.M23, m.M24);
			}
			else if(row == 2)
			{
				return new Vector4(m.M31, m.M32, m.M33, m.M34);
			}
			else if(row == 3)
			{
				return new Vector4(m.M41, m.M42, m.M43, m.M44);
			}

			throw new ArgumentOutOfRangeException("row");
		}
		#endregion

		#region MouseState
		public static bool IsInsideWindow(this MouseState mouseState)
		{
			Point mousePosition = new Point(mouseState.X, mouseState.Y);

			return idEngine.Instance.GraphicsDevice.Viewport.Bounds.Contains(mousePosition);
		}
		#endregion

		#region Vector2
		public static float Get(this Vector2 v, int component)
		{
			if(component == 0)
			{
				return v.X;
			}
			else if(component == 1)
			{
				return v.Y;
			}

			throw new ArgumentOutOfRangeException("component");
		}

		public static Vector3 ToVector3(this Vector2 v)
		{
			return new Vector3(v.X, v.Y, 0);
		}
		#endregion

		#region Vector3
		public static Vector2 ToVector2(this Vector3 v)
		{
			return new Vector2(v.X, v.Y);
		}
		#endregion
	}
}