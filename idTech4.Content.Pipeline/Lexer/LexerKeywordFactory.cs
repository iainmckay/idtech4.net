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
using System.Reflection;

namespace idTech4.Content.Pipeline.Lexer
{
	public class LexerKeywordFactory
	{
		#region Members
		private Dictionary<string, Type> _keywords = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Constructor
		public LexerKeywordFactory()
		{

		}
		#endregion

		#region Methods
		public void ScanAssembly(Assembly assembly, string ns = null)
		{
			foreach(Type type in assembly.GetTypes())
			{
				if((string.IsNullOrEmpty(ns) == false) && (ns != type.Namespace))
				{
					continue;
				}

				LexerKeywordAttribute[] lexerKeywordAttributes = (LexerKeywordAttribute[]) type.GetCustomAttributes(typeof(LexerKeywordAttribute), false);

				if(lexerKeywordAttributes.Length > 0)
				{
					foreach(LexerKeywordAttribute lexerKeywordAttribute in lexerKeywordAttributes)
					{
						_keywords.Add(lexerKeywordAttribute.Name, type);
					}
				}
			}
		}

		public void ScanNamespace(string ns)
		{
			ScanAssembly(Assembly.GetCallingAssembly(), ns);
		}

		public LexerKeyword<TContent> Create<TContent>(string keyword)
		{
			if(_keywords.ContainsKey(keyword) == true)
			{
				Type type = _keywords[keyword];

				return (LexerKeyword<TContent>) type.Assembly.CreateInstance(type.FullName, false);
			}

			return null;
		}
		#endregion
	}
}