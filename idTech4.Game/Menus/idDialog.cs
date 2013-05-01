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
using idTech4.Services;
using idTech4.UI.SWF;

namespace idTech4.Game.Menus
{
	public class idDialog : IDialog
	{
		#region Members
		private idSWF _dialog;
		private idSWF _saveIndicator;
		#endregion

		#region IDialog implementation
		#region Properties
		public bool IsActive
		{
			get
			{
				if(_dialog != null)
				{
					return _dialog.IsActive;
				}

				return false;
			}
		}
		#endregion

		#region Methods
		public void Init()
		{
			idLog.Warning("TODO: Dialog Shutdown();");
			idSWFManager swfManager = idEngine.Instance.GetService<idSWFManager>();

			_dialog        = swfManager.Load("dialog");
			_saveIndicator = swfManager.Load("save_indicator");

			if(_dialog != null)
			{
				_dialog.SetGlobal("DIALOG_ACCEPT",              (int) DialogType.Accept);
				_dialog.SetGlobal("DIALOG_CONTINUE",            (int) DialogType.Continue);
				_dialog.SetGlobal("DIALOG_ACCEPT_CANCEL",       (int) DialogType.AcceptCancel);
				_dialog.SetGlobal("DIALOG_YES_NO",              (int) DialogType.YesNo);
				_dialog.SetGlobal("DIALOG_CANCEL",              (int) DialogType.Cancel);
				_dialog.SetGlobal("DIALOG_WAIT",                (int) DialogType.Wait);
				_dialog.SetGlobal("DIALOG_WAIT_BLACKOUT",       (int) DialogType.WaitBlackout);
				_dialog.SetGlobal("DIALOG_WAIT_CANCEL",         (int) DialogType.WaitCancel);
				_dialog.SetGlobal("DIALOG_DYNAMIC",             (int) DialogType.Dynamic);
				_dialog.SetGlobal("DIALOG_QUICK_SAVE",          (int) DialogType.QuickSave);
				_dialog.SetGlobal("DIALOG_TIMER_ACCEPT_REVERT", (int) DialogType.TimerAcceptRevert);
				_dialog.SetGlobal("DIALOG_CRAWL_SAVE",          (int) DialogType.CrawlSave);
				_dialog.SetGlobal("DIALOG_CONTINUE_LARGE",      (int) DialogType.ContinueLarge);
				_dialog.SetGlobal("DIALOG_BENCHMARK",           (int) DialogType.Benchmark);
			}
		}
		#endregion
		#endregion
	}

	public enum DialogType
	{
		Invalid = -1,
		Accept,
		Continue,
		AcceptCancel,
		YesNo,
		Cancel,
		Wait,
		WaitBlackout,
		WaitCancel,
		Dynamic,
		QuickSave,
		TimerAcceptRevert,
		CrawlSave,
		ContinueLarge,
		Benchmark
	}
}