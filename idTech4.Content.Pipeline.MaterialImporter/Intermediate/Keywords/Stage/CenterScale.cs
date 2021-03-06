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

using Microsoft.Xna.Framework.Content.Pipeline;

using idTech4.Content.Pipeline.Lexer;
using idTech4.Renderer;
using idTech4.Text;

namespace idTech4.Content.Pipeline.Intermediate.Material.Keywords.Stage
{
	[LexerKeyword("centerScale")]
	public class CenterScale : LexerKeyword<MaterialContent>
	{
		public override bool Parse(idLexer lexer, ContentImporterContext context, MaterialContent content)
		{
			MaterialStage stage = (MaterialStage) this.Tag;

			int a = ParseExpression(lexer);
			content.MatchToken(lexer, ",");
			int b = ParseExpression(lexer);

			int[,] matrix = new int[2, 3];

			// this subtracts 0.5, then scales, then adds 0.5
			matrix[0, 0] = a;
			matrix[0, 1] = GetExpressionConstant(0);
			matrix[0, 2] = EmitOp(GetExpressionConstant(0.5f), EmitOp(GetExpressionConstant(0.5f), a, ExpressionOperationType.Multiply), ExpressionOperationType.Subtract);

			matrix[1, 0] = GetExpressionConstant(0);
			matrix[1, 1] = b;
			matrix[1, 2] = EmitOp(GetExpressionConstant(0.5f), EmitOp(GetExpressionConstant(0.5f), b, ExpressionOperationType.Multiply), ExpressionOperationType.Subtract);

			MultiplyTextureMatrix(ref stage.Texture, matrix);

			return true;
		}
	}
}