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

using idTech4.Renderer;
using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// SWF object structure
	/// --------------------
	/// BUTTON (Frames: up, over, out, down, release, disabled, sel_up, sel_over, sel_out, sel_down, sel_release, selecting, unselecting)
	/// 	txtOption
	/// 		txtValue (Text)
	/// 	optionType (Frames: One per mainMenuOption_t enum)
	/// 		sliderBar
	/// 			bar (Frames: 1-100 for percentage filled)
	/// 			btnLess
	/// 			btnMore
	/// 		sliderText
	/// 			txtVal (Text)
	/// 			btnLess
	/// 			btnMore
	/// <para/>
	/// Future work:
	/// - Perhaps this should be called "MultiButton", since it merges additional controls with a standard button?
	/// </remarks>
	public class idMenuWidget_Button : idMenuWidget
	{
		#region Properties
		public ButtonState ButtonState
		{
			get
			{
				return _state;
			}
			set
			{
				_state = value;
			}
		}

		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		public bool IgnoreColor
		{
			get
			{
				return _ignoreColor;
			}
			set
			{
				_ignoreColor = value;
			}
		}

		public idMaterial Image
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
			}
		}

		public string Label
		{
			get
			{
				return _label;
			}
			set
			{
				_label = value;
			}
		}

		public idSWFScriptFunction Press
		{
			get
			{
				return _scriptFunction;
			}
			set
			{
				_scriptFunction = value;
			}
		}
		#endregion

		#region Members
		private List<string> _values = new List<string>();
		private string _label;
		private string _description;
		private ButtonState _state;
		private idMaterial _image;
		private idSWFScriptFunction _scriptFunction;
		private bool _ignoreColor;
		#endregion

		#region Constructor
		public idMenuWidget_Button()
			: base()
		{
			_state = ButtonState.Up;
		}
		#endregion

		#region Methods
		public string GetValue(int index)
		{
			return _values[index];
		}

		public void SetValues(List<string> list)
		{
			_values.Clear();
			_values.AddRange(list);
		}

		/*protected void SetupTransitionInfo(WidgetTransitionType transitionType, WidgetState state, ButtonState sourceState, ButtonState destState)
		{
			idLog.Warning("TODO: SetupTransitionInfo");
			trans.prefixes.Clear();
			if ( buttonState == WIDGET_STATE_DISABLED ) {
				trans.animationName = "disabled";
			} else {
				const int animIndex = (int)destAnimState * ANIM_STATE_MAX + (int)sourceAnimState;
				trans.animationName = ANIM_STATE_TRANSITIONS[ animIndex ];
				if ( buttonState == WIDGET_STATE_SELECTING ) {
					trans.prefixes.Append( "sel_" );
				}
			}
			trans.prefixes.Append( "" );
		}*/

		protected void AnimateToState(ButtonState targetState, bool force = false)
		{
			if((force == false) && (targetState == this.ButtonState))
			{
				return;
			}

			if(this.Sprite != null)
			{
				idLog.Warning("TODO: AnimateToState");

				/*widgetTransition_t trans;
				SetupTransitionInfo( trans, GetState(), GetAnimState(), targetAnimState );
				if ( trans.animationName[0] != '\0' ) {
					for ( int i = 0; i < trans.prefixes.Num(); ++i ) {
						const char * const frameLabel = va( "%s%s", trans.prefixes[ i ], trans.animationName );
						if ( GetSprite()->FrameExists( frameLabel ) ) {
							GetSprite()->PlayFrame( frameLabel );
							Update();
							break;
						}
					}
				}*/

				idSWFSpriteInstance focusSprite = this.Sprite.ScriptObject.GetSprite("focusIndicator");

				if(focusSprite != null)
				{
					if(targetState == Menus.ButtonState.Over)
					{
						focusSprite.PlayFrame("show");
					} 
					else 
					{
						focusSprite.PlayFrame("hide");
					}
				}
			}

			this.ButtonState = targetState;
		}
		#endregion

		#region idMenuWidget implementation
		public override bool ExecuteEvent(idWidgetEvent ev)
		{
 			bool handled = false;

			// do nothing at all if it's disabled
			if(this.State != WidgetState.Disabled)
			{
				switch(ev.Type)
				{
					case WidgetEventType.Press:
						if(this.MenuData != null)
						{
							idLog.Warning("TODO: GetMenuData()->PlaySound( GUI_SOUND_ADVANCE );	");
						}
				
						AnimateToState(ButtonState.Down);
						handled = true;
						break;
			
					case WidgetEventType.Release:
						AnimateToState(ButtonState.Up);
						idLog.Warning("TODO: GetMenuData()->ClearWidgetActionRepeater();");
						handled = true;
						break;
			
					case WidgetEventType.RollOver:
						if(this.MenuData != null)
						{
							idLog.Warning("TODO: GetMenuData()->PlaySound( GUI_SOUND_ROLL_OVER );");
						}
				
						AnimateToState(ButtonState.Over);
						handled = true;
						break;
			
					case WidgetEventType.RollOut:
						AnimateToState(ButtonState.Up);
						idLog.Warning("TODO: GetMenuData()->ClearWidgetActionRepeater();");
						handled = true;
						break;
			
					case WidgetEventType.FocusOff:
						this.State = WidgetState.Normal;
						handled = true;
						break;
			
					case WidgetEventType.FocusOn:
						this.State = WidgetState.Selecting;
						handled = true;
						break;
			
					case WidgetEventType.ScrollLeftRelease:
						idLog.Warning("TODO: GetMenuData()->ClearWidgetActionRepeater();");
						break;
			
					case WidgetEventType.ScrollRightRelease:
						idLog.Warning("TODO: GetMenuData()->ClearWidgetActionRepeater();");
						break;
				}
			}

			base.ExecuteEvent(ev);

			return handled;
		}
	
		public override void Update()
		{
			if((this.MenuData != null) && (this.MenuData.UserInterface != null))
			{
				BindSprite(this.MenuData.UserInterface.RootObject);
			}

			if(this.Sprite == null)
			{
				return;
			}

			idSWFScriptObject spriteObject = this.Sprite.ScriptObject;

			if(string.IsNullOrEmpty(_label) == true)
			{
				if(_values.Count > 0)
				{
					for(int val = 0; val < _values.Count; ++val)
					{
						idSWFScriptObject textObject = spriteObject.GetNestedObject(string.Format("label{0}", val), "txtVal");

						if(textObject != null)
						{
							idSWFTextInstance text = textObject.Text;
							text.IgnoreColor       = _ignoreColor;
							text.IsTooltip         = _ignoreColor; // ignoreColor does double duty as "allow tooltips"
							text.Text              = _values[val];
							text.SetStrokeInfo(true, 0.75f, 2.0f);
						}
					}
				} 
				else if(_image != null)
				{
					idSWFSpriteInstance buttonImage = spriteObject.GetNestedSprite("img");

					if(buttonImage != null)
					{
						buttonImage.SetMaterial(_image);
					}

					buttonImage = spriteObject.GetNestedSprite("imgTop");

					if(buttonImage != null)
					{
						buttonImage.SetMaterial(_image);
					}
				} 
				else 
				{
					ClearSprite();
				}
			} 
			else 
			{
				idSWFScriptObject textObject = spriteObject.GetNestedObject("label0", "txtVal");

				if(textObject != null)
				{
					idSWFTextInstance text = textObject.Text;
					text.IgnoreColor       = _ignoreColor;
					text.IsTooltip         = _ignoreColor; // ignoreColor does double duty as "allow tooltips"
					text.Text              = _label;
					text.SetStrokeInfo(true, 0.75f, 2.0f);
				}
			}

			// events
			spriteObject.Set("onPress",   new idWrapWidgetEvent(this, WidgetEventType.Press, 0));
			spriteObject.Set("onRelease", new idWrapWidgetEvent(this, WidgetEventType.Release, 0));

			idSWFScriptObject hitBox = spriteObject.GetObject("hitBox");
			
			if(hitBox == null) 
			{
				hitBox = spriteObject;
			}

			hitBox.Set("onRollOver", new idWrapWidgetEvent(this, WidgetEventType.RollOver, 0));
			hitBox.Set("onRollOut",  new idWrapWidgetEvent(this, WidgetEventType.RollOut, 0));
		}
		#endregion
	}

	public enum ButtonState
	{
		Up,
		Down,
		Over
	}
}