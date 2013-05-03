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
	/// <summary>
	/// Shows a help tooltip message based on observed events.  
	/// </summary>
	/// <remarks>
	/// It's expected that the widgets being observed are all buttons, and therefore have a GetDescription() 
	/// call to get the help message.
	/// <para/>
	/// SWF object structure
	/// --------------------
	/// HELPTOOLTIP (Frames: shown, shown, hide, hidden)
	/// 	txtOption
	/// 		txtValue (Text)
	/// Note: Frame 1 should, effectively, be a "hidden" state.
	/// <para/>
	/// Future work:
	/// - Make this act more like a help tooltip when on PC?
	/// </remarks>
	public class idMenuWidget_Help : idMenuWidget
	{
		#region Members
		private string _lastFocusedMessage;		// message from last widget that had focus
		private string _lastHoveredMessage;		// message from last widget that was hovered over
		private bool _hideMessage;
		#endregion

		#region Constructor
		public idMenuWidget_Help()
			: base()
		{

		}
		#endregion

		#region Methods
		private void Observe_FocusOn(idMenuWidget widget, idWidgetEvent ev, idMenuWidget_Button button)
		{
			_hideMessage        = false;
			_lastFocusedMessage = button.Description;
			_lastHoveredMessage = string.Empty;
			
			Update();
		}

		private void Observe_RollOut(idMenuWidget widget, idWidgetEvent ev, idMenuWidget_Button button)
		{
			_hideMessage        = false;
			_lastHoveredMessage = string.Empty;
			
			Update();
		}

		private void Observe_RollOver(idMenuWidget widget, idWidgetEvent ev, idMenuWidget_Button button)
		{
			string desc = button.Description;

			if(desc == string.Empty)
			{
				_hideMessage = true;
			}
			else
			{
				_hideMessage        = false;
				_lastHoveredMessage = button.Description;
			}

			Update();
		}
		#endregion

		#region idMenuWidget implementation
		public override void Update()
		{
			if(this.SWFObject == null)
			{
				return;
			}

			idSWFScriptObject root = this.SWFObject.RootObject;

			if(BindSprite(root) == false)
			{
				return;
			}

			string msg = (_lastHoveredMessage.Length > 0) ? _lastHoveredMessage : _lastFocusedMessage;

			if((string.IsNullOrEmpty(msg) == false) && (_hideMessage == false))
			{
				// try to show it if...
				//		- we are on the first frame
				//		- we aren't still animating while being between the "show" and "shown" frames
				// 
				if((this.Sprite.CurrentFrame != this.Sprite.FindFrame("shown"))
					&& ((this.Sprite.CurrentFrame == 1) || ((this.Sprite.IsPlaying == true) && (this.Sprite.IsBetweenFrames("shown", "shown"))) == false))
				{
					this.Sprite.PlayFrame("show");
				}

				idSWFScriptObject textObject = this.Sprite.ScriptObject.GetNestedObject("txtOption", "txtValue");

				if(textObject != null)
				{
					idSWFTextInstance text = textObject.Text;
					text.Text = msg;
					text.SetStrokeInfo(true, 0.75f, 2.0f);
				}
			} 
			else 
			{
				// try to hide it
				if((this.Sprite.CurrentFrame != 1)
					&& (this.Sprite.CurrentFrame != this.Sprite.FindFrame("hidden"))
					&& (this.Sprite.IsBetweenFrames("hide", "hidden") == false)) 
				{
					this.Sprite.PlayFrame("hide");
				}
			}
		}

		public override void ObserveEvent(idMenuWidget widget, idWidgetEvent ev)
		{
			idMenuWidget_Button button = widget as idMenuWidget_Button;

			if(button == null)
			{
				return;
			}

			switch(ev.Type)
			{
				case WidgetEventType.FocusOn:
					Observe_FocusOn(widget, ev, button);
					break;

				case WidgetEventType.FocusOff:
					// Don't do anything when losing focus. Focus updates come in pairs, so we can
					// skip doing anything on the "lost focus" event, and instead do updates on the
					// "got focus" event.
					break;
		
				case WidgetEventType.RollOver:
					Observe_RollOver(widget, ev, button);
					break;
		
				case WidgetEventType.RollOut:
					Observe_RollOut(widget, ev, button);
					break;
			}
		}
		#endregion
	}
}