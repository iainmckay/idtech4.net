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

using idTech4.Content.Pipeline.Lexer;
using idTech4.Renderer;
using idTech4.Text;

namespace idTech4.Content.Pipeline.Intermediate.Material.Keywords.Stage
{
	[LexerKeyword("blend")]
	public class Blend : LexerKeyword<MaterialContent>
	{
		public override bool Parse(idLexer lexer, ContentImporterContext context, MaterialContent content)
		{
			idToken token;

			if((token = lexer.ReadToken()) == null)
			{
				return false;
			}

			string tokenValue   = token.ToString();
			string tokenLower   = tokenValue.ToLower();
			MaterialStage stage = (MaterialStage) this.Tag;

			// blending combinations
			if(tokenLower == "blend")
			{
				stage.DrawStateBits = MaterialStates.SourceBlendSourceAlpha | MaterialStates.DestinationBlendOneMinusSourceAlpha;
			}
			else if(tokenLower == "add")
			{
				stage.DrawStateBits = MaterialStates.SourceBlendOne | MaterialStates.DestinationBlendOne;
			}
			else if((tokenLower == "filter") || (tokenLower == "modulate"))
			{
				stage.DrawStateBits = MaterialStates.SourceBlendDestinationColor | MaterialStates.DestinationBlendZero;
			}
			else if(tokenLower == "none")
			{
				// none is used when defining an alpha mask that doesn't draw
				stage.DrawStateBits = MaterialStates.SourceBlendZero | MaterialStates.DestinationBlendOne;
			}
			else if(tokenLower == "bumpmap")
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
				MaterialStates sourceBlendMode = GetSourceBlendMode(tokenLower);

				content.MatchToken(lexer, ",");

				if((token = lexer.ReadToken()) == null)
				{
					return;
				}

				tokenLower = token.ToString().ToLower();

				MaterialStates destinationBlendMode = GetDestinationBlendMode(tokenLower);

				stage.DrawStateBits = sourceBlendMode | destinationBlendMode;
			}

			return true;
		}
	}
}