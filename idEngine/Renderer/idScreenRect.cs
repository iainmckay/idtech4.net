﻿/*
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

namespace idTech4.Renderer
{
	public struct idScreenRect
	{
		// inclusive pixel bounds inside viewport
		public short X1;
		public short X2;
		public short Y1;
		public short Y2;

		// for depth bounds test
		public float MinZ;
		public float MaxZ;

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
	}
}
