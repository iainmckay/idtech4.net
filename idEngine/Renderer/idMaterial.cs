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

		private MaterialCoverage _coverage;
		private float _sort;					// lower numbered shaders draw before higher numbered.

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

			// TODO
			_sort = MaterialSort.Bad;
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

			/*decalInfo.stayTime = 10000;
			decalInfo.fadeTime = 4000;
			decalInfo.start[0] = 1;
			decalInfo.start[1] = 1;
			decalInfo.start[2] = 1;
			decalInfo.start[3] = 1;
			decalInfo.end[0] = 0;
			decalInfo.end[1] = 0;
			decalInfo.end[2] = 0;
			decalInfo.end[3] = 0;*/
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

				// end of material definition
				if(token.Value == "}")
				{
					break;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "qer_editorImage") == 0)
				{
					token = lexer.ReadTokenOnLine();
					_editorImageName = token.Value;
					lexer.SkipRestOfLine();
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "description") == 0)
				{
					token = lexer.ReadTokenOnLine();
					_description = token.Value;
				}
				// check for the surface / content bit flags
				else if(CheckSurfaceParameter(token) == true)
				{

				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "polygonOffset") == 0)
				{
					this.MaterialFlag = Renderer.MaterialFlags.PolygonOffset;

					if((token = lexer.ReadTokenOnLine()) == null)
					{
						_polygonOffset = 1;
					}
					else
					{
						_polygonOffset = token.FloatValue;
					}
				}
				// noshadow
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "noShadows") == 0)
				{
					this.MaterialFlag = MaterialFlags.NoShadows;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "suppressInSubview") == 0)
				{
					_suppressInSubview = true;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "portalSky") == 0)
				{
					_portalSky = true;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "noSelfShadow") == 0)
				{
					this.MaterialFlag = Renderer.MaterialFlags.NoSelfShadow;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "noPortalFog") == 0)
				{
					this.MaterialFlag = Renderer.MaterialFlags.NoPortalFog;
				}
				// forceShadows allows nodraw surfaces to cast shadows.
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "forceShadows") == 0)
				{
					this.MaterialFlag = Renderer.MaterialFlags.ForceShadows;
				}
				// overlay / decal suppression
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "noOverlays") == 0)
				{
					_allowOverlays = false;
				}
				// moster blood overlay forcing for alpha tested or translucent surfaces
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "forceOverlays") == 0)
				{
					_parsingData.ForceOverlays = true;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "translucent") == 0)
				{
					_coverage = MaterialCoverage.Translucent;
				}
				// global zero clamp
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "zeroClamp") == 0)
				{
					textureRepeatDefault = TextureRepeat.ClampToZero;
				}
				// global clamp
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "clamp") == 0)
				{
					textureRepeatDefault = TextureRepeat.Clamp;
				}
				// global clamp
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "alphaZeroClamp") == 0)
				{
					textureRepeatDefault = TextureRepeat.ClampToZero;
				}
				// forceOpaque is used for skies-behind-windows
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "forceOpaque") == 0)
				{
					_coverage = MaterialCoverage.Opaque;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "twoSided") == 0)
				{
					_cullType = CullType.TwoSided;

					// twoSided implies no-shadows, because the shadow
					// volume would be coplanar with the surface, giving depth fighting
					// we could make this no-self-shadows, but it may be more important
					// to receive shadows from no-self-shadow monsters
					this.MaterialFlag = Renderer.MaterialFlags.NoShadows;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "backSided") == 0)
				{
					_cullType = CullType.Back;

					// the shadow code doesn't handle this, so just disable shadows.
					// We could fix this in the future if there was a need.
					this.MaterialFlag = Renderer.MaterialFlags.NoShadows;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "fogLight") == 0)
				{
					_fogLight = true;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "blendLight") == 0)
				{
					_blendLight = true;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "ambientLight") == 0)
				{
					_ambientLight = true;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "mirror") == 0)
				{
					_sort = (float) MaterialSort.Subview;
					_coverage = MaterialCoverage.Opaque;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "noFog") == 0)
				{
					_noFog = true;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "unsmoothedTangents") == 0)
				{
					_unsmoothedTangents = true;
				}
				// lightFallofImage <imageprogram>
				// specifies the image to use for the third axis of projected
				// light volumes
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "lightFallOffImage") == 0)
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
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "guiSurf") == 0)
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
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "sort") == 0)
				{
					ParseSort(lexer);
				}
				// spectrum <integer>
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "spectrum") == 0)
				{
					token = lexer.ReadTokenOnLine();

					int.TryParse(token.Value, out _spectrum);
				}
				// deform < sprite | tube | flare >
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "deform") == 0)
				{
					ParseDeform(lexer);
				}
				// decalInfo <staySeconds> <fadeSeconds> ( <start rgb> ) ( <end rgb> )
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "decalInfo") == 0)
				{
					ParseDecalInfo(lexer);
				}
				// renderbump <args...>
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "renderBump") == 0)
				{
					_renderBump = lexer.ParseRestOfLine();
				}
				// diffusemap for stage shortcut
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "diffuseMap") == 0)
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
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "specularMap") == 0)
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
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "bumpMap") == 0)
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
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "DECAL_MACRO") == 0)
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
				else if(token.Value == "{")
				{
					// create the new stage
					ParseStage(lexer, textureRepeatDefault);
				}
				else
				{
					idConsole.WriteLine("unknown general material parameter '{0}' in '{1}'", token.Value, this.Name);
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
			if(cullType == CT_TWO_SIDED)
			{
				for(i = 0; i < numStages; i++)
				{
					if(pd->parseStages[i].lighting != SL_AMBIENT || pd->parseStages[i].texture.texgen != TG_EXPLICIT)
					{
						if(cullType == CT_TWO_SIDED)
						{
							cullType = CT_FRONT_SIDED;
							shouldCreateBackSides = true;
						}
						break;
					}
				}
			}

			// currently a surface can only have one unique texgen for all the stages on old hardware
			texgen_t firstGen = TG_EXPLICIT;
			for(i = 0; i < numStages; i++)
			{
				if(pd->parseStages[i].texture.texgen != TG_EXPLICIT)
				{
					if(firstGen == TG_EXPLICIT)
					{
						firstGen = pd->parseStages[i].texture.texgen;
					}
					else if(firstGen != pd->parseStages[i].texture.texgen)
					{
						common->Warning("material '%s' has multiple stages with a texgen", GetName());
						break;
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

			if(Enum.IsDefined(typeof(MaterialSort), token.Value) == true)
			{
				_sort = (float) Enum.Parse(typeof(MaterialSort), token.Value, true);
			}
			else
			{
				float.TryParse(token.Value, out _sort);
			}
		}

		private void ParseDeform(idLexer lexer)
		{
			idToken token = lexer.ExpectAnyToken();

			if(token == null)
			{
				return;
			}

			if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "sprite") == 0)
			{
				_deformType = DeformType.Sprite;
				_cullType = CullType.TwoSided;

				this.MaterialFlag = MaterialFlags.NoShadows;
			}
			else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "tube") == 0)
			{
				_deformType = DeformType.Tube;
				_cullType = CullType.TwoSided;

				this.MaterialFlag = MaterialFlags.NoShadows;
			}
			else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "flare") == 0)
			{
				_deformType = DeformType.Flare;
				_cullType = CullType.TwoSided;
				_deformRegisters[0] = ParseExpression(lexer);

				this.MaterialFlag = MaterialFlags.NoShadows;
			}
			else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "expand") == 0)
			{
				_deformType = DeformType.Expand;
				_deformRegisters[0] = ParseExpression(lexer);
			}
			else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "move") == 0)
			{
				_deformType = DeformType.Move;
				_deformRegisters[0] = ParseExpression(lexer);
			}
			else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "turbulent") == 0)
			{
				_deformType = DeformType.Turbulent;

				if((token = lexer.ExpectAnyToken()) == null)
				{
					lexer.Warning("deform particle missing particle name");
					this.MaterialFlag = MaterialFlags.Defaulted;
				}
				else
				{
					_deformDecl = idE.DeclManager.FindType(DeclType.Table, token.Value, true);

					_deformRegisters[0] = ParseExpression(lexer);
					_deformRegisters[1] = ParseExpression(lexer);
					_deformRegisters[2] = ParseExpression(lexer);
				}
			}
			else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "eyeBall") == 0)
			{
				_deformType = DeformType.Eyeball;
			}
			else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "particle") == 0)
			{
				_deformType = DeformType.Particle;

				if((token = lexer.ExpectAnyToken()) == null)
				{
					lexer.Warning("deform particle missing particle name");
					this.MaterialFlag = MaterialFlags.Defaulted;
				}
				else
				{
					_deformDecl = idE.DeclManager.FindType(DeclType.Particle, token.Value, true);
				}
			}
			else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "particle2") == 0)
			{
				_deformType = DeformType.Particle2;

				if((token = lexer.ExpectAnyToken()) == null)
				{
					lexer.Warning("deform particle missing particle name");
					this.MaterialFlag = MaterialFlags.Defaulted;
				}
				else
				{
					_deformDecl = idE.DeclManager.FindType(DeclType.Table, token.Value, true);

					_deformRegisters[0] = ParseExpression(lexer);
					_deformRegisters[1] = ParseExpression(lexer);
					_deformRegisters[2] = ParseExpression(lexer);
				}
			}
			else
			{
				lexer.Warning("Bad deform type '{0}'", token.Value);
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

			int a = ParseExpressionPriority(src, priority - 1);

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

			if((priority == 1) && (token.Value == "*"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Multiply, priority);
			}
			else if((priority == 1) && (token.Value == "/"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Divide, priority);
			}
			else if((priority == 1) && (token.Value == "%"))
			{
				// implied truncate both to integer
				return ParseEmitOp(lexer, a, ExpressionOperationType.Modulo, priority);
			}
			else if((priority == 2) && (token.Value == "+"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Add, priority);
			}
			else if((priority == 2) && (token.Value == "-"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Subtract, priority);
			}
			else if((priority == 3) && (token.Value == ">"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.GreaterThan, priority);
			}
			else if((priority == 3) && (token.Value == ">="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.GreaterThanOrEquals, priority);
			}
			else if((priority == 3) && (token.Value == ">"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.GreaterThan, priority);
			}
			else if((priority == 3) && (token.Value == ">="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.GreaterThanOrEquals, priority);
			}
			else if((priority == 3) && (token.Value == "<"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.LessThan, priority);
			}
			else if((priority == 3) && (token.Value == "<="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.LessThanOrEquals, priority);
			}
			else if((priority == 3) && (token.Value == "=="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.Equals, priority);
			}
			else if((priority == 3) && (token.Value == "!="))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.NotEquals, priority);
			}
			else if((priority == 4) && (token.Value == "&&"))
			{
				return ParseEmitOp(lexer, a, ExpressionOperationType.And, priority);
			}
			else if((priority == 4) && (token.Value == "||"))
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

			if(token.Value == "(")
			{
				int a = ParseExpression(lexer);
				MatchToken(lexer, ")");

				return a;
			}

			if(Enum.IsDefined(typeof(ExpressionRegister), token.Value) == true)
			{
				ExpressionRegister reg = (ExpressionRegister) Enum.Parse(typeof(ExpressionRegister), token.Value);
				_parsingData.RegistersAreConstant = false;

				return (int) reg;
			}

			if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "fragmentPrograms") == 0)
			{
				// TODO: return GetExpressionConstant((float) glConfig.ARBFragmentProgramAvailable);
			}
			else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "sound") == 0)
			{
				_parsingData.RegistersAreConstant = false;

				return EmitOp(0, 0, ExpressionOperationType.Sound);
			}
			// parse negative numbers
			else if(token.Value == "-")
			{
				token = lexer.ReadToken();

				if((token.Type == TokenType.Number) || (token.Value == "."))
				{
					return GetExpressionConstant(-token.FloatValue);
				}

				lexer.Warning("Bad negative number '{0}'", token.Value);
				this.MaterialFlag = MaterialFlags.Defaulted;

				return 0;
			}
			else if((token.Type == TokenType.Number) || (token.Value == ".") || (token.Value == "-"))
			{
				return GetExpressionConstant(token.FloatValue);
			}

			// see if it is a table name
			idDeclTable table = (idDeclTable) idE.DeclManager.FindType(DeclType.Table, token.Value, false);

			if(table == null)
			{
				lexer.Warning("Bad term '{0}'", token.Value);
				this.MaterialFlag = MaterialFlags.Defaulted;

				return 0;
			}

			// parse a table expression
			MatchToken(lexer, "[");

			int b = ParseExpression(lexer);

			MatchToken(lexer, "]");

			return EmitOp(table.Index, b, ExpressionOperationType.Table);
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

		public int GetExpressionTemporary()
		{
			_parsingData.RegisterIsTemporary.Add(true);

			return (_parsingData.RegisterIsTemporary.Count - 1);
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
				this.Text = string.Format("material {0} // IMPLICITLY GENERATED\n"
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
		public float[] Start = new float[4];

		/// <summary>
		/// Vertex color at fade-out (possibly out of 0.0 - 1.0 range, will clamp after calc).
		/// </summary>
		public float[] End = new float[4];
	}

	internal struct ShaderStage
	{
		// TODO
		/*int					conditionRegister;	// if registers[conditionRegister] == 0, skip stage
		stageLighting_t		lighting;			// determines which passes interact with lights
		int					drawStateBits;
		colorStage_t		color;
		bool				hasAlphaTest;
		int					alphaTestRegister;
		textureStage_t		texture;
		stageVertexColor_t	vertexColor;
		bool				ignoreAlphaTest;	// this stage should act as translucent, even
												// if the surface is alpha tested
		float				privatePolygonOffset;	// a per-stage polygon offset

		newShaderStage_t	*newStage;			// vertex / fragment program based stage*/
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