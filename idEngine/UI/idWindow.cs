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
using System.Text;

using Microsoft.Xna.Framework;

using idTech4.Input;
using idTech4.Math;
using idTech4.Renderer;
using idTech4.Text;
using idTech4.Text.Decl;

namespace idTech4.UI
{
	public class idWindow : IDisposable
	{
		#region Constants
		private static readonly string[] ScriptNames = new string[] {
			"onMouseEnter",
			"onMouseExit",
			"onAction",
			"onActivate",
			"onDeactivate",
			"onESC",
			"onEvent",
			"onTrigger",
			"onActionRelease",
			"onEnter",
			"onEnterRelease"
		};
		#endregion

		#region Properties
		public Vector4 BackColor
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _backColor;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_backColor.Set(value);
			}
		}


		public idMaterial Background
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _background;
			}
		}

		public string BackgroundName
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _backgroundName;
			}
		}

		public Vector4 BorderColor
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _borderColor;
			}
		}

		public float BorderSize
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _borderSize;
			}
		}

		public virtual idWindow Buddy
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return null;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
			}
		}

		public idWindow CaptureChild
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				if((this.Flags & WindowFlags.Desktop) == WindowFlags.Desktop)
				{
					return this.UserInterface.Desktop._captureChild;
				}

				return null;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				if(value == null)
				{
					_captureChild = value;
				}
				else
				{
					// only one child can have the focus
					idWindow last = null;
					int c = _children.Count;

					for(int i = 0; i < c; i++)
					{
						if((_children[i].Flags & WindowFlags.Capture) == WindowFlags.Capture)
						{
							last = _children[i];
							last.HandleCaptureLost();

							break;
						}
					}

					value.Flags |= WindowFlags.Capture;
					value.HandleCaptureGained();

					this.UserInterface.Desktop._captureChild = value;
				}
			}
		}

		public int ChildCount
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _drawWindows.Count;
			}
		}

		public idRectangle ClientRectangle
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _clientRect;
			}
		}

		public string Command
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _command;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_command = value;
			}
		}

		public Cursor Cursor
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _cursor;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_cursor = value;
			}
		}

		public idDeviceContext DeviceContext
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _context;
			}
		}

		public idRectangle DrawRectangle
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _drawRect;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_drawRect = value;
			}
		}

		public WindowFlags Flags
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _flags;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_flags = value;
			}
		}

		public idWindow FocusedChild
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _focusedChild;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_focusedChild = value;
			}
		}

		public idFontFamily FontFamily
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _fontFamily;
			}
		}

		public Vector4 ForeColor
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _foreColor;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_foreColor.Set(value);
			}
		}

		public bool HasOperations
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return (_ops.Count > 0);
			}
		}

		public bool HideCursor
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _hideCursor;
			}
		}

		public bool Hover
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _hover;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_hover = value;
			}
		}

		public bool IsInteractive
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				if(_scripts[(int) ScriptName.Action] != null)
				{
					return true;
				}

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
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

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

				for(int i = 0; i < _scripts.Length; i++)
				{
					if(_scripts[i] != null)
					{
						return false;
					}
				}

				if(_timeLineEvents.Count > 0)
				{
					return false;
				}
				else if(_namedEvents.Count > 0)
				{
					return false;
				}

				return true;
			}
		}

		public bool IsVisible
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _visible;
			}
		}

		public Vector4 MaterialColor
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _materialColor;
			}
		}

		public float MaterialScaleX
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _materialScaleX;
			}
		}

		public float MaterialScaleY
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _materialScaleY;
			}
		}

		/// <summary>
		/// Gets the name of the window.
		/// </summary>
		public string Name
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _name;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_name = value;
			}
		}

		public bool NoEvents
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _noEvents;
			}
		}

		public Vector2 Origin
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

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
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _parent;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_parent = value;
			}
		}

		public idRectangle Rectangle
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _rect.Data;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_rect.Set(value);
			}
		}


		public float Rotate
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _rotate;
			}
		}

		public Vector2 Shear
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _shear;
			}
		}

		public string Text
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _text;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_text.Set(value);
			}
		}

		public TextAlign TextAlign
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _textAlign;
			}
		}

		public float TextAlignX
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _textAlignX;
			}
		}

		public float TextAlignY
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _textAlignY;
			}
		}

		public float TextScale
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _textScale;
			}
		}

		public int TextShadow
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _textShadow;
			}
		}

		public idRectangle TextRectangle
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _textRect;
			}
		}

		public idUserInterface UserInterface
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

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

		private Cursor _cursor;

		private string _command = string.Empty;
		private int _timeLine; // time stamp used for various fx

		private float _forceAspectWidth;
		private float _forceAspectHeight;
		private float _materialScaleX;
		private float _materialScaleY;
		private float _borderSize;

		private bool _hover;
		private bool _actionDownRun;
		private bool _actionUpRun;

		private TextAlign _textAlign;
		private float _textAlignX;
		private float _textAlignY;
		private int _textShadow;

		private float _actualX;						// physical coords
		private float _actualY;						// ''
		private int _childID;						// this childs id
		private int _lastTimeRun;					//
		private idRectangle _drawRect;				// overall rect
		private idRectangle _clientRect;				// client area
		private idRectangle _textRect;
		private Vector2 _origin;
		private Vector2 _shear;

		private idFontFamily _fontFamily;

		private idMaterial _background;

		private idWindow _focusedChild;				// if a child window has the focus
		private idWindow _captureChild;				// if a child window has mouse capture
		private idWindow _overChild;				// if a child window has mouse capture

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

		private idGuiScriptList[] _scripts = new idGuiScriptList[(int) ScriptName.Count];
		private List<idTimeLineEvent> _timeLineEvents = new List<idTimeLineEvent>();
		private List<TransitionData> _transitions = new List<TransitionData>();
		private List<idNamedEvent> _namedEvents = new List<idNamedEvent>();

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

		~idWindow()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		public virtual void Activate(bool activate, ref string act)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// make sure win vars are updated before activation
			UpdateVariables();
			RunScript((activate == true) ? ScriptName.Activate : ScriptName.Deactivate);

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
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			child._childID = _children.Count;
			_children.Add(child);
		}

		public void AddCommand(string command)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(_command.Length > 0)
			{
				_command += string.Format(" ; {0}", command);
			}
			else
			{
				_command = command;
			}
		}

		public void AddDefinedVariable(idWindowVariable var)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(_definedVariables.Contains(var) == false)
			{
				_definedVariables.Add(var);
			}
		}

		public void AddTransition(idWindowVariable dest, Vector4 from, Vector4 to, int time, float accelTime, float decelTime)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			TransitionData data = new TransitionData(dest);
			data.Interp.Init(this.UserInterface.Time, accelTime * time, decelTime * time, time, from, to);

			_transitions.Add(data);
		}

		public void AddUpdateVariable(idWindowVariable var)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_updateVariables.Add(var);
		}

		public void BringToTop(idWindow window)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if((window != null) && ((window.Flags & WindowFlags.Modal) == 0))
			{
				return;
			}

			int c = _children.Count;

			for(int i = 0; i < c; i++)
			{
				if(_children[i] == window)
				{
					// this is it move from i - 1 to 0 to i to 1 then shove this one into 0
					for(int j = i + 1; j < c; j++)
					{
						_children[j - 1] = _children[j];
					}

					_children[c - 1] = window;
					break;
				}
			}
		}

		public void ClientToScreen(ref idRectangle rect)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			int x, y;
			idWindow p;

			for(p = this, x = 0, y = 0; p != null; p = p.Parent)
			{
				x += (int) p.Rectangle.X;
				y += (int) p.Rectangle.Y;
			}

			rect.X += x;
			rect.Y += y;
		}

		public bool Contains(idRectangle rect, float x, float y)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			rect.X += _actualX - _drawRect.X;
			rect.Y += _actualY - _drawRect.Y;

			return rect.Contains(x, y);
		}

		public virtual void Draw(float x, float y)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			int skipShaders = idE.CvarSystem.GetInteger("r_skipGuiShaders");

			if((skipShaders == 1) || (this.DeviceContext == null))
			{
				return;
			}

			int time = this.UserInterface.Time;

			if(((this.Flags & WindowFlags.Desktop) == WindowFlags.Desktop) && (skipShaders != 3))
			{
				RunTimeEvents(time);
			}

			if(skipShaders == 2)
			{
				return;
			}

			if((this.Flags & WindowFlags.ShowTime) == WindowFlags.ShowTime)
			{
				this.DeviceContext.DrawText(string.Format("{0:0} seconds\n{1}", (float) (_timeLine - _timeLine) / 1000, this.UserInterface.State.GetString("name")), 0.35f, 0, idColor.White, new idRectangle(100, 0, 80, 80), false);
			}

			if((this.Flags & WindowFlags.ShowCoordinates) == WindowFlags.ShowCoordinates)
			{
				this.DeviceContext.ClippingEnabled = false;
				this.DeviceContext.DrawText(string.Format("x: {0} y: {1} cursorx: {2} cursory: {3}", (int) _rect.X, (int) _rect.Y, (int) this.UserInterface.CursorX, (int) this.UserInterface.CursorY), 0.25f, 0, idColor.White, new idRectangle(0, 0, 100, 20), false);
				this.DeviceContext.ClippingEnabled = true;
			}

			if(_visible == false)
			{
				return;
			}
			
			CalculateClientRectangle(0, 0);
			SetFont();

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
			
			this.DeviceContext.GetTransformInformation(out oldOrigin, out oldTransform);

			SetupTransforms(x, y);
			DrawBackground(_drawRect);
			DrawBorderAndCaption(_drawRect);

			if((_flags & WindowFlags.NoClip) == 0)
			{
				this.DeviceContext.PushClipRectangle(_clientRect);
			}

			if(skipShaders < 5)
			{
				DrawText(time, x, y);
			}

			if(idE.CvarSystem.GetInteger("gui_debug") > 0)
			{
				DrawDebug(time, x, y);
			}

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
			this.DeviceContext.SetTransformInformation(oldOrigin, oldTransform);

			if((this.Flags & WindowFlags.NoClip) == 0)
			{
				this.DeviceContext.PopClipRectangle();
			}

			if((idE.CvarSystem.GetBool("gui_edit") == true)
				|| (((this.Flags & WindowFlags.Desktop) == WindowFlags.Desktop) 
						&& ((this.Flags & WindowFlags.NoCursor) == 0)
						&& (this.HideCursor == false)
						&& ((this.UserInterface.IsActive == true) || ((this.Flags & WindowFlags.MenuInterface) == WindowFlags.MenuInterface))))
			{
				this.DeviceContext.SetTransformInformation(Vector3.Zero, Matrix.Identity);
				this.UserInterface.DrawCursor();
			}

			if((idE.CvarSystem.GetBool("gui_debug") == true) && ((this.Flags & WindowFlags.Desktop) == WindowFlags.Desktop))
			{
				this.DeviceContext.ClippingEnabled = false;
				this.DeviceContext.DrawText(string.Format("x: {0:00} y: {1:00}", this.UserInterface.CursorX, this.UserInterface.CursorY), 0.25f, 0, idColor.White, new idRectangle(0, 0, 100, 20), false);
				this.DeviceContext.DrawText(this.UserInterface.SourceFile, 0.25f, 0, idColor.White, new idRectangle(0, 20, 300, 20), false);
				this.DeviceContext.ClippingEnabled = true;
			}
			
			_drawRect.Offset(-x, -y);
			_clientRect.Offset(-x, -y);
			_textRect.Offset(-x, -y);
		}

		/// <summary>
		/// Returns the child window at the given index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public idWindow GetChild(int index)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return _drawWindows[index].Window;
		}

		public int GetChildIndex(idWindow window)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			for(int find = 0; find < _drawWindows.Count; find++)
			{
				if(_drawWindows[find].Window == window)
				{
					return find;
				}
			}

			return -1;
		}

		public idWindowVariable GetVariableByName(string name)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			DrawWindow owner = new DrawWindow();
			return GetVariableByName(name, false, ref owner);
		}

		public idWindowVariable GetVariableByName(string name, bool fixup)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			DrawWindow owner = new DrawWindow();
			return GetVariableByName(name, fixup, ref owner);
		}

		public virtual idWindowVariable GetVariableByName(string name, bool fixup, ref DrawWindow owner)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

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
				if(nameLower.Equals(var.Name, StringComparison.OrdinalIgnoreCase) == true)
				{
					ret = var;
					break;
				}
			}

			if(ret != null)
			{
				if((fixup == true) && (name.StartsWith("$") == false))
				{
					DisableRegister(name);
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

		public float EvaluateRegisters()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return EvaluateRegisters(-1, false);
		}

		public float EvaluateRegisters(int test)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return EvaluateRegisters(test, false);
		}

		public float EvaluateRegisters(int test, bool force)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

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
		public void EvaluateRegisters(ref float[] registers)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

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
						if(op.A == null)
						{
							registers[op.C] = 0.0f;
							break;
						}
						else if((op.B >= 0) && (registers[op.B] >= 0) && (registers[op.B] < 4))
						{
							throw new Exception("WTF?");
							// grabs vector components
							/*idWinVector4 var = (idWinVector4) op.A;
							registers[op.C] = var.
							idWinVec4 *var = (idWinVec4 *)( op->a );
							registers[op->c] = ((idVec4&)var)[registers[op->b]];*/
						}
						else
						{
							registers[op.C] = ((idWindowVariable) op.A).X;
						}
						break;
					case WindowExpressionOperationType.VarS:
						if(op.A != null)
						{
							float.TryParse(((idWinString) op.A), out registers[op.C]);
						}
						else
						{
							registers[op.C] = 0;
						}
						break;

					case WindowExpressionOperationType.VarF:
						if(op.A != null)
						{
							registers[op.C] = ((idWinFloat) op.A).X;
						}
						else
						{
							registers[op.C] = 0;
						}
						break;

					case WindowExpressionOperationType.VarI:
						if(op.A != null)
						{
							registers[op.C] = ((idWinInteger) op.A).X;
						}
						else
						{
							registers[op.C] = 0;
						}
						break;

					case WindowExpressionOperationType.VarB:
						if(op.A != null)
						{
							registers[op.C] = ((idWinBool) op.A).X;
						}
						else
						{
							registers[op.C] = 0;
						}
						break;
				}
			}
		}

		public DrawWindow FindChildByName(string name)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

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
					if(win.Simple.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
					{
						return win;
					}
				}
			}

			return null;
		}

		public void FixupParameters()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			foreach(idWindow child in _children)
			{
				child.FixupParameters();
			}

			for(int i = 0; i < _scripts.Length; i++)
			{
				if(_scripts[i] != null)
				{
					_scripts[i].FixupParameters(this);
				}
			}

			foreach(idTimeLineEvent ev in _timeLineEvents)
			{
				ev.Event.FixupParameters(this);
			}

			foreach(idNamedEvent ev in _namedEvents)
			{
				ev.Event.FixupParameters(this);
			}

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

			if((_flags & WindowFlags.Desktop) == WindowFlags.Desktop)
			{
				CalculateRectangles(0, 0);
			}
		}

		public virtual void HandleBuddyUpdate(idWindow buddy)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		public virtual string HandleEvent(SystemEvent e, ref bool updateVisuals)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if((_flags & WindowFlags.Desktop) == WindowFlags.Desktop)
			{
				_actionDownRun = false;
				_actionUpRun = false;

				if((_expressionRegisters.Count > 0) && (_ops.Count > 0))
				{
					EvaluateRegisters();
				}

				RunTimeEvents(_gui.Time);
				CalculateRectangles(0, 0);

				this.DeviceContext.Cursor = Cursor.Arrow;
			}
						
			if((_visible == true) && (_noEvents == false))
			{
				if(e.Type == SystemEventType.Key)
				{
					string keyReturn = HandleKeyEvent(e, (Keys) e.Value, (e.Value2 == 1), ref updateVisuals);
					
					if(keyReturn != string.Empty)
					{
						return keyReturn;
					}
				} 
				else if(e.Type == SystemEventType.Mouse)
				{
					string mouseReturn = HandleMouseEvent(e.Value, e.Value2, ref updateVisuals);

					if(mouseReturn != string.Empty)
					{
						return mouseReturn;
					}
				}
				/*} else if (event->evType == SE_NONE) {
				} else if (event->evType == SE_CHAR) {
					if (GetFocusedChild()) {
						const char *childRet = GetFocusedChild()->HandleEvent(event, updateVisuals);
						if (childRet && *childRet) {
							return childRet;
						}
					}
				}*/
			}
		
			/*gui->GetReturnCmd() = cmd;
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
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(rebuild == true)
			{
				CleanUp();
			}

			_timeLineEvents.Clear();
			_namedEvents.Clear();
			_transitions.Clear();

			idToken token2;
			idToken token = parser.ExpectTokenType(TokenType.Name, 0);
			DrawWindow drawWindow;

			SetInitialState(token.ToString());

			parser.ExpectTokenString("{");
			token = parser.ExpectAnyToken();

			bool ret = true;

			while(token.ToString() != "}")
			{
				string tokenLower = token.ToString().ToLower();

				// track what was parsed so we can maintain it for the guieditor
				parser.SetMarker();

				if((tokenLower == "windowdef") || (tokenLower == "animationdef"))
				{
					if(tokenLower == "animationdef")
					{
						_visible.Set(false);
						_rect.Set(new idRectangle(0, 0, 0, 0));
					}

					token = parser.ExpectTokenType(TokenType.Name, 0);
					token2 = token;

					parser.UnreadToken(token);

					drawWindow = FindChildByName(token2.ToString());

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
							SetFocus(window, false);

							drawWindow.Window = window;
							_drawWindows.Add(drawWindow);
						}
					}
				}
				else if(tokenLower == "editdef")
				{
					SaveExpressionParseState();

					idEditWindow window = new idEditWindow(_context, _gui);
					window.Parse(parser, rebuild);

					RestoreExpressionParseState();

					AddChild(window);

					window.Parent = this;

					drawWindow = new DrawWindow();
					drawWindow.Simple = null;
					drawWindow.Window = window;

					_drawWindows.Add(drawWindow);
				}
				else if(tokenLower == "choicedef")
				{
					SaveExpressionParseState();

					idChoiceWindow window = new idChoiceWindow(_context, _gui);
					window.Parse(parser, rebuild);

					RestoreExpressionParseState();

					AddChild(window);

					window.Parent = this;

					drawWindow = new DrawWindow();
					drawWindow.Simple = null;
					drawWindow.Window = window;

					_drawWindows.Add(drawWindow);
				}
				else if(tokenLower == "sliderdef")
				{
					SaveExpressionParseState();

					idSliderWindow window = new idSliderWindow(_context, _gui);
					window.Parse(parser, rebuild);

					RestoreExpressionParseState();

					AddChild(window);

					window.Parent = this;

					drawWindow = new DrawWindow();
					drawWindow.Simple = null;
					drawWindow.Window = window;

					_drawWindows.Add(drawWindow);
				}
				else if(tokenLower == "markerdef")
				{
					idConsole.Warning("TODO: markerDef");
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
				else if(tokenLower == "binddef")
				{
					SaveExpressionParseState();

					idBindWindow window = new idBindWindow(_context, _gui);
					window.Parse(parser, rebuild);

					RestoreExpressionParseState();

					AddChild(window);

					window.Parent = this;

					drawWindow = new DrawWindow();
					drawWindow.Simple = null;
					drawWindow.Window = window;

					_drawWindows.Add(drawWindow);
				}
				else if(tokenLower == "listdef")
				{
					SaveExpressionParseState();

					idListWindow window = new idListWindow(_context, _gui);
					window.Parse(parser, rebuild);

					RestoreExpressionParseState();

					AddChild(window);

					window.Parent = this;

					drawWindow = new DrawWindow();
					drawWindow.Simple = null;
					drawWindow.Window = window;

					_drawWindows.Add(drawWindow);
				}
				else if(tokenLower == "fielddef")
				{
					idConsole.Warning("TODO: fieldDef");
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
				else if(tokenLower == "renderdef")
				{					
					SaveExpressionParseState();

					idRenderWindow window = new idRenderWindow(_context, _gui);
					window.Parse(parser, rebuild);

					RestoreExpressionParseState();

					AddChild(window);

					window.Parent = this;

					drawWindow = new DrawWindow();
					drawWindow.Simple = null;
					drawWindow.Window = window;

					_drawWindows.Add(drawWindow);
				}
				else if(tokenLower == "gamessddef")
				{
					idConsole.Warning("TODO: gameSSDDef");
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
				else if(tokenLower == "gamebearshootdef")
				{
					idConsole.Warning("TODO: gameBearShootDef");
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
				else if(tokenLower == "gamebustoutdef")
				{
					idConsole.Warning("TODO: gameBustOutDef");
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
				else if(tokenLower == "onnamedevent")
				{
					// read the event name
					if((token = parser.ReadToken()) == null)
					{
						parser.Error("Expected event name");
						return false;
					}

					idNamedEvent ev = new idNamedEvent(token.ToString());
					parser.SetMarker();

					if(ParseScript(parser, ev.Event) == false)
					{
						ret = false;
						break;
					}

					_namedEvents.Add(ev);
				}
				else if(tokenLower == "ontime")
				{
					idTimeLineEvent ev = new idTimeLineEvent();

					if((token = parser.ReadToken()) == null)
					{
						parser.Error("Unexpected end of file");
						return false;
					}

					int tmp;
					int.TryParse(token.ToString(), out tmp);

					ev.Time = tmp;

					// reset the mark since we dont want it to include the time
					parser.SetMarker();

					if(ParseScript(parser, ev.Event) == false)
					{
						ret = false;
						break;
					}

					// this is a timeline event
					ev.Pending = true;
					_timeLineEvents.Add(ev);
				}
				else if(tokenLower == "definefloat")
				{
					token = parser.ReadToken();
					tokenLower = token.ToString().ToLower();

					idWinFloat var = new idWinFloat(tokenLower);

					_definedVariables.Add(var);

					// add the float to the editors wrapper dict
					// Set the marker after the float name
					parser.SetMarker();

					// Read in the float 
					_regList.AddRegister(tokenLower, RegisterType.Float, parser, this, var);
				}
				else if(tokenLower == "definevec4")
				{
					token = parser.ReadToken();
					tokenLower = token.ToString().ToLower();

					idWinVector4 var = new idWinVector4(tokenLower);

					// set the marker so we can determine what was parsed
					// set the marker after the vec4 name
					parser.SetMarker();

					// FIXME: how about we add the var to the desktop instead of this window so it won't get deleted
					//        when this window is destoyed which even happens during parsing with simple windows ?
					//definedVars.Append(var);
					_gui.Desktop._definedVariables.Add(var);
					_gui.Desktop._regList.AddRegister(tokenLower, RegisterType.Vector4, parser, _gui.Desktop, var);
				}
				else if(tokenLower == "float")
				{
					token = parser.ReadToken();
					tokenLower = token.ToString();

					idWinFloat var = new idWinFloat(tokenLower);
					_definedVariables.Add(var);

					// add the float to the editors wrapper dict
					// set the marker to after the float name
					parser.SetMarker();

					// Parse the float
					_regList.AddRegister(tokenLower, RegisterType.Float, parser, this, var);
				}
				else if(ParseScriptEntry(token, parser) == true)
				{

				}
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
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

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
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

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
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return ParseExpressionPriority(parser, 4 /* TOP_PRIORITY */, var);
		}

		public void ResetTime(int time)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_timeLine = _gui.Time - time;

			foreach(idTimeLineEvent e in _timeLineEvents)
			{
				if(e.Time >= time)
				{
					e.Pending = true;
				}
			}

			_noTime.Set(false);

			int c = _transitions.Count;

			for(int i = 0; i < c; i++)
			{
				if((_transitions[i].Variable != null) && (_transitions[i].Interp.IsDone(this.UserInterface.Time) == true))
				{
					_transitions.RemoveAt(i);

					i--;
					c--;
				}
			}
		}

		public virtual void RunNamedEvent(string name)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			foreach(idNamedEvent e in _namedEvents)
			{
				if(e.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == false)
				{
					continue;
				}

				UpdateVariables();

				// make sure we got all the current values for stuff
				if((_expressionRegisters.Count > 0) && (_ops.Count > 0))
				{
					EvaluateRegisters(-1, true);
				}

				RunScriptList(e.Event);

				break;
			}

			// run the event in all the children as well
			foreach(idWindow window in _children)
			{
				window.RunNamedEvent(name);
			}
		}

		public bool RunScript(ScriptName name)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			return RunScriptList(_scripts[(int) name]);
		}

		public bool RunScriptList(idGuiScriptList list)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(list == null)
			{
				return false;
			}

			list.Execute(this);

			return true;
		}

		public void ScreenToClient(ref idRectangle rect)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			int x, y;
			idWindow p;

			for(p = this, x = 0, y = 0; p != null; p = p.Parent)
			{
				x += (int) p.Rectangle.X;
				y += (int) p.Rectangle.Y;
			}

			rect.X -= x;
			rect.Y -= y;
		}

		public void SetupFromState()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			SetupBackground();

			if(_borderSize > 0)
			{
				_flags |= WindowFlags.Border;
			}

			if((_regList.FindRegister("rotate") != null) || (_regList.FindRegister("shear") != null))
			{
				this.Flags |= WindowFlags.Transform;
			}

			CalculateClientRectangle(0, 0);

			if(_scripts[(int) ScriptName.Action] != null)
			{
				this.Cursor = UI.Cursor.Hand;
				this.Flags |= WindowFlags.CanFocus;
			}
		}
		
		public idWindow SetFocus(idWindow window, bool scripts = true)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			// only one child can have the focus
			idWindow lastFocus = null;

			if((window.Flags & WindowFlags.CanFocus) == WindowFlags.CanFocus)
			{
				lastFocus = this.UserInterface.Desktop.FocusedChild;

				if(lastFocus != null)
				{
					lastFocus.HandleFocusLost();
				}

				//  call on lose focus
				if((scripts == true) && (lastFocus != null))
				{
					// calling this broke all sorts of guis
					// lastFocus->RunScript(ON_MOUSEEXIT);
				}


				//  call on gain focus
				if((scripts == true) && (window != null))
				{
					// calling this broke all sorts of guis
					// w->RunScript(ON_MOUSEENTER);
				}

				window.HandleFocusGained();

				this.UserInterface.Desktop.FocusedChild = window;
			}

			return lastFocus;
		}

		public void StartTransition()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			this.Flags |= WindowFlags.InTransition;
		}

		public virtual void StateChanged(bool redraw)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			UpdateVariables();

			if((_expressionRegisters.Count > 0) && (_ops.Count > 0))
			{
				EvaluateRegisters();
			}

			foreach(DrawWindow drawWindow in _drawWindows)
			{
				if(drawWindow.Window != null)
				{
					drawWindow.Window.StateChanged(redraw);
				}
				else
				{
					drawWindow.Simple.StateChanged(redraw);
				}
			}

			if(redraw == true)
			{
				if((this.Flags & WindowFlags.Desktop) == WindowFlags.Desktop)
				{
					Draw(0, 0);
				}

				// TODO: cinematic
				/*if ( background && background->CinematicLength() ) {
					background->UpdateCinematic( gui->GetTime() );
				}*/
			}
		}

		public void Trigger()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			RunScript(ScriptName.Trigger);

			for(int i = 0; i < _children.Count; i++)
			{
				_children[i].Trigger();
			}

			StateChanged(true);
		}
		#endregion

		#region Protected
		protected virtual void DrawBackground(idRectangle drawRect)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(_backColor.W != 0)
			{
				_context.DrawFilledRectangle(drawRect.X, drawRect.Y, drawRect.Width, drawRect.Height, _backColor);
			}

			if((_background != null) && (_materialColor.W != 0))
			{
				float scaleX, scaleY;

				if((_flags & WindowFlags.NaturalMaterial) == WindowFlags.NaturalMaterial)
				{
					scaleX = _drawRect.Width / _background.ImageWidth;
					scaleY = _drawRect.Height / _background.ImageHeight;
				}
				else
				{
					scaleX = _materialScaleX;
					scaleY = _materialScaleY;
				}

				_context.DrawMaterial(_drawRect.X, _drawRect.Y, _drawRect.Width, _drawRect.Height, _background, _materialColor, scaleX, scaleY);
			}
		}

		protected virtual void OnFocusGained()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		protected virtual void OnFocusLost()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		protected virtual bool ParseInternalVariable(string name, idScriptParser parser)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			name = name.ToLower();

			if(name == "bordersize")
			{
				_borderSize = parser.ParseFloat();
			}
			else if(name == "comment")
			{
				_comment = ParseString(parser);
			}
			else if(name == "font")
			{
				string font = ParseString(parser);
				_fontFamily = _context.FindFont(font);
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

				if(token.ToString() != ",")
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
				_textAlignX = parser.ParseFloat();
			}
			else if(name == "textaligny")
			{
				_textAlignY = parser.ParseFloat();
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

		protected string ParseString(idScriptParser parser)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idToken token = parser.ReadToken();

			if(token != null)
			{
				return token.ToString();
			}

			return string.Empty;
		}

		protected virtual void PostParse()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		protected virtual string RouteMouseCoordinates(float x, float y)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(this.CaptureChild != null)
			{
				//FIXME: unkludge this whole mechanism
				return this.CaptureChild.RouteMouseCoordinates(x, y);
			}

			if((x == -2000) || (y == -2000))
			{
				return string.Empty;
			}
	
			int c = _children.Count;
			string str = string.Empty;

			while(c > 0)
			{
				idWindow child = _children[--c];

				if((child.IsVisible == true)
					&& (child.NoEvents == false)
					&& (child.Contains(child.DrawRectangle, this.UserInterface.CursorX, this.UserInterface.CursorY) == true))
				{
					this.DeviceContext.Cursor = child.Cursor;

					if(_overChild != child)
					{
						if(_overChild != null)
						{
							_overChild.HandleMouseExit();

							str = _overChild.Command;

							if(str != string.Empty)
							{
								this.UserInterface.Desktop.AddCommand(str);
								_overChild.Command = string.Empty;
							}
						}

						_overChild = child;
						_overChild.HandleMouseEnter();

						str = _overChild.Command;

						if(str != string.Empty)
						{
							this.UserInterface.Desktop.AddCommand(str);
							_overChild.Command = string.Empty;
						}
					} 
					else 
					{
						if((child.Flags & WindowFlags.HoldCapture) == 0)
						{
							child.RouteMouseCoordinates(x, y);
						}
					}

					return string.Empty;
				}
			}

			if(_overChild != null)
			{
				_overChild.HandleMouseExit();

				str = _overChild.Command;

				if(str != string.Empty)
				{
					this.UserInterface.Desktop.AddCommand(str);
					_overChild.Command = string.Empty;
				}

				_overChild = null;
			}

			return string.Empty;
		}

		protected virtual bool RunTimeEvents(int time)
		{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

			if((time - _lastTimeRun) < idE.UserCommandMillseconds)
			{
				return false;
			}

			_lastTimeRun = time;

			UpdateVariables();

			if((_expressionRegisters.Count > 0) && (_ops.Count > 0))
			{
				EvaluateRegisters();
			}

			if((_flags & WindowFlags.InTransition) == WindowFlags.InTransition)
			{
				Transition();
			}

			Time();

			// renamed ON_EVENT to ON_FRAME
			RunScript(ScriptName.Frame);

			foreach(idWindow child in _children)
			{
				child.RunTimeEvents(time);
			}

			return true;
		}
		#endregion

		#region Private
		private void CalculateClientRectangle(float offsetX, float offsetY)
		{
			_drawRect = _rect.Data;

			if((_flags & WindowFlags.InvertRectangle) == WindowFlags.InvertRectangle)
			{
				_drawRect.X = _rect.X - _rect.Width;
				_drawRect.Y = _rect.Y - _rect.Height;
			}

			if(((_flags & (WindowFlags.HorizontalCenter | WindowFlags.VerticalCenter)) != 0) && (_parent != null))
			{
				// in this case treat xofs and yofs as absolute top left coords
				// and ignore the original positioning
				if((_flags & WindowFlags.HorizontalCenter) == WindowFlags.HorizontalCenter)
				{
					_drawRect.X = (_parent.Rectangle.Width - _rect.Width) / 2;
				}
				else
				{
					_drawRect.Y = (_parent.Rectangle.Height - _rect.Height) / 2;
				}
			}

			_drawRect.X += offsetX;
			_drawRect.Y += offsetY;

			_clientRect = _drawRect;

			if((_rect.Height > 0.0f) && (_rect.Width > 0.0f))
			{
				if(((_flags & WindowFlags.Border) == WindowFlags.Border) && (_borderSize != 0.0f))
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

		private void CalculateRectangles(float x, float y)
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
			for(int i = 0; i < _drawWindows.Count; i++)
			{
				_drawWindows[i].Simple = null;
			}

			// ensure the register list gets cleaned up
			_regList.Reset();

			// cleanup the named events
			foreach(idWindow win in _children)
			{
				win.Dispose();
			}

			_children.Clear();
			_namedEvents.Clear();
			_drawWindows.Clear();
			_definedVariables.Clear();
			_timeLineEvents.Clear();

			for(int i = 0; i < _scripts.Length; i++)
			{
				_scripts[i] = null;
			}

			Init();
		}

		private void DisableRegister(string name)
		{
			idRegister reg = _regList.FindRegister(name);

			if(reg != null)
			{
				reg.Enabled = false;
			}
		}
		
		private void DrawBorderAndCaption(idRectangle drawRect)
		{
			if(((_flags & WindowFlags.Border) == WindowFlags.Border) && (_borderSize > 0) && (_borderColor.W > 0))
			{
				_context.DrawRectangle(drawRect.X, drawRect.Y, drawRect.Width, drawRect.Height, _borderSize, _borderColor);
			}
		}

		private void DrawDebug(int time, float x, float y)
		{
			if(_context == null)
			{
				return;
			}

			_context.ClippingEnabled = false;

			if(idE.CvarSystem.GetInteger("gui_debug") == 1)
			{
				_context.DrawRectangle(_drawRect.X, _drawRect.Y, _drawRect.Width, _drawRect.Height, 1, idColor.Red);
			}
			else if(idE.CvarSystem.GetInteger("gui_debug") == 2)
			{
				string str = this.Text;
				StringBuilder buffer = new StringBuilder();

				if(str.Length > 0)
				{
					buffer.AppendLine(str);
				}

				buffer.AppendFormat("Rect: {0}, {1}, {2}, {3}\n", _rect.X, _rect.Y, _rect.Width, _rect.Height);
				buffer.AppendFormat("Draw Rect: {0}, {1}, {2}, {3}\n", _drawRect.X, _drawRect.Y, _drawRect.Width, _drawRect.Height);
				buffer.AppendFormat("Client Rect: {0}, {1}, {2}, {3}\n", _clientRect.X, _clientRect.Y, _clientRect.Width, _clientRect.Height);
				buffer.AppendFormat("Cursor: {0} : {1}\n", this.UserInterface.CursorX, this.UserInterface.CursorY);

				_context.DrawText(buffer.ToString(), _textScale, _textAlign, _foreColor, _textRect, true);
			}

			_context.ClippingEnabled = true;
		}

		private void DrawText(int time, float x, float y)
		{
			if(_text == string.Empty)
			{
				return;
			}

			if(_textShadow > 0)
			{
				string shadowText = idHelper.RemoveColors(_text);
				idRectangle shadowRect = _textRect;
				shadowRect.X += _textShadow;
				shadowRect.Y += _textShadow;

				_context.DrawText(shadowText, _textScale, _textAlign, idColor.Black, shadowRect, (_flags & WindowFlags.NoWrap) == 0, -1);
			}

			_context.DrawText(_text, _textScale, _textAlign, _foreColor, _textRect, (_flags & WindowFlags.NoWrap) == 0, -1);

			if(idE.CvarSystem.GetBool("gui_edit") == true)
			{
				this.DeviceContext.ClippingEnabled = false;
				this.DeviceContext.DrawText(string.Format("x: {0} y:{1}", (int) _rect.X, (int) _rect.Y), 0.25f, 0, idColor.White, new idRectangle(_rect.X, _rect.Y - 15, 100, 20), false);
				this.DeviceContext.DrawText(string.Format("w: {0} h:{1}", (int) _rect.Width, (int) _rect.Height), 0.25f, 0, idColor.White, new idRectangle(_rect.X + _rect.Width, _rect.Width + _rect.Height + 5, 100, 20), false);
				this.DeviceContext.ClippingEnabled = true;
			}
		}

		private int EmitOperation(object a, int b, WindowExpressionOperationType opType)
		{
			WindowExpressionOperation op;
			return EmitOperation(a, b, opType, out op);
		}

		private int EmitOperation(object a, int b, WindowExpressionOperationType opType, out WindowExpressionOperation op)
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

		private void HandleCaptureGained()
		{

		}

		private void HandleCaptureLost()
		{
			this.Flags &= ~WindowFlags.Capture;
		}

		private void HandleFocusGained()
		{
			this.Flags |= WindowFlags.Focus;
		}

		private void HandleFocusLost()
		{
			this.Flags &= ~WindowFlags.Focus;
		}

		private string HandleKeyEvent(SystemEvent e, Keys key, bool down, ref bool updateVisuals)
		{
			EvaluateRegisters(-1, true);

			updateVisuals = true;

			if(key == Keys.Mouse1)
			{
				if((down == false) && (this.CaptureChild != null))
				{
					this.CaptureChild.HandleCaptureLost();
					this.UserInterface.Desktop.CaptureChild = null;

					return string.Empty;
				}

				int c = _children.Count;

				while(--c >= 0)
				{
					idWindow child = _children[c];

					if((child.IsVisible == true) 
						&& (child.Contains(child.DrawRectangle, this.UserInterface.CursorX, this.UserInterface.CursorY) == true)
						&& (child.NoEvents == false))
					{
						if(down == true)
						{
							BringToTop(child);
							SetFocus(child);

							if((child.Flags & WindowFlags.HoldCapture) == WindowFlags.HoldCapture)
							{
								this.CaptureChild = child;
							}
						}

						if(child.Contains(child.ClientRectangle, this.UserInterface.CursorX, this.UserInterface.CursorY) == true)
						{
							//if ((gui_edit.GetBool() && (child->flags & WIN_SELECTED)) || (!gui_edit.GetBool() && (child->flags & WIN_MOVABLE))) {
							//	SetCapture(child);
							//}
								
							SetFocus(child);
							string childReturn = child.HandleEvent(e, ref updateVisuals);

							if(childReturn != string.Empty)
							{
								return childReturn;
							}

							if((child.Flags & WindowFlags.Modal) == WindowFlags.Modal)
							{
								return string.Empty;
							}
						}
						else
						{
							if(down == true)
							{
								SetFocus(child);

								bool capture = true;

								if((capture == true) && (((child.Flags & WindowFlags.Movable) == WindowFlags.Movable) || (idE.CvarSystem.GetBool("gui_edit") == true)))
								{
									this.CaptureChild = child;
								}

								return string.Empty;
							}
						}
					}
				}

				if((down == true) && (_actionDownRun == false))
				{
					_actionDownRun = RunScript(ScriptName.Action);
				}
				else if(_actionUpRun == false)
				{
					_actionUpRun = RunScript(ScriptName.ActionRelease);
				}
			} 
			else if (key == Keys.Mouse2)
			{
				if((down == false) && (this.CaptureChild != null))
				{
					this.CaptureChild.HandleCaptureLost();
					this.UserInterface.Desktop.CaptureChild = null;

					return string.Empty;
				}

				int c = _children.Count;

				while(--c >= 0)
				{
					idWindow child = _children[c];

					if((child.IsVisible == true) 
						&& (child.Contains(child.DrawRectangle, this.UserInterface.CursorX, this.UserInterface.CursorY) == true)
						&& (child.NoEvents == false))
					{
						if(down == true)
						{
							BringToTop(child);
							SetFocus(child);
						}

						if((child.Contains(child.ClientRectangle, this.UserInterface.CursorX, this.UserInterface.CursorY) == true)
							|| (this.CaptureChild == child))
						{
							if(((idE.CvarSystem.GetBool("gui_edit") == true) && ((child.Flags & WindowFlags.Selected) == WindowFlags.Selected))
								|| (idE.CvarSystem.GetBool("gui_edit") == false) && ((child.Flags & WindowFlags.Movable) == WindowFlags.Movable))
							{
								this.CaptureChild = child;
							}

							string childReturn = child.HandleEvent(e, ref updateVisuals);

							if(childReturn != string.Empty)
							{
								return childReturn;
							}

							if((child.Flags & WindowFlags.Modal) == WindowFlags.Modal)
							{
								return string.Empty;
							}
						}
					}
				}
			} 
			else if(key == Keys.Mouse3)
			{
				if(idE.CvarSystem.GetBool("gui_edit") == true)
				{
					int c = _children.Count;

					for(int i = 0; i < c; i++)
					{
						if(_children[i].DrawRectangle.Contains(this.UserInterface.CursorX, this.UserInterface.CursorY) == true)
						{
							if(down == true)
							{
								_children[i].Flags ^= WindowFlags.Selected;

								if((_children[i].Flags & WindowFlags.Selected) == WindowFlags.Selected)
								{
									this.Flags &= ~WindowFlags.Selected;
									return "childsel";
								}
							}
						}
					}
				}
			} 
			else if((key == Keys.Tab) && (down == true))
			{
				if(this.FocusedChild != null)
				{
					string childRet = this.FocusedChild.HandleEvent(e, ref updateVisuals);

					if(childRet != string.Empty)
					{
						return childRet;
					}

					// If the window didn't handle the tab, then move the focus to the next window
					// or the previous window if shift is held down
					int direction = 1;

					if(idE.Input.IsKeyDown(Keys.LeftShift) == true)
					{
						direction = -1;
					}

					idWindow currentFocus = this.FocusedChild;
					idWindow child = this.FocusedChild;
					idWindow parent = child.Parent;

					while(parent != null)
					{
						bool foundFocus = false;
						bool recurse = false;
						int index = 0;

						if(child != null)
						{
							index = parent.GetChildIndex(child) + direction;
						}
						else if(direction < 0)
						{
							index = parent.ChildCount - 1;
						}

						while((index < parent.ChildCount) && (index >= 0))
						{
							idWindow testWindow = parent.GetChild(index);

							if(testWindow == currentFocus)
							{
								// we managed to wrap around and get back to our starting window
								foundFocus = true;
								break;
							}
							else if((testWindow != null) && (testWindow.NoEvents == false) && (testWindow.IsVisible == true))
							{
								if((testWindow.Flags & WindowFlags.CanFocus) == WindowFlags.CanFocus)
								{
									SetFocus(testWindow);
									foundFocus = true;
									break;
								}
								else if(testWindow.ChildCount > 0)
								{
									parent = testWindow;
									child = null;
									recurse = true;
									break;
								}
							}

							index += direction;
						}

						if(foundFocus == true)
						{
							// we found a child to focus on
							break;
						} 
						else if(recurse == true) 
						{
							// we found a child with children
							continue;
						} 
						else 
						{
							// we didn't find anything, so go back up to our parent
							child = parent;
							parent = child.Parent;

							if(parent == this.UserInterface.Desktop)
							{
								// we got back to the desktop, so wrap around but don't actually go to the desktop
								parent = null;
								child = null;
							}
						}
					}
				}
			} 
			else if((key == Keys.Escape) && (down == true))
			{
				if(this.FocusedChild != null)
				{
					string childRet = this.FocusedChild.HandleEvent(e, ref updateVisuals);

					if(childRet != string.Empty)
					{
						return childRet;
					}
				}

				RunScript(ScriptName.Escape);
			}
			else if(key == Keys.Enter)
			{
				if(this.FocusedChild != null)
				{
					string childRet = this.FocusedChild.HandleEvent(e, ref updateVisuals);

					if(childRet != string.Empty)
					{
						return childRet;
					}
				}

				if((this.Flags & WindowFlags.WantEnter) == WindowFlags.WantEnter)
				{
					if(down == true)
					{
						RunScript(ScriptName.Action);
					}
					else
					{
						RunScript(ScriptName.ActionRelease);
					}
				}
			}
			else
			{
				if(this.FocusedChild != null)
				{
					string childRet = this.FocusedChild.HandleEvent(e, ref updateVisuals);

					if(childRet != string.Empty)
					{
						return childRet;
					}
				}
			}

			return string.Empty;
		}
		
		private void HandleMouseEnter()
		{
			this.Hover = true;

			if(_noEvents == false)
			{
				RunScript(ScriptName.MouseEnter);
			}
		}

		private string HandleMouseEvent(int deltaX, int deltaY, ref bool updateVisuals)
		{
			updateVisuals = true;

			return RouteMouseCoordinates(deltaX, deltaY);
		}

		private void HandleMouseExit()
		{
			this.Hover = false;

			if(_noEvents == false)
			{
				RunScript(ScriptName.MouseExit);
			}
		}

		private void Init()
		{
			_childID = 0;
			_flags = 0;
			_lastTimeRun = 0;

			_origin = Vector2.Zero;
						
			_fontFamily = null;
			_timeLine = -1;
			_offsetX = 0;
			_offsetY = 0;
			
			_cursor = Cursor.Arrow;
			_forceAspectWidth = 640;
			_forceAspectHeight = 480;
			_materialScaleX = 1;
			_materialScaleY = 1;
			_borderSize = 0;

			_textAlign = TextAlign.Left;
			_textAlignX = 0;
			_textAlignY = 0;
			
			_noEvents.Set(false);
			_noTime.Set(false);
			_visible.Set(true);
			_hideCursor.Set(false);

			_shear = Vector2.Zero;
			_rotate.Set(0);
			_textScale.Set(0.35f);

			_backColor.Set(Vector4.Zero);
			_foreColor.Set(new Vector4(1, 1, 1, 1));
			_hoverColor.Set(new Vector4(1, 1, 1, 1));
			_materialColor.Set(new Vector4(1, 1, 1, 1));
			_borderColor.Set(Vector4.Zero);

			_background = null;
			_backgroundName.Set(string.Empty);

			_focusedChild = null;
			_captureChild = null;
			_overChild = null;

			// TODO
			/*
			*/
			_parent = null;
			/*saveOps = NULL;
			saveRegs = NULL;*/
			_timeLine = -1;
			_textShadow = 0;
			_hover = false;
			
			for(int i = 0; i < _scripts.Length; i++)
			{
				_scripts[i] = null;
			}
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

		private int ParseExpressionPriority(idScriptParser parser, int priority, idWindowVariable var = null, int component = 0)
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
				// we won't get EOF in a real file, but we can when parsing from generated strings
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
		
		private bool ParseRegisterEntry(string name, idScriptParser parser)
		{
			string work = name.ToLower();
			idWindowVariable var = GetVariableByName(work, false);

			if(var != null)
			{
				RegisterType regType;

				// check builtins first
				if(_builtInVariables.TryGetValue(work, out regType) == true)
				{
					_regList.AddRegister(work, regType, parser, this, var);

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
						if((token.SubType & TokenSubType.Integer) == TokenSubType.Integer)
						{
							var = new idWinInteger(work);
							var.Set(token.ToString());
						}
						else if((token.SubType & TokenSubType.Float) == TokenSubType.Float)
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

		private bool ParseScript(idScriptParser parser, idGuiScriptList list)
		{
			return ParseScript(parser, list, false);
		}

		private bool ParseScript(idScriptParser parser, idGuiScriptList list, bool elseBlock)
		{
			bool ifElseBlock = false;

			idToken token;

			// scripts start with { ( unless parm is true ) and have ; separated command lists.. commands are command,
			// arg.. basically we want everything between the { } as it will be interpreted at
			// run time
			if(elseBlock == true)
			{
				token = parser.ReadToken();

				if(token.ToString().ToLower() == "if")
				{
					ifElseBlock = true;
				}

				parser.UnreadToken(token);

				if((ifElseBlock == false) && (parser.ExpectTokenString("{") == false))
				{
					return false;
				}
			}
			else if(parser.ExpectTokenString("{") == false)
			{
				return false;
			}

			int nest = 0;
			string tokenLower;

			while(true)
			{
				if((token = parser.ReadToken()) == null)
				{
					parser.Error("Unexpected end of file");
					return false;
				}

				tokenLower = token.ToString().ToLower();

				if(tokenLower == "{")
				{
					nest++;
				}
				else if(tokenLower == "}")
				{
					if(nest-- <= 0)
					{
						return true;
					}
				}

				idGuiScript script = new idGuiScript();

				if(tokenLower == "if")
				{
					script.ConditionRegister = ParseExpression(parser);

					ParseScript(parser, script.IfList);

					if((token = parser.ReadToken()) != null)
					{
						if(token.ToString() == "else")
						{
							// pass true to indicate we are parsing an else condition
							ParseScript(parser, script.ElseList, true);
						}
						else
						{
							parser.UnreadToken(token);
						}
					}

					list.Append(script);

					// if we are parsing an else if then return out so 
					// the initial "if" parser can handle the rest of the tokens
					if(ifElseBlock == true)
					{
						return true;
					}

					continue;
				}
				else
				{
					parser.UnreadToken(token);
				}

				// empty { } is not allowed
				if(token.ToString() == "{")
				{
					parser.Error("Unexpected {");
					return false;
				}

				script.Parse(parser);
				list.Append(script);
			}
		}

		private bool ParseScriptEntry(idToken token, idScriptParser parser)
		{
			for(int i = 0; i < (int) ScriptName.Count; i++)
			{
				if(token.ToString().ToLower() == ScriptNames[i].ToLower())
				{
					_scripts[i] = new idGuiScriptList();

					return ParseScript(parser, _scripts[i]);
				}
			}

			return false;
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
			object tmp;

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
				var.Init(tokenValue, this);

				tmp = var;
				b = component;

				if(var is idWinVector4)
				{
					if((token = parser.ReadToken()) != null)
					{
						if(token.ToString() == "[")
						{
							b = ParseExpression(parser);
							parser.ExpectTokenString("]");
						}
						else
						{
							parser.UnreadToken(token);
						}
					}

					return EmitOperation(tmp, b, WindowExpressionOperationType.Var);
				}
				else if(var is idWinFloat)
				{
					return EmitOperation(tmp, b, WindowExpressionOperationType.VarF);
				}
				else if(var is idWinInteger)
				{
					return EmitOperation(tmp, b, WindowExpressionOperationType.VarI);
				}
				else if(var is idWinBool)
				{
					return EmitOperation(tmp, b, WindowExpressionOperationType.VarB);
				}
				else if(var is idWinString)
				{
					return EmitOperation(tmp, b, WindowExpressionOperationType.VarS);
				}
				else
				{
					parser.Warning("Variable expression not vec4, float or int '{0}'", token.ToString());
				}

				return 0;
			}
			else
			{
				// ugly but used for post parsing to fixup named vars
				EmitOperation(token.ToString(), -2, WindowExpressionOperationType.Var);
			}

			return 0;
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

			if(_rotate.X > 0)
			{
				idRotation rot = new idRotation(origin, new Vector3(0, 0, 1), _rotate.X);

				transform = rot.ToMatrix();
			}

			if((_shear.X > 0) || (_shear.Y > 0))
			{
				Matrix mat = Matrix.Identity;
				mat.M12 = _shear.X;
				mat.M21 = _shear.Y;

				transform *= mat;
			}

			if(transform != Matrix.Identity)
			{
				_context.SetTransformInformation(origin, transform);
			}
		}

		private void SetFont()
		{
			_context.FontFamily = _fontFamily;
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

		private void Time()
		{
			if(_noTime == true)
			{
				return;
			}

			if(_timeLine == -1)
			{
				_timeLine = _gui.Time;
			}

			_command = string.Empty;

			foreach(idTimeLineEvent e in _timeLineEvents)
			{
				if((e.Pending == true) && ((_gui.Time - _timeLine) >= e.Time))
				{
					e.Pending = false;
					RunScriptList(e.Event);
				}
			}

			if(_gui.IsActive == true)
			{
				_gui.PendingCommand += _command;
			}
		}

		private void Transition()
		{
			bool clear = true;
			int count = _transitions.Count;

			for(int i = 0; i < count; i++)
			{
				TransitionData data = _transitions[i];
				idWinRectangle r = null;
				idWinVector4 v4 = data.Variable as idWinVector4;
				idWinFloat value = null;

				if(v4 == null)
				{
					r = data.Variable as idWinRectangle;

					if(r == null)
					{
						value = data.Variable as idWinFloat;
					}
				}

				if((data.Variable != null) && (data.Interp.IsDone(this.UserInterface.Time) == true))
				{
					if(v4 != null)
					{
						v4.Set(data.Interp.EndValue);
					}
					else if(value != null)
					{
						value.Set(data.Interp.EndValue.X);
					}
					else
					{
						r.Set(data.Interp.EndValue);
					}
				}
				else
				{
					clear = false;

					if(data.Variable != null)
					{
						if(v4 != null)
						{
							v4.Set(data.Interp.GetCurrentValue(this.UserInterface.Time));
						}
						else if(value != null)
						{
							value.Set(data.Interp.GetCurrentValue(this.UserInterface.Time).X);
						}
						else
						{
							r.Set(data.Interp.GetCurrentValue(this.UserInterface.Time));
						}
					}
					else
					{
						idConsole.Warning("Invalid transitional data for window {0} in gui {1}", this.Name, this.UserInterface.SourceFile);
					}
				}
			}

			if(clear == true)
			{
				_transitions.Clear();
				_flags &= ~WindowFlags.InTransition;
			}
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

		#region IDisposable
		#region Properties
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}
		#endregion

		#region Members
		private bool _disposed;
		#endregion

		#region Methods
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(disposing == true)
			{
				CleanUp();

				_gui = null;
				_context = null;
				_parent = null;
				_background = null;
				_focusedChild = null;
				_captureChild = null;
				_overChild = null;				
				_children = null;
				_drawWindows = null;
				_definedVariables = null;
				_updateVariables = null;
				_scripts = null;
				_timeLineEvents = null;
				_transitions = null;
				_namedEvents = null;
				_expressionRegisters = null;
				_ops = null;
				_regList = null;
				_lastEval = null;
				_builtInVariables = null;
			}

			_disposed = true;
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

	public enum Cursor
	{
		Arrow,
		Hand,
		Count
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

	public enum ScriptName
	{
		MouseEnter = 0,
		MouseExit,
		Action,
		Activate,
		Deactivate,
		Escape,
		Frame,
		Trigger,
		ActionRelease,
		Enter,
		EnterRelease,

		Count
	}

	public struct WindowExpressionOperation
	{
		public WindowExpressionOperationType Type;
		public object A;
		public int B;
		public int C;
		public int D;
	}

	public sealed class idTimeLineEvent
	{
		#region Properties
		public idGuiScriptList Event
		{
			get
			{
				return _scriptList;
			}
		}

		public bool Pending
		{
			get
			{
				return _pending;
			}
			set
			{
				_pending = value;
			}
		}

		public int Time
		{
			get
			{
				return _time;
			}
			set
			{
				_time = value;
			}
		}
		#endregion

		#region Members
		private idGuiScriptList _scriptList = new idGuiScriptList();
		private bool _pending;
		private int _time;
		#endregion

		#region Constructor
		public idTimeLineEvent()
		{

		}
		#endregion
	}

	public struct TransitionData
	{
		public idWindowVariable Variable;
		public int Offset;
		public idInterpolateAccelerationDecelerationLinear<Vector4> Interp;

		public TransitionData(idWindowVariable var)
		{
			this.Variable = var;
			this.Interp = new idInterpolateAccelerationDecelerationLinear<Vector4>();
			this.Offset = 0;
		}
	}
	
	public sealed class idNamedEvent
	{
		#region Properties
		public idGuiScriptList Event
		{
			get
			{
				return _scriptList;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}
		#endregion

		#region Members
		private idGuiScriptList _scriptList;
		private string _name;
		#endregion

		#region Constructor
		public idNamedEvent(string name)
		{
			_name = name;
			_scriptList = new idGuiScriptList();
		}
		#endregion
	}
}