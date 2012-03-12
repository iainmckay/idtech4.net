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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4.Renderer
{
	public sealed class idFont
	{
		#region Properties
		public idFontGlyph[] Glyphs
		{
			get
			{
				return _glyphs;
			}
		}

		public float GlyphScale
		{
			get
			{
				return _glyphScale;
			}
		}

		public int MaxWidth
		{
			get
			{
				return _maxWidth;
			}
		}

		public int MaxHeight
		{
			get
			{
				return _maxHeight;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}
		#endregion

		#region Members
		private string _name;
		private int _maxWidth;
		private int _maxHeight;
		private float _glyphScale;
		private idFontGlyph[] _glyphs = new idFontGlyph[idE.GlyphsPerFont];
		#endregion

		#region Constructor
		public idFont(string name)
		{
			_name = name;
		}
		#endregion

		#region Methods
		#region Public
		public void Init(BinaryReader reader, string fontName)
		{
			int junk;
			byte[] tmpBytes = new byte[32];
			char[] tmpChars;

			for(int i = 0; i < idE.GlyphsPerFont; i++)
			{
				_glyphs[i].Height = reader.ReadInt32();
				_glyphs[i].Top = reader.ReadInt32();
				_glyphs[i].Bottom = reader.ReadInt32();
				_glyphs[i].Pitch = reader.ReadInt32();
				_glyphs[i].SkipX = reader.ReadInt32();
				_glyphs[i].ImageWidth = reader.ReadInt32();
				_glyphs[i].ImageHeight = reader.ReadInt32();
				_glyphs[i].S = reader.ReadSingle();
				_glyphs[i].T = reader.ReadSingle();
				_glyphs[i].S2 = reader.ReadSingle();
				_glyphs[i].T2 = reader.ReadSingle();

				junk /* font.glyphs[i].glyph */ = reader.ReadInt32();
			
				//FIXME: the +6, -6 skips the embedded fonts/ 
				reader.BaseStream.Seek(6, SeekOrigin.Current);
				tmpBytes = reader.ReadBytes(26);
				tmpChars = Encoding.ASCII.GetChars(tmpBytes);

				_glyphs[i].MaterialName = new String(tmpChars, 0, Array.IndexOf(tmpChars, '\0'));
			}

			_glyphScale = reader.ReadSingle();

			_maxWidth = 0;
			_maxHeight = 0;

			for(int i = idE.GlyphStart; i < idE.GlyphEnd; i++)
			{
				string materialName = string.Format("{0}/{1}", fontName, _glyphs[i].MaterialName);

				_glyphs[i].Glyph = idE.DeclManager.FindMaterial(materialName);
				_glyphs[i].Glyph.Sort = (float) MaterialSort.Gui;

				if(_maxHeight < _glyphs[i].Height)
				{
					_maxHeight = _glyphs[i].Height;
				}

				if(_maxWidth < _glyphs[i].SkipX)
				{
					_maxWidth = _glyphs[i].SkipX;
				}
			}
		}
		#endregion
		#endregion
	}

	public sealed class idFontFamily
	{
		#region Properties				
		public idFont Large
		{
			get
			{
				return _fontLarge;
			}
			set
			{
				_fontLarge = value;
			}
		}

		public idFont Medium
		{
			get
			{
				return _fontMedium;
			}
			set
			{
				_fontMedium = value;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public idFont Small
		{
			get
			{
				return _fontSmall;
			}
			set
			{
				_fontSmall = value;
			}
		}
		#endregion

		#region Members
		private idFont _fontSmall;
		private idFont _fontMedium;
		private idFont _fontLarge;
		private string _name;
		#endregion

		#region Constructor
		public idFontFamily(string name)
		{
			_name = name;
		}
		#endregion
	}

	public struct idFontGlyph
	{
		#region Members
		/// <summary>Number of scan lines.</summary>
		public int Height;
		/// <summary>Top of glyph in buffer.</summary>
		public int Top;
		/// <summary>Bottom of glyph in buffer.</summary>
		public int Bottom;
		/// <summary>Width for copying.</summary>
		public int Pitch;
		/// <summary>X adjustment.</summary>
		public int SkipX;
		/// <summary>Width of actual image.</summary>
		public int ImageWidth;
		/// <summary>Height of actual image.</summary>
		public int ImageHeight;
		/// <summary>X offset in image where glyph starts.</summary>
		public float S;
		/// <summary>Y offset in image where glyph starts.</summary>
		public float T;
		public float S2;
		public float T2;
		/// <summary>Material with the glyph.</summary>
		public idMaterial Glyph;
		public string MaterialName;
		#endregion
	}
}