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

namespace idTech4.Renderer
{
	public sealed class idRenderModelManager
	{
		#region Members
		private Dictionary<string, idRenderModel> _models = new Dictionary<string, idRenderModel>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Constructor
		public idRenderModelManager()
		{
			new idCvar("r_mergeModelSurfaces", "1", "combine model surfaces with the same material", CvarFlags.Bool | CvarFlags.Renderer);
			new idCvar("r_slopVertex", "0.01", "merge xyz coordinates this far apart", CvarFlags.Renderer);
			new idCvar("r_slopTexCoord", "0.001", "merge texture coordinates this far apart", CvarFlags.Renderer);
			new idCvar("r_slopNormal", "0.02", "merge normals that dot less than this", CvarFlags.Renderer);
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// World map parsing will add all the inline models with this call.
		/// </summary>
		/// <param name="model"></param>
		public void AddModel(idRenderModel model)
		{
			_models.Add(model.Name, model);
		}

		/// <summary>
		/// Allocates a new empty render model.
		/// </summary>
		/// <returns></returns>
		public idRenderModel AllocateModel()
		{
			return new idStaticRenderModel();
		}
		#endregion
		#endregion
	}

	public struct RenderModelSurface
	{
		public int ID;
		public idMaterial Material;
		public Surface Geometry;
	}
}