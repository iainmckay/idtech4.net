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

namespace idTech4.Text
{
	public sealed class idScriptParser
	{
		#region Properties
		public bool IsLoaded
		{
			get
			{
				return _loaded;
			}
		}
		#endregion

		#region Members
		private LexerOptions _options;

		private bool _loaded;										// set when a source file is loaded from file or memory
		private string _fileName;									// file name of the script
		private string _includePath;								// path to include files

		private bool _osPath;										// true if the file was loaded from an OS path

		private Stack<idLexer> _scriptStack;						// stack with scripts of the source
		private Stack<idToken> _tokens;								// tokens to read first
		private List<ScriptDefinition> _defines;					// list with macro definitions
		private Dictionary<string, ScriptDefinition> _defineDict;
		private Stack<ScriptIndentation> _indentStack;				// stack with indents
		
		private int _skip;											// > 0 if skipping conditional code
		private int _markerPosition;

		private LexerPunctuation[] _punctuation;

		// global defines
		private static Dictionary<string, ScriptDefinition> _globalDefines = new Dictionary<string, ScriptDefinition>();
		#endregion

		#region Constructor
		public idScriptParser(LexerOptions options)
		{
			_options = options;
			_markerPosition = -1;
			_scriptStack = new Stack<idLexer>();
			_tokens = new Stack<idToken>();
			_defines = new List<ScriptDefinition>();
			_indentStack = new Stack<ScriptIndentation>();
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Returns true if the next token equals the given string and removes the token from the source.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public bool CheckTokenString(string str)
		{
			idToken token;

			if((token = ReadToken()) == null)
			{
				return false;
			}

			// if the token is available
			if(token.ToString() == str)
			{
				return true;
			}

			UnreadSourceToken(token);

			return false;
		}

		public void Error(string format, params object[] args)
		{
			if(_scriptStack.Count > 0)
			{
				_scriptStack.Peek().Error(format, args);
			}
		}

		/// <summary>
		/// Load the given source.
		/// </summary>
		/// <returns></returns>
		public bool LoadMemory(string content, string name)
		{
			if(_loaded == true)
			{
				idConsole.FatalError("idScriptParser::LoadMemory: another source already loaded");
				return false;
			}

			idLexer script = new idLexer(_options);
			script.LoadMemory(content, name);

			if(script.IsLoaded == false)
			{
				return false;
			}

			_fileName = name;

			_scriptStack.Clear();
			_indentStack.Clear();
			_tokens.Clear();

			_skip = 0;
			_loaded = true;

			if(_defineDict == null)
			{
				_defines.Clear();
				_defineDict = new Dictionary<string, ScriptDefinition>();

				AddGlobalDefinesToSource();
			}

			return true;
		}

		public idToken ReadToken()
		{
			idToken token;
			ScriptDefinition define;

			while(true)
			{
				if((token = ReadSourceToken()) == null)
				{
					return null;
				}

				// check for precompiler directives
				if((token.Type == TokenType.Punctuation) && (token.ToString() == "#"))
				{
					// read the precompiler directive
					if(ReadDirective() == false)
					{
						return null;
					}

					continue;
				}

				// if skipping source because of conditional compilation
				if(_skip > 0)
				{
					continue;
				}

				// recursively concatenate strings that are behind each other still resolving defines
				if((token.Type == TokenType.String) && ((_scriptStack.Peek().Options & LexerOptions.NoStringConcatination) == 0))
				{
					idToken newToken = ReadToken();

					if(newToken != null)
					{
						if(newToken.Type == TokenType.String)
						{
							token.Append(newToken.ToString());
						}
						else
						{
							UnreadSourceToken(newToken);
						}
					}
				}

				if((_scriptStack.Peek().Options & LexerOptions.NoDollarPrecompilation) == 0)
				{
					// check for special precompiler directives
					if((token.Type == TokenType.Punctuation) && (token.ToString() == "$"))
					{
						// read the precompiler directive
						if(ReadDollarDirective() == true)
						{
							continue;
						}
					}
				}

				// if the token is a name
				if((token.Type == TokenType.Name) && ((token.Flags & TokenFlags.RecursiveDefine) != 0))
				{
					// check if the name is a define macro
					if(_defineDict.ContainsKey(token.ToString()) == true)
					{
						// expand the defined macro
						// TODO
						/*if(ExpandDefineIntoSource(token, define) == false)
						{
							return null;
						}*/

						continue;
					}
				}

				// found a token
				return token;
			}
		}

		public void Warning(string format, params object[] args)
		{
			if(_scriptStack.Count > 0)
			{
				_scriptStack.Peek().Warning(format, args);
			}
		}
		#endregion

		#region Private
		private void AddDefineToHash(ScriptDefinition define, Dictionary<string, ScriptDefinition> dict)
		{
			dict.Add(define.Name, define);
		}

		private void AddGlobalDefinesToSource()
		{
			foreach(KeyValuePair<string, ScriptDefinition> kvp in _globalDefines)
			{
				ScriptDefinition newDefine = CopyDefine(kvp.Value);
				AddDefineToHash(newDefine, _defineDict);
			}
		}

		private ScriptDefinition CopyDefine(ScriptDefinition define)
		{
			ScriptDefinition newDefine = new Text.ScriptDefinition();
			newDefine.Name = define.Name;
			newDefine.Flags = define.Flags;
			newDefine.BuiltIn = define.BuiltIn;

			List<idToken> tokens = new List<idToken>();
			List<idToken> parameters = new List<idToken>();

			// copy the define tokens
			newDefine.Tokens = new idToken[define.Tokens.Length];

			foreach(idToken token in define.Tokens)
			{
				tokens.Add(new idToken(token));
			}

			// copy the define parameters
			foreach(idToken token in define.Parameters)
			{
				parameters.Add(new idToken(token));
			}

			newDefine.Tokens = tokens.ToArray();
			newDefine.Parameters = parameters.ToArray();

			return newDefine;
		}

		private bool Directive_Define()
		{
			idToken token, t;
			ScriptDefinition define;

			if((token = ReadLine()) == null)
			{
				Error("#define without name");
				return false;
			}
			else if(token.Type != TokenType.Name)
			{
				UnreadSourceToken(token);
				Error("expected name after #define, found '{0}'", token.ToString());

				return false;
			}

			// check if the define already exists
			if(_defineDict.ContainsKey(token.ToString()) == true)
			{
				define = _defineDict[token.ToString()];

				if((define.Flags & DefineFlags.Fixed) == DefineFlags.Fixed)
				{
					Error("can't redefine '{0}'", token.ToString());
					return false;
				}

				Warning("redefinition of '{0}'", token.ToString());

				// unread the define name before executing the #undef directive
				UnreadSourceToken(token);

				if(Directive_UnDefine() == false)
				{
					return false;
				}

				// if the define was not removed (define->flags & DEFINE_FIXED)
				define = _defineDict[token.ToString()];
			}

			// allocate define
			define = new ScriptDefinition();
			define.Name = token.ToString();
			define.Parameters = new idToken[] { };
			define.Tokens = new idToken[] { };

			// add the define to the source
			AddDefineToHash(define, _defineDict);

			// if nothing is defined, just return
			if((token = ReadLine()) == null)
			{
				return true;
			}


			// if it is a define with parameters
			if((token.WhiteSpaceBeforeToken == 0) && (token.ToString() == "("))
			{
				List<idToken> parameters = new List<idToken>();

				// read the define parameters
				if(CheckTokenString(")") == false)
				{
					while(true)
					{
						if((token = ReadLine()) == null)
						{
							Error("expected define parameter");
							return false;
						}
						// if it isn't a name
						else if(token.Type != TokenType.Name)
						{
							Error("invalid define parameter");
							return false;
						}
						// TODO
						/*else if(FindDefineParameter(define, token.ToString()) >= 0)
						{
							Error("two of the same define parameters");
							return false;
						}*/

						// add the define parm
						t = new idToken(token);
						t.ClearTokenWhiteSpace();

						parameters.Add(t);

						// read next token
						if((token = ReadLine()) == null)
						{
							Error("define parameters not terminated");
							return false;
						}

						if(token.ToString() == ")")
						{
							break;
						}

						// then it must be a comma
						if(token.ToString() != ",")
						{
							Error("define not terminated");
							return false;
						}
					}
				}

				define.Parameters = parameters.ToArray();

				if((token = ReadLine()) == null)
				{
					return true;
				}
			}

			List<idToken> tokens = new List<idToken>();

			do
			{
				t = new idToken(token);

				if((t.Type == TokenType.Name) && (t.ToString() == define.Name))
				{
					t.Flags |= TokenFlags.RecursiveDefine;
					Warning("recursive define (removed recursion)");
				}

				t.ClearTokenWhiteSpace();

				tokens.Add(t);
			}
			while((token = ReadLine()) != null);

			define.Tokens = tokens.ToArray();

			if(define.Tokens.Length > 0)
			{
				// check for merge operators at the beginning or end
				if((define.Tokens[0].ToString() == "##") || (define.Tokens[define.Tokens.Length - 1].ToString() == "##"))
				{
					Error("define with misplaced ##");
					return false;
				}
			}

			return true;
		}

		private bool Directive_Else()
		{
			IndentType type;
			int skip;

			PopIndent(out type, out skip);

			if(type == 0)
			{
				Error("misplaced #else");
			}
			else if(type == IndentType.Else)
			{
				Error("#else after #else");
			}
			else
			{
				PushIndent(IndentType.Else, skip);
				return true;
			}

			return false;
		}

		private bool Directive_EndIf()
		{
			IndentType type;
			int skip;

			PopIndent(out type, out skip);

			if(type == 0)
			{
				Error("misplaced #endif");
				return false;
			}

			return true;
		}

		private bool Directive_ElseIf()
		{
			long value;
			double tmp;
			int skip;
			IndentType type;

			PopIndent(out type, out skip);

			if((type == 0) || (type == IndentType.Else))
			{
				Error("misplaced #elif");
			}
			else if(Evaluate(out value, out tmp, true) == false)
			{

			}
			else
			{
				skip = (value == 0) ? 1 : 0;

				PushIndent(IndentType.ElseIf, skip);

				return true;
			}

			return false;
		}

		private bool Directive_Error()
		{
			idToken token;

			if(((token = ReadLine()) == null) || (token.Type != TokenType.String))
			{
				Error("#error without string");
				return false;
			}

			Error("#error: {0}", token.ToString());

			return true;
		}

		private bool Directive_Eval()
		{
			long value;
			double tmp;

			if(Evaluate(out value, out tmp, true) == false)
			{
				return false;
			}

			idLexer script = _scriptStack.Peek();
			idToken token = new idToken();
			token.Line = script.LineNumber;
			token.Append(value.ToString());
			token.Type = TokenType.Number;
			token.SubType = TokenSubType.Integer | TokenSubType.Long | TokenSubType.Decimal;

			UnreadSourceToken(token);

			if(value < 0)
			{
				UnreadSignToken();
			}

			return true;
		}

		private bool Directive_EvalFloat()
		{
			double value;
			long tmp;

			if(Evaluate(out tmp, out value, false) == false)
			{
				return false;
			}

			idLexer script = _scriptStack.Peek();
			idToken token = new idToken();
			token.Line = script.LineNumber;
			token.Append(idMath.Abs((float) value).ToString("00"));
			token.Type = TokenType.Number;
			token.SubType = TokenSubType.Float | TokenSubType.Long | TokenSubType.Decimal;

			UnreadSourceToken(token);

			if(value < 0)
			{
				UnreadSignToken();
			}

			return true;
		}

		private bool Directive_If()
		{
			long value;
			double tmp;
			int skip;

			if(Evaluate(out value, out tmp, true) == false)
			{
				return false;
			}

			skip = (value == 0) ? 1 : 0;

			PushIndent(IndentType.If, skip);

			return true;
		}

		private bool Directive_IfDefined()
		{
			return Directive_IfDefinedActual(IndentType.IfDefined);
		}

		private bool Directive_IfDefinedActual(IndentType type)
		{
			idToken token;
			int skip;

			if((token = ReadLine()) == null)
			{
				Error("#ifdef without name");
			}
			else if(token.Type != TokenType.Name)
			{
				UnreadSourceToken(token);
				Error("expected name after #ifdef, found '{0}'", token.ToString());
			}
			else
			{
				if(_defineDict.ContainsKey(token.ToString()) == true)
				{
					skip = ((type == IndentType.IfDefined) == false) ? 1 : 0;
				}
				else
				{
					skip = ((type == IndentType.IfDefined) == true) ? 1 : 0;
				}

				PushIndent(type, skip);

				return true;
			}

			return false;
		}

		private bool Directive_IfNotDefined()
		{
			return Directive_IfDefinedActual(IndentType.IfNotDefined);
		}

		private bool Directive_Include()
		{
			idLexer script;
			idToken token;
			string path;

			if((token = ReadSourceToken()) == null)
			{
				Error("#include without file name");
				return false;
			}
			else if(token.LinesCrossed > 0)
			{
				Error("#include without file name");
				return false;
			}
			else if(token.Type == TokenType.String)
			{
				script = new idLexer();

				// try relative to the current file

				// TODO: replace with something like Path.Combine.
				path = _scriptStack.Peek().FileName;
				path += "/";
				path += token.ToString();

				throw new Exception("XX");
				/*if(script.LoadFile(path, _osPath) == false)
				{
					// try absolute path
					path = token.ToString();

					if(script.LoadFile(path, _osPath) == false)
					{
						// try from the include path
						path = _includePath + token.ToString();

						if(script.LoadFile(path, _osPath) == false)
						{
							script = null;
						}
					}
				}*/
			}
			else if((token.Type == TokenType.Punctuation) && (token.ToString() == "<"))
			{
				path = _includePath;

				while((token = ReadSourceToken()) != null)
				{
					if(token.LinesCrossed > 0)
					{
						UnreadSourceToken(token);
						break;
					}
					else if((token.Type == TokenType.Punctuation) && (token.ToString() == ">"))
					{
						break;
					}

					path += token.ToString();
				}

				if(token.ToString() != ">")
				{
					Warning("#include missing trailing >");
				}
				else if(path == string.Empty)
				{
					Error("#include without file name between < >");
					return false;
				}
				else if((_options & LexerOptions.NoBaseIncludes) == LexerOptions.NoBaseIncludes)
				{
					return true;
				}

				script = new idLexer();

				throw new Exception("ZZ");
				/*if(script.LoadFile(_includePath + path, _osPath) == false)
				{
					script = null;
				}*/
			}
			else
			{
				Error("#include without file name");
				return false;
			}
			
			if(script == null)
			{
				Error("file '{0}' not found", path);
				return false;
			}

			script.Options = _options;
			script.Punctuation = _punctuation;

			PushScript(script);

			return true;
		}

		private bool Directive_Line()
		{
			idToken token;

			Error("#line directive not supported");

			while((token = ReadLine()) != null)
			{

			}

			return true;
		}

		private bool Directive_Pragma()
		{
			idToken token;

			Warning("#pragma directive not supported");

			while((token = ReadLine()) != null)
			{

			}

			return true;
		}

		private bool Directive_UnDefine()
		{
			idToken token;

			if((token = ReadLine()) == null)
			{
				Error("undef without name");
				return false;
			}
			else if(token.Type != TokenType.Name)
			{
				UnreadSourceToken(token);
				Error("expected name but found '{0}'", token.ToString());

				return false;
			}

			throw new Exception("NOT IMPLEMENTED, GOT BORED!");

			/*hash = PC_NameHash( token.c_str() );
			for (lastdefine = NULL, define = idParser::definehash[hash]; define; define = define->hashnext) {
				if (!strcmp(define->name, token.c_str()))
				{
					if (define->flags & DEFINE_FIXED) {
						idParser::Warning( "can't undef '%s'", token.c_str() );
					}
					else {
						if (lastdefine) {
							lastdefine->hashnext = define->hashnext;
						}
						else {
							idParser::definehash[hash] = define->hashnext;
						}
						FreeDefine(define);
					}
					break;
				}
				lastdefine = define;
			}
			return true;*/
		}

		private bool Directive_Warning()
		{
			idToken token;

			if(((token = ReadLine()) == null) || (token.Type != TokenType.String))
			{
				Warning("#warning without string");
				return false;
			}

			Warning("#warning: {0}", token.ToString());

			return true;
		}

		private bool DollarDirective_EvalFloat()
		{
			double value;
			long tmp;

			if(DollarEvaluate(out tmp, out value, false) == false)
			{
				return false;
			}

			idToken token = new idToken();
			token.Line = _scriptStack.Peek().LineNumber;
			token.Set(idMath.Abs((float) value).ToString("00"));
			token.Type = TokenType.Number;
			token.SubType = TokenSubType.Float | TokenSubType.Long | TokenSubType.Decimal | TokenSubType.ValuesValid;
			token.SetInteger((ulong) idMath.Abs((float) value));

			UnreadSourceToken(token);

			if(value < 0)
			{
				UnreadSignToken();
			}

			return true;
		}

		private bool DollarDirective_EvalInt()
		{
			long value;
			double tmp;

			if(DollarEvaluate(out value, out tmp, true) == false)
			{
				return false;
			}

			idToken token = new idToken();
			token.Line = _scriptStack.Peek().LineNumber;
			token.Set(value.ToString());
			token.Type = TokenType.Number;
			token.SubType = TokenSubType.Integer | TokenSubType.Long | TokenSubType.Decimal | TokenSubType.ValuesValid;
			token.SetInteger((ulong) idMath.Abs(value));
			token.SetFloat((ulong) idMath.Abs(value));

			UnreadSourceToken(token);

			if(value < 0)
			{
				UnreadSignToken();
			}

			return true;
		}

		private bool DollarEvaluate(out long intValue, out double floatValue, bool integer)
		{
			int indent;
			bool defined = false;
			idToken token, t;
			ScriptDefinition define;
			List<idToken> tokens = new List<idToken>();

			intValue = 0;
			floatValue = 0;

			if((token = ReadSourceToken()) == null)
			{
				Error("no leading (after $evalint/$evalfloat");
				return false;
			}
			else if((token = ReadSourceToken()) == null)
			{
				Error("nothing to evaluate");
				return false;
			}

			indent = 1;

			do 
			{
				// if the token is a name
				if(token.Type == TokenType.Name)
				{
					if(defined == true)
					{
						defined = false;
						tokens.Add(new idToken(token));
					}
					else if(token.ToString() == "defined")
					{
						defined = true;
						tokens.Add(new idToken(token));
					}
					else
					{
						// then it must be a define
						if(_defineDict.ContainsKey(token.ToString()) == false)
						{
							Warning("can't evaluate '{0}', not defined", token.ToString());
							return false;
						}
						// TODO
						/*else if(ExpandDefineIntoSource(token, define) == false)
						{
							return false;
						}*/
					}
				}
				// if the token is a number or a punctuation
				else if((token.Type == TokenType.Number) || (token.Type == TokenType.Punctuation))
				{
					if(token.ToString().StartsWith("(") == true)
					{
						indent++;
					}
					else if(token.ToString().StartsWith(")") == true)
					{
						indent--;
					}

						if(indent <= 0)
						{
							break;
						}

					tokens.Add(new idToken(token));
				}
				else
				{
					Error("can't evaluate '{0}'", token.ToString());
					return false;
				}
			}
			while((token = ReadSourceToken()) != null);

			// TODO
			/*if(EvaluateTokens(tokens.ToArray(), ref intValue, ref floatValue, integer) == false)
			{
				return false;
			}*/

			return true;
		}

		private bool Evaluate(out long intValue, out double floatValue, bool integer)
		{
			idToken token, t;
			List<idToken> tokens = new List<idToken>();
			ScriptDefinition define;
			bool defined = false;
			
			intValue = 0;
			floatValue = 0;

			if((token = ReadLine()) == null)
			{
				Error("no value after #if/#elif");
				return false;
			}

			do
			{
				// if the token is a name
				if(token.Type == TokenType.Name)
				{
					if(defined == true)
					{
						defined = false;
						tokens.Add(new idToken(token));
					}
					else if(token.ToString() == "defined")
					{
						defined = true;
						tokens.Add(new idToken(token));
					}
					else
					{
						// then it must be a define
						if(_defineDict.ContainsKey(token.ToString()) == false)
						{
							Error("can't evaluate '{0}', not defined", token.ToString());
							return false;
						}
						// TODO
						/*else if(ExpandDefineIntoSource(token, _defineDict[token.ToString()]) == false)
						{
							return false;
						}*/
					}
				}
				// if the token is a number or a punctuation
				else if((token.Type == TokenType.Number) || (token.Type == TokenType.Punctuation))
				{
					tokens.Add(new idToken(token));
				}
				else
				{
					Error("can't evaluate '{0}'", token.ToString());
				}
			}
			while((token = ReadToken()) != null);

			// TODO
			/*if(EvaluateTokens(tokens.ToArray(), ref intValue, ref floatValue, integer) == false)
			{
				return false;
			}*/

			return true;
		}
	
		/*private bool ExpandDefineIntoSource(idToken token, ScriptDefinition define)
		{

	idToken *firsttoken, *lasttoken;

	if ( !idParser::ExpandDefine( deftoken, define, &firsttoken, &lasttoken ) ) {
		return false;
	}
	// if the define is not empty
	if ( firsttoken && lasttoken ) {
		firsttoken->linesCrossed += deftoken->linesCrossed;
		lasttoken->next = idParser::tokens;
		idParser::tokens = firsttoken;
	}
	return true;
}*/

		private void PopIndent(out IndentType type, out int skip)
		{
			type = 0;
			skip = 0;

			if(_indentStack.Count == 0)
			{
				return;
			}

			ScriptIndentation indent = _indentStack.Peek();

			// must be an indent from the current script
			if(indent.Script != _scriptStack.Peek())
			{
				return;
			}

			type = indent.Type;
			skip = indent.Skip;

			_indentStack.Pop();
			_skip -= indent.Skip;
		}

		private void PushIndent(IndentType type, int skip)
		{
			ScriptIndentation indent = new ScriptIndentation();
			indent.Type = type;
			indent.Script = _scriptStack.Peek();
			indent.Skip = (skip != 0) ? 1 : 0;

			_skip += indent.Skip;
			_indentStack.Push(indent);
		}

		private void PushScript(idLexer script)
		{
			foreach(idLexer s in _scriptStack)
			{
				if(s.FileName.Equals(script.FileName, StringComparison.OrdinalIgnoreCase) == true)
				{
					Warning("'{0}' recursively included", script.FileName);
					return;
				}
			}

			// push the script on the script stack
			_scriptStack.Push(script);
		}

		private bool ReadDirective()
		{
			idToken token;

			// read the directive name
			if((token = ReadSourceToken()) == null)
			{
				Error("found '#' without name");
				return false;
			}
			// directive name must be on the same line
			else if(token.LinesCrossed > 0)
			{
				UnreadSourceToken(token);
				Error("found '#' at end of line");

				return false;
			}
			// if it is a name
			else if(token.Type == TokenType.Name)
			{
				if(token.ToString() == "if")
				{
					return Directive_If();
				}
				else if(token.ToString() == "ifdef")
				{
					return Directive_IfDefined();
				}
				else if(token.ToString() == "ifndef")
				{
					return Directive_IfNotDefined();
				}
				else if(token.ToString() == "elif")
				{
					return Directive_ElseIf();
				}
				else if(token.ToString() == "else")
				{
					return Directive_Else();
				}
				else if(token.ToString() == "endif")
				{
					return Directive_EndIf();
				}
				else if(_skip > 0)
				{
					// skip the rest of the line
					while((token = ReadLine()) != null)
					{

					}

					return true;
				}
				else
				{
					if(token.ToString() == "include")
					{
						return Directive_Include();
					}
					else if(token.ToString() == "define")
					{
						return Directive_Define();
					}
					else if(token.ToString() == "undef")
					{
						return Directive_UnDefine();
					}
					else if(token.ToString() == "line")
					{
						return Directive_Line();
					}
					else if(token.ToString() == "error")
					{
						return Directive_Error();
					}
					else if(token.ToString() == "warning")
					{
						return Directive_Warning();
					}
					else if(token.ToString() == "pragma")
					{
						return Directive_Pragma();
					}
					else if(token.ToString() == "eval")
					{
						return Directive_Eval();
					}
					else if(token.ToString() == "evalfloat")
					{
						return Directive_EvalFloat();
					}
				}
			}

			Error("unknown precompiler directive '{0}'", token.ToString());

			return false;
		}

		private bool ReadDollarDirective()
		{
			idToken token;

			// read the directive name
			if((token = ReadSourceToken()) == null)
			{
				Error("found '$' without name");
				return false;
			}
			// directive name must be on the same line
			else if(token.LinesCrossed > 0)
			{
				UnreadSourceToken(token);
				Error("found '$' at end of line");

				return false;
			}
			// if if is a name
			else if(token.Type == TokenType.Name)
			{
				if(token.ToString() == "evalint")
				{
					return DollarDirective_EvalInt();
				}
				else if(token.ToString() == "evalfloat")
				{
					return DollarDirective_EvalFloat();
				}
			}

			UnreadSourceToken(token);

			return false;
		}

		/// <summary>
		/// Reads a token from the current line, continues reading on the next line only if a backslash '\' is found.
		/// </summary>
		/// <returns></returns>
		private idToken ReadLine()
		{
			int crossLine = 0;
			idToken token;

			do
			{
				if((token = ReadSourceToken()) == null)
				{
					return null;
				}

				if(token.LinesCrossed > crossLine)
				{
					UnreadSourceToken(token);

					return null;
				}

				crossLine = 1;
			}
			while(token.ToString() == "\\");

			return token;
		}

		private idToken ReadSourceToken()
		{
			idLexer script;
			idToken token;
			IndentType type;
			int skip;
			int changedScript = 0;

			if(_scriptStack.Count == 0)
			{
				idConsole.FatalError("idScriptParser::ReadSourceToken: not loaded");
				return null;
			}

			// if there's no token already available
			while(_tokens.Count == 0)
			{
				// if there's a token to read from the script
				if((token = _scriptStack.Peek().ReadToken()) != null)
				{
					token.LinesCrossed += changedScript;

					// set the marker based on the start of the token read in
					if(_markerPosition == -1)
					{
						_markerPosition = token.WhiteSpaceEndPosition;
					}

					return token;
				}

				// if at the end of the script
				if(_scriptStack.Peek().IsEndOfFile == true)
				{
					// remove all indents of the script
					while((_indentStack.Count > 0) && (_indentStack.Peek().Script == _scriptStack.Peek()))
					{
						Warning("missing #endif");
						PopIndent(out type, out skip);
					}

					changedScript = 1;
				}

				// if this was the initial script
				if(_scriptStack.Count == 1)
				{
					return null;
				}

				// remove the script and return to the previous one
				script = _scriptStack.Pop();
			}

			// copy the already available token
			token = _tokens.Pop();

			return token;
		}

		private void UnreadSignToken()
		{
			idToken token = new idToken();
			token.Line = _scriptStack.Peek().LineNumber;
			token.WhiteSpaceStartPosition = 0;
			token.WhiteSpaceEndPosition = 0;
			token.LinesCrossed = 0;
			token.Flags = 0;
			token.Set("-");
			token.Type = TokenType.Punctuation;
			// TODO: token.SubType = LexerPunctuationID.Subtract;

			UnreadSourceToken(token);
		}

		private void UnreadSourceToken(idToken token)
		{
			_tokens.Push(token);
		}
		#endregion
		#endregion
	}

	[Flags]
	public enum TokenFlags
	{
		RecursiveDefine = 1
	}

	public enum IndentType
	{
		If = 0x0001,
		Else = 0x0002,
		ElseIf = 0x0004,
		IfDefined = 0x0008,
		IfNotDefined = 0x0010
	}

	[Flags]
	public enum DefineFlags
	{
		Fixed = 0x001
	}

	/// <summary>
	/// Macro definitions.
	/// </summary>
	public struct ScriptDefinition
	{
		/// <summary>Definition name.</summary>
		public string Name;
		/// <summary>Flags.</summary>
		public DefineFlags Flags;
		public int BuiltIn;
		/// <summary>Definition parameters.</summary>
		public idToken[] Parameters;
		/// <summary>Macro tokens (possibily containing parameter tokens).</summary>
		public idToken[] Tokens;
	}

	/// <summary>
	/// Indents used for conditional compilation directives: #if, #else, #elif, #ifdef, #ifndef.
	/// </summary>
	public struct ScriptIndentation
	{
		/// <summary>Type of the indentation.</summary>
		public IndentType Type;
		/// <summary>Are we skipping the current indent?</summary>
		public int Skip;
		/// <summary>Script the indent was in.</summary>
		public idLexer Script;
	}
}