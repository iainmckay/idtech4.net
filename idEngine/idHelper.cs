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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace idTech4
{
	public static class idHelper
	{
		public static char CharacterFromKeyCode(Keys key, Keys modifiers)
		{
			char c = '\0';

			if((key >= Keys.D0) && (key <= Keys.Z))
			{
				c = (char) key;

				if((modifiers & Keys.Shift) == 0)
				{
					c = Char.ToLower(c);
				}
				else
				{
					switch(key)
					{
						case Keys.D0:
							return ')';   
						case Keys.D1:
							return '!';
						case Keys.D2:
							return '"';
						case Keys.D3:
							return '£';
						case Keys.D4:
							return '$';
						case Keys.D5:
							return '%';
						case Keys.D6:
							return '^';
						case Keys.D7:
							return '&';
						case Keys.D8:
							return '*';
						case Keys.D9:
							return '(';
					}
				}
			}
			else if((modifiers & Keys.Shift) == 0)
			{
				switch(key)
				{
					case Keys.OemOpenBrackets:
						return '[';
					case Keys.OemCloseBrackets:
						return ']';
					case Keys.OemSemicolon:
						return ';';
					case Keys.OemQuotes:
						return '#';
					case Keys.Oemtilde:
						return '\'';
					case Keys.OemPeriod:
						return '.';
					case Keys.Oemcomma:
						return ',';
					case Keys.OemQuestion:
						return '/';
					case Keys.OemMinus:
						return '-';
					case Keys.Oemplus:
						return '=';
					case Keys.Oem5:
						return '\\';
					case Keys.Oem8:
						return '`';

					case Keys.NumPad0:
						return '0';
					case Keys.NumPad1:
						return '1';
					case Keys.NumPad2:
						return '2';
					case Keys.NumPad3:
						return '3';
					case Keys.NumPad4:
						return '4';
					case Keys.NumPad5:
						return '5';
					case Keys.NumPad6:
						return '6';
					case Keys.NumPad7:
						return '7';
					case Keys.NumPad8:
						return '8';
					case Keys.NumPad9:
						return '9';
				}
			}
			else
			{
				switch(key)
				{
					case Keys.OemOpenBrackets:
						return '{';
					case Keys.OemCloseBrackets:
						return '}';
					case Keys.OemSemicolon:
						return ':';
					case Keys.OemQuotes:
						return '~';
					case Keys.Oemtilde:
						return '@';
					case Keys.OemPeriod:
						return '>';
					case Keys.Oemcomma:
						return '<';
					case Keys.OemQuestion:
						return '?';
					case Keys.OemMinus:
						return '_';
					case Keys.Oemplus:
						return '+';
					case Keys.Oem5:
						return '|';
					case Keys.Oem8:
						return '¬';
				}
			}

			switch(key)
			{
				case Keys.Multiply:
					return '*';
				case Keys.Divide:
					return '/';
				case Keys.Add:
					return '+';
				case Keys.Subtract:
					return '-';
				case Keys.Decimal:
					return '.';
				case Keys.Space:
					return ' ';
			}

			return c;
		}
		
		public static int ColorIndex(idColor color)
		{
			return ((int) color & 15);
		}

		public static T[] Flatten<T>(T[,] source)
		{
			int d1 = source.GetUpperBound(0) + 1;
			int d2 = source.GetUpperBound(1) + 1;

			T[] flat = new T[d1 * d2];
			
			for(int y = 0; y < d1; y++)
			{
				for(int x = 0; x < d2; x++)
				{
					flat[(y * d2) + x] = source[y, x];
				}
			}

			return flat;
		}

		public static T[] Flatten<T>(T[,,] source)
		{
			int d1 = source.GetUpperBound(0);
			int d2 = source.GetUpperBound(1);
			int d3 = source.GetUpperBound(2);

			T[] flat = new T[d1 * d2 * d3];

			for(int y = 0; y < d1; y++)
			{
				for(int x = 0; x < d2; x++)
				{
					for(int z = 0; z < d3; z++)
					{
						flat[((y * d1) * d3) + z] = source[y, x, z];
					}
				}
			}

			return flat;
		}

		public static bool IsColor(string buffer, int index)
		{
			if((index + 1) >= buffer.Length)
			{
				return false;
			}

			return ((buffer[index] == (int) idColor.Escape) && (buffer[index + 1] != '\0') && (buffer[index + 1] != ' '));
		}

		public static int MakePowerOfTwo(int num)
		{
			int pot = 0;

			for(pot = 1; pot < num; pot <<= 1)
			{

			}

			return pot;
		}

		public static Rectangle ParseRectangle(string str)
		{
			try
			{
				string[] parts = str.Split(' ');

				if(parts.Length == 4)
				{
					return new Rectangle(
						int.Parse(parts[0]),
						int.Parse(parts[1]),
						int.Parse(parts[2]),
						int.Parse(parts[3]));
				}
			}
			catch
			{

			}

			return Rectangle.Empty;
		}

		public static Vector2 ParseVector2(string str)
		{
			try
			{
				string[] parts = str.Split(' ');

				if(parts.Length == 2)
				{
					return new Vector2(
						float.Parse(parts[0]),
						float.Parse(parts[1]));
				}
			}
			catch
			{

			}

			return Vector2.Zero;
		}

		public static Vector3 ParseVector3(string str)
		{
			try
			{
				string[] parts = str.Split(' ');

				if(parts.Length == 3)
				{
					return new Vector3(
						float.Parse(parts[0]),
						float.Parse(parts[1]),
						float.Parse(parts[2]));
				}
			}
			catch
			{

			}

			return Vector3.Zero;
		}

		public static Vector4 ParseVector4(string str)
		{
			try
			{
				string[] parts = str.Split(' ');

				if(parts.Length == 4)
				{
					return new Vector4(
						float.Parse(parts[0]),
						float.Parse(parts[1]),
						float.Parse(parts[2]),
						float.Parse(parts[3]));
				}
			}
			catch
			{

			}

			return Vector4.Zero;
		}

		public static string RemoveColors(string str)
		{
			StringBuilder newStr = new StringBuilder();

			for(int i = 0; i < str.Length; i++)
			{
				char c = str[i];

				if(IsColor(str, i) == true)
				{
					i++;
				}
				else
				{
					newStr.Append(c);
				}
			}

			return newStr.ToString();
		}

		public static string WrapText(string text, int columnWidth, int offset)
		{
			string str = string.Empty;
			int lineCount = text.Length / columnWidth;

			if((text.Length % columnWidth) != 0)
			{
				lineCount++;
			}

			for(int i = 0; i < lineCount; i++)
			{
				int width = columnWidth;

				if(((i * columnWidth) + columnWidth) > text.Length)
				{
					width = text.Length - (i * columnWidth);
				}

				str += text.Substring(i * columnWidth, width).PadLeft(offset);
			}

			return str;
		}
	}

	public enum idColor
	{
		Escape = '^',
		Default = '0',
		Red = '1',
		Green = '2',
		Yellow = '3',
		Blue = '4',
		Cyan = '5',
		Magenta = '6',
		White = '7',
		Gray = '8',
		Black = '9'
	}

	public static class idColorString
	{
		public const string Default = "^0";
		public const string Red = "^1";
		public const string Green = "^2";
		public const string Yellow = "^3";
		public const string Blue = "^4";
		public const string Cyan = "^5";
		public const string Magenta = "^6";
		public const string White = "^7";
		public const string Gray = "^8";
		public const string Black = "^9";
	}
}