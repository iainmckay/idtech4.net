﻿/*
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

using idTech4.Text;
using idTech4.Text.Decl;

namespace idTech4.Renderer
{
	/// <summary>
	/// Material.
	/// </summary>
	/// <remarks>
	/// Any errors during parsing just set MF_DEFAULTED and return, rather than throwing
	/// a hard error. This will cause the material to fall back to default material,
	/// but otherwise let things continue.
	/// <p/>
	/// Each material may have a set of calculations that must be evaluated before
	/// drawing with it.
	/// <p/>
	/// Every expression that a material uses can be evaluated at one time, which
	/// will allow for perfect common subexpression removal when I get around to
	/// writing it.
	/// <p/>
	/// Without this, scrolling an entire surface could result in evaluating the
	/// same texture matrix calculations a half dozen times.
	/// <p/>
	/// Open question: should I allow arbitrary per-vertex color, texCoord, and vertex
	/// calculations to be specified in the material code?
	/// <p/>
	/// Every stage will definately have a valid image pointer.
	/// <p/>
	/// We might want the ability to change the sort value based on conditionals,
	/// but it could be a hassle to implement,
	/// </remarks>
	public sealed class idMaterial : idDecl
	{
		#region Constants
		private const int TopPriority = 4;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets material specific flags.
		/// </summary>		
		public MaterialFlags MaterialFlag
		{
			get
			{
				return _materialFlags;
			}
			set
			{
				_materialFlags |= value;
			}
		}
		#endregion

		#region members
		private string _description;			// description
		private string _renderBump;				// renderbump command options, without the "renderbump" at the start.

		private ContentFlags _contentFlags;
		private SurfaceFlags _surfaceFlags;
		private MaterialFlags _materialFlags;
		private CullType _cullType;

		private DeformType _deformType;
		private idDecl _deformDecl;				// for surface emitted particle deforms and tables.
		private int[] _deformRegisters;			// numeric parameter for deforms.

		private DecalInfo _decalInfo;
		private int[] _texGenRegisters;

		private MaterialCoverage _coverage;
		private float _sort;					// lower numbered shaders draw before higher numbered.
		private bool _shouldCreateBackSides;

		// we defer loading of the editor image until it is asked for, so the game doesn't load up all the invisible and uncompressed images.
		// If editorImage is NULL, it will atempt to load editorImageName, and set editorImage to that or defaultImage
		private string _editorImageName;

		private bool _suppressInSubview;
		private bool _portalSky;
		private bool _fogLight;
		private bool _blendLight;
		private bool _ambientLight;
		private bool _unsmoothedTangents;
		private bool _hasSubview;
		private bool _allowOverlays;
		private bool _noFog;					// surface does not create fog interactions

		private float _polygonOffset;
		private int _spectrum;					// for invisible writing, used for both lights and surfaces.

		private MaterialParsingData _parsingData;
		#endregion

		#region Constructor
		public idMaterial()
		{
			Init();
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Test for existance of specific material flag(s).
		/// </summary>
		/// <param name="flag"></param>
		/// <returns></returns>
		public bool TestMaterialFlag(MaterialFlags flag)
		{
			return ((_materialFlags & flag) != 0);
		}
		#endregion

		#region Private
		private void Init()
		{
			_description = "<none>";
			_renderBump = string.Empty;

			_contentFlags = ContentFlags.Solid;
			_surfaceFlags = SurfaceFlags.None;
			_materialFlags = MaterialFlags.Defaulted;

			_sort = (float) MaterialSort.Bad;
			_coverage = MaterialCoverage.Bad;
			_cullType = CullType.Front;

			_deformType = DeformType.None;
			_deformRegisters = new int[4];

			/*numOps = 0;
			ops = NULL;
			numRegisters = 0;
			expressionRegisters = NULL;
			constantRegisters = NULL;
			numStages = 0;
			numAmbientStages = 0;
			stages = NULL;
			editorImage = NULL;
			lightFalloffImage = NULL;
			shouldCreateBackSides = false;
			entityGui = 0;*/

			_fogLight = false;
			_blendLight = false;
			_ambientLight = false;
			_noFog = false;
			_hasSubview = false;
			_allowOverlays = true;
			_unsmoothedTangents = false;

			/*gui = NULL;
			
			editorAlpha = 1.0;*/
			_spectrum = 0;
			/* refCount = 0;*/

			_polygonOffset = 0;
			_suppressInSubview = false;
			_portalSky = false;

			_decalInfo.StayTime = 10000;
			_decalInfo.FadeTime = 4000;
			_decalInfo.Start = new float[] { 1, 1, 1, 1 };
			_decalInfo.End = new float[] { 0, 0, 0, 0 };
		}

		/// <summary>
		/// Parses the material, if there are any errors during parsing the defaultShader will be set.
		/// </summary>
		/// <param name="lexer"></param>
		private void ParseMaterial(idLexer lexer)
		{
			int s = 0;

			TextureRepeat textureRepeatDefault = TextureRepeat.Repeat; // allow a global setting for repeat.
			idToken token = null;

			string tokenValue;
			string tokenLower;

			while(true)
			{
				if(TestMaterialFlag(Renderer.MaterialFlags.Defaulted) == true)
				{
					// we have a parse error.
					return;
				}

				if((token = lexer.ExpectAnyToken()) == null)
				{
					this.MaterialFlag = MaterialFlags.Defaulted;
					return;
				}

				tokenValue = token.ToString();
				tokenLower = tokenValue.ToLower();

				// end of material definition
				if(tokenLower == "}")
				{
					break;
				}
				else if(tokenLower == "qer_editorImage")
				{
					_editorImageName = lexer.ReadTokenOnLine().ToString();
					lexer.SkipRestOfLine();
				}
				else if(tokenLower == "description")
				{
					_description = lexer.ReadTokenOnLine().ToString();
				}
				// check for the surface / content bit flags
				else if(CheckSurfaceParameter(token) == true)
				{

				}
				else if(tokenLower == "polygonoffset")
				{
					this.MaterialFlag = Renderer.MaterialFlags.PolygonOffset;

					if((token = lexer.ReadTokenOnLine()) == null)
					{
						_polygonOffset = 1;
					}
					else
					{
						_polygonOffset = token.ToFloat();
					}
				}
				// noshadow
				else if(tokenLower == "noshadows")
				{
					this.MaterialFlag = MaterialFlags.NoShadows;
				}
				else if(tokenLower == "suppressinsubview")
				{
					_suppressInSubview = true;
				}
				else if(tokenLower == "portalsky")
				{
					_portalSky = true;
				}
				else if(tokenLower == "noselfshadow")
				{
					this.MaterialFlag = Renderer.MaterialFlags.NoSelfShadow;
				}
				else if(tokenLower == "noportalfog")
				{
					this.MaterialFlag = Renderer.MaterialFlags.NoPortalFog;
				}
				// forceShadows allows nodraw surfaces to cast shadows.
				else if(tokenLower == "forceshadows")
				{
					this.MaterialFlag = Renderer.MaterialFlags.ForceShadows;
				}
				// overlay / decal suppression
				else if(tokenLower == "nooverlays")
				{
					_allowOverlays = false;
				}
				// moster blood overlay forcing for alpha tested or translucent surfaces
				else if(tokenLower == "forceoverlays")
				{
					_parsingData.ForceOverlays = true;
				}
				else if(tokenLower == "translucent")
				{
					_coverage = MaterialCoverage.Translucent;
				}
				// global zero clamp
				else if(tokenLower == "zeroclamp")
				{
					textureRepeatDefault = TextureRepeat.ClampToZero;
				}
				// global clamp
				else if(tokenLower == "clamp")
				{
					textureRepeatDefault = TextureRepeat.Clamp;
				}
				// global clamp
				else if(tokenLower == "alphazeroclamp")
				{
					textureRepeatDefault = TextureRepeat.ClampToZero;
				}
				// forceOpaque is used for skies-behind-windows
				else if(tokenLower == "forceopaque")
				{
					_coverage = MaterialCoverage.Opaque;
				}
				else if(tokenLower == "twosided")
				{
					_cullType = CullType.TwoSided;

					// twoSided implies no-shadows, because the shadow
					// volume would be coplanar with the surface, giving depth fighting
					// we could make this no-self-shadows, but it may be more important
					// to receive shadows from no-self-shadow monsters
					this.MaterialFlag = Renderer.MaterialFlags.NoShadows;
				}
				else if(tokenLower == "backsided")
				{
					_cullType = CullType.Back;

					// the shadow code doesn't handle this, so just disable shadows.
					// We could fix this in the future if there was a need.
					this.MaterialFlag = Renderer.MaterialFlags.NoShadows;
				}
				else if(tokenLower == "foglight")
				{
					_fogLight = true;
				}
				else if(tokenLower == "blendlight")
				{
					_blendLight = true;
				}
				else if(tokenLower == "ambientlight")
				{
					_ambientLight = true;
				}
				else if(tokenLower == "mirror")
				{
					_sort = (float) MaterialSort.Subview;
					_coverage = MaterialCoverage.Opaque;
				}
				else if(tokenLower == "nofog")
				{
					_noFog = true;
				}
				else if(tokenLower == "unsmoothedtngents")
				{
					_unsmoothedTangents = true;
				}
				// lightFallofImage <imageprogram>
				// specifies the image to use for the third axis of projected
				// light volumes
				else if(tokenLower == "lightFallOffImage")
				{
					idConsole.Warning("TODO: idMaterial keyword lightFallOffImage");
					/* TODO: lightFallOffImage
					str = R_ParsePastImageProgram( src );
					idStr	copy;

					copy = str;	// so other things don't step on it
					lightFalloffImage = globalImages->ImageFromFile( copy, TF_DEFAULT, false, TR_CLAMP /* TR_CLAMP_TO_ZERO */
					/*, TD_DEFAULT );*/
				}
				// guisurf <guifile> | guisurf entity
				// an entity guisurf must have an idUserInterface
				// specified in the renderEntity
				else if(tokenLower == "guisurf")
				{
					idConsole.Warning("TODO: idMaterial keyword guiSurf");
					token = lexer.ReadTokenOnLine();

					// TODO: guiSurf
					/*if ( !token.Icmp( "entity" ) ) {
						entityGui = 1;
					} else if ( !token.Icmp( "entity2" ) ) {
						entityGui = 2;
					} else if ( !token.Icmp( "entity3" ) ) {
						entityGui = 3;
					} else {
						gui = uiManager->FindGui( token.c_str(), true );
					}*/
				}
				// sort
				else if(tokenLower == "sort")
				{
					ParseSort(lexer);
				}
				// spectrum <integer>
				else if(tokenLower == "spectrum")
				{
					int.TryParse(lexer.ReadTokenOnLine().ToString(), out _spectrum);
				}
				// deform < sprite | tube | flare >
				else if(tokenLower == "deform")
				{
					ParseDeform(lexer);
				}
				// decalInfo <staySeconds> <fadeSeconds> ( <start rgb> ) ( <end rgb> )
				else if(tokenLower == "decalinfo")
				{
					ParseDecalInfo(lexer);
				}
				// renderbump <args...>
				else if(tokenLower == "renderbump")
				{
					_renderBump = lexer.ParseRestOfLine();
				}
				// diffusemap for stage shortcut
				else if(tokenLower == "diffusemap")
				{
					// TODO: diffuseMap
					idConsole.Warning("TODO: idMaterial keyword diffuseMap");
					/*str = R_ParsePastImageProgram( src );
					idStr::snPrintf( buffer, sizeof( buffer ), "blend diffusemap\nmap %s\n}\n", str );
					newSrc.LoadMemory( buffer, strlen(buffer), "diffusemap" );
					newSrc.SetFlags( LEXFL_NOFATALERRORS | LEXFL_NOSTRINGCONCAT | LEXFL_NOSTRINGESCAPECHARS | LEXFL_ALLOWPATHNAMES );
					ParseStage( newSrc, trpDefault );
					newSrc.FreeSource();*/
				}
				// specularmap for stage shortcut
				else if(tokenLower == "specularmap")
				{
					// TODO: specularMap
					idConsole.Warning("TODO: idMaterial keyword specularMap");
					/*str = R_ParsePastImageProgram( src );
					idStr::snPrintf( buffer, sizeof( buffer ), "blend specularmap\nmap %s\n}\n", str );
					newSrc.LoadMemory( buffer, strlen(buffer), "specularmap" );
					newSrc.SetFlags( LEXFL_NOFATALERRORS | LEXFL_NOSTRINGCONCAT | LEXFL_NOSTRINGESCAPECHARS | LEXFL_ALLOWPATHNAMES );
					ParseStage( newSrc, trpDefault );
					newSrc.FreeSource();*/
				}
				// normalmap for stage shortcut
				else if(tokenLower == "bumpmap")
				{
					// TODO: bumpMap
					idConsole.Warning("TODO: idMaterial keyword bumpMap");
					/*str = R_ParsePastImageProgram( src );
					idStr::snPrintf( buffer, sizeof( buffer ), "blend bumpmap\nmap %s\n}\n", str );
					newSrc.LoadMemory( buffer, strlen(buffer), "bumpmap" );
					newSrc.SetFlags( LEXFL_NOFATALERRORS | LEXFL_NOSTRINGCONCAT | LEXFL_NOSTRINGESCAPECHARS | LEXFL_ALLOWPATHNAMES );
					ParseStage( newSrc, trpDefault );
					newSrc.FreeSource();*/
				}
				// DECAL_MACRO for backwards compatibility with the preprocessor macros
				else if(tokenLower == "decal_macro")
				{
					// polygonOffset
					this.MaterialFlag = Renderer.MaterialFlags.PolygonOffset;
					_polygonOffset = -1;

					// discrete
					_surfaceFlags |= SurfaceFlags.Discrete;
					_contentFlags &= ~ContentFlags.Solid;

					// sort decal
					_sort = (float) MaterialSort.Decal;

					// noShadows
					this.MaterialFlag = Renderer.MaterialFlags.NoShadows;
				}
				else if(tokenValue == "{")
				{
					// create the new stage
					ParseStage(lexer, textureRepeatDefault);
				}
				else
				{
					idConsole.WriteLine("unknown general material parameter '{0}' in '{1}'", tokenValue, this.Name);
					return;
				}
			}

			// add _flat or _white stages if needed
			AddImplicitStages();

			// order the diffuse / bump / specular stages properly
			SortInteractionStages();

			// if we need to do anything with normals (lighting or environment mapping)
			// and two sided lighting was asked for, flag
			// shouldCreateBackSides() and change culling back to single sided,
			// so we get proper tangent vectors on both sides

			// we can't just call ReceivesLighting(), because the stages are still
			// in temporary form
			if(_cullType == CullType.TwoSided)
			{
				for(int i = 0; i < _parsingData.ShaderStages.Count; i++)
				{
					if((_parsingData.ShaderStages[i].Lighting != StageLighting.Ambient) || (_parsingData.ShaderStages[i].Texture.TextureCoordinates != TextureCoordinateGeneration.Explicit))
					{
						if(_cullType == CullType.TwoSided)
						{
							_cullType = CullType.Front;
							_shouldCreateBackSides = true;
						}

						break;
					}
				}
			}

			// currently a surface can only have one unique texgen for all the stages on old hardware
			TextureCoordinateGeneration firstGen = TextureCoordinateGeneration.Explicit;

			for(int i = 0; i < _parsingData.ShaderStages.Count; i++)
			{
				if(_parsingData.ShaderStages[i].Texture.TextureCoordinates != TextureCoordinateGeneration.Explicit)
				{
					if(firstGen == TextureCoordinateGeneration.Explicit)
					{
						firstGen = _parsingData.ShaderStages[i].Texture.TextureCoordinates;
					}
					else if(firstGen != _parsingData.ShaderStages[i].Texture.TextureCoordinates)
					{
						idConsole.Warning("material '{0}' has multiple stages with a texgen", this.Name);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Adds implicit stages to the material.
		/// </summary>
		/// <remarks>
		/// If a material has diffuse or specular stages without any
		/// bump stage, add an implicit _flat bumpmap stage.
		/// <p/>
		/// It is valid to have either a diffuse or specular without the other.
		/// <p/>
		/// It is valid to have a reflection map and a bump map for bumpy reflection.
		/// </remarks>
		/// <param name="textureRepeatDefault"></param>
		private void AddImplicitStages(TextureRepeat textureRepeatDefault = TextureRepeat.Repeat)
		{
			bool hasDiffuse = false;
			bool hasSpecular = false;
			bool hasBump = false;
			bool hasReflection = false;

			for(int i = 0; i < _parsingData.ShaderStages.Count; i++)
			{
				switch(_parsingData.ShaderStages[i].Lighting)
				{
					case StageLighting.Bump:
						hasBump = true;
						break;

					case StageLighting.Diffuse:
						hasDiffuse = true;
					break;

					case StageLighting.Specular:
						hasSpecular = true;
						break;
				}

				if(_parsingData.ShaderStages[i].Texture.TextureCoordinates == TextureCoordinateGeneration.ReflectCube)
				{
					hasReflection = true;
				}
			}

			// if it doesn't have an interaction at all, don't add anything
			if((hasBump == false) && (hasDiffuse == false) && (hasSpecular == false))
			{
				return;
			}

			idLexer lexer = new idLexer(LexerOptions.NoFatalErrors | LexerOptions.NoStringConcatination | LexerOptions.NoStringEscapeCharacters | LexerOptions.AllowPathNames);
			
			if(hasBump == false)
			{
				string bump = "blend bumpmap\nmap _flat\n}\n";

				lexer.LoadMemory(bump, "bumpmap");
				ParseStage(lexer, textureRepeatDefault);
			}

			if((hasDiffuse == false) && (hasSpecular == false) && (hasReflection == false))
			{
				string bump = "blend bumpmap\nmap _white\n}\n";

				lexer.LoadMemory(bump, "diffusemap");
				ParseStage(lexer, textureRepeatDefault);
			}
		}

		/// <summary>
		/// Sorts the shader stages.
		/// </summary>
		/// <remarks>
		/// The renderer expects bump, then diffuse, then specular
		/// There can be multiple bump maps, followed by additional
		/// diffuse and specular stages, which allows cross-faded bump mapping.
		/// <para/>
		/// Ambient stages can be interspersed anywhere, but they are
		/// ignored during interactions, and all the interaction
		/// stages are ignored during ambient drawing.
		/// </remarks>
		private void SortInteractionStages()
		{
			int i = 0, j = 0;

			for(i = 0; i < _parsingData.ShaderStages.Count; i = j)
			{
				// find the next bump map
				for(j = i + 1; j < _parsingData.ShaderStages.Count; j++)
				{
					if(_parsingData.ShaderStages[j].Lighting == StageLighting.Bump)
					{
						// if the very first stage wasn't a bumpmap,
						// this bumpmap is part of the first group
						if(_parsingData.ShaderStages[i].Lighting != StageLighting.Bump)
						{
							continue;
						}

						break;
					}
				}
			}

			// bubble sort everything bump / diffuse / specular
			for(int l = 1; l < j - i; l++)
			{
				for(int k = i; k < k - l; k++)
				{
					if(_parsingData.ShaderStages[k].Lighting > _parsingData.ShaderStages[k+1].Lighting)
					{
						ShaderStage temp = _parsingData.ShaderStages[k];

						_parsingData.ShaderStages[k] = _parsingData.ShaderStages[k + 1];
						_parsingData.ShaderStages[k + 1] = temp;
					}
				}
			}
		}

		private void ParseSort(idLexer lexer)
		{
			idToken token = lexer.ReadTokenOnLine();

			if(token == null)
			{
				lexer.Warning("missing sort parameter");
				this.MaterialFlag = MaterialFlags.Defaulted;

				return;
			}

			try
			{
				_sort = (float) Enum.Parse(typeof(MaterialSort), token.ToString(), true);
			}
			catch
			{
				float.TryParse(token.ToString(), out _sort);
			}
		}

		private void ParseDeform(idLexer lexer)
		{
			idToken token = lexer.ExpectAnyToken();

			if(token == null)
			{
				return;
			}

			string tokenValue = token.ToString();
			string tokenLower = tokenValue.ToLower();

			if(tokenLower == "sprite")
			{
				_deformType = DeformType.Sprite;
				_cullType = CullType.TwoSided;

				this.MaterialFlag = MaterialFlags.NoShadows;
			}
			else if(tokenLower == "tube")
			{
				_deformType = DeformType.Tube;
				_cullType = CullType.TwoSided;

				this.MaterialFlag = MaterialFlags.NoShadows;
			}
			else if(tokenLower == "flare")
			{
				_deformType = DeformType.Flare;
				_cullType = CullType.TwoSided;
				_deformRegisters[0] = ParseExpression(lexer);

				this.MaterialFlag = MaterialFlags.NoShadows;
			}
			else if(tokenLower == "expand")
			{
				_deformType = DeformType.Expand;
				_deformRegisters[0] = ParseExpression(lexer);
			}
			else if(tokenLower == "move")
			{
				_deformType = DeformType.Move;
				_deformRegisters[0] = ParseExpression(lexer);
			}
			else if(tokenLower == "turbulent")
			{
				_deformType = DeformType.Turbulent;

				if((token = lexer.ExpectAnyToken()) == null)
				{
					lexer.Warning("deform particle missing particle name");
					this.MaterialFlag = MaterialFlags.Defaulted;
				}
				else
				{
					_deformDecl = idE.DeclManager.FindType(DeclType.Table, token.ToString(), true);

					_deformRegisters[0] = ParseExpression(lexer);
					_deformRegisters[1] = ParseExpression(lexer);
					_deformRegisters[2] = ParseExpression(lexer);
				}
			}
			else if(tokenLower == "eyeball")
			{
				_deformType = DeformType.Eyeball;
			}
			else if(tokenLower == "particle")
			{
				_deformType = DeformType.Particle;

				if((token = lexer.ExpectAnyToken()) == null)
				{
					lexer.Warning("deform particle missing particle name");
					this.MaterialFlag = MaterialFlags.Defaulted;
				}
				else
				{
					_deformDecl = idE.DeclManager.FindType(DeclType.Particle, token.ToString(), true);
				}
			}
			else if(tokenLower == "particle2")
			{
				_deformType = DeformType.Particle2;

				if((token = lexer.ExpectAnyToken()) == null)
				{
					lexer.Warning("deform particle missing particle name");
					this.MaterialFlag = MaterialFlags.Defaulted;
				}
				else
				{
					_deformDecl = idE.DeclManager.FindType(DeclType.Table, token.ToString(), true);

					_deformRegisters[0] = ParseExpression(lexer);
					_deformRegisters[1] = ParseExpression(lexer);
					_deformRegisters[2] = ParseExpression(lexer);
				}
			}
			else
			{
				lexer.Warning("Bad deform type '{0}'", tokenValue);
				this.MaterialFlag = MaterialFlags.Defaulted;
			}
		}

		private void ParseDecalInfo(idLexer lexer)
		{
			_decalInfo.StayTime = (int) lexer.ParseFloat() * 1000;
			_decalInfo.FadeTime = (int) lexer.ParseFloat() * 1000;
			_decalInfo.Start = lexer.Parse1DMatrix(4);
			_decalInfo.End = lexer.Parse1DMatrix(4);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lexer"></param>
		/// <returns>A register index.</returns>
		private int ParseExpression(idLexer lexer)
		{
			return ParseExpressionPriority(lexer, TopPriority);
		}

		private int ParseExpressionPriority(idLexer lexer, int priority)
		{
			idToken token;

			if(priority == 0)
			{
				return ParseTerm(lexer);
			}

			int a = ParseExpressionPriority(lexer, priority - 1);

			if(TestMaterialFlag(MaterialFlags.Defaulted) == true)
			{
				// we have a parse error.
				return 0;
			}

			if((token = lexer.ReadToken()) == null)
			{
				// we won't get EOF in a real file, but we can
				// when parsing from generated strings
				return a;
			}

			string tokenValue = token.ToString();

			if((priority == 1) && (tokenValue == "*"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Multiply, priority);
			}
			else if((priority == 1) && (tokenValue == "/"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Divide, priority);
			}
			else if((priority == 1) && (tokenValue == "%"))
			{
				// implied truncate both to integer
				return ParseEmitOp(lexer, a, ExpressionOperationType.Modulo, priority);
			}
			else if((priority == 2) && (tokenValue == "+"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Add, priority);
			}
			else if((priority == 2) && (tokenValue == "-"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Subtract, priority);
			}
			else if((priority == 3) && (tokenValue == ">"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.GreaterThan, priority);
			}
			else if((priority == 3) && (tokenValue == ">="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.GreaterThanOrEquals, priority);
			}
			else if((priority == 3) && (tokenValue == ">"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.GreaterThan, priority);
			}
			else if((priority == 3) && (tokenValue == ">="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.GreaterThanOrEquals, priority);
			}
			else if((priority == 3) && (tokenValue == "<"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.LessThan, priority);
			}
			else if((priority == 3) && (tokenValue == "<="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.LessThanOrEquals, priority);
			}
			else if((priority == 3) && (tokenValue == "=="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Equals, priority);
			}
			else if((priority == 3) && (tokenValue == "!="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.NotEquals, priority);
			}
			else if((priority == 4) && (tokenValue == "&&"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.And, priority);
			}
			else if((priority == 4) && (tokenValue == "||"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Or, priority);
			}

			// assume that anything else terminates the expression
			// not too robust error checking...

			lexer.UnreadToken = token;

			return a;
		}

		private int ParseEmitOp(idLexer lexer, int a, ExpressionOperationType opType, int priority)
		{
			int b = ParseExpressionPriority(lexer, priority);

			return EmitOp(a, b, opType);
		}

		private int EmitOp(int a, int b, ExpressionOperationType opType)
		{
			// optimize away identity operations
			if(opType == ExpressionOperationType.Add)
			{
				if((_parsingData.RegisterIsTemporary[a] == false) && (_parsingData.ShaderRegisters[a] == 0))
				{
					return b;
				}
				else if((_parsingData.RegisterIsTemporary[b] == false) && (_parsingData.ShaderRegisters[b] == 0))
				{
					return a;
				}
				else if((_parsingData.RegisterIsTemporary[a] == false) && (_parsingData.RegisterIsTemporary[b] == false))
				{
					return GetExpressionConstant(_parsingData.ShaderRegisters[a] + _parsingData.ShaderRegisters[b]);
				}
			}
			else if(opType == ExpressionOperationType.Multiply)
			{
				if((_parsingData.RegisterIsTemporary[a] == false) && (_parsingData.ShaderRegisters[a] == 1))
				{
					return b;
				}
				else if((_parsingData.RegisterIsTemporary[a] == false) && (_parsingData.ShaderRegisters[a] == 0))
				{
					return a;
				}
				else if((_parsingData.RegisterIsTemporary[b] == false) && (_parsingData.ShaderRegisters[b] == 1))
				{
					return a;
				}
				else if((_parsingData.RegisterIsTemporary[b] == false) && (_parsingData.ShaderRegisters[b] == 0))
				{
					return b;
				}
				else if((_parsingData.RegisterIsTemporary[a] == false) && (_parsingData.RegisterIsTemporary[b] == false))
				{
					return GetExpressionConstant(_parsingData.ShaderRegisters[a] * _parsingData.ShaderRegisters[b]);
				}
			}

			ExpressionOperation op = GetExpressionOperation();
			op.OperationType = opType;
			op.A = a;
			op.B = b;
			op.C = GetExpressionTemporary();

			return op.C;
		}

		/// <summary>
		/// See if the current token matches one of the surface parm bit flags.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		private bool CheckSurfaceParameter(idToken token)
		{
			// TODO: infoParms
			/*for ( int i = 0 ; i < numInfoParms ; i++ ) {
				if ( !token->Icmp( infoParms[i].name ) ) {
					if ( infoParms[i].surfaceFlags & SURF_TYPE_MASK ) {
						// ensure we only have one surface type set
						surfaceFlags &= ~SURF_TYPE_MASK;
					}
					surfaceFlags |= infoParms[i].surfaceFlags;
					contentFlags |= infoParms[i].contents;
					if ( infoParms[i].clearSolid ) {
						contentFlags &= ~CONTENTS_SOLID;
					}
					return true;
				}
			}*/

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lexer"></param>
		/// <returns>A register index.</returns>
		private int ParseTerm(idLexer lexer)
		{
			idToken token = lexer.ReadToken();

			string tokenValue = token.ToString();
			string tokenLower = tokenValue.ToLower();

			if(tokenValue == "(")
			{
				int a = ParseExpression(lexer);
				MatchToken(lexer, ")");

				return a;
			}

			try
			{
				ExpressionRegister reg = (ExpressionRegister) Enum.Parse(typeof(ExpressionRegister), tokenValue);
				_parsingData.RegistersAreConstant = false;

				return (int) reg;
			}
			catch
			{

			}

			if(tokenLower == "fragmentPrograms")
			{
				// TODO: return GetExpressionConstant((float) glConfig.ARBFragmentProgramAvailable);
			}
			else if(tokenLower == "sound")
			{
				_parsingData.RegistersAreConstant = false;

				return EmitOp(0, 0, ExpressionOperationType.Sound);
			}
			// parse negative numbers
			else if(tokenLower == "-")
			{
				token = lexer.ReadToken();

				if((token.Type == TokenType.Number) || (token.ToString() == "."))
				{
					return GetExpressionConstant(-token.ToFloat());
				}

				lexer.Warning("Bad negative number '{0}'", token.ToString());
				this.MaterialFlag = MaterialFlags.Defaulted;

				return 0;
			}
			else if((token.Type == TokenType.Number) || (tokenValue == ".") || (tokenValue == "-"))
			{
				return GetExpressionConstant(token.ToFloat());
			}

			// see if it is a table name
			idDeclTable table = (idDeclTable) idE.DeclManager.FindType(DeclType.Table, tokenValue, false);

			if(table == null)
			{
				lexer.Warning("Bad term '{0}'", tokenValue);
				this.MaterialFlag = MaterialFlags.Defaulted;

				return 0;
			}

			// parse a table expression
			MatchToken(lexer, "[");

			int b = ParseExpression(lexer);

			MatchToken(lexer, "]");

			return EmitOp(table.Index, b, ExpressionOperationType.Table);
		}

		private void ParseStage(idLexer lexer, TextureRepeat textureRepeatDefault)
		{
			TextureFilter textureFilter = TextureFilter.Default;
			TextureRepeat textureRepeat = textureRepeatDefault;
			TextureDepth textureDepth = TextureDepth.Default;
			CubeFiles cubeMap = CubeFiles.TwoD;

			bool allowPicmip = true;
			string imageName = string.Empty;

			NewShaderStage newStage = new NewShaderStage();
			newStage.VertexParameters = new int[4, 4];

			ShaderStage shaderStage = new ShaderStage();
			shaderStage.Color.Registers = new int[4];

			int[,] matrix = new int[2, 3];

			idToken token;
			int a, b;

			string tokenValue;
			string tokenLower;

			while(true)
			{
				if(TestMaterialFlag(MaterialFlags.Defaulted) == true)
				{
					// we have a parse error.
					return;
				}
				else if((token = lexer.ExpectAnyToken()) == null)
				{
					this.MaterialFlag = MaterialFlags.Defaulted;
					return;
				}

				tokenValue = token.ToString();
				tokenLower = tokenValue.ToLower();

				// the close brace for the entire material ends the draw block
				if(tokenLower == "}")
				{
					break;
				}
				// BSM Nerve: Added for stage naming in the material editor
				else if(tokenLower == "name")
				{
					lexer.SkipRestOfLine();
				}
				// image options
				else if(tokenLower == "blend")
				{
					ParseBlend(lexer, ref shaderStage);
				}
				else if(tokenLower == "map")
				{
					idConsole.WriteLine("TODO: material map keyword");
					/*str = R_ParsePastImageProgram( src );
					idStr::Copynz( imageName, str, sizeof( imageName ) );*/
				}
				else if(tokenLower == "remoterendermap")
				{
					shaderStage.Texture.Dynamic = DynamicImageType.RemoteRender;
					shaderStage.Texture.Width = lexer.ParseInt();
					shaderStage.Texture.Height = lexer.ParseInt();
				}
				else if(tokenLower == "mirrorrendermap")
				{
					shaderStage.Texture.Dynamic = DynamicImageType.MirrorRender;
					shaderStage.Texture.Width = lexer.ParseInt();
					shaderStage.Texture.Height = lexer.ParseInt();
					shaderStage.Texture.TextureCoordinates = TextureCoordinateGeneration.Screen;
				}
				else if(tokenLower == "xrayrendermap")
				{
					shaderStage.Texture.Dynamic = DynamicImageType.XRayRender;
					shaderStage.Texture.Width = lexer.ParseInt();
					shaderStage.Texture.Height = lexer.ParseInt();
					shaderStage.Texture.TextureCoordinates = TextureCoordinateGeneration.Screen;
				}
				else if(tokenLower == "screen")
				{
					shaderStage.Texture.TextureCoordinates = TextureCoordinateGeneration.Screen;
				}
				else if(tokenLower == "screen2")
				{
					shaderStage.Texture.TextureCoordinates = TextureCoordinateGeneration.Screen;
				}
				else if(tokenLower == "glasswarp")
				{
					shaderStage.Texture.TextureCoordinates = TextureCoordinateGeneration.GlassWarp;
				}
				else if(tokenLower == "videomap")
				{
					// note that videomaps will always be in clamp mode, so texture
					// coordinates had better be in the 0 to 1 range
					if((token = lexer.ReadToken()) == null)
					{
						idConsole.Warning("missing parameter for 'videoMap' keyword in material '{0}'", this.Name);
					}
					else
					{
						bool loop = false;

						if(token.ToString().Equals("loop", StringComparison.OrdinalIgnoreCase) == true)
						{
							loop = true;

							if((token = lexer.ReadToken()) == null)
							{
								idConsole.Warning("missing parameter for 'videoMap' keyword in material '{0}'", this.Name);
								continue;
							}
						}

						idConsole.Warning("TODO: material videoMap keyword");

						// TODO
						/*ts->cinematic = idCinematic::Alloc();
						ts->cinematic->InitFromFile( token.c_str(), loop );*/
					}
				}
				else if(tokenLower == "soundmap")
				{
					if((token = lexer.ReadToken()) == null)
					{
						idConsole.Warning("missing parameter for 'soundMap' keyword in material '{0}'", this.Name);
					}
					else
					{
						idConsole.Warning("TODO: material soundMap keyword");

						// TODO
						/*ts->cinematic = new idSndWindow();
						ts->cinematic->InitFromFile( token.c_str(), true );*/
					}
				}
				else if(tokenLower == "cubemap")
				{
					idConsole.Warning("TODO: material cubeMap keyword");

					// TODO
					/*str = R_ParsePastImageProgram( src );
					idStr::Copynz( imageName, str, sizeof( imageName ) );
					cubeMap = CF_NATIVE;*/
				}
				else if(tokenLower == "cameracubemap")
				{
					idConsole.Warning("TODO: material cameraCubeMap keyword");

					/*str = R_ParsePastImageProgram( src );
					idStr::Copynz( imageName, str, sizeof( imageName ) );
					cubeMap = CF_CAMERA;*/
				}
				else if(tokenLower == "ignorealphatest")
				{
					shaderStage.IgnoreAlphaTest = true;
				}
				else if(tokenLower == "nearest")
				{
					textureFilter = TextureFilter.Nearest;
				}
				else if(tokenLower == "linear")
				{
					textureFilter = TextureFilter.Linear;
				}
				else if(tokenLower == "clamp")
				{
					textureRepeat = TextureRepeat.Clamp;
				}
				else if(tokenLower == "noclamp")
				{
					textureRepeat = TextureRepeat.Repeat;
				}
				else if(tokenLower == "zeroclamp")
				{
					textureRepeat = TextureRepeat.ClampToZero;
				}
				else if(tokenLower == "alphazeroclamp")
				{
					textureRepeat = TextureRepeat.ClampToZeroAlpha;
				}
				else if((tokenLower == "uncompressed")
					|| (tokenLower == "highquality"))
				{
					if(idE.CvarSystem.GetInt("image_ignoreHighQuality") == 0)
					{
						textureDepth = TextureDepth.HighQuality;
					}
				}
				else if(tokenLower == "forcehighquality")
				{
					textureDepth = TextureDepth.HighQuality;
				}
				else if(tokenLower == "nopicmip")
				{
					allowPicmip = false;
				}
				else if(tokenLower == "vertexcolor")
				{
					shaderStage.VertexColor = StageVertexColor.Modulate;
				}
				else if(tokenLower == "inversevertexcolor")
				{
					shaderStage.VertexColor = StageVertexColor.InverseModulate;
				}
				// privatePolygonOffset.
				else if(tokenLower == "privatepolygonoffset")
				{
					if((token = lexer.ReadTokenOnLine()) == null)
					{
						shaderStage.PrivatePolygonOffset = 1;
					}
					else
					{
						// explict larger (or negative) offset.
						lexer.UnreadToken = token;
						shaderStage.PrivatePolygonOffset = lexer.ParseFloat();
					}
				}
				// texture coordinate generation
				else if(tokenLower == "texgen")
				{
					token = lexer.ExpectAnyToken();
					tokenValue = token.ToString();
					tokenLower = tokenValue.ToLower();

					if(tokenLower == "normal")
					{
						shaderStage.Texture.TextureCoordinates = TextureCoordinateGeneration.DiffuseCube;
					}
					else if(tokenLower == "reflect")
					{
						shaderStage.Texture.TextureCoordinates = TextureCoordinateGeneration.ReflectCube;
					}
					else if(tokenLower == "skybox")
					{
						shaderStage.Texture.TextureCoordinates = TextureCoordinateGeneration.SkyboxCube;
					}
					else if(tokenLower == "wobblesky")
					{
						shaderStage.Texture.TextureCoordinates = TextureCoordinateGeneration.WobbleSkyCube;

						_texGenRegisters = new int[4];
						_texGenRegisters[0] = ParseExpression(lexer);
						_texGenRegisters[1] = ParseExpression(lexer);
						_texGenRegisters[2] = ParseExpression(lexer);
					}
					else
					{
						idConsole.Warning("bad texGen '{0}' in material {1}", tokenValue, this.Name);
						this.MaterialFlag = MaterialFlags.Defaulted;
					}
				}
				else if((tokenLower == "scroll")
					|| (tokenLower == "translate"))
				{
					a = ParseExpression(lexer);
					MatchToken(lexer, ",");
					b = ParseExpression(lexer);

					matrix[0, 0] = GetExpressionConstant(1);
					matrix[0, 1] = GetExpressionConstant(0);
					matrix[0, 2] = a;

					matrix[1, 0] = GetExpressionConstant(0);
					matrix[1, 1] = GetExpressionConstant(1);
					matrix[1, 2] = b;

					MultiplyTextureMatrix(ref shaderStage.Texture, matrix);
				}
				else if(tokenLower == "scale")
				{
					a = ParseExpression(lexer);
					MatchToken(lexer, ",");
					b = ParseExpression(lexer);

					// this just scales without a centering.
					matrix[0, 0] = a;
					matrix[0, 1] = GetExpressionConstant(0);
					matrix[0, 2] = GetExpressionConstant(0);

					matrix[1, 0] = GetExpressionConstant(0);
					matrix[1, 1] = b;
					matrix[1, 2] = GetExpressionConstant(0);

					MultiplyTextureMatrix(ref shaderStage.Texture, matrix);
				}
				else if(tokenLower == "centerscale")
				{
					a = ParseExpression(lexer);
					MatchToken(lexer, ",");
					b = ParseExpression(lexer);

					// this subtracts 0.5, then scales, then adds 0.5.
					matrix[0, 0] = a;
					matrix[0, 1] = GetExpressionConstant(0);
					matrix[0, 2] = EmitOp(GetExpressionConstant(0.5f), EmitOp(GetExpressionConstant(0.5f), a, ExpressionOperationType.Multiply), ExpressionOperationType.Subtract);

					matrix[1, 0] = GetExpressionConstant(0);
					matrix[1, 1] = b;
					matrix[1, 2] = EmitOp(GetExpressionConstant(0.5f), EmitOp(GetExpressionConstant(0.5f), b, ExpressionOperationType.Multiply), ExpressionOperationType.Subtract);

					MultiplyTextureMatrix(ref shaderStage.Texture, matrix);
				}
				else if(tokenLower == "shear")
				{
					a = ParseExpression(lexer);
					MatchToken(lexer, ",");
					b = ParseExpression(lexer);

					// this subtracts 0.5, then shears, then adds 0.5.
					matrix[0, 0] = GetExpressionConstant(1);
					matrix[0, 1] = a;
					matrix[0, 2] = EmitOp(GetExpressionConstant(-0.5f), a, ExpressionOperationType.Multiply);

					matrix[1, 0] = b;
					matrix[1, 1] = GetExpressionConstant(1);
					matrix[1, 2] = EmitOp(GetExpressionConstant(-0.5f), b, ExpressionOperationType.Multiply);

					MultiplyTextureMatrix(ref shaderStage.Texture, matrix);
				}
				else if(tokenLower == "rotate")
				{
					int sinReg, cosReg;

					// in cycles
					a = ParseExpression(lexer);

					idDeclTable table = (idDeclTable) idE.DeclManager.FindType(DeclType.Table, "sinTable", false);

					if(table == null)
					{
						idConsole.Warning("no sinTable for rotate defined");
						this.MaterialFlag = MaterialFlags.Defaulted;

						return;
					}

					sinReg = EmitOp(table.Index, a, ExpressionOperationType.Table);

					table = (idDeclTable) idE.DeclManager.FindType(DeclType.Table, "cosTable", false);

					if(table == null)
					{
						idConsole.Warning("no cosTable for rotate defined");
						this.MaterialFlag = MaterialFlags.Defaulted;

						return;
					}

					cosReg = EmitOp(table.Index, a, ExpressionOperationType.Table);

					// this subtracts 0.5, then rotates, then adds 0.5.
					matrix[0, 0] = cosReg;
					matrix[0, 1] = EmitOp(GetExpressionConstant(0), sinReg, ExpressionOperationType.Subtract);
					matrix[0, 2] = EmitOp(EmitOp(EmitOp(GetExpressionConstant(-0.5f), cosReg, ExpressionOperationType.Multiply),
									EmitOp(GetExpressionConstant(0.5f), sinReg, ExpressionOperationType.Multiply), ExpressionOperationType.Add),
										GetExpressionConstant(0.5f), ExpressionOperationType.Add);

					matrix[1, 0] = sinReg;
					matrix[1, 1] = cosReg;
					matrix[1, 2] = EmitOp(EmitOp(EmitOp(GetExpressionConstant(-0.5f), sinReg, ExpressionOperationType.Multiply),
									EmitOp(GetExpressionConstant(-0.5f), cosReg, ExpressionOperationType.Multiply), ExpressionOperationType.Add),
										GetExpressionConstant(0.5f), ExpressionOperationType.Add);

					MultiplyTextureMatrix(ref shaderStage.Texture, matrix);
				}
				// color mask options
				// TODO: not sure what i'm doing with renderer yet
				/*if ( !token.Icmp( "maskRed" ) ) {
					ss->drawStateBits |= GLS_REDMASK;
					continue;
				}		
				if ( !token.Icmp( "maskGreen" ) ) {
					ss->drawStateBits |= GLS_GREENMASK;
					continue;
				}		
				if ( !token.Icmp( "maskBlue" ) ) {
					ss->drawStateBits |= GLS_BLUEMASK;
					continue;
				}		
				if ( !token.Icmp( "maskAlpha" ) ) {
					ss->drawStateBits |= GLS_ALPHAMASK;
					continue;
				}		
				if ( !token.Icmp( "maskColor" ) ) {
					ss->drawStateBits |= GLS_COLORMASK;
					continue;
				}		
				if ( !token.Icmp( "maskDepth" ) ) {
					ss->drawStateBits |= GLS_DEPTHMASK;
					continue;
				}		*/
				else if(tokenLower == "alphatest")
				{
					shaderStage.HasAlphaTest = true;
					shaderStage.AlphaTestRegister = ParseExpression(lexer);

					_coverage = MaterialCoverage.Perforated;
				}
				// shorthand for 2D modulated
				else if(tokenLower == "colored")
				{
					shaderStage.Color.Registers[0] = (int) ExpressionRegister.Parm0;
					shaderStage.Color.Registers[1] = (int) ExpressionRegister.Parm1;
					shaderStage.Color.Registers[2] = (int) ExpressionRegister.Parm2;
					shaderStage.Color.Registers[3] = (int) ExpressionRegister.Parm3;

					_parsingData.RegistersAreConstant = false;
				}
				else if(tokenLower == "color")
				{
					shaderStage.Color.Registers[0] = ParseExpression(lexer);
					MatchToken(lexer, ",");

					shaderStage.Color.Registers[1] = ParseExpression(lexer);
					MatchToken(lexer, ",");

					shaderStage.Color.Registers[2] = ParseExpression(lexer);
					MatchToken(lexer, ",");

					shaderStage.Color.Registers[3] = ParseExpression(lexer);
				}
				else if(tokenLower == "red")
				{
					shaderStage.Color.Registers[0] = ParseExpression(lexer);
				}
				else if(tokenLower == "green")
				{
					shaderStage.Color.Registers[1] = ParseExpression(lexer);
				}
				else if(tokenLower == "blue")
				{
					shaderStage.Color.Registers[2] = ParseExpression(lexer);
				}
				else if(tokenLower == "alpha")
				{
					shaderStage.Color.Registers[3] = ParseExpression(lexer);
				}
				else if(tokenLower == "rgb")
				{
					shaderStage.Color.Registers[0] = shaderStage.Color.Registers[1] = shaderStage.Color.Registers[2] = ParseExpression(lexer);
				}
				else if(tokenLower == "rgba")
				{
					shaderStage.Color.Registers[0] = shaderStage.Color.Registers[1] = shaderStage.Color.Registers[2] = shaderStage.Color.Registers[3] = ParseExpression(lexer);
				}
				else if(tokenLower == "if")
				{
					shaderStage.ConditionRegister = ParseExpression(lexer);
				}
				else if(tokenLower == "program")
				{
					if((token = lexer.ReadTokenOnLine()) != null)
					{
						idConsole.Warning("TODO: material program keyword");
						// TODO
						/*newStage.vertexProgram = R_FindARBProgram( GL_VERTEX_PROGRAM_ARB, token.c_str() );
						newStage.fragmentProgram = R_FindARBProgram( GL_FRAGMENT_PROGRAM_ARB, token.c_str() );*/
					}
				}
				else if(tokenLower == "fragmentprogram")
				{
					if((token = lexer.ReadTokenOnLine()) != null)
					{
						idConsole.Warning("TODO: material fragmentProgram keyword");
						// TODO
						//newStage.fragmentProgram = R_FindARBProgram( GL_FRAGMENT_PROGRAM_ARB, token.c_str() );
					}
				}
				else if(tokenLower == "vertexprogram")
				{
					if((token = lexer.ReadTokenOnLine()) != null)
					{
						idConsole.Warning("TODO: material vertexProgram keyword");
						// TODO
						//newStage.vertexProgram = R_FindARBProgram( GL_VERTEX_PROGRAM_ARB, token.c_str() );
					}
				}
				else if(tokenLower == "megatexture")
				{
					if((token = lexer.ReadTokenOnLine()) != null)
					{
						idConsole.Warning("TODO: material megaTexture keyword");
						// TODO
						/*newStage.megaTexture = new idMegaTexture;
						if ( !newStage.megaTexture->InitFromMegaFile( token.c_str() ) ) {
							delete newStage.megaTexture;
							SetMaterialFlag( MF_DEFAULTED );
							continue;
						}
						newStage.vertexProgram = R_FindARBProgram( GL_VERTEX_PROGRAM_ARB, "megaTexture.vfp" );
						newStage.fragmentProgram = R_FindARBProgram( GL_FRAGMENT_PROGRAM_ARB, "megaTexture.vfp" );*/
					}
				}
				else if(tokenLower == "vertexparm")
				{
					ParseVertexParameter(lexer, ref newStage);
				}
				else if(tokenLower == "fragmentmap")
				{
					ParseFragmentMap(lexer, ref newStage);
				}
				else
				{
					idConsole.Warning("unknown token '{0}' in material '{1}'", tokenValue, this.Name);

					this.MaterialFlag = MaterialFlags.Defaulted;

					return;
				}
			}

			// if we are using newStage, allocate a copy of it
			// TODO
			/*if ( newStage.fragmentProgram || newStage.vertexProgram ) {
				ss->newStage = (newShaderStage_t *)Mem_Alloc( sizeof( newStage ) );
				*(ss->newStage) = newStage;
			}*/

			// successfully parsed a stage
			_parsingData.ShaderStages.Add(shaderStage);

			// select a compressed depth based on what the stage is
			if(textureDepth == TextureDepth.Default)
			{
				switch(shaderStage.Lighting)
				{
					case StageLighting.Bump:
						textureDepth = TextureDepth.Bump;
						break;

					case StageLighting.Diffuse:
						textureDepth = TextureDepth.Diffuse;
						break;

					case StageLighting.Specular:
						textureDepth = TextureDepth.Specular;
						break;
				}
			}

			// now load the image with all the parms we parsed
			// TODO: image loading
			/*if ( imageName[0] ) {
				ts->image = globalImages->ImageFromFile( imageName, tf, allowPicmip, trp, td, cubeMap );
				if ( !ts->image ) {
					ts->image = globalImages->defaultImage;
				}
			} else if ( !ts->cinematic && !ts->dynamic && !ss->newStage ) {
				common->Warning( "material '%s' had stage with no image", GetName() );
				ts->image = globalImages->defaultImage;
			}*/
		}

		private void ParseBlend(idLexer lexer, ref ShaderStage stage)
		{
			idToken token;

			if((token = lexer.ReadToken()) == null)
			{
				return;
			}

			string tokenValue = token.ToString();
			string tokenLower = tokenValue.ToLower();

			// blending combinations
			// TODO: don't know what i'm doing with renderer yet
			/*if ( !token.Icmp( "blend" ) ) {
				stage->drawStateBits = GLS_SRCBLEND_SRC_ALPHA | GLS_DSTBLEND_ONE_MINUS_SRC_ALPHA;
				return;
			}
			if ( !token.Icmp( "add" ) ) {
				stage->drawStateBits = GLS_SRCBLEND_ONE | GLS_DSTBLEND_ONE;
				return;
			}
			if ( !token.Icmp( "filter" ) || !token.Icmp( "modulate" ) ) {
				stage->drawStateBits = GLS_SRCBLEND_DST_COLOR | GLS_DSTBLEND_ZERO;
				return;
			}
			if (  !token.Icmp( "none" ) ) {
				// none is used when defining an alpha mask that doesn't draw
				stage->drawStateBits = GLS_SRCBLEND_ZERO | GLS_DSTBLEND_ONE;
				return;
			}*/
			if(tokenLower == "bumpmap")
			{
				stage.Lighting = StageLighting.Bump;
			}
			else if(tokenLower == "diffusemap")
			{
				stage.Lighting = StageLighting.Diffuse;
			}
			else if(tokenLower == "specularmap")
			{
				stage.Lighting = StageLighting.Specular;
			}
			else
			{
				// TODO
				//srcBlend = NameToSrcBlendMode(token);

				MatchToken(lexer, ",");

				if((token = lexer.ReadToken()) == null)
				{
					return;
				}

				/*dstBlend = NameToDstBlendMode(token);

				stage->drawStateBits = srcBlend | dstBlend;*/
			}
		}

		/// <summary>
		/// Parses a vertex parameter.
		/// </summary>
		/// <remarks>
		/// If there is a single value, it will be repeated across all elements.
		/// If there are two values, 3 = 0.0, 4 = 1.0.
		/// if there are three values, 4 = 1.0.
		/// </remarks>
		/// <param name="lexer"></param>
		/// <param name="newStage"></param>
		private void ParseVertexParameter(idLexer lexer, ref NewShaderStage newStage)
		{
			idToken token = lexer.ReadTokenOnLine();
			int parm = token.ToInt32();
			int tmp = 0;

			string tokenValue = token.ToString();

			if((int.TryParse(tokenValue, out tmp) == false) || (parm < 0))
			{
				idConsole.Warning("bad vertexParm number");
				this.MaterialFlag = MaterialFlags.Defaulted;

				return;
			}

			newStage.VertexParameters[parm, 0] = ParseExpression(lexer);
			token = lexer.ReadTokenOnLine();
			tokenValue = token.ToString();

			if((tokenValue == string.Empty) || (tokenValue != ","))
			{
				newStage.VertexParameters[parm, 1] = newStage.VertexParameters[parm, 2] = newStage.VertexParameters[parm, 3] = newStage.VertexParameters[parm, 0];
			}
			else
			{
				newStage.VertexParameters[parm, 1] = ParseExpression(lexer);
				token = lexer.ReadTokenOnLine();
				tokenValue = token.ToString();

				if((tokenValue == string.Empty) || (tokenValue != ","))
				{
					newStage.VertexParameters[parm, 2] = GetExpressionConstant(0);
					newStage.VertexParameters[parm, 3] = GetExpressionConstant(1);
				}
				else
				{
					newStage.VertexParameters[parm, 2] = ParseExpression(lexer);
					token = lexer.ReadTokenOnLine();
					tokenValue = token.ToString();

					if((tokenValue == string.Empty) || (tokenValue != ","))
					{
						newStage.VertexParameters[parm, 3] = GetExpressionConstant(1);
					}
					else
					{
						newStage.VertexParameters[parm, 3] = ParseExpression(lexer);
					}
				}
			}
		}

		private void ParseFragmentMap(idLexer lexer, ref NewShaderStage newStage)
		{
			TextureFilter textureFilter = TextureFilter.Default;
			TextureRepeat textureRepeat = TextureRepeat.Repeat;
			TextureDepth textureDepth = TextureDepth.Default;
			CubeFiles cubeMap = CubeFiles.TwoD;

			bool allowPicmip = true;

			idToken token = lexer.ReadTokenOnLine();
			int unit = token.ToInt32();
			int tmp;

			if((int.TryParse(token.ToString(), out tmp) == false) || (unit < 0))
			{
				idConsole.Warning("bad fragmentMap number");
				this.MaterialFlag = MaterialFlags.Defaulted;

				return;
			}

			// unit 1 is the normal map.. make sure it gets flagged as the proper depth
			if(unit == 1)
			{
				textureDepth = TextureDepth.Bump;
			}

			string tokenValue;
			string tokenLower;

			while(true)
			{
				token = lexer.ReadTokenOnLine();
				tokenValue = token.ToString();
				tokenLower = tokenValue.ToLower();

				if(tokenLower == "cubemap")
				{
					cubeMap = CubeFiles.Native;
				}
				else if(tokenLower == "cameracubemap")
				{
					cubeMap = CubeFiles.Camera;
				}
				else if(tokenLower == "nearest")
				{
					textureFilter = TextureFilter.Nearest;
				}
				else if(tokenLower == "linear")
				{
					textureFilter = TextureFilter.Linear;
				}
				else if(tokenLower == "clamp")
				{
					textureRepeat = TextureRepeat.Clamp;
				}
				else if(tokenLower == "noclamp")
				{
					textureRepeat = TextureRepeat.Repeat;
				}
				else if(tokenLower == "zeroclamp")
				{
					textureRepeat = TextureRepeat.ClampToZero;
				}
				else if(tokenLower == "alphazeroclamp")
				{
					textureRepeat = TextureRepeat.ClampToZeroAlpha;
				}
				else if(tokenLower == "forcehighquality")
				{
					textureDepth = TextureDepth.HighQuality;
				}
				else if((tokenLower == "uncompressed")
					|| (tokenLower == "highquality"))
				{
					if(idE.CvarSystem.GetInt("image_ignoreHighQuality") == 0)
					{
						textureDepth = TextureDepth.HighQuality;
					}
				}
				else if(tokenLower == "nopicmip")
				{
					allowPicmip = false;
				}
				else
				{
					// assume anything else is the image name
					lexer.UnreadToken = token;
					break;
				}
			}

			// TODO
			/*str = R_ParsePastImageProgram( src );

			newStage->fragmentProgramImages[unit] = 
				globalImages->ImageFromFile( str, tf, allowPicmip, trp, td, cubeMap );
			if ( !newStage->fragmentProgramImages[unit] ) {
				newStage->fragmentProgramImages[unit] = globalImages->defaultImage;
			}*/
		}

		/// <summary>
		/// Sets defaultShader and returns false if the next token doesn't match.
		/// </summary>
		/// <param name="lexer"></param>
		/// <param name="match"></param>
		/// <returns></returns>
		private bool MatchToken(idLexer lexer, string match)
		{
			if(lexer.ExpectTokenString(match) == false)
			{
				this.MaterialFlag = MaterialFlags.Defaulted;

				return false;
			}

			return true;
		}

		private int GetExpressionConstant(float f)
		{
			int i = 0;

			for(i = 0; i < _parsingData.ShaderRegisters.Count; i++)
			{
				if((_parsingData.RegisterIsTemporary[i] == false) && (_parsingData.ShaderRegisters[i] == f))
				{
					return i;
				}
			}

			_parsingData.RegisterIsTemporary[i] = false;
			_parsingData.ShaderRegisters[i] = f;

			return i;
		}

		private ExpressionOperation GetExpressionOperation()
		{
			ExpressionOperation op = new ExpressionOperation();
			_parsingData.ShaderOperations.Add(op);

			return op;
		}

		private int GetExpressionTemporary()
		{
			_parsingData.RegisterIsTemporary.Add(true);

			return (_parsingData.RegisterIsTemporary.Count - 1);
		}

		private void MultiplyTextureMatrix(ref TextureStage textureStage, int[,] registers)
		{
			if(textureStage.Matrix == null)
			{
				textureStage.Matrix = registers;
				return;
			}

			int[,] old = textureStage.Matrix;

			// multiply the two matricies
			textureStage.Matrix[0, 0] = EmitOp(
											EmitOp(old[0, 0], registers[0, 0], ExpressionOperationType.Multiply),
											EmitOp(old[0, 1], registers[1, 0], ExpressionOperationType.Multiply), ExpressionOperationType.Add);

			textureStage.Matrix[0, 1] = EmitOp(
											EmitOp(old[0, 0], registers[0, 1], ExpressionOperationType.Multiply),
											EmitOp(old[0, 1], registers[1, 1], ExpressionOperationType.Multiply), ExpressionOperationType.Add);

			textureStage.Matrix[0, 2] = EmitOp(
											EmitOp(
												EmitOp(old[0, 0], registers[0, 2], ExpressionOperationType.Multiply),
												EmitOp(old[0, 1], registers[1, 2], ExpressionOperationType.Multiply), ExpressionOperationType.Add),
													old[0, 2], ExpressionOperationType.Add);

			textureStage.Matrix[1, 0] = EmitOp(
											EmitOp(old[1, 0], registers[0, 0], ExpressionOperationType.Multiply),
											EmitOp(old[1, 1], registers[1, 0], ExpressionOperationType.Multiply), ExpressionOperationType.Add);

			textureStage.Matrix[1, 1] = EmitOp(
											EmitOp(old[1, 0], registers[0, 1], ExpressionOperationType.Multiply),
											EmitOp(old[1, 1], registers[1, 1], ExpressionOperationType.Multiply), ExpressionOperationType.Add);

			textureStage.Matrix[1, 2] = EmitOp(
											EmitOp(
												EmitOp(old[1, 0], registers[0, 1], ExpressionOperationType.Multiply),
												EmitOp(old[1, 1], registers[1, 1], ExpressionOperationType.Multiply), ExpressionOperationType.Add),
													old[1, 2], ExpressionOperationType.Add);
		}
		#endregion
		#endregion

		#region idDecl implementation
		public override string GetDefaultDefinition()
		{
			return "{\n\t{\t\tblend\tblend\n\t\tmap\t\t_default\n\t}\n}";
		}

		/// <summary>
		/// Parses the current material definition and finds all necessary images.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool Parse(string text)
		{
			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.LoadMemory(text, this.FileName, this.LineNumber);
			lexer.SkipUntilString("{");

			// reset to the unparsed state
			Init();

			_parsingData = new MaterialParsingData(); // this is only valid during parsing.

			// parse it
			ParseMaterial(lexer);

			// TODO
			// if we are doing an fs_copyfiles, also reference the editorImage
			/*if ( cvarSystem->GetCVarInteger( "fs_copyFiles" ) ) {
				GetEditorImage();
			}

			//
			// count non-lit stages
			numAmbientStages = 0;
			int i;
			for ( i = 0 ; i < numStages ; i++ ) {
				if ( pd->parseStages[i].lighting == SL_AMBIENT ) {
					numAmbientStages++;
				}
			}

			// see if there is a subview stage
			if ( sort == SS_SUBVIEW ) {
				hasSubview = true;
			} else {
				hasSubview = false;
				for ( i = 0 ; i < numStages ; i++ ) {
					if ( pd->parseStages[i].texture.dynamic ) {
						hasSubview = true;
					}
				}
			}

			// automatically determine coverage if not explicitly set
			if ( coverage == MC_BAD ) {
				// automatically set MC_TRANSLUCENT if we don't have any interaction stages and 
				// the first stage is blended and not an alpha test mask or a subview
				if ( !numStages ) {
					// non-visible
					coverage = MC_TRANSLUCENT;
				} else if ( numStages != numAmbientStages ) {
					// we have an interaction draw
					coverage = MC_OPAQUE;
				} else if ( 
					( pd->parseStages[0].drawStateBits & GLS_DSTBLEND_BITS ) != GLS_DSTBLEND_ZERO ||
					( pd->parseStages[0].drawStateBits & GLS_SRCBLEND_BITS ) == GLS_SRCBLEND_DST_COLOR ||
					( pd->parseStages[0].drawStateBits & GLS_SRCBLEND_BITS ) == GLS_SRCBLEND_ONE_MINUS_DST_COLOR ||
					( pd->parseStages[0].drawStateBits & GLS_SRCBLEND_BITS ) == GLS_SRCBLEND_DST_ALPHA ||
					( pd->parseStages[0].drawStateBits & GLS_SRCBLEND_BITS ) == GLS_SRCBLEND_ONE_MINUS_DST_ALPHA
					) {
					// blended with the destination
						coverage = MC_TRANSLUCENT;
				} else {
					coverage = MC_OPAQUE;
				}
			}

			// translucent automatically implies noshadows
			if ( coverage == MC_TRANSLUCENT ) {
				SetMaterialFlag( MF_NOSHADOWS );
			} else {
				// mark the contents as opaque
				contentFlags |= CONTENTS_OPAQUE;
			}

			// if we are translucent, draw with an alpha in the editor
			if ( coverage == MC_TRANSLUCENT ) {
				editorAlpha = 0.5;
			} else {
				editorAlpha = 1.0;
			}

			// the sorts can make reasonable defaults
			if ( sort == SS_BAD ) {
				if ( TestMaterialFlag(MF_POLYGONOFFSET) ) {
					sort = SS_DECAL;
				} else if ( coverage == MC_TRANSLUCENT ) {
					sort = SS_MEDIUM;
				} else {
					sort = SS_OPAQUE;
				}
			}

			// anything that references _currentRender will automatically get sort = SS_POST_PROCESS
			// and coverage = MC_TRANSLUCENT

			for ( i = 0 ; i < numStages ; i++ ) {
				shaderStage_t	*pStage = &pd->parseStages[i];
				if ( pStage->texture.image == globalImages->currentRenderImage ) {
					if ( sort != SS_PORTAL_SKY ) {
						sort = SS_POST_PROCESS;
						coverage = MC_TRANSLUCENT;
					}
					break;
				}
				if ( pStage->newStage ) {
					for ( int j = 0 ; j < pStage->newStage->numFragmentProgramImages ; j++ ) {
						if ( pStage->newStage->fragmentProgramImages[j] == globalImages->currentRenderImage ) {
							if ( sort != SS_PORTAL_SKY ) {
								sort = SS_POST_PROCESS;
								coverage = MC_TRANSLUCENT;
							}
							i = numStages;
							break;
						}
					}
				}
			}

			// set the drawStateBits depth flags
			for ( i = 0 ; i < numStages ; i++ ) {
				shaderStage_t	*pStage = &pd->parseStages[i];
				if ( sort == SS_POST_PROCESS ) {
					// post-process effects fill the depth buffer as they draw, so only the
					// topmost post-process effect is rendered
					pStage->drawStateBits |= GLS_DEPTHFUNC_LESS;
				} else if ( coverage == MC_TRANSLUCENT || pStage->ignoreAlphaTest ) {
					// translucent surfaces can extend past the exactly marked depth buffer
					pStage->drawStateBits |= GLS_DEPTHFUNC_LESS | GLS_DEPTHMASK;
				} else {
					// opaque and perforated surfaces must exactly match the depth buffer,
					// which gets alpha test correct
					pStage->drawStateBits |= GLS_DEPTHFUNC_EQUAL | GLS_DEPTHMASK;
				}
			}

			// determine if this surface will accept overlays / decals

			if ( pd->forceOverlays ) {
				// explicitly flaged in material definition
				allowOverlays = true;
			} else {
				if ( !IsDrawn() ) {
					allowOverlays = false;
				}
				if ( Coverage() != MC_OPAQUE ) {
					allowOverlays = false;
				}
				if ( GetSurfaceFlags() & SURF_NOIMPACT ) {
					allowOverlays = false;
				}
			}

			// add a tiny offset to the sort orders, so that different materials
			// that have the same sort value will at least sort consistantly, instead
			// of flickering back and forth
		/* this messed up in-game guis
			if ( sort != SS_SUBVIEW ) {
				int	hash, l;

				l = name.Length();
				hash = 0;
				for ( int i = 0 ; i < l ; i++ ) {
					hash ^= name[i];
				}
				sort += hash * 0.01;
			}
		*/

			/*if (numStages) {
				stages = (shaderStage_t *)R_StaticAlloc( numStages * sizeof( stages[0] ) );
				memcpy( stages, pd->parseStages, numStages * sizeof( stages[0] ) );
			}

			if ( numOps ) {
				ops = (expOp_t *)R_StaticAlloc( numOps * sizeof( ops[0] ) );
				memcpy( ops, pd->shaderOps, numOps * sizeof( ops[0] ) );
			}

			if ( numRegisters ) {
				expressionRegisters = (float *)R_StaticAlloc( numRegisters * sizeof( expressionRegisters[0] ) );
				memcpy( expressionRegisters, pd->shaderRegisters, numRegisters * sizeof( expressionRegisters[0] ) );
			}

			// see if the registers are completely constant, and don't need to be evaluated
			// per-surface
			CheckForConstantRegisters();

			pd = NULL;	// the pointer will be invalid after exiting this function*/

			// finish things up
			if(TestMaterialFlag(MaterialFlags.Defaulted))
			{
				MakeDefault();
				return false;
			}
			return true;
		}

		protected override bool GenerateDefaultText()
		{
			// if there exists an image with the same name
			if(true) //fileSystem->ReadFile( GetName(), NULL ) != -1 ) {
			{
				this.SourceText = string.Format("material {0} // IMPLICITLY GENERATED\n"
					+ "{\n{\nblend blend\n"
					+ "colored\n map \"{1}\"\nclamp\n}\n}\n", this.Name, this.Name);

				return true;
			}

			return false;
		}

		protected override void ClearData()
		{
			// TODO
			/*if ( stages ) {
				// delete any idCinematic textures
				for ( i = 0; i < numStages; i++ ) {
					if ( stages[i].texture.cinematic != NULL ) {
						delete stages[i].texture.cinematic;
						stages[i].texture.cinematic = NULL;
					}
					if ( stages[i].newStage != NULL ) {
						Mem_Free( stages[i].newStage );
						stages[i].newStage = NULL;
					}
				}
				R_StaticFree( stages );
				stages = NULL;
			}
			if ( expressionRegisters != NULL ) {
				R_StaticFree( expressionRegisters );
				expressionRegisters = NULL;
			}
			if ( constantRegisters != NULL ) {
				R_StaticFree( constantRegisters );
				constantRegisters = NULL;
			}
			if ( ops != NULL ) {
				R_StaticFree( ops );
				ops = NULL;
			}*/
		}
		#endregion
	}

	public enum MaterialFlags
	{
		Defaulted = 1 << 0,
		PolygonOffset = 1 << 1,
		NoShadows = 1 << 2,
		ForceShadows = 1 << 3,
		NoSelfShadow = 1 << 4,
		/// <summary>This fog volume won't ever consder a portal fogged out.</summary>
		NoPortalFog = 1 << 5,
		/// <summary>In use (visible) per editor.</summary>
		EditorVisible = 1 << 6
	}

	/// <summary>
	/// Content flags.
	/// </summary>
	/// <remarks>
	/// Make sure to keep the defines in doom_defs.script up to date with these!
	/// </remarks>
	public enum ContentFlags
	{
		/// <summary>An eye is never valid in a solid.</summary>
		Solid = 1 << 0,
		/// <summary>Blocks visibility (for ai).</summary>
		Opaque = 1 << 1,
		/// <summary>Used for water.</summary>
		Water = 1 << 2,
		/// <summary>Solid to players.</summary>
		PlayerClip = 1 << 3,
		/// <summary>Solid to monsters.</summary>
		MonsterClip = 1 << 4,
		/// <summary>Solid to moveable entities.</summary>
		MoveableClip = 1 << 5,
		/// <summary>Solid to IK.</summary>
		IkClip = 1 << 6,
		/// <summary>Used to detect blood decals.</summary>
		Blood = 1 << 7,
		/// <summary>sed for actors.</summary>
		Body = 1 << 8,
		/// <summary>Used for projectiles.</summary>
		Projectile = 1 << 9,
		/// <summary>Used for dead bodies.</summary>
		Corpse = 1 << 10,
		/// <summary>Used for render models for collision detection.</summary>
		RenderModel = 1 << 11,
		/// <summary>Used for triggers.</summary>
		Trigger = 1 << 12,
		/// <summary>Solid for AAS.</summary>
		AasSolid = 1 << 13,
		/// <summary>Used to compile an obstacle into AAS that can be enabled/disabled.</summary>
		AasObstacle = 1 << 14,
		/// <summary>Used for triggers that are activated by the flashlight.</summary>
		FlashlightTrigger = 1 << 15,

		/// <summary>Portal separating renderer areas.</summary>
		AreaPortal = 1 << 20,
		/// <summary>Don't cut this brush with CSG operations in the editor.</summary>
		NoCsg = 1 << 21,

		RemoveUtil = ~(AreaPortal | NoCsg)
	}

	internal sealed class MaterialParsingData
	{
		public List<bool> RegisterIsTemporary = new List<bool>();
		public List<float> ShaderRegisters = new List<float>();

		public List<ExpressionOperation> ShaderOperations = new List<ExpressionOperation>();
		public List<ShaderStage> ShaderStages = new List<ShaderStage>();

		public bool RegistersAreConstant;
		public bool ForceOverlays;
	}

	internal sealed class ExpressionOperation
	{
		public ExpressionOperationType OperationType;
		public int A, B, C;
	}

	internal struct DecalInfo
	{
		/// <summary>
		/// msec for no change.
		/// </summary>
		public int StayTime;

		/// <summary>
		/// msec to fade vertex colors over.
		/// </summary>
		public int FadeTime;

		/// <summary>
		/// Vertex color at spawn (possibly out of 0.0 - 1.0 range, will clamp after calc).
		/// </summary>
		public float[] Start;

		/// <summary>
		/// Vertex color at fade-out (possibly out of 0.0 - 1.0 range, will clamp after calc).
		/// </summary>
		public float[] End;
	}

	internal struct ShaderStage
	{
		public int ConditionRegister;			// if registers[conditionRegister] == 0, skip stage.
		public StageLighting Lighting;			// determines which passes interact with lights.

		/*int					drawStateBits;*/
		public ColorStage Color;
		public bool HasAlphaTest;
		public int AlphaTestRegister;

		public TextureStage Texture;
		public StageVertexColor VertexColor;

		public bool IgnoreAlphaTest;			// this stage should act as translucent, even if the surface is alpha tested.
		public float PrivatePolygonOffset;		// a per-stage polygon offset.

		NewShaderStage NewStage;				// vertex / fragment program based stage.
	}

	internal struct TextureStage
	{
		// TODO
		/*idCinematic *		cinematic;
		idImage *			image;*/
		public TextureCoordinateGeneration TextureCoordinates;
		public int[,] Matrix;	// we only allow a subset of the full projection matrix.

		// dynamic image variables
		public DynamicImageType Dynamic;
		public int Width;
		public int Height;
		public int FrameCount;
	}

	internal struct ColorStage
	{
		public int[] Registers;
	}

	internal struct NewShaderStage
	{
		public int VertexProgram;
		public int[,] VertexParameters; // evaluated register indexes.

		public int FragmentProgram;
		public idImage[] FragmentProgramImages;

		// TODO: public idMegaTexture MegaTexture; // handles all the binding and parameter setting.
	}

	public enum SurfaceTypes
	{
		None,
		Metal,
		Stone,
		Flesh,
		Wood,
		Cardboard,
		Liquid,
		Glass,
		Plastic,
		Ricochet,
		Extra1,
		Extra2,
		Extra3,
		Extra4,
		Extra5,
		Extra6
	}

	public enum SurfaceFlags
	{
		/// <summary>Encodes the material type (metal, flesh, concrete, etc.).</summary>
		Bit0 = 1 << 0,
		Bit1 = 1 << 1,
		Bit2 = 1 << 2,
		Bit3 = 1 << 3,
		Mask = (1 << 4) - 1,

		/// <summary>Nver give falling damage.</summary>
		NoDamage = 1 << 4,
		/// <summary>Effects game physics.</summary>
		Slick = 1 << 5,
		/// <summary>Collision surface.</summary>
		Collision = 1 << 6,
		/// <summary>Player can climb up this surface.</summary>
		Ladder = 1 << 7,
		/// <summary>Don't make missile explosions.</summary>
		NoImpact = 1 << 8,
		/// <summary>No footstep sounds.</summary>
		NoSteps = 1 << 9,
		/// <summary>Not clipped or merged by utilities.</summary>
		Discrete = 1 << 10,
		/// <summary>dmap won't cut surface at each bsp boundary.</summary>
		NoFragment = 1 << 11,
		/// <summary>Renderbump will draw this surface as 0x80 0x80 0x80, which won't collect light from any angle</summary>
		NullNormal = 1 << 12,

		None
	}

	internal enum ExpressionOperationType
	{
		Add,
		Subtract,
		Multiply,
		Divide,
		Modulo,
		Table,
		GreaterThan,
		GreaterThanOrEquals,
		LessThan,
		LessThanOrEquals,
		Equals,
		NotEquals,
		And,
		Or,
		Sound
	}

	public enum ExpressionRegister
	{
		Time,

		Parm0,
		Parm1,
		Parm2,
		Parm3,
		Parm4,
		Parm5,
		Parm6,
		Parm7,
		Parm8,
		Parm9,
		Parm10,
		Parm11,

		Global0,
		Global1,
		Global2,
		Global3,
		Global4,
		Global5,
		Global6,
		Global7
	}

	public enum MaterialCoverage
	{
		Bad,
		/// <summary>Completely fills the triangle, will have black drawn on fillDepthBuffer.</summary>
		Opaque,
		/// <summary>May have alpha tested holes.</summary>
		Perforated,
		/// <summary>Blended with background.</summary>
		Translucent
	}

	public enum MaterialSort
	{
		/// <summary>Mirrors, view screens, etc.</summary>
		Subview = -3,
		Gui = -2,
		Bad = -1,
		Opaque,
		PortalSky,
		/// <summary>Scorch marks, etc.</summary>
		Decal,
		Far,
		/// <summary>Normal translucent.</summary>
		Medium,
		Close,
		/// <summary>Gun smoke puffs.</summary>
		AlmostNearest,
		/// <summary>Screen blood blobs.</summary>
		Nearest,
		/// <summary>After a screen copy to texture.</summary>
		PostProcess = 100
	}

	public enum CullType
	{
		Front,
		Back,
		TwoSided
	}

	public enum DeformType
	{
		None,
		Sprite,
		Tube,
		Flare,
		Expand,
		Move,
		Eyeball,
		Particle,
		Particle2,
		Turbulent
	}
}