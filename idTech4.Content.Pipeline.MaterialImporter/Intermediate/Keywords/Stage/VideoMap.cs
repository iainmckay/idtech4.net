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

namespace idTech4.Content.Pipeline.Intermediate.Material.Keywords.Stage
{
	[LexerKeyword("videoMap")]
	public class VideoMap : LexerKeyword<MaterialContent>
	{
		public override bool Parse(idLexer lexer, ContentImporterContext context, MaterialContent content)
		{
			idToken token;
			MaterialStage stage = (MaterialStage) this.Tag;
			
			// note that videomaps will always be in clamp mode, so texture coordinates had better be in the 0 to 1 range
			if((token = lexer.ReadToken()) == null)
			{
				context.Logger.LogWarning(null, null, "Missing parameter for 'videoMap' keyword.");
				return false;
			}
			else
			{
				bool loop = false;

				if(token.ToString().Equals("loop", StringComparison.OrdinalIgnoreCase) == true)
				{
					loop = true;

					if((token = lexer.ReadToken()) == null)
					{
						context.Logger.LogWarning(null, null, "Missing parameter for 'videoMap' keyword.");
						return false;
					}
				}

				context.Logger.LogWarning(null, null, "TODO: material videoMap keyword");

				// TODO: cinematic
				/*ts->cinematic = idCinematic::Alloc();
				ts->cinematic->InitFromFile( token.c_str(), loop );*/
			}
		
			return true;
		}
	}
}