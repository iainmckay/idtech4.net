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
	public class idDeclAudio : idDecl
	{
		#region Properties
		public string Audio
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _audio;
			}
		}

		public string AudioName
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _audioName;
			}
		}

		public string Info
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _info;
			}
		}
		#endregion

		#region Members
		private string _info;
		private string _audioName;

		private idSoundMaterial _audio;
		#endregion

		#region Constructor
		public idDeclAudio()
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
				return "{\n\t{\n\t\tname\t\"Default Audio\"\n\t}\n}";
			}
		}

		public override int MemoryUsage
		{
			get
			{
				idLog.Warning("TODO: idDeclAudio.MemoryUsage");

				return 0;
			}
		}
		#endregion

		#region Methods
		protected override void ClearData()
		{
			base.ClearData();
		}

		public override bool Parse(string text)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			string baseStrID = string.Format("#str_{0}_audio_", this.Name);
			bool useOldStrings = idEngine.Instance.GetService<ICVarSystem>().GetBool("g_useOldPDAStrings");
			ILocalization localization = idEngine.Instance.GetService<ILocalization>();
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

			idLexer lexer = new idLexer(LexerOptions.NoStringConcatination | LexerOptions.AllowPathNames | LexerOptions.AllowMultiCharacterLiterals | LexerOptions.AllowBackslashStringConcatination | LexerOptions.NoFatalErrors);
			lexer.LoadMemory(text, this.FileName, this.LineNumber);
			lexer.SkipUntilString("{");

			idToken token;
			string tokenValue;

			while(true)
			{
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenValue = token.ToString().ToLower();

				if(tokenValue == "}")
				{
					break;
				}

				if(tokenValue == "audio")
				{
					_audio = declManager.FindSound(lexer.ReadToken().ToString());
				}
				else if(tokenValue == "info")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_info = token.ToString();
					}
					else
					{
						_info = localization.GetString(baseStrID + "info");
					}
				}
				else if(tokenValue == "name")
				{
					token = lexer.ReadToken();

					if(useOldStrings == true)
					{
						_audioName = token.ToString();
					}
					else
					{
						_audioName = localization.GetString(baseStrID + "name");
					}
				}
			}

			if(lexer.HadError == true)
			{
				lexer.Warning("Video decl '{0}' had a parse error", this.Name);
				return false;
			}

			return true;
		}
		#endregion
		#endregion
	}
}