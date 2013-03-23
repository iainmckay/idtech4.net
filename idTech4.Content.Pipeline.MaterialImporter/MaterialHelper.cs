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
using Microsoft.Xna.Framework.Content.Pipeline;

using idTech4.Content.Pipeline.Intermediate.Material;
using idTech4.Content.Pipeline.Lexer;
using idTech4.Renderer;
using idTech4.Text;

namespace idTech4.Content.Pipeline
{
	public class MaterialHelper
	{
		private static LexerKeywordFactory _stageKeywordFactory;

		public static void ParseStage(idLexer lexer, ContentImporterContext context, MaterialContent materialContent, TextureRepeat textureRepeatDefault)
		{
			if(_stageKeywordFactory == null)
			{
				_stageKeywordFactory = new LexerKeywordFactory();
				_stageKeywordFactory.ScanNamespace("idTech4.Content.Pipeline.Intermediate.Material.Keywords.Stage");
			}

			TextureFilter textureFilter = TextureFilter.Default;
			TextureRepeat textureRepeat = textureRepeatDefault;
			TextureUsage textureDepth   = TextureUsage.Default;
			CubeFiles cubeMap           = CubeFiles.TwoD;
			
			NewMaterialStage newStage   = new NewMaterialStage();
			newStage.VertexParameters   = new int[4, 4];
			idLog.Warning("TODO: newStage.glslProgram = -1;");

			MaterialStage materialStage     = new MaterialStage();
			materialStage.ConditionRegister = GetExpressionConstant(1);

			materialStage.Color.Registers   = new int[4];
			materialStage.Color.Registers[0]
				= materialStage.Color.Registers[1]
				= materialStage.Color.Registers[2]
				= materialStage.Color.Registers[3] = GetExpressionConstant(1);

			

			string tokenValue;
			string tokenLower;

			while(true)
			{
				if(TestMaterialFlag(MaterialFlags.Defaulted) == true)
				{
					// we have a parse error
					return;
				}
				else if((token = lexer.ExpectAnyToken()) == null)
				{
					materialContent.MaterialFlags |= MaterialFlags.Defaulted;
					return;
				}

				tokenValue = token.ToString();
				tokenLower = tokenValue.ToLower();

				// the close brace for the entire material ends the draw block
				if(tokenLower == "}")
				{
					break;
				}

				LexerKeyword<MaterialContent> keyword = _stageKeywordFactory.Create<MaterialContent>(tokenValue);

				if(keyword == null)
				{
					context.Logger.LogWarning(null, null, "unknown stage material parameter '{0}'", tokenValue);
					return;
				}

				if(keyword.Parse(lexer, context, materialContent) == false)
				{
					context.Logger.LogWarning(null, null, "TODO: failed parsing");
				}
			
				
				// privatePolygonOffset
				
				
				
				
				// color mask options
				else if(tokenLower == "maskred")
				{
					materialStage.DrawStateBits |= MaterialStates.RedMask;
				}
				else if(tokenLower == "maskgreen")
				{
					materialStage.DrawStateBits |= MaterialStates.GreenMask;
				}
				else if(tokenLower == "maskblue")
				{
					materialStage.DrawStateBits |= MaterialStates.BlueMask;
				}
				else if(tokenLower == "maskalpha")
				{
					materialStage.DrawStateBits |= MaterialStates.AlphaMask;
				}
				else if(tokenLower == "maskcolor")
				{
					materialStage.DrawStateBits |= MaterialStates.ColorMask;
				}
				else if(tokenLower == "maskdepth")
				{
					materialStage.DrawStateBits |= MaterialStates.DepthMask;
				}
				else if(tokenLower == "alphatest")
				{
					materialStage.HasAlphaTest = true;
					materialStage.AlphaTestRegister = ParseExpression(lexer);

					_coverage = MaterialCoverage.Perforated;
				}
				// shorthand for 2D modulated
				else if(tokenLower == "colored")
				{
					materialStage.Color.Registers[0] = (int) ExpressionRegister.Parm0;
					materialStage.Color.Registers[1] = (int) ExpressionRegister.Parm1;
					materialStage.Color.Registers[2] = (int) ExpressionRegister.Parm2;
					materialStage.Color.Registers[3] = (int) ExpressionRegister.Parm3;

					_parsingData.RegistersAreConstant = false;
				}
				else if(tokenLower == "color")
				{
					materialStage.Color.Registers[0] = ParseExpression(lexer);
					MatchToken(lexer, ",");

					materialStage.Color.Registers[1] = ParseExpression(lexer);
					MatchToken(lexer, ",");

					materialStage.Color.Registers[2] = ParseExpression(lexer);
					MatchToken(lexer, ",");

					materialStage.Color.Registers[3] = ParseExpression(lexer);
				}
				else if(tokenLower == "red")
				{
					materialStage.Color.Registers[0] = ParseExpression(lexer);
				}
				else if(tokenLower == "green")
				{
					materialStage.Color.Registers[1] = ParseExpression(lexer);
				}
				else if(tokenLower == "blue")
				{
					materialStage.Color.Registers[2] = ParseExpression(lexer);
				}
				else if(tokenLower == "alpha")
				{
					materialStage.Color.Registers[3] = ParseExpression(lexer);
				}
				else if(tokenLower == "rgb")
				{
					materialStage.Color.Registers[0] = materialStage.Color.Registers[1] = materialStage.Color.Registers[2] = ParseExpression(lexer);
				}
				else if(tokenLower == "rgba")
				{
					materialStage.Color.Registers[0] = materialStage.Color.Registers[1] = materialStage.Color.Registers[2] = materialStage.Color.Registers[3] = ParseExpression(lexer);
				}
				else if(tokenLower == "if")
				{
					materialStage.ConditionRegister = ParseExpression(lexer);
				}
				else if(tokenLower == "program")
				{
					if((token = lexer.ReadTokenOnLine()) != null)
					{
						idLog.Warning("TODO: material program keyword");
						// TODO
						/*newStage.vertexProgram = renderProgManager.FindVertexShader(token.c_str());
						newStage.fragmentProgram = renderProgManager.FindFragmentShader(token.c_str());*/
					}
				}
				else if(tokenLower == "fragmentprogram")
				{
					if((token = lexer.ReadTokenOnLine()) != null)
					{
						idLog.Warning("TODO: material fragmentProgram keyword");
						// TODO
						//newStage.fragmentProgram = renderProgManager.FindFragmentShader( token.c_str() );
					}
				}
				else if(tokenLower == "vertexprogram")
				{
					if((token = lexer.ReadTokenOnLine()) != null)
					{
						idLog.Warning("TODO: material vertexProgram keyword");
						// TODO
						//newStage.vertexProgram = renderProgManager.FindVertexShader(token.c_str());
					}
				}
				else if(tokenLower == "vertexparm")
				{
					ParseVertexParameter(lexer, newStage);
				}
				else if(tokenLower == "vertexparm2")
				{
					ParseVertexParameter2(lexer, newStage);
				}
				else if(tokenLower == "fragmentmap")
				{
					ParseFragmentMap(lexer, ref newStage);
				}
				else
				{
					idLog.Warning("unknown token '{0}' in material '{1}'", tokenValue, this.Name);

					this.MaterialFlag = MaterialFlags.Defaulted;

					return;
				}
			}

			// if we are using newStage, allocate a copy of it
			if((newStage.FragmentProgram != 0) || (newStage.VertexProgram != 0))
			{
				idLog.Warning("TODO: newStage.glslProgram = renderProgManager.FindGLSLProgram(GetName(), newStage.vertexProgram, newStage.fragmentProgram);");
				materialStage.NewStage = newStage;
			}

			// select a compressed depth based on what the stage is
			if(textureDepth == TextureUsage.Default)
			{
				switch(materialStage.Lighting)
				{
					case StageLighting.Bump:
						textureDepth = TextureUsage.Bump;
						break;

					case StageLighting.Diffuse:
						textureDepth = TextureUsage.Diffuse;
						break;

					case StageLighting.Specular:
						textureDepth = TextureUsage.Specular;
						break;
				}
			}

			// create a new coverage stage on the fly - copy all data from the current stage
			if((textureDepth == TextureUsage.Diffuse) && (materialStage.HasAlphaTest == true))
			{
				// create new coverage stage
				MaterialStage newCoverageStage = (MaterialStage) materialStage.Clone();
				_parsingData.Stages.Add(newCoverageStage);

				// toggle alphatest off for the current stage so it doesn't get called during the depth fill pass
				materialStage.HasAlphaTest = false;

				// toggle alpha test on for the coverage stage
				newCoverageStage.HasAlphaTest = true;
				newCoverageStage.Lighting = StageLighting.Coverage;

				TextureStage coverageTextureStage = newCoverageStage.Texture;

				// now load the image with all the parms we parsed for the coverage stage
				if(string.IsNullOrEmpty(imageName) == false)
				{
					coverageTextureStage.Image = imageManager.LoadFromFile(imageName, textureFilter, textureRepeat, TextureUsage.Coverage, cubeMap);

					if(coverageTextureStage.Image == null)
					{
						coverageTextureStage.Image = imageManager.DefaultImage;
					}
				}
				else if(/*TODO: (coverageTextureStage.Cinematic == false) && */ (coverageTextureStage.Dynamic == 0) && (materialStage.NewStage == null))
				{
					idLog.Warning("material '{0}' had stage with no image", this.Name);

					coverageTextureStage.Image = imageManager.DefaultImage;
				}
			}

			// now load the image with all the parms we parsed
			if(string.IsNullOrEmpty(imageName) == false)
			{
				materialStage.Texture.Image = imageManager.LoadFromFile(imageName, textureFilter, textureRepeat, textureDepth, cubeMap);

				if(materialStage.Texture.Image == null)
				{
					materialStage.Texture.Image = imageManager.DefaultImage;
				}
			}
			else if(/*TODO: !ts->cinematic &&*/ (materialStage.Texture.Dynamic == 0) && (materialStage.NewStage.IsEmpty == true))
			{
				idLog.Warning("material '{0}' had stage with no image", this.Name);
				materialStage.Texture.Image = imageManager.DefaultImage;
			}

			// successfully parsed a stage.
			_parsingData.Stages.Add(materialStage);
		}
	}
}