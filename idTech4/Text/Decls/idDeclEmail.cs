/*
===========================================================================

Doom 3 BFG Edition GPL Source Code
Copyright (C) 1993-2012 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 BFG Edition GPL Source Code ("Doom 3 BFG Edition Source Code").  

Doom 3 BFG Edition Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 BFG Edition Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 BFG Edition Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 BFG Edition Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 BFG Edition Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;

using idTech4.Services;

namespace idTech4.Text.Decls
{
	public sealed class idDeclEmail : idDecl
	{
		#region Properties
		public string Date
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _date;
			}
		}

		public string From
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _from;
			}
		}

		public string Image
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _image;
			}
		}

		public string Subject
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _subject;
			}
		}

		public string Text
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _text;
			}
		}

		public string To
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _to;
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
		#region Properties
		public override string DefaultDefinition
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return "{\n\t{\n\t\tto\t5Mail recipient\n\t\tsubject\t5Nothing\n\t\tfrom\t5No one\n\t}\n}";
			}
		}

		public override int MemoryUsage
		{
			get
			{
				idLog.Warning("TODO: idDeclEmail.MemoryUsage");
				return 0;
			}
		}
		#endregion

		#region Methods
		public override bool Parse(string text)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			string baseStrID = string.Format("#str_{0}_email_", this.Name);
			bool useOldStrings = idEngine.Instance.GetService<ICVarSystem>().GetBool("g_useOldPDAStrings");
			ILocalization localization = idEngine.Instance.GetService<ILocalization>();
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

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
				else if(tokenLower == "date")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_date = token.ToString();
					}
					else
					{
						_date = localization.Get(baseStrID + "from");
					}
				}
				else if(tokenLower == "from")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_from = token.ToString();
					}
					else
					{
						_from = localization.Get(baseStrID + "from");
					}
				}
				else if(tokenLower == "subject")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_subject = token.ToString();
					}
					else
					{
						_to = localization.Get(baseStrID + "subject");
					}
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

					if(useOldStrings == false)
					{
						_text = localization.Get(baseStrID + "text");
					}
				}
				else if(tokenLower == "to")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_to = token.ToString();
					}
					else
					{
						_to = localization.Get(baseStrID + "subject");
					}
				}				
			}

			if(lexer.HadError == true)
			{
				lexer.Warning("Email decl '{0}' had a parse error", this.Name);
				return false;
			}

			return true;
		}
		#endregion
		#endregion
	}
}