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

using Microsoft.Xna.Framework.Content;

namespace idTech4.UI.SWF
{
	public class idSWFEditText : idSWFDictionaryEntry
	{
		#region Properties
		public TextAlign Align
		{
			get
			{
				return _align;
			}
		}

		public idSWFRect Bounds
		{
			get
			{
				return _bounds;
			}
		}

		public idSWFColorRGBA Color
		{
			get
			{
				return _color;
			}
		}

		public EditTextFlags Flags
		{
			get
			{
				return _flags;
			}
		}

		public ushort FontHeight
		{
			get
			{
				return _fontHeight;
			}
		}

		public ushort FontID
		{
			get
			{
				return _fontID;
			}
		}

		public string InitialText
		{
			get
			{
				return _initialText;
			}
		}

		public short Leading
		{
			get
			{
				return _leading;
			}
		}

		public ushort LeftMargin
		{
			get
			{
				return _leftMargin;
			}
		}

		public ushort RightMargin
		{
			get
			{
				return _rightMargin;
			}
		}

		public string Variable
		{
			get
			{
				return _variable;
			}
		}
		#endregion

		#region Members
		private idSWFRect _bounds;
		private EditTextFlags _flags;
		private ushort _fontID;
		private ushort _fontHeight;

		private idSWFColorRGBA _color = idSWFColorRGBA.Default;
				
		private TextAlign _align;
		private ushort _leftMargin;
		private ushort _rightMargin;
		private ushort _indent;
		private short _leading;

		private string _variable;
		private string _initialText;
		private ushort _maxLength = 0xFFFF;
		#endregion

		#region idSWFDictionaryEntry implementation
		internal override void LoadFrom(ContentReader input)
		{
			_bounds.LoadFrom(input);

			_flags       = (EditTextFlags) input.ReadUInt32();
			_fontID      = input.ReadUInt16();
			_fontHeight  = input.ReadUInt16();

			_color.LoadFrom(input);

			_maxLength   = input.ReadUInt16();
			_align       = (TextAlign) input.ReadInt32();
			_leftMargin  = input.ReadUInt16();
			_rightMargin = input.ReadUInt16();
			_indent      = input.ReadUInt16();
			_leading     = input.ReadInt16();

			_variable    = input.ReadString();
			_initialText = input.ReadString();
		}
		#endregion
	}

	[Flags]
	public enum EditTextFlags
	{
		None,
		WordWrap  = 1 << 0,
		MultiLine = 1 << 1,
		Password  = 1 << 2,
		ReadOnly  = 1 << 3,
		AutoSize  = 1 << 4,
		Border    = 1 << 5
	}
}