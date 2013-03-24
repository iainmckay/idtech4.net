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
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using idTech4.Renderer;
using idTech4.Services;

namespace idTech4.UI.SWF
{
	public class idSWF
	{
		#region Constants
		private const float AlphaEpsilon   = 0.001f;
		private const int StencilDecrement = -1;
		private const int StencilIncrement = -2;
		#endregion

		#region Members
		// mouse coords for all flash files
		private static int _mouseX = -1;
		private static int _mouseY = -1;
		
		private float _frameWidth;
		private float _frameHeight;
		private ushort _frameRate;
		private float _renderBorder;
		private float _swfScale;

		private Vector2	_scaleToVirtual;

		private long	_lastRenderTime;

		private bool _isActive;
		private bool _inhibitControl;
		private bool _useInhibtControl;

		// certain screens need to be rendered when the pause menu is up so if this flag is
		// set on the gui we will allow it to render at a paused state;
		private bool _pausedRender;

		private bool _mouseEnabled;
		private bool _useMouse;

		private bool _blackbars;
		private bool _crop;
		private bool _paused;
		private bool _hasHitObject;

		private idMaterial _atlasMaterial;
		private idMaterial _guiSolid;
		private idMaterial _guiCursorArrow;
		private idMaterial _guiCursorHand;
		private idMaterial _white;

		private Random _random;

		private idSWFSprite _mainSprite;
		private idSWFSpriteInstance _mainSpriteInstance;

		private idSWFDictionaryEntry[] _dictionary;
		#endregion

		#region Constructor
		public idSWF()
		{			
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();
			ICVarSystem cvarSystem   = idEngine.Instance.GetService<ICVarSystem>();

			_atlasMaterial  = null;
			_swfScale       = 1.0f;
			_scaleToVirtual = new Vector2(1, 1);

			_random			= new Random();

			_guiSolid       = declManager.FindMaterial("guiSolid");
			_guiCursorArrow = declManager.FindMaterial("ui/assets/guicursor_arrow");
			_guiCursorHand  = declManager.FindMaterial("ui/assets/guicursor_hand");
			_white          = declManager.FindMaterial("_white");

			// TODO:
			/*tooltipButtonImage.Append( keyButtonImages_t( "<JOY1>", "guis/assets/hud/controller/xb360/a", "guis/assets/hud/controller/ps3/cross", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY2>", "guis/assets/hud/controller/xb360/b", "guis/assets/hud/controller/ps3/circle", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY3>", "guis/assets/hud/controller/xb360/x", "guis/assets/hud/controller/ps3/square", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY4>", "guis/assets/hud/controller/xb360/y", "guis/assets/hud/controller/ps3/triangle", 37, 37, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY_TRIGGER2>", "guis/assets/hud/controller/xb360/rt", "guis/assets/hud/controller/ps3/r2", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY_TRIGGER1>", "guis/assets/hud/controller/xb360/lt", "guis/assets/hud/controller/ps3/l2", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY5>", "guis/assets/hud/controller/xb360/lb", "guis/assets/hud/controller/ps3/l1", 52, 32, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<JOY6>", "guis/assets/hud/controller/xb360/rb", "guis/assets/hud/controller/ps3/r1", 52, 32, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<MOUSE1>", "guis/assets/hud/controller/mouse1", "", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<MOUSE2>", "guis/assets/hud/controller/mouse2", "", 64, 52, 0 ) );
			tooltipButtonImage.Append( keyButtonImages_t( "<MOUSE3>", "guis/assets/hud/controller/mouse3", "", 64, 52, 0 ) );
	 	
		   for ( int index = 0; index < tooltipButtonImage.Num(); index++ ) {
			   if ( ( tooltipButtonImage[index].xbImage != NULL ) && ( tooltipButtonImage[index].xbImage[0] != '\0' ) ) {
				   declManager->FindMaterial( tooltipButtonImage[index].xbImage );
			   }
			   if ( ( tooltipButtonImage[index].psImage != NULL ) && ( tooltipButtonImage[index].psImage[0] != '\0' ) ) {
				   declManager->FindMaterial( tooltipButtonImage[index].psImage );
			   }
		   }*/

			_useInhibtControl = true;
			_useMouse         = true;
		
			// TODO: 
			/*globals = idSWFScriptObject::Alloc();
			globals->Set( "_global", globals );

			globals->Set( "Object", &scriptFunction_Object );

			shortcutKeys = idSWFScriptObject::Alloc();
			scriptFunction_shortcutKeys_clear.Bind( this );
			scriptFunction_shortcutKeys_clear.Call( shortcutKeys, idSWFParmList() );
			globals->Set( "shortcutKeys", shortcutKeys );

			globals->Set( "deactivate", scriptFunction_deactivate.Bind( this ) );
			globals->Set( "inhibitControl", scriptFunction_inhibitControl.Bind( this ) );
			globals->Set( "useInhibit", scriptFunction_useInhibit.Bind( this ) );
			globals->Set( "precacheSound", scriptFunction_precacheSound.Bind( this ) );
			globals->Set( "playSound", scriptFunction_playSound.Bind( this ) );
			globals->Set( "stopSounds",scriptFunction_stopSounds.Bind( this ) );
			globals->Set( "getPlatform", scriptFunction_getPlatform.Bind( this ) );
			globals->Set( "getTruePlatform", scriptFunction_getTruePlatform.Bind( this ) );
			globals->Set( "getLocalString", scriptFunction_getLocalString.Bind( this ) );
			globals->Set( "swapPS3Buttons", scriptFunction_swapPS3Buttons.Bind( this ) );
			globals->Set( "_root", mainspriteInstance->scriptObject );
			globals->Set( "strReplace", scriptFunction_strReplace.Bind( this ) );
			globals->Set( "getCVarInteger", scriptFunction_getCVarInteger.Bind( this ) );
			globals->Set( "setCVarInteger", scriptFunction_setCVarInteger.Bind( this ) );

			globals->Set( "acos", scriptFunction_acos.Bind( this ) );
			globals->Set( "cos", scriptFunction_cos.Bind( this ) );
			globals->Set( "sin", scriptFunction_sin.Bind( this ) );
			globals->Set( "round", scriptFunction_round.Bind( this ) );
			globals->Set( "pow", scriptFunction_pow.Bind( this ) );
			globals->Set( "sqrt", scriptFunction_sqrt.Bind( this ) );
			globals->Set( "abs", scriptFunction_abs.Bind( this ) );
			globals->Set( "rand", scriptFunction_rand.Bind( this ) );
			globals->Set( "floor", scriptFunction_floor.Bind( this ) );
			globals->Set( "ceil", scriptFunction_ceil.Bind( this ) );
			globals->Set( "toUpper", scriptFunction_toUpper.Bind( this ) );

			globals->SetNative( "platform", swfScriptVar_platform.Bind( &scriptFunction_getPlatform ) );
			globals->SetNative( "blackbars", swfScriptVar_blackbars.Bind( this ) );
			globals->SetNative( "cropToHeight", swfScriptVar_crop.Bind( this ) );
			globals->SetNative( "cropToFit", swfScriptVar_crop.Bind( this ) );
			globals->SetNative( "crop", swfScriptVar_crop.Bind( this ) );*/
						
		

			// TODO: soundWorld = soundWorld_;
		}

		// TODO: cleanup
		/*idSWF::~idSWF() {
			spriteInstanceAllocator.Free( mainspriteInstance );
			delete mainsprite;

			for ( int i = 0 ; i < dictionary.Num() ; i++ ) {
				if ( dictionary[i].sprite ) {
					delete dictionary[i].sprite;
					dictionary[i].sprite = NULL;
				}
				if ( dictionary[i].shape ) {
					delete dictionary[i].shape;
					dictionary[i].shape = NULL;
				}
				if ( dictionary[i].font ) {
					delete dictionary[i].font;
					dictionary[i].font = NULL;
				}
				if ( dictionary[i].text ) {
					delete dictionary[i].text;
					dictionary[i].text = NULL;
				}
				if ( dictionary[i].edittext ) {
					delete dictionary[i].edittext;
					dictionary[i].edittext = NULL;
				}
			}
	
			globals->Clear();
			tooltipButtonImage.Clear();
			globals->Release();

			shortcutKeys->Clear();
			shortcutKeys->Release();
		}*/
		#endregion

		#region Drawing
		public void Draw(IRenderSystem renderSystem, long time = 0, bool isSplitScreen = false)
		{
			if(this.IsActive == false)
			{
				return;
			}

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			if(cvarSystem.GetInt("swf_stopat") > 0)
			{
				if(_mainSpriteInstance.CurrentFrame == cvarSystem.GetInt("swf_stopat"))
				{
					cvarSystem.Set("swf_timescale", 0.0f);
				}
			}

			long currentTime = idEngine.Instance.ElapsedTime;
			int framesToRun = 0;

			if(_paused == true)
			{
				_lastRenderTime = currentTime;
			}

			float swfTimeScale = cvarSystem.GetFloat("swf_timescale");

			if(swfTimeScale > 0.0f)
			{
				if(_lastRenderTime == 0)
				{
					_lastRenderTime = currentTime;
					framesToRun     = 1;
				}
				else
				{
					float deltaTime = (currentTime - _lastRenderTime);
					float fr        = ((float) _frameRate / 256.0f) * swfTimeScale;
					
					framesToRun      = (int) ((fr * deltaTime) / 1000.0f);
					_lastRenderTime += (long) (framesToRun * (1000.0f / fr));

					if(framesToRun > 10)
					{
						framesToRun = 10;
					}
				}

				for(int i = 0; i < framesToRun; i++) 
				{
					_mainSpriteInstance.Run();
					_mainSpriteInstance.RunActions();
				}
			}

			float pixelAspect = renderSystem.PixelAspect;
			float sysWidth    = renderSystem.Width * ((pixelAspect > 1.0f) ? pixelAspect : 1.0f);
			float sysHeight   = renderSystem.Height / ((pixelAspect < 1.0f) ? pixelAspect : 1.0f);
			float scale       = _swfScale * sysHeight / (float) _frameHeight;

			idSWFRenderState renderState = new idSWFRenderState();
			renderState.StereoDepth      = _mainSpriteInstance.StereoDepth;
			renderState.Matrix.XX        = scale;
			renderState.Matrix.YY        = scale;
			renderState.Matrix.TX        = 0.5f * (sysWidth - (_frameWidth * scale));
			renderState.Matrix.TY        = 0.5f * (sysHeight - (_frameHeight * scale));

			_renderBorder = renderState.Matrix.TX / scale;

			_scaleToVirtual.X = (float) Constants.ScreenWidth / sysWidth;
			_scaleToVirtual.Y = (float) Constants.ScreenHeight / sysHeight;

			DrawSprite(renderSystem, _mainSpriteInstance, renderState, time, isSplitScreen);

			if(_blackbars == true)
			{
				float barWidth = renderState.Matrix.TX + 0.5f;
				float barHeight = renderState.Matrix.TY + 0.5f;

				if(barWidth > 0.0f)
				{
					renderSystem.Color = new Color(0, 0, 0, 1);

					DrawStretchPicture(0, 0, barWidth, sysHeight, 0, 0, 1, 1, _white);
					DrawStretchPicture(sysWidth - barWidth, 0, barWidth, sysHeight, 0, 0, 1, 1, _white);
				}

				if(barHeight > 0.0f)
				{
					renderSystem.Color = new Color(0, 0, 0, 1);

					DrawStretchPicture(0, 0, sysWidth, barHeight, 0, 0, 1, 1, _white);
					DrawStretchPicture(0, sysHeight - barHeight, sysWidth, barHeight, 0, 0, 1, 1, _white);
				}
			}

			// TODO: mouse input
			/*if ( isMouseInClientArea && ( mouseEnabled && useMouse ) && ( InhibitControl() || ( !InhibitControl() && !useInhibtControl ) ) ) {
				gui->SetGLState( GLS_SRCBLEND_SRC_ALPHA | GLS_DSTBLEND_ONE_MINUS_SRC_ALPHA );
				gui->SetColor( idVec4( 1.0f, 1.0f, 1.0f, 1.0f ) );
				idVec2 mouse = renderState.matrix.Transform( idVec2( mouseX - 1, mouseY - 2 ) );
				//idSWFScriptObject * hitObject = HitTest( mainspriteInstance, swfRenderState_t(), mouseX, mouseY, NULL );
				if ( !hasHitObject ) { //hitObject == NULL ) {
					DrawStretchPic( mouse.x, mouse.y, 32.0f, 32.0f, 0, 0, 1, 1, guiCursor_arrow );
				} else {
					DrawStretchPic( mouse.x, mouse.y, 32.0f, 32.0f, 0, 0, 1, 1, guiCursor_hand );
				}
			}*/

			// restore the GL State
			renderSystem.SetRenderState(0);
		}

		private void DrawSprite(IRenderSystem renderSystem, idSWFSpriteInstance spriteInstance, idSWFRenderState renderState, long time, bool isSplitScreen)
		{
			if(spriteInstance == null)
			{
				idLog.Warning("RenderSprite: spriteInstance == null");
				return;
			}

			if(spriteInstance.IsVisible == false)
			{
				return;
			}

			ICVarSystem cvarSystem = idEngine.Instance.GetService<ICVarSystem>();

			if(((renderState.ColorXForm.Mul.W + renderState.ColorXForm.Add.W) <= AlphaEpsilon) && (cvarSystem.GetFloat("swf_forceAlpha") <= 0.0f))
			{
				return;
			}

			List<idSWFDisplayEntry> activeMasks = new List<idSWFDisplayEntry>();

			foreach(idSWFDisplayEntry display in spriteInstance.DisplayList)
			{
				for(int j = 0; j < activeMasks.Count; j++)
				{
					idSWFDisplayEntry mask = activeMasks[j];

					if(display.Depth > mask.ClipDepth)
					{
						idLog.Warning("TODO: RenderMask(renderSystem, mask, renderState, StencilDecrement);");
						activeMasks.RemoveAt(j);
					}

				}

				if(display.ClipDepth > 0)
				{
					activeMasks.Add(display);
					idLog.Warning("TODO: RenderMask(renderSystem, display, renderState, StencilIncrement);");
					continue;
				}

				idSWFDictionaryEntry entry = FindDictionaryEntry(display.CharacterID);

				if(entry == null)
				{
					continue;
				}

				idSWFRenderState renderState2;

				if(spriteInstance.StereoDepth != StereoDepthType.None)
				{
					renderState2.StereoDepth = spriteInstance.StereoDepth;
				}
				else if(renderState.StereoDepth != StereoDepthType.None)
				{
					renderState2.StereoDepth = renderState.StereoDepth;
				}

				renderState2.Matrix     = display.Matrix.Multiply(renderState.Matrix);
				renderState2.ColorXForm = display.ColorXForm.Multiply(renderState.ColorXForm);
				renderState2.Ratio      = display.Ratio;

				if(display.BlendMode != 0)
				{
					renderState2.BlendMode = (byte) display.BlendMode;
				}
				else
				{
					renderState2.BlendMode = renderState.BlendMode;
				}

				renderState2.ActiveMasks = renderState.ActiveMasks + activeMasks.Count;

				if(spriteInstance.MaterialOverride != null)
				{
					renderState2.Material       = spriteInstance.MaterialOverride;
					renderState2.MaterialWidth  = spriteInstance.MaterialWidth;
					renderState2.MaterialHeight = spriteInstance.MaterialHeight;
				}
				else
				{
					renderState2.Material       = renderState.Material;
					renderState2.MaterialWidth  = renderState.MaterialWidth;
					renderState2.MaterialHeight = renderState.MaterialHeight;
				}

				float xOffset = 0.0f;
				float yOffset = 0.0f;

				if(entry is idSWFSprite)
				{
					display.SpriteInstance.SetAlignment(spriteInstance.OffsetX, spriteInstance.OffsetY);

					if(display.SpriteInstance.Name.StartsWith("_") == true)
					{
						//if ( display.spriteInstance->name.Icmp( "_leftAlign" ) == 0 ) {
						//	float adj = (float)frameWidth  * 0.10;
						//	renderState2.matrix.tx = ( display.matrix.tx - adj ) * renderState.matrix.xx;
						//}
						//if ( display.spriteInstance->name.Icmp( "_rightAlign" ) == 0 ) {
						//	renderState2.matrix.tx = ( (float)renderSystem->GetWidth() - ( ( (float)frameWidth - display.matrix.tx - adj ) * renderState.matrix.xx ) );
						//}
						float titleSafe = cvarSystem.GetFloat("swf_titleSafe");
						float widthAdj  = titleSafe * _frameWidth;
						float heightAdj = titleSafe * _frameHeight;

						float pixelAspect = renderSystem.PixelAspect;
						float sysWidth    = renderSystem.Width * ((pixelAspect > 1.0f) ? pixelAspect : 1.0f);
						float sysHeight   = renderSystem.Height / ((pixelAspect < 1.0f) ? pixelAspect : 1.0f);

						if(display.SpriteInstance.Name.Equals("_fullScreen", StringComparison.OrdinalIgnoreCase) == true)
						{
							float xScale = sysWidth / (float) _frameWidth;
							float yScale = sysHeight / (float) _frameHeight;

							renderState2.Matrix.TX = display.Matrix.TX * renderState.Matrix.XX;
							renderState2.Matrix.TY = display.Matrix.TY * renderState.Matrix.YY;
							renderState2.Matrix.XX = xScale;
							renderState2.Matrix.YY = yScale;
						}

						if(display.SpriteInstance.Name.Equals("_absTop", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TY = display.Matrix.TY * renderState2.Matrix.YY;
							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_top", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TY = (display.Matrix.TY + heightAdj) * renderState.Matrix.YY;
							display.SpriteInstance.SetAlignment(spriteInstance.OffsetY + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_topLeft", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = (display.Matrix.TX + widthAdj) * renderState.Matrix.XX;
							renderState2.Matrix.TY = (display.Matrix.TY + heightAdj) * renderState.Matrix.YY;

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_left") == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = (display.Matrix.TX + widthAdj) * renderState.Matrix.XX;
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.ToLower().Contains("_absleft") == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = display.Matrix.TX * renderState.Matrix.XX;
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.ToLower().Contains("_bottomleft") == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = (display.Matrix.TX + widthAdj) * renderState.Matrix.XX;
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							float prevY = renderState2.Matrix.TY;
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY + heightAdj) * renderState.Matrix.YY));
							yOffset = ((renderState2.Matrix.TY - prevY) / renderState.Matrix.YY);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_absBottom", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY) * renderState.Matrix.YY));
							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_bottom", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY + heightAdj) * renderState.Matrix.YY));
							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_topRight", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX + widthAdj) * renderState.Matrix.XX));
							renderState2.Matrix.TY = (display.Matrix.TY + heightAdj) * renderState.Matrix.YY;

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_right", StringComparison.OrdinalIgnoreCase) == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX + widthAdj) * renderState.Matrix.XX));
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.ToLower().Contains("_absright") == true)
						{
							float prevX = renderState2.Matrix.TX;
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX) * renderState.Matrix.XX));
							xOffset = ((renderState2.Matrix.TX - prevX) / renderState.Matrix.XX);

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_bottomRight", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX + widthAdj) * renderState.Matrix.XX));
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY + heightAdj) * renderState.Matrix.YY));

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						// ABSOLUTE CORNERS OF SCREEN
						else if(display.SpriteInstance.Name.Equals("_absTopLeft", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = display.Matrix.TX * renderState.Matrix.XX;
							renderState2.Matrix.TY = display.Matrix.TY * renderState.Matrix.YY;

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_absTopRight", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX) * renderState.Matrix.XX));
							renderState2.Matrix.TY = display.Matrix.TY * renderState.Matrix.YY;

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_absBottomLeft", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = display.Matrix.TX * renderState.Matrix.XX;
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY) * renderState.Matrix.YY));

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
						else if(display.SpriteInstance.Name.Equals("_absBottomRight", StringComparison.OrdinalIgnoreCase) == true)
						{
							renderState2.Matrix.TX = ((float) sysWidth - (((float) _frameWidth - display.Matrix.TX) * renderState.Matrix.XX));
							renderState2.Matrix.TY = ((float) sysHeight - (((float) _frameHeight - display.Matrix.TY) * renderState.Matrix.YY));

							display.SpriteInstance.SetAlignment(spriteInstance.OffsetX + xOffset, spriteInstance.OffsetY + yOffset);
						}
					}

					idLog.Warning("TODO: RenderSprite(renderSystem, display.SpriteInstance, renderState2, time, isSplitScreen);");
				}
				else if(entry is idSWFMorphShape)
				{
					idLog.Warning("TODO: RenderMorphShape(renderSystem, (idSWFShape) entry, renderState2);");
				}
				else if(entry is idSWFShape)
				{
					idLog.Warning("TODO: RenderShape(renderSystem, (idSWFShape) entry, renderState2);");
				}
				else if(entry is idSWFEditText)
				{
					idLog.Warning("TODO: RenderEditText(renderSystem, display.TextInstance, renderState2, time, isSplitScreen);");
				}
				else
				{
					//idLib::Warning( "%s: Tried to render an unrenderable character %d", filename.c_str(), entry->type );
				}
			}

			for(int j = 0; j < activeMasks.Count; j++)
			{
				idLog.Warning("TODO: RenderMask(renderSystem, activeMasks[j], renderState, StencilDecrement);");
			}
		}

		private void DrawStretchPicture(float x, float y, float width, float height, float s1, float t1, float s2, float t2, idMaterial material)
		{
			idEngine.Instance.GetService<IRenderSystem>().DrawStretchPicture(x * _scaleToVirtual.X, y * _scaleToVirtual.Y, width * _scaleToVirtual.X, height * _scaleToVirtual.Y, s1, t1, s2, t2, material);
		}
		#endregion

		#region Loading
		internal void LoadFrom(ContentReader input)
		{
			IDeclManager declManager = idEngine.Instance.GetService<IDeclManager>();
			ICVarSystem cvarSystem   = idEngine.Instance.GetService<ICVarSystem>();

			_mainSpriteInstance = null;

			// ------------------------------
			// BEGIN XNB LOAD
			_frameWidth  = input.ReadSingle();
			_frameHeight = input.ReadSingle();
			_frameRate   = input.ReadUInt16();

			input.ReadInt32(); // dict type - sprite

			_mainSprite = new idSWFSprite(this);
			_mainSprite.LoadFrom(input);

			_dictionary = new idSWFDictionaryEntry[input.ReadInt32()];

			for(int i = 0; i < _dictionary.Length; i++)
			{
				_dictionary[i] = CreateDictionaryEntry((idSWFDictionaryType) input.ReadInt32());
				_dictionary[i].LoadFrom(input);
			}

			// END XNB LOAD
			// ------------------------------

			idLog.Warning("TODO: _atlasMaterial      = declManager.FindMaterial(Path.GetFileNameWithoutExtension(binaryFileName));");

			_mainSpriteInstance = new idSWFSpriteInstance();
			_mainSpriteInstance.Initialize(_mainSprite, null, 0);

			// Do this to touch any external references (like sounds)
			// But disable script warnings because many globals won't have been created yet
			int debug = cvarSystem.GetInt("swf_debug");
			cvarSystem.Set("swf_debug", 0);						

			_mainSpriteInstance.Run();
			_mainSpriteInstance.RunActions();
			_mainSpriteInstance.RunTo(0);

			cvarSystem.Set("swf_debug", debug);

			if(_mouseX == -1)
			{
				_mouseX = (int) (_frameWidth / 2);
			}

			if(_mouseY == -1)
			{
				_mouseY = (int) (_frameHeight / 2);
			}
		}

		private idSWFDictionaryEntry CreateDictionaryEntry(idSWFDictionaryType type)
		{
			switch(type)
			{
				case idSWFDictionaryType.Null:
					return new idSWFNull();

				case idSWFDictionaryType.Image:
					return new idSWFImage();

				case idSWFDictionaryType.Shape:
				case idSWFDictionaryType.Morph:
					return new idSWFShape();

				case idSWFDictionaryType.Sprite:
					return new idSWFSprite(this);

				case idSWFDictionaryType.Font:
					return new idSWFFont();

				case idSWFDictionaryType.Text:
					return new idSWFText();

				case idSWFDictionaryType.EditText:
					return new idSWFEditText();

				default:
					idEngine.Instance.Error("Unknown SWF dictionary type");
					break;
			}

			return null;
		}
		#endregion

		#region Misc.
		public idSWFDictionaryEntry FindDictionaryEntry(int characterID)
		{
			if(_dictionary.Length < (characterID + 1))
			{
				idLog.Warning("could not find character {0}", characterID);
				return null;
			}

			return _dictionary[characterID];
		}
		#endregion

		#region State
		#region Properties
		public bool IsActive
		{
			get
			{
				return _isActive;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// When a SWF is deactivated, it rewinds the timeline back to the start.
		/// </remarks>
		/// <param name="show"></param>
		public void Activate(bool show)
		{
			if((_isActive == false) && (show == true))
			{
				_inhibitControl = false;
				_lastRenderTime = idEngine.Instance.ElapsedTime;

				_mainSpriteInstance.ClearDisplayList();
				_mainSpriteInstance.Play();
				_mainSpriteInstance.Run();
				_mainSpriteInstance.RunActions();
			}
			
			_isActive = show;
		}
		#endregion
		#endregion
	}

	public enum idSWFTag
	{
		End                          = 0,
		ShowFrame                    = 1,
		DefineShape                  = 2,
		PlaceObject                  = 4,
		RemoveObject                 = 5,
		DefineBits                   = 6,
		DefineButton                 = 7,
		JpegTables                   = 8,
		SetBackgroundColor           = 9,
		DefineFont                   = 10,
		DefineText                   = 11,
		DoAction                     = 12,
		DefineFontInfo               = 13,
		DefineSound                  = 14,
		StartSound                   = 15,
		DefineButtonSound            = 17,
		SoundStreamHead              = 18,
		SoundStreamBlock             = 19,
		DefineBitsLossless           = 20,
		DefineBitsJpeg2              = 21,
		DefineShape2                 = 22,
		DefineButtonCxForm           = 23,
		Protect                      = 24,
		PlaceObject2                 = 26,
		RemoveObject2                = 28,
		DefineShape3                 = 32,
		DefineText2                  = 33,
		DefineButton2                = 34,
		DefineBitsJpeg3              = 35,
		DefineBitsLossless2          = 36,
		DefineEditText               = 37,
		DefineSprite                 = 39,
		FrameLabel                   = 43,
		SoundStreamHead2             = 45,
		DefineMorphShape             = 46,
		DefineFont2                  = 48,
		ExportAssets                 = 57,
		EnableDebugger               = 58,
		DoInitAction                 = 59,
		DefineVideoStream            = 60,
		VideoFrame                   = 61,
		DefineFontInfo2              = 62,
		EnableDebugger2              = 64,
		ScriptLimits                 = 65,
		SetTabIndex                  = 66,
		FileAttributes               = 69,
		PlaceObject3                 = 70,
		ImportAssets2                = 71,
		DefineFontAlignZones         = 73,
		CsmTextSettings              = 74,
		DefineFont3                  = 75,
		SymbolClass                  = 76,
		Metadata                     = 77,
		DefineScalingGrid            = 78,
		DoAbc                        = 82,
		DefineShape4                 = 83,
		DefineMorphShape2            = 84,
		DefineSceneAndFrameLabelData = 86,
		DefineBinaryData             = 87,
		DefineFontName               = 88,
		StartSound2                  = 89
	}

	public enum idSWFDictionaryType
	{
		Null,
		Image,
		Shape,
		Morph,
		Sprite,
		Font,
		Text,
		EditText
	}

	public struct idSWFRect
	{
		public Vector2 TopLeft;
		public Vector2 BottomRight;

		internal void LoadFrom(ContentReader input)
		{
			this.TopLeft     = input.ReadVector2();
			this.BottomRight = input.ReadVector2();
		}
	}

	public struct idSWFColorRGBA
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public idSWFColorRGBA(byte r, byte g, byte b, byte a)
		{
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
		}

		internal void LoadFrom(ContentReader input)
		{
			this.R = input.ReadByte();
			this.G = input.ReadByte();
			this.B = input.ReadByte();
			this.A = input.ReadByte();
		}

		public static idSWFColorRGBA Default = new idSWFColorRGBA(255, 255, 255, 255);
	}

	public struct idSWFRenderState
	{
		public idSWFMatrix Matrix;
		public idSWFColorXForm ColorXForm;

		public idMaterial Material;
		public int MaterialWidth;
		public int MaterialHeight;

		public int ActiveMasks;
		public byte BlendMode;
		public float Ratio;
		public StereoDepthType StereoDepth;
	}
}