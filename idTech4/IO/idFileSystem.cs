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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
	/// <p/>
	/// The base path for file saving can be set to "fs_savepath" or "fs_basepath".
	/// </remarks>
	public class idFileSystem : IFileSystem
	{
		#region Constructor
		public idFileSystem()
		{
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
			if(FileExists("default.cfg") == fakse)
			{
				engine.FatalError("Couldn't load default.cfg");
			}
		}
		#endregion

		#region Methods
		#region Initialization
		private void Startup()
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
			ICommandSystem cmdSystem = idEngine.Instance.GetService<ICommandSystem>();

			idLog.WriteLine("------ Initializing File System ------");
			
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
		private void SetupGameDirectory(string gameName)
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
		#endregion
		#endregion

		#region IFileSystem implementation
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
		#endregion

		#region P/Invoke
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);
		#endregion
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
		fileInPack_t		*buildBuffer;*/
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