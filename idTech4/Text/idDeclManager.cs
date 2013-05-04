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
using System.IO;

using idTech4.Renderer;
using idTech4.Services;
using idTech4.Sound;
using idTech4.Text.Decls;

namespace idTech4.Text
{
	/// <summary>
	/// All "small text" data types, like materials, sound shaders, fx files,
	/// entity defs, etc. are managed uniformly, allowing reloading, purging,
	/// listing, printing, etc. All "large text" data types that never have more
	/// than one declaration in a given file, like maps, models, AAS files, etc.
	/// are not handled here.
	/// </summary>
	/// <remarks>
	/// A decl will never, ever go away once it is created. The manager is
	/// guaranteed to always return the same decl pointer for a decl type/name
	/// combination. The index of a decl in the per type list also stays the
	/// same throughout the lifetime of the engine. Although the pointer to
	/// a decl always stays the same, one should never maintain pointers to
	/// data inside decls. The data stored in a decl is not garranteed to stay
	/// the same for more than one engine frame.
	/// <p/>
	/// The decl indexes of explicitely defined decls are guarenteed to be
	/// consistent based on the parsed decl files. However, the indexes of
	/// implicit decls may be different based on the order in which levels
	/// are loaded.
	/// <p/>
	/// The decl namespaces are separate for each type. Comments for decls go
	/// above the text definition to keep them associated with the proper decl.
	/// <p/>
	/// During decl parsing, errors should never be issued, only warnings
	/// followed by a call to MakeDefault().
	/// </remarks>
	public class idDeclManager : IDeclManager
	{
		#region Members
		private bool _initialized;
		private int _checksum; // checksum of all loaded decl text.
		private int _indent; // for MediaPrint.
		private bool _insideLevelLoad;

		private Dictionary<DeclType, idDeclType> _declTypes = new Dictionary<DeclType, idDeclType>();
		private List<idDeclFolder> _declFolders = new List<idDeclFolder>();

		private Dictionary<string, idDeclFile> _loadedFiles = new Dictionary<string, idDeclFile>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<DeclType, List<idDecl>> _declsByType = new Dictionary<DeclType, List<idDecl>>();
		private idDeclFile _implicitDecls = new idDeclFile();	// this holds all the decls that were created because explicit
																// text definitions were not found. Decls that became default
																// because of a parse error are not in this list.
		#endregion

		#region Constructor
		public idDeclManager()
		{

		}			
		#endregion

		#region IDeclManager implementation
		#region Properties
		public idDeclFile ImplicitDeclFile
		{
			get
			{
				return _implicitDecls;
			}
		}

		public int MediaPrintIndent
		{
			get
			{
				return _indent;
			}
			set
			{
				_indent = value;
			}
		}
		#endregion

		#region Decl
		public idDecl DeclByIndex(DeclType type, int index, bool forceParse = true)
		{
			if(_declTypes.ContainsKey(type) == false)
			{
				idEngine.Instance.FatalError("DeclByIndex: bad type: {0}", type.ToString().ToLower());
			}

			if((index < 0) || (index >= _declsByType[type].Count))
			{
				idEngine.Instance.Error("DeclByIndex: out of range [{0}: {1} < {2}]", type, index, _declsByType[type].Count);
			}

			idDecl decl = _declsByType[type][index];

			if((forceParse == true) && (decl.State == DeclState.Unparsed))
			{
				decl.ParseLocal();
			}

			return decl;
		}
		#endregion

		#region Find
		/// <summary>
		/// Finds the decl with the given name and type; returning a default version if requested.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="makeDefault"> If true, a default decl of appropriate type will be created.</param>
		/// <returns>Decl with the given name or a default decl of the requested type if not found or null if makeDefault is false.</returns>
		public idDecl FindType(DeclType type, string name, bool makeDefault = true)
		{
			return FindType<idDecl>(type, name, makeDefault);
		}

		/// <summary>
		/// Finds the decl with the given name and type; returning a default version if requested.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="makeDefault"> If true, a default decl of appropriate type will be created.</param>
		/// <returns>Decl with the given name or a default decl of the requested type if not found or null if makeDefault is false.</returns>
		public T FindType<T>(DeclType type, string name, bool makeDefault = true) where T : idDecl
		{
			if((name == null) || (name == string.Empty))
			{
				name = "_emptyName";
			}

			idDecl decl = FindTypeWithoutParsing(type, name, makeDefault);

			if(decl == null)
			{
				return null;
			}

			// if it hasn't been parsed yet, parse it now
			if(decl.State == DeclState.Unparsed)
			{
				decl.ParseLocal();
			}

			// mark it as referenced
			decl.ReferencedThisLevel = true;
			decl.EverReferenced = true;

			if(_insideLevelLoad == true)
			{
				decl.ParsedOutsideLevelLoad = false;
			}

			return (decl as T);
		}

		public idMaterial FindMaterial(string name, bool makeDefault = true)
		{
			return FindType<idMaterial>(DeclType.Material, name, makeDefault);
		}

		public idDeclSkin FindSkin(string name, bool makeDefault = true)
		{
			return FindType<idDeclSkin>(DeclType.Skin, name, makeDefault);
		}

		public idSoundMaterial FindSound(string name, bool makeDefault = true)
		{
			return FindType<idSoundMaterial>(DeclType.Sound, name, makeDefault);
		}

		/// <summary>
		/// This finds or creates the decl, but does not cause a parse.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="makeDefault"></param>
		/// <returns></returns>
		public idDecl FindTypeWithoutParsing(DeclType type, string name, bool makeDefault = true)
		{
			if(_declTypes.ContainsKey(type) == false)
			{
				idEngine.Instance.FatalError("find type without parsing: bad type {0}", type.ToString().ToLower());
			}
			else
			{
				string canonicalName = name;

				foreach(idDecl decl in _declsByType[type])
				{
					if(decl.Name.Equals(canonicalName, StringComparison.OrdinalIgnoreCase) == true)
					{
						// only print these when decl_show is set to 2, because it can be a lot of clutter
						if(idEngine.Instance.GetService<ICVarSystem>().GetInt("decl_show") > 1)
						{
							MediaPrint("referencing {0} {1}", type.ToString().ToLower(), name);
						}

						return decl;
					}
				}

				if(makeDefault == true)
				{
					idDecl newDecl                 = _declTypes[type].Allocator.Create();
					newDecl.Name                   = canonicalName.Replace("\\", "/");
					newDecl.Type                   = type;
					newDecl.State                  = DeclState.Unparsed;
					newDecl.SourceFile             = _implicitDecls;
					newDecl.ParsedOutsideLevelLoad = !_insideLevelLoad;
					newDecl.Index                  = _declsByType[type].Count;

					_declsByType[type].Add(newDecl);

					return newDecl;
				}
			}

			return null;
		}

		public DeclType GetDeclTypeFromName(string name)
		{
			foreach(KeyValuePair<DeclType, idDeclType> kvp in _declTypes)
			{
				if(kvp.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
				{
					return kvp.Value.Type;
				}
			}

			return DeclType.Unknown;
		}
		#endregion

		#region Initialization
		#region Properties
		public bool IsInitialized
		{
			get
			{
				return _initialized;
			}
		}
		#endregion

		#region Methods
		public void Initialize()
		{
			if(this.IsInitialized == true)
			{
				throw new Exception("idDeclManager has already been initialized.");
			}

			idLog.WriteLine("----- Initializing Decls -----");

			_checksum = 0;

			// decls used throughout the engine
			RegisterDeclType("table", DeclType.Table, new idDeclAllocator<idDeclTable>());
			RegisterDeclType("material", DeclType.Material, new idDeclAllocator<idMaterial>());
			RegisterDeclType("skin", DeclType.Skin, new idDeclAllocator<idDeclSkin>());
			RegisterDeclType("sound", DeclType.Sound, new idDeclAllocator<idSoundMaterial>());

			RegisterDeclType("entityDef", DeclType.EntityDef, new idDeclAllocator<idDeclEntity>());
			RegisterDeclType("mapDef", DeclType.MapDef, new idDeclAllocator<idDeclEntity>());
			RegisterDeclType("fx", DeclType.Fx, new idDeclAllocator<idDeclFX>());
			RegisterDeclType("particle", DeclType.Particle, new idDeclAllocator<idDeclParticle>());
			/*RegisterDeclType("articulatedFigure",	DeclType.ArticulatedFigure,	new idDeclAllocator<idDeclAF>());*/
			RegisterDeclType("pda", DeclType.Pda, new idDeclAllocator<idDeclPDA>());
			RegisterDeclType("email", DeclType.Email, new idDeclAllocator<idDeclEmail>());
			RegisterDeclType("video", DeclType.Video, new idDeclAllocator<idDeclVideo>());
			RegisterDeclType("audio", DeclType.Audio, new idDeclAllocator<idDeclAudio>());

			RegisterDeclFolder("materials", ".mtr", DeclType.Material);

			_initialized = true;
		}
		#endregion
		#endregion

		#region Misc.
		public void BeginLevelLoad()
		{
			_insideLevelLoad = true;


			// clear all the referencedThisLevel flags and purge all the data
			// so the next reference will cause a reparse
			foreach(KeyValuePair<DeclType, List<idDecl>> kvp in _declsByType)
			{
				foreach(idDecl decl in kvp.Value)
				{
					decl.Purge();
				}
			}
		}

		public void EndLevelLoad()
		{
			_insideLevelLoad = false;

			// we don't need to do anything here, but the image manager, model manager,
			// and sound sample manager will need to free media that was not referenced
		}

		/// <summary>
		/// This is just used to nicely indent media caching prints.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void MediaPrint(string format, params object[] args)
		{
			if(idEngine.Instance.GetService<ICVarSystem>().GetInt("decl_show") == 0)
			{
				return;
			}

			for(int i = 0; i < _indent; i++)
			{
				idLog.Write("    ");
			}

			idLog.WriteLine(format, args);
		}
		#endregion

		#region Registration
		/// <summary>
		/// Registers a new folder with decl files.
		/// </summary>
		/// <param name="folder"></param>
		/// <param name="extension"></param>
		/// <param name="defaultType"></param>
		public void RegisterDeclFolder(string folder, string extension, DeclType defaultType)
		{
			// check whether this folder / extension combination already exists
			foreach(idDeclFolder tmp in _declFolders)
			{
				if((tmp.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase) == true)
					&& (tmp.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase) == true))
				{
					idLog.Warning("decl folder '{0}' already exists", folder);
					return;
				}
			}

			idDeclFolder declFolder = new idDeclFolder();
			declFolder.Folder = folder;
			declFolder.Extension = extension;
			declFolder.DefaultType = defaultType;

			_declFolders.Add(declFolder);

			// scan for decl files
			idFileList fileList = idEngine.Instance.GetService<IFileSystem>().ListFiles(declFolder.Folder, declFolder.Extension, true);
			idDeclFile declFile = null;

			// load and parse decl files
			foreach(string file in fileList.Files)
			{
				string fileName = Path.Combine(declFolder.Folder, file);

				// check whether this file has already been loaded
				if(_loadedFiles.ContainsKey(fileName) == true)
				{
					declFile = _loadedFiles[fileName];
				}
				else
				{
					declFile = new idDeclFile(fileName, defaultType);
					_loadedFiles.Add(fileName, declFile);
				}

				declFile.LoadAndParse();
			}
		}

		/// <summary>
		/// Registers a new decl type.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <param name="allocator"></param>
		public void RegisterDeclType(string name, DeclType type, idDeclAllocatorBase allocator)
		{
			if(_declTypes.ContainsKey(type) == true)
			{
				idLog.Warning("decl type '{0}' already exists", name);
			}
			else
			{
				idDeclType declType = new idDeclType();
				declType.Name = name;
				declType.Type = type;
				declType.Allocator = allocator;

				_declTypes.Add(type, declType);
				_declsByType.Add(type, new List<idDecl>());
			}
		}
		#endregion
		#endregion

		#region idDeclType
		private sealed class idDeclType
		{
			public string Name;
			public DeclType Type;
			public idDeclAllocatorBase Allocator;
		}
		#endregion

		#region idDeclFolder
		private sealed class idDeclFolder
		{
			public string Folder;
			public string Extension;
			public DeclType DefaultType;
		}
		#endregion
	}
}