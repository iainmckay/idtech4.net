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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Tao.OpenGl;

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

		public int MaxTextureFilter
		{
			get
			{
				return _textureMaxFilter;
			}
		}

		public int MinTextureFilter
		{
			get
			{
				return _textureMinFilter;
			}
		}

		public float TextureAnisotropy
		{
			get
			{
				return _textureAnisotropy;
			}
		}

		public float TextureLodBias
		{
			get
			{
				return _textureLODBias;
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
		private Dictionary<string, idImage> _images = new Dictionary<string, idImage>(StringComparer.OrdinalIgnoreCase);

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

		// default filter modes for images
		private int _textureMinFilter;
		private int _textureMaxFilter;
		private float _textureAnisotropy;
		private float _textureLODBias;
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
		/// Disable the active texture unit.
		/// </summary>
		public void BindNullTexture()
		{
			TextureUnit unit = idE.Backend.GLState.TextureUnits[idE.Backend.GLState.CurrentTextureUnit];

			// TODO: RB_LogComment( "BindNull()\n" );

			if(unit.Type == TextureType.Cubic)
			{
				Gl.glDisable(Gl.GL_TEXTURE_CUBE_MAP_EXT);
			}
			else if(unit.Type == TextureType.ThreeD) 
			{
				Gl.glDisable(Gl.GL_TEXTURE_3D);
			} 
			else if(unit.Type == TextureType.TwoD)
			{
				Gl.glDisable(Gl.GL_TEXTURE_2D);
			}

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

			string str = idE.CvarSystem.GetString("image_filter");

			if(idImageManager.ImageFilters.ContainsKey(str) == false)
			{
				idConsole.Warning("bad r_textureFilter: '{0}'", str);
			}

			// set the values for future images
			_textureMinFilter = ImageFilters[str].Minimize;
			_textureMaxFilter = ImageFilters[str].Maximize;
			_textureAnisotropy = idE.CvarSystem.GetFloat("image_anisotropy");

			if(_textureAnisotropy < 1)
			{
				_textureAnisotropy = 1;
			}
			else if(_textureAnisotropy > idE.GLConfig.MaxTextureAnisotropy)
			{
				_textureAnisotropy = idE.GLConfig.MaxTextureAnisotropy;
			}

			_textureLODBias = idE.CvarSystem.GetFloat("image_lodbias");
		}

		public void CompleteBackgroundImageLoads()
		{
			idConsole.WriteLine("TODO: CompleteBackgroundImageLoads");
			/*idImage	*remainingList = NULL;
			idImage	*next;

					foreach(idImage image in _backgroundImageLoads)
					{
						if(image.BackgroundLoadComplete == true)
						{
							_activeBackgroundImageLoads--;

					fileSystem->CloseFile( image->bgl.f );
					// upload the image
					image->UploadPrecompressedImage( (byte *)image->bgl.file.buffer, image->bgl.file.length );
					R_StaticFree( image->bgl.file.buffer );
					if ( image_showBackgroundLoads.GetBool() ) {
						common->Printf( "R_CompleteBackgroundImageLoad: %s\n", image->imgName.c_str() );
					}
				} else {
					image->bglNext = remainingList;
					remainingList = image;
				}
			}
			if ( image_showBackgroundLoads.GetBool() ) {
				static int prev;
				if ( numActiveBackgroundImageLoads != prev ) {
					prev = numActiveBackgroundImageLoads;
					common->Printf( "background Loads: %i\n", numActiveBackgroundImageLoads );
				}
			}

			backgroundImageLoads = remainingList;*/
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
			_flatNormalMap = LoadFromCallback("_flat", GenerateFlatNormalImage);
			_ambientNormalMap = LoadFromCallback("_ambient", GenerateAmbientNormalImage);
			_specularTableImage = LoadFromCallback("_specularTable", GenerateSpecularTableImage);
			_specular2DTableImage = LoadFromCallback("_specular2DTable", GenerateSpecular2DTableImage);
			_rampImage = LoadFromCallback("_ramp", GenerateRampImage);
			_alphaRampImage = LoadFromCallback("_alphaRamp", GenerateRampImage);
			_alphaNotchImage = LoadFromCallback("_alphaNotch", GenerateAlphaNotchImage);
			_fogImage = LoadFromCallback("_fog", GenerateFogImage);
			_fogEnterImage = LoadFromCallback("_fogEnter", GenerateFogEnterImage);
			// TODO: _normalCubeMapImage = LoadFromCallback("_normalCubeMap", makeNormalizeVectorCubeMap);
			_noFalloffImage = LoadFromCallback("_noFalloff", GenerateNoFalloffImage);

			LoadFromCallback("_quadratic", GenerateQuadraticImage);

			// cinematicImage is used for cinematic drawing
			// scratchImage is used for screen wipes/doublevision etc..
			_cinematicImage = LoadFromCallback("_cinematic", GenerateRGBA8Image);
			_scratchImage = LoadFromCallback("_scratch", GenerateRGBA8Image);
			_scratchImage2 = LoadFromCallback("_scratch2", GenerateRGBA8Image);
			_accumImage = LoadFromCallback("_accum", GenerateRGBA8Image);
			// TODO: _scratchCubeMapImage = LoadFromCallback("_scratchCubeMap", makeNormalizeVectorCubeMap);
			_currentRenderImage = LoadFromCallback("_currentRender", GenerateRGBA8Image);

			// TODO: cmds
			/*cmdSystem->AddCommand("reloadImages", R_ReloadImages_f, CMD_FL_RENDERER, "reloads images");
			cmdSystem->AddCommand("listImages", R_ListImages_f, CMD_FL_RENDERER, "lists images");
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

			// see if the image already exists
			if(_images.ContainsKey(name) == true)
			{
				return _images[name];
			}

			// create the image and issue the callback
			idImage image = CreateImage(name);
			image.Generator = generator;

			if(idE.CvarSystem.GetBool("image_preload") == true)
			{
				// check for precompressed, load is from the front end
				image.ReferencedOutsideLevelLoad = true;
				image.ActuallyLoadImage(true, false);
			}

			return image;
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
				// TODO: pics[i] = data[0, 0];
			}

			// this must be a cube map for fragment programs to simply substitute for the normalization cube map
			idConsole.WriteLine("TODO: image->GenerateCubeImage( pics, 2, TF_DEFAULT, true, TD_HIGH_QUALITY );");
		}

		public static void GenerateBlackImage(idImage image)
		{
			// solid black texture
			byte[] data = new byte[DefaultImageSize * DefaultImageSize * 4];
			image.Generate(data, DefaultImageSize, DefaultImageSize, TextureFilter.Default, false, TextureRepeat.Repeat, TextureDepth.Default);
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

			Gl.glTexParameterfv(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_BORDER_COLOR, color);
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
					int b = (int) (Math.Pow(f, y) * 255.0f);

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
			float total = Math.Abs(targetHeight - viewHeight);
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
			image.Generate(idHelper.Flatten<byte>(data), FogEnterSize, FogEnterSize, TextureFilter.Linear, false, TextureRepeat.Clamp, TextureDepth.HighQuality);
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
					float d = (float) Math.Sqrt((x - (FogSize / 2)) * (x - (FogSize / 2))
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
					d = Math.Abs(d);
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
		#endregion

		#region Private
		private void InitFilters()
		{
			ImageFilters.Add("GL_LINEAR_MIPMAP_NEAREST", new ImageFilter("GL_LINEAR_MIPMAP_NEAREST", Gl.GL_LINEAR_MIPMAP_NEAREST, Gl.GL_LINEAR));
			ImageFilters.Add("GL_LINEAR_MIPMAP_LINEAR", new ImageFilter("GL_LINEAR_MIPMAP_LINEAR", Gl.GL_LINEAR_MIPMAP_LINEAR, Gl.GL_LINEAR));
			ImageFilters.Add("GL_NEAREST", new ImageFilter("GL_NEAREST", Gl.GL_NEAREST, Gl.GL_NEAREST));
			ImageFilters.Add("GL_LINEAR", new ImageFilter("GL_LINEAR", Gl.GL_LINEAR, Gl.GL_LINEAR));
			ImageFilters.Add("GL_NEAREST_MIPMAP_NEAREST", new ImageFilter("GL_NEAREST_MIPMAP_NEAREST", Gl.GL_NEAREST_MIPMAP_NEAREST, Gl.GL_NEAREST));
			ImageFilters.Add("GL_NEAREST_MIPMAP_LINEAR", new ImageFilter("GL_NEAREST_MIPMAP_LINEAR", Gl.GL_NEAREST_MIPMAP_LINEAR, Gl.GL_NEAREST));
		}

		private void InitCvars()
		{
			new idCvar("image_filter", "GL_LINEAR_MIPMAP_LINEAR", ImageFilters.Keys.ToArray(), "changes texture filtering on mipmapped images", new ArgCompletion_String(ImageFilters.Keys.ToArray()), CvarFlags.Renderer | CvarFlags.Archive);
			new idCvar("image_anisotropy", "1", "set the maximum texture anisotropy if available", CvarFlags.Renderer | CvarFlags.Archive);
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

		private idImage CreateImage(string name)
		{
			idImage image = new idImage(name);
			_images.Add(name, image);

			return image;
		}
		#endregion
		#endregion
	}

	public delegate void ImageLoadCallback(idImage image);

	public struct ImageFilter
	{
		public string Label;
		public int Minimize;
		public int Maximize;

		public ImageFilter(string label, int min, int max)
		{
			this.Label = label;
			this.Minimize = min;
			this.Maximize = max;
		}
	}
}