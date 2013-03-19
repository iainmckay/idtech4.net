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
using System.IO;
using System.Text;

namespace idTech4.Content.Pipeline
{
	public class idBinaryReader : BinaryReader
	{
		#region Constructors
		public idBinaryReader(Stream input) 
			: base(input)
		{

		}

		public idBinaryReader(Stream input, Encoding encoding)
			: base(input, encoding)
		{

		}
		#endregion

		public override double ReadDouble()
		{
			byte[] b = this.ReadBytes(sizeof(double));
			Swap(ref b);

			return BitConverter.ToDouble(b, 0);
		}

		public override float ReadSingle()
		{
			byte[] b = this.ReadBytes(sizeof(float));
			Swap(ref b);

			return BitConverter.ToSingle(b, 0);
		}

		public override short ReadInt16()
		{
			byte[] b = this.ReadBytes(sizeof(short));
			Swap(ref b);

			return BitConverter.ToInt16(b, 0);
		}

		public override int ReadInt32()
		{
			byte[] b = this.ReadBytes(sizeof(int));
			Swap(ref b);

			return BitConverter.ToInt32(b, 0);
		}

		public override long ReadInt64()
		{
			byte[] b = this.ReadBytes(sizeof(long));
			Swap(ref b);

			return BitConverter.ToInt64(b, 0);
		}

		public override ushort ReadUInt16()
		{
			byte[] b = this.ReadBytes(sizeof(ushort));
			Swap(ref b);

			return BitConverter.ToUInt16(b, 0);
		}

		public override uint ReadUInt32()
		{
			byte[] b = this.ReadBytes(sizeof(uint));
			Swap(ref b);

			return BitConverter.ToUInt32(b, 0);
		}

		public override ulong ReadUInt64()
		{
			byte[] b = this.ReadBytes(sizeof(ulong));
			Swap(ref b);

			return BitConverter.ToUInt64(b, 0);
		}

		public override byte[] ReadBytes(int count)
		{
			return base.ReadBytes(count);
		}

		public override string ReadString()
		{
			int length = base.ReadInt32();

			if(length >= 0)
			{
				return Encoding.UTF8.GetString(ReadBytes(length));
			}

			return string.Empty;
		}

		private void Swap(ref byte[] b)
		{
			if(b.Length == 1)
			{
			}
			else if(b.Length == 2)
			{
				byte t = b[0];
				b[0]   = b[1];
				b[1]   = t;
			}
			else if(b.Length == 4)
			{
				byte t = b[0];
				b[0]   = b[3];
				b[3]   = t;

				t    = b[1];
				b[1] = b[2];
				b[2] = t;
			}
			else if(b.Length == 8)
			{
				byte t = b[0];
				b[0]   = b[7];
				b[7]   = t;

				t    = b[1];
				b[1] = b[6];
				b[6] = t;

				t    = b[2];
				b[2] = b[5];
				b[5] = t;

				t    = b[3];
				b[3] = b[4];
				b[4] = t;
			}
			else
			{
				Debug.Assert(false);
			}
		}
	}
}