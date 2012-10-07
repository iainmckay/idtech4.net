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

using idTech4.Renderer;

namespace idTech4.Geometry
{
	public class idPatchSurface : idSurface
	{
		#region Properties
		/// <summary>
		/// True if the vertices are spaced out.
		/// </summary>
		public bool Expanded
		{
			get
			{
				return _expanded;
			}
		}

		/// <summary>
		/// Width of the patch.
		/// </summary>
		public int Width
		{
			get
			{
				return _width;
			}
		}

		/// <summary>
		/// Height of the patch.
		/// </summary>
		public int Height
		{
			get
			{
				return _height;
			}
		}

		/// <summary>
		/// Maximum width allocated for.
		/// </summary>
		public int MaxWidth
		{
			get
			{
				return _maxWidth;
			}
		}

		/// <summary>
		/// Maximum height allocated for.
		/// </summary>
		public int MaxHeight
		{
			get
			{
				return _maxHeight;
			}
		}
		#endregion

		#region Members
		private int _width;	
		private int _height;
		private int _maxWidth;
		private int _maxHeight;
		private bool _expanded;
		#endregion

		#region Constructor
		public idPatchSurface() : base()
		{

		}

		public idPatchSurface(int maxWidth, int maxHeight) : base()
		{
			_width = maxWidth;
			_height = maxHeight;
			_maxWidth = maxWidth;
			_maxHeight = maxHeight;
			_vertices = new Vertex[maxWidth * maxHeight];
		}
		#endregion
	}
}