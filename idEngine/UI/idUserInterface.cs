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
	public sealed class idUserInterface
	{
		#region Properties
		public idWindow Desktop
		{
			get
			{
				return _desktop;
			}
		}

		public bool IsActive
		{
			get
			{
				return _active;
			}
		}

		public bool IsInteractive
		{
			get
			{
				return _interactive;
			}
		}

		public bool IsUnique
		{
			get
			{
				return _uniqued;
			}
			set
			{
				_uniqued = value;
			}
		}

		public string PendingCommand
		{
			get
			{
				return _pendingCommand;
			}
			set
			{
				_pendingCommand = value;
			}
		}

		public string SourceFile
		{
			get
			{
				return _sourceFile;
			}
		}

		public idDict State
		{
			get
			{
				return _state;
			}
		}

		public int Time
		{
			get
			{
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
		#endregion

		#region Methods
		#region Public
		public string Activate(bool activate, int time)
		{
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
			_referenceCount++;
		}

		public void Draw(int time)
		{
			if(idE.CvarSystem.GetInteger("r_skipGuiShaders") > 5)
			{
				return;
			}
			
			if((_loading == false) && (_desktop != null))
			{
				_time = time;
				
				idE.UIManager.Context.PushClipRectangle(idE.UIManager.ScreenRectangle);
				_desktop.Draw(0, 0);
				idE.UIManager.Context.PopClipRectangle();
			}
		}

		public string HandleEvent(SystemEvent e, int time)
		{
			bool updateVisuals = false;
			return HandleEvent(e, time, ref updateVisuals);
		}

		public string HandleEvent(SystemEvent e, int time, ref bool updateVisuals)
		{
			_time = time;

			// TODO
			/*if ( bindHandler && event->evType == SE_KEY && event->evValue2 == 1 ) {
				const char *ret = bindHandler->HandleEvent( event, updateVisuals );
				bindHandler = NULL;
				return ret;
			}

			if ( event->evType == SE_MOUSE ) {
				cursorX += event->evValue;
				cursorY += event->evValue2;

				if (cursorX < 0) {
					cursorX = 0;
				}
				if (cursorY < 0) {
					cursorY = 0;
				}
			}*/

			if(_desktop != null)
			{
				_desktop.HandleEvent(e, ref updateVisuals);
			}

			return string.Empty;
		}

		public void HandleNamedEvent(string name)
		{
			_desktop.RunNamedEvent(name);
		}

		public bool InitFromFile(string path, bool rebuild = true, bool cache = true)
		{
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
				_desktop.Rectangle = new Rectangle(0, 0, 640, 480);
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
		#endregion
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