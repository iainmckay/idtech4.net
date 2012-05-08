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

		public Cursor Cursor
		{
			get
			{
				return _cursor;
			}
			set
			{
				_cursor = value;
			}
		}

		public idFontFamily FontFamily
		{
			get
			{
				return _currentFontFamily;
			}
			set
			{
				if(value == null)
				{
					_currentFontFamily = _fontFamilies[0];
				}
				else
				{
					_currentFontFamily = value;
				}
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

		private Cursor _cursor;

		private idMaterial _whiteImage;
		private idMaterial[] _cursorImages = new idMaterial[(int) Cursor.Count];

		private idFont _currentFont;
		private idFontFamily _currentFontFamily;
		private List<idFontFamily> _fontFamilies = new List<idFontFamily>();

		private Stack<idRectangle> _clipRectangles = new Stack<idRectangle>();
		private string _fontLanguage;
		#endregion

		#region Constructor
		public idDeviceContext()
		{
			Clear();
		}
		#endregion

		#region Methods
		#region Public
		public void DrawCursor(ref float x, ref float y, float size)
		{
			if(x < 0)
			{
				x = 0;
			}

			if(x >= _videoWidth)
			{
				x = _videoWidth;
			}

			if(y < 0)
			{
				y = 0;
			}

			if(y >= _videoHeight)
			{
				y = _videoHeight;
			}

			idE.RenderSystem.Color = idColor.White;

			AdjustCoordinates(ref x, ref y, ref size, ref size);
			DrawStretchPicture(x, y, size, size, 0, 0, 1, 1, _cursorImages[(int) _cursor]);
		}

		public void DrawFilledRectangle(float x, float y, float width, float height, Vector4 color)
		{
			if(color.W == 0.0f)
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

		public void DrawMaterial(float x, float y, float width, float height, idMaterial material, Vector4 color, float scaleX, float scaleY)
		{
			idE.RenderSystem.Color = color;

			float s0, s1, t0, t1;

			// 
			//  handle negative scales as well	
			if(scaleX < 0)
			{
				width *= -1;
				scaleX *= -1;
			}

			if(scaleY < 0)
			{
				height *= -1;
				scaleY *= -1;
			}

			// 
			if(width < 0)
			{
				// flip about vertical
				width = -width;
				s0 = 1 * scaleX;
				s1 = 0;
			}
			else
			{
				s0 = 0;
				s1 = 1 * scaleX;
			}

			if(height < 0)
			{
				// flip about horizontal
				height = -height;
				t0 = 1 * scaleY;
				t1 = 0;
			}
			else
			{
				t0 = 0;
				t1 = 1 * scaleY;
			}

			if(ClipCoordinates(ref x, ref y, ref width, ref height, ref s0, ref t0, ref s1, ref t1) == true)
			{
				return;
			}

			AdjustCoordinates(ref x, ref y, ref width, ref height);
			DrawStretchPicture(x, y, width, height, s0, t0, s1, t1, material);
		}

		public void DrawRectangle(float x, float y, float width, float height, float size, Vector4 color)
		{
			if(color.W == 0.0f)
			{
				return;
			}

			idE.RenderSystem.Color = color;

			if(ClipCoordinates(ref x, ref y, ref width, ref height) == true)
			{
				return;
			}

			AdjustCoordinates(ref x, ref y, ref width, ref height);

			DrawStretchPicture(x, y, size, height, 0, 0, 0, 0, _whiteImage);
			DrawStretchPicture(x + width - size, y, size, height, 0, 0, 0, 0, _whiteImage);
			DrawStretchPicture(x, y, width, size, 0, 0, 0, 0, _whiteImage);
			DrawStretchPicture(x, y + height - size, width, size, 0, 0, 0, 0, _whiteImage);
		}

		public void DrawStretchPicture(float x, float y, float width, float height, float s, float t, float s2, float t2, idMaterial material)
		{
			Vertex[] verts = new Vertex[4];
			int[] indexes = new int[6];

			/*indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			indexes[3] = 0;
			indexes[4] = 2;
			indexes[5] = 3;*/
			
			indexes[0] = 3;
			indexes[1] = 0;
			indexes[2] = 2;
			indexes[3] = 2;
			indexes[4] = 0;
			indexes[5] = 1;

			verts[0].Position = new Vector3(x, y, 0);
			verts[0].TextureCoordinates = new Vector2(s, t);
			verts[0].Normal = new Vector3(0, 0, 1);
			/*verts[0].Tangents = new Vector3[] {
				new Vector3(1, 0, 0),
				new Vector3(0, 1, 0)
			};*/

			verts[1].Position = new Vector3(x + width, y, 0);
			verts[1].TextureCoordinates = new Vector2(s2, t);
			verts[1].Normal = new Vector3(0, 0, 1);
			/*verts[1].Tangents = new Vector3[] {
				new Vector3(1, 0, 0),
				new Vector3(0, 1, 0)
			};*/

			verts[2].Position = new Vector3(x + width, y + height, 0);
			verts[2].TextureCoordinates = new Vector2(s2, t2);
			verts[2].Normal = new Vector3(0, 0, 1);
			/*verts[2].Tangents = new Vector3[] {
				new Vector3(1, 0, 0),
				new Vector3(0, 1, 0)
			};*/

			verts[3].Position = new Vector3(x, y + height, 0);
			verts[3].TextureCoordinates = new Vector2(s, t2);
			verts[3].Normal = new Vector3(0, 0, 1);
			/*verts[3].Tangents = new Vector3[] {
				new Vector3(1, 0, 0),
				new Vector3(0, 1, 0)
			};*/

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

			idE.RenderSystem.DrawStretchPicture(verts.ToArray(), indexes.ToArray(), material, ident);
		}

		public int DrawText(string text, float textScale, TextAlign textAlign, Vector4 color, idRectangle rectDraw, bool wrap, int cursor = -1, bool calcOnly = false, List<int> breaks = null, int limit = 0)
		{
			float textWidth = 0;

			float charSkip = MaxCharacterWidth(textScale) + 1;
			float lineSkip = MaxCharacterHeight(textScale);
			float cursorSkip = (cursor >= 0) ? charSkip : 0;

			SetFontByScale(textScale);

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

			if(breaks != null)
			{
				breaks.Add(0);
			}
			
			while(true)
			{
				c = idHelper.GetBufferCharacter(text, textPosition);
			
				if((c == '\n') || (c == '\r') || (c == '\0'))
				{
					lineBreak = true;

					if(((c == '\n') && (idHelper.GetBufferCharacter(text, textPosition + 1) == '\r'))
						|| ((c == '\r') && (idHelper.GetBufferCharacter(text, textPosition + 1) == '\n')))
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
						count += DrawText(x, y, textScale, color, buffer.ToString(0, (newLine > 0) ? newLine : length), 0, 0, 0, cursor);
						
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

					if(breaks != null)
					{
						breaks.Add(textPosition);
					}

					length = 0;
					newLine = 0;
					newLineWidth = 0;
					textWidth = 0;
					lineBreak = false;
					wordBreak = false;
				}
				else
				{
					length++;
					buffer.Append(idHelper.GetBufferCharacter(text, textPosition++));

					// update the width
					if((buffer[length - 1] != (int) idColorIndex.Escape)
						&& ((length <= 1) || (buffer[length - 2] != (int) idColorIndex.Escape)))
					{
						byte c2 = (byte) buffer[length - 1];
						textWidth += textScale * _currentFont.GlyphScale * _currentFont.Glyphs[(char) c2].SkipX;
					}
				}
			}

			return (int) (rectDraw.Width / charSkip);
		}

		public idFontFamily FindFont(string name)
		{
			string nameLower = name.ToLower();
			
			foreach(idFontFamily fontFamily in _fontFamilies)
			{
				if(fontFamily.Name.Equals(nameLower) == true)
				{
					return fontFamily;
				}
			}

			// if the font was not found, try to register it
			string fileName = name.Replace("fonts", string.Format("fonts/{0}", _fontLanguage));
			idFontFamily fontFamily2 = idE.RenderSystem.RegisterFont(name, fileName);

			if(fontFamily2 != null)
			{
				_fontFamilies.Add(fontFamily2);
			}
			else
			{
				idConsole.WriteLine("Could not register font {0} [{1}]", name, fileName);
			}

			return fontFamily2;
		}

		public int GetCharacterWidth(char c, float scale)
		{
			SetFontByScale(scale);

			return (int) (_currentFont.Glyphs[c].SkipX * (scale * _currentFont.GlyphScale));
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

			SetSize(idE.VirtualScreenWidth, idE.VirtualScreenHeight);

			_currentFontFamily = _fontFamilies[0];
			/*
			 * TODO*/

			_cursorImages[(int) Cursor.Arrow] = idE.DeclManager.FindMaterial("ui/assets/guicursor_arrow.tga");
			_cursorImages[(int) Cursor.Hand] = idE.DeclManager.FindMaterial("ui/assets/guicursor_hand.tga");
			_cursorImages[(int) Cursor.Arrow].Sort = (float) MaterialSort.Gui;
			_cursorImages[(int) Cursor.Hand].Sort = (float) MaterialSort.Gui;

			/*scrollBarImages[SCROLLBAR_HBACK] = declManager->FindMaterial("ui/assets/scrollbarh.tga");
			scrollBarImages[SCROLLBAR_VBACK] = declManager->FindMaterial("ui/assets/scrollbarv.tga");
			scrollBarImages[SCROLLBAR_THUMB] = declManager->FindMaterial("ui/assets/scrollbar_thumb.tga");
			scrollBarImages[SCROLLBAR_RIGHT] = declManager->FindMaterial("ui/assets/scrollbar_right.tga");
			scrollBarImages[SCROLLBAR_LEFT] = declManager->FindMaterial("ui/assets/scrollbar_left.tga");
			scrollBarImages[SCROLLBAR_UP] = declManager->FindMaterial("ui/assets/scrollbar_up.tga");
			scrollBarImages[SCROLLBAR_DOWN] = declManager->FindMaterial("ui/assets/scrollbar_down.tga");*/

			/*crollBarImages[SCROLLBAR_HBACK]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_VBACK]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_THUMB]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_RIGHT]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_LEFT]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_UP]->SetSort( SS_GUI );
			scrollBarImages[SCROLLBAR_DOWN]->SetSort( SS_GUI );*/

			_cursor = Cursor.Arrow;

			/*overStrikeMode = true;*/
			
			_initialized = true;
		}

		public int MaxCharacterWidth(float scale)
		{
			SetFontByScale(scale);

			return (int) System.Math.Ceiling(_currentFont.MaxWidth * (scale * _currentFont.GlyphScale));
		}

		public int MaxCharacterHeight(float scale)
		{
			SetFontByScale(scale);

			return (int) System.Math.Ceiling(_currentFont.MaxHeight * (scale * _currentFont.GlyphScale));
		}

		public void PopClipRectangle()
		{
			if(_clipRectangles.Count > 0)
			{
				_clipRectangles.Pop();
			}
		}

		public void PushClipRectangle(idRectangle rect)
		{
			_clipRectangles.Push(rect);
		}

		public void PushClipRectangle(float x, float y, float width, float height)
		{
			_clipRectangles.Push(new idRectangle(x, y, width, height));
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

			_currentFont = null;
			_currentFontFamily = null;
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

			foreach(idRectangle clipRect in _clipRectangles)
			{
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

		private int DrawText(float x, float y, float scale, Vector4 color, string text, float adjust, int limit, int style, int cursor)
		{
			SetFontByScale(scale);

			float useScale = scale * _currentFont.GlyphScale;
			int count = 0;
			idFontGlyph glyph;

			if((text != string.Empty) && (color.W != 0.0f))
			{
				char c, c2;
				int textPosition = 0;
				int length = text.Length;
				Vector4 newColor = color;

				idE.RenderSystem.Color = color;

				if((limit > 0) && (length > limit))
				{
					length = limit;
				}

				while(((c = idHelper.GetBufferCharacter(text, textPosition)) != '\0') && (count < length))
				{
					if((c < idE.GlyphStart) || (c > idE.GlyphEnd))
					{
						textPosition++;
						c = idHelper.GetBufferCharacter(text, textPosition);

						continue;
					}

					glyph = _currentFont.Glyphs[c];

					if(idHelper.IsColor(text, textPosition) == true)
					{
						c2 = idHelper.GetBufferCharacter(text, textPosition + 1);

						if(c2 == (int) idColorIndex.Default)
						{
							newColor = color;
						}
						else
						{
							newColor = idHelper.ColorForIndex(c2);
						}

						if((cursor == count) || (cursor == (count + 1)))
						{
							float partialSkip = ((glyph.SkipX * useScale) + adjust) / 5.0f;

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
						float adjustY = useScale * glyph.Top;

						PaintCharacter(x, y - adjustY, glyph.ImageWidth, glyph.ImageHeight, useScale, glyph.S, glyph.T, glyph.S2, glyph.T2, glyph.Glyph);


						/* TODO: if(cursor == count)
						{
							// TODO: DrawEditCursor(x, y, scale);
						}*/

						x += (glyph.SkipX * useScale) + adjust;
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
			if(scale <= idE.CvarSystem.GetFloat("gui_smallFontLimit"))
			{
				_currentFont = _currentFontFamily.Small;
			}
			else if(scale <= idE.CvarSystem.GetFloat("gui_mediumFontLimit"))
			{
				_currentFont = _currentFontFamily.Medium;
			}
			else
			{
				_currentFont = _currentFontFamily.Large;
			}
		}

		private void SetupFonts()
		{
			_fontLanguage = idE.CvarSystem.GetString("sys_lang");
	
			// western european languages can use the english font
			if((_fontLanguage == "french")
				|| (_fontLanguage == "german")
				|| (_fontLanguage == "spanish")
				|| (_fontLanguage == "italian") ) 
			{
				_fontLanguage = "english";
			}

			// default font has to be added first
			FindFont("fonts");
		}
		#endregion
		#endregion
	}
}