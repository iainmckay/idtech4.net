using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace idTech4.Text.Decl
{
	public class idDeclVideo : idDecl
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

		public string Preview
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _preview;
			}
		}

		public string Video
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _video;
			}
		}

		public string VideoName
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _videoName;
			}
		}
		#endregion

		#region Members
		private string _preview;
		private string _video;
		private string _videoName;
		private string _info;
		private string _audio;
		#endregion

		#region Constructor
		public idDeclVideo()
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
				return "{\n\t{\n\t\tname\t\"Default Video\"\n\t}\n}"; 
			}
		}

		public override int MemoryUsage
		{
			get
			{
				idConsole.Warning("TODO: idDeclVideo.MemoryUsage");

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
					_audio = lexer.ReadToken().ToString();
					idE.DeclManager.FindSound(_audio);
				}
				else if(tokenValue == "info")
				{
					_info = lexer.ReadToken().ToString();
				}
				else if(tokenValue == "name")
				{
					_videoName = lexer.ReadToken().ToString();
				}
				else if(tokenValue == "preview")
				{
					_preview = lexer.ReadToken().ToString();
				}
				else if(tokenValue == "video")
				{
					_video = lexer.ReadToken().ToString();
					idE.DeclManager.FindMaterial(_video);
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