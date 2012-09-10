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
	public sealed class idEditWindow : idWindow
	{
		#region Members
		private int _maxChars;
		private int	_paintOffset;
		private int	_cursorPosition;
		private int	_cursorLine;
		private int	_cvarMax;
		private bool _wrap;
		private bool _readOnly;
		private bool _numeric;
		private string _sourceFile;
		private idSliderWindow _scroller;
		private List<int> _breaks = new List<int>();
		private float _sizeBias;
		private int	_textIndex;
		private int	_lastTextLength;
		private bool _forceScroll;

		private idCvar _cvar;
		private idWinBool _password = new idWinBool("password");
		private idWinString _cvarStr = new idWinString("cvar");	
		private idWinBool _liveUpdate = new idWinBool("liveUpdate");
		private idWinString _cvarGroup = new idWinString("cvarGroup");
		#endregion

		#region Constructor
		public idEditWindow(idUserInterface gui)
			: base(gui)
		{
			Init();
		}

		public idEditWindow(idDeviceContext context, idUserInterface gui)
			: base(gui, context)
		{
			Init();
		}
		#endregion

		#region Methods
		#region Private
		private void EnsureCursorVisible()
		{
			if(_readOnly == true)
			{
				_cursorPosition = -1;
			}
			else if(_maxChars == 1)
			{
				_cursorPosition = 0;
			}

			if(this.DeviceContext == null)
			{
				return;
			}

			SetFont();

			if(_wrap == false)
			{
				int cursorX = 0;

				if(_password == true)
				{
					cursorX = _cursorPosition * this.DeviceContext.GetCharacterWidth('*', this.TextScale);
				}
				else
				{
					int i = 0;
					int length = this.Text.Length;

					while((i < length) && (i < _cursorPosition))
					{
						if(idHelper.IsColor(this.Text, i) == true)
						{
							i += 2;
						}
						else 
						{
							cursorX += this.DeviceContext.GetCharacterWidth(this.Text[i], this.TextScale);
							i++;
						}
					}
				}

				int maxWidth = (int) this.MaximumCharacterWidth;
				int left = cursorX - maxWidth;
				int right = (int) (cursorX - this.TextRectangle.Width) + maxWidth;

				if(_paintOffset > left)
				{
					// when we go past the left side, we want the text to jump 6 characters
					_paintOffset = left - maxWidth * 6;
				}

				if(_paintOffset < right)
				{
					_paintOffset = right;
				}

				if(_paintOffset < 0)
				{
					_paintOffset = 0;
				}

				_scroller.SetRange(0, 0, 1);
			}
			else 
			{
				// word wrap
				_breaks.Clear();

				idRectangle rect = this.TextRectangle;
				rect.Width -= _sizeBias;

				this.DeviceContext.DrawText(this.Text, this.TextScale, this.TextAlign, idColor.White, rect, true, ((this.Flags & WindowFlags.Focus) == WindowFlags.Focus) ? _cursorPosition : -1, true, _breaks);

				int fit = (int) (this.TextRectangle.Height / (this.MaximumCharacterHeight + 5));

				if(fit < (_breaks.Count + 1))
				{
					_scroller.SetRange(0, _breaks.Count + 1 - fit, 1);
				}
				else
				{
					// the text fits completely in the box
					_scroller.SetRange(0, 0, 1);
				}

				if(_forceScroll == true)
				{
					_scroller.Value = _breaks.Count - fit;
				}
				else if(_readOnly == true)
				{

				}
				else
				{
					_cursorLine = 0;
					int count = _breaks.Count;

					for(int i = 1; i < count; i++)
					{
						if(_cursorPosition >= _breaks[i])
						{
							_cursorLine = i;
						}
						else
						{
							break;
						}
					}

					int topLine = (int) _scroller.Value;

					if(_cursorLine < topLine)
					{
						_scroller.Value = _cursorLine;
					}
					else if(_cursorLine >= (topLine + fit))
					{
						_scroller.Value = (_cursorLine - fit) + 1;
					}
				}
			}
		}

		private void Init()
		{
			_maxChars = 128;
			_numeric = false;
			_paintOffset = 0;
			_cursorPosition = 0;
			_cursorLine = 0;
			_cvarMax = 0;
			_wrap = false;
			_sourceFile = "";
			_scroller = null;
			_sizeBias = 0;
			_lastTextLength = 0;
			_forceScroll = false;			
			_cvar = null;
			_readOnly = false;

			_password.Set(false);
			_liveUpdate.Set(true);			

			_scroller = new idSliderWindow(this.DeviceContext, this.UserInterface);
		}

		private void InitConsoleVariables()
		{
			if((_cvarStr == null) || (_cvarStr == string.Empty))
			{
				if((_text.Name == null) || (_text.Name == string.Empty))
				{
					idConsole.Warning("idEditWindow.InitConsoleVariables: gui '{0}' window '{1}' has an empty cvar string", this.UserInterface.SourceFile, this.Name);
				}

				_cvar = null;
			}
			else
			{
				_cvar = idE.CvarSystem.Find(_cvarStr);

				if(_cvar == null)
				{
					idConsole.Warning("idEditWindow.InitConsoleVariables: gui '{0}' window '{1}' references undefined cvar '{2}'", this.UserInterface.SourceFile, this.Name, _cvarStr);
				}
			}
		}

		private void InitScroller(bool horizontal)
		{
			string thumbImage = "guis/assets/scrollbar_thumb.tga";
			string barImage = "guis/assets/scrollbarv.tga";
			string scrollerName = "_scrollerWinV";

			if(horizontal == true)
			{
				barImage = "guis/assets/scrollbarh.tga";
				scrollerName = "_scrollerWinH";
			}

			idMaterial mat = idE.DeclManager.FindMaterial(barImage);
			mat.Sort = (float) MaterialSort.Gui;

			_sizeBias = mat.ImageWidth;

			idRectangle scrollRect;

			if(horizontal == true)
			{
				_sizeBias = mat.ImageHeight;

				scrollRect.X = 0;
				scrollRect.Y = this.ClientRectangle.Height - _sizeBias;
				scrollRect.Width = this.ClientRectangle.Width;
				scrollRect.Height = _sizeBias;
			}
			else
			{
				scrollRect.X = this.ClientRectangle.Width - _sizeBias;
				scrollRect.Y = 0;
				scrollRect.Width = _sizeBias;
				scrollRect.Height = this.ClientRectangle.Height;

			}

			_scroller.InitWithDefaults(scrollerName, scrollRect, this.ForeColor, this.MaterialColor, mat.Name, thumbImage, !horizontal, true);

			InsertChild(_scroller, null);

			_scroller.Buddy = this;
		}

		private void UpdateConsoleVariables(bool read, bool force = false)
		{
			if((force == true) || (_liveUpdate == true))
			{
				if(_cvar != null)
				{
					if(read == true)
					{
						this.Text = _cvar.ToString();
					}
					else
					{
						_cvar.Set(this.Text);

						if((_cvarMax > 0) && (_cvar.ToInt() > _cvarMax))
						{
							_cvar.Set(_cvarMax);
						}
					}
				}
			}
		}
		#endregion
		#endregion

		#region idWindow implementation
		#region Public
		public override void Activate(bool activate, ref string act)
		{
			base.Activate(activate, ref act);
			
			if(activate == true)
			{
				UpdateConsoleVariables(true, true);
				EnsureCursorVisible();
			}
		}

		public override void Draw(float x, float y)
		{
			UpdateConsoleVariables(true);

			Vector4 color = this.ForeColor;

			int length = this.Text.Length;

			if(length != _lastTextLength)
			{
				_scroller.Value = 0.0f;
				EnsureCursorVisible();

				_lastTextLength = length;
			}

			float scale = this.TextScale;
			string str = this.Text;

			if(_password == true)
			{
				str = new String('*', this.Text.Length);
			}

			if(_cursorPosition > length)
			{
				_cursorPosition = length;
			}

			idRectangle rect = this.TextRectangle;
			rect.X -= _paintOffset;
			rect.Width += _paintOffset;

			if((_wrap == true) && (_scroller.High > 0.0f))
			{
				float lineHeight = this.MaximumCharacterHeight + 5;

				rect.Y -= _scroller.Value * lineHeight;
				rect.Width -= _sizeBias;
				rect.Height = (_breaks.Count + 1) * lineHeight;
			}

			if((this.Hover == true) && (this.NoEvents == false) && (this.Contains(this.UserInterface.CursorX, this.UserInterface.CursorY) == true))
			{
				color = this.HoverColor;
			}
			else
			{
				this.Hover = false;
			}

			if((this.Flags & WindowFlags.Focus) == WindowFlags.Focus)
			{
				color = this.HoverColor;
			}

			this.DeviceContext.DrawText(str, scale, 0, color, rect, _wrap, ((this.Flags & WindowFlags.Focus) == WindowFlags.Focus) ? _cursorPosition : -1);
		}
				
		public override idWindowVariable GetVariableByName(string name, bool fixup, ref DrawWindow owner)
		{
			string nameLower = name.ToLower();

			if(nameLower == "cvar")
			{
				return _cvarStr;
			}
			else if(nameLower == "password")
			{
				return _password;
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
			idConsole.Warning("TODO: EditWindow HandleEvent");
			/* TODO: static char buffer[ MAX_EDITFIELD ];
			const char *ret = "";

			if ( wrap ) {
				// need to call this to allow proper focus and capturing on embedded children
				ret = idWindow::HandleEvent( event, updateVisuals );
				if ( ret && *ret ) {
					return ret;
				}
			}

			if ( ( event->evType != SE_CHAR && event->evType != SE_KEY ) ) {
				return ret;
			}

			idStr::Copynz( buffer, text.c_str(), sizeof( buffer ) );
			int key = event->evValue;
			int len = text.Length();

			if ( event->evType == SE_CHAR ) {
				if ( event->evValue == Sys_GetConsoleKey( false ) || event->evValue == Sys_GetConsoleKey( true ) ) {
					return "";
				}

				if ( updateVisuals ) {
					*updateVisuals = true;
				}

				if ( maxChars && len > maxChars ) {
					len = maxChars;
				}
	
				if ( ( key == K_ENTER || key == K_KP_ENTER ) && event->evValue2 ) {
					RunScript( ON_ACTION );
					RunScript( ON_ENTER );
					return cmd;
				}

				if ( key == K_ESCAPE ) {
					RunScript( ON_ESC );
					return cmd;
				}

				if ( readonly ) {
					return "";
				}

				if ( key == 'h' - 'a' + 1 || key == K_BACKSPACE ) {	// ctrl-h is backspace
   					if ( cursorPos > 0 ) {
						if ( cursorPos >= len ) {
							buffer[len - 1] = 0;
							cursorPos = len - 1;
						} else {
							memmove( &buffer[ cursorPos - 1 ], &buffer[ cursorPos ], len + 1 - cursorPos);
							cursorPos--;
						}

						text = buffer;
						UpdateCvar( false );
						RunScript( ON_ACTION );
					}

					return "";
   				}

   				//
   				// ignore any non printable chars (except enter when wrap is enabled)
   				//
				if ( wrap && (key == K_ENTER || key == K_KP_ENTER) ) {
				} else if ( !idStr::CharIsPrintable( key ) ) {
					return "";
				}

				if ( numeric ) {
					if ( ( key < '0' || key > '9' ) && key != '.' ) {
	       				return "";
					}
				}

				if ( dc->GetOverStrike() ) {
					if ( maxChars && cursorPos >= maxChars ) {
	       				return "";
					}
				} else {
					if ( ( len == MAX_EDITFIELD - 1 ) || ( maxChars && len >= maxChars ) ) {
	       				return "";
					}
					memmove( &buffer[ cursorPos + 1 ], &buffer[ cursorPos ], len + 1 - cursorPos );
				}

				buffer[ cursorPos ] = key;

				text = buffer;
				UpdateCvar( false );
				RunScript( ON_ACTION );

				if ( cursorPos < len + 1 ) {
					cursorPos++;
				}
				EnsureCursorVisible();

			} else if ( event->evType == SE_KEY && event->evValue2 ) {

				if ( updateVisuals ) {
					*updateVisuals = true;
				}

				if ( key == K_DEL ) {
					if ( readonly ) {
						return ret;
					}
					if ( cursorPos < len ) {
						memmove( &buffer[cursorPos], &buffer[cursorPos + 1], len - cursorPos);
						text = buffer;
						UpdateCvar( false );
						RunScript( ON_ACTION );
					}
					return ret;
				}

				if ( key == K_RIGHTARROW )  {
					if ( cursorPos < len ) {
						if ( idKeyInput::IsDown( K_CTRL ) ) {
							// skip to next word
							while( ( cursorPos < len ) && ( buffer[ cursorPos ] != ' ' ) ) {
								cursorPos++;
							}

							while( ( cursorPos < len ) && ( buffer[ cursorPos ] == ' ' ) ) {
								cursorPos++;
							}
						} else {
							if ( cursorPos < len ) {
								cursorPos++;
							}
						}
					} 

					EnsureCursorVisible();

					return ret;
				}

				if ( key == K_LEFTARROW ) {
					if ( idKeyInput::IsDown( K_CTRL ) ) {
						// skip to previous word
						while( ( cursorPos > 0 ) && ( buffer[ cursorPos - 1 ] == ' ' ) ) {
							cursorPos--;
						}

						while( ( cursorPos > 0 ) && ( buffer[ cursorPos - 1 ] != ' ' ) ) {
							cursorPos--;
						}
					} else {
						if ( cursorPos > 0 ) {
							cursorPos--;
						}
					}

					EnsureCursorVisible();

					return ret;
				}

				if ( key == K_HOME ) {
					if ( idKeyInput::IsDown( K_CTRL ) || cursorLine <= 0 || ( cursorLine >= breaks.Num() ) ) {
						cursorPos = 0;
					} else {
						cursorPos = breaks[cursorLine];
					}
					EnsureCursorVisible();
					return ret;
				}

				if ( key == K_END )  {
					if ( idKeyInput::IsDown( K_CTRL ) || (cursorLine < -1) || ( cursorLine >= breaks.Num() - 1 ) ) {
						cursorPos = len;
					} else {
						cursorPos = breaks[cursorLine + 1] - 1;
					}
					EnsureCursorVisible();
					return ret;
				}

				if ( key == K_INS ) {
					if ( !readonly ) {
						dc->SetOverStrike( !dc->GetOverStrike() );
					}
					return ret;
				}

				if ( key == K_DOWNARROW ) {
					if ( idKeyInput::IsDown( K_CTRL ) ) {
						scroller->SetValue( scroller->GetValue() + 1.0f );
					} else {
						if ( cursorLine < breaks.Num() - 1 ) {
							int offset = cursorPos - breaks[cursorLine];
							cursorPos = breaks[cursorLine + 1] + offset;
							EnsureCursorVisible();
						}
					}
				}

				if (key == K_UPARROW ) {
					if ( idKeyInput::IsDown( K_CTRL ) ) {
						scroller->SetValue( scroller->GetValue() - 1.0f );
					} else {
						if ( cursorLine > 0 ) {
							int offset = cursorPos - breaks[cursorLine];
							cursorPos = breaks[cursorLine - 1] + offset;
							EnsureCursorVisible();
						}
					}
				}

				if ( key == K_ENTER || key == K_KP_ENTER ) {
					RunScript( ON_ACTION );
					RunScript( ON_ENTER );
					return cmd;
				}

				if ( key == K_ESCAPE ) {
					RunScript( ON_ESC );
					return cmd;
				}

			} else if ( event->evType == SE_KEY && !event->evValue2 ) {
				if ( key == K_ENTER || key == K_KP_ENTER ) {
					RunScript( ON_ENTERRELEASE );
					return cmd;
				} else {
					RunScript( ON_ACTIONRELEASE );
				}
			}

			return ret;*/
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
		protected override void OnFocusGained()
		{
			base.OnFocusGained();

			_cursorPosition = this.Text.Length;
			EnsureCursorVisible();
		}

		protected override bool ParseInternalVariable(string name, Text.idScriptParser parser)
		{
			string nameLower = name.ToLower();

			if(nameLower == "maxchars")
			{
				_maxChars = parser.ParseInteger();
			}
			else if(nameLower == "numeric")
			{
				_numeric = parser.ParseBool();
			}
			else if(nameLower == "wrap")
			{
				_wrap = parser.ParseBool();
			}
			else if(nameLower == "readonly")
			{
				_readOnly = parser.ParseBool();
			}
			else if(nameLower == "forcescroll")
			{
				_forceScroll = parser.ParseBool();
			}
			else if(nameLower == "source")
			{
				_sourceFile = ParseString(parser);
			}
			else if(nameLower == "password")
			{
				_password.Set(parser.ParseBool());
			}
			else if(nameLower == "cvarmax")
			{
				_cvarMax = parser.ParseInteger();
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

			if(_maxChars == 0)
			{
				_maxChars = 10;
			}

			if((_sourceFile != null) && (_sourceFile != string.Empty))
			{
				byte[] tmp = idE.FileSystem.ReadFile(_sourceFile);
				this.Text = Encoding.UTF8.GetString(tmp);
			}

			InitConsoleVariables();
			InitScroller(false);

			EnsureCursorVisible();

			this.Flags |= WindowFlags.CanFocus;
		}
		#endregion
		#endregion
	}
}