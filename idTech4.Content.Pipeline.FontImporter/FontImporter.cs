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
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using idTech4.Content.Pipeline;
using idTech4.Content.Pipeline.Intermediate.Fonts;

using TImport = idTech4.Content.Pipeline.Intermediate.Fonts.FontContent;

namespace idTech4.Content.Pipeline
{
	[ContentImporter(".dat", DisplayName = "Font - idTech4", DefaultProcessor = "FontProcessor")]
	public class FontImporter : ContentImporter<TImport>
	{
		#region Constants
		private const int FontVersion = 42;
		private const int FontMagic   = (FontVersion | ('i' << 24) | ('d' << 16) | ('f' << 8));
		#endregion

		public override TImport Import(string filename, ContentImporterContext context)
		{
			//System.Diagnostics.Debugger.Launch();

			TImport outContent = new TImport();
			Stream source      = File.OpenRead(filename);
			
			using(idBinaryReader r = new idBinaryReader(source))
			{
				uint version = r.ReadUInt32();

				if(version != FontMagic)
				{
					throw new InvalidContentException(string.Format("Wrong version, expected {0} but got {1}", FontMagic, version));
				}

				short pointSize = r.ReadInt16();

				if(pointSize != 48)
				{
					throw new InvalidContentException(string.Format("Expected a point size of 48 but got {0}", pointSize));
				}

				outContent.Ascender  = r.ReadInt16();
				outContent.Descender = r.ReadInt16();

				int glyphCount = r.ReadInt16();

				outContent.Glyphs           = new FontGlyph[glyphCount];
				outContent.CharacterIndices = new uint[glyphCount];

				for(int i = 0; i < outContent.Glyphs.Length; i++)
				{
					outContent.Glyphs[i]                    = new FontGlyph();
					outContent.Glyphs[i].Width              = r.ReadByte();
					outContent.Glyphs[i].Height             = r.ReadByte();
					outContent.Glyphs[i].Top                = r.ReadByte();
					outContent.Glyphs[i].Left               = r.ReadByte();
					outContent.Glyphs[i].SkipX              = r.ReadByte();
					
					// padding
					r.ReadByte();
				
					outContent.Glyphs[i].TextureCoordinates = new Vector2(r.ReadUInt16(false), r.ReadUInt16(false));
				}

				for(int i = 0; i < outContent.CharacterIndices.Length; i++)
				{
					outContent.CharacterIndices[i] = r.ReadUInt32(false);
				}

				outContent.Ascii = new char[128];

				for(int i = 0; i < outContent.Ascii.Length; i++)
				{
					outContent.Ascii[i] = (char) 0;
				}

				for(int i = 0; i < outContent.Glyphs.Length; i++)
				{
					if(outContent.CharacterIndices[i] < 128)
					{
						outContent.Ascii[outContent.CharacterIndices[i]] = (char) i;
					}
					else
					{
						// since the characters are sorted, as soon as we find a non-ascii character, we can stop.
						break;
					}
				}
			}

			return outContent;
		}
	}
}