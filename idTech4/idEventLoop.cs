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
using System.Collections.Generic;
using System.IO;

using idTech4.Services;

namespace idTech4
{
	/// <summary>
	/// The event loop receives events from the system and dispatches them to the various parts of the engine. 
	/// </sumary>
	/// <remarks>
	/// The event loop also handles journaling.
	/// The file system copies .cfg files to the journaled file.
	/// </remarks>
	public sealed class idEventLoop : IEventLoop
	{
		#region Members
		// all events will have this subtracted from their time
		private long _initialTimeOffset;

		private Queue<SystemEvent> _events = new Queue<SystemEvent>();

		private Stream _journalFile;
		private Stream _journalDataFile;
		#endregion

		#region Constructor
		public idEventLoop()
		{
			
		}
		#endregion

		#region Methods
		/// <summary>
		/// It is possible to get an event at the beginning of a frame that
		/// has a time stamp lower than the last event from the previous frame.
		/// </summary>
		/// <returns></returns>
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
			SystemEvent ev = new SystemEvent(SystemEventType.None);
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			//idConsole.Warning("TODO: journalling");
			// either get an event from the system or the journal file
			// TODO: journalling
			if(cvarSystem.GetInt("com_journal") == 2)
			{
				idLog.Warning("TODO: journalling");

				/*r = com_journalFile->Read(&ev, sizeof(ev));
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
				}*/
			}
			else
			{
				// return if we have data
				if(_events.Count > 0)
				{
					return _events.Dequeue();
				}

				// write the journal value out if needed				
				if(cvarSystem.GetInt("com_journal") == 1)
				{
					idLog.Warning("TODO: journal writing");

					/*r = com_journalFile->Write(&ev, sizeof(ev));
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
					}*/
				}
			}

			return ev;
		}

		private void ProcessEvent(SystemEvent ev)
		{
			ICommandSystem cmdSystem = idEngine.Instance.GetService<ICommandSystem>();
			IInputSystem inputSystem = idEngine.Instance.GetService<IInputSystem>();

			// track key up / down states
			if(ev.Type == SystemEventType.Key)
			{
				inputSystem.PreliminaryKeyEvent((Keys) ev.Value, (ev.Value2 != 0));
			}

			if(ev.Type == SystemEventType.Console)
			{
				idLog.Warning("TODO: external console");

				// from a text console outside the game window
				//cmdSystem->BufferCommandText( CMD_EXEC_APPEND, (char *)ev.evPtr );
				//cmdSystem->BufferCommandText( CMD_EXEC_APPEND, "\n" );
			}
			else
			{
				idEngine.Instance.ProcessEvent(ev);
			}
		}
		#endregion

		#region IEventLoop implementation
		#region Frame
		#region Properties
		/// <summary>
		/// Gets the current time in a way that will be journaled properly, as opposed to idEngine.ElapsedTime, which always reads a real timer.
		/// </summary>
		public long ElapsedTime
		{
			get
			{
				return (idEngine.Instance.ElapsedTime - _initialTimeOffset);
			}
		}
		#endregion

		#region Methods
		public void Queue(SystemEventType type, int value, int value2, int deviceNumber)
		{
			_events.Enqueue(new SystemEvent(type, value, value2, deviceNumber));
		}

		/// <summary>
		/// Dispatches all pending events and returns the current time.
		/// </summary>
		/// <param name="commandExecution"></param>
		/// <returns></returns>
		public bool RunEventLoop(bool commandExecution = true)
		{
			SystemEvent ev;
			ICommandSystem cmdSystem = idEngine.Instance.GetService<ICommandSystem>();

			while(true)
			{
				if(commandExecution == true)
				{
					// execute any bound commands before processing another event
					cmdSystem.ExecuteCommandBuffer();
				}

				ev = GetEvent();

				// if no more events are available
				if(ev.Type == SystemEventType.None)
				{
					return false;
				}

				ProcessEvent(ev);
			}
		}
		#endregion
		#endregion

		#region Initialization
		public void Initialize()
		{
			idEngine engine        = idEngine.Instance;
			ICVarSystem cvarSystem = engine.GetService<ICVarSystem>();
			IFileSystem fileSystem = engine.GetService<IFileSystem>();

			_initialTimeOffset = engine.ElapsedTime;

			engine.StartupVariable("journal");

			switch(cvarSystem.GetInt("com_journal"))
			{
				case 1:
					idLog.WriteLine("Journaling events");

					_journalFile     = fileSystem.OpenFileWrite("journal.dat");
					_journalDataFile = fileSystem.OpenFileWrite("journaldata.dat");
					break;

				case 2:
					idLog.WriteLine("Replaying journaled events");

					_journalFile     = fileSystem.OpenFileRead("journal.dat");
					_journalDataFile = fileSystem.OpenFileRead("journaldata.dat");
					break;
			}

			if((_journalFile == null) || (_journalDataFile == null))
			{
				cvarSystem.Set("com_journal", 0);

				_journalFile     = null;
				_journalDataFile = null;

				idLog.WriteLine("Couldn't open journal files");
			}	
		}
		#endregion
		#endregion
	}
}