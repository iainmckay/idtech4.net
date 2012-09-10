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

using idTech4.Input;
using idTech4.Renderer;

namespace idTech4.UI
{
	public sealed class idSliderWindow : idWindow
	{
		#region Properties
		public float High
		{
			get
			{
				return _high;
			}
		}

		public float Low
		{
			get
			{
				return _low;
			}
		}

		public float Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value.Set(value);
			}
		}
		#endregion

		#region Members
		private float _low;
		private float _high;
		
		private float _thumbWidth;
		private float _thumbHeight;
		private idRectangle _thumbRect;
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
		#region Public
		public void InitWithDefaults(string name, idRectangle rect, Vector4 foreColor, Vector4 materialColor, string background, string thumbMaterial, bool vertical, bool scrollbar)
		{
			SetInitialState(name);

			_rect.Set(rect);
			_foreColor.Set(foreColor);
			_materialColor.Set(materialColor);

			_thumbMaterial = idE.DeclManager.FindMaterial(thumbMaterial);
			_thumbMaterial.Sort = (float) MaterialSort.Gui;

			_thumbWidth = _thumbMaterial.ImageWidth;
			_thumbHeight = _thumbMaterial.ImageHeight;

			_background = idE.DeclManager.FindMaterial(_backgroundName);
			_background.Sort = (float) MaterialSort.Gui;

			_vertical = vertical;
			_scrollBar = scrollbar;

			this.Flags |= WindowFlags.HoldCapture;
		}

		public void SetRange(float low, float high, float step)
		{
			_low = low;
			_high = high;
			_stepSize = step;
		}
		#endregion

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

		private void InitCvar()
		{
			if(_cvarStr == string.Empty)
			{
				if(this.Buddy == null)
				{
					idConsole.Warning("idSliderWindow::InitCvar: gui '{0}' in window '{1}' has an empty cvar string", this.UserInterface.SourceFile, this.Name);
				}

				_cvarInit = true;
				_cvar = null;
			}
			else
			{
				_cvar = idE.CvarSystem.Find(_cvarStr.ToString());

				if(_cvar == null)
				{
					idConsole.Warning("idSliderWindow::InitCvar: gui '{0}' in window '{1}' references undefined cvar '{2}'", this.UserInterface.SourceFile, this.Name, _cvarStr.ToString());
					_cvarInit = true;
				}
			}
		}

		private void UpdateConsoleVariables(bool read, bool force = false)
		{
			if((this.Buddy != null) || (_cvar == null))
			{
				return;
			}

			if((force == true) || (_liveUpdate == true))
			{
				_value.Set(_cvar.ToFloat());

				if(_value != this.UserInterface.State.GetFloat(_cvarStr))
				{
					if(read == true)
					{
						this.UserInterface.State.Set(_cvarStr, _value);
					}
					else
					{
						_value.Set(this.UserInterface.State.GetFloat(_cvarStr));
						_cvar.Set(_value);
					}
				}
			}
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
			
			if(activate == true)
			{
				UpdateConsoleVariables(true, true);
			}
		}

		public override void Draw(float x, float y)
		{
			Vector4 color = this.ForeColor;

			if((_cvar == null) && (_buddyWindow == null))
			{
				return;
			}

			if((_thumbWidth == 0) || (_thumbHeight == 0))
			{
				_thumbWidth = _thumbMaterial.ImageWidth;
				_thumbHeight = _thumbMaterial.ImageHeight;
			}
			
			UpdateConsoleVariables(true);

			if(_value > _high)
			{
				_value.Set(_high);
			}
			else if(_value < _low)
			{
				_value.Set(_low);
			}

			float range = _high - _low;

			if(range <= 0.0f)
			{
				return;
			}
			
			float thumbPosition = (range > 0) ? ((_value - _low) / range) : 0;

			if(_vertical == true)
			{
				if(_verticalFlip == true)
				{
					thumbPosition = 1.0f - thumbPosition;
				}

				thumbPosition *= this.DrawRectangle.Height - _thumbHeight;
				thumbPosition += this.DrawRectangle.Y;

				_thumbRect.Y = thumbPosition;
				_thumbRect.X = this.DrawRectangle.X;
			}
			else
			{
				thumbPosition *= this.DrawRectangle.Width - _thumbWidth;
				thumbPosition += this.DrawRectangle.X;

				_thumbRect.X = thumbPosition;
				_thumbRect.Y = this.DrawRectangle.Y;
			}

			_thumbRect.Width = _thumbWidth;
			_thumbRect.Height = _thumbHeight;

			if((this.Hover != null) && (this.NoEvents == false) && (this.Contains(this.UserInterface.CursorX, this.UserInterface.CursorY) == true))
			{
				color = this.HoverColor;
			}
			else
			{
				this.Hover = false;
			}

			if((this.Flags & WindowFlags.Capture) == WindowFlags.Capture)
			{
				color = this.HoverColor;
				this.Hover = true;
			}

			this.DeviceContext.DrawMaterial(_thumbRect.X, _thumbRect.Y, _thumbRect.Width, _thumbRect.Height, _thumbMaterial, color);

			if((this.Flags & WindowFlags.Focus) == WindowFlags.Focus)
			{
				this.DeviceContext.DrawRectangle(_thumbRect.X + 1.0f, _thumbRect.Y + 1.0f, _thumbRect.Width - 2.0f, _thumbRect.Height - 2.0f, 1.0f, color);
			}
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

		public override string HandleEvent(SystemEvent e, ref bool updateVisuals)
		{
			if(((e.Type == SystemEventType.Key) && (e.Value2 > 0)) == false)
			{
				return string.Empty;
			}

			Keys key = (Keys) e.Value;

			if((e.Value2 > 0) && (key == Keys.Mouse1))
			{
				this.CaptureChild = this;
				RouteMouseCoordinates(0, 0);

				return string.Empty;
			}
 
			if((key == Keys.Right) || ((key == Keys.Mouse2) && ((this.UserInterface.CursorY > _thumbRect.Y) == true)))
			{
				_value.Set(_value + _stepSize);
			}

			if((key == Keys.Left) || ((key == Keys.Mouse2) && ((this.UserInterface.CursorY < _thumbRect.Y) == true)))
			{
				_value.Set(_value - _stepSize);
			}

			if(this.Buddy != null)
			{
				this.Buddy.HandleBuddyUpdate(this);
			}
			else
			{
				this.UserInterface.State.Set(_cvarStr.ToString(), _value);
			}

			UpdateConsoleVariables(false);
			
			return string.Empty;
		}

		public override void RunNamedEvent(string name)
		{			
			if(name.StartsWith("cvar read") == true)
			{
				if(name.Substring(10) == _cvarGroup.ToString())
				{
					UpdateConsoleVariables(true, true);
				}
			}
			else if(name.StartsWith("cvar write") == true)
			{
				if(name.Substring(11) == _cvarGroup.ToString())
				{
					UpdateConsoleVariables(false, true);
				}
			}
		}
		#endregion

		#region Protected
		protected override void DrawBackground(idRectangle drawRect)
		{
			if((_cvar == null) && (this.Buddy == null))
			{
				return;
			}

			if((_high - _low) <= 0.0f)
			{
				return;
			}

			idRectangle r = this.DrawRectangle;

			if(_scrollBar == false)
			{
				if(_vertical == true)
				{
					r.Y += _thumbHeight / 2.0f;
					r.Height -= _thumbHeight;
				}
				else
				{
					r.X += _thumbWidth / 2.0f;
					r.Width -= _thumbWidth;
				}
			}
	
			base.DrawBackground(r);
		}

		protected override bool ParseInternalVariable(string name, Text.idScriptParser parser)
		{
			string nameLower = name.ToLower();

			if((nameLower == "stepsize") || (nameLower == "step"))
			{
				_stepSize = parser.ParseFloat();
			}
			else if(nameLower == "low")
			{
				_low = parser.ParseFloat();
			}
			else if(nameLower == "high")
			{
				_high = parser.ParseFloat();
			}
			else if(nameLower == "vertical")
			{
				_vertical = parser.ParseBool();
			}
			else if(nameLower == "verticalflip")
			{
				_verticalFlip = parser.ParseBool();
			}
			else if(nameLower == "scrollbar")
			{
				_scrollBar = parser.ParseBool();
			}
			else if(nameLower == "thumbshader")
			{
				_thumbMaterialName = ParseString(parser);
				idE.DeclManager.FindMaterial(_thumbMaterialName);
			}
			else
			{
				return base.ParseInternalVariable(name, parser);
			}

			return true;
		}

		protected override void PostParse()
		{
			base.PostParse();
						
			_value.Set(0.0f);

			_thumbMaterial = idE.DeclManager.FindMaterial(_thumbMaterialName);
			_thumbMaterial.Sort = (float) MaterialSort.Gui;
			_thumbWidth = _thumbMaterial.ImageWidth;
			_thumbHeight = _thumbMaterial.ImageHeight;


			this.Flags |= WindowFlags.HoldCapture | WindowFlags.CanFocus;

			InitCvar();
		}

		protected override string RouteMouseCoordinates(float x, float y)
		{
			if((this.Flags & WindowFlags.Capture) == 0)
			{
				return string.Empty;
			}

			idRectangle rect = this.DrawRectangle;
			rect.X = this.ActualX;
			rect.Y = this.ActualY;
			rect.X += _thumbWidth / 2.0f;
			rect.Width -= _thumbWidth;

			float percent = 0;

			if(_vertical == true)
			{
				rect.Y += _thumbHeight / 2.0f;
				rect.Height -= _thumbHeight;

				if((this.UserInterface.CursorY >= rect.Y) && (this.UserInterface.CursorY <= rect.Bottom))
				{
					percent = (this.UserInterface.CursorY - rect.Y) / rect.Height;

					if(_verticalFlip == true)
					{
						percent = 1.0f - percent;
					}

					_value.Set(_low + (_high - _low) * percent);
				}
				else if(this.UserInterface.CursorY < rect.Y)
				{
					if(_verticalFlip == true)
					{
						_value.Set(_high);
					}
					else
					{
						_value.Set(_low);
					}
				}
				else
				{
					if(_verticalFlip == true)
					{
						_value.Set(_low);
					}
					else
					{
						_value.Set(_high);
					}
				}
			}
			else
			{
				rect.X += _thumbWidth / 2.0f;
				rect.Width -= _thumbWidth;

				if((this.UserInterface.CursorX >= rect.X) && (this.UserInterface.CursorX <= rect.Right))
				{
					percent = (this.UserInterface.CursorX - rect.X) / rect.Width;
					_value.Set(_low + (_high - _low) * percent);
				}
				else if(this.UserInterface.CursorX < rect.X)
				{
					_value.Set(_low);
				}
				else

				{
					_value.Set(_high);
				}
			}

			if(this.Buddy != null)
			{
				this.Buddy.HandleBuddyUpdate(this);
			}
			else
			{
				this.UserInterface.State.Set(_cvarStr.ToString(), _value);
			}

			UpdateConsoleVariables(false);

			return string.Empty;
		}
		#endregion
		#endregion
	}
}