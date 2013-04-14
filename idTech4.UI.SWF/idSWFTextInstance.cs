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

using idTech4.Services;
using idTech4.UI.SWF.Scripting;

using XMath = System.Math;

namespace idTech4.UI.SWF
{
	public class idSWFTextInstance
	{
		#region Properties
		public bool IgnoreColor
		{
			get
			{
				return _ignoreColor;
			}
			set
			{
				_ignoreColor = value;
			}
		}

		public idSWFScriptObject ScriptObject
		{
			get
			{
				return _scriptObject;
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text             = value;
				_lengthCalculated = false;
			}
		}

		public float TextLength
		{
			get
			{
				// CURRENTLY ONLY WORKS FOR SINGLE LINE TEXTFIELDS
				/*ILocalization localization = idEngine.Instance.GetService<ILocalization>();
				
				if((_lengthCalculated > 0) && (string.IsNullOrEmpty(_variable) == true))
				{
					return _textLength;
				}

				string lengthCheck = "";
				float length       = 0.0f;

				if(_swf != null)
				{
					if(string.IsNullOrEmpty(_variable) == false)
					{
						idSWFScriptVariable var = _swf.GetGlobal(_variable);

						if(var.IsUndefined == true)
						{
							lengthCheck = _text;
						}
						else
						{
							lengthCheck = var.ToString();
						}

						length = localization.GetString(lengthCheck);
					}
					else 
					{
						lengthCheck = localization.GetString(_text);
					}

					idSWFEditText shape            = _editText;
					idSWFDictionaryEntry fontEntry = _swf.FindDictionaryEntry(shape.FontID, SWF_DICT_FONT);
					idSWFFont swfFont              = fontEntry.Font;

					float width = XMath.Abs(shape.Bounds.BR.X - shape.Bounds.T1.X);
					float postTrans = SWFTWIP(shape.FontHeight);
				
					const idFont * fontInfo = swfFont->fontID;
					float glyphScale = postTrans / 48.0f;

					int tlen = txtLengthCheck.Length();
					int index = 0;
					while ( index < tlen ) {
						scaledGlyphInfo_t glyph;
						fontInfo->GetScaledGlyph( glyphScale, txtLengthCheck.UTF8Char( index ), glyph );

						len += glyph.xSkip;
						if ( useStroke ) {
							len += ( swf_textStrokeSizeGlyphSpacer.GetFloat() * strokeWeight * glyphScale );
						}

						if ( !( shape->flags & SWF_ET_AUTOSIZE ) && len >= width ) {
							len = width;
							break;
						}
					}
				}

				lengthCalculated = true;
				textLength = len;
				return textLength;*/

				return 0;
			}
		}

		public bool Tooltip
		{
			get
			{
				return _toolTip;
			}
			set
			{
				_toolTip = value;
			}
		}
		#endregion

		#region Members
		private idSWFEditText _editText;
		private idSWF _owner;

		// this text instance's script object
		private idSWFScriptObject _scriptObject;
		private static idSWFScriptObject_SpriteInstancePrototype _scriptObjectPrototype = new idSWFScriptObject_SpriteInstancePrototype();

		private string _text;
		private string _randomtext;
		private string _variable;
		private idSWFColorRGBA _color;

		private bool _visible;
		private bool _toolTip;

		private int _selectionStart;
		private int _selectionEnd;
		private bool _ignoreColor;

		private int _scroll;
		private int _scrollTime;
		private int _maxScroll;
		private int _maxLines;
		private float _glyphScale;
		private idSWFRect _bounds;
		private float _lineSpacing;

		private bool _shiftHeld;
		private int _lastInputTime;

		private bool _useDropShadow;
		private bool _useStroke;

		private float _strokeStrength;
		private float _strokeWeight;

		private int	_textLength;
		private bool _lengthCalculated;

		private idSWFTextRenderMode _renderMode;
		private bool _generatingText;
		private int	_rndSpotsVisible;
		private int	_rndSpacesVisible;
		private int	_charMultiplier;
		private int	_textSpotsVisible;
		private int	_rndTime;
		private int	_startRndTime;
		private int	_prevReplaceIndex;
		private bool _triggerGenerate;
		private int	_renderDelay;
		private bool _scrollUpdate;
		private string _soundClip;
		private bool _needsSoundUpdate;
		private int[] _indexArray;
		private Random _random;

		// used for subtitles
		private bool _isSubtitle;
		private int	_subLength;
		private int	_subCharDisplayTime;
		private int	_subAlign;
		private bool _subUpdating;
		private int	_subCharStartIndex;
		private int	_subNextStartIndex;
		private int	_subCharEndIndex;
		private int	_subDisplayTime;
		private int	_subStartTime;
		private int	_subSourceID;
		private string _subtitleText;
		private bool _subNeedsSwitch;
		private bool _subForceKillQueued;
		private bool _subForceKill;
		private int	_subKillTimeDelay;
		private int	_subSwitchTime;
		private int	_subLastWordIndex;
		private int	_subPrevLastWordIndex;
		private string _subSpeaker;
		private bool _subWaitClear;
		private bool _subInitialLine;

		// input text
		private int	_inputTextStartChar;
		#endregion

		#region Constructor
		public idSWFTextInstance()
		{
			_random = new Random();
		}
	
		// TODO: cleanup
		/*idSWFTextInstance::~idSWFTextInstance() {
			scriptObject.SetText( NULL );
			scriptObject.Clear();
			scriptObject.Release();

			subtitleTimingInfo.Clear();
		}*/
		#endregion

		#region Initialization
		public void Initialize(idSWFEditText editText, idSWF owner)
		{
			ICVarSystem cvarSystem     = idEngine.Instance.GetService<ICVarSystem>();
			ILocalization localization = idEngine.Instance.GetService<ILocalization>();

			_editText             = editText;
			_owner                = owner;
			
			_text                  = localization.Get(_editText.InitialText);

			_lengthCalculated     = false;
			_variable             = editText.Variable;
			_color                = editText.Color;
			_visible              = true;

			_selectionStart       = -1;
			_selectionEnd         = -1;

			_scroll               = 0;
			_scrollTime           = 0;
			_maxScroll            = 0;
			_maxLines             = 0;
			_lineSpacing          = 0;
			_glyphScale           = 1.0f;

			_shiftHeld            = false;
			_toolTip              = false;
			_renderMode           = idSWFTextRenderMode.Normal;
			_generatingText       = false;
			_triggerGenerate      = false;
			_rndSpotsVisible      = 0;
			_textSpotsVisible     = 0;
			_startRndTime         = 0;
			_charMultiplier       = 0;
			_prevReplaceIndex     = 0;
			_scrollUpdate         = false;
			_ignoreColor          = false;

			_isSubtitle           = false;
			_subLength            = 0;
			_subAlign             = 0;
			_subUpdating          = false;
			_subCharStartIndex    = 0;
			_subNextStartIndex    = 0;
			_subCharEndIndex      = 0;
			_subDisplayTime       = 0;
			_subStartTime         = -1;
			_subSourceID          = -1;
			_subNeedsSwitch       = false;
			_subForceKill         = false;
			_subKillTimeDelay     = 0;
			_subSwitchTime        = 0;
			_subLastWordIndex     = 0;
			_subPrevLastWordIndex = 0;
			_subInitialLine       = true;

			_textLength           = 0;

			_inputTextStartChar   = 0;

			_renderDelay          = cvarSystem.GetInt("swf_textRndLetterDelay");
			_needsSoundUpdate     = false;
			_useDropShadow        = false;
			_useStroke            = false;
			_strokeStrength       = 1.0f;
			_strokeWeight         = cvarSystem.GetFloat("swf_textStrokeSize");

			_scriptObject         = new idSWFScriptObject(this, _scriptObjectPrototype);
		}
		#endregion

		#region Settings
		public void SetStrokeInfo(bool use, float strength = 0.75f, float weight = 1.75f)
		{
			_useStroke = use;

			if(_useStroke == true)
			{
				_strokeWeight   = weight;
				_strokeStrength = strength;
			}
		}
		#endregion
	}

	public enum idSWFTextRenderMode
	{
		Normal,
		RandomAppear,
		RandomAppearCapitals,
		Paragraph,
		AutoScroll
	}
}