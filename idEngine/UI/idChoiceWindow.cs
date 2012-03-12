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
		private idCvar _cvar;

		private List<string> _choices = new List<string>();
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
			if(_cvarStr.ToString().Length > 0)
			{
				_cvar = idE.CvarSystem.Find(_cvarStr.ToString());

				if(_cvar == null)
				{
					idConsole.Warning("idChoiceWindow::InitVariables: gui '{0}' in window '{1}' references undefined cvar '{2}'", this.UserInterface.SourceFile, this.Name, _cvarStr);
					return;
				}

				// TODO: updateStr.Append( &cvarStr );
			}

			if(_guiStr.ToString().Length > 0)
			{
				// TODO: updateStr.Append( &guiStr );
			}

			// TODO: updateStr.SetGuiInfo( gui->GetStateDict() );
			// TODO: updateStr.Update();
		}

		private void UpdateChoice()
		{
			return;

			/* TODO: if ( !updateStr.Num() ) {
				return;
			}
			/*UpdateVars( true );	
			updateStr.Update();
			if ( choiceType == 0 ) {
				// ChoiceType 0 stores current as an integer in either cvar or gui
				// If both cvar and gui are defined then cvar wins, but they are both updated
				if ( updateStr[ 0 ]->NeedsUpdate() ) {
					currentChoice = atoi( updateStr[ 0 ]->c_str() );
				}
				ValidateChoice();
			} else {
				// ChoiceType 1 stores current as a cvar string
				int c = ( values.Num() ) ? values.Num() : choices.Num();
				int i;
				for ( i = 0; i < c; i++ ) {
					if ( idStr::Icmp( cvarStr.c_str(), ( values.Num() ) ? values[i] : choices[i] ) == 0 ) {
						break;
					}
				}
				if (i == c) {
					i = 0;
				}
				currentChoice = i;
				ValidateChoice();
			}*/
		}

		private void UpdateChoicesAndValues()
		{
			// TODO: UpdateChoicesAndValues
			/*idToken token;
			idStr str2, str3;
			idLexer src;

			if(latchedChoices.Icmp(choicesStr))
			{
				choices.Clear();
				src.FreeSource();
				src.SetFlags(LEXFL_NOFATALERRORS | LEXFL_ALLOWPATHNAMES | LEXFL_ALLOWMULTICHARLITERALS | LEXFL_ALLOWBACKSLASHSTRINGCONCAT);
				src.LoadMemory(choicesStr, choicesStr.Length(), "<ChoiceList>");
				if(src.IsLoaded())
				{
					while(src.ReadToken(&token))
					{
						if(token == ";")
						{
							if(str2.Length())
							{
								str2.StripTrailingWhitespace();
								str2 = common->GetLanguageDict()->GetString(str2);
								choices.Append(str2);
								str2 = "";
							}
							continue;
						}
						str2 += token;
						str2 += " ";
					}
					if(str2.Length())
					{
						str2.StripTrailingWhitespace();
						choices.Append(str2);
					}
				}
				latchedChoices = choicesStr.c_str();
			}
			if(choiceVals.Length() && latchedVals.Icmp(choiceVals))
			{
				values.Clear();
				src.FreeSource();
				src.SetFlags(LEXFL_ALLOWPATHNAMES | LEXFL_ALLOWMULTICHARLITERALS | LEXFL_ALLOWBACKSLASHSTRINGCONCAT);
				src.LoadMemory(choiceVals, choiceVals.Length(), "<ChoiceVals>");
				str2 = "";
				bool negNum = false;
				if(src.IsLoaded())
				{
					while(src.ReadToken(&token))
					{
						if(token == "-")
						{
							negNum = true;
							continue;
						}
						if(token == ";")
						{
							if(str2.Length())
							{
								str2.StripTrailingWhitespace();
								values.Append(str2);
								str2 = "";
							}
							continue;
						}
						if(negNum)
						{
							str2 += "-";
							negNum = false;
						}
						str2 += token;
						str2 += " ";
					}
					if(str2.Length())
					{
						str2.StripTrailingWhitespace();
						values.Append(str2);
					}
				}
				if(choices.Num() != values.Num())
				{
					common->Warning("idChoiceWindow:: gui '%s' window '%s' has value count unequal to choices count", gui->GetSourceFile(), name.c_str());
				}
				latchedVals = choiceVals.c_str();
			}*/
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
						_cvar.SetString(_cvarStr.ToString());
					}
				}

				if((read == false) && (_guiStr.NeedsUpdate == true))
				{
					_guiStr.Set(_currentChoice.ToString());
				}
			}
		}
		#endregion
		#endregion

		#region idWindow implementation
		#region Public
		public override void Activate(bool activate, ref string act)
		{
			base.Activate(activate, ref act);

			if(activate == true) 
			{
				// sets the gui state based on the current choice the window contains
				UpdateChoice();
			}
		}

		public override void Draw(int x, int y)
		{
			idConsole.Warning("TODO: ChoiceWindow Draw");

			/*idVec4 color = foreColor;

			UpdateChoicesAndVals();
			UpdateChoice();

			// FIXME: It'd be really cool if textAlign worked, but a lot of the guis have it set wrong because it used to not work
			textAlign = 0;

			if ( textShadow ) {
				idStr shadowText = choices[currentChoice];
				idRectangle shadowRect = textRect;

				shadowText.RemoveColors();
				shadowRect.x += textShadow;
				shadowRect.y += textShadow;

				dc->DrawText( shadowText, textScale, textAlign, colorBlack, shadowRect, false, -1 );
			}

			if ( hover && !noEvents && Contains(gui->CursorX(), gui->CursorY()) ) {
				color = hoverColor;
			} else {
				hover = false;
			}
			if ( flags & WIN_FOCUS ) {
				color = hoverColor;
			}

			dc->DrawText( choices[currentChoice], textScale, textAlign, color, textRect, false, -1 );*/
		}

		public override idWindowVariable GetVariableByName(string name, bool fixup, ref DrawWindow owner)
		{
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

		public override string HandleEvent(SystemEvent e)
		{
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
			idConsole.Warning("TODO: ChoiceWindow RunNamedEvent");
			/*idStr event, group;
	
			if ( !idStr::Cmpn( eventName, "cvar read ", 10 ) ) {
				event = eventName;
				group = event.Mid( 10, event.Length() - 10 );
				if ( !group.Cmp( updateGroup ) ) {
					UpdateVars( true, true );
				}
			} else if ( !idStr::Cmpn( eventName, "cvar write ", 11 ) ) {
				event = eventName;
				group = event.Mid( 11, event.Length() - 11 );
				if ( !group.Cmp( updateGroup ) ) {
					UpdateVars( false, true );
				}
			}*/
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