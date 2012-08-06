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
using System.Runtime.InteropServices;

using idTech4.Text;

namespace idTech4.Text
{
	public class idDecl : IDisposable
	{
		#region Properties
		#region Public
		public virtual string DefaultDefinition
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return "{ }";
			}
		}

		/// <summary>
		/// Gets if this decl was ever used.
		/// </summary>
		public bool EverReferenced
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _everReferenced;
			}
			internal set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_everReferenced = value;
			}
		}

		/// <summary>
		/// Gets the name of the file this declaration is defined in.
		/// </summary>
		public string FileName
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				if(_sourceFile == null)
				{
					return "*invalid*";
				}

				return _sourceFile.FileName;
			}
		}

		/// <summary>
		/// Gets the index in the per-type list.
		/// </summary>
		public int Index
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _index;
			}
			internal set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_index = value;
			}
		}

		/// <summary>
		/// True if the decl was defaulted or the text was created with a call to SetDefaultText.
		/// </summary>
		public bool IsImplicit
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return (this.SourceFile == idE.DeclManager.ImplicitDeclFile);
			}
		}

		/// <summary>
		/// Gets the line number where the actual declaration token starts.
		/// </summary>
		public int LineNumber
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _sourceLine;
			}
		}

		/// <summary>
		/// Gets the size in bytes that this type consumes.
		/// </summary>
		public virtual int MemoryUsage
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return 0;

				// TODO: how to do this in .net?
				//return Marshal.SizeOf(this);
			}
		}

		/// <summary>
		/// Gets the name of this decl.
		/// </summary>
		public string Name
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _name;
			}
			internal set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_name = value;
			}
		}

		/// <summary>
		/// Gets whether or not this was loaded outside of a level load.
		/// </summary>
		/// <remarks>
		/// These decls will never be purged.
		/// </remarks>
		public bool ParsedOutsideLevelLoad
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _parsedOutsideLevelLoad;
			}
			internal set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_parsedOutsideLevelLoad = value;
			}
		}

		/// <summary>
		/// Gets if this decl was used for the current level.
		/// </summary>
		public bool ReferencedThisLevel
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _referencedThisLevel;
			}
			internal set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_referencedThisLevel = value;
			}
		}
		
		/// <summary>
		/// Gets the state of this decl.
		/// </summary>
		public DeclState State
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _state;
			}
			internal set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_state = value;
			}
		}
		
		/// <summary>
		/// Gets the type of this decl.
		/// </summary>
		public DeclType Type
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _type;
			}
			internal set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_type = value;
			}
		}
		#endregion

		#region Internal
		/// <summary>
		/// Used during file reloading to make sure a decl that has
		/// its source removed will be defaulted.
		/// </summary>
		internal bool RedefinedInReload
		{
			get
			{
				return _redefinedInReload;
			}
			set
			{
				_redefinedInReload = value;
			}
		}


		/// <summary>
		/// Gets the line number where the actual declaration token starts.
		/// </summary>
		internal int SourceLine
		{
			get
			{
				return _sourceLine;
			}
			set
			{
				_sourceLine = value;
			}
		}

		/// <summary>
		/// Gets or sets the source file in which the decl was defined.
		/// </summary>
		internal idDeclFile SourceFile
		{
			get
			{
				return _sourceFile;
			}
			set
			{
				_sourceFile = value;
			}
		}

		/// <summary>
		/// Gets or sets the offset in source file to decl text.
		/// </summary>
		internal int SourceTextOffset
		{
			get
			{
				return _sourceTextOffset;
			}
			set
			{
				_sourceTextOffset = value;
			}
		}

		/// <summary>
		/// Gets or sets the length of decl text in source file.
		/// </summary>
		internal int SourceTextLength
		{
			get
			{
				return _sourceTextLength;
			}
			set
			{
				_sourceTextLength = value;
			}
		}

		internal string SourceText
		{
			get
			{
				return _textSource;
			}
			set
			{
				_textSource = value;
			}
		}
		#endregion
		#endregion

		#region Members
		private int _index;						// index in per-type list.
		private string _name;					// name of the decl.
		private string _textSource;				// decl text definition.
		private int _compressedLength;			// compressed length.
		private idDeclFile _sourceFile;			// source file in which the decl was defined.
		private int _sourceTextOffset;			// offset in source file to decl text.
		private int _sourceTextLength;			// length of decl text in source file.
		private int _sourceLine;				// this is where the actual declaration token starts.
		private int _checksum;					// checksum of the decl text.

		private DeclType _type;					// decl type.
		private DeclState _state;				// decl state.

		private bool _parsedOutsideLevelLoad;	// these decls will never be purged.
		private bool _everReferenced;			// set to true if the decl was ever used.
		private bool _referencedThisLevel;		// set to true when the decl is used for the current level.
		private bool _redefinedInReload;		// used during file reloading to make sure a decl that has
												// its source removed will be defaulted.

		private static int _recursionLevel;
		#endregion

		#region Constructor
		public idDecl()
		{
			_name = "unnamed";
			_type = DeclType.EntityDef;
			_state = DeclState.Unparsed;
		}

		~idDecl()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		public void EnsureNotPurged()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(_state == DeclState.Unparsed)
			{
				ParseLocal();
			}
		}

		public void MakeDefault()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_state = DeclState.Defaulted;

			idE.DeclManager.MediaPrint("DEFAULTED");

			string defaultText = this.DefaultDefinition;

			// a parse error inside a DefaultDefinition() string could
			// cause an infinite loop, but normal default definitions could
			// still reference other default definitions, so we can't
			// just dump out on the first recursion.
			if(++_recursionLevel > 100)
			{
				idConsole.FatalError("make default: bad defaultDefinition(): {0}", defaultText);
				return;
			}

			// always free data before parsing
			ClearData();

			// parse
			Parse(defaultText);

			// we could still eventually hit the recursion if we have enough Error() calls inside Parse...
			_recursionLevel--;
		}

		public virtual bool Parse(string text)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idLexer lexer = new idLexer(idDeclFile.LexerOptions);
			lexer.LoadMemory(text, this.FileName, this.LineNumber);
			lexer.SkipUntilString("{");
			lexer.SkipBracedSection(false);

			return true;
		}

		public void Purge()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// never purge things that were referenced outside level load,
			// like the console and menu graphics
			if(this.ParsedOutsideLevelLoad == true)
			{
				return;
			}

			_referencedThisLevel = false;

			MakeDefault();

			// the next Find() for this will re-parse the real data
			_state = DeclState.Unparsed;
		}
		#endregion

		#region Protected
		protected virtual void ClearData()
		{
			_sourceFile = null;
		}

		protected virtual bool GenerateDefaultText()
		{
			return false;
		}
		#endregion

		#region Internal
		/// <summary>
		/// Parses the decl definition.  After calling parse, a decl will be guaranteed usable.
		/// </summary>
		internal void ParseLocal()
		{
			bool generatedDefaultText = false;
			
			// always free data before parsing
			ClearData();

			idE.DeclManager.MediaPrint("parsing {0} {1}", this.Type.ToString().ToLower(), this.Name);

			// if no text source try to generate default text
			if(_textSource == null)
			{
				generatedDefaultText = GenerateDefaultText();
			}

			// indent for DEFAULTED or media file references
			idE.DeclManager.Indent++;

			// no text immediately causes a MakeDefault()
			if(_textSource == null)
			{
				MakeDefault();
				idE.DeclManager.Indent--;

				return;
			}

			_state = DeclState.Parsed;

			// parse
			Parse(_textSource);
			
			// free generated text
			if(generatedDefaultText == true)
			{
				_textSource = null;
			}

			idE.DeclManager.Indent--;
		}
		#endregion
		#endregion

		#region IDisposable implementation
		#region Properties
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}
		#endregion

		#region Members
		private bool _disposed;
		#endregion

		#region Methods
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(disposing == true)
			{
				ClearData();
			}

			_disposed = true;
		}
		#endregion
		#endregion
	}
}