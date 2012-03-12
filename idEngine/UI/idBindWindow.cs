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

namespace idTech4.UI
{
	public sealed class idBindWindow : idWindow
	{
		#region Members
		private idWinString _bindName;
		private bool _waitingOnKey;
		#endregion

		#region Constructor
		public idBindWindow(idUserInterface gui)
			: base(gui)
		{
			Init();
		}

		public idBindWindow(idDeviceContext context, idUserInterface gui)
			: base(gui, context)
		{
			Init();
		}
		#endregion

		#region Methods
		#region Private
		private void Init()
		{
			_bindName = new idWinString("bind");
			_waitingOnKey = false;
		}
		#endregion
		#endregion

		#region idWindow implementation
		#region Public
		public override void Activate(bool activate, ref string act)
		{
			base.Activate(activate, ref act);
			
			_bindName.Update();
		}

		public override void Draw(int x, int y)
		{
			idConsole.Warning("TODO: BindWindow Draw");

			/*idVec4 color = foreColor;

			idStr str;
			if ( waitingOnKey ) {
				str = common->GetLanguageDict()->GetString( "#str_07000" );
			} else if ( bindName.Length() ) {
				str = bindName.c_str();
			} else {
				str = common->GetLanguageDict()->GetString( "#str_07001" );
			}

			if ( waitingOnKey || ( hover && !noEvents && Contains(gui->CursorX(), gui->CursorY()) ) ) {
				color = hoverColor;
			} else {
				hover = false;
			}

			dc->DrawText(str, textScale, textAlign, color, textRect, false, -1);*/
		}

		public override idWindowVariable GetVariableByName(string name, bool fixup, ref DrawWindow owner)
		{
			if(name.ToLower() == "bind")
			{
				return _bindName;
			}

			return base.GetVariableByName(name, fixup, ref owner);
		}

		public override string HandleEvent(SystemEvent e)
		{
			idConsole.Warning("TODO: BindWindow HandleEvent");
			/*static char ret[ 256 ];
	
			if (!(event->evType == SE_KEY && event->evValue2)) {
				return "";
			}

			int key = event->evValue;

			if (waitingOnKey) {
				waitingOnKey = false;
				if (key == K_ESCAPE) {
					idStr::snPrintf( ret, sizeof( ret ), "clearbind \"%s\"", bindName.GetName());
				} else {
					idStr::snPrintf( ret, sizeof( ret ), "bind %i \"%s\"", key, bindName.GetName());
				}
				return ret;
			} else {
				if (key == K_MOUSE1) {
					waitingOnKey = true;
					gui->SetBindHandler(this);
					return "";
				}
			}

			return "";*/

			return string.Empty;
		}
		#endregion

		#region Protected
		protected override void PostParse()
		{
			base.PostParse();

			_bindName.SetGuiInfo(this.UserInterface.State, _bindName.ToString());
			_bindName.Update();

			//bindName = state.GetString("bind");

			this.Flags |= WindowFlags.HoldCapture | WindowFlags.CanFocus;
		}
		#endregion
		#endregion
	}
}