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

using idTech4.Services;

namespace idTech4.Text.Decls
{
	public class idDeclPDA : idDecl
	{
		#region Properties
		private List<idDeclVideo> _videoList = new List<idDeclVideo>();
		private List<idDeclAudio> _audioList = new List<idDeclAudio>();
		private List<idDeclEmail> _emailList = new List<idDeclEmail>();

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
				idLog.Warning("TODO: idDeclPDA.MemoryUsage");
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

			string baseStrID = string.Format("#str_{0}_pda_", this.Name);
			bool useOldStrings = idEngine.Instance.GetService<ICVarSystem>().GetBool("g_useOldPDAStrings");
			ILocalization localization = idEngine.Instance.GetService<ILocalization>();
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

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
				else if(tokenLower == "fullname")
				{
					token = lexer.ReadToken();
					
					if(useOldStrings == true)
					{
						_fullName = (token != null) ? token.ToString() : string.Empty;
					}
					else
					{
						_fullName = localization.Get( baseStrID + "fullname");
					}
				}
				else if(tokenLower == "icon")
				{
					token = lexer.ReadToken();
					_icon = (token != null) ? token.ToString() : string.Empty;
				}
				else if(tokenLower == "id")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_id = (token != null) ? token.ToString() : string.Empty;
					}
					else
					{
						_id = localization.Get(baseStrID + "id");
					}
				}
				else if(tokenLower == "name")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_pdaName = (token != null) ? token.ToString() : string.Empty;
					}
					else
					{
						_pdaName = localization.Get( baseStrID + "name");
					}
				}
				else if(tokenLower == "pda_email")
				{
					token = lexer.ReadToken();
					
					_emailList.Add(declManager.FindType<idDeclEmail>(DeclType.Email, token.ToString()));
				}
				else if(tokenLower == "pda_audio")
				{
					token = lexer.ReadToken();
					
					_audioList.Add(declManager.FindType<idDeclAudio>(DeclType.Audio, token.ToString()));
				}
				else if(tokenLower == "pda_video")
				{
					token = lexer.ReadToken();
					
					_videoList.Add(declManager.FindType<idDeclVideo>(DeclType.Video, token.ToString()));
				}
					else if(tokenLower == "post")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_post = (token != null) ? token.ToString() : string.Empty;
					}
					else
					{
						_post = localization.Get(baseStrID + "post");
					}
				}
				else if(tokenLower == "security")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_security = (token != null) ? token.ToString() : string.Empty;
					}
					else
					{
						_security = localization.Get(baseStrID + "security");
					}
				}
				else if(tokenLower == "title")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_title = (token != null) ? token.ToString() : string.Empty;
					}
					else
					{
						_title = localization.Get(baseStrID + "title");
					}
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