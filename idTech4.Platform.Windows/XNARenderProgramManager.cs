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
		private BackendState _state;

		private Effect _currentEffect;
		private Effect[] _effects         = new Effect[(int) BuiltinShader.Count];
		//private Vector4[] _effectUniforms = new Vector4[(int) RenderParameter.User + Constants.MaxEffectUserParameters];

		private Uniform[] _effectUniforms                         = new Uniform[(int) RenderParameter.User1 + Constants.MaxEffectUserParameters];
		private Dictionary<string, Uniform> _effectUniformsByName = new Dictionary<string, Uniform>();
		#endregion

		#region Constructor
		internal XNARenderProgramManager(BackendState state)
		{
			_state = state;
		}
		#endregion

		#region Initialization
		public void Initialize()
		{
			idLog.WriteLine("----- Initializing Render Shaders -----");

			_contentManager = idEngine.Instance.Content;

			IFileSystem fileSystem = idEngine.Instance.GetService<IFileSystem>();

			string[] builtins                                           = new string[(int) BuiltinShader.Count];
			builtins[(int) BuiltinShader.Gui] = "gui";
			builtins[(int) BuiltinShader.Color] = "color";
			builtins[(int) BuiltinShader.SimpleShade] = "simpleshade";
			builtins[(int) BuiltinShader.Textured] = "texture";
			builtins[(int) BuiltinShader.TextureVertexColor] = "texture_color";
			builtins[(int) BuiltinShader.TextureVertexColorSkinned] = "texture_color_skinned";
			builtins[(int) BuiltinShader.TextureCoordinatesVertexColor] = "texture_color_texgen";
			builtins[(int) BuiltinShader.Interaction] = "interaction";
			builtins[(int) BuiltinShader.InteractionSkinned] = "interaction_skinned";
			builtins[(int) BuiltinShader.InteractionAmbient] = "interactionAmbient";
			builtins[(int) BuiltinShader.InteractionAmbientSkinned] = "interactionAmbient_skinned";
			builtins[(int) BuiltinShader.Environment] = "environment";
			builtins[(int) BuiltinShader.EnvironmentSkinned] = "environment_skinned";
			builtins[(int) BuiltinShader.BumpyEnvironment] = "bumpyEnvironment";
			builtins[(int) BuiltinShader.BumpyEnvironmentSkinned] = "bumpyEnvironment_skinned";

			builtins[(int) BuiltinShader.Depth] = "depth";
			builtins[(int) BuiltinShader.DepthSkinned] = "depth_skinned";

			builtins[(int) BuiltinShader.Shadow] = "shadow";
			builtins[(int) BuiltinShader.ShadowSkinned] = "shadow_skinned";
			builtins[(int) BuiltinShader.ShadowDebug] = "shadowDebug";
			builtins[(int) BuiltinShader.ShadowDebugSkinned] = "shadowDebug_skinned";

			builtins[(int) BuiltinShader.BlendLight] = "blendlight";
			builtins[(int) BuiltinShader.Fog] = "fog";
			builtins[(int) BuiltinShader.FogSkinned] = "fog_skinned";
			builtins[(int) BuiltinShader.SkyBox] = "skybox";
			builtins[(int) BuiltinShader.WobbleSky] = "wobblesky";
			builtins[(int) BuiltinShader.PostProcess] = "postprocess";
			builtins[(int) BuiltinShader.StereoDeGhost] = "stereoDeGhost";
			builtins[(int) BuiltinShader.StereoWarp] = "stereoWarp";
			builtins[(int) BuiltinShader.ZCullReconstruct] = "zcullReconstruct";
			builtins[(int) BuiltinShader.Bink] = "bink";
			builtins[(int) BuiltinShader.BinkGui] = "bink_gui";
			builtins[(int) BuiltinShader.StereoInterface] = "stereoInterlace";
			builtins[(int) BuiltinShader.MotionBlur] = "motionBlur";

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

			_effectUniforms[(int) RenderParameter.ScreenCorrectionFactor] = new Vector4Uniform("g_ScreenCorrectionFactor");
			_effectUniforms[(int) RenderParameter.WindowCoordinate] = new Vector4Uniform("g_WindowCoordinate");
			_effectUniforms[(int) RenderParameter.DiffuseModifier] = new Vector4Uniform("g_DiffuseModifier");
			_effectUniforms[(int) RenderParameter.SpecularModifier] = new Vector4Uniform("g_SpecularModifier");

			_effectUniforms[(int) RenderParameter.LocalLightOrigin] = new Vector4Uniform("g_LocalLightOrigin");
			_effectUniforms[(int) RenderParameter.LocalViewOrigin] = new Vector4Uniform("g_LocalViewOrigin");

			_effectUniforms[(int) RenderParameter.LightProjectionS] = new Vector4Uniform("g_LightProjectionS");
			_effectUniforms[(int) RenderParameter.LightProjectionT] = new Vector4Uniform("g_LightProjectionT");
			_effectUniforms[(int) RenderParameter.LightProjectionQ] = new Vector4Uniform("g_LightProjectionQ");
			_effectUniforms[(int) RenderParameter.LightFallOffS] = new Vector4Uniform("g_LightFallOffS");

			_effectUniforms[(int) RenderParameter.BumpMatrixS] = new Vector4Uniform("g_BumpMatrixS");
			_effectUniforms[(int) RenderParameter.BumpMatrixT] = new Vector4Uniform("g_BumpMatrixT");

			_effectUniforms[(int) RenderParameter.DiffuseMatrixS] = new Vector4Uniform("g_DiffuseMatrixS");
			_effectUniforms[(int) RenderParameter.DiffuseMatrixT] = new Vector4Uniform("g_DiffuseMatrixT");

			_effectUniforms[(int) RenderParameter.SpecularMatrixS] = new Vector4Uniform("g_SpecularMatrixS");
			_effectUniforms[(int) RenderParameter.SpecularMatrixT] = new Vector4Uniform("g_SpecularMatrixT");

			_effectUniforms[(int) RenderParameter.VertexColorModulate] = new Vector4Uniform("g_VertexColorModulate");
			_effectUniforms[(int) RenderParameter.VertexColorAdd] = new Vector4Uniform("g_VertexColorAdd");

			_effectUniforms[(int) RenderParameter.Color] = new Vector4Uniform("g_Color");
			_effectUniforms[(int) RenderParameter.ViewOrigin] = new Vector4Uniform("g_ViewOrigin");
			_effectUniforms[(int) RenderParameter.GlobalEyePosition] = new Vector4Uniform("g_GlobalEyePosition");

			_effectUniforms[(int) RenderParameter.ModelViewProjectionMatrix] = new MatrixUniform("g_ModelViewProjectionMatrix");
			_effectUniforms[(int) RenderParameter.ModelMatrix] = new MatrixUniform("g_ModelMatrix");
			_effectUniforms[(int) RenderParameter.ProjectionMatrix] = new MatrixUniform("g_ProjectionMatrix");
			_effectUniforms[(int) RenderParameter.ModelViewMatrix] = new MatrixUniform("g_ModelViewMatrix");

			_effectUniforms[(int) RenderParameter.TextureMatrixS] = new Vector4Uniform("g_TextureMatrixS");
			_effectUniforms[(int) RenderParameter.TextureMatrixT] = new Vector4Uniform("g_TextureMatrixT");

			_effectUniforms[(int) RenderParameter.Texture0] = new TextureUniform("g_Texture0");
			_effectUniforms[(int) RenderParameter.Texture1] = new TextureUniform("g_Texture1");
			_effectUniforms[(int) RenderParameter.Texture2] = new TextureUniform("g_Texture2");
			_effectUniforms[(int) RenderParameter.Texture3] = new TextureUniform("g_Texture3");
			_effectUniforms[(int) RenderParameter.Texture4] = new TextureUniform("g_Texture4");
			_effectUniforms[(int) RenderParameter.Texture5] = new TextureUniform("g_Texture5");
			_effectUniforms[(int) RenderParameter.Texture6] = new TextureUniform("g_Texture6");
			_effectUniforms[(int) RenderParameter.Texture7] = new TextureUniform("g_Texture7");

			_effectUniforms[(int) RenderParameter.TextureCoordinates0S] = new Vector4Uniform("g_TextureCoordinates0S");
			_effectUniforms[(int) RenderParameter.TextureCoordinates0T] = new Vector4Uniform("g_TextureCoordinates0T");
			_effectUniforms[(int) RenderParameter.TextureCoordinates0Q] = new Vector4Uniform("g_TextureCoordinates0Q");
			_effectUniforms[(int) RenderParameter.TextureCoordinates0Enabled] = new Vector4Uniform("g_TextureCoordinates0Enabled");

			_effectUniforms[(int) RenderParameter.TextureCoordinates1S] = new Vector4Uniform("g_TextureCoordinates1S");
			_effectUniforms[(int) RenderParameter.TextureCoordinates1T] = new Vector4Uniform("g_TextureCoordinates1T");
			_effectUniforms[(int) RenderParameter.TextureCoordinates1Q] = new Vector4Uniform("g_TextureCoordinates1Q");
			_effectUniforms[(int) RenderParameter.TextureCoordinates1Enabled] = new Vector4Uniform("g_TextureCoordinates1Enabled");

			_effectUniforms[(int) RenderParameter.WobbleSkyX] = new Vector4Uniform("g_WobbleSkyX");
			_effectUniforms[(int) RenderParameter.WobbleSkyY] = new Vector4Uniform("g_WobbleSkyY");
			_effectUniforms[(int) RenderParameter.WobbleSkyZ] = new Vector4Uniform("g_WobbleSkyZ");

			_effectUniforms[(int) RenderParameter.OverBright] = new Vector4Uniform("g_OverBright");
			_effectUniforms[(int) RenderParameter.EnableSkinning] = new Vector4Uniform("g_EnableSkinning");
			_effectUniforms[(int) RenderParameter.AlphaTest] = new Vector4Uniform("g_AlphaTest");

			_effectUniforms[(int) RenderParameter.User1] = new Vector4Uniform("g_User1");
			_effectUniforms[(int) RenderParameter.User2] = new Vector4Uniform("g_User2");
			_effectUniforms[(int) RenderParameter.User3] = new Vector4Uniform("g_User3");
			_effectUniforms[(int) RenderParameter.User4] = new Vector4Uniform("g_User4");
			_effectUniforms[(int) RenderParameter.User5] = new Vector4Uniform("g_User5");
			_effectUniforms[(int) RenderParameter.User6] = new Vector4Uniform("g_User6");
			_effectUniforms[(int) RenderParameter.User7] = new Vector4Uniform("g_User7");
			_effectUniforms[(int) RenderParameter.User8] = new Vector4Uniform("g_User8");

			for(int i = 0; i < _effectUniforms.Length; i++)
			{
				if(_effectUniforms[i] != null)
				{
					_effectUniformsByName.Add(_effectUniforms[i].Name, _effectUniforms[i]);
				}
			}

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
		public void CommitUniforms()
		{
			for(int i = 0; i < _state.TextureUnits.Length; i++)
			{
				((TextureUniform) _effectUniformsByName[string.Format("g_Texture{0}", i)]).Value = _state.TextureUnits[i].CurrentTexture;
			}

			foreach(EffectParameter effectParameter in _currentEffect.Parameters)
			{
				_effectUniformsByName[effectParameter.Name].Set(effectParameter);
			}
		}

		public void SetScreenCorrectionFactor(Vector4 value)
		{
			SetVector4(RenderParameter.ScreenCorrectionFactor, value);
		}

		public void SetWindowCoordinate(Vector4 value)
		{
			SetVector4(RenderParameter.WindowCoordinate, value);
		}

		public void SetDiffuseModifier(Vector4 value)
		{
			SetVector4(RenderParameter.DiffuseModifier, value);
		}

		public void SetSpecularModifier(Vector4 value)
		{
			SetVector4(RenderParameter.SpecularModifier, value);
		}

		public void SetLocalLightOrigin(Vector4 value)
		{
			SetVector4(RenderParameter.LocalLightOrigin, value);
		}

		public void SetLocalViewOrigin(Vector4 value)
		{
			SetVector4(RenderParameter.LocalViewOrigin, value);
		}

		public void SetLightProjectionS(Vector4 value)
		{
			SetVector4(RenderParameter.LightProjectionS, value);
		}

		public void SetLightProjectionT(Vector4 value)
		{
			SetVector4(RenderParameter.LightProjectionT, value);
		}

		public void SetLightProjectionQ(Vector4 value)
		{
			SetVector4(RenderParameter.LightProjectionQ, value);
		}

		public void SetLightFallOffS(Vector4 value)
		{
			SetVector4(RenderParameter.LightFallOffS, value);
		}

		public void SetBumpMatrixS(Vector4 value)
		{
			SetVector4(RenderParameter.BumpMatrixS, value);
		}

		public void SetBumpMatrixT(Vector4 value)
		{
			SetVector4(RenderParameter.BumpMatrixT, value);
		}

		public void SetDiffuseMatrixS(Vector4 value)
		{
			SetVector4(RenderParameter.DiffuseMatrixS, value);
		}

		public void SetDiffuseMatrixT(Vector4 value)
		{
			SetVector4(RenderParameter.DiffuseMatrixT, value);
		}

		public void SetSpecularMatrixS(Vector4 value)
		{
			SetVector4(RenderParameter.SpecularMatrixS, value);
		}

		public void SetSpecularMatrixT(Vector4 value)
		{
			SetVector4(RenderParameter.SpecularMatrixT, value);
		}

		public void SetVertexColorModulate(Vector4 value)
		{
			SetVector4(RenderParameter.VertexColorModulate, value);
		}

		public void SetVertexColorAdd(Vector4 value)
		{
			SetVector4(RenderParameter.VertexColorAdd, value);
		}

		public void SetColor(float r, float g, float b)
		{
			SetColor(r, g, b, 1.0f);
		}

		public void SetColor(float r, float g, float b, float a)
		{
			SetColor(new Vector4(MathHelper.Clamp(r, 0, 1), MathHelper.Clamp(g, 0, 1), MathHelper.Clamp(b, 0, 1), MathHelper.Clamp(a, 0, 1)));
		}

		public void SetColor(Vector4 value)
		{
			SetVector4(RenderParameter.Color, value);
		}

		public void SetViewOrigin(Vector4 value)
		{
			SetVector4(RenderParameter.ViewOrigin, value);
		}

		public void SetGlobalEyePosition(Vector4 value)
		{
			SetVector4(RenderParameter.GlobalEyePosition, value);
		}

		public void SetModelViewProjectionMatrix(Matrix value)
		{
			SetMatrix(RenderParameter.ModelViewProjectionMatrix, value);
		}

		public void SetModelMatrix(Matrix value)
		{
			SetMatrix(RenderParameter.ModelMatrix, value);
		}

		public void SetProjectionMatrix(Matrix value)
		{
			SetMatrix(RenderParameter.ProjectionMatrix, value);
		}

		public void SetModelViewMatrix(Matrix value)
		{
			SetMatrix(RenderParameter.ModelViewMatrix, value);
		}

		public void SetTextureMatrixS(Vector4 value)
		{
			SetVector4(RenderParameter.TextureMatrixS, value);
		}

		public void SetTextureMatrixT(Vector4 value)
		{
			SetVector4(RenderParameter.TextureMatrixT, value);
		}

		public void SetTexture0(Texture value)
		{
			SetTexture(RenderParameter.Texture0, value);
		}

		public void SetTexture1(Texture value)
		{
			SetTexture(RenderParameter.Texture1, value);
		}

		public void SetTexture2(Texture value)
		{
			SetTexture(RenderParameter.Texture2, value);
		}

		public void SetTexture3(Texture value)
		{
			SetTexture(RenderParameter.Texture3, value);
		}

		public void SetTexture4(Texture value)
		{
			SetTexture(RenderParameter.Texture4, value);
		}

		public void SetTexture5(Texture value)
		{
			SetTexture(RenderParameter.Texture5, value);
		}

		public void SetTexture6(Texture value)
		{
			SetTexture(RenderParameter.Texture6, value);
		}

		public void SetTexture7(Texture value)
		{
			SetTexture(RenderParameter.Texture7, value);
		}

		public void SetTextureCoordinates0S(Vector4 value)
		{
			SetVector4(RenderParameter.TextureCoordinates0S, value);
		}

		public void SetTextureCoordinates0T(Vector4 value)
		{
			SetVector4(RenderParameter.TextureCoordinates0T, value);
		}

		public void SetTextureCoordinates0Q(Vector4 value)
		{
			SetVector4(RenderParameter.TextureCoordinates0Q, value);
		}

		public void SetTextureCoordinates0Enabled(Vector4 value)
		{
			SetVector4(RenderParameter.TextureCoordinates0Enabled, value);
		}

		public void SetTextureCoordinates1S(Vector4 value)
		{
			SetVector4(RenderParameter.TextureCoordinates1S, value);
		}

		public void SetTextureCoordinates1T(Vector4 value)
		{
			SetVector4(RenderParameter.TextureCoordinates1T, value);
		}

		public void SetTextureCoordinates1Q(Vector4 value)
		{
			SetVector4(RenderParameter.TextureCoordinates1Q, value);
		}

		public void SetTextureCoordinates1Enabled(Vector4 value)
		{
			SetVector4(RenderParameter.TextureCoordinates1Enabled, value);
		}

		public void SetWobbleSkyX(Vector4 value)
		{
			SetVector4(RenderParameter.WobbleSkyX, value);
		}

		public void SetWobbleSkyY(Vector4 value)
		{
			SetVector4(RenderParameter.WobbleSkyY, value);
		}

		public void SetWobbleSkyZ(Vector4 value)
		{
			SetVector4(RenderParameter.WobbleSkyZ, value);
		}

		public void SetOverBright(Vector4 value)
		{
			SetVector4(RenderParameter.OverBright, value);
		}

		public void SetEnableSkinning(Vector4 value)
		{
			SetVector4(RenderParameter.EnableSkinning, value);
		}

		public void SetAlphaTest(Vector4 value)
		{
			SetVector4(RenderParameter.AlphaTest, value);
		}

		public void SetUser1(Vector4 value)
		{
			SetVector4(RenderParameter.User1, value);
		}

		public void SetUser2(Vector4 value)
		{
			SetVector4(RenderParameter.User2, value);
		}

		public void SetUser3(Vector4 value)
		{
			SetVector4(RenderParameter.User3, value);
		}

		public void SetUser4(Vector4 value)
		{
			SetVector4(RenderParameter.User4, value);
		}

		public void SetUser5(Vector4 value)
		{
			SetVector4(RenderParameter.User5, value);
		}

		public void SetUser6(Vector4 value)
		{
			SetVector4(RenderParameter.User6, value);
		}

		public void SetUser7(Vector4 value)
		{
			SetVector4(RenderParameter.User7, value);
		}

		public void SetUser8(Vector4 value)
		{
			SetVector4(RenderParameter.User8, value);
		}

		private void SetVector4(RenderParameter renderParameter, Vector4 value)
		{
			((Vector4Uniform) _effectUniforms[(int) renderParameter]).Value = value;
		}

		private void SetMatrix(RenderParameter renderParameter, Matrix value)
		{
			((MatrixUniform) _effectUniforms[(int) renderParameter]).Value = value;
		}

		private void SetTexture(RenderParameter renderParameter, Texture value)
		{
			((TextureUniform) _effectUniforms[(int) renderParameter]).Value = value;
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

		ModelViewProjectionMatrix,
		ModelMatrix,
		ProjectionMatrix,
		ModelViewMatrix,

		TextureMatrixS,
		TextureMatrixT,

		Texture0,
		Texture1,
		Texture2,
		Texture3,
		Texture4,
		Texture5,
		Texture6,
		Texture7,

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
		User1,
		User2,
		User3,
		User4,
		User5,
		User6,
		User7,
		User8
	}

	internal abstract class Uniform
	{
		public string Name;

		public abstract void Set(EffectParameter effectParameter);
	}

	internal class Vector4Uniform : Uniform
	{
		public Vector4 DefaultValue;
		public Vector4 Value;

		public Vector4Uniform(string name)
			: this(name, Vector4.Zero)
		{

		}

		public Vector4Uniform(string name, Vector4 defaultvalue)
		{
			this.Name = name;
			this.DefaultValue = defaultvalue;
		}

		public override void Set(EffectParameter effectParameter)
		{
			effectParameter.SetValue(this.Value);
		}
	}

	internal class MatrixUniform : Uniform
	{
		public Matrix DefaultValue;
		public Matrix Value;

		public MatrixUniform(string name)
			: this(name, Matrix.Identity)
		{

		}

		public MatrixUniform(string name, Matrix defaultvalue)
		{
			this.Name = name;
			this.DefaultValue = defaultvalue;
		}

		public override void Set(EffectParameter effectParameter)
		{
			effectParameter.SetValue(this.Value);
		}
	}

	internal class TextureUniform : Uniform
	{
		public Texture DefaultValue;
		public Texture Value;

		public TextureUniform(string name)
			: this(name, null)
		{

		}

		public TextureUniform(string name, Texture defaultvalue)
		{
			this.Name = name;
			this.DefaultValue = defaultvalue;
		}

		public override void Set(EffectParameter effectParameter)
		{
			effectParameter.SetValue(this.Value);
		}
	}
}