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

using idTech4.Game;
using idTech4.Input;
using idTech4.IO;
using idTech4.Net;
using idTech4.Renderer;
using idTech4.Sound;
using idTech4.Text;
using idTech4.UI;

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
		#region Public
		public static readonly idPlatform Platform = new idPlatform();

		public const string GameName = "DOOM 3";
		public const string EngineVersion = "DOOM 1.3.1";
		public const string BaseGameDirectory = "base";

		public static readonly string Version = string.Format("{0}.{1}{2} {3} {4} {5}", 
			EngineVersion, 
			idVersion.BuildCount,
			(Platform.IsDebug == true) ? "-debug" : "",
			(Platform.Is64Bit == true) ? "x86" : "x64",
			idVersion.BuildDate, idVersion.BuildTime);

		public const int GlyphStart = 0;
		public const int GlyphEnd = 255;
		public const int GlyphCharacterStart = 32;
		public const int GlyphCharacterEnd = 127;
		public const int GlyphsPerFont = GlyphEnd - GlyphStart + 1;

		public const int MaxPrintMessageSize = 4096;
		public const int MaxCommandArgs = 64;
		public const int MaxCommandStringLength = 2048;
		public const int MaxWarningList = 256;				
		public const int MaxUserCommandRelay = 10;
		public const int MaxEntityMaterialParameters = 12;
		public const int MaxExpressionRegisters = 4096;
		public const int MaxGlobalMaterialParameters = 12;
		public const int MaxRenderCrops = 8;

		// all drawing is done to a 640 x 480 virtual screen size
		// and will be automatically scaled to the real resolution
		public const int VirtualScreenWidth = 640;
		public const int VirtualScreenHeight = 480;

		public const int SmallCharacterWidth = 8;
		public const int SmallCharacterHeight = 16;
		public const int BigCharacterWidth = 16;
		public const int BigCharacterHeight = 16;

		public const string MasterServerAddress = "dnet.ua-corp.com";
		public const int MasterServerPort = 27650;

		/// <summary>60 frames per second.</summary>
		public const int UserCommandHertz = 60;
		public const int UserCommandMillseconds = 1000 / UserCommandHertz;

		public static idSystem System;
		public static idBaseGame Game;

		public static readonly idCvarSystem CvarSystem = new idCvarSystem();
		public static readonly idLangDict Language = new idLangDict();
		public static readonly idSession Session = new idSession();
		public static readonly idFileSystem FileSystem = new idFileSystem();
		
		public static readonly idCmdSystem CmdSystem = new idCmdSystem();
		public static readonly idDeclManager DeclManager = new idDeclManager();
		public static readonly idRenderSystem RenderSystem = new idRenderSystem();
		public static readonly idImageManager ImageManager = new idImageManager();
		public static readonly idUserInterfaceManager UIManager = new idUserInterfaceManager();
		public static readonly idEventLoop EventLoop = new idEventLoop();
		public static readonly idInputSystem Input = new idInputSystem();
		public static readonly idSoundSystem SoundSystem = new idSoundSystem();

		public static readonly idUserCommandGenerator UserCommandGenerator = new idUserCommandGenerator();
		#endregion

		#region Internal
		internal const string ConfigSpecification = "config.spec";
		internal const string ConfigFile = "DoomConfig.cfg";

		internal static idGameConsole Console = new idGameConsole();
		internal static SystemConsole SystemConsole = new SystemConsole();
		internal static idAsyncNetwork AsyncNetwork = new idAsyncNetwork();

		internal static BackEndState Backend = new BackEndState(); // TODO: refactor in to render library so we can support XNA more easily.
		internal static GLConfig GLConfig = new GLConfig();
		#endregion
	}
}