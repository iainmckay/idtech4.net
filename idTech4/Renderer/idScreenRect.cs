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
namespace idTech4.Renderer
{
	public struct idScreenRect
	{
		#region Properties
		public bool IsEmpty
		{
			get
			{
				return ((X1 > X2) || (Y1 > Y2));
			}
		}
		#endregion

		#region Fields
		// inclusive pixel bounds inside viewport
		public short X1;
		public short X2;
		public short Y1;
		public short Y2;

		// for depth bounds test
		public float MinZ;
		public float MaxZ;
		#endregion

		#region Methods
		public void AddPoint(float x, float y)
		{
			short ix = (short) x;
			short iy = (short) y;

			if(ix < this.X1)
			{
				this.X1 = ix;
			}

			if(ix > this.X2)
			{
				this.X2 = ix;
			}

			if(iy < this.Y1)
			{
				this.Y1 = iy;
			}

			if(iy > this.Y2)
			{
				this.Y2 = iy;
			}
		}

		public void Clear()
		{
			this.X1   = this.Y1 = 32000;
			this.X2   = this.Y2 = -32000;

			this.MinZ = 0.0f;
			this.MaxZ = 1.0f;
		}

		public void Expand()
		{
			this.X1--;
			this.Y1--;
			this.X2++;
			this.Y2++;
		}

		public void Intersect(idScreenRect rect)
		{
			if(rect.X1 > this.X1)
			{
				this.X1 = rect.X1;
			}

			if(rect.X2 < this.X2)
			{
				this.X2 = rect.X2;
			}

			if(rect.Y1 > this.Y1)
			{
				this.Y1 = rect.Y1;
			}

			if(rect.Y2 < this.Y2)
			{
				this.Y2 = rect.Y2;
			}
		}

		public void Union(idScreenRect rect)
		{
			if(rect.X1 < this.X1)
			{
				this.X1 = rect.X1;
			}

			if(rect.X2 > this.X2)
			{
				this.X2 = rect.X2;
			}

			if(rect.Y1 < this.Y1)
			{
				this.Y1 = rect.Y1;
			}

			if(rect.Y2 > this.Y2)
			{
				this.Y2 = rect.Y2;
			}
		}
		#endregion

		#region Overloads
		public override bool Equals(object obj)
		{
			if(obj is idScreenRect)
			{
				return (this == (idScreenRect) obj);
			}

			return base.Equals(obj);
		}

		public static bool operator ==(idScreenRect r1, idScreenRect r2)
		{
			return ((r1.X1 == r2.X1) && (r1.X2 == r2.X2) && (r1.Y1 == r2.Y2) && (r1.Y2 == r2.Y2)
				&& (r1.MinZ == r2.MinZ) && (r1.MaxZ == r2.MaxZ));
		}

		public static bool operator !=(idScreenRect r1, idScreenRect r2)
		{
			return !(r1 == r2);
		}
		#endregion
	}
}