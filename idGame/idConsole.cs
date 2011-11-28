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

using idTech4.Threading;

namespace idTech4.Game
{
	public class idConsole
	{
		public static void Write(string format, params object[] args)
		{
			idE.Write(format, args);
		}

		public static void WriteLine(string format, params object[] args)
		{
			idE.WriteLine(format, args);
		}

		public static void DWrite(string format, params object[] args)
		{
			idE.DWrite(format, args);
		}

		public static void DWriteLine(string format, params object[] args)
		{
			idE.DWriteLine(format, args);
		}

		public static void Warning(string format, params object[] args)
		{
			idThread thread = idThread.CurrentThread;

			if(thread != null)
			{
				thread.Warning(format, args);
			}
			else
			{
				idE.Warning(format, args);
			}
		}

		public static void DWarning(string format, params object[] args)
		{
			idThread thread = idThread.CurrentThread;

			if(thread != null)
			{
				thread.DWarning(format, args);
			}
			else
			{
				idE.DWarning(format, args);
			}
		}

		public static void Error(string format, params object[] args)
		{
			idThread thread = idThread.CurrentThread;

			if(thread != null)
			{
				thread.Error(format, args);
			}
			else
			{
				idE.Error(format, args);
			}
		}
	}
}
