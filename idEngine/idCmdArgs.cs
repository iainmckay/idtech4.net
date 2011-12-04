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

namespace idTech4
{
	/// <summary>
	/// Command arguments.
	/// </summary>
	public sealed class idCmdArgs
	{
		#region Properties
		public int Length
		{
			get
			{
				return _args.Length;
			}
		}
		#endregion

		#region Members
		private string[] _args = new string[] { };
		#endregion

		#region Constructor
		public idCmdArgs()
		{

		}

		public idCmdArgs(string text, bool keepAsStrings)
		{
			TokenizeString(text, keepAsStrings);
		}
		#endregion

		#region Methods
		/// <summary>
		/// Gets the argument at the specified index.
		/// </summary>
		/// <param name="idx"></param>
		/// <returns>Argument value or an empty string if outside the range of arguments.</returns>
		public string Get(int idx)
		{
			if((idx >= 0) && (idx < _args.Length))
			{
				return _args[idx];
			}

			return string.Empty;
		}

		/// <summary>
		/// Gets the specified range as a single string.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public string Get(int start, int end)
		{
			return Get(start, end, false);
		}

		/// <summary>
		/// Gets the specified range as a single string.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public string Get(int start, int end, bool escapeArgs)
		{
			if(end < 0)
			{
				end = _args.Length - 1;
			}
			else if(end >= _args.Length)
			{
				end = _args.Length - 1;
			}

			StringBuilder b = new StringBuilder();

			if(escapeArgs == true)
			{
				b.Append('"');
			}

			for(int i = start; i <= end; i++)
			{
				if(i > start)
				{
					if(escapeArgs == true)
					{
						b.Append("\" \"");
					}
					else
					{
						b.Append(" ");
					}
				}

				if((escapeArgs == true) && (_args[i].IndexOf('\\') != -1))
				{
					for(int j = 0; j < _args[i].Length; j++)
					{
						if(_args[i][j] == '\\')
						{
							b.Append("\\\\");
						}
						else
						{
							b.Append(_args[i].Substring(i));
						}
					}
				}
				else
				{
					b.Append(_args[i]);
				}
			}

			if(escapeArgs == true)
			{
				b.Append('"');
			}

			return b.ToString();
		}

		public void Clear()
		{
			_args = new string[] { };
		}

		/// <summary>
		/// Takes a string and breaks it up into arg tokens.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="keepAsStrings">true to only seperate tokens from whitespace and comments, ignoring punctuation.</param>
		public void TokenizeString(string text, bool keepAsStrings)
		{
			// clear previous args.
			_args = new string[] { };

			if(text.Length == 0)
			{
				return;
			}

			idLexer lexer = new idLexer();
			lexer.LoadMemory(text, "idCmdSystem.TokenizeString");
			lexer.Options = LexerOptions.NoErrors | LexerOptions.NoWarnings | LexerOptions.NoStringConcatination | LexerOptions.AllowPathNames | LexerOptions.NoStringEscapeCharacters | LexerOptions.AllowIPAddresses | ((keepAsStrings == true) ? LexerOptions.OnlyStrings : 0);

			idToken token = null, number = null;
			List<string> newArgs = new List<string>();
			int len = 0, totalLength = 0;

			string tokenValue;

			while(true)
			{
				if(newArgs.Count == idE.MaxCommandArgs)
				{
					break; // this is usually something malicious.
				}

				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenValue = token.ToString();

				if((keepAsStrings == false) && (tokenValue == "-"))
				{
					// check for negative numbers.
					if((number = lexer.CheckTokenType(TokenType.Number, 0)) != null)
					{
						token.Set("-" + number);
					}
				}

				// check for cvar expansion
				if(tokenValue == "$")
				{
					if((token = lexer.ReadToken()) == null)
					{
						break;
					}

					if(idE.CvarSystem.IsInitialized == true)
					{
						token.Set(idE.CvarSystem.GetString(token.ToString()));
					}
					else
					{
						token.Set("<unknown>");
					}
				}

				tokenValue = token.ToString();

				len = tokenValue.Length;
				totalLength += len + 1;

				// regular token
				newArgs.Add(tokenValue);
			}

			_args = newArgs.ToArray();
		}

		public void AppendArg(string text)
		{
			if(this.Length == 0)
			{
				_args = new string[] { text };
			}
			else
			{
				List<string> args = new List<string>(_args);
				args.Add(text);

				_args = args.ToArray();
			}
		}

		public override string ToString()
		{
			return Get(1, -1, false);
		}
		#endregion
	}
}