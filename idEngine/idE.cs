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
using System.Reflection;

using idTech4.IO;

namespace idTech4
{
	public enum ErrorType
	{
		None = 0,
		/// <summary>Exit the entire game with a popup window.</summary>
		Fatal,
		/// <summary>Print to console and disconnect from the game.</summary>
		Drop,
		/// <summary>Don't kill the server.</summary>
		Disconnect
	}

	public sealed class idE
	{
		public static readonly idPlatform Platform = new idPlatform();

		public const string GameName = "DOOM 3";
		public const string EngineVersion = "DOOM 1.3.1";

		public static readonly string Version = string.Format("{0}.{1}{2} {3} {4} {5}", 
			EngineVersion, 
			idVersion.BuildCount,
			(Platform.IsDebug == true) ? "-debug" : "",
			(Platform.Is64Bit == true) ? "x86" : "x64",
			idVersion.BuildDate, idVersion.BuildTime);

		public const int MaxPrintMessageSize = 4096;
		public const int MaxCommandArgs = 64;
		public const int MaxCommandStringLength = 2048;
		public const int MaxWarningList = 256;

		public const string BaseGameDirectory = "base";

		/// <summary>60 frames per second.</summary>
		public const int UserCommandHertz = 60;
		public const int UserCommandMillseconds = 1000 / UserCommandHertz;

		public static readonly idSystem System = new idSystem();
		public static readonly idFileSystem FileSystem = new idFileSystem();
		public static readonly idCvarSystem CvarSystem = new idCvarSystem();
		public static readonly idCmdSystem CmdSystem = new idCmdSystem();

		internal static Main Game;
		internal static SystemConsole SystemConsole = new SystemConsole();
	}
}