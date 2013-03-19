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
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace idTech4.Content.Pipeline.Intermediate.SWF
{
	public class SWFText : SWFDictionaryEntry
	{
		public SWFRect Bounds;
		public SWFMatrix Matrix;

		public SWFTextRecord[] Records;
		public SWFGlyphEntry[] Glyphs;

		public override void Write(ContentWriter output)
		{
			output.Write((int) SWFDictionaryType.Text);

			this.Bounds.Write(output);
			this.Matrix.Write(output);

			output.Write(this.Records.Length);

			for(int i = 0; i < this.Records.Length; i++)
			{
				this.Records[i].Write(output);
			}

			output.Write(this.Glyphs.Length);

			for(int i = 0; i < this.Glyphs.Length; i++)
			{
				this.Glyphs[i].Write(output);
			}
		}
	}

	public class SWFTextRecord
	{
		public ushort FontID;
		public SWFColorRGBA Color = SWFColorRGBA.Default;
		public short OffsetX;
		public short OffsetY;
		public ushort TextHeight;
		public ushort FirstGlyph;
		public ushort GlyphCount;

		public void Write(ContentWriter output)
		{
			output.Write(this.FontID);

			this.Color.Write(output);

			output.Write(this.OffsetX);
			output.Write(this.OffsetY);
			output.Write(this.TextHeight);
			output.Write(this.FirstGlyph);
			output.Write(this.GlyphCount);
		}
	}

	public struct SWFGlyphEntry
	{
		public uint Index;
		public int Advance;

		public void Write(ContentWriter output)
		{
			output.Write(this.Index);
			output.Write(this.Advance);
		}
	}
}