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
using System.IO;
using System.Linq;
using System.Text;

namespace idTech4
{
	public sealed class idEventLoop
	{
		#region Members
		// all events will have this subtracted from their time
		private int _initialTimeOffset;

		private Stream _journalFile;
		private Stream _journalDataFile;
		#endregion

		#region Constructor
		public idEventLoop()
		{
			new idCvar("com_journal", "0", 0, 2, "1 = record journal, 2 = play back journal", new ArgCompletion_Integer(0, 2), CvarFlags.Init | CvarFlags.System);
		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			_initialTimeOffset = idE.System.Time;

			idE.System.StartupVariable("journal", false);

			switch(idE.CvarSystem.GetInteger("com_journal"))
			{
				case 1:
					idConsole.WriteLine("Journaling events");

					_journalFile = idE.FileSystem.OpenFileWrite("journal.dat");
					_journalDataFile = idE.FileSystem.OpenFileWrite("journaldata.dat");
					break;

				case 2:
					idConsole.WriteLine("Replaying journaled events");

					_journalFile = idE.FileSystem.OpenFileRead("journal.dat");
					_journalDataFile = idE.FileSystem.OpenFileRead("journaldata.dat");
					break;
			}

			if((_journalFile == null) || (_journalDataFile == null))
			{
				idE.CvarSystem.SetInteger("com_journal", 0);

				_journalFile = null;
				_journalDataFile = null;

				idConsole.WriteLine("Couldn't open journal files");
			}
		}
		#endregion
		#endregion
	}
}