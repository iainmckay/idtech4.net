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

using Microsoft.Xna.Framework;

using idTech4.Renderer;

namespace idTech4.UI
{
	public sealed class idSliderWindow : idWindow
	{
		#region Constructor
		private float _low;
		private float _high;
		
		private float _thumbWidth;
		private float _thumbHeight;
		private Rectangle _thumbRect;
		private idMaterial _thumbMaterial;
		private string _thumbMaterialName;

		private float _stepSize;
		private float _lastValue;

		private bool _vertical;
		private bool _verticalFlip;
		private bool _scrollBar;
		
		private idWindow _buddyWindow;
		private idCvar _cvar;
		private bool _cvarInit;

		private idWinFloat _value = new idWinFloat("value");
		private idWinString _cvarStr = new idWinString("cvar");
		private idWinBool _liveUpdate = new idWinBool("liveUpdate");
		private idWinString _cvarGroup = new idWinString("cvarGroup");
		#endregion

		#region Constructor
		public idSliderWindow(idUserInterface gui)
			: base(gui)
		{
			Init();
		}

		public idSliderWindow(idDeviceContext context, idUserInterface gui)
			: base(gui, context)
		{
			Init();
		}
		#endregion

		#region Methods
		#region Private
		private void Init()
		{
			_value.Set(0.0f);
			_low = 0.0f;
			_high = 100.0f;
			_stepSize = 1.0f;
			_thumbMaterial = idE.DeclManager.FindMaterial("_default");
			_buddyWindow = null;

			_cvar = null;
			_cvarInit = false;
			_vertical = false;
			_scrollBar = false;
			_verticalFlip = false;

			_liveUpdate.Set(true);
		}
		#endregion
		#endregion

		#region idWindow implementation
		#region Properties
		public override idWindow Buddy
		{
			get
			{
				return _buddyWindow;
			}
			set
			{
				_buddyWindow = value;
			}
		}
		#endregion

		#region Public
		public override void Activate(bool activate, ref string act)
		{
			base.Activate(activate, ref act);
			idConsole.WriteLine("TODO: SliderWindow Activate");
			/*if(activate)
			{
				UpdateCvar(true, true);
			}*/
		}

		public override void Draw(int x, int y)
		{
			idConsole.WriteLine("TODO: SliderWindow Draw");
			/*idVec4 color = foreColor;

			if(!cvar && !buddyWin)
			{
				return;
			}

			if(!thumbWidth || !thumbHeight)
			{
				thumbWidth = thumbMat->GetImageWidth();
				thumbHeight = thumbMat->GetImageHeight();
			}

			UpdateCvar(true);
			if(value > high)
			{
				value = high;
			}
			else if(value < low)
			{
				value = low;
			}

			float range = high - low;

			if(range <= 0.0f)
			{
				return;
			}

			float thumbPos = (range) ? (value - low) / range : 0.0;
			if(vertical)
			{
				if(verticalFlip)
				{
					thumbPos = 1.f - thumbPos;
				}
				thumbPos *= drawRect.h - thumbHeight;
				thumbPos += drawRect.y;
				thumbRect.y = thumbPos;
				thumbRect.x = drawRect.x;
			}
			else
			{
				thumbPos *= drawRect.w - thumbWidth;
				thumbPos += drawRect.x;
				thumbRect.x = thumbPos;
				thumbRect.y = drawRect.y;
			}
			thumbRect.w = thumbWidth;
			thumbRect.h = thumbHeight;

			if(hover && !noEvents && Contains(gui->CursorX(), gui->CursorY()))
			{
				color = hoverColor;
			}
			else
			{
				hover = false;
			}
			if(flags & WIN_CAPTURE)
			{
				color = hoverColor;
				hover = true;
			}

			dc->DrawMaterial(thumbRect.x, thumbRect.y, thumbRect.w, thumbRect.h, thumbMat, color);
			if(flags & WIN_FOCUS)
			{
				dc->DrawRect(thumbRect.x + 1.0f, thumbRect.y + 1.0f, thumbRect.w - 2.0f, thumbRect.h - 2.0f, 1.0f, color);
			}*/
		}

		public override idWindowVariable GetVariableByName(string name, bool fixup, ref DrawWindow owner)
		{
			string nameLower = name.ToLower();

			if(nameLower == "value")
			{
				return _value;
			}
			else if(nameLower == "cvar")
			{
				return _cvarStr;
			}
			else if(nameLower == "liveupdate")
			{
				return _liveUpdate;
			}
			else if(nameLower == "cvargroup")
			{
				return _cvarGroup;
			}

			return base.GetVariableByName(name, fixup, ref owner);
		}

		public override string HandleEvent(SystemEvent e)
		{
			idConsole.WriteLine("TODO: SliderWindow HandleEvent");
			/* TODO: if (!(event->evType == SE_KEY && event->evValue2)) {
				return "";
			}

			int key = event->evValue;

			if ( event->evValue2 && key == K_MOUSE1 ) {
				SetCapture(this);
				RouteMouseCoords(0.0f, 0.0f);
				return "";
			} 

			if ( key == K_RIGHTARROW || key == K_KP_RIGHTARROW || ( key == K_MOUSE2 && gui->CursorY() > thumbRect.y ) )  {
				value = value + stepSize;
			}

			if ( key == K_LEFTARROW || key == K_KP_LEFTARROW || ( key == K_MOUSE2 && gui->CursorY() < thumbRect.y ) ) {
				value = value - stepSize;
			}

			if (buddyWin) {
				buddyWin->HandleBuddyUpdate(this);
			} else {
				gui->SetStateFloat( cvarStr, value );
				UpdateCvar( false );
			}

			return "";*/
			return string.Empty;
		}
		#endregion

		#region Protected
		protected override void DrawBackground(Microsoft.Xna.Framework.Rectangle drawRect)
		{
			idConsole.WriteLine("TODO: SliderWindow DrawBackground");
			/*if ( !cvar && !buddyWin ) {
				return;
			}

			if ( high - low <= 0.0f ) {
				return;
			}

			idRectangle r = _drawRect;
			if (!scrollbar) {
				if ( vertical ) {
					r.y += thumbHeight / 2.f;
					r.h -= thumbHeight;
				} else {
					r.x += thumbWidth / 2.0;
					r.w -= thumbWidth;
				}
			}*/
	
			base.DrawBackground(drawRect);
		}

		protected override bool ParseInternalVariable(string name, Text.idScriptParser parser)
		{
			string nameLower = name.ToLower();

			if((nameLower == "stepsize") || (nameLower == "step"))
			{
				_stepSize = parser.ParseFloat();
				return true;
			}
			else if(nameLower == "low")
			{
				_low = parser.ParseFloat();
				return true;
			}
			else if(nameLower == "high")
			{
				_high = parser.ParseFloat();
				return true;
			}
			else if(nameLower == "vertical")
			{
				_vertical = parser.ParseBool();
				return true;
			}
			else if(nameLower == "verticalflip")
			{
				_verticalFlip = parser.ParseBool();
				return true;
			}
			else if(nameLower == "scrollbar")
			{
				_scrollBar = parser.ParseBool();
				return true;
			}
			else if(nameLower == "thumbshader")
			{
				_thumbMaterialName = ParseString(parser);
				idE.DeclManager.FindMaterial(_thumbMaterialName);

				return true;
			}

			return base.ParseInternalVariable(name, parser);
		}

		protected override void PostParse()
		{
			base.PostParse();

			
			_value.Set(0.0f);

			_thumbMaterial = idE.DeclManager.FindMaterial(_thumbMaterialName);
			_thumbMaterial.Sort = (float) MaterialSort.Gui;
			_thumbWidth = _thumbMaterial.ImageWidth;
			_thumbHeight = _thumbMaterial.ImageHeight;

			//vertical = state.GetBool("vertical");
			//scrollbar = state.GetBool("scrollbar");

			this.Flags |= WindowFlags.HoldCapture | WindowFlags.CanFocus;
			/* TODO: InitCvar();*/
		}

		protected override string RouteMouseCoordinates(float x, float y)
		{
			idConsole.WriteLine("TODO: SliderWindow RouteMouseCoordinates");
			/*float pct;

			if(!(flags & WIN_CAPTURE))
			{
				return "";
			}

			idRectangle r = drawRect;
			r.x = actualX;
			r.y = actualY;
			r.x += thumbWidth / 2.0;
			r.w -= thumbWidth;
			if(vertical)
			{
				r.y += thumbHeight / 2;
				r.h -= thumbHeight;
				if(gui->CursorY() >= r.y && gui->CursorY() <= r.Bottom())
				{
					pct = (gui->CursorY() - r.y) / r.h;
					if(verticalFlip)
					{
						pct = 1.f - pct;
					}
					value = low + (high - low) * pct;
				}
				else if(gui->CursorY() < r.y)
				{
					if(verticalFlip)
					{
						value = high;
					}
					else
					{
						value = low;
					}
				}
				else
				{
					if(verticalFlip)
					{
						value = low;
					}
					else
					{
						value = high;
					}
				}
			}
			else
			{
				r.x += thumbWidth / 2;
				r.w -= thumbWidth;
				if(gui->CursorX() >= r.x && gui->CursorX() <= r.Right())
				{
					pct = (gui->CursorX() - r.x) / r.w;
					value = low + (high - low) * pct;
				}
				else if(gui->CursorX() < r.x)
				{
					value = low;
				}
				else
				{
					value = high;
				}
			}

			if(buddyWin)
			{
				buddyWin->HandleBuddyUpdate(this);
			}
			else
			{
				gui->SetStateFloat(cvarStr, value);
			}
			UpdateCvar(false);

			return "";*/
			return string.Empty;
		}

		protected override void RunNamedEvent(string name)
		{
			idConsole.WriteLine("TODO: SliderWindow RunNamedEvent");
			/*idStr event, group;
	
			if ( !idStr::Cmpn( eventName, "cvar read ", 10 ) ) {
				event = eventName;
				group = event.Mid( 10, event.Length() - 10 );
				if ( !group.Cmp( cvarGroup ) ) {
					UpdateCvar( true, true );
				}
			} else if ( !idStr::Cmpn( eventName, "cvar write ", 11 ) ) {
				event = eventName;
				group = event.Mid( 11, event.Length() - 11 );
				if ( !group.Cmp( cvarGroup ) ) {
					UpdateCvar( false, true );
				}
			}*/
		}
		#endregion
		#endregion
	}
}
