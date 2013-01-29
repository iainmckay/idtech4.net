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
using System.Linq;
using System.Runtime.InteropServices;

using idTech4.Services;

namespace idTech4.IO
{
	/// <summary>
	/// File System.
	/// </summary>
	/// <remarks>
	/// No stdio calls should be used by any part of the game, because of all sorts
	/// of directory and separator char issues. Throughout the game a forward slash
	/// should be used as a separator. The file system takes care of the conversion
	/// to an OS specific separator. The file system treats all file and directory
	/// names as case insensitive.
	/// <p/>
	/// The following cvars store paths used by the file system:
	///	"fs_basepath"		path to local install
	/// "fs_savepath"		path to config, save game, etc. files, read & write
	/// <para/>
	/// The base path for file saving can be set to "fs_savepath" or "fs_basepath".
	/// <para/>
	/// Note: Unlike Doom 3 BFG, we do not use a binary resource file, we still use zip files.
	/// </remarks>
	public class idFileSystem : IFileSystem
	{
		#region Members
		private bool _initialized;

		private string _gameFolder;	// this will be a single name without separators
		private List<SearchPath> _searchPaths = new List<SearchPath>();
		private List<idResourceContainer> _resourceContainers = new List<idResourceContainer>();

		private string _manifestName;
		private List<string> _fileManifest = new List<string>();
		// TODO: private idPreloadManifest _preloadList = new idPreloadManifest();

		private int _filesOpenedAsCachedCount;
		#endregion

		#region Constructor
		public idFileSystem()
		{

		}		
		#endregion

		#region Methods
		#region Initialization
		private void Startup()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			ICommandSystem cmdSystem = idEngine.Instance.GetService<ICommandSystem>();

			idLog.WriteLine("------ Initializing File System ------");

			InitPrecache();
			SetupGameDirectories(idLicensee.BaseGameDirectory);			

			// fs_game_base override
			string gameBase = cvarSystem.GetString("fs_game_base");

			if((gameBase != string.Empty) && (gameBase != idLicensee.BaseGameDirectory))
			{
				SetupGameDirectories(gameBase);
			}

			// fs_game override
			string game = cvarSystem.GetString("fs_game");

			if((game != string.Empty) 
				&& (game.Equals(idLicensee.BaseGameDirectory, StringComparison.OrdinalIgnoreCase) == false)
				&& (game.Equals(gameBase, StringComparison.OrdinalIgnoreCase) == false))
			{
				SetupGameDirectories(game);
			}

			// print the current search paths
			cmdSystem.BufferCommandText("path", Execute.Now);

			idLog.WriteLine("file system initialized.");
			idLog.WriteLine("--------------------------------------");
		}

		/// <summary>
		/// Takes care of the correct search order.
		/// </summary>
		/// <param name="gameName"></param>
		private void SetupGameDirectories(string gameName)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			// setup basepath
			if(cvarSystem.GetString("fs_basepath") != string.Empty)
			{
				AddGameDirectory(cvarSystem.GetString("fs_basepath"), gameName);
			}

			// setup savepath
			if(cvarSystem.GetString("fs_savepath") != string.Empty)
			{
				AddGameDirectory(cvarSystem.GetString("fs_savepath"), gameName);
			}
		}

		/// <summary>
		/// Sets gameFolder, adds the directory to the head of the search paths.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="directory"></param>
		private void AddGameDirectory(string path, string directory)
		{
			// check if the search path already exists
			foreach(SearchPath searchPath in _searchPaths)
			{
				if((searchPath.Path.Equals(path) == true)
					&& (searchPath.GameDirectory.Equals(directory) == true))
				{
					return;
				}
			}

			_gameFolder = directory;

			//
			// add the directory to the search path
			//
			_searchPaths.Add(new SearchPath(path, directory));

			string resourceDirectory = GetAbsolutePath(path, directory, string.Empty);
			
			// get a list of resource files
			string[] resourceFiles = Directory.GetFiles(resourceDirectory, "*.resources");
			Array.Sort(resourceFiles);
			Stream resourceStream;
	
			foreach(string resourceFile in resourceFiles)
			{
				resourceStream = (resourceFile == "_ordered.resources") ? OpenFileReadMemory(resourceFile) : OpenFileRead(resourceFile);

				if(resourceStream == null)
				{
					idLog.Warning("Unable to open resource file {0}", resourceFile);
				}
				else
				{
					_resourceContainers.Add(new idResourceContainer(resourceStream));

					idLog.WriteLine("Loaded resource file {0}", resourceFile);
				}
			}
		}

		private void InitPrecache()
		{
			if(idEngine.Instance.GetService<ICVarSystem>().GetBool("fs_enableBackgroundCaching") == false)
			{
				return;
			}

			_filesOpenedAsCachedCount = 0;
		}
		#endregion

		#region Resource Tracking
		private void ReOpenCacheFiles()
		{
			if(idEngine.Instance.GetService<ICVarSystem>().GetBool("fs_enableBackgroundCaching") == false)
			{
				return;
			}
		}
		#endregion
		#endregion

		#region IFileSystem implementation
		#region Checks
		/// <summary>
		/// Checks if the given physical file exists.
		/// </summary>
		/// <param name="path">Path on the underlying filesystem.</param>
		/// <returns>True if the file exists, false if not.</returns>
		public bool FileExists(string path)
		{
			Stream file = OpenFileRead(path, FileFlags.SearchDirectories);

			if(file == null)
			{
				return false;
			}

			file.Dispose();

			return true;
		}

		/// <summary>
		/// Checks if the given file exists in a resource container.
		/// </summary>
		/// <param name="path">Path inside a resource container.</param>
		/// <returns>True if the file exists, false if not.</returns>
		public bool ResourceFileExists(string path)
		{
			idLog.WriteLine("TODO: ResourceFileExists");
			return false;
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
				throw new Exception("idFileSystem has already been initialized.");
			}

			// allow command line parms to override our defaults
			// we have to specially handle this, because normal command
			// line variable sets don't happen until after the filesystem
			// has already been initialized
			idEngine engine = idEngine.Instance;

			engine.StartupVariable("fs_basepath");
			engine.StartupVariable("fs_savepath");
			engine.StartupVariable("fs_game");
			engine.StartupVariable("fs_game_base");

			ICVarSystem cvarSystem = engine.GetService<ICVarSystem>();

			if(cvarSystem.GetString("fs_basepath") == string.Empty)
			{
				cvarSystem.Set("fs_basepath", this.DefaultBasePath);
			}

			if(cvarSystem.GetString("fs_savepath") == string.Empty)
			{
				cvarSystem.Set("fs_savepath", this.DefaultSavePath);
			}

			// try to start up normally
			Startup();

			// if we can't find default.cfg, assume that the paths are
			// busted and error out now, rather than getting an unreadable
			// graphics screen when the font fails to load
			// Dedicated servers can run with no outside files at all
			if(FileExists("default.cfg") == false)
			{
				engine.FatalError("Couldn't load default.cfg");
			}

			_initialized = true;
		}
		#endregion
		#endregion

		#region Paths
		#region Properties
		public string DefaultBasePath
		{
			get
			{
				return Environment.CurrentDirectory;
			}
		}

		public string DefaultSavePath
		{
			get
			{
				string path = null;

				// only available in vista onwards
				if(Environment.OSVersion.Version.Major >= 6)
				{
					IntPtr pathPtr;
					int hr = SHGetKnownFolderPath(ref Constants.FolderID_SavedGames_IdTech5, 0, IntPtr.Zero, out pathPtr);

					if(hr == 0)
					{
						path = Marshal.PtrToStringUni(pathPtr);
						Marshal.FreeCoTaskMem(pathPtr);
					}
				}

				if(path == null)
				{
					path = Environment.GetFolderPath(Environment.SpecialFolder.Personal, Environment.SpecialFolderOption.Create);
					path += "\\My Games";
				}

				path += idLicensee.SavePath;

				return path;
			}
		}
		#endregion

		#region Methods
		public string GetAbsolutePath(string baseDirectory, string gameDirectory, string relativePath)
		{
			// handle case of this already being an absolute path
			if(Path.IsPathRooted(relativePath) == true)
			{
				return Path.GetFullPath(relativePath);
			}

			return Path.GetFullPath(Path.Combine(baseDirectory, gameDirectory, relativePath));
		}

		public idFileList ListFiles(string relativePath, string extension, bool sort = false, bool fullRelativePath = false, string gameDirectory = null)
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
					list.Add("*" + part);
				}
			}

			return list.ToArray();
		}

		private string[] GetFileList(string relativePath, string[] extensions, bool fullRelativePath, string gameDirectory)
		{
			if(extensions.Length == 0)
			{
				return new string[] { };
			}

			if(string.IsNullOrEmpty(relativePath) == true)
			{
				return new string[] { };
			}

			List<string> fileList = new List<string>();

			if(_resourceContainers.Count > 0)
			{
				idLog.Warning("TODO: important! GetFileList from resource container");

				/*int idx = resourceFiles.Num() - 1;
				while ( idx >= 0 ) {
					for ( int i = 0; i < resourceFiles[ idx ]->cacheTable.Num(); i++ ) {
						idResourceCacheEntry & rt = resourceFiles[ idx ]->cacheTable[ i ];
						// if the name is not long anough to at least contain the path

						if ( rt.filename.Length() <= pathLength ) {
							continue;
						}

						// check for a path match without the trailing '/'
						if ( pathLength && idStr::Icmpn( rt.filename, relativePath, pathLength - 1 ) != 0 ) {
							continue;
						}
 
						// ensure we have a path, and not just a filename containing the path
						if ( rt.filename[ pathLength ] == '\0' || rt.filename[pathLength - 1] != '/' ) {
							continue;
						}
 
						// make sure the file is not in a subdirectory
						int j = pathLength;
						for ( ; rt.filename[j+1] != '\0'; j++ ) {
							if ( rt.filename[ j ] == '/' ) {
								break;
							}
						}
						if ( rt.filename[ j + 1 ] ) {
							continue;
						}

						// check for extension match
						for ( j = 0; j < extensions.Num(); j++ ) {
							if ( rt.filename.Length() >= extensions[j].Length() && extensions[j].Icmp( rt.filename.c_str() +   rt.filename.Length() - extensions[j].Length() ) == 0 ) {
								break;
							}
						}
						if ( j >= extensions.Num() ) {
							continue;
						}

						// unique the match
						if ( fullRelativePath ) {
							idStr work = relativePath;
							work += "/";
							work += rt.filename.c_str() + pathLength;
							work.StripTrailing( '/' );
							AddUnique( work, list, hashIndex );
						} else {
							idStr work = rt.filename.c_str() + pathLength;
							work.StripTrailing( '/' );
							AddUnique( work, list, hashIndex );
						}
					}
					idx--;
				}*/
			}

			// search through the path, one element at a time, adding to list
			foreach(SearchPath searchPath in _searchPaths)
			{
				if(string.IsNullOrEmpty(gameDirectory) == false)
				{
					if(searchPath.GameDirectory != gameDirectory)
					{
						continue;
					}
				}

				string path = GetAbsolutePath(searchPath.Path, searchPath.GameDirectory, relativePath);

				// scan for files in the filesystem
				if(Directory.Exists(path) == false)
				{
					continue;
				}

				string[] tmp;

				try
				{
					tmp = Directory.GetFiles(path, string.Join("|", extensions), SearchOption.TopDirectoryOnly);
				}
				catch(Exception)
				{
					tmp = new string[] { };
				}

				List<string> sysFiles = new List<string>(tmp);
				sysFiles.Remove(".");
				sysFiles.Remove("..");

				// if we are searching for directories, remove . and ..					
				int count = sysFiles.Count;

				for(int j = 0; j < count; j++)
				{
					sysFiles[j] = sysFiles[j].Substring(path.Length + 1);

					if(fullRelativePath == true)
					{
						sysFiles[j] = Path.Combine(relativePath, sysFiles[j]);
					}
				}

				fileList.AddRange(sysFiles);
			}

			return fileList.ToArray();
		}
		#endregion
		#endregion

		#region Reading
		public Stream OpenFileRead(string relativePath, bool allowCopyFiles = true, string gameDirectory = null)
		{
			return OpenFileRead(relativePath, FileFlags.SearchDirectories, allowCopyFiles, gameDirectory);
		}

		public Stream OpenFileRead(string relativePath, out DateTime lastModified, bool allowCopyFiles = true, string gameDirectory = null)
		{
			return OpenFileRead(relativePath, FileFlags.SearchDirectories, out lastModified, allowCopyFiles, gameDirectory);
		}

		/// <summary>
		/// Finds the file in the search path, following search flag recommendations.
		/// </summary>
		public Stream OpenFileRead(string relativePath, FileFlags searchFlags, bool allowCopyFiles = true, string gameDirectory = null)
		{
			DateTime lastModified;

			return OpenFileRead(relativePath, searchFlags, out lastModified, allowCopyFiles, gameDirectory);
		}

		/// <summary>
		/// Finds the file in the search path, following search flag recommendations.
		/// </summary>
		public Stream OpenFileRead(string relativePath, FileFlags searchFlags, out DateTime lastModified, bool allowCopyFiles = true, string gameDirectory = null)
		{
			lastModified = DateTime.MinValue;

			if(relativePath == null)
			{
				idEngine.Instance.FatalError("idFileSystem::OpenFileRead: NULL 'relativePath' parameter passed");
				return null;
			}

			// qpaths are not supposed to have a leading slash
			if((relativePath[0] == '/') || (relativePath[0] == '\\'))
			{
				relativePath = relativePath.Substring(1);
			}

			// make absolutely sure that it can't back up the path.
			// The searchpaths do guarantee that something will always
			// be prepended, so we don't need to worry about "c:" or "//limbo" 
			if((relativePath.StartsWith("..") == true) || (relativePath.StartsWith("::") == true))
			{
				return null;
			}

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			if(cvarSystem.GetInt("fs_debug") > 0)
			{
				idLog.WriteLine("FILE DEBUG: opening {0}", relativePath);
			}

			if((_resourceContainers.Count > 0) && (cvarSystem.GetInt("fs_resourceLoadPriority") == 1))
			{
				idLog.WriteLine("TODO: GetResourceFile({0})", relativePath);

				/*idFile * rf = GetResourceFile( relativePath, ( searchFlags & FSFLAG_RETURN_FILE_MEM ) != 0, out lastModified );
				if ( rf != NULL ) {
					return rf;
				}*/
			}

			//
			// search through the path, one element at a time
			//
			if((searchFlags & FileFlags.SearchDirectories) != 0)
			{
				for(int idx = _searchPaths.Count - 1; idx >= 0; idx--)
				{
					SearchPath searchPath = _searchPaths[idx];

					if((string.IsNullOrEmpty(gameDirectory) == false) && (searchPath.GameDirectory != gameDirectory))
					{
						continue;
					}

					string filePath = GetAbsolutePath(searchPath.Path, searchPath.GameDirectory, relativePath);

					if(File.Exists(filePath) == false)
					{
						continue;
					}

					FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
					lastModified = File.GetLastWriteTime(filePath);

					if(cvarSystem.GetInt("fs_debug") > 0)
					{
						idLog.WriteLine("idFileSystem::OpenFileRead: {0} (found in '{1}')", relativePath, Path.GetDirectoryName(filePath));
					}

					// if fs_copyfiles is set
					if(allowCopyFiles == true)
					{
						idLog.WriteLine("TODO: allowCopyFiles");

						/*idStr copypath;
						idStr name;
						copypath = BuildOSPath( fs_savepath.GetString(), searchPaths[sp].gamedir, relativePath );
						netpath.ExtractFileName( name );
						copypath.StripFilename();
						copypath += PATHSEPARATOR_STR;
						copypath += name;

						if ( fs_buildResources.GetBool() ) {
							idStrStatic< MAX_OSPATH > relativePath = OSPathToRelativePath( copypath );
							relativePath.BackSlashesToSlashes();
							relativePath.ToLower();

							if ( IsSoundSample( relativePath ) ) {
								idStrStatic< MAX_OSPATH > samplePath = relativePath;
								samplePath.SetFileExtension( "idwav" );
								if ( samplePath.Find( "generated/" ) == -1 ) {
									samplePath.Insert( "generated/", 0 );
								}
								fileManifest.AddUnique( samplePath );
								if ( relativePath.Find( "/vo/", false ) >= 0 ) {
									// this is vo so add the language variants
									for ( int i = 0; i < Sys_NumLangs(); i++ ) {
										const char *lang = Sys_Lang( i );
										if ( idStr::Icmp( lang, ID_LANG_ENGLISH ) == 0 ) {
											continue;
										}
										samplePath = relativePath;
										samplePath.Replace( "/vo/", va( "/vo/%s/", lang ) );
										samplePath.SetFileExtension( "idwav" );
										if ( samplePath.Find( "generated/" ) == -1 ) {
											samplePath.Insert( "generated/", 0 );
										}
										fileManifest.AddUnique( samplePath );

									}
								}
							} else if ( relativePath.Icmpn( "guis/", 5 ) == 0 ) {
								// this is a gui so add the language variants
								for ( int i = 0; i < Sys_NumLangs(); i++ ) {
									const char *lang = Sys_Lang( i );
									if ( idStr::Icmp( lang, ID_LANG_ENGLISH ) == 0 ) {
										fileManifest.Append( relativePath );
										continue;
									}
									idStrStatic< MAX_OSPATH > guiPath = relativePath;
									guiPath.Replace( "guis/", va( "guis/%s/", lang ) );
									fileManifest.Append( guiPath );
								}
							} else {
								// never add .amp files
								if ( strstr( relativePath, ".amp" ) == NULL ) {
									fileManifest.Append( relativePath );
								}
							}

						}

						if ( fs_copyfiles.GetBool() ) {
							CopyFile( netpath, copypath );
						}*/
					}

					if((searchFlags & FileFlags.ReturnMemoryFile) != 0)
					{
						MemoryStream memoryStream = new MemoryStream((int) stream.Length);
						stream.CopyTo(memoryStream);
						stream.Dispose();

						return memoryStream;
					}

					return stream;
				}


				if((_resourceContainers.Count > 0) && (cvarSystem.GetInt("fs_resourceLoadPriority") == 0))
				{
					idLog.WriteLine("TODO: GetResourceFile({0})", relativePath);

					/*idFile * rf = GetResourceFile( relativePath, ( searchFlags & FSFLAG_RETURN_FILE_MEM ) != 0, out lastModified);
					if ( rf != NULL ) {
						return rf;
					}*/
				}
			}

			if(cvarSystem.GetInt("fs_debug") > 0)
			{
				idLog.WriteLine("FILE DEBUG: Can't find {0}", relativePath);
			}

			return null;
		}

		public Stream OpenFileReadMemory(string relativePath, bool allowCopyFiles = true, string gameDirectory = null)
		{
			return OpenFileRead(relativePath, FileFlags.SearchDirectories | FileFlags.ReturnMemoryFile, allowCopyFiles, gameDirectory);
		}

		public Stream OpenFileReadMemory(string relativePath, out DateTime lastModified, bool allowCopyFiles = true, string gameDirectory = null)
		{
			return OpenFileRead(relativePath, FileFlags.SearchDirectories | FileFlags.ReturnMemoryFile, out lastModified, allowCopyFiles, gameDirectory);
		}

		public byte[] ReadFile(string relativePath, bool allowCopyFiles = true, string gameDirectory = null)
		{
			Stream stream = OpenFileRead(relativePath, allowCopyFiles, gameDirectory);

			if(stream == null)
			{
				return null;
			}

			byte[] data = new byte[stream.Length];
			stream.Read(data, 0, data.Length);

			return data;
		}
		#endregion

		#region Resource Tracking
		#region Properties
		public bool BackgroundCacheEnabled
		{
			get
			{
				return idEngine.Instance.GetService<ICVarSystem>().GetBool("fs_enableBackgroundCaching");
			}
			set
			{
				if(idEngine.Instance.GetService<ICVarSystem>().GetBool("fs_enableBackgroundCaching") == false)
				{
					return;
				}
			}
		}

		public bool UsingResourceFiles
		{
			get
			{
				return (_resourceContainers.Count > 0);
			}
		}
		#endregion

		#region Methods
		public void BeginLevelLoad(string name /*TODO: , char *_blockBuffer, int _blockBufferSize*/)
		{
			if(string.IsNullOrEmpty(name) == true)
			{
				return;
			}

			/* TODO: resource buffer
			resourceBufferPtr = ( byte* )_blockBuffer;
			resourceBufferAvailable = _blockBufferSize;
			resourceBufferSize = _blockBufferSize;*/

			_manifestName = Path.GetFileName(name);

			_fileManifest.Clear();
			// TODO: _preloadList.Clear();

			this.BackgroundCacheEnabled = false;

			ReOpenCacheFiles();

			if(_resourceContainers.Count > 0)
			{
				idLog.Warning("TODO: AddResourceFile(string.Format(\"{0}.resources\", _manifestName));");
			}
		}

		public void EndLevelLoad()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			if(cvarSystem.GetBool("fs_buildResources") == true)
			{
				idLog.Warning("TODO: fs_buildResources");

				/*int saveCopyFiles = fs_copyfiles.GetInteger();
				fs_copyfiles.SetInteger(0);

				idStr manifestFileName = manifestName;
				manifestFileName.StripPath();
				manifestFileName.SetFileExtension("manifest");
				manifestFileName.Insert("maps/", 0);
				idFile* outFile = fileSystem->OpenFileWrite(manifestFileName);
				if(outFile != NULL)
				{
					int num = fileManifest.Num();
					outFile->WriteBig(num);
					for(int i = 0; i < num; i++)
					{
						outFile->WriteString(fileManifest[i]);
					}
					delete outFile;
				}

				idStrStatic<MAX_OSPATH> preloadName = manifestName;
				preloadName.Insert("maps/", 0);
				preloadName += ".preload";
				idFile* fileOut = fileSystem->OpenFileWrite(preloadName, "fs_savepath");
				preloadList.WriteManifestToFile(fileOut);
				delete fileOut;

				fs_copyfiles.SetInteger(saveCopyFiles);*/
			}

			this.BackgroundCacheEnabled = true;

			// TODO: resource buffer
			/*resourceBufferPtr = NULL;
			resourceBufferAvailable = 0;
			resourceBufferSize = 0;*/
		}
		#endregion
		#endregion

		#region Writing
		public Stream OpenFileWrite(string relativePath, string basePath = "fs_savepath")
		{
			idEngine engine = idEngine.Instance;
			ICVarSystem cvarSystem = engine.GetService<ICVarSystem>();

			string path = cvarSystem.GetString(basePath);

			if(string.IsNullOrEmpty(path) == true)
			{
				path = cvarSystem.GetString("fs_savepath");
			}

			path = GetAbsolutePath(path, _gameFolder, relativePath);
			
			if(cvarSystem.GetInt("fs_debug") > 0)
			{
				idLog.WriteLine("idFileSystem::OpenFileWrite: {0}", path);
			}

			idLog.DeveloperWriteLine("writing to: {0}", path);

			return File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
		}
		#endregion
		#endregion

		#region P/Invoke
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);
		#endregion
	}

	public sealed class SearchPath
	{
		public string Path; // c:\doom3
		public string GameDirectory; // base

		public SearchPath(string path, string directory)
		{
			this.Path = path;
			this.GameDirectory = directory;
		}
	}
}