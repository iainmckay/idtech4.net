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
	public class idMenuWidget_DynamicList : idMenuWidget_List
	{
		#region Properties
		public bool ControlList
		{
			set
			{
				_controlList = value;
			}
		}

		public bool IgnoreColor
		{
			set
			{
				_ignoreColor = value;
			}
		}
		#endregion

		#region Members
		private List<List<string>> _listItemInfo = new List<List<string>>();
		private bool _controlList;
		private bool _ignoreColor;
		#endregion

		#region Constructor
		public idMenuWidget_DynamicList()
			: base()
		{

		}
		#endregion

		#region Methods
		public void Recalculate() 
		{
			idSWF swf = this.SWFObject;

			if(swf == null)
			{
				return;
			}

			idSWFScriptObject root  = swf.RootObject;
			int childCount          = this.Children.Length;

			for(int i = 0; i < childCount; ++i)
			{
				idMenuWidget child = GetChildByIndex(i);
				child.SetSpritePath(this.SpritePath, "info", "list", string.Format("item{0}", i));

				if(child.BindSprite(root) == true)
				{
					child.State = WidgetState.Normal;
					child.Sprite.StopFrame(1);
				}
			}
		}

		public void SetListData(List<List<string>> list)
		{
			_listItemInfo.Clear();

			for(int i = 0; i < list.Count; ++i)
			{
				List<string> values = new List<string>();

				for(int j = 0; j < list[i].Count; ++j)
				{
					values.Add(list[i][j]);
				}

				_listItemInfo.Add(values);
			}
		}
		#endregion

		#region idMenuWidget_List implementation
		#region Properties
		public override int TotalNumberOfOptions
		{
			get 
			{ 
				if(_controlList == true)
				{
					return this.Children.Length;
				}

				return _listItemInfo.Count;
			}
		}
		#endregion

		#region Methods
		public override void Initialize(idMenuHandler data)
		{
			base.Initialize(data);
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

			for(int optionIndex = 0; optionIndex < this.VisibleOptionCount; ++optionIndex)
			{
				if(optionIndex >= this.Children.Length)
				{
					idSWFSpriteInstance item = this.Sprite.ScriptObject.GetNestedSprite(string.Format("item{0}", optionIndex));

					if(item != null)
					{
						item.IsVisible = false;
						continue;
					}
				}

				idMenuWidget child = GetChildByIndex(optionIndex);
				int childIndex     = this.ViewOffset + optionIndex;
				bool shown         = false;

				child.SetSpritePath(this.SpritePath, string.Format("item{0}", optionIndex));

				if(child.BindSprite(root) == true)
				{
					if(optionIndex >= this.TotalNumberOfOptions)
					{
						child.ClearSprite();
						continue;
					}
					else
					{
						//const int controlIndex = GetNumVisibleOptions() - Min( GetNumVisibleOptions(), GetTotalNumberOfOptions() ) + optionIndex;
						shown = PrepareListElement(child, childIndex);
						child.Update();
					}

					if(shown == false)
					{
						child.State = WidgetState.Hidden;
					}
					else
					{
						if(optionIndex == this.FocusIndex)
						{
							child.State = WidgetState.Selecting;
						}
						else
						{
							child.State = WidgetState.Normal;
						}
					}
				}
			}

			idSWFSpriteInstance upSprite = this.Sprite.ScriptObject.GetSprite("upIndicator");

			if(upSprite != null)
			{
				upSprite.IsVisible = (this.ViewOffset > 0);
			}

			idSWFSpriteInstance downSprite = this.Sprite.ScriptObject.GetSprite("downIndicator");

			if(downSprite != null)
			{
				downSprite.IsVisible = ((this.ViewOffset + this.VisibleOptionCount) < this.TotalNumberOfOptions);
			}
		}

		public override bool PrepareListElement(idMenuWidget widget, int childIndex)
		{
 			idLog.Warning("TODO: idMenuWidget_ScoreboardButton scoreboardButton = widget as idMenuWidget_ScoreboardButton;");

			/*if ( sbButton != NULL ) {
				return true;
			}*/

			if(_listItemInfo.Count == 0)
			{
				return true;
			}

			if(childIndex > _listItemInfo.Count)
			{
				return false;
			}

			idMenuWidget_Button button = widget as idMenuWidget_Button;

			if(button != null)
			{
				button.IgnoreColor = _ignoreColor;
				button.SetValues(_listItemInfo[childIndex]);

				if(_listItemInfo[childIndex].Count > 0)
				{
					return true;
				}
			}

			return false;
		}
		#endregion
		#endregion
	}
}