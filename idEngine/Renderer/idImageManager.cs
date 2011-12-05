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
using Microsoft.Xna.Framework.Graphics;

namespace idTech4.Renderer
{
	public class idImageManager
	{
		#region Constants
		public static Dictionary<string, ImageFilter> ImageFilters = new Dictionary<string, ImageFilter>();
		#endregion

		#region Members
		private List<idImage> _images = new List<idImage>();

		private idImage _defaultImage;
		private idImage _flatNormalMap;					// 128 128 255 in all pixels.
		private idImage _ambientNormalMap;				// tr.ambientLightVector encoded in all pixels.
		private idImage _rampImage;						// 0-255 in RGBA in S.
		private idImage _alphaRampImage;				// 0-255 in alpha, 255 in RGB.
		private idImage _alphaNotchImage;				// 2x1 texture with just 1110 and 1111 with point sampling.
		private idImage _whiteImage;					// full of 0xff.
		private idImage _blackImage;					// full of 0x00.
		private idImage _normalCubeMapImage;			// cube map to normalize STR into RGB.
		private idImage _noFalloffImage;				// all 255, but zero clamped.
		private idImage _fogImage;						// increasing alpha is denser fog.
		private idImage _fogEnterImage;					// adjust fogImage alpha based on terminator plane.
		private idImage _cinematicImage;
		private idImage _scratchImage;
		private idImage _scratchImage2;
		private idImage _accumImage;
		private idImage _currentRenderImage;			// for SS_POST_PROCESS shaders.
		private idImage _scratchCubeMapImage;
		private idImage _specularTableImage;			// 1D intensity texture with our specular function.
		private idImage _specular2DTableImage;			// 2D intensity texture with our specular function with variable specularity.
		private idImage _borderClampImage;				// white inside, black outside.
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
		public void Init()
		{
			// set default texture filter modes
			ChangeTextureFilter();

			// create built in images
			/*defaultImage = ImageFromFunction("_default", R_DefaultImage);
			whiteImage = ImageFromFunction("_white", R_WhiteImage);
			blackImage = ImageFromFunction("_black", R_BlackImage);
			borderClampImage = ImageFromFunction("_borderClamp", R_BorderClampImage);
			flatNormalMap = ImageFromFunction("_flat", R_FlatNormalImage);
			ambientNormalMap = ImageFromFunction("_ambient", R_AmbientNormalImage);
			specularTableImage = ImageFromFunction("_specularTable", R_SpecularTableImage);
			specular2DTableImage = ImageFromFunction("_specular2DTable", R_Specular2DTableImage);
			rampImage = ImageFromFunction("_ramp", R_RampImage);
			alphaRampImage = ImageFromFunction("_alphaRamp", R_RampImage);
			alphaNotchImage = ImageFromFunction("_alphaNotch", R_AlphaNotchImage);
			fogImage = ImageFromFunction("_fog", R_FogImage);
			fogEnterImage = ImageFromFunction("_fogEnter", R_FogEnterImage);
			normalCubeMapImage = ImageFromFunction("_normalCubeMap", makeNormalizeVectorCubeMap);
			noFalloffImage = ImageFromFunction("_noFalloff", R_CreateNoFalloffImage);
			ImageFromFunction("_quadratic", R_QuadraticImage);

			// cinematicImage is used for cinematic drawing
			// scratchImage is used for screen wipes/doublevision etc..
			cinematicImage = ImageFromFunction("_cinematic", R_RGBA8Image);
			scratchImage = ImageFromFunction("_scratch", R_RGBA8Image);
			scratchImage2 = ImageFromFunction("_scratch2", R_RGBA8Image);
			accumImage = ImageFromFunction("_accum", R_RGBA8Image);
			scratchCubeMapImage = ImageFromFunction("_scratchCubeMap", makeNormalizeVectorCubeMap);
			currentRenderImage = ImageFromFunction("_currentRender", R_RGBA8Image);

			cmdSystem->AddCommand("reloadImages", R_ReloadImages_f, CMD_FL_RENDERER, "reloads images");
			cmdSystem->AddCommand("listImages", R_ListImages_f, CMD_FL_RENDERER, "lists images");
			cmdSystem->AddCommand("combineCubeImages", R_CombineCubeImages_f, CMD_FL_RENDERER, "combines six images for roq compression");*/

			// should forceLoadImages be here?
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
			/*


				if ( i == 6 ) {
					common->Warning( "bad r_textureFilter: '%s'", string);
					// default to LINEAR_MIPMAP_NEAREST
					i = 0;
				}

				// set the values for future images
				textureMinFilter = textureFilters[i].minimize;
				textureMaxFilter = textureFilters[i].maximize;
				textureAnisotropy = image_anisotropy.GetFloat();
				if ( textureAnisotropy < 1 ) {
					textureAnisotropy = 1;
				} else if ( textureAnisotropy > glConfig.maxTextureAnisotropy ) {
					textureAnisotropy = glConfig.maxTextureAnisotropy;
				}
				textureLODBias = image_lodbias.GetFloat();

				// change all the existing mipmap texture objects with default filtering

				for ( i = 0 ; i < images.Num() ; i++ ) {
					unsigned int	texEnum = GL_TEXTURE_2D;

					glt = images[ i ];

					switch( glt->type ) {
					case TT_2D:
						texEnum = GL_TEXTURE_2D;
						break;
					case TT_3D:
						texEnum = GL_TEXTURE_3D;
						break;
					case TT_CUBIC:
						texEnum = GL_TEXTURE_CUBE_MAP_EXT;
						break;
					}

					// make sure we don't start a background load
					if ( glt->texnum == idImage::TEXTURE_NOT_LOADED ) {
						continue;
					}
					glt->Bind();
					if ( glt->filter == TF_DEFAULT ) {
						qglTexParameterf(texEnum, GL_TEXTURE_MIN_FILTER, globalImages->textureMinFilter );
						qglTexParameterf(texEnum, GL_TEXTURE_MAG_FILTER, globalImages->textureMaxFilter );
					}
					if ( glConfig.anisotropicAvailable ) {
						qglTexParameterf(texEnum, GL_TEXTURE_MAX_ANISOTROPY_EXT, globalImages->textureAnisotropy );
					}	
					if ( glConfig.textureLODBiasAvailable ) {
						qglTexParameterf(texEnum, GL_TEXTURE_LOD_BIAS_EXT, globalImages->textureLODBias );
					}
				}
			}*/
		}
		#endregion

		#region Private
		private void InitFilters()
		{

		}

		private void InitCvars()
		{
			// TODO: new idCvar("image_filter", ImageFilters[1], "changes texture filtering on mipmapped images", ImageFilters.Keys.ToArray(), CvarFlags.Renderer | CvarFlags.Archive /* TODO: ,idCmdSystem::ArgCompletion_String<imageFilter> */);
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
		#endregion
		#endregion
	}

	public struct ImageFilter
	{
		public TextureFilter Minimize;
		public TextureFilter Maximize;

		public ImageFilter(TextureFilter min, TextureFilter max)
		{
			this.Minimize = min;
			this.Maximize = max;
		}
	}
}