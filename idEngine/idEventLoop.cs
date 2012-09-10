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
		#region Properties
		public int Milliseconds
		{
			get
			{
				return (idE.System.Milliseconds - _initialTimeOffset);
			}
		}
		#endregion

		#region Members
		// all events will have this subtracted from their time
		private int _initialTimeOffset;

		private Queue<SystemEvent> _events = new Queue<SystemEvent>();

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
		public void ClearEvents()
		{
			_events.Clear();
		}

		public void Init()
		{
			_initialTimeOffset = idE.System.Milliseconds;

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

		public void Queue(SystemEventType type, int value, int value2)
		{
			_events.Enqueue(new SystemEvent(type, value, value2));
		}

		public bool RunEventLoop()
		{
			return RunEventLoop(true);
		}

		public bool RunEventLoop(bool commandExecution)
		{
			SystemEvent ev;

			while(true)
			{
				if(commandExecution == true)
				{
					// execute any bound commands before processing another event
					idE.CmdSystem.ExecuteCommandBuffer();
				}

				ev = GetEvent();

				// if no more events are available
				if(ev.Type == SystemEventType.None)
				{
					return false;
				}

				ProcessEvent(ev);
			}

			return false;	// never reached
		}
		#endregion

		#region Private
		private SystemEvent GetEvent()
		{
			//idConsole.Warning("TODO: pushed events");
			// TODO: pushed events
			/*if(com_pushedEventsHead > com_pushedEventsTail)
			{
				com_pushedEventsTail++;
				return com_pushedEvents[(com_pushedEventsTail - 1) & (MAX_PUSHED_EVENTS - 1)];
			}*/

			return GetRealEvent();
		}

		private SystemEvent GetRealEvent()
		{
			SystemEvent ev;

			//idConsole.Warning("TODO: journalling");
			// either get an event from the system or the journal file
			// TODO: journalling
			/*if(com_journal.GetInteger() == 2)
			{
				r = com_journalFile->Read(&ev, sizeof(ev));
				if(r != sizeof(ev))
				{
					common->FatalError("Error reading from journal file");
				}
				if(ev.evPtrLength)
				{
					ev.evPtr = Mem_ClearedAlloc(ev.evPtrLength);
					r = com_journalFile->Read(ev.evPtr, ev.evPtrLength);
					if(r != ev.evPtrLength)
					{
						common->FatalError("Error reading from journal file");
					}
				}
			}
			else*/
			{
				// return if we have data
				if(_events.Count > 0)
				{
					return _events.Dequeue();
				}
				else 
				{
					// return the empty event 
					ev = new SystemEvent(SystemEventType.None);
				}

				// write the journal value out if needed
				// TODO: journalling
				/*if(com_journal.GetInteger() == 1)
				{
					r = com_journalFile->Write(&ev, sizeof(ev));
					if(r != sizeof(ev))
					{
						common->FatalError("Error writing to journal file");
					}
					if(ev.evPtrLength)
					{
						r = com_journalFile->Write(ev.evPtr, ev.evPtrLength);
						if(r != ev.evPtrLength)
						{
							common->FatalError("Error writing to journal file");
						}
					}
				}*/
			}

			return ev;
		}

		private void ProcessEvent(SystemEvent ev)
		{
			if(ev.Type == SystemEventType.Console)
			{
				idConsole.Warning("TODO: external console");

				// from a text console outside the game window
				// TODO: console
				//cmdSystem->BufferCommandText( CMD_EXEC_APPEND, (char *)ev.evPtr );
				//cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "\n" );
			} 
			else 
			{
				idE.Session.ProcessEvent(ev);
			}
		}
		#endregion
		#endregion
	}

	public sealed class SystemEvent : EventArgs
	{
		#region Properties
		public SystemEventType Type
		{
			get
			{
				return _type;
			}
		}

		public int Value
		{
			get
			{
				return _value;
			}
		}

		public int Value2
		{
			get
			{
				return _value2;
			}
		}
		#endregion

		#region Members
		private SystemEventType _type;
		private int _value;
		private int _value2;
		#endregion

		#region Constructor
		public SystemEvent(SystemEventType type)
			: base()
		{
			_type = type;
		}

		public SystemEvent(SystemEventType type, int value, int value2)
		{
			_type = type;
			_value = value;
			_value2 = value2;
		}
		#endregion
	}

	public enum SystemEventType
	{
		/// <summary>EventTime is still valid.</summary>
		None,
		/// <summary>Value is a key code, Value2 is the down flag.</summary>
		Key,
		/// <summary>Value is an ascii character.</summary>
		Char,
		/// <summary>Value and Value2 are relative signed x / y moves.</summary>
		Mouse,
		/// <summary>Value is an axis number and Value2 is the current state (-127 to 127).</summary>
		JoystickAxis,
		/// <summary>Ptr is a char*, from typing something at a non-game console.</summary>
		Console
	}
}