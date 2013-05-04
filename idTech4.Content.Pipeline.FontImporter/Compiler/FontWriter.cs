using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using idTech4.Content.Pipeline.Intermediate.Fonts;

using TWrite = idTech4.Content.Pipeline.Intermediate.Fonts.FontContent;

namespace idTech4.Content.Pipeline.Compiler
{
	[ContentTypeWriter]
	public class FontWriter : ContentTypeWriter<TWrite>
	{
		#region ContentTypeWriter implementation
		protected override void Write(ContentWriter output, TWrite value)
		{
			output.Write(value.Ascender);
			output.Write(value.Descender);

			output.Write(value.Glyphs.Length);

			foreach(FontGlyph glyph in value.Glyphs)
			{
				output.Write(glyph.Width);
				output.Write(glyph.Height);
				output.Write(glyph.Top);
				output.Write(glyph.Left);
				output.Write(glyph.SkipX);
				output.Write(glyph.TextureCoordinates);
			}

			output.Write(value.CharacterIndices.Length);

			for(int i = 0; i < value.CharacterIndices.Length; i++)
			{
				output.Write(value.CharacterIndices[i]);
			}

			output.Write(value.Ascii.Length);

			for(int i = 0; i < value.Ascii.Length; i++)
			{
				output.Write(value.Ascii[i]);
			}

			output.Write(value.MaterialName);
		}

		public class FontContent
		{
			public short Ascender;
			public short Descender;

			public FontGlyph[] Glyphs;

			/// <summary>
			/// This is a sorted array of all characters in the font. 
			/// </summary>
			/// <remarks>
			/// This maps directly to glyphData, so if charIndex[0] is 42 then glyphData[0] is character 42.
			/// </remarks>
			public uint[] CharacterIndices;

			/// <summary>
			/// As an optimization, provide a direct mapping for the ascii character set. 
			/// </summary>
			public char[] Ascii;
		}

		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			// TODO: change this to the name of your ContentTypeReader
			// class which will be used to load this data.
			return "idTech4.Content.Pipeline.FontReader, idTech4";
		}
		#endregion
	}
}