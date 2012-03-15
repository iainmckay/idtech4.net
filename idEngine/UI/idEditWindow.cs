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
		#endregion
		#endregion

		#region idWindow implementation
		#region Public
		public override void Activate(bool activate, ref string act)
		{
			base.Activate(activate, ref act);
			idConsole.Warning("TODO: EditWindow Activate");
			/* TODO: if(activate)
			{
				UpdateCvar(true, true);
				EnsureCursorVisible();
			}*/
		}

		public override void Draw(int x, int y)
		{
			idConsole.Warning("TODO: EditWindow Draw");

			/*idVec4 color = foreColor;

			UpdateCvar( true );

			int len = text.Length();
			if ( len != lastTextLength ) {
				scroller->SetValue( 0.0f );
				EnsureCursorVisible();
				lastTextLength = len;
			}
			float scale = textScale;

			idStr		pass;
			const char* buffer;
			if ( password ) {		
				const char* temp = text;
				for ( ; *temp; temp++ )	{
					pass += "*";
				}
				buffer = pass;
			} else {
				buffer = text;
			}

			if ( cursorPos > len ) {
				cursorPos = len;
			}

			idRectangle rect = textRect;

			rect.x -= paintOffset;
			rect.w += paintOffset;

			if ( wrap && scroller->GetHigh() > 0.0f ) {
				float lineHeight = GetMaxCharHeight( ) + 5;
				rect.y -= scroller->GetValue() * lineHeight;
				rect.w -= sizeBias;
				rect.h = ( breaks.Num() + 1 ) * lineHeight;
			}

			if ( hover && !noEvents && Contains(gui->CursorX(), gui->CursorY()) ) {
				color = hoverColor;
			} else {
				hover = false;
			}
			if ( flags & WIN_FOCUS ) {
				color = hoverColor;
			}

			dc->DrawText( buffer, scale, 0, color, rect, wrap, (flags & WIN_FOCUS) ? cursorPos : -1);*/
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
			idConsole.Warning("TODO: EditWindow RunNamedEvent");
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

		#region Protected
		protected override void GainFocus()
		{
			idConsole.Warning("TODO: EditWindow GainFocus");
			/*cursorPos = text.Length();
			EnsureCursorVisible();*/
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
			idConsole.Warning("TODO: EditWindow PostParse");
			/* TODO: if(maxChars == 0)
			{
				maxChars = 10;
			}
			if(sourceFile.Length())
			{
				void* buffer;
				fileSystem->ReadFile(sourceFile, &buffer);
				text = (char*) buffer;
				fileSystem->FreeFile(buffer);
			}

			InitCvar();
			InitScroller(false);

			EnsureCursorVisible();*/

			this.Flags |= WindowFlags.CanFocus;
		}
		#endregion
		#endregion
	}
}
