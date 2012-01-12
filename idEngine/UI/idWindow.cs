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
using idTech4.Text.Decl;

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
			set
			{
				_backColor.Set(value);
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
			set
			{
				_drawRect = value;
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
			set
			{
				_foreColor.Set(value);
			}
		}

		public bool HideCursor
		{
			get
			{
				return _hideCursor;
			}
		}

		public bool IsInteractive
		{
			get
			{
				// TODO: scripts
				/*if ( scripts[ ON_ACTION ] ) {
					return true;
				}*/

				foreach(idWindow child in _children)
				{
					if(child.IsInteractive == true)
					{
						return true;
					}
				}

				return false;
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
			set
			{
				_name = value;
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
			set
			{
				_rect.Set(value);
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
			set
			{
				_text.Set(value);
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

		private int _offsetX;
		private int _offsetY;

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

		private idWinBool _noTime = new idWinBool("noTime");
		private idWinBool _visible = new idWinBool("visible");
		private idWinBool _noEvents = new idWinBool("noEvents");
		private idWinBool _hideCursor = new idWinBool("hideCursor");
		private idWinRectangle _rect = new idWinRectangle("rect");			// overall rect
		private idWinVector4 _backColor = new idWinVector4("backColor");
		private idWinVector4 _foreColor = new idWinVector4("foreColor");
		private idWinVector4 _hoverColor = new idWinVector4("hoverColor");
		private idWinVector4 _borderColor = new idWinVector4("borderColor");
		private idWinVector4 _materialColor = new idWinVector4("matColor");
		private idWinFloat _textScale = new idWinFloat("textScale");
		private idWinFloat _rotate = new idWinFloat("rotate");
		private idWinString _text = new idWinString("text");
		private idWinBackground _backgroundName = new idWinBackground("background");

		private List<idWindow> _children = new List<idWindow>();
		private List<DrawWindow> _drawWindows = new List<DrawWindow>();

		private List<idWindowVariable> _definedVariables = new List<idWindowVariable>();
		private List<idWindowVariable> _updateVariables = new List<idWindowVariable>();

		private bool[] _saveTemporaries;
		private bool[] _registerIsTemporary = new bool[idE.MaxExpressionRegisters];
		private List<float> _expressionRegisters = new List<float>();
		private List<WindowExpressionOperation> _ops = new List<WindowExpressionOperation>();

		private idRegisterList _regList = new idRegisterList();

		private static float[] _regs = new float[idE.MaxExpressionRegisters];
		private static idWindow _lastEval;

		private static Dictionary<string, RegisterType> _builtInVariables = new Dictionary<string, RegisterType>(StringComparer.OrdinalIgnoreCase);
		#endregion

		#region Constructor
		static idWindow()
		{
			_builtInVariables.Add("foreColor", RegisterType.Vector4);
			_builtInVariables.Add("hoverColor", RegisterType.Vector4);
			_builtInVariables.Add("backColor", RegisterType.Vector4);
			_builtInVariables.Add("borderColor", RegisterType.Vector4);
			_builtInVariables.Add("rect", RegisterType.Rectangle);
			_builtInVariables.Add("matColor", RegisterType.Vector4);
			_builtInVariables.Add("scale", RegisterType.Vector2);
			_builtInVariables.Add("translate", RegisterType.Vector2);
			_builtInVariables.Add("rotate", RegisterType.Float);
			_builtInVariables.Add("textScale", RegisterType.Float);
			_builtInVariables.Add("visible", RegisterType.Bool);
			_builtInVariables.Add("noEvents", RegisterType.Bool);
			_builtInVariables.Add("text", RegisterType.String);
			_builtInVariables.Add("background", RegisterType.String);
			_builtInVariables.Add("runScript", RegisterType.String);
			_builtInVariables.Add("varBackground", RegisterType.String);
			_builtInVariables.Add("cvar", RegisterType.String);
			_builtInVariables.Add("choices", RegisterType.String);
			_builtInVariables.Add("choiceVar", RegisterType.String);
			_builtInVariables.Add("bind", RegisterType.String);
			_builtInVariables.Add("modelRotate", RegisterType.Vector4);
			_builtInVariables.Add("modelOrigin", RegisterType.Vector4);
			_builtInVariables.Add("lightOrigin", RegisterType.Vector4);
			_builtInVariables.Add("lightColor", RegisterType.Vector4);
			_builtInVariables.Add("viewOffset", RegisterType.Vector4);
			_builtInVariables.Add("hideCursor", RegisterType.Bool);
		}

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
		public void Activate(bool activate, ref string act)
		{
			// make sure win vars are updated before activation
			UpdateVariables();

			// TODO: int n = (activate) ? ON_ACTIVATE : ON_DEACTIVATE;
			// TODO: RunScript(n);

			foreach(idWindow child in _children)
			{
				child.Activate(activate, ref act);
			}

			if(act.Length > 0)
			{
				act += " ; ";
			}
		}

		public void AddChild(idWindow child)
		{
			child._childID = _children.Count;
			_children.Add(child);
		}

		public void AddUpdateVariable(idWindowVariable var)
		{
			idConsole.WriteLine("AddUpdateVariable: {0}", var.Name);
			_updateVariables.Add(var);
		}

		public void Draw(int x, int y)
		{
			int skipShaders = idE.CvarSystem.GetInteger("r_skipGuiShaders");

			if((skipShaders == 1) || (_context == null))
			{
				return;
			}

			int time = _gui.Time;

			if(((_flags & WindowFlags.Desktop) != 0) && (skipShaders != 3))
			{
				// TODO: RunTimeEvents( time );
			}

			if(skipShaders == 2)
			{
				return;
			}

			// TODO: flags & WIN_SHOWTIME
			/*if ( flags & WIN_SHOWTIME ) {
				dc->DrawText(va(" %0.1f seconds\n%s", (float)(time - timeLine) / 1000, gui->State().GetString("name")), 0.35f, 0, dc->colorWhite, idRectangle(100, 0, 80, 80), false);
			}*/

			// TODO: flags & WIN_SHOWCOORDS
			/*if ( flags & WIN_SHOWCOORDS ) {
				dc->EnableClipping(false);
				sprintf(str, "x: %i y: %i  cursorx: %i cursory: %i", (int)rect.x(), (int)rect.y(), (int)gui->CursorX(), (int)gui->CursorY());
				dc->DrawText(str, 0.25f, 0, dc->colorWhite, idRectangle(0, 0, 100, 20), false);
				dc->EnableClipping(true);
			}*/

			if(_visible == false)
			{
				return;
			}
			
			CalculateClientRectangle(0, 0);
			// TODO: SetFont();

			// see if this window forces a new aspect ratio
			_context.SetSize(_forceAspectWidth, _forceAspectHeight);


			//FIXME: go to screen coord tracking
			_drawRect.Offset(x, y);
			_clientRect.Offset(x, y);
			_textRect.Offset(x, y);
			_actualX = _drawRect.X;
			_actualY = _drawRect.Y;

			Vector3 oldOrigin;
			Matrix oldTransform;

			_context.GetTransformInformation(out oldOrigin, out oldTransform);

			SetupTransforms(x, y);

			DrawBackground(_drawRect);

			// TODO: DrawBorderAndCaption(drawRect);

			if((_flags & WindowFlags.NoClip) == 0)
			{
				_context.PushClipRectangle(_clientRect);
			}

			if(skipShaders < 5)
			{
				// TODO: DrawText(time, x, y);
			}

			// TODO: debug draw
			/*if ( gui_debug.GetInteger() ) {
				DebugDraw(time, x, y);
			}*/

			foreach(DrawWindow drawWindow in _drawWindows)
			{
				if(drawWindow.Window != null)
				{
					drawWindow.Window.Draw(_clientRect.X + _offsetX, _clientRect.Y + _offsetY);
				}
				else
				{
					drawWindow.Simple.Draw(_clientRect.X + _offsetX, _clientRect.Y + _offsetY);
				}
			}

			// Put transforms back to what they were before the children were processed
			_context.SetTransformInformation(oldOrigin, oldTransform);

			if((_flags & WindowFlags.NoClip) == 0)
			{
				_context.PopClipRectangle();
			}

			// TODO
			/*if (gui_edit.GetBool()  || (flags & WIN_DESKTOP && !( flags & WIN_NOCURSOR )  && !hideCursor && (gui->Active() || ( flags & WIN_MENUGUI ) ))) {
				dc->SetTransformInfo(vec3_origin, mat3_identity);
				gui->DrawCursor();
			}

			if (gui_debug.GetInteger() && flags & WIN_DESKTOP) {
				dc->EnableClipping(false);
				sprintf(str, "x: %1.f y: %1.f",  gui->CursorX(), gui->CursorY());
				dc->DrawText(str, 0.25, 0, dc->colorWhite, idRectangle(0, 0, 100, 20), false);
				dc->DrawText(gui->GetSourceFile(), 0.25, 0, dc->colorWhite, idRectangle(0, 20, 300, 20), false);
				dc->EnableClipping(true);
			}*/

			_drawRect.Offset(-x, -y);
			_clientRect.Offset(-x, -y);
			_textRect.Offset(-x, -y);
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

		public void FixupParameters()
		{
			foreach(idWindow child in _children)
			{
				child.FixupParameters();
			}

			// TODO: scripts
			/*for (i = 0; i < SCRIPT_COUNT; i++) {
				if (scripts[i]) {
					scripts[i]->FixupParms(this);
				}
			}*/

			// TODO: events
			/*c = timeLineEvents.Num();
			for (i = 0; i < c; i++) {
				timeLineEvents[i]->event->FixupParms(this);
			}

			c = namedEvents.Num();
			for (i = 0; i < c; i++) {
				namedEvents[i]->mEvent->FixupParms(this);
			}*/

			int count = _ops.Count;
			
			for(int i = 0; i < count; i++)
			{
				if(_ops[i].B == -2)
				{
					// need to fix this up
					string p = (string) _ops[i].A;

					WindowExpressionOperation op = _ops[i];
					op.A = GetVariableByName(p, true);
					op.B = -1;

					_ops[i] = op;
				}
			}

			if((_flags & WindowFlags.Desktop) != 0)
			{
				CalculateRectangles(0, 0);
			}
		}

		public string HandleEvent(SystemEventArgs e)
		{
			if((_flags & WindowFlags.Desktop) != 0)
			{
				// TODO
				/*actionDownRun = false;
				actionUpRun = false;*/

				if((_expressionRegisters.Count > 0) && (_ops.Count > 0))
				{
					EvaluateRegisters();
				}

				// TODO: RunTimeEvents(gui->GetTime());

				CalculateRectangles(0, 0);

				// TODO: dc->SetCursor( idDeviceContext::CURSOR_ARROW );
			}

			// TODO
			/*if (visible && !noEvents) {

				if (event->evType == SE_KEY) {
					EvalRegs(-1, true);
					if (updateVisuals) {
						*updateVisuals = true;
					}

					if (event->evValue == K_MOUSE1) {

						if (!event->evValue2 && GetCaptureChild()) {
							GetCaptureChild()->LoseCapture();
							gui->GetDesktop()->captureChild = NULL;
							return "";
						} 

						int c = children.Num();
						while (--c >= 0) {
							if (children[c]->visible && children[c]->Contains(children[c]->drawRect, gui->CursorX(), gui->CursorY()) && !(children[c]->noEvents)) {
								idWindow *child = children[c];
								if (event->evValue2) {
									BringToTop(child);
									SetFocus(child);
									if (child->flags & WIN_HOLDCAPTURE) {
										SetCapture(child);
									}
								}
								if (child->Contains(child->clientRect, gui->CursorX(), gui->CursorY())) {
									//if ((gui_edit.GetBool() && (child->flags & WIN_SELECTED)) || (!gui_edit.GetBool() && (child->flags & WIN_MOVABLE))) {
									//	SetCapture(child);
									//}
									SetFocus(child);
									const char *childRet = child->HandleEvent(event, updateVisuals);
									if (childRet && *childRet) {
										return childRet;
									} 
									if (child->flags & WIN_MODAL) {
										return "";
									}
								} else {
									if (event->evValue2) {
										SetFocus(child);
										bool capture = true;
										if (capture && ((child->flags & WIN_MOVABLE) || gui_edit.GetBool())) {
											SetCapture(child);
										}
										return "";
									} else {
									}
								}
							}
						}
						if (event->evValue2 && !actionDownRun) {
							actionDownRun = RunScript( ON_ACTION );
						} else if (!actionUpRun) {
							actionUpRun = RunScript( ON_ACTIONRELEASE );
						}
					} else if (event->evValue == K_MOUSE2) {

						if (!event->evValue2 && GetCaptureChild()) {
							GetCaptureChild()->LoseCapture();
							gui->GetDesktop()->captureChild = NULL;
							return "";
						}

						int c = children.Num();
						while (--c >= 0) {
							if (children[c]->visible && children[c]->Contains(children[c]->drawRect, gui->CursorX(), gui->CursorY()) && !(children[c]->noEvents)) {
								idWindow *child = children[c];
								if (event->evValue2) {
									BringToTop(child);
									SetFocus(child);
								}
								if (child->Contains(child->clientRect,gui->CursorX(), gui->CursorY()) || GetCaptureChild() == child) {
									if ((gui_edit.GetBool() && (child->flags & WIN_SELECTED)) || (!gui_edit.GetBool() && (child->flags & WIN_MOVABLE))) {
										SetCapture(child);
									}
									const char *childRet = child->HandleEvent(event, updateVisuals);
									if (childRet && *childRet) {
										return childRet;
									} 
									if (child->flags & WIN_MODAL) {
										return "";
									}
								}
							}
						}
					} else if (event->evValue == K_MOUSE3) {
						if (gui_edit.GetBool()) {
							int c = children.Num();
							for (int i = 0; i < c; i++) {
								if (children[i]->drawRect.Contains(gui->CursorX(), gui->CursorY())) {
									if (event->evValue2) {
										children[i]->flags ^= WIN_SELECTED;
										if (children[i]->flags & WIN_SELECTED) {
											flags &= ~WIN_SELECTED;
											return "childsel";
										}
									}
								}
							}
						}
					} else if (event->evValue == K_TAB && event->evValue2) {
						if (GetFocusedChild()) {
							const char *childRet = GetFocusedChild()->HandleEvent(event, updateVisuals);
							if (childRet && *childRet) {
								return childRet;
							}

							// If the window didn't handle the tab, then move the focus to the next window
							// or the previous window if shift is held down

							int direction = 1;
							if ( idKeyInput::IsDown( K_SHIFT ) ) {
								direction = -1;
							}

							idWindow *currentFocus = GetFocusedChild();
							idWindow *child = GetFocusedChild();
							idWindow *parent = child->GetParent();
							while ( parent ) {
								bool foundFocus = false;
								bool recurse = false;
								int index = 0;
								if ( child ) {
									index = parent->GetChildIndex( child ) + direction;
								} else if ( direction < 0 ) {
									index = parent->GetChildCount() - 1;
								}
								while ( index < parent->GetChildCount() && index >= 0) {
									idWindow *testWindow = parent->GetChild( index );
									if ( testWindow == currentFocus ) {
										// we managed to wrap around and get back to our starting window
										foundFocus = true;
										break;
									}
									if ( testWindow && !testWindow->noEvents && testWindow->visible ) {
										if ( testWindow->flags & WIN_CANFOCUS ) {
											SetFocus( testWindow );
											foundFocus = true;
											break;
										} else if ( testWindow->GetChildCount() > 0 ) {
											parent = testWindow;
											child = NULL;
											recurse = true;
											break;
										}
									}
									index += direction;
								}
								if ( foundFocus ) {
									// We found a child to focus on
									break;
								} else if ( recurse ) {
									// We found a child with children
									continue;
								} else {
									// We didn't find anything, so go back up to our parent
									child = parent;
									parent = child->GetParent();
									if ( parent == gui->GetDesktop() ) {
										// We got back to the desktop, so wrap around but don't actually go to the desktop
										parent = NULL;
										child = NULL;
									}
								}
							}
						}
					} else if (event->evValue == K_ESCAPE && event->evValue2) {
						if (GetFocusedChild()) {
							const char *childRet = GetFocusedChild()->HandleEvent(event, updateVisuals);
							if (childRet && *childRet) {
								return childRet;
							}
						}
						RunScript( ON_ESC );
					} else if (event->evValue == K_ENTER ) {
						if (GetFocusedChild()) {
							const char *childRet = GetFocusedChild()->HandleEvent(event, updateVisuals);
							if (childRet && *childRet) {
								return childRet;
							}
						}
						if ( flags & WIN_WANTENTER ) {
							if ( event->evValue2 ) {
								RunScript( ON_ACTION );
							} else {
								RunScript( ON_ACTIONRELEASE );
							}
						}
					} else {
						if (GetFocusedChild()) {
							const char *childRet = GetFocusedChild()->HandleEvent(event, updateVisuals);
							if (childRet && *childRet) {
								return childRet;
							}
						}
					}

				} else if (event->evType == SE_MOUSE) {
					if (updateVisuals) {
						*updateVisuals = true;
					}
					const char *mouseRet = RouteMouseCoords(event->evValue, event->evValue2);
					if (mouseRet && *mouseRet) {
						return mouseRet;
					}
				} else if (event->evType == SE_NONE) {
				} else if (event->evType == SE_CHAR) {
					if (GetFocusedChild()) {
						const char *childRet = GetFocusedChild()->HandleEvent(event, updateVisuals);
						if (childRet && *childRet) {
							return childRet;
						}
					}
				}
			}

			gui->GetReturnCmd() = cmd;
			if ( gui->GetPendingCmd().Length() ) {
				gui->GetReturnCmd() += " ; ";
				gui->GetReturnCmd() += gui->GetPendingCmd();
				gui->GetPendingCmd().Clear();
			}
			cmd = "";
			return gui->GetReturnCmd();*/

			return string.Empty;
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

						// TODO: use this again once I get winvar bugs sorted out
						/*if(window.IsSimple == true)
						{
							drawWindow.Simple = new idSimpleWindow(window);
							_drawWindows.Add(drawWindow);
						}
						else*/
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
				/*else if (ParseScriptEntry(token, src)) 
				{
					
				}*/
				else if(ParseInternalVariable(token.ToString(), parser) == true)
				{

				}
				else
				{
					ParseRegisterEntry(token.ToString(), parser);
				}

				if((token = parser.ReadToken()) == null)
				{
					parser.Error("Unexpected end of file");
					ret = false;

					break;
				}
			}

			if(ret == true)
			{
				EvaluateRegisters(-1, true);
			}
			
			SetupFromState();
			PostParse();

			return ret;
		}

		/// <summary>
		/// Returns a register index.
		/// </summary>
		/// <param name="parser"></param>
		/// <returns></returns>
		public int ParseExpression(idScriptParser parser)
		{
			return ParseExpressionPriority(parser, 4 /* TOP_PRIORITY */, null);
		}

		/// <summary>
		/// Returns a register index.
		/// </summary>
		/// <param name="parser"></param>
		/// <param name="var"></param>
		/// <returns></returns>
		public int ParseExpression(idScriptParser parser, idWindowVariable var)
		{
			return ParseExpressionPriority(parser, 4 /* TOP_PRIORITY */, var);
		}

		/// <summary>
		/// Returns a register index.
		/// </summary>
		/// <param name="parser"></param>
		/// <param name="var"></param>
		/// <param name="component"></param>
		/// <returns></returns>
		public int ParseExpression(idScriptParser parser, idWindowVariable var, int component)
		{
			return ParseExpressionPriority(parser, 4 /* TOP_PRIORITY */, var);
		}

		public void SetupFromState()
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

		private void CalculateRectangles(int x, int y)
		{
			CalculateClientRectangle(x, y);

			_drawRect.Offset(x, y);
			_clientRect.Offset(x, y);
			_actualX = _drawRect.X;
			_actualY = _drawRect.Y;

			foreach(DrawWindow drawWindow in _drawWindows)
			{
				if(drawWindow.Window != null)
				{
					drawWindow.Window.CalculateRectangles(_clientRect.X + _offsetX, _clientRect.Y + _offsetY);
				}
			}

			_drawRect.Offset(-x, -y);
			_clientRect.Offset(-x, -y);
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
			_definedVariables.Clear();

			// TODO
			/*
			timeLineEvents.DeleteContents(true);
			for (i = 0; i < SCRIPT_COUNT; i++) {
				delete scripts[i];
			}*/

			Init();
		}

		private void DrawBackground(Rectangle drawRect)
		{
			if(_backColor.W != 0)
			{
				_context.DrawFilledRectangle(drawRect.X, drawRect.Y, drawRect.Width, drawRect.Height, _backColor);
			}

			if((_background != null) && (_materialColor.W != 0))
			{
				idConsole.WriteLine("TODO: DrawMaterial");

				/*if ( background && matColor.w() ) {
					float scalex, scaley;
					if ( flags & WIN_NATURALMAT ) {
						scalex = drawRect.w / background->GetImageWidth();
						scaley = drawRect.h / background->GetImageHeight();
					} else {
						scalex = matScalex;
						scaley = matScaley;
					}
					dc->DrawMaterial(drawRect.x, drawRect.y, drawRect.w, drawRect.h, background, matColor, scalex, scaley);
				}*/
			}
		}

		private void DrawText(int time, int x, int y)
		{
			if(_text == string.Empty)
			{
				return;
			}

			if(_textShadow > 0)
			{
				string shadowText = idHelper.RemoveColors(_text);
				Rectangle shadowRect = _textRect;
				shadowRect.X += _textShadow;
				shadowRect.Y += _textShadow;

				// TODO: _context.DrawText(shadowText, _textScale, _textAlign, _colorBlack, shadowRect, (_flags & WindowFlags.NoWrap) == 0, -1);
			}

			_context.DrawText(_text, _textScale, _textAlign, _foreColor, _textRect, (_flags & WindowFlags.NoWrap) == 0, -1);

			// TODO: gui_edit
			/*if ( gui_edit.GetBool() ) {
				dc->EnableClipping( false );
				dc->DrawText( va( "x: %i  y: %i", ( int )rect.x(), ( int )rect.y() ), 0.25, 0, dc->colorWhite, idRectangle( rect.x(), rect.y() - 15, 100, 20 ), false );
				dc->DrawText( va( "w: %i  h: %i", ( int )rect.w(), ( int )rect.h() ), 0.25, 0, dc->colorWhite, idRectangle( rect.x() + rect.w(), rect.w() + rect.h() + 5, 100, 20 ), false );
				dc->EnableClipping( true );
			}*/
		}

		private int EmitOperation(int a, int b, WindowExpressionOperationType opType)
		{
			WindowExpressionOperation op;
			return EmitOperation(a, b, opType, out op);
		}

		private int EmitOperation(int a, int b, WindowExpressionOperationType opType, out WindowExpressionOperation op)
		{
			int i = _expressionRegisters.Count;
			_registerIsTemporary[i] = true;
			_expressionRegisters.Add(0);

			i = _expressionRegisters.Count;

			op = new WindowExpressionOperation();
			op.Type = opType;
			op.A = a;
			op.B = b;
			op.C = i;

			_ops.Add(op);

			return op.C;
		}

		private float EvaluateRegisters()
		{
			return EvaluateRegisters(-1, false);
		}

		private float EvaluateRegisters(int test, bool force)
		{
			if((force == false) && (test >= 0) && (test < idE.MaxExpressionRegisters) && (_lastEval == this))
			{
				return _regs[test];
			}

			_lastEval = this;

			if(_expressionRegisters.Count > 0)
			{
				_regList.SetToRegisters(ref _regs);
				EvaluateRegisters(ref _regs);
				_regList.GetFromRegisters(_regs);
			}

			if((test >= 0) && (test < idE.MaxExpressionRegisters))
			{
				return _regs[test];
			}

			return 0.0f;
		}

		/// <summary>
		/// Parameters are taken from the localSpace and the renderView,
		/// then all expressions are evaluated, leaving the shader registers
		/// set to their apropriate values.
		/// </summary>
		/// <param name="registers"></param>
		private void EvaluateRegisters(ref float[] registers)
		{
			int expressionRegisterCount = _expressionRegisters.Count;
			int opCount = _ops.Count;

			// copy the constants
			for(int i = (int) WindowExpressionRegister.PredefinedCount; i < expressionRegisterCount; i++)
			{
				registers[i] = _expressionRegisters[i];
			}

			// copy the local and global parameters
			registers[(int) WindowExpressionRegister.Time] = _gui.Time;

			foreach(WindowExpressionOperation op in _ops)
			{
				if(op.B == -2)
				{
					continue;
				}

				switch(op.Type)
				{
					case WindowExpressionOperationType.Add:
						registers[op.C] = registers[(int) op.A] + registers[op.B];
						break;

					case WindowExpressionOperationType.Subtract:
						registers[op.C] = registers[(int) op.A] - registers[op.B];
						break;

					case WindowExpressionOperationType.Multiply:
						registers[op.C] = registers[(int) op.A] * registers[op.B];
						break;

					case WindowExpressionOperationType.Divide:
						if(registers[op.B] == 0.0f)
						{
							idConsole.Warning("Divide by zero in window '{0}' in {1}", this.Name, _gui.SourceFile);
							registers[op.C] = registers[(int) op.A];
						}
						else
						{
							registers[op.C] = registers[(int) op.A] / registers[op.B];
						}
						break;

					case WindowExpressionOperationType.Modulo:
						int b = (int) registers[op.B];
						b = (b != 0) ? b : 1;

						registers[op.C] = (int) registers[(int) op.A] % b;
						break;

					case WindowExpressionOperationType.Table:
						idDeclTable table = (idDeclTable) idE.DeclManager.DeclByIndex(DeclType.Table, (int) op.A);
						registers[op.C] = table.Lookup(registers[op.B]);
						break;

					case WindowExpressionOperationType.GreaterThan:
						registers[op.C] = (registers[(int) op.A] > registers[op.B]) ? 1 : 0;
						break;

					case WindowExpressionOperationType.GreaterThanOrEqual:
						registers[op.C] = (registers[(int) op.A] >= registers[op.B]) ? 1 : 0;
						break;

					case WindowExpressionOperationType.LessThan:
						registers[op.C] = (registers[(int) op.A] < registers[op.B]) ? 1 : 0;
						break;

					case WindowExpressionOperationType.LessThanOrEqual:
						registers[op.C] = (registers[(int) op.A] <= registers[op.B]) ? 1 : 0;
						break;

					case WindowExpressionOperationType.Equal:
						registers[op.C] = (registers[(int) op.A] == registers[op.B]) ? 1 : 0;
						break;

					case WindowExpressionOperationType.NotEqual:
						registers[op.C] = (registers[(int) op.A] != registers[op.B]) ? 1 : 0;
						break;

					case WindowExpressionOperationType.Conditional:
						registers[op.C] = (registers[(int) op.A] > 0) ? registers[op.B] : registers[op.D];
						break;

					case WindowExpressionOperationType.And:
						registers[op.C] = ((registers[(int) op.A] > 0) && (registers[op.B] > 0)) ? 1 : 0;
						break;

					case WindowExpressionOperationType.Or:
						registers[op.C] = ((registers[(int) op.A] > 0) || (registers[op.B] > 0)) ? 1 : 0;
						break;

					case WindowExpressionOperationType.Var:
						idConsole.WriteLine("TODO: WindowExpressionOperationType.Var");
						/*if(op.A == null)
						{
							registers[op.C] = 0.0f;
							break;
						}
						else if((op.B >= 0) && (registers[op.B] >= 0) && (registers[op.B] < 4))
						{
							// grabs vector components
							idWinVector4 var = (idWinVector4) op.A;
							registers[op.C] = 
							idWinVec4 *var = (idWinVec4 *)( op->a );
							registers[op->c] = ((idVec4&)var)[registers[op->b]];
						} else {
							registers[op->c] = ((idWinVar*)(op->a))->x();
						}*/
						break;
					case WindowExpressionOperationType.VarS:
						idConsole.WriteLine("TODO: WindowExpressionOperationType.VarS");

						/*if (op->a) {
							idWinStr *var = (idWinStr*)(op->a);
							registers[op->c] = atof(var->c_str());
						} else {
							registers[op->c] = 0;
						}*/
						break;

					case WindowExpressionOperationType.VarF:
						idConsole.WriteLine("TODO: WindowExpressionOperationType.VarF");

						/**if (op->a) {
							idWinFloat *var = (idWinFloat*)(op->a);
							registers[op->c] = *var;
						} else {
							registers[op->c] = 0;
						}*/
						break;

					case WindowExpressionOperationType.VarI:
						/*if (op->a) {
							idWinInt *var = (idWinInt*)(op->a);
							registers[op->c] = *var;
						} else {
							registers[op->c] = 0;
						}*/
						break;

					case WindowExpressionOperationType.VarB:
						/*if (op->a) {
							idWinBool *var = (idWinBool*)(op->a);
							registers[op->c] = *var;
						} else {
							registers[op->c] = 0;
						}*/
						break;
				}
			}
		}

		private int ExpressionConstant(float f)
		{
			int i;

			for(i = (int) WindowExpressionRegister.PredefinedCount; i < _expressionRegisters.Count; i++)
			{
				if((_registerIsTemporary[i] == false) && (_expressionRegisters[i] == f))
				{
					return i;
				}
			}

			int c = _expressionRegisters.Count;

			if(i > c)
			{
				while(i > c)
				{
					_expressionRegisters.Add(-9999999);
					i--;
				}
			}

			i = _expressionRegisters.Count;

			_expressionRegisters.Add(f);
			_registerIsTemporary[i] = false;

			return i;
		}

		private idWindowVariable GetVariableByName(string name)
		{
			DrawWindow owner = new DrawWindow();
			return GetVariableByName(name, false, ref owner);
		}

		private idWindowVariable GetVariableByName(string name, bool fixup)
		{
			DrawWindow owner = new DrawWindow();
			return GetVariableByName(name, fixup, ref owner);
		}

		private idWindowVariable GetVariableByName(string name, bool fixup, ref DrawWindow owner)
		{
			idWindowVariable ret = null;

			if(owner != null)
			{
				owner = null;
			}

			string nameLower = name.ToLower();

			if(nameLower == "notime")
			{
				ret = _noTime;
			}
			else if(nameLower == "background")
			{
				ret = _backgroundName;
			}
			else if(nameLower == "visible")
			{
				ret = _visible;
			}
			else if(nameLower == "rect")
			{
				ret = _rect;
			}
			else if(nameLower == "backcolor")
			{
				ret = _backColor;
			}
			else if(nameLower == "matcolor")
			{
				ret = _materialColor;
			}
			else if(nameLower == "forecolor")
			{
				ret = _foreColor;
			}
			else if(nameLower == "hovercolor")
			{
				ret = _hoverColor;
			}
			else if(nameLower == "bordercolor")
			{
				ret = _borderColor;
			}
			else if(nameLower == "textscale")
			{
				ret = _textScale;
			}
			else if(nameLower == "rotate")
			{
				ret = _rotate;
			}
			else if(nameLower == "noevents")
			{
				ret = _noEvents;
			}
			else if(nameLower == "text")
			{
				ret = _text;
			}
			else if(nameLower == "backgroundname")
			{
				ret = _backgroundName;
			}
			else if(nameLower == "hidecursor")
			{
				ret = _hideCursor;
			}

			string key = name;
			bool guiVar = key.StartsWith(idWindowVariable.Prefix);

			foreach(idWindowVariable var in _definedVariables)
			{
				if(nameLower.Equals(var.Name) == true)
				{
					ret = var;
					break;
				}
			}

			if(ret != null)
			{
				if((fixup == true) && (name != "$"))
				{
					idConsole.WriteLine("TODO: idWindow.DisableRegister");
					// TODO: DisableRegister(name);
				}

				if(_parent != null)
				{
					owner = _parent.FindChildByName(_name);
				}

				return ret;
			}

			int keyLength = key.Length;

			if((keyLength > 5) && (guiVar == true))
			{
				idWindowVariable var = new idWinString(name);
				var.Init(name, this);

				_definedVariables.Add(var);

				return var;
			}
			else if(fixup == true)
			{
				int n = key.IndexOf("::");

				if(n > 0)
				{
					string winName = key.Substring(0, n);
					string var = key.Substring(n + 2);

					DrawWindow win = this.UserInterface.Desktop.FindChildByName(winName);

					if(win != null)
					{
						if(win.Window != null)
						{
							return win.Window.GetVariableByName(var, false, ref owner);
						}
						else
						{
							owner = win;
						}

						return win.Simple.GetVariableByName(var);
					}
				}
			}

			return null;
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
			timeLine = -1;*/
			_textShadow = 0;

			/*hover = false;

			for(int i = 0; i < SCRIPT_COUNT; i++)
			{
				scripts[i] = NULL;
			}

			hideCursor = false;*/
		}

		private int ParseEmitOperation(idScriptParser parser, int a, WindowExpressionOperationType opType, int priority)
		{
			WindowExpressionOperation op;
			return ParseEmitOperation(parser, a, opType, priority, out op);
		}

		private int ParseEmitOperation(idScriptParser parser, int a, WindowExpressionOperationType opType, int priority, out WindowExpressionOperation op)
		{
			int b = ParseExpressionPriority(parser, priority);
			return EmitOperation(a, b, opType, out op);
		}

		private int ParseExpressionPriority(idScriptParser parser, int priority)
		{
			return ParseExpressionPriority(parser, priority, null, 0);
		}

		private int ParseExpressionPriority(idScriptParser parser, int priority, idWindowVariable var)
		{
			return ParseExpressionPriority(parser, priority, var, 0);
		}

		private int ParseExpressionPriority(idScriptParser parser, int priority, idWindowVariable var, int component)
		{
			if(priority == 0)
			{
				return ParseTerm(parser, var, component);
			}

			idToken token;
			string tokenValue;
			int a = ParseExpressionPriority(parser, priority - 1, var, component);

			if((token = parser.ReadToken()) == null)
			{
				// we won't get EOF in a real file, but we can
				// when parsing from generated strings
				return a;
			}

			tokenValue = token.ToString();

			if(priority == 1)
			{
				if(tokenValue == "*")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.Multiply, priority);
				}
				else if(tokenValue == "/")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.Divide, priority);
				}
				else if(tokenValue == "%")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.Modulo, priority);
				}
			}
			else if(priority == 2)
			{
				if(tokenValue == "+")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.Add, priority);
				}
				else if(tokenValue == "-")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.Subtract, priority);
				}
			}
			else if(priority == 3)
			{
				if(tokenValue == ">")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.GreaterThan, priority);
				}
				else if(tokenValue == ">=")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.GreaterThanOrEqual, priority);
				}
				else if(tokenValue == "<")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.LessThan, priority);
				}
				else if(tokenValue == "<=")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.LessThanOrEqual, priority);
				}
				else if(tokenValue == "==")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.Equal, priority);
				}
				else if(tokenValue == "!=")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.NotEqual, priority);
				}
			}
			else if(priority == 4)
			{
				if(tokenValue == "&&")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.And, priority);
				}
				else if(tokenValue == "||")
				{
					return ParseEmitOperation(parser, a, WindowExpressionOperationType.Or, priority);
				}
				else if(tokenValue == "?")
				{
					WindowExpressionOperation op;
					int o = ParseEmitOperation(parser, a, WindowExpressionOperationType.Conditional, priority, out op);

					if((token = parser.ReadToken()) == null)
					{
						return o;
					}
					else if(token.ToString() == ":")
					{
						a = ParseExpressionPriority(parser, priority - 1, var);
						op.D = a;
					}

					return priority;
				}
			}

			// assume that anything else terminates the expression
			// not too robust error checking...
			parser.UnreadToken(token);

			return a;
		}

		private bool ParseInternalVariable(string name, idScriptParser parser)
		{
			name = name.ToLower();

			if(name == "bordersize")
			{
				_borderSize = (int) parser.ParseFloat();
			}
			else if(name == "comment")
			{
				_comment = ParseString(parser);
			}
			else if(name == "font")
			{
				string font = ParseString(parser);
				idConsole.WriteLine("TODO: fontNum = dc->FindFont( fontStr );");
			}
			else if(name == "forceaspectwidth")
			{
				_forceAspectWidth = parser.ParseFloat();
			}
			else if(name == "forceaspectheight")
			{
				_forceAspectHeight = parser.ParseFloat();
			}
			else if(name == "invertrect")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.InvertRectangle;
				}
			}
			else if(name == "naturalmatscale")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.NaturalMaterial;
				}
			}
			else if(name == "noclip")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.NoClip;
				}
			}
			else if(name == "nocursor")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.NoCursor;
				}
			}
			else if(name == "nowrap")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.NoWrap;
				}
			}
			else if(name == "matscalex")
			{
				_materialScaleX = parser.ParseFloat();
			}
			else if(name == "matscaley")
			{
				_materialScaleY = parser.ParseFloat();
			}
			else if(name == "menugui")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.MenuInterface;
				}
			}
			else if(name == "modal")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.Modal;
				}
			}
			else if(name == "name")
			{
				_name = ParseString(parser);
			}
			else if(name == "play")
			{
				idConsole.Warning("play encountered during gui parse.. see robert");
				string tmp = ParseString(parser);
			}
			else if(name == "shadow")
			{
				_textShadow = parser.ParseInteger();
			}
			else if(name == "shear")
			{
				_shear.X = parser.ParseFloat();

				idToken token = parser.ReadToken();

				if(token.ToString() == ",")
				{
					parser.Error("Expected comma in shear definition");

					return false;
				}

				_shear.Y = parser.ParseFloat();
			}
			else if(name == "showcoords")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.ShowCoordinates;
				}
			}
			else if(name == "showtime")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.ShowTime;
				}
			}
			else if(name == "textalign")
			{
				_textAlign = (TextAlign) parser.ParseInteger();
			}
			else if(name == "textalignx")
			{
				_textAlignX = (int) parser.ParseFloat();
			}
			else if(name == "textaligny")
			{
				_textAlignY = (int) parser.ParseFloat();
			}
			else if(name == "wantenter")
			{
				if(parser.ParseBool() == true)
				{
					_flags |= WindowFlags.WantEnter;
				}
			}
			else
			{
				return false;
			}

			return true;
		}

		private bool ParseRegisterEntry(string name, idScriptParser parser)
		{
			string work = name.ToLower();
			idWindowVariable var = GetVariableByName(work, false);

			if(var != null)
			{
				// check builtins first
				if(_builtInVariables.ContainsKey(work) == true)
				{
					_regList.AddRegister(work, _builtInVariables[work], parser, this, var);

					return true;
				}
			}

			// not predefined so just read the next token and add it to the state
			idToken token;

			if((token = parser.ReadToken()) != null)
			{
				if(var != null)
				{
					var.Set(token.ToString());
					return true;
				}

				switch(token.Type)
				{
					case TokenType.Number:
						if((token.SubType & TokenSubType.Integer) != 0)
						{
							var = new idWinInteger(work);
							var.Set(token.ToString());
						}
						else if((token.SubType & TokenSubType.Float) != 0)
						{
							var = new idWinFloat(work);
							var.Set(token.ToString());
						}
						else
						{
							var = new idWinString(work);
							var.Set(token.ToString());
						}

						_definedVariables.Add(var);
						break;

					default:
						var = new idWinString(work);
						var.Set(token.ToString());

						_definedVariables.Add(var);
						break;
				}
			}

			return true;
		}

		private string ParseString(idScriptParser parser)
		{
			idToken token = parser.ReadToken();

			if(token != null)
			{
				return token.ToString();
			}

			return string.Empty;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parser"></param>
		/// <param name="var"></param>
		/// <param name="component"></param>
		/// <returns>Returns a register index.</returns>
		private int ParseTerm(idScriptParser parser, idWindowVariable var, int component)
		{
			int a, b;

			idToken token = parser.ReadToken();
			string tokenValue = token.ToString().ToLower();

			if(tokenValue == "(")
			{
				a = ParseExpression(parser);
				parser.ExpectTokenString(")");

				return a;
			}
			else if(tokenValue == "time")
			{
				return (int) WindowExpressionRegister.Time;
			}
			// parse negative numbers
			else if(tokenValue == "-")
			{
				token = parser.ReadToken();

				if((token.Type == TokenType.Number) || (token.ToString() == "."))
				{
					return ExpressionConstant(-token.ToFloat());
				}

				parser.Warning("Bad negative number '{0}'", token.ToString());

				return 0;
			}

			if((token.Type == TokenType.Number) || (token.ToString() == ".") || (token.ToString() == "-"))
			{
				return ExpressionConstant(token.ToFloat());
			}

			// see if it is a table name
			idDeclTable table = idE.DeclManager.FindType<idDeclTable>(DeclType.Table, token.ToString(), false);

			if(table != null)
			{
				a = table.Index;

				// parse a table expression
				parser.ExpectTokenString("[");
				b = ParseExpression(parser);
				parser.ExpectTokenString("]");

				return EmitOperation(a, b, WindowExpressionOperationType.Table);
			}

			if(var == null)
			{
				var = GetVariableByName(token.ToString(), true);
			}

			if(var != null)
			{
				idConsole.WriteLine("TODO: ParseTerm var");
				/*a = (int)var;
				//assert(dynamic_cast<idWinVec4*>(var));
				var->Init(token, this);
				b = component;
				if (dynamic_cast<idWinVec4*>(var)) {
					if (src->ReadToken(&token)) {
						if (token == "[") {
							b = ParseExpression(src);
							src->ExpectTokenString("]");
						} else {
							src->UnreadToken(&token);
						}
					}
					return EmitOp(a, b, WOP_TYPE_VAR);
				} else if (dynamic_cast<idWinFloat*>(var)) {
					return EmitOp(a, b, WOP_TYPE_VARF);
				} else if (dynamic_cast<idWinInt*>(var)) {
					return EmitOp(a, b, WOP_TYPE_VARI);
				} else if (dynamic_cast<idWinBool*>(var)) {
					return EmitOp(a, b, WOP_TYPE_VARB);
				} else if (dynamic_cast<idWinStr*>(var)) {
					return EmitOp(a, b, WOP_TYPE_VARS);
				} else {
					src->Warning("Var expression not vec4, float or int '%s'", token.c_str());
				}*/
				return 0;
			}
			else
			{
				// ugly but used for post parsing to fixup named vars
				idConsole.WriteLine("TODO: ParseTerm str");

				/*char *p = new char[token.Length()+1];
				strcpy(p, token);
				a = (int)p;
				b = -2;
				return EmitOp(a, b, WOP_TYPE_VAR);*/
			}

			return 0;
		}

		private void PostParse()
		{

		}

		private void RestoreExpressionParseState()
		{
			_registerIsTemporary = _saveTemporaries;
			_saveTemporaries = null;
		}

		private void SaveExpressionParseState()
		{
			_saveTemporaries = _registerIsTemporary;
		}

		private void SetupBackground()
		{
			if(_backgroundName != string.Empty)
			{
				_background = idE.DeclManager.FindMaterial(_backgroundName);
				_background.ImageClassification = 1; // just for resource tracking

				if((_background != null) && (_background.TestMaterialFlag(MaterialFlags.Defaulted) == false))
				{
					_background.Sort = (float) MaterialSort.Gui;
				}
			}

			_backgroundName.Material = _background;
		}

		private void SetupTransforms(float x, float y)
		{
			Matrix transform = Matrix.Identity;
			Vector3 origin = new Vector3(_origin.X + x, _origin.Y + y, 0);

			// TODO: rotate
			/*if ( rotate ) {
				static idRotation rot;
				static idVec3 vec(0, 0, 1);
				rot.Set( org, vec, rotate );
				trans = rot.ToMat3();
			}*/

			// TODO: shear
			/*if ( shear.x || shear.y ) {
				static idMat3 smat;
				smat.Identity();
				smat[0][1] = shear.x;
				smat[1][0] = shear.y;
				trans *= smat;
			}*/

			if(transform != Matrix.Identity)
			{
				_context.SetTransformInformation(origin, transform);
			}
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

		private void UpdateVariables()
		{
			foreach(idWindowVariable var in _updateVariables)
			{
				var.Update();
			}
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

	public enum WindowExpressionOperationType
	{
		Add,
		Subtract,
		Multiply,
		Divide,
		Modulo,
		Table,
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
		Equal,
		NotEqual,
		And,
		Or,
		Var,
		VarS,
		VarF,
		VarI,
		VarB,
		Conditional
	}

	public enum WindowExpressionRegister
	{
		Time,
		PredefinedCount
	}

	public struct WindowExpressionOperation
	{
		public WindowExpressionOperationType Type;
		public object A;
		public int B;
		public int C;
		public int D;
	}
}