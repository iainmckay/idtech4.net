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

namespace idTech4.Text.Decl
{
	/// <summary>
	/// Tables are used to map a floating point input value to a floating point
	///	output value, with optional wrap / clamp and interpolation.
	/// </summary>
	public sealed class idDeclTable : idDecl
	{
		#region Members
		private bool _clamp;
		private bool _snap;
		private float[] _values;
		#endregion

		#region Constructor
		public idDeclTable()
			: base()
		{

		}
		#endregion

		#region idDecl implementation
		protected override void ClearData()
		{
			_clamp = false;
			_snap = false;
			_values = new float[] { };
		}

		public override string GetDefaultDefinition()
		{
			return "{ { 0 } }";
		}

		public override bool Parse(string text)
		{
			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.SkipUntilString("{");

			idToken token;
			List<float> values = new List<float>();

			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}
				idConsole.WriteLine("TOK: {0}", token.Value);

				if(token.Value == "}")
				{
					break;
				}

				if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "snap") == 0)
				{
					_snap = true;
				}
				else if(StringComparer.InvariantCultureIgnoreCase.Compare(token.Value, "clamp") == 0)
				{
					_clamp = true;
				}
				else if(token.Value == "{")
				{
					while(true)
					{
						bool errorFlag;
						float v = lexer.ParseFloat(out errorFlag);

						if(errorFlag == true)
						{
							// we got something non-numeric
							MakeDefault();
							return false;
						}

						values.Add(v);

						token = lexer.ReadToken();

						if(token.Value == "}")
						{
							break;
						}

						if(token.Value == ",")
						{
							continue;
						}

						lexer.Warning("expected comma or brace");
						MakeDefault();

						return false;
					}
				}
				else
				{
					lexer.Warning("unknown token '{0}'", token.Value);
					MakeDefault();

					return false;
				}
			}

			// copy the 0 element to the end, so lerping doesn't
			// need to worry about the wrap case
			float val = values[0];
			values.Add(val);

			_values = values.ToArray();

			return true;
		}
		#endregion
	}
}
