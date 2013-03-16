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
using Microsoft.Xna.Framework;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.UI
{
	public class idDeviceContext
	{
		#region Members
		private bool _initialized;

		private float _xScale;
		private float _yScale;
		private float _xOffset;
		private float _yOffset;

		private idMaterial _whiteImage;

		private bool _enableClipping;
		private bool _overStrikeMode;
		private Matrix _matrix;
		private bool _matrixIsIdentity;
		private Vector3 _origin;
		#endregion

		#region Constructor
		public idDeviceContext()
		{
			Clear();
		}
		#endregion

		#region Methods
		#region Initialization
		public void Init()
		{
			IDeclManager declManager   = idEngine.Instance.GetService<IDeclManager>();
			IRenderSystem renderSystem = idEngine.Instance.GetService<IRenderSystem>();

			_xScale = 1.0f;
			_yScale = 1.0f;
			_xOffset = 0.0f;
			_yOffset = 0.0f;

			_whiteImage = declManager.FindMaterial("guis/assets/white.tga");
			_whiteImage.Sort = (float) MaterialSort.Gui;

			/*activeFont = renderSystem->RegisterFont( "" );
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
			cursorImages[CURSOR_HAND_JOY1] = declManager->FindMaterial("ui/assets/guicursor_hand_cross.tga");
			cursorImages[CURSOR_HAND_JOY2] = declManager->FindMaterial("ui/assets/guicursor_hand_circle.tga");
			cursorImages[CURSOR_HAND_JOY3] = declManager->FindMaterial("ui/assets/guicursor_hand_square.tga");
			cursorImages[CURSOR_HAND_JOY4] = declManager->FindMaterial("ui/assets/guicursor_hand_triangle.tga");
			cursorImages[CURSOR_HAND_JOY1] = declManager->FindMaterial("ui/assets/guicursor_hand_a.tga");
			cursorImages[CURSOR_HAND_JOY2] = declManager->FindMaterial("ui/assets/guicursor_hand_b.tga");
			cursorImages[CURSOR_HAND_JOY3] = declManager->FindMaterial("ui/assets/guicursor_hand_x.tga");
			cursorImages[CURSOR_HAND_JOY4] = declManager->FindMaterial("ui/assets/guicursor_hand_y.tga");
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
			cursor = CURSOR_ARROW;*/

			_enableClipping   = true;
			_overStrikeMode   = true;
			_matrix           = Matrix.Identity;
			_matrixIsIdentity = true;
			_origin           = Vector3.Zero;

			_initialized = true;
		}
		#endregion

		#region State
		private void Clear()
		{
			_initialized = false;
		}
		#endregion
		#endregion
	}
}