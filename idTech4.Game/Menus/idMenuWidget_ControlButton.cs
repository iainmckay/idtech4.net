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
using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public class idMenuWidget_ControlButton : idMenuWidget_Button
	{
		#region Constants
		// script name for the control object for a given type of button
		private readonly string[] ControlSpriteNames = {
			null,
			"sliderBar",
			"sliderText", 
			"sliderText",
			null,
			"sliderText",
		};
		#endregion

		#region Properties
		public bool IsDisabled
		{
			get
			{
				return _disabled;
			}
			set
			{
				_disabled = value;
			}
		}

		public MenuOptionType OptionType
		{
			get
			{
				return _optionType;
			}
			set
			{
				_optionType = value;
			}
		}
		#endregion

		#region Members
		private MenuOptionType _optionType;
		private bool _disabled;
		#endregion

		#region Constructor
		public idMenuWidget_ControlButton() 
			: base()
		{
			_optionType = MenuOptionType.ButtonText;
		}
		#endregion

		#region Methods
		public void SetupEvents(int delay, int index)
		{
			AddEventAction(WidgetEventType.ScrollLeft).Set(WidgetActionType.StartRepeater, (int) WidgetActionType.AdjustField, -1, delay, index );
			AddEventAction(WidgetEventType.ScrollRight).Set(WidgetActionType.StartRepeater, (int) WidgetActionType.AdjustField, 1, delay, index);
			AddEventAction(WidgetEventType.ScrollLeftRelease).Set(WidgetActionType.StopRepeater);
			AddEventAction(WidgetEventType.ScrollRightRelease).Set(WidgetActionType.StopRepeater);
			AddEventAction(WidgetEventType.ScrollLeftStickLeft).Set(WidgetActionType.StartRepeater, (int) WidgetActionType.AdjustField, -1, delay, index);
			AddEventAction(WidgetEventType.ScrollLeftStickRight).Set(WidgetActionType.StartRepeater, (int) WidgetActionType.AdjustField, 1, delay, index);
			AddEventAction(WidgetEventType.ScrollLeftStickLeftRelease).Set(WidgetActionType.StopRepeater);
			AddEventAction(WidgetEventType.ScrollLeftStickRightRelease).Set(WidgetActionType.StopRepeater);
		}
		#endregion

		#region idMenuWidget_Button implementation
		public override void Update()
		{
			if(this.Sprite == null)
			{
				return;
			}

			idSWFScriptObject spriteObject = this.Sprite.ScriptObject.GetNestedObject("type");

			if(spriteObject == null)
			{
				return;
			}

			idSWFSpriteInstance type = spriteObject.Sprite;

			if(type == null)
			{
				return;
			}

			if(this.OptionType != MenuOptionType.ButtonFullTextSlider)
			{
				type.StopFrame((int) this.OptionType + 1);
			}

			idSWFTextInstance text = spriteObject.GetNestedText("label0", "txtVal");

			if(text != null)
			{
				text.Text = this.Label;
				text.SetStrokeInfo(true, 0.75f, 2.0f);
			}

			if(ControlSpriteNames[(int) this.OptionType] != null) 
			{
				idSWFSpriteInstance controlSprite = null;

				if(ControlSpriteNames[(int) this.OptionType] != null)
				{
					controlSprite = type.ScriptObject.GetSprite(ControlSpriteNames[(int) this.OptionType]);
					
					if(controlSprite != null)
					{
						if(this.DataSource != null)
						{
							idSWFScriptVariable fieldValue = this.DataSource.GetField(this.DataSourceFieldIndex);

							if(this.OptionType == MenuOptionType.SliderBar)
							{
								controlSprite.StopFrame(1 + fieldValue.ToInt32());
							}
							else if(this.OptionType == MenuOptionType.SliderToggle)
							{
								idSWFTextInstance txtInfo = controlSprite.ScriptObject.GetNestedText("txtVal");

								if(txtInfo != null)
								{
									txtInfo.Text = fieldValue.ToBool() ? "#str_swf_enabled" : "#str_swf_disabled";
									txtInfo.SetStrokeInfo(true, 0.75f, 2.0f);
								}
							} 
							else if((this.OptionType == MenuOptionType.SliderText) || (this.OptionType == MenuOptionType.ButtonFullTextSlider))
							{
								idSWFTextInstance txtInfo = controlSprite.ScriptObject.GetNestedText("txtVal");

								if(txtInfo != null)
								{
									txtInfo.Text = fieldValue.ToString();
									txtInfo.SetStrokeInfo(true, 0.75f, 2.0f);
								}
							}
						}				

						idSWFScriptObject btnLess = this.Sprite.ScriptObject.GetObject("btnLess");
						idSWFScriptObject btnMore = this.Sprite.ScriptObject.GetObject("btnMore");

						if((btnLess != null) && (btnMore != null))
						{
							if(_disabled == true)
							{
								btnLess.Sprite.IsVisible = false;
								btnMore.Sprite.IsVisible = false;
							} 
							else 
							{
								btnLess.Sprite.IsVisible = true;
								btnMore.Sprite.IsVisible = true;

								btnLess.Set("onPress",    new idWrapWidgetEvent(this, WidgetEventType.ScrollLeft, 0));
								btnLess.Set("onRelease",  new idWrapWidgetEvent(this, WidgetEventType.ScrollLeftRelease, 0));

								btnMore.Set("onPress",    new idWrapWidgetEvent(this, WidgetEventType.ScrollRight, 0));
								btnMore.Set("onRelease",  new idWrapWidgetEvent(this, WidgetEventType.ScrollRightRelease, 0));

								btnLess.Set("onRollOver", new idWrapWidgetEvent(this, WidgetEventType.RollOver, 0));
								btnLess.Set("onRollOut",  new idWrapWidgetEvent(this, WidgetEventType.RollOut, 0));

								btnMore.Set("onRollOver", new idWrapWidgetEvent(this, WidgetEventType.RollOver, 0));
								btnMore.Set("onRollOut",  new idWrapWidgetEvent(this, WidgetEventType.RollOut, 0));
							}
						}
					}
				}
			} 
			else 
			{
				idSWFScriptObject btnLess = this.Sprite.ScriptObject.GetObject("btnLess");
				idSWFScriptObject btnMore = this.Sprite.ScriptObject.GetObject("btnMore");

				if((btnLess != null) && (btnMore != null))
				{
					btnLess.Sprite.IsVisible = false;
					btnMore.Sprite.IsVisible = false;
				}
			}

			// events
			this.Sprite.ScriptObject.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.Press, 0));
			this.Sprite.ScriptObject.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.Release, 0));

			idSWFScriptObject hitBox = this.Sprite.ScriptObject.GetObject("hitBox");

			if(hitBox == null) 
			{
				hitBox = this.Sprite.ScriptObject;
			}

			hitBox.Set("onRollOver", new idWrapWidgetEvent(this, WidgetEventType.RollOver, 0));
			hitBox.Set("onRollOut",  new idWrapWidgetEvent(this, WidgetEventType.RollOut, 0));

		}
		#endregion
	}
}