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
using idTech4.Services;
using idTech4.UI.SWF;

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

		private int _activeScreen;
		private int _nextScreen;
		private int _transition;
		private int _platform;

		protected idSWF _gui;
	
		// TODO
		/*actionRepeater_t			actionRepeater;
		idMenuScreen *				menuScreens[MAX_SCREEN_AREAS];
		idList< idMenuWidget *, TAG_IDLIB_LIST_MENU>	children;
	
		idStaticList< idStr, NUM_GUI_SOUNDS >		sounds;

		idMenuWidget_CommandBar *	cmdBar;*/
		#endregion

		#region Constructor
		public idMenuHandler()
		{
			_activeScreen = -1;
			_nextScreen   = -1;
			_transition   = -1;

			/*for(int index = 0; index < MAX_SCREEN_AREAS; ++index)
			{
				menuScreens[index] = NULL;
			}*/

			// TODO: sounds.SetNum(NUM_GUI_SOUNDS);
		}

		// TODO: cleanup
		/*idMenuHandler::~idMenuHandler() {
			Cleanup();	
		}*/
		#endregion

		#region Initialization
		public void Init(string swfFile /* TODO:, idSoundWorld * sw*/)
		{
			Cleanup();

			_gui      = idEngine.Instance.GetService<idSWFManager>().Load(swfFile/* TODO: , sw*/);
			_platform = 2;
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

			idLog.Warning("TODO: gui->SetGlobal( \"updateMenuDisplay\", new (TAG_SWF) idSWFScriptFunction_updateMenuDisplay( gui, this ) );");
			idLog.Warning("TODO: gui->SetGlobal( \"activateMenus\", new (TAG_SWF) idSWFScriptFunction_activateMenu( this ) );");

			_gui.Activate(show);
		}

		private void Cleanup()
		{
			idLog.Warning("TODO: Cleanup");
			/*for(int index = 0; index < children.Num(); ++index)
			{
				assert(children[index]->GetRefCount() > 0);
				children[index]->Release();
			}
			children.Clear();

			for(int index = 0; index < MAX_SCREEN_AREAS; ++index)
			{
				if(menuScreens[index] != NULL)
				{
					menuScreens[index]->Release();
				}z
			}

			delete gui;
			gui = NULL;*/
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
		#endregion
	}
}