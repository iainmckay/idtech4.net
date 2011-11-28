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

namespace idTech4.Game
{
	public class idR
	{
		public static readonly int UserCommandHertz = 60; // 60 frames per second
		public static readonly int UserCommandRate = 1000 / UserCommandHertz;

		public static readonly int MaxClients = idE.MaxClients;
		public static readonly int MaxGameEntities = idE.MaxGameEntities;
		public static readonly int MaxRenderEntityGui = idE.MaxRenderEntityGui;

		public static readonly int GameEntityBits = idE.GameEntityBits;

		public static readonly int EntityIndexNone = idE.MaxGameEntities - 1;
		public static readonly int EntityIndexWorld = idE.MaxGameEntities - 2;		
		public static readonly int EntityCountNormalMax = idE.MaxGameEntities - 2;

		public static readonly string EngineVersion = idE.EngineVersion;

		public static readonly int BuildNumber = idE.BuildNumber;
		public static readonly string BuildType = idE.BuildType;
		public static readonly string BuildArch = idE.BuildArch;
		public static readonly string BuildDate = idE.BuildDate;
		public static readonly string BuildTime = idE.BuildTime;

		public static readonly idDeclManager DeclManager = idE.DeclManager;
		public static readonly idCvarSystem CvarSystem = idE.CvarSystem;
		public static readonly idNetworkSystem NetworkSystem = idE.NetworkSystem;
		public static readonly idCollisionModelManager CollisionModelManager = idE.CollisionModelManager;
		public static readonly idRenderModelManager RenderModelManager = idE.RenderModelManager;
		public static readonly idFileSystem FileSystem = idE.FileSystem;
		public static readonly idUIManager UIManager = idE.UIManager;
		public static readonly idLangDict Language = idE.Language;

		public static idGameLocal Game;
		public static idGameEditLocal GameEdit;
	}
}
