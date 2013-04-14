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
using System.IO;

using Microsoft.Xna.Framework.Content;

namespace idTech4.UI.SWF
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// What the swf file format calls a "sprite" is known as a "movie clip" in Flash.
	/// There is one main sprite, and many many sub-sprites.
	/// Only the main sprite is allowed to add things to the dictionary.
	/// </remarks>
	public class idSWFSprite : idSWFDictionaryEntry
	{
		#region Properties
		public byte[][] DoInitActions
		{
			get
			{
				return _doInitActions;
			}
		}

		public ushort FrameCount
		{
			get
			{
				return _frameCount;
			}
		}

		public idSWFFrameLabel[] FrameLabels
		{
			get
			{
				return _frameLabels;
			}
		}

		public int FrameOffsetCount
		{
			get
			{
				return _frameOffsets.Length;
			}
		}

		public idSWF Owner
		{
			get
			{
				return _owner;
			}
		}
		#endregion

		#region Members
		private idSWF _owner;

		private ushort _frameCount;

		// frameOffsets contains offsets into the commands list for each frame
		// the first command for frame 3 is frameOffsets[2] and the last command is frameOffsets[3]
		private uint[] _frameOffsets;

		private idSWFFrameLabel[] _frameLabels;
		private idSWFSpriteCommand[] _commands;

		//// [ES-BrianBugh 1/16/10] - There can be multiple DoInitAction tags, and all need to be executed.
		private byte[][] _doInitActions;
		#endregion

		#region Constructor
		public idSWFSprite(idSWF owner)
		{
			_owner = owner;
		}
		#endregion

		#region Misc
		public idSWFSpriteCommand GetCommand(uint index)
		{
			return _commands[index];
		}

		public uint GetFrameOffset(int index)
		{
			return _frameOffsets[index];
		}
		#endregion

		#region idSWFDictionaryEntry implementation
		internal override void LoadFrom(ContentReader input)
		{
			_frameCount   = input.ReadUInt16();
			_frameOffsets = new uint[input.ReadInt32()];

			for(int i = 0; i < _frameOffsets.Length; i++)
			{
				_frameOffsets[i] = input.ReadUInt32();
			}

			_frameLabels = new idSWFFrameLabel[input.ReadInt32()];
			
			for(int i = 0; i < _frameLabels.Length; i++)
			{
				_frameLabels[i].FrameNumber = (int) input.ReadUInt32();
				_frameLabels[i].Label       = input.ReadString();
			}

			_commands = new idSWFSpriteCommand[input.ReadInt32()];

			for(int i = 0; i < _commands.Length; i++)
			{
				_commands[i]        = new idSWFSpriteCommand();
				_commands[i].Tag    = (idSWFTag) input.ReadInt32();

				int length          = input.ReadInt32();
				byte[] data         = input.ReadBytes(length);

				_commands[i].Stream = new idSWFBitStream(data);
			}

			_doInitActions = new byte[input.ReadInt32()][];

			for(int i = 0; i < _doInitActions.Length; i++)
			{
				int length        = input.ReadInt32();
				byte[] data       = input.ReadBytes(length);

				_doInitActions[i] = data;
			}
		}
		#endregion
	}

	#region Types
	public struct idSWFFrameLabel
	{
		public string Label;
		public int FrameNumber;
	}
	#endregion

	public class idSWFSpriteCommand
	{
		public idSWFTag Tag;
		public idSWFBitStream Stream;
	}
}