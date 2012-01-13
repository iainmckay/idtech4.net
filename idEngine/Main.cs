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
using System.Threading;
using System.Windows.Forms;

namespace idTech4
{
	public sealed class Main
	{
		#region Constructor
		public Main()
		{
			
		}
		#endregion

		#region Methods
		#region Initialization
		private void Initialize(string[] args)
		{
			// TODO
			/*const HCURSOR hcurSave = ::SetCursor( LoadCursor( 0, IDC_WAIT ) );
			Sys_SetPhysicalWorkMemory( 192 << 20, 1024 << 20 );
			Sys_GetCurrentMemoryStatus( exeLaunchMemoryStats );*/

			// done before Com/Sys_Init since we need this for error output
			CreateConsole();

			idE.System.Init(args);

			/*
			Sys_StartAsyncThread();*/

			// hide or show the early console as necessary
			if((idE.CvarSystem.GetInteger("win_viewlog") > 0) || (idE.CvarSystem.GetBool("com_skipRenderer") == true) /* TODO: || idAsyncNetwork::serverDedicated.GetInteger()*/) 
			{
				idE.SystemConsole.Show(1, true);
			}
			else
			{
				idE.SystemConsole.Show(0, false);
			}

			/*#ifdef SET_THREAD_AFFINITY 
				// give the main thread an affinity for the first cpu
				SetThreadAffinityMask( GetCurrentThread(), 1 );
			#endif
			
			// Launch the script debugger
			if ( strstr( lpCmdLine, "+debugger" ) ) {
				// DebuggerClientInit( lpCmdLine );
				return 0;
			}*/
		}

		private void CreateConsole()
		{
			// don't show it now that we have a splash screen up
			if(idE.CvarSystem.GetBool("win32_viewlog") == true)
			{
				idE.SystemConsole.Show();
				idE.SystemConsole.FocusInput();
			}

			idConsole.ClearInputHistory();
		}

		private void InitializeSystem()
		{

		}
		#endregion

		public void Run(string[] args)
		{
			Initialize(args);

			while(idE.Quit == false)
			{
				// if "viewlog" has been modified, show or hide the log console
				if(idE.CvarSystem.IsModified("win_viewlog") == true)
				{
					if((idE.CvarSystem.GetBool("com_skipRenderer") == false) /* TODO: && idAsyncNetwork::serverDedicated.GetInteger() != 1)*/)
					{
						idE.SystemConsole.Show(idE.CvarSystem.GetInteger("win_viewlog"), false);
					}

					idE.CvarSystem.ClearModified("win_viewlog");
				}

				// TODO
				/*#ifdef DEBUG
				Sys_MemFrame();
				#endif
				*/

				idE.System.Frame();
				Application.DoEvents();
				Thread.Sleep(0);
			}
		}
		#endregion
	}
}