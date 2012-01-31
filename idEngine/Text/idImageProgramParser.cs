using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		public byte[] ParseImageProgram(idLexer lexer)
		{
			_lexer = lexer;

			int width = 0;
			int height = 0;
			DateTime timeStamp = DateTime.Now;;
			TextureDepth depth = TextureDepth.Default;

			return ParseImageProgram(ref width, ref height, ref timeStamp, ref depth, true);
		}

		public byte[] ParseImageProgram(idLexer lexer, ref int width, ref int height, ref DateTime timeStamp, ref TextureDepth depth)
		{
			_lexer = lexer;
			return ParseImageProgram(ref width, ref height, ref timeStamp, ref depth, false);
		}

		public byte[] ParseImageProgram(string source, ref int width, ref int height, ref DateTime timeStamp, ref TextureDepth depth)
		{
			_lexer = new idLexer(LexerOptions.NoFatalErrors | LexerOptions.NoStringConcatination | LexerOptions.NoStringEscapeCharacters | LexerOptions.AllowPathNames);
			_lexer.LoadMemory(source, source);

			return ParseImageProgram(ref width, ref height, ref timeStamp, ref depth, false);
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

		private byte[] ParseImageProgram(ref int width, ref int height, ref DateTime timeStamp, ref TextureDepth depth, bool parseOnly)
		{
			idToken token = _lexer.ReadToken();			
			AppendToken(token);
			
			string tokenLower = token.ToString().ToLower();
		
			if(tokenLower == "heightmap")
			{
				idConsole.WriteLine("TODO: image program heightmap");
				/*MatchAndAppendToken(_lexer, "(");

	if ( !token.Icmp( "heightmap" ) ) {
		MatchAndAppendToken( src, "(" );

		if ( !R_ParseImageProgram_r( src, pic, width, height, timestamps, depth ) ) {
			return false;
		}

		MatchAndAppendToken( src, "," );

		src.ReadToken( &token );
		AppendToken( token );
		scale = token.GetFloatValue();
		
		// process it
		if ( pic ) {
			R_HeightmapToNormalMap( *pic, *width, *height, scale );
			if ( depth ) {
				*depth = TD_BUMP;
			}
		}

		MatchAndAppendToken( src, ")" );
		return true;*/

				return null;
			}
			else if(tokenLower == "addnormals")
			{
				idConsole.WriteLine("image program addnormals");

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
					R_AddNormalMaps( *pic, *width, *height, pic2, width2, height2 );
					R_StaticFree( pic2 );
					if ( depth ) {
						*depth = TD_BUMP;
					}
				}

				MatchAndAppendToken( src, ")" );
				return true;*/

				return null;
			}
			else if(tokenLower == "smoothnormals")
			{
				idConsole.WriteLine("image program smoothnormals");
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
				idConsole.WriteLine("image program add");

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
				idConsole.WriteLine("image program scale");
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
				idConsole.WriteLine("image program invertalpha");
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
				idConsole.WriteLine("image program invertcolor");

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
				byte[] data = ParseImageProgram(ref width, ref height, ref timeStamp, ref depth, parseOnly);

				if(parseOnly == false)
				{
					// copy red to green, blue, and alpha			
					int c = width * height * 4;

					for(int i = 0; i < c; i += 4)
					{
						data[i + 1] = data[i + 2] = data[i + 3] = data[i];
					}
				}

				MatchAndAppendToken(_lexer, ")");

				return data;
			}
			else if(tokenLower == "makealpha")
			{
				idConsole.WriteLine("image program makealpha");
				/*int		i;

				MatchAndAppendToken( src, "(" );

				R_ParseImageProgram_r( src, pic, width, height, timestamps, depth );

				// average RGB into alpha, then set RGB to white
				if ( pic ) {
					int		c;
					c = *width * *height * 4;
					for ( i = 0 ; i < c ; i+=4 ) {
						(*pic)[i+3] = ( (*pic)[i+0] + (*pic)[i+1] + (*pic)[i+2] ) / 3;
						(*pic)[i+0] = 
						(*pic)[i+1] = 
						(*pic)[i+2] = 255;
					}
				}

				MatchAndAppendToken( src, ")" );
				return true;*/

				return null;
			}

			// if we are just parsing instead of loading or checking, don't do the R_LoadImage
			if(parseOnly == true)
			{
				return new byte[] {};
			}

			// load it as an image
			return idE.ImageManager.LoadImage(token.ToString(), ref width, ref height, ref timeStamp, true);
		}
		#endregion
		#endregion
	}
}