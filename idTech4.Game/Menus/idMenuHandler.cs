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
using System.Collections.Generic;

using idTech4.Services;
using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public abstract class idMenuHandler
	{
		#region Properties
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

		public List<idMenuWidget> _children = new List<idMenuWidget>();
		public idMenuScreen[] _menuScreens  = new idMenuScreen[GameConstants.MaxScreenAreas];
		// TODO
		/*actionRepeater_t			actionRepeater;
		idList< idMenuWidget *, TAG_IDLIB_LIST_MENU>	children;
	
		idStaticList< idStr, NUM_GUI_SOUNDS >		sounds;

		idMenuWidget_CommandBar *	cmdBar;*/
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
			}
			children.Clear();

			for ( int index = 0; index < MAX_SCREEN_AREAS; ++index ) {
				if ( menuScreens[ index ] != NULL ) {
					menuScreens[ index ]->Release();
				}
			}

			delete gui;
			gui = NULL;*/
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

			_gui.Globals.Set("updateMenuDisplay",	new idSWFScriptFunction_UpdateMenuDisplay(_gui, this));
			_gui.Globals.Set("activateMenus",		new idSWFScriptFunction_ActivateMenus(this));

			_gui.Activate(show);
		}

		public virtual void TriggerMenu() 
		{

		}
		#endregion

		#region Frame
		public virtual void Update()
		{
			// TODO: PumpWidgetActionRepeater();

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