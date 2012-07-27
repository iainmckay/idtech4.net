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

namespace idTech4.Text.Decl
{
	public class idDeclEntity : idDecl
	{
		#region Properties
		public idDict Dict
		{
			get
			{
				return _dict;
			}
		}
		#endregion

		#region Members
		private idDict _dict;
		#endregion

		#region Constructor
		public idDeclEntity() : base()
		{
			_dict = new idDict();
		}
		#endregion
	
		#region idDecl implementation
		protected override void ClearData()
		{
			_dict.Clear();
		}

		public override string  GetDefaultDefinition()
		{
 			 return "{\n\t\"DEFAULTED\"\t\"1\"\n}";
		}

		public override bool Parse(string text)
		{
			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.SkipUntilString("{");

			idToken token;
			idToken token2;
			string value;

			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				value = token.ToString();

				if(value == "}")
				{
					break;
				}

				if(token.Type != TokenType.String)
				{
					lexer.Warning("Expected quoted string, but found '{0}'", value);
					MakeDefault();

					return false;
				}

				if((token2 = lexer.ReadToken()) == null)
				{
					lexer.Warning("Unexpected end of file");
					MakeDefault();

					return false;
				}

				if(_dict.ContainsKey(value) == true)
				{
					lexer.Warning("'{0}' already defined", value);
				}

				_dict.Set(value, token2.ToString());
			}

			// we always automatically set a "classname" key to our name
			_dict.Set("classname", this.Name);

			// "inherit" keys will cause all values from another entityDef to be copied into this one
			// if they don't conflict.  We can't have circular recursions, because each entityDef will
			// never be parsed more than once

			// find all of the dicts first, because copying inherited values will modify the dict
			List<idDeclEntity> defList = new List<idDeclEntity>();
			List<string> keysToRemove = new List<string>();

			foreach(KeyValuePair<string, string> kvp in _dict.MatchPrefix("inherit"))
			{
				idDeclEntity copy = idE.DeclManager.FindType<idDeclEntity>(DeclType.EntityDef, kvp.Value, false);

				if(copy == null)
				{
					lexer.Warning("Unknown entityDef '{0}' inherited by '{1}'", kvp.Value, this.Name);
				}
				else
				{
					defList.Add(copy);
				}

				// delete this key/value pair
				keysToRemove.Add(kvp.Key);
			}

			_dict.Remove(keysToRemove.ToArray());

			// now copy over the inherited key / value pairs
			foreach(idDeclEntity def in defList)
			{
				_dict.SetDefaults(def._dict);
			}

			// precache all referenced media
			// do this as long as we arent in modview
			idE.Game.CacheDictionaryMedia(_dict);
		
			return true;
		}
		#endregion
	}
}