/*
===========================================================================

Doom 3 GPL Source Code
Copyright (C) 1999-2011 id Software LLC, a ZeniMax Media company. 

This file is part of the Doom 3 GPL Source Code (?Doom 3 Source Code?).  

Doom 3 Source Code is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Doom 3 Source Code is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Doom 3 Source Code.  If not, see <http://www.gnu.org/licenses/>.

In addition, the Doom 3 Source Code is also subject to certain additional terms. You should have received a copy of these additional terms immediately following the terms and conditions of the GNU General Public License which accompanied the Doom 3 Source Code.  If not, please request a copy in writing from id Software at the address below.

If you have questions concerning this license or the applicable additional terms, you may contact in writing id Software LLC, c/o ZeniMax Media Inc., Suite 120, Rockville, Maryland 20850 USA.

===========================================================================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace idTech4.UI
{
	public sealed class idInputField
	{
		#region Properties
		public string Buffer
		{
			get
			{
				return _buffer.ToString();
			}
			set
			{
				_buffer.Clear();
				_buffer.Append(value);

				this.SelectionStart = _buffer.Length;
				this.SelectionDirection = 0;
			}
		}

		public int SelectionStart
		{
			get
			{				
				if(_selectionDirection > 0)
				{
					return _selectionStart;
				}
				else
				{
					int val = _selectionStart + _selectionDirection;

					if(val < 0)
					{
						val = 0;
					}

					return val;
				}
			}
			set
			{
				if(value < 0)
				{
					value = 0;
				}
				else if(value > _buffer.Length)
				{
					value = _buffer.Length;
				}

				_selectionStart = value;
			}
		}

		public int SelectionLength
		{
			get
			{
				if(_selectionDirection > 0)
				{
					return _selectionDirection;
				}

				return -_selectionDirection;
			}
		}

		private int SelectionDirection
		{
			get
			{
				return _selectionDirection;
			}
			set
			{
				if((this.SelectionStart == 0) && (value < 0))
				{
					value++;
				}
				else if((this.SelectionStart + value) > _buffer.Length)
				{
					value = this.SelectionDirection;
				}

				_selectionDirection = value;
			}
		}


		public int Length
		{
			get
			{
				return _buffer.Length;
			}
		}
		#endregion

		#region Members
		private int _scrollPosition;
		private int _widthInCharacters;

		private int _selectionStart;
		private int _selectionDirection;

		private StringBuilder _buffer = new StringBuilder();
		private CommandCompletionHandler _autoComplete;
		#endregion

		#region Constructor
		public idInputField()
		{
			Clear();
		}
		#endregion

		#region Methods
		public void Clear()
		{
			_buffer.Clear();
			_selectionStart = 0;
			_selectionDirection = 0;
			_scrollPosition = 0;
			_autoComplete = null;
		}

		public void ProcessKeyPress(KeyEventArgs e)
		{
		
		}

		public void ProcessKeyDown(KeyEventArgs e)
		{
			if((e.Alt == true) || (e.Control == true))
			{
				return;
			}
			else if((e.KeyCode == Keys.Home) || (e.KeyCode == Keys.End))
			{
				this.SelectionStart = (e.KeyCode == Keys.Home) ? 0 : _buffer.Length;
				this.SelectionDirection = 0;
			}
			else if((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right))
			{
				if(e.Shift == true)
				{
					this.SelectionDirection += (e.KeyCode == Keys.Left) ? -1 : 1;
				}
				else
				{
					if(this.SelectionDirection != 0)
					{
						if(this.SelectionDirection > 0)
						{
							this.SelectionStart = this.SelectionStart + this.SelectionDirection;
						}
						else
						{
							this.SelectionStart = this.SelectionStart;
						}

						this.SelectionDirection = 0;
					}
					else
					{
						this.SelectionStart += (e.KeyCode == Keys.Left) ? -1 : 1;
					}
				}
			}
			else if((e.KeyCode == Keys.Back) || (e.KeyCode == Keys.Delete))
			{
				if((e.KeyCode == Keys.Delete) && (this.SelectionStart == _buffer.Length))
				{
					// don't want delete, deleting past the end of the buffer.
					return;
				}
				else if((e.KeyCode == Keys.Back) && (this.SelectionStart == 0))
				{
					// don't want backspace deleting paste the start of the buffer.
					return;
				}
				else if(this.SelectionLength > 0)
				{
					_buffer.Remove(this.SelectionStart, this.SelectionLength);
					this.SelectionStart = this.SelectionStart;
				}
				else
				{
					int dir = (e.KeyCode == Keys.Back) ? -1 : 0;

					_buffer.Remove(this.SelectionStart + dir, 1);
					this.SelectionStart += dir;
				}

				if(this.SelectionStart > _buffer.Length)
				{
					this.SelectionStart = _buffer.Length;
				}

				this.SelectionDirection = 0;
			}
			else if((e.KeyValue >= 48) && (e.KeyValue < 90) || (e.KeyValue >= 186) && (e.KeyValue <= 226))
			{
				string key = ((char) e.KeyValue).ToString().ToLower();

				if((e.Modifiers & Keys.Shift) == Keys.Shift)
				{
					key = key.ToUpper();
				}

				_buffer.Insert(_selectionStart, key);

				this.SelectionStart++;
				this.SelectionDirection = 0;
			}
		}
		#endregion
	}
}
