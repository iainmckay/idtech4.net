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
using System;
using System.Text;

namespace idTech4.Text
{
	/// <summary>
	/// idToken is a token read from a file or memory with idLexer or idParser.
	/// </summary>
	public sealed class idToken
	{
		#region Properties
		#region Public
		/// <summary>
		/// Gets or sets the type of this token.
		/// </summary>
		public TokenType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		/// <summary>
		/// Gets or sets the sub type of this token.
		/// </summary>
		public TokenSubType SubType
		{
			get
			{
				return _subType;
			}
			set
			{
				_subType = value;
			}
		}

		/// <summary>
		/// Gets or sets the token options, used for recursive defines.
		/// </summary>
		public LexerOptions Options
		{
			get
			{
				return _options;
			}
			set
			{
				_options = value;
			}
		}

		/// <summary>
		/// Gets or sets the line in the script this token was on.
		/// </summary>
		public int Line
		{
			get
			{
				return _line;
			}
			set
			{
				_line = value;
			}
		}

		/// <summary>
		/// Gets or sets the number of lines crossed in white space before the token.
		/// </summary>
		public int LinesCrossed
		{
			get
			{
				return _linesCrossed;
			}
			set
			{
				_linesCrossed = value;
			}
		}

		/// <summary>
		/// Gets the length of the token value.
		/// </summary>
		public int Length
		{
			get
			{
				return _value.Length;
			}
		}

		public bool IsNumeric
		{
			get
			{
				// FIXME: don't think this'll hand all cases of being numeric.
				float tmp;

				return float.TryParse(_value.ToString(), out tmp);
			}
		}
		#endregion

		#region Internal
		/// <summary>
		/// Gets the length of whitespace before token.
		/// </summary>
		public int WhiteSpaceBeforeToken
		{
			get
			{
				return (_whiteSpaceEndPosition - _whiteSpaceStartPosition);
			}
		}

		/// <summary>
		/// Gets or sets the start of white space before token, only used by idLexer.
		/// </summary>
		internal int WhiteSpaceStartPosition
		{
			get
			{
				return _whiteSpaceStartPosition;
			}
			set
			{
				_whiteSpaceStartPosition = value;
			}
		}

		/// <summary>
		/// Gets or sets the end of white space before token, only used by idLexer.
		/// </summary>
		internal int WhiteSpaceEndPosition
		{
			get
			{
				return _whiteSpaceEndPosition;
			}
			set
			{
				_whiteSpaceEndPosition = value;
			}
		}

		/// <summary>
		/// Gets or sets the flags for this token, primarily used for recursive defines.
		/// </summary>
		public TokenFlags Flags
		{
			get
			{
				return _flags;
			}
			set
			{
				_flags = value;
			}
		}
		#endregion
		#endregion

		#region Members
		private TokenType _type;								// token type
		private TokenSubType _subType;							// token sub type
		private LexerOptions _options;							// token flags, used for recursive defines

		private int _line;										// line in script the token was on.
		private int _linesCrossed;								// number of lines crossed in white space before token

		private StringBuilder _value = new StringBuilder();
		private string _valueCache;
		private bool _valueCacheFilled;

		private ulong _intValue;								// integer value
		private double _floatValue;								// floating point value
		private int _whiteSpaceStartPosition;					// start of white space before token, only used by idLexer
		private int _whiteSpaceEndPosition;						// end of white space before token, only used by idLexer	

		private TokenFlags _flags;								// used for recursive defines
		#endregion

		#region Constructor
		public idToken()
		{

		}

		public idToken(idToken token)
		{
			throw new Exception("blaaaaaaaaaah");
		}
		#endregion

		#region Methods
		#region Public
		/// <summary>
		/// Forget whitespace before token.
		/// </summary>
		public void ClearWhiteSpace()
		{
			_whiteSpaceStartPosition = 0;
			_whiteSpaceEndPosition = 0;
			_linesCrossed = 0;
		}

		public void StripQuotes()
		{
			_value = _value.Replace("\"", "");
		}

		public int ToInt32()
		{
			return (int) ToInt64();
		}

		public long ToInt64()
		{
			long tmp;
			long.TryParse(ToString(), out tmp);

			return tmp;
		}

		public ulong ToUInt64()
		{
			ulong tmp;
			ulong.TryParse(ToString(), out tmp);

			return tmp;
		}

		public float ToFloat()
		{
			return (float) ToDouble();
		}

		public double ToDouble()
		{
			double tmp;
			double.TryParse(ToString(), out tmp);

			return tmp;
		}

		public override string ToString()
		{
			if(_valueCacheFilled == false)
			{
				_valueCache = _value.ToString();
				_valueCacheFilled = true;
			}

			return _valueCache;
		}
		#endregion

		#region Internal
		internal void SetInteger(ulong value)
		{
			_intValue = value;
			_floatValue = value;
		}

		internal void SetFloat(double value)
		{
			_floatValue = value;
			_intValue = (ulong) value;
		}

		internal void Append(string str)
		{
			_value.Append(str);
			_valueCacheFilled = false;
		}

		internal void Append(char chr)
		{
			_value.Append(chr);
			_valueCacheFilled = false;
		}

		internal void Set(string str)
		{
			_value.Clear();
			_value.Append(str);
			_valueCacheFilled = false;
		}
		#endregion
		#endregion
	}

	[Flags]
	public enum TokenFlags
	{
		RecursiveDefine = 1
	}

	public enum TokenType
	{
		Unknown		= 0,
		String		= 1,
		Literal		= 2,
		Number		= 3,
		Name		= 4,
		Punctuation	= 5
	}

	[Flags]
	public enum TokenSubType
	{
		Unknown = 0,

		/// <summary>Integer.</summary>
		Integer				= 0x00001,

		/// <summary>Decimal number.</summary>
		Decimal				= 0x00002,

		/// <summary>Hexadecimal number.</summary>
		Hex					= 0x00004,

		/// <summary>Octal number.</summary>
		Octal				= 0x00008,

		/// <summary>Binary number.</summary>
		Binary				= 0x00010,

		/// <summary>Long int.</summary>
		Long				= 0x00020,

		/// <summary>Unsigned int.</summary>
		Unsigned			= 0x00040,

		/// <summary>Floating point number.</summary>
		Float				= 0x00080,

		/// <summary>Float.</summary>
		SinglePrecision		= 0x00100,

		/// <summary>Double.</summary>
		DoublePrecision		= 0x00200,

		/// <summary>Long double.</summary>
		ExtendedPrecision	= 0x00400,

		/// <summary>Infinite 1.#INF</summary>
		Infinite			= 0x00800,

		/// <summary>Indefinite 1.#IND</summary>
		Indefinite			= 0x01000,

		/// <summary>NaN.</summary>
		NaN					= 0x02000,

		/// <summary>IP Address.</summary>
		IPAddress			= 0x04000,

		/// <summary>IP Port.</summary>
		IPPort				= 0x08000,

		/// <summary>Set if int value and float value are valid.</summary>
		ValuesValid			= 0x10000
	}
}