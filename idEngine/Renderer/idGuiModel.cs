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

namespace idTech4.Renderer
{
	public sealed class idGuiModel
	{
		#region Members
		private GuiModelSurface _surface;

		private List<GuiModelSurface> _surfaces = new List<GuiModelSurface>();
		/*private List<idIndex> _indexes = new List<idIndex>();
		private List<idVertex> _vertices = new List<idVertex>();*/
		#endregion
	
		#region Constructor
		public idGuiModel()
		{
			Clear();
		}
		#endregion

		#region Methods
		#region Public
		public void Clear()
		{
			_surfaces.Clear();
			/*_indexes.Clear();
			_vertices.Clear();*/

			AdvanceSurface();
		}
		#endregion

		#region Private
		private void AdvanceSurface()
		{
			GuiModelSurface s = new GuiModelSurface();

			if(_surfaces.Count > 0)
			{
				s.Color = _surface.Color;
				s.Material = _surface.Material;
			}
			else
			{
				s.Color = new Color(1, 1, 1, 1);
				// TODO: s.Material = 
			}

			s.IndexCount = 0;
			//s.FirstIndex = _indexes.Count;
			s.VertexCount = 0;
			//s.FirstVertex = _vertices.Count;

			_surfaces.Add(s);
			_surface = s;
		}
		#endregion
		#endregion
	}

	internal struct GuiModelSurface
	{
		public idMaterial Material;
		public Color Color;

		public int FirstVertex;
		public int VertexCount;

		public int FirstIndex;
		public int IndexCount;
	}
}
