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
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public class idMenuScreen_Shell_Credits : idMenuScreen 
	{
		#region Members
		private int _creditIndex;
		private idMenuWidget_Button _backButton;
		#endregion

		#region Constructor
		public idMenuScreen_Shell_Credits() 
			: base()
		{

		}
		#endregion

		#region idMenuScreen implementation
		public override bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			if(_menuData == null)
			{
				return true;
			}

			if(_menuData.ActiveScreen != ShellArea.Credits)
			{
				return false;
			}

			WidgetActionType actionType = action.Type;
			ICommandSystem cmdSystem    = idEngine.Instance.GetService<ICommandSystem>();

			switch(actionType)
			{
				case WidgetActionType.GoBack:
					idMenuHandler_Shell shell = _menuData as idMenuHandler_Shell;
					bool complete             = false;

					if(shell != null)
					{
						idLog.Warning("TODO: complete = shell->GetGameComplete();");
					}

					if(complete == true)
					{
						cmdSystem.BufferCommandText("disconnect", Execute.Now);
					}
					else
					{
						_menuData.SetNextScreen(ShellArea.Root, MainMenuTransition.Simple);
					}
			
					return true;
			}

			return base.HandleAction(action, ev, widget, forceHandled);
		}

		public override void Initialize(idMenuHandler data)
		{
			base.Initialize(data);

			if(data != null)
			{
				this.UserInterface = data.UserInterface;
			}

			SetSpritePath("menuCredits");

			_backButton = new idMenuWidget_Button();
			_backButton.Initialize(data);
			_backButton.Label = "#str_02305";
			_backButton.SetSpritePath(this.SpritePath, "info", "btnBack");
			_backButton.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.GoBack);

			AddChild(_backButton);

			idLog.Warning("TODO: SetupCreditList();");
		}

		public override void ShowScreen(MainMenuTransition transitionType)
		{
			if(_menuData != null)
			{
				idMenuHandler_Shell shell = _menuData as idMenuHandler_Shell;
				bool complete             = false;

				if(shell != null)
				{
					idLog.Warning("TODO: complete = shell->GetGameComplete();");
				}

				if(complete == true)
				{
					idLog.Warning("TODO: menuData->PlaySound( GUI_SOUND_MUSIC );");
				}
			}

			base.ShowScreen(transitionType);

			_creditIndex = 0;
			idLog.Warning("TODO: UpdateCredits();");
		}

		public override void Update()
		{
			if(_menuData != null)
			{
				idMenuWidget_CommandBar cmdBar = _menuData.CommandBar;

				if(cmdBar != null)
				{
					idLog.Warning("TODO: Shell_Credits update");

					/*cmdBar->ClearAllButtons();

					idMenuHandler_Shell * shell = dynamic_cast< idMenuHandler_Shell * >( menuData );
					bool complete = false;
					if ( shell != NULL ) {
						complete = shell->GetGameComplete();
					}

					idMenuWidget_CommandBar::buttonInfo_t * buttonInfo;
					if ( !complete ) {
						buttonInfo = cmdBar->GetButton( idMenuWidget_CommandBar::BUTTON_JOY2 );
						if ( menuData->GetPlatform() != 2 ) {
							buttonInfo->label = "#str_00395";
						}
						buttonInfo->action.Set( WIDGET_ACTION_GO_BACK );
					} else {
						buttonInfo = cmdBar->GetButton( idMenuWidget_CommandBar::BUTTON_JOY1 );
						if ( menuData->GetPlatform() != 2 ) {
							buttonInfo->label = "#str_swf_continue";
						}
						buttonInfo->action.Set( WIDGET_ACTION_GO_BACK );
					}*/
				}		
			}

			idSWFScriptObject root = this.SWFObject.RootObject;

			if(BindSprite(root) == true)
			{
				idSWFTextInstance heading = this.Sprite.ScriptObject.GetNestedText("info", "txtHeading");
		
				if(heading != null)
				{
					heading.Text = "#str_02218";
					heading.SetStrokeInfo(true, 0.75f, 1.75f);
				}
			}

			if(_backButton != null)
			{
				_backButton.BindSprite(root);
			}

			base.Update();
		}
		#endregion
	}
}