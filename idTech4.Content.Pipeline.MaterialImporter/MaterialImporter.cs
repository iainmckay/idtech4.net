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
using System.IO;

using Microsoft.Xna.Framework.Content.Pipeline;

using idTech4.Content.Pipeline.Intermediate.Material;
using idTech4.Content.Pipeline.Lexer;
using idTech4.Renderer;
using idTech4.Text;

using TImport = idTech4.Content.Pipeline.Intermediate.Material.MaterialContent;

namespace idTech4.Content.Pipeline
{
	[ContentImporter(".mtr", DisplayName = "Material - idTech4", DefaultProcessor = "MaterialProcessor")]
	public class MaterialImporter : ContentImporter<TImport>
	{
		#region Constants
		public const int TopPriority             = 4;
		public const int PredefinedRegisterCount = 21;

		private readonly MaterialInfoParameter[] InfoParameters = new MaterialInfoParameter[] {
			// game relevant attributes
			new MaterialInfoParameter("solid",			false, SurfaceFlags.None,        ContentFlags.Solid),		// may need to override a clearSolid
			new MaterialInfoParameter("water",			true,	SurfaceFlags.None,       ContentFlags.Water),		// used for water
			new MaterialInfoParameter("playerclip",		false,	SurfaceFlags.None,       ContentFlags.PlayerClip ),	// solid to players
			new MaterialInfoParameter("monsterclip",	false,	SurfaceFlags.None,       ContentFlags.MonsterClip),	// solid to monsters
			new MaterialInfoParameter("moveableclip",	false,	SurfaceFlags.None,       ContentFlags.MoveableClip),// solid to moveable entities
			new MaterialInfoParameter("ikclip",			false,	SurfaceFlags.None,	     ContentFlags.IkClip),		// solid to IK
			new MaterialInfoParameter("blood",			false,	SurfaceFlags.None,	     ContentFlags.Blood),		// used to detect blood decals
			new MaterialInfoParameter("trigger",		false,	SurfaceFlags.None,       ContentFlags.Trigger),		// used for triggers
			new MaterialInfoParameter("aassolid",		false,	SurfaceFlags.None,	     ContentFlags.AasSolid),	// solid for AAS
			new MaterialInfoParameter("aasobstacle",	false,	SurfaceFlags.None,       ContentFlags.AasObstacle),// used to compile an obstacle into AAS that can be enabled/disabled
			new MaterialInfoParameter("flashlight_trigger",	false,	SurfaceFlags.None,	 ContentFlags.FlashlightTrigger), // used for triggers that are activated by the flashlight
			new MaterialInfoParameter("nonsolid",		true,	SurfaceFlags.None,	     ContentFlags.None),					// clears the solid flag
			new MaterialInfoParameter("nullNormal",		false,	SurfaceFlags.NullNormal, ContentFlags.None),		// renderbump will draw as 0x80 0x80 0x80

			// utility relevant attributes
			new MaterialInfoParameter("areaportal",		true,	SurfaceFlags.None,	     ContentFlags.AreaPortal),	// divides areas
			new MaterialInfoParameter("qer_nocarve",	true,	SurfaceFlags.None,       ContentFlags.NoCsg),		// don't cut brushes in editor

			new MaterialInfoParameter("discrete",		true,	SurfaceFlags.Discrete,   ContentFlags.None),		// surfaces should not be automatically merged together or
																													// clipped to the world,
																													// because they represent discrete objects like gui shaders
																													// mirrors, or autosprites
			new MaterialInfoParameter("noFragment",		false,	SurfaceFlags.NoFragment, ContentFlags.None),

			new MaterialInfoParameter("slick",			false,	SurfaceFlags.Slick,		 ContentFlags.None),
			new MaterialInfoParameter("collision",		false,	SurfaceFlags.Collision,	 ContentFlags.None),
			new MaterialInfoParameter("noimpact",		false,	SurfaceFlags.NoImpact,	 ContentFlags.None),		// don't make impact explosions or marks
			new MaterialInfoParameter("nodamage",		false,	SurfaceFlags.NoDamage,	 ContentFlags.None),		// no falling damage when hitting
			new MaterialInfoParameter("ladder",			false,	SurfaceFlags.Ladder,	 ContentFlags.None),		// climbable
			new MaterialInfoParameter("nosteps",		false,	SurfaceFlags.NoSteps,	 ContentFlags.None),		// no footsteps
 
			// material types for particle, sound, footstep feedback
			new MaterialInfoParameter("metal",			false,  SurfaceFlags.Metal,		ContentFlags.None),	// metal
			new MaterialInfoParameter("stone",			false,  SurfaceFlags.Stone,		ContentFlags.None),	// stone
			new MaterialInfoParameter("flesh",			false,  SurfaceFlags.Flesh,		ContentFlags.None),	// flesh
			new MaterialInfoParameter("wood",			false,  SurfaceFlags.Wood,		ContentFlags.None),	// wood
			new MaterialInfoParameter("cardboard",		false,	SurfaceFlags.Cardboard,	ContentFlags.None),	// cardboard
			new MaterialInfoParameter("liquid",			false,	SurfaceFlags.Liquid,	ContentFlags.None),	// liquid
			new MaterialInfoParameter("glass",			false,	SurfaceFlags.Glass,		ContentFlags.None),	// glass
			new MaterialInfoParameter("plastic",		false,	SurfaceFlags.Plastic,	ContentFlags.None),	// plastic
			new MaterialInfoParameter("ricochet",		false,	SurfaceFlags.Ricochet,	ContentFlags.None),	// behaves like metal but causes a ricochet sound

			// unassigned surface types
			new MaterialInfoParameter("surftype10",		false,	SurfaceFlags.T10,	    ContentFlags.None),
			new MaterialInfoParameter("surftype11",		false,	SurfaceFlags.T11,	    ContentFlags.None),
			new MaterialInfoParameter("surftype12",		false,	SurfaceFlags.T12,	    ContentFlags.None),
			new MaterialInfoParameter("surftype13",		false,	SurfaceFlags.T13,	    ContentFlags.None),
			new MaterialInfoParameter("surftype14",		false,	SurfaceFlags.T14,	    ContentFlags.None),
			new MaterialInfoParameter("surftype15",		false,	SurfaceFlags.T15,	    ContentFlags.None)
		};
		#endregion

		public override TImport Import(string fileName, ContentImporterContext context)
		{
			//System.Diagnostics.Debugger.Launch();

			LexerKeywordFactory generalKeywordFactory = new LexerKeywordFactory();
			generalKeywordFactory.ScanNamespace("idTech4.Content.Pipeline.Intermediate.Material.Keywords.General");
									
			string content = File.ReadAllText(fileName);

			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.LoadMemory(content, fileName, 1);
			lexer.SkipUntilString("{");

			MaterialContent materialContent      = new MaterialContent();
			materialContent.TextureRepeatDefault = TextureRepeat.Repeat; // allow a global setting for repeat
			materialContent.RegisterCount        = PredefinedRegisterCount; // leave space for the parms to be copied in.
			materialContent.RegistersAreConstant = true;

			for(int i = 0; i < materialContent.RegisterCount; i++)
			{
				materialContent.RegisterIsTemporary[i] = true; // they aren't constants that can be folded
			}

			idToken token = null;

			string tokenValue;
			int count;

			while(true)
			{
				if((content.MaterialFlags & MaterialFlags.Defaulted) != 0)
				{
					// we have a parse error
					RaiseError();

					return null;
				}

				if((token = lexer.ExpectAnyToken()) == null)
				{
					content.MaterialFlags |= MaterialFlags.Defaulted;
					RaiseError();

					return null;
				}

				tokenValue = token.ToString();

				// end of material definition
				if(tokenValue == "}")
				{
					break;
				}
				else if(tokenValue == "{")
				{
					// create the new stage
					ParseStage(lexer, materialContent.TextureRepeatDefault);
				}
				// check for the surface / content bit flags
				else if(CheckSurfaceParameter(token) == true)
				{

				}
				else
				{
					LexerKeyword<MaterialContent> keyword = generalKeywordFactory.Create<MaterialContent>(tokenValue);

					if(keyword == null)
					{
						context.Logger.LogWarning(null, null, "unknown general material parameter '{0}' in '{1}'", tokenValue);
						return null;
					}

					if(keyword.Parse(lexer, context, materialContent) == false)
					{
						context.Logger.LogWarning(null, null, "TODO: failed parsing");
					}
				}
			}

			// add _flat or _white stages if needed
			AddImplicitStages(materialContent);

			// order the diffuse / bump / specular stages properly
			SortInteractionStages(materialContent);

			// if we need to do anything with normals (lighting or environment mapping)
			// and two sided lighting was asked for, flag
			// shouldCreateBackSides() and change culling back to single sided,
			// so we get proper tangent vectors on both sides

			// we can't just call ReceivesLighting(), because the stages are still
			// in temporary form
			if(materialContent.CullType == CullType.Two)
			{
				count = materialContent.Stages.Count;

				for(int i = 0; i < count; i++)
				{
					if((materialContent.Stages[i].Lighting != StageLighting.Ambient) || (materialContent.Stages[i].Texture.TextureCoordinates != TextureCoordinateGeneration.Explicit))
					{
						if(materialContent.CullType == CullType.Two)
						{
							materialContent.CullType              = CullType.Front;
							materialContent.ShouldCreateBackSides = true;
						}

						break;
					}
				}
			}

			// currently a surface can only have one unique texgen for all the stages on old hardware
			TextureCoordinateGeneration firstGen = TextureCoordinateGeneration.Explicit;

			count = materialContent.Stages.Count;

			for(int i = 0; i < count; i++)
			{
				if(materialContent.Stages[i].Texture.TextureCoordinates != TextureCoordinateGeneration.Explicit)
				{
					if(firstGen == TextureCoordinateGeneration.Explicit)
					{
						firstGen = materialContent.Stages[i].Texture.TextureCoordinates;
					}
					else if(firstGen != materialContent.Stages[i].Texture.TextureCoordinates)
					{
						context.Logger.LogWarning(null, null, "material '{0}' has multiple stages with a texgen", fileName);
						break;
					}
				}
			}

			return materialContent;
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
		private void AddImplicitStages(MaterialContent materialContent, TextureRepeat textureRepeatDefault = TextureRepeat.Repeat)
		{
			bool hasDiffuse    = false;
			bool hasSpecular   = false;
			bool hasBump       = false;
			bool hasReflection = false;
			int count          = materialContent.Stages.Count;

			for(int i = 0; i < count; i++)
			{
				switch(materialContent.Stages[i].Lighting)
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

				if(materialContent.Stages[i].Texture.TextureCoordinates == TextureCoordinateGeneration.ReflectCube)
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
				string bump = "blend diffusemap\nmap _white\n}\n";

				lexer.LoadMemory(bump, "diffusemap");
				ParseStage(lexer, textureRepeatDefault);
			}
		}

		private bool CheckSurfaceParameter(idToken token)
		{
			string tokenLower = token.ToString().ToLower();

			foreach(MaterialInfoParameter infoParameter in InfoParameters)
			{
				if(tokenLower.Equals(infoParameter.Name, StringComparison.OrdinalIgnoreCase) == true)
				{
					if((infoParameter.SurfaceFlags & Renderer.SurfaceFlags.TypeMask) == Renderer.SurfaceFlags.TypeMask)
					{
						// ensure we only have one surface type set
						_surfaceFlags &= ~SurfaceFlags.TypeMask;
					}

					_surfaceFlags |= infoParameter.SurfaceFlags;
					_contentFlags |= infoParameter.ContentFlags;

					if(infoParameter.ClearSolid == true)
					{
						_contentFlags &= ~ContentFlags.Solid;
					}

					return true;
				}
			}

			return false;
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
		private void SortInteractionStages(MaterialContent materialContent)
		{
			int i     = 0, j = 0;
			int count = materialContent.Stages.Count;

			for(i = 0; i < count; i = j)
			{
				// find the next bump map
				for(j = i + 1; j < count; j++)
				{
					if(materialContent.Stages[j].Lighting == StageLighting.Bump)
					{
						// if the very first stage wasn't a bumpmap,
						// this bumpmap is part of the first group.
						if(materialContent.Stages[i].Lighting != StageLighting.Bump)
						{
							continue;
						}
						break;
					}
				}
			}

			// bubble sort everything bump / diffuse / specular.
			for(int l = 1; l < j - i; l++)
			{
				for(int k = i; k < k - l; k++)
				{
					if(materialContent.Stages[k].Lighting > materialContent.Stages[k + 1].Lighting)
					{
						MaterialStage temp = materialContent.Stages[k];

						materialContent.Stages[k]     = materialContent.Stages[k + 1];
						materialContent.Stages[k + 1] = temp;
					}
				}
			}
		}
	}
}