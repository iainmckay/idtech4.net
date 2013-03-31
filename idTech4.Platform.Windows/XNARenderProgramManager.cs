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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using idTech4.Services;

namespace idTech4.Platform.Windows
{
	public class XNARenderProgramManager
	{
		#region Properties
		public Effect Effect
		{
			get
			{
				return _currentEffect;
			}
		}
		#endregion

		#region Members
		private ContentManager _contentManager;

		private Effect _currentEffect;
		private Effect[] _effects         = new Effect[(int) BuiltinShader.Count];
		private Vector4[] _effectUniforms = new Vector4[(int) RenderParameter.User + Constants.MaxEffectUserParameters];
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

			builtins[(int) BuiltinShader.Shadow]                        = "shadow";
			builtins[(int) BuiltinShader.ShadowSkinned]                 = "shadow_skinned";
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
			
			for(int i = 0; i < builtins.Length; i++)
			{
				/*vertexShaders[i].name = builtins[i].name;
				fragmentShaders[i].name = builtins[i].name;
				builtinShaders[builtins[i].index] = i;
				LoadVertexShader( i );
				LoadFragmentShader( i );*/

				string fileName = Path.Combine("renderprogs", "xna", builtins[i]);

				if(fileSystem.FileExists(fileName + ".xnb") == true)
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

		#region Binding
		public void BindBuiltin(BuiltinShader shader)
		{
			// TODO: RENDERLOG_PRINTF( "Binding GLSL Program %s\n", glslPrograms[vIndex].name.c_str() );
			_currentEffect = _effects[(int) shader];
		}

		public void Unbind()
		{
			_currentEffect = null;
		}
		#endregion

		#region Uniforms
		private RenderParameter GetRenderParameterFromName(string name)
		{
			switch(name)
			{
				case "g_ScreenCorrectionFactor":     return RenderParameter.ScreenCorrectionFactor;
				case "g_WindowCoordinate":           return RenderParameter.WindowCoordinate;
				case "g_DiffuseModifier":            return RenderParameter.DiffuseModifier;
				case "g_SpecularModifier":           return RenderParameter.SpecularModifier;

				case "g_LocalLightOrigin":           return RenderParameter.LocalLightOrigin;
				case "g_LocalViewOrigin":            return RenderParameter.LocalViewOrigin;

				case "g_LightProjectionS":           return RenderParameter.LightProjectionS;
				case "g_LightProjectionT":           return RenderParameter.LightProjectionT;
				case "g_LightProjectionQ":           return RenderParameter.LightProjectionQ;
				case "g_LightFallOffS":              return RenderParameter.LightFallOffS;

				case "g_BumpMatrixS":                return RenderParameter.BumpMatrixS;
				case "g_BumpMatrixT":                return RenderParameter.BumpMatrixT;

				case "g_DiffuseMatrixS":             return RenderParameter.DiffuseMatrixS;
				case "g_DiffuseMatrixT":             return RenderParameter.DiffuseMatrixT;

				case "g_SpecularMatrixS":            return RenderParameter.SpecularMatrixS;
				case "g_SpecularMatrixT":            return RenderParameter.SpecularMatrixT;

				case "g_VertexColorModulate":        return RenderParameter.VertexColorModulate;
				case "g_VertexColorAdd":             return RenderParameter.VertexColorAdd;

				case "g_Color":                      return RenderParameter.Color;
				case "g_ViewOrigin":                 return RenderParameter.ViewOrigin;
				case "g_GlobalEyePosition":          return RenderParameter.GlobalEyePosition;

				case "g_ModelViewProjectionMatrixX": return RenderParameter.ModelViewProjectionMatrixX;
				case "g_ModelViewProjectionMatrixY": return RenderParameter.ModelViewProjectionMatrixY;
				case "g_ModelViewProjectionMatrixZ": return RenderParameter.ModelViewProjectionMatrixZ;
				case "g_ModelViewProjectionMatrixW": return RenderParameter.ModelViewProjectionMatrixW;

				case "g_ModelMatrixX":               return RenderParameter.ModelMatrixX;
				case "g_ModelMatrixY":               return RenderParameter.ModelMatrixY;
				case "g_ModelMatrixZ":               return RenderParameter.ModelMatrixZ;
				case "g_ModelMatrixW":               return RenderParameter.ModelMatrixW;

				case "g_ProjectionMatrixX":          return RenderParameter.ProjectionMatrixX;
				case "g_ProjectionMatrixY":          return RenderParameter.ProjectionMatrixY;
				case "g_ProjectionMatrixZ":          return RenderParameter.ProjectionMatrixZ;
				case "g_ProjectionMatrixW":          return RenderParameter.ProjectionMatrixW;

				case "g_ModelViewMatrixX":           return RenderParameter.ModelViewMatrixX;
				case "g_ModelViewMatrixY":           return RenderParameter.ModelViewMatrixY;
				case "g_ModelViewMatrixZ":           return RenderParameter.ModelViewMatrixZ;
				case "g_ModelViewMatrixW":           return RenderParameter.ModelViewMatrixW;

				case "g_TextureMatrixS":             return RenderParameter.TextureMatrixS;
				case "g_TextureMatrixT":             return RenderParameter.TextureMatrixT;

				case "g_TextureCoordinates0S":       return RenderParameter.TextureCoordinates0S;
				case "g_TextureCoordinates0T":       return RenderParameter.TextureCoordinates0T;
				case "g_TextureCoordinates0Q":       return RenderParameter.TextureCoordinates0Q;
				case "g_TextureCoordinates0Enabled": return RenderParameter.TextureCoordinates0Enabled;

				case "g_TextureCoordinates1S":       return RenderParameter.TextureCoordinates1S;
				case "g_TextureCoordinates1T":       return RenderParameter.TextureCoordinates1T;
				case "g_TextureCoordinates1Q":       return RenderParameter.TextureCoordinates1Q;
				case "g_TextureCoordinates1Enabled": return RenderParameter.TextureCoordinates1Enabled;

				case "g_WobbleSkyX":                 return RenderParameter.WobbleSkyX;
				case "g_WobbleSkyY":                 return RenderParameter.WobbleSkyY;
				case "g_WobbleSkyZ":                 return RenderParameter.WobbleSkyZ;

				case "g_OverBright":                 return RenderParameter.OverBright;
				case "g_EnableSkinning":             return RenderParameter.EnableSkinning;
				case "g_AlphaTest":                  return RenderParameter.AlphaTest;

				case "g_User1":                      return RenderParameter.User1;
				case "g_User2":                      return RenderParameter.User2;
				case "g_User3":                      return RenderParameter.User3;
				case "g_User4":                      return RenderParameter.User4;
				case "g_User5":                      return RenderParameter.User5;
				case "g_User6":                      return RenderParameter.User6;
				case "g_User7":                      return RenderParameter.User7;
				case "g_User8":                      return RenderParameter.User8;
			}

			throw new ArgumentException("name");
		}

		public void CommitUniforms()
		{
			foreach(EffectParameter effectParameter in _currentEffect.Parameters)
			{
				effectParameter.SetValue(_effectUniforms[(int) GetRenderParameterFromName(effectParameter.Name)]);
			}			
		}

		public void SetUniformValue(RenderParameter renderParameter, Vector4 value)
		{
			_effectUniforms[(int) renderParameter] = value;
		}
		#endregion
	}

	public enum BuiltinShader
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

	// This enum list corresponds to the global constant register indecies as defined in global.inc for all
	// shaders.  We used a shared pool to keeps things simple.  If something changes here then it also
	// needs to change in global.inc and vice versa
	public enum RenderParameter
	{
		// For backwards compatibility, do not change the order of the first 17 items
		ScreenCorrectionFactor = 0,
		WindowCoordinate,
		DiffuseModifier,
		SpecularModifier,

		LocalLightOrigin,
		LocalViewOrigin,

		LightProjectionS,
		LightProjectionT,
		LightProjectionQ,
		LightFallOffS,

		BumpMatrixS,
		BumpMatrixT,

		DiffuseMatrixS,
		DiffuseMatrixT,

		SpecularMatrixS,
		SpecularMatrixT,

		VertexColorModulate,
		VertexColorAdd,

		// The following are new and can be in any order

		Color,
		ViewOrigin,
		GlobalEyePosition,

		ModelViewProjectionMatrixX,
		ModelViewProjectionMatrixY,
		ModelViewProjectionMatrixZ,
		ModelViewProjectionMatrixW,

		ModelMatrixX,
		ModelMatrixY,
		ModelMatrixZ,
		ModelMatrixW,

		ProjectionMatrixX,
		ProjectionMatrixY,
		ProjectionMatrixZ,
		ProjectionMatrixW,

		ModelViewMatrixX,
		ModelViewMatrixY,
		ModelViewMatrixZ,
		ModelViewMatrixW,

		TextureMatrixS,
		TextureMatrixT,

		TextureCoordinates0S,
		TextureCoordinates0T,
		TextureCoordinates0Q,
		TextureCoordinates0Enabled,

		TextureCoordinates1S,
		TextureCoordinates1T,
		TextureCoordinates1Q,
		TextureCoordinates1Enabled,

		WobbleSkyX,
		WobbleSkyY,
		WobbleSkyZ,

		OverBright,
		EnableSkinning,
		AlphaTest,

		Total,
		User,
		User1,
		User2,
		User3,
		User4,
		User5,
		User6,
		User7,
		User8
	}
}