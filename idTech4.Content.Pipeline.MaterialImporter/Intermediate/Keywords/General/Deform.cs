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

using Microsoft.Xna.Framework.Content.Pipeline;

using idTech4.Content.Pipeline.Lexer;
using idTech4.Renderer;
using idTech4.Text;

namespace idTech4.Content.Pipeline.Intermediate.Material.Keywords.General
{
	[LexerKeyword("deform")]
	public class Deform : LexerKeyword<MaterialContent>
	{
		public override bool Parse(idLexer lexer, ContentImporterContext context, MaterialContent content)
		{
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();
			idToken token            = lexer.ExpectAnyToken();

			if(token == null)
			{
				return false;
			}

			string tokenValue = token.ToString();
			string tokenLower = tokenValue.ToLower();

			if(tokenLower == "sprite")
			{
				content.DeformType    = DeformType.Sprite;
				content.CullType      = CullType.Two;
				content.MaterialFlags |= MaterialFlags.NoShadows;
			}
			else if(tokenLower == "tube")
			{
				content.DeformType    = DeformType.Tube;
				content.CullType       = CullType.Two;
				content.MaterialFlags |= MaterialFlags.NoShadows;
			}
			else if(tokenLower == "flare")
			{
				content.DeformType         = DeformType.Flare;
				content.CullType           = CullType.Two;
				content.DeformRegisters[0] = ParseExpression(lexer);
				content.MaterialFlags      = MaterialFlags.NoShadows;
			}
			else if(tokenLower == "expand")
			{
				content.DeformType         = DeformType.Expand;
				content.DeformRegisters[0] = ParseExpression(lexer);
			}
			else if(tokenLower == "move")
			{
				content.DeformType         = DeformType.Move;
				content.DeformRegisters[0] = ParseExpression(lexer);
			}
			else if(tokenLower == "turbulent")
			{
				content.DeformType = DeformType.Turbulent;

				if((token = lexer.ExpectAnyToken()) == null)
				{
					lexer.Warning("deform particle missing particle name");
					content.MaterialFlags |= MaterialFlags.Defaulted;
				}
				else
				{
					content.DeformDeclType     = DeclType.Table;
					content.DeformDeclName     = TokenType.String(); = declManager.FindType(DeclType.Table, token.ToString(), true);

					content.DeformRegisters[0] = ParseExpression(lexer);
					content.DeformRegisters[1] = ParseExpression(lexer);
					content.DeformRegisters[2] = ParseExpression(lexer);
				}
			}
			else if(tokenLower == "eyeball")
			{
				content.DeformType = DeformType.Eyeball;
			}
			else if(tokenLower == "particle")
			{
				content.DeformType = DeformType.Particle;

				if((token = lexer.ExpectAnyToken()) == null)
				{
					lexer.Warning("deform particle missing particle name");
					content.MaterialFlags |= MaterialFlags.Defaulted;
				}
				else
				{
					content.DeformDeclType = DeclType.Particle;
					content.DeformDeclName = token.ToString();
				}
			}
			else if(tokenLower == "particle2")
			{
				content.DeformType = DeformType.Particle2;

				if((token = lexer.ExpectAnyToken()) == null)
				{
					lexer.Warning("deform particle missing particle name");
					content.MaterialFlags = MaterialFlags.Defaulted;
				}
				else
				{
					content.DeformDeclType = DeclType.Table;
					content.DeformDeclName = token.ToString();
				}
			}
			else
			{
				lexer.Warning("Bad deform type '{0}'", tokenValue);
				content.MaterialFlags |= MaterialFlags.Defaulted;
			}

			return true;
		}
	}
}