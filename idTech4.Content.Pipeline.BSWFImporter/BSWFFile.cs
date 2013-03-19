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
using System.IO;
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Content.Pipeline.Intermediate.SWF;

namespace idTech4.Content.Pipeline
{
	public class BSWFFile
	{
		#region Constants
		public const int Version = 16; // bumped to 16 for storing atlas image dimensions for unbuffered loads
		public const uint Magic  = (( 'B' << 24 ) | ('S' << 16) | ('W' << 8) | Version);
		#endregion

		#region Constructor
		public BSWFFile()
		{
			
		}
		#endregion

		#region Methods
		private SWFContent Load(Stream source)
		{
			BinaryReader r = new idBinaryReader(source);

			uint magic     = r.ReadUInt32();
			long timestamp = r.ReadInt64();

			if(magic != Magic)
			{
				return null;
			}

			SWFContent content  = new SWFContent();
			content.FrameWidth  = r.ReadSingle();
			content.FrameHeight = r.ReadSingle();
			content.FrameRate   = r.ReadUInt16();

			content.MainSprite  = LoadSprite(r);
			content.Dictionary  = new SWFDictionaryEntry[r.ReadInt32()];

			for(int i = 0; i < content.Dictionary.Length; i++)
			{
				SWFDictionaryType entryType = (SWFDictionaryType) r.ReadInt32();

				switch(entryType)
				{
					case SWFDictionaryType.Null:
						content.Dictionary[i] = new SWFNull();
						break;

					case SWFDictionaryType.Image:
						content.Dictionary[i] = LoadImage(r);
						break;
				
					case SWFDictionaryType.Morph:
					case SWFDictionaryType.Shape:
						content.Dictionary[i] = LoadShape(r);
						break;

					case SWFDictionaryType.Sprite:
						content.Dictionary[i] = LoadSprite(r);
						break;

					case SWFDictionaryType.Font:
						content.Dictionary[i] = LoadFont(r);
						break;

					case SWFDictionaryType.Text:
						content.Dictionary[i] = LoadText(r);
						break;
			
					case SWFDictionaryType.EditText:
						content.Dictionary[i] = LoadEditText(r);				
						break;
				}
			}

			return content;
		}

		private SWFImage LoadImage(BinaryReader r)
		{
			SWFImage entry           = new SWFImage();
			entry.MaterialName       = r.ReadString();

			entry.ImageSize.X        = r.ReadInt32();
			entry.ImageAtlasOffset.X = r.ReadInt32();
			entry.ImageSize.Y        = r.ReadInt32();
			entry.ImageAtlasOffset.Y = r.ReadInt32();
			entry.ChannelScale       = new Vector4(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());

			return entry;
		}

		private SWFFont LoadFont(BinaryReader r)
		{
			SWFFont font = new SWFFont();
			font.Name    = r.ReadString();
			font.Ascent  = r.ReadInt16();
			font.Descent = r.ReadInt16();
			font.Leading = r.ReadInt16();
			font.Glyphs  = new SWFFontGlyph[r.ReadInt32()];

			for(int g = 0; g < font.Glyphs.Length; g++)
			{
				SWFFontGlyph glyph = font.Glyphs[g] = new SWFFontGlyph();
				glyph.Code         = r.ReadUInt16();
				glyph.Advance      = r.ReadInt16();
				glyph.Vertices     = new Vector2[r.ReadInt32()];

				for(int v = 0; v < glyph.Vertices.Length; v++)
				{
					glyph.Vertices[v] = new Vector2(r.ReadSingle(), r.ReadSingle());
				}

				glyph.Indices = new ushort[r.ReadInt32()];

				for(int i = 0; i < glyph.Indices.Length; i++)
				{
					glyph.Indices[i] = r.ReadUInt16();
				}
			}

			return font;
		}

		private SWFShape LoadShape(BinaryReader r)
		{
			SWFShape entry                  = new SWFShape();
			entry.StartBounds.TopLeft.X     = r.ReadSingle();
			entry.StartBounds.TopLeft.Y     = r.ReadSingle();
			entry.StartBounds.BottomRight.X = r.ReadSingle();
			entry.StartBounds.BottomRight.Y = r.ReadSingle();

			entry.EndBounds.TopLeft.X       = r.ReadSingle();
			entry.EndBounds.TopLeft.Y       = r.ReadSingle();
			entry.EndBounds.BottomRight.X   = r.ReadSingle();
			entry.EndBounds.BottomRight.Y   = r.ReadSingle();

			entry.FillDraws = new SWFShapeDrawFill[r.ReadInt32()];

			for(int d = 0; d < entry.FillDraws.Length; d++)
			{
				SWFShapeDrawFill fillDraw = entry.FillDraws[d] = new SWFShapeDrawFill();
				SWFFillStyle style        = fillDraw.Style;

				style.Type           = r.ReadByte();
				style.SubType        = r.ReadByte();

				style.StartColor.R   = r.ReadByte();
				style.StartColor.G   = r.ReadByte();
				style.StartColor.B   = r.ReadByte();
				style.StartColor.A   = r.ReadByte();

				style.EndColor.R     = r.ReadByte();
				style.EndColor.G     = r.ReadByte();
				style.EndColor.B     = r.ReadByte();
				style.EndColor.A     = r.ReadByte();

				style.StartMatrix.XX = r.ReadSingle();
				style.StartMatrix.YY = r.ReadSingle();
				style.StartMatrix.XY = r.ReadSingle();
				style.StartMatrix.YX = r.ReadSingle();
				style.StartMatrix.TX = r.ReadSingle();
				style.StartMatrix.XY = r.ReadSingle();

				style.EndMatrix.XX   = r.ReadSingle();
				style.EndMatrix.YY   = r.ReadSingle();
				style.EndMatrix.XY   = r.ReadSingle();
				style.EndMatrix.YX   = r.ReadSingle();
				style.EndMatrix.TX   = r.ReadSingle();
				style.EndMatrix.XY   = r.ReadSingle();

				style.Gradient         = new SWFGradient();
				style.Gradient.Records = new SWFGradientRecord[r.ReadByte()];

				for(int g = 0; g < entry.FillDraws[d].Style.Gradient.Records.Length; g++)
				{
					style.Gradient.Records[g].StartRatio   = r.ReadByte();
					style.Gradient.Records[g].EndRatio     = r.ReadByte();

					style.Gradient.Records[g].StartColor.R = r.ReadByte();
					style.Gradient.Records[g].StartColor.G = r.ReadByte();
					style.Gradient.Records[g].StartColor.B = r.ReadByte();
					style.Gradient.Records[g].StartColor.A = r.ReadByte();

					style.Gradient.Records[g].EndColor.R   = r.ReadByte();
					style.Gradient.Records[g].EndColor.G   = r.ReadByte();
					style.Gradient.Records[g].EndColor.B   = r.ReadByte();
					style.Gradient.Records[g].EndColor.A   = r.ReadByte();
				}

				style.FocalPoint = r.ReadSingle();
				style.BitmapID   = r.ReadUInt16();

				fillDraw.StartVertices = new Vector2[r.ReadInt32()];

				for(int v = 0; v < fillDraw.StartVertices.Length; v++)
				{
					fillDraw.StartVertices[v] = new Vector2(r.ReadSingle(), r.ReadSingle());
				}

				fillDraw.EndVertices = new Vector2[r.ReadInt32()];

				for(int v = 0; v < fillDraw.EndVertices.Length; v++)
				{
					fillDraw.EndVertices[v] = new Vector2(r.ReadSingle(), r.ReadSingle());
				}

				fillDraw.Indices = new ushort[r.ReadInt32()];

				for(int i = 0; i < fillDraw.Indices.Length; i++)
				{
					fillDraw.Indices[i] = r.ReadUInt16();
				}
			}
			
			entry.LineDraws = new SWFShapeDrawLine[r.ReadInt32()];

			for(int d = 0; d < entry.LineDraws.Length; d++)
			{
				SWFShapeDrawLine lineDraw = entry.LineDraws[d] = new SWFShapeDrawLine();
				SWFLineStyle style        = lineDraw.Style;
				style.StartWidth          = r.ReadUInt16();
				style.EndWidth            = r.ReadUInt16();
				style.StartColor.R        = r.ReadByte();
				style.StartColor.G        = r.ReadByte();
				style.StartColor.B        = r.ReadByte();
				style.StartColor.A        = r.ReadByte();
				style.EndColor.R          = r.ReadByte();
				style.EndColor.G          = r.ReadByte();
				style.EndColor.B          = r.ReadByte();
				style.EndColor.A          = r.ReadByte();

				lineDraw.StartVertices = new Vector2[r.ReadInt32()];

				for(int v = 0; v < lineDraw.StartVertices.Length; v++)
				{
					lineDraw.StartVertices[v] = new Vector2(r.ReadSingle(), r.ReadSingle());
				}

				lineDraw.EndVertices = new Vector2[r.ReadInt32()];

				for(int v = 0; v < lineDraw.EndVertices.Length; v++)
				{
					lineDraw.EndVertices[v] = new Vector2(r.ReadSingle(), r.ReadSingle());
				}

				lineDraw.Indices = new ushort[r.ReadInt32()];

				for(int i = 0; i < lineDraw.Indices.Length; i++)
				{
					lineDraw.Indices[i] = r.ReadUInt16();
				}
			}

			return entry;
		}

		private SWFSprite LoadSprite(BinaryReader r)
		{
			SWFSprite sprite  = new SWFSprite();
			sprite.FrameCount = r.ReadUInt16();

			// frameOffsets contains offsets into the commands list for each frame
			// the first command for frame 3 is frameOffsets[2] and the last command is frameOffsets[3]
			sprite.FrameOffsets = new uint[r.ReadInt32()];

			for(int i = 0; i < sprite.FrameOffsets.Length; i++)
			{
				sprite.FrameOffsets[i] = r.ReadUInt32();
			}

			sprite.FrameLabels = new SWFFrameLabel[r.ReadInt32()];

			for(int i = 0; i < sprite.FrameLabels.Length; i++)
			{
				sprite.FrameLabels[i].Number = r.ReadUInt32();
				sprite.FrameLabels[i].Label  = r.ReadString();
			}

			byte[] tmp                       = r.ReadBytes((int) r.ReadUInt32());
			MemoryStream commandBuffer       = new MemoryStream(tmp, false);
			BinaryReader commandBufferReader = new BinaryReader(commandBuffer);

			sprite.Commands = new SWFSpriteCommand[r.ReadInt32()];

			for(int i = 0; i < sprite.Commands.Length; i++)
			{
				sprite.Commands[i]        = new SWFSpriteCommand();
				sprite.Commands[i].Tag    = (SWFTag) r.ReadInt32();
				sprite.Commands[i].Stream = new MemoryStream(commandBufferReader.ReadBytes((int) r.ReadUInt32()), false);
			}

			sprite.DoInitActions = new MemoryStream[r.ReadInt32()];

			for(int i = 0; i < sprite.DoInitActions.Length; i++)
			{
				sprite.DoInitActions[i] = new MemoryStream(commandBufferReader.ReadBytes((int) r.ReadUInt32()), false);
			}

			return sprite;
		}

		private SWFText LoadText(BinaryReader r)
		{
			SWFText text            = new SWFText();
			text.Bounds.TopLeft     = new Vector2(r.ReadSingle(), r.ReadSingle());
			text.Bounds.BottomRight = new Vector2(r.ReadSingle(), r.ReadSingle());

			text.Matrix.XX          = r.ReadSingle();
			text.Matrix.YY          = r.ReadSingle();
			text.Matrix.XY          = r.ReadSingle();
			text.Matrix.YX          = r.ReadSingle();
			text.Matrix.TX          = r.ReadSingle();
			text.Matrix.XY          = r.ReadSingle();

			text.Records = new SWFTextRecord[r.ReadInt32()];

			for(int t = 0; t < text.Records.Length; t++)
			{
				SWFTextRecord textRecord = text.Records[t] = new SWFTextRecord();
				textRecord.FontID        = r.ReadUInt16();

				textRecord.Color.R       = r.ReadByte();
				textRecord.Color.G       = r.ReadByte();
				textRecord.Color.B       = r.ReadByte();
				textRecord.Color.A       = r.ReadByte();

				textRecord.OffsetX       = r.ReadInt16();
				textRecord.OffsetY       = r.ReadInt16();
				textRecord.TextHeight    = r.ReadUInt16();
				textRecord.FirstGlyph    = r.ReadUInt16();
				textRecord.GlyphCount    = r.ReadUInt16();
			}

			text.Glyphs = new SWFGlyphEntry[r.ReadInt32()];

			for(int g = 0; g < text.Glyphs.Length; g++)
			{
				text.Glyphs[g].Index   = r.ReadUInt32();
				text.Glyphs[g].Advance = r.ReadInt32();
			}

			return text;
		}

		private SWFEditText LoadEditText(BinaryReader r)
		{
			SWFEditText editText        = new SWFEditText();
			editText.Bounds.TopLeft     = new Vector2(r.ReadSingle(), r.ReadSingle());
			editText.Bounds.BottomRight = new Vector2(r.ReadSingle(), r.ReadSingle());
			editText.Flags              = r.ReadUInt32();
			editText.FontID             = r.ReadUInt16();
			editText.FontHeight         = r.ReadUInt16();

			editText.Color.R            = r.ReadByte();
			editText.Color.G            = r.ReadByte();
			editText.Color.B            = r.ReadByte();
			editText.Color.A            = r.ReadByte();

			editText.MaxLength          = r.ReadUInt16();
			editText.Align              = r.ReadInt32();
			editText.LeftMargin         = r.ReadUInt16();
			editText.RightMargin        = r.ReadUInt16();
			editText.Indent             = r.ReadUInt16();
			editText.Leading            = r.ReadInt16();
			editText.Variable           = r.ReadString();
			editText.InitialText        = r.ReadString();

			return editText;
		}

		public static SWFContent LoadFrom(string filename)
		{
			// TODO: no error handling
			using(Stream stream = File.OpenRead(filename))
			{
				return new BSWFFile().Load(stream);
			}
		}
		#endregion
	}
}