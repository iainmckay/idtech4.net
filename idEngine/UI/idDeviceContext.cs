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
	public sealed class idDeviceContext
	{
		#region Members
		private bool _initialized;
		#endregion

		#region Constructor
		public idDeviceContext()
		{
			Clear();
		}
		#endregion

		#region Methods
		#region Public
		public void Init()
		{
			/*xScale = 0.0;
			SetSize(VIRTUAL_WIDTH, VIRTUAL_HEIGHT);
			whiteImage = declManager->FindMaterial("guis/assets/white.tga");
			whiteImage->SetSort( SS_GUI );
			mbcs = false;
			SetupFonts();
			activeFont = &fonts[0];
			colorPurple = idVec4(1, 0, 1, 1);
			colorOrange = idVec4(1, 1, 0, 1);
			colorYellow = idVec4(0, 1, 1, 1);
			colorGreen = idVec4(0, 1, 0, 1);
			colorBlue = idVec4(0, 0, 1, 1);
			colorRed = idVec4(1, 0, 0, 1);
			colorWhite = idVec4(1, 1, 1, 1);
			colorBlack = idVec4(0, 0, 0, 1);
			colorNone = idVec4(0, 0, 0, 0);
			cursorImages[CURSOR_ARROW] = declManager->FindMaterial("ui/assets/guicursor_arrow.tga");
			cursorImages[CURSOR_HAND] = declManager->FindMaterial("ui/assets/guicursor_hand.tga");
			scrollBarImages[SCROLLBAR_HBACK] = declManager->FindMaterial("ui/assets/scrollbarh.tga");
			scrollBarImages[SCROLLBAR_VBACK] = declManager->FindMaterial("ui/assets/scrollbarv.tga");
			scrollBarImages[SCROLLBAR_THUMB] = declManager->FindMaterial("ui/assets/scrollbar_thumb.tga");
			scrollBarImages[SCROLLBAR_RIGHT] = declManager->FindMaterial("ui/assets/scrollbar_right.tga");
			scrollBarImages[SCROLLBAR_LEFT] = declManager->FindMaterial("ui/assets/scrollbar_left.tga");
			scrollBarImages[SCROLLBAR_UP] = declManager->FindMaterial("ui/assets/scrollbar_up.tga");
			scrollBarImages[SCROLLBAR_DOWN] = declManager->FindMaterial("ui/assets/scrollbar_down.tga");
			cursorImages[CURSOR_ARROW]->SetSort( SS_GUI );
			cursorImages[CURSOR_HAND]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_HBACK]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_VBACK]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_THUMB]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_RIGHT]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_LEFT]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_UP]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_DOWN]->SetSort( SS_GUI );
			cursor = CURSOR_ARROW;
			enableClipping = true;
			overStrikeMode = true;
			mat.Identity();
			origin.Zero();*/
			_initialized = true;
		}
		#endregion

		#region Private
		private void Clear()
		{
			_initialized = false;
			// TODO
			/*_useFont = NULL;
			_activeFont = NULL;
			_mbcs = false;*/
		}
		#endregion
		#endregion
	}
}