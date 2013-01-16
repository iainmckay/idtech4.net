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
using System.IO;

namespace idTech4.Services
{
	public interface IFileSystem
	{
		#region Checks
		/// <summary>
		/// Checks if the given physical file exists.
		/// </summary>
		/// <param name="path">Path on the underlying filesystem.</param>
		/// <returns>True if the file exists, false if not.</returns>
		bool FileExists(string path);

		/// <summary>
		/// Checks if the given file exists in a resource container.
		/// </summary>
		/// <param name="path">Path inside a resource container.</param>
		/// <returns>True if the file exists, false if not.</returns>
		bool ResourceFileExists(string path);
		#endregion

		#region Resource Tracking
		#region Properties
		bool BackgroundCacheEnabled { get; set; }

		bool UsingResourceFiles { get; }
		#endregion

		#region Methods
		void BeginLevelLoad(string name /*TODO: , char *_blockBuffer, int _blockBufferSize*/);
		void EndLevelLoad();
		#endregion
		#endregion

		#region Paths
		#region Properties
		string DefaultBasePath { get; }
		string DefaultSavePath { get; }
		#endregion

		#region Methods
		string GetAbsolutePath(string baseDirectory, string gameDirectory, string relativePath);
		string[] GetExtensionList(string extension);
		idFileList ListFiles(string relativePath, string extension, bool sort = false, bool fullRelativePath = false, string gameDirectory = null);
		#endregion
		#endregion

		#region Reading
		Stream OpenFileRead(string relativePath, bool allowCopyFiles = true, string gameDirectory = null);
		Stream OpenFileRead(string relativePath, out DateTime lastModified, bool allowCopyFiles = true, string gameDirectory = null);

		/// <summary>
		/// Finds the file in the search path, following search flag recommendations.
		/// </summary>
		Stream OpenFileRead(string relativePath, FileFlags searchFlags, bool allowCopyFiles = true, string gameDirectory = null);
		Stream OpenFileRead(string relativePath, FileFlags searchFlags, out DateTime lastModified, bool allowCopyFiles = true, string gameDirectory = null);

		Stream OpenFileReadMemory(string relativePath, bool allowCopyFiles = true, string gameDirectory = null);
		Stream OpenFileReadMemory(string relativePath, out DateTime lastModified, bool allowCopyFiles = true, string gameDirectory = null);

		byte[] ReadFile(string relativePath, bool allowCopyFiles = true, string gameDirectory = null);
		#endregion

		#region Writing
		Stream OpenFileWrite(string relativePath, string basePath = "fs_savepath");
		#endregion
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
	
	[Flags]
	public enum FileFlags
	{
		SearchDirectories = (1 << 0),
		ReturnMemoryFile = (1 << 1)
	}
}