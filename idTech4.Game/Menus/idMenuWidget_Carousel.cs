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
using idTech4.Renderer;
using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

namespace idTech4.Game.Menus
{
	/// <summary>
	/// Displays a list of items in a looping carousel pattern.
	/// </summary>
	public class idMenuWidget_Carousel : idMenuWidget
	{
		#region Properties
		public idMaterial[] Images
		{
			get
			{
				return _imageList;
			}
			set
			{
				_imageList = (idMaterial[]) value.Clone();
			}
		}

		public int MoveDiff
		{
			get
			{
				return _moveDiff;
			}
			set
			{
				_moveDiff = value;
			}
		}

		public int MoveToIndexTarget
		{
			get
			{
				return _moveToIndex;
			}
			set
			{
				_moveToIndex = value;
			}
		}

		public bool ScrollLeft
		{
			get
			{
				return _scrollLeft;
			}
		}

		public int TotalNumberOfOptions
		{
			get
			{
				if(_imageList == null)
				{
					return 0;
				}

				return _imageList.Length;
			}
		}

		public int ViewIndex
		{
			get
			{
				return _viewIndex;
			}
			set
			{
				_viewIndex = value;
			}
		}

		public int VisibleOptionCount
		{
			get
			{
				return _visibleOptionCount;
			}
			set
			{
				_visibleOptionCount = value;
			}
		}
		#endregion	

		#region Members
		private int _visibleOptionCount;
		private int _viewIndex;
		private int _moveToIndex;
		private int	_moveDiff;
		private bool _fastScroll;
		private bool _scrollLeft;
		private idMaterial[] _imageList;
		#endregion

		#region Constructor
		public idMenuWidget_Carousel()
			: base()
		{
				
		}
		#endregion

		#region Methods
		public bool PrepareListElement(idMenuWidget widget, int childIndex)
		{
			return true;
		}

		public void MoveToIndex(int index, bool instant = false)
		{
			if(instant == true)
			{
				_viewIndex   = index;
				_moveDiff    = 0;
				_moveToIndex = _viewIndex;

				idSWFScriptObject root = this.SWFObject.RootObject;

				if(BindSprite(root) == true)
				{
					this.Sprite.StopFrame(1);
				}

				Update();
			}
			else
			{
				if(index == 0)
				{
					_fastScroll = false;
					_moveDiff   = 0;
					_viewIndex  = _moveToIndex;
				}
				else
				{
					int midPoint = this.VisibleOptionCount / 2;

					_scrollLeft = false;

					if(index > midPoint)
					{
						_moveDiff   = index - midPoint;
						_scrollLeft = true;
					}
					else
					{
						_moveDiff = index;
					}

					if(_scrollLeft == true)
					{
						_moveToIndex = _viewIndex - _moveDiff;

						if(_moveToIndex < 0)
						{
							_moveToIndex = 0;
							_moveDiff   -= (0 - _moveToIndex);
						}
					}
					else
					{
						_moveToIndex = _viewIndex + _moveDiff;

						if(_moveToIndex >= this.TotalNumberOfOptions)
						{
							_moveDiff    = this.TotalNumberOfOptions - this.ViewIndex - 1;
							_moveToIndex = this.TotalNumberOfOptions - 1;
						}
					}

					if(_moveDiff != 0)
					{
						if(_moveDiff > 1)
						{
							if(_scrollLeft == true)
							{
								this.Sprite.PlayFrame("leftFast");
							}
							else
							{
								this.Sprite.PlayFrame("rightFast");
							}
						}
						else
						{
							if(_scrollLeft == true)
							{
								this.Sprite.PlayFrame("left");
							}
							else
							{
								this.Sprite.PlayFrame("right");
							}
						}
					}
				}
			}
		}

		public void MoveToFirstItem(bool instant = true)
		{
			if(instant == true)
			{
				_moveDiff    = 0;
				_viewIndex   = 0;
				_moveToIndex = 0;

				idSWFScriptObject root = this.SWFObject.RootObject;

				if(BindSprite(root) == true)
				{
					this.Sprite.StopFrame(1);
				}

				Update();
			}
		}

		public void MoveToLastItem(bool instant = true)
		{
			if(instant == true) 
			{
				_moveDiff    = 0;
				_viewIndex   = this.TotalNumberOfOptions - 1;
				_moveToIndex = this.TotalNumberOfOptions - 1;

				idSWFScriptObject root = this.SWFObject.RootObject;
					
				if(BindSprite(root) == true) 
				{
					this.Sprite.StopFrame(1);
				}
		
				Update();
			}
		}
		#endregion

		#region idMenuWidget implementation
		public override void Initialize(idMenuHandler data)
		{
			base.Initialize(data);

			if(this.SWFObject != null) 
			{
				this.SWFObject.SetGlobal("refreshCarousel", new idCarouselRefresh(this));
			}
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

			int midPoint = (this.VisibleOptionCount / 2) + 1;

			for(int optionIndex = 0; optionIndex < this.VisibleOptionCount; ++optionIndex)
			{
				int listIndex = _viewIndex + optionIndex;

				if(optionIndex >= midPoint) 
				{
					listIndex = _viewIndex - (optionIndex - (midPoint - 1));
				}

				idMenuWidget child = GetChildByIndex(optionIndex);
				child.SetSpritePath(this.SpritePath, string.Format("item{0}", optionIndex));

				if(child.BindSprite(root) == true) 
				{
					if((listIndex < 0) || (listIndex >= this.TotalNumberOfOptions))
					{
						child.State = WidgetState.Hidden;
					} 
					else 
					{
						idMenuWidget_Button button = (idMenuWidget_Button) child;
						button.Image               = _imageList[listIndex];

						child.Update();

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
		}

		public override bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			 return base.HandleAction(action, ev, widget, forceHandled);
		}
		#endregion

		#region Carousel
		private class idCarouselRefresh : idSWFScriptFunction
		{
			#region Members
			private idMenuWidget_Carousel _widget;
			#endregion

			#region Constructor
			public idCarouselRefresh(idMenuWidget_Carousel widget)
			{
				_widget = widget;
			}
			#endregion

			#region idSWFScriptFunction implementation
			public override idSWFScriptVariable Invoke(idSWFScriptObject scriptObj, idSWFParameterList parms)
			{
				if(_widget == null)
				{
					return new idSWFScriptVariable();
				}

				if(_widget.MoveDiff != 0)
				{
					int diff = _widget.MoveDiff;
					diff--;

					if(_widget.ScrollLeft == true)
					{
						_widget.ViewIndex = _widget.ViewIndex - 1;
					}
					else
					{
						_widget.ViewIndex = _widget.ViewIndex + 1;
					}

					if(diff > 0)
					{
						if(_widget.ScrollLeft == true)
						{
							_widget.MoveToIndexTarget = (_widget.VisibleOptionCount / 2) + diff;
						}
						else
						{
							_widget.MoveToIndexTarget = diff;
						}
					}
					else
					{
						_widget.MoveDiff = 0;
					}
				}

				_widget.Update();

				return new idSWFScriptVariable();
			}
			#endregion
		}
		#endregion
	}
}