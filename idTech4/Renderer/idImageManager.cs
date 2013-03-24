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
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework.Graphics;

using idTech4.Services;
using idTech4.Text;

namespace idTech4.Renderer
{
	public sealed class idImageManager : IImageManager
	{
		#region Members
		private List<idImage> _images                        = new List<idImage>();
		private Dictionary<string, idImage> _imageDictionary = new Dictionary<string, idImage>(StringComparer.OrdinalIgnoreCase);

		private idImage _defaultImage;
		private idImage _blackImage;					// full of 0x00
		private idImage _whiteImage;					// full of 0xff
		private idImage _noFalloffImage;				// all 255, but zero clamped

		private idImage _loadingIconImage;				// loading icon must exist always
		private idImage _hellLoadingIconImage;			// loading icon must exist always

		private bool _insideLevelLoad;					// don't actually load images now
		private bool _preloadingMapImages;				// unless this is set
		#endregion

		#region Constructor
		public idImageManager()
		{

		}
		#endregion

		#region IImageManager implementation
		#region Fetching
		public void BindNullTexture()
		{
			// TODO: RENDERLOG_PRINTF( "BindNull()\n" );
		}

		public idImage DefaultImage
		{
			get
			{
				return _defaultImage;
			}
		}
		#endregion

		#region Image Generators
		private void GenerateDefaultImage(idImage image)
		{
			image.MakeDefault();
		}

		private void GenerateBlackImage(idImage image)
		{
			byte[,,] data = new byte[Constants.DefaultImageSize, Constants.DefaultImageSize, 4];

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

			image.Generate(idHelper.Flatten(data), Constants.DefaultImageSize, Constants.DefaultImageSize, TextureFilter.Default, TextureRepeat.Repeat, TextureUsage.Default);
		}

		private void GenerateWhiteImage(idImage image)
		{
			byte[,,] data = new byte[Constants.DefaultImageSize, Constants.DefaultImageSize, 4];

			for(int x = 0; x < data.GetUpperBound(0) + 1; x++)
			{
				for(int y = 0; y < data.GetUpperBound(1) + 1; y++)
				{
					data[x, y, 0] = 
						data[x, y, 1] =
						data[x, y, 2] =
						data[x, y, 3] = 255;
				}
			}

			image.Generate(idHelper.Flatten(data), Constants.DefaultImageSize, Constants.DefaultImageSize, TextureFilter.Default, TextureRepeat.Repeat, TextureUsage.Default);
		}

		/// <summary>
		/// This is a solid white texture that is zero clamped.
		/// </summary>
		/// <param name="image"></param>
		private void GenerateNoFallOffImage(idImage image)
		{
			byte[,,] data = new byte[16, Constants.FallOffTextureSize, 4];

			for(int x = 1; x < Constants.FallOffTextureSize; x++)
			{
				for(int y = 1; y < 15; y++)
				{
					data[y, x, 0] = 
						data[y, x, 1] =
						data[y, x, 2] =
						data[y, x, 3] = 255;
				}
			}

			image.Generate(idHelper.Flatten(data), Constants.FallOffTextureSize, 16, TextureFilter.Default, TextureRepeat.ClampToZero, TextureUsage.LookupTableMono);
		}
		#endregion

		#region Initialization
		public void Init()
		{
			CreateIntrinsicImages();
		}

		private void CreateIntrinsicImages()
		{
			// create built in images
			_defaultImage                  = LoadFromGenerator("_default", GenerateDefaultImage);
			_whiteImage                    = LoadFromGenerator("_white",   GenerateWhiteImage);
			_blackImage                    = LoadFromGenerator("_black",   GenerateBlackImage);
			/*_flatNormalMap               = ImageFromFile("_flat",      TextureFilter.Default, false, TextureRepeat.Repeat, TextureUsage.Bump);
			// TODO: _alphaNotchImage      = LoadFromCallback("_alphaNotch", GenerateAlphaNotchImage);
			_fogImage                      = ImageFromFile("_fog",       TextureFilter.Linear,  TextureRepeat.Clamp, TextureUsage.LookupTableAlpha);
			_fogEnterImage                 = ImageFromFile("_fogEnter",  TextureFilter.Linear,  TextureRepeat.Clamp, TextureUsage.LookupTableAlpha);*/
			_noFalloffImage                = LoadFromGenerator("_noFalloff",   GenerateNoFallOffImage);

			//ImageFromFile("_quadratic", TextureFilter.Default, false, TextureRepeat.Clamp, TextureDepth.HighQuality);

			// scratchImage is used for screen wipes/doublevision etc..
			/*_scratchImage              = ImageFromFile("_scratch", TextureFilter.Default, false, TextureRepeat.Repeat, TextureDepth.HighQuality);
			_scratchImage2               = ImageFromFile("_scratch2", TextureFilter.Default, false, TextureRepeat.Repeat, TextureDepth.HighQuality);*/
			// TODO: _accumImage         = LoadFromCallback("_accum", GenerateRGBA8Image);*/
			//_currentRenderImage        = LoadFromCallback("_currentRender", GenerateRGBA8Image);
			//_currentDepthImage         = LoadFromCallback("_currentRender", R_DepthImage);

			// save a copy of this for material comparison, because currentRenderImage may get
			// reassigned during stereo rendering
			// TODO: originalCurrentRenderImage = currentRenderImage;

			_loadingIconImage     = LoadFromFile("textures/loadingicon2", TextureFilter.Default, TextureRepeat.Clamp, TextureUsage.Default, CubeFiles.TwoD);
			_hellLoadingIconImage = LoadFromFile("textures/loadingicon3", TextureFilter.Default, TextureRepeat.Clamp, TextureUsage.Default, CubeFiles.TwoD);

			// TODO: release_assert( loadingIconImage->referencedOutsideLevelLoad );
			// TODO: release_assert( hellLoadingIconImage->referencedOutsideLevelLoad );			
		}
		#endregion	

		#region Loading
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Called only by renderSystem::BeginLevelLoad.
		/// </remarks>
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

				if((image.ReferencedOutsideLevelLoad == false) && (image.IsLoaded == true))
				{
					image.Purge();
				}

				image.LevelLoadReferenced = false;
			}
		}

		/// <summary>
		/// Loads unloaded level images.
		/// </summary>
		/// <param name="pacifider"></param>
		private int LoadLevelImages(bool pacifider)
		{
			int	loadCount = 0;

			foreach(idImage image in _images)
			{
				if(pacifider == true)
				{
					idLog.Warning("TODO: common->UpdateLevelLoadPacifier();");
				}

				if(image.Generator != null)
				{
					continue;
				}

				if((image.LevelLoadReferenced == true) && (image.IsLoaded == false))
				{
					loadCount++;
					image.ActuallyLoadImage(false);
				}
			}

			return loadCount;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Called only by renderSystem::EndLevelLoad.
		/// </remarks>
		public void EndLevelLoad()
		{
			_insideLevelLoad = false;

			idLog.WriteLine("----- idImageManager::EndLevelLoad -----");


			long start = idEngine.Instance.ElapsedTime;
			int loadCount = LoadLevelImages(true);
			long end = idEngine.Instance.ElapsedTime;

			idLog.WriteLine("{0} images loaded in {0} seconds", loadCount, (end - start) * 0.001);
			idLog.WriteLine("----------------------------------------");
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
		public idImage LoadFromFile(string name, TextureFilter filter, TextureRepeat repeat, TextureUsage usage, CubeFiles cubeMap = CubeFiles.TwoD)
		{
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

			if((string.IsNullOrEmpty(name) == true)
				|| (name.Equals("default", StringComparison.OrdinalIgnoreCase) == true)
				|| (name.Equals("_default", StringComparison.OrdinalIgnoreCase) == true))
			{
				declManager.MediaPrint("DEFAULTED");

				return this.DefaultImage;
			}
			else if((name.StartsWith("fonts", StringComparison.OrdinalIgnoreCase) == true) 
				|| (name.StartsWith("newfonts", StringComparison.OrdinalIgnoreCase) == true))
			{
				usage = TextureUsage.Font;
			}
			else if(name.StartsWith("lights", StringComparison.OrdinalIgnoreCase) == true)
			{
				 usage = TextureUsage.Light;
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
					idEngine.Instance.Error("Image '{0}' has been referenced with conflicting cube map states", name);
				}
				else if((image.Filter != filter) || (image.Repeat != repeat))
				{
					// we might want to have the system reset these parameters on every bind and
					// share the image data	

					// FIXME: this might be the wrong behaviour.  original d3 would return a new image but our dictionary requires unique keys.
					return image;
				}
				else if(image.Usage != usage)
				{
					// if an image is used differently then we need 2 copies of it because usage affects the way it's compressed and swizzled					
				}
				else
				{
					image.Usage               = usage;
					image.LevelLoadReferenced = true;
					
					if((_insideLevelLoad == false) || (_preloadingMapImages == true))
					{
						image.ReferencedOutsideLevelLoad = ((_insideLevelLoad == false) && (_preloadingMapImages == false));
						image.ActuallyLoadImage(false);	// load is from front end

						declManager.MediaPrint("{0}x{1} {2} (reload for mixed references)", image.Width, image.Height, image.Name);
					}
					return image;
				}
			}
			
			//
			// create a new image
			//
			image = CreateImage(name, filter, repeat, usage, cubeMap);
			image.LevelLoadReferenced = true;

			// load it if we aren't in a level preload
			if((_insideLevelLoad == false) || (_preloadingMapImages == true))
			{
				image.ReferencedOutsideLevelLoad = true;
				image.ActuallyLoadImage(false);	// load is from front end

				declManager.MediaPrint("{0}x{1} {2}", image.Width, image.Height, image.Name);
			}
			else
			{
				declManager.MediaPrint(image.Name);
			}

			return image;
		}

		/// <summary>
		/// Images that are procedurally generated are allways specified
		/// with a callback which must work at any time, allowing the render
		/// system to be completely regenerated if needed.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public idImage LoadFromGenerator(string name, ImageLoadCallback generator)
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

			// check for precompressed, load is from the front end
			image.ReferencedOutsideLevelLoad = true;
			image.ActuallyLoadImage(false);

			return image;
		}

		public Texture2D LoadImage(string name, ref DateTime timeStamp)
		{			
			timeStamp = File.GetLastWriteTime(name);
			name      = Path.Combine(Path.GetDirectoryName(name), Path.GetFileNameWithoutExtension(name));

			if(name == "_emptyName")
			{
				idLog.Warning("FIXME: this shouldn't be happening!!!!");
				return null;
			}

			return idEngine.Instance.Content.Load<Texture2D>(name);
		}

		public Texture2D LoadImageProgram(string name, ref DateTime timeStamp, ref TextureUsage usage)
		{
			return ParseImageProgram(name, ref timeStamp, ref usage);
		}

		/// <summary>
		/// Used to parse an image program from a text stream.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="timeStamp"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		private Texture2D ParseImageProgram(string source, ref DateTime timeStamp, ref TextureUsage usage)
		{
			return new idImageProgramParser().ParseImageProgram(source, ref timeStamp, ref usage);
		}

		private idImage CreateImage(string name, ImageLoadCallback generator)
		{
			idImage image = new idImage(name, generator);

			_images.Add(image);
			_imageDictionary.Add(name, image);

			return image;
		}

		private idImage CreateImage(string name, TextureFilter filter, TextureRepeat repeat, TextureUsage usage, CubeFiles cubeMap)
		{
			idImage image = new idImage(name, filter, repeat, usage, cubeMap);

			_images.Add(image);
			_imageDictionary.Add(name, image);

			return image;
		}
		#endregion
		#endregion
	}

	public delegate void ImageLoadCallback(idImage image);
}