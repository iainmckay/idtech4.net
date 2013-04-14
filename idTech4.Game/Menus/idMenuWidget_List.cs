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
using System.Diagnostics;

using idTech4.UI.SWF;
using idTech4.UI.SWF.Scripting;

using XMath = System.Math;

namespace idTech4.Game.Menus
{
	/// <summary>
	/// Provides a paged view of this widgets children.
	/// </summary>
	/// <remarks>
	/// Each child is expected to take on the following naming scheme.  Children outside of the given 
	/// window size (NumVisibleOptions) are not rendered, and will affect which type of arrow indicators are shown.
	/// <para/>
	/// Future work:
	/// - Make upIndicator another kind of widget (Image widget?)
	/// </summary>
	public class idMenuWidget_List : idMenuWidget
	{
		#region Properties
		public bool IsWrappingAllowed
		{
			get
			{
				return _allowWrapping;
			}
			set
			{
				_allowWrapping = value;
			}
		}

		public virtual int TotalNumberOfOptions
		{
			get
			{
				return this.Children.Length;
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

		public int ViewOffset
		{
			get
			{
				return _viewOffset;
			}
			set
			{
				_viewOffset = value;
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
		#endregion

		#region Members
		private int	_visibleOptionCount;
		private int	_viewOffset;
		private int	_viewIndex;
		private bool _allowWrapping;
		#endregion

		#region Constructor
		public idMenuWidget_List() 
			: base()
		{

		}
		#endregion

		#region Methods
		/// <summary>
		/// Pure functional encapsulation of how to calculate a new index and offset based on how the user chose to move through the list.
		/// </summary>
		/// <param name="outIndex"></param>
		/// <param name="outOffset"></param>
		/// <param name="currentIndex"></param>
		/// <param name="currentOffset"></param>
		/// <param name="windowSize"></param>
		/// <param name="maxSize"></param>
		/// <param name="indexDelta"></param>
		/// <param name="allowWrapping"></param>
		/// <param name="wrapAround"></param>
		public void CalculatePositionFromIndexDelta(out int outIndex, out int outOffset, int currentIndex, int currentOffset, int windowSize, int maxSize, int indexDelta, bool allowWrapping, bool wrapAround)
		{
			Debug.Assert(indexDelta != 0);
				
			int newIndex = currentIndex + indexDelta;
			bool wrapped = false;

			if(indexDelta > 0)
			{
				// moving down the list
				if(newIndex > (maxSize - 1)) 
				{
					if(allowWrapping == true) 
					{
						if(wrapAround == true) 
						{
							wrapped  = true;
							newIndex = 0 + (newIndex - maxSize);
						} 
						else 
						{
							newIndex = 0;
						}
					} 
					else 
					{
						newIndex = maxSize - 1;
					}
				}
			} 
			else 
			{
				// moving up the list
				if(newIndex < 0) 
				{
					if(allowWrapping == true) 
					{
						if(wrapAround == true) 
						{
							newIndex = maxSize + newIndex;
						} 
						else
						{
							newIndex = maxSize - 1;
						}
					}
					else 
					{
						newIndex = 0;
					}
				}
			}

			// calculate the offset
			if((newIndex - currentOffset) >= windowSize) 
			{
				outOffset = newIndex - windowSize + 1;
			} 
			else if(currentOffset > newIndex) 
			{
				if(wrapped == true) 
				{
					outOffset = 0;
				} 
				else 
				{
					outOffset = newIndex;
				}
			} 
			else 
			{
				outOffset = currentOffset;
			}

			outIndex = newIndex;

			// the intended behavior is that outOffset and outIndex are always within maxSize of each
			// other, as they are meant to model a window of items that should be visible in the list.
			Debug.Assert((outIndex - outOffset) < windowSize);
			Debug.Assert((outIndex >= outOffset) && (outIndex >= 0) && (outOffset >= 0));
		}

		public void CalculatePositionFromOffsetDelta(out int outIndex, out int outOffset, int currentIndex, int currentOffset, int windowSize, int maxSize, int offsetDelta)
		{
			// shouldn't be setting both indexDelta AND offsetDelta
			// FIXME: make this simpler code - just pass a boolean to control it?
			Debug.Assert(offsetDelta != 0);
	
			int newOffset = XMath.Max(currentIndex + offsetDelta, 0);
		
			if(newOffset >= maxSize) 
			{
				// scrolling past the end - just scroll all the way to the end
				outIndex   = maxSize - 1;
				outOffset  = XMath.Max(maxSize - windowSize, 0);
			} 
			else if(newOffset >= (maxSize - windowSize)) 
			{
				// scrolled to the last window
				outIndex  = newOffset;
				outOffset = XMath.Max(maxSize - windowSize, 0);
			} 
			else 
			{
				outIndex = outOffset = newOffset;
			}

			// the intended behavior is that outOffset and outIndex are always within maxSize of each
			// other, as they are meant to model a window of items that should be visible in the list.
			Debug.Assert((outIndex - outOffset) < windowSize);
			Debug.Assert((outIndex >= outOffset) && (outIndex >= 0) && (outOffset >= 0));
		}

		public void Scroll(int scrollAmount, bool wrapAround = false) 
		{
			if(this.TotalNumberOfOptions == 0)
			{
				return;
			}

			int newIndex, newOffset;

			CalculatePositionFromIndexDelta(out newIndex, out newOffset, this.ViewIndex, this.ViewOffset, this.VisibleOptionCount, this.TotalNumberOfOptions, scrollAmount, this.IsWrappingAllowed, wrapAround);
	
			if(newOffset != this.ViewOffset) 
			{
				this.ViewOffset = newOffset;

				if(_menuData != null)
				{
					idLog.Warning("TODO: menuData->PlaySound( GUI_SOUND_FOCUS );");
				}

				Update();
			}

			if(newIndex != this.ViewIndex) 
			{
				this.ViewIndex = newIndex;
				this.SetFocusIndex(newIndex - newOffset);
			}
		}

		public void ScrollOffset(int scrollAmount)
		{
			if(this.TotalNumberOfOptions == 0)
			{
				return;
			}
			
			int newIndex, newOffset;

			CalculatePositionFromOffsetDelta(out newIndex, out newOffset, this.ViewIndex, this.ViewOffset, this.VisibleOptionCount, this.TotalNumberOfOptions, scrollAmount);
	
			if(newOffset != this.ViewOffset) 
			{
				this.ViewOffset = newOffset;
		
				Update();
			}

			if(newIndex != this.ViewIndex)
			{
				this.ViewIndex  = newIndex;
				this.SetFocusIndex(newIndex - newOffset);
			}
		}

		public virtual bool PrepareListElement(idMenuWidget widget, int childIndex)
		{
			return true;
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

			for(int optionIndex = 0; optionIndex < this.VisibleOptionCount; ++optionIndex)
			{
				int childIndex = this.ViewOffset + optionIndex;
				bool shown     = false;

				if(optionIndex < this.Children.Length)
				{
					idMenuWidget child = GetChildByIndex(optionIndex);
					int controlIndex   = this.VisibleOptionCount - XMath.Min(this.VisibleOptionCount, this.TotalNumberOfOptions) + optionIndex;

					child.SetSpritePath(this.SpritePath, string.Format("item{0}", controlIndex));

					if(child.BindSprite(root) == true)
					{
						PrepareListElement(child, childIndex);
						child.Update();
						shown = true;
					}
				}

				if(shown == false)
				{
					// hide the item
					idSWFSpriteInstance sprite = this.Sprite.ScriptObject.GetSprite(string.Format("item{0}", optionIndex - this.TotalNumberOfOptions));

					if(sprite != null)
					{
						sprite.IsVisible = false;
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

		public override bool HandleAction(idWidgetAction action, idWidgetEvent ev, idMenuWidget widget, bool forceHandled = false)
		{
			idSWFParameterList parms = action.Parameters;

			if(action.Type == WidgetActionType.ScrollVertical)
			{
				ScrollType scrollType = (ScrollType) ev.Argument;
				
				if(scrollType == ScrollType.Single)
				{
					Scroll(parms[0].ToInt32());
				}
				else if(scrollType == ScrollType.Page)
				{
					ScrollOffset(parms[0].ToInt32() * (this.VisibleOptionCount - 1));
				}
				else if(scrollType == ScrollType.Full)
				{
					ScrollOffset(parms[0].ToInt32() * 999);
				}

				return true;
			}

			return HandleAction(action, ev, widget, forceHandled);
		}

		public override void ObserveEvent(idMenuWidget widget, idWidgetEvent ev)
		{
			ExecuteEvent(ev);
		}
		#endregion
	}
}