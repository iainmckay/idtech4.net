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
	public class idMenuScreen_Shell_ControllerLayout : idMenuScreen 
	{
		#region Constants
		private const int LayoutOptionCount = 1;
		#endregion

		#region Members
		private idMenuWidget_DynamicList _options;
		private idMenuWidget_Button _backButton;
		#endregion

		#region Constructor
		public idMenuScreen_Shell_ControllerLayout()
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

			if(_menuData.ActiveScreen != ShellArea.ControllerLayout)
			{
				return false;
			}

			idLog.Warning("TODO: Shell_ControllerLayout handle action");

			/*widgetAction_t actionType = action.GetType();
			const idSWFParmList & parms = action.GetParms();

			switch ( actionType ) {
				case WIDGET_ACTION_GO_BACK: {
					menuData->SetNextScreen( SHELL_AREA_GAMEPAD, MENU_TRANSITION_SIMPLE );
					return true;
				}
				case WIDGET_ACTION_PRESS_FOCUSED: {
					if ( parms.Num() != 1 ) {
						return true;
					}

					if ( options == NULL ) {
						return true;
					}

					int selectionIndex = parms[0].ToInteger();
					if ( selectionIndex != options->GetFocusIndex() ) {
						options->SetViewIndex( options->GetViewOffset() + selectionIndex );
						options->SetFocusIndex( selectionIndex );
					}						

					layoutData.AdjustField( selectionIndex, 1 );
					options->Update();
					UpdateBindingInfo();
					return true;
				}
				case WIDGET_ACTION_START_REPEATER: {

					if ( options == NULL ) {
						return true;
					}

					if ( parms.Num() == 4 ) {
						int selectionIndex = parms[3].ToInteger();
						if ( selectionIndex != options->GetFocusIndex() ) {
							options->SetViewIndex( options->GetViewOffset() + selectionIndex );
							options->SetFocusIndex( selectionIndex );
						}
					}
					break;
				}
				case WIDGET_ACTION_ADJUST_FIELD: {
					if ( widget != NULL && widget->GetDataSource() != NULL ) {
						widget->GetDataSource()->AdjustField( widget->GetDataSourceFieldIndex(), parms[ 0 ].ToInteger() );
						widget->Update();
					}
					UpdateBindingInfo();
					return true;
				}
			}*/

			return base.HandleAction(action, ev, widget, forceHandled);
		}

		public override void HideScreen(MainMenuTransition transitionType)
		{
			idLog.Warning("TODO: layoutData.CommitData();");

			base.HideScreen(transitionType);
		}

		public override void Initialize(idMenuHandler data)
		{
			base.Initialize(data);

			if(data != null)
			{
				this.UserInterface = data.UserInterface;
			}

			SetSpritePath("menuControllerLayout");

			_options = new idMenuWidget_DynamicList();
			_options.VisibleOptionCount = LayoutOptionCount;
			_options.SetSpritePath(this.SpritePath, "info", "controlInfo", "options");
			_options.IsWrappingAllowed = true;
			_options.ControlList       = true;
			_options.Initialize(data);

			AddChild(_options);

			_backButton = new idMenuWidget_Button();
			_backButton.Initialize(data);
			_backButton.Label = "#str_swf_gamepad_heading";
			_backButton.SetSpritePath(this.SpritePath, "info", "btnBack");
			_backButton.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.GoBack);

			AddChild(_backButton);
			
			idMenuWidget_ControlButton control = new idMenuWidget_ControlButton();
			control.OptionType = MenuOptionType.ButtonFullTextSlider;
			control.Label = "CONTROL LAYOUT";	// Auto Weapon Reload
			idLog.Warning("TODO: control.SetDataSource(layoutData, Layo control->SetDataSource( &layoutData, idMenuDataSource_LayoutSettings::LAYOUT_FIELD_LAYOUT );");
			control.SetupEvents(GameConstants.DefaultRepeatTime, _options.Children.Length);
			control.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.PressFocused, _options.Children.Length);
			
			_options.AddChild(control);
		}

		public override void ShowScreen(MainMenuTransition transitionType)
		{
			idLog.Warning("TODO: layoutData.LoadData();");

			base.ShowScreen(transitionType);

			if(this.Sprite != null)
			{
				idSWFSpriteInstance layout360 = this.Sprite.ScriptObject.GetNestedSprite("info", "controlInfo", "layout360");
				idSWFSpriteInstance layoutPS3 = this.Sprite.ScriptObject.GetNestedSprite("info", "controlInfo", "layoutPS3");

				if((layout360 != null) && (layoutPS3 != null))
				{
					layout360.IsVisible = true;
					layoutPS3.IsVisible = false;
				}
			}

			idLog.Warning("TODO: UpdateBindingInfo();");
		}

		public override void Update()
		{
			if(_menuData != null)
			{
				idMenuWidget_CommandBar cmdBar = _menuData.CommandBar;

				if(cmdBar != null)
				{
					cmdBar.ClearAllButtons();

					ButtonInfo buttonInfo = cmdBar.GetButton(Button.Joystick2);

					if(_menuData.GetPlatform() != 2)
					{
						buttonInfo.Label = "#str_00395";
					}

					buttonInfo.Action.Set(WidgetActionType.GoBack);

					buttonInfo = cmdBar.GetButton(Button.Joystick1);
					buttonInfo.Action.Set(WidgetActionType.PressFocused);
				}		
			}

			idSWFScriptObject root = this.SWFObject.RootObject;

			if(BindSprite(root) == true)
			{
				idSWFTextInstance heading = this.Sprite.ScriptObject.GetNestedText("info", "txtHeading");

				if(heading != null)
				{
					heading.Text = "#str_swf_controller_layout";	// CONTROLLER LAYOUT
					heading.SetStrokeInfo(true, 0.75f, 1.75f);
				}

				idSWFSpriteInstance gradient = this.Sprite.ScriptObject.GetNestedSprite("info", "gradient");

				if((gradient != null) && (heading != null))
				{
					gradient.PositionX = heading.TextLength;
				}

				if(_menuData != null)
				{
					idSWFSpriteInstance layout = this.Sprite.ScriptObject.GetNestedSprite("info", "controlInfo", "layout360");

					if(layout != null)
					{
						if(_menuData.GetPlatform(true) == 2)
						{
							layout.StopFrame(1);
						}
						else
						{
							layout.StopFrame(_menuData.GetPlatform(true) + 1);
						}
					}
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