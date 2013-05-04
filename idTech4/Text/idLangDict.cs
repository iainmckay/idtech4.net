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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using idTech4.Text;

namespace idTech4
{
	public sealed class idLangDict
	{
		#region Constants
		private const int MaxRedirectionDepth = 2;
		#endregion

		#region Members
		private int _regexReplaceIndex;
		private Dictionary<string, string> _elements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Constructor
		public idLangDict()
		{

		}
		#endregion

		#region Members
		#region Public
		public void Clear()
		{
			_elements.Clear();
		}

		public string Get(string key)
		{
			string val = Find(key);

			if(val == null)
			{
				return key;
			}

			return val;
		}

		public string Find(string key)
		{
			int depth = 0;

			return Find_r(key, depth);
		}
		/*	string val;

			if((key.StartsWith("#str_") == false) && (key.StartsWith("#font_") == false))
			{
				return key;
			}

			if(_elements.TryGetValue(key, out val) == true)
			{
				return val;
			}

			idLog.Warning("Unknown string id {0}", key);

			return null;
		}*/

		private string Find_r(string key, int depth)
		{
			depth++;

			if(depth > MaxRedirectionDepth)
			{
				// This isn't an error because we assume the error will be obvious somewhere in a GUI or something,
				// and the whole point of tracking the depth is to avoid a crash.
				idLog.Warning("String '{0}', indirection depth > {1}", key, MaxRedirectionDepth);
				return null;
			}

			if(string.IsNullOrEmpty(key) == true)
			{
				return null;
			}

			if(_elements.ContainsKey(key) == false)
			{
				return null;
			}

			string value = _elements[key];

			if(value.StartsWith("#str_", StringComparison.OrdinalIgnoreCase) == true)
			{
				// this string is re-directed to another entry
				return Find_r(value, depth);
			}

			return value;
		}

		public bool Load(string buffer, string name)
		{
			if(string.IsNullOrEmpty(buffer) == true)
			{
				// let whoever called us deal with the failure (so sys_lang can be reset)
				return false;
			}

			idLog.WriteLine("Reading {0} as UTF-8", name);

			// FIXME: first character is something odd
			buffer = buffer.Substring(1);

			idLexer lexer = new idLexer(LexerOptions.NoFatalErrors | LexerOptions.NoStringConcatination | LexerOptions.AllowMultiCharacterLiterals | LexerOptions.AllowBackslashStringConcatination);
			lexer.LoadMemory(buffer, name);

			if(lexer.IsLoaded == false)
			{
				return false;
			}

			idToken token, token2;

			lexer.ExpectTokenString("{");

			while((token = lexer.ReadToken()) != null)
			{
				if(token.ToString() == "}")
				{
					break;
				}
				else if((token2 = lexer.ReadToken()) != null)
				{
					if(token2.ToString() == "}")
					{
						break;
					}

					_regexReplaceIndex = 0;

					// stock d3 language files contain sprintf formatters, we need to replace them
					string val = token2.ToString();
					val = Regex.Replace(val, "%s|%d|%x", new MatchEvaluator(ReplaceHandler));

					_elements.Add(token.ToString(), val);
				}
			}

			idLog.WriteLine("{0} strings read", _elements.Count);

			return true;
		}
		#endregion

		#region Private
		private string ReplaceHandler(Match match)
		{
			return String.Format("{{{0}}}", _regexReplaceIndex++);
		}
		#endregion
		#endregion
	}
}