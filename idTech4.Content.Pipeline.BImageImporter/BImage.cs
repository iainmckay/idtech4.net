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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace idTech4.Content.Pipeline
{
	/// <summary>
	/// BImage is for loading generated files by idImage.
	/// </summary>
	/// <remarks>
	/// Values are big-endian.
	/// </remarks>
	public class BImage
	{
		#region Constants
		public const int Version = 10;
		public const uint Magic  = (uint) (('B' << 0) | ('I' << 8) | ('M' << 16) | (Version << 24));
		#endregion

		#region Properties
		public BImageColorFormat ColorFormat
		{
			get
			{
				return _header.ColorFormat;
			}
		}

		public BImageFormat Format
		{
			get
			{
				return _header.Format;
			}
		}

		public BImageType Type
		{
			get
			{
				return _header.TextureType;
			}
		}

		public int Width
		{
			get
			{
				return _header.Width;
			}
		}

		public int Height
		{
			get
			{
				return _header.Height;
			}
		}

		public int LevelCount
		{
			get
			{
				return _header.LevelCount;
			}
		}
		#endregion

		#region Members
		private BImageFile _header;
		private BImageData[] _images;
		#endregion

		#region Constructor
		public BImage()
		{

		}
		#endregion

		#region Methods
		#region Fetching
		public BImageData GetData(int level)
		{
			return _images[level];
		}
		#endregion

		#region Loading
		public bool LoadFrom(Stream source)
		{
			BinaryReader r = new idBinaryReader(source);

			_header = new BImageFile();
			_header.LoadFrom(r);

			if(Magic != _header.HeaderMagic)
			{
				return false;
			}

			int imageCount = _header.LevelCount;

			if(_header.TextureType == BImageType.Cubic)
			{
				imageCount *= 6;
			}

			_images = new BImageData[imageCount];

			for(int i = 0; i < imageCount; i++)
			{
				BImageData image = _images[i] = new BImageData();

				if(image.LoadFrom(r, _header.Format) == false)
				{
					return false;
				}

				Debug.Assert((image.Level >= 0) && (image.Level < imageCount));
				Debug.Assert((image.DestZ == 0) || (_header.TextureType == BImageType.Cubic));
			}

			return true;
		}

		public static BImage LoadFrom(string filename)
		{
			// TODO: no error handling
			using(Stream stream = File.OpenRead(filename))
			{
				BImage image = new BImage();

				if(image.LoadFrom(stream) == false)
				{
					return null;
				}

				return image;
			}
		}
		#endregion
		#endregion

		#region Internal Types
		private class BImageFile
		{
			public long SourceFileTime;
			public int HeaderMagic;
			public BImageType TextureType;
			public BImageFormat Format;
			public BImageColorFormat ColorFormat;
			public int Width;
			public int Height;
			public int LevelCount;

			// one or more bimageImage_t structures follow

			public bool LoadFrom(BinaryReader r)
			{
				this.SourceFileTime = r.ReadInt64();
				this.HeaderMagic    = r.ReadInt32();
				this.TextureType    = (BImageType) r.ReadInt32();
				this.Format         = (BImageFormat) r.ReadInt32();
				this.ColorFormat    = (BImageColorFormat) r.ReadInt32();
				this.Width          = r.ReadInt32();
				this.Height         = r.ReadInt32();
				this.LevelCount     = r.ReadInt32();

				return true;
			}
		}
		#endregion
	}

	public class BImageData
	{
		#region Properties
		public int Level
		{
			get
			{
				return _level;
			}
		}

		public int DestZ
		{
			get
			{
				return _destZ;
			}
		}

		public int Width
		{
			get
			{
				return _width;
			}
		}

		public int Height
		{
			get
			{
				return _height;
			}
		}

		public int DataSize
		{
			get
			{
				return _dataSize;
			}
		}

		public byte[] Data
		{
			get
			{
				return _data;
			}
		}
		#endregion

		#region Members
		private int _level;
		private int _destZ;
		private int _width;
		private int _height;
		private int _dataSize;
		private byte[] _data;
		#endregion

		#region Loading
		public bool LoadFrom(BinaryReader r, BImageFormat format)
		{
			_level    = r.ReadInt32();
			_destZ    = r.ReadInt32();
			_width    = r.ReadInt32();
			_height   = r.ReadInt32();
			_dataSize = r.ReadInt32();

			Debug.Assert(_dataSize > 0);

			// DXT images need to be padded to 4x4 block sizes, but the original image
			// sizes are still retained, so the stored data size may be larger than
			// just the multiplication of dimensions
			Debug.Assert(this.DataSize >= ((this.Width * this.Height * (BitsForFormat(format) / 8))));

			_data = r.ReadBytes(this.DataSize);

			Debug.Assert(this.Data.Length == this.DataSize);

			return true;
		}

		private int BitsForFormat(BImageFormat format)
		{
			switch(format)
			{
				case BImageFormat.None:       return 0;
				case BImageFormat.RGBA8:      return 32;
				case BImageFormat.XRGB8:      return 32;
				case BImageFormat.RGB565:     return 16;
				case BImageFormat.L8A8:       return 16;
				case BImageFormat.Alpha:      return 8;
				case BImageFormat.Luminance8: return 8;
				case BImageFormat.Intensity8: return 8;
				case BImageFormat.Dxt1:       return 4;
				case BImageFormat.Dxt5:       return 8;
				case BImageFormat.Depth:      return 32;
				case BImageFormat.X16:        return 16;
				case BImageFormat.Y16X16:     return 32;

				default:
					return 0;
			}
		}
		#endregion
	}

	public enum BImageType : int
	{
		Disabled,
		TwoD,
		Cubic
	}

	public enum BImageFormat : int
	{
		None,

		//------------------------
		// Standard color image formats
		//------------------------

		RGBA8,			// 32 bpp
		XRGB8,			// 32 bpp

		//------------------------
		// Alpha channel only
		//------------------------

		// Alpha ends up being the same as L8A8 in our current implementation, because straight 
		// alpha gives 0 for color, but we want 1.
		Alpha,

		//------------------------
		// Luminance replicates the value across RGB with a constant A of 255
		// Intensity replicates the value across RGBA
		//------------------------

		L8A8,			// 16 bpp
		Luminance8,		//  8 bpp
		Intensity8,		//  8 bpp

		//------------------------
		// Compressed texture formats
		//------------------------

		Dxt1,			// 4 bpp
		Dxt5,			// 8 bpp

		//------------------------
		// Depth buffer formats
		//------------------------

		Depth,			// 24 bpp

		//------------------------
		//
		//------------------------

		X16,			// 16 bpp
		Y16X16,			// 32 bpp
		RGB565			// 16 bpp
	}

	public enum BImageColorFormat : int
	{
		/// <summary>
		/// RGBA.
		/// </summary>
		Default,

		/// <summary>
		/// XY format and use the fast DXT5 compressor.
		/// </summary>
		NormalDxt5,

		/// <summary>
		/// Convert RGBA to CoCg_Y format.
		/// </summary>
		YCoCgDxt5,

		/// <summary>
		/// Copy the alpha channel to green.
		/// </summary>
		GreenAlpha
	}
}