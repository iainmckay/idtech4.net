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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace idTech4.Content.Pipeline.Intermediate.SWF
{
	public class SWFContent
	{
		public float FrameWidth;
		public float FrameHeight;
		public ushort FrameRate;

		public SWFSprite MainSprite;
		public SWFDictionaryEntry[] Dictionary;
	}

	public class SWFSprite : SWFDictionaryEntry
	{
		public ushort FrameCount;
		public uint[] FrameOffsets;
		public SWFFrameLabel[] FrameLabels;
		public SWFSpriteCommand[] Commands;
		public MemoryStream[] DoInitActions;

		public override void Write(ContentWriter output)
		{
			output.Write((int) SWFDictionaryType.Sprite);
			output.Write(this.FrameCount);
			output.Write(this.FrameOffsets.Length);

			for(int i = 0; i < this.FrameOffsets.Length; i++)
			{
				output.Write(this.FrameOffsets[i]);
			}

			output.Write(this.FrameLabels.Length);

			for(int i = 0; i < this.FrameLabels.Length; i++)
			{
				output.Write(this.FrameLabels[i].Number);
				output.Write(this.FrameLabels[i].Label);
			}

			output.Write(this.Commands.Length);

			for(int i = 0; i < this.Commands.Length; i++)
			{
				SWFSpriteCommand command = this.Commands[i];			

				byte[] data = new byte[command.Stream.Length];

				command.Stream.Seek(0, System.IO.SeekOrigin.Begin);
				command.Stream.Read(data, 0, data.Length);

				output.Write((int) command.Tag);
				output.Write((int) command.Stream.Length);
				output.Write(data);
			}

			output.Write(this.DoInitActions.Length);

			for(int i = 0; i < this.DoInitActions.Length; i++)
			{
				byte[] data = new byte[this.DoInitActions[i].Length];

				this.DoInitActions[i].Seek(0, System.IO.SeekOrigin.Begin);
				this.DoInitActions[i].Read(data, 0, data.Length);

				output.Write((int) this.DoInitActions[i].Length);
				output.Write(data);
			}
		}
	}

	public struct SWFFrameLabel
	{
		public string Label;
		public uint Number;
	}

	public struct SWFRect
	{
		public Vector2 TopLeft;
		public Vector2 BottomRight;

		public void Write(ContentWriter output)
		{
			output.Write(this.TopLeft);
			output.Write(this.BottomRight);
		}
	}

	public class SWFSpriteCommand
	{
		public SWFTag Tag;
		public MemoryStream Stream;
	}

	public enum SWFTag
	{
		End                          = 0,
		ShowFrame                    = 1,
		DefineShape                  = 2,
		PlaceObject                  = 4,
		RemoveObject                 = 5,
		DefineBits                   = 6,
		DefineButton                 = 7,
		JpegTables                   = 8,
		SetBackgroundColor           = 9,
		DefineFont                   = 10,
		DefineText                   = 11,
		DoAction                     = 12,
		DefineFontInfo               = 13,
		DefineSound                  = 14,
		StartSound                   = 15,
		DefineButtonSound            = 17,
		SoundStreamHead              = 18,
		SoundStreamBlock             = 19,
		DefineBitsLossless           = 20,
		DefineBitsJpeg2              = 21,
		DefineShape2                 = 22,
		DefineButtonCxForm           = 23,
		Protect                      = 24,
		PlaceObject2                 = 26,
		RemoveObject2                = 28,
		DefineShape3                 = 32,
		DefineText2                  = 33,
		DefineButton2                = 34,
		DefineBitsJpeg3              = 35,
		DefineBitsLossless2          = 36,
		DefineEditText               = 37,
		DefineSprite                 = 39,
		FrameLabel                   = 43,
		SoundStreamHead2             = 45,
		DefineMorphShape             = 46,
		DefineFont2                  = 48,
		ExportAssets                 = 57,
		EnableDebugger               = 58,
		DoInitAction                 = 59,
		DefineVideoStream            = 60,
		VideoFrame                   = 61,
		DefineFontInfo2              = 62,
		EnableDebugger2              = 64,
		ScriptLimits                 = 65,
		SetTabIndex                  = 66,
		FileAttributes               = 69,
		PlaceObject3                 = 70,
		ImportAssets2                = 71,
		DefineFontAlignZones         = 73,
		CsmTextSettings              = 74,
		DefineFont3                  = 75,
		SymbolClass                  = 76,
		Metadata                     = 77,
		DefineScalingGrid            = 78,
		DoAbc                        = 82,
		DefineShape4                 = 83,
		DefineMorphShape2            = 84,
		DefineSceneAndFrameLabelData = 86,
		DefineBinaryData             = 87,
		DefineFontName               = 88,
		StartSound2                  = 89
	}

	public enum SWFDictionaryType
	{
		Null,
		Image,
		Shape,
		Morph,
		Sprite,
		Font,
		Text,
		EditText
	}
}