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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace idTech4.IO
{
	/// <summary>
	/// All of Doom's data access is through a hierarchical file system, but the contents of 
	/// the file system can be transparently merged from several sources.
	/// </summary>
	/// <remarks>
	/// Throughout the game a forward slash should be used as a separator. The file system 
	/// takes care of the conversion to an OS specific separator. The file system treats all 
	/// file and directory names as case insensitive.
	/// <p/>
	/// The following cvars store paths used by the file system:
	///	"fs_basepath"		path to local install, read-only
	/// "fs_savepath"		path to config, save game, etc. files, read & write
	/// "fs_cdpath"			path to cd, read-only
	/// "fs_devpath"		path to files created during development, read & write
	/// <p/>
	/// The base path for file saving can be set to "fs_savepath" or "fs_devpath".
	/// <p/>
	/// A "relativePath" is a reference to game file data.
	/// "..", "\\", and ":" are explicitly illegal in qpaths to prevent any references
	/// outside the Doom directory system.
	/// <p/>
	/// The "base path" is the path to the directory holding all the game directories and
	/// usually the executable. It defaults to the current directory, but can be overridden
	/// with "+set fs_basepath c:\doom" on the command line. The base path cannot be modified
	/// at all after startup.
	/// <p/>
	/// The "save path" is the path to the directory where game files will be saved. It defaults
	/// to the base path, but can be overridden with a "+set fs_savepath c:\doom" on the
	/// command line. Any files that are created during the game (demos, screenshots, etc.) will
	/// be created reletive to the save path.
	/// <p/>
	/// The "cd path" is the path to an alternate hierarchy that will be searched if a file
	/// is not located in the base path. A user can do a partial install that copies some
	/// data to a base path created on their hard drive and leave the rest on the cd. It defaults
	/// to the current directory, but it can be overridden with "+set fs_cdpath g:\doom" on the
	/// command line.
	/// <p/>
	/// The "dev path" is the path to an alternate hierarchy where the editors and tools used
	/// during development (Radiant, AF editor, dmap, runAAS) will write files to. It defaults to
	/// the cd path, but can be overridden with a "+set fs_devpath c:\doom" on the command line.
	/// <p/>
	/// If a user runs the game directly from a CD, the base path would be on the CD. This
	/// should still function correctly, but all file writes will fail (harmlessly).
	/// <p/>
	/// The "base game" is the directory under the paths where data comes from by default, and
	/// can be either "base" or "demo".
	/// <p/>
	/// The "current game" may be the same as the base game, or it may be the name of another
	/// directory under the paths that should be searched for files before looking in the base
	/// game. The game directory is set with "+set fs_game myaddon" on the command line. This is
	/// the basis for addons.
	/// <p/>
	/// No other directories outside of the base game and current game will ever be referenced by
	/// filesystem functions.
	/// <p/>
	/// To save disk space and speed up file loading, directory trees can be collapsed into zip
	/// files. The files use a ".pk4" extension to prevent users from unzipping them accidentally,
	/// but otherwise they are simply normal zip files. A game directory can have multiple zip
	/// files of the form "pak0.pk4", "pak1.pk4", etc. Zip files are searched in decending order
	/// from the highest number to the lowest, and will always take precedence over the filesystem.
	/// This allows a pk4 distributed as a patch to override all existing data.
	/// <p/>
	/// Because we will have updated executables freely available online, there is no point to
	/// trying to restrict demo / oem versions of the game with code changes. Demo / oem versions
	/// should be exactly the same executables as release versions, but with different data that
	/// automatically restricts where game media can come from to prevent add-ons from working.
	/// <p/>
	/// After the paths are initialized, Doom will look for the product.txt file. If not found
	/// and verified, the game will run in restricted mode. In restricted mode, only files
	/// contained in demo/pak0.pk4 will be available for loading, and only if the zip header is
	/// verified to not have been modified. A single exception is made for DoomConfig.cfg. Files
	/// can still be written out in restricted mode, so screenshots and demos are allowed.
	/// Restricted mode can be tested by setting "+set fs_restrict 1" on the command line, even
	/// if there is a valid product.txt under the basepath or cdpath.
	/// <p/>
	/// If the "fs_copyfiles" cvar is set to 1, then every time a file is sourced from the cd
	/// path, it will be copied over to the save path. This is a development aid to help build
	/// test releases and to copy working sets of files.
	/// <p/>
	/// If the "fs_copyfiles" cvar is set to 2, any file found in fs_cdpath that is newer than
	/// it's fs_savepath version will be copied to fs_savepath (in addition to the fs_copyfiles 1
	/// behaviour).
	/// <p/>
	/// If the "fs_copyfiles" cvar is set to 3, files from both basepath and cdpath will be copied
	/// over to the save path. This is useful when copying working sets of files mainly from base
	/// path with an additional cd path (which can be a slower network drive for instance).
	/// <p/>
	/// If the "fs_copyfiles" cvar is set to 4, files that exist in the cd path but NOT the base path
	/// will be copied to the save path
	/// <p/>
	/// NOTE: fs_copyfiles and case sensitivity. On fs_caseSensitiveOS 0 filesystems ( win32 ), the
	/// copied files may change casing when copied over.
	/// <p/>
	/// The relative path "sound/newstuff/test.wav" would be searched for in the following places:
	/// for save path, dev path, base path, cd path:
	///		for current game, base game:
	///			search directory
	///				search zip files
	/// <p/>
	/// downloaded files, to be written to save path + current game's directory
	/// <p/>
	/// The filesystem can be safely shutdown and reinitialized with different
	/// basedir / cddir / game combinations, but all other subsystems that rely on it
	/// (sound, video) must also be forced to restart.
	/// <p/>
	/// "fs_caseSensitiveOS":
	/// This cvar is set on operating systems that use case sensitive filesystems (Linux and OSX)
	/// It is a common situation to have the media reference filenames, whereas the file on disc 
	/// only matches in a case-insensitive way. When "fs_caseSensitiveOS" is set, the filesystem
	/// will always do a case insensitive search.
	/// IMPORTANT: This only applies to files, and not to directories. There is no case-insensitive
	/// matching of directories. All directory names should be lowercase, when "com_developer" is 1,
	/// the filesystem will warn when it catches bad directory situations (regardless of the
	/// "fs_caseSensitiveOS" setting)
	/// When bad casing in directories happen and "fs_caseSensitiveOS" is set, BuildOSPath will
	/// attempt to correct the situation by forcing the path to lowercase. This assumes the media
	/// is stored all lowercase.
	/// <p/>
	/// "additional mod path search":
	/// fs_game_base can be used to set an additional search path
	/// in search order, fs_game, fs_game_base, BASEGAME
	/// for instance to base a mod of D3 + D3XP assets, fs_game mymod, fs_game_base d3xp
	/// </remarks>
	public sealed class idFileSystem
	{
		#region Properties
		public bool IsInitialized
		{
			get
			{
				return (_searchPaths.Count > 0);
			}
		}
		#endregion

		#region Members
		private Thread _backgroundDownloadThread;
		private Queue<BackgroundDownload> _backgroundDownloads = new Queue<BackgroundDownload>();

		private List<SearchPath> _searchPaths = new List<SearchPath>();
		private string _gameFolder; // this will be a single name without separators.

		private bool _loadedFileFromDir; // set to true once a file was loaded from a directory - can't switch to pure anymore.

		private int _readCount; // total bytes read.
		private int _loadCount;	// total files read.
		private int _loadStack;	// total files in memory.

		private List<int> _restartChecksums = new List<int>(); // used during a restart to set things in right order.
		private List<int> _addonChecksums = new List<int>(); // list of checksums that should go to the search list directly (for restarts).
		#endregion

		#region Constructor
		public idFileSystem()
		{
			new idCvar("fs_restrict", "", "", CvarFlags.System | CvarFlags.Init | CvarFlags.Bool);
			new idCvar("fs_debug", "0", 0, 2, "", new ArgCompletion_Integer(0, 2), CvarFlags.System | CvarFlags.Integer);
			new idCvar("fs_copyfiles", "0", 0, 4, "", new ArgCompletion_Integer(0, 3), CvarFlags.System | CvarFlags.Init | CvarFlags.Integer);
			new idCvar("fs_basepath", "", "", CvarFlags.System | CvarFlags.Init);
			new idCvar("fs_savepath", "", "", CvarFlags.System | CvarFlags.Init);
			new idCvar("fs_cdpath", "", "", CvarFlags.System | CvarFlags.Init);
			new idCvar("fs_devpath", "", "", CvarFlags.System | CvarFlags.Init);
			new idCvar("fs_game", "", "mod path", CvarFlags.System | CvarFlags.Init | CvarFlags.ServerInfo);
			new idCvar("fs_game_base", "", "alternate mod path, searched after the main fs_game path, before the basedir", CvarFlags.System | CvarFlags.Init | CvarFlags.ServerInfo);
			new idCvar("fs_caseSensitiveOS", idE.Platform.IsWindows ? "0" : "1", "", CvarFlags.System | CvarFlags.Bool);
			new idCvar("fs_searchAddons", "0", "search all addon pk4s (disables addon functionality)", CvarFlags.System | CvarFlags.Bool);
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Called only at inital startup, not when the filesystem is resetting due to a game change.
		/// </summary>
		public void Init()
		{
			// allow command line parms to override our defaults
			// we have to specially handle this, because normal command
			// line variable sets don't happen until after the filesystem
			// has already been initialized
			idE.System.StartupVariable("fs_basepath", false);
			idE.System.StartupVariable("fs_savepath", false);
			idE.System.StartupVariable("fs_cdpath", false);
			idE.System.StartupVariable("fs_devpath", false);
			idE.System.StartupVariable("fs_game", false);
			idE.System.StartupVariable("fs_game_base", false);
			idE.System.StartupVariable("fs_copyFiles", false);
			idE.System.StartupVariable("fs_restrict", false);
			idE.System.StartupVariable("fs_searchAddons", false);

			// TODO #if !ID_ALLOW_D3XP
			/*if ( fs_game.GetString()[0] && !idStr::Icmp( fs_game.GetString(), "d3xp" ) ) {
				 fs_game.SetString( NULL );
			}
			if ( fs_game_base.GetString()[0] && !idStr::Icmp( fs_game_base.GetString(), "d3xp" ) ) {
				  fs_game_base.SetString( NULL );
			}
			#endif	*/

			if(idE.CvarSystem.GetString("fs_basepath") == string.Empty)
			{
				idE.CvarSystem.SetString("fs_basepath", Environment.CurrentDirectory);
			}

			if(idE.CvarSystem.GetString("fs_savepath") == string.Empty)
			{
				idE.CvarSystem.SetString("fs_savepath", idE.CvarSystem.GetString("fs_basepath"));
			}

			if(idE.CvarSystem.GetString("fs_cdpath") == string.Empty)
			{
				idE.CvarSystem.SetString("fs_cdpath", string.Empty);
			}

			if(idE.CvarSystem.GetString("fs_devpath") == string.Empty)
			{
				idE.CvarSystem.SetString("fs_devpath", idE.CvarSystem.GetString("fs_savepath"));
			}

			// try to start up normally
			Startup();

			// spawn a thread to handle background file reads
			StartBackgroundDownloadThread();

			// if we can't find default.cfg, assume that the paths are
			// busted and error out now, rather than getting an unreadable
			// graphics screen when the font fails to load
			// Dedicated servers can run with no outside files at all
			if(FileExists("default.cfg") == false)
			{
				idConsole.FatalError("Couldn't load default.cfg");
			}
		}

		public string CreatePath(string baseDirectory, string gameDirectory, string relativePath)
		{
			// TODO
			/*if ( fs_caseSensitiveOS.GetBool() || com_developer.GetBool() ) {
				// extract the path, make sure it's all lowercase
				idStr testPath, fileName;

				sprintf( testPath, "%s/%s", game , relativePath );
				testPath.StripFilename();

				if ( testPath.HasUpper() ) {

					common->Warning( "Non-portable: path contains uppercase characters: %s", testPath.c_str() );

					// attempt a fixup on the fly
					if ( fs_caseSensitiveOS.GetBool() ) {
						testPath.ToLower();
						fileName = relativePath;
						fileName.StripPath();
						sprintf( newPath, "%s/%s/%s", base, testPath.c_str(), fileName.c_str() );
						ReplaceSeparators( newPath );
						common->DPrintf( "Fixed up to %s\n", newPath.c_str() );
						idStr::Copynz( OSPath, newPath, sizeof( OSPath ) );
						return OSPath;
					}
				}
			}*/

			return Path.Combine(baseDirectory, gameDirectory, relativePath);
		}

		public idFileList GetFiles(string relativePath, string extension)
		{
			return GetFiles(relativePath, extension, false, false, null);
		}

		public idFileList GetFiles(string relativePath, string extension, bool sort)
		{
			return GetFiles(relativePath, extension, sort, false, null);
		}

		public idFileList GetFiles(string relativePath, string extension, bool sort, bool fullRelativePath, string gameDirectory)
		{
			string[] extensionList = GetExtensionList(extension);
			string[] fileList = GetFileList(relativePath, extensionList, fullRelativePath, gameDirectory);

			if(sort == true)
			{
				fileList = fileList.OrderBy(x => x).ToArray();
			}

			return new idFileList(relativePath, fileList);

		}

		public string[] GetExtensionList(string extension)
		{
			string[] parts = extension.Split('|');
			List<string> list = new List<string>();

			foreach(string part in parts)
			{
				if(part != string.Empty)
				{
					list.Add(part);
				}
			}

			return list.ToArray();
		}

		public bool FileExists(string relativePath)
		{
			return FileExists(relativePath, null);
		}

		public bool FileExists(string relativePath, string basePath)
		{
			Pack pack;
			bool found;

			if(basePath != null)
			{
				relativePath = RelativePathToOSPath(relativePath, basePath);
			}

			Stream s = OpenFileRead(relativePath, true, out pack, null, FileSearch.SearchDirectories | FileSearch.SearchPaks, true, out found);

			if(s != null)
			{
				s.Dispose();
			}

			return found;
		}

		public Stream OpenExplicitFileRead(string osPath)
		{
			if(_searchPaths.Count == 0)
			{
				idConsole.FatalError("Filesystem call made without initialization");
			}

			if(idE.CvarSystem.GetInteger("fs_debug") > 0)
			{
				idConsole.WriteLine("idFileSystem.OpenExplicitFileRead: {0}", osPath);
			}

			idConsole.DeveloperWriteLine("idFileSystem.OpenExplicitFileRead - reading from: {0}", osPath);

			return OpenOSFile(osPath, FileMode.Open, FileAccess.Read);
		}

		public Stream OpenFileRead(string relativePath)
		{
			return OpenFileRead(relativePath, true, null);
		}

		public Stream OpenFileRead(string relativePath, bool allowCopyFiles)
		{
			return OpenFileRead(relativePath, allowCopyFiles, null);
		}

		public Stream OpenFileRead(string relativePath, bool allowCopyFiles, string gameDirectory)
		{
			Pack p;

			return OpenFileRead(relativePath, allowCopyFiles, out p, gameDirectory, FileSearch.SearchDirectories | FileSearch.SearchPaks);
		}

		public Stream OpenFileRead(string relativePath, bool allowCopyFiles, out Pack foundInPack, FileSearch searchFlags)
		{
			return OpenFileRead(relativePath, allowCopyFiles, out foundInPack, null, searchFlags);
		}

		public Stream OpenFileRead(string relativePath, bool allowCopyFiles, out Pack foundInPack, string gameDirectory, FileSearch searchFlags)
		{
			bool found;
			return OpenFileRead(relativePath, allowCopyFiles, out foundInPack, null, searchFlags, false, out found);
		}

		public Stream OpenFileRead(string relativePath, bool allowCopyFiles, out Pack foundInPack, string gameDirectory, FileSearch searchFlags, bool checkOnly, out bool found)
		{
			if(_searchPaths.Count == 0)
			{
				idConsole.FatalError("Filesystem call made without initialization");
			}

			if((relativePath == null) || (relativePath == string.Empty))
			{
				idConsole.FatalError("open file read: null 'relativePath' parameter passed");
			}

			found = false;
			foundInPack = null;

			// qpaths are not supposed to have a leading slash.
			relativePath = relativePath.TrimEnd('/');

			// make absolutely sure that it can't back up the path.
			// the searchpaths do guarantee that something will always
			// be prepended, so we don't need to worry about "c:" or "//limbo". 
			if((relativePath.Contains("..") == true) || (relativePath.Contains("::") == true))
			{
				return null;
			}

			//
			// search through the path, one element at a time
			//
			foreach(SearchPath searchPath in _searchPaths)
			{
				if((searchPath.Directory != null) && (searchFlags.HasFlag(FileSearch.SearchDirectories) == true))
				{
					// check a file in the directory tree.
					// if we are running restricted, the only files we
					// will allow to come from the directory are .cfg files.
					if((idE.CvarSystem.GetBool("fs_restrict") == true) /* TODO: serverPaks.Num()*/)
					{
						if(FileAllowedFromDirectory(relativePath) == false)
						{
							continue;
						}
					}

					idDirectory dir = searchPath.Directory;

					if((gameDirectory != null) && (gameDirectory != string.Empty))
					{
						if(dir.GameDirectory != gameDirectory)
						{
							continue;
						}
					}

					string netPath = CreatePath(dir.Path, dir.GameDirectory, relativePath);
					Stream file = null;

					if(File.Exists(netPath) == true)
					{
						try
						{
							file = File.OpenRead(netPath);
						}
						catch(Exception x)
						{
							idConsole.DeveloperWriteLine("OpenFileRead Exception: {0}", x.ToString());

							continue;
						}
					}

					if(file == null)
					{
						continue;
					}

					if(idE.CvarSystem.GetInteger("fs_debug") > 0)
					{
						idConsole.WriteLine("open file read: {0} (found in '{1}/{2}')", relativePath, dir.Path, dir.GameDirectory);
					}

					if((_loadedFileFromDir == false) && (FileAllowedFromDirectory(relativePath) == false))
					{
						// TODO
						/*if(restartChecksums.Num())
						{
							common->FatalError("'%s' loaded from directory: Failed to restart with pure mode restrictions for server connect", relativePath);
						}*/

						idConsole.DeveloperWriteLine("filesystem: switching to pure mode will require a restart. '{0}' loaded from directory.", relativePath);
						_loadedFileFromDir = true;
					}

					// TODO: if fs_copyfiles is set
					/*if(allowCopyFiles && fs_copyfiles.GetInteger())
					{

						idStr copypath;
						idStr name;
						copypath = BuildOSPath(fs_savepath.GetString(), dir->gamedir, relativePath);
						netpath.ExtractFileName(name);
						copypath.StripFilename();
						copypath += PATHSEPERATOR_STR;
						copypath += name;

						bool isFromCDPath = !dir->path.Cmp(fs_cdpath.GetString());
						bool isFromSavePath = !dir->path.Cmp(fs_savepath.GetString());
						bool isFromBasePath = !dir->path.Cmp(fs_basepath.GetString());

						switch(fs_copyfiles.GetInteger())
						{
							case 1:
								// copy from cd path only
								if(isFromCDPath)
								{
									CopyFile(netpath, copypath);
								}
								break;
							case 2:
								// from cd path + timestamps
								if(isFromCDPath)
								{
									CopyFile(netpath, copypath);
								}
								else if(isFromSavePath || isFromBasePath)
								{
									idStr sourcepath;
									sourcepath = BuildOSPath(fs_cdpath.GetString(), dir->gamedir, relativePath);
									FILE* f1 = OpenOSFile(sourcepath, "r");
									if(f1)
									{
										ID_TIME_T t1 = Sys_FileTimeStamp(f1);
										fclose(f1);
										FILE* f2 = OpenOSFile(copypath, "r");
										if(f2)
										{
											ID_TIME_T t2 = Sys_FileTimeStamp(f2);
											fclose(f2);
											if(t1 > t2)
											{
												CopyFile(sourcepath, copypath);
											}
										}
									}
								}
								break;
							case 3:
								if(isFromCDPath || isFromBasePath)
								{
									CopyFile(netpath, copypath);
								}
								break;
							case 4:
								if(isFromCDPath && !isFromBasePath)
								{
									CopyFile(netpath, copypath);
								}
								break;
						}
					}*/

					if(file != null)
					{
						found = true;
					}

					return file;
				}
				/*else if(search->pack && (searchFlags & FSFLAG_SEARCH_PAKS))
				{

					if(!search->pack->hashTable[hash])
					{
						continue;
					}

					// disregard if it doesn't match one of the allowed pure pak files
					if(serverPaks.Num())
					{
						GetPackStatus(search->pack);
						if(search->pack->pureStatus != PURE_NEVER && !serverPaks.Find(search->pack))
						{
							continue; // not on the pure server pak list
						}
					}

					// look through all the pak file elements
					pak = search->pack;

					if(searchFlags & FSFLAG_BINARY_ONLY)
					{
						// make sure this pak is tagged as a binary file
						if(pak->binary == BINARY_UNKNOWN)
						{
							int confHash;
							fileInPack_t* pakFile;
							confHash = HashFileName(BINARY_CONFIG);
							pak->binary = BINARY_NO;
							for(pakFile = search->pack->hashTable[confHash]; pakFile; pakFile = pakFile->next)
							{
								if(!FilenameCompare(pakFile->name, BINARY_CONFIG))
								{
									pak->binary = BINARY_YES;
									break;
								}
							}
						}
						if(pak->binary == BINARY_NO)
						{
							continue; // not a binary pak, skip
						}
					}

					for(pakFile = pak->hashTable[hash]; pakFile; pakFile = pakFile->next)
					{
						// case and separator insensitive comparisons
						if(!FilenameCompare(pakFile->name, relativePath))
						{
							idFile_InZip* file = ReadFileFromZip(pak, pakFile, relativePath);

							if(foundInPak)
							{
								*foundInPak = pak;
							}

							if(!pak->referenced && !(searchFlags & FSFLAG_PURE_NOREF))
							{
								// mark this pak referenced
								if(fs_debug.GetInteger())
								{
									common->Printf("idFileSystem::OpenFileRead: %s -> adding %s to referenced paks\n", relativePath, pak->pakFilename.c_str());
								}
								pak->referenced = true;
							}

							if(fs_debug.GetInteger())
							{
								common->Printf("idFileSystem::OpenFileRead: %s (found in '%s')\n", relativePath, pak->pakFilename.c_str());
							}
							return file;
						}
					}
				}
			}*/

				// TODO
				/*if ( searchFlags & FSFLAG_SEARCH_ADDONS ) {
					for ( search = addonPaks; search; search = search->next ) {
						assert( search->pack );
						fileInPack_t	*pakFile;
						pak = search->pack;
						for ( pakFile = pak->hashTable[hash]; pakFile; pakFile = pakFile->next ) {
							if ( !FilenameCompare( pakFile->name, relativePath ) ) {
								idFile_InZip *file = ReadFileFromZip( pak, pakFile, relativePath );
								if ( foundInPak ) {
									*foundInPak = pak;
								}
								// we don't toggle pure on paks found in addons - they can't be used without a reloadEngine anyway
								if ( fs_debug.GetInteger( ) ) {
									common->Printf( "idFileSystem::OpenFileRead: %s (found in addon pk4 '%s')\n", relativePath, search->pack->pakFilename.c_str() );
								}
								return file;
							}
						}
					}*/
			}

			if(idE.CvarSystem.GetInteger("fs_debug") > 0)
			{
				idConsole.WriteLine("Can't find {0}", relativePath);
			}

			return null;
		}

		public Stream OpenFileWrite(string relativePath)
		{
			return OpenFileWrite(relativePath, "fs_savepath");
		}

		public Stream OpenFileWrite(string relativePath, string basePath)
		{
			if(this.IsInitialized == false)
			{
				idConsole.FatalError("Filesystem call made without initialization");
			}

			string path = idE.CvarSystem.GetString(basePath);

			if(path == string.Empty)
			{
				path = idE.CvarSystem.GetString("fs_savepath");
			}

			string osPath = CreatePath(path, _gameFolder, relativePath);

			if(idE.CvarSystem.GetInteger("fs_debug") > 0)
			{
				idConsole.WriteLine("idFileSystem::OpenFileWrite: {0}", osPath);
			}

			idConsole.DeveloperWriteLine("writing to: {0}", osPath);

			try
			{
				Stream s = OpenOSFile(osPath, FileMode.Create, FileAccess.Write);

				if(s == null)
				{
					return null;
				}

				return s;
			}
			catch(IOException x)
			{
				idConsole.WriteLine("Could not open '{0}' because: {1}", osPath, x.Message);
			}

			return null;
		}

		public void QueueBackgroundLoad(BackgroundDownload backgroundDownload)
		{
			_backgroundDownloads.Enqueue(backgroundDownload);

			// TODO: will we handle downloads differently to files?  they're all streams
			// at the end of the day.
			/*if ( bgl->opcode == DLTYPE_FILE ) {
				if ( dynamic_cast<idFile_Permanent *>(bgl->f) ) {
					// add the bgl to the background download list
					Sys_EnterCriticalSection();
					bgl->next = backgroundDownloads;
					backgroundDownloads = bgl;
					Sys_TriggerEvent();
					Sys_LeaveCriticalSection();
				} else {
					// read zipped file directly
					bgl->f->Seek( bgl->file.position, FS_SEEK_SET );
					bgl->f->Read( bgl->file.buffer, bgl->file.length );
					bgl->completed = true;
				}
			} else {
				Sys_EnterCriticalSection();
				bgl->next = backgroundDownloads;
				backgroundDownloads = bgl;
				Sys_TriggerEvent();
				Sys_LeaveCriticalSection();
			}*/
		}

		public byte[] ReadFile(string relativePath)
		{
			DateTime tmp;
			return ReadFile(relativePath, out tmp);
		}

		/// <summary>
		/// Reads a complete file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="timeStamp"></param>
		/// <returns>null if loading failed (file did not exist or other issue).</returns>
		public byte[] ReadFile(string relativePath, out DateTime timeStamp)
		{
			if(_searchPaths.Count == 0)
			{
				idConsole.FatalError("Filesystem call made without initialization");
			}

			if(relativePath == string.Empty)
			{
				idConsole.FatalError("read file with empty name");
			}

			timeStamp = DateTime.Now;

			bool isConfig = false;

			// if this is a .cfg file and we are playing back a journal, read
			// it from the journal file
			// TODO
			/*if(strstr(relativePath, ".cfg") == relativePath + strlen(relativePath) - 4)
			{
				isConfig = true;
				if(eventLoop && eventLoop->JournalLevel() == 2)
				{
					int r;

					loadCount++;
					loadStack++;

					common->DPrintf("Loading %s from journal file.\n", relativePath);
					len = 0;
					r = eventLoop->com_journalDataFile->Read(&len, sizeof(len));
					if(r != sizeof(len))
					{
						*buffer = NULL;
						return -1;
					}
					buf = (byte*) Mem_ClearedAlloc(len + 1);
					*buffer = buf;
					r = eventLoop->com_journalDataFile->Read(buf, len);
					if(r != len)
					{
						common->FatalError("Read from journalDataFile failed");
					}

					// guarantee that it will have a trailing 0 for string operations
					buf[len] = 0;

					return len;
				}
			}
			else
			{
				isConfig = false;
			}*/

			// look for it in the filesystem or pack files
			Stream s = OpenFileRead(relativePath);

			if(s == null)
			{
				return null;
			}

			// TODO
			/*if(timestamp)
			{
				*timestamp = f->Timestamp();
			}*/

			_loadCount++;
			_loadStack++;

			byte[] data = new byte[s.Length];
			s.Read(data, 0, data.Length);

			// TODO
			// if we are journalling and it is a config file, write it to the journal file
			/*if(isConfig && eventLoop && eventLoop->JournalLevel() == 1)
			{
				common->DPrintf("Writing %s to journal file.\n", relativePath);
				eventLoop->com_journalDataFile->Write(&len, sizeof(len));
				eventLoop->com_journalDataFile->Write(buf, len);
				eventLoop->com_journalDataFile->Flush();
			}*/

			return data;
		}

		public string RelativePathToOSPath(string relativePath, string basePath)
		{
			string path = idE.CvarSystem.GetString(basePath);

			if(path == null)
			{
				path = idE.CvarSystem.GetString("fs_savepath");
			}

			return CreatePath(path, _gameFolder, relativePath);
		}
		#endregion

		#region Private
		private void Startup()
		{
			idConsole.WriteLine("------ Initializing File System ------");

			if(_restartChecksums.Count > 0)
			{
				idConsole.WriteLine("restarting in pure mode with {0} pak files", _restartChecksums.Count);
			}

			if(_addonChecksums.Count > 0)
			{
				idConsole.WriteLine("restarting filesystem with {0} addon pak file(s) to include", _addonChecksums.Count);
			}

			SetupGameDirectories(idE.BaseGameDirectory);

			// fs_game_base override
			if((idE.CvarSystem.GetString("fs_game_base") != string.Empty) && (StringComparer.InvariantCultureIgnoreCase.Compare(idE.CvarSystem.GetString("fs_game_base"), idE.BaseGameDirectory) != 0))
			{
				SetupGameDirectories(idE.CvarSystem.GetString("fs_game_base"));
			}

			// fs_game override
			if((idE.CvarSystem.GetString("fs_game") != string.Empty)
				&& (StringComparer.InvariantCultureIgnoreCase.Compare(idE.CvarSystem.GetString("fs_game"), idE.BaseGameDirectory) != 0)
				&& (StringComparer.InvariantCultureIgnoreCase.Compare(idE.CvarSystem.GetString("fs_base"), idE.CvarSystem.GetString("fs_game_base")) != 0))
			{
				SetupGameDirectories(idE.CvarSystem.GetString("fs_game"));
			}

			// currently all addons are in the search list - deal with filtering out and dependencies now
			// scan	through and deal with dependencies

			// TODO: we don't deal with addon paks yet.
			#region
			/*search = &searchPaths;
			while ( *search ) {
				if ( !( *search )->pack || !( *search )->pack->addon ) {
					search = &( ( *search )->next );
					continue;
				}
				pak = ( *search )->pack;
				if ( fs_searchAddons.GetBool() ) {
					// when we have fs_searchAddons on we should never have addonChecksums
					assert( !addonChecksums.Num() );
					pak->addon_search = true;
					search = &( ( *search )->next );
					continue;
				}
				addon_index = addonChecksums.FindIndex( pak->checksum );
				if ( addon_index >= 0 ) {
					assert( !pak->addon_search );	// any pak getting flagged as addon_search should also have been removed from addonChecksums already
					pak->addon_search = true;
					addonChecksums.RemoveIndex( addon_index );
					FollowAddonDependencies( pak );
				}
				search = &( ( *search )->next );
			}

			// now scan to filter out addons not marked addon_search
			search = &searchPaths;
			while ( *search ) {
				if ( !( *search )->pack || !( *search )->pack->addon ) {
					search = &( ( *search )->next );
					continue;
				}
				assert( !( *search )->dir );
				pak = ( *search )->pack;
				if ( pak->addon_search ) {
					common->Printf( "Addon pk4 %s with checksum 0x%x is on the search list\n",
									pak->pakFilename.c_str(), pak->checksum );
					search = &( ( *search )->next );
				} else {
					// remove from search list, put in addons list
					searchpath_t *paksearch = *search;
					*search = ( *search )->next;
					paksearch->next = addonPaks;
					addonPaks = paksearch;
					common->Printf( "Addon pk4 %s with checksum 0x%x is on addon list\n",
									pak->pakFilename.c_str(), pak->checksum );				
				}
			}
			 

			// all addon paks found and accounted for
			assert( !addonChecksums.Num() );
			addonChecksums.Clear();	// just in case*/

			// TODO: restart checksums
			/*if ( restartChecksums.Num() ) {
				search = &searchPaths;
				while ( *search ) {
					if ( !( *search )->pack ) {
						search = &( ( *search )->next );
						continue;
					}
					if ( ( i = restartChecksums.FindIndex( ( *search )->pack->checksum ) ) != -1 ) {
						if ( i == 0 ) {
							// this pak is the next one in the pure search order
							serverPaks.Append( ( *search )->pack );
							restartChecksums.RemoveIndex( 0 );
							if ( !restartChecksums.Num() ) {
								break; // early out, we're done
							}
							search = &( ( *search )->next );
							continue;
						} else {
							// this pak will be on the pure list, but order is not right yet
							searchpath_t	*aux;
							aux = ( *search )->next;
							if ( !aux ) {
								// last of the list can't be swapped back
								if ( fs_debug.GetBool() ) {
									common->Printf( "found pure checksum %x at index %d, but the end of search path is reached\n", ( *search )->pack->checksum, i );
									idStr checks;
									checks.Clear();
									for ( i = 0; i < serverPaks.Num(); i++ ) {
										checks += va( "%p ", serverPaks[ i ] );
									}
									common->Printf( "%d pure paks - %s \n", serverPaks.Num(), checks.c_str() );
									checks.Clear();
									for ( i = 0; i < restartChecksums.Num(); i++ ) {
										checks += va( "%x ", restartChecksums[ i ] );
									}
									common->Printf( "%d paks left - %s\n", restartChecksums.Num(), checks.c_str() );
								}
								common->FatalError( "Failed to restart with pure mode restrictions for server connect" );
							}
							// put this search path at the end of the list
							searchpath_t *search_end;
							search_end = ( *search )->next;
							while ( search_end->next ) {
								search_end = search_end->next;
							}
							search_end->next = *search;
							*search = ( *search )->next;
							search_end->next->next = NULL;
							continue;
						}
					}
					// this pak is not on the pure list
					search = &( ( *search )->next );
				}
				// the list must be empty
				if ( restartChecksums.Num() ) {
					if ( fs_debug.GetBool() ) {
						idStr checks;
						checks.Clear();
						for ( i = 0; i < serverPaks.Num(); i++ ) {
							checks += va( "%p ", serverPaks[ i ] );
						}
						common->Printf( "%d pure paks - %s \n", serverPaks.Num(), checks.c_str() );
						checks.Clear();
						for ( i = 0; i < restartChecksums.Num(); i++ ) {
							checks += va( "%x ", restartChecksums[ i ] );
						}
						common->Printf( "%d paks left - %s\n", restartChecksums.Num(), checks.c_str() );
					}
					common->FatalError( "Failed to restart with pure mode restrictions for server connect" );
				}
				// also the game pak checksum
				// we could check if the game pak is actually present, but we would not be restarting if there wasn't one @ first pure check
				gamePakChecksum = restartGamePakChecksum;
			}*/
			#endregion

			// add our commands
			// TODO: not too fussed about these commands right now
			idE.CmdSystem.AddCommand("dir", "lists a folder", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Dir)/* TODO: idCmdSystem::ArgCompletion_FileName*/);
			/*cmdSystem->AddCommand( "dirtree", DirTree_f, CMD_FL_SYSTEM, "lists a folder with subfolders" );*/
			idE.CmdSystem.AddCommand("path", "lists search paths", CommandFlags.System, new EventHandler<CommandEventArgs>(Cmd_Path));
			/*cmdSystem->AddCommand( "touchFile", TouchFile_f, CMD_FL_SYSTEM, "touches a file" );
			cmdSystem->AddCommand( "touchFileList", TouchFileList_f, CMD_FL_SYSTEM, "touches a list of files" );*/

			// print the current search paths
			Cmd_Path(this, new CommandEventArgs(new idCmdArgs()));

			idConsole.WriteLine("file system initialized.");
			idConsole.WriteLine("--------------------------------------");
		}

		private void StartBackgroundDownloadThread()
		{
			ThreadStart threadStart = new ThreadStart(ProcessBackgroundDownloadThread);
			_backgroundDownloadThread = new Thread(threadStart);
			_backgroundDownloadThread.IsBackground = true;
			_backgroundDownloadThread.Start();
		}

		/// <summary>
		/// Sets gameFolder, adds the directory to the head of the search paths, then loads any pk4 files.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="directory"></param>
		private void AddGameDirectory(string path, string directory)
		{
			// check if the search path already exists
			foreach(SearchPath searchPath in _searchPaths)
			{
				// if this element is a pak file
				if(searchPath.Directory == null)
				{
					continue;
				}

				if((searchPath.Directory.Path == path) && (searchPath.Directory.GameDirectory == directory))
				{
					return;
				}
			}

			_gameFolder = directory;

			// add the directory to the search path.
			SearchPath search = new SearchPath();
			search.Directory = new idDirectory(path, directory);

			_searchPaths.Add(search);

			// find all pak files in this directory
			string pakFile = CreatePath(path, directory, "");
			string[] pakFiles = Directory.GetFiles(pakFile, "*.pk4", SearchOption.TopDirectoryOnly);

			// sort them so that later alphabetic matches override
			// earlier ones. This makes pak1.pk4 override pak0.pk4
			pakFiles = pakFiles.OrderByDescending(x => x).ToArray();

			foreach(string pakName in pakFiles)
			{
				Pack pak = LoadZipFile(pakName);

				if(pak == null)
				{
					continue;
				}

				// insert the pak after the directory it comes from
				search = new SearchPath();
				search.Pack = pak;

				_searchPaths.Add(search);

				idConsole.WriteLine("Loaded pk4 {0}", pakName);
			}
		}

		/// <summary>
		/// Takes care of the correct search order.
		/// </summary>
		private void SetupGameDirectories(string gameName)
		{
			// setup cdpath
			if(idE.CvarSystem.GetString("fs_cdpath") != string.Empty)
			{
				AddGameDirectory(idE.CvarSystem.GetString("fs_cdpath"), gameName);
			}

			// setup basepath
			if(idE.CvarSystem.GetString("fs_basepath") != string.Empty)
			{
				AddGameDirectory(idE.CvarSystem.GetString("fs_basepath"), gameName);
			}

			// setup devpath
			if(idE.CvarSystem.GetString("fs_devpath") != string.Empty)
			{
				AddGameDirectory(idE.CvarSystem.GetString("fs_devpath"), gameName);
			}

			// setup savepath
			if(idE.CvarSystem.GetString("fs_savepath") != string.Empty)
			{
				AddGameDirectory(idE.CvarSystem.GetString("fs_savepath"), gameName);
			}
		}


		private string[] GetFileList(string relativePath, string[] extensions, bool fullRelativePath)
		{
			return GetFileList(relativePath, extensions, fullRelativePath, null);
		}

		/// <summary>
		/// Does not clear the list first so this can be used to progressively build a file list.
		/// When 'sort' is true only the new files added to the list are sorted.
		/// </summary>
		private string[] GetFileList(string relativePath, string[] extensions, bool fullRelativePath, string gameDirectory)
		{
			List<string> fileList = new List<string>();

			if(_searchPaths.Count == 0)
			{
				idConsole.FatalError("Filesystem call made without initialization");
			}

			string[] dirExtensions = extensions;

			// build extension search pattern
			for(int i = 0; i < dirExtensions.Length; i++)
			{
				if(dirExtensions[i] != "/")
				{
					dirExtensions[i] = string.Format("*{0}", dirExtensions[i]);
				}
			}

			// search through the path, one element at a time, adding to list
			foreach(SearchPath searchPath in _searchPaths)
			{
				if(searchPath.Directory != null)
				{
					string extensionPattern = String.Join("|", dirExtensions);

					if(extensionPattern == string.Empty)
					{
						extensionPattern = "*";
					}

					if((gameDirectory != null) && (gameDirectory != string.Empty))
					{
						if(searchPath.Directory.GameDirectory != gameDirectory)
						{
							continue;
						}
					}

					string netPath = CreatePath(searchPath.Directory.Path, searchPath.Directory.GameDirectory, relativePath);

					// scan for files in the filesystem
					if(Directory.Exists(netPath) == false)
					{
						continue;
					}

					string[] tmp;

					if((dirExtensions.Length == 1) && (dirExtensions[0] == "/"))
					{
						tmp = Directory.GetDirectories(netPath, "*", SearchOption.TopDirectoryOnly);
					}
					else
					{
						try
						{
							tmp = Directory.GetFiles(netPath, extensionPattern, SearchOption.TopDirectoryOnly);
						}
						catch
						{
							tmp = new string[] { };
						}
					}

					List<string> sysFiles = new List<string>(tmp);

					sysFiles.Remove(".");
					sysFiles.Remove("..");

					// if we are searching for directories, remove . and ..					
					for(int j = 0; j < sysFiles.Count; j++)
					{
						sysFiles[j] = sysFiles[j].Substring(netPath.Length + 1);

						if(fullRelativePath == true)
						{
							sysFiles[j] = Path.Combine(relativePath, sysFiles[j]);
						}
					}

					fileList.AddRange(sysFiles);
				}
				else if(searchPath.Pack != null)
				{
					// look through all the pak file elements

					// exclude any extra packs if we have server paks to search
					// TODO
					/*if ( serverPaks.Num() ) {
						GetPackStatus( search->pack );
						if ( search->pack->pureStatus != PURE_NEVER && !serverPaks.Find( search->pack ) ) {
							continue; // not on the pure server pak list
						}
					}*/

					ZipFile zipFile = searchPath.Pack.Zip;
					int pathLength = relativePath.Length;

					if(pathLength > 0)
					{
						pathLength++;
					}

					// TODO: profile this to see if it's faster to cache the file table.
					foreach(ZipEntry zipEntry in zipFile)
					{
						string name = zipEntry.Name;

						// if the name is not long anough to at least contain the path.
						if(name.Length <= pathLength)
						{
							continue;
						}

						// check for a path match without the trailing '/'
						if((pathLength > 0) && (StringComparer.InvariantCultureIgnoreCase.Compare(name.Substring(0, pathLength - 1), relativePath) != 0))
						{
							continue;
						}

						// make sure the file is not in a subdirectory
						if(name.IndexOf("/", pathLength) != -1)
						{
							continue;
						}

						// check for extension match
						bool extMatch = false;

						if(extensions.Length == 0)
						{
							extMatch = true;
						}
						else
						{
							foreach(string ext in extensions)
							{
								if(name.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase) == true)
								{
									extMatch = true;
									break;
								}
							}
						}

						if(extMatch == false)
						{
							continue;
						}

						if(fullRelativePath == true)
						{
							fileList.Add(Path.Combine(relativePath, name).TrimEnd('/'));
						}
						else
						{
							fileList.Add(name.TrimEnd('/'));
						}
					}
				}
			}

			return fileList.ToArray();
		}

		private Pack LoadZipFile(string zip)
		{
			ZipFile zipFile = new ZipFile(zip);

			Pack pack = new Pack();
			pack.FileName = zip;
			pack.Zip = zipFile;
			pack.FileCount = (int) zipFile.Count;

			// TODO: check if this is an addon pak
			/*pack->addon = false;
			confHash = HashFileName( ADDON_CONFIG );
			for ( pakFile = pack->hashTable[confHash]; pakFile; pakFile = pakFile->next ) {
				if ( !FilenameCompare( pakFile->name, ADDON_CONFIG ) ) {			
					pack->addon = true;			
					idFile_InZip *file = ReadFileFromZip( pack, pakFile, ADDON_CONFIG );
					// may be just an empty file if you don't bother about the mapDef
					if ( file && file->Length() ) {
						char *buf;
						buf = new char[ file->Length() + 1 ];
						file->Read( (void *)buf, file->Length() );
						buf[ file->Length() ] = '\0';
						pack->addon_info = ParseAddonDef( buf, file->Length() );
						delete[] buf;
					}
					if ( file ) {
						CloseFile( file );
					}
					break;
				}
			}*/

			return pack;
		}

		/// <summary>
		/// Some files can be obtained from directories without compromising si_pure.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private bool FileAllowedFromDirectory(string path)
		{
			if((path.EndsWith(".cfg") == true) // for config files.
				|| (path.EndsWith(".dat") == true) // for journal files.
				|| (path.EndsWith(".dll") == true) // dynamic modules are handled a different way for pure.
				|| (path.EndsWith(".scriptcfg") == true)) // configuration script, such as map cycle.
			{
				// TODO
				/*#if ID_PURE_ALLOWDDS
						 || !strcmp( path + l - 4, ".dds" )
				#endif*/
				// note: cd and xp keys, as well as config.spec are opened through an explicit OS path and don't hit this
				return true;
			}

			// savegames
			if((path.StartsWith("savegames") == true) && ((path.EndsWith(".tga") == true) || (path.EndsWith(".txt") == true) || (path.EndsWith(".save") == true)))
			{
				return true;
			}

			// screen shots
			if((path.StartsWith("screenshots") == true) && (path.EndsWith(".tga") == true))
			{
				return true;
			}

			// objective tgas
			if((path.StartsWith("maps/game") == true) && (path.EndsWith(".tga") == true))
			{
				return true;
			}

			// splash screens extracted from addons
			if((path.StartsWith("guis/assets/splash/addon") == true) && (path.EndsWith(".tga") == true))
			{
				return true;
			}

			return false;
		}
		#endregion

		#region Command handlers
		private void Cmd_Path(object sender, CommandEventArgs e)
		{
			idConsole.WriteLine("Current search path:");

			foreach(SearchPath path in _searchPaths)
			{
				if(path.Pack != null)
				{
					if(idE.CvarSystem.GetBool("developer") == true)
					{
						if(path.Pack.IsAddon == true)
						{
							idConsole.WriteLine("{0} ({1} files - {2} - addon)", path.Pack.FileName, path.Pack.FileCount, (path.Pack.IsReferenced == true) ? "referenced" : "not referenced");
						}
						else
						{
							idConsole.WriteLine("{0} ({1} files - {2})", path.Pack.FileName, path.Pack.FileCount, (path.Pack.IsReferenced == true) ? "referenced" : "not referenced");
						}
					}
					else
					{
						idConsole.WriteLine("{0} ({1} files)", path.Pack.FileName, path.Pack.FileCount);
					}

					// TODO
					/*if(fileSystemLocal.serverPaks.Num())
					{
						if(fileSystemLocal.serverPaks.Find(sp->pack))
						{
							common->Printf("    on the pure list\n");
						}
						else
						{
							common->Printf("    not on the pure list\n");
						}
					}*/
				}
				else
				{
					idConsole.WriteLine(Path.Combine(path.Directory.Path, path.Directory.GameDirectory));
				}
			}

			// TODO
			/*common->Printf("game DLL: 0x%x in pak: 0x%x\n", fileSystemLocal.gameDLLChecksum, fileSystemLocal.gamePakChecksum);*/

			/*for(i = 0; i < MAX_GAME_OS; i++)
			{
				if(fileSystemLocal.gamePakForOS[i])
				{
					common->Printf("OS %d - pak 0x%x\n", i, fileSystemLocal.gamePakForOS[i]);
				}
			}*/

			// show addon packs that are *not* in the search lists
			/*common->Printf("Addon pk4s:\n");
			for(sp = fileSystemLocal.addonPaks; sp; sp = sp->next)
			{
				if(com_developer.GetBool())
				{
					common->Printf("%s (%i files - 0x%x)\n", sp->pack->pakFilename.c_str(), sp->pack->numfiles, sp->pack->checksum);
				}
				else
				{
					common->Printf("%s (%i files)\n", sp->pack->pakFilename.c_str(), sp->pack->numfiles);
				}
			}*/
		}


		private Stream OpenOSFile(string name, FileMode mode, FileAccess access)
		{
			string caseSensitiveName;
			return OpenOSFile(name, mode, access, out caseSensitiveName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="mode"></param>
		/// <param name="caseSensitiveName">Set to case sensitive file name as found on disc (fs_caseSensitiveOS only).</param>
		/// <returns></returns>
		private Stream OpenOSFile(string name, FileMode mode, FileAccess access, out string caseSensitiveName)
		{
			Stream s = null;

			if(File.Exists(name) == false)
			{
				if((mode == FileMode.Create) || (mode == FileMode.CreateNew) || (mode == FileMode.OpenOrCreate))
				{
					s = File.Create(name);
				}
			}
			else
			{
				s = File.Open(name, mode, access, FileShare.Read);
			}

			caseSensitiveName = string.Empty;

			if((s == null) && (idE.CvarSystem.GetBool("fs_caseSensitiveOS") == true))
			{
				idConsole.WriteLine("UH OH, should really handle fs_caseSensitiveOS");

				return null;

				/*fpath = fileName;
				fpath.StripFilename();
				fpath.StripTrailing( PATHSEPERATOR_CHAR );
				if ( ListOSFiles( fpath, NULL, list ) == -1 ) {
					return NULL;
				}
		
				for ( i = 0; i < list.Num(); i++ ) {
					entry = fpath + PATHSEPERATOR_CHAR + list[i];
					if ( !entry.Icmp( fileName ) ) {
						fp = fopen( entry, mode );
						if ( fp ) {
							if ( caseSensitiveName ) {
								*caseSensitiveName = entry;
								caseSensitiveName->StripPath();
							}
							if ( fs_debug.GetInteger() ) {
								common->Printf( "idFileSystemLocal::OpenFileRead: changed %s to %s\n", fileName, entry.c_str() );
							}
							break;
						} else {
							// not supposed to happen if ListOSFiles is doing it's job correctly
							common->Warning( "idFileSystemLocal::OpenFileRead: fs_caseSensitiveOS 1 could not open %s", entry.c_str() );
						}
					}
				}*/
			}

			caseSensitiveName = Path.GetFileName(name);

			return s;
		}

		private void ProcessBackgroundDownloadThread()
		{
			while(true)
			{
				if(_backgroundDownloads.Count != 0)
				{
					BackgroundDownload backgroundDownload = _backgroundDownloads.Dequeue();

					if(backgroundDownload.Type == DownloadType.File)
					{
						throw new Exception("X");
						/*#if defined(WIN32)
							_read( static_cast<idFile_Permanent*>(bgl->f)->GetFilePtr()->_file, bgl->file.buffer, bgl->file.length );
						#else
							fread(  bgl->file.buffer, bgl->file.length, 1, static_cast<idFile_Permanent*>(bgl->f)->GetFilePtr() );
						#endif*/

						backgroundDownload.Completed = true;
					}
					else
					{
						/*#if ID_ENABLE_CURL
									// DLTYPE_URL
									// use a local buffer for curl error since the size define is local
									char error_buf[ CURL_ERROR_SIZE ];
									bgl->url.dlerror[ 0 ] = '\0';
									CURL *session = curl_easy_init();
									CURLcode ret;
									if ( !session ) {
										bgl->url.dlstatus = CURLE_FAILED_INIT;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									ret = curl_easy_setopt( session, CURLOPT_ERRORBUFFER, error_buf );
									if ( ret ) {
										bgl->url.dlstatus = ret;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									ret = curl_easy_setopt( session, CURLOPT_URL, bgl->url.url.c_str() );
									if ( ret ) {
										bgl->url.dlstatus = ret;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									ret = curl_easy_setopt( session, CURLOPT_FAILONERROR, 1 );
									if ( ret ) {
										bgl->url.dlstatus = ret;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									ret = curl_easy_setopt( session, CURLOPT_WRITEFUNCTION, idFileSystemLocal::CurlWriteFunction );
									if ( ret ) {
										bgl->url.dlstatus = ret;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									ret = curl_easy_setopt( session, CURLOPT_WRITEDATA, bgl );
									if ( ret ) {
										bgl->url.dlstatus = ret;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									ret = curl_easy_setopt( session, CURLOPT_NOPROGRESS, 0 );
									if ( ret ) {
										bgl->url.dlstatus = ret;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									ret = curl_easy_setopt( session, CURLOPT_PROGRESSFUNCTION, idFileSystemLocal::CurlProgressFunction );
									if ( ret ) {
										bgl->url.dlstatus = ret;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									ret = curl_easy_setopt( session, CURLOPT_PROGRESSDATA, bgl );
									if ( ret ) {
										bgl->url.dlstatus = ret;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									bgl->url.dlnow = 0;
									bgl->url.dltotal = 0;
									bgl->url.status = DL_INPROGRESS;
									ret = curl_easy_perform( session );
									if ( ret ) {
										Sys_Printf( "curl_easy_perform failed: %s\n", error_buf );
										idStr::Copynz( bgl->url.dlerror, error_buf, MAX_STRING_CHARS );
										bgl->url.dlstatus = ret;
										bgl->url.status = DL_FAILED;
										bgl->completed = true;
										continue;
									}
									bgl->url.status = DL_DONE;
									bgl->completed = true;
						#else
									bgl->url.status = DL_FAILED;
									bgl->completed = true;
						#endif*/
					}
				}

				Thread.Sleep(0);
			}
		}
		#endregion

		#region Command handlers
		private void Cmd_Dir(object sender, CommandEventArgs e)
		{
			if((e.Args.Length < 2) || (e.Args.Length > 3))
			{
				idConsole.WriteLine("usage: dir <directory> [extension]");
			}
			else
			{
				string relativePath = string.Empty;
				string extension = string.Empty;

				if(e.Args.Length == 2)
				{
					relativePath = e.Args.Get(1);
				}
				else
				{
					relativePath = e.Args.Get(1);
					extension = e.Args.Get(2);

					if(extension.StartsWith(".") == false)
					{
						idConsole.Warning("extension should have a leading dot");
					}
				}

				//relativePath = relativePath.Replace('\\', '/');
				relativePath = relativePath.TrimEnd('/');

				idConsole.WriteLine("Listing of {0}/*{1}", relativePath, extension);
				idConsole.WriteLine("---------------");

				idFileList fileList = GetFiles(relativePath, extension);

				foreach(string file in fileList.Files)
				{
					idConsole.WriteLine("{0}", file);
				}

				idConsole.WriteLine("{0} files", fileList.Files.Length);
			}
		}
		#endregion
		#endregion
	}

	public enum FileSearch
	{
		SearchDirectories = (1 << 0),
		SearchPaks = (1 << 1),
		Pure = (1 << 2),
		BinaryOnly = (1 << 3),
		SearchAddons = (1 << 4)
	}

	public enum DownloadType
	{
		Url,
		File
	}

	public class BackgroundDownload
	{
		public DownloadType Type;
		public Stream Stream;
		/*typedef struct backgroundDownload_s {
		struct backgroundDownload_s	*next;	// set by the fileSystem
		fileDownload_t		file;
		urlDownload_t		url;
	} backgroundDownload_t;*/

		public bool Completed;
	}

	public sealed class Pack
	{
		public string FileName; // c:\doom\base\pak0.pk4
		public ZipFile Zip;
		public int FileCount;

		public bool IsReferenced;
		public bool IsAddon; // this is an addon pack - addon_search tells if it's 'active'.
		// TODO
		/*
		int					checksum;
		int					length;
		binaryStatus_t		binary;
			
		bool				addon_search;				// is in the search list
		addonInfo_t			*addon_info;
		pureStatus_t		pureStatus;
		bool				isNew;						// for downloaded paks
		fileInPack_t		*hashTable[FILE_HASH_SIZE];
		fileInPack_t		*buildBuffer;
	} pack_t;*/
	}

	public sealed class idDirectory
	{
		public string Path; // c:\doom
		public string GameDirectory; // base

		public idDirectory(string path, string directory)
		{
			this.Path = path;
			this.GameDirectory = directory;
		}
	}

	public struct SearchPath
	{
		public Pack Pack; // only one of pack/dir will be non null.
		public idDirectory Directory;
	}

	public sealed class idFileList
	{
		public string BaseDirectory;
		public string[] Files;

		public idFileList(string baseDirectory, string[] files)
		{
			this.BaseDirectory = baseDirectory;
			this.Files = files;
		}
	}
}