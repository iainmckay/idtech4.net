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
using System;
using System.Collections.Generic;
using System.Diagnostics;

using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	/// <summary>
	/// We're using a model/view architecture, so this is the combination of both model and view.  The
	/// other part of the view is the SWF itself.
	/// </summary>
	public class idMenuWidget
	{
		#region Properties
		public idMenuWidget[] Children
		{
			get
			{
				return _children.ToArray();
			}
		}

		public WidgetState State
		{
			get
			{
				return _widgetState;
			}
			set
			{
				if(this.Sprite != null)
				{
					// FIXME: will need some more intelligence in the transitions to go from, say,
					// selected_up -> up ... but this should work fine for now.
					if(value == WidgetState.Hidden)
					{
						this.Sprite.IsVisible = false;
					}
					else
					{
						this.Sprite.IsVisible = true;

						if(value == WidgetState.Disabled)
						{
							this.Sprite.PlayFrame("disabled");
						}
						else if(value == WidgetState.Selecting)
						{
							if(_widgetState == WidgetState.Normal)
							{
								this.Sprite.PlayFrame("selecting"); // transition from unselected to selected
							}
							else
							{
								this.Sprite.PlayFrame("sel_up");
							}
						}
						else if(value == WidgetState.Selected)
						{
							this.Sprite.PlayFrame("sel_up");
						}
						else if(value == WidgetState.Normal)
						{
							if(_widgetState == WidgetState.Selecting)
							{
								this.Sprite.PlayFrame("unselecting"); // transition from selected to unselected
							}
							else if((_widgetState != WidgetState.Hidden) && (_widgetState != WidgetState.Normal))
							{
								this.Sprite.PlayFrame("out");
							}
							else
							{
								this.Sprite.PlayFrame("up");
							}
						}
					}

					Update();
				}

				_widgetState = value;
			}
		}
		#endregion

		#region Members
		protected idMenuHandler _menuData;

		private bool _handlerIsParent;

		private idSWF _swfObject;
		private idSWFSpriteInstance _boundSprite;
		private idMenuWidget _parent;

		private List<string> _spritePath      = new List<string>();
		private List<idMenuWidget> _children  = new List<idMenuWidget>();
		private List<idMenuWidget> _observers = new List<idMenuWidget>();

		private int[] _eventActionLookup;
		private List<List<idWidgetAction>> _eventActions = new List<List<idWidgetAction>>();

		private idMenuDataSource _dataSource;
		private int	_dataSourceFieldIndex;

		private int	_focusIndex;
		private WidgetState _widgetState;
		#endregion

		#region Constructor
		public idMenuWidget()
		{
			_eventActionLookup = new int[(int) WidgetEventType.MaxEvents];

			for(int i = 0; i < _eventActionLookup.Length; ++i)
			{
				_eventActionLookup[i] = -1;
			}
		}
		#endregion

		#region Data
		#region Properties
		public idMenuDataSource DataSource
		{
			get
			{
				return _dataSource;
			}
		}

		public int DataSourceFieldIndex
		{
			get
			{
				return _dataSourceFieldIndex;
			}
			set
			{
				_dataSourceFieldIndex = value;
			}
		}

		public idMenuWidget Focus
		{
			get
			{
				if((_focusIndex >= 0) && (_focusIndex < _children.Count))
				{
					return _children[_focusIndex];
				}

				return null;
			}
		}

		public int FocusIndex
		{
			get
			{
				return _focusIndex;
			}
		}

		public idMenuHandler MenuData
		{
			get
			{
				if(_parent != null)
				{
					return _parent.MenuData;
				}

				return _menuData;
			}
		}

		public idSWFSpriteInstance Sprite
		{
			get
			{
				return _boundSprite;
			}
		}

		public string[] SpritePath
		{
			get
			{
				return _spritePath.ToArray();
			}
		}
		#endregion
	
		#region Methods
		/// <summary>
		/// Takes the sprite path strings and resolves it to an actual sprite relative to a given root.
		/// </summary>
		/// <remarks>
		/// This is setup in this manner, because we can't resolve from path -> sprite immediately since
		/// SWFs aren't necessarily loaded at the time widgets are instantiated.
		/// </remarks>
		/// <param name="root"></param>
		/// <returns></returns>
		public bool BindSprite(idSWFScriptObject root)
		{
			string[] args = new string[6];

			Debug.Assert(this.SpritePath.Length > 0);

			for(int i = 0; i < this.SpritePath.Length; ++i)
			{
				args[i] = this.SpritePath[i];
			}

			_boundSprite = root.GetNestedSprite(args[0], args[1], args[2], args[3], args[4], args[5]);

			return (_boundSprite != null);
		}

		public void ClearSprite()
		{
			if(this.Sprite == null)
			{
				return;
			}

			this.Sprite.IsVisible = false;
			_boundSprite = null;
		}

		public void SetSpritePath(string arg1, string arg2 = null, string arg3 = null, string arg4 = null, string arg5 = null)
		{
			string[] args = { arg1, arg2, arg3, arg4, arg5 };
			_spritePath.Clear();

			for(int i = 0; i < args.Length; ++i)
			{
				if(args[i] == null)
				{
					break;
				}

				_spritePath.Add(args[i]);
			}
		}

		public void SetSpritePath(List<string> spritePath, string arg1, string arg2 = null, string arg3 = null, string arg4 = null, string arg5 = null)
		{
			SetSpritePath(spritePath.ToArray(), arg1, arg2, arg3, arg4, arg5);
		}

		public void SetSpritePath(string[] spritePath, string arg1, string arg2 = null, string arg3 = null, string arg4 = null, string arg5 = null)
		{
			string[] args = { arg1, arg2, arg3, arg4, arg5 };
			_spritePath = new List<string>(spritePath);

			for(int i = 0; i < args.Length; ++i)
			{
				if(args[i] == null)
				{
					break;
				}

				_spritePath.Add(args[i]);
			}
		}

		public void SetDataSource(idMenuDataSource dataSource, int fieldIndex)
		{
			_dataSource           = dataSource;
			_dataSourceFieldIndex = fieldIndex;
		}

		public void SetFocusIndex(int index, bool skipSound = false)
		{
			if(this.Children.Length == 0)
			{
				return;
			}

			int oldIndex = _focusIndex;

			Debug.Assert((index >= 0) && (index < this.Children.Length)); //&& oldIndex >= 0 && oldIndex < GetChildren().Num() );

			_focusIndex = index;

			if((oldIndex != _focusIndex) && (skipSound == false))
			{
				if(_menuData != null)
				{
					idLog.Warning("TODO: menuData->PlaySound( GUI_SOUND_FOCUS );");
				}
			}	

			idSWFParameterList parameters = new idSWFParameterList();
			parameters.Add(oldIndex);
			parameters.Add(index);

			// need to mark the widget as having lost focus
			if((oldIndex != index) && (oldIndex >= 0) && (oldIndex < this.Children.Length) && (GetChildByIndex(oldIndex).State != WidgetState.Hidden))
			{
				idLog.Warning("TODO: GetChildByIndex( oldIndex ).ReceiveEvent( idWidgetEvent( WIDGET_EVENT_FOCUS_OFF, 0, NULL, parms ) );");
			}

			//assert( GetChildByIndex( index ).GetState() != WIDGET_STATE_HIDDEN );
			idLog.Warning("TODO: GetChildByIndex( index ).ReceiveEvent( idWidgetEvent( WIDGET_EVENT_FOCUS_ON, 0, NULL, parms ) );");
		}
		#endregion
		#endregion

		#region Event Handling
		public virtual bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			bool handled = false;

			if(this.Parent != null)
			{
				handled = this.Parent.HandleAction(action, ev, widget);
			}
			else
			{
				if(forceHandled == true)
				{
					return false;
				}

				idMenuHandler data = this.MenuData;

				if(data != null)
				{
					return data.HandleAction(action, ev, widget, false);
				}
			}

			return handled;
		}

		public virtual void ObserveEvent(idMenuWidget widget, idWidgetEvent ev)
		{

		}

		public void SendEventToObservers(idWidgetEvent ev)
		{
			foreach(idMenuWidget widget in _observers)
			{
				widget.ObserveEvent(this, ev);
			}
		}

		public void RegisterEventObserver(idMenuWidget observer)
		{
			if(_observers.Contains(observer) == false)
			{
				_observers.Add(observer);
			}
		}

		/// <remarks>
		/// Events received through this function are passed to the innermost focused widget first, and then
		/// propagates back through each widget within the focus chain.  The first widget that handles the
		/// event will stop propagation.
		/// <para/>
		/// Each widget along the way will fire off an event to its observers, whether or not it actually
		/// handles the event.
		/// <para/>
		/// Note: How the focus chain is calculated:
		/// Descend through GetFocus() calls until you reach a NULL focus.  The terminating widget is the
		/// innermost widget, while *this* widget is the outermost widget.
		/// </remarks>
		/// <param name="ev"></param>
		public void ReceiveEvent(idWidgetEvent ev)
		{
			List<idMenuWidget> focusChain = new List<idMenuWidget>(16);

			int focusRunawayCount      = focusChain.Count;
			idMenuWidget focusedWidget = this;

			while((focusedWidget != null) && (--focusRunawayCount != 0))
			{
				focusChain.Add(focusedWidget);
				focusedWidget = focusedWidget.Focus;
			}

			// if hitting this then more than likely you have a self-referential chain.  If that's not
			// the case, then you may need to increase the size of the focusChain list.
			Debug.Assert(focusRunawayCount != 0);

			for(int focusIndex = focusChain.Count - 1; focusIndex >= 0; --focusIndex)
			{
				idMenuWidget widget = focusChain[focusIndex];

				if(widget.ExecuteEvent(ev) == true)
				{
					break; // this widget has handled the event, so stop propagation
				}
			}
		}

		/// <summary>
		/// Handles the event directly, and doesn't pass it through the focus chain.
		/// </summary>
		/// <remarks>
		/// This should only be used in very specific circumstances!  Most events should go to the focus.
		/// </remarks>
		/// <param name="ev"></param>
		/// <returns></returns>
		public virtual bool ExecuteEvent(idWidgetEvent ev)
		{
			idWidgetAction[] actions = GetEventActions(ev.Type);

			if(actions != null)
			{
				for(int actionIndex = 0; actionIndex < actions.Length; ++actionIndex)
				{
					HandleAction(actions[actionIndex], ev, this);
				}
			}

			SendEventToObservers(ev);

			return ((actions != null) && (actions.Length > 0));
		}

		public idWidgetAction[] GetEventActions(WidgetEventType eventType)
		{
			if(_eventActionLookup[(int) eventType] == -1)
			{
				return null;
			}

			return _eventActions[_eventActionLookup[(int) eventType]].ToArray();
		}

		public idWidgetAction AddEventAction(WidgetEventType eventType)
		{
			if(_eventActionLookup[(int) eventType] == -1)
			{
				_eventActionLookup[(int) eventType] = _eventActions.Count;
				_eventActions.Add(new List<idWidgetAction>());
			}

			idWidgetAction action = new idWidgetAction();

			_eventActions[_eventActionLookup[(int) eventType]].Add(action);

			return action;
		}

		public void ClearEventActions()
		{
			_eventActions.Clear();

			for(int i = 0; i < _eventActionLookup.Length; ++i)
			{
				_eventActionLookup[i] = -1;
			}
		}
		#endregion

		#region Hierarchy
		#region Properties
		public idMenuWidget Parent
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

		public idSWF SWFObject 
		{
			get
			{
				if(_swfObject != null)
				{
					return _swfObject;
				}

				if(_parent != null)
				{
					return _parent.SWFObject;
				}

				if(_menuData != null)
				{
					return _menuData.UserInterface;
				}

				return null;
			}
			set
			{
				_swfObject = value;
			}
		}

		public bool HandlerIsParent
		{
			get	
			{
				return _handlerIsParent;
			}
			set
			{
				_handlerIsParent = value;
			}
		}
		#endregion

		#region Methods
		public idMenuWidget GetChildByIndex(int index)
		{
			return _children[index];
		}

		public void AddChild(idMenuWidget widget)
		{
			if(_children.Contains(widget) == true)
			{
				return;	// attempt to add a widget that was already in the list
			}

			if(widget.Parent != null)
			{
				// take out of previous parent
				widget.Parent.RemoveChild(widget);
			}

			widget.Parent = this;
			_children.Add(widget);
		}

		public void RemoveChild(idMenuWidget widget)
		{
			Debug.Assert(widget.Parent == this);

			_children.Remove(widget);
			
			widget.Parent = null;
		}

		public void RemoveAllChildren()
		{
			foreach(idMenuWidget widget in _children)
			{
				Debug.Assert(widget.Parent == this);

				widget.Parent = null;
			}
		
			_children.Clear();
		}

		public bool HasChild(idMenuWidget widget)
		{
			return _children.Contains(widget);
		}		

		protected void ForceFocusIndex(int index)
		{
			_focusIndex = index;
		}
		#endregion
		#endregion

		#region Initialization
		public virtual void Initialize(idMenuHandler data)
		{
			_menuData = data;
		}
		#endregion

		#region Frame
		public virtual void Update() 
		{ 
		
		}

		public virtual void Show()
		{
			if(this.SWFObject == null)
			{
				return;
			}

			if(BindSprite(this.SWFObject.RootObject) == false)
			{
				return;
			}

			this.Sprite.IsVisible = true;

			int currentFrame = this.Sprite.CurrentFrame;
			int findFrame = this.Sprite.FindFrame("rollOn");
			int idleFrame = this.Sprite.FindFrame("idle");

			if((currentFrame == findFrame) || ((currentFrame > 1) && (currentFrame <= idleFrame)))
			{
				return;
			}

			this.Sprite.PlayFrame(findFrame);
		}

		public virtual void Hide()
		{
			if(this.SWFObject == null)
			{
				return;
			}

			if(BindSprite(this.SWFObject.RootObject) == false)
			{
				return;
			}

			int currentFrame = this.Sprite.CurrentFrame;
			int findFrame    = this.Sprite.FindFrame("rollOff");

			if((currentFrame >= findFrame) || (currentFrame == 1))
			{
				return;
			}

			this.Sprite.PlayFrame(findFrame);
		}
		#endregion
	}

	public class idWidgetAction
	{
		#region Properties
		public WidgetActionType Type
		{
			get
			{
				return _action;
			}
		}

		public idSWFParameterList Parameters
		{
			get
			{
				return _parameters;
			}
		}

		public idSWFScriptFunction ScriptFunction
		{
			get
			{
				return _scriptFunction;
			}
		}
		#endregion

		#region Members
		private WidgetActionType _action;
		private idSWFParameterList _parameters;
		private idSWFScriptFunction _scriptFunction;
		#endregion

		#region Constructors
		public idWidgetAction()
		{
			_action = WidgetActionType.None;
		}

		public idWidgetAction(idWidgetAction src)
		{
			_action         = src.Type;
			_parameters     = src.Parameters;
			_scriptFunction = src.ScriptFunction;
		}
		#endregion
	
		#region Overloads
		public static bool operator ==(idWidgetAction a, idWidgetAction b)
		{
		    if(Object.ReferenceEquals(a, b) == true)
			{
				return true;
			}

			if(((object) a == null) || ((object) b == null))
			{
				return false;
			}

			if((a.Type != b.Type)
				|| (a.Parameters.Count != b.Parameters.Count))
			{
				return false;
			}

			// everything else is equal, so check all parms. NOTE: this assumes we are only sending
			// integral types.
			for(int i = 0; i < a.Parameters.Count; ++i)
			{
				if((a.Parameters[i].TypeOf() != b.Parameters[i].TypeOf())
					|| (a.Parameters[i].ToInt32() != b.Parameters[i].ToInt32()))
				{
					return false;
				}
			}

			return true;
		}

		public static bool operator !=(idWidgetAction a, idWidgetAction b)
		{
			return !(a == b);
		}

		public override bool Equals(Object obj)
		{
			return (this == obj);
		}

		public bool Equals(idWidgetAction action)
		{
			return (this == action);
		}
		#endregion

		#region Methods
		public void Set(idSWFScriptFunction function)
		{
			_action         = WidgetActionType.Function;
			_scriptFunction = function;
		}

		public void Set(WidgetActionType type)
		{
			_action = type;
			_parameters.Clear();
		}

		public void Set(WidgetActionType type, idSWFScriptVariable var)
		{
			_action = _action;

			_parameters.Clear();
			_parameters.Add(var);
		}

		public void Set(WidgetActionType type, idSWFScriptVariable var1, idSWFScriptVariable var2)
		{
			_action = _action;

			_parameters.Clear();
			_parameters.Add(var1);
			_parameters.Add(var2);
		}

		public void Set(WidgetActionType type, idSWFScriptVariable var1, idSWFScriptVariable var2, idSWFScriptVariable var3)
		{
			_action = _action;

			_parameters.Clear();
			_parameters.Add(var1);
			_parameters.Add(var2);
			_parameters.Add(var3);
		}

		public void Set(WidgetActionType type, idSWFScriptVariable var1, idSWFScriptVariable var2, idSWFScriptVariable var3, idSWFScriptVariable var4)
		{
			_action = _action;

			_parameters.Clear();
			_parameters.Add(var1);
			_parameters.Add(var2);
			_parameters.Add(var3);
			_parameters.Add(var4);
		}
		#endregion
	}

	public class idWidgetEvent
	{
		#region Properties
		public int Argument
		{
			get
			{
				return _arg;
			}
		}

		public idSWFParameterList Parameters
		{
			get
			{
				return _parameters;
			}
		}

		public WidgetEventType Type
		{
			get
			{
				return _type;
			}
		}

		public idSWFScriptObject ScriptObject
		{
			get
			{
				return _object;
			}
		}
		#endregion

		#region Members
		private WidgetEventType _type;
		private int _arg;
		private idSWFScriptObject _object;
		private idSWFParameterList _parameters;
		#endregion

		#region Constructor
		public idWidgetEvent()
		{
			_type = WidgetEventType.Press;
		}

		public idWidgetEvent(WidgetEventType type, int arg, idSWFScriptObject scriptObject, idSWFParameterList parameters)
		{
			_type       = type;
			_arg        = arg;
			_object     = scriptObject;
			_parameters = parameters;
		}
		#endregion
	}

	public enum WidgetState
	{
		Hidden,
		Normal,
		Selecting,
		Selected,
		Disabled
	}

	public enum WidgetEventType
	{
		Press,
		Release,
		RollOver,
		RollOut,
		FocusOn,
		FocusOff,

		ScrollLeftStickUp,
		ScrollLeftStickUpRelease,
		ScrollLeftStickDown,
		ScrollLeftStickDownRelease,
		ScrollLeftStickLeft,
		ScrollLeftStickLeftRelease,
		ScrollLeftStickRight,
		ScrollLeftStickRightRelease,

		ScrollRightStickUp,
		ScrollRightStickUpRelease,
		ScrollRightStickDown,
		ScrollRightStickDownRelease,
		ScrollRightStickLeft,
		ScrollRightStickLeftRelease,
		ScrollRightStickRight,
		ScrollRightStickRightRelease,

		ScrollUp,
		ScrollUpRelease,
		ScrollDown,
		ScrollDownRelease,
		ScrollLeft,
		ScrollLeftRelease,
		ScrollRight,
		ScrollRightRelease,

		DragStart,
		DragStop,

		ScrollPageDown,
		ScrollPageDownRelease,
		ScrollPageUp,
		ScrollPageUpRelease,

		Scroll,
		ScrollRelease,
		Back,
		Command,
		TabNext,
		TabPrevious,

		MaxEvents
	}

	public enum WidgetActionType
	{
		None,
		Command,
		Function,
		ScrollVertical,
		ScrollVerticalVariable,
		ScrollPage,
		ScrollHorizontal,
		ScrollTab,
		StartRepeater,
		StopRepeater,
		AdjustField,
		PressFocused,
		Joystick3OnPress,
		Joystick4OnPress,

		GotoMenu,
		GoBack,
		ExitGame,
		LaunchMultiplayer,
		MenuBarSelect,
		EmailHover,

		// PDA USER DATA ACTIONS
		PdaSelectUser,
		SelectGamerTag,
		PdaSelectNavigation,
		PdaSelectAudio,
		PdaSelectVideo,
		PdaSelectItem,
		ScrollDrag,

		// PDA EMAIL ACTIONS
		PdaSelectEmail,
		PdaClose,
		Refresh,
		MutePlayer,
	}

	public enum ScrollType
	{
		Single,
		Page,
		Full,
		Top,
		End
	}
}