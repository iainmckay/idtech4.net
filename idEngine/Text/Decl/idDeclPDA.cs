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
	public class idDeclPDA : idDecl
	{
		#region Properties
		private List<string> _videoList = new List<string>();
		private List<string> _audioList = new List<string>();
		private List<string> _emailList = new List<string>();

		private string _pdaName;
		private string _fullName;
		private string _icon;
		private string _id;
		private string _post;
		private string _title;
		private string _security;

		private int _originalEmailCount;
		private int _originalVideoCount;
		#endregion

		#region Constructor
		public idDeclPDA()
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

				return "{\n\tname  \"default pda\"\n}"; 
			}
		}

		public override int MemoryUsage
		{
			get
			{
				idConsole.Warning("TODO: idDeclPDA.MemoryUsage");
				return 0;
			}
		}
		#endregion

		#region Methods
		#region Public
		public override bool Parse(string text)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idToken token;
			string tokenLower;

			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.LoadMemory(text, this.FileName, this.LineNumber);
			lexer.SkipUntilString("{");

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
				else if(tokenLower == "name")
				{
					token = lexer.ReadToken();
					_pdaName = (token != null) ? token.ToString() : string.Empty;
				}
				else if(tokenLower == "fullname")
				{
					token = lexer.ReadToken();
					_fullName = (token != null) ? token.ToString() : string.Empty;
				}
				else if(tokenLower == "icon")
				{
					token = lexer.ReadToken();
					_icon = (token != null) ? token.ToString() : string.Empty;
				}
				else if(tokenLower == "id")
				{
					token = lexer.ReadToken();
					_id = (token != null) ? token.ToString() : string.Empty;
				}
				else if(tokenLower == "post")
				{
					token = lexer.ReadToken();
					_post = (token != null) ? token.ToString() : string.Empty;
				}
				else if(tokenLower == "title")
				{
					token = lexer.ReadToken();
					_title = (token != null) ? token.ToString() : string.Empty;
				}
				else if(tokenLower == "security")
				{
					token = lexer.ReadToken();
					_security = (token != null) ? token.ToString() : string.Empty;
				}
				else if(tokenLower == "pda_email")
				{
					token = lexer.ReadToken();
					_emailList.Add(token.ToString());

					idE.DeclManager.FindType(DeclType.Email, token.ToString());
				}
				else if(tokenLower == "pda_audio")
				{
					token = lexer.ReadToken();
					_audioList.Add(token.ToString());

					idE.DeclManager.FindType(DeclType.Audio, token.ToString());
				}
				else if(tokenLower == "pda_video")
				{
					token = lexer.ReadToken();
					_videoList.Add(token.ToString());

					idE.DeclManager.FindType(DeclType.Video, token.ToString());
				}
			}

			if(lexer.HadError == true)
			{
				lexer.Warning("PDA decl '{0}' had a parse error", this.Name);
				return false;
			}

			_originalVideoCount = _videoList.Count;
			_originalEmailCount = _emailList.Count;

			return true;
		}
		#endregion

		#region Protected
		protected override void ClearData()
		{
			base.ClearData();

			_videoList.Clear();
			_audioList.Clear();
			_emailList.Clear();

			_originalEmailCount = 0;
			_originalVideoCount = 0;
		}
		#endregion
		#endregion
		#endregion
	}
}
