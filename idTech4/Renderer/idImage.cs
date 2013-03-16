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
using System;
using System.IO;

using Microsoft.Xna.Framework.Graphics;

using idTech4.Services;

namespace idTech4.Renderer
{
	public class idImage
	{
		#region Properties
		public CubeFiles CubeFiles
		{
			get
			{

				return _cubeFiles;
			}
		}

		public TextureFilter Filter
		{
			get
			{
				return _filter;
			}
		}

		public ImageLoadCallback Generator
		{
			get
			{
				return _generator;
			}
		}

		public bool IsLoaded
		{
			get
			{
				return (_texture != null);
			}
		}

		public bool LevelLoadReferenced
		{
			get
			{
				return _levelLoadReferenced;
			}
			internal set
			{
				_levelLoadReferenced = value;
			}
		}

		/// <summary>
		/// Gets the name of this image.
		/// </summary>
		/// <remarks>
		/// Game path, including extension (except for cube maps), may be an image program.
		/// </remarks>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		public bool ReferencedOutsideLevelLoad
		{
			get
			{
				return _referencedOutsideLevelLoad;
			}
			set
			{
				_referencedOutsideLevelLoad = value;
			}
		}

		public TextureRepeat Repeat
		{
			get
			{
				return _repeat;
			}
		}

		public Texture2D Texture
		{
			get
			{
				return _texture;
			}
		}

		public TextureType Type
		{
			get
			{
				return _type;
			}
		}

		public TextureUsage Usage
		{
			get
			{
				return _usage;
			}
			internal set
			{
				_usage = value;
			}
		}

		public int Height
		{
			get
			{
				return _height;
			}
		}

		public int Width
		{
			get
			{
				return _width;
			}
		}
		#endregion

		#region Members
		private string _name;

		private TextureType _type;
		private TextureFilter _filter;
		private TextureRepeat _repeat;
		private TextureUsage _usage;
		private CubeFiles _cubeFiles;			// determines the naming and flipping conventions for the six images

		private ImageLoadCallback _generator;

		private Texture2D _texture;
		private int _width;
		private int _height;

		private bool _referencedOutsideLevelLoad;
		private bool _levelLoadReferenced;		// for determining if it needs to be purged
		private bool _defaulted;				// true if the default image was generated because a file couldn't be loaded
		private DateTime _sourceFileTime;		// the most recent of all images used in creation, for reloadImages command
		private DateTime _binaryFileTime;		// the time stamp of the binary file
		
		// only used for generating images
		private TextureFormat _format;
		private TextureColorFormat _colorFormat;
		private int _levelCount;
		private bool _gammaMips;
		#endregion

		#region Constructor
		public idImage(string name, TextureFilter filter, TextureRepeat repeat, TextureUsage usage, CubeFiles cubeMap)
		{
			_name      = name;
			_filter    = filter;
			_repeat    = repeat;
			_usage     = usage;
			_cubeFiles = cubeMap;

			_sourceFileTime = DateTime.MinValue;
			_binaryFileTime = DateTime.MinValue;
		}

		public idImage(string name, ImageLoadCallback generator)
		{
			_name      = name;
			_generator = generator;

			_filter    = TextureFilter.Default;
			_repeat    = TextureRepeat.Repeat;
			_usage     = TextureUsage.Default;
			_cubeFiles = CubeFiles.TwoD;

			_sourceFileTime = DateTime.MinValue;
			_binaryFileTime = DateTime.MinValue;
		}
		#endregion

		#region Methods
		#region Generating
		public void Generate(byte[] data, int width, int height, TextureFilter filter, TextureRepeat repeat, TextureUsage usage)
		{
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();

			Purge();

			_filter    = filter;
			_repeat    = repeat;
			_usage     = usage;
			_cubeFiles = CubeFiles.TwoD;

			_type       = TextureType.TwoD;
			_width      = width;
			_height     = height;

			DeriveOptions();

			if(_gammaMips == true)
			{
				idLog.WriteLine("TODO: gamma mips");
			}

			SurfaceFormat surfaceFormat = SurfaceFormat.Color;

			switch(_format)
			{
				case TextureFormat.RGBA8:
				case TextureFormat.XRGB8:
				case TextureFormat.L8A8:
				case TextureFormat.Luminance8:
				case TextureFormat.Depth:
				case TextureFormat.X16:
				case TextureFormat.Y16X16:
					idEngine.Instance.Error("unsupported texture format: {0}", _format);
					break;

				case TextureFormat.Alpha:
					surfaceFormat = SurfaceFormat.Alpha8;
					break;

				case TextureFormat.Dxt1:
					surfaceFormat = SurfaceFormat.Dxt1;
					break;

				case TextureFormat.Dxt5:
					//surfaceFormat = SurfaceFormat.Dxt5;
					surfaceFormat = SurfaceFormat.Color;
					break;

				case TextureFormat.RGB565:
					surfaceFormat = SurfaceFormat.Bgr565;
					break;

				case TextureFormat.Intensity8:
					surfaceFormat = SurfaceFormat.Color;
					break;
			}

			_texture = renderSystem.CreateTexture(width, height, false, surfaceFormat);
			_texture.SetData<byte>(data);
		}
		#endregion

		#region Loading
		/// <summary>
		/// Absolutely every image goes through this path.  On exit, the idImage will have a valid texture that can be bound.
		/// </summary>
		/// <param name="checkForPrecompressed"></param>
		/// <param name="fromBackEnd"></param>
		public void ActuallyLoadImage(bool fromBackEnd)
		{
			// this is the ONLY place generatorFunction will ever be called
			if(_generator != null)
			{
				_generator(this);
				return;
			}

			ICVarSystem cvarSystem     = idEngine.Instance.GetService<ICVarSystem>();
			IImageManager imageManager = idEngine.Instance.GetService<IImageManager>();
			IFileSystem fileSystem     = idEngine.Instance.GetService<IFileSystem>();

			if(cvarSystem.GetInt("com_productionMode") != 0)
			{
				_sourceFileTime = DateTime.MinValue;

				if(_cubeFiles != CubeFiles.TwoD)
				{
					_type   = TextureType.Cubic;
					_repeat = TextureRepeat.Clamp;
				}
				else
				{
					_type = TextureType.TwoD;
				}
			}
			else
			{
				if(_cubeFiles != CubeFiles.TwoD)
				{
					_type   = TextureType.Cubic;
					_repeat = TextureRepeat.Clamp;

					idLog.WriteLine("TODO: R_LoadCubeImages(GetName(), cubeFiles, NULL, NULL, &sourceFileTime);");
				}
				else
				{
					_type = TextureType.TwoD;
					//idLog.WriteLine("TODO: imageManager.LoadImageProgram(this.Name, ref _sourceFileTime, ref _usage);");
				}
			}

			// figure out opts.colorFormat and opts.format so we can make sure the binary image is up to date

			// TODO: we don't support generating binary images
			/*DeriveOpts();*/

			string generatedName = GetGeneratedName(this.Name, this.Usage, this.CubeFiles);

			// BFHACK, do not want to tweak on buildgame so catch these images here
			if(fileSystem.FileExists(generatedName + ".xnb") == false)
			{
				int c = 1;

				while(c-- > 0)
				{
					if(generatedName.Contains("guis/assets/white#__0000") == true)
					{
						generatedName = generatedName.Replace("white#__0000", "white#__0200");
						// TODO: binaryFileTime = im.LoadFromGeneratedFile( sourceFileTime );
						break;
					}
					else if(generatedName.Contains("guis/assets/white#__0100") == true)
					{
						generatedName = generatedName.Replace("white#__0100", "white#__0200");
						// TODO: binaryFileTime = im.LoadFromGeneratedFile( sourceFileTime );
						break;
					}
					else if(generatedName.Contains("textures/black#__0100") == true)
					{
						generatedName = generatedName.Replace("black#__0100", "black#__0200");
						// TODO: binaryFileTime = im.LoadFromGeneratedFile( sourceFileTime );
						break;
					}
					else if(generatedName.Contains("textures/decals/bulletglass1_d#__0100") == true)
					{
						generatedName = generatedName.Replace("bulletglass1_d#__0100", "bulletglass1_d#__0200");
						// TODO: binaryFileTime = im.LoadFromGeneratedFile( sourceFileTime );
						break;
					}
					else if(generatedName.Contains("models/monsters/skeleton/skeleton01_d#__1000") == true)
					{
						generatedName = generatedName.Replace("skeleton01_d#__1000", "skeleton01_d#__0100");
						// TODO: binaryFileTime = im.LoadFromGeneratedFile( sourceFileTime );
						break;
					}
				}
			}

			_texture = imageManager.LoadImage(generatedName, ref _binaryFileTime);

			if(_texture != null)
			{
				_width = _texture.Width;
				_height = _texture.Height;

				if(cvarSystem.GetBool("fs_buildresources") == true)
				{
					// for resource gathering write this image to the preload file for this map
					idLog.WriteLine("TODO: fileSystem->AddImagePreload( GetName(), filter, repeat, usage, cubeFiles );");
				}
			}
		}
		#endregion

		#region Misc
		/// <summary>
		/// The default image will be grey with a white box outline
		/// to allow you to see the mapping coordinates on a surface.
		/// </summary>
		public void MakeDefault()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			byte[,,] data          = new byte[Constants.DefaultImageSize, Constants.DefaultImageSize, 4];

			if(cvarSystem.GetBool("developer") == true)
			{
				// grey center
				for(int x = 0; x < data.GetUpperBound(0) + 1; x++)
				{
					for(int y = 0; y < data.GetUpperBound(1) + 1; y++)
					{
						data[x, y, 0] =
							data[x, y, 1] =
							data[x, y, 2] = 32;
						data[x, y, 3] = 255;
					}
				}
				
				// white border
				for(int x = 0; x < Constants.DefaultImageSize; x++)
				{
					data[0, x, 0] =
						data[0, x, 1] =
						data[0, x, 2] =
						data[0, x, 3] = 255;

					data[x, 0, 0] =
						data[x, 0, 1] =
						data[x, 0, 2] =
						data[x, 0, 3] = 255;

					data[Constants.DefaultImageSize - 1, x, 0] =
						data[Constants.DefaultImageSize - 1, x, 1] =
						data[Constants.DefaultImageSize - 1, x, 2] =
						data[Constants.DefaultImageSize - 1, x, 3] = 255;

					data[x, Constants.DefaultImageSize - 1, 0] =
						data[x, Constants.DefaultImageSize - 1, 1] =
						data[x, Constants.DefaultImageSize - 1, 2] =
						data[x, Constants.DefaultImageSize - 1, 3] = 255;
				}
			} 
			else 
			{
				for(int x = 0; x < data.GetUpperBound(0) + 1; x++)
				{
					for(int y = 0; y < data.GetUpperBound(1) + 1; y++)
					{
						data[x, y, 0] =
							data[x, y, 1] =
							data[x, y, 2] = 0;
						data[x, y, 3] = 255;
					}
				}
			}

			Generate(idHelper.Flatten(data), Constants.DefaultImageSize, Constants.DefaultImageSize, TextureFilter.Default, TextureRepeat.Repeat, TextureUsage.Default);

			_defaulted = true;
		}

		private void DeriveOptions()
		{
			_colorFormat = TextureColorFormat.Default;

			switch(_usage)
			{
				case TextureUsage.Coverage:
					_format      = TextureFormat.Dxt1;
					_colorFormat = TextureColorFormat.GreenAlpha;
					break;

				case TextureUsage.Depth:
					_format = TextureFormat.Depth;
					break;

				case TextureUsage.Diffuse:
					// TD_DIFFUSE gets only set to when its a diffuse texture for an interaction
					_gammaMips   = true;
					_format      = TextureFormat.Dxt5;
					_colorFormat = TextureColorFormat.YCoCgDxt5;
					break;

				case TextureUsage.Specular:
					_gammaMips   = true;
					_format      = TextureFormat.Dxt1;
					_colorFormat = TextureColorFormat.Default;
					break;

				case TextureUsage.Default:
					_gammaMips   = true;
					_format      = TextureFormat.Dxt5;
					_colorFormat = TextureColorFormat.Default;
					break;

				case TextureUsage.Bump:
					_format = TextureFormat.Dxt5;
					_colorFormat = TextureColorFormat.NormalDxt5;
					break;

				case TextureUsage.Font:
					_format      = TextureFormat.Dxt1;
					_colorFormat = TextureColorFormat.GreenAlpha;
					_levelCount  = 4; // we only support 4 levels because we align to 16 in the exporter
					_gammaMips   = true;
					break;

				case TextureUsage.Light:
					_format    = TextureFormat.RGB565;
					_gammaMips = true;
					break;

				case TextureUsage.LookupTableMono:
					_format = TextureFormat.Intensity8;
					break;

				case TextureUsage.LookupTableAlpha:
					_format = TextureFormat.Alpha;
					break;

				case TextureUsage.LookupTableRGB1:
				case TextureUsage.LookupTableRGBA:
					_format = TextureFormat.RGBA8;
					break;

				default:
					_format = TextureFormat.RGBA8;
					break;
			}
			
			if(_levelCount == 0)
			{
				_levelCount = 1;

				if((_filter == TextureFilter.Linear) || (_filter == TextureFilter.Nearest))
				{
					// don't create mip maps if we aren't going to be using them
				}
				else
				{
					int tempWidth  = _width;
					int tempHeight = _height;

					while((tempWidth > 1) || (tempHeight > 1))
					{
						tempWidth  >>= 1;
						tempHeight >>= 1;

						if(((_format == TextureFormat.Dxt1) || (_format == TextureFormat.Dxt5))
							&& (((tempWidth & 0x3) != 0) || ((tempHeight & 0x3) != 0)))
						{
							break;
						}

						_levelCount++;
					}
				}
			}
		}

		public void Purge()
		{
			idLog.Warning("TODO: image.purge");

			if(_texture != null)
			{
				// TODO: because the content manager doesn't actually remove the texture, we don't support
				// purging/reloading right now.
				//idConsole.Warning("TODO: _texture.Dispose();");
				_texture = null;
			}

			// clear all the current binding caches, so the next bind will do a real one
			/*for(int i = 0; i < MAX_MULTITEXTURE_UNITS; i++)
			{
				backEnd.glState.tmu[i].current2DMap = TEXTURE_NOT_LOADED;
				backEnd.glState.tmu[i].currentCubeMap = TEXTURE_NOT_LOADED;
			}*/
		}

		private string GetGeneratedName(string name, TextureUsage usage, CubeFiles cubeFiles)
		{
			name = Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name));
			name += string.Format("#__{0:00}{1:00}", (int) usage, (int) cubeFiles);
			
			name = "generated/images/" + name;
			name = name.Replace("(", "/")
					.Replace(",", "/")
					.Replace(")", "")
					.Replace(" ", "");

			return name;
		}
		#endregion
		#endregion
	}

	public enum TextureFormat : int
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

	public enum TextureColorFormat : int
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

	public enum TextureType
	{
		Disabled,
		TwoD,
		Cubic
	}

	public enum TextureUsage
	{
		/// <summary>May be compressed, and always zeros the alpha channel.</summary>
		Specular,
		/// <summary>May be compressed.</summary>
		Diffuse,
		/// <summary>Will use compressed formats when possible.</summary>
		Default,
		/// <summary>May be compressed with 8 bit lookup.</summary>
		Bump,
		Font,
		Light,
		/// <summary>Mono lookup table (including alpha).</summary>
		LookupTableMono,
		/// <summary>Alpha lookup table with a white color channel.</summary>
		LookupTableAlpha,
		/// <summary>RGB lookup table with a solid white alpha.</summary>
		LookupTableRGB1,
		/// <summary>RGBA lookup table.</summary>
		LookupTableRGBA,
		/// <summary>Coverage map for fill depth pass when YCoCG is used.</summary>
		Coverage,
		/// <summary>Depth buffer copy for motion blur.</summary>
		Depth
	}

	public enum TextureFilter
	{
		Linear,
		Nearest,
		/// <summary>Use the user-specified r_textureFilter.</summary>
		Default
	}

	public enum TextureRepeat
	{
		Repeat,
		Clamp,
		/// <summary>Guarantee 0,0,0,255 edge for projected textures.</summary>
		ClampToZero,
		/// <summary>Guarantee 0 alpha edge for projected textures.</summary>
		ClampToZeroAlpha
	}

	public enum CubeFiles
	{
		/// <summary>Not a cube map.</summary>
		TwoD,
		/// <summary>_px, _nx, _py, etc, directly sent to the renderer.</summary>
		Native,
		/// <summary>_forward, _back, etc, rotated and flipped as needed before sending to the renderer.</summary>
		Camera
	}

	public enum DynamicImageType
	{
		Static,
		Scratch, // video, screen wipe, etc.
		CubeRender,
		MirrorRender,
		XRayRender,
		RemoteRender
	}

	public enum TextureCoordinateGeneration
	{
		Explicit,
		DiffuseCube,
		ReflectCube,
		SkyboxCube,
		WobbleSkyCube,
		Screen, // screen aligned, for mirrorRenders and screen space temporaries.
		Screen2,
		GlassWarp
	}

	public enum StageLighting
	{
		Ambient, // execute after lighting.
		Bump,
		Diffuse,
		Specular,
		Coverage
	}

	/// <summary>
	/// Cross-blended terrain textures need to modulate the color by the vertex color to smoothly blend between two textures.
	/// </summary>
	public enum StageVertexColor
	{
		Ignore,
		Modulate,
		InverseModulate
	}
}