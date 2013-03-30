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
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using idTech4.Services;

namespace idTech4.Platform.Windows
{
	public class XNARenderProgramManager
	{
		#region Members
		private List<Effect> _effects;
		private ContentManager _contentManager;
		#endregion

		#region Constructor
		public XNARenderProgramManager()
		{

		}
		#endregion

		#region Initialization
		public void Initialize()
		{
			idLog.WriteLine("----- Initializing Render Shaders -----");

			_contentManager = idEngine.Instance.Content;

			IFileSystem fileSystem = idEngine.Instance.GetService<IFileSystem>();

			string[] builtins                                           = new string[(int) BuiltinShader.Count];
			builtins[(int) BuiltinShader.Gui]                           = "gui";
			builtins[(int) BuiltinShader.Color]                         = "color";
			builtins[(int) BuiltinShader.SimpleShade]                   = "simpleshade";
			builtins[(int) BuiltinShader.Textured]                      = "texture";
			builtins[(int) BuiltinShader.TextureVertexColor]            = "texture_color";
			builtins[(int) BuiltinShader.TextureVertexColorSkinned]     = "texture_color_skinned";
			builtins[(int) BuiltinShader.TextureCoordinatesVertexColor] = "texture_color_texgen";
			builtins[(int) BuiltinShader.Interaction]                   = "interaction";
			builtins[(int) BuiltinShader.InteractionSkinned]            = "interaction_skinned";
			builtins[(int) BuiltinShader.InteractionAmbient]            = "interactionAmbient";
			builtins[(int) BuiltinShader.InteractionAmbientSkinned]     = "interactionAmbient_skinned";
			builtins[(int) BuiltinShader.Environment]                   = "environment";
			builtins[(int) BuiltinShader.EnvironmentSkinned]            = "environment_skinned";
			builtins[(int) BuiltinShader.BumpyEnvironment]              = "bumpyEnvironment";
			builtins[(int) BuiltinShader.BumpyEnvironmentSkinned]       = "bumpyEnvironment_skinned";

			builtins[(int) BuiltinShader.Depth]                         = "depth";
			builtins[(int) BuiltinShader.DepthSkinned]                  = "depth_skinned";
			builtins[(int) BuiltinShader.ShadowDebug]                   = "shadowDebug";
			builtins[(int) BuiltinShader.ShadowDebugSkinned]            = "shadowDebug_skinned";

			builtins[(int) BuiltinShader.BlendLight]                    = "blendlight";
			builtins[(int) BuiltinShader.Fog]                           = "fog";
			builtins[(int) BuiltinShader.FogSkinned]                    = "fog_skinned";
			builtins[(int) BuiltinShader.SkyBox]                        = "skybox";
			builtins[(int) BuiltinShader.WobbleSky]                     = "wobblesky";
			builtins[(int) BuiltinShader.PostProcess]                   = "postprocess";
			builtins[(int) BuiltinShader.StereoDeGhost]                 = "stereoDeGhost";
			builtins[(int) BuiltinShader.StereoWarp]                    = "stereoWarp";
			builtins[(int) BuiltinShader.ZCullReconstruct]              = "zcullReconstruct";
			builtins[(int) BuiltinShader.Bink]                          = "bink";
			builtins[(int) BuiltinShader.BinkGui]                       = "bink_gui";
			builtins[(int) BuiltinShader.StereoInterface]               = "stereoInterlace";
			builtins[(int) BuiltinShader.MotionBlur]                    = "motionBlur";

			_effects = new List<Effect>(builtins.Length);

			for(int i = 0; i < builtins.Length; i++)
			{
				/*vertexShaders[i].name = builtins[i].name;
				fragmentShaders[i].name = builtins[i].name;
				builtinShaders[builtins[i].index] = i;
				LoadVertexShader( i );
				LoadFragmentShader( i );*/

				string fileName = Path.Combine("renderprogs", "xna", builtins[i]);

				if(fileSystem.FileExists(fileName) == true)
				{
					idLog.WriteLine("...loading {0}", fileName);
					_effects[i] = _contentManager.Load<Effect>(fileName);
				}
				else
				{
					idLog.WriteLine("...couldn't find {0}", fileName);
				}				
			}

			// Special case handling for fastZ shaders
			// TODO: builtinShaders[BUILTIN_SHADOW] = FindVertexShader( "shadow.vp" );
			// TODO: builtinShaders[BUILTIN_SHADOW_SKINNED] = FindVertexShader( "shadow_skinned.vp" );

			// TODO: FindGLSLProgram( "shadow.vp", builtinShaders[BUILTIN_SHADOW], -1 );
			// TODO: FindGLSLProgram( "shadow_skinned.vp", builtinShaders[BUILTIN_SHADOW_SKINNED], -1 );

			// TODO: glslUniforms.SetNum( RENDERPARM_USER + MAX_GLSL_USER_PARMS, vec4_zero );

			// TODO: 
			/*vertexShaders[builtinShaders[BUILTIN_TEXTURE_VERTEXCOLOR_SKINNED]].usesJoints = true;
			vertexShaders[builtinShaders[BUILTIN_INTERACTION_SKINNED]].usesJoints = true;
			vertexShaders[builtinShaders[BUILTIN_INTERACTION_AMBIENT_SKINNED]].usesJoints = true;
			vertexShaders[builtinShaders[BUILTIN_ENVIRONMENT_SKINNED]].usesJoints = true;
			vertexShaders[builtinShaders[BUILTIN_BUMPY_ENVIRONMENT_SKINNED]].usesJoints = true;
			vertexShaders[builtinShaders[BUILTIN_DEPTH_SKINNED]].usesJoints = true;
			vertexShaders[builtinShaders[BUILTIN_SHADOW_SKINNED]].usesJoints = true;
			vertexShaders[builtinShaders[BUILTIN_SHADOW_DEBUG_SKINNED]].usesJoints = true;
			vertexShaders[builtinShaders[BUILTIN_FOG_SKINNED]].usesJoints = true;

			cmdSystem->AddCommand( "reloadShaders", R_ReloadShaders, CMD_FL_RENDERER, "reloads shaders" );*/
		}
		#endregion

		#region BuiltinShader
		private enum BuiltinShader
		{
			Gui,
			Color,
			SimpleShade,
			Textured,
			TextureVertexColor,
			TextureVertexColorSkinned,
			TextureCoordinatesVertexColor,

			Interaction,
			InteractionSkinned,
			InteractionAmbient,
			InteractionAmbientSkinned,

			Environment,
			EnvironmentSkinned,

			BumpyEnvironment,
			BumpyEnvironmentSkinned,

			Depth,
			DepthSkinned,

			Shadow,
			ShadowSkinned,
			ShadowDebug,
			ShadowDebugSkinned,

			BlendLight,
			Fog,
			FogSkinned,
			SkyBox,
			WobbleSky,
			PostProcess,
			StereoDeGhost,
			StereoWarp,
			ZCullReconstruct,
			Bink,
			BinkGui,
			StereoInterface,
			MotionBlur,

			Count
		}
		#endregion
	}
}