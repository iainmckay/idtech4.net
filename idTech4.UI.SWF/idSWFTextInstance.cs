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

using idTech4.Renderer;
using idTech4.Services;
using idTech4.UI.SWF.Scripting;

using XMath = System.Math;

namespace idTech4.UI.SWF
{
	public class idSWFTextInstance
	{
		#region Properties
		public idSWFRect Bounds
		{
			get
			{
				return _bounds;
			}
			set
			{
				_bounds = value;
			}
		}

		public idSWFColorRGBA Color
		{
			get
			{
				return _color;
			}
		}

		public idSWFEditText EditText
		{
			get
			{
				return _editText;
			}
		}

		public float GlyphScale
		{
			get
			{
				return _glyphScale;
			}
			set
			{
				_glyphScale = value;
			}
		}

		public bool HasDropShadow
		{
			get
			{
				return _useDropShadow;
			}
		}

		public bool HasStroke
		{
			get
			{
				return _useStroke;
			}
		}

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

		public bool IsSubtitle
		{
			get
			{
				return _isSubtitle;
			}
		}

		public bool IsTooltip
		{
			get
			{
				return _isToolTip;
			}
			set
			{
				_isToolTip = value;
			}
		}

		public bool IsUpdatingSubtitle
		{
			get
			{
				return _subtitleUpdating;
			}
		}

		public bool IsVisible
		{
			get
			{
				return _isVisible;
			}
		}

		public float LineSpacing
		{
			get
			{
				return _lineSpacing;
			}
			set
			{
				_lineSpacing = value;
			}
		}

		public int MaxLines
		{
			get
			{
				return _maxLines;
			}
			set
			{
				_maxLines = value;
			}
		}

		public TextRenderMode RenderMode
		{
			get
			{
				return _renderMode;
			}
		}

		public idSWFScriptObject ScriptObject
		{
			get
			{
				return _scriptObject;
			}
		}

		public int SelectionStart
		{
			get
			{
				return _selectionStart;
			}
			set
			{
				_selectionStart = value;
			}
		}

		public int SelectionEnd
		{
			get
			{
				return _selectionEnd;
			}
			set
			{
				_selectionEnd = value;
			}
		}

		public float StrokeStrength
		{
			get
			{
				return _strokeStrength;
			}
		}

		public float StrokeWeight
		{
			get
			{
				return _strokeWeight;
			}
		}

		public int SubtitleEndIndex
		{
			get
			{
				return _subtitleCharacterEndIndex;
			}
		}

		public int SubtitleStartIndex
		{
			get
			{
				return _subtitleCharacterStartIndex;
			}
		}

		public string SubtitleText
		{
			get
			{
				return _subtitleText;
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
				ILocalization localization = idEngine.Instance.GetService<ILocalization>();
				ICVarSystem cvarSystem     = idEngine.Instance.GetService<ICVarSystem>();
				
				if((_lengthCalculated == true) && (string.IsNullOrEmpty(_variable) == true))
				{
					return _textLength;
				}

				string lengthCheck = "";
				float length       = 0.0f;
				
				if(_owner != null)
				{
					if(string.IsNullOrEmpty(_variable) == false)
					{
						idSWFScriptVariable var = _owner.GetGlobal(_variable);

						if(var.IsUndefined == true)
						{
							lengthCheck = _text;
						}
						else
						{
							lengthCheck = var.ToString();
						}

						lengthCheck = localization.Get(lengthCheck);
					}
					else 
					{
						lengthCheck = localization.Get(_text);
					}

					idSWFEditText shape = _editText;
					idSWFFont swfFont   = _owner.FindDictionaryEntry(shape.FontID, typeof(idSWFFont)) as idSWFFont;

					float width     = XMath.Abs(shape.Bounds.BottomRight.X - shape.Bounds.TopLeft.X);
					float postTrans = idSWFHelper.Twip(shape.FontHeight);
				
					idFont fontInfo  = swfFont.Font;
					float glyphScale = postTrans / 48.0f;

					int tlen  = lengthCheck.Length;
					int index = 0;

					while(index < tlen) 
					{
						ScaledGlyph glyph = fontInfo.GetScaledGlyph(glyphScale, lengthCheck[index]);

						length += glyph.SkipX;

						if(_useStroke == true) 
						{
							length += (cvarSystem.GetFloat("swf_textStrokeSizeGlyphSpacer") * _strokeWeight * glyphScale);
						}

						if(((shape.Flags & EditTextFlags.AutoSize) == 0) && (length >= width)) 
						{
							length = width;
							break;
						}
					}
				}

				_lengthCalculated = true;
				_textLength       = (int) length;
				
				return _textLength;
			}
		}

		public string Variable
		{
			get
			{
				return _variable;
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

		private bool _isVisible;
		private bool _isToolTip;

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

		private TextRenderMode _renderMode;
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
		private int	_subtitleLength;
		private int	_subtitleCharacterDisplayTime;
		private int	_subtitleAlign;
		private bool _subtitleUpdating;
		private int	_subtitleCharacterStartIndex;
		private int	_subtitleCharacterEndIndex;
		private int	_subtitleNextStartIndex;
		private int	_subtitleDisplayTime;
		private int	_subtitleStartTime;
		private int	_subtitleSourceID;
		private string _subtitleText;
		private bool _subtitleNeedsSwitch;
		private bool _subtitleForceKillQueued;
		private bool _subtitleForceKill;
		private int	_subtitleKillTimeDelay;
		private int	_subtitleSwitchTime;
		private int	_subtitleLastWordIndex;
		private int	_subtitlePrevLastWordIndex;
		private string _subtitleSpeaker;
		private bool _subtitleWaitClear;
		private bool _subtitleInitialLine;

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

			_editText                    = editText;
			_owner                       = owner;			
			_text                        = localization.Get(_editText.InitialText);

			_lengthCalculated            = false;
			_variable                    = editText.Variable;
			_color                       = editText.Color;
			_isVisible                   = true;

			_selectionStart              = -1;
			_selectionEnd                = -1;

			_scroll                      = 0;
			_scrollTime                  = 0;
			_maxScroll                   = 0;
			_maxLines                    = 0;
			_lineSpacing                 = 0;
			_glyphScale                  = 1.0f;

			_shiftHeld                   = false;
			_isToolTip                   = false;
			_renderMode                  = TextRenderMode.Normal;
			_generatingText              = false;
			_triggerGenerate             = false;
			_rndSpotsVisible             = 0;
			_textSpotsVisible            = 0;
			_startRndTime                = 0;
			_charMultiplier              = 0;
			_prevReplaceIndex            = 0;
			_scrollUpdate                = false;
			_ignoreColor                 = false;
			_isSubtitle                  = false;
			_subtitleLength              = 0;
			_subtitleAlign               = 0;
			_subtitleUpdating            = false;
			_subtitleCharacterStartIndex = 0;
			_subtitleNextStartIndex      = 0;
			_subtitleCharacterEndIndex   = 0;
			_subtitleDisplayTime         = 0;
			_subtitleStartTime           = -1;
			_subtitleSourceID            = -1;
			_subtitleNeedsSwitch         = false;
			_subtitleForceKill           = false;
			_subtitleKillTimeDelay       = 0;
			_subtitleSwitchTime          = 0;
			_subtitleLastWordIndex       = 0;
			_subtitlePrevLastWordIndex   = 0;
			_subtitleInitialLine         = true;

			_textLength                  = 0;
			_inputTextStartChar          = 0;

			_renderDelay                 = cvarSystem.GetInt("swf_textRndLetterDelay");
			_needsSoundUpdate            = false;
			_useDropShadow               = false;
			_useStroke                   = false;
			_strokeStrength              = 1.0f;
			_strokeWeight                = cvarSystem.GetFloat("swf_textStrokeSize");
			_scriptObject                = new idSWFScriptObject(this, _scriptObjectPrototype);
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

	public enum TextRenderMode
	{
		Normal,
		RandomAppear,
		RandomAppearCapitals,
		Paragraph,
		AutoScroll
	}

	public enum TextAlign
	{
		Left,
		Right,
		Center,
		Justify
	}
}