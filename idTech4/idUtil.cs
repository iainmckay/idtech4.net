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
using System.Diagnostics;

using Microsoft.Xna.Framework;

using idTech4.Math;

namespace idTech4
{
	public class idUtil
	{
		#region Buffer Manipulation
		public static char GetBufferCharacter(string buffer, int position)
		{
			if((position < 0) || (position >= buffer.Length))
			{
				return '\0';
			}
						
			return buffer[position];
		}
		#endregion

		#region Parsing
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
		#endregion
	}
}