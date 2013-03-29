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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.UI.SWF
{
	public class idSWFImage : idSWFDictionaryEntry
	{
		#region Properties
		public Vector4 ChannelScale
		{
			get
			{
				return _channelScale;
			}
		}

		public Vector2 ImageSize
		{
			get
			{
				return _imageSize;
			}
		}

		public Vector2 ImageAtlasOffset
		{
			get
			{
				return _imageAtlasOffset;
			}
		}

		public idMaterial Material
		{
			get
			{
				return _material;
			}
		}
		#endregion

		#region Members
		private idMaterial _material;
		private Vector2 _imageSize;
		private Vector2 _imageAtlasOffset;
		private Vector4 _channelScale;
		#endregion

		#region Constructor
		public idSWFImage()
			: base()
		{
			_channelScale = new Vector4(1, 1, 1, 1);
		}
		#endregion

		#region idSWFDictionaryEntry implementation
		internal override void LoadFrom(ContentReader input)
		{
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

			string materialName = input.ReadString();

			if(materialName.StartsWith(".") == true)
			{
				_material = null;
			}
			else
			{
				_material = declManager.FindMaterial(materialName);
			}

			_imageSize        = input.ReadVector2();
			_imageAtlasOffset = input.ReadVector2();
			_channelScale     = input.ReadVector4();
		}
		#endregion
	}
}
