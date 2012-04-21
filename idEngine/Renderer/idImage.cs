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

using Microsoft.Xna.Framework.Graphics;

using idTech4.Math;

using Tao.OpenGl;

namespace idTech4.Renderer
{
	public sealed class idImage : IDisposable
	{
		#region Properties
		public bool AllowDownSize
		{
			get
			{
				return _allowDownSize;
			}
		}

		/// <summary>
		/// Just for resource profiling.
		/// </summary>
		public int Classification
		{
			get
			{
				return _classification;
			}
			set
			{
				_classification = value;
			}
		}

		public CubeFiles CubeFiles
		{
			get
			{
				return _cubeFiles;
			}
		}

		public TextureDepth Depth
		{
			get
			{
				return _depth;
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
			set
			{
				_generator = value;
			}
		}

		/// <summary>
		/// Gets if the default image was generated because a file couldn't be loaded.
		/// </summary>
		public bool IsDefaulted
		{
			get
			{
				return _defaulted;
			}
		}

		/// <summary>
		/// Gets whether or not the data has been loaded yet and sent off to the GPU.
		/// </summary>
		public bool IsLoaded
		{
			get
			{
				return (_texture != null);
			}
		}

		/*public bool IsPartialImage
		{
			get
			{
				return _isPartialImage;
			}
			internal set
			{
				_isPartialImage = value;
			}
		}*/
		
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
		
		public TextureType Type
		{
			get
			{
				return _type;
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

		private bool _backgroundLoadInProgress;
		private bool _defaulted;				// true if the default image was generated because a file couldn't be loaded
		private bool _allowDownSize;			// this also doubles as a don't-partially-load flag

		private bool _referencedOutsideLevelLoad;
		private bool _levelLoadReferenced;		// for determining if it needs to be purged

		private DateTime _timeStamp;			// the most recent of all images used in creation, for reloadImages command
		//private bool _precompressedFile;		// true when it was loaded from a .d3t file

		private Texture2D _texture;
		private int _width;
		private int _height;

		private TextureType _type;
		private TextureFilter _filter;
		private TextureRepeat _repeat;
		private TextureDepth _depth;
		private CubeFiles _cubeFiles;			// determines the naming and flipping conventions for the six images

		private ImageLoadCallback _generator;

		private int _frameUsed;					// for texture usage in frame statistics
		private int _bindCount;					// incremented each bind

		private int _classification;

		// background loading information.
		/*private idImage _partialImage;			// shrunken, space-saving version
		private bool _isPartialImage;			// true if this is pointed to by another image*/
		#endregion

		#region Constructor
		public idImage(string name, ImageLoadCallback generator)
		{
			_name = name;
			_generator = generator;
			_type = TextureType.Disabled;

			_filter = TextureFilter.Default;
			_repeat = TextureRepeat.Repeat;
			_depth = TextureDepth.Default;
			_cubeFiles = CubeFiles.TwoD;
		}

		public idImage(string name, TextureType type, TextureFilter filter, TextureRepeat repeat, TextureDepth depth, CubeFiles cubeMap, bool allowDownSize)
		{
			_name = name;
			_type = type;

			_filter = filter;
			_repeat = repeat;
			_depth = depth;
			_cubeFiles = cubeMap;
			_allowDownSize = allowDownSize;
		}

		~idImage()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Absolutely every image goes through this path.  On exit, the idImage will have a valid OpenGL texture number that can be bound.
		/// </summary>
		/// <param name="checkForPrecompressed"></param>
		/// <param name="fromBackEnd"></param>
		public void ActuallyLoadImage(bool checkForPrecompressed, bool fromBackEnd)
		{
			// this is the ONLY place generatorFunction will ever be called
			if(_generator != null)
			{
				_generator(this);
				return;
			}

			// if we are a partial image, we are only going to load from a compressed file
			/*if(_isPartialImage == true)
			{
				if(CheckPrecompressedImage(false) == true)
				{
					return;
				}

				// this is an error -- the partial image failed to load
				MakeDefault();
			}
			 
			//
			// load the image from disk
			//
			else */if(_cubeFiles != Renderer.CubeFiles.TwoD)
			{
				idConsole.Warning("TODO: cube files");
				/*byte	*pics[6];

				// we don't check for pre-compressed cube images currently
				R_LoadCubeImages( imgName, cubeFiles, pics, &width, &timestamp );

				if ( pics[0] == NULL ) {
					common->Warning( "Couldn't load cube image: %s", imgName.c_str() );
					MakeDefault();
					return;
				}

				GenerateCubeImage( (const byte **)pics, width, filter, allowDownSize, depth );
				precompressedFile = false;

				for ( int i = 0 ; i < 6 ; i++ ) {
					if ( pics[i] ) {
						R_StaticFree( pics[i] );
					}
				}*/
			}
			else
			{
				// see if we have a pre-generated image file that is
				// already image processed and compressed
				/*if((checkForPrecompressed == true) && (idE.CvarSystem.GetBool("image_usePrecompressedTextures") == true))
				{
					if(CheckPrecompressedImage(true) == true)
					{
						// we got the precompressed image
						return;
					}

					// fall through to load the normal image
				}*/
					   
				_texture = idE.ImageManager.LoadImageProgram(this.Name, ref _timeStamp, ref _depth);
					   
				if(_texture == null)
				{
					idConsole.Warning("Couldn't load image: {0}", this.Name);
					MakeDefault();

					return;
				}

				_width = _texture.Width;
				_height = _texture.Height;

				// build a hash for checking duplicate image files
				// NOTE: takes about 10% of image load times (SD)
				// may not be strictly necessary, but some code uses it, so let's leave it in
				// TODO: imageHash = MD4_BlockChecksum( pic, width * height * 4 );
				//Generate(data, width, height, _filter, _allowDownSize, _repeat, _depth);

				//_precompressedFile = false;

				// write out the precompressed version of this file if needed
				// TODO: WritePrecompressedImage();
			}
		}

		/// <summary>
		/// Automatically enables 2D mapping, cube mapping, or 3D texturing if needed.
		/// </summary>
		public void Bind()
		{
			// TODO
			/*if ( tr.logFile ) {
				RB_LogComment( "idImage::Bind( %s )\n", imgName.c_str() );
			}*/

			// if this is an image that we are caching, move it to the front of the LRU chain.
			/*if(_partialImage != null)
			{
				idConsole.Warning("TODO: Bind LRU _partialImage");

				/*if ( cacheUsageNext ) {
					// unlink from old position
					cacheUsageNext->cacheUsagePrev = cacheUsagePrev;
					cacheUsagePrev->cacheUsageNext = cacheUsageNext;
				}
				// link in at the head of the list
				cacheUsageNext = globalImages->cacheLRU.cacheUsageNext;
				cacheUsagePrev = &globalImages->cacheLRU;

				cacheUsageNext->cacheUsagePrev = this;
				cacheUsagePrev->cacheUsageNext = this;*/
			/*}*/

			// load the image if necessary (FIXME: not SMP safe!).
			if(this.IsLoaded == false)
			{
				/*if(_partialImage != null)
				{
					// if we have a partial image, go ahead and use that
					_partialImage.Bind();

					// start a background load of the full thing if it isn't already in the queue
					if(_backgroundLoadInProgress == false)
					{
						StartBackgroundLoad();
					}

					return;
				}*/

				// load the image on demand here, which isn't our normal game operating mode
				//ActuallyLoadImage(true, true); // check for precompressed, load is from back end
			}

			// bump our statistic counters
			_frameUsed = idE.Backend.FrameCount;
			_bindCount++;

			TextureUnit textureUnit = idE.Backend.GLState.TextureUnits[idE.Backend.GLState.CurrentTextureUnit];

			if(idE.Backend.GLState.CurrentTextureUnit < idE.Backend.GLState.TextureUnits.Length)
			{
				textureUnit.Type = _type;
				textureUnit.CurrentTexture = _texture;
				textureUnit.Filter = _filter;
				
			}
		}

		/// <summary>
		/// Used by callback functions to specify the actual data
		/// data goes from the bottom to the top line of the image, as OpenGL expects it
		/// These perform an implicit Bind() on the current texture unit.
		/// </summary>
		/// <remarks>
		/// The alpha channel bytes should be 255 if you don't want the channel.
		/// We need a material characteristic to ask for specific texture modes.
		/// Designed limitations of flexibility:
		/// No support for texture borders.
		/// No support for texture border color.
		/// No support for texture environment colors or GL_BLEND or GL_DECAL
		/// texture environments, because the automatic optimization to single
		/// or dual component textures makes those modes potentially undefined.
		/// No non-power-of-two images.
		/// No palettized textures.
		/// There is no way to specify separate wrap/clamp values for S and T.
		/// There is no way to specify explicit mip map levels.
		/// </remarks>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="filter"></param>
		/// <param name="allowDownSize"></param>
		/// <param name="repeat"></param>
		/// <param name="depth"></param>
		public void Generate(byte[] data, int width, int height, TextureFilter filter, bool allowDownSize, TextureRepeat repeat, TextureDepth depth)
		{
			// FIXME: should we implement cinematics this way, instead of with explicit calls?
			/*Purge();

			_filter = filter;
			_allowDownSize = allowDownSize;
			_repeat = repeat;
			_depth = depth;*/

			idConsole.Warning("TODO: generate");

			// if we don't have a rendering context, just return after we
			// have filled in the parms.  We must have the values set, or
			// an image match from a shader before OpenGL starts would miss
			// the generated texture
			if(idE.RenderSystem.IsRunning == false)
			{
				return;
			}

			// don't let mip mapping smear the texture into the clamped border
			/*bool preserveBorder = (_repeat == TextureRepeat.ClampToZero);

			// make sure it is a power of 2
			int scaledWidth = idHelper.MakePowerOfTwo(width);
			int scaledHeight = idHelper.MakePowerOfTwo(height);

			if((scaledWidth != width) || (scaledHeight != height))
			{
				idConsole.Error("Image.Generate: not a power of 2 image.");
			}

			// Optionally modify our width/height based on options/hardware			
			GetDownSize(ref scaledWidth, ref scaledHeight);

			byte[] scaledBuffer = null;
			/*idE.RenderSystem.CheckOpenGLErrors();
			uint[] tex = new uint[1];
			//Gl.glGenTextures(1, tex);
			_texNumber = (int) tex[0];
			idE.RenderSystem.CheckOpenGLErrors();*/
			/*_loaded = true;

			// select proper internal format before we resample
			_internalFormat = Gl.GL_RGB8;  // SelectInternalFormat(data, 1, width, height, depth, out _isMonochrome);

			// copy or resample data as appropriate for first MIP level.
			if((scaledWidth == width) && (scaledHeight == height))
			{
				// we must copy even if unchanged, because the border zeroing
				// would otherwise modify const data
				scaledBuffer = data;
			}
			else
			{
				idConsole.Warning("TODO: DONT SUPPORT MIMAP RIGHT NOW");

				// resample down as needed (FIXME: this doesn't seem like it resamples anymore!)
				// scaledBuffer = R_ResampleTexture( pic, width, height, width >>= 1, height >>= 1 );
				/*scaledBuffer = R_MipMap( pic, width, height, preserveBorder );
				width >>= 1;
				height >>= 1;
				if ( width < 1 ) {
					width = 1;
				}
				if ( height < 1 ) {
					height = 1;
				}

				while ( width > scaled_width || height > scaled_height ) {
					shrunk = R_MipMap( scaledBuffer, width, height, preserveBorder );
					R_StaticFree( scaledBuffer );
					scaledBuffer = shrunk;

					width >>= 1;
					height >>= 1;
					if ( width < 1 ) {
						width = 1;
					}
					if ( height < 1 ) {
						height = 1;
					}
				}

				// one might have shrunk down below the target size
				scaled_width = width;
				scaled_height = height;*/
			/*}*/

			/*_uploadWidth = scaledWidth;
			_uploadHeight = scaledHeight;
			_type = TextureType.TwoD;

			// zero the border if desired, allowing clamped projection textures
			// even after picmip resampling or careless artists.
			if(repeat == TextureRepeat.ClampToZero)
			{
				byte[] rgba = new byte[4] { 0, 0, 0, 255 };
				SetBorderTexels(scaledBuffer, width, height, rgba);
			}
			else if(repeat == TextureRepeat.ClampToZeroAlpha)
			{
				byte[] rgba = new byte[4] { 255, 255, 255, 0 };
				SetBorderTexels(scaledBuffer, width, height, rgba);
			}*/

			/*if((_generator == null) && ((_depth == TextureDepth.Bump) && (idE.CvarSystem.GetBool("image_writeNormalTGA") == true) || (_depth != TextureDepth.Bump) && (idE.CvarSystem.GetBool("image_writeTGA") == true)))
			{
				idConsole.Warning("TODO: gen = null && bump && write");
				// Optionally write out the texture to a .tga
				/*char filename[MAX_IMAGE_NAME];
				ImageProgramStringToCompressedFileName( imgName, filename );
				char *ext = strrchr(filename, '.');
				if ( ext ) {
					strcpy( ext, ".tga" );
					// swap the red/alpha for the write
					/*
					if ( depth == TD_BUMP ) {
						for ( int i = 0; i < scaled_width * scaled_height * 4; i += 4 ) {
							scaledBuffer[ i ] = scaledBuffer[ i + 3 ];
							scaledBuffer[ i + 3 ] = 0;
						}
					}
					*/
				// TODO: R_WriteTGA( filename, scaledBuffer, scaled_width, scaled_height, false );

				// put it back
				/*
				if ( depth == TD_BUMP ) {
					for ( int i = 0; i < scaled_width * scaled_height * 4; i += 4 ) {
						scaledBuffer[ i + 3 ] = scaledBuffer[ i ];
						scaledBuffer[ i ] = 0;
					}
				}
				*/
				/*}*/
		/*	}*/

			// swap the red and alpha for rxgb support
			// do this even on tga normal maps so we only have to use
			// one fragment program.
			// if the image is precompressed (either in palletized mode or true rxgb mode)
			// then it is loaded above and the swap never happens here.
			/*if((depth == TextureDepth.Bump) && (idE.CvarSystem.GetInteger("image_useNormalCompression") != 1))
			{
				for(int i = 0; i < scaledWidth * scaledHeight * 4; i += 4)
				{
					scaledBuffer[i + 3] = scaledBuffer[i];
					scaledBuffer[i] = 0;
				}
			}

			// upload the main image level
			Bind();

			if(_internalFormat == Gl.GL_COLOR_INDEX8_EXT)
			{
				idConsole.Warning("TODO: UploadCompressedNormalMap( scaled_width, scaled_height, scaledBuffer, 0 );");
			}
			else
			{
				//Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, _internalFormat, scaledWidth, scaledHeight, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, scaledBuffer);
			}

			// create and upload the mip map levels, which we do in all cases, even if we don't think they are needed
			/*int miplevel = 0;
			// TODO: remove this if not needed
			while((scaledWidth > 1) || (scaledHeight > 1))
			{
				// preserve the border after mip map unless repeating
				scaledBuffer = MipMap(scaledBuffer, scaledWidth, scaledHeight, preserveBorder);
				scaledWidth >>= 1;
				scaledHeight >>= 1;

				if(scaledWidth < 1)
				{
					scaledWidth = 1;
				}

				if(scaledHeight < 1)
				{
					scaledHeight = 1;
				}

				miplevel++;

				// this is a visualization tool that shades each mip map
				// level with a different color so you can see the
				// rasterizer's texture level selection algorithm
				// Changing the color doesn't help with lumminance/alpha/intensity formats...
				// TODO
				/*if ( depth == TD_DIFFUSE && globalImages->image_colorMipLevels.GetBool() ) {
					R_BlendOverTexture( (byte *)scaledBuffer, scaled_width * scaled_height, mipBlendColors[miplevel] );
				}*/

			// upload the mip map
			/*if(_internalFormat == Gl.GL_COLOR_INDEX8_EXT)
			{
				idConsole.Warning("TODO: UploadCompressedNormalMap( scaled_width, scaled_height, scaledBuffer, miplevel );");
			}
			else
			{
				//Gl.glTexImage2D(Gl.GL_TEXTURE_2D, miplevel, _internalFormat, scaledWidth, scaledHeight, 0, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, scaledBuffer);
			}
		}*/

			//SetImageFilterAndRepeat();
		}

		public void MakeDefault()
		{
			byte[, ,] data = new byte[idImageManager.DefaultImageSize, idImageManager.DefaultImageSize, 4];

			if(idE.CvarSystem.GetBool("developer") == true)
			{
				// grey center
				for(int y = 0; y < idImageManager.DefaultImageSize; y++)
				{
					for(int x = 0; x < idImageManager.DefaultImageSize; x++)
					{
						data[y, x, 0] = 32;
						data[y, x, 1] = 32;
						data[y, x, 2] = 32;
						data[y, x, 3] = 255;
					}
				}

				// white border
				for(int x = 0; x < idImageManager.DefaultImageSize; x++)
				{
					data[0, x, 0]
						= data[0, x, 1]
						= data[0, x, 2]
						= data[0, x, 3] = 255;

					data[x, 0, 0] = data[x, 0, 1]
						= data[x, 0, 2]
						= data[x, 0, 3] = 255;

					data[idImageManager.DefaultImageSize - 1, x, 0]
						= data[idImageManager.DefaultImageSize - 1, x, 1]
						= data[idImageManager.DefaultImageSize - 1, x, 2]
						= data[idImageManager.DefaultImageSize - 1, x, 3] = 255;

					data[x, idImageManager.DefaultImageSize - 1, 0]
						= data[x, idImageManager.DefaultImageSize - 1, 1]
						= data[x, idImageManager.DefaultImageSize - 1, 2]
						= data[x, idImageManager.DefaultImageSize - 1, 3] = 255;
				}
			}
			else
			{
				// completely black.
			}

			Generate(idHelper.Flatten<byte>(data), idImageManager.DefaultImageSize, idImageManager.DefaultImageSize, TextureFilter.Default, true, TextureRepeat.Repeat, TextureDepth.Default);

			_defaulted = true;
		}

		/// <summary>
		/// Frees the texture object, but leaves the structure so it can be reloaded.
		/// </summary>
		public void Purge()
		{
			if(this.IsLoaded == true)
			{
				/*_texture.Dispose();
				_texture = null;*/
			}

			// clear all the current binding caches, so the next bind will do a real one
			for(int i = 0; i < idE.Backend.GLState.TextureUnits.Length; i++)
			{
				idE.Backend.GLState.TextureUnits[i].CurrentTexture = null;
			}
		}

		public void Reload(bool checkPrecompressed, bool force)
		{
			// always regenerate functional images
			if(_generator != null)
			{
				idConsole.DeveloperWriteLine("regenerating {0}.", this.Name);
				_generator(this);
			}
			else
			{
				// check file times
				if(force == false)
				{
					//ID_TIME_T current;

					if(_cubeFiles != CubeFiles.TwoD)
					{
						idConsole.Warning("TODO: R_LoadCubeImages");
						//R_LoadCubeImages(imgName, cubeFiles, NULL, NULL, &current);
					}
					else
					{
						// get the current values
						idConsole.Warning("TODO: R_LoadImageProgram");
						//R_LoadImageProgram(imgName, NULL, NULL, NULL, &current);
					}

					/*if(current <= timestamp)
					{
						return;
					}*/
				}

				idConsole.DeveloperWriteLine("reloading {0}.", this.Name);

				Purge();

				// force no precompressed image check, which will cause it to be reloaded
				// from source, and another precompressed file generated.
				// Load is from the front end, so the back end must be synced
				ActuallyLoadImage(checkPrecompressed, false);
			}
		}
		#endregion

		#region Private
		/// <summary>
		/// If fullLoad is false, only the small mip levels of the image will be loaded.
		/// </summary>
		/// <returns></returns>
		private bool CheckPrecompressedImage(bool fullLoad)
		{
			if((idE.RenderSystem.IsRunning == false) || (idE.GLConfig.TextureCompressionAvailable == false))
			{
				return false;
			}

			/*#if 1 // ( _D3XP had disabled ) - Allow grabbing of DDS's from original Doom pak files
				// if we are doing a copyFiles, make sure the original images are referenced
				if ( fileSystem->PerformingCopyFiles() ) {
					return false;
				}
			#endif*/

			if((_depth == TextureDepth.Bump) && (idE.CvarSystem.GetInteger("image_useNormalCompression") != 2))
			{
				return false;
			}

			// god i love last minute hacks :-)
			if((idE.CvarSystem.GetInteger("com_machineSpec") >= 1) && (idE.CvarSystem.GetInteger("com_videoRam") >= 128) && (_name.StartsWith("lights/", StringComparison.OrdinalIgnoreCase) == true))
			{
				return false;
			}

			idConsole.Warning("TODO: CheckPrecompressedImage");


			/*char filename[MAX_IMAGE_NAME];
			ImageProgramStringToCompressedFileName( imgName, filename );

			// get the file timestamp
			ID_TIME_T precompTimestamp;
			fileSystem->ReadFile( filename, NULL, &precompTimestamp );


			if ( precompTimestamp == FILE_NOT_FOUND_TIMESTAMP ) {
				return false;
			}

			if ( !generatorFunction && timestamp != FILE_NOT_FOUND_TIMESTAMP ) {
				if ( precompTimestamp < timestamp ) {
					// The image has changed after being precompressed
					return false;
				}
			}

			timestamp = precompTimestamp;

			// open it and just read the header
			idFile *f;

			f = fileSystem->OpenFileRead( filename );
			if ( !f ) {
				return false;
			}

			int	len = f->Length();
			if ( len < sizeof( ddsFileHeader_t ) ) {
				fileSystem->CloseFile( f );
				return false;
			}

			if ( !fullLoad && len > globalImages->image_cacheMinK.GetInteger() * 1024 ) {
				len = globalImages->image_cacheMinK.GetInteger() * 1024;
			}

			byte *data = (byte *)R_StaticAlloc( len );

			f->Read( data, len );

			fileSystem->CloseFile( f );

			unsigned long magic = LittleLong( *(unsigned long *)data );
			ddsFileHeader_t	*_header = (ddsFileHeader_t *)(data + 4);
			int ddspf_dwFlags = LittleLong( _header->ddspf.dwFlags );

			if ( magic != DDS_MAKEFOURCC('D', 'D', 'S', ' ')) {
				common->Printf( "CheckPrecompressedImage( %s ): magic != 'DDS '\n", imgName.c_str() );
				R_StaticFree( data );
				return false;
			}

			// if we don't support color index textures, we must load the full image
			// should we just expand the 256 color image to 32 bit for upload?
			if ( ddspf_dwFlags & DDSF_ID_INDEXCOLOR && !glConfig.sharedTexturePaletteAvailable ) {
				R_StaticFree( data );
				return false;
			}

			// upload all the levels
			UploadPrecompressedImage( data, len );

			R_StaticFree( data );

			return true;*/

			return false;
		}

		/// <summary>
		/// Helper function that takes the current width/height and might make them smaller.
		/// </summary>
		/// <param name="scaledWidth"></param>
		/// <param name="scaledHeight"></param>
		private void GetDownSize(ref int scaledWidth, ref int scaledHeight)
		{
			int size = 0;

			// perform optional picmip operation to save texture memory
			if((_depth == TextureDepth.Specular) && (idE.CvarSystem.GetInteger("image_downSizeSpecular") > 0))
			{
				size = idE.CvarSystem.GetInteger("image_downSizeSpecular");

				if(size == 0)
				{
					size = 64;
				}
			}
			else if((_depth == TextureDepth.Bump) && (idE.CvarSystem.GetInteger("image_downSizeBump") > 0))
			{
				size = idE.CvarSystem.GetInteger("image_downSizeBump");

				if(size == 0)
				{
					size = 64;
				}
			}
			else if(((_allowDownSize == true) || (idE.CvarSystem.GetBool("image_forceDownSize") == true)) && (idE.CvarSystem.GetInteger("image_downSize") > 0))
			{
				size = idE.CvarSystem.GetInteger("image_downSizeLimit");

				if(size == 0)
				{
					size = 256;
				}
			}

			if(size > 0)
			{
				while((scaledWidth > size) || (scaledHeight > size))
				{
					if(scaledWidth > 1)
					{
						scaledWidth >>= 1;
					}

					if(scaledHeight > 1)
					{
						scaledHeight >>= 1;
					}
				}
			}

			// clamp to minimum size
			if(scaledWidth < 1)
			{
				scaledWidth = 1;
			}

			if(scaledHeight < 1)
			{
				scaledHeight = 1;
			}

			// clamp size to the hardware specific upper limit
			// scale both axis down equally so we don't have to
			// deal with a half mip resampling
			// This causes a 512*256 texture to sample down to
			// 256*128 on a voodoo3, even though it could be 256*256
			while((scaledWidth > idE.GLConfig.MaxTextureSize) || (scaledHeight > idE.GLConfig.MaxTextureSize))
			{
				scaledWidth >>= 1;
				scaledHeight >>= 1;
			}
		}

		/// <summary>
		/// Returns a new copy of the texture, quartered in size and filtered.
		/// </summary>
		/// <remarks>
		/// If a texture is intended to be used in GL_CLAMP or GL_CLAMP_TO_EDGE mode with
		/// a completely transparent border, we must prevent any blurring into the outer
		/// ring of texels by filling it with the border from the previous level.  This
		/// will result in a slight shrinking of the texture as it mips, but better than
		/// smeared clamps...
		/// </remarks>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="preserveBorder"></param>
		/// <returns></returns>
		private byte[] MipMap(byte[] data, int width, int height, bool preserveBorder)
		{
			if((width < 1) || (height < 1) || ((width + height) == 2))
			{
				idConsole.FatalError("MipMap called with size {0},{1}", width, height);
			}

			byte[] border = new byte[] {
				data[0], data[1], data[2], data[3]
			};

			int row = width * 4;
			int newWidth = width >> 1;
			int newHeight = height >> 1;

			if(newWidth == 0)
			{
				newWidth = 1;
			}

			if(newHeight == 0)
			{
				newHeight = 1;
			}

			byte[] newData = new byte[newWidth * newHeight * 4];
			int dataOffset = 0;
			int newDataOffset = 0;

			width >>= 1;
			height >>= 1;

			if((width == 0) || (height == 0))
			{
				width += height;	// get largest

				if(preserveBorder == true)
				{
					for(int i = 0; i < width; i++, newDataOffset += 4)
					{
						newData[newDataOffset] = border[0];
						newData[newDataOffset + 1] = border[1];
						newData[newDataOffset + 2] = border[2];
						newData[newDataOffset + 3] = border[3];
					}
				}
				else
				{
					for(int i = 0; i < width; i++, newDataOffset += 4, dataOffset += 8)
					{
						newData[newDataOffset] = (byte) ((data[dataOffset] + data[dataOffset + 4]) >> 1);
						newData[newDataOffset + 1] = (byte) ((data[dataOffset + 1] + data[dataOffset + 5]) >> 1);
						newData[newDataOffset + 2] = (byte) ((data[dataOffset + 2] + data[dataOffset + 6]) >> 1);
						newData[newDataOffset + 3] = (byte) ((data[dataOffset + 3] + data[dataOffset + 7]) >> 1);
					}
				}

				return newData;
			}

			for(int i = 0; i < height; i++, dataOffset += row)
			{
				for(int j = 0; j < width; j++, newDataOffset += 4, dataOffset += 8)
				{
					newData[newDataOffset] = (byte) ((data[dataOffset] + data[dataOffset + 4] + data[dataOffset + row] + data[dataOffset + row + 4]) >> 2);
					newData[newDataOffset + 1] = (byte) ((data[dataOffset + 1] + data[dataOffset + 5] + data[dataOffset + row + 1] + data[dataOffset + row + 5]) >> 2);
					newData[newDataOffset + 2] = (byte) ((data[dataOffset + 2] + data[dataOffset + 6] + data[dataOffset + row + 2] + data[dataOffset + row + 6]) >> 2);
					newData[newDataOffset + 3] = (byte) ((data[dataOffset + 3] + data[dataOffset + 7] + data[dataOffset + row + 3] + data[dataOffset + row + 7]) >> 2);
				}
			}

			// copy the old border texel back around if desired
			if(preserveBorder == true)
			{
				SetBorderTexels(newData, width, height, border);
			}

			return newData;
		}

		private int SelectInternalFormat(byte[] data, int dataSize, int width, int height, TextureDepth minimumDepth, out bool isMonochrome)
		{
			int offset = 0;
			int scanOffset = 0;
			int innerOffset = 0;

			// determine if the rgb channels are all the same
			// and if either all rgb or all alpha are 255
			int c = width * height;
			int rgbDiffer = 0;
			int rgbaDiffer = 0;
			int rgbOr = 0;
			int rgbAnd = -1;
			int aOr = 0;
			int aAnd = -1;

			// until shown otherwise
			isMonochrome = true;

			for(int side = 0; side < dataSize; side++)
			{
				scanOffset = side;
				innerOffset = 0;

				for(int i = 0; i < c; i++, innerOffset += 4)
				{
					int cOr, cAnd;

					offset = scanOffset + innerOffset;

					aOr |= data[offset + 3];
					aAnd &= data[offset + 3];

					cOr = data[offset] | data[offset + 1] | data[offset + 2];
					cAnd = data[offset] & data[offset + 1] & data[offset + 2];

					// if rgb are all the same, the or and and will match
					rgbDiffer |= (cOr ^ cAnd);

					// our "isMonochrome" test is more lax than rgbDiffer,
					// allowing the values to be off by several units and
					// still use the NV20 mono path
					if((idMath.Abs(data[offset] - data[offset + 1]) > 16)
						|| (idMath.Abs(data[offset] - data[offset + 2]) > 16))
					{
						isMonochrome = false;
					}

					rgbOr |= cOr;
					rgbAnd &= cAnd;

					cOr |= data[offset + 3];
					cAnd &= data[offset + 3];

					rgbaDiffer |= (cOr ^ cAnd);
				}
			}

			// we assume that all 0 implies that the alpha channel isn't needed,
			// because some tools will spit out 32 bit images with a 0 alpha instead
			// of 255 alpha, but if the alpha actually is referenced, there will be
			// different behavior in the compressed vs uncompressed states.
			bool needAlpha = ((aAnd != 255) && (aOr != 0));

			// catch normal maps first
			if(minimumDepth == TextureDepth.Bump)
			{
				if((idE.CvarSystem.GetBool("image_useCompression") == true) && (idE.CvarSystem.GetInteger("image_useNormalCompression") == 1) && idE.GLConfig.SharedTexturePaletteAvailable)
				{
					// image_useNormalCompression should only be set to 1 on nv_10 and nv_20 paths.
					return Gl.GL_COLOR_INDEX8_EXT;
				}
				else if((idE.CvarSystem.GetBool("image_useCompression") == true) && (idE.CvarSystem.GetInteger("image_useNormalCompression") > 0) && idE.GLConfig.TextureCompressionAvailable)
				{
					// image_useNormalCompression == 2 uses rxgb format which produces really good quality for medium settings.
					return Gl.GL_COMPRESSED_RGBA_S3TC_DXT5_EXT;
				}
				else
				{
					// we always need the alpha channel for bump maps for swizzling
					return Gl.GL_RGBA8;
				}
			}

			// allow a complete override of image compression with a cvar
			if(idE.CvarSystem.GetBool("image_useCompression") == false)
			{
				minimumDepth = TextureDepth.HighQuality;
			}

			if(minimumDepth == TextureDepth.Specular)
			{
				// we are assuming that any alpha channel is unintentional
				if(idE.GLConfig.TextureCompressionAvailable == true)
				{
					return Gl.GL_COMPRESSED_RGB_S3TC_DXT1_EXT;
				}
				else
				{
					return Gl.GL_RGB5;
				}
			}
			else if(minimumDepth == TextureDepth.Diffuse)
			{
				// we might intentionally have an alpha channel for alpha tested textures
				if(idE.GLConfig.TextureCompressionAvailable == true)
				{
					if(needAlpha == false)
					{
						return Gl.GL_COMPRESSED_RGB_S3TC_DXT1_EXT;
					}
					else
					{
						return Gl.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT;
					}
				}
				else if((aAnd == 255) || (aOr == 0))
				{
					return Gl.GL_RGB5;
				}
				else
				{
					return Gl.GL_RGBA4;
				}
			}

			// there will probably be some drivers that don't
			// correctly handle the intensity/alpha/luminance/luminance+alpha
			// formats, so provide a fallback that only uses the rgb/rgba formats
			if(idE.CvarSystem.GetBool("image_useAllFormats") == false)
			{
				// pretend rgb is varying and inconsistant, which
				// prevents any of the more compact forms
				rgbDiffer = 1;
				rgbaDiffer = 1;
				rgbAnd = 0;
			}

			// cases without alpha
			if(needAlpha == false)
			{
				if(minimumDepth == TextureDepth.HighQuality)
				{
					return Gl.GL_RGB8; // four bytes
				}

				if(idE.GLConfig.TextureCompressionAvailable == true)
				{
					return Gl.GL_COMPRESSED_RGB_S3TC_DXT1_EXT; // half byte
				}

				return Gl.GL_RGB5; // two bytes
			}

			// cases with alpha.
			if(rgbaDiffer == 0)
			{
				if((minimumDepth != TextureDepth.HighQuality) && (idE.GLConfig.TextureCompressionAvailable == true))
				{
					return Gl.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT; // one byte
				}

				return Gl.GL_INTENSITY8; // single byte for all channels
			}

			if(minimumDepth == TextureDepth.HighQuality)
			{
				return Gl.GL_RGBA8; // four bytes
			}

			if(idE.GLConfig.TextureCompressionAvailable == true)
			{
				return Gl.GL_COMPRESSED_RGBA_S3TC_DXT3_EXT; // one byte
			}

			if(rgbDiffer == 0)
			{
				return Gl.GL_LUMINANCE8_ALPHA8; // two bytes, max quality
			}

			return Gl.GL_RGBA4;	// two bytes.
		}

		private void SetBorderTexels(byte[] data, int width, int height, byte[] border)
		{
			int offset = 0;

			for(int i = 0; i < height; i++, offset += (width * 4))
			{
				data[offset] = border[0];
				data[offset + 1] = border[1];
				data[offset + 2] = border[2];
				data[offset + 3] = border[3];
			}

			offset = (width - 1) * 4;

			for(int i = 0; i < height; i++, offset += (width * 4))
			{
				data[offset] = border[0];
				data[offset + 1] = border[1];
				data[offset + 2] = border[2];
				data[offset + 3] = border[3];
			}

			offset = 0;

			for(int i = 0; i < width; i++, offset += 4)
			{
				data[offset] = border[0];
				data[offset + 1] = border[1];
				data[offset + 2] = border[2];
				data[offset + 3] = border[3];
			}

			offset = width * 4 * (height - 1);

			for(int i = 0; i < width; i++, offset += 4)
			{
				data[offset] = border[0];
				data[offset + 1] = border[1];
				data[offset + 2] = border[2];
				data[offset + 3] = border[3];
			}
		}

		private void SetImageFilterAndRepeat()
		{
			// set the minimize / maximize filtering.
			switch(_filter)
			{
				case TextureFilter.Default:
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, idE.ImageManager.MinTextureFilter);
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, idE.ImageManager.MaxTextureFilter);
					break;

				case TextureFilter.Linear:
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
					break;

				case TextureFilter.Nearest:
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
					break;
			}

			if(idE.GLConfig.AnisotropicAvailable == true)
			{
				// only do aniso filtering on mip mapped images.
				if(_filter == TextureFilter.Default)
				{
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAX_ANISOTROPY_EXT, idE.ImageManager.TextureAnisotropy);
				}
				else
				{
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAX_ANISOTROPY_EXT, 1);
				}
			}

			if(idE.GLConfig.TextureLodBiasAvailable == true)
			{
				//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_LOD_BIAS_EXT, idE.ImageManager.TextureLodBias);
			}

			// set the wrap/clamp modes.
			switch(_repeat)
			{
				case TextureRepeat.Repeat:
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
					break;

				case TextureRepeat.ClampToBorder:
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_BORDER);
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_BORDER);
					break;

				case TextureRepeat.ClampToZero:
				case TextureRepeat.ClampToZeroAlpha:
				case TextureRepeat.Clamp:
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
					//Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
					break;
			}
		}

		private void StartBackgroundLoad()
		{
			if(idE.CvarSystem.GetBool("image_showBackgroundLoads") == true)
			{
				idConsole.WriteLine("idImage.StartBackgroundLoad: {0}", _name); ;
			}

			_backgroundLoadInProgress = true;

			/*if(_precompressedFile == false)
			{
				idConsole.WriteLine("idImage.StartBackgroundLoad: {0} wasn't a precompressed file", _name);
				return;
			}*/

			idE.ImageManager.QueueBackgroundLoad(this);
		}
		#endregion
		#endregion

		#region IDisposable implementation
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException("idImage");
			}

			if(disposing == true)
			{
				if(this.IsLoaded == true)
				{
					Purge();
				}
			}

			_disposed = true;
		}
		#endregion
	}

	public enum TextureDepth
	{
		/// <summary>May be compressed, and always zeros the alpha channel.</summary>
		Specular,
		/// <summary>May be compressed.</summary>
		Diffuse,
		/// <summary>Will use compressed formats when possible.</summary>
		Default,
		/// <summary>May be compressed with 8 bit lookup.</summary>
		Bump,
		/// <summary>Either 32 bit or a component format, no loss at all.</summary>
		HighQuality
	}

	public enum TextureType
	{
		Disabled,
		TwoD,
		ThreeD,
		Cubic,
		Rectangle
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
		/// <summary>This should replace TR_CLAMP_TO_ZERO and TR_CLAMP_TO_ZERO_ALPHA but I don't want to risk changing it right now.</summary>
		ClampToBorder,
		/// <summary>Guarantee 0,0,0,255 edge for projected textures, set AFTER image format selection</summary>
		ClampToZero,
		/// <summary>Guarantee 0 alpha edge for projected textures, set AFTER image format selection</summary>
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
		Specular
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