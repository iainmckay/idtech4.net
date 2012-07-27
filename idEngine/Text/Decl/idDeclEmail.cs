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
	public sealed class idDeclEmail : idDecl
	{
		#region Properties
		public string Text
		{
			get
			{
				return _text;
			}
		}

		public string Subject
		{
			get
			{
				return _subject;
			}
		}

		public string Date
		{
			get
			{
				return _date;
			}
		}

		public string To
		{
			get
			{
				return _to;
			}
		}

		public string From
		{
			get
			{
				return _from;
			}
		}

		public string Image
		{
			get
			{
				return _image;
			}
		}
		#endregion

		#region Members
		private string _text;
		private string _subject;
		private string _date;
		private string _to;
		private string _from;
		private string _image;
		#endregion

		#region Constructor
		public idDeclEmail()
			: base()
		{

		}
		#endregion

		#region idDecl implementation
		public override bool Parse(string text)
		{
			idLexer lexer = new idLexer(LexerOptions.NoStringConcatination | LexerOptions.AllowPathNames | LexerOptions.AllowMultiCharacterLiterals | LexerOptions.AllowBackslashStringConcatination | LexerOptions.NoFatalErrors);
			lexer.LoadMemory(text, this.FileName, this.LineNumber);
			lexer.SkipUntilString("{");

			idToken token;

			_text = string.Empty;

			string tokenLower;
			string tokenValue;

			// scan through, identifying each individual parameter
			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenValue = token.ToString();
				tokenLower = tokenValue.ToLower();

				if(tokenValue == "}")
				{
					break;
				}
				else if(tokenLower == "subject")
				{
					_subject = lexer.ReadToken().ToString();
				}
				else if(tokenLower == "to")
				{
					_to = lexer.ReadToken().ToString();
				}
				else if(tokenLower == "from")
				{
					_from = lexer.ReadToken().ToString();
				}
				else if(tokenLower == "date")
				{
					 _date = lexer.ReadToken().ToString();
				}
				else if(tokenLower == "text")
				{
					token = lexer.ReadToken();
					tokenValue = token.ToString();

					if(tokenValue != "{")
					{
						lexer.Warning("Email dec '{0}' had a parse error", this.Name);
						return false;
					}

					while(((token = lexer.ReadToken()) != null) && (token.ToString() != "}"))
					{
						_text += token.ToString();
					}
				}
				else if(tokenLower == "image")
				{
					_image = lexer.ReadToken().ToString();
				}
			}

			if(lexer.HadError == true)
			{
				lexer.Warning("Email decl '{0}' had a parse error", this.Name);
				return false;
			}

			return true;
		}

		public override string GetDefaultDefinition()
		{
			return "{\n\t{\n\t\tto\t5Mail recipient\n\t\tsubject\t5Nothing\n\t\tfrom\t5No one\n\t}\n}";
		}
		#endregion
	}
}