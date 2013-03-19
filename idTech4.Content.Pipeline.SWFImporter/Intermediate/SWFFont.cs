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
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace idTech4.Content.Pipeline.Intermediate.SWF
{
	public class SWFFont : SWFDictionaryEntry
	{
		public short Ascent;
		public short Descent;
		public short Leading;

		public string Name;

		public SWFFontGlyph[] Glyphs;

		public override void Write(ContentWriter output)
		{
			output.Write((int) SWFDictionaryType.Font);
			output.Write(this.Ascent);
			output.Write(this.Descent);
			output.Write(this.Leading);
			output.Write(this.Name);
			
			output.Write(this.Glyphs.Length);

			for(int i = 0; i < this.Glyphs.Length; i++)
			{
				this.Glyphs[i].Write(output);
			}
		}
	}

	public class SWFFontGlyph
	{
		public ushort Code;
		public short Advance;

		public Vector2[] Vertices;
		public ushort[] Indices;

		public void Write(ContentWriter output)
		{
			output.Write(this.Code);
			output.Write(this.Advance);
			
			output.Write(this.Vertices.Length);

			for(int i = 0; i < this.Vertices.Length; i++)
			{
				output.Write(this.Vertices[i]);
			}

			output.Write(this.Indices.Length);

			for(int i = 0; i < this.Indices.Length; i++)
			{
				output.Write(this.Indices[i]);
			}
		}
	}
}