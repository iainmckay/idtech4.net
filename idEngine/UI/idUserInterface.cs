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

		public string SourceFile
		{
			get
			{
				return _sourceFile;
			}
		}
		#endregion

		#region Members
		private bool _active;
		private bool _loading;
		private bool _interactive;
		private bool _uniqued;

		// TODO
		/*private idDict _state;
		private idWindow _desktop;
		private idWindow _bindHandler;*/

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
		}
		#endregion

		#region Methods
		#region Public
		public void AddReference()
		{
			_referenceCount++;
		}

		public bool InitFromFile(string path)
		{
			return InitFromFile(path, true, true);
		}

		public bool InitFromFile(string path, bool rebuild, bool cache)
		{
			if(path == string.Empty)
			{
				return false;
			}

			_loading = true;
			// TODO
			/*_desktop = new idWindow(this, idE.UIManager.Context);
			_desktop.Flag = Window.Desktop;*/

			// TODO
			/*_sourceFile = path;
			_state.Set("text", "Test Text!");*/

			// load the timestamp so reload guis will work correctly
			string content = idE.FileSystem.ReadFile(path, out _timeStamp);

			idScriptParser parser = new idScriptParser(LexerOptions.NoFatalErrors | LexerOptions.NoStringConcatination | LexerOptions.AllowMultiCharacterLiterals | LexerOptions.AllowBackslashStringConcatination);
			parser.LoadMemory(content, path);

			if(parser.IsLoaded == true)
			{
				idToken token;

				while((token = parser.ReadToken()) != null)
				{
					if(token.ToString().Equals("windowDef", StringComparison.OrdinalIgnoreCase) == true)
					{
						// TODO
						/*if(_desktop.Parse(parser, rebuild) == true)
						{
							_desktop.FixupParameters();
						}*/

						continue;
					}
				}

				// TODO: _state.Set("name", path);
			} 
			else 
			{
				// TODO
				/*_desktop.Name = "Desktop";
				_desktop.Text = string.Format("Invalid GUI: {0}", path);
				_desktop.Rectangle = new Rectangle(0, 0, 640, 480);
				_desktop.DrawRectangle = _desktop.Rectangle;
				_desktop.ForeColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
				_desktop.BackColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
				_desktop.SetupFromState();*/

				idConsole.Warning("Couldn't load gui: '{0}'", path);
			}

			// TODO: _interactive = _desktop.IsInteractive;

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
}