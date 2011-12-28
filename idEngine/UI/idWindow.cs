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
using idTech4.Text;

namespace idTech4.UI
{
	public sealed class idWindow
	{
		#region Properties
		public Vector4 BackColor
		{
			get
			{
				return _backColor;
			}
		}


		public idMaterial Background
		{
			get
			{
				return _background;
			}
		}

		public string BackgroundName
		{
			get
			{
				return _backgroundName;
			}
		}

		public Vector4 BorderColor
		{
			get
			{
				return _borderColor;
			}
		}

		public int BorderSize
		{
			get
			{
				return _borderSize;
			}
		}

		public Rectangle ClientRectangle
		{
			get
			{
				return _clientRect;
			}
		}

		public idDeviceContext DeviceContext
		{
			get
			{
				return _context;
			}
		}

		public Rectangle DrawRectangle
		{
			get
			{
				return _drawRect;
			}
		}

		public WindowFlags Flags
		{
			get
			{
				return _flags;
			}
			set
			{
				_flags = value;
			}
		}

		public Vector4 ForeColor
		{
			get
			{
				return _foreColor;
			}
		}

		public bool HideCursor
		{
			get
			{
				return _hideCursor;
			}
		}
				
		/// <summary>
		/// Gets whether or not this is a simple window definition.
		/// </summary>
		public bool IsSimple
		{
			get
			{
				if(_ops.Count > 0)
				{
					return false;
				}
				else if((_flags & (WindowFlags.HorizontalCenter | WindowFlags.VerticalCenter)) != 0)
				{
					return false;
				}
				else if((_children.Count > 0) || (_drawWindows.Count > 0))
				{
					return false;
				}
				// TODO: scripts
				/*else if(_scripts.Count > 0)
				{
					return false;
				}*/
				// TODO: events
				/*if (timeLineEvents.Num()) {
					return false;
				}

				if ( namedEvents.Num() ) {
					return false;
				}*/

				return true;
			}
		}

		public bool IsVisible
		{
			get
			{
				return _visible;
			}
		}

		/// <summary>
		/// Gets the name of the window.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		public float MaterialScaleX
		{
			get
			{
				return _materialScaleX;
			}
		}

		public float MaterialScaleY
		{
			get
			{
				return _materialScaleY;
			}
		}

		public Vector2 Origin
		{
			get
			{
				return _origin;
			}
		}

		/// <summary>
		/// Get/set the parent of this window.
		/// </summary>
		public idWindow Parent
		{
			get
			{
				return _parent;
			}
			set
			{
				_parent = value;
			}
		}

		public Rectangle Rectangle
		{
			get
			{
				return _rect.Data;
			}
		}

		
		public float Rotate
		{
			get
			{
				return _rotate;
			}
		}

		public Vector2 Shear
		{
			get
			{
				return _shear;
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
		}

		public TextAlign TextAlign
		{
			get
			{
				return _textAlign;
			}
		}

		public int TextAlignX
		{
			get
			{
				return _textAlignX;
			}
		}

		public int TextAlignY
		{
			get
			{
				return _textAlignY;
			}
		}

		public float TextScale
		{
			get
			{
				return _textScale;
			}
		}

		public int TextShadow
		{
			get
			{
				return _textShadow;
			}
		}
		
		public Rectangle TextRectangle
		{
			get
			{
				return _textRect;
			}
		}

		public idUserInterface UserInterface
		{
			get
			{
				return _gui;
			}
		}
		#endregion

		#region Members
		private idUserInterface _gui;
		private idDeviceContext _context;
		private idWindow _parent;

		private WindowFlags _flags;

		private string _name;
		private string _comment;

		private float _forceAspectWidth;
		private float _forceAspectHeight;
		private float _materialScaleX;
		private float _materialScaleY;
		private int _borderSize;

		private TextAlign _textAlign;
		private int _textAlignX;
		private int _textAlignY;
		private int _textShadow;

		private float _actualX;						// physical coords
		private float _actualY;						// ''
		private int _childID;						// this childs id
		private int _lastTimeRun;					//
		private Rectangle _drawRect;				// overall rect
		private Rectangle _clientRect;				// client area
		private Rectangle _textRect;
		private Vector2 _origin;
		private Vector2 _shear;

		private idMaterial _background;

		private idWinBool _noTime = new idWinBool();
		private idWinBool _visible = new idWinBool();
		private idWinBool _noEvents = new idWinBool();
		private idWinBool _hideCursor = new idWinBool();
		private idWinRectangle _rect = new idWinRectangle();				// overall rect
		private idWinVector4 _backColor = new idWinVector4();
		private idWinVector4 _foreColor = new idWinVector4();
		private idWinVector4 _hoverColor = new idWinVector4();
		private idWinVector4 _borderColor = new idWinVector4();
		private idWinVector4 _materialColor = new idWinVector4();
		private idWinFloat _textScale = new idWinFloat();
		private idWinFloat _rotate = new idWinFloat();
		private idWinString _text = new idWinString();
		private idWinBackground _backgroundName = new idWinBackground();

		private List<idWindow> _children = new List<idWindow>();
		private List<DrawWindow> _drawWindows = new List<DrawWindow>();

		private List<idWindowVariable> _definedVariables = new List<idWindowVariable>();
		private List<idWindowVariable> _updateVariables = new List<idWindowVariable>();

		private bool[] _saveTemporaries;
		private List<bool> _registerIsTemporary = new List<bool>();
		private List<ExpressionOperation> _ops = new List<ExpressionOperation>();
		#endregion

		#region Constructor
		public idWindow(idUserInterface gui)
		{
			_gui = gui;

			Init();
		}

		public idWindow(idUserInterface gui, idDeviceContext context)
		{
			_context = context;
			_gui = gui;

			Init();
		}
		#endregion

		#region Methods
		#region Public
		public void AddChild(idWindow child)
		{
			child._childID = _children.Count;
			_children.Add(child);
		}

		public void AddUpdateVariable(idWindowVariable var)
		{
			_updateVariables.Add(var);
		}

		public DrawWindow FindChildByName(string name)
		{
			if(name.Equals(_name, StringComparison.OrdinalIgnoreCase) == true)
			{
				return new DrawWindow(this);
			}

			foreach(DrawWindow win in _drawWindows)
			{
				if(win.Window != null)
				{
					if(win.Window.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
					{
						return win;
					}

					DrawWindow childWin = win.Window.FindChildByName(name);

					if(childWin != null)
					{
						return childWin;
					}
				}
				else
				{
					if(win.Simple.Name.Equals(name) == true)
					{
						return win;
					}
				}
			}

			return null;
		}

		public bool Parse(idScriptParser parser, bool rebuild)
		{
			if(rebuild == true)
			{
				CleanUp();
			}

			// TODO: events
			/*

			timeLineEvents.Clear();
			transitions.Clear();

			namedEvents.DeleteContents( true );*/

			idToken token2;
			idToken token = parser.ExpectTokenType(TokenType.Name, 0);

			SetInitialState(token.ToString());

			parser.ExpectTokenString("{");
			token = parser.ExpectAnyToken();

			bool ret = true;

			while(token.ToString() != "}")
			{
				// track what was parsed so we can maintain it for the guieditor
				parser.SetMarker();

				if((token.ToString() == "windowDef") || (token.ToString() == "animationDef"))
				{
					if(token.ToString() == "animationDef")
					{
						_visible.Set(false);
						_rect.Set(new Rectangle(0, 0, 0, 0));
					}

					token = parser.ExpectTokenType(TokenType.Name, 0);
					token2 = token;

					parser.UnreadToken(token);

					DrawWindow drawWindow = FindChildByName(token2.ToString());

					if((drawWindow != null) && (drawWindow.Window != null))
					{
						SaveExpressionParseState();
						drawWindow.Window.Parse(parser, rebuild);
						RestoreExpressionParseState();
					}
					else
					{
						idWindow window = new idWindow(_gui, _context);

						SaveExpressionParseState();
						window.Parse(parser, rebuild);
						RestoreExpressionParseState();

						window.Parent = this;

						drawWindow = new DrawWindow();

						if(window.IsSimple == true)
						{
							drawWindow.Simple = new idSimpleWindow(window);
							_drawWindows.Add(drawWindow);
						}
						else
						{
							AddChild(window);
							// TODO: SetFocus(window, false);

							drawWindow.Window = window;
							_drawWindows.Add(drawWindow);
						}
					}
				}
				else if(token.ToString() == "editDef")
				{
					idConsole.WriteLine("TODO: editDef");
					/*idEditWindow *win = new idEditWindow(dc, gui);
		  			SaveExpressionParseState();
					win->Parse(src, rebuild);	
		  			RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "choiceDef")
				{
					idConsole.WriteLine("TODO: choiceDef");

					/*idChoiceWindow *win = new idChoiceWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "sliderDef")
				{
					idConsole.WriteLine("TODO: sliderDef");
					/*idSliderWindow *win = new idSliderWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "markerDef")
				{
					idConsole.WriteLine("TODO: markerDef");
					/*idMarkerWindow *win = new idMarkerWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "bindDef")
				{
					idConsole.WriteLine("TODO: bindDef");
					/*idBindWindow *win = new idBindWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "listDef")
				{
					idConsole.WriteLine("TODO: listDef");
					/*idListWindow *win = new idListWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "fieldDef")
				{
					idConsole.WriteLine("TODO: fieldDef");
					/*idFieldWindow *win = new idFieldWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "renderDef")
				{
					idConsole.WriteLine("TODO: renderDef");
					/*idRenderWindow *win = new idRenderWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "gameSSDDef")
				{
					idConsole.WriteLine("TODO: gameSSDDef");
					/*idGameSSDWindow *win = new idGameSSDWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "gameBearShootDef")
				{
					idConsole.WriteLine("TODO: gameBearShootDef");
					/*idGameBearShootWindow *win = new idGameBearShootWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				else if(token.ToString() == "gameBustOutDef")
				{
					idConsole.WriteLine("TODO: gameBustOutDef");
					/*idGameBustOutWindow *win = new idGameBustOutWindow(dc, gui);
					SaveExpressionParseState();
					win->Parse(src, rebuild);	
					RestoreExpressionParseState();
					AddChild(win);
					win->SetParent(this);
					dwt.simp = NULL;
					dwt.win = win;
					drawWindows.Append(dwt);*/
				}
				// 
				//  added new onEvent
				else if(token.ToString() == "onNamedEvent")
				{
					idConsole.WriteLine("TODO: onNamedEvent");
					// Read the event name
					/*if ( !src->ReadToken(&token) ) {
						src->Error( "Expected event name" );
						return false;
					}

					rvNamedEvent* ev = new rvNamedEvent ( token );
			
					src->SetMarker ( );
			
					if ( !ParseScript ( src, *ev->mEvent ) ) {
						ret = false;
						break;
					}
	
					namedEvents.Append(ev);*/
				}
				else if(token.ToString() == "onTime")
				{
					idConsole.WriteLine("TODO: onTime");
					/*idTimeLineEvent *ev = new idTimeLineEvent;

					if ( !src->ReadToken(&token) ) {
						src->Error( "Unexpected end of file" );
						return false;
					}
					ev->time = atoi(token.c_str());
			
					// reset the mark since we dont want it to include the time
					src->SetMarker ( );

					if (!ParseScript(src, *ev->event, &ev->time)) {
						ret = false;
						break;
					}

					// this is a timeline event
					ev->pending = true;
					timeLineEvents.Append(ev);*/
				}
				else if(token.ToString() == "definefloat")
				{
					idConsole.WriteLine("definefloat");
					/*src->ReadToken(&token);
					work = token;
					work.ToLower();
					idWinFloat *varf = new idWinFloat();
					varf->SetName(work);
					definedVars.Append(varf);

					// add the float to the editors wrapper dict
					// Set the marker after the float name
					src->SetMarker ( );

					// Read in the float 
					regList.AddReg(work, idRegister::FLOAT, src, this, varf);*/
				}
				else if(token.ToString() == "definevec4")
				{
					idConsole.WriteLine("definevec4");
					/*src->ReadToken(&token);
					work = token;
					work.ToLower();
					idWinVec4 *var = new idWinVec4();
					var->SetName(work);

					// set the marker so we can determine what was parsed
					// set the marker after the vec4 name
					src->SetMarker ( );

					// FIXME: how about we add the var to the desktop instead of this window so it won't get deleted
					//        when this window is destoyed which even happens during parsing with simple windows ?
					//definedVars.Append(var);
					gui->GetDesktop()->definedVars.Append( var );
					gui->GetDesktop()->regList.AddReg( work, idRegister::VEC4, src, gui->GetDesktop(), var );*/
				}
				else if(token.ToString() == "float")
				{
					idConsole.WriteLine("TODO: float");
					/*src->ReadToken(&token);
					work = token;
					work.ToLower();
					idWinFloat *varf = new idWinFloat();
					varf->SetName(work);
					definedVars.Append(varf);

					// add the float to the editors wrapper dict
					// set the marker to after the float name
					src->SetMarker ( );

					// Parse the float
					regList.AddReg(work, idRegister::FLOAT, src, this, varf);*/
				}
				// TODO
				/*else if (ParseScriptEntry(token, src)) {

				} else if (ParseInternalVar(token, src)) {

				}
				else {
					ParseRegEntry(token, src);
				} */

				if((token = parser.ReadToken()) == null)
				{
					parser.Error("Unexpected end of file");
					ret = false;

					break;
				}
			}

			if(ret == true)
			{
				// TODO: EvaluateRegisters(-1, true);
			}

			SetupFromState();
			PostParse();

			return ret;
		}
		#endregion

		#region Private
		private void CalculateClientRectangle(int offsetX, int offsetY)
		{
			_drawRect = _rect.Data;

			if((_flags & WindowFlags.InvertRectangle) != 0)
			{
				_drawRect.X = (int) (_rect.X - _rect.Width);
				_drawRect.Y = (int) (_rect.Y - _rect.Height);
			}

			if(((_flags & (WindowFlags.HorizontalCenter | WindowFlags.VerticalCenter)) != 0) && (_parent != null))
			{
				// in this case treat xofs and yofs as absolute top left coords
				// and ignore the original positioning
				if((_flags & WindowFlags.HorizontalCenter) != 0)
				{
					_drawRect.X = (int) ((_parent.Rectangle.Width - _rect.Width) / 2);
				}
				else
				{
					_drawRect.Y = (int) ((_parent.Rectangle.Height - _rect.Height) / 2);
				}
			}

			_drawRect.X += offsetX;
			_drawRect.Y += offsetY;

			_clientRect = _drawRect;

			if((_rect.Height > 0.0f) && (_rect.Width > 0.0f))
			{
				if(((_flags & WindowFlags.Border) != 0) && (_borderSize != 0.0f))
				{
					_clientRect.X += _borderSize;
					_clientRect.Y += _borderSize;
					_clientRect.Width -= _borderSize;
					_clientRect.Height -= _borderSize;
				}

				_textRect = _clientRect;
				_textRect.X += 2;
				_textRect.Y += 2;
				_textRect.Width -= 2;
				_textRect.Height -= 2;

				_textRect.X += _textAlignX;
				_textRect.Y += _textAlignY;
			}

			_origin = new Vector2(_rect.X + (_rect.Width / 2), _rect.Y + (_rect.Height / 2));
	}

		private void CleanUp()
		{			
			// TODO
			/*int i, c = drawWindows.Num();
			for (i = 0; i < c; i++) {
				delete drawWindows[i].simp;
			}

			// ensure the register list gets cleaned up
			regList.Reset ( );
	
			// Cleanup the named events
			namedEvents.DeleteContents(true);*/

			_drawWindows.Clear();
			_children.Clear();

			// TODO
			/*definedVars.DeleteContents(true);
			timeLineEvents.DeleteContents(true);
			for (i = 0; i < SCRIPT_COUNT; i++) {
				delete scripts[i];
			}*/

			Init();
		}

		private void Init()
		{
			_childID = 0;
			_flags = 0;
			_lastTimeRun = 0;

			_origin = Vector2.Zero;

			/*fontNum = 0;
			timeLine = -1;
			xOffset = yOffset = 0.0;
			cursor = 0;*/
			_forceAspectWidth = 640;
			_forceAspectHeight = 480;
			_materialScaleX = 1;
			_materialScaleY = 1;
			_borderSize = 0;
			
			_textAlign = TextAlign.Left;
			_textAlignX = 0;
			_textAlignY = 0;

			_noTime.Set(false);
			_visible.Set(true);
			_shear = Vector2.Zero;

			_noEvents.Set(false);
			_rotate.Set(0);
			_textScale.Set(0.35f);

			_backColor.Set(Vector4.Zero);
			_foreColor.Set(new Vector4(1, 1, 1, 1));
			_hoverColor.Set(new Vector4(1, 1, 1, 1));
			_materialColor.Set(new Vector4(1, 1, 1, 1));
			_borderColor.Set(Vector4.Zero);

			_background = null;
			_backgroundName.Set(string.Empty);

			// TODO
			/*
			focusedChild = NULL;
			captureChild = NULL;
			overChild = NULL;*/
			_parent = null;
			/*saveOps = NULL;
			saveRegs = NULL;
			timeLine = -1;
			textShadow = 0;
			hover = false;

			for(int i = 0; i < SCRIPT_COUNT; i++)
			{
				scripts[i] = NULL;
			}

			hideCursor = false;*/
		}
				
		private void PostParse()
		{

		}

		private void RestoreExpressionParseState()
		{
			_registerIsTemporary.Clear();
			_registerIsTemporary.AddRange(_saveTemporaries);

			_saveTemporaries = null;
		}

		private void SaveExpressionParseState()
		{
			_saveTemporaries = _registerIsTemporary.ToArray();
		}
				
		private void SetupBackground()
		{
			if(_backgroundName != string.Empty)
			{
				_background = idE.DeclManager.FindMaterial(_backgroundName);
				_background.ImageClassification = 1; // just for resource tracking

				if((_background != null) && (_background.TestMaterialFlag(MaterialFlags.Defaulted) == false))
				{
					_background.Sort = MaterialSort.Gui;
				}
			}

			_backgroundName.Material = _background;
		}

		private void SetupFromState()
		{
			SetupBackground();

			if(_borderSize > 0)
			{
				_flags |= WindowFlags.Border;
			}

			// TODO: rotate/shear
			/*if (regList.FindReg("rotate") || regList.FindReg("shear")) {
				flags |= WIN_TRANSFORM;
			}*/

			CalculateClientRectangle(0, 0);

			// TODO: scripts
			/*if ( scripts[ ON_ACTION ] ) {
				cursor = idDeviceContext::CURSOR_HAND;
				flags |= WIN_CANFOCUS;
			}*/
		}

		private void SetInitialState(string name)
		{
			_name = name;

			_materialScaleX = 1.0f;
			_materialScaleY = 1.0f;

			_forceAspectWidth = 640.0f;
			_forceAspectHeight = 480.0f;

			_noTime.Set(false);
			_visible.Set(true);
			_flags = 0;
		}
		#endregion
		#endregion
	}

	public sealed class DrawWindow
	{
		public idSimpleWindow Simple;
		public idWindow Window;

		public DrawWindow()
		{

		}

		public DrawWindow(idSimpleWindow simple)
		{
			Simple = simple;
		}

		public DrawWindow(idWindow window)
		{
			Window = window;
		}
	}

	public enum TextAlign
	{
		Left,
		Center,
		Right
	}
}