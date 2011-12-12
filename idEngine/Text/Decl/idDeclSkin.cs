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

using idTech4.Renderer;

namespace idTech4.Text.Decl
{
	public sealed class idDeclSkin : idDecl
	{
		#region Properties
		public int ModelAssociationCount
		{
			get
			{
				return _associatedModels.Length;
			}
		}
		#endregion

		#region Members
		private SkinMapping[] _mappings;
		private string[] _associatedModels;
		#endregion

		#region Constructor
		public idDeclSkin()
			: base()
		{

		}
		#endregion

		#region Methods
		public string GetassociatedModel(int index)
		{
			if((index >= 0) && (index < _associatedModels.Length))
			{
				return _associatedModels[index];
			}

			return string.Empty;
		}

		public idMaterial RemapShaderBySkin(idMaterial shader)
		{
			if(shader == null)
			{
				return null;
			}

			// never remap surfaces that were originally nodraw, like collision hulls.
			if(shader.IsDrawn == false)
			{
				return shader;
			}

			for(int i = 0; i < _mappings.Length; i++)
			{
				SkinMapping map = _mappings[i];

				// null = wildcard match.
				if((map.From == null) || (map.From == shader))
				{
					return map.To;
				}
			}

			// didn't find a match or wildcard, so stay the same.
			return shader;
		}
		#endregion

		#region idDecl implementation
		#region Public
		public override bool Parse(string text)
		{
			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.SkipUntilString("{");

			List<SkinMapping> mappings = new List<SkinMapping>();
			List<string> associatedModels = new List<string>();

			idToken token, token2;
			string tokenLower;

			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenLower = token.ToString().ToLower();

				if(tokenLower == "}")
				{
					break;
				}
				else if((token2 = lexer.ReadToken()) == null)
				{
					lexer.Warning("Unexpected end of file");
					MakeDefault();

					break;
				}
				else if(tokenLower == "model")
				{
					associatedModels.Add(token2.ToString());
					continue;
				}

				SkinMapping map = new SkinMapping();
				map.To = idE.DeclManager.FindMaterial(token2.ToString());

				if(tokenLower == "*")
				{
					// wildcard.
					map.From = null;
				}
				else
				{
					map.From = idE.DeclManager.FindMaterial(token.ToString());
				}
				
				mappings.Add(map);
			}

			_mappings = mappings.ToArray();
			_associatedModels = associatedModels.ToArray();

			return false;
		}

		public override string GetDefaultDefinition()
		{
			return "{\n\t\"*\"\t\"_default\"\n}";
		}
		#endregion

		#region Protected
		protected override void ClearData()
		{
			base.ClearData();

			_mappings = null;
			_associatedModels = null;
		}

		protected override bool GenerateDefaultText()
		{
			// if there exists a material with the same name.
			if(idE.DeclManager.FindType(DeclType.Material, this.Name, false) != null)
			{
				this.SourceText = "skin " + this.Name + " // IMPLICITLY GENERATED\n"
					+ "{\n\t_default " + this.Name + "\n}\n";
		
				return true;
			}

			return false;
		}
		#endregion		
		#endregion
	}

	public struct SkinMapping
	{
		public idMaterial From; // null == any unmatched shader.
		public idMaterial To;
	}
}
