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
using System.Diagnostics;

using Microsoft.Xna.Framework;

using idTech4.Math;
using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.UI.SWF.Scripting
{
	public class idSWFScriptObject_SpriteInstancePrototype : idSWFScriptObject
	{
		#region Constructor
		public idSWFScriptObject_SpriteInstancePrototype()
			: base()
		{
			Set("duplicateMovieClip",	new idSWFScriptFunction_Sprite(ScriptFunction_duplicateMovieClip));
			Set("gotoAndPlay",			new idSWFScriptFunction_Sprite(ScriptFunction_gotoAndPlay));
			Set("gotoAndStop",			new idSWFScriptFunction_Sprite(ScriptFunction_gotoAndStop));
			Set("swapDepths",			new idSWFScriptFunction_Sprite(ScriptFunction_swapDepths));
			Set("nextFrame",			new idSWFScriptFunction_Sprite(ScriptFunction_nextFrame));
			Set("prevFrame",			new idSWFScriptFunction_Sprite(ScriptFunction_prevFrame));
			Set("play",					new idSWFScriptFunction_Sprite(ScriptFunction_play));
			Set("stop",					new idSWFScriptFunction_Sprite(ScriptFunction_stop));

			SetNative("_alpha",         new idSWFScriptNativeVariable_Sprite(ScriptVariable_getAlpha, ScriptVariable_setAlpha));
			SetNative("_brightness",    new idSWFScriptNativeVariable_Sprite(ScriptVariable_getBrightness, ScriptVariable_setBrightness));
			SetNative("_currentframe",  new idSWFScriptNativeVariable_Sprite(ScriptVariable_getCurrentFrame));
			SetNative("_droptarget",    new idSWFScriptNativeVariable_Sprite(ScriptVariable_getDropTarget));
			SetNative("_focusrect",     new idSWFScriptNativeVariable_Sprite(ScriptVariable_getFocusRect));
			SetNative("_framesloaded",  new idSWFScriptNativeVariable_Sprite(ScriptVariable_getFramesLoaded));
			SetNative("_height",        new idSWFScriptNativeVariable_Sprite(ScriptVariable_getHeight, ScriptVariable_setHeight));
			SetNative("_highquality",   new idSWFScriptNativeVariable_Sprite(ScriptVariable_getHighQuality));
			SetNative("_itemindex",     new idSWFScriptNativeVariable_Sprite(ScriptVariable_getItemIndex, ScriptVariable_setItemIndex));
			SetNative("_mousex",        new idSWFScriptNativeVariable_Sprite(ScriptVariable_getMouseX));
			SetNative("_mousey",        new idSWFScriptNativeVariable_Sprite(ScriptVariable_getMouseY));
			SetNative("_name",          new idSWFScriptNativeVariable_Sprite(ScriptVariable_getName));
			SetNative("_quality",       new idSWFScriptNativeVariable_Sprite(ScriptVariable_getQuality));
			SetNative("_rotation",      new idSWFScriptNativeVariable_Sprite(ScriptVariable_getRotation, ScriptVariable_setRotation));
			SetNative("_soundbuftime",  new idSWFScriptNativeVariable_Sprite(ScriptVariable_getSoundBufferTime));
			SetNative("_stereoDepth",   new idSWFScriptNativeVariable_Sprite(ScriptVariable_getStereoDepth, ScriptVariable_setStereoDepth));
			SetNative("_target",        new idSWFScriptNativeVariable_Sprite(ScriptVariable_getTarget));
			SetNative("_totalframes",   new idSWFScriptNativeVariable_Sprite(ScriptVariable_getTotalFrames));
			SetNative("_url",           new idSWFScriptNativeVariable_Sprite(ScriptVariable_getUrl));
			SetNative("_visible",       new idSWFScriptNativeVariable_Sprite(ScriptVariable_getVisible, ScriptVariable_setVisible));
			SetNative("_width",         new idSWFScriptNativeVariable_Sprite(ScriptVariable_getWidth, ScriptVariable_setWidth));
			SetNative("_x",             new idSWFScriptNativeVariable_Sprite(ScriptVariable_getX, ScriptVariable_setX));
			SetNative("_xscale",        new idSWFScriptNativeVariable_Sprite(ScriptVariable_getXScale, ScriptVariable_setXScale));
			SetNative("_y",             new idSWFScriptNativeVariable_Sprite(ScriptVariable_getY, ScriptVariable_setY));
			SetNative("_yscale",        new idSWFScriptNativeVariable_Sprite(ScriptVariable_getYScale, ScriptVariable_setYScale));
			
			SetNative("material",       new idSWFScriptNativeVariable_Sprite(ScriptVariable_getMaterial, ScriptVariable_setMaterial));
			SetNative("materialWidth",  new idSWFScriptNativeVariable_Sprite(ScriptVariable_getMaterialWidth, ScriptVariable_setMaterialWidth));
			SetNative("materialHeight", new idSWFScriptNativeVariable_Sprite(ScriptVariable_getMaterialHeight, ScriptVariable_setMaterialHeight));
			SetNative("xOffset",        new idSWFScriptNativeVariable_Sprite(ScriptVariable_getXOffset, ScriptVariable_setXOffset));
			SetNative("onEnterFrame",   new idSWFScriptNativeVariable_Sprite(ScriptVariable_getOnEnterFrame, ScriptVariable_setOnEnterFrame));
			//SetNative("onLoad", new idSWFScriptNativeVariable_Sprite(ScriptVariable_getOnLoad, ScriptVariable_setOnLoad));
		}
		#endregion

		#region Script Functions
		private idSWFScriptVariable ScriptFunction_duplicateMovieClip(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_gotoAndPlay(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			if(parms.Count > 0)
			{
				spriteInstance.ClearActions();
				spriteInstance.RunTo(spriteInstance.FindFrame(parms[0].ToString()));
				spriteInstance.Play();
			}
			else
			{
				idLog.Warning("gotoAndPlay: expected 1 paramater");
			}

			return new idSWFScriptVariable();
		}

		private idSWFScriptVariable ScriptFunction_gotoAndStop(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_swapDepths(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_nextFrame(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_prevFrame(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_play(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}

		private idSWFScriptVariable ScriptFunction_stop(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFParameterList parms)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Script Variables
		private idSWFScriptVariable ScriptVariable_getAlpha(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			if(spriteInstance == null)
			{
				return new idSWFScriptVariable(1.0f);
			}

			idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != spriteInstance))
			{
				idLog.Warning("_alpha: Couldn't find our display entry in our parents display list");
				return new idSWFScriptVariable(1.0f);
			}

			return new idSWFScriptVariable(displayEntry.ColorXForm.Mul.W);
		}

		private void ScriptVariable_setAlpha(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			spriteInstance.Alpha = value.ToFloat();
		}

		private idSWFScriptVariable ScriptVariable_getBrightness(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			if(spriteInstance == null)
			{
				return new idSWFScriptVariable(1.0f);
			}

			idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != spriteInstance))
			{
				idLog.Warning("_brightness: Couldn't find our display entry in our parents display list");
				return new idSWFScriptVariable(1.0f);
			}

			// this works as long as the user only used the "brightess" control in the editor
			// if they used anything else (tint/advanced) then this will return fairly random values
			Vector4 mul = displayEntry.ColorXForm.Mul;
			Vector4 add = displayEntry.ColorXForm.Add;

			float avgMul = (mul.X + mul.Y + mul.Z ) / 3.0f;
			float avgAdd = (add.X + add.Y + add.Z ) / 3.0f;
	
			if(avgAdd > 1.0f) 
			{
				return new idSWFScriptVariable(avgAdd);
			} 
			else 
			{
				return new idSWFScriptVariable(avgMul - 1.0f);
			}
		}

		private void ScriptVariable_setBrightness(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			if(spriteInstance == null)
			{
				return;
			}

			idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != spriteInstance))
			{
				idLog.Warning("_brightness: Couldn't find our display entry in our parents display list");
				return;
			}

			// this emulates adjusting the "brightness" slider in the editor
			// although the editor forces alpha to 100%
			float b = value.ToFloat();
			float c = 1.0f - b;
	
			if(b < 0.0f) 
			{
				c = 1.0f + b;
				b = 0.0f;
			}
	
			displayEntry.ColorXForm.Add = new Vector4(b, b, b, displayEntry.ColorXForm.Add.W);
			displayEntry.ColorXForm.Mul = new Vector4(c, c, c, displayEntry.ColorXForm.Mul.W);
		}

		private idSWFScriptVariable ScriptVariable_getCurrentFrame(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.CurrentFrame);
		}

		private idSWFScriptVariable ScriptVariable_getDropTarget(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(string.Empty);
		}

		private idSWFScriptVariable ScriptVariable_getFocusRect(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(true);
		}

		private idSWFScriptVariable ScriptVariable_getFramesLoaded(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.FrameCount);
		}

		private idSWFScriptVariable ScriptVariable_getHeight(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(0.0f);
		}

		private void ScriptVariable_setHeight(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			
		}

		private idSWFScriptVariable ScriptVariable_getHighQuality(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(2);
		}

		private idSWFScriptVariable ScriptVariable_getItemIndex(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.ItemIndex);
		}

		private void ScriptVariable_setItemIndex(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			spriteInstance.ItemIndex = value.ToInt32();
		}

		private idSWFScriptVariable ScriptVariable_getMaterial(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			if(spriteInstance.MaterialOverride == null)
			{
				return new idSWFScriptVariable();
			}
			else
			{
				return new idSWFScriptVariable(spriteInstance.MaterialOverride.Name);
			}
		}

		private void ScriptVariable_setMaterial(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			if(value.IsString == false)
			{
				spriteInstance.MaterialOverride = null;
			}
			else
			{
				IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();

				// God I hope this material was referenced during map load
				spriteInstance.SetMaterial(declManager.FindMaterial(value.ToString(), false));
			}
		}

		private idSWFScriptVariable ScriptVariable_getMaterialWidth(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.MaterialWidth);
		}

		private void ScriptVariable_setMaterialWidth(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			Debug.Assert(value.ToInt32() > 0);
			Debug.Assert(value.ToInt32() <= 8192);

			spriteInstance.MaterialWidth = (ushort) value.ToInt32();
		}

		private idSWFScriptVariable ScriptVariable_getMaterialHeight(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.MaterialHeight);
		}

		private void ScriptVariable_setMaterialHeight(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			Debug.Assert(value.ToInt32() > 0);
			Debug.Assert(value.ToInt32() <= 8192);

			spriteInstance.MaterialHeight = (ushort) value.ToInt32();
		}

		private idSWFScriptVariable ScriptVariable_getMouseX(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			idLog.Warning("TODO: getMouseX");
			return new idSWFScriptVariable(0);
		}

		private idSWFScriptVariable ScriptVariable_getMouseY(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			idLog.Warning("TODO: getMouseY");
			return new idSWFScriptVariable(0);
		}

		private idSWFScriptVariable ScriptVariable_getName(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.Name);
		}

		private idSWFScriptVariable ScriptVariable_getOnEnterFrame(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.OnEnterFrame);
		}

		private void ScriptVariable_setOnEnterFrame(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			spriteInstance.OnEnterFrame = value;
		}

		private idSWFScriptVariable ScriptVariable_getQuality(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable("BEST");
		}

		private idSWFScriptVariable ScriptVariable_getRotation(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			if(spriteInstance == null)
			{
				return new idSWFScriptVariable(0.0f);
			}

			idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != spriteInstance))
			{
				idLog.Warning("_rotation: Couldn't find our display entry in our parents display list");
				return new idSWFScriptVariable(0.0f);
			}

			Vector2 scale = displayEntry.Matrix.Scale(new Vector2(0, 1));
			scale.Normalize();

			float rotation = idMath.ToDegrees(idMath.Acos(scale.Y));

			if(scale.X < 0.0f)
			{
				rotation = -rotation;
			}
	
			return new idSWFScriptVariable(rotation);
		}

		private void ScriptVariable_setRotation(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			if(spriteInstance == null)
			{
				return;
			}

			idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != spriteInstance))
			{
				idLog.Warning("_rotation: Couldn't find our display entry in our parents display list");
				return;
			}

			idSWFMatrix matrix = displayEntry.Matrix;
			float xScale = matrix.Scale(new Vector2(1, 0)).Length();
			float yScale = matrix.Scale(new Vector2(0, 1)).Length();

			float s, c;

			idMath.SinCos(idMath.ToRadians(value.ToFloat()), out s, out c);

			matrix.XX = c * xScale;
			matrix.YX = s * xScale;
			matrix.XY = -s * yScale;
			matrix.YY = c * yScale;

			displayEntry.Matrix = matrix;
		}

		private idSWFScriptVariable ScriptVariable_getSoundBufferTime(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(0);
		}

		private idSWFScriptVariable ScriptVariable_getStereoDepth(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable((int) spriteInstance.StereoDepth);
		}

		private void ScriptVariable_setStereoDepth(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			spriteInstance.StereoDepth = (StereoDepthType) value.ToInt32();
		}

		private idSWFScriptVariable ScriptVariable_getTarget(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(string.Empty);
		}

		private idSWFScriptVariable ScriptVariable_getTotalFrames(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.FrameCount);
		}

		private idSWFScriptVariable ScriptVariable_getUrl(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(string.Empty);
		}

		private idSWFScriptVariable ScriptVariable_getVisible(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.IsVisible);
		}

		private void ScriptVariable_setVisible(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			spriteInstance.IsVisible = value.ToBool();
		}

		private idSWFScriptVariable ScriptVariable_getWidth(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(0.0f);
		}

		private void ScriptVariable_setWidth(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			
		}

		private idSWFScriptVariable ScriptVariable_getX(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.PositionX);
		}

		private void ScriptVariable_setX(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			spriteInstance.PositionX = value.ToFloat();
		}

		private idSWFScriptVariable ScriptVariable_getXOffset(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.OffsetX);
		}

		private void ScriptVariable_setXOffset(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			spriteInstance.OffsetX = value.ToFloat();
		}

		private idSWFScriptVariable ScriptVariable_getXScale(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			if(spriteInstance.Parent == null)
			{
				return new idSWFScriptVariable(1.0f);
			}

			idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != spriteInstance))
			{
				idLog.Warning("_xscale: Couldn't find our display entry in our parents display list");
				return new idSWFScriptVariable(1.0f);
			}

			return new idSWFScriptVariable(displayEntry.Matrix.Scale(new Vector2(1, 0)).Length() * 100.0f);
		}

		private void ScriptVariable_setXScale(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			if(spriteInstance.Parent == null)
			{
				return;
			}

			idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != spriteInstance))
			{
				idLog.Warning("_xscale: Couldn't find our display entry in our parents display list");
				return;
			}

			float newScale = value.ToFloat() / 100.0f;
	
			// this is done funky to maintain the current rotation
			Vector2 currentScale = displayEntry.Matrix.Scale(new Vector2(1.0f, 0.0f));
			currentScale.Normalize();

			if(currentScale.Length() == 0.0f)
			{
				displayEntry.Matrix.XX = newScale;
				displayEntry.Matrix.YX = 0.0f;
			}
			else
			{
				displayEntry.Matrix.XX = currentScale.X * newScale;
				displayEntry.Matrix.YX = currentScale.Y * newScale;
			}
		}

		private idSWFScriptVariable ScriptVariable_getY(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			return new idSWFScriptVariable(spriteInstance.PositionY);
		}

		private void ScriptVariable_setY(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			spriteInstance.PositionY = value.ToFloat();
		}

		private idSWFScriptVariable ScriptVariable_getYScale(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance)
		{
			if(spriteInstance.Parent == null)
			{
				return new idSWFScriptVariable(1.0f);
			}

			idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != spriteInstance))
			{
				idLog.Warning("_yscale: Couldn't find our display entry in our parents display list");
				return new idSWFScriptVariable(1.0f);
			}

			return new idSWFScriptVariable(displayEntry.Matrix.Scale(new Vector2(0, 1)).Length() * 100.0f);
		}

		private void ScriptVariable_setYScale(idSWFScriptObject scriptObj, idSWFSpriteInstance spriteInstance, idSWFScriptVariable value)
		{
			if(spriteInstance.Parent == null)
			{
				return;
			}

			idSWFDisplayEntry displayEntry = spriteInstance.Parent.FindDisplayEntry(spriteInstance.Depth);

			if((displayEntry == null) || (displayEntry.SpriteInstance != spriteInstance))
			{
				idLog.Warning("_yscale: Couldn't find our display entry in our parents display list");
				return;
			}

			float newScale = value.ToFloat() / 100.0f;
	
			// this is done funky to maintain the current rotation
			Vector2 currentScale = displayEntry.Matrix.Scale(new Vector2(0.0f, 1.0f));
			currentScale.Normalize();

			if(currentScale.Length() == 0.0f)
			{
				displayEntry.Matrix.YY = newScale;
				displayEntry.Matrix.XY = 0.0f;
			}
			else
			{
				displayEntry.Matrix.YY = currentScale.Y * newScale;
				displayEntry.Matrix.XY = currentScale.X * newScale;
			}
		}
		#endregion
	}
}