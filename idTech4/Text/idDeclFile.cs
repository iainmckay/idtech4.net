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
using System.Collections.Generic;
using System.Text;

using idTech4.Services;

namespace idTech4.Text
{
	public sealed class idDeclFile
	{
		#region Constants
		public const LexerOptions LexerOptions = Text.LexerOptions.NoStringConcatination	| Text.LexerOptions.NoStringEscapeCharacters
													| Text.LexerOptions.AllowPathNames		| Text.LexerOptions.AllowMultiCharacterLiterals
													| Text.LexerOptions.AllowBackslashStringConcatination | Text.LexerOptions.NoFatalErrors;
		#endregion

		#region Properties
		public DeclType DefaultType
		{
			get
			{
				return _defaultType;
			}
		}

		public string FileName
		{
			get
			{
				return _fileName;
			}
		}

		public int FileSize
		{
			get
			{
				return _fileSize;
			}
		}

		public int LineCount
		{
			get
			{
				return _lineCount;
			}
		}
		#endregion

		#region Members
		private string _fileName;
		private int _fileSize;
		private int _lineCount;
		private int _checksum;

		private DeclType _defaultType;
		private List<idDecl> _decls = new List<idDecl>();
		#endregion

		#region Constructor
		public idDeclFile()
		{
			_fileName = "<implicit file>";
			_defaultType = DeclType.Unknown;
		}

		public idDeclFile(string fileName, DeclType defaultType)
		{
			_fileName = fileName;
			_defaultType = defaultType;
		}
		#endregion

		#region Methods
		/// <summary>
		/// This is used during both the initial load, and any reloads.
		/// </summary>
		/// <returns></returns>
		public int LoadAndParse()
		{
			// services we need
			idEngine engine = idEngine.Instance;

			IFileSystem fileSystem = engine.GetService<IFileSystem>();
			IDeclManager declManager = engine.GetService<IDeclManager>();

			// load the text
			idLog.DeveloperWriteLine("...loading '{0}'", this.FileName);

			byte[] data = fileSystem.ReadFile(this.FileName);

			if(data == null)
			{
				engine.FatalError("couldn't load {0}", this.FileName);
				return 0;
			}

			string content = UTF8Encoding.UTF8.GetString(data);
			idLexer lexer = new idLexer();
			lexer.Options = LexerOptions;

			if(lexer.LoadMemory(content, this.FileName) == false)
			{
				engine.Error("Couldn't parse {0}", this.FileName);
				return 0;
			}

			// mark all the defs that were from the last reload of this file
			foreach(idDecl decl in _decls)
			{
				decl.RedefinedInReload = false;
			}

			// TODO: checksum = MD5_BlockChecksum( buffer, length );

			_fileSize = content.Length;

			int startMarker, sourceLine;
			int size;
			string name;
			bool reparse;
			idToken token;
			idDecl newDecl;
			DeclType identifiedType;

			string tokenValue;

			// scan through, identifying each individual declaration
			while(true)
			{
				startMarker = lexer.FileOffset;
				sourceLine = lexer.LineNumber;

				// parse the decl type name
				if((token = lexer.ReadToken()) == null)
				{
					break;
				}

				tokenValue = token.ToString();

				// get the decl type from the type name
				identifiedType = declManager.GetDeclTypeFromName(tokenValue);

				if(identifiedType == DeclType.Unknown)
				{
					if(tokenValue == "{")
					{
						// if we ever see an open brace, we somehow missed the [type] <name> prefix
						lexer.Warning("Missing decl name");
						lexer.SkipBracedSection(false);

						continue;
					}
					else
					{
						if(this.DefaultType == DeclType.Unknown)
						{
							lexer.Warning("No type");
							continue;
						}

						lexer.UnreadToken = token;

						// use the default type
						identifiedType = this.DefaultType;
					}
				}

				// now parse the name
				if((token = lexer.ReadToken()) == null)
				{
					lexer.Warning("Type without definition at the end of file");
					break;
				}

				tokenValue = token.ToString();

				if(tokenValue == "{")
				{
					// if we ever see an open brace, we somehow missed the [type] <name> prefix
					lexer.Warning("Missing decl name");
					lexer.SkipBracedSection(false);

					continue;
				}

				// FIXME: export decls are only used by the model exporter, they are skipped here for now
				if(identifiedType == DeclType.ModelExport)
				{
					lexer.SkipBracedSection();
					continue;
				}

				name = tokenValue;

				// make sure there's a '{'
				if((token = lexer.ReadToken()) == null)
				{
					lexer.Warning("Type without definition at end of file");
					break;
				}

				tokenValue = token.ToString();

				if(tokenValue != "{")
				{
					lexer.Warning("Expecting '{{' but found '{0}'", tokenValue);
					continue;
				}

				lexer.UnreadToken = token;

				// now take everything until a matched closing brace
				lexer.SkipBracedSection();
				size = lexer.FileOffset - startMarker;

				// look it up, possibly getting a newly created default decl
				reparse = false;
				newDecl = declManager.FindTypeWithoutParsing(identifiedType, name, false);

				if(newDecl != null)
				{
					// update the existing copy
					if((newDecl.SourceFile != this) || (newDecl.RedefinedInReload == true))
					{
						lexer.Warning("{0} '{1}' previously defined at {2}:{3}", identifiedType.ToString().ToLower(), name, newDecl.FileName, newDecl.LineNumber);
						continue;
					}

					if(newDecl.State != DeclState.Unparsed)
					{
						reparse = true;
					}
				}
				else
				{
					// allow it to be created as a default, then add it to the per-file list
					newDecl = declManager.FindTypeWithoutParsing(identifiedType, name, true);

					if(newDecl == null)
					{
						lexer.Warning("could not instanciate decl '{0}' with name '{1}'", identifiedType.ToString().ToLower(), name);
						continue;
					}

					_decls.Add(newDecl);
				}

				newDecl.RedefinedInReload = true;
				newDecl.SourceText = content.Substring(startMarker, size);
				newDecl.SourceFile = this;
				newDecl.SourceTextOffset = startMarker;
				newDecl.SourceTextLength = size;
				newDecl.SourceLine = sourceLine;
				newDecl.State = DeclState.Unparsed;

				// if it is currently in use, reparse it immedaitely
				if(reparse)
				{
					newDecl.ParseLocal();
				}
			}

			_lineCount = lexer.LineNumber;

			// any defs that weren't redefinedInReload should now be defaulted
			foreach(idDecl decl in _decls)
			{
				if(decl.RedefinedInReload == false)
				{
					decl.MakeDefault();
					decl.SourceTextOffset = decl.SourceFile.FileSize;
					decl.SourceTextLength = 0;
					decl.SourceLine = decl.SourceFile.LineCount;
				}
			}

			return _checksum;
		}
		#endregion
	}
}