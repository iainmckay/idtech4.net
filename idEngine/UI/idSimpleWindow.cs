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
	public sealed class idSimpleWindow
	{
		#region Properties
		public string Name
		{
			get
			{
				return _name;
			}
		}

		public idWindow Parent
		{
			get
			{
				return _parent;
			}
		}
		#endregion

		#region Members
		private string _name;

		private idUserInterface _gui;
		private idDeviceContext _context;
		private WindowFlags _flags;

		private idWindow _parent;

		private Rectangle _drawRect; // overall rect
		private Rectangle _clientRect; // client area
		private Rectangle _textRect;

		private Vector2 _origin;
		private int _fontNumber;

		private float _materialScaleX;
		private float _materialScaleY;
		private int _borderSize;
		private TextAlign _textAlign;
		private float _textAlignX;
		private float _textAlignY;
		private int _textShadow;

		private idWinString _text = new idWinString();
		private idWinBool _visible = new idWinBool();
		private idWinRectangle _rect = new idWinRectangle();
		private idWinVector4 _backColor = new idWinVector4();
		private idWinVector4 _foreColor = new idWinVector4();
		private idWinVector4 _materialColor = new idWinVector4();
		private idWinVector4 _borderColor = new idWinVector4();
		private idWinFloat _textScale = new idWinFloat();
		private idWinFloat _rotate = new idWinFloat();
		private idWinVector2 _shear = new idWinVector2();
		private idWinBackground _backgroundName = new idWinBackground();
		private idWinBool _hideCursor = new idWinBool();

		private idMaterial _background;
		#endregion

		#region Constructor
		public idSimpleWindow(idWindow win)
		{
			_gui = win.UserInterface;
			_context = win.DeviceContext;

			_drawRect = win.DrawRectangle;
			_clientRect = win.ClientRectangle;
			_textRect = win.TextRectangle;

			_origin = win.Origin;
			// TODO: _fontNumber = win.FontNumber;
			_name = win.Name;

			_materialScaleX = win.MaterialScaleX;
			_materialScaleY = win.MaterialScaleY;

			_borderSize = win.BorderSize;
			_textAlign = win.TextAlign;
			_textAlignX = win.TextAlignX;
			_textAlignY = win.TextAlignY;
			_background = win.Background;
			_flags = win.Flags;
			_textShadow = win.TextShadow;

			_visible.Set(win.IsVisible);
			_text.Set(win.Text);
			_rect.Set(win.Rectangle);
			_backColor.Set(win.BackColor);
			_foreColor.Set(win.ForeColor);
			_borderColor.Set(win.BorderColor);
			_textScale.Set(win.TextScale);
			_rotate.Set(win.Rotate);
			_shear.Set(win.Shear);
			_backgroundName.Set(win.BackgroundName);

			if(_backgroundName != string.Empty)
			{
				_background = idE.DeclManager.FindMaterial(_backgroundName);
				_background.Sort = MaterialSort.Gui; ;
				_background.ImageClassification = 1; // just for resource tracking
			}

			_backgroundName.Material = _background;

			_parent = win.Parent;
			_hideCursor.Set(win.HideCursor);

			if(_parent != null)
			{
				if(_text.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_text);
				}

				if(_visible.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_visible);
				}

				if(_rect.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_rect);
				}

				if(_backColor.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_backColor);
				}

				if(_materialColor.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_materialColor);
				}

				if(_foreColor.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_foreColor);
				}

				if(_borderColor.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_borderColor);
				}

				if(_textScale.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_textScale);
				}

				if(_rotate.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_rotate);
				}

				if(_shear.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_shear);
				}

				if(_backgroundName.NeedsUpdate == true)
				{
					_parent.AddUpdateVariable(_backgroundName);
				}
			}
		}
		#endregion
	}
}
