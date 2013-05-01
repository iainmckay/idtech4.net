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
	public class idMenuScreen_Shell_GamePad : idMenuScreen
	{
		#region Constants
		private const int ControlOptionCount = 8;
		#endregion

		#region Members
		private idMenuWidget_DynamicList _options;
		private idMenuWidget_Button _buttonBack;
		#endregion

		#region Constructor
		public idMenuScreen_Shell_GamePad()
			: base()
		{

		}
		#endregion

		#region idMenuScreen implementation
		public override void Initialize(idMenuHandler data)
		{
			base.Initialize(data);

			if(data != null)
			{
				this.UserInterface = data.UserInterface;
			}

			ILocalization localization = idEngine.Instance.GetService<ILocalization>();

			SetSpritePath("menuGamepad");

			_options = new idMenuWidget_DynamicList();
			_options.VisibleOptionCount = ControlOptionCount;
			_options.SetSpritePath(this.SpritePath, "info", "options");
			_options.IsWrappingAllowed = true;
			_options.ControlList       = true;
			_options.Initialize(data);

			AddChild(_options);
			
			idMenuWidget_Help helpWidget = new idMenuWidget_Help();
			helpWidget.SetSpritePath(this.SpritePath, "info", "helpTooltip");
			
			AddChild(helpWidget);
			
			_buttonBack = new idMenuWidget_Button();
			_buttonBack.Initialize(data);
			_buttonBack.Label = localization.Get("#str_04158").ToUpper();
			_buttonBack.SetSpritePath(this.SpritePath, "info", "btnBack");
			_buttonBack.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.GoBack);

			AddChild(_buttonBack);
			
			idMenuWidget_ControlButton control;

#if !ID_PC
			control             = new idMenuWidget_ControlButton();
			control.OptionType  = MenuOptionType.ButtonText;
			control.Label       = "#str_swf_gamepad_config";	// Gamepad Configuration
			control.Description = "#str_swf_config_desc";
			control.RegisterEventObserver(helpWidget);
			control.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) GamePadMenuCommands.Configuration);	
			
			_options.AddChild(control);
#endif

			control             = new idMenuWidget_ControlButton();
			control.OptionType  = MenuOptionType.SliderToggle;
			control.Label       = "#str_swf_lefty_flip";
			idLog.Warning("TODO: control->SetDataSource( &gamepadData, idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_LEFTY );");
			control.SetupEvents(GameConstants.DefaultRepeatTime, _options.Children.Length);
			control.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) GamePadMenuCommands.Lefty);
			control.RegisterEventObserver(helpWidget);
			
			_options.AddChild(control);
			
			control             = new idMenuWidget_ControlButton();
			control.OptionType  = MenuOptionType.SliderToggle;
			control.Label       = "#str_swf_invert_gamepad";
			idLog.Warning("TODO: control->SetDataSource( &gamepadData, idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_INVERT );");
			control.SetupEvents(GameConstants.DefaultRepeatTime, _options.Children.Length);
			control.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) GamePadMenuCommands.Invert);
			control.RegisterEventObserver(helpWidget);
			
			_options.AddChild(control);
			
			control             = new idMenuWidget_ControlButton();
			control.OptionType  = MenuOptionType.SliderToggle;
			control.Label       = "#str_swf_vibration";
			idLog.Warning("TODO: control->SetDataSource( &gamepadData, idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_VIBRATE );");
			control.SetupEvents(GameConstants.DefaultRepeatTime, _options.Children.Length);
			control.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) GamePadMenuCommands.Vibrate);
			control.RegisterEventObserver(helpWidget);
			
			_options.AddChild(control);

			control             = new idMenuWidget_ControlButton();
			control.OptionType  = MenuOptionType.SliderBar;
			control.Label       = "#str_swf_hor_sens";
			idLog.Warning("TODO: control->SetDataSource( &gamepadData, idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_HOR_SENS );");
			control.SetupEvents(2, _options.Children.Length);
			control.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) GamePadMenuCommands.HorizontalSensitivity);
			control.RegisterEventObserver(helpWidget);
			
			_options.AddChild(control);

			control             = new idMenuWidget_ControlButton();
			control.OptionType  = MenuOptionType.SliderBar;
			control.Label       = "#str_swf_vert_sens";
			idLog.Warning("TODO: control->SetDataSource( &gamepadData, idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_VERT_SENS );");
			control.SetupEvents(2, _options.Children.Length);
			control.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) GamePadMenuCommands.VerticalSensitivity);
			control.RegisterEventObserver(helpWidget);
			
			_options.AddChild(control);

			control             = new idMenuWidget_ControlButton();
			control.OptionType  = MenuOptionType.SliderToggle;
			control.Label       = "#str_swf_joy_gammaLook";
			idLog.Warning("TODO: control->SetDataSource( &gamepadData, idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_ACCELERATION );");
			control.SetupEvents(GameConstants.DefaultRepeatTime, _options.Children.Length);
			control.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) GamePadMenuCommands.Acceleration);
			control.RegisterEventObserver(helpWidget);
			
			_options.AddChild(control);
			
			control             = new idMenuWidget_ControlButton();
			control.OptionType  = MenuOptionType.SliderToggle;
			control.Label       = "#str_swf_joy_mergedThreshold";
			idLog.Warning("TODO: control->SetDataSource( &gamepadData, idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_THRESHOLD );");
			control.SetupEvents(GameConstants.DefaultRepeatTime, _options.Children.Length);
			control.AddEventAction(WidgetEventType.Press).Set(WidgetActionType.Command, (int) GamePadMenuCommands.Threshold);
			control.RegisterEventObserver(helpWidget);
			
			_options.AddChild(control);

			_options.AddEventAction(WidgetEventType.ScrollDown).Set(new idWidgetActionHandler(_options,                 WidgetActionType.ScrollDownStartRepeater, WidgetEventType.ScrollDown));
			_options.AddEventAction(WidgetEventType.ScrollUp).Set(new idWidgetActionHandler(_options,                   WidgetActionType.ScrollUpStartRepeater,   WidgetEventType.ScrollUp));
			_options.AddEventAction(WidgetEventType.ScrollDownRelease).Set(new idWidgetActionHandler(_options,          WidgetActionType.StopRepeater,            WidgetEventType.ScrollDownRelease));
			_options.AddEventAction(WidgetEventType.ScrollUpRelease).Set(new idWidgetActionHandler(_options,            WidgetActionType.StopRepeater,            WidgetEventType.ScrollUpRelease));
			_options.AddEventAction(WidgetEventType.ScrollLeftStickDown).Set(new idWidgetActionHandler(_options,        WidgetActionType.ScrollDownStartRepeater, WidgetEventType.ScrollLeftStickDown));
			_options.AddEventAction(WidgetEventType.ScrollLeftStickUp).Set(new idWidgetActionHandler(_options,          WidgetActionType.ScrollUpStartRepeater,   WidgetEventType.ScrollLeftStickUp));
			_options.AddEventAction(WidgetEventType.ScrollLeftStickDownRelease).Set(new idWidgetActionHandler(_options, WidgetActionType.StopRepeater,            WidgetEventType.ScrollLeftStickDownRelease));
			_options.AddEventAction(WidgetEventType.ScrollLeftStickUpRelease).Set(new idWidgetActionHandler(_options,   WidgetActionType.StopRepeater,            WidgetEventType.ScrollLeftStickUpRelease));
		}

		public override void ShowScreen(MainMenuTransition transitionType)
		{
			idLog.Warning("TODO: gamepadData.LoadData();");

			base.ShowScreen(transitionType);
		}

		public override bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			if(_menuData == null)
			{
				return true;
			}

			if(_menuData.ActiveScreen != ShellArea.GamePad)
			{
				return false;
			}

			idLog.Warning("TODO: Shell_GamePad handle action");

			/*widgetAction_t actionType = action.GetType();
			const idSWFParmList & parms = action.GetParms();

			switch ( actionType ) {
				case WIDGET_ACTION_GO_BACK: {
					menuData->SetNextScreen( SHELL_AREA_CONTROLS, MENU_TRANSITION_SIMPLE );
					return true;
				}
				case WIDGET_ACTION_COMMAND: {

					if ( options == NULL ) {
						return true;
					}

					int selectionIndex = options->GetFocusIndex();
					if ( parms.Num() > 0 ) {
						selectionIndex = parms[0].ToInteger();
					}

					if ( selectionIndex != options->GetFocusIndex() ) {
						options->SetViewIndex( options->GetViewOffset() + selectionIndex );
						options->SetFocusIndex( selectionIndex );
					}

					switch ( parms[0].ToInteger() ) {
		#ifndef ID_PC
						case GAMEPAD_CMD_CONFIG: {
							menuData->SetNextScreen( SHELL_AREA_CONTROLLER_LAYOUT, MENU_TRANSITION_SIMPLE );
							break;
						}
		#endif
						case GAMEPAD_CMD_INVERT: {
							gamepadData.AdjustField( idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_INVERT, 1 );
							options->Update();
							break;
						}
						case GAMEPAD_CMD_LEFTY: {
							gamepadData.AdjustField( idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_LEFTY, 1 );
							options->Update();
							break;
						}
						case GAMEPAD_CMD_VIBRATE: {
							gamepadData.AdjustField( idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_VIBRATE, 1 );
							options->Update();
							break;
						}
						case GAMEPAD_CMD_HOR_SENS: {
							gamepadData.AdjustField( idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_HOR_SENS, 1 );
							options->Update();
							break;
						}
						case GAMEPAD_CMD_VERT_SENS: {
							gamepadData.AdjustField( idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_VERT_SENS, 1 );
							options->Update();
							break;
						}
						case GAMEPAD_CMD_ACCELERATION: {
							gamepadData.AdjustField( idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_ACCELERATION, 1 );
							options->Update();
							break;
						}
						case GAMEPAD_CMD_THRESHOLD: {
							gamepadData.AdjustField( idMenuDataSource_GamepadSettings::GAMEPAD_FIELD_THRESHOLD, 1 );
							options->Update();
							break;
						}
					}

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
			}*/

			return base.HandleAction(action, ev, widget, forceHandled);
		}

		public override void HideScreen(MainMenuTransition transitionType)
		{
			idLog.Warning("TODO: gamepaddata commit");
			/*if ( gamepadData.IsDataChanged() ) {
				gamepadData.CommitData();
			}*/

			if(_menuData != null)
			{
				idMenuHandler_Shell handler = _menuData as idMenuHandler_Shell;

				if(handler != null)
				{
					handler.SetupPCOptions();
				}
			}

			base.HideScreen(transitionType);
		}

		public override void Update()
		{
			if(_menuData != null)
			{
				idMenuWidget_CommandBar cmdBar = _menuData.CommandBar;

				if(cmdBar != null)
				{
					idLog.Warning("TODO: Shell_GamePad update");
			
					/*cmdBar->ClearAllButtons();
					idMenuWidget_CommandBar::buttonInfo_t * buttonInfo;			
					buttonInfo = cmdBar->GetButton( idMenuWidget_CommandBar::BUTTON_JOY2 );
					if ( menuData->GetPlatform() != 2 ) {
						buttonInfo->label = "#str_00395";
					}
					buttonInfo->action.Set( WIDGET_ACTION_GO_BACK );

					buttonInfo = cmdBar->GetButton( idMenuWidget_CommandBar::BUTTON_JOY1 );
					if ( menuData->GetPlatform() != 2 ) {
						buttonInfo->label = "#str_SWF_SELECT";
					}
					buttonInfo->action.Set( WIDGET_ACTION_PRESS_FOCUSED );*/
				}		
			}

			idSWFScriptObject root = this.SWFObject.RootObject;

			if(BindSprite(root) == true)
			{
				idSWFTextInstance heading = this.Sprite.ScriptObject.GetNestedText("info", "txtHeading");

				if(heading != null)
				{
					heading.Text = "#str_swf_gamepad_heading";	// CONTROLS
					heading.SetStrokeInfo(true, 0.75f, 1.75f);
				}

				idSWFSpriteInstance gradient = this.Sprite.ScriptObject.GetNestedSprite("info", "gradient");

				if((gradient != null) && (heading != null))
				{
					gradient.PositionX = heading.TextLength;
				}
			}

			if(_buttonBack != null)
			{
				_buttonBack.BindSprite(root);
			}

			base.Update();
		}
		#endregion
	}

	public enum GamePadMenuCommands
	{
#if !ID_PC
		Configuration,
#endif

		Lefty,
		Invert,
		Vibrate,
		HorizontalSensitivity,
		VerticalSensitivity,
		Acceleration,
		Threshold
	}
}