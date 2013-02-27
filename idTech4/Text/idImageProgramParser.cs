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
using System.Text;

using Microsoft.Xna.Framework.Graphics;

using idTech4.Services;
using idTech4.Renderer;

namespace idTech4.Text
{
	public class idImageProgramParser
	{
		#region Properties
		public string Source
		{
			get
			{
				return _builder.ToString();
			}
		}
		#endregion

		#region Members
		private idLexer _lexer;
		private StringBuilder _builder = new StringBuilder();
		#endregion

		#region Constructor
		public idImageProgramParser()
		{

		}
		#endregion

		#region Methods
		#region Public
		public Texture2D ParseImageProgram(idLexer lexer)
		{
			_lexer = lexer;

			DateTime timeStamp = DateTime.Now;
			TextureUsage usage = TextureUsage.Default;

			return ParseImageProgram(ref timeStamp, ref usage, true);
		}

		public Texture2D ParseImageProgram(idLexer lexer, ref DateTime timeStamp, ref TextureUsage usage)
		{
			_lexer = lexer;

			return ParseImageProgram(ref timeStamp, ref usage, false);
		}

		public Texture2D ParseImageProgram(string source, ref DateTime timeStamp, ref TextureUsage usage)
		{
			_lexer = new idLexer(LexerOptions.NoFatalErrors | LexerOptions.NoStringConcatination | LexerOptions.NoStringEscapeCharacters | LexerOptions.AllowPathNames);
			_lexer.LoadMemory(source, source);

			return ParseImageProgram(ref timeStamp, ref usage, false);
		}

		#endregion

		#region Private
		private void AppendToken(idToken token)
		{
			if(_builder.Length > 0)
			{
				_builder.AppendFormat(" {0}", token.ToString());
			}
			else
			{
				_builder.Append(token.ToString());
			}
		}

		private void MatchAndAppendToken(idLexer lexer, string match)
		{
			if(_lexer.ExpectTokenString(match) == false)
			{
				return;
			}

			// a matched token won't need a leading space
			_builder.Append(match);
		}

		private Texture2D ParseImageProgram(ref DateTime timeStamp, ref TextureUsage usage, bool parseOnly)
		{
			idToken token = _lexer.ReadToken();			
			string tokenLower = token.ToString().ToLower();

			// ince all interaction shaders now assume YCoCG diffuse textures.  We replace all entries for the intrinsic 
			// _black texture to the black texture on disk.  Doing this will cause a YCoCG compliant texture to be generated.
			// Without a YCoCG compliant black texture we will get color artifacts for any interaction
			// material that specifies the _black texture.
			if(tokenLower == "_black")
			{
				token.Set("textures\\black");
			}
			// also check for _white
			else if(tokenLower == "_white")
			{
				token.Set("guis\\assets\\white");
			}

			AppendToken(token);

			if(tokenLower == "heightmap")
			{
				MatchAndAppendToken(_lexer, "(");

				Texture2D tex = ParseImageProgram(_lexer, ref timeStamp, ref usage);

				if(tex == null)
				{
					return null;
				}

				MatchAndAppendToken(_lexer, ",");
				token = _lexer.ReadToken();
				AppendToken(token);

				float scale = token.ToFloat();

				// process it
				if(tex != null)
				{
					idLog.Warning("TODO: R_HeightmapToNormalMap( *pic, *width, *height, scale );");
					usage = TextureUsage.Bump;
				}

				MatchAndAppendToken(_lexer, ")");

				return tex;
			}
			else if(tokenLower == "addnormals")
			{
				MatchAndAppendToken(_lexer, "(");

				/*byte	*pic2;
				int		width2, height2;*/

				Texture2D tex, tex2;

				if((tex = ParseImageProgram(_lexer, ref timeStamp, ref usage)) == null)
				{
					return null;
				}

				MatchAndAppendToken(_lexer, ",");

				if((tex2 = ParseImageProgram(_lexer, ref timeStamp, ref usage)) == null)
				{
					tex.Dispose();

					idLog.Warning("TODO: content doesn't get unloaded, this texture will remain disposed for ever!");

					return null;
				}

				// process it
				if(tex != null)
				{
					// TODO: tex2.Dispose();
					idLog.Warning("TODO: content doesn't get unloaded, this texture will remain disposed for ever!");

					idLog.Warning("TODO: R_AddNormalMaps( *pic, *width, *height, pic2, width2, height2 );");
					usage = TextureUsage.Bump;
				}

				MatchAndAppendToken(_lexer, ")");

				return tex;
			}
			else if(tokenLower == "smoothnormals")
			{
				idLog.WriteLine("TODO: image program smoothnormals");
				/*MatchAndAppendToken( src, "(" );

				if ( !R_ParseImageProgram_r( src, pic, width, height, timestamps, depth ) ) {
					return false;
				}

				if ( pic ) {
					R_SmoothNormalMap( *pic, *width, *height );
					if ( depth ) {
						*depth = TD_BUMP;
					}
				}

				MatchAndAppendToken( src, ")" );
				return true;*/
				return null;
			}
			else if(tokenLower == "add")
			{
				idLog.WriteLine("TODO: image program add");

				/*byte	*pic2;
				int		width2, height2;

				MatchAndAppendToken( src, "(" );

				if ( !R_ParseImageProgram_r( src, pic, width, height, timestamps, depth ) ) {
					return false;
				}

				MatchAndAppendToken( src, "," );

				if ( !R_ParseImageProgram_r( src, pic ? &pic2 : NULL, &width2, &height2, timestamps, depth ) ) {
					if ( pic ) {
						R_StaticFree( *pic );
						*pic = NULL;
					}
					return false;
				}
		
				// process it
				if ( pic ) {
					R_ImageAdd( *pic, *width, *height, pic2, width2, height2 );
					R_StaticFree( pic2 );
				}

				MatchAndAppendToken( src, ")" );
				return true;*/

				return null;
			}
			else if(tokenLower == "scale")
			{
				idLog.WriteLine("TODO: image program scale");
				/*float	scale[4];
				int		i;

				MatchAndAppendToken( src, "(" );

				R_ParseImageProgram_r( src, pic, width, height, timestamps, depth );

				for ( i = 0 ; i < 4 ; i++ ) {
					MatchAndAppendToken( src, "," );
					src.ReadToken( &token );
					AppendToken( token );
					scale[i] = token.GetFloatValue();
				}

				// process it
				if ( pic ) {
					R_ImageScale( *pic, *width, *height, scale );
				}

				MatchAndAppendToken( src, ")" );
				return true;*/

				return null;
			}
			else if(tokenLower == "invertalpha")
			{
				idLog.WriteLine("TODO: image program invertalpha");
				/*MatchAndAppendToken( src, "(" );

				R_ParseImageProgram_r( src, pic, width, height, timestamps, depth );

				// process it
				if ( pic ) {
					R_InvertAlpha( *pic, *width, *height );
				}

				MatchAndAppendToken( src, ")" );
				return true;*/

				return null;
			}
			else if(tokenLower == "invertcolor")
			{
				idLog.WriteLine("TODO: image program invertcolor");

				/*MatchAndAppendToken( src, "(" );

				R_ParseImageProgram_r( src, pic, width, height, timestamps, depth );

				// process it
				if ( pic ) {
					R_InvertColor( *pic, *width, *height );
				}

				MatchAndAppendToken( src, ")" );
				return true;*/

				return null;
			}
			else if(tokenLower == "makeintensity")
			{
				MatchAndAppendToken(_lexer, "(");
				Texture2D t = ParseImageProgram(ref timeStamp, ref usage, parseOnly);

				idLog.Warning("TODO: makeintensity");
				/*if(parseOnly == false)
				{
					// copy red to green, blue, and alpha			
					int c = width * height * 4;

					for(int i = 0; i < c; i += 4)
					{
						data[i + 1] = data[i + 2] = data[i + 3] = data[i];
					}
				}*/

				MatchAndAppendToken(_lexer, ")");

				return t;
			}
			else if(tokenLower == "makealpha")
			{
				MatchAndAppendToken(_lexer, "(");

				Texture2D tex = ParseImageProgram(_lexer, ref timeStamp, ref usage);

				// average RGB into alpha, then set RGB to white
				if(tex != null)
				{
					idLog.Warning("TODO: average alpha image");

					/*int		c;
					c = *width * *height * 4;
					for ( i = 0 ; i < c ; i+=4 ) {
						(*pic)[i+3] = ( (*pic)[i+0] + (*pic)[i+1] + (*pic)[i+2] ) / 3;
						(*pic)[i+0] = 
						(*pic)[i+1] = 
						(*pic)[i+2] = 255;
					}*/
				}

				MatchAndAppendToken(_lexer, ")");

				return tex;
			}

			// if we are just parsing instead of loading or checking, don't do the R_LoadImage
			if(parseOnly == true)
			{
				return null;
			}

			// load it as an image
			return idEngine.Instance.GetService<IImageManager>().LoadImage(token.ToString(), ref timeStamp);
		}
		#endregion
		#endregion
	}
}