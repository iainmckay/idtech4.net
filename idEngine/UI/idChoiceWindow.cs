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
	public sealed class idChoiceWindow : idWindow
	{
		#region Members
		private int _currentChoice;
		private int _choiceType;

		private idWinBool _liveUpdate = new idWinBool("liveUpdate");
		private idWinString _choicesStr = new idWinString("values");
		private idWinString _choiceValues = new idWinString("values");
		private idWinString _cvarStr = new idWinString("cvar");
		private idWinString _guiStr = new idWinString("gui");
		private idWinString _updateGroup = new idWinString("updateGroup");
		private idWinMultiVar _updateStr = new idWinMultiVar();
		private idCvar _cvar;

		private List<string> _choices = new List<string>();
		private List<string> _values = new List<string>();

		private string _latchedChoices = string.Empty;
		#endregion

		#region Constructor
		public idChoiceWindow(idUserInterface gui)
			: base(gui)
		{
			Init();
		}

		public idChoiceWindow(idDeviceContext context, idUserInterface gui)
			: base(gui, context)
		{
			Init();
		}
		#endregion

		#region Methods
		#region Private
		private void Init()
		{
			_currentChoice = 0;
			_choiceType = 0;
			_cvar = null;
			_choices.Clear();
			_liveUpdate.Set(true);
		}

		private void InitVariables()
		{
			if(_cvarStr.ToString() != string.Empty)
			{
				_cvar = idE.CvarSystem.Find(_cvarStr.ToString());

				if(_cvar == null)
				{
					idConsole.Warning("idChoiceWindow::InitVariables: gui '{0}' in window '{1}' references undefined cvar '{2}'", this.UserInterface.SourceFile, this.Name, _cvarStr);
					return;
				}

				_updateStr.Add(_cvarStr);
			}

			if(_guiStr.ToString().Length > 0)
			{
				_updateStr.Add(_guiStr);
			}

			_updateStr.SetGuiInfo(this.UserInterface.State);
			_updateStr.Update();
		}

		private void UpdateChoice()
		{
			if(_updateStr.Count == 0)
			{
				return;
			}

			UpdateVariables(true);

			_updateStr.Update();

			if(_choiceType == 0)
			{
				// ChoiceType 0 stores current as an integer in either cvar or gui
				// If both cvar and gui are defined then cvar wins, but they are both updated
				if(_updateStr[0].NeedsUpdate == true)
				{
					if(_updateStr[0].ToString() == string.Empty)
					{
						_currentChoice = 0;
					}
					else
					{
						_currentChoice = Int32.Parse(_updateStr[0].ToString());
					}
				}

				ValidateChoice();
			} 
			else 
			{
				// ChoiceType 1 stores current as a cvar string
				int count = (_values.Count > 0) ? _values.Count : _choices.Count;
				int i;

				for(i = 0; i < count; i++)
				{
					if(_cvarStr.ToString().Equals((_values.Count > 0) ? _values[i] : _choices[i], StringComparison.OrdinalIgnoreCase) == true)
					{
						break;
					}
				}

				if(i == count)
				{
					i = 0;
				}

				_currentChoice = i;

				ValidateChoice();
			}
		}

		private void UpdateChoicesAndValues()
		{
			idToken token;
			string str2 = string.Empty;

			if(_latchedChoices.Equals(_choicesStr.ToString(), StringComparison.OrdinalIgnoreCase) == true)
			{
				_choices.Clear();

				idLexer lexer = new idLexer(LexerOptions.NoFatalErrors | LexerOptions.AllowPathNames | LexerOptions.AllowMultiCharacterLiterals | LexerOptions.AllowBackslashStringConcatination);
				
				if(lexer.LoadMemory(_choicesStr.ToString(), "<ChoiceList>") == true)
				{
					while((token = lexer.ReadToken()) != null)
					{
						if(token.ToString() == ";")
						{
							if(str2.Length > 0)
							{
								str2 = idE.Language.Get(str2.TrimEnd());
								_choices.Add(str2);
								str2 = string.Empty;
							}

							continue;
						}

						str2 += token.ToString();
						str2 += " ";
					}

					if(str2.Length > 0)
					{
						_choices.Add(str2.TrimEnd());
					}
				}

				_latchedChoices = _choicesStr.ToString();
			}

			if((_choiceValues.ToString() != string.Empty) && (_latchedChoices.Equals(_choiceValues.ToString(), StringComparison.OrdinalIgnoreCase) == false))
			{
				_values.Clear();

				str2 = string.Empty;
				bool negNum = false;
				idLexer lexer = new idLexer(LexerOptions.AllowPathNames | LexerOptions.AllowMultiCharacterLiterals | LexerOptions.AllowBackslashStringConcatination);

				if(lexer.LoadMemory(_choiceValues.ToString(), "<ChoiceVals>") == true)
				{
					while((token = lexer.ReadToken()) != null)
					{
						if(token.ToString() == "-")
						{
							negNum = true;
						}
						else if(token.ToString() == ";")
						{
							if(str2.Length > 0)
							{
								_values.Add(str2.TrimEnd());
								str2 = string.Empty;
							}
						}
						else if(negNum == true)
						{
							str2 += "-";
							negNum = false;
						}
						else
						{
							str2 += token.ToString();
							str2 += " ";
						}
					}

					if(str2.Length > 0)
					{
						_values.Add(str2.TrimEnd());
					}
				}

				if(_choices.Count != _values.Count)
				{
					idConsole.Warning("idChoiceWindow:: gui '{0}' window '{1}' has value count unequal to choices count", this.UserInterface.SourceFile, this.Name);
				}

				_latchedChoices = _choiceValues.ToString();
			}
		}

		private void UpdateVariables(bool read)
		{
			UpdateVariables(read, false);
		}

		private void UpdateVariables(bool read, bool force)
		{
			if((force == true) || (_liveUpdate == true))
			{
				if((_cvar != null) && (_cvarStr.NeedsUpdate == true))
				{
					if(read == true)
					{
						_cvarStr.Set(_cvarStr.ToString());
					}
					else
					{
						_cvar.Set(_cvarStr.ToString());
					}
				}

				if((read == false) && (_guiStr.NeedsUpdate == true))
				{
					_guiStr.Set(_currentChoice.ToString());
				}
			}
		}

		private void ValidateChoice()
		{
			if((_currentChoice < 0) || (_currentChoice >= _choices.Count))
			{
				_currentChoice = 0;
			}

			if(_choices.Count == 0)
			{
				_choices.Add("No Choices Defined");
			}
		}
		#endregion
		#endregion

		#region idWindow implementation
		#region Public
		public override void Activate(bool activate, ref string act)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			base.Activate(activate, ref act);

			if(activate == true) 
			{
				// sets the gui state based on the current choice the window contains
				UpdateChoice();
			}
		}

		public override void Draw(float x, float y)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			Vector4 color = this.ForeColor;

			UpdateChoicesAndValues();
			UpdateChoice();

			// FIXME: It'd be really cool if textAlign worked, but a lot of the guis have it set wrong because it used to not work
			this.TextAlign = TextAlign.Left;

			if(this.TextShadow > 0)
			{
				string shadowText = _choices[_currentChoice];
				idRectangle shadowRect = this.TextRectangle;

				shadowText = idHelper.RemoveColors(shadowText);
				shadowRect.X += this.TextShadow;
				shadowRect.Y += this.TextShadow;

				this.DeviceContext.DrawText(shadowText, this.TextScale, this.TextAlign, idColor.Black, shadowRect, false, -1);
			}

			if((this.Hover == true) && (this.NoEvents == false) && (this.Contains(this.UserInterface.CursorX, this.UserInterface.CursorY) == true))
			{
				color = this.HoverColor;
			}
			else
			{
				this.Hover = false;
			}

			if((this.Flags & WindowFlags.Focus) == WindowFlags.Focus)
			{
				color = this.HoverColor;
			}

			this.DeviceContext.DrawText(_choices[_currentChoice], this.TextScale, this.TextAlign, color, this.TextRectangle, false, -1);
		}

		public override idWindowVariable GetVariableByName(string name, bool fixup, ref DrawWindow owner)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			string nameLower = name.ToLower();

			if(nameLower == "choices")
			{
				return _choicesStr;
			}
			else if(nameLower == "values")
			{
				return _choiceValues;
			}
			else if(nameLower == "cvar")
			{
				return _cvarStr;
			}
			else if(nameLower == "gui")
			{
				return _guiStr;
			}
			else if(nameLower == "liveupdate")
			{
				return _liveUpdate;
			}
			else if(nameLower == "updategroup")
			{
				return _updateGroup;
			}

			return base.GetVariableByName(name, fixup, ref owner);
		}

		public override string HandleEvent(SystemEvent e, ref bool updateVisuals)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			idConsole.Warning("TODO: ChoiceWindow HandleEvent");
			/*int key;
	bool runAction = false;
	bool runAction2 = false;

	if ( event->evType == SE_KEY ) {
		key = event->evValue;

		if ( key == K_RIGHTARROW || key == K_KP_RIGHTARROW || key == K_MOUSE1)  {
			// never affects the state, but we want to execute script handlers anyway
			if ( !event->evValue2 ) {
				RunScript( ON_ACTIONRELEASE );
				return cmd;
			}
			currentChoice++;
			if (currentChoice >= choices.Num()) {
				currentChoice = 0;
			}
			runAction = true;
		}

		if ( key == K_LEFTARROW || key == K_KP_LEFTARROW || key == K_MOUSE2) {
			// never affects the state, but we want to execute script handlers anyway
			if ( !event->evValue2 ) {
				RunScript( ON_ACTIONRELEASE );
				return cmd;
			}
			currentChoice--;
			if (currentChoice < 0) {
				currentChoice = choices.Num() - 1;
			}
			runAction = true;
		}

		if ( !event->evValue2 ) {
			// is a key release with no action catch
			return "";
		}

	} else if ( event->evType == SE_CHAR ) {

		key = event->evValue;

		int potentialChoice = -1;
		for ( int i = 0; i < choices.Num(); i++ ) {
			if ( toupper(key) == toupper(choices[i][0]) ) {
				if ( i < currentChoice && potentialChoice < 0 ) {
					potentialChoice = i;
				} else if ( i > currentChoice ) {
					potentialChoice = -1;
					currentChoice = i;
					break;
				}
			}
		}
		if ( potentialChoice >= 0 ) {
			currentChoice = potentialChoice;
		}

		runAction = true;
		runAction2 = true;

	} else {
		return "";
	}

	if ( runAction ) {
		RunScript( ON_ACTION );
	}

	if ( choiceType == 0 ) {
		cvarStr.Set( va( "%i", currentChoice ) );
	} else if ( values.Num() ) {
		cvarStr.Set( values[ currentChoice ] );
	} else {
		cvarStr.Set( choices[ currentChoice ] );
	}

	UpdateVars( false );

	if ( runAction2 ) {
		RunScript( ON_ACTIONRELEASE );
	}
	
	return cmd;*/

			return string.Empty;
		}

		public override void RunNamedEvent(string name)
		{
			if(this.Disposed == true)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}

			string ev, group;

			if(name.StartsWith("cvar read") == true)
			{
				ev = name;
				group = ev.Substring(10);

				if(group == _updateGroup.ToString())
				{
					UpdateVariables(true, true);
				}
			}
			else if(name.StartsWith("cvar write") == true)
			{
				ev = name;
				group = ev.Substring(11);

				if(group == _updateGroup.ToString())
				{
					UpdateVariables(false, true);
				}
			}
		}
		#endregion

		#region Protected
		protected override bool ParseInternalVariable(string name, Text.idScriptParser parser)
		{
			string nameLower = name.ToLower();

			if(name == "choicetype")
			{
				_choiceType = parser.ParseInteger();
				return true;
			}
			else if(name == "currentchoice")
			{
				_currentChoice = parser.ParseInteger();
				return true;
			}

			return base.ParseInternalVariable(name, parser);
		}

		protected override void PostParse()
		{
			base.PostParse();
			
			UpdateChoicesAndValues();

			InitVariables();

			UpdateChoice();
			UpdateVariables(false);

			this.Flags |= WindowFlags.CanFocus;
		}
		#endregion
		#endregion
	}
}