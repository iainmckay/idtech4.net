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

using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using TImport = Microsoft.Xna.Framework.Content.Pipeline.Graphics.Texture2DContent;

namespace idTech4.Content.Pipeline
{
	[ContentImporter(".bimage", DisplayName = "BImage - idTech4", DefaultProcessor = "TextureProcessor")]
	public class BImageImporter : ContentImporter<TImport>
	{
		public override TImport Import(string filename, ContentImporterContext context)
		{
			//System.Diagnostics.Debugger.Launch();

			BImage image          = BImage.LoadFrom(filename);
			TImport outContent    = new TImport();			
			
			for(int i = 0; i < image.LevelCount; i++)
			{
				BitmapContent content = ProcessLevel(i, image);

				if(content != null)
				{
					outContent.Mipmaps.Add(content);
				}

				// we only bring in the first mipmap level because the chain is incomplete
				// and xna 4.0 requires a full chain so we just let xna generate them
				// for us.
				break;
			}

			if(image.LevelCount > 1)
			{
				//outContent.GenerateMipmaps(true);
			}
	
			return outContent;
		}

		private BitmapContent ProcessLevel(int level, BImage image)
		{
			BImageData imageData  = image.GetData(level);
			BitmapContent content = null;

			switch(image.Format)
			{
				case BImageFormat.Dxt5:
					switch(image.ColorFormat)
					{
						case BImageColorFormat.Default:
							content = new Dxt5BitmapContent(imageData.Width, imageData.Height);
							content.SetPixelData(imageData.Data);
							break;

						default:
							throw new NotSupportedException(string.Format("{0} color format is not supported", image.ColorFormat));
					}
					break;

				case BImageFormat.Dxt1:
					switch(image.ColorFormat)
					{
						case BImageColorFormat.Default:
							content = new Dxt1BitmapContent(imageData.Width, imageData.Height);
							content.SetPixelData(imageData.Data);
							break;

						default:
							throw new NotSupportedException(string.Format("{0} color format is not supported", image.ColorFormat));
					}
					break;

				case BImageFormat.RGB565:
					switch(image.ColorFormat)
					{
						case BImageColorFormat.Default:
							content = new PixelBitmapContent<Bgr565>(imageData.Width, imageData.Height);
							content.SetPixelData(imageData.Data);
							break;

						default:
							throw new NotSupportedException(string.Format("{0} color format is not supported", image.ColorFormat));
					}
					break;

				default:
					throw new NotSupportedException(string.Format("{0} format is not supported", image.Format));
			}

			return content;
		}
	}
}