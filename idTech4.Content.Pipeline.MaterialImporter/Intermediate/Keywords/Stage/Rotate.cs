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
	[LexerKeyword("rotate")]
	public class Rotate : LexerKeyword<MaterialContent>
	{
		public override bool Parse(idLexer lexer, ContentImporterContext context, MaterialContent content)
		{
			MaterialStage stage = (MaterialStage) this.Tag;

			int sinReg, cosReg;

			// in cycles
			int a = ParseExpression(lexer);

			idDeclTable table = declManager.FindType<idDeclTable>(DeclType.Table, "sinTable", false);

			if(table == null)
			{
				idLog.Warning("no sinTable for rotate defined");
				this.MaterialFlag = MaterialFlags.Defaulted;

				return;
			}

			sinReg = EmitOp(table.Index, a, ExpressionOperationType.Table);
			table = declManager.FindType<idDeclTable>(DeclType.Table, "cosTable", false);

			if(table == null)
			{
				idLog.Warning("no cosTable for rotate defined");
				this.MaterialFlag = MaterialFlags.Defaulted;

				return;
			}

			cosReg = EmitOp(table.Index, a, ExpressionOperationType.Table);

			// this subtracts 0.5, then rotates, then adds 0.5
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

			MultiplyTextureMatrix(ref materialStage.Texture, matrix);
	
			return true;
		}
	}
}