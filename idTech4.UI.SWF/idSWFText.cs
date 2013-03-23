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
using Microsoft.Xna.Framework.Content;

namespace idTech4.UI.SWF
{
	public class idSWFText : idSWFDictionaryEntry
	{
		#region Members
		private idSWFRect _bounds;
		private idSWFMatrix _matrix;

		private idSWFTextRecord[] _records;
		private idSWFGlyphEntry[] _glyphs;
		#endregion

		#region idSWFDictionaryEntry implementation
		internal override void LoadFrom(ContentReader input)
		{
			_bounds.LoadFrom(input);
			_matrix.LoadFrom(input);

			_records = new idSWFTextRecord[input.ReadInt32()];

			for(int i = 0; i < _records.Length; i++)
			{
				_records[i].LoadFrom(input);
			}

			_glyphs = new idSWFGlyphEntry[input.ReadInt32()];

			for(int i = 0; i < _glyphs.Length; i++)
			{
				_glyphs[i].LoadFrom(input);
			}
		}
		#endregion
	}

	public class idSWFTextRecord
	{
		#region Members
		private ushort _fontID;
		private idSWFColorRGBA _color = idSWFColorRGBA.Default;
		private short _offsetX;
		private short _offsetY;
		private ushort _textHeight;
		private ushort _firstGlyph;
		private ushort _glyphCount;
		#endregion

		internal void LoadFrom(ContentReader input)
		{
			_fontID = input.ReadUInt16();

			_color.LoadFrom(input);

			_offsetX = input.ReadInt16();
			_offsetY = input.ReadInt16();

			_textHeight = input.ReadUInt16();
			_firstGlyph = input.ReadUInt16();
			_glyphCount = input.ReadUInt16();
		}
	}

	public struct idSWFGlyphEntry
	{
		public uint Index;
		public int Advance;

		internal void LoadFrom(ContentReader input)
		{
			this.Index   = input.ReadUInt32();
			this.Advance = input.ReadInt32();
		}
	}
}