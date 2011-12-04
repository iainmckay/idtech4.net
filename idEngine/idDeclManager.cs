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
using System.IO;
using System.Linq;
using System.Text;

using idTech4.IO;
using idTech4.Renderer;
using idTech4.Text;
using idTech4.Text.Decl;

namespace idTech4
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
	public sealed class idDeclManager
	{
		#region Properties
		#region Internal
		internal int Indent
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
		#endregion

		#region Members
		private int _checksum; // checksum of all loaded decl text.
		private Dictionary<DeclType, idDeclType> _declTypes = new Dictionary<DeclType, idDeclType>();
		private List<idDeclFolder> _declFolders = new List<idDeclFolder>();

		private Dictionary<string, idDeclFile> _loadedFiles = new Dictionary<string, idDeclFile>(StringComparer.InvariantCultureIgnoreCase);
		private Dictionary<DeclType, List<idDecl>> _declsByType = new Dictionary<DeclType, List<idDecl>>();

		private idDeclFile _implicitDecls = new idDeclFile();	// this holds all the decls that were created because explicit
																// text definitions were not found. Decls that became default
																// because of a parse error are not in this list.
		private int _indent; // for MediaPrint.
		private bool _insideLevelLoad;
		#endregion

		#region Constructor
		public idDeclManager()
		{
			new idCvar("decl_show", "0", "set to 1 to print parses, 2 to also print references", 0, 2, CvarFlags.System /* TODO: , idCmdSystem::ArgCompletion_Integer<0,2>*/);
		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			idConsole.WriteLine("----- Initializing Decls -----");

			_checksum = 0;

#if USE_COMPRESSED_DECLS
			SetupHuffman();
#endif

#if GET_HUFFMAN_FREQUENCIES
			ClearHuffmanFrequencies();
#endif

			// decls used throughout the engine
			RegisterDeclType("table", DeclType.Table, new idDeclAllocator<idDeclTable>());
			RegisterDeclType("material", DeclType.Material, new idDeclAllocator<idMaterial>());
			/*RegisterDeclType("skin", DeclType.Skin, new idDeclAllocator<idDeclSkin>());
			RegisterDeclType("sound", DeclType.Sound, new idDeclAllocator<idSoundShader>());

			RegisterDeclType("entityDef", DeclType.EntityDef, new idDeclAllocator<idDeclEntityDef>());
			RegisterDeclType("mapDef", DeclType.MapDef, new idDeclAllocator<idDeclEntityDef>());
			RegisterDeclType("fx", DeclType.Fx, new idDeclAllocator<idDeclFX>());
			RegisterDeclType("particle", DeclType.Particle, new idDeclAllocator<idDeclParticle>());
			RegisterDeclType("articulatedFigure", DeclType.Af, new idDeclAllocator<idDeclAF>());
			RegisterDeclType("pda", DeclType.Pda, new idDeclAllocator<idDeclPDA>());*/
			RegisterDeclType("email", DeclType.Email, new idDeclAllocator<idDeclEmail>());
			/*RegisterDeclType("video", DeclType.Video, new idDeclAllocator<idDeclVideo>());
			RegisterDeclType("audio", DeclType.Audio, new idDeclAllocator<idDeclAudio>());*/

			RegisterDeclFolder("materials", ".mtr", DeclType.Material);
			//RegisterDeclFolder("skins", ".skin", DeclType.Skin);
			//RegisterDeclFolder("sound", ".sndshd", DeclType.Sound);

			// add console commands
			idE.CmdSystem.AddCommand("listDecls", "list all decls", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_ListDecls));

			// TODO
			/*cmdSystem->AddCommand( "reloadDecls", ReloadDecls_f, CMD_FL_SYSTEM, "reloads decls" );
			cmdSystem->AddCommand( "touch", TouchDecl_f, CMD_FL_SYSTEM, "touches a decl" );

			cmdSystem->AddCommand( "listTables", idListDecls_f<DECL_TABLE>, CMD_FL_SYSTEM, "lists tables", idCmdSystem::ArgCompletion_String<listDeclStrings> );
			cmdSystem->AddCommand( "listMaterials", idListDecls_f<DECL_MATERIAL>, CMD_FL_SYSTEM, "lists materials", idCmdSystem::ArgCompletion_String<listDeclStrings> );
			cmdSystem->AddCommand( "listSkins", idListDecls_f<DECL_SKIN>, CMD_FL_SYSTEM, "lists skins", idCmdSystem::ArgCompletion_String<listDeclStrings> );
			cmdSystem->AddCommand( "listSoundShaders", idListDecls_f<DECL_SOUND>, CMD_FL_SYSTEM, "lists sound shaders", idCmdSystem::ArgCompletion_String<listDeclStrings> );

			cmdSystem->AddCommand( "listEntityDefs", idListDecls_f<DECL_ENTITYDEF>, CMD_FL_SYSTEM, "lists entity defs", idCmdSystem::ArgCompletion_String<listDeclStrings> );
			cmdSystem->AddCommand( "listFX", idListDecls_f<DECL_FX>, CMD_FL_SYSTEM, "lists FX systems", idCmdSystem::ArgCompletion_String<listDeclStrings> );
			cmdSystem->AddCommand( "listParticles", idListDecls_f<DECL_PARTICLE>, CMD_FL_SYSTEM, "lists particle systems", idCmdSystem::ArgCompletion_String<listDeclStrings> );
			cmdSystem->AddCommand( "listAF", idListDecls_f<DECL_AF>, CMD_FL_SYSTEM, "lists articulated figures", idCmdSystem::ArgCompletion_String<listDeclStrings>);

			cmdSystem->AddCommand( "listPDAs", idListDecls_f<DECL_PDA>, CMD_FL_SYSTEM, "lists PDAs", idCmdSystem::ArgCompletion_String<listDeclStrings> );
			cmdSystem->AddCommand( "listEmails", idListDecls_f<DECL_EMAIL>, CMD_FL_SYSTEM, "lists Emails", idCmdSystem::ArgCompletion_String<listDeclStrings> );
			cmdSystem->AddCommand( "listVideos", idListDecls_f<DECL_VIDEO>, CMD_FL_SYSTEM, "lists Videos", idCmdSystem::ArgCompletion_String<listDeclStrings> );
			cmdSystem->AddCommand( "listAudios", idListDecls_f<DECL_AUDIO>, CMD_FL_SYSTEM, "lists Audios", idCmdSystem::ArgCompletion_String<listDeclStrings> );

			cmdSystem->AddCommand( "printTable", idPrintDecls_f<DECL_TABLE>, CMD_FL_SYSTEM, "prints a table", idCmdSystem::ArgCompletion_Decl<DECL_TABLE> );
			cmdSystem->AddCommand( "printMaterial", idPrintDecls_f<DECL_MATERIAL>, CMD_FL_SYSTEM, "prints a material", idCmdSystem::ArgCompletion_Decl<DECL_MATERIAL> );
			cmdSystem->AddCommand( "printSkin", idPrintDecls_f<DECL_SKIN>, CMD_FL_SYSTEM, "prints a skin", idCmdSystem::ArgCompletion_Decl<DECL_SKIN> );
			cmdSystem->AddCommand( "printSoundShader", idPrintDecls_f<DECL_SOUND>, CMD_FL_SYSTEM, "prints a sound shader", idCmdSystem::ArgCompletion_Decl<DECL_SOUND> );

			cmdSystem->AddCommand( "printEntityDef", idPrintDecls_f<DECL_ENTITYDEF>, CMD_FL_SYSTEM, "prints an entity def", idCmdSystem::ArgCompletion_Decl<DECL_ENTITYDEF> );
			cmdSystem->AddCommand( "printFX", idPrintDecls_f<DECL_FX>, CMD_FL_SYSTEM, "prints an FX system", idCmdSystem::ArgCompletion_Decl<DECL_FX> );
			cmdSystem->AddCommand( "printParticle", idPrintDecls_f<DECL_PARTICLE>, CMD_FL_SYSTEM, "prints a particle system", idCmdSystem::ArgCompletion_Decl<DECL_PARTICLE> );
			cmdSystem->AddCommand( "printAF", idPrintDecls_f<DECL_AF>, CMD_FL_SYSTEM, "prints an articulated figure", idCmdSystem::ArgCompletion_Decl<DECL_AF> );

			cmdSystem->AddCommand( "printPDA", idPrintDecls_f<DECL_PDA>, CMD_FL_SYSTEM, "prints an PDA", idCmdSystem::ArgCompletion_Decl<DECL_PDA> );
			cmdSystem->AddCommand( "printEmail", idPrintDecls_f<DECL_EMAIL>, CMD_FL_SYSTEM, "prints an Email", idCmdSystem::ArgCompletion_Decl<DECL_EMAIL> );
			cmdSystem->AddCommand( "printVideo", idPrintDecls_f<DECL_VIDEO>, CMD_FL_SYSTEM, "prints a Audio", idCmdSystem::ArgCompletion_Decl<DECL_VIDEO> );
			cmdSystem->AddCommand( "printAudio", idPrintDecls_f<DECL_AUDIO>, CMD_FL_SYSTEM, "prints an Video", idCmdSystem::ArgCompletion_Decl<DECL_AUDIO> );

			cmdSystem->AddCommand( "listHuffmanFrequencies", ListHuffmanFrequencies_f, CMD_FL_SYSTEM, "lists decl text character frequencies" );*/

			idConsole.WriteLine("------------------------------");
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
				idConsole.Warning("decl type '{0}' already exists", name);
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
				if((StringComparer.InvariantCultureIgnoreCase.Compare(tmp.Folder, folder) == 0)
					&& (StringComparer.InvariantCultureIgnoreCase.Compare(tmp.Extension, extension) == 0))
				{
					idConsole.Warning("decl folder '{0}' already exists", folder);
					return;
				}
			}

			idDeclFolder declFolder = new idDeclFolder();
			declFolder.Folder = folder;
			declFolder.Extension = extension;
			declFolder.DefaultType = defaultType;

			_declFolders.Add(declFolder);

			// scan for decl files
			idFileList fileList = idE.FileSystem.GetFiles(declFolder.Folder, declFolder.Extension, true);
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

		public DeclType GetDeclTypeFromName(string name)
		{
			try
			{
				return (DeclType) Enum.Parse(typeof(DeclType), name, true);
			}
			catch
			{

			}

			return DeclType.Unknown;
		}

		/// <summary>
		/// This is just used to nicely indent media caching prints.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public void MediaPrint(string format, params object[] args)
		{
			if(idE.CvarSystem.GetInt("decl_show") == 0)
			{
				return;
			}

			for(int i = 0; i < _indent; i++)
			{
				idConsole.Write("    ");
			}

			idConsole.WriteLine(format, args);
		}

		/// <summary>
		/// Finds the decl with the given name and type; returning a default version if not found.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <returns>Decl with the given name or a default decl of the requested type if not found.</returns>
		public idDecl FindType(DeclType type, string name)
		{
			return FindType(type, name, true);
		}

		/// <summary>
		/// Finds the decl with the given name and type; returning a default version if requested.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="makeDefault"> If true, a default decl of appropriate type will be created.</param>
		/// <returns>Decl with the given name or a default decl of the requested type if not found or null if makeDefault is false.</returns>
		public idDecl FindType(DeclType type, string name, bool makeDefault)
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

			return decl;
		}
		#endregion

		#region Internal
		/// <summary>
		/// This finds or creats the decl, but does not cause a parse.  This is only used internally.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		internal idDecl FindTypeWithoutParsing(DeclType type, string name)
		{
			return FindTypeWithoutParsing(type, name, true);
		}

		/// <summary>
		/// This finds or creates the decl, but does not cause a parse.  This is only used internally.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="makeDefault"></param>
		/// <returns></returns>
		internal idDecl FindTypeWithoutParsing(DeclType type, string name, bool makeDefault)
		{
			if(_declTypes.ContainsKey(type) == false)
			{
				idConsole.FatalError("find type without parsing: bad type {0}", type.ToString().ToLower());
			}
			else
			{
				string canonicalName = name;

				foreach(idDecl decl in _declsByType[type])
				{
					if(decl.Name.Equals(canonicalName, StringComparison.OrdinalIgnoreCase) == true)
					{
						// only print these when decl_show is set to 2, because it can be a lot of clutter
						if(idE.CvarSystem.GetInt("decl_show") > 1)
						{
							MediaPrint("referencing {0} {1}", type.ToString().ToLower(), name);
						}

						return decl;
					}
				}

				if(makeDefault == true)
				{
					idDecl newDecl = _declTypes[type].Allocator.Create();
					newDecl.Name = canonicalName;
					newDecl.Type = type;
					newDecl.State = DeclState.Unparsed;
					newDecl.SourceFile = _implicitDecls;
					newDecl.ParsedOutsideLevelLoad = !_insideLevelLoad;
					newDecl.Index = _declsByType[type].Count;

					_declsByType[type].Add(newDecl);

					return newDecl;
				}
			}

			return null;
		}
		#endregion

		#region Private
		private string MakeNameCanonical(string str)
		{
			int idx = str.IndexOf('.');
			str = str.Replace('\\', '/').ToLower();
			
			if(idx != -1)
			{
				return str.Substring(0, idx);
			}

			return str;
		}
		#endregion

		#region Command handlers
		private void Cmd_ListDecls(object sender, CommandEventArgs e)
		{
			int totalDecls = 0;
			int totalText = 0;
			int totalStructures = 0;

			int num, size;

			foreach(KeyValuePair<DeclType, List<idDecl>> kvp in _declsByType)
			{
				num = kvp.Value.Count;
				totalDecls += num;
				size = 0;

				for(int j = 0; j < num; j++)
				{
					size += kvp.Value[j].Size;
				}

				totalStructures += size;

				idConsole.WriteLine("{0}k {1} {2}", size >> 10, num, kvp.Key.ToString().ToLower());
			}

			foreach(KeyValuePair<string, idDeclFile> kvp in _loadedFiles)
			{
				totalText += kvp.Value.FileSize;
			}

			idConsole.WriteLine("{0} total decls is {1} decl files", totalDecls, _loadedFiles.Count);
			idConsole.WriteLine("{0}KB in text, {1}KB in structures", totalText >> 10, totalStructures >> 10);
		}
		#endregion
		#endregion
	}
		
	public enum DeclType
	{
		Unknown,
		Table,
		Material,
		Skin,
		Sound,
		EntityDef,
		ModelDef,
		Fx,
		Particle,
		Af,
		Pda,
		Video,
		Audio,
		Email,
		ModelExport,
		MapDef
	}

	public enum DeclState
	{
		Unparsed,
		/// <summary>Set if a parse failed due to an error, or the lack of any source.</summary>
		Defaulted,
		Parsed
	}

	public abstract class idDeclAllocatorBase
	{
		public abstract idDecl Create();
	}

	public sealed class idDeclAllocator<T> : idDeclAllocatorBase where T : idDecl, new() 
	{
		public override idDecl Create()
		{
			return (idDecl) new T();
		}
	}

	internal sealed class idDeclType
	{
		public string Name;
		public DeclType Type;
		public idDeclAllocatorBase Allocator;
	}

	internal sealed class idDeclFolder
	{
		public string Folder;
		public string Extension;
		public DeclType DefaultType;
	}
}