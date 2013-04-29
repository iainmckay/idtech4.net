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
using System.Diagnostics;
using System.Text;

namespace idTech4.UI.SWF
{
	public class idSWFBitStream
	{
		#region Constants
		private int[] MaskForBitCount = new int[] {
			(int) ((1L << 0x00) - 1), (int) ((1L << 0x01) - 1), (int) ((1L << 0x02) - 1), (int) ((1L << 0x03) - 1),
			(int) ((1L << 0x04) - 1), (int) ((1L << 0x05) - 1), (int) ((1L << 0x06) - 1), (int) ((1L << 0x07) - 1),
			(int) ((1L << 0x08) - 1), (int) ((1L << 0x09) - 1), (int) ((1L << 0x0A) - 1), (int) ((1L << 0x0B) - 1),
			(int) ((1L << 0x0C) - 1), (int) ((1L << 0x0D) - 1), (int) ((1L << 0x0E) - 1), (int) ((1L << 0x0F) - 1),
			(int) ((1L << 0x10) - 1), (int) ((1L << 0x11) - 1), (int) ((1L << 0x12) - 1), (int) ((1L << 0x13) - 1),
			(int) ((1L << 0x14) - 1), (int) ((1L << 0x15) - 1), (int) ((1L << 0x16) - 1), (int) ((1L << 0x17) - 1),
			(int) ((1L << 0x18) - 1), (int) ((1L << 0x19) - 1), (int) ((1L << 0x1A) - 1), (int) ((1L << 0x1B) - 1),
			(int) ((1L << 0x1C) - 1), (int) ((1L << 0x1D) - 1), (int) ((1L << 0x1E) - 1), (int) ((1L << 0x1F) - 1),
			-1
		};

		private int[] SignForBitCount = new int[] {
			(-1) << (0x01 - 1), (-1) << (0x01 - 1), (-1) << (0x02 - 1), (-1) << (0x03 - 1),
			(-1) << (0x04 - 1), (-1) << (0x05 - 1), (-1) << (0x06 - 1), (-1) << (0x07 - 1),
			(-1) << (0x08 - 1), (-1) << (0x09 - 1), (-1) << (0x0A - 1), (-1) << (0x0B - 1),
			(-1) << (0x0C - 1), (-1) << (0x0D - 1), (-1) << (0x0E - 1), (-1) << (0x0F - 1),
			(-1) << (0x10 - 1), (-1) << (0x11 - 1), (-1) << (0x12 - 1), (-1) << (0x13 - 1),
			(-1) << (0x14 - 1), (-1) << (0x15 - 1), (-1) << (0x16 - 1), (-1) << (0x17 - 1),
			(-1) << (0x18 - 1), (-1) << (0x19 - 1), (-1) << (0x1A - 1), (-1) << (0x1B - 1),
			(-1) << (0x1C - 1), (-1) << (0x1D - 1), (-1) << (0x1E - 1), (-1) << (0x1F - 1),
			(-1) << (0x20 - 1)
		};
		#endregion

		#region Properties
		public byte[] Data
		{
			get
			{
				return _data;
			}
			set
			{
				_data = value;
			}
		}

		public bool HasData
		{
			get
			{
				return (_data != null);
			}
		}

		public int Length
		{
			get
			{
				return _data.Length;
			}
		}

		public int Position
		{
			get
			{
				return _position;
			}
		}
		#endregion

		#region Members
		private int _position;
		private byte[] _data;

		private ulong _currentBit;
		private ulong _currentByte;
		#endregion

		#region Constructor
		public idSWFBitStream()
		{

		}

		public idSWFBitStream(byte[] data)
		{
			_data = data;
		}
		#endregion

		#region Methods
		public byte ReadByte()
		{
			ResetBits();
			return _data[_position++];
		}

		public idSWFColorXForm ReadColorXFormRGBA()
		{
			ulong regCurrentBit  = 0;
			ulong regCurrentByte = 0;

			uint hasAddTerms = ReadInternalU(ref regCurrentBit, ref regCurrentByte, 1);
			uint hasMulTerms = ReadInternalU(ref regCurrentBit, ref regCurrentByte, 1);
			uint bitCount    = ReadInternalU(ref regCurrentBit, ref regCurrentByte, 4);

			int[] m = new int[4];
			int[] a = new int[4];

			if(hasMulTerms == 0)
			{
				m[0] =
					m[1] =
					m[2] =
					m[3] = 256;
			}
			else
			{
				m[0] = ReadInternalS(ref regCurrentBit, ref regCurrentByte, bitCount);
				m[1] = ReadInternalS(ref regCurrentBit, ref regCurrentByte, bitCount);
				m[2] = ReadInternalS(ref regCurrentBit, ref regCurrentByte, bitCount);
				m[3] = ReadInternalS(ref regCurrentBit, ref regCurrentByte, bitCount);
			}

			if(hasAddTerms == 0)
			{
				a[0] =
					a[1] =
					a[2] =
					a[3] = 256;
			}
			else
			{
				a[0] = ReadInternalS(ref regCurrentBit, ref regCurrentByte, bitCount);
				a[1] = ReadInternalS(ref regCurrentBit, ref regCurrentByte, bitCount);
				a[2] = ReadInternalS(ref regCurrentBit, ref regCurrentByte, bitCount);
				a[3] = ReadInternalS(ref regCurrentBit, ref regCurrentByte, bitCount);
			}

			_currentBit  = regCurrentBit;
			_currentByte = regCurrentByte;

			idSWFColorXForm colorXForm = new idSWFColorXForm();
			colorXForm.Mul.X = Fixed8(m[0]);
			colorXForm.Mul.Y = Fixed8(m[1]);
			colorXForm.Mul.Z = Fixed8(m[2]);
			colorXForm.Mul.W = Fixed8(m[3]);

			colorXForm.Add.X = Fixed8(a[0]);
			colorXForm.Add.Y = Fixed8(a[1]);
			colorXForm.Add.Z = Fixed8(a[2]);
			colorXForm.Add.W = Fixed8(a[3]);

			return colorXForm;
		}

		public byte[] ReadData(int length)
		{
			byte[] b = new byte[length];
			Array.Copy(_data, _position, b, 0, length);

			_position += length;

			return b;
		}

		public double ReadDouble()
		{
			byte[] swfIsRetarded = ReadData(8);
			byte[] buffer = {
				swfIsRetarded[4],
				swfIsRetarded[5],
				swfIsRetarded[6],
				swfIsRetarded[7],
				swfIsRetarded[0],
				swfIsRetarded[1],
				swfIsRetarded[2],
				swfIsRetarded[3]
			};

			return BitConverter.ToDouble(buffer, 0);
		}

		public idSWFMatrix ReadMatrix()
		{
			ulong regCurrentBit  = 0;
			ulong regCurrentByte = 0;
			uint hasScale        = ReadInternalU(ref regCurrentBit, ref regCurrentByte, 1);

			uint nBits;
			int xx;
			int yy;

			if(hasScale == 0)
			{
				xx = 65536;
				yy = 65536;
			}
			else
			{
				nBits = ReadInternalU(ref regCurrentBit, ref regCurrentByte, 5);
				xx    = ReadInternalS(ref regCurrentBit, ref regCurrentByte, nBits);
				yy    = ReadInternalS(ref regCurrentBit, ref regCurrentByte, nBits);
			}

			uint hasRotate = ReadInternalU(ref regCurrentBit, ref regCurrentByte, 1);

			int yx;
			int xy;

			if(hasRotate == 0)
			{
				yx = 0;
				xy = 0;
			} 
			else 
			{
				nBits = ReadInternalU(ref regCurrentBit, ref regCurrentByte, 5);
				yx    = ReadInternalS(ref regCurrentBit, ref regCurrentByte, nBits);
				xy    = ReadInternalS(ref regCurrentBit, ref regCurrentByte, nBits);
			}

			nBits  = ReadInternalU(ref regCurrentBit, ref regCurrentByte, 5);
			int tx = ReadInternalS(ref regCurrentBit, ref regCurrentByte, nBits);
			int ty = ReadInternalS(ref regCurrentBit, ref regCurrentByte, nBits);

			_currentBit  = regCurrentBit;
			_currentByte = regCurrentByte;
			
			idSWFMatrix matrix = new idSWFMatrix();
			matrix.XX = Fixed16(xx);
			matrix.YY = Fixed16(yy);
			matrix.YX = Fixed16(yx);
			matrix.XY = Fixed16(xy);
			matrix.TX = Twip(tx);
			matrix.TY = Twip(ty);

			return matrix;
		}

		public ushort ReadUInt16()
		{
			ResetBits();
			_position += 2;

			return (ushort) (_data[_position - 2] | (_data[_position - 1] << 8));
		}

		public int ReadInt32()
		{
			ResetBits();

			_position += 4;

			return (_data[_position - 4] | (_data[_position - 3] << 8) | (_data[_position - 2] << 16) | (_data[_position - 1] << 24));
		}

		public string ReadString()
		{
			int i = 0;

			for(i = 0; _data[i + _position] != 0; i++) { }

			string s   = Encoding.ASCII.GetString(_data, _position, i);
			_position += i + 1;

			return s;
		}
		
		public void Rewind()
		{
			_position = 0;
		}

		private uint ReadInternalU(ref ulong regCurrentBit, ref ulong regCurrentByte, uint bitCount)
		{
			Debug.Assert(bitCount <= 32);

			// read bits with only one microcoded shift instruction (shift with variable) on the consoles
			// this routine never reads more than 7 bits beyond the requested number of bits from the stream
			// such that calling ResetBits() never discards more than 7 bits and aligns with the next byte
			ulong extraByteCount = (bitCount - regCurrentBit + 7) >> 3;
			regCurrentBit = regCurrentBit + (extraByteCount << 3) - bitCount;

			for(int i = 0; i < (int) extraByteCount; i++)
			{
				regCurrentByte = (regCurrentByte << 8) | _data[_position + i];
			}

			_position += (int) extraByteCount;

			return (uint) (((int) regCurrentByte >> (int) regCurrentBit) & MaskForBitCount[bitCount]);
		}

		private int ReadInternalS(ref ulong regCurrentBit, ref ulong regCurrentByte, uint bitCount)
		{
			int i = (int) ReadInternalU(ref regCurrentBit, ref regCurrentByte, bitCount);

			// sign extend without microcoded shift instruction (shift with variable) on the consoles
			int s = SignForBitCount[bitCount];
	
			return ((i + s) ^ s);
		}

		private void ResetBits()
		{
			_currentBit  = 0;
			_currentByte = 0;
		}

		private float Twip(int twip)
		{
			return (twip * (1.0f / 20.0f));
		}

		private float Fixed16(int fix) 
		{
			return (fix * (1.0f / 65536.0f));
		}

		private float Fixed8(int fix)
		{
			return (fix * (1.0f / 256.0f));
		}
		#endregion
	}
}