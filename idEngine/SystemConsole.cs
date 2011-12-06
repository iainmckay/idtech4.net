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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using idTech4.UI;

namespace idTech4
{
	internal partial class SystemConsole : Form
	{
		#region Members
		private bool _quitOnClose = true;
		private idInputField _consoleField = new idInputField();
		#endregion

		#region Constructor
		public SystemConsole()
		{
			InitializeComponent();

			_consoleField.InputAvailable += new EventHandler<EventArgs>(OnInputAvailable);
		}
		#endregion

		#region Methods
		#region Public
		public void FocusInput()
		{
			_input.Focus();
		}

		public void Append(string text)
		{
			_log.AppendText(text);
			_log.Select(_log.TextLength, 1);
			_log.ScrollToCaret();
		}

		public void Show(int visLevel, bool quitOnClose)
		{
			_quitOnClose = quitOnClose;

			switch(visLevel)
			{
				case 0:
					this.Hide();
					break;
				case 1:
					this.Show();
					break;
				case 2:
					this.WindowState = FormWindowState.Minimized;
					break;
				default:
					// TODO: Sys_Error("Invalid visLevel %d sent to Sys_ShowConsole\n", visLevel);
					break;
			}
		}
		#endregion

		#region Event handlers
		private void OnQuitClicked(object sender, EventArgs e)
		{
			idE.System.Quit();
		}

		private void OnInputKeyDown(object sender, KeyEventArgs e)
		{
			e.Handled = true;

			_consoleField.ProcessKeyDown(e);

			_input.Text = _consoleField.Buffer;
			_input.Select(_consoleField.SelectionStart, _consoleField.SelectionLength);
		}
		
		private void OnClearClicked(object sender, EventArgs e)
		{
			_log.Clear();
			_input.Focus();
		}

		private void OnInputAvailable(object sender, EventArgs e)
		{
			idConsole.WriteLine("] {0}", _input.Text);
						
			idE.CmdSystem.BufferCommandText(_input.Text);
			// TODO: REMOVE, is only here because we don't have the game loop written yet
			idE.CmdSystem.ExecuteCommandBuffer();
		}
		#endregion
		#endregion

		#region Window overrides
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			idE.System.Quit();
		}
		#endregion	

		private void OnInputKeyPressed(object sender, KeyPressEventArgs e)
		{
			e.Handled = true;
		}
	}
}
