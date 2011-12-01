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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4.Text
{
	/// <summary>
	/// Lexer options.
	/// </summary>
	[Flags]
	public enum LexerOptions
	{
		/// <summary>Don't print any errors.</summary>
		NoErrors = 1 << 0,
		/// <summary>Don't print any warnings.</summary>
		NoWarnings = 1 << 1,
		/// <summary>Errors aren't fatal.</summary>
		NoFatalErrors = 1 << 2,
		/// <summary>Multiple strings seperated by whitespaces are not concatenated.</summary>
		NoStringConcatination = 1 << 3,
		/// <summary>No escape characters inside strings.</summary>
		NoStringEscapeCharacters = 1 << 4,
		/// <summary>Don't use the $ sign for precompilation.</summary>
		NoDollarPrecompilation = 1 << 5,
		/// <summary>Don't include files embraced with < >.</summary>
		NoBaseIncludes = 1 << 6,
		/// <summary>Allow path seperators in names.</summary>
		AllowPathNames = 1 << 7,
		/// <summary>Allow names to start with a number.</summary>
		AllowNumberNames = 1 << 8,
		/// <summary>Allow IP addresses to be parsed as numbers.</summary>
		AllowIPAddresses = 1 << 9,
		/// <summary>Allow float exceptions like 1.#INF or 1.#IND to be parsed.</summary>
		AllowFloatExceptions = 1 << 10,
		/// <summary>Allow multi character literals.</summary>
		AllowMultiCharacterLiterals = 1 << 11,
		/// <summary>Allow multiple strings seperated by '\' to be concatenated.</summary>
		AllowBackslashStringConcatination = 1 << 12,
		/// <summary>Parse as whitespace deliminated strings (quoted strings keep quotes).</summary>
		OnlyStrings = 1 << 13
	}

	/// <summary>
	/// Lexer punctuation.
	/// </summary>
	public enum LexerPunctuationID
	{
		RightShiftAssign = 1,
		LeftShiftAssign = 2,
		Parameters = 3,
		PreCompilerMerge = 4,

		LogicAnd = 5,
		LogicOr = 6,
		LogicGreaterThanOrEqual = 7,
		LogicLessThanOrEqual = 8,
		LogicEqual = 9,
		LogicNotEqual = 10,

		MultiplyAssign = 11,
		DivideAssign = 12,
		ModulusAssign = 13,
		AdditionAssign = 14,
		SubtractAssign = 15,
		Increment = 16,
		Decrement = 17,

		BinaryAndAssign = 18,
		BinaryOrAssign = 19,
		BinaryXORAssign = 20,
		RightShift = 21,
		LeftShift = 22,

		PointerReference = 23,

		CPP1 = 24,
		CPP2 = 25,
		Multiply = 26,
		Divide = 27,
		Modulus = 28,
		Addition = 29,
		Subtract = 30,
		Assign = 31,

		BinaryAnd = 32,
		BinaryOr = 33,
		BinaryXOR = 34,
		BinaryNot = 35,

		LogicNot = 36,
		LogicGreater = 37,
		LogicLess = 38,

		Reference = 39,
		Comma = 40,
		Semicolon = 41,
		Colon = 42,
		QuestionMark = 43,

		ParenthesesOpen = 44,
		ParenthesesClose = 45,
		BraceOpen = 46,
		BraceClose = 47,
		SquareBracketOpen = 48,
		SquareBracketClose = 49,
		Backslash = 50,

		Precompile = 51,
		Dollar = 52
	}

	public struct LexerPunctuation
	{
		public string P; // punctuation character(s).
		public LexerPunctuationID N; // punctuation id.

		public LexerPunctuation(string p, LexerPunctuationID n)
		{
			P = p;
			N = n;
		}
	}

	/// <summary>
	/// Lexicographical parser
	/// </summary>
	/// <remarks>
	/// A number directly following the escape character '\' in a string is
	/// assumed to be in decimal format instead of octal. Binary numbers of
	/// the form 0b.. or 0B.. can also be used.
	/// </remarks>
	public class idLexer
	{
		#region Constants
		#region Default punctuation table
		public static readonly LexerPunctuation[] DefaultPunctuation = new LexerPunctuation[] {
			// binary operators
			new LexerPunctuation(">>=", LexerPunctuationID.RightShiftAssign),
			new LexerPunctuation("<<=", LexerPunctuationID.LeftShiftAssign),
			//
			new LexerPunctuation("...", LexerPunctuationID.Parameters),
			//define merge operator
			new LexerPunctuation("##", LexerPunctuationID.PreCompilerMerge),
			// logic operators
			new LexerPunctuation("&&", LexerPunctuationID.LogicAnd),
			new LexerPunctuation("||", LexerPunctuationID.LogicOr),
			new LexerPunctuation(">=", LexerPunctuationID.LogicGreaterThanOrEqual),
			new LexerPunctuation("<=", LexerPunctuationID.LogicLessThanOrEqual),
			new LexerPunctuation("==", LexerPunctuationID.LogicEqual),
			new LexerPunctuation("!=", LexerPunctuationID.LogicNotEqual),
			//arithmatic operators
			new LexerPunctuation("*=", LexerPunctuationID.MultiplyAssign),
			new LexerPunctuation("/=", LexerPunctuationID.DivideAssign),
			new LexerPunctuation("%=", LexerPunctuationID.ModulusAssign),
			new LexerPunctuation("+=", LexerPunctuationID.AdditionAssign),
			new LexerPunctuation("-=", LexerPunctuationID.SubtractAssign),
			new LexerPunctuation("++", LexerPunctuationID.Increment),
			new LexerPunctuation("--", LexerPunctuationID.Decrement),
			//binary operators
			new LexerPunctuation("&=", LexerPunctuationID.BinaryAndAssign),
			new LexerPunctuation("|=", LexerPunctuationID.BinaryOrAssign),
			new LexerPunctuation("^=", LexerPunctuationID.BinaryXORAssign),
			new LexerPunctuation(">>", LexerPunctuationID.RightShift),
			new LexerPunctuation("<<", LexerPunctuationID.LeftShift),
			//reference operators
			new LexerPunctuation("->", LexerPunctuationID.PointerReference),
			//C++
			new LexerPunctuation("::", LexerPunctuationID.CPP1),
			new LexerPunctuation(".*", LexerPunctuationID.CPP2),		
			//arithmatic operators
			new LexerPunctuation("*", LexerPunctuationID.Multiply),
			new LexerPunctuation("/", LexerPunctuationID.Divide),
			new LexerPunctuation("%", LexerPunctuationID.Modulus),
			new LexerPunctuation("+", LexerPunctuationID.Addition),
			new LexerPunctuation("-", LexerPunctuationID.Subtract),
			new LexerPunctuation("=", LexerPunctuationID.Assign),
			//binary operators
			new LexerPunctuation("&", LexerPunctuationID.BinaryAnd),
			new LexerPunctuation("|", LexerPunctuationID.BinaryOr),
			new LexerPunctuation("^", LexerPunctuationID.BinaryXOR),
			new LexerPunctuation("~", LexerPunctuationID.BinaryNot),
			//logic operators
			new LexerPunctuation("!", LexerPunctuationID.LogicNot),
			new LexerPunctuation(">", LexerPunctuationID.LogicGreater),
			new LexerPunctuation("<", LexerPunctuationID.LogicLess),
			//reference operator
			new LexerPunctuation(".", LexerPunctuationID.Reference),
			//seperators
			new LexerPunctuation(",", LexerPunctuationID.Comma),
			new LexerPunctuation(";", LexerPunctuationID.Semicolon),
			//label indication
			new LexerPunctuation(":", LexerPunctuationID.Colon),
			//if statement
			new LexerPunctuation("?", LexerPunctuationID.QuestionMark),
			//embracements
			new LexerPunctuation("(", LexerPunctuationID.ParenthesesOpen),
			new LexerPunctuation(")", LexerPunctuationID.ParenthesesClose),
			new LexerPunctuation("{", LexerPunctuationID.BraceOpen),
			new LexerPunctuation("}", LexerPunctuationID.BraceClose),
			new LexerPunctuation("[", LexerPunctuationID.SquareBracketOpen),
			new LexerPunctuation("]", LexerPunctuationID.SquareBracketClose),
			//
			new LexerPunctuation("\\", LexerPunctuationID.Backslash),
			//precompiler operator
			new LexerPunctuation("#", LexerPunctuationID.Precompile),
			new LexerPunctuation("$", LexerPunctuationID.Dollar)
		};
		#endregion
		#endregion

		#region Properties
		/// <summary>
		/// Is a script is loaded?
		/// </summary>
		public bool IsLoaded
		{
			get
			{
				return _loaded;
			}
		}

		/// <summary>
		/// Gets or sets the options to use.
		/// </summary>
		public LexerOptions Options
		{
			get
			{
				return _options;
			}
			set
			{
				_options = value;
			}
		}

		/// <summary>
		/// Punctuation to use for this script.
		/// </summary>
		public LexerPunctuation[] Punctuation
		{
			get
			{
				return _punctuation;
			}
			set
			{
				// TODO
				/*#ifdef PUNCTABLE
				if (p) {
					idLexer::CreatePunctuationTable( p );
				} else {
					idLexer::CreatePunctuationTable( default_punctuations );
				}
				#endif //PUNCTABLE*/

				if(value != null)
				{
					_punctuation = value;
				}
				else
				{
					_punctuation = idLexer.DefaultPunctuation;
				}
			}
		}

		public int FileOffset
		{
			get
			{
				return _scriptPosition;
			}
		}

		public int LineNumber
		{
			get
			{
				return _line;
			}
		}

		public idToken UnreadToken
		{
			set
			{
				if(_tokenAvailable == true)
				{
					idConsole.FatalError("unread token called twice");
				}

				_token = value;
				_tokenAvailable = true;
			}
		}

		/// <summary>
		/// Gets whether or not there were errors.
		/// </summary>
		public bool HadError
		{
			get
			{
				return _hadError;
			}
		}
		#endregion

		#region Members
		private bool _loaded; // set when a script file is loaded from file or memory.
		private string _fileName; // file name of the script.
		private TimeSpan _fileTime; // file time.
		private bool _allocated;	// true if buffer memory was allocated.
		private string _buffer; // buffer containing the script.
		private int _length; // length of the script in bytes.
		private int _line; // current line in script.
		private int _lastLine; // line before reading token.
		private bool _tokenAvailable; // set by unreadToken.
		private LexerOptions _options;	// several script flags.
		private idToken _token; // available token.
		private idLexer _next; // next script in a chain.
		private bool _hadError; // set by idLexer::Error, even if the error is supressed.

		private int _scriptPosition; // current position in the script.
		private int _endPosition; // position at the end of the script.
		private int _lastScriptPosition; // script position before reading token.
		private int _whiteSpaceStartPosition; // start of last white space.
		private int _whiteSpaceEndPosition; // end of last white space.

		private LexerPunctuation[] _punctuation; // the punctuation used in the script.
		#endregion

		#region Constructor
		public idLexer()
		{
			this.Punctuation = null;
		}

		public idLexer(LexerOptions options)
			: this()
		{
			_options = options;
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Load a script from the given memory.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool LoadMemory(string text, string name)
		{
			return LoadMemory(text, name, 1);
		}

		/// <summary>
		/// Load a script from the given memory and a specified line offset,
		/// so source strings extracted from a file can still refer to proper line numbers in the file.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="name"></param>
		/// <param name="startLine"></param>
		/// <returns></returns>
		public bool LoadMemory(string text, string name, int startLine)
		{
			if(this.IsLoaded == true)
			{
				idConsole.Error("idLexer.LoadMemory: another script is already loaded");
				return false;
			}

			_fileName = name;
			_fileTime = TimeSpan.Zero;
			_buffer = text;
			_length = text.Length;

			_scriptPosition = 0;
			_lastScriptPosition = 0;
			_endPosition = _buffer.Length;

			_tokenAvailable = false;
			_line = startLine;
			_lastLine = startLine;
			_allocated = false;
			_loaded = true;

			return true;
		}

		/// <summary>
		/// Reads the next token.
		/// </summary>
		/// <returns></returns>
		public idToken ReadToken()
		{
			if(this.IsLoaded == false)
			{
				idConsole.Error("idLexer.ReadToken: no file loaded");
				return null;
			}

			// if there is a token available (from unreadToken)
			if(_tokenAvailable == true)
			{
				_tokenAvailable = false;
				return _token;
			}

			// save script pointer
			_lastScriptPosition = _scriptPosition;
			
			// save line counter
			_lastLine = _line;

			// clear the token stuff
			_token = new idToken();

			// start of the white space
			_whiteSpaceStartPosition = _scriptPosition;
			_token.WhiteSpaceStartPosition = _scriptPosition;
			
			// read white space before token
			if(ReadWhiteSpace() == false)
			{
				return null;
			}


			// end of the white space
			_whiteSpaceEndPosition = _scriptPosition;
			_token.WhiteSpaceEndPosition = _scriptPosition;

			// line the token is on
			_token.Line = _line;

			// number of lines crossed before token
			_token.LinesCrossed = _line - _lastLine;
			_token.Options = 0;

			char c = GetBufferCharacter(_scriptPosition);

			// if we're keeping everything as whitespace deliminated strings
			if((_options & LexerOptions.OnlyStrings) == LexerOptions.OnlyStrings)
			{
				// if there is a leading quote
				if((c == '"') || (c == '\''))
				{
					if(ReadString(_token, c) == false)
					{
						return null;
					}
				}
				else if(ReadName(_token) == false)
				{
					return null;
				}
			}
			// if there is a number
			else if(((c >= '0') && (c <= '9')) || ((c == '.') && ((GetBufferCharacter(_scriptPosition + 1) >= '0') && (GetBufferCharacter(_scriptPosition + 1) <= '9'))))
			{
				if(ReadNumber(_token) == false)
				{
					return null;
				}

				// if names are allowed to start with a number
				if((_options & LexerOptions.AllowNumberNames) != 0)
				{
					c = GetBufferCharacter(_scriptPosition);

					if(((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z')) || (c == '_'))
					{
						if(ReadName(_token) == false)
						{
						return null;
						}
					}
				}
			}
		
			// if there is a leading quote
			else if((c == '"') || (c == '\''))
			{
				if(ReadString(_token, c) == false)
				{
					return null;
				}
			}
			// if there is a name
			else if(((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z')) || (c == '_'))
			{
				if(ReadName(_token) == false)
				{
					return null;
				}
			}
			// names may also start with a slash when pathnames are allowed
			else if(((_options & LexerOptions.AllowPathNames) != 0) && ((c == '/') || (c == '\\') || (c == '.')))
			{
				if(ReadName(_token) == false)
				{
					return null;
				}
			}
			// check for punctuations
			else if(ReadPunctuation(_token) == false)
			{
				Error("unknown punctuation {0}", c);
				return null;
			}

			// succesfully read a token
			return _token;
		}

		/// <summary>
		/// Reads the token when a token with the given type is available.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="subType"></param>
		/// <returns></returns>
		public idToken CheckTokenType(TokenType type, TokenSubType subType)
		{
			idToken token;

			if((token = ReadToken()) == null)
			{
				return null;
			}

			// if the type matches
			if((token.Type == type) && ((token.SubType & subType) == subType))
			{
				return token;
			}

			// unread token
			_scriptPosition = _lastScriptPosition;
			_line = _lastLine;

			return null;
		}

		public void Error(string format, params object[] args)
		{
			_hadError = true;

			if((_options & LexerOptions.NoErrors) != 0)
			{
				return;
			}

			if((_options & LexerOptions.NoFatalErrors) != 0)
			{
				idConsole.Warning("file {0}, line {1}: {2}", _fileName, _line, string.Format(format, args));
			}
			else
			{
				idConsole.Error("file {0}, line {1}: {2}", _fileName, _line, string.Format(format, args));
			}
		}

		public void Warning(string format, params object[] args)
		{
			if((_options & LexerOptions.NoWarnings) != 0)
			{
				return;
			}

			idConsole.Warning("file {0}, line {1}: {2}", _fileName, _line, string.Format(format, args));
		}

		/// <summary>
		/// Skips until a matching close brace is found.
		/// </summary>
		/// <returns></returns>
		public bool SkipBracedSection()
		{
			return SkipBracedSection(true);
		}

		/// <summary>
		/// Skips until a matching close brace is found.
		/// </summary>
		/// <param name="parseFirstBrace"></param>
		/// <returns></returns>
		public bool SkipBracedSection(bool parseFirstBrace)
		{
			int depth = (parseFirstBrace == true) ? 0 : 1;
			idToken token;

			do
			{
				if((token = ReadToken()) == null)
				{
					return false;
				}

				if(token.Type == TokenType.Punctuation)
				{
					if(token.Value == "{")
					{
						depth++;
					}
					else if(token.Value == "}")
					{
						depth--;
					}
				}
			}
			while(depth > 0);

			return true;
		}

		public bool SkipUntilString(string str)
		{
			idToken token;

			while((token = ReadToken()) != null)
			{
				if(token.Value == str)
				{
					return true;
				}
			}

			return false;
		}
		#endregion

		#region Private
		/// <summary>
		/// Reads two strings with only a white space between them as one string.
		/// </summary>
		/// <param name="token"></param>
		/// <param name="quote"></param>
		/// <returns></returns>
		private bool ReadString(idToken token, char quote)
		{
			char ch;
			int tmpScriptPosition;
			int tmpLine;

			if(quote == '"')
			{
				token.Type = TokenType.String;
			}
			else
			{
				token.Type = TokenType.Literal;
			}

			// leading quote
			_scriptPosition++;

			while(true)
			{
				// if there is an escape character and escape characters are allowed.
				if((GetBufferCharacter(_scriptPosition) == '\\') && ((_options & LexerOptions.NoStringEscapeCharacters) == 0))
				{
					if(ReadEscapeCharacter(out ch) == false)
					{
						return false;
					}

					token.Value += ch;
				}
				// if a trailing quote
				else if(GetBufferCharacter(_scriptPosition) == quote)
				{
					// step over the quote
					_scriptPosition++;

					// if consecutive strings should not be concatenated
					if(((_options & LexerOptions.NoStringConcatination) != 0) && (((_options & LexerOptions.AllowBackslashStringConcatination) == 0) || (quote != '"')))
					{
						break;
					}
					
					tmpScriptPosition = _scriptPosition;
					tmpLine = _line;

					// read white space between possible two consecutive strings
					if(ReadWhiteSpace() == false)
					{
						_scriptPosition = tmpScriptPosition;
						_line = tmpLine;
						break;
					}

					if((_options & LexerOptions.NoStringConcatination) == LexerOptions.NoStringConcatination)
					{
						if(GetBufferCharacter(_scriptPosition) == '\\')
						{
							_scriptPosition = tmpScriptPosition;
							_line = tmpLine;
							break;
						}

						// step over the '\\'
						_scriptPosition++;

						if((ReadWhiteSpace() == false) || (GetBufferCharacter(_scriptPosition) != quote))
						{
							Error("expecting string after '\' terminated line");
							return false;
						}
					}
					
					// if there's no leading qoute
					if(GetBufferCharacter(_scriptPosition) != quote)
					{
						_scriptPosition = tmpScriptPosition;
						_line = tmpLine;
						break;
					}

					// step over the new leading quote
					_scriptPosition++;
				}
				else 
				{
					if(GetBufferCharacter(_scriptPosition) == '\0')
					{
						Error("missing trailing quote");
						return false;
					}

					if(GetBufferCharacter(_scriptPosition) == '\n')
					{
						Error("newline inside string");
						return false;
					}

					token.Value += GetBufferCharacter(_scriptPosition++);
				}
			}

			if(token.Type == TokenType.Literal)
			{
				if((_options & LexerOptions.AllowMultiCharacterLiterals) == 0)
				{
					if(token.Value.Length != 1)
					{
						Warning("literal is not one character long");
					}
				}

				token.SubType = (TokenSubType) token.Value[0];
			}
			else
			{
				// the sub type is the length of the string
				token.SubType = (TokenSubType) token.Value.Length;
			}

			return true;
		}

		private bool ReadName(idToken token)
		{
			char c;
			token.Type = TokenType.Name;
			
			do
			{
				token.Value += GetBufferCharacter(_scriptPosition++);
				c = GetBufferCharacter(_scriptPosition);
			}
			while(((c >= 'a') && (c <= 'z'))
			|| ((c >= 'A') && (c <= 'Z'))
			|| ((c >= '0') && (c <= '9'))
			|| (c == '_')
				// if treating all tokens as strings, don't parse '-' as a seperate token
			|| (((_options & LexerOptions.OnlyStrings) != 0) && (c == '-'))
				// if special path name characters are allowed
			|| (((_options & LexerOptions.AllowPathNames) != 0) && ((c == '/') || (c == '\\') || (c == ':') || (c == '.'))));

			//the sub type is the length of the name
			token.SubType = (TokenSubType) token.Value.Length;

			return true;
		}

		/// <summary>
		/// Reads spaces, tabs, C-like comments, etc.
		/// </summary>
		/// <remarks>
		/// When a newline character is found the scripts line counter is increased.
		/// </remarks>
		/// <returns></returns>
		private bool ReadWhiteSpace()
		{
			char c;
			
			while(true)
			{
				// skip white space
				while((c = GetBufferCharacter(_scriptPosition)) <= ' ')
				{
					if(c == '\0')
					{
						return false;
					}

					if(c == '\n')
					{
						_line++;
					}

					_scriptPosition++;
				}
				
				// skip comments
				if(GetBufferCharacter(_scriptPosition) == '/')
				{
					// comments //
					if(GetBufferCharacter(_scriptPosition + 1) == '/')
					{
						_scriptPosition++;

						do
						{
							_scriptPosition++;

							if(GetBufferCharacter(_scriptPosition) == '\0')
							{
								return false;
							}
						}
						while(GetBufferCharacter(_scriptPosition) != '\n');

						_line++;
						_scriptPosition++;

						if(GetBufferCharacter(_scriptPosition) == '\0')
						{
							return false;
						}

						continue;
					}
					// comments /* */
					else if(GetBufferCharacter(_scriptPosition + 1) == '*')
					{
						_scriptPosition++;

						while(true)
						{
							_scriptPosition++;

							if(GetBufferCharacter(_scriptPosition) == '\0')
							{
								return false;
							}

							if(GetBufferCharacter(_scriptPosition) == '\n')
							{
								_line++;
							}
							else if(GetBufferCharacter(_scriptPosition) == '/')
							{
								if(GetBufferCharacter(_scriptPosition - 1) == '*')
								{
									break;
								}

								if(GetBufferCharacter(_scriptPosition + 1) == '*')
								{
									Warning("nested comment");
								}
							}
						}

						_scriptPosition += 2;

						if(GetBufferCharacter(_scriptPosition) == '\0')
						{
							return false;
						}

						continue;
					}
				}

				break;
			}

			return true;
		}

		private bool ReadNumber(idToken token)
		{
			token.Type = TokenType.Number;
			token.SubType = 0;
			token.IntValue = 0;
			token.FloatValue = 0;

			char c = GetBufferCharacter(_scriptPosition);
			char c2 = GetBufferCharacter(_scriptPosition + 1);

			if((c == '0') && (c2 != '.'))
			{
				if((c2 == 'x') || (c2 == 'X'))
				{
					token.Value += GetBufferCharacter(_scriptPosition++);
					token.Value += GetBufferCharacter(_scriptPosition++);

					c = GetBufferCharacter(_scriptPosition);

					while(((c >= 0) && (c <= '9')) || ((c >= 'a') && (c <= 'f')) || ((c >= 'A') && (c <= 'F')))
					{
						token.Value += c;
						c = GetBufferCharacter(++_scriptPosition);
					}

					token.SubType = TokenSubType.Hex | TokenSubType.Integer;
				}
				// check for a binary number
				else if((c2 == 'b') || (c2 == 'B'))
				{
					token.Value += GetBufferCharacter(_scriptPosition++);
					token.Value += GetBufferCharacter(_scriptPosition++);

					c = GetBufferCharacter(_scriptPosition);

					while((c == '0') || (c == '1'))
					{
						token.Value += c;
						c = GetBufferCharacter(++_scriptPosition);
					}

					token.SubType = TokenSubType.Binary | TokenSubType.Integer;
				}
				// its an octal number
				else
				{
					token.Value += GetBufferCharacter(_scriptPosition++);
					c = GetBufferCharacter(_scriptPosition);

					while((c >= '0') && (c <= '7'))
					{
						token.Value += c;
						c = GetBufferCharacter(++_scriptPosition);
					}

					token.SubType = TokenSubType.Octal | TokenSubType.Integer;
				}
			}
			else
			{
				// decimal integer or floating point number or ip address
				int dot = 0;

				while(true)
				{
					if((c >= '0') && (c <= '9'))
					{

					}
					else if(c == '.')
					{
						dot++;
					}
					else
					{
						break;
					}

					token.Value += c;
					c = GetBufferCharacter(++_scriptPosition);
				}

				if((c == 'e') && (dot == 0))
				{
					//We have scientific notation without a decimal point
					dot++;
				}

				// if a floating point number
				if(dot == 1)
				{
					token.SubType = TokenSubType.Decimal | TokenSubType.Float;

					// check for floating point exponent
					if(c == 'e')
					{
						//Append the e so that GetFloatValue code works
						token.Value += c;
						c = GetBufferCharacter(++_scriptPosition);

						if((c == '-') || (c == '+'))
						{
							token.Value += c;
							c = GetBufferCharacter(++_scriptPosition);
						}

						while((c >= '0') || (c <= '9'))
						{
							token.Value += c;
							c = GetBufferCharacter(++_scriptPosition);
						}
					}
					// check for floating point exception infinite 1.#INF or indefinite 1.#IND or NaN
					else if(c == '#')
					{
						c2 = (char) 4;

						if(CheckString("INF") == true)
						{
							token.SubType |= TokenSubType.Infinite;
						}
						else if(CheckString("IND") == true)
						{
							token.SubType |= TokenSubType.Indefinite;
						}
						else if(CheckString("NAN") == true)
						{
							token.SubType |= TokenSubType.NaN;
						}
						else if(CheckString("QNAN") == true)
						{
							token.SubType |= TokenSubType.NaN;
							c2++;
						}

						for(int i = 0; i < c2; i++)
						{
							token.Value += c;
							c = GetBufferCharacter(++_scriptPosition);
						}

						while((c >= '0') && (c <= '9'))
						{
							token.Value += c;
							c = GetBufferCharacter(++_scriptPosition);
						}

						if((_options & LexerOptions.AllowFloatExceptions) == 0)
						{
							Error("parsed {0}", token);
						}
					}
				}
				else if(dot > 1)
				{
					if((_options & LexerOptions.AllowIPAddresses) == 0)
					{
						Error("more than one dot in number");
						return false;
					}

					if(dot != 3)
					{
						Error("ip address should have three dots");
						return false;
					}

					token.SubType = TokenSubType.IPAddress;
				}
				else
				{
					token.SubType = TokenSubType.Decimal | TokenSubType.Integer;
				}
			}

			if((token.SubType & TokenSubType.Float) != 0)
			{
				if(c > ' ')
				{
					// single-precision: float
					if((c == 'f') || (c == 'F'))
					{
						token.SubType |= TokenSubType.SinglePrecision;
						_scriptPosition++;
					}
					// extended-precision: long double
					else if((c == 'l') || (c == 'L'))
					{
						token.SubType |= TokenSubType.ExtendedPrecision;
						_scriptPosition++;
					}
					// default is double-precision: double
					else
					{
						_token.SubType |= TokenSubType.DoublePrecision;
					}
				}
				else
				{
					token.SubType |= TokenSubType.DoublePrecision;
				}
			}
			else if((token.SubType & TokenSubType.Integer) != 0)
			{
				if(c > ' ')
				{
					// default: signed long
					for(int i = 0; i < 2; i++)
					{
						// long integer
						if((c == 'l') || (c == 'L'))
						{
							token.SubType |= TokenSubType.Long;
						}
						// unsigned integer
						else if((c == 'u') || (c == 'U'))
						{
							token.SubType |= TokenSubType.Unsigned;
						}
						else
						{
							break;
						}

						c = GetBufferCharacter(++_scriptPosition);

					}
				}
			}
			else if((token.SubType & TokenSubType.IPAddress) != 0)
			{
				if(c == ':')
				{
					token.Value += c;
					c = GetBufferCharacter(++_scriptPosition);

					while((c >= '0') && (c <= '9'))
					{
						token.Value += c;
						c = GetBufferCharacter(++_scriptPosition);
					}

					token.SubType |= TokenSubType.IPPort;
				}
			}

			return true;
		}

		private bool ReadPunctuation(idToken token)
		{
			int i, l;
			string p;
			LexerPunctuation punc;

			// TODO
			/*#ifdef PUNCTABLE
			for (n = idLexer::punctuationtable[(unsigned int)*(idLexer::script_p)]; n >= 0; n = idLexer::nextpunctuation[n])
			{
				punc = &(idLexer::punctuations[n]);
			#else*/

			for(i = 0; i < _punctuation.Length; i++)
			{
				punc = _punctuation[i];

				/*#endif*/

				p = punc.P;

				// check for this punctuation in the script
				// TODO: && idLexer::script_p[l]
				for(l = 0; ((l < p.Length) && ((_scriptPosition + l) < _buffer.Length)); l++)
				{
					if(GetBufferCharacter(_scriptPosition + l) != p[l])
					{
						break;
					}
				}

				if(l >= p.Length)
				{
					StringBuilder sb = new StringBuilder(token.Value);

					for(i = 0; i < l; i++)
					{
						sb.Append(p[i]);
					}

					token.Value = sb.ToString();

					_scriptPosition += l;

					token.Type = TokenType.Punctuation;
					token.SubType = (TokenSubType) punc.N;

					return true;
				}
			}

			return false;
		}

		private bool ReadEscapeCharacter(out char escapeCharacter)
		{
			char c;
			int i, val;

			// step over the leading '\\'
			_scriptPosition++;

			// determine the escape character
			switch(GetBufferCharacter(_scriptPosition))
			{
				case '\\': c = '\\'; break;
				case 'n': c = '\n'; break;
				case 'r': c = '\r'; break;
				case 't': c = '\t'; break;
				case 'v': c = '\v'; break;
				case 'b': c = '\b'; break;
				case 'f': c = '\f'; break;
				case 'a': c = '\a'; break;
				case '\'': c = '\''; break;
				case '\"': c = '\"'; break;
				//case '\?': c = '\?'; break;
				case 'x': 
					{
						_scriptPosition++;

						for(i = 0, val = 0; ; i++, _scriptPosition++)
						{
							c = GetBufferCharacter(_scriptPosition);

							if((c >= '0') && (c <= '9'))
							{
								c = (char) (c - '0');
							}
							else if((c >= 'A') && (c <= 'Z'))
							{
								c = (char) (c - 'A' + 10);
							}
							else if((c >= 'a') && (c <= 'z'))
							{
								c = (char) (c - 'a' + 10);
							}
							else
							{
								break;
							}

							val = (val << 4) + c;
						}

						_scriptPosition--;

						if(val > 0xFF)
						{
							Warning("too large value in escape character");
							val = 0xFF;
						}

						c = (char) val;
						break;
					}
				default: //NOTE: decimal ASCII code, NOT octal
					{
						if((GetBufferCharacter(_scriptPosition) < '0') || (GetBufferCharacter(_scriptPosition) > '9'))
						{
							Error("unknown escape char");
						}

						for(i = 0, val = 0; ; i++, _scriptPosition++)
						{
							c = GetBufferCharacter(_scriptPosition);

							if((c >= '0') && (c <= '9'))
							{
								c = (char) (c - '0');
							}
							else
							{
								break;
							}

							val = val * 10 + c;
						}

						_scriptPosition--;

						if(val > 0xFF)
						{
							Warning("too large value in escape character");
							val = 0xFF;
						}
	
						c = (char) val;
						break;
					}
				}

			// step over the escape character or the last digit of the number
			_scriptPosition++;

			// store the escape character
			escapeCharacter = c;

			// succesfully read escape character
			return true;
		}

		private bool CheckString(string str)
		{
			for(int i = 0; i < str.Length; i++)
			{
				if(GetBufferCharacter(_scriptPosition + i) != str[i])
				{
					return false;
				}
			}

			return true;
		}

		private char GetBufferCharacter(int position)
		{
			if((position < 0) || (position >= _buffer.Length))
			{
				return '\0';
			}

			return _buffer[position];
		}
		#endregion
		#endregion
	}
}