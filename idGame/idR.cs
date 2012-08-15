using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using idTech4.Collision;
using idTech4.Game.Animation;
using idTech4.IO;
using idTech4.Net;
using idTech4.Text;
using idTech4.Renderer;
using idTech4.UI;

namespace idTech4.Game
{
	public class idR
	{
		public static readonly int UserCommandHertz = 60; // 60 frames per second
		public static readonly int UserCommandRate = 1000 / UserCommandHertz;

		public static readonly int MaxClients = idE.MaxNetworkClients;
		public static readonly int MaxGameEntities = idE.MaxGameEntities;
		public static readonly int MaxRenderEntityGui = idE.MaxRenderEntityGui;

		public static readonly int GameEntityBits = idE.GameEntityBits;

		public static readonly int EntityIndexNone = idE.MaxGameEntities - 1;
		public static readonly int EntityIndexWorld = idE.MaxGameEntities - 2;		
		public static readonly int EntityCountNormalMax = idE.MaxGameEntities - 2;

		public static readonly string EngineVersion = idE.EngineVersion;
		
		public static readonly idDeclManager DeclManager = idE.DeclManager;
		public static readonly idCvarSystem CvarSystem = idE.CvarSystem;
		public static readonly idAsyncNetwork NetworkSystem = idE.AsyncNetwork;
		public static readonly idCollisionModelManager CollisionModelManager = idE.CollisionModelManager;
		public static readonly idRenderModelManager RenderModelManager = idE.RenderModelManager;
		public static readonly idFileSystem FileSystem = idE.FileSystem;
		public static readonly idUserInterfaceManager UIManager = idE.UIManager;
		public static readonly idLangDict Language = idE.Language;
		public static readonly idAnimManager AnimManager = new idAnimManager();

		public static idGame Game;
		public static idGameEdit GameEdit;
	}
}