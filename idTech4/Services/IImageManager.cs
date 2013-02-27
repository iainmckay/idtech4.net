using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using idTech4.Renderer;
using TextureFilter = idTech4.Renderer.TextureFilter;

namespace idTech4.Services
{
	interface IImageManager
	{
		#region Fetching
		idImage DefaultImage { get; }
		#endregion

		#region Initialization
		void Init();
		#endregion

		#region Loading
		idImage ImageFromFile(string name, TextureFilter filter, TextureRepeat repeat, TextureUsage usage, CubeFiles cubeMap = CubeFiles.TwoD);
		Texture2D LoadImage(string name, ref DateTime timeStamp);
		Texture2D LoadImageProgram(string name, ref DateTime timeStamp, ref TextureUsage usage);
		#endregion
	}
}