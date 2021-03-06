﻿/*
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
using System.Collections.Generic;

using idTech4.Services;
using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public abstract class idMenuHandler
	{
		#region Properties
		public ShellArea ActiveScreen
		{
			get
			{
				return _activeScreen;
			}
		}

		public idMenuWidget_CommandBar CommandBar
		{
			get
			{
				return _cmdBar;
			}
		}

		public bool IsActive
		{
			get
			{
				if(_gui != null)
				{
					return _gui.IsActive;
				}

				return false;
			}
		}

		public idSWF UserInterface
		{
			get
			{
				return _gui;
			}
		}
		#endregion

		#region Members
		private bool _scrollingMenu;
		private int _scrollCounter;

		protected ShellArea _activeScreen;
		protected ShellArea _nextScreen;
		protected MainMenuTransition _transition;

		private int _platform;

		protected idSWF _gui;
		protected idSWF _introGui;

		protected List<idMenuWidget> _children = new List<idMenuWidget>();
		protected idMenuScreen[] _menuScreens  = new idMenuScreen[GameConstants.MaxScreenAreas];
		
		protected ActionRepeater _actionRepeater = new ActionRepeater();
	
		/*idStaticList< idStr, NUM_GUI_SOUNDS >		sounds;*/

		protected idMenuWidget_CommandBar _cmdBar;
		#endregion

		#region Constructor
		public idMenuHandler()
		{
			_activeScreen = ShellArea.Invalid;
			_nextScreen   = ShellArea.Invalid;
			_transition   = MainMenuTransition.Invalid;

			for(int index = 0; index < _menuScreens.Length; ++index)
			{
				_menuScreens[index] = null;
			}

			// TODO: sounds.SetNum(NUM_GUI_SOUNDS);
		}

		// TODO: cleanup
		/*idMenuHandler::~idMenuHandler() {
			Cleanup();	
		}*/
		#endregion

		#region Children
		public void AddChild(idMenuWidget widget)
		{
			widget.SWFObject       = _gui;
			widget.HandlerIsParent = true;

			_children.Add(widget);
		}
		#endregion

		#region Events
		public virtual bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			WidgetActionType actionType   = action.Type;
			idSWFParameterList parameters = action.Parameters;

			switch(actionType)
			{
				case WidgetActionType.AdjustField:
					if((widget != null) && (widget.DataSource != null))
					{
						widget.DataSource.AdjustField(widget.DataSourceFieldIndex, parameters[0].ToInt32());
						widget.Update();
					}

					return true;
				
				case WidgetActionType.Function:
					if(action.ScriptFunction != null)
					{
						action.ScriptFunction.Invoke(ev.ScriptObject, ev.Parameters);
					}

					return true;
				
				case WidgetActionType.PressFocused:
					idMenuScreen screen = _menuScreens[(int) _activeScreen];

					if(screen != null)
					{
						idWidgetEvent pressEvent = new idWidgetEvent(WidgetEventType.Press, 0, ev.ScriptObject, new idSWFParameterList());
						screen.ReceiveEvent(pressEvent);
					}

					return true;
				
				case WidgetActionType.StartRepeater:
					idLog.Warning("TODO: HandleAction.StartRepeater");
					/*idWidgetAction repeatAction;
					widgetAction_t repeatActionType = static_cast< widgetAction_t >( parms[ 0 ].ToInteger() );
					assert( parms.Num() >= 2 );
					int repeatDelay = DEFAULT_REPEAT_TIME;
					if ( parms.Num() >= 3 ) {
						repeatDelay = parms[2].ToInteger();
					} 
					repeatAction.Set( repeatActionType, parms[ 1 ], repeatDelay );
					StartWidgetActionRepeater( widget, repeatAction, event );*/
					
					return true;
				
				case WidgetActionType.StopRepeater:
					ClearWidgetActionRepeater();
					return true;				
			}

			if(widget.HandlerIsParent == false)
			{
				for(int index = 0; index < _children.Count; ++index)
				{
					if(_children[index] != null)
					{
						if(_children[index].HandleAction(action, ev, widget, forceHandled) == true)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public virtual bool HandleGuiEvent(SystemEvent ev)
		{
			if((_gui != null) && (_activeScreen != ShellArea.Invalid))
			{
				_gui.HandleEvent(ev);
			}

			return false; 
		}
		#endregion

		#region Initialization
		public virtual void Initialize(string swfFile /* TODO:, idSoundWorld * sw*/)
		{
			Cleanup();

			_gui      = idEngine.Instance.GetService<idSWFManager>().Load(swfFile/* TODO: , sw*/);
			_platform = 2;
		}

		protected virtual void Cleanup()
		{
			idLog.Warning("TODO: idMenuHandler.Cleanup");

			/*for ( int index = 0; index < children.Num(); ++index ) {
				assert( children[ index ]->GetRefCount() > 0 );
				children[ index ]->Release();
			}*/

			_children.Clear();

			/*for ( int index = 0; index < MAX_SCREEN_AREAS; ++index ) {
				if ( menuScreens[ index ] != NULL ) {
					menuScreens[ index ]->Release();
				}
			}

			delete gui;*/

			_gui = null;
		}
		#endregion

		#region State
		public virtual void ActivateMenu(bool show)
		{
			if(_gui == null)
			{
				return;
			}

			if(show == false)
			{
				_gui.Activate(show);
				return;
			}

			_gui.Globals.Set("updateMenuDisplay", new idSWFScriptFunction_UpdateMenuDisplay(_gui, this));
			_gui.Globals.Set("activateMenus",     new idSWFScriptFunction_ActivateMenus(this));

			_gui.Activate(show);
		}

		public int GetPlatform(bool realPlatform = false)
		{
			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			if((_platform == 2) && (cvarSystem.GetBool("in_useJoystick") == true) && (realPlatform == false))
			{
				return 0;
			}

			return _platform;
		}

		public void SetNextScreen(ShellArea area, MainMenuTransition transitionType)
		{
			_nextScreen = area;
			_transition = transitionType;
		}

		public virtual void TriggerMenu() 
		{

		}
		#endregion

		#region Frame
		public virtual void Update()
		{
			PumpWidgetActionRepeater();

			if((_gui != null) && (_gui.IsActive == true))
			{
				_gui.Draw(idEngine.Instance.GetService<IRenderSystem>(), idEngine.Instance.ElapsedTime);
			}
		}

		public void UpdateChildren()
		{
			for(int index = 0; index < _children.Count; ++index)
			{
				if(_children[index] != null)
				{
					_children[index].Update();
				}
			}
		}

		public virtual void UpdateMenuDisplay(int menu)
		{
			if(_menuScreens[menu] != null)
			{
				_menuScreens[menu].Update();
			}

			UpdateChildren();
		}
		#endregion

		#region ActionRepeater
		protected void StartWidgetActionRepeater(idMenuWidget widget, idWidgetAction action, idWidgetEvent ev)
		{
			if((_actionRepeater.IsActive == true) && (_actionRepeater.Action == action))
			{
				return;	// don't attempt to reactivate an already active repeater
			}

			_actionRepeater.IsActive        = true;
			_actionRepeater.Action          = action;
			_actionRepeater.Widget          = widget;
			_actionRepeater.Event           = ev;
			_actionRepeater.RepetitionCount = 0;
			_actionRepeater.NextRepeatTime  = 0;
			_actionRepeater.ScreenIndex     = _activeScreen;	// repeaters are cleared between screens

			if(action.Parameters.Count == 2)
			{
				_actionRepeater.RepeatDelay = action.Parameters[1].ToInt32();
			}
			else
			{
				_actionRepeater.RepeatDelay = GameConstants.DefaultRepeatTime;
			}

			// do the first event immediately
			PumpWidgetActionRepeater();
		}

		protected void PumpWidgetActionRepeater()
		{
			if(_actionRepeater.IsActive == false)
			{
				return;
			}

			if((_activeScreen != _actionRepeater.ScreenIndex) || (_nextScreen != _activeScreen)) 
			{ // || common->IsDialogActive() ) {
				_actionRepeater.IsActive = false;
				return;
			}

			if(_actionRepeater.NextRepeatTime > idEngine.Instance.ElapsedTime)
			{
				return;
			}

			// need to hold down longer on the first iteration before we continue to scroll
			if(_actionRepeater.RepetitionCount == 0) 
			{
				_actionRepeater.NextRepeatTime = idEngine.Instance.ElapsedTime + 400;
			} 
			else 
			{
				_actionRepeater.NextRepeatTime = idEngine.Instance.ElapsedTime + _actionRepeater.RepeatDelay;
			}

			if(_actionRepeater.Widget != null)
			{
				_actionRepeater.Widget.HandleAction(_actionRepeater.Action, _actionRepeater.Event, _actionRepeater.Widget);
				_actionRepeater.RepetitionCount++;
			}
		}

		protected void ClearWidgetActionRepeater()
		{
			_actionRepeater.IsActive = false;
		}

		protected class ActionRepeater
		{
			#region Members
			public idMenuWidget Widget;
			public idWidgetEvent Event;
			public idWidgetAction Action;
			public int RepetitionCount;
			public long	NextRepeatTime;
			public int RepeatDelay;
			public ShellArea ScreenIndex;
			public bool IsActive;
			#endregion

			#region Constructor
			public ActionRepeater()
			{
				this.ScreenIndex = ShellArea.Invalid;
				this.RepeatDelay = GameConstants.DefaultRepeatTime;
			}
			#endregion
		}
		#endregion
	}

	public class idSWFScriptFunction_UpdateMenuDisplay : idSWFScriptFunction
	{
		#region Members
		private idSWF _gui;
		private idMenuHandler _handler;
		#endregion

		#region Constructor
		public idSWFScriptFunction_UpdateMenuDisplay(idSWF gui, idMenuHandler handler)
		{
			_gui     = gui;
			_handler = handler;
		}
		#endregion

		#region idSWFScriptFunction implementation
		public override idSWFScriptVariable Invoke(idSWFScriptObject scriptObj, idSWFParameterList parms)
		{
			if(_handler != null)
			{
				int screen = parms[0].ToInt32();

				_handler.UpdateMenuDisplay(screen);
			}

			return new idSWFScriptVariable();
		}
		#endregion
	}

	public class idSWFScriptFunction_ActivateMenus : idSWFScriptFunction
	{
		#region Members
		private idMenuHandler _handler;
		#endregion

		#region Constructor
		public idSWFScriptFunction_ActivateMenus(idMenuHandler handler)
		{
			_handler = handler;
		}
		#endregion

		#region idSWFScriptFunction implementation
		public override idSWFScriptVariable Invoke(idSWFScriptObject scriptObj, idSWFParameterList parms)
		{
			if(_handler != null)
			{				
				_handler.TriggerMenu();
			}

			return new idSWFScriptVariable();
		}
		#endregion
	}
}