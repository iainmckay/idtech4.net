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

		private Texture2D _texture;
		private int _width;
		private int _height;

		private bool _referencedOutsideLevelLoad;
		private bool _levelLoadReferenced;		// for determining if it needs to be purged
		private bool _defaulted;				// true if the default image was generated because a file couldn't be loaded
		private DateTime _sourceFileTime;		// the most recent of all images used in creation, for reloadImages command
		private DateTime _binaryFileTime;		// the time stamp of the binary file
		#endregion

		#region Constructor
		public idImage(string name, TextureFilter filter, TextureRepeat repeat, TextureUsage usage, CubeFiles cubeMap)
		{
			_name = name;
			_filter = filter;
			_repeat = repeat;
			_usage = usage;
			_cubeFiles = cubeMap;

			_sourceFileTime = DateTime.MinValue;
			_binaryFileTime = DateTime.MinValue;
		}
		#endregion

		#region Methods
		#region Loading
		/// <summary>
		/// Absolutely every image goes through this path.  On exit, the idImage will have a valid texture that can be bound.
		/// </summary>
		/// <param name="checkForPrecompressed"></param>
		/// <param name="fromBackEnd"></param>
		public void ActuallyLoadImage(bool fromBackEnd)
		{
			// this is the ONLY place generatorFunction will ever be called
			// TODO: generator
			/*if(_generator != null)
			{
				_generator(this);
				return;
			}*/

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
					idLog.WriteLine("TODO: imageManager.LoadImageProgram(this.Name, ref _sourceFileTime, ref _usage);");
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

			if(/*(fileSystem.InProductionMode == true) && */(_texture != null))
			{
				_width = _texture.Width;
				_height = _texture.Height;

				if(cvarSystem.GetBool("fs_buildresources") == true)
				{
					// for resource gathering write this image to the preload file for this map
					idLog.WriteLine("TODO: fileSystem->AddImagePreload( GetName(), filter, repeat, usage, cubeFiles );");
				}
			}

			// TODO: we don't support bimage generation
			/* else {
				if ( cubeFiles != CF_2D ) {
					int size;
					byte * pics[6];

					if ( !R_LoadCubeImages( GetName(), cubeFiles, pics, &size, &sourceFileTime ) || size == 0 ) {
						idLib::Warning( "Couldn't load cube image: %s", GetName() );
						return;
					}

					opts.textureType = TT_CUBIC;
					repeat = TR_CLAMP;
					opts.width = size;
					opts.height = size;
					opts.numLevels = 0;
					DeriveOpts();
					im.LoadCubeFromMemory( size, (const byte **)pics, opts.numLevels, opts.format, opts.gammaMips );
					repeat = TR_CLAMP;

					for ( int i = 0; i < 6; i++ ) {
						if ( pics[i] ) {
							Mem_Free( pics[i] );
						}
					}
				} else {
					int width, height;
					byte * pic;

					// load the full specification, and perform any image program calculations
					R_LoadImageProgram( GetName(), &pic, &width, &height, &sourceFileTime, &usage );

					if ( pic == NULL ) {
						idLib::Warning( "Couldn't load image: %s : %s", GetName(), generatedName.c_str() );
						// create a default so it doesn't get continuously reloaded
						opts.width = 8;
						opts.height = 8;
						opts.numLevels = 1;
						DeriveOpts();
						AllocImage();
				
						// clear the data so it's not left uninitialized
						idTempArray<byte> clear( opts.width * opts.height * 4 );
						memset( clear.Ptr(), 0, clear.Size() );
						for ( int level = 0; level < opts.numLevels; level++ ) {
							SubImageUpload( level, 0, 0, 0, opts.width >> level, opts.height >> level, clear.Ptr() );
						}

						return;
					}

					opts.width = width;
					opts.height = height;
					opts.numLevels = 0;
					DeriveOpts();
					im.Load2DFromMemory( opts.width, opts.height, pic, opts.numLevels, opts.format, opts.colorFormat, opts.gammaMips );

					Mem_Free( pic );
				}
				binaryFileTime = im.WriteGeneratedFile( sourceFileTime );
			}*/
		}
		#endregion

		#region Misc
		private string GetGeneratedName(string name, TextureUsage usage, CubeFiles cubeFiles)
		{
			name = Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name));
			name += string.Format("#__{0:00}{1:00}", (int) usage, (int) cubeFiles);
			
			return "generated/images/" + name;
		}
		#endregion
		#endregion
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