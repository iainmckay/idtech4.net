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
using System;
using System.Collections.Generic;
using System.Text;

using idTech4.Services;

namespace idTech4.Input
{
	/// <summary>
	/// Provides a mechanism for entering text.  Handles most of the modern expectations of a text box (c+p, select-all, etc.).
	/// </summary>
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

		public int Length
		{
			get
			{
				return _buffer.Length;
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

		public int WidthInCharacters
		{
			get
			{
				return _widthInCharacters;
			}
			set
			{
				_widthInCharacters = value;
			}
		}
		#endregion

		#region Events
		public event EventHandler<EventArgs> InputAvailable;
		#endregion

		#region Members
		private int _widthInCharacters;
		private int _selectionStart;
		private int _selectionDirection;

		private StringBuilder _buffer = new StringBuilder();
		private AutoComplete _autoComplete = new AutoComplete();

		private int _historyLine = 0;
		private List<string> _historyLines = new List<string>();
		#endregion

		#region Constructor
		public idInputField()
		{
			Clear();
		}
		#endregion

		#region Methods
		#region public
		public void Clear()
		{
			_buffer.Clear();
			_selectionStart = 0;
			_selectionDirection = 0;
			_autoComplete = null;
		}

		/*public void ProcessKeyDown(KeyEventArgs e)
		{
			if((e.Alt == true) || (e.Control == true))
			{
				if((e.Control == true) && (e.KeyCode == Keys.A))
				{
					this.SelectionStart = 0;
					this.SelectionDirection = _buffer.Length;
				}
			}
			else if((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Enter))
			{
				if(_buffer.Length > 0)
				{
					if(InputAvailable != null)
					{
						InputAvailable(this, new EventArgs());
					}

					_historyLines.Add(_buffer.ToString());
					_historyLine = _historyLines.Count;

					_buffer.Clear();

					_selectionStart = 0;
					_selectionDirection = 0;
				}
			}
			else if(e.KeyCode == Keys.Tab)
			{
				AutoComplete();
				return;
			}
			else if((e.KeyCode == Keys.Home) || (e.KeyCode == Keys.End))
			{
				if(e.Shift == true)
				{
					// we're shift selecting while pressing home or end, figure out where to select from.
					int start = this.SelectionStart;
					this.SelectionStart = (e.KeyCode == Keys.Home) ? 0 : (this.SelectionDirection != 0) ? -this.SelectionDirection : start;

					if(e.KeyCode == Keys.End)
					{
						this.SelectionDirection = _buffer.Length - start;
					}
					else
					{
						this.SelectionDirection = -(start + 1);
					}
				}
				else
				{
					this.SelectionDirection = 0;
					this.SelectionStart = (e.KeyCode == Keys.Home) ? 0 : _buffer.Length;
				}
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
						if((this.SelectionDirection != 0) && (e.KeyCode == Keys.Right))
						{
							this.SelectionStart = this.SelectionStart + this.SelectionLength;
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
			else if((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down))
			{
				// command history
				_historyLine += (e.KeyCode == Keys.Up) ? -1 : 1;

				if(_historyLine < 0)
				{
					_historyLine = 0;
				}
				else if(_historyLine >= _historyLines.Count)
				{
					_historyLine = _historyLines.Count;
				}

				_buffer.Clear();

				if(_historyLine < _historyLines.Count)
				{
					_buffer.Append(_historyLines[_historyLine]);
				}

				this.SelectionStart = this.Length;
				this.SelectionDirection = 0;
			}
			else if((e.KeyCode == Keys.Back) || (e.KeyCode == Keys.Delete))
			{
				if((e.KeyCode == Keys.Delete) && (this.SelectionStart == _buffer.Length))
				{
					// don't want to delete past the end of the buffer.
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
			else
			{
				char c = idHelper.CharacterFromKeyCode(e.KeyCode, e.Modifiers);

				if(c != '\0')
				{
					_buffer.Insert(_selectionStart, c);

					this.SelectionStart++;
					this.SelectionDirection = 0;
				}
			}

			// pressing tab exits the method above, so if we got here
			// the user pressed another key and broke out of autocomplete.
			_autoComplete = null;
		}*/

		public void AutoComplete()
		{
			if(_buffer.Length == 0)
			{
				return;
			}

			string argCompletionStr = string.Empty;

			if(_autoComplete == null)
			{
				CommandArguments args = new CommandArguments(_buffer.ToString(), true);
				ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();
				ICommandSystem cmdSystem = idEngine.Instance.GetService<ICommandSystem>();

				argCompletionStr = args.ToString();

				_autoComplete = new AutoComplete();
				_autoComplete.CompletionString = args.Get(0);
				_autoComplete.MatchCount = 0;
				_autoComplete.MatchIndex = 0;

				string[] matches = cmdSystem.CommandCompletion(new Predicate<string>(FindMatch));

				if(matches.Length > 1)
				{
					foreach(string s in matches)
					{
						idLog.WriteLine("    {0}", s);
					}
				}

				matches = cvarSystem.CommandCompletion(new Predicate<string>(FindMatch));

				if(matches.Length > 1)
				{
					foreach(string s in matches)
					{
						idLog.WriteLine("    {0} {1}= \"{2}\"", s, idColorString.White, cvarSystem.GetString(s));
					}
				}
				else if(matches.Length == 1)
				{
					this.Buffer = string.Format("{0} ", matches[0]);

					string[] argMatches = cvarSystem.ArgumentCompletion(matches[0], argCompletionStr);

					foreach(string s in argMatches)
					{
						idLog.WriteLine("    {0}", s);
					}
				}
			}
		}
		#endregion

		#region Private
		private bool FindMatch(string str)
		{
			return (str.ToLower().StartsWith(_autoComplete.CompletionString.ToLower()) == true);
		}
		#endregion
		#endregion
	}

	public class AutoComplete
	{
		public int Length;
		public string CompletionString;
		public string CurrentMatch;
		public int MatchCount;
		public int MatchIndex;
		public int FindMatchIndex;
	}
}