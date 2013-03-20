using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using idTech4.Content.Pipeline.Intermediate.SWF;

// TODO: replace this with the type you want to write out.
using TWrite = idTech4.Content.Pipeline.Intermediate.SWF.SWFContent;

namespace idTech4.Content.Pipeline.Compiler
{
	[ContentTypeWriter]
	public class SWFTypeWriter : ContentTypeWriter<TWrite>
	{
		#region ContentTypeWriter implementation
		protected override void Write(ContentWriter output, TWrite value)
		{
			output.Write(value.FrameWidth);
			output.Write(value.FrameHeight);
			output.Write(value.FrameRate);	

			value.MainSprite.Write(output);

			output.Write(value.Dictionary.Length);

			foreach(SWFDictionaryEntry entry in value.Dictionary)
			{
				entry.Write(output);
			}
		}

		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			// TODO: change this to the name of your ContentTypeReader
			// class which will be used to load this data.
			return "idTech4.UI.SWF.SWFTypeReader, idTech4.UI.SWF";
		}
		#endregion
	}
}