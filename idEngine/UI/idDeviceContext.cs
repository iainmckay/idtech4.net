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

using idTech4.Renderer;

namespace idTech4.UI
{
	public sealed class idDeviceContext
	{
		#region Properties
		public bool ClippingEnabled
		{
			get
			{
				return _enableClipping;
			}
			set
			{
				_enableClipping = value;
			}
		}
		#endregion

		#region Members
		private bool _initialized;
		private Matrix _matrix;
		private Vector3 _origin;

		private float _scaleX;
		private float _scaleY;

		private float _videoWidth;
		private float _videoHeight;

		private bool _enableClipping;
		private bool _mbcs;

		private idMaterial _whiteImage;

		private Stack<Rectangle> _clipRectangles = new Stack<Rectangle>();
		#endregion

		#region Constructor
		public idDeviceContext()
		{
			Clear();
		}
		#endregion

		#region Methods
		#region Public
		public void DrawFilledRectangle(float x, float y, float width, float height, Color color)
		{
			if(color.A == 0.0f)
			{
				return;
			}

			idE.RenderSystem.Color = color;
	
			if(ClipCoordinates(ref x, ref y, ref width, ref height) == true)
			{
				return;
			}
						
			AdjustCoordinates(ref x, ref y, ref width, ref height);
			DrawStretchPicture(x, y, width, height, 0, 0, 0, 0, _whiteImage);
		}

		public void DrawStretchPicture(float x, float y, float width, float height, float s, float t, float s2, float t2, idMaterial shader)
		{
			Vertex[] verts = new Vertex[4];
			int[] indexes = new int[6];

			indexes[0] = 3;
			indexes[1] = 0;
			indexes[2] = 2;
			indexes[3] = 2;
			indexes[4] = 0;
			indexes[5] = 1;

			verts[0].Position = new float[] { x, y, 0 };
			verts[0].TextureCoordinates = new float[] { s, t };
			verts[0].Normal = new float[] { 0, 0, 1 };
			verts[0].Tangents = new float[,] {
				{ 1, 0, 0 },
				{ 0, 1, 0 }
			};

			verts[1].Position = new float[] { x + width, y, 0 };
			verts[1].TextureCoordinates = new float[] { s2, t };
			verts[1].Normal = new float[] { 0, 0, 1 };
			verts[1].Tangents = new float[,] {
				{ 1, 0, 0 },
				{ 0, 1, 0 }
			};

			verts[2].Position = new float[] { x + width, y + height, 0 };
			verts[2].TextureCoordinates = new float[] { s2, t2 };
			verts[2].Normal = new float[] { 0, 0, 1 };
			verts[2].Tangents = new float[,] {
				{ 1, 0, 0 },
				{ 0, 1, 0 }
			};

			verts[3].Position = new float[] { x, y + height, 0 };
			verts[3].TextureCoordinates = new float[] { s, t };
			verts[3].Normal = new float[] { 0, 0, 1 };
			verts[3].Tangents = new float[,] {
				{ 1, 0, 0 },
				{ 0, 1, 0 }
			};

			bool ident = _matrix != Matrix.Identity;

			if(ident == true)
			{
				idConsole.Write("TODO: IDENT == true");
				/*verts[0].Position -= _origin;
				verts[0].Position *= _matrix.Translation;
				verts[0].Position += _origin;
				verts[1].Position -= _origin;
				verts[1].Position *= _matrix.Translation;
				verts[1].Position += _origin;
				verts[2].Position -= _origin;
				verts[2].Position *= _matrix.Translation;
				verts[2].Position += _origin;
				verts[3].Position -= _origin;
				verts[3].Position *= _matrix.Translation;
				verts[3].Position += _origin;*/
			}

			idE.RenderSystem.DrawStretchPicture(verts.ToArray(), indexes.ToArray(), shader, ident);
		}

		public int DrawText(string text, float textScale, TextAlign textAlign, Color color, Rectangle rectDraw, bool wrap)
		{
			return DrawText(text, textScale, textAlign, color, rectDraw, wrap, -1, false, null, 0);
		}

		public int DrawText(string text, float textScale, TextAlign textAlign, Color color, Rectangle rectDraw, bool wrap, int cursor)
		{
			return DrawText(text, textScale, textAlign, color, rectDraw, wrap, cursor, false, null, 0);
		}

		public int DrawText(string text, float textScale, TextAlign textAlign, Color color, Rectangle rectDraw, bool wrap, int cursor, bool calcOnly, int[] breaks, int limit)
		{
			SetFontByScale(textScale);

			float textWidth = 0;

			float charSkip = MaxCharacterWidth(textScale) + 1;
			float lineSkip = MaxCharacterHeight(textScale);
			float cursorSkip = (cursor >= 0) ? charSkip : 0;

			// TODO: edit cursor
			/*if (!calcOnly && !(text && *text)) {
				if (cursor == 0) {
					renderSystem->SetColor(color);
					DrawEditCursor(rectDraw.x, lineSkip + rectDraw.y, textScale);
				}
				return idMath::FtoiFast( rectDraw.w / charSkip );
			}*/

			char c;
			int textPosition = 0;
			int length = 0, newLine = 0, newLineWidth = 0, newLinePosition = 0, count = 0;
			bool lineBreak = false;
			bool wordBreak = false;
			float y = lineSkip + rectDraw.Y;

			StringBuilder buffer = new StringBuilder();

			// TODO: text breaks
			//if(breaks != null)
			/*breaks->Append(0);
		}*/

			c = text[0];

			while((c = idHelper.GetBufferCharacter(text, textPosition)) != '\0')
			{
				if((c == '\n') || (c == '\r') || (c == '\0'))
				{
					lineBreak = true;

					if(((c == '\n') && (idHelper.GetBufferCharacter(text, textPosition + 1) == '\r'))
						|| ((c == '\r') && (idHelper.GetBufferCharacter(text, textPosition + 1) == '\r')))
					{
						textPosition++;
						c = idHelper.GetBufferCharacter(text, textPosition);
					}
				}

				int nextCharWidth = (int) ((idHelper.CharacterIsPrintable(c) == true) ? GetCharacterWidth(c, textScale) : cursorSkip);

				// FIXME: this is a temp hack until the guis can be fixed not not overflow the bounding rectangles
				//  the side-effect is that list boxes and edit boxes will draw over their scroll bars
				//	The following line and the !linebreak in the if statement below should be removed
				nextCharWidth = 0;

				if((lineBreak == false) && ((textWidth + nextCharWidth) > rectDraw.Width))
				{
					// the next character will cause us to overflow, if we haven't yet found a suitable
					// break spot, set it to be this character
					if((length > 0) && (newLine == 0))
					{
						newLine = length;
						newLinePosition = textPosition;
						newLineWidth = (int) textWidth;
					}

					wordBreak = true;
				}
				else if((lineBreak == true) || ((wrap == true) && ((c == ' ') || (c == '\t'))))
				{
					// The next character is in view, so if we are a break character, store our position
					newLine = length;
					newLinePosition = textPosition + 1;
					newLineWidth = (int) textWidth;
				}

				if((lineBreak == true) || (wordBreak == true))
				{
					float x = rectDraw.X;

					if(textAlign == TextAlign.Right)
					{
						x = rectDraw.X + rectDraw.Width - newLineWidth;
					}
					else if(textAlign == TextAlign.Center)
					{
						x = rectDraw.X + ((rectDraw.Width - newLineWidth) / 2);
					}

					if((wrap == true) || (newLine > 0))
					{
						// this is a special case to handle breaking in the middle of a word.
						// if we didn't do this, the cursor would appear on the end of this line
						// and the beginning of the next
						if((wordBreak == true) && (cursor >= newLine) && (newLine == length))
						{
							cursor++;
						}
					}

					if(calcOnly == false)
					{
						count += DrawText(x, y, textScale, color, buffer.ToString(), 0, 0, 0, cursor);
						buffer.Clear();
					}

					if(cursor < newLine)
					{
						cursor = -1;
					}
					else if(cursor >= 0)
					{
						cursor -= (newLine + 1);
					}

					if(wrap == false)
					{
						return newLine;
					}

					if(((limit > 0) && (count > limit)) || (c == '\0'))
					{
						break;
					}

					y += lineSkip + 5;

					if((calcOnly == false) && (y > rectDraw.Bottom))
					{
						break;
					}

					textPosition = newLinePosition;

					// TODO: text breaks
					/*if (breaks) {
						breaks->Append(p - text);
					}*/

					length = 0;
					newLine = 0;
					newLineWidth = 0;
					textWidth = 0;
					lineBreak = false;
					wordBreak = false;

					continue;
				}

				textPosition++;

				// update the width
				if((text[textPosition - 1] != (int) idColor.Escape)
					&& ((length <= 1) || (text[textPosition - 2] != (int) idColor.Escape)))
				{
					// TODO: textWidth += textScale * _useFont.GlyphScale * _useFont.Glyphs[text[textPosition] - 1].SkipX;
				}
			}

			return (int) (rectDraw.Width / charSkip);
		}

		public int GetCharacterWidth(char c, float scale)
		{
			SetFontByScale(scale);
			idConsole.WriteLine("TODO: idDeviceContext.GetCharacterWidth");
			return 0; 
			// return (int) (_useFont.Glyphs[c].SkipX * (scale * _useFont.GlyphScale));
		}

		public void GetTransformInformation(out Vector3 origin, out Matrix transform)
		{
			origin = _origin;
			transform = _matrix;
		}

		public void Init()
		{
			_scaleX = 0.0f;

			_whiteImage = idE.DeclManager.FindMaterial("guis/assets/white.tga");
			_whiteImage.Sort = (float) MaterialSort.Gui;

			SetupFonts();

			_matrix = Matrix.Identity;
			_origin = Vector3.Zero;
			_enableClipping = true;
			_mbcs = false;
			/*
			SetSize(VIRTUAL_WIDTH, VIRTUAL_HEIGHT);
			
			activeFont = &fonts[0];
			colorPurple = idVec4(1, 0, 1, 1);
			colorOrange = idVec4(1, 1, 0, 1);
			colorYellow = idVec4(0, 1, 1, 1);
			colorGreen = idVec4(0, 1, 0, 1);
			colorBlue = idVec4(0, 0, 1, 1);
			colorRed = idVec4(1, 0, 0, 1);
			colorWhite = idVec4(1, 1, 1, 1);
			colorBlack = idVec4(0, 0, 0, 1);
			colorNone = idVec4(0, 0, 0, 0);
			cursorImages[CURSOR_ARROW] = declManager->FindMaterial("ui/assets/guicursor_arrow.tga");
			cursorImages[CURSOR_HAND] = declManager->FindMaterial("ui/assets/guicursor_hand.tga");
			scrollBarImages[SCROLLBAR_HBACK] = declManager->FindMaterial("ui/assets/scrollbarh.tga");
			scrollBarImages[SCROLLBAR_VBACK] = declManager->FindMaterial("ui/assets/scrollbarv.tga");
			scrollBarImages[SCROLLBAR_THUMB] = declManager->FindMaterial("ui/assets/scrollbar_thumb.tga");
			scrollBarImages[SCROLLBAR_RIGHT] = declManager->FindMaterial("ui/assets/scrollbar_right.tga");
			scrollBarImages[SCROLLBAR_LEFT] = declManager->FindMaterial("ui/assets/scrollbar_left.tga");
			scrollBarImages[SCROLLBAR_UP] = declManager->FindMaterial("ui/assets/scrollbar_up.tga");
			scrollBarImages[SCROLLBAR_DOWN] = declManager->FindMaterial("ui/assets/scrollbar_down.tga");
			cursorImages[CURSOR_ARROW]->SetSort( SS_GUI );
			cursorImages[CURSOR_HAND]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_HBACK]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_VBACK]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_THUMB]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_RIGHT]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_LEFT]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_UP]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_DOWN]->SetSort( SS_GUI );
			cursor = CURSOR_ARROW;
			overStrikeMode = true;*/
			_initialized = true;
		}

		public int MaxCharacterWidth(float scale)
		{
			SetFontByScale(scale);

			return 0;
			// TODO: return (int) (_activeFont.MaxWidth * (scale * _useFont.GlyphScale));
		}

		public int MaxCharacterHeight(float scale)
		{
			SetFontByScale(scale);

			return 0;
			// TODO: return (int) (_activeFont.MaxHeight * (scale * _useFont.GlyphScale));
		}

		public void PopClipRectangle()
		{
			if(_clipRectangles.Count > 0)
			{
				_clipRectangles.Pop();
			}
		}

		public void PushClipRectangle(Rectangle rect)
		{
			_clipRectangles.Push(rect);
		}

		public void PushClipRectangle(int x, int y, int width, int height)
		{
			_clipRectangles.Push(new Rectangle(x, y, width, height));
		}

		public void SetSize(float width, float height)
		{
			_videoWidth = idE.VirtualScreenWidth;
			_videoHeight = idE.VirtualScreenHeight;

			_scaleX = _scaleY = 0;

			if((width != 0.0f) && (height != 0.0f))
			{
				_scaleX = _videoWidth * (1.0f / width);
				_scaleY = _videoHeight * (1.0f / height);
			}
		}

		public void SetTransformInformation(Vector3 origin, Matrix transform)
		{
			_matrix = transform;
			_origin = origin;
		}
		#endregion

		#region Private
		private void AdjustCoordinates(ref float x, ref float y, ref float width, ref float height)
		{
			x *= _scaleX;
			y *= _scaleY;
			width *= _scaleX;
			height *= _scaleY;
		}

		private void Clear()
		{
			_initialized = false;

			// TODO: _useFont = null;
			// TODO: _activeFont = null;
			_mbcs = false;
		}

		private bool ClipCoordinates(ref float x, ref float y, ref float width, ref float height)
		{
			float s = 0, t = 0, s2 = 0, t2 = 0;

			return ClipCoordinates(ref x, ref y, ref width, ref height, ref s, ref t, ref s2, ref t2);
		}

		private bool ClipCoordinates(ref float x, ref float y, ref float width, ref float height, ref float s, ref float t, ref float s2, ref float t2)
		{
			if((_enableClipping == false) || (_clipRectangles.Count == 0))
			{
				return false;
			}

			int count = _clipRectangles.Count;

			while(--count > 0)
			{
				Rectangle clipRect = _clipRectangles.ElementAt(count);

				float ox = x;
				float oy = y;
				float ow = width;
				float oh = height;

				if((ow <= 0.0f) || (oh <= 0.0f))
				{
					break;
				}

				if(x < clipRect.X)
				{
					width -= clipRect.X - x;
					x = clipRect.X;
				}
				else if(x > (clipRect.X + clipRect.Width))
				{
					x = width = y = height = 0;
				}

				if(y < clipRect.Y)
				{
					height -= clipRect.Y - y;
					y = clipRect.Y;
				}
				else if(y > (clipRect.Y + clipRect.Height))
				{
					x = width = y = height = 0;
				}

				if(width > clipRect.Width)
				{
					width = clipRect.Width - x + clipRect.X;
				}
				else if((x + width) > (clipRect.X + clipRect.Width))
				{
					width = clipRect.Right - x;
				}

				if(height > clipRect.Height)
				{
					height = clipRect.Height - y + clipRect.Y;
				}
				else if((y + height) > (clipRect.Y + clipRect.Height))
				{
					height = clipRect.Bottom - y;
				}

				if(ow > 0.0f)
				{
					float ns1, ns2, nt1, nt2;

					// upper left
					float u = (x - ox) / ow;
					ns1 = s * (1.0f - u) + s2 * u;

					// upper right
					u = (x + width - ox) / ow;
					ns2 = s * (1.0f - u) + s2 * u;

					// lower left
					u = (y - oy) / oh;
					nt1 = t * (1.0f - u) + t2 * u;

					// lower right
					u = (y + height - oy) / oh;
					nt2 = t * (1.0f - u) + t2 * u;

					// set values
					s = ns1;
					s2 = ns2;
					t = nt1;
					t2 = nt2;
				}
			}

			return ((width == 0) || (height == 0));
		}

		private int DrawText(float x, float y, float scale, Color color, string text, float adjust, int limit, int style, int cursor)
		{
			SetFontByScale(scale);

			float useScale = scale /* TODO:  *_useFont.GlyphScale*/;
			int count = 0;

			// TODO: Glyph glyph;

			if((text != string.Empty) && (color.A != 0.0f))
			{
				char c, c2;
				int textPosition = 0;
				int length = text.Length;
				Color newColor = color;

				idE.RenderSystem.Color = color;

				if((limit > 0) && (length > limit))
				{
					length = limit;
				}

				while(((c = idHelper.GetBufferCharacter(text, textPosition)) != '\0') && (count < length))
				{
					if((c < idE.GlyphStart) || (c > idE.GlyphStart))
					{
						textPosition++;
						c = idHelper.GetBufferCharacter(text, textPosition);

						continue;
					}

					// TODO: glyph = _useFont.Glyphs[c];

					if(idHelper.IsColor(text, textPosition) == true)
					{
						c2 = idHelper.GetBufferCharacter(text, textPosition + 1);

						if(c2 == (int) idColor.Default)
						{
							newColor = color;
						}
						else
						{
							idConsole.WriteLine("TODO: newColor = idHelper.ColorForIndex");
							// TODO: newColor = idHelper.ColorForIndex(c2);
						}

						if((cursor == count) || (cursor == (count + 1)))
						{
							float partialSkip = (/* TODO: (glyph.SkipX * useScale) + */ adjust) / 5.0f;

							if(cursor == count)
							{
								partialSkip *= 2.0f;
							}
							else
							{
								idE.RenderSystem.Color = newColor;
							}

							// TODO: DrawEditCursor(x - partialSkip, y, scale);
						}

						idE.RenderSystem.Color = newColor;

						textPosition += 2;
						count += 2;
						c = idHelper.GetBufferCharacter(text, textPosition);

						continue;
					}
					else
					{
						// TODO
						/*float adjY = useScale * glyph.Top;

						PaintCharacter(x, y - adjY, glyph.ImageWidth, glyph.ImageHeight, useScale, glyph.S, glyph.T, glyph.S2, glyph.T2, glyph.Glyph);

						if(cursor == count)
						{
							// TODO: DrawEditCursor(x, y, scale);
						}

						x += (glyph.SkipX * useScale) + adjust;*/
						textPosition++;
						count++;
						c = idHelper.GetBufferCharacter(text, textPosition);
					}
				}

				if(cursor == length)
				{
					// TODO: DrawEditCursor(x, y, scale);
				}
			}

			return count;
		}

		private void PaintCharacter(float x, float y, float width, float height, float scale, float s, float t, float s2, float t2, idMaterial shader)
		{
			float tmpWidth = width * scale;
			float tmpHeight = height * scale;

			if(ClipCoordinates(ref x, ref y, ref tmpWidth, ref tmpHeight, ref s, ref t, ref s2, ref t2) == true)
			{
				return;
			}

			AdjustCoordinates(ref x, ref y, ref tmpWidth, ref tmpHeight);
			DrawStretchPicture(x, y, tmpWidth, tmpHeight, s, t, s2, t2, shader);
		}

		private void SetFontByScale(float scale)
		{
			idConsole.WriteLine("TODO: idDeviceContext.SetFontByScale");
			/*if(scale <= idE.CvarSystem.GetFloat("gui_smallFontLimit"))
			{
				_useFont = _activeFont.FontInfoSmall;

				_activeFont.MaxWidth = _activeFont.MaxWidthSmall;
				_activeFont.MaxHeight = _activeFont.MaxHeightSmall;
			}
			else if(scale <= idE.CvarSystem.GetFloat("gui_mediumFontLimit"))
			{
				_useFont = _activeFont.FontInfoMedium;

				_activeFont.MaxWidth = _activeFont.MaxWidthMedium;
				_activeFont.MaxHeight = _activeFont.MaxHeightMedium;
			}
			else
			{
				_useFont = _activeFont.FontInfoLarge;

				_activeFont.MaxWidth = _activeFont.MaxWidthLarge;
				_activeFont.MaxHeight = _activeFont.MaxHeightLarge;
			}*/
		}

		private void SetupFonts()
		{
			// TODO: SetupFonts
			idConsole.WriteLine("TODO: idDeviceContext.SetupFonts");
			/*fonts.SetGranularity( 1 );

			fontLang = cvarSystem->GetCVarString( "sys_lang" );
	
			// western european languages can use the english font
			if ( fontLang == "french" || fontLang == "german" || fontLang == "spanish" || fontLang == "italian" ) {
				fontLang = "english";
			}

			// Default font has to be added first
			FindFont( "fonts" );*/
		}
		#endregion
		#endregion
	}
}