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
	/// <summary>
	/// The nav bar is set up with the main option being at the safe frame line.
	/// </summary>
	public class idMenuWidget_MenuBar : idMenuWidget_DynamicList
	{
		#region Properties
		public float ButtonSpacing
		{
			get
			{
				return _rightSpacer;
			}
			set
			{
				_rightSpacer = value;
			}
		}
		#endregion

		#region Members
		private List<string> _headings = new List<string>();
		private float _totalWidth;
		private float _buttonPosition;
		private float _rightSpacer;
		#endregion

		#region Constructor
		public idMenuWidget_MenuBar() 
			: base()
		{

		}
		#endregion

		#region idMenuWidget_DynamicList implementation
		#region Properties
		public override int TotalNumberOfOptions
		{
			get
			{
				return this.Children.Length;
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

			_totalWidth     = 0.0f;
			_buttonPosition = 0.0f;

			for(int index = 0; index < this.VisibleOptionCount; ++index)
			{
				if(index >= this.Children.Length)
				{
					break;
				}

				if(index != 0) 
				{
					_totalWidth += _rightSpacer;
				}

				idMenuWidget child = GetChildByIndex(index);
				child.SetSpritePath(this.SpritePath, string.Format("btn{0}", index));

				if(child.BindSprite(root) == true)
				{
					PrepareListElement(child, index);
					child.Update();
				}
			}

			// 640 is half the size of our flash files width
			float xPos = 640.0f - (_totalWidth / 2.0f);

			this.Sprite.PositionX = xPos;

			idSWFSpriteInstance backing = this.Sprite.ScriptObject.GetNestedSprite("backing");

			if(backing != null)
			{
				if((_menuData != null) && (_menuData.GetPlatform() != 2))
				{
					backing.IsVisible = false;
				}
				else
				{
					backing.IsVisible = true;
					backing.PositionX = _totalWidth / 2.0f;
				}
			}
		}

		public override bool PrepareListElement(idMenuWidget widget, int childIndex)
		{
			if(childIndex >= this.VisibleOptionCount)
			{
				return false;
			}

			idMenuWidget_MenuButton button = widget as idMenuWidget_MenuButton;

			if((button == null) || (button.Sprite == null))
			{
				return false;
			}

			if(childIndex >= _headings.Count)
			{
				button.Label = "";
			}
			else
			{
				button.Label = _headings[childIndex];

				idSWFTextInstance textInstance = button.Sprite.ScriptObject.GetNestedText("txtVal");

				if(textInstance != null)
				{
					textInstance.SetStrokeInfo(true, 0.7f, 1.25f);
					textInstance.Text = _headings[childIndex];

					button.Position = _buttonPosition;

					_totalWidth     += textInstance.TextLength;
					_buttonPosition += _rightSpacer + textInstance.TextLength;
				}
			}
	
			return true;
		}

		public void SetListHeadings(List<string> list)
		{
			_headings.Clear();

			for(int index = 0; index < list.Count; ++index)
			{
				_headings.Add(list[index]);
			}
		}
		#endregion
		#endregion
	}
}