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

using idTech4.Input;
using idTech4.Services;

namespace idTech4
{
	public class idConsole : IConsole
	{
		#region Members
		private bool _initialized;

		// allow these constants to be adjusted for HMD
		private int _localSafeLeft;
		private int _localSafeRight;
		private int _localSafeTop;
		private int _localSafeBottom;
		private int _localSafeWidth;
		private int _localSafeHeight;

		private int _lineWidth;
		private int _totalLines;

		private idInputField _consoleField;
		#endregion
		
		#region Constructor
		public idConsole()
		{

		}
		#endregion

		#region IConsole implementation
		#region Initialization
		#region Properties
		public bool IsInitialized
		{
			get
			{
				return _initialized;
			}
		}
		#endregion

		#region Methods
		public void Initialize()
		{
			if(this.IsInitialized == true)
			{
				throw new Exception("idConsole has already been initialized.");
			}

			_localSafeLeft = 32;
			_localSafeRight = 608;
			_localSafeTop = 24;
			_localSafeBottom = 456;
			_localSafeWidth = _localSafeRight - _localSafeLeft;
			_localSafeHeight = _localSafeBottom - _localSafeTop;

			_lineWidth = ((_localSafeWidth / Constants.SmallCharacterWidth) - 2);
			_totalLines = Constants.ConsoleTextSize / _lineWidth;

			_consoleField = new idInputField();
			_consoleField.WidthInCharacters = _lineWidth;

			_initialized = true;
		}

		public void Close()
		{
			idLog.Warning("TODO: console.close");
		}
		#endregion
		#endregion
		#endregion
	}
}