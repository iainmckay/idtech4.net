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
using idTech4.Renderer;
using idTech4.Services;

namespace idTech4
{
	public class idHelper
	{
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

		public static T[] Flatten<T>(T[, ,] source)
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
						flat[x * d2 * d3 + y * d3 + z] = source[x, y, z];
					}
				}
			}

			return flat;
		}

		public static int FrameToMillsecond(long frame)
		{
			return (int) ((frame * Constants.EngineHzNumerator) / Constants.EngineHzDenominator);
		}

		public static MaterialStates MakeStencilReference(ulong x)
		{
			return (MaterialStates) ((x << (int) MaterialStates.StencilFunctionReferenceShift) & ((int) MaterialStates.StencilFunctionReferenceBits));
		}

		public static MaterialStates MakeStencilMask(ulong x)
		{
			return (MaterialStates) ((x << (int) MaterialStates.StencilFunctionReferenceShift) & ((int) MaterialStates.StencilFunctionReferenceBits));
		}
	}
}