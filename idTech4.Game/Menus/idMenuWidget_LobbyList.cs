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

using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	public class idMenuWidget_LobbyList : idMenuWidget_List
	{
		#region Properties
		public int EntryCount
		{
			get
			{
				return _entryCount;
			}
			set
			{
				_entryCount = value;
			}
		}
		#endregion

		#region Members
		private int _entryCount;
		private List<string> _headings = new List<string>();
		#endregion

		#region Constructor
		public idMenuWidget_LobbyList()
			: base()
		{

		}
		#endregion
	  
		#region idMenuWidget_LobbyList implementation
		#region Properties
		public override int TotalNumberOfOptions
		{
			get
			{
				return _entryCount;
			}
		}
		#endregion

		#region Methods
		public override bool PrepareListElement(idMenuWidget widget, int childIndex)
		{
			idMenuWidget_LobbyButton button = widget as idMenuWidget_LobbyButton;
	
			if(button == null) 
			{
				return false;
			}

			if(button.IsValid == false)
			{
				return false;
			}

			return true;
		}

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

			for(int i = 0; i < _headings.Count; ++i)
			{
				idSWFTextInstance txtHeading = this.Sprite.ScriptObject.GetNestedText(string.Format("heading{0}", i));

				if(txtHeading != null)
				{
					txtHeading.Text = _headings[i];
					txtHeading.SetStrokeInfo(true, 0.75f, 1.75f);
				}
			}

			for(int optionIndex = 0; optionIndex < this.VisibleOptionCount; ++optionIndex)
			{
				bool shown = false;

				if(optionIndex < this.Children.Length)
				{
					idMenuWidget child = GetChildByIndex(optionIndex);
					child.SetSpritePath(this.SpritePath, string.Format("item{0}", optionIndex));

					if(child.BindSprite(root) == true)
					{
						shown = PrepareListElement(child, optionIndex);

						if(shown == true)
						{
							child.Sprite.IsVisible = true;
							child.Update();
						}
						else
						{
							child.Sprite.IsVisible = false;
						}
					}
				}
			}
		}
		#endregion
		#endregion

	}
}