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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Tao.OpenGl;

using idTech4.IO;
using idTech4.Math;
using idTech4.Text;

using XTextureFilter = Microsoft.Xna.Framework.Graphics.TextureFilter;

namespace idTech4.Renderer
{
	public class idImageManager
	{
		#region Constants
		public const int DefaultImageSize = 16;

		public const int QuadraticWidth = 32;
		public const int QuadraticHeight = 4;

		public const int FalloffTextureSize = 64;

		public const int FogSize = 128;
		public const int FogEnterSize = 64;
		public const float FogEnter = (FogEnterSize + 1.0f) / (FogEnterSize * 2);

		public static Dictionary<string, ImageFilter> ImageFilters = new Dictionary<string, ImageFilter>();
		#endregion

		#region Properties
		public idImage CurrentRenderImage
		{
			get
			{
				return _currentRenderImage;
			}
		}

		public idImage DefaultImage
		{
			get
			{
				return _defaultImage;
			}
		}

		public SamplerState DefaultClampTextureSampler
		{
			get
			{
				return _defaultClampTextureSampler;
			}
		}

		public SamplerState DefaultRepeatTextureSampler
		{
			get
			{
				return _defaultRepeatTextureSampler;
			}
		}

		public SamplerState LinearClampTextureSampler
		{
			get
			{
				return _linearClampTextureSampler;
			}
		}

		public SamplerState LinearRepeatTextureSampler
		{
			get
			{
				return _linearRepeatTextureSampler;
			}
		}

		public SamplerState NearestClampTextureSampler
		{
			get
			{
				return _nearestClampTextureSampler;
			}
		}

		public SamplerState NearestRepeatTextureSampler
		{
			get
			{
				return _nearestRepeatTextureSampler;
			}
		}

		public idImage WhiteImage
		{
			get
			{
				return _whiteImage;
			}
		}
		#endregion

		#region Members
		private List<idImage> _images = new List<idImage>();
		private Dictionary<string, idImage> _imageDictionary = new Dictionary<string, idImage>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<idImage, BackgroundDownload> _backgroundImageLoads = new Dictionary<idImage, BackgroundDownload>();

		private idImage _defaultImage;
		private idImage _flatNormalMap;					// 128 128 255 in all pixels
		private idImage _ambientNormalMap;				// tr.ambientLightVector encoded in all pixels
		private idImage _rampImage;						// 0-255 in RGBA in S
		private idImage _alphaRampImage;				// 0-255 in alpha, 255 in RGB
		private idImage _alphaNotchImage;				// 2x1 texture with just 1110 and 1111 with point sampling
		private idImage _whiteImage;					// full of 0xff
		private idImage _blackImage;					// full of 0x00
		private idImage _normalCubeMapImage;			// cube map to normalize STR into RGB
		private idImage _noFalloffImage;				// all 255, but zero clamped
		private idImage _fogImage;						// increasing alpha is denser fog
		private idImage _fogEnterImage;					// adjust fogImage alpha based on terminator plane
		private idImage _cinematicImage;
		private idImage _scratchImage;
		private idImage _scratchImage2;
		private idImage _accumImage;
		private idImage _currentRenderImage;			// for SS_POST_PROCESS shaders
		private idImage _scratchCubeMapImage;
		private idImage _specularTableImage;			// 1D intensity texture with our specular function
		private idImage _specular2DTableImage;			// 2D intensity texture with our specular function with variable specularity
		private idImage _borderClampImage;				// white inside, black outside

		private SamplerState _defaultClampTextureSampler;
		private SamplerState _linearClampTextureSampler;
		private SamplerState _nearestClampTextureSampler;
		private SamplerState _defaultRepeatTextureSampler;
		private SamplerState _linearRepeatTextureSampler;
		private SamplerState _nearestRepeatTextureSampler;

		private bool _insideLevelLoad;					// don't actually load images now
		#endregion

		#region Constructor
		public idImageManager()
		{
			InitFilters();
			InitCvars();
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Mark all file based images as currently unused,
		/// but don't free anything.  Calls to ImageFromFile() will
		/// either mark the image as used, or create a new image without
		/// loading the actual data.
		/// </summary>
		public void BeginLevelLoad()
		{
			_insideLevelLoad = true;

			foreach(idImage image in _images)
			{
				// generator function images are always kept around
				if(image.Generator != null)
				{
					continue;
				}

				if(idE.CvarSystem.GetBool("com_purgeAll") == true)
				{
					image.Purge();
				}

				image.LevelLoadReferenced = false;;
			}
		}

		/// <summary>
		/// Free all images marked as unused, and load all images that are necessary.
		/// This architecture prevents us from having the union of two level's
		/// worth of data present at one time.
		/// </summary>
		public void EndLevelLoad()
		{
			_insideLevelLoad = false;

			if(idE.CvarSystem.GetInteger("net_serverDedicated") != 0)
			{
				return;
			}

			idConsole.WriteLine("----- idImageManager::EndLevelLoad -----");

			int start = idE.System.Milliseconds;
			int	purgeCount = 0;
			int	keepCount = 0;
			int	loadCount = 0;

			// purge the ones we don't need
			foreach(idImage image in _images)
			{
				if(image.Generator != null)
				{
					continue;
				}

				if((image.LevelLoadReferenced == false) && (image.ReferencedOutsideLevelLoad == false))
				{
					purgeCount++;
					image.Purge();
				}
				else if(image.IsLoaded == true)
				{
					keepCount++;
				}
			}

			// load the ones we do need, if we are preloading
			foreach(idImage image in _images)
			{
				if(image.Generator != null)
				{
					continue;
				}

				if((image.LevelLoadReferenced == true) && (image.IsLoaded == false))
				{
					loadCount++;
					image.ActuallyLoadImage(true, false);

					if((loadCount & 15) == 0)
					{
						// TODO: session->PacifierUpdate();
					}
				}
			}

			int end = idE.System.Milliseconds;

			idConsole.WriteLine("{0} purged from previous", purgeCount);
			idConsole.WriteLine("{0} kept from previous", keepCount);
			idConsole.WriteLine("{0} new loaded", loadCount);
			idConsole.WriteLine("all images loaded in {0:0} seconds", (end - start) * 0.001);
			idConsole.WriteLine("----------------------------------------");
		}

		/// <summary>
		/// Finds or loads the given image, always returning a valid image pointer.
		/// Loading of the image may be deferred for dynamic loading.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="filter"></param>
		/// <param name="allowDownSize"></param>
		/// <param name="repeat"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		public idImage ImageFromFile(string name, TextureFilter filter, bool allowDownSize, TextureRepeat repeat, TextureDepth depth)
		{
			return ImageFromFile(name, filter, allowDownSize, repeat, depth, CubeFiles.TwoD);
		}

		/// <summary>
		/// Finds or loads the given image, always returning a valid image pointer.
		/// Loading of the image may be deferred for dynamic loading.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="filter"></param>
		/// <param name="allowDownSize"></param>
		/// <param name="repeat"></param>
		/// <param name="depth"></param>
		/// <param name="cubeMap"></param>
		/// <returns></returns>
		public idImage ImageFromFile(string name, TextureFilter filter, bool allowDownSize, TextureRepeat repeat, TextureDepth depth, CubeFiles cubeMap)
		{
			if((name == null) || (name == string.Empty)
				|| (name.Equals("default", StringComparison.OrdinalIgnoreCase) == true)
				|| (name.Equals("_default", StringComparison.OrdinalIgnoreCase) == true))
			{
				idE.DeclManager.MediaPrint("DEFAULTED");

				return this.DefaultImage;
			}

			idImage image;

			// strip any .tga file extensions from anywhere in the _name, including image program parameters
			name = name.Replace(".tga", "");

			//
			// see if the image is already loaded, unless we
			// are in a reloadImages call
			//
			if(_imageDictionary.TryGetValue(name, out image) == true)
			{
				// the built in's, like _white and _flat always match the other options
				if(name.StartsWith("_") == true)
				{
					return image;
				}

				if(image.CubeFiles != cubeMap)
				{
					idConsole.Error("Image '{0}' has been referenced with conflicting cube map states", name);
				}

				if((image.Filter != filter) || (image.Repeat != repeat))
				{
					// we might want to have the system reset these parameters on every bind and
					// share the image data	

					// FIXME: this might be the wrong behaviour.  original d3 would return a new image but our dictionary
					// requires unique keys.
					return image;
				}
				else
				{
					if((image.AllowDownSize == allowDownSize) && (image.Depth == depth))
					{
						// note that it is used this level load
						image.LevelLoadReferenced = true;

						return image;
					}

					// the same image is being requested, but with a different allowDownSize or depth
					// so pick the highest of the two and reload the old image with those parameters
					if(image.AllowDownSize == false)
					{
						allowDownSize = false;
					}

					if(image.Depth > depth)
					{
						depth = image.Depth;
					}

					if((image.AllowDownSize == allowDownSize) && (image.Depth == depth))
					{
						// the already created one is already the highest quality
						image.LevelLoadReferenced = true;

						return image;
					}

					/*image.AllowDownSize = allowDownSize;
					image.Depth = depth;*/
					image.LevelLoadReferenced = true;


					if((idE.CvarSystem.GetBool("image_preload") == true) && (_insideLevelLoad == false))
					{
						image.ReferencedOutsideLevelLoad = true;
						image.ActuallyLoadImage(true, false); // check for precompressed, load is from front end

						idE.DeclManager.MediaPrint("{0}x{1} {1} (reload for mixed referneces)", image.Width, image.Height, image.Name);
					}

					return image;
				}
			}

			// HACK: to allow keep fonts from being mip'd, as new ones will be introduced with localization
			// this keeps us from having to make a material for each font tga
			if(name.Contains("fontImage_") == true)
			{
				allowDownSize = false;
			}

			//
			// create a new image
			//
			image = CreateImage(name, TextureType.TwoD, filter, repeat, depth, cubeMap, allowDownSize);
			image.LevelLoadReferenced = true;

			// load it if we aren't in a level preload
			if((idE.CvarSystem.GetBool("image_preload") == true) && (_insideLevelLoad == false))
			{
				image.ReferencedOutsideLevelLoad = true;
				image.ActuallyLoadImage(true, false); // check for precompressed, load is from front end

				idE.DeclManager.MediaPrint("{0}x{1} {2}", image.Width, image.Height, image.Name);
			}
			else
			{
				idE.DeclManager.MediaPrint(image.Name);
			}

			return image;
		}

		/// <summary>
		/// Disable the active texture unit.
		/// </summary>
		public void BindNullTexture()
		{
			TextureUnit unit = idE.Backend.GLState.TextureUnits[idE.Backend.GLState.CurrentTextureUnit];
			unit.CurrentTexture = null;
			unit.Type = TextureType.Disabled;
		}

		/// <summary>
		/// Resets filtering on all loaded images.  New images will automatically pick up the current values.
		/// </summary>
		public void ChangeTextureFilter()
		{
			// if these are changed dynamically, it will force another ChangeTextureFilter
			idE.CvarSystem.ClearModified("image_filter");
			idE.CvarSystem.ClearModified("image_anisotropy");
			idE.CvarSystem.ClearModified("image_lodbias");

			SetSamplers();			
		}

		public void CompleteBackgroundLoading()
		{
			idImage image;
			BackgroundDownload backgroundDownload;
			List<idImage> complete = new List<idImage>();

			foreach(KeyValuePair<idImage, BackgroundDownload> kvp in _backgroundImageLoads)
			{
				image = kvp.Key;
				backgroundDownload = kvp.Value;

				if(backgroundDownload.Completed == true)
				{
					backgroundDownload.Stream.Dispose();
					backgroundDownload.Stream = null;

					// upload the image
					idConsole.Warning("TODO: image.UploadPrecompressedImage");
					/*image->UploadPrecompressedImage( (byte *)image->bgl.file.buffer, image->bgl.file.length );
					R_StaticFree( image->bgl.file.buffer );*/

					complete.Add(image);

					if(idE.CvarSystem.GetBool("image_showBackgroundLoads") == true)
					{
						idConsole.WriteLine("idImageManager.CompleteBackgroundLoading: {0}", image.Name);
					}
				}
			}

			foreach(idImage tmp in complete)
			{
				_backgroundImageLoads.Remove(tmp);
			}

			// TODO
			/*if ( image_showBackgroundLoads.GetBool() ) {
				static int prev;
				if ( numActiveBackgroundImageLoads != prev ) {
					prev = numActiveBackgroundImageLoads;
					common->Printf( "background Loads: %i\n", numActiveBackgroundImageLoads );
				}
			}*/
		}

		public void Init()
		{
			// set default texture filter modes
			ChangeTextureFilter();

			// create built in images
			_defaultImage = LoadFromCallback("_default", GenerateDefaultImage);
			_whiteImage = LoadFromCallback("_white", GenerateWhiteImage);
			_blackImage = LoadFromCallback("_black", GenerateBlackImage);
			_borderClampImage = LoadFromCallback("_borderClamp", GenerateBorderClampImage);
			// TODO: _flatNormalMap = LoadFromCallback("_flat", GenerateFlatNormalImage);
			// TODO: _ambientNormalMap = LoadFromCallback("_ambient", GenerateAmbientNormalImage);
			_specularTableImage = LoadFromCallback("_specularTable", GenerateSpecularTableImage);
			_specular2DTableImage = LoadFromCallback("_specular2DTable", GenerateSpecular2DTableImage);
			_rampImage = LoadFromCallback("_ramp", GenerateRampImage);
			_alphaRampImage = LoadFromCallback("_alphaRamp", GenerateRampImage);
			_alphaNotchImage = LoadFromCallback("_alphaNotch", GenerateAlphaNotchImage);
			_fogImage = LoadFromCallback("_fog", GenerateFogImage);
			_fogEnterImage = LoadFromCallback("_fogEnter", GenerateFogEnterImage);
			// TODO: _normalCubeMapImage = LoadFromCallback("_normalCubeMap", makeNormalizeVectorCubeMap);*/
			_noFalloffImage = LoadFromCallback("_noFalloff", GenerateNoFalloffImage);

			LoadFromCallback("_quadratic", GenerateQuadraticImage);

			// cinematicImage is used for cinematic drawing
			// scratchImage is used for screen wipes/doublevision etc..
			_cinematicImage = LoadFromCallback("_cinematic", GenerateRGBA8Image);
			_scratchImage = LoadFromCallback("_scratch", GenerateRGBA8Image);
			_scratchImage2 = LoadFromCallback("_scratch2", GenerateRGBA8Image);
			_accumImage = LoadFromCallback("_accum", GenerateRGBA8Image);
			// TODO: _scratchCubeMapImage = LoadFromCallback("_scratchCubeMap", makeNormalizeVectorCubeMap);*/
			_currentRenderImage = LoadFromCallback("_currentRender", GenerateRGBA8Image);

			// TODO: cmds
			idE.CmdSystem.AddCommand("reloadImages", "reloads images", CommandFlags.Renderer, new EventHandler<CommandEventArgs>(Cmd_ReloadImages));
			/*cmdSystem->AddCommand("listImages", R_ListImages_f, CMD_FL_RENDERER, "lists images");
			cmdSystem->AddCommand("combineCubeImages", R_CombineCubeImages_f, CMD_FL_RENDERER, "combines six images for roq compression");*/

			// should forceLoadImages be here?
		}

		/// <summary>
		/// Images that are procedurally generated are allways specified
		/// with a callback which must work at any time, allowing the render
		/// system to be completely regenerated if needed.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public idImage LoadFromCallback(string name, ImageLoadCallback generator)
		{
			if(name == null)
			{
				throw new ArgumentNullException("name");
			}

			// strip any .tga file extensions from anywhere in the _name
			name = name.Replace(".tga", "");
			name = name.Replace("\\", "/");

			idImage image;

			// see if the image already exists
			if(_imageDictionary.TryGetValue(name, out image) == true)
			{
				return image;
			}

			// create the image and issue the callback
			image = CreateImage(name, generator);

			if(idE.CvarSystem.GetBool("image_preload") == true)
			{
				// check for precompressed, load is from the front end
				image.ReferencedOutsideLevelLoad = true;
				image.ActuallyLoadImage(true, false);
			}

			return image;
		}

		public Texture2D LoadImageProgram(string name, ref DateTime timeStamp, ref TextureDepth depth)
		{
			return ParseImageProgram(name, ref timeStamp, ref depth);
		}

		public string ImageProgramStringToCompressedFileName(string program)
		{
			Regex regex = new Regex(@"[\<\>\:\|""\.]", RegexOptions.Compiled);
			program = regex.Replace(program, "_");

			regex = new Regex(@"[\/\\\\]", RegexOptions.Compiled);
			program = regex.Replace(program, "/");

			regex = new Regex(@"[\),]", RegexOptions.Compiled);
			program = regex.Replace(program, "");

			program = program.Replace("/ ", "/");

			return string.Format("dds/{0}.dds", program);
		}

		public void QueueBackgroundLoad(idImage image)
		{
			string fileName = idE.ImageManager.ImageProgramStringToCompressedFileName(image.Name);

			BackgroundDownload backgroundDownload = new BackgroundDownload();
			backgroundDownload.Stream = idE.FileSystem.OpenFileRead(fileName);

			if(backgroundDownload.Stream == null)
			{
				idConsole.Warning("idImageManager.StartBackgroundLoad: Couldn't load {0}", image.Name);
			}
			else
			{
				idE.FileSystem.QueueBackgroundLoad(backgroundDownload);

				_backgroundImageLoads.Add(image, backgroundDownload);

				// TODO: purge image cache
				/*// purge some images if necessary
				int		totalSize = 0;
				for ( idImage *check = globalImages->cacheLRU.cacheUsageNext ; check != &globalImages->cacheLRU ; check = check->cacheUsageNext ) {
					totalSize += check->StorageSize();
				}
				int	needed = this->StorageSize();

				while ( ( totalSize + needed ) > globalImages->image_cacheMegs.GetFloat() * 1024 * 1024 ) {
					// purge the least recently used
					idImage	*check = globalImages->cacheLRU.cacheUsagePrev;
					if ( check->texnum != TEXTURE_NOT_LOADED ) {
						totalSize -= check->StorageSize();
						if ( globalImages->image_showBackgroundLoads.GetBool() ) {
							common->Printf( "purging %s\n", check->imgName.c_str() );
						}
						check->PurgeImage();
					}
					// remove it from the cached list
					check->cacheUsageNext->cacheUsagePrev = check->cacheUsagePrev;
					check->cacheUsagePrev->cacheUsageNext = check->cacheUsageNext;
					check->cacheUsageNext = NULL;
					check->cacheUsagePrev = NULL;
				}*/
			}
		}

		public void ReloadImages()
		{
			// build the compressed normal map palette
			SetNormalPalette();
			Cmd_ReloadImages(this, new CommandEventArgs(new idCmdArgs("reloadImages reload", false)));
		}
		#endregion

		#region Static
		public static void GenerateAmbientNormalImage(idImage image)
		{
			byte[, ,] data = new byte[DefaultImageSize, DefaultImageSize, 4];

			int red = (idE.CvarSystem.GetInteger("image_useNormalCompression") == 1) ? 0 : 3;
			int alpha = (red == 0) ? 3 : 0;

			Vector4 ambientLightVector = idE.RenderSystem.AmbientLightVector;

			// flat normal map for default bunp mapping
			for(int i = 0; i < 4; i++)
			{
				data[0, i, red] = (byte) (255 * ambientLightVector.X);
				data[0, i, 1] = (byte) (255 * ambientLightVector.Y);
				data[0, i, 2] = (byte) (255 * ambientLightVector.Z);
				data[0, i, alpha] = 255;
			}

			byte[,] pics = new byte[6, 4];

			for(int i = 0; i < 6; i++)
			{
				pics[i, 0] = data[0, 0, 0];
				pics[i, 1] = data[0, 0, 1];
				pics[i, 2] = data[0, 0, 2];
				pics[i, 3] = data[0, 0, 3];
			}

			// this must be a cube map for fragment programs to simply substitute for the normalization cube map
			idConsole.Warning("TODO: image->GenerateCubeImage( pics, 2, TF_DEFAULT, true, TD_HIGH_QUALITY );");
		}

		public static void GenerateBlackImage(idImage image)
		{
			// solid black texture
			byte[] data = new byte[DefaultImageSize * DefaultImageSize * 4];
			//image.Generate(data, DefaultImageSize, DefaultImageSize, TextureFilter.Default, false, TextureRepeat.Repeat, TextureDepth.Default);
		}

		public static void GenerateBorderClampImage(idImage image)
		{
			// the size determines how far away from the edge the blocks start fading
			int borderClampSize = 32;

			byte[, ,] data = new byte[borderClampSize, borderClampSize, 4];

			// solid white texture with a single pixel black border
			for(int y = 0; y < borderClampSize; y++)
			{
				for(int x = 0; x < borderClampSize; x++)
				{
					data[y, x, 0] = 255;
					data[y, x, 1] = 255;
					data[y, x, 2] = 255;
					data[y, x, 3] = 255;
				}
			}

			for(int i = 0; i < borderClampSize; i++)
			{
				data[i, 0, 0] =
					data[i, 0, 1] =
					data[i, 0, 2] =
					data[i, 0, 3] =

				data[i, borderClampSize - 1, 0] =
					data[i, borderClampSize - 1, 1] =
					data[i, borderClampSize - 1, 2] =
					data[i, borderClampSize - 1, 3] =

				data[0, i, 0] =
					data[0, i, 1] =
					data[0, i, 2] =
					data[0, i, 3] =

				data[borderClampSize - 1, i, 0] =
					data[borderClampSize - 1, i, 1] =
					data[borderClampSize - 1, i, 2] =
					data[borderClampSize - 1, i, 3] = 0;
			}

			image.Generate(idHelper.Flatten<byte>(data), borderClampSize, borderClampSize, TextureFilter.Linear, false, TextureRepeat.ClampToBorder, TextureDepth.Default);

			if(idE.RenderSystem.IsRunning == false)
			{
				// can't call qglTexParameterfv yet
				return;
			}

			// explicit zero border
			float[] color = new float[4];


			//Gl.glTexParameterfv(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_BORDER_COLOR, color);
		}
		
		public static void GenerateDefaultImage(idImage image)
		{
			image.MakeDefault();
		}

		public static void GenerateFlatNormalImage(idImage image)
		{
			byte[, ,] data = new byte[DefaultImageSize, DefaultImageSize, 4];

			int red = (idE.CvarSystem.GetInteger("image_useNormalCompression") == 1) ? 0 : 3;
			int alpha = (red == 0) ? 3 : 0;

			// flat normal map for default bunp mapping
			for(int i = 0; i < 4; i++)
			{
				data[0, i, red] = 128;
				data[0, i, 1] = 128;
				data[0, i, 2] = 255;
				data[0, i, alpha] = 255;
			}

			image.Generate(idHelper.Flatten<byte>(data), 2, 2, TextureFilter.Default, true, TextureRepeat.Repeat, TextureDepth.HighQuality);
		}

		public static void GenerateWhiteImage(idImage image)
		{
			// solid white texture
			byte[] data = new byte[DefaultImageSize * DefaultImageSize * 4];

			for(int i = 0; i < data.Length; i++)
			{
				data[i] = 255;
			}

			image.Generate(data, DefaultImageSize, DefaultImageSize, TextureFilter.Default, false, TextureRepeat.Repeat, TextureDepth.Default);
		}

		public static void GenerateRGBA8Image(idImage image)
		{
			byte[, ,] data = new byte[DefaultImageSize, DefaultImageSize, 4];

			data[0, 0, 0] = 16;
			data[0, 0, 1] = 32;
			data[0, 0, 2] = 48;
			data[0, 0, 3] = 96;

			image.Generate(idHelper.Flatten<byte>(data), DefaultImageSize, DefaultImageSize, TextureFilter.Default, false, TextureRepeat.Repeat, TextureDepth.HighQuality);
		}

		public static void GenerateRGB8Image(idImage image)
		{
			byte[, ,] data = new byte[DefaultImageSize, DefaultImageSize, 4];

			data[0, 0, 0] = 16;
			data[0, 0, 1] = 32;
			data[0, 0, 2] = 48;
			data[0, 0, 3] = 255;

			image.Generate(idHelper.Flatten<byte>(data), DefaultImageSize, DefaultImageSize, TextureFilter.Default, false, TextureRepeat.Repeat, TextureDepth.HighQuality);
		}

		public static void GenerateAlphaNotchImage(idImage image)
		{
			byte[,] data = new byte[2, 4];

			// this is used for alpha test clip planes
			data[0, 0] = data[0, 1] = data[0, 2] = 255;
			data[0, 3] = 0;
			data[1, 0] = data[1, 1] = data[1, 2] = 255;
			data[1, 3] = 255;

			image.Generate(idHelper.Flatten<byte>(data), 2, 1, TextureFilter.Nearest, false, TextureRepeat.Clamp, TextureDepth.HighQuality);
		}

		/// <summary>
		/// Creates a 0-255 ramp image.
		/// </summary>
		/// <param name="image"></param>
		public static void GenerateRampImage(idImage image)
		{
			byte[,] data = new byte[256, 4];

			for(int x = 0; x < 256; x++)
			{
				data[x, 0] =
					data[x, 1] =
					data[x, 2] =
					data[x, 3] = (byte) x;
			}

			image.Generate(idHelper.Flatten<byte>(data), 256, 1, TextureFilter.Nearest, false, TextureRepeat.Clamp, TextureDepth.HighQuality);
		}

		/// <summary>
		/// Creates a ramp that matches our fudged specular calculation.
		/// </summary>
		/// <param name="image"></param>
		public static void GenerateSpecularTableImage(idImage image)
		{
			byte[,] data = new byte[256, 4];

			for(int x = 0; x < 256; x++)
			{
				float f = x / 255.0f;

				// this is the behavior of the hacked up fragment programs that can't really do a power function
				f = (f - 0.75f) * 4;

				if(f < 0)
				{
					f = 0;
				}

				f = f * f;

				int b = (int) (f * 255);

				data[x, 0] =
					data[x, 1] =
					data[x, 2] =
					data[x, 3] = (byte) b;
			}

			image.Generate(idHelper.Flatten<byte>(data), 256, 1, TextureFilter.Linear, false, TextureRepeat.Clamp, TextureDepth.HighQuality);
		}

		/// <summary>
		/// Create a 2D table that calculates (reflection dot , specularity).
		/// </summary>
		/// <param name="image"></param>
		public static void GenerateSpecular2DTableImage(idImage image)
		{
			byte[, ,] data = new byte[256, 256, 4];

			for(int x = 0; x < 256; x++)
			{
				float f = x / 255.0f;

				for(int y = 0; y < 256; y++)
				{
					int b = (int) (idMath.Pow(f, y) * 255.0f);

					if(b == 0)
					{
						// as soon as b equals zero all remaining values in this column are going to be zero
						// we early out to avoid pow() underflows
						break;
					}

					data[y, x, 0] =
					data[y, x, 1] =
					data[y, x, 2] =
					data[y, x, 3] = (byte) b;
				}
			}

			image.Generate(idHelper.Flatten<byte>(data), 256, 256, TextureFilter.Linear, false, TextureRepeat.Clamp, TextureDepth.HighQuality);
		}

		/// <summary>
		/// Creates a 0-255 ramp image.
		/// </summary>
		/// <param name="image"></param>
		public static void GenerateAlphaRampImage(idImage image)
		{
			byte[,] data = new byte[256, 4];

			for(int x = 0; x < 256; x++)
			{
				data[x, 0] =
					data[x, 1] =
					data[x, 2] = 255;
				data[x, 3] = (byte) x;
			}

			image.Generate(idHelper.Flatten<byte>(data), 256, 1, TextureFilter.Nearest, false, TextureRepeat.Clamp, TextureDepth.HighQuality);
		}

		private static float FogFraction(float viewHeight, float targetHeight)
		{
			float total = idMath.Abs(targetHeight - viewHeight);
			float rampRange = 8;
			float deepRange = -30;

			// only ranges that cross the ramp range are special
			if((targetHeight > 0) && (viewHeight > 0))
			{
				return 0.0f;
			}
			else if((targetHeight < -rampRange) && (viewHeight < -rampRange))
			{
				return 1.0f;
			}

			float above = 0;

			if(targetHeight > 0)
			{
				above = targetHeight;
			}
			else if(viewHeight > 0)
			{
				above = viewHeight;
			}

			float rampTop, rampBottom;

			if(viewHeight > targetHeight)
			{
				rampTop = viewHeight;
				rampBottom = targetHeight;
			}
			else
			{
				rampTop = targetHeight;
				rampBottom = viewHeight;
			}

			if(rampTop > 0)
			{
				rampTop = 0;
			}

			if(rampBottom < -rampRange)
			{
				rampBottom = -rampRange;
			}

			float rampSlope = 1.0f / rampRange;

			if(total == 0)
			{
				return -viewHeight * rampSlope;
			}

			float ramp = (1.0f - (rampTop * rampSlope + rampBottom * rampSlope) * -0.5f) * (rampTop - rampBottom);
			float frac = (total - above - ramp) / total;

			// after it gets moderately deep, always use full value
			float deepest = (viewHeight < targetHeight) ? viewHeight : targetHeight;
			float deepFrac = deepest / deepRange;

			if(deepFrac >= 1.0f)
			{
				return 1.0f;
			}

			return (frac * (1.0f - deepFrac) + deepFrac);
		}

		/// <summary>
		/// Modulate the fog alpha density based on the distance of the start and end points to the terminator plane.
		/// </summary>
		/// <param name="image"></param>
		public static void GenerateFogEnterImage(idImage image)
		{
			byte[, ,] data = new byte[FogEnterSize, FogEnterSize, 4];

			for(int x = 0; x < FogEnterSize; x++)
			{
				for(int y = 0; y < FogEnterSize; y++)
				{
					float d = FogFraction(x - (FogEnterSize / 2), y - (FogEnterSize / 2));
					int b = (byte) (d * 255);

					if(b <= 0)
					{
						b = 0;
					}
					else if(b > 255)
					{
						b = 255;
					}

					data[y, x, 0] =
						data[y, x, 1] =
						data[y, x, 2] = 255;
					data[y, x, 3] = (byte) b;
				}
			}

			// if mipmapped, acutely viewed surfaces fade wrong
			//image.Generate(idHelper.Flatten<byte>(data), FogEnterSize, FogEnterSize, TextureFilter.Linear, false, TextureRepeat.Clamp, TextureDepth.HighQuality);
		}

		/// <summary>
		/// We calculate distance correctly in two planes, but the third will still be projection based.
		/// </summary>
		/// <param name="image"></param>
		public static void GenerateFogImage(idImage image)
		{
			byte[, ,] data = new byte[FogSize, FogSize, 4];
			float[] step = new float[256];
			float remaining = 1.0f;

			for(int i = 0; i < 256; i++)
			{
				step[i] = remaining;
				remaining *= 0.982f;
			}

			for(int x = 0; x < FogSize; x++)
			{
				for(int y = 0; y < FogSize; y++)
				{
					float d = (float) idMath.Sqrt((x - (FogSize / 2)) * (x - (FogSize / 2))
						+ (y - (FogSize / 2)) * (y - (FogSize / 2)));
					d /= FogSize / 2 - 1;

					int b = (byte) (d * 255);

					if(b <= 0)
					{
						b = 0;
					}
					else if(b > 255)
					{
						b = 255;
					}

					b = (byte) (255 * (1.0f - step[b]));

					if((x == 0) || (x == (FogSize - 1)) || (y == 0) || (y == (FogSize - 1)))
					{
						b = 255; // avoid clamping issues
					}

					data[y, x, 0] =
						data[y, x, 1] =
						data[y, x, 2] = 255;
					data[y, x, 3] = (byte) b;
				}
			}

			image.Generate(idHelper.Flatten<byte>(data), FogSize, FogSize, TextureFilter.Linear, false, TextureRepeat.Clamp, TextureDepth.HighQuality);
		}

		public static void GenerateQuadraticImage(idImage image)
		{
			byte[, ,] data = new byte[QuadraticHeight, QuadraticWidth, 4];

			for(int x = 0; x < QuadraticWidth; x++)
			{
				for(int y = 0; y < QuadraticHeight; y++)
				{
					float d = x - ((QuadraticWidth / 2) - 0.5f);
					d = idMath.Abs(d);
					d -= 0.5f;
					d /= QuadraticWidth / 2;

					d = 1.0f - d;
					d = d * d;

					int b = (byte) (d * 255);

					if(b <= 0)
					{
						b = 0;
					}
					else if(b > 255)
					{
						b = 255;
					}

					data[y, x, 0] =
						data[y, x, 1] =
						data[y, x, 2] = (byte) b;
					data[y, x, 3] = 255;
				}
			}

			image.Generate(idHelper.Flatten<byte>(data), QuadraticWidth, QuadraticHeight, TextureFilter.Default, false, TextureRepeat.Clamp, TextureDepth.HighQuality);
		}

		/// <summary>
		/// This is a solid white texture that is zero clamped.
		/// </summary>
		/// <param name="image"></param>
		public static void GenerateNoFalloffImage(idImage image)
		{
			byte[, ,] data = new byte[16, FalloffTextureSize, 4];

			for(int x = 1; x < (FalloffTextureSize - 1); x++)
			{
				for(int y = 1; y < 15; y++)
				{
					data[y, x, 0] = 255;
					data[y, x, 1] = 255;
					data[y, x, 2] = 255;
					data[y, x, 3] = 255;
				}
			}

			image.Generate(idHelper.Flatten<byte>(data), FalloffTextureSize, 16, TextureFilter.Default, false, TextureRepeat.ClampToZero, TextureDepth.HighQuality);
		}

		/// <summary>
		/// Loads any of the supported image types into a cannonical 32 bit format.
		/// </summary>
		/// <remarks>
		/// 
		/// Automatically attempts to load .jpg files if .tga files fail to load.
		/// <p/>
		/// Anything that is going to make this into a texture would use
		/// makePowerOf2 = true, but something loading an image as a lookup
		/// table of some sort would leave it in identity form.
		/// <p/>
		/// It is important to do this at image load time instead of texture load
		/// time for bump maps.
		/// <p/>
		/// timestamp may be NULL if the value is going to be ignored
		/// <p/>
		/// If data is NULL, the image won't actually be loaded, it will just find the
		/// timestamp.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="timeStamp"></param>
		/// <param name="makePowerOf2"></param>
		/// <returns></returns>
		public Texture2D LoadImage(string name, ref DateTime timeStamp, bool makePowerOf2)
		{
			// TODO: timestamp
			timeStamp = DateTime.Now;

			name = Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name));

			return idE.System.Content.Load<Texture2D>(name);
		}

		/// <summary>
		/// If data is NULL, the timestamps will be filled in, but no image will be generated
		/// If both data and timeStamp are NULL, it will just advance past it, which can be
		/// used to parse an image program from a text stream.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="timeStamp"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		public Texture2D ParseImageProgram(string source, ref DateTime timeStamp, ref TextureDepth depth)
		{
			return new idImageProgramParser().ParseImageProgram(source, ref timeStamp, ref depth);
		}
		#endregion

		#region Private
		private idImage CreateImage(string name, ImageLoadCallback generator)
		{
			idImage image = new idImage(name, generator);

			_images.Add(image);
			_imageDictionary.Add(name, image);

			return image;
		}

		private idImage CreateImage(string name, TextureType type, TextureFilter filter, TextureRepeat repeat, TextureDepth depth, CubeFiles cubeMap, bool allowDownSize)
		{
			idImage image = new idImage(name, type, filter, repeat, depth, cubeMap, allowDownSize);

			_images.Add(image);
			_imageDictionary.Add(name, image);

			return image;
		}

		private void InitCvars()
		{
			new idCvar("image_filter", "GL_LINEAR_MIPMAP_LINEAR", ImageFilters.Keys.ToArray(), "changes texture filtering on mipmapped images", new ArgCompletion_String(ImageFilters.Keys.ToArray()), CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_anisotropy", "1", "set the maximum texture anisotropy if available", CvarFlags.Integer | CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_lodbias", "0", "change lod bias on mipmapped images", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_downSize", "0", "controls texture downsampling", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_forceDownSize", "0", "", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("image_roundDown", "1", "round bad sizes down to nearest power of two", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("image_colorMipLevels", "0", "development aid to see texture mip usage", CvarFlags.Renderer | CvarFlags.Bool);
			new idCvar("image_preload", "1", "if 0, dynamically load all images", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("image_useCompression", "1", "0 = force everything to high quality", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("image_useAllFormats", "1", "allow alpha/intensity/luminance/luminance+alpha", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("image_useNormalCompression", "2", "2 = use rxgb compression for normal maps, 1 = use 256 color compression for normal maps if available", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("image_usePrecompressedTextures", "1", "use .dds files if present", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("image_writePrecompressedTextures", "0", "write .dds files if necessary", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_writeNormalTGA", "0", "write .tgas of the final normal maps for debugging", CvarFlags.Renderer | CvarFlags.Bool);
			new idCvar("image_writeNormalTGAPalletized", "0", "write .tgas of the final palletized normal maps for debugging", CvarFlags.Renderer | CvarFlags.Bool);
			new idCvar("image_writeTGA", "0", "write .tgas of the non normal maps for debugging", CvarFlags.Renderer | CvarFlags.Bool);
			new idCvar("image_useOfflineCompression", "0", "write a batch file for offline compression of DDS files", CvarFlags.Renderer | CvarFlags.Bool);
			new idCvar("image_cacheMinK", "200", "maximum KB of precompressed files to read at specification time", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Integer);
			new idCvar("image_cacheMegs", "20", "maximum MB set aside for temporary loading of full-sized precompressed images", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_useCache", "0", "1 = do background load image caching", CvarFlags.Renderer | CvarFlags.Archive | CvarFlags.Bool);
			new idCvar("image_showBackgroundLoads", "0", "1 = print number of outstanding background loads", CvarFlags.Renderer | CvarFlags.Bool);
			new idCvar("image_downSizeSpecular", "0", "controls specular downsampling", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_downSizeBump", "0", "controls normal map downsampling", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_downSizeSpecularLimit", "64", "controls specular downsampled limit", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_downSizeBumpLimit", "128", "controls normal map downsample limit", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_ignoreHighQuality", "0", "ignore high quality setting on materials", CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_downSizeLimit", "256", "controls diffuse map downsample limit", CvarFlags.Renderer | CvarFlags.Archive);
		}

		private void InitFilters()
		{
			ImageFilters.Add("GL_LINEAR_MIPMAP_NEAREST", new ImageFilter("GL_LINEAR_MIPMAP_NEAREST", XTextureFilter.MinLinearMagPointMipPoint));
			ImageFilters.Add("GL_LINEAR_MIPMAP_LINEAR", new ImageFilter("GL_LINEAR_MIPMAP_LINEAR", XTextureFilter.MinLinearMagPointMipLinear));
			ImageFilters.Add("GL_NEAREST", new ImageFilter("GL_NEAREST", XTextureFilter.PointMipLinear));
			ImageFilters.Add("GL_LINEAR", new ImageFilter("GL_LINEAR", XTextureFilter.LinearMipPoint));
			ImageFilters.Add("GL_NEAREST_MIPMAP_NEAREST", new ImageFilter("GL_NEAREST_MIPMAP_NEAREST", XTextureFilter.MinPointMagLinearMipPoint));
			ImageFilters.Add("GL_NEAREST_MIPMAP_LINEAR", new ImageFilter("GL_NEAREST_MIPMAP_LINEAR", XTextureFilter.MinPointMagLinearMipLinear));
		}
		
		private void SetNormalPalette()
		{
			idConsole.Warning("TODO: SetNormalPalette");
			/*int		i, j;
			idVec3	v;
			float	t;
			//byte temptable[768];
			byte	*temptable = compressedPalette;
			int		compressedToOriginal[16];

			// make an ad-hoc separable compression mapping scheme
			for ( i = 0 ; i < 8 ; i++ ) {
				float	f, y;

				f = ( i + 1 ) / 8.5;
				y = idMath::Sqrt( 1.0 - f * f );
				y = 1.0 - y;

				compressedToOriginal[7-i] = 127 - (int)( y * 127 + 0.5 );
				compressedToOriginal[8+i] = 128 + (int)( y * 127 + 0.5 );
			}

			for ( i = 0 ; i < 256 ; i++ ) {
				if ( i <= compressedToOriginal[0] ) {
					originalToCompressed[i] = 0;
				} else if ( i >= compressedToOriginal[15] ) {
					originalToCompressed[i] = 15;
				} else {
					for ( j = 0 ; j < 14 ; j++ ) {
						if ( i <= compressedToOriginal[j+1] ) {
							break;
						}
					}
					if ( i - compressedToOriginal[j] < compressedToOriginal[j+1] - i ) {
						originalToCompressed[i] = j;
					} else {
						originalToCompressed[i] = j + 1;
					}
				}
			}

		#if 0
			for ( i = 0; i < 16; i++ ) {
				for ( j = 0 ; j < 16 ; j++ ) {

					v[0] = ( i - 7.5 ) / 8;
					v[1] = ( j - 7.5 ) / 8;

					t = 1.0 - ( v[0]*v[0] + v[1]*v[1] );
					if ( t < 0 ) {
						t = 0;
					}
					v[2] = idMath::Sqrt( t );

					temptable[(i*16+j)*3+0] = 128 + floor( 127 * v[0] + 0.5 );
					temptable[(i*16+j)*3+1] = 128 + floor( 127 * v[1] );
					temptable[(i*16+j)*3+2] = 128 + floor( 127 * v[2] );
				}
			}
		#else
			for ( i = 0; i < 16; i++ ) {
				for ( j = 0 ; j < 16 ; j++ ) {

					v[0] = ( compressedToOriginal[i] - 127.5 ) / 128;
					v[1] = ( compressedToOriginal[j] - 127.5 ) / 128;

					t = 1.0 - ( v[0]*v[0] + v[1]*v[1] );
					if ( t < 0 ) {
						t = 0;
					}
					v[2] = idMath::Sqrt( t );

					temptable[(i*16+j)*3+0] = (byte)(128 + floor( 127 * v[0] + 0.5 ));
					temptable[(i*16+j)*3+1] = (byte)(128 + floor( 127 * v[1] ));
					temptable[(i*16+j)*3+2] = (byte)(128 + floor( 127 * v[2] ));
				}
			}
		#endif

			// color 255 will be the "nullnormal" color for no reflection
			temptable[255*3+0] =
			temptable[255*3+1] =
			temptable[255*3+2] = 128;

			if ( !glConfig.sharedTexturePaletteAvailable ) {
				return;
			}

			qglColorTableEXT( GL_SHARED_TEXTURE_PALETTE_EXT,
							   GL_RGB,
							   256,
							   GL_RGB,
							   GL_UNSIGNED_BYTE,
							   temptable );

			qglEnable( GL_SHARED_TEXTURE_PALETTE_EXT );*/
		}

		private void SetSamplers()
		{
			string str = idE.CvarSystem.GetString("image_filter");
			ImageFilter filter;

			if(idImageManager.ImageFilters.TryGetValue(str, out filter) == false)
			{
				idConsole.Warning("bad r_textureFilter: '{0}'", str);
			}
			
			// set the values for future images
			_defaultClampTextureSampler = new SamplerState();
			_defaultClampTextureSampler.Filter = filter.Filter;
			_defaultClampTextureSampler.AddressU = TextureAddressMode.Clamp;
			_defaultClampTextureSampler.AddressV = TextureAddressMode.Clamp;
			_defaultClampTextureSampler.AddressW = TextureAddressMode.Clamp;

			_defaultRepeatTextureSampler = new SamplerState();
			_defaultRepeatTextureSampler.Filter = filter.Filter;
			_defaultRepeatTextureSampler.AddressU = TextureAddressMode.Wrap;
			_defaultRepeatTextureSampler.AddressV = TextureAddressMode.Wrap;
			_defaultRepeatTextureSampler.AddressW = TextureAddressMode.Wrap;

			_linearClampTextureSampler = new SamplerState();
			_linearClampTextureSampler.Filter = XTextureFilter.Linear;
			_linearClampTextureSampler.AddressU = TextureAddressMode.Clamp;
			_linearClampTextureSampler.AddressV = TextureAddressMode.Clamp;
			_linearClampTextureSampler.AddressW = TextureAddressMode.Clamp;

			_linearRepeatTextureSampler = new SamplerState();
			_linearRepeatTextureSampler.Filter = XTextureFilter.Linear;
			_linearRepeatTextureSampler.AddressU = TextureAddressMode.Wrap;
			_linearRepeatTextureSampler.AddressV = TextureAddressMode.Wrap;
			_linearRepeatTextureSampler.AddressW = TextureAddressMode.Wrap;

			_nearestClampTextureSampler = new SamplerState();
			_nearestClampTextureSampler.Filter = XTextureFilter.Point;
			_nearestClampTextureSampler.AddressU = TextureAddressMode.Clamp;
			_nearestClampTextureSampler.AddressV = TextureAddressMode.Clamp;
			_nearestClampTextureSampler.AddressW = TextureAddressMode.Clamp;

			_nearestRepeatTextureSampler = new SamplerState();
			_nearestRepeatTextureSampler.Filter = XTextureFilter.Point;
			_nearestRepeatTextureSampler.AddressU = TextureAddressMode.Wrap;
			_nearestRepeatTextureSampler.AddressV = TextureAddressMode.Wrap;
			_nearestRepeatTextureSampler.AddressW = TextureAddressMode.Wrap;
			
			if(idE.GLConfig.AnisotropicAvailable == true)
			{
				int anisotropy = idE.CvarSystem.GetInteger("image_anisotropy");

				if(anisotropy < 1)
				{
					anisotropy = 1;
				}
				else if(anisotropy > idE.GLConfig.MaxTextureAnisotropy)
				{
					anisotropy = idE.GLConfig.MaxTextureAnisotropy;
				}

				_defaultClampTextureSampler.MaxAnisotropy = anisotropy;
				_linearClampTextureSampler.MaxAnisotropy = anisotropy;
				_nearestClampTextureSampler.MaxAnisotropy = anisotropy;

				_defaultRepeatTextureSampler.MaxAnisotropy = anisotropy;
				_linearRepeatTextureSampler.MaxAnisotropy = anisotropy;
				_nearestRepeatTextureSampler.MaxAnisotropy = anisotropy;
			}

			if(idE.GLConfig.TextureLodBiasAvailable == true)
			{
				float lodBias = idE.CvarSystem.GetFloat("image_lodbias");

				_defaultClampTextureSampler.MipMapLevelOfDetailBias = lodBias;
				_linearClampTextureSampler.MipMapLevelOfDetailBias = lodBias;
				_nearestClampTextureSampler.MipMapLevelOfDetailBias = lodBias;

				_defaultRepeatTextureSampler.MipMapLevelOfDetailBias = lodBias;
				_linearRepeatTextureSampler.MipMapLevelOfDetailBias = lodBias;
				_nearestRepeatTextureSampler.MipMapLevelOfDetailBias = lodBias;
			}
		}
		#endregion

		#region Command handlers
		/// <summary>
		/// Regenerate all images that came directly from files that have changed, so
		/// any saved changes will show up in place.
		/// <p/>
		/// New r_texturesize/r_texturedepth variables will take effect on reload.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Cmd_ReloadImages(object sender, CommandEventArgs e)
		{
			// this probably isn't necessary...
			ChangeTextureFilter();

			bool all = false;
			bool checkPrecompressed = false; // if we are doing this as a vid_restart, look for precompressed like normal

			if(e.Args.Length == 2)
			{
				if(e.Args.Get(1).ToLower() == "all")
				{
					all = true;
				}
				else if(e.Args.Get(1).ToLower() == "reload")
				{
					all = true;
					checkPrecompressed = true;
				}
				else
				{
					idConsole.WriteLine("USAGE: reloadImages <all>");
					return;
				}
			}

			foreach(idImage image in _images)
			{
				image.Reload(checkPrecompressed, all);
			}
		}
		#endregion
		#endregion
	}

	public delegate void ImageLoadCallback(idImage image);

	public struct ImageFilter
	{
		public string Label;
		public XTextureFilter Filter;

		public ImageFilter(string label, XTextureFilter filter)
		{
			this.Label = label;
			this.Filter = filter;
		}
	}
}