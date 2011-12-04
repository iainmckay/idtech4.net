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
using Microsoft.Xna.Framework;

namespace idTech4
{
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

	public static class idHelper
	{
		public static bool IsColor(string buffer, int index)
		{
			if((index + 1) >= buffer.Length)
			{
				return false;
			}

			return ((buffer[index] == (int) idColor.Escape) && (buffer[index + 1] != '\0') && (buffer[index + 1] != ' '));
		}

		public static int ColorIndex(idColor color)
		{
			return ((int) color & 15);
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
}