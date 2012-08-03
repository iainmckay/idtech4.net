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

using Microsoft.Xna.Framework;

using idTech4.Text;

namespace idTech4.UI
{
	public sealed class idUserInterface : IDisposable
	{
		#region Properties
		public float CursorX
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _cursorX;
			}
		}

		public float CursorY
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _cursorY;
			}
		}

		public idWindow Desktop
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _desktop;
			}
		}

		public bool IsActive
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _active;
			}
		}

		public bool IsInteractive
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _interactive;
			}
		}

		public bool IsUnique
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _uniqued;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_uniqued = value;
			}
		}

		public string PendingCommand
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _pendingCommand;
			}
			set
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				_pendingCommand = value;
			}
		}

		public int ReferenceCount
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _referenceCount;
			}
		}

		public string SourceFile
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _sourceFile;
			}
		}

		public idDict State
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _state;
			}
		}

		public int Time
		{
			get
			{
				if(this.Disposed == true)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}

				return _time;
			}
		}
		#endregion

		#region Members
		private bool _active;
		private bool _loading;
		private bool _interactive;
		private bool _uniqued;

		private idDict _state;
		private idWindow _desktop;
		private idWindow _bindHandler;

		private string _sourceFile;
		private string _activateStr;
		private string _pendingCommand;
		private string _returnCommand;
		private DateTime _timeStamp;

		private float _cursorX;
		private float _cursorY;

		private int _time;

		private int _referenceCount;
		#endregion

		#region Constructor
		public idUserInterface()
		{
			_referenceCount = 1;
			_state = new idDict();
		}

		~idUserInterface()
		{
			Dispose(false);
		}
		#endregion

		#region Methods
		#region Public
		public string Activate(bool activate, int time)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_time = time;
			_active = activate;

			if(_desktop != null)
			{
				_activateStr = string.Empty;
				_desktop.Activate(activate, ref _activateStr);

				return _activateStr;
			}

			return string.Empty;
		}

		public void AddReference()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_referenceCount++;
		}

		public void ClearReferences()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_referenceCount = 0;
		}

		public void Draw(int time)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(idE.CvarSystem.GetInteger("r_skipGuiShaders") > 5)
			{
				return;
			}
			
			if((_loading == false) && (_desktop != null))
			{
				_time = time;
				
				idE.UIManager.Context.PushClipRectangle(idE.UIManager.ScreenRectangle);
				this.Desktop.Draw(0, 0);
				idE.UIManager.Context.PopClipRectangle();
			}
		}

		public void DrawCursor()
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if((this.Desktop == null) || ((this.Desktop.Flags & WindowFlags.MenuInterface) == WindowFlags.MenuInterface))
			{
				idE.UIManager.Context.DrawCursor(ref _cursorX, ref _cursorY, 32.0f);
			}
			else
			{
				idE.UIManager.Context.DrawCursor(ref _cursorX, ref _cursorY, 64.0f);
			}
		}

		public string HandleEvent(SystemEvent e, int time)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			bool updateVisuals = false;
			return HandleEvent(e, time, ref updateVisuals);
		}

		public string HandleEvent(SystemEvent e, int time, ref bool updateVisuals)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_time = time;

			if((_bindHandler != null) && (e.Type == SystemEventType.Key) && (e.Value2 == 1))
			{
				string ret = _bindHandler.HandleEvent(e, ref updateVisuals);
				_bindHandler = null;
				
				return ret;
			}

			if(e.Type == SystemEventType.Mouse)
			{
				_cursorX += e.Value;
				_cursorY += e.Value2;

				if(_cursorX < 0)
				{
					_cursorX = 0;
				}

				if(_cursorY < 0)
				{
					_cursorY = 0;
				}
			}

			if(this.Desktop != null)
			{
				this.Desktop.HandleEvent(e, ref updateVisuals);
			}

			return string.Empty;
		}

		public void HandleNamedEvent(string name)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			this.Desktop.RunNamedEvent(name);
		}

		public bool InitFromFile(string path, bool rebuild = true, bool cache = true)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(path == string.Empty)
			{
				return false;
			}

			_loading = true;
			
			if((rebuild == true) || (_desktop == null))
			{
				_desktop = new idWindow(this, idE.UIManager.Context);
			}

			_sourceFile = path;
			_state.Set("text", "Test Text!");

			// load the timestamp so reload guis will work correctly
			byte[] data = idE.FileSystem.ReadFile(path, out _timeStamp);
			string content = UTF8Encoding.UTF8.GetString(data);
			idScriptParser parser = null;

			if(content != null)
			{
				parser = new idScriptParser(LexerOptions.NoFatalErrors | LexerOptions.NoStringConcatination | LexerOptions.AllowMultiCharacterLiterals | LexerOptions.AllowBackslashStringConcatination);
				parser.LoadMemory(content, path);
			}

			if((parser != null) && (parser.IsLoaded == true))
			{
				idToken token;

				while((token = parser.ReadToken()) != null)
				{
					if(token.ToString().Equals("windowDef", StringComparison.OrdinalIgnoreCase) == true)
					{
						if(_desktop.Parse(parser, rebuild) == true)
						{
							_desktop.Flags = WindowFlags.Desktop;
							_desktop.FixupParameters();
						}
					}
				}

				_state.Set("name", path);
			}
			else
			{
				_desktop.Name = "Desktop";
				_desktop.Flags = WindowFlags.Desktop;
				_desktop.Text = string.Format("Invalid GUI: {0}", path);
				_desktop.Rectangle = new idRectangle(0, 0, 640, 480);
				_desktop.DrawRectangle = _desktop.Rectangle;
				_desktop.ForeColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
				_desktop.BackColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
				_desktop.SetupFromState();

				idConsole.Warning("Couldn't load gui: '{0}'", path);
			}

			_interactive = _desktop.IsInteractive;

			if(idE.UIManager.FindInternalInterface(this) == null)
			{
				idE.UIManager.AddInternalInterface(this);
			}

			_loading = false;

			return true;
		}

		public void StateChanged(int time, bool redraw = false)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_time = time;

			if(this.Desktop != null)
			{
				this.Desktop.StateChanged(redraw);
			}

			if(this.State.GetBool("noninteractive") == true)
			{
				_interactive = false;
			}
			else
			{
				if(this.Desktop != null)
				{
					_interactive = this.Desktop.IsInteractive;
				}
				else
				{
					_interactive = false;
				}
			}
		}

		public void Trigger(int time)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			_time = time;

			if(_desktop != null)
			{
				_desktop.Trigger();
			}
		}
		#endregion
		#endregion

		#region IDisposable implementation
		public bool Disposed
		{
			get
			{
				return _disposed;
			}
		}

		private bool _disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			if(disposing == true)
			{
				_state = null;
				_desktop = null;
				_bindHandler = null;
			}

			_disposed = true;
		}
		#endregion
	}

	[Flags]
	public enum WindowFlags
	{
		Child = 0x00000001,
		Caption = 0x00000002,
		Border = 0x00000004,
		Sizable = 0x00000008, 
		Movable = 0x00000010,
		Focus = 0x00000020,
		Capture = 0x00000040,
		HorizontalCenter = 0x00000080,
		VerticalCenter = 0x00000100,
		Modal = 0x00000200,
		InTransition = 0x00000400,
		CanFocus = 0x00000800,
		Selected = 0x00001000,
		Transform = 0x00002000,
		HoldCapture = 0x00004000,
		NoWrap = 0x00008000,
		NoClip = 0x00010000,
		InvertRectangle = 0x00020000,
		NaturalMaterial = 0x00040000,
		NoCursor = 0x00080000,
		MenuInterface = 0x00100000,
		Active = 0x00200000,
		ShowCoordinates = 0x00400000,
		ShowTime = 0x00800000,
		WantEnter = 0x01000000,

		Desktop = 0x10000000
	}
}