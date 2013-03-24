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
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace idTech4.Content.Pipeline.Intermediate.SWF
{
	public class SWFShape : SWFDictionaryEntry
	{
		public SWFRect StartBounds;
		public SWFRect EndBounds;

		public SWFShapeDrawFill[] FillDraws;
		public SWFShapeDrawLine[] LineDraws;

		#region Constructor
		public SWFShape()
		{

		}
		#endregion

		#region SWFDictionaryEntry implementation
		public override void Write(ContentWriter output)
		{
			if(this is SWFMorphShape)
			{
				output.Write((int) SWFDictionaryType.Morph);
			}
			else
			{
				output.Write((int) SWFDictionaryType.Shape);
			}

			this.StartBounds.Write(output);
			this.EndBounds.Write(output);

			output.Write(this.FillDraws.Length);

			for(int i = 0; i < this.FillDraws.Length; i++)
			{
				this.FillDraws[i].Write(output);
			}

			output.Write(this.LineDraws.Length);

			for(int i = 0; i < this.LineDraws.Length; i++)
			{
				this.LineDraws[i].Write(output);
			}
		}
		#endregion
	}

	public class SWFShapeDrawFill
	{
		public SWFFillStyle Style = new SWFFillStyle();

		public Vector2[] StartVertices;
		public Vector2[] EndVertices;
		public ushort[] Indices;

		public void Write(ContentWriter output)
		{
			this.Style.Write(output);

			output.Write(this.StartVertices.Length);

			for(int i = 0; i < this.StartVertices.Length; i++)
			{
				output.Write(this.StartVertices[i]);
			}

			output.Write(this.EndVertices.Length);

			for(int i = 0; i < this.EndVertices.Length; i++)
			{
				output.Write(this.EndVertices[i]);
			}

			output.Write(this.Indices.Length);

			for(int i = 0; i < this.Indices.Length; i++)
			{
				output.Write(this.Indices[i]);
			}
		}
	}

	public class SWFShapeDrawLine
	{
		public SWFLineStyle Style = new SWFLineStyle();

		public Vector2[] StartVertices;
		public Vector2[] EndVertices;
		public ushort[] Indices;

		public void Write(ContentWriter output)
		{
			this.Style.Write(output);

			output.Write(this.StartVertices.Length);

			for(int i = 0; i < this.StartVertices.Length; i++)
			{
				output.Write(this.StartVertices[i]);
			}

			output.Write(this.EndVertices.Length);

			for(int i = 0; i < this.EndVertices.Length; i++)
			{
				output.Write(this.EndVertices[i]);
			}

			output.Write(this.Indices.Length);

			for(int i = 0; i < this.Indices.Length; i++)
			{
				output.Write(this.Indices[i]);
			}
		}
	}

	public class SWFFillStyle
	{
		/// <summary>
		/// 0 = solid, 1 = gradient, 4 = bitmap.
		/// </summary>
		public byte Type;
	
		/// <summary>
		/// 0 = linear, 2 = radial, 3 = focal; 0 = repeat, 1 = clamp, 2 = near repeat, 3 = near clamp.
		/// </summary>
		public byte SubType;

		public SWFColorRGBA StartColor = SWFColorRGBA.Default;
		public SWFColorRGBA EndColor   = SWFColorRGBA.Default;

		public SWFMatrix StartMatrix;
		public SWFMatrix EndMatrix;

		public SWFGradient Gradient;
		public float FocalPoint;
		public ushort BitmapID;

		public void Write(ContentWriter output)
		{
			output.Write(this.Type);
			output.Write(this.SubType);

			this.StartColor.Write(output);
			this.EndColor.Write(output);

			this.StartMatrix.Write(output);
			this.EndMatrix.Write(output);
			this.Gradient.Write(output);

			output.Write(this.FocalPoint);
			output.Write(this.BitmapID);
		}
	}


	public class SWFLineStyle
	{
		public ushort StartWidth = 20;
		public ushort EndWidth   = 20;

		public SWFColorRGBA StartColor = SWFColorRGBA.Default;
		public SWFColorRGBA EndColor   = SWFColorRGBA.Default;

		public void Write(ContentWriter output)
		{
			output.Write(this.StartWidth);
			output.Write(this.EndWidth);

			this.StartColor.Write(output);
			this.EndColor.Write(output);
		}
	}

	public struct SWFGradientRecord
	{
		public byte StartRatio;
		public byte EndRatio;

		public SWFColorRGBA StartColor;
		public SWFColorRGBA EndColor;

		public void Write(ContentWriter output)
		{
			output.Write(this.StartRatio);
			output.Write(this.EndRatio);

			this.StartColor.Write(output);
			this.EndColor.Write(output);
		}
	}

	public struct SWFGradient
	{
		public SWFGradientRecord[] Records;

		public void Write(ContentWriter output)
		{
			output.Write(this.Records.Length);

			for(int i = 0; i < this.Records.Length; i++)
			{
				this.Records[i].Write(output);
			}
		}
	}

	public struct SWFMatrix
	{
		public float XX;
		public float YY;
		public float XY;
		public float YX;
		public float TX;
		public float TY;

		public SWFMatrix(float xx, float yy, float xy, float yx, float tx, float ty)
		{
			this.XX = xx;
			this.YY = yy;
			this.XY = xy;
			this.YX = yx;
			this.TX = tx;
			this.TY = ty;
		}

		public void Write(ContentWriter output)
		{
			output.Write(this.XX);
			output.Write(this.YY);
			output.Write(this.XY);
			output.Write(this.YX);
			output.Write(this.TX);
			output.Write(this.TY);
		}

		public static SWFMatrix Default = new SWFMatrix(1, 1, 0, 0, 0, 0);
	}

	public struct SWFColorRGB
	{
		public byte R;
		public byte G;
		public byte B;

		public SWFColorRGB(byte r, byte g, byte b)
		{
			this.R = r;
			this.G = g;
			this.B = b;
		}

		public void Write(ContentWriter output)
		{
			output.Write(this.R);
			output.Write(this.G);
			output.Write(this.B);
		}

		public static SWFColorRGB Default = new SWFColorRGB(255, 255, 255);
	}

	public struct SWFColorRGBA
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public SWFColorRGBA(byte r, byte g, byte b, byte a)
		{
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
		}

		public void Write(ContentWriter output)
		{
			output.Write(this.R);
			output.Write(this.G);
			output.Write(this.B);
			output.Write(this.A);
		}

		public static SWFColorRGBA Default = new SWFColorRGBA(255, 255, 255, 255);
	}
}