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

using Microsoft.Xna.Framework;

namespace idTech4
{
	public class idColor
	{
		public static readonly Vector4 Black      = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
		public static readonly Vector4 White      = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
		public static readonly Vector4 Red        = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
		public static readonly Vector4 Green      = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
		public static readonly Vector4 Blue       = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
		public static readonly Vector4 Yellow     = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
		public static readonly Vector4 Magenta    = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
		public static readonly Vector4 Cyan       = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);
		public static readonly Vector4 Orange     = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
		public static readonly Vector4 Purple     = new Vector4(0.6f, 0.0f, 0.6f, 1.0f);
		public static readonly Vector4 Pink       = new Vector4(0.73f, 0.4f, 0.48f, 1.0f);
		public static readonly Vector4 Brown      = new Vector4(0.4f, 0.35f, 0.08f, 1.0f);
		public static readonly Vector4 Grey       = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
		public static readonly Vector4 LightGrey  = new Vector4(0.75f, 0.75f, 0.75f, 1.0f);
		public static readonly Vector4 MediumGrey = new Vector4(0.0f, 0.5f, 0.5f, 1.0f);
		public static readonly Vector4 DarkGrey   = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);

		public static Vector4 FromIndex(int index)
		{
			idColorIndex tmp;

			if(Enum.TryParse<idColorIndex>(index.ToString(), out tmp) == true)
			{
				switch(tmp)
				{
					case idColorIndex.Red:
						return idColor.Red;

					case idColorIndex.Green:
						return idColor.Green;

					case idColorIndex.Yellow:
						return idColor.Yellow;

					case idColorIndex.Blue:
						return idColor.Blue;

					case idColorIndex.Cyan:
						return idColor.Cyan;

					case idColorIndex.Magenta:
						return idColor.Magenta;

					case idColorIndex.White:
						return idColor.White;

					case idColorIndex.Gray:
						return idColor.Grey;

					case idColorIndex.Black:
						return idColor.Black;
				}
			}

			return idColor.White;
		}

		public static bool IsColor(string buffer, int index)
		{
			if((index + 1) >= buffer.Length)
			{
				return false;
			}

			return ((buffer[index] == (int) idColorIndex.Escape) && (buffer[index + 1] != '\0') && (buffer[index + 1] != ' '));
		}

		public static string StripColors(string str)
		{
            StringBuilder newStr = new StringBuilder();
            int length = str.Length;

            for(int i = 0; i < length; i++)
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
	}

	public enum idColorIndex
	{
		Escape  = '^',
		Default = '0',
		Red     = '1',
		Green   = '2',
		Yellow  = '3',
		Blue    = '4',
		Cyan    = '5',
		Magenta = '6',
		White   = '7',
		Gray    = '8',
		Black   = '9'
	}

	public static class idColorString
	{
		public const string Default = "^0";
		public const string Red     = "^1";
		public const string Green   = "^2";
		public const string Yellow  = "^3";
		public const string Blue    = "^4";
		public const string Cyan    = "^5";
		public const string Magenta = "^6";
		public const string White   = "^7";
		public const string Gray    = "^8";
		public const string Black   = "^9";
	}
}