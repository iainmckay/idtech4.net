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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.UI.SWF
{
	public class idSWFFont : idSWFDictionaryEntry
	{
		#region Properties
		public idFont Font
		{
			get
			{
				return _font;
			}
		}
		#endregion

		#region Members
		private short _ascent;
		private short _descent;
		private short _leading;

		private idFont _font;
		private idSWFFontGlyph[] _glyphs;
		#endregion

		#region idSWFDictionaryEntry implementation
		internal override void LoadFrom(ContentReader input)
		{
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();

			_ascent = input.ReadInt16();
			_descent = input.ReadInt16();
			_leading = input.ReadInt16();

			_font = renderSystem.LoadFont(input.ReadString());

			_glyphs = new idSWFFontGlyph[input.ReadInt32()];

			for(int i = 0; i < _glyphs.Length; i++)
			{
				_glyphs[i] = new idSWFFontGlyph();
				_glyphs[i].LoadFrom(input);
			}
		}
		#endregion
	}

	public class idSWFFontGlyph
	{
		#region Members
		private ushort _code;
		private short _advance;

		private Vector2[] _vertices;
		private ushort[] _indices;
		#endregion

		internal void LoadFrom(ContentReader input)
		{
			_code     = input.ReadUInt16();
			_advance  = input.ReadInt16();

			_vertices = new Vector2[input.ReadInt32()];
			
			for(int i = 0; i < _vertices.Length; i++)
			{
				_vertices[i] = input.ReadVector2();
			}

			_indices = new ushort[input.ReadInt32()];

			for(int i = 0; i < _indices.Length; i++)
			{
				_indices[i] = input.ReadUInt16();
			}
		}
	}
}