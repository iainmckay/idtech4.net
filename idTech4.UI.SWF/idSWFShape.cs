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

namespace idTech4.UI.SWF
{
	public class idSWFShape : idSWFDictionaryEntry
	{
		#region Members
		private idSWFRect _startBounds;
		private idSWFRect _endBounds;

		private idSWFShapeDrawFill[] _fillDraws;
		private idSWFShapeDrawLine[] _lineDraws;
		#endregion

		#region Constructor
		public idSWFShape()
		{

		}
		#endregion

		#region idSWFDictionaryEntry implementation
		internal override void LoadFrom(ContentReader input)
		{
			_startBounds.LoadFrom(input);
			_endBounds.LoadFrom(input);

			_fillDraws = new idSWFShapeDrawFill[input.ReadInt32()];

			for(int i = 0; i < _fillDraws.Length; i++)
			{
				_fillDraws[i] = new idSWFShapeDrawFill();
				_fillDraws[i].LoadFrom(input);
			}

			_lineDraws = new idSWFShapeDrawLine[input.ReadInt32()];

			for(int i = 0; i < _lineDraws.Length; i++)
			{
				_lineDraws[i] = new idSWFShapeDrawLine();
				_lineDraws[i].LoadFrom(input);
			}
		}
		#endregion
	}

	public class idSWFShapeDrawFill
	{
		#region Members
		private idSWFFillStyle _style = new idSWFFillStyle();

		private Vector2[] _startVertices;
		private Vector2[] _endVertices;
		private ushort[] _indices;
		#endregion

		internal void LoadFrom(ContentReader input)
		{
			_style.LoadFrom(input);

			_startVertices = new Vector2[input.ReadInt32()];

			for(int i = 0; i < _startVertices.Length; i++)
			{
				_startVertices[i] = input.ReadVector2();
			}

			_endVertices = new Vector2[input.ReadInt32()];

			for(int i = 0; i < _endVertices.Length; i++)
			{
				_endVertices[i] = input.ReadVector2();
			}

			_indices = new ushort[input.ReadInt32()];

			for(int i = 0; i < _indices.Length; i++)
			{
				_indices[i] = input.ReadUInt16();
			}
		}
	}

	public class idSWFShapeDrawLine
	{
		#region Members
		private idSWFLineStyle _style = new idSWFLineStyle();

		private Vector2[] _startVertices;
		private Vector2[] _endVertices;
		private ushort[] _indices;
		#endregion

		internal void LoadFrom(ContentReader input)
		{
			_style.LoadFrom(input);

			_startVertices = new Vector2[input.ReadInt32()];

			for(int i = 0; i < _startVertices.Length; i++)
			{
				_startVertices[i] = input.ReadVector2();
			}

			_endVertices = new Vector2[input.ReadInt32()];

			for(int i = 0; i < _endVertices.Length; i++)
			{
				_endVertices[i] = input.ReadVector2();
			}

			_indices = new ushort[input.ReadInt32()];

			for(int i = 0; i < _indices.Length; i++)
			{
				_indices[i] = input.ReadUInt16();
			}
		}
	}

	public class idSWFFillStyle
	{
		#region Members
		/// <summary>
		/// 0 = solid, 1 = gradient, 4 = bitmap.
		/// </summary>
		private byte _type;
	
		/// <summary>
		/// 0 = linear, 2 = radial, 3 = focal; 0 = repeat, 1 = clamp, 2 = near repeat, 3 = near clamp.
		/// </summary>
		private byte _subType;

		private idSWFColorRGBA _startColor = idSWFColorRGBA.Default;
		private idSWFColorRGBA _endColor   = idSWFColorRGBA.Default;

		private idSWFMatrix _startMatrix;
		private idSWFMatrix _endMatrix;

		private idSWFGradient _gradient;
		private float _focalPoint;
		private ushort _bitmapID;
		#endregion

		internal void LoadFrom(ContentReader input)
		{
			_type       = input.ReadByte();
			_subType    = input.ReadByte();

			_startColor.LoadFrom(input);
			_endColor.LoadFrom(input);
			_startMatrix.LoadFrom(input);
			_endMatrix.LoadFrom(input);
			_gradient.LoadFrom(input);

			_focalPoint = input.ReadSingle();
			_bitmapID   = input.ReadUInt16();
		}
	}

	public class idSWFLineStyle
	{
		#region Members
		private ushort _startWidth = 20;
		private ushort _endWidth   = 20;

		private idSWFColorRGBA _startColor = idSWFColorRGBA.Default;
		private idSWFColorRGBA _endColor   = idSWFColorRGBA.Default;
		#endregion

		internal void LoadFrom(ContentReader input)
		{
			_startWidth = input.ReadUInt16();
			_endWidth   = input.ReadUInt16();

			_startColor.LoadFrom(input);
			_endColor.LoadFrom(input);
		}
	}

	public struct idSWFGradientRecord
	{
		public byte StartRatio;
		public byte EndRatio;

		public idSWFColorRGBA StartColor;
		public idSWFColorRGBA EndColor;

		internal void LoadFrom(ContentReader input)
		{
			this.StartRatio = input.ReadByte();
			this.EndRatio   = input.ReadByte();

			this.StartColor.LoadFrom(input);
			this.StartColor.LoadFrom(input);
		}
	}

	public struct idSWFGradient
	{
		public idSWFGradientRecord[] Records;

		internal void LoadFrom(ContentReader input)
		{
			this.Records = new idSWFGradientRecord[input.ReadInt32()];

			for(int i = 0; i < this.Records.Length; i++)
			{
				this.Records[i].LoadFrom(input);
			}
		}
	}

	public struct idSWFMatrix
	{
		public float XX;
		public float YY;
		public float XY;
		public float YX;
		public float TX;
		public float TY;

		public idSWFMatrix(float xx, float yy, float xy, float yx, float tx, float ty)
		{
			this.XX = xx;
			this.YY = yy;
			this.XY = xy;
			this.YX = yx;
			this.TX = tx;
			this.TY = ty;
		}

		internal void LoadFrom(ContentReader input)
		{
			this.XX = input.ReadSingle();
			this.YY = input.ReadSingle();
			this.XY = input.ReadSingle();
			this.YX = input.ReadSingle();
			this.TX = input.ReadSingle();
			this.TY = input.ReadSingle();
		}

		public idSWFMatrix Multiply(idSWFMatrix a)
		{
			idSWFMatrix result = new idSWFMatrix();
			result.XX = this.XX * a.XX + this.YX * a.XY;
			result.YX = this.XX * a.YX + this.YX * a.YY;
			result.XY = this.XY * a.XX + this.YY * a.XY;
			result.YY = this.XY * a.YX + this.YY * a.YY;
			result.TX = this.TX * a.XX + this.TY * a.XY + a.TX;
			result.TY = this.TX * a.YX + this.TY * a.YY + a.TY;

			return result;
		}

		public static idSWFMatrix Default = new idSWFMatrix(1, 1, 0, 0, 0, 0);
	}

	public struct idSWFColorXForm
	{
		public Vector4 Mul;
		public Vector4 Add;

		public idSWFColorXForm Multiply(idSWFColorXForm a)
		{
			idSWFColorXForm result = new idSWFColorXForm();
			result.Mul = this.Mul * a.Mul;
			result.Add = (this.Add * a.Mul) + a.Add;

			return result;
		}		
	}
}