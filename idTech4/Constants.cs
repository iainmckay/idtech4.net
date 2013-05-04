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

namespace idTech4
{
	public class Constants
	{
		public const int ConsoleTextSize				= 0x30000;

		public const int MaxEntityMaterialParameters	= 12;
		public const int MaxExpressionRegisters			= 4096;
		public const int MaxGlobalMaterialParameters	= 12;
		public const int MaxRenderCrops					= 8;
		public const int MaxVertexParameters			= 4;
		public const int MaxWarningList					= 256;

		public const int MaxEffectUserParameters        = 8;

		// if we exceed these limits we stop rendering GUI surfaces
		public const int MaxGuiIndexes                  = 20000 * 6;
		public const int MaxGuiVertices                 = 20000 * 4;

		public const int SmallCharacterWidth			= 8;
		public const int SmallCharacterHeight			= 16;
		public const int BigCharacterWidth				= 16;
		public const int BigCharacterHeight				= 16;

		public const int DefaultImageSize				= 16;
		public const int FallOffTextureSize				= 64;

		// all drawing is done to a 640 x 480 virtual screen size
		// and will be automatically scaled to the real resolution
		public const int ScreenWidth                    = 640;
		public const int ScreenHeight                   = 480;

		public const float MinimumResolutionScale       = 0.5f;
		public const float MaximumResolutionScale       = 1.0f;

		/// Latched version of cvar, updated between map loads
		public const float EngineHzLatched              = 60.0f;
		public const long EngineHzNumerator             = 100L * 1000L;
		public const long EngineHzDenominator           = 100L * 60L;

		public const string DefaultFont                 = "Arial_Narrow";

		public static Guid FolderID_SavedGames_IdTech5	= new Guid(0x4c5c32ff, 0xbb9d, 0x43b0, 0xb5, 0xb4, 0x2d, 0x72, 0xe5, 0x4e, 0xaa, 0xa4);
	}
}