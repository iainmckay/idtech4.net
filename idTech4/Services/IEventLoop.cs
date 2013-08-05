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

namespace idTech4.Services
{
	public interface IEventLoop
	{
		#region Frame
		#region Properties
		long ElapsedTime { get; }
		#endregion

		#region Methods
		void Queue(SystemEventType type, int value, int value2, int deviceNumber);
		bool RunEventLoop(bool commandExecution = true);
		#endregion
		#endregion

		#region Initialization
		void Initialize();
		#endregion
	}

	public sealed class SystemEvent : EventArgs
	{
		#region Properties
		public int DeviceNumber
		{
			get
			{
				return _deviceNumber;
			}
		}

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
		private int _deviceNumber;
		#endregion

		#region Constructor
		public SystemEvent(SystemEventType type)
			: base()
		{
			_type = type;
		}

		public SystemEvent(SystemEventType type, int value, int value2, int deviceNumber)
		{
			_type         = type;
			_value        = value;
			_value2       = value2;
			_deviceNumber = deviceNumber;
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
		/// <summary>Value and Value2 are meaninless, this indicates the mouse has left the client area.</summary>
		MouseLeave,
		/// <summary>Value is an axis number and Value2 is the current state (-127 to 127).</summary>
		Joystick,
		/// <summary>Ptr is a char*, from typing something at a non-game console.</summary>
		Console
	}
}
