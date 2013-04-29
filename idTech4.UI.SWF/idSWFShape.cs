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

using idTech4.Math;

namespace idTech4.UI.SWF
{
	public class idSWFShape : idSWFDictionaryEntry
	{
		#region Properties
		public idSWFShapeDrawFill[] Fills
		{
			get
			{
				return _fillDraws;
			}
		}

		public idSWFShapeDrawLine[] Lines
		{
			get
			{
				return _lineDraws;
			}
		}

		public idSWFRect StartBounds
		{
			get
			{
				return _startBounds;
			}
		}

		public idSWFRect EndBounds
		{
			get
			{
				return _endBounds;
			}
		}
		#endregion

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
		#region Properties
		public Vector2[] EndVertices
		{
			get
			{
				return _endVertices;
			}
		}

		public ushort[] Indices
		{
			get
			{
				return _indices;
			}
		}

		public Vector2[] StartVertices
		{
			get
			{
				return _startVertices;
			}
		}

		public idSWFFillStyle Style
		{
			get
			{
				return _style;
			}
		}
		#endregion

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
		#region Properties
		public Vector2[] EndVertices
		{
			get
			{
				return _endVertices;
			}
		}

		public ushort[] Indices
		{
			get
			{
				return _indices;
			}
		}

		public idSWFLineStyle Style
		{
			get
			{
				return _style;
			}
		}

		public Vector2[] StartVertices
		{
			get
			{
				return _startVertices;
			}
		}
		#endregion

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
		#region Properties
		public ushort BitmapID
		{
			get
			{
				return _bitmapID;
			}
		}

		public idSWFColorRGBA EndColor
		{
			get
			{
				return _endColor;
			}
		}

		public idSWFColorRGBA StartColor
		{
			get
			{
				return _startColor;
			}
		}

		public idSWFMatrix StartMatrix
		{
			get
			{
				return _startMatrix;
			}
		}

		public byte Type
		{
			get
			{
				return _type;
			}
		}
		#endregion

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
		#region Properties
		public idSWFColorRGBA EndColor
		{
			get
			{
				return _endColor;
			}
		}

		public ushort EndWidth
		{
			get
			{
				return _endWidth;
			}
		}

		public idSWFColorRGBA StartColor
		{
			get
			{
				return _endColor;
			}
		}

		public ushort StartWidth
		{
			get
			{
				return _startWidth;
			}
		}
		#endregion

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

		public idSWFMatrix Inverse()
		{
			idSWFMatrix inverse = idSWFMatrix.Default;
			
			float det = ((this.XX * this.YY) - (this.YX * this.XY));

			if(idMath.Abs(det) < idMath.FloatSmallestNonDenormal)
			{
				return (idSWFMatrix) this.MemberwiseClone();
			}

			float invDet = 1.0f / det;

			inverse.XX = invDet * this.YY;
			inverse.YX = invDet * -this.YX;
			inverse.XY = invDet * -this.XY;
			inverse.YY = invDet * this.XX;
			//inverse.tx = invDet * ( xy * ty ) - ( yy * tx );
			//inverse.ty = invDet * ( yx * tx ) - ( xx * ty );

			return inverse;
		}

		public idSWFMatrix Multiply(idSWFMatrix a)
		{
			idSWFMatrix result = idSWFMatrix.Default;
			result.XX = this.XX * a.XX + this.YX * a.XY;
			result.YX = this.XX * a.YX + this.YX * a.YY;
			result.XY = this.XY * a.XX + this.YY * a.XY;
			result.YY = this.XY * a.YX + this.YY * a.YY;
			result.TX = this.TX * a.XX + this.TY * a.XY + a.TX;
			result.TY = this.TX * a.YX + this.TY * a.YY + a.TY;

			return result;
		}

		public Vector2 Scale(Vector2 s)
		{
			return new Vector2((s.X * this.XX) + (s.Y * this.XY),
						(s.Y * this.YY) + (s.X * this.YX));
		}

		public Vector2 Transform(Vector2 t)
		{
			return new Vector2((t.X * this.XX) + (t.Y * this.XY) + this.TX,
						(t.Y * this.YY) + (t.X * this.YX) + this.TY);
		}

		public static idSWFMatrix Default = new idSWFMatrix(1, 1, 0, 0, 0, 0);
	}

	public struct idSWFColorXForm
	{
		public Vector4 Mul;
		public Vector4 Add;

		public idSWFColorXForm(Vector4 mul, Vector4 add)
		{
			this.Mul = mul;
			this.Add = add;
		}

		public idSWFColorXForm Multiply(idSWFColorXForm a)
		{
			idSWFColorXForm result = new idSWFColorXForm();
			result.Mul = this.Mul * a.Mul;
			result.Add = (this.Add * a.Mul) + a.Add;

			return result;
		}

		public static idSWFColorXForm Default = new idSWFColorXForm(new Vector4(1, 1, 1, 1), Vector4.Zero);
	}
}