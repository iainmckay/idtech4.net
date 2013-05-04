/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using idTech4.Services;

namespace idTech4.Renderer
{
	public class idFont
	{
		#region Properties
		public short Ascender
		{
			get
			{
				return _ascender;
			}
		}

		public short Descender
		{
			get
			{
				return _descender;
			}
		}
		#endregion

		#region Members
		private short _ascender;
		private short _descender;

		private FontGlyph[] _glyphs;
		private uint[] _characterIndices;
		private char[] _ascii;

		private idMaterial _material;
		#endregion

		#region Constructor
		internal idFont()
		{

		}
		#endregion

		#region Metrics.
		public ScaledGlyph GetScaledGlyph(float scale, uint index)
		{
			int i        = GetGlyphIndex((int) index);
			int asterisk = 42;

			if((i == -1) && (index != asterisk))
			{
				i = GetGlyphIndex(asterisk);
			}

			if(i >= 0) 
			{
				float invMaterialWidth  = 1.0f / _material.ImageWidth;
				float invMaterialHeight = 1.0f / _material.ImageHeight;

				FontGlyph glyphInfo = _glyphs[i];

				ScaledGlyph scaledGlyph = new ScaledGlyph();
				scaledGlyph.SkipX       = scale * glyphInfo.SkipX;
				scaledGlyph.Top         = scale * glyphInfo.Top;
				scaledGlyph.Left        = scale * glyphInfo.Left;
				scaledGlyph.Width       = scale * glyphInfo.Width;
				scaledGlyph.Height      = scale * glyphInfo.Height;
				scaledGlyph.S1          = (glyphInfo.TextureCoordinates.X - 0.5f) * invMaterialWidth;
				scaledGlyph.T1          = (glyphInfo.TextureCoordinates.Y - 0.5f) * invMaterialHeight;
				scaledGlyph.S2          = (glyphInfo.TextureCoordinates.X + glyphInfo.Width + 0.5f) * invMaterialWidth;
				scaledGlyph.T2          = (glyphInfo.TextureCoordinates.Y + glyphInfo.Height + 0.5f) * invMaterialHeight;
				scaledGlyph.Material    =_material;

				return scaledGlyph;
			}

			return new ScaledGlyph();
		}

		private int GetGlyphIndex(int index)
		{
			if(index < 128) 
			{
				return _ascii[index];
			}

			if(_glyphs.Length == 0)
			{
				return -1;
			}

			if(_characterIndices == null)
			{
				return index;
			}

			int length = _glyphs.Length;
			int mid    = _glyphs.Length;
			int offset = 0;

			while(mid > 0)
			{
				mid = length >> 1;

				if(_characterIndices[offset + mid] <= index)
				{
					offset += mid;
				}

				length -= mid;
			}

			return ((_characterIndices[offset] == index) ? offset : -1);
		}
		#endregion

		#region Loading
		internal void LoadFrom(ContentReader input)
		{
			_ascender  = input.ReadInt16();
			_descender = input.ReadInt16();

			_glyphs = new FontGlyph[input.ReadInt32()];

			for(int i = 0; i < _glyphs.Length; i++)
			{
				_glyphs[i] = new FontGlyph(
					input.ReadInt32(),
					input.ReadInt32(),
					input.ReadInt32(),
					input.ReadInt32(),
					input.ReadInt32(),
					input.ReadVector2());
			}

			_characterIndices = new uint[input.ReadInt32()];

			for(int i = 0; i < _characterIndices.Length; i++)
			{
				_characterIndices[i] = input.ReadUInt32();
			}

			_ascii = new char[input.ReadInt32()];

			for(int i = 0; i < _ascii.Length; i++)
			{
				_ascii[i] = input.ReadChar();
			}

			_material = idEngine.Instance.GetService<IDeclManager>().FindMaterial(input.ReadString());
		}
		#endregion
	}

	public class FontGlyph
	{
		#region Properties
		/// <summary>Width of glyph in pixels.</summary>
		public int Width
		{
			get
			{
				return _width;
			}
		}

		/// <summary>Height of glyph in pixels.</summary>
		public int Height
		{
			get
			{
				return _height;
			}
		}

		/// <summary>Distance in pixels from the base line to the top of the glyph.</summary>
		public int Top
		{
			get
			{
				return _top;
			}
		}

		/// <summary>Distance in pixels from the pen to the left edge of the glyph.</summary>
		public int Left
		{
			get
			{
				return _left;
			}
		}

		/// <summary>X adjustment after rendering this glyph.</summary>
		public int SkipX
		{
			get
			{
				return _skipX;
			}
		}

		/// <summary>
		/// Texture coordinates for the glyph.
		/// </summary>
		public Vector2 TextureCoordinates
		{
			get
			{
				return _textureCoordinates;
			}
		}
		#endregion

		#region Members
		private int _width;
		private int _height;
		private int _top;
		private int _left;
		private int _skipX;
		private Vector2 _textureCoordinates;
		#endregion

		#region Constructor
		public FontGlyph(int width, int height, int top, int left, int skipX, Vector2 textureCoordinates)
		{
			_width              = width;
			_height             = height;
			_top                = top;
			_left               = left;
			_skipX              = skipX;
			_textureCoordinates = textureCoordinates;
		}
		#endregion
	}

	public struct ScaledGlyph
	{
		public float Top;
		public float Left;
		public float Width;
		public float Height;
		public float SkipX;

		public float S1;
		public float T1;

		public float S2;
		public float T2;

		public idMaterial Material;
	}
}